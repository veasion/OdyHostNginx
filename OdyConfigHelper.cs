using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class OdyConfigHelper
    {

        /// <summary>
        /// project => envs => *.conf
        /// </summary>
        public static OdyProjectConfig loadConfig(string path)
        {
            List<ProjectConfig> projects = new List<ProjectConfig>();
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] proDirs = dir.GetDirectories();
            foreach (var proDir in proDirs)
            {
                ProjectConfig pro = new ProjectConfig
                {
                    Name = proDir.Name
                };
                List<EnvConfig> envs = new List<EnvConfig>();
                DirectoryInfo[] envDirs = proDir.GetDirectories();
                foreach (var envDir in envDirs)
                {
                    EnvConfig env = new EnvConfig
                    {
                        EnvName = envDir.Name
                    };
                    FileInfo[] files = envDir.GetFiles("*.conf");
                    List<NginxConfig> confs = new List<NginxConfig>();
                    foreach (var file in files)
                    {
                        bool isUpstream = false;
                        string context = FileHelper.readTextFile(file.FullName);
                        if (file.Name.EndsWith("upstream.conf") && context.IndexOf("upstream") > 0)
                        {
                            isUpstream = true;
                        }
                        else if (context.StartsWith("upstream"))
                        {
                            isUpstream = true;
                        }
                        if (isUpstream)
                        {
                            env.UpstreamFileName = file.Name;
                            env.Upstreams = parseUpstream(context);
                        }
                        else
                        {
                            NginxConfig conf = parseConf(context);
                            conf.FileName = file.Name;
                            confs.Add(conf);
                        }
                    }
                    env.Configs = confs;
                    if (!(env.Upstreams == null || env.Upstreams.Count == 0 || env.Configs == null || env.Configs.Count == 0))
                    {
                        envs.Add(env);
                    }
                }
                pro.Envs = envs;
                if (pro.Envs != null && pro.Envs.Count > 0)
                {
                    projects.Add(pro);
                }
            }
            OdyProjectConfig odyProjectConfig = new OdyProjectConfig
            {
                Projects = projects
            };
            return odyProjectConfig;
        }

        public static List<HostConfig> getHosts(OdyProjectConfig config)
        {
            HashSet<string> domainSet = new HashSet<string>();
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
                        foreach (var configs in e.Configs)
                        {
                            domainSet.Add(configs.ServerName);
                        }
                    }
                }
            }
            List<HostConfig> hosts = new List<HostConfig>();
            foreach (var domain in domainSet)
            {
                HostConfig host = new HostConfig
                {
                    Use = true,
                    Ip = "127.0.0.1",
                    Domain = domain
                };
                hosts.Add(host);
            }
            return hosts;
        }

        private static List<NginxUpstream> parseUpstream(string context)
        {
            int startIndex = 0;
            List<NginxUpstream> upstreams = new List<NginxUpstream>();
            while ((startIndex = context.IndexOf("upstream", startIndex)) != -1)
            {
                NginxUpstream upstream = new NginxUpstream();
                string serverName = StringHelper.substring(context, "upstream", "{", startIndex);
                if (StringHelper.isBlank(serverName))
                {
                    continue;
                }
                upstream.ServerName = serverName.Trim();
                startIndex = context.IndexOf("{", startIndex + 1);
                string host = StringHelper.substring(context, "server", ";", startIndex);
                if (StringHelper.isBlank(host))
                {
                    continue;
                }
                host = host.Trim();
                string[] hostArray = host.Split(':');
                upstream.Ip = hostArray[0].Trim();
                if (hostArray.Length > 1 && !StringHelper.isNumberic(hostArray[1].Trim()))
                {
                    upstream.Port = Convert.ToInt32(hostArray[1].Trim());
                }
                else
                {
                    upstream.Port = 80;
                }
                upstreams.Add(upstream);
            }
            return upstreams;
        }

        private static NginxConfig parseConf(string context)
        {
            NginxConfig conf = new NginxConfig
            {
                Body = context
            };
            string listen = StringHelper.substring(context, "listen", ";");
            if (!StringHelper.isBlank(listen))
            {
                conf.Listen = listen.Trim();
            }
            string server_name = StringHelper.substring(context, "server_name", ";");
            if (!StringHelper.isBlank(server_name))
            {
                conf.ServerName = server_name.Trim();
            }
            return conf;
        }

        /// <summary>
        /// /projects/envs/*.conf
        /// </summary>
        public static void writeConfig(OdyProjectConfig config, string path)
        {
            foreach (var projectConfig in config.Projects)
            {
                string projectDir = path + "\\" + projectConfig.Name;
                FileHelper.mkdirAndDel(projectDir);
                List<EnvConfig> envs = projectConfig.Envs;
                foreach (var env in envs)
                {
                    string envDir = projectDir + "\\" + env.EnvName;
                    FileHelper.mkdirAndDel(envDir);
                    string upstreamFile = envDir + "\\" + env.UpstreamFileName;
                    StringBuilder upstreamBody = new StringBuilder();
                    List<NginxUpstream> upstreams = env.Upstreams;
                    foreach (var upstream in upstreams)
                    {
                        upstreamBody.Append("upstream ");
                        upstreamBody.Append(upstream.ServerName);
                        upstreamBody.AppendLine(" {");
                        upstreamBody.Append("server ");
                        upstreamBody.Append(upstream.Ip);
                        upstreamBody.Append(upstream.Port);
                        upstreamBody.AppendLine(";");
                        upstreamBody.AppendLine("}");
                    }
                    FileHelper.writeFile(upstreamFile, Encoding.UTF8, upstreamBody.ToString());
                    List<NginxConfig> confs = env.Configs;
                    foreach (var conf in confs)
                    {
                        string confFile = envDir + "\\" + conf.FileName;
                        FileHelper.writeFile(confFile, Encoding.UTF8, conf.Body);
                    }
                }
            }
        }
    }

}
