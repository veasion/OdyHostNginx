using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class ApplicationHelper
    {

        private static Nginx nginx = new WindowsNginxImpl();
        private static SwitchHost switchHost = new WindowsLocalHostImpl();

        public static void exit()
        {
            try
            {
                nginx.stop();
                switchHost.switchHost((List<HostConfig>)null, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void applySwitch(List<HostConfig> hosts)
        {
            switchHost.switchHost(hosts.Where(host => host.Use).ToList(), true);
        }

        public static void applyNginx(OdyProjectConfig config)
        {
            string configDir = WindowsNginxImpl.nginxConfigDir;
            OdyConfigHelper.writeConfig(config, configDir, true);
            List<string> confs = new List<string>();
            getUseConfig(config, confs);
            nginx.include(confs);
            if (config.Use)
            {
                nginx.restart();
            }
            else
            {
                nginx.stop();
            }
        }

        public static OdyProjectConfig copyUserConfigToNginx(bool replace)
        {
            FileHelper.copyDirectory(OdyConfigHelper.userNginxConfigDir, WindowsNginxImpl.nginxConfigDir, replace);
            return OdyConfigHelper.loadConfig(WindowsNginxImpl.nginxConfigDir);
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
                        confs.Add(p.Name + "\\" + e.EnvName + "\\*.conf");
                    }
                }
            }
        }

    }
}
