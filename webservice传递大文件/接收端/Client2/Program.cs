using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Client2
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost/net/WebService1.asmx";
            string method = "DownloadBigFile";

            string result = HttpPostWebService(url, method);

            Console.WriteLine(result);
            Console.ReadKey();
        }

        public static string HttpPostWebService(string url, string method)
        {
            string result = string.Empty;
            string param = string.Empty;
            byte[] bytes = null;

            Stream writer = null;
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            request = (HttpWebRequest)WebRequest.Create(url + "/" + method);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = 0;
            try
            {
                response = (HttpWebResponse)request.GetResponse();      //获得响应
            }
            catch (WebException ex)
            {
                return "";
            }

            Stream stream = response.GetResponseStream();        //获取响应流

            byte[] btArray = new byte[5120];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
            int contentSize = stream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次
            long startPosition = 0;
            long currPostion = startPosition;
            var writeStream = new FileStream(@"C:\Users\admin\Desktop\a.mp4", FileMode.Create);

            while (contentSize > 0)// 如果读取长度大于零则继续读
            {
                currPostion += contentSize;
                int percent = (int)(currPostion * 100 / response.ContentLength);
                System.Console.WriteLine("percent=" + percent + "%");
                writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                contentSize = stream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
            }

            writeStream.Close();
            response.Close();
            stream.Dispose();
            stream.Close();

            return result;
        }
    }
}
