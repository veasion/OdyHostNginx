using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OdyHostNginx
{
    class ApplicationHelper
    {

        public static bool autoExit = false;
        private static Nginx nginx = new WindowsNginxImpl();
        private static SwitchHost switchHost = new WindowsLocalHostImpl();

        public static void exit(bool auto)
        {
            try
            {
                nginx.stop();
                switchHost.switchHost((List<HostConfig>)null, false);
                ConfigDialogData.httpPacketClient.Shutdown();
                SSHClientHelper.closeAll();
            }
            catch (Exception e)
            {
                Logger.error("退出程序", e);
            }
            finally
            {
                autoExit = auto;
                if (auto)
                {
                    Application.Current.Shutdown();
                }
            }
        }

        public static void applySwitch(List<HostConfig> hosts)
        {
            switchHost.switchHost(hosts.Where(host => host.Use).ToList(), true);
        }

        public static void applyNginx(OdyProjectConfig config, bool writeBody, bool checkStatus)
        {
            string configDir = WindowsNginxImpl.nginxConfigDir;
            OdyConfigHelper.writeUserHosts(config);
            OdyConfigHelper.writeConfig(config, configDir, writeBody);
            List<string> confs = new List<string>();
            getUseConfig(config, confs);
            nginx.include(confs);
            if (config.Use && confs.Count > 0)
            {
                nginx.restart();
                checkRunStatus(config, confs);
            }
            else
            {
                nginx.stop();
            }
        }

        public static OdyProjectConfig copyUserConfigToNginx(Dictionary<string, UpstreamDetails> upstreamDetailsMap, bool replace)
        {
            FileHelper.copyDirectory(OdyConfigHelper.userNginxConfigDir, WindowsNginxImpl.nginxConfigDir, replace, true);
            return OdyConfigHelper.loadConfig(WindowsNginxImpl.nginxConfigDir, upstreamDetailsMap);
        }

        public static void hostGroupToProjectConfig(OdyProjectConfig p, Dictionary<string, List<HostConfig>> hostGroup)
        {
            if (hostGroup == null || hostGroup.Count <= 0)
            {
                foreach (var item in p.Projects)
                {
                    if (item.HostGroup)
                    {
                        p.Projects.Remove(item);
                        break;
                    }
                }
                return;
            }
            ProjectConfig hostPro = null;
            Dictionary<string, bool> envUseMap = new Dictionary<string, bool>();
            foreach (var item in p.Projects)
            {
                if (item.HostGroup)
                {
                    hostPro = item;
                    if (hostPro != null && hostPro.Envs != null && hostPro.Envs.Count > 0)
                    {
                        hostPro.Envs.ForEach(e => envUseMap[e.EnvName] = e.Use);
                    }
                    break;
                }
            }
            if (hostPro == null)
            {
                hostPro = new ProjectConfig();
                hostPro.HostGroup = true;
                hostPro.Name = "Host Group";
                p.Projects.Add(hostPro);
            }
            List<EnvConfig> envs = new List<EnvConfig>();
            foreach (var groupName in hostGroup.Keys)
            {
                EnvConfig env = new EnvConfig();
                env.Use = false;
                env.HostGroup = true;
                env.EnvName = groupName;
                env.Hosts = hostGroup[groupName];
                env.Project = hostPro;
                if (envUseMap.ContainsKey(groupName) && envUseMap[groupName])
                {
                    env.Use = true;
                    if (env.Hosts != null)
                    {
                        env.Hosts.ForEach(h => h.Use = true);
                    }
                }
                envs.Add(env);
            }
            hostPro.Envs = envs;
            if (envs.Count <= 0)
            {
                p.Projects.Remove(hostPro);
            }
        }

        private static void getUseConfig(OdyProjectConfig config, List<string> confs)
        {
            if (config.Use)
            {
                foreach (var p in config.Projects)
                {
                    if (p.HostGroup) continue;
                    foreach (var e in p.Envs)
                    {
                        if (!e.Use)
                        {
                            continue;
                        }
                        confs.Add(p.Name + "/" + e.EnvName + "/*.conf");
                    }
                }
            }
        }

        public static bool needRunNginx(OdyProjectConfig config)
        {
            if (config.Use)
            {
                foreach (var project in config.Projects)
                {
                    if (project.HostGroup) continue;
                    foreach (var env in project.Envs)
                    {
                        if (env.Use)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void checkRunStatus(OdyProjectConfig config, List<string> confs)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(3000);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (needRunNginx(config) && !nginx.isRun())
                    {
                        nginx.restart();
                    }
                    else
                    {
                        return;
                    }
                    Thread.Sleep(2000);
                    if (needRunNginx(config) && !nginx.isRun())
                    {
                        bool hasChinese = StringHelper.hasChinese(FileHelper.getCurrentDirectory());
                        if (hasChinese)
                        {
                            MessageBox.Show("启动 nginx 失败，运行目录中不能含有中文！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        foreach (var item in confs)
                        {
                            if (StringHelper.hasChinese(item))
                            {
                                MessageBox.Show("启动 nginx 失败，项目环境名称中不能含有中文！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        MessageBoxResult result = MessageBox.Show("启动 nginx 失败！是否查看日志？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start("notepad.exe", WindowsNginxImpl.nginxLogPath);
                        }
                    }
                });
            });
        }

    }
}
