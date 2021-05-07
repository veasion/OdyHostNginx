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
    public partial class TranslateWindows : Form
    {

        I18nHelper i18n;

        public TranslateWindows()
        {
            InitializeComponent();
            try
            {
                i18n = new I18nHelper();
            }
            catch (Exception e)
            {
                MessageBox.Show("加载i18n配置异常：" + e.Message);
            }
        }

        private void But_select_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "请选择需要翻译的vue文件";
            dialog.Filter = "vue文件(*.vue)|*.vue";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_vueFiles.Lines = dialog.FileNames;
                if (i18n != null)
                {
                    this.textBox_enUs.Text = i18n.getEnJsFilePath(dialog.FileName);
                }
            }
        }

        private void But_translate_Click(object sender, EventArgs e)
        {
            try
            {
                this.Text = "正在翻译...";
                string[] lines = this.textBox_vueFiles.Lines;
                if (lines == null || lines.Length == 0)
                {
                    MessageBox.Show("请选择vue文件");
                    return;
                }
                string enUsContext = null;
                if (!StringHelper.isBlank(this.textBox_enUs.Text))
                {
                    enUsContext = FileHelper.readTextFile(this.textBox_enUs.Text);
                }
                Dictionary<string, string> wordMap = new Dictionary<string, string>();
                foreach (var file in lines)
                {
                    if (!File.Exists(file))
                    {
                        MessageBox.Show("文件不存在: " + file);
                        continue;
                    }
                    string context = FileHelper.readTextFile(file);
                    HashSet<string> result = i18n.getPlaceHolders(context, true);
                    if (result != null && result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            if (!wordMap.ContainsKey(item))
                            {
                                if (enUsContext != null && i18n.hasKey(enUsContext, item))
                                {
                                    continue;
                                }
                                wordMap.Add(item, "");
                            }
                        }
                    }
                }
                if (wordMap.Count == 0)
                {
                    this.textBox_result.Text = "已存在或无需翻译";
                    return;
                }
                this.textBox_result.Text = i18n.getJSON(wordMap);

                int count = 0;
                List<string> keys = wordMap.Keys.ToList();
                foreach (var key in keys)
                {
                    wordMap[key] = i18n.translate(key);
                    this.textBox_result.Text = i18n.getJSON(wordMap);
                    this.Text = "翻译进度(" + (++count) + "/" + wordMap.Count + ")";
                }

                this.textBox_result.Text = i18n.getJSON(wordMap);
            }
            catch (Exception ex)
            {
                Logger.error("翻译", ex);
                MessageBox.Show("翻译失败：" + ex.Message);
            }
            finally
            {
                this.Text = "TranslateWindows";
                this.but_translate.Enabled = true;
            }
        }

        private void textBox_KeyDown_SelectAll(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                (sender as TextBox).SelectAll();
        }
    }
}
