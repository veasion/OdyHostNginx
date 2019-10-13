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

        public static void applyNginx(OdyProjectConfig config, bool writeBody)
        {
            string configDir = WindowsNginxImpl.nginxConfigDir;
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
            FileHelper.copyDirectory(OdyConfigHelper.userNginxConfigDir, WindowsNginxImpl.nginxConfigDir, replace);
            return OdyConfigHelper.loadConfig(WindowsNginxImpl.nginxConfigDir, upstreamDetailsMap);
        }

        private static void getUseConfig(OdyProjectConfig config, List<string> confs)
        {
            if (config.Use)
            {
                foreach (var p in config.Projects)
                {
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

        private static void checkRunStatus(OdyProjectConfig config, List<string> confs)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                Thread.Sleep(500);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (config.Use && !nginx.isRun())
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
