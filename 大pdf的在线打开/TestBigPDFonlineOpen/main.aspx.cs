using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TestBigPDFonlineOpen
{
    public partial class main : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            byte[] buffer = new Byte[10000];

            int length = 0;
            long dataToRead;

            int offset = 0;
            int count = 300000;

            var client = new OssClient("oss-cn-hangzhou-zjzwy01-d01-a.cloud.zj.gov.cn", "qAALhWQY2oMy1cwE", "1XfG1fugfUt8v3BfoYEnCuVHlGQrYA");
            var _filePath = "迁云前数据库备份/套红正文.pdf";
            try
            {


                dataToRead = client.GetObjectMetadata("oa-oss", _filePath).ContentLength;

                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Length", dataToRead.ToString());

                while (dataToRead > 0)
                {
                    if (dataToRead < count)
                        count = (int)dataToRead;
                    buffer = Read(client, _filePath, offset, count);
                    offset += count;
                    if (Response.IsClientConnected)
                    {
                        Response.OutputStream.Write(buffer, 0, count);
                        Response.Flush();
                        buffer = new Byte[count];
                        dataToRead = dataToRead - count;
                    }
                    else
                    {
                        dataToRead = -1;
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Write("Error : " + ex.StackTrace + ex.Message); ;
            }
            finally
            {

                Response.End();
            }
        }

        public string UrlEncodeUTF8(string str)
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
        public byte[] Read(OssClient client,string fileName, long offset, int count)
        {
            try
            {
                fileName = handlePath(fileName);

                byte[] buffer = new byte[1024];
                byte[] result = new byte[count];
                int resultIndex = 0;
                var length = 0;
                var getObjectRequest = new GetObjectRequest("oa-oss", fileName);
                getObjectRequest.SetRange(offset, offset + count - 1);

                var obj = client.GetObject(getObjectRequest);
                var contentLength = obj.ContentLength;
                using (var requestStream = obj.Content)
                {

                    while ((length = requestStream.Read(buffer, 0, 1024)) != 0)
                    {
                        if (length < 1024)
                        {
                            var aaa = 111;
                        }
                        Array.Copy(buffer, 0, result, resultIndex, length);
                        resultIndex += length;
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException("获取文件失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 处理路径字符串
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string handlePath(string path)
        {
            path = path.Replace("\\\\", "\\");
            path = path.Replace("\\", "/");
            path = path.Replace("//", "/");
            path = path.TrimStart('/');
            return path;
        }
    }
}