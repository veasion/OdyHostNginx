namespace OdyHostNginx
{
    partial class ModifyRequstWindows
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyRequstWindows));
            this.panel_modifys = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel_modifys
            // 
            this.panel_modifys.AutoScroll = true;
            this.panel_modifys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_modifys.Location = new System.Drawing.Point(0, 0);
            this.panel_modifys.Margin = new System.Windows.Forms.Padding(4);
            this.panel_modifys.Name = "panel_modifys";
            this.panel_modifys.Size = new System.Drawing.Size(687, 627);
            this.panel_modifys.TabIndex = 0;
            // 
            // ModifyRequstWindows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 627);
            this.Controls.Add(this.panel_modifys);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ModifyRequstWindows";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modify Requst";
            this.SizeChanged += new System.EventHandler(this.ModifyRequstWindows_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_modifys;
    }
}