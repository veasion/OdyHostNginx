using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

    }
}
