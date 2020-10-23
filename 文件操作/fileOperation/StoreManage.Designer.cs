namespace FrameWorkService.Imp
{
    partial class StoreManage
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbl_connect_result = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.txtStoreName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtAkSecret = new System.Windows.Forms.TextBox();
            this.AkSecret = new System.Windows.Forms.Label();
            this.txtAkID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOssAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chk_oss = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.text_Paths = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Bt_Exit = new System.Windows.Forms.Button();
            this.Bt_Confrim = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbl_connect_result);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.txtStoreName);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtAkSecret);
            this.groupBox1.Controls.Add(this.AkSecret);
            this.groupBox1.Controls.Add(this.txtAkID);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtOssAddress);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chk_oss);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.text_Paths);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.Bt_Exit);
            this.groupBox1.Controls.Add(this.Bt_Confrim);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(393, 333);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "文件存储服务";
            // 
            // lbl_connect_result
            // 
            this.lbl_connect_result.AutoSize = true;
            this.lbl_connect_result.Location = new System.Drawing.Point(93, 281);
            this.lbl_connect_result.Name = "lbl_connect_result";
            this.lbl_connect_result.Size = new System.Drawing.Size(28, 13);
            this.lbl_connect_result.TabIndex = 41;
            this.lbl_connect_result.Text = "ver1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 275);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 25);
            this.button2.TabIndex = 40;
            this.button2.Text = "OSS连接测试";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtStoreName
            // 
            this.txtStoreName.Location = new System.Drawing.Point(110, 230);
            this.txtStoreName.Name = "txtStoreName";
            this.txtStoreName.Size = new System.Drawing.Size(250, 20);
            this.txtStoreName.TabIndex = 39;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 230);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 38;
            this.label5.Text = "存储对象名称：";
            // 
            // txtAkSecret
            // 
            this.txtAkSecret.Location = new System.Drawing.Point(86, 182);
            this.txtAkSecret.Name = "txtAkSecret";
            this.txtAkSecret.Size = new System.Drawing.Size(274, 20);
            this.txtAkSecret.TabIndex = 37;
            // 
            // AkSecret
            // 
            this.AkSecret.AutoSize = true;
            this.AkSecret.Location = new System.Drawing.Point(14, 193);
            this.AkSecret.Name = "AkSecret";
            this.AkSecret.Size = new System.Drawing.Size(63, 13);
            this.AkSecret.TabIndex = 36;
            this.AkSecret.Text = "AkSecret：";
            // 
            // txtAkID
            // 
            this.txtAkID.Location = new System.Drawing.Point(68, 144);
            this.txtAkID.Name = "txtAkID";
            this.txtAkID.Size = new System.Drawing.Size(292, 20);
            this.txtAkID.TabIndex = 35;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 34;
            this.label4.Text = "AkID：";
            // 
            // txtOssAddress
            // 
            this.txtOssAddress.Location = new System.Drawing.Point(68, 105);
            this.txtOssAddress.Name = "txtOssAddress";
            this.txtOssAddress.Size = new System.Drawing.Size(292, 20);
            this.txtOssAddress.TabIndex = 33;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "OSS地址：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "使用OSS：";
            // 
            // chk_oss
            // 
            this.chk_oss.AutoSize = true;
            this.chk_oss.Location = new System.Drawing.Point(68, 72);
            this.chk_oss.Name = "chk_oss";
            this.chk_oss.Size = new System.Drawing.Size(15, 14);
            this.chk_oss.TabIndex = 30;
            this.chk_oss.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(290, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(70, 22);
            this.button1.TabIndex = 29;
            this.button1.Text = "存储路径";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // text_Paths
            // 
            this.text_Paths.Location = new System.Drawing.Point(68, 28);
            this.text_Paths.Name = "text_Paths";
            this.text_Paths.Size = new System.Drawing.Size(216, 20);
            this.text_Paths.TabIndex = 28;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "存储路径：";
            // 
            // Bt_Exit
            // 
            this.Bt_Exit.Location = new System.Drawing.Point(290, 276);
            this.Bt_Exit.Name = "Bt_Exit";
            this.Bt_Exit.Size = new System.Drawing.Size(70, 25);
            this.Bt_Exit.TabIndex = 26;
            this.Bt_Exit.Text = "退出";
            this.Bt_Exit.UseVisualStyleBackColor = true;
            this.Bt_Exit.Click += new System.EventHandler(this.Bt_Exit_Click);
            // 
            // Bt_Confrim
            // 
            this.Bt_Confrim.Location = new System.Drawing.Point(214, 276);
            this.Bt_Confrim.Name = "Bt_Confrim";
            this.Bt_Confrim.Size = new System.Drawing.Size(70, 25);
            this.Bt_Confrim.TabIndex = 25;
            this.Bt_Confrim.Text = "确定";
            this.Bt_Confrim.UseVisualStyleBackColor = true;
            this.Bt_Confrim.Click += new System.EventHandler(this.Bt_Confrim_Click);
            // 
            // StoreManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 373);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StoreManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "文件存储服务配置界面";
            this.Load += new System.EventHandler(this.StoreManage_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button Bt_Exit;
        private System.Windows.Forms.Button Bt_Confrim;
        private System.Windows.Forms.TextBox text_Paths;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox txtOssAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chk_oss;
        private System.Windows.Forms.TextBox txtAkID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtAkSecret;
        private System.Windows.Forms.Label AkSecret;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtStoreName;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lbl_connect_result;
    }
}