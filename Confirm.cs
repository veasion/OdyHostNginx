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
    public delegate string CofirmParamCheck(List<string> values);

    public partial class Confirm : Form
    {

        private bool isClick;
        private ConfigShowParam param;
        private List<TextBox> textBoxes;

        public Confirm(ConfigShowParam param)
        {
            this.param = param;
            this.isClick = false;
            this.textBoxes = new List<TextBox>();
            InitializeComponent();
        }

        public List<string> showAndGetResult()
        {
            this.Text = param.Title;
            this.button.Text = param.Button;
            int top = 30, kvTop = 5, kvHeight = 40, bottom = 30;
            int kvPanelHeight = top + (kvTop + kvHeight) * param.Keys.Count + bottom;
            this.Height = kvPanelHeight + this.panel1.Height + 10;
            this.kvPanel.Height = kvPanelHeight;
            for (int i = 0; i < param.Keys.Count; i++)
            {
                string key = param.Keys[i];
                string value = "";
                if (param.Values != null && param.Values.Count > i)
                {
                    value = param.Values[i];
                }

                Label label = new Label();
                label.Text = key;
                label.Height = kvHeight;
                label.AutoSize = true;

                TextBox textBox = new TextBox();
                textBox.Text = value;
                textBox.Height = kvHeight;
                textBox.Width = 150;

                int left = (this.Width - label.Width - textBox.Width) / 2 - 10;
                if (left < 20)
                {
                    left = 20;
                }
                else if (left > 100)
                {
                    left = 100;
                }
                label.Location = new Point(left, top + 5 + (kvTop + kvHeight) * i);
                textBox.Location = new Point(label.Width + left + 10, top + (kvTop + kvHeight) * i);

                textBoxes.Add(textBox);
                this.kvPanel.Controls.Add(label);
                this.kvPanel.Controls.Add(textBox);
            }
            this.ShowDialog();
            if (!isClick)
            {
                return null;
            }
            List<string> values = new List<string>();
            foreach (var item in textBoxes)
            {
                values.Add(StringHelper.isBlank(item.Text) ? null : item.Text);
            }
            return values;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            if (param.Check != null)
            {
                List<string> values = new List<string>();
                foreach (var item in textBoxes)
                {
                    values.Add(StringHelper.isBlank(item.Text) ? null : item.Text);
                }
                string error = param.Check(values);
                if (error != null)
                {
                    MessageBox.Show(error);
                    return;
                }
            }
            else
            {
                foreach (var item in textBoxes)
                {
                    if (StringHelper.isBlank(item.Text))
                    {
                        MessageBox.Show("值不能为空");
                        return;
                    }
                }
            }
            this.isClick = true;
            this.Close();
        }

    }
}
