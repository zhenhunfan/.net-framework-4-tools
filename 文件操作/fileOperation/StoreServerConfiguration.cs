using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace FrameWorkService.Imp
{
    public  class StoreServerConfiguration
    {
        private string m_Paths;//文件存储路径
        public string Paths
        {
            get { return m_Paths; }
            set { m_Paths = value; }
        }

        private string m_UseOSS;
        /// <summary>
        /// 使用OSS
        /// </summary>
        public string UseOSS
        {
            get { return m_UseOSS; }
            set { m_UseOSS = value; }
        }

        private string m_Oss_Address;
        /// <summary>
        /// OSS地址
        /// </summary>
        public string OssAddress
        {
            get { return m_Oss_Address; }
            set { m_Oss_Address = value; }
        }

        private string m_AccessKeyId;
        /// <summary>
        /// OSS的AKID
        /// </summary>
        public string AccessKeyId
        {
            get { return m_AccessKeyId; }
            set { m_AccessKeyId = value; }
        }

        private string m_AccessKeySecret;
        /// <summary>
        /// OSS的AKSecret
        /// </summary>
        public string AccessKeySecret
        {
            get { return m_AccessKeySecret; }
            set { m_AccessKeySecret = value; }
        }

        private string m_StoreName;
        /// <summary>
        /// OSS的AKSecret
        /// </summary>
        public string StoreName
        {
            get { return m_StoreName; }
            set { m_StoreName = value; }
        }

        /// <summary>
        /// 加载配置实例
        /// </summary>
        /// <returns></returns>
        public static StoreServerConfiguration Load()
        {
            StoreServerConfiguration instance = new StoreServerConfiguration();
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(StoreServerConfiguration));
                StreamReader sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + @"ConfigXml\StoreServerConfiguration.Config");
                instance = xs.Deserialize(sr) as StoreServerConfiguration;
                sr.Close();
                sr.Dispose();
            }
            catch
            { }
            return instance;

        }
        /// <summary>
        /// 保存配置
        /// </summary>
        public int Save()
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(StoreServerConfiguration));
                string path = AppDomain.CurrentDomain.BaseDirectory + @"ConfigXml\StoreServerConfiguration.Config";
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                StreamWriter sw = new StreamWriter(path);
                xs.Serialize(sw, this);
                sw.Close();
                sw.Dispose();
                return 1;
            }
            catch { return 0; }
        }
    }
}
