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
    public delegate void ModifyRequstExecutor(List<ModifyRequstBean> modifys);

    public partial class ModifyRequstWindows : Form
    {
        private ToolTip toolTip = new ToolTip();
        private List<ModifyRequstBean> list;
        private ModifyRequstExecutor executor;

        public ModifyRequstWindows(List<ModifyRequstBean> init, ModifyRequstExecutor executor)
        {
            this.executor = executor;
            if (init != null)
            {
                this.list = new List<ModifyRequstBean>(init);
            }
            else
            {
                this.list = new List<ModifyRequstBean>();
            }
            this.Width = this.Width < 705 ? 705 : this.Width;
            this.Height = this.Width < 674 ? 674 : this.Width;
            InitializeComponent();
            this.drawing();
        }

        private void ModifyRequstWindows_SizeChanged(object sender, EventArgs e)
        {
            drawing();
        }

        private void Button_apply_Click(object sender, EventArgs e)
        {
            this.executor(list);
        }

        public void updateData(List<ModifyRequstBean> update)
        {
            list = new List<ModifyRequstBean>(update);
            drawing();
        }

        private void drawing()
        {
            this.panel_modifys.Controls.Clear();
            List<Panel> panels = new List<Panel>();
            int panelY = 12;
            for (int i = 0; i < list.Count; i++)
            {
                ModifyRequstBean item = list[i];
                Panel panel = this.buildPanel(item, i);
                int panelX = (this.panel_modifys.Width - panel.Width) / 2;
                panel.Location = new Point(panelX, panelY);
                panelY = panel.Location.Y + panel.Height + 15;
                panels.Add(panel);
            }
            foreach (var panel in panels)
            {
                this.panel_modifys.Controls.Add(panel);
            }
            // add
            Label label_add = new Label
            {
                Text = "[+]",
                AutoSize = true,
                Cursor = Cursors.Hand,
                ForeColor = Color.DodgerBlue,
                Font = new Font("微软雅黑", 14.25f)
            };
            int labelX = (this.panel_modifys.Width - 30) / 2;
            label_add.Location = new Point(labelX, panelY);
            label_add.Click += Label_add_Click;
            this.panel_modifys.Controls.Add(label_add);

            // apply
            Button button_apply = new Button();
            button_apply.Text = "apply";
            button_apply.Size = new Size(100, 30);
            button_apply.Click += Button_apply_Click;
            int applyWidth = this.Width - button_apply.Width - 50;
            int applyHeight = this.Height - 100;
            if (panelY > applyHeight)
            {
                applyHeight = panelY;
            }
            button_apply.Location = new Point(applyWidth, applyHeight);
            this.panel_modifys.Controls.Add(button_apply);
        }

        private Panel buildPanel(ModifyRequstBean item, int index)
        {

            Panel panel = new Panel
            {
                Width = 640,
                Height = 425 - (item.InterceptType == 0 ? 0 : 80),
                BorderStyle = BorderStyle.FixedSingle
            };

            panel.Controls.Add(new Label
            {
                Text = "拦截",
                AutoSize = true,
                Location = new Point(16, 16)
            });

            // intercept
            ComboBox cbx_intercept = new ComboBox
            {
                Size = new Size(125, 23),
                Location = new Point(73, 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbx_intercept.Tag = item;
            cbx_intercept.Items.AddRange(new string[] { "请求", "响应" });
            cbx_intercept.DataBindings.Add("SelectedIndex", item, "interceptType");
            cbx_intercept.SelectedIndexChanged += Cbx_intercept_SelectedIndexChanged;
            panel.Controls.Add(cbx_intercept);

            panel.Controls.Add(new Label
            {
                Text = "URL",
                AutoSize = true,
                Location = new Point(333, 16)
            });

            // match
            ComboBox cbx_match = new ComboBox
            {
                Size = new Size(80, 23),
                Location = new Point(373, 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbx_match.Items.AddRange(new string[] { "包含", "等于", "正则" });
            cbx_match.DataBindings.Add("SelectedIndex", item, "matchType");
            panel.Controls.Add(cbx_match);

            // matchText
            TextBox textBox_matchText = new TextBox();
            textBox_matchText.Size = new Size(132, 25);
            textBox_matchText.Location = new Point(461, 11);
            textBox_matchText.DataBindings.Add("Text", item, "matchText");
            textBox_matchText.KeyDown += textBox_KeyDown_SelectAll;
            textBox_matchText.MouseEnter += (object sender, EventArgs e) =>
            {
                toolTip.Show((sender as TextBox).Text, sender as TextBox);
            };
            panel.Controls.Add(textBox_matchText);

            // delete
            Label label_delete = new Label
            {
                Text = "X",
                AutoSize = true,
                Cursor = Cursors.Hand,
                ForeColor = Color.Red,
                Font = new Font("微软雅黑", 9),
                Location = new Point(608, 5),
                Name = "label_delete_" + index
            };
            label_delete.Click += Label_del_Click;
            panel.Controls.Add(label_delete);

            int margin = 10;
            int y = cbx_intercept.Location.Y + cbx_intercept.Height + margin;

            // params
            if (item.InterceptType == 0)
            {
                GroupBox groupBox_parmas = new GroupBox();
                groupBox_parmas.Text = "params";
                groupBox_parmas.Size = new Size(595, 80);
                groupBox_parmas.Location = new Point(15, y);
                panel.Controls.Add(groupBox_parmas);
                y += groupBox_parmas.Height + margin;

                TextBox textBox_params = new TextBox();
                textBox_params.Size = new Size(groupBox_parmas.Width - 20, 25);
                textBox_params.Location = new Point(10, (groupBox_parmas.Height - textBox_params.Height) / 2);
                textBox_params.DataBindings.Add("Text", item, "paramsStr");
                textBox_params.KeyDown += textBox_KeyDown_SelectAll;
                textBox_params.MouseEnter += (object sender, EventArgs e) =>
                {
                    toolTip.Show((sender as TextBox).Text, sender as TextBox);
                };
                groupBox_parmas.Controls.Add(textBox_params);
            }

            // headers
            GroupBox groupBox_headers = new GroupBox();
            groupBox_headers.Text = "header";
            groupBox_headers.Size = new Size(595, 120);
            groupBox_headers.Location = new Point(15, y);
            Panel panel_headers = new Panel();
            panel_headers.AutoScroll = true;
            panel_headers.Dock = DockStyle.Fill;
            panel_headers.HorizontalScroll.Enabled = false;
            panel_headers.HorizontalScroll.Visible = false;
            groupBox_headers.Controls.Add(panel_headers);
            y += groupBox_headers.Height + margin;
            this.buildHeaders(panel_headers, item, index);
            panel.Controls.Add(groupBox_headers);

            // body/response
            GroupBox groupBox_body = new GroupBox();
            groupBox_body.Text = item.InterceptType == 0 ? "body" : "response";
            groupBox_body.Size = new Size(595, 150);
            groupBox_body.Location = new Point(15, y);
            panel.Controls.Add(groupBox_body);

            RichTextBox richTextBox_body = new RichTextBox();
            richTextBox_body.Dock = DockStyle.Fill;
            richTextBox_body.DataBindings.Add("Text", item, "body");
            groupBox_body.Controls.Add(richTextBox_body);

            return panel;
        }

        private void Cbx_intercept_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = (sender as ComboBox).SelectedIndex;
            ModifyRequstBean item = (sender as ComboBox).Tag as ModifyRequstBean;
            if (selectedIndex != item.InterceptType)
            {
                item.InterceptType = selectedIndex;
                drawing();
            }
        }

        private void buildHeaders(Panel parent, ModifyRequstBean item, int index)
        {
            parent.Controls.Clear();
            int kvY = 10;
            if (item.Headers != null)
            {
                kvY = 5;
                foreach (var key in item.Headers.Keys)
                {
                    Dictionary<string, object> tag = new Dictionary<string, object>
                    {
                        { "key", key },
                        { "item", item },
                        { "index", index },
                        { "parent",parent }
                    };

                    Panel panel_kv = new Panel();
                    panel_kv.Tag = "panel_kv";
                    panel_kv.Size = new Size(parent.Width - 30, 30);
                    panel_kv.Location = new Point(5, kvY);
                    panel_kv.BorderStyle = BorderStyle.None;
                    kvY = panel_kv.Location.Y + panel_kv.Height + 5;

                    int textWidth = 180;
                    int controlX = (panel_kv.Width - 35) / 2 - textWidth - 8;
                    if (controlX < 0) controlX = 0;

                    TextBox textBox_key = new TextBox();
                    textBox_key.Text = key;
                    textBox_key.Tag = tag;
                    textBox_key.Location = new Point(controlX, 0);
                    textBox_key.Size = new Size(textWidth, panel_kv.Height);
                    textBox_key.KeyDown += textBox_KeyDown_SelectAll;
                    textBox_key.TextChanged += TextBox_kv_TextChanged;
                    textBox_key.MouseEnter += (object sender, EventArgs e) =>
                    {
                        toolTip.Show((sender as TextBox).Text, sender as TextBox);
                    };
                    controlX = textBox_key.Width + textBox_key.Location.X + 10;

                    Label label_ky_eq = new Label
                    {
                        Text = ":",
                        AutoSize = true,
                        Font = new Font("微软雅黑", 9),
                        Location = new Point(controlX, 2),
                        Name = "label_ky_eq_" + index
                    };
                    controlX = label_ky_eq.Location.X + 25;

                    TextBox textBox_val = new TextBox();
                    textBox_val.Tag = tag;
                    textBox_val.Text = item.Headers[key];
                    textBox_val.Location = new Point(controlX, 0);
                    textBox_val.Size = new Size(textWidth, panel_kv.Height);
                    textBox_val.KeyDown += textBox_KeyDown_SelectAll;
                    textBox_val.TextChanged += TextBox_kv_TextChanged;
                    textBox_val.MouseEnter += (object sender, EventArgs e) =>
                    {
                        toolTip.Show((sender as TextBox).Text, sender as TextBox);
                    };
                    controlX = textBox_val.Location.X + textWidth + 20;

                    Label label_ky_del = new Label
                    {
                        Tag = tag,
                        Text = "X",
                        AutoSize = true,
                        Cursor = Cursors.Hand,
                        ForeColor = Color.Brown,
                        Font = new Font("微软雅黑", 9),
                        Location = new Point(controlX, 2),
                        Name = "label_ky_eq_" + index
                    };
                    label_ky_del.Click += Label_ky_del_Click;

                    panel_kv.Controls.Add(textBox_key);
                    panel_kv.Controls.Add(label_ky_eq);
                    panel_kv.Controls.Add(textBox_val);
                    panel_kv.Controls.Add(label_ky_del);

                    parent.Controls.Add(panel_kv);
                }
            }
            // add
            Label label_ky_add = new Label
            {
                Text = "[+]",
                AutoSize = true,
                Cursor = Cursors.Hand,
                ForeColor = Color.SteelBlue,
                Font = new Font("微软雅黑", 9)
            };
            label_ky_add.Tag = new Dictionary<string, object>
            {
                { "item", item },
                { "index", index },
                { "parent",parent }
            };
            int labelX = (parent.Width - 35) / 2;
            label_ky_add.Location = new Point(labelX, kvY);
            label_ky_add.Click += Label_ky_add_Click;
            parent.Controls.Add(label_ky_add);
        }

        private void textBox_KeyDown_SelectAll(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                (sender as TextBox).SelectAll();
        }

        private void Label_add_Click(object sender, EventArgs e)
        {
            ModifyRequstBean init = new ModifyRequstBean();
            init.Headers = new Dictionary<string, string>();
            init.Headers.Add("Content-Type", "application/json;charset=utf-8");
            list.Add(init);
            this.drawing();
        }

        private void Label_del_Click(object sender, EventArgs e)
        {
            string name = (sender as Label).Name;
            int index = int.Parse(name.Substring(name.LastIndexOf("_") + 1));
            list.RemoveAt(index);
            this.drawing();
        }

        private void TextBox_kv_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Dictionary<string, object> tag = textBox.Tag as Dictionary<string, object>;
            int index = (int)tag["index"];
            Panel parent = (Panel)tag["parent"];
            list[index].Headers = getHeaders(parent);
        }

        private Dictionary<string, string> getHeaders(Panel panelHeaders)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (var item in panelHeaders.Controls)
            {
                if (item is Panel && "panel_kv".Equals((item as Panel).Tag))
                {
                    string key = null, value = null;
                    Panel panel_kv = item as Panel;
                    foreach (var s in panel_kv.Controls)
                    {
                        if (s is TextBox && (s as TextBox).Tag != null)
                        {
                            TextBox textBox = s as TextBox;
                            if (key == null)
                            {
                                key = textBox.Text.Trim();
                            }
                            else
                            {
                                value = textBox.Text.Trim();
                            }
                        }
                    }
                    map[key] = value;
                }
            }
            return map;
        }

        private void Label_ky_add_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> tag = (sender as Label).Tag as Dictionary<string, object>;
            int index = (int)tag["index"];
            Panel parent = (Panel)tag["parent"];
            ModifyRequstBean item = (ModifyRequstBean)tag["item"];
            list[index].Headers = getHeaders(parent);
            if (!list[index].Headers.ContainsKey(""))
            {
                list[index].Headers.Add("", "");
            }
            this.buildHeaders(parent, item, index);
        }

        private void Label_ky_del_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> tag = (sender as Label).Tag as Dictionary<string, object>;
            string key = (string)tag["key"];
            int index = (int)tag["index"];
            Panel parent = (Panel)tag["parent"];
            ModifyRequstBean item = (ModifyRequstBean)tag["item"];
            list[index].Headers.Remove(key);
            this.buildHeaders(parent, item, index);
        }
    }
}
