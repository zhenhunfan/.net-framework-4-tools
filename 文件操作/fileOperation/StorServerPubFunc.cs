using System;
using System.Collections.Generic;
using System.Text;
using FrameWorkService.Common;
using FrameWorkService.Interface.Basic;
using System.Net;
using System.Threading;
namespace FrameWorkService.Imp
{
    public class StorServerPubFunc
    {
        public static string logfile = @"LogFile\StorServerLog.txt";
        public static ServiceConfigure sc;
        public static IServiceContentFactory m_scf;
        
        public static void RecordLogFile(string message)
        {
            LogRecord.Record(logfile, message);
        }
        public static void RecordLogFile(Exception ex)
        {
            LogRecord.Record(logfile, ex);
        }
        public static string ServerIP
        {
            get
            {
                string strHostName = Dns.GetHostName();   //得到本机的主机名                    
                string strAddr = Dns.GetHostByName(strHostName).AddressList[0].ToString(); //假设本地主机为单网卡
                return strAddr;
            }
        }
        public static string Port
        {
            get
            {
                if (sc == null)
                {
                    sc = ServiceConfigure.Load();
                }
                return sc.Port.ToString();
            }
        }
        public static IServiceContentFactory ServiceContentFactory
        {
            get {
                if (m_scf == null)
                {
                   m_scf=  Client.ProxyFactory.GetContentFactory(ServerIP, Port);
                }
                return m_scf;
            }
        }

        public static bool ConvertToSwf(string sourceFile, string targetFile)
        {
            string exe = AppDomain.CurrentDomain.BaseDirectory + @"SWFTool\pdf2swf.exe ";
            
            StringBuilder sb = new StringBuilder();
            sb.Append(" \"" + sourceFile + "\"");
            sb.Append(" -o \"" + targetFile + "\"");
            sb.Append(" -s flashversion=9");
            sb.Append("  -t -f -T 9");
           // sb.Append(" -s languagedir=\"E:\\xpdf\\xpdf-chinese-simplified\"");
           // if (endpage > GetPageCount(pdfPath)) endpage = GetPageCount(pdfPath);
          //  sb.Append(" -p " + "\"" + beginpage + "" + "-" + endpage + "\"");
          //  sb.Append(" -j " + photoQuality);  
            string Command = sb.ToString();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = exe;
            p.StartInfo.Arguments = Command;
            p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.BeginErrorReadLine();
            p.WaitForExit();
            p.Close();
            p.Dispose();
            return true;
        }

        public static bool ConvertToSwf1(string sourceFile, string targetFile)
        {
            //这个样的写法，有些文件不能执行，死在那里了，不知道什么原因
            //启动memcached服务 启动一个cmd进程
            //System.Diagnostics.Process.Start(strcommand);

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe"; ;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = false;//由调用程序获取输出信息设置成true 会执行停在那里了
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            string strcommand = AppDomain.CurrentDomain.BaseDirectory + @"SWFTool\pdf2swf.exe " + sourceFile + " -o " + targetFile + " -t -f -T 9";
            //strcommand=@"C:\SWFTool\pdf2swf.exe c:\SWFTool\result.pdf -o c:\SWFTool\result.swf -t -f -T 9";
            //string strcommand = AppDomain.CurrentDomain.BaseDirectory + @"SWFTool\pdf2swf.exe result.pdf  -o result.swf -t -f -T 9";
            //向cmd窗口发送输入信息 "&exit"         
            p.StandardInput.WriteLine(strcommand + "&exit");

            p.StandardInput.AutoFlush = true;
            //p.StandardInput.WriteLine("exit");
            //向标准输入写入要执行的命令。这里使用&是批处理命令的符号，表示前面一个命令不管是否执行成功都执行后面(exit)命令，如果不执行exit命令，后面调用ReadToEnd()方法会假死
            //同类的符号还有&&和||前者表示必须前一个命令执行成功才会执行后面的命令，后者表示必须前一个命令执行失败才会执行后面的命令

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
            return true;

        }
       
    }
}
