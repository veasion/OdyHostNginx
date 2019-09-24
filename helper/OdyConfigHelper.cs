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

        public static string userNginxConfigDir = FileHelper.getCurrentDirectory() + "\\config";

        /// <summary>
        /// project => envs => *.conf
        /// </summary>
        public static OdyProjectConfig loadConfig(string path)
        {
            return loadConfig(path, null);
        }

        public static OdyProjectConfig loadConfig(string path, Dictionary<string, UpstreamDetails> upstreamDetailsMap)
        {
            if (path == null)
            {
                path = WindowsNginxImpl.nginxConfigDir;
            }
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
                        Project = pro,
                        EnvName = envDir.Name
                    };
                    parseEnv(env, envDir);
                    if (!(env.Upstreams == null || env.Upstreams.Count == 0 || env.Configs == null || env.Configs.Count == 0))
                    {
                        if (upstreamDetailsMap != null)
                        {
                            fillUpstream(upstreamDetailsMap, env);
                        }
                        env.Configs.ForEach(conf => conf.Body = null);
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

        public static void reloadEnv(EnvConfig env, bool hasConfBody)
        {
            string projectName = env.Project.Name;
            string envName = env.EnvName;
            DirectoryInfo envDir = new DirectoryInfo(WindowsNginxImpl.nginxConfigDir + "\\" + projectName + "\\" + envName);
            if (envDir.Exists)
            {
                parseEnv(env, envDir);
                if (!hasConfBody)
                {
                    env.Configs.ForEach(conf => conf.Body = null);
                }
            }
        }

        private static void parseEnv(EnvConfig env, DirectoryInfo envDir)
        {
            FileInfo[] files = envDir.GetFiles("*.conf");
            List<NginxConfig> confs = new List<NginxConfig>();
            foreach (var file in files)
            {
                bool isUpstream = false;
                string context = FileHelper.readTextFile(file.FullName, WindowsNginxImpl.confEncoding);
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
                    env.Upstreams = parseUpstream(env, context);
                }
                else
                {
                    NginxConfig conf = parseConf(context);
                    conf.Env = env;
                    conf.FileName = file.Name;
                    conf.FilePath = file.FullName;
                    confs.Add(conf);
                }
            }
            env.Configs = confs;
        }

        public static void fillUpstream(Dictionary<string, UpstreamDetails> upstreamDetailsMap, EnvConfig env)
        {
            int index, start;
            string uri, contextPath;
            List<string> uris;
            List<string> contextPaths;
            foreach (var conf in env.Configs)
            {
                string context = conf.Body;
                int len = context.Length;
                foreach (var item in env.Upstreams)
                {
                    index = 0;
                    uris = new List<string>();
                    contextPaths = new List<string>();
                    while ((index = context.IndexOf(item.ServerName, index)) > 0)
                    {
                        start = context.LastIndexOf("location", index);
                        if (start != -1)
                        {
                            uri = StringHelper.substring(context, "/", "{", start, 200);
                            if (!StringHelper.isBlank(uri))
                            {
                                uris.Add("/" + uri.Trim());
                            }
                        }
                        index += item.ServerName.Length;
                        if (index < len && context[index] == '/')
                        {
                            contextPath = StringHelper.substring(context, "/", ";", index, 100);
                            if (!StringHelper.isBlank(contextPath))
                            {
                                contextPaths.Add(contextPath);
                            }
                        }
                    }
                    upstreamDetailsMap[item.ServerName] = new UpstreamDetails
                    {
                        Uris = uris,
                        ContextPaths = contextPaths,
                        ServerName = item.ServerName
                    };
                }
            }
        }

        public static List<HostConfig> loadUserHosts()
        {
            // TODO 读取用户 hosts 配置
            return new List<HostConfig>();
        }

        public static List<HostConfig> getHosts(OdyProjectConfig config)
        {
            HashSet<string> domainSet = new HashSet<string>();
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

        private static List<NginxUpstream> parseUpstream(EnvConfig env, string context)
        {
            int startIndex = 0;
            List<NginxUpstream> upstreams = new List<NginxUpstream>();
            while ((startIndex = context.IndexOf("upstream", startIndex)) != -1)
            {
                NginxUpstream upstream = new NginxUpstream
                {
                    Env = env
                };
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
        public static void writeConfig(OdyProjectConfig config, string path, bool replace)
        {
            if (path == null)
            {
                path = WindowsNginxImpl.nginxConfigDir;
            }
            // 加载 body
            config.Projects.ForEach(p => p.Envs.ForEach(e => e.Configs.ForEach(c => c.Body = c.Body)));
            foreach (var projectConfig in config.Projects)
            {
                string projectDir = path + "\\" + projectConfig.Name;
                FileHelper.mkdirAndDel(projectDir, replace);
                List<EnvConfig> envs = projectConfig.Envs;
                foreach (var env in envs)
                {
                    string envDir = projectDir + "\\" + env.EnvName;
                    FileHelper.mkdirAndDel(envDir, replace);
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
                    if (replace || !File.Exists(upstreamFile))
                    {
                        FileHelper.writeFile(upstreamFile, WindowsNginxImpl.confEncoding, upstreamBody.ToString());
                    }
                    List<NginxConfig> confs = env.Configs;
                    foreach (var conf in confs)
                    {
                        string confFile = envDir + "\\" + conf.FileName;
                        if (replace || !File.Exists(confFile))
                        {
                            FileHelper.writeFile(confFile, WindowsNginxImpl.confEncoding, conf.Body);
                        }
                    }
                }
            }
            // 清楚 body
            config.Projects.ForEach(p => p.Envs.ForEach(e => e.Configs.ForEach(c => c.Body = null)));
        }

    }

}
