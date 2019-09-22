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

        public static void applySwitch(List<HostConfig> hosts)
        {
            switchHost.switchHost(hosts.Where(host => host.Use).ToList(), true);
        }

        public static void applyNginx(OdyProjectConfig config)
        {
            string configDir = WindowsNginxImpl.nginxConfigDir;
            foreach (var project in config.Projects)
            {
                FileHelper.delDir(configDir + "\\" + project.Name, true);
            }
            OdyConfigHelper.writeConfig(config, configDir);
            List<string> confs = new List<string>();
            getUseConfig(config, confs);
            nginx.include(confs);
            nginx.restart();
        }

        private static void getUseConfig(OdyProjectConfig config, List<string> confs)
        {
            if (config.Use)
            {
                foreach (var p in config.Projects)
                {
                    if (!p.Use)
                    {
                        continue;
                    }
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
