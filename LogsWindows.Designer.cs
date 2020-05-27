namespace OdyHostNginx
{
    partial class LogsWindows
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogsWindows));
            this.logText = new System.Windows.Forms.TextBox();
            this.saveLable = new System.Windows.Forms.Label();
            this.controlBut = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // logText
            // 
            this.logText.BackColor = System.Drawing.SystemColors.WindowText;
            this.logText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logText.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.logText.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.logText.Location = new System.Drawing.Point(0, 0);
            this.logText.MaxLength = 327670;
            this.logText.Multiline = true;
            this.logText.Name = "logText";
            this.logText.ReadOnly = true;
            this.logText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logText.Size = new System.Drawing.Size(1092, 551);
            this.logText.TabIndex = 0;
            // 
            // saveLable
            // 
            this.saveLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.saveLable.AutoSize = true;
            this.saveLable.BackColor = System.Drawing.SystemColors.WindowText;
            this.saveLable.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveLable.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.saveLable.Location = new System.Drawing.Point(1007, 9);
            this.saveLable.Name = "saveLable";
            this.saveLable.Size = new System.Drawing.Size(53, 15);
            this.saveLable.TabIndex = 2;
            this.saveLable.Text = "[保存]";
            this.saveLable.Click += new System.EventHandler(this.SaveLable_Click);
            // 
            // controlBut
            // 
            this.controlBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.controlBut.AutoSize = true;
            this.controlBut.BackColor = System.Drawing.SystemColors.ControlText;
            this.controlBut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.controlBut.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.controlBut.Location = new System.Drawing.Point(948, 9);
            this.controlBut.Name = "controlBut";
            this.controlBut.Size = new System.Drawing.Size(53, 15);
            this.controlBut.TabIndex = 0;
            this.controlBut.Text = "[暂停]";
            this.controlBut.Click += new System.EventHandler(this.ControlBut_Click);
            // 
            // LogsWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1092, 551);
            this.Controls.Add(this.saveLable);
            this.Controls.Add(this.controlBut);
            this.Controls.Add(this.logText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogsWindows";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "logs";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox logText;
        private System.Windows.Forms.Label controlBut;
        private System.Windows.Forms.Label saveLable;
    }
}