using System;
using System.Drawing;
using System.IO;
using System.Web.Services;

namespace Server
{
    /// <summary>
    /// WaterMark 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class WaterMark : System.Web.Services.WebService
    {
        /// <summary>
        /// 水印
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public byte[] WaterMarker2(string words, byte[] img)
        {
            MemoryStream ms = null;
            Graphics g = null;
            Image image = null;
            Bitmap bitmap = null;
            try
            {
                ms = new MemoryStream(img);
                ms.Position = 0;
                image = Image.FromStream(ms);

                bitmap = new Bitmap(image);
                //图片的宽度与高度
                int width = bitmap.Width, height = bitmap.Height;
                //水印文字
                string text = string.Format("义乌市国土资源局\n 用户：{0}", words);
                g = Graphics.FromImage(bitmap);
                g.DrawImage(bitmap, 0, 0);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
                Font crFont = new Font("微软雅黑", 50, FontStyle.Bold);
                SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(60, 137, 131, 131));
                //将原点移动 到图片中点
                //以原点为中心 转 -45度
                g.TranslateTransform(width / 2, height / 2);
                g.RotateTransform(-45);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        g.DrawString(text, crFont, semiTransBrush, 700 * (i - 1), 400 * (j - 2));
                    }
                }

                ms.Close();
                ms.Dispose();

                ms = new MemoryStream();
                bitmap.Save(ms, image.RawFormat);

                var result = ms.ToArray();
                return result;
            }
            catch (Exception _ex)
            {
                throw _ex;
            }
            finally
            {
                if (g != null)
                {
                    g.Dispose();
                }
                if (image != null)
                {
                    image.Dispose();
                }
                if (bitmap != null)
                    bitmap.Dispose();
                if (ms != null)
                    ms.Dispose();
            }
            
        }
    }
}
