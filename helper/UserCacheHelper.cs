using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class UserCacheHelper
    {

        static bool loaded = false;
        static string cachePath = FileHelper.getCurrentDirectory() + "\\bin\\config\\cache.json";

        public static void loadCache(OdyProjectConfig odyConfig)
        {
            if (loaded) return;
            try
            {
                if (!File.Exists(cachePath)) return;
                string json = FileHelper.readTextFile(cachePath);
                JToken jt = JToken.Parse(json);
                if (jt == null || !jt.HasValues) return;
                if (odyConfig.Projects != null)
                {
                    loadConfig(jt, odyConfig.Projects);
                }
                if (jt["_CONFIG"] != null)
                {
                    odyConfig.Config = jt["_CONFIG"];
                }
            }
            catch (Exception e)
            {
                Logger.error("加载缓存", e);
            }
            loaded = true;
        }

        private static void loadConfig(JToken config, List<ProjectConfig> projects)
        {
            projects.ForEach(p =>
            {
                JToken j = config[p.Name];
                if (j == null || p.Envs == null) return;
                foreach (var e in p.Envs)
                {
                    loadHost(j, e.Hosts);
                    loadHost(j, e.UserHosts);
                    e.Use = j[e.EnvName] != null;
                }
            });
        }

        private static void loadHost(JToken host, List<HostConfig> userHosts)
        {
            if (userHosts != null)
            {
                foreach (var h in userHosts)
                {
                    if (host[h.Domain] != null)
                    {
                        h.Use = true;
                    }
                }
            }
        }

        public static void saveCache(OdyProjectConfig odyConfig)
        {
            try
            {
                Dictionary<string, object> cache = new Dictionary<string, object>();
                if (odyConfig != null && odyConfig.Projects != null)
                {
                    cacheConfig(odyConfig, odyConfig.Projects, cache);
                }
                if (cache.Count > 0)
                {
                    FileHelper.writeJsonFile(cache, cachePath);
                }
            }
            catch (Exception e)
            {
                Logger.error("保存缓存", e);
            }
        }

        private static void cacheConfig(OdyProjectConfig odyConfig, List<ProjectConfig> projects, Dictionary<string, object> cache)
        {
            Dictionary<string, object> evhMap;
            foreach (var p in projects)
            {
                if (p.Envs != null)
                {
                    evhMap = new Dictionary<string, object>();
                    foreach (var e in p.Envs)
                    {
                        if (e.Use)
                        {
                            evhMap[e.EnvName] = true;
                        }
                        cacheHost(e.Hosts, evhMap);
                        cacheHost(e.UserHosts, evhMap);
                        savePortCache(e.Upstreams, odyConfig.Config);
                    }
                    if (evhMap != null)
                    {
                        cache[p.Name] = evhMap;
                    }
                }
            }
            cache["_CONFIG"] = odyConfig.Config;
        }

        private static void savePortCache(List<NginxUpstream> ups, JToken config)
        {
            if (config["portCache"] == null)
            {
                config["portCache"] = JToken.Parse("{}");
            }
            if (ups == null) return;
            foreach (var item in ups)
            {
                if (item.Port != item.OldPort && item.ContextPath != null)
                {
                    config["portCache"][item.ContextPath] = item.Port.ToString();
                }
            }
        }

        private static void cacheHost(List<HostConfig> userHosts, Dictionary<string, object> cache)
        {
            if (userHosts != null)
            {
                userHosts.ForEach(h =>
                {
                    if (h.Use)
                    {
                        cache[h.Domain] = true;
                    }
                });
            }
        }
    }
}
