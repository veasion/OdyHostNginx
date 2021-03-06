using Newtonsoft.Json.Linq;
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

        public static string deleteStartsWith = "deleted_";
        public static string sysHostsPath = WindowsLocalHostImpl.hostsPath;
        public static string nginxConfigDir = WindowsNginxImpl.nginxConfigDir;
        public static string userNginxConfigDir = FileHelper.getCurrentDirectory() + "\\config";
        public static string userOdyNginxConfigDir = userNginxConfigDir + "\\ody";
        public static string userHostsDir = FileHelper.getCurrentDirectory() + "\\bin\\hosts";
        public static string odyConfigBackDir = FileHelper.getCurrentDirectory() + "\\config_back";
        public static string userHostsFileName = "hosts.json";
        public static string modifyResponse = FileHelper.getCurrentDirectory() + "\\ModifyResponse.txt";

        private static Dictionary<string, int> priority = new Dictionary<string, int>();

        static OdyConfigHelper()
        {
            priority["oms-web"] = 99;
            priority["oms-api"] = 98;
            priority["oms-dataex"] = 97;
            priority["ouser-web"] = 96;
            priority["ouser-center"] = 95;
            priority["ouser-service"] = 94;
            priority["crm-web"] = 94;
            priority["back-merchant-web"] = 93;
            priority["back-product-web"] = 92;
            priority["back-product-service"] = 91;
            priority["frontier-trade-web"] = 90;
            priority["frontier-cms"] = 90;
            priority["hermes-web"] = 89;
            priority["back-promotion-web"] = 89;
            priority["basics-promotion-service"] = 89;
            priority["social-web"] = 88;
            priority["social-back-web"] = 87;
            priority["search"] = 86;
            priority["search-backend-web"] = 85;
            priority["odts-web"] = 84;
            priority["opay-web"] = 84;
            priority["ody-scheduler"] = 84;
            priority["osc-web"] = 84;
            priority["agent-web"] = 83;
            priority["opms-web"] = 82;
            priority["pms-web"] = 82;
            priority["ofms-web"] = 82;
            priority["back-finance-web"] = 81;
            priority["ofms-web"] = 81;
            priority["appdflow-web"] = 80;
            priority["ad-whale-web"] = 79;
        }

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
                path = nginxConfigDir;
            }
            JToken userHostsJToken = getUserHostsJToken();
            List<ProjectConfig> projects = new List<ProjectConfig>();
            DirectoryInfo dir = new DirectoryInfo(path);
            DirectoryInfo[] proDirs = dir.GetDirectories();
            foreach (var proDir in proDirs)
            {
                if (proDir.Name.StartsWith(deleteStartsWith))
                {
                    continue;
                }
                ProjectConfig pro = new ProjectConfig
                {
                    Name = proDir.Name
                };
                List<EnvConfig> envs = new List<EnvConfig>();
                DirectoryInfo[] envDirs = proDir.GetDirectories();
                foreach (var envDir in envDirs)
                {
                    if (envDir.Name.StartsWith(deleteStartsWith))
                    {
                        continue;
                    }
                    EnvConfig env = new EnvConfig
                    {
                        Project = pro,
                        EnvName = envDir.Name
                    };
                    parseEnv(env, envDir);
                    loadUserHosts(userHostsJToken, env);
                    if (!(env.Configs == null || env.Configs.Count == 0))
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
            DirectoryInfo envDir = new DirectoryInfo(nginxConfigDir + "\\" + projectName + "\\" + envName);
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
            env.Hosts = getHosts(env);
        }

        public static void fillUpstream(Dictionary<string, UpstreamDetails> upstreamDetailsMap, EnvConfig env)
        {
            int index, start;
            string uri, contextPath;
            HashSet<string> uris;
            HashSet<string> contextPaths;
            foreach (var conf in env.Configs)
            {
                string context = conf.Body;
                int len = context.Length;
                if (env.Upstreams == null)
                {
                    env.ReplaceHost = true;
                    continue;
                }
                foreach (var item in env.Upstreams)
                {
                    index = 0;
                    uris = new HashSet<string>();
                    contextPaths = new HashSet<string>();
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
                                contextPath = contextPath.Trim();
                                if (contextPath.Contains("/"))
                                {
                                    int startIndex = contextPath.IndexOf("/");
                                    contextPath = contextPath.Substring(0, startIndex);
                                }
                                contextPaths.Add(contextPath);
                            }
                        }
                    }
                    if (upstreamDetailsMap.ContainsKey(item.ServerName))
                    {
                        foreach (var str in uris)
                        {
                            upstreamDetailsMap[item.ServerName].Uris.Add(str);
                        }
                        foreach (var str in contextPaths)
                        {
                            upstreamDetailsMap[item.ServerName].ContextPaths.Add(str);
                        }
                    }
                    else
                    {
                        upstreamDetailsMap[item.ServerName] = new UpstreamDetails
                        {
                            ServerName = item.ServerName,
                            Uris = uris ?? new HashSet<string>(),
                            ContextPaths = contextPaths ?? new HashSet<string>()
                        };
                    }
                }
            }
        }

        public static Dictionary<string, List<HostConfig>> loadHostGroup()
        {
            Dictionary<string, List<HostConfig>> dir = new Dictionary<string, List<HostConfig>>();
            string[] files = Directory.GetFiles(userHostsDir);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (userHostsFileName.Equals(fileName)) continue;
                List<HostConfig> list = loadHosts(file);
                dir[fileName] = list;
            }
            return dir;
        }

        public static bool createHostGroup(string name)
        {
            try
            {
                return FileHelper.writeFile(userHostsDir + "\\" + name, FileHelper.UTF_8, "");
            }
            catch
            {
                return false;
            }
        }

        public static string createOdyEnv(string name, string domain, string ip)
        {
            if (!Directory.Exists(odyConfigBackDir)) return "config_back 不存在";
            string dir = userOdyNginxConfigDir + "\\" + name;
            if (Directory.Exists(dir)) return "Env Name 已存在";
            FileHelper.copyDirectory(odyConfigBackDir, dir, false);
            Dictionary<string, string> replaceMap = new Dictionary<string, string>();
            replaceMap.Add("#name", name);
            replaceMap.Add("#adminDomain", domain);
            replaceMap.Add("#adminIp", ip);
            FileHelper.replaceDirFileContent(dir, replaceMap);
            return null;
        }

        public static void delHostGroup(string name)
        {
            string path = userHostsDir + "\\" + name;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static bool modifyHostGroup(string oldName, string newName)
        {
            string oldPath = userHostsDir + "\\" + oldName;
            string newPath = userHostsDir + "\\" + newName;
            if (File.Exists(oldPath) && !File.Exists(newPath))
            {
                File.Copy(oldPath, newPath);
                File.Delete(oldPath);
                return true;
            }
            return false;
        }

        private static List<HostConfig> loadHosts(string path)
        {
            List<HostConfig> list = new List<HostConfig>();
            FileHelper.readTextFile(path, FileHelper.UTF_8, (index, line) =>
            {
                if (!StringHelper.isBlank(line))
                {
                    line = line.Trim();
                    if (line.StartsWith("#"))
                    {
                        return true;
                    }
                    string[] hosts = line.Split('=');
                    if (hosts != null && hosts.Length > 1)
                    {
                        HostConfig config = new HostConfig();
                        config.Domain = hosts[0];
                        config.Ip = hosts[1];
                        list.Add(config);
                    }
                }
                return true;
            });
            return list;
        }

        private static void writeHosts(string path, List<HostConfig> userHostConfigs)
        {
            StringBuilder sb = new StringBuilder();
            if (userHostConfigs != null && userHostConfigs.Count > 0)
            {
                foreach (var item in userHostConfigs)
                {
                    sb.Append(item.Domain).Append("=").AppendLine(item.Ip);
                }
            }
            FileHelper.writeFile(path, FileHelper.UTF_8, sb.ToString());
        }


        public static void writeHostGroup(string groupName, List<HostConfig> userHostConfigs)
        {
            writeHosts(userHostsDir + "\\" + groupName, userHostConfigs);
        }

        public static void writeUserHosts(OdyProjectConfig odyProjectConfig)
        {
            string userHostPath = userHostsDir + "\\" + userHostsFileName;
            Dictionary<string, Dictionary<string, string>> map = new Dictionary<string, Dictionary<string, string>>();
            if (odyProjectConfig != null && odyProjectConfig.Projects != null)
            {
                foreach (var p in odyProjectConfig.Projects)
                {
                    if (p.Envs == null) continue;
                    foreach (var env in p.Envs)
                    {
                        if (env.UserHosts == null || env.UserHosts.Count == 0)
                        {
                            continue;
                        }
                        string key = env.Project.Name + "." + env.EnvName;
                        Dictionary<string, string> userHostMap = new Dictionary<string, string>();
                        foreach (var host in env.UserHosts)
                        {
                            userHostMap[host.Domain] = host.Ip;
                        }
                        map[key] = userHostMap;
                    }
                }
            }
            FileHelper.writeJsonFile(map, userHostPath);
        }

        public static JToken getUserHostsJToken()
        {
            try
            {
                string userHostPath = userHostsDir + "\\" + userHostsFileName;
                if (!File.Exists(userHostPath)) return null;
                string json = FileHelper.readTextFile(userHostPath);
                return JToken.Parse(json);
            }
            catch (Exception e)
            {
                Logger.error("加载" + userHostsFileName, e);
                return null;
            }
        }

        public static void loadUserHosts(EnvConfig env)
        {
            loadUserHosts(getUserHostsJToken(), env);
        }

        public static void loadUserHosts(JToken jToken, EnvConfig env)
        {
            env.UserHosts = new List<HostConfig>();
            if (jToken == null || !jToken.HasValues) return;
            string key = env.Project.Name + "." + env.EnvName;
            if (jToken[key] != null)
            {
                Dictionary<string, string> userHostMap = jToken[key].ToObject<Dictionary<string, string>>();
                foreach (var domain in userHostMap.Keys)
                {
                    env.UserHosts.Add(new HostConfig
                    {
                        Use = false,
                        Domain = domain,
                        Ip = userHostMap[domain]
                    });
                }
            }
        }

        private static List<HostConfig> getHosts(EnvConfig env)
        {
            HashSet<string> domainSet = new HashSet<string>();
            foreach (var configs in env.Configs)
            {
                if (!StringHelper.isBlank(configs.ServerName))
                {
                    configs.ServerName = configs.ServerName.Trim();
                    string[] servers = configs.ServerName.Split(' ');
                    if (servers.Length > 1)
                    {
                        foreach (var item in servers)
                        {
                            if (!StringHelper.isBlank(item))
                            {
                                domainSet.Add(item.Trim());
                            }
                        }
                    }
                    else
                    {
                        domainSet.Add(configs.ServerName);
                    }
                }
            }
            List<HostConfig> hosts = new List<HostConfig>();
            foreach (var domain in domainSet)
            {
                HostConfig host = new HostConfig
                {
                    Use = false,
                    Domain = domain,
                    Ip = "127.0.0.1"
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
                if (StringHelper.isBlank(serverName)) continue;
                upstream.ServerName = serverName.Trim();
                startIndex = context.IndexOf("{", startIndex + 1);
                string host = StringHelper.substring(context, "server", ";", startIndex);
                if (StringHelper.isBlank(host)) continue;
                host = host.Trim();
                string[] hostArray = host.Split(':');
                upstream.Ip = hostArray[0].Trim();
                if (hostArray.Length > 1 && StringHelper.isInt(hostArray[1].Trim()))
                {
                    upstream.Port = Convert.ToInt32(hostArray[1].Trim());
                }
                else
                {
                    upstream.Port = 80;
                }
                upstream.OldIp = upstream.Ip;
                upstream.OldPort = upstream.Port;
                upstreams.Add(upstream);
            }
            parseUserOldUpstream(env, upstreams);
            return upstreams;
        }

        private static void parseUserOldUpstream(EnvConfig env, List<NginxUpstream> upstreams)
        {
            string oldFile = userNginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName + "\\" + env.UpstreamFileName;
            string context = FileHelper.readTextFile(oldFile, WindowsNginxImpl.confEncoding);
            Dictionary<string, NginxUpstream> dic = new Dictionary<string, NginxUpstream>();
            upstreams.ForEach(up => dic[up.ServerName] = up);
            int startIndex = 0;
            NginxUpstream u;
            while ((startIndex = context.IndexOf("upstream", startIndex)) != -1)
            {
                string serverName = StringHelper.substring(context, "upstream", "{", startIndex);
                if (StringHelper.isBlank(serverName)) continue;
                if (!dic.TryGetValue(serverName.Trim(), out u)) continue;
                startIndex = context.IndexOf("{", startIndex + 1);
                string host = StringHelper.substring(context, "server", ";", startIndex);
                if (StringHelper.isBlank(host)) continue;
                host = host.Trim();
                string[] hostArray = host.Split(':');
                u.OldIp = hostArray[0].Trim();
                if (hostArray.Length > 1 && StringHelper.isInt(hostArray[1].Trim()))
                {
                    u.OldPort = Convert.ToInt32(hostArray[1].Trim());
                }
            }
        }

        private static int priorityServerName(string serverName)
        {
            int p;
            if (priority.TryGetValue(serverName, out p))
            {
                return p;
            }
            else
            {
                foreach (var item in priority.Keys)
                {
                    if (serverName.IndexOf(item) != -1)
                    {
                        return priority[item] - serverName.Length * 2;
                    }
                }
                return 0;
            }
        }

        public static void sortUpstream(List<NginxUpstream> upstreams)
        {
            // 配置 local 排前面，否则按优先级排序，名字越长越靠后
            if (upstreams != null && upstreams.Count > 0)
            {
                upstreams.Sort((x, y) =>
                {
                    if ("127.0.0.1".Equals(x.Ip) && !"127.0.0.1".Equals(y.Ip))
                    {
                        return -1;
                    }
                    else if ("127.0.0.1".Equals(y.Ip) && !"127.0.0.1".Equals(x.Ip))
                    {
                        return 1;
                    }
                    int xp = priorityServerName(x.ServerName);
                    int yp = priorityServerName(y.ServerName);
                    if (xp != 0 || yp != 0)
                    {
                        return xp > yp ? -1 : 1;
                    }
                    else
                    {
                        return x.ServerName.Length > y.ServerName.Length ? 1 : -1;
                    }
                });
            }
        }

        public static void sortHosts(List<HostConfig> hosts)
        {
            // 使用中的排在前面，否则域名越长越靠后
            if (hosts != null)
            {
                hosts.Sort((x, y) =>
                {
                    if (x.Use && !y.Use)
                    {
                        return -1;
                    }
                    else if (y.Use && !x.Use)
                    {
                        return 1;
                    }
                    return x.Domain.Length > y.Domain.Length ? 1 : -1;
                });
            }
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
                server_name = server_name.Trim();
                if (server_name.IndexOf(" ") > 0)
                {
                    server_name = server_name.Split(' ')[0].Trim();
                }
                conf.ServerName = server_name;
            }
            return conf;
        }

        /// <summary>
        /// /projects/envs/*.conf
        /// </summary>
        public static void writeConfig(OdyProjectConfig config, string path, bool writeBody)
        {
            if (path == null)
            {
                path = nginxConfigDir;
            }
            if (writeBody)
            {
                // 加载 body
                config.Projects.ForEach(p => {
                    if (p.HostGroup) return;
                    p.Envs.ForEach(e => e.Configs.ForEach(c => c.Body = c.Body));
                });
            }
            foreach (var projectConfig in config.Projects)
            {
                if (projectConfig.HostGroup) continue;
                string projectDir = path + "\\" + projectConfig.Name;
                FileHelper.mkdirAndDel(projectDir, writeBody);
                List<EnvConfig> envs = projectConfig.Envs;
                foreach (var env in envs)
                {
                    string envDir = projectDir + "\\" + env.EnvName;
                    FileHelper.mkdirAndDel(envDir, writeBody);
                    if (env.ReplaceHost || isOdyDockerEnv(env)) {
                        List<NginxConfig> confs = env.Configs;
                        foreach (var conf in confs)
                        {
                            List<HostConfig> hosts = env.Hosts;
                            int sIndex = conf.Body.IndexOf("server_name");
                            int eIndex = conf.Body.IndexOf(";", sIndex);
                            string body = conf.Body.Substring(0, sIndex) + "server_name " + hosts[0].Domain + conf.Body.Substring(eIndex);
                            FileHelper.writeFile(envDir + "\\" + conf.FileName, WindowsNginxImpl.confEncoding, body);
                        }
                    }
                    if (!env.ReplaceHost || isOdyDockerEnv(env))
                    {
                        string upstreamFile = envDir + "\\" + env.UpstreamFileName;
                        StringBuilder upstreamBody = new StringBuilder();
                        List<NginxUpstream> upstreams = env.Upstreams;
                        foreach (var upstream in upstreams)
                        {
                            if (StringHelper.isBlank(upstream.Ip))
                            {
                                continue;
                            }
                            upstreamBody.Append("upstream ");
                            upstreamBody.Append(upstream.ServerName);
                            upstreamBody.AppendLine(" {");
                            upstreamBody.Append("server ");
                            upstreamBody.Append(upstream.Ip);
                            upstreamBody.Append(":");
                            upstreamBody.Append(upstream.Port > 0 ? upstream.Port : 80);
                            upstreamBody.AppendLine(";");
                            upstreamBody.AppendLine("}");
                        }
                        FileHelper.writeFile(upstreamFile, WindowsNginxImpl.confEncoding, upstreamBody.ToString());
                        if (writeBody)
                        {
                            List<NginxConfig> confs = env.Configs;
                            foreach (var conf in confs)
                            {
                                FileHelper.writeFile(envDir + "\\" + conf.FileName, WindowsNginxImpl.confEncoding, conf.Body);
                            }
                        }
                    }
                }
            }
            if (writeBody)
            {
                // 清除 body，采用懒加载
                config.Projects.ForEach(p => p.Envs.ForEach(e => e.Configs.ForEach(c => c.Body = null)));
            }
        }

        public static void deleteProject(ProjectConfig project)
        {
            FileHelper.delDir(nginxConfigDir + "\\" + project.Name, true);
            DirectoryInfo envDir = new DirectoryInfo(userNginxConfigDir + "\\" + project.Name);
            if (envDir.Exists)
            {
                // envDir.MoveTo(userNginxConfigDir + "\\" + deleteStartsWith + project.Name);
                FileHelper.delDir(userNginxConfigDir + "\\" + project.Name, true);
            }
        }

        public static void deleteEnv(EnvConfig env)
        {
            FileHelper.delDir(nginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName, true);
            DirectoryInfo envDir = new DirectoryInfo(userNginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName);
            if (envDir.Exists)
            {
                // envDir.MoveTo(userNginxConfigDir + "\\" + env.Project.Name + "\\" + deleteStartsWith + env.EnvName);
                FileHelper.delDir(userNginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName, true);
            }
        }

        public static bool renameEnv(EnvConfig env, string newName)
        {
            string oldPath = userNginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName;
            string newPath = userNginxConfigDir + "\\" + env.Project.Name + "\\" + newName;
            DirectoryInfo oldDir = new DirectoryInfo(oldPath);
            DirectoryInfo newDir = new DirectoryInfo(newPath);
            if (oldDir.Exists && !newDir.Exists)
            {
                oldDir.MoveTo(newPath);
                FileHelper.delDir(nginxConfigDir + "\\" + env.Project.Name + "\\" + env.EnvName, true);
                FileHelper.copyDirectory(newPath, nginxConfigDir + "\\" + env.Project.Name + "\\" + newName, true);
                return true;
            }
            return false;
        }

        public static string[] projectEnvName(string[] names)
        {
            string[] pe = new string[2];
            foreach (var name in names)
            {
                int index = -1;
                string fileName = name, str = null;
                if ((index = fileName.LastIndexOf("\\")) > -1)
                {
                    fileName = fileName.Substring(index + 1);
                }
                if (fileName.StartsWith("adminportal"))
                {
                    str = StringHelper.substring(fileName, "adminportal", ".oudianyun.com");
                }
                else if (fileName.StartsWith("api."))
                {
                    str = StringHelper.substring(fileName, "api.", ".com.conf");
                }
                if (str != null)
                {
                    index = str.LastIndexOf("dev");
                    if (index == -1)
                    {
                        index = str.LastIndexOf("test");
                    }
                    if (index == -1)
                    {
                        index = str.LastIndexOf("trunk");
                    }
                    if (index > -1)
                    {
                        pe[0] = str.Substring(0, index);
                        pe[1] = str.Substring(index);
                        break;
                    }
                }
            }
            return pe;
        }

        public static bool isOdyDockerEnv(EnvConfig env)
        {
            return env != null &&
                env.Hosts != null && env.Hosts.Count == 1 &&
                env.Upstreams.Count >= 2 &&
                env.Upstreams[0].OldIp.Equals(env.Upstreams[1].OldIp);
        }

    }

}
