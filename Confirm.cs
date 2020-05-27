using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OdyHostNginx
{
    public partial class Confirm : Form
    {

        public string title = "Confirm";
        public string name = "请输入：";
        public string but = "确认";
        public string value = "";

        public Confirm()
        {
            InitializeComponent();
        }

        public string showAndGetResult()
        {
            this.Text = title;
            this.label1.Text = name;
            this.textBox1.Text = value;
            this.button1.Text = but;
            value = null;
            this.ShowDialog();
            return value;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (StringHelper.isBlank(this.textBox1.Text) || this.textBox1.Lines.Length == 0)
            {
                MessageBox.Show("不能为空");
                return;
            }
            value = this.textBox1.Text;
            this.Close();
        }
    }
}
