namespace OdyHostNginx
{
    partial class TranslateWindows
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TranslateWindows));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_vueFiles = new System.Windows.Forms.TextBox();
            this.but_select = new System.Windows.Forms.Button();
            this.textBox_enUs = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.but_translate = new System.Windows.Forms.Button();
            this.textBox_result = new System.Windows.Forms.TextBox();
            this.checkBox_filter = new System.Windows.Forms.CheckBox();
            this.checkBox_merge = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Vue文件";
            // 
            // textBox_vueFiles
            // 
            this.textBox_vueFiles.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_vueFiles.Location = new System.Drawing.Point(84, 12);
            this.textBox_vueFiles.Multiline = true;
            this.textBox_vueFiles.Name = "textBox_vueFiles";
            this.textBox_vueFiles.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_vueFiles.Size = new System.Drawing.Size(432, 40);
            this.textBox_vueFiles.TabIndex = 1;
            this.textBox_vueFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown_SelectAll);
            // 
            // but_select
            // 
            this.but_select.Location = new System.Drawing.Point(531, 15);
            this.but_select.Name = "but_select";
            this.but_select.Size = new System.Drawing.Size(58, 23);
            this.but_select.TabIndex = 2;
            this.but_select.Text = "选择";
            this.but_select.UseVisualStyleBackColor = true;
            this.but_select.Click += new System.EventHandler(this.But_select_Click);
            // 
            // textBox_enUs
            // 
            this.textBox_enUs.Font = new System.Drawing.Font("宋体", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_enUs.Location = new System.Drawing.Point(84, 67);
            this.textBox_enUs.Name = "textBox_enUs";
            this.textBox_enUs.Size = new System.Drawing.Size(432, 19);
            this.textBox_enUs.TabIndex = 4;
            this.textBox_enUs.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown_SelectAll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(15, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "en_US.js";
            // 
            // but_translate
            // 
            this.but_translate.Location = new System.Drawing.Point(269, 107);
            this.but_translate.Name = "but_translate";
            this.but_translate.Size = new System.Drawing.Size(75, 23);
            this.but_translate.TabIndex = 5;
            this.but_translate.Text = "翻译";
            this.but_translate.UseVisualStyleBackColor = true;
            this.but_translate.Click += new System.EventHandler(this.But_translate_Click);
            // 
            // textBox_result
            // 
            this.textBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_result.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_result.Location = new System.Drawing.Point(7, 155);
            this.textBox_result.MaxLength = 327670;
            this.textBox_result.Multiline = true;
            this.textBox_result.Name = "textBox_result";
            this.textBox_result.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_result.Size = new System.Drawing.Size(602, 328);
            this.textBox_result.TabIndex = 6;
            this.textBox_result.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown_SelectAll);
            // 
            // checkBox_filter
            // 
            this.checkBox_filter.AutoSize = true;
            this.checkBox_filter.Checked = true;
            this.checkBox_filter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_filter.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.checkBox_filter.Location = new System.Drawing.Point(538, 65);
            this.checkBox_filter.Name = "checkBox_filter";
            this.checkBox_filter.Size = new System.Drawing.Size(51, 21);
            this.checkBox_filter.TabIndex = 7;
            this.checkBox_filter.Text = "过滤";
            this.checkBox_filter.UseVisualStyleBackColor = true;
            // 
            // checkBox_merge
            // 
            this.checkBox_merge.AutoSize = true;
            this.checkBox_merge.Checked = true;
            this.checkBox_merge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_merge.Location = new System.Drawing.Point(11, 130);
            this.checkBox_merge.Name = "checkBox_merge";
            this.checkBox_merge.Size = new System.Drawing.Size(54, 16);
            this.checkBox_merge.TabIndex = 8;
            this.checkBox_merge.Text = "merge";
            this.checkBox_merge.UseVisualStyleBackColor = true;
            // 
            // TranslateWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 489);
            this.Controls.Add(this.checkBox_merge);
            this.Controls.Add(this.checkBox_filter);
            this.Controls.Add(this.textBox_result);
            this.Controls.Add(this.but_translate);
            this.Controls.Add(this.textBox_enUs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.but_select);
            this.Controls.Add(this.textBox_vueFiles);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TranslateWindows";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "vue国际化翻译";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_vueFiles;
        private System.Windows.Forms.Button but_select;
        private System.Windows.Forms.TextBox textBox_enUs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button but_translate;
        private System.Windows.Forms.TextBox textBox_result;
        private System.Windows.Forms.CheckBox checkBox_filter;
        private System.Windows.Forms.CheckBox checkBox_merge;
    }
}