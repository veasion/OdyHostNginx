using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace OdyHostNginx
{
    class WindowsLocalHostImpl : SwitchHost
    {

        public static Encoding hostsEncoding = Encoding.ASCII;
        public static string flushdnsCmd = "ipconfig /flushdns";
        public static string hostsPath = @"C:\Windows\System32\drivers\etc\hosts";

        public void switchHost(string domain, string ip, bool enable)
        {
            if (StringHelper.isBlank(domain))
            {
                return;
            }
            if (enable && StringHelper.isBlank(ip))
            {
                return;
            }
            switchHost(new string[] { domain }, new string[] { ip }, enable);
        }

        public void switchHost(string[] domains, string[] ips, bool enable)
        {
            string check = checkDomains(domains, ips, enable);
            if (check != null)
            {
                throw new ServiceException(check);
            }
            try
            {
                // 更改 host
                bool suc = updateHost(domains, ips, enable);
                if (!suc)
                {
                    MessageBox.Show("切换host失败，请重试！");
                    return;
                }
                // 刷新 dns
                CmdHelper.Cmd(flushdnsCmd);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("请右键以管理员身份运行该程序，谢谢！");
                Environment.Exit(0);
            }
        }

        private bool updateHost(string[] domains, string[] ips, bool enable)
        {
            StringBuilder sb = new StringBuilder();

            File.SetAttributes(hostsPath, File.GetAttributes(hostsPath) & (~FileAttributes.ReadOnly));

            FileHelper.readTextFile(hostsPath, hostsEncoding, (index, line) =>
            {
                if (isAnnotation(line) || !exists(line, domains))
                {
                    sb.AppendLine(line);
                }
            });

            if (enable)
            {
                for (int i = 0; i < domains.Length; i++)
                {
                    sb.AppendLine(ips[i] + "  " + domains[i]);
                }
            }
            bool suc = false;
            if (sb.Length > 0)
            {
                suc = FileHelper.writeFile(hostsPath, hostsEncoding, sb.ToString());
            }
            return suc;
        }

        private bool isAnnotation(string line)
        {
            return line != null && line.TrimStart().StartsWith("#");
        }

        private bool exists(string line, string[] domains)
        {
            if (domains.Length > 0)
            {
                for (int i = 0; i < domains.Length; i++)
                {
                    if (exists(line, domains[i]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool exists(string line, string domain)
        {
            if (StringHelper.isBlank(line) || StringHelper.isBlank(domain))
            {
                return false;
            }
            domain = domain.Trim();
            int index;
            if (domain != null && line != null && (index = line.IndexOf(domain)) != -1)
            {
                if (index > 0 && !"".Equals(line.Substring(index - 1, 1).Trim()))
                {
                    return false;
                }
                index += domain.Length;
                if (index < line.Length && !"".Equals(line.Substring(index, 1).Trim()))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private string checkDomains(string[] domains, string[] ips, bool enable)
        {
            if (domains == null || domains.Length == 0 || (enable && (ips == null || ips.Length == 0)))
            {
                return "domain / ip is null";
            }
            else if (enable && domains.Length > ips.Length)
            {
                return "domains != ips";
            }
            for (int i = 0; i < domains.Length; i++)
            {
                domains[i] = domains[i].Trim();
                if (enable)
                {
                    ips[i] = ips[i].Trim();
                }
            }
            return null;
        }

    }
}
