using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web; 
using System.Web.Hosting; 
using System.Net; 
using System.IO;

namespace BasicWebServerCSharp
{
    class Program { 
        static void Main(string[] args) { 
            HttpListener listener = new HttpListener(); 
            listener.Prefixes.Add("http://localhost:8081/"); 
            listener.Prefixes.Add("http://127.0.0.1:8081/"); 
            listener.Start(); 
            Console.WriteLine("Listening for requests on http://localhost:8081/"); 
            while (true) { 
                HttpListenerContext ctx = listener.GetContext(); 
                string page = ctx.Request.Url.LocalPath; //.Replace("/", ""); 
                string query = ctx.Request.Url.Query.Replace("?", ""); 
                Console.WriteLine("Received request for {0}?{1}", page, query);
                
                //read post paramaters
                StreamReader inputStream = new StreamReader(ctx.Request.InputStream);
                string data = inputStream.ReadToEnd();

                //write output
                StreamWriter outputStream = new StreamWriter(ctx.Response.OutputStream);
                outputStream.Write(GetFile(page,"C"));
                outputStream.Flush(); 
                ctx.Response.Close(); 
            } 
        }
        public static string GetFile(string localPath, string driveLetter)
        {
            if (localPath.EndsWith("/"))
                localPath = localPath + "index.html";

            return System.IO.File.ReadAllText(driveLetter + ":/" + localPath);
        }
    }
}