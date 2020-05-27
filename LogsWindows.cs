using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OdyHostNginx
{
    public partial class LogsWindows : Form
    {

        Thread t;
        SSHConnInfo conn;
        SshClient ssh = null;
        bool isOpen = false;
        bool isSuspend = false;

        public LogsWindows()
        {
            isOpen = false;
            InitializeComponent();
            FormClosed += (sender, e) => { Stop(); };
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        public LogsWindows(Logger.LoggerHandler loggerHandler)
        {
            isOpen = true;
            InitializeComponent();
            FormClosed += (sender, e) =>
            {
                isOpen = false;
                Logger.logHandler = null;
            };
            this.WindowState = FormWindowState.Normal;
            this.Show();
            WriteLine("Listening logs...");
            Control.CheckForIllegalCrossThreadCalls = false;
            Logger.logHandler = new Logger.LoggerHandler((log) =>
            {
                if (isOpen)
                {
                    WriteLine(log);
                }
                loggerHandler?.Invoke(log);
            });
        }

        public string openAndGetCommand(NginxUpstream up) {
            this.Text = up.ContextPath;
            conn = SSHClientHelper.getConnInfo(up);
            ssh = SSHClientHelper.open(conn);
            isOpen = ssh.IsConnected;
            return SSHClientHelper.tryGetCommand(ssh, conn);
        }

        public void showLogs(string command)
        {
            this.Show();
            string terminalName = "xterm-256color";
            uint columns = 80;
            uint rows = 160;
            uint width = 80;
            uint height = 160;
            int bufferSize = 500;

            var actual = ssh.CreateShellStream(terminalName, columns, rows, width, height, bufferSize, null);

            actual.WriteLine(command);

            t = new Thread(() =>
            {
                try
                {
                    while (isOpen)
                    {
                        while (!isSuspend && actual.CanRead)
                        {
                            WriteLine(actual.ReadLine());
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.error(e);
                }
            });
            t.Start();
        }

        private void WriteLine(string line)
        {
            this.logText.AppendText("\r\n" + line);
        }

        public void Stop()
        {
            isOpen = false;
            SSHClientHelper.close(ssh);
            t = null;
        }

        private void ControlBut_Click(object sender, EventArgs e)
        {
            isSuspend = !isSuspend;
            this.controlBut.Text = isSuspend ? "[继续]" : "[暂停]";
        }

        private void SaveLable_Click(object sender, EventArgs e)
        {
            string path = FileHelper.getDesktopDirectory() + "\\log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            if (FileHelper.writeFile(path, this.logText.Text))
            {
                MessageBox.Show("保存成功！\r\n\r\n路径：" + path);
            }
        }

    }
}
