/**************************************************************************
 * 
 *  2007-08-22,CAIFL,编写存储服务器初始版,主要用于OA系统中的附件的统一存储.
 * 
 * 
 * ***********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using FrameWorkService.Interface;
namespace FrameWorkService.Imp
{
    public class StoreServer : MarshalByRefObject, IStore,Iconfigure
    {
        private StoreServerConfiguration ssc = null;
        #region IStore 成员

        public List<string> GetConfigInfo()
        {
            if (ssc == null)
            {
                ssc = StoreServerConfiguration.Load();
            }
            List<string> ConfigInfo = new List<string>();

            ConfigInfo.Add(ssc.ServiceIP);
            ConfigInfo.Add(ssc.Status);
            ConfigInfo.Add(ssc.Paths);
            return ConfigInfo;
        }

        public long GetStoreSize()
        {
            return -1;
        }

        public long GetDriverFreespace()
        {
            List<string> ConfigInfoList = GetConfigInfo();
            try
            {
                int firstIndex = ConfigInfoList[2].IndexOf(":");
                if (firstIndex > 0)
                {
                    string diskName = ConfigInfoList[2].Substring(0, firstIndex + 1);

                    SelectQuery selectQuery = new SelectQuery("SELECT * FROM win32_logicaldisk");
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectQuery);
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        string name = disk["Name"].ToString(); //获取磁盘驱动器名称
                        if (string.Compare(diskName, name, true) == 0)
                        {
                            //获取可用空间
                            return Convert.ToInt64(disk["FreeSpace"]);
                        }
                    }
                }
            }
            catch(Exception ex)
            { StorServerPubFunc.RecordLogFile(ex); }

            throw new Exception("获取磁盘可用空间失败,指定的磁盘驱动器不存在.");
        }

        public void Write(string fileName, byte[] buffer)
        {
            this.Write(fileName, buffer, true);
        }

        public void Write(string fileName, byte[] buffer, bool append)
        {
            if (fileName == null)
                throw new Exception("目标文件名不能为空.");

            if (buffer == null)
                throw new Exception("写入的内容不能为空.");
            string directoryName = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            try
            {
                if (append)
                {
                    try
                    {

                        using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Write))
                        {
                            //int offset = 0;
                            //if( fs.Length > 0 )
                            //    offset = (int)(fs.Length - 1);

                            fs.Write(buffer, 0, buffer.Length);
                            fs.Flush();

                            fs.Close();
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("指定的文件不存在,可能已经被移除.");
                    }
                }
                else
                {
                    using (FileStream fs = File.Create(fileName))
                    {
                        fs.Write(buffer, 0, buffer.Length);
                        fs.Close();
                    }
                }
            }
            catch (IOException ex)
            {
                StorServerPubFunc.RecordLogFile(ex);
                throw new Exception("操作文件失败." + ex.Message);
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName">目前位置</param>
        /// <param name="destFileName">新的位置</param>
        public void Move(string sourceFileName, string destFileName)
        {
            if (File.Exists(sourceFileName))
            {
                if (!sourceFileName.Equals(destFileName))
                {
                    try
                    {
                        string destDirectoryName = Path.GetDirectoryName(destFileName);
                        if (destDirectoryName != null)
                        {
                            if (!Directory.Exists(destDirectoryName))
                                Directory.CreateDirectory(destDirectoryName);
                        }
                        if (File.Exists(destFileName))
                            File.Delete(destFileName);
                        File.Move(sourceFileName, destFileName);
                    }
                    catch (FileNotFoundException fex)
                    {
                        StorServerPubFunc.RecordLogFile(fex);
                        throw new Exception("指定的文件不存在,可能已经被移除.");
                    }
                    catch (IOException ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("操作文件失败." + ex.Message);
                    }
                }
            }
            else
            { StorServerPubFunc.RecordLogFile(sourceFileName + "文件不存在"); }
        }

        public long GetFileLength(string fileName)
        {
            try
            {
                FileInfo file = new FileInfo(fileName);
                if (file.Exists)
                    return file.Length;

                throw new Exception("指定的文件不存在,可能已经被移除.");
            }
            catch (IOException ex)
            {
                StorServerPubFunc.RecordLogFile(ex);
                throw new Exception("获取文件大小失败." + ex.Message);
            }
        }

        public byte[] Read(string fileName)
        {
            try
            {
                byte[] buffer = null;

                using (FileStream stream = File.OpenRead(fileName))
                {
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    stream.Close();
                }

                return buffer;
            }
            catch (FileNotFoundException fex)
            {
                StorServerPubFunc.RecordLogFile(fex);
                throw new Exception("指定的文件不存在,可能已经被移除.");
            }
            catch (IOException ex)
            {
                StorServerPubFunc.RecordLogFile(ex);
                throw new Exception("获取文件大小失败." + ex.Message);
            }
        }

        public byte[] Read(string fileName, int offset, int count)
        {
            try
            {
                byte[] buffer = new byte[count];
                int actualLength = 0;

                using (FileStream stream = File.OpenRead(fileName))
                {
                    actualLength = stream.Read(buffer, offset, count);
                    stream.Close();
                }

                if (actualLength < count)
                {
                    byte[] result = new byte[actualLength];
                    Buffer.BlockCopy(buffer, 0, result, 0, actualLength);

                    return result;
                }

                return buffer;
            }
            catch (FileNotFoundException fex)
            {
                StorServerPubFunc.RecordLogFile(fex);
                throw new Exception("指定的文件不存在,可能已经被移除.");
            }
            catch (IOException ex)
            {
                StorServerPubFunc.RecordLogFile(ex);
                throw new Exception("读取文件失败." + ex.Message);
            }
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="scrname">目前路径</param>
        /// <param name="destname">新的路径</param>
        public void Copy(string scrname, string destname)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(destname)))
                    Directory.CreateDirectory(Path.GetDirectoryName(destname));
                if (File.Exists(scrname) && !File.Exists(destname))
                {
                    System.IO.File.Copy(scrname, destname);
                }
            }
            catch (Exception ex)
            { StorServerPubFunc.RecordLogFile(ex); }
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public void Delete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception ex)
            { StorServerPubFunc.RecordLogFile(ex); }
        }

        /// <summary>
        /// 将整个文件夹复制到目标文件夹中。
        /// </summary>
        /// <param name="srcPath">源文件夹</param>
        /// <param name="aimPath">目标文件夹</param>
        public void CopyDir(string srcPath, string aimPath)
        {
            if (Directory.Exists(srcPath))
            {
                try
                {
                    // 检查目标目录是否以目录分割字符结束如果不是则添加之
                    if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                        aimPath += Path.DirectorySeparatorChar;
                    // 判断目标目录是否存在如果不存在则新建之
                    if (!Directory.Exists(aimPath))
                        Directory.CreateDirectory(aimPath);
                    // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                    // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                    // string[] fileList = Directory.GetFiles(srcPath);
                    string[] fileList = Directory.GetFileSystemEntries(srcPath);
                    // 遍历所有的文件和目录
                    foreach (string file in fileList)
                    {
                        // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                        if (Directory.Exists(file))
                            CopyDir(file, aimPath + Path.GetFileName(file));
                        // 否则直接Copy文件
                        else
                            File.Copy(file, aimPath + Path.GetFileName(file), true);
                    }
                }
                catch(Exception ex)
                {
                    StorServerPubFunc.RecordLogFile(ex);
                    throw new Exception("无法复制!");
                }
            }
        }

        /// <summary>
        /// 将整个文件夹内容删除。
        /// </summary>
        /// <param name="aimPath">目标文件夹</param>
        public void DeleteDirContent(string aimPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加之
                if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                    aimPath += Path.DirectorySeparatorChar;
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                // 如果你指向Delete目标文件下面的文件而不包含目录请使用下面的方法
                // string[] fileList = Directory.GetFiles(aimPath);
                string[] fileList = Directory.GetFileSystemEntries(aimPath);
                // 遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    // 先当作目录处理如果存在这个目录就递归Delete该目录下面的文件
                    if (Directory.Exists(file))
                    {
                        DeleteDir(aimPath + Path.GetFileName(file));
                    }
                    // 否则直接Delete文件
                    else
                    {
                        File.Delete(aimPath + Path.GetFileName(file));
                    }
                }
                //删除文件夹
                //System.IO .Directory .Delete (aimPath,true);
            }
            catch(Exception ex)
            {
                StorServerPubFunc.RecordLogFile(ex);
                throw new Exception("无法删除!");
            }
        }

        /// <summary>
        /// 将整个文件夹删除。
        /// </summary>
        /// <param name="aimPath">目标文件夹</param>
        public void DeleteDir(string aimPath)
        {
            try
            {
                if (Directory.Exists(aimPath))
                    System.IO.Directory.Delete(aimPath, true);
                
            }
            catch(Exception ex)
            {
                StorServerPubFunc.RecordLogFile(ex);
                throw new Exception("无法删除!");
            }
        }

        #endregion

        #region Iconfigure 成员

        public void ShowConfigureWindow(System.Windows.Forms.IWin32Window parent)
        {
            StoreManage sm = new StoreManage();
            sm.Show(parent);
        }

        #endregion

        #region ICommon 成员

        public void ReloadConfigureFile()
        {
            StoreServerConfiguration.Load();
        }

        public string GetLogFileName()
        {
            return StorServerPubFunc.logfile;
        }

        #endregion
    }
}
