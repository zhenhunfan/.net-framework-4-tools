/**************************************************************************
 * 
 *  2007-08-22,CAIFL,编写存储服务器初始版,主要用于OA系统中的附件的统一存储.
 * 
 * 
 * ***********************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using Aspose.Words;
using Aspose.Words.Drawing;
using FrameWorkService.Interface;
using FrameWorkService.Interface.Basic;
using Microsoft.VisualBasic.FileIO;
using System.Threading;
using System.Collections;
using ServiceContentGisqOAStore;
using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace FrameWorkService.Imp
{
    /// <summary>
    /// 
    /// </summary>
    public class ContentStoreServer :absCommon, IContentFileStore
    {
        private static StoreServerConfiguration ssc = null;
        private static string serverpath = null;
        private static DateTime m_LastInvokeTime = new DateTime();
        private static long m_InvokeTimes = 0;
        private static Thread thread = null;   //用来发送执行文件的转换  
        private static Hashtable m_pdf2swfhash = new Hashtable();              //有效的链接session,如果要验证，以后加一个属性

        private static OssClient client = null;

        /// <summary>
        /// 处理oss相关信息
        /// </summary>
        private void handleOss()
        {
            if (ssc.UseOSS == "1")
            {
                if (string.IsNullOrEmpty(ssc.OssAddress))
                {
                    StorServerPubFunc.RecordLogFile("没有设置OSS地址");
                    throw new Exception("服务问题：没有设置OSS地址!");
                }else if (string.IsNullOrEmpty(ssc.AccessKeyId))
                {
                    StorServerPubFunc.RecordLogFile("没有设置AccessKeyId");
                    throw new Exception("服务问题：没有设置AccessKeyId!");
                }
                else if (string.IsNullOrEmpty(ssc.AccessKeySecret))
                {
                    StorServerPubFunc.RecordLogFile("没有设置AccessKeySecret");
                    throw new Exception("服务问题：没有设置AccessKeySecret!");
                }
                else if (string.IsNullOrEmpty(ssc.StoreName))
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储对象名称");
                    throw new Exception("服务问题：没有设置存储对象名称!");
                }else
                {
                    if (client == null)
                    {
                        client = new OssClient(ssc.OssAddress, ssc.AccessKeyId, ssc.AccessKeySecret);
                    }
                }
            }
        }

        #region IStore 成员

        public long GetStoreSize()
        {
            return -1;
        }
        public ContentStoreServer()
        {           
            if (ssc == null)
            {
                ssc = StoreServerConfiguration.Load();
                serverpath = ssc.Paths;
            }
            if (ssc.UseOSS=="1" && client == null)
            {
                client = new OssClient(ssc.OssAddress, ssc.AccessKeyId, ssc.AccessKeySecret);
            }
        } 

        public string GetStoreRootPhysicalPath() {
            if (ssc.UseOSS == "1")
                return ssc.StoreName;
            return serverpath;
        }

        public string GetPCStoreRootPhysicalPath()
        {
            return serverpath;
        }

        public long GetDriverFreespace()
        {
            

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    try
                    {
                        int firstIndex = serverpath.IndexOf(":");
                        if (firstIndex > 0)
                        {
                            string diskName = serverpath.Substring(0, firstIndex + 1);

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
                    catch (Exception ex)
                    { StorServerPubFunc.RecordLogFile(ex); }

                    throw new Exception("服务问题：获取磁盘可用空间失败,指定的磁盘驱动器不存在.");
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            }
            #endregion

            #region 使用OSS

            return -1;

            #endregion
        }

        public void Write(string fileName, byte[] buffer)
        {
            this.Write(fileName, buffer, 0, true);
        }
        
        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName">文件名</param>s
        /// <param name="buffer">二进制</param>
        /// <param name="append">是否追加</param>
        public void Write(string fileName, byte[] buffer, bool append){
            Write(fileName, buffer, 0, append);
        }

        public void Write(string fileName, byte[] buffer, long offset, bool append)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = fileName;

            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                handleOss();
                fileName = handlePath(fileName);
                if (!append)//假如是新建文档
                {
                    var stream = new MemoryStream(buffer);

                    client.PutObject(ssc.StoreName, fileName, stream);
                }else
                {
                    SyncAppendObject(ssc.StoreName, fileName, buffer);
                }
            }

            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    fileName = serverpath + "\\" + fileName;
                    if (fileName == null)
                        throw new Exception("服务问题：目标文件名不能为空.");

                    if (buffer == null)
                        throw new Exception("服务问题：写入的内容不能为空.");
                    string directoryName = Path.GetDirectoryName(fileName);

                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);

                    try
                    {
                        using (FileStream fs = new FileStream(fileName, offset > 0 ? FileMode.OpenOrCreate : (append ? FileMode.Append : FileMode.OpenOrCreate), FileAccess.Write, FileShare.Write))
                        {
                            if (offset > 0)
                                fs.Seek(offset, SeekOrigin.Begin);
                            fs.Write(buffer, 0, buffer.Length);

                            fs.Flush();

                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (IOException ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new IOException("服务问题：操作文件失败." + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("服务问题：操作文件失败." + ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            }
            #endregion

            EventPath = EventPath.Substring(0, EventPath.LastIndexOf(@"\")+1);
            EventPublic(EventPath);
        }

        /// <summary>
        /// 同步追加文件内容
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="content"></param>
        private void SyncAppendObject(string bucketName,string key,byte[] content)
        {
            long position = 0;
            ulong initCrc = 0;
            try
            {
                var metadata = client.GetObjectMetadata(bucketName, key);
                position = metadata.ContentLength;
                initCrc = ulong.Parse(metadata.Crc64);
            }
            catch (Exception) { }

            try
            {
                var stream = new MemoryStream(content);
                var request = new AppendObjectRequest(bucketName, key)
                {
                    ObjectMetadata = new ObjectMetadata(),
                    Content = stream,
                    Position = position,
                    InitCrc = initCrc
                };

                var result = client.AppendObject(request);
            }
            catch (OssException ex)
            {
                // Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                //     ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                throw new Exception(key + "追加文件内容失败:" + ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(key + "追加文件内容失败:" + ex.Message);
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="sourceFileName">目前位置</param>
        /// <param name="destFileName">新的位置</param>
        public void Move(string sourceFileName, string destFileName)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = destFileName;

            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    sourceFileName = handlePath(sourceFileName);
                    destFileName = handlePath(destFileName);
                    CopyObect(ssc.StoreName, sourceFileName, ssc.StoreName, destFileName);

                    //假如不是重命名，则删除原来的对象
                    if (Path.GetDirectoryName(sourceFileName) != Path.GetDirectoryName(destFileName))
                    {
                        client.DeleteObject(ssc.StoreName, sourceFileName);
                    }
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile(ex);
                    throw ex;
                }
            } 
            
            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    sourceFileName = serverpath + "\\" + sourceFileName;
                    destFileName = serverpath + "\\" + destFileName;
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
                                {
                                    StorServerPubFunc.RecordLogFile(destFileName + "已经存在，无法移动");
                                    throw new Exception("服务问题" + destFileName + "已经存在，无法移动");
                                }
                                File.Move(sourceFileName, destFileName);
                            }
                            catch (FileNotFoundException fex)
                            {
                                StorServerPubFunc.RecordLogFile(fex);
                                throw new FileNotFoundException("服务问题：指定的文件不存在,可能已经被移除.");
                            }
                            catch (IOException iex)
                            {
                                StorServerPubFunc.RecordLogFile(iex);
                                throw new IOException("服务问题：操作文件失败." + iex.Message);
                            }
                            catch (Exception ex)
                            {
                                StorServerPubFunc.RecordLogFile(ex);
                                throw new Exception("服务问题：操作文件失败." + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        StorServerPubFunc.RecordLogFile(sourceFileName + "文件不存在");
                        throw new FileNotFoundException("服务问题：" + sourceFileName + "文件不存在");
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            }
            #endregion

            //发布事件
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (EventPath[EventPath.Length - 1] != Path.DirectorySeparatorChar)
                EventPath += Path.DirectorySeparatorChar;
            EventPublic(EventPath);
        }


        /// <summary>
        /// 移动文件夹 尽量不使用这个方法，因为如果有文件不能移动的话，会只移动了一半文件，引起问题
        /// </summary>
        /// <param name="sourcePath">目前位置</param>
        /// <param name="destPath">新的位置</param>
        public void MoveDir(string sourcePath, string destPath)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = sourcePath;

            #region 使用oss
            if (ssc.UseOSS == "1")
            {
                MoveDirAndDirContent(sourcePath, destPath);
            } 
            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    sourcePath = GetRightPath(serverpath + "\\" + sourcePath + "\\");
                    destPath = GetRightPath(serverpath + "\\" + destPath + "\\");
                    if (Directory.Exists(sourcePath))
                    {
                        if (!sourcePath.Equals(destPath))
                        {
                            try
                            {
                                FileSystem.MoveDirectory(sourcePath, destPath);
                                //Directory.Move(sourcePath, destPath);
                            }
                            catch (FileNotFoundException fex)
                            {
                                StorServerPubFunc.RecordLogFile(fex);
                                throw new FileNotFoundException("服务问题：指定的文件不存在,可能已经被移除.");
                            }
                            catch (IOException iex)
                            {
                                StorServerPubFunc.RecordLogFile(iex);
                                throw new IOException("服务问题：操作文件失败." + iex.Message);
                            }
                            catch (Exception ex)
                            {
                                StorServerPubFunc.RecordLogFile(ex);
                                throw new Exception("服务问题：操作文件失败." + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        StorServerPubFunc.RecordLogFile(sourcePath + "文件夹不存在");
                        throw new FileNotFoundException("服务问题：" + sourcePath + "文件夹不存在");
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            } 
            #endregion

            //发布事件
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (EventPath[EventPath.Length - 1] != Path.DirectorySeparatorChar)
                EventPath += Path.DirectorySeparatorChar;
            EventPublic(EventPath);
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public long GetFileLength(string fileName)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;

            if (ssc.UseOSS == "1")
            {
                handleOss();
                fileName = handlePath(fileName);
                var metadata = client.GetObjectMetadata(ssc.StoreName, fileName);
                return metadata.ContentLength;
            }

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    fileName = serverpath + "\\" + fileName;
                    try
                    {
                        FileInfo file = new FileInfo(fileName);
                        if (file.Exists)
                            return file.Length;

                        throw new FileNotFoundException("服务问题：指定的文件不存在,可能已经被移除.");
                    }
                    catch (IOException ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new IOException("服务问题：获取文件大小失败." + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("服务问题：操作文件失败." + ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("服务问题：没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");

                }
            }
            #endregion

            return -1;
        }

        public byte[] Read(string fileName)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;

            #region 使用OSS
            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    fileName = handlePath(fileName);
                    var obj = client.GetObject(ssc.StoreName, fileName);

                    var buffer = new byte[obj.ContentLength];
                    int actual = 0;
                    var requestStream = obj.Content;

                    //先保存到内存流中MemoryStream  
                    MemoryStream ms = new MemoryStream();

                    var length = (int)(obj.ContentLength < 1024 ? obj.ContentLength : 1024);
                    while ((actual = requestStream.Read(buffer, 0, length)) > 0)
                    {
                        ms.Write(buffer, 0, actual);
                    }

                    ms.Position = 0;

                    //再从内存流中读取到byte数组中  

                    buffer = ms.ToArray();

                    return buffer;
                }
                catch (OssException ex)
                {
                    StorServerPubFunc.RecordLogFile(ex);
                    throw new FileNotFoundException("文件读取失败："+ex.Message);
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile(ex);
                    throw new FileNotFoundException("文件读取失败：" + ex.Message);
                }
            }
            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    fileName = serverpath + "\\" + fileName;
                    try
                    {
                        byte[] buffer = null;

                        using (FileStream stream = File.OpenRead(fileName))
                        {
                            buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);

                            stream.Close();
                            stream.Dispose();
                        }

                        return buffer;
                    }
                    catch (FileNotFoundException fex)
                    {
                        StorServerPubFunc.RecordLogFile(fex);
                        throw new FileNotFoundException("服务问题：指定的文件不存在,可能已经被移除.");
                    }
                    catch (IOException ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new IOException("服务问题：获取文件大小失败." + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("服务问题：操作文件失败." + ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");

                }
            }
            #endregion

            return null;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Read(string fileName, long offset, int count)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    fileName = handlePath(fileName);
                    
                    byte[] buffer = new byte[1024];
                    byte[] result = new byte[count];
                    int resultIndex = 0;
                    var length = 0;
                    var getObjectRequest = new GetObjectRequest(ssc.StoreName, fileName);
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
                    StorServerPubFunc.RecordLogFile(ex);
                    throw new FileNotFoundException("获取文件失败：" + ex.Message);
                }
            }

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    fileName = serverpath + "\\" + fileName;
                    try
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
                    catch (FileNotFoundException fex)
                    {
                        StorServerPubFunc.RecordLogFile(fex);
                        throw new FileNotFoundException("服务问题：指定的文件不存在,可能已经被移除.");
                    }
                    catch (IOException ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new IOException("服务问题：读取文件失败." + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("服务问题：操作文件失败." + ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");

                }
            }
            #endregion
            return null;
        }

        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="scrname">目前文件路径</param>
        /// <param name="destname">新的文件路径</param>
        public void Copy(string scrname, string destname)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = destname;

            if (ssc.UseOSS == "1")
            {
                handleOss();
                scrname = handlePath(scrname);
                destname = handlePath(destname);
                try
                {
                    CopyObect(ssc.StoreName, scrname, ssc.StoreName, destname);
                }catch(Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("复制文件失败："+ex.Message);
                    throw new Exception("复制文件失败：" + ex.Message);
                }
            }

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                scrname = serverpath + "\\" + scrname;
                destname = serverpath + "\\" + destname;
                if (serverpath != null && serverpath != string.Empty)
                {
                    try
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(destname)))
                            Directory.CreateDirectory(Path.GetDirectoryName(destname));
                        //File.Exists(scrname) && 如果已经存在怎么样
                        if (!File.Exists(destname))
                        {
                            System.IO.File.Copy(scrname, destname);
                        }
                        else
                        {
                            StorServerPubFunc.RecordLogFile(scrname + "不存在或者" + destname + "已存在");
                            throw new FileNotFoundException("服务问题：" + scrname + "不存在或者" + destname + "已存在");
                        }
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法拷贝");
                    throw new Exception("服务问题：没有设置存储路径，无法拷贝!");
                }
            } 
            #endregion

            EventPath = EventPath.Substring(0, EventPath.LastIndexOf(@"\")+1);
            EventPublic(EventPath);
        }

        /// <summary>
        /// 判断文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>true为存在 false为不存在</returns>
        public bool IsFileExists(string filePath)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = filePath;

            if (ssc.UseOSS == "1")
            {
                #region 使用OSS
                try
                {
                    handleOss();
                    filePath = handlePath(filePath);
                    return client.DoesObjectExist(ssc.StoreName, filePath);
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile(ex);
                    throw new Exception(ex.Message);
                }
                #endregion
            }
            else
            {
                #region 使用windows文件系统
                if (serverpath != null && serverpath != string.Empty)
                {
                    filePath = serverpath + "\\" + filePath;
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                } 
                #endregion
            }
            EventPath = EventPath.Substring(0, EventPath.LastIndexOf(@"\") + 1);
            EventPublic(EventPath);
        }

        /// <summary>
        /// 移动文件夹和文件夹的内容( 修改文件夹名称 可以用此方法)
        /// </summary>
        /// <param name="srcPath">源路径</param>
        /// <param name="newPath">新路径</param>
        public void MoveDirAndDirContent(string srcPath, string newPath)
        {
            if (!srcPath.Equals(newPath))
            {
                try
                {
                    CopyDir(srcPath, newPath);
                }
                catch (Exception ex)
                { throw new Exception(ex.Message); }
                try
                {
                    
                    DeleteDir(srcPath);
                }
                catch { }
            }
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public void Delete(string path)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = path;

            if (ssc.UseOSS == "1")
            {
                handleOss();
                try
                {
                    path = handlePath(path);
                    client.DeleteObject(ssc.StoreName, path);
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("文件删除失败" + ex.Message);
                    throw new FileNotFoundException("文件删除失败" + ex.Message);
                }
            }
            
            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    path = serverpath + "\\" + path;
                    try
                    {
                        if (File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                        else
                        {
                            StorServerPubFunc.RecordLogFile("文件已经被删除");
                            throw new FileNotFoundException("文件已经被删除!");
                        }
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            }
            #endregion

            EventPath = EventPath.Substring(0, EventPath.LastIndexOf(@"\")+1);
            EventPublic(EventPath);
        }
        /// <summary>
        /// 将整个文件夹复制到目标文件夹中。
        /// </summary>
        /// <param name="srcPath">源文件夹</param>
        /// <param name="aimPath">目标文件夹</param>
        public void CopyDir(string srcPath, string aimPath)
        {   //检查是否是死循环 如a\b复制到a\b\d
            string EventPath = aimPath;
            if (serverpath != null && serverpath != string.Empty)
            {
                srcPath = serverpath + "\\" + srcPath;
                srcPath = GetRightPath(srcPath);
                //srcPath = handlePath(srcPath);
                aimPath = serverpath + "\\" + aimPath;
                aimPath = GetRightPath(aimPath);
                //aimPath = handlePath(aimPath);
                if (aimPath.Contains(srcPath))
                {
                    StorServerPubFunc.RecordLogFile("不能将父目录复制到子文件夹中");
                    throw new Exception("服务问题：不能将父目录复制到子文件夹中!");
                }
                m_LastInvokeTime = System.DateTime.Now;
                m_InvokeTimes++;
                EventPath = aimPath;
                ExecCopyDir(srcPath, aimPath);
            }
            else
            {
                StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                throw new Exception("服务问题：没有设置存储路径，无法保存!");
            }
            //发布事件
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (EventPath[EventPath.Length - 1] != Path.DirectorySeparatorChar)
                EventPath += Path.DirectorySeparatorChar;
            EventPublic(EventPath);
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
            var serverPathTemp = serverpath.Replace("\\", "/");

            if (path.IndexOf(serverPathTemp) > -1)
            {
                path = path.Substring(serverPathTemp.Length);
            }
            path = path.TrimStart('/');
            return path;
        }

        /// <summary>
        /// 将整个文件夹复制到目标文件夹中。
        /// </summary>
        /// <param name="srcPath">源文件夹</param>
        /// <param name="aimPath">目标文件夹</param>
        private void ExecCopyDir(string srcPath, string aimPath)
        {
            if (ssc.UseOSS == "1")
            {
                #region 使用OSS

                handleOss();
                srcPath = handlePath(srcPath);
                if (!srcPath.EndsWith("/"))
                {
                    srcPath += "/";
                }
                aimPath = handlePath(aimPath);
                if (!aimPath.EndsWith("/"))
                {
                    aimPath += "/";
                }
                List<string> keys = new List<string>();
                keys = ListObjectsWithSummari(keys,ssc.StoreName, srcPath);
                
                foreach (string file in keys)
                {
                    if (file.Substring(file.Length-1,1) != "/")
                        CopyObect(ssc.StoreName, file, ssc.StoreName, aimPath + (file.Substring(srcPath.Length)));
                }
                
                #endregion
            }
            else
            {
                #region 使用windows文件系统
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
                        //string[] fileList = Directory.GetFileSystemEntries(srcPath);
                        //string[] DirectoryList = Directory.GetDirectories(srcPath);
                        string[] fileList = Directory.GetFileSystemEntries(srcPath);
                        // 遍历所有的文件和目录
                        foreach (string file in fileList)
                        {
                            // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                            if (Directory.Exists(file))
                                ExecCopyDir(file, aimPath + Path.GetFileName(file));
                            // 否则直接Copy文件
                            else
                                File.Copy(file, aimPath + Path.GetFileName(file), true);
                        }
                        //FileSystem.CopyDirectory(srcPath, aimPath);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("附件不存在" + srcPath);
                    throw new FileNotFoundException("服务问题：附件不存在" + srcPath);
                }
                #endregion
            }
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="sourceBucket">原文件所在存储空间的名称</param>
        /// <param name="sourceKey">原文件的名称</param>
        /// <param name="targetBucket">目标文件所在存储空间的名称</param>
        /// <param name="targetKey">目标文件的名称</param>
        private void CopyObect(string sourceBucket, string sourceKey, string targetBucket, string targetKey)
        {
            try
            {
                var metadata = new ObjectMetadata();
                metadata.AddHeader(Aliyun.OSS.Util.HttpHeaders.ContentType, "text/html");
                sourceKey = handlePath(sourceKey);
                targetKey = handlePath(targetKey);
                targetBucket = handlePath(targetBucket);
                var req = new CopyObjectRequest(sourceBucket, sourceKey, targetBucket, targetKey)
                {
                    NewObjectMetadata = metadata
                };
                var ret = client.CopyObject(req);
            }
            catch (Exception ex)
            {
                StorServerPubFunc.RecordLogFile(sourceKey + "文件复制失败!" + ex.Message);
                throw new Exception(sourceKey + "文件复制失败!" + ex.Message);
            }
        }

        /// <summary>
        /// 列出指定存储空间下其Key以prefix为前缀的文件的摘要信息OssObjectSummary
        /// 获取该目录下的文件及目录下的文件
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        /// <param name="prefix">限定返回的文件必须以此作为前缀</param>  
        private List<string> ListObjectsWithSummari(List<string> SummariList, string bucketName, string prefix)
        {
            try
            {
                List<string> keys = new List<string>();
                if (prefix.Substring(0, 1) == "/")
                    prefix = prefix.Substring(1, prefix.Length-1);
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Prefix = prefix,
                    Delimiter = "/"
                };
                var result = client.ListObjects(listObjectsRequest);
                
                foreach (var summary in result.ObjectSummaries)
                {
                    SummariList.Add(summary.Key);
                }
                foreach (var prefixes in result.CommonPrefixes)
                {
                    ListObjectsWithSummari(SummariList,bucketName, prefixes);
                }
                return SummariList;
            }
            catch (Exception ex)
            {
                StorServerPubFunc.RecordLogFile(prefix + "文件夹遍历失败!" + ex.Message);
                throw new Exception(prefix + "文件夹遍历失败!" + ex.Message);
            }
        }

        /// <summary>
        /// 列出指定存储空间下其Key以prefix为前缀的子目录的摘要信息OssObjectPrefix
        /// </summary>
        /// <param name="bucketName">存储空间的名称</param>
        /// <param name="prefix">限定返回的文件必须以此作为前缀</param>  
        private List<string> ListObjectsWithPrefix(string bucketName, string prefix)
        {
            try
            {
                List<string> keys = new List<string>();
                if (prefix.Substring(0, 1) == "\\")
                    prefix = prefix.Substring(1, prefix.Length - 1);
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Prefix = prefix,
                    Delimiter = "/"
                };
                var result = client.ListObjects(listObjectsRequest);

                foreach (var prefixes in result.CommonPrefixes)
                {
                    keys.Add(prefixes);
                }
                return keys;
            }
            catch (Exception ex)
            {
                StorServerPubFunc.RecordLogFile(prefix + "文件夹遍历失败!" + ex.Message);
                throw new Exception(prefix + "文件夹遍历失败!" + ex.Message);
            }
        }

        /// <summary>
        /// 获取文件夹下文件的个数
        /// </summary>
        /// <param name="srcPath">文件夹路径</param>
        /// <returns>子文件个数</returns>
        public int GetFileCount(string srcPath)
        {
            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    srcPath = handlePath(srcPath);
                    List<string> keys = new List<string>();
                    keys = ListObjectsWithSummari(keys,ssc.StoreName, srcPath);
                    return keys.Count;
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("获取文件夹下文件的个数失败:" + ex.Message);
                    throw new FileNotFoundException("获取文件夹下文件的个数失败:" + ex.Message);
                }
            }

            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    srcPath = serverpath + "\\" + srcPath;
                    srcPath = GetRightPath(srcPath);
                    if (Directory.Exists(srcPath))
                    {
                        try
                        {
                            string[] fileList = Directory.GetFileSystemEntries(srcPath);
                            return fileList.Length;
                        }
                        catch (Exception ex)
                        {
                            StorServerPubFunc.RecordLogFile(ex);
                            throw new Exception(ex.Message);
                        }
                    }
                    else
                    {
                        StorServerPubFunc.RecordLogFile("附件不存在" + srcPath);
                        throw new FileNotFoundException("服务问题：附件不存在" + srcPath);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            }
            #endregion

            return -1;
        }

        /// <summary>
        /// 获取文件的目录名称
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetFileDirName(string fileName)
        {
            fileName = fileName.Replace("/", "\\");
            var _lastIndex = fileName.LastIndexOf("\\");
            return fileName.Substring(0, _lastIndex);
        }

        /// <summary>
        /// 将整个文件夹内容删除。
        /// </summary>
        /// <param name="aimPath">目标文件夹</param>
        public void DeleteDirContent(string aimPath)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = aimPath;

            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    aimPath = handlePath(aimPath);
                    List<string> keys = new List<string>();
                    keys = ListObjectsWithSummari(keys, ssc.StoreName, aimPath);

                    foreach (string key in keys)
                    {
                        client.DeleteObject(ssc.StoreName, key);
                    }
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("OSS删除文件夹失败:" + ex.Message);
                    throw new FileNotFoundException("OSS删除文件夹失败:" + ex.Message);
                }
            } 

            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    aimPath = serverpath + "\\" + aimPath;
                    aimPath = GetRightPath(aimPath);
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
                                DeleteDir(EventPath + Path.GetFileName(file));
                            }
                            // 否则直接Delete文件
                            else
                            {
                                File.Delete(EventPath + Path.GetFileName(file));
                            }
                        }
                        //删除文件夹
                        //System.IO .Directory .Delete (aimPath,true);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex);
                        throw new Exception("服务问题：" + ex.Message);
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                    throw new Exception("服务问题：没有设置存储路径，无法保存!");
                }
            } 
            #endregion
            
            //发布事件
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (EventPath[EventPath.Length - 1] != Path.DirectorySeparatorChar)
                EventPath += Path.DirectorySeparatorChar;
            EventPublic(EventPath);
        }

        /// <summary>
        /// 将整个文件夹删除。
        /// </summary>
        /// <param name="aimPath">目标文件夹</param>
        public void DeleteDir(string aimPath)
        {
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            string EventPath = aimPath;

            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    aimPath = handlePath(aimPath);
                    List<string> keys = new List<string>();
                    keys = ListObjectsWithSummari(keys, ssc.StoreName, aimPath);

                    foreach (string key in keys)
                    {
                        client.DeleteObject(ssc.StoreName, key);
                    }
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("OSS删除文件夹失败:" + ex.Message);
                    throw new FileNotFoundException("OSS删除文件夹失败:" + ex.Message);
                }
            } 

            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                if (!HaveSubFolder(aimPath))
                {
                    m_LastInvokeTime = System.DateTime.Now;
                    m_InvokeTimes++;
                    if (serverpath != null && serverpath != string.Empty)
                    {
                        aimPath = serverpath + "\\" + aimPath;
                        aimPath = GetRightPath(aimPath);
                        try
                        {
                            if (Directory.Exists(aimPath))
                                System.IO.Directory.Delete(aimPath, true);
                            else
                            {
                                StorServerPubFunc.RecordLogFile(aimPath + "已经不存在");
                                throw new FileNotFoundException("服务问题：" + aimPath + "已经不存在");

                            }

                        }
                        catch (Exception ex)
                        {
                            StorServerPubFunc.RecordLogFile(ex);
                            throw new Exception("服务问题：" + ex.Message);
                        }
                    }
                    else
                    {
                        StorServerPubFunc.RecordLogFile("存储路径为（" + serverpath + "）");
                        throw new Exception("服务问题：没有设置存储路径!");
                    }
                }
                else
                {
                    StorServerPubFunc.RecordLogFile(aimPath + "下存在子文件夹，无法删除");
                    throw new Exception("服务问题：" + aimPath + "下存在子文件夹，无法删除");

                }
            } 
            #endregion

            //发布事件
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (EventPath[EventPath.Length - 1] != Path.DirectorySeparatorChar)
                EventPath += Path.DirectorySeparatorChar;
            EventPublic(EventPath);
        }

        /// <summary>
        /// 判断目录下是否有子文件夹，true标识有子文件夹
        /// </summary>
        /// <param name="aimPath"></param>
        /// <returns></returns>
        public bool HaveSubFolder(string aimPath)
        {
            int i = 0;

            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    aimPath = handlePath(aimPath);

                    var keys = ListObjectsWithPrefix(ssc.StoreName, aimPath);

                    foreach (string key in keys)
                    {
                        if (Path.GetDirectoryName(key) != aimPath)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("OSS判断目录下是否有子文件夹失败："+ex.Message);
                    throw new Exception("OSS判断目录下是否有子文件夹失败：" + ex.Message);
                }
            }

            #endregion

            #region 使用windwos文件系统
            if (ssc.UseOSS != "1")
            {
                if (serverpath != null && serverpath != string.Empty)
                {
                    try
                    {
                        aimPath = serverpath + "\\" + aimPath;
                        aimPath = GetRightPath(aimPath);
                        // 检查目标目录是否以目录分割字符结束如果不是则添加之
                        if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                            aimPath += Path.DirectorySeparatorChar;
                        // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                        // 如果你指向Delete目标文件下面的文件而不包含目录请使用下面的方法
                        // string[] fileList = Directory.GetFiles(aimPath);
                        if (Directory.Exists(Path.GetDirectoryName(aimPath)))
                        {
                            string[] fileList = Directory.GetFileSystemEntries(aimPath);
                            // 遍历所有的文件和目录
                            foreach (string file in fileList)
                            {
                                if (Directory.Exists(file))
                                {
                                    i++;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            StorServerPubFunc.RecordLogFile(aimPath + "不存在");
                            throw new FileNotFoundException("服务问题：" + aimPath + "不存在");
                        }

                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(ex.ToString());
                        throw new Exception("服务问题：" + ex.Message);
                    }
                    if (i != 0)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            #endregion

            return false;
        }

        /// <summary>
        /// 判断目录下是存在此文件
        /// </summary>
        /// <param name="aimPath"></param>
        /// <returns></returns>
        public bool HaveFile(string aimPath)
        {

            #region 使用OSS

            if (ssc.UseOSS == "1")
            {
                try
                {
                    handleOss();
                    aimPath = handlePath(aimPath);

                    return client.DoesObjectExist(ssc.StoreName, aimPath);
                }
                catch (Exception ex)
                {
                    StorServerPubFunc.RecordLogFile("OSS判断目录下是存在此文件失败：" + ex.Message);
                    throw new Exception("OSS判断目录下是存在此文件失败：" + ex.Message);
                }
            }

            #endregion

            #region 使用windows文件系统
            if (ssc.UseOSS != "1")
            {
                int i = 0;
                if (serverpath != null && serverpath != string.Empty)
                {
                    try
                    {
                        aimPath = serverpath + "\\" + aimPath;

                        if (!File.Exists(aimPath))
                        {
                            return false;
                        }
                        else
                        { return true; }

                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
            #endregion

            return false;
        }

        /// <summary>
        /// 获取正确的目录
        /// </summary>
        /// <param name="parent"></param>
        private string GetRightPath(string oldpath)
        {
            return Path.GetDirectoryName(oldpath + "\\") + "\\";
        }

        /// <summary>
        /// tif转pdf
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void Tif2Pdf(string fileName)
        {
            try
            {
                //假如使用OSS，首先下载到本地
                var _fileOrigin = serverpath + "\\" + fileName;
                if (ssc.UseOSS == "1")
                {
                    handleOss();
                    var filePathOss = handlePath(fileName);

                    var result = client.GetObject(ssc.StoreName, filePathOss);

                    using (var requestStream = result.Content)
                    {
                        var directoryName = GetFileDirName(_fileOrigin);
                        if (!Directory.Exists(directoryName))
                            Directory.CreateDirectory(directoryName);
                        using (var fs = File.Open(_fileOrigin, FileMode.OpenOrCreate))
                        {
                            int length = 4 * 1024;
                            var buf = new byte[length];
                            do
                            {
                                length = requestStream.Read(buf, 0, length);
                                fs.Write(buf, 0, length);
                            } while (length != 0);
                        }
                    }
                }

                
                var _index = _fileOrigin.LastIndexOf(".", StringComparison.Ordinal);
                var _fileDest = _fileOrigin.Substring(0, _index) + ".pdf";
                ConvertImageToPdf(_fileOrigin, _fileDest);

                //处理完后再把文件上传到OSS
                if (ssc.UseOSS == "1")
                {
                    client.PutObject(ssc.StoreName, handlePath(_fileDest.Substring(serverpath.Length)), _fileDest);
                    File.Delete(_fileDest);
                    File.Delete(_fileOrigin);
                }
            }
            catch(Exception _ex)
            {
                StorServerPubFunc.RecordLogFile(_ex);
            }
        }

        /// <summary>
        /// ceb转pdf
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void Ceb2Pdf(string fileName)
        {
            try
            {
                //假如使用OSS，首先下载到本地
                if (ssc.UseOSS == "1")
                {
                    handleOss();
                    var filePathOss = handlePath(fileName);

                    var result = client.GetObject(ssc.StoreName, filePathOss);

                    using (var requestStream = result.Content)
                    {
                        using (var fs = File.Open(serverpath + "\\" + fileName, FileMode.OpenOrCreate))
                        {
                            int length = 4 * 1024;
                            var buf = new byte[length];
                            do
                            {
                                length = requestStream.Read(buf, 0, length);
                                fs.Write(buf, 0, length);
                            } while (length != 0);
                        }
                    }
                }

                var _fileOrigin = serverpath + "\\" + fileName;
                var _index = _fileOrigin.LastIndexOf(".", StringComparison.Ordinal);
                var _fileDest = _fileOrigin.Substring(0, _index) + ".pdf";
                C2F.OpenDoc(_fileOrigin, _fileDest);

                //处理完后再把文件上传到OSS
                if (ssc.UseOSS == "1")
                {
                    client.PutObject(ssc.StoreName, handlePath(_fileDest.Substring(serverpath.Length)), _fileDest);
                    File.Delete(_fileDest);
                    File.Delete(_fileOrigin);
                }
            }
            catch (Exception _ex)
            {
                StorServerPubFunc.RecordLogFile(_ex);
            }
        }

        /// <summary>
        /// Converts an image to PDF using Aspose.Words for .NET.
        /// </summary>
        /// <param name="inputFileName">File name of input image file.</param>
        /// <param name="outputFileName">Output PDF file name.</param>
        private void ConvertImageToPdf(string inputFileName, string outputFileName)
        {
            // Create Aspose.Words.Document and DocumentBuilder. 
            // The builder makes it simple to add content to the document.
            var _doc = new Document();
            var _builder = new DocumentBuilder(_doc);

            // Read the image from file, ensure it is disposed.
            using (Image _image = Image.FromFile(inputFileName))
            {
                // Get the number of frames in the image.
                int _framesCount = _image.GetFrameCount(FrameDimension.Page);

                // Loop through all frames.
                for (int _frameIdx = 0; _frameIdx < _framesCount; _frameIdx++)
                {
                    // Insert a section break before each new page, in case of a multi-frame TIFF.
                    if (_frameIdx != 0)
                        _builder.InsertBreak(BreakType.SectionBreakNewPage);

                    // Select active frame.
                    _image.SelectActiveFrame(FrameDimension.Page, _frameIdx);

                    // We want the size of the page to be the same as the size of the image.
                    // Convert pixels to points to size the page to the actual image size.
                    PageSetup ps = _builder.PageSetup;
                    ps.PageWidth = ConvertUtil.PixelToPoint(_image.Width, _image.HorizontalResolution);
                    ps.PageHeight = ConvertUtil.PixelToPoint(_image.Height, _image.VerticalResolution);

                    // Insert the image into the document and position it at the top left corner of the page.
                    _builder.InsertImage(
                        _image,
                        RelativeHorizontalPosition.Page,
                        0,
                        RelativeVerticalPosition.Page,
                        0,
                        ps.PageWidth,
                        ps.PageHeight,
                        WrapType.None);
                }
            }

            // Save the document to PDF.
            _doc.Save(outputFileName);
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public FileInfo GetFileInfo(string filePath)
        {
            FileInfo fi = null;
            string fileName = "";

            //假如用了OSS
            if (ssc.UseOSS == "1")
            {
                handleOss();
                var filePathOss = handlePath(filePath);

                var result = client.GetObject(ssc.StoreName, filePathOss);

                using (var requestStream = result.Content)
                {
                    using (var fs = File.Open(serverpath+"\\"+filePath, FileMode.OpenOrCreate))
                    {
                        int length = 4 * 1024;
                        var buf = new byte[length];
                        do
                        {
                            length = requestStream.Read(buf, 0, length);
                            fs.Write(buf, 0, length);
                        } while (length != 0);
                    }
                }
            }

            if (serverpath != null && serverpath != string.Empty)
            {
                fileName = serverpath + "\\" + filePath;
                fi = new FileInfo(fileName);
            }
            return fi;
        }

        #endregion

        #region Iconfigure 成员

        public override void ShowConfigureWindow(System.Windows.Forms.IWin32Window parent)
        {
            StoreManage sm = new StoreManage();
            sm.ShowDialog(parent);
        }

        #endregion

        #region ICommon 成员

        public override  void ReloadConfigureFile()
        {
            StoreServerConfiguration.Load();
        }

        public override string GetLogFileName()
        {
            return AppDomain.CurrentDomain.BaseDirectory + StorServerPubFunc.logfile;
        }

        public override DateTime LastInvokeTime
        {
            get
            {
                return m_LastInvokeTime;
            }
        }
        public override long InvokeTimes
        {
            get { return m_InvokeTimes; }
        }
        #endregion

        #region 发布事件
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="strPath">发布事件的参数，文件路径</param>
        private void EventPublic(string strPath)
        {
            try
            {
                StorServerPubFunc.ServiceContentFactory.EventPublic(this, new SerivceEventArgs(strPath, "文件存储事件", null));
            }
            catch(Exception obje)
            {                
                StorServerPubFunc.RecordLogFile("事件发布失败!");
                StorServerPubFunc.RecordLogFile(obje);
            }
        }
        #endregion

        #region IContentFileStore 成员
        //
        #region 文件转成成swf格式
        /// <summary>
        /// 文件格式转换，转成成swf格式，在后台转换
        /// </summary>
        /// <param name="filePath"></param>
        public void Pdf2Swf(string fileName)
        {
            return;
            m_LastInvokeTime = System.DateTime.Now;
            m_InvokeTimes++;
            if (serverpath != null && serverpath != string.Empty)
            {                
                if (fileName.Substring(fileName.LastIndexOf(".")).ToUpper() != ".PDF")
                {
                    throw new Exception("不是pdf文件不能转换!");
                }
            }
            else
            {
                StorServerPubFunc.RecordLogFile("没有设置存储路径，无法保存");
                throw new Exception("服务问题：没有设置存储路径，无法保存!");
            }

            //加入到hash队列中
            if (!m_pdf2swfhash.Contains(fileName))
            {
                lock (m_pdf2swfhash.SyncRoot)
                {
                    m_pdf2swfhash.Add(fileName, fileName);
                }
            }

            //调用后台的交换线程
            if (thread == null || ( //等待转换程序时是 WaitSleepJoin状态
                thread.ThreadState != ThreadState.Running && thread.ThreadState != ThreadState.WaitSleepJoin
                ))
            {
                //运行中，不用再启动                   
                thread = new Thread(new ThreadStart(this.exePdf2Swf));
                thread.Priority = ThreadPriority.Lowest;
                thread.Start();
            }     
            
        }
        #endregion

        #region 后台线程的执行
        /// <summary>
        /// 后台线程的执行
        /// </summary>
        public void exePdf2Swf()
        {
            string sourceFile = "";// @"E:\工作目录\学习文档\17技术专题\2014-08-20统一在线显示文档\File2swf\bin\Debug\SWFTool\result.pdf";
            string targetFile = "";// @"E:\工作目录\学习文档\17技术专题\2014-08-20统一在线显示文档\File2swf\bin\Debug\SWFTool\result.swf";
            string fileName = "";
            List<string> sessionList = new List<string>();

            while (m_pdf2swfhash.Count > 0)
            {
                lock (m_pdf2swfhash.SyncRoot) //锁定防止新的值插入
                {
                    System.Collections.IDictionaryEnumerator enumerator = m_pdf2swfhash.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        sessionList.Add(enumerator.Key.ToString());
                    }
                }
                for (int i = sessionList.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        fileName = sessionList[i].ToString();
                        sourceFile = serverpath + "\\" + fileName;
                        targetFile = serverpath + "\\" + fileName.Substring(0, fileName.LastIndexOf(".") + 1) + "swf";


                        StorServerPubFunc.ConvertToSwf(sourceFile, targetFile);
                    }
                    catch (Exception ex)
                    {
                        StorServerPubFunc.RecordLogFile(fileName + "转换成swf失败");
                        StorServerPubFunc.RecordLogFile(ex);
                    }
                    //删除集合中的内容
                    lock (m_pdf2swfhash.SyncRoot)
                    {
                        m_pdf2swfhash.Remove(fileName);
                    }
                }
            }

        }
        #endregion

        #region 获取文件中的文本内容
        /// <summary>
        /// 获取文件中的文本内容的方法，暂只处理.doc/.docx/.wps/.pdf
        /// <para>返回：文件中的文本，程序异常或各种失败均返回null</para>
        /// <para>注：本方法会自动将多个空格、多个tab键与多个换行符替换成单个空格（单个存在的不替换）。</para>
        /// </summary>
        /// <param name="fileName">文件的相对路径（相对于服务所在位置）（包含部分路径与文件名以及扩展名）</param>
        /// <returns></returns>
        public string GetFileText(string fileName)
        {
            string result = null;

            try
            {
                m_LastInvokeTime = System.DateTime.Now;
                m_InvokeTimes++;

                //假如使用了OSS，则先把文件下载到原来的serverpath+filename目录下
                if (ssc.UseOSS == "1")
                {
                    handleOss();
                    var filePathOss = handlePath(fileName);

                    var resultOss = client.GetObject(ssc.StoreName, filePathOss);

                    using (var requestStream = resultOss.Content)
                    {
                        using (var fs = File.Open(serverpath + "\\" + fileName, FileMode.OpenOrCreate))
                        {
                            int length = 4 * 1024;
                            var buf = new byte[length];
                            do
                            {
                                length = requestStream.Read(buf, 0, length);
                                fs.Write(buf, 0, length);
                            } while (length != 0);
                        }
                    }
                }

                if (string.IsNullOrEmpty(serverpath) == false)
                {
                    //to lower.
                    fileName =serverpath.ToLower()+"\\"+ fileName.ToLower();

                    //exist.
                    if (File.Exists(fileName))
                    {
                        //expend name.
                        string kExpendName = Path.GetExtension(fileName);
                        if (string.IsNullOrEmpty(kExpendName) == false)
                        {
                            //pointing direction.
                            if (kExpendName == ".doc" || kExpendName == ".docx" || kExpendName == ".wps")
                            {
                                Aspose.Words.Document doc = new Aspose.Words.Document(fileName);
                                result = doc.GetText();

                            }
                            else if (kExpendName == ".pdf")
                            {
                                Aspose.Pdf.Document doc = new Aspose.Pdf.Document(fileName);
                                Aspose.Pdf.Text.TextAbsorber tab = new Aspose.Pdf.Text.TextAbsorber();//no why why handle like this because these code is just copy from API of Aspose.
                                doc.Pages.Accept(tab);
                                doc.Dispose();

                                result = tab.Text;
                            }
                            else
                            {
                                result = null;
                                StorServerPubFunc.RecordLogFile("暂不支持你指定的文件格式，当前仅支持以下格式：.doc/.docx/.wps/.pdf");
                            }

                            string regStr = @"\s{2,}";//match to more than one of these char: " " \t \n 
                            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(regStr);

                            result = reg.Replace(result, " ");
                        }
                        else
                        {
                            result = null;
                            StorServerPubFunc.RecordLogFile("文件路径不是一个有效的文件路径！");
                        }
                    }
                    else
                    {
                        result = null;
                        StorServerPubFunc.RecordLogFile("文件不存在！");
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
                StorServerPubFunc.RecordLogFile("获取文件中的文本时异常，详细：" + ex.Message);
            }

            return result;
        }
        #endregion

        //
        #endregion


      
    }
}
