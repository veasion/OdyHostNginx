using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OdyHostNginx
{
    public partial class NginxConfigWindows : Form
    {
        public NginxConfigWindows()
        {
            InitializeComponent();
            ConfigDialogData.success = false;
            if (ConfigDialogData.projectName != null && ConfigDialogData.envName != null)
            {
                this.envName.Text = ConfigDialogData.envName;
                this.projectName.Text = ConfigDialogData.projectName;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            ConfigDialogData.projectName = null;
            ConfigDialogData.envName = null;
            ConfigDialogData.path = null;
            ConfigDialogData.success = false;
            this.Close();
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            if (StringHelper.isBlank(this.projectName.Text) || StringHelper.isBlank(this.envName.Text))
            {
                MessageBox.Show("名称不能为空");
                return;
            }
            this.envName.Text = this.envName.Text.Trim();
            this.projectName.Text = this.projectName.Text.Trim();
            if (!Regex.IsMatch(this.projectName.Text + this.envName.Text, "^[0-9a-zA-Z_]{1,}$"))
            {
                MessageBox.Show("名称只能由数字、字母、下划线组成");
                return;
            }
            ConfigDialogData.projectName = this.projectName.Text;
            ConfigDialogData.envName = this.envName.Text;
            ConfigDialogData.success = true;
            string path = OdyConfigHelper.userNginxConfigDir + "\\" + ConfigDialogData.projectName + "\\" + ConfigDialogData.envName;
            if (Directory.Exists(path))
            {
                DialogResult result = MessageBox.Show("该项目环境已存在，相同配置时是否替换？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK)
                {
                    MessageBox.Show("请更换项目或环境名称");
                    return;
                }
            }
            ConfigDialogData.path = path;
            this.Close();
        }

    }
}
