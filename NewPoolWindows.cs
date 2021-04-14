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
    public partial class NewPoolWindows : Form
    {
        private bool update;
        private string upstream;
        private List<CheckBox> checkBoxs;
        private Dictionary<CheckBox, EnvConfig> checkBoxMap;
        private string defUpstreamConfig = "upstream #{poolName}-domain-#{envName} {\nserver #{adminIp}:80;\n}";
        private string defAdminConfig = "    location #{location} {\n        proxy_pass http://#{poolName}-domain-#{envName}#{contextPath};\n        proxy_set_header   Host    $host:$server_port;\n        proxy_set_header   X-Real-IP   $remote_addr;\n        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;\n        break;\n    }";

        public NewPoolWindows(OdyProjectConfig odyProjectConfig)
        {
            update = false;
            InitializeComponent();
            checkBoxs = new List<CheckBox>();
            checkBoxMap = new Dictionary<CheckBox, EnvConfig>();
            CheckBox checkBox = new CheckBox
            {
                Text = "base",
                Checked = true
            };
            checkBoxs.Add(checkBox);
            checkBoxMap.Add(checkBox, null);
            foreach (var project in odyProjectConfig.Projects)
            {
                foreach (var env in project.Envs)
                {
                    if (OdyConfigHelper.isOdyDockerEnv(env))
                    {
                        checkBox = new CheckBox
                        {
                            Text = env.EnvName,
                            Checked = true
                        };
                        checkBoxs.Add(checkBox);
                        checkBoxMap.Add(checkBox, env);
                    }
                }
            }
            int x = 10;
            for (int i = 0; i < checkBoxs.Count; i++)
            {
                checkBoxs[i].Location = new Point(x, 25);
                this.groupBox_envs.Controls.Add(checkBoxs[i]);
                x += checkBoxs[i].Width + 10;
            }
            this.TextBox_TextChanged(null, null);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            string location = this.textBox_location.Text.Trim();
            string poolName = this.textBox_poolName.Text.Trim();
            string contextPath = this.textBox_contextPath.Text.Trim();
            this.upstream = defUpstreamConfig.Replace("#{poolName}", poolName);
            this.richTextBox.Text = defAdminConfig
                .Replace("#{location}", location)
                .Replace("#{poolName}", poolName)
                .Replace("#{contextPath}", contextPath);
        }

        private void Button_apply_Click(object sender, EventArgs e)
        {
            this.TextBox_TextChanged(null, null);
            string location = this.textBox_location.Text.Trim();
            string poolName = this.textBox_poolName.Text.Trim();
            string contextPath = this.textBox_contextPath.Text.Trim();
            if (StringHelper.isBlank(location) || StringHelper.isBlank(poolName) || StringHelper.isBlank(contextPath))
            {
                MessageBox.Show("必填项不能为空？");
                return;
            }
            if (StringHelper.hasChinese(poolName) &&
                MessageBox.Show("poolName不能存在中文，配置错误是否继续执行？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == DialogResult.Cancel)
            {
                return;
            }
            if (MessageBox.Show("确定在所选环境中新增pool吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == DialogResult.Cancel)
            {
                return;
            }
            if (!location.Contains("/") &&
                MessageBox.Show("location可能配置错误是否继续执行？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == DialogResult.Cancel)
            {
                return;
            }
            if (!contextPath.StartsWith("/") &&
                MessageBox.Show("contextPath可能配置错误是否继续执行？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == DialogResult.Cancel)
            {
                return;
            }
            foreach (var check in checkBoxs)
            {
                if (!check.Checked)
                {
                    continue;
                }
                EnvConfig env = checkBoxMap[check];
                string[] confgPath;
                string nginxConfig, upstreamConfig, upstreamName;
                if (env == null)
                {
                    // base
                    upstreamName = this.textBox_poolName.Text.Trim() + "-domain-#name";
                    nginxConfig = this.richTextBox.Text.Replace("#{envName}", "#name");
                    upstreamConfig = this.upstream.Replace("#{envName}", "#name").Replace("#{adminIp}", "#adminIp");
                    confgPath = new string[] { OdyConfigHelper.odyConfigBackDir };
                }
                else
                {
                    // env
                    string adminIp = env.Upstreams[0].OldIp;
                    upstreamName = this.textBox_poolName.Text.Trim() + "-domain-" + env.EnvName;
                    nginxConfig = this.richTextBox.Text.Replace("#{envName}", env.EnvName);
                    upstreamConfig = this.upstream.Replace("#{envName}", env.EnvName).Replace("#{adminIp}", adminIp);
                    confgPath = new string[] {
                        OdyConfigHelper.nginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName,
                        OdyConfigHelper.userNginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName
                    };
                }
                foreach (var path in confgPath)
                {
                    if (!Directory.Exists(path))
                    {
                        Logger.info("环境新Pool保存失败，路径不存在: " + path);
                        continue;
                    }
                    addNewPool(check.Text, path, nginxConfig, upstreamConfig, location, upstreamName);
                }

            }
            MessageBox.Show("执行完毕！");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void addNewPool(string envName, string path, string nginxConfig, string upstreamConfig, string location, string upstreamName)
        {
            Logger.info(path);
            Logger.info(nginxConfig);
            Logger.info(upstreamConfig);
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                string context = FileHelper.readTextFile(file, WindowsNginxImpl.confEncoding);
                if (context.Contains("server_name") && context.Contains("proxy_pass "))
                {
                    // nginx
                    if (context.Contains(location))
                    {
                        MessageBox.Show(envName + "中已存在" + location);
                        continue;
                    }
                    int idx = context.LastIndexOf("}");
                    if (idx == -1)
                    {
                        MessageBox.Show(envName + "配置异常: " + file);
                        continue;
                    }
                    string conf = context.Substring(0, idx) + "\n" + nginxConfig + "\n" + context.Substring(idx);
                    FileHelper.writeFile(file, WindowsNginxImpl.confEncoding, conf);
                    update = true;
                }
                else if (context.Contains("upstream ") && context.Contains("server "))
                {
                    // upstream
                    if (context.Contains(upstreamName))
                    {
                        Logger.info(file + "已存在" + upstreamName);
                        continue;
                    }
                    FileHelper.writeFile(file, WindowsNginxImpl.confEncoding, context + "\n" + upstreamConfig);
                    update = true;
                }
                else
                {
                    Logger.info("未知文件: " + file);
                }
            }
        }

        public bool IsUpdate()
        {
            return update;
        }
    }
}
