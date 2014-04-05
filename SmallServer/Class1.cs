﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace SmallServer
{

    public class Server
    {
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,
              [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
             [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] 
        byte[] pBuffer,
              int cbSize,
                 [MarshalAs(UnmanagedType.LPWStr)]  string pwzMimeProposed,
              int dwMimeFlags,
              out IntPtr ppwzMimeOut,
              int dwReserved);

        private HttpListener _listener = new HttpListener();

        public HttpListener Listener
        {
            get { return _listener; }
            set { _listener = value; }
        }

        private HttpListenerContext _context;

        public HttpListenerContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        private string _physicalpath = "C:/";

        public string PhysicalPath
        {
            get { return _physicalpath; }
            set { _physicalpath = value; }
        }

        private List<string> _prefixes = new List<string>() { 
        };

        public List<string> Prefixes
        {
            get { return _prefixes; }
            set { _prefixes = value; }
        }
        
        /// <summary>
        /// Method to start listening to the requested prefix
        /// </summary>
        public void Start() {
            Console.WriteLine("Small Server Starting...");
            Console.WriteLine("Adding Prefixes...");
            foreach (var prefix in this.Prefixes)
            {
                this.Listener.Prefixes.Add(prefix);
                Console.WriteLine("Listening for requests on {0}", prefix);
            }
            this.Listener.Start();

            while (true)
            {
                //Set the current context to the listener context.
                this.Context = this.Listener.GetContext();
                //Get the page requested.
                string page = this.Context.Request.Url.LocalPath;
                //Get any query parameters that were passed along (get)
                string query = this.Context.Request.Url.Query.Replace("?", "");
                Console.WriteLine("Received request for {0}?{1}", page, query);

                //read anything that was included in the body (ie json)
                StreamReader inputStream = new StreamReader(this.Context.Request.InputStream);
                string bodyData = inputStream.ReadToEnd();

                //write response headers
                this.Context.Response.Headers.Add("SmallServer","1.0");

                //write output response
                var fileBytes = this.GetFile(page);
                this.Context.Response.OutputStream.Write(fileBytes,0,fileBytes.Length);
                this.Context.Response.OutputStream.Flush();
                this.Context.Response.Close();
            } 
        }

        private byte[] GetFile(string localPath)
        {
            if (localPath.EndsWith("/"))
                localPath = localPath + "index.html";
            Console.WriteLine("Fufilling Request...");
            try
            {
                this.Context.Response.Headers.Add("Content-Type", getMimeFromFile(this.PhysicalPath + localPath));
                return System.IO.File.ReadAllBytes(this.PhysicalPath + localPath);
            }
            catch 
            {
                Console.WriteLine("Error Occured With Request For:{0}", localPath);
                return System.Text.Encoding.UTF8.GetBytes("<html><head></head><body>Small Server <br/> Sorry, Error Occured. <br/> :(</body></html>");
            }
        }

        public string GetRouterIp()
        {
            try
            {
                ////http://www.fluxbytes.com/csharp/how-to-get-your-ip-in-c/
                //IPAddress localAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                //if (localAddress != null)
                //{
                //    return localAddress.ToString();
                //}
                IPHostEntry host;
                string localIP = "?";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString() == "InterNetwork")
                    {
                        localIP = ip.ToString();
                    }
                }
                return localIP;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Ensures that file exists and retrieves the content type 
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Returns for instance "images/jpeg" </returns>
        public static string getMimeFromFile(string file)
        {
            IntPtr mimeout;
            if (!System.IO.File.Exists(file))
                throw new FileNotFoundException(file + " not found");

            int MaxContent = (int)new FileInfo(file).Length;
            if (MaxContent > 4096) MaxContent = 4096;
            FileStream fs = File.OpenRead(file);


            byte[] buf = new byte[MaxContent];
            fs.Read(buf, 0, MaxContent);
            fs.Close();
            int result = FindMimeFromData(IntPtr.Zero, file, buf, MaxContent, null, 0, out mimeout, 0);

            if (result != 0)
                throw Marshal.GetExceptionForHR(result);
            string mime = Marshal.PtrToStringUni(mimeout);
            Marshal.FreeCoTaskMem(mimeout);
            return mime;
        }


    }
}
