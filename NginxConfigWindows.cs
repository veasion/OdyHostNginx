using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OdyHostNginx
{
    public partial class NginxConfigWindows : Form
    {
        public NginxConfigWindows()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            NginxConfigData.projectName = null;
            NginxConfigData.envName = null;
            NginxConfigData.path = null;
            NginxConfigData.import = false;
            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            NginxConfigData.projectName = this.projectName.Text;
            NginxConfigData.envName = this.envName.Text;
            NginxConfigData.import = true;
            string path = OdyConfigHelper.userNginxConfigDir + "\\" + NginxConfigData.projectName + "\\" + NginxConfigData.envName;
            if (Directory.Exists(path))
            {
                DialogResult result = MessageBox.Show("该项目环境已存在，是否替换？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK)
                {
                    MessageBox.Show("请更换项目或环境名称");
                    return;
                }
            }
            NginxConfigData.path = path;
            this.Close();
        }

    }
}
