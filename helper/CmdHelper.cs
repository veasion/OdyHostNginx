using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OdyHostNginx
{
    class CmdHelper
    {

        public static void Cmd(string cmd)
        {
            Cmd(new string[] { cmd });
        }

        public static void Cmd(string[] cmd)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            for (int i = 0; i < cmd.Length; i++)
            {
                p.StandardInput.WriteLine(cmd[i].ToString());
            }
            p.StandardInput.WriteLine("exit");
            p.Close();
        }

        public static bool CloseProcess(string procName)
        {
            bool suc = false;
            Process[] processes = Process.GetProcessesByName(procName);
            if (processes != null && processes.Length > 0)
            {
                foreach (Process p in processes)
                {
                    if (!p.CloseMainWindow())
                    {
                        p.Kill();
                        suc = true;
                    }
                }
            }
            return suc;
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void openDingTalk()
        {
            Process[] processes = Process.GetProcessesByName("DingTalk");
            if (processes != null && processes.Length > 0)
            {
                Process p = processes[0];
                if (p.MainWindowHandle != null)
                {
                    SwitchToThisWindow(p.MainWindowHandle, true);
                    SetForegroundWindow(p.MainWindowHandle);
                    Thread.Sleep(5);
                    SendKeys.SendWait("^+f");
                    Thread.Sleep(50);
                    SendKeys.SendWait("罗卓伟");
                    Thread.Sleep(50);
                    SendKeys.SendWait("{ENTER}");
                }
            }
        }

    }
}
