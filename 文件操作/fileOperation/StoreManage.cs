using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FrameWorkService.Imp
{
    public partial class StoreManage : Form
    {
        public StoreManage()
        {
            InitializeComponent();
        }

        private void Bt_Confrim_Click(object sender, EventArgs e)
        {

            if (text_Paths.Text.Trim() == string.Empty)
            {
                MessageBox.Show("路径不能为空");
                return;
            }

            if (chk_oss.Checked)
            {
                if (string.IsNullOrEmpty(txtOssAddress.Text))
                {
                    MessageBox.Show("OSS路径不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(txtAkID.Text))
                {
                    MessageBox.Show("AkID不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(txtAkSecret.Text))
                {
                    MessageBox.Show("AkSecret不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(txtStoreName.Text))
                {
                    MessageBox.Show("存储对象名称不能为空");
                    return;
                }
            }
           
            StoreServerConfiguration sc = new StoreServerConfiguration();
            sc.Paths = text_Paths.Text;

            sc.UseOSS = chk_oss.Checked ? "1" : "0";
            sc.OssAddress = txtOssAddress.Text.Trim();
            sc.AccessKeyId = txtAkID.Text.Trim();
            sc.AccessKeySecret = txtAkSecret.Text.Trim();
            sc.StoreName = txtStoreName.Text.Trim();

            if (sc.Save() == 1)
                MessageBox.Show("保存成功");
            else
                MessageBox.Show("保存失败");

        }

        private void Bt_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StoreManage_Load(object sender, EventArgs e)
        {
            StoreServerConfiguration sc = StoreServerConfiguration.Load();
            this.text_Paths.Text = sc.Paths;

            chk_oss.Checked = sc.UseOSS == "1";
            txtAkID.Text = sc.AccessKeyId;
            txtAkSecret.Text = sc.AccessKeySecret;
            txtOssAddress.Text = sc.OssAddress;
            txtStoreName.Text = sc.StoreName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1.ShowDialog();
            this.text_Paths.Text = this.folderBrowserDialog1.SelectedPath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OssClient client = null;
            try
            {
                client = new OssClient(txtOssAddress.Text.Trim(), txtAkID.Text.Trim(), txtAkSecret.Text.Trim());
                lbl_connect_result.Text = "连接成功";

            }
            catch(Exception ex)
            {
                lbl_connect_result.Text = "连接失败";
            }
            finally
            {
            }
        }
    }
}
