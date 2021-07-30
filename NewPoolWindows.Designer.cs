namespace OdyHostNginx
{
    partial class NewPoolWindows
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewPoolWindows));
            this.groupBox_envs = new System.Windows.Forms.GroupBox();
            this.textBox_location = new System.Windows.Forms.TextBox();
            this.label_location = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.label_poolName = new System.Windows.Forms.Label();
            this.textBox_poolName = new System.Windows.Forms.TextBox();
            this.label_contextPath = new System.Windows.Forms.Label();
            this.textBox_contextPath = new System.Windows.Forms.TextBox();
            this.button_apply = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_envs
            // 
            this.groupBox_envs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_envs.AutoSize = true;
            this.groupBox_envs.Location = new System.Drawing.Point(12, 13);
            this.groupBox_envs.Name = "groupBox_envs";
            this.groupBox_envs.Size = new System.Drawing.Size(656, 52);
            this.groupBox_envs.TabIndex = 0;
            this.groupBox_envs.TabStop = false;
            this.groupBox_envs.Text = "envs";
            // 
            // textBox_location
            // 
            this.textBox_location.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_location.Location = new System.Drawing.Point(108, 85);
            this.textBox_location.Name = "textBox_location";
            this.textBox_location.Size = new System.Drawing.Size(127, 23);
            this.textBox_location.TabIndex = 2;
            this.textBox_location.Text = "^~ /ouser-web";
            this.textBox_location.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // label_location
            // 
            this.label_location.AutoSize = true;
            this.label_location.Location = new System.Drawing.Point(43, 91);
            this.label_location.Name = "label_location";
            this.label_location.Size = new System.Drawing.Size(59, 12);
            this.label_location.TabIndex = 3;
            this.label_location.Text = "location:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.richTextBox);
            this.panel1.Location = new System.Drawing.Point(21, 131);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(600, 188);
            this.panel1.TabIndex = 4;
            // 
            // richTextBox
            // 
            this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.Size = new System.Drawing.Size(600, 188);
            this.richTextBox.TabIndex = 10;
            this.richTextBox.Text = resources.GetString("richTextBox.Text");
            // 
            // label_poolName
            // 
            this.label_poolName.AutoSize = true;
            this.label_poolName.Location = new System.Drawing.Point(250, 91);
            this.label_poolName.Name = "label_poolName";
            this.label_poolName.Size = new System.Drawing.Size(59, 12);
            this.label_poolName.TabIndex = 6;
            this.label_poolName.Text = "poolName:";
            // 
            // textBox_poolName
            // 
            this.textBox_poolName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_poolName.Location = new System.Drawing.Point(315, 85);
            this.textBox_poolName.Name = "textBox_poolName";
            this.textBox_poolName.Size = new System.Drawing.Size(81, 23);
            this.textBox_poolName.TabIndex = 5;
            this.textBox_poolName.Text = "ouser-web";
            this.textBox_poolName.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // label_contextPath
            // 
            this.label_contextPath.AutoSize = true;
            this.label_contextPath.Location = new System.Drawing.Point(412, 91);
            this.label_contextPath.Name = "label_contextPath";
            this.label_contextPath.Size = new System.Drawing.Size(77, 12);
            this.label_contextPath.TabIndex = 8;
            this.label_contextPath.Text = "contextPath:";
            // 
            // textBox_contextPath
            // 
            this.textBox_contextPath.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_contextPath.Location = new System.Drawing.Point(495, 85);
            this.textBox_contextPath.Name = "textBox_contextPath";
            this.textBox_contextPath.Size = new System.Drawing.Size(93, 23);
            this.textBox_contextPath.TabIndex = 7;
            this.textBox_contextPath.Text = "/ouser-web";
            this.textBox_contextPath.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
            // 
            // button_apply
            // 
            this.button_apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_apply.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_apply.Location = new System.Drawing.Point(576, 315);
            this.button_apply.Name = "button_apply";
            this.button_apply.Size = new System.Drawing.Size(75, 29);
            this.button_apply.TabIndex = 9;
            this.button_apply.Text = "apply";
            this.button_apply.UseVisualStyleBackColor = true;
            this.button_apply.Click += new System.EventHandler(this.Button_apply_Click);
            // 
            // NewPoolWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(680, 351);
            this.Controls.Add(this.button_apply);
            this.Controls.Add(this.label_contextPath);
            this.Controls.Add(this.textBox_contextPath);
            this.Controls.Add(this.label_poolName);
            this.Controls.Add(this.textBox_poolName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label_location);
            this.Controls.Add(this.textBox_location);
            this.Controls.Add(this.groupBox_envs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewPoolWindows";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add New Pool";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_envs;
        private System.Windows.Forms.TextBox textBox_location;
        private System.Windows.Forms.Label label_location;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Label label_poolName;
        private System.Windows.Forms.TextBox textBox_poolName;
        private System.Windows.Forms.Label label_contextPath;
        private System.Windows.Forms.TextBox textBox_contextPath;
        private System.Windows.Forms.Button button_apply;
    }
}