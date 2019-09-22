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

        public void switchHost(HostConfig host, bool enable)
        {
            if (StringHelper.isBlank(host.Domain))
            {
                return;
            }
            if (enable && StringHelper.isBlank(host.Ip))
            {
                return;
            }
            List<HostConfig> hosts = new List<HostConfig>
            {
                host
            };
            switchHost(hosts, enable);
        }

        public void switchHost(List<HostConfig> hosts, bool enable)
        {
            string check = checkDomains(hosts, enable);
            if (check != null)
            {
                throw new ServiceException(check);
            }
            try
            {
                // 更改 host
                bool suc = updateHost(hosts, enable);
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

        private bool updateHost(List<HostConfig> hosts, bool enable)
        {
            StringBuilder sb = new StringBuilder();

            File.SetAttributes(hostsPath, File.GetAttributes(hostsPath) & (~FileAttributes.ReadOnly));

            FileHelper.readTextFile(hostsPath, hostsEncoding, (index, line) =>
            {
                if (isAnnotation(line) || !exists(line, hosts))
                {
                    sb.AppendLine(line);
                }
            });

            if (enable)
            {
                for (int i = 0; i < hosts.Count; i++)
                {
                    sb.AppendLine(hosts[i].Ip + "  " + hosts[i].Domain);
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

        private bool exists(string line, List<HostConfig> hosts)
        {
            if (hosts.Count > 0)
            {
                for (int i = 0; i < hosts.Count; i++)
                {
                    if (exists(line, hosts[i].Domain))
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

        private string checkDomains(List<HostConfig> hosts, bool enable)
        {
            if (hosts == null || hosts.Count == 0)
            {
                return "domain / ip is null";
            }
            foreach (var host in hosts)
            {
                if (StringHelper.isBlank(host.Domain))
                {
                    return "domain 不能为空";
                }
                if (enable)
                {
                    if (StringHelper.isBlank(host.Ip))
                    {
                        return host.Domain + "对应的ip不能为空";
                    }
                    host.Ip = host.Ip.Trim();
                }
                host.Domain = host.Domain.Trim();
            }
            return null;
        }

    }
}
