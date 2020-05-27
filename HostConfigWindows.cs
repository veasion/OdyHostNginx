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
    public partial class HostConfigWindows : Form
    {
        public HostConfigWindows()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            ConfigDialogData.hosts = null;
            ConfigDialogData.success = false;
            this.Close();
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            if (StringHelper.isBlank(this.domainText.Text) || this.domainText.Lines.Length == 0)
            {
                MessageBox.Show("host不能为空");
                return;
            }
            List<HostConfig> hosts = new List<HostConfig>();
            string[] lines = this.domainText.Lines;
            for (int i = 0; i < lines.Length; i++)
            {
                string host = lines[i].Trim();
                if ("".Equals(host)) continue;
                int index = host.IndexOf(" ");
                if (index < 0)
                {
                    MessageBox.Show("第" + (i + 1) + "行host格式错误");
                    return;
                }
                string ip = host.Substring(0, index);
                string domain = host.Substring(index).Trim();
                if (StringHelper.isBlank(domain) || StringHelper.isBlank(domain))
                {
                    MessageBox.Show("第" + (i + 1) + "行域名为空");
                    return;
                }
                else if (!StringHelper.isIp(ip))
                {
                    MessageBox.Show("第" + (i + 1) + "行ip错误");
                    return;
                }
                else if (!StringHelper.isDomain(domain))
                {
                    MessageBox.Show("第" + (i + 1) + "行域名错误");
                    return;
                }
                hosts.Add(new HostConfig
                {
                    Ip = ip,
                    Domain = domain
                });
            }
            ConfigDialogData.hosts = hosts;
            ConfigDialogData.success = true;
            this.Close();
        }

        private void DomainText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\x1')
            {
                ((TextBox)sender).SelectAll();
                e.Handled = true;
            }
        }
    }
}
