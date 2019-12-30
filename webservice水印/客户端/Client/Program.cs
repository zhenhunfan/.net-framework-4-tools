using System;
using System.IO;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            testService();
            Console.ReadKey();
        }

        public static void testService()
        {
            service.WaterMarkSoapClient client = new service.WaterMarkSoapClient();

            var img = File.ReadAllBytes(@"C:\Users\admin\Desktop\SoftResearch\dotnet\webservice水印\客户端\Client\划拨土地使用权转让办理出让手续申请、审批表(复)(2).JPG");

            var resultimg = client.WaterMarker2("系统管理员",  img);

            FileStream fs = new FileStream(@"C:\Users\admin\Desktop\SoftResearch\dotnet\webservice水印\客户端\Client\bbb.jpg", FileMode.Create);

            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(resultimg);

            bw.Close();

            fs.Close();
        }
    }
}
