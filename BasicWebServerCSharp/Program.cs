﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicWebServerCSharp
{
    class Program { 
        static void Main(string[] args) {
            SmallServer.Server server = new SmallServer.Server();
            server.PhysicalPath = @"C:\web\workspace";
            server.Prefixes.Add("http://localhost:8081/");
            server.Prefixes.Add("http://127.0.0.1:8081/");
            server.Prefixes.Add("http://" + server.GetRouterIp() + ":8081/");
            Console.WriteLine("http://" + server.GetRouterIp() + "/");
            server.Start();
        }
        
    }
}