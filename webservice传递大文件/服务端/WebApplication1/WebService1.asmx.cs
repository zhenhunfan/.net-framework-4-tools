using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebApplication1
{
    /// <summary>
    /// WebService1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        [WebMethod]
        public void DownloadBigFile()
        {
            var _filePath = System.AppDomain.CurrentDomain.BaseDirectory+"\\a.mp4";

            
            byte[] buffer = new Byte[10000];

            long dataToRead;

            int offset = 0;
            int count = 300000;

            dataToRead = GetFileLength(_filePath);

            Context.Response.Clear();
            Context.Response.ClearHeaders();
            Context.Response.ClearContent();
            Context.Response.ContentType = "application/ms-download";
            Context.Response.AddHeader("Content-Length", dataToRead.ToString());
            Context.Response.AddHeader("Content-Disposition", "attachment; filename=" + urlEncodeUTF8(Path.GetFileName(_filePath)));

            while (dataToRead > 0)
            {
                if (dataToRead < count)
                    count = (int)dataToRead;
                buffer = Read(_filePath, offset, count);
                offset += count;
                if (Context.Response.IsClientConnected)
                {
                    Context.Response.OutputStream.Write(buffer, 0, count);
                    Context.Response.Flush();
                    buffer = new Byte[count];
                    dataToRead = dataToRead - count;
                }
                else
                {
                    dataToRead = -1;
                }
            }
        }

        private long GetFileLength(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
                return file.Length;
            return 0;
        }

        private string urlEncodeUTF8(string str)
        {
            return HttpUtility.UrlEncode(str, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] Read(string fileName, long offset, int count)
        {
            byte[] buffer = new byte[count];
            int actualLength = 0;

            using (FileStream stream = File.OpenRead(fileName))
            {
                stream.Seek(offset, SeekOrigin.Begin);

                actualLength = stream.Read(buffer, 0, count);
                stream.Close();
                stream.Dispose();
            }

            if (actualLength < count)
            {
                byte[] result = new byte[actualLength];
                Buffer.BlockCopy(buffer, 0, result, 0, actualLength);

                return result;
            }

            return buffer;
        }
    }
}
