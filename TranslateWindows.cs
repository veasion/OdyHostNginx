using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace OdyHostNginx
{
    public partial class TranslateWindows : Form
    {

        I18nHelper i18n;
        Dictionary<string, string> wordMap = new Dictionary<string, string>();

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
            dialog.Filter = "vue/js文件|*.vue;*.js";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_vueFiles.Lines = dialog.FileNames;
                if (i18n != null)
                {
                    this.textBox_enUs.Text = i18n.getEnJsFilePath(dialog.FileName);
                }
            }
        }

        private void updateText(string text)
        {
            try
            {
                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    this.Text = text;
                });
            }
            catch (Exception)
            {
                this.Text = text;
            }
        }

        private void updateResultText(string text)
        {
            try
            {
                textBox_result.BeginInvoke((MethodInvoker)delegate ()
                {
                    textBox_result.Text = text;
                });
            }
            catch (Exception)
            {
                textBox_result.Text = text;
            }
        }

        private void showMessage(string message)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate ()
            {
                MessageBox.Show(message);
            });
            }
            catch (Exception)
            {
                MessageBox.Show(message);
            }
        }

        private void updateButEnabled(bool enabled)
        {
            but_translate.BeginInvoke((MethodInvoker)delegate ()
            {
                but_translate.Enabled = enabled;
            });
        }

        private void But_translate_Click(object sender, EventArgs e)
        {
            string text = this.Text;
            string enUs = this.textBox_enUs.Text;
            bool merge = this.checkBox_merge.Checked;
            bool filter = this.checkBox_filter.Checked;
            string[] lines = this.textBox_vueFiles.Lines;
            if (!but_translate.Enabled)
            {
                return;
            }
            new Thread(() =>
            {
                doHandle(text, enUs, filter, merge, lines);
            })
            { IsBackground = true }.Start();
        }

        private void doHandle(string text, string enUs, bool filter, bool merge, string[] lines)
        {
            try
            {
                updateButEnabled(false);
                updateText("正在翻译...");
                if (lines == null || lines.Length == 0)
                {
                    showMessage("请选择vue文件");
                    return;
                }
                string enUsContext = null;
                if (filter && !StringHelper.isBlank(enUs))
                {
                    enUsContext = FileHelper.readTextFile(enUs);
                }
                if (!merge)
                {
                    updateResultText("");
                    wordMap = new Dictionary<string, string>();
                }
                foreach (var file in lines)
                {
                    if (!File.Exists(file))
                    {
                        showMessage("文件不存在: " + file);
                        continue;
                    }
                    string context = FileHelper.readTextFile(file);
                    HashSet<string> result = i18n.getPlaceHolders(file, context, true);
                    if (result != null && result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            if (wordMap.ContainsKey(item))
                            {
                                continue;
                            }
                            if (filter && enUsContext != null && i18n.hasKey(enUsContext, item))
                            {
                                continue;
                            }
                            wordMap.Add(item, "");
                        }
                    }
                }
                if (wordMap.Count == 0)
                {
                    if (merge)
                    {
                        showMessage("已存在或无需翻译");
                    }
                    else
                    {
                        updateResultText("已存在或无需翻译");
                    }
                    return;
                }
                updateResultText(i18n.getJSON(wordMap));

                int count = 0;
                List<string> keys = wordMap.Keys.ToList();
                foreach (var key in keys)
                {
                    wordMap[key] = i18n.translate(key);
                    updateResultText(i18n.getJSON(wordMap));
                    updateText("翻译进度 ( " + (++count) + " / " + wordMap.Count + " )");
                    Thread.Sleep(1000);
                }

                updateResultText(i18n.getJSON(wordMap));
            }
            catch (Exception ex)
            {
                Logger.error("翻译", ex);
                showMessage("翻译失败：" + ex.Message);
            }
            finally
            {
                updateText(text);
                updateButEnabled(true);
            }
        }

        private void textBox_KeyDown_SelectAll(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                (sender as TextBox).SelectAll();
        }
    }
}
