using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleApp1.ws;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            ws.WebService1SoapClient client = new WebService1SoapClient();
            client.DownloadBigFile();
        }
    }
}
