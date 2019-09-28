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
            ConfigDialogData.domain = null;
            ConfigDialogData.ip = null;
            ConfigDialogData.success = false;
            this.Close();
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            if (StringHelper.isBlank(this.ipText.Text) || StringHelper.isBlank(this.domainText.Text))
            {
                MessageBox.Show("domain / ip cannot be empty");
                return;
            }
            else if (!StringHelper.isIp(this.ipText.Text))
            {
                MessageBox.Show("ip error");
                return;
            }
            else if (!StringHelper.isDomain(this.domainText.Text))
            {
                MessageBox.Show("domain error");
                return;
            }
            ConfigDialogData.ip = this.ipText.Text;
            ConfigDialogData.domain = this.domainText.Text;
            ConfigDialogData.success = true;
            this.Close();
        }

    }
}
