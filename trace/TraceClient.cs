using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class TraceClient
    {

        static Dictionary<string, TracesInfo> cache = new Dictionary<string, TracesInfo>();

        public static TracesInfo traces(string url)
        {
            try
            {
                TracesInfo root = null;
                if (url.IndexOf("/zipkin//") > 0)
                {
                    url = url.Replace("/zipkin//", "/zipkin/");
                }
                UpgradeVo u = UpgradeHelper.getUpgrade(false);
                string v = u.TraceVersion;
                if (v == null || "".Equals(v.Trim()))
                {
                    v = "v1";
                }
                url = url.Replace("/zipkin/traces/", "/zipkin/api/" + v + "/trace/");
                cache.TryGetValue(url, out root);
                if (root != null)
                {
                    return root;
                }
                string jsonStr = HttpHelper.get(url);
                if (StringHelper.isEmpty(jsonStr)) return null;
                Dictionary<string, TracesInfo> dic = new Dictionary<string, TracesInfo>();
                JToken jsonArr = JToken.Parse(jsonStr);
                TracesInfo traces;
                foreach (var j in jsonArr)
                {
                    if ("v1".Equals(v))
                    {
                        traces = parseV1(j, dic);
                    }
                    else
                    {
                        traces = parseV2(j, dic);
                    }
                    if (root == null && traces != null && traces.Id.Equals(getStr(j, "traceId")))
                    {
                        root = traces;
                    }
                }
                children(root, dic);
                if (cache.Count > 20)
                {
                    cache.Clear();
                }
                cache[url] = root;
                return root;
            }
            catch (Exception e)
            {
                Logger.error("解析trace", e);
            }
            return null;
        }

        private static TracesInfo parseV1(JToken j, Dictionary<string, TracesInfo> dic)
        {
            TracesInfo traces = new TracesInfo();
            traces.Id = getStr(j, "id");
            if (traces.Id == null)
            {
                return null;
            }
            dic[traces.Id] = traces;
            traces.Name = getStr(j, "name");
            traces.Pid = getStr(j, "parentId");
            traces.Timestamp = getLong(j, "timestamp");
            traces.Duration = getLong(j, "duration");
            if (j["binaryAnnotations"] != null)
            {
                Dictionary<string, string> details = new Dictionary<string, string>();
                foreach (var dj in j["binaryAnnotations"])
                {
                    string key = getStr(dj, "key");
                    string value = getStr(dj, "value");
                    if (key != null)
                    {
                        if ("query.sql".Equals(key) && value != null)
                        {
                            value = Regex.Replace(value, @"(\n\s{0,}\n){1,}", "\n");
                        }
                        details[key] = value;
                    }
                }
                traces.Details = details;
            }
            if (j["annotations"] != null)
            {
                string serviceName = null;
                string clientName = null;
                foreach (var aj in j["annotations"])
                {
                    if (aj["endpoint"] != null)
                    {
                        string value = getStr(aj, "value");
                        string name = getStr(aj["endpoint"], "serviceName");
                        if ("ss".Equals(value))
                        {
                            serviceName = name;
                        }
                        else if ("cs".Equals(value))
                        {
                            clientName = name;
                        }
                    }
                }
                traces.ClientName = clientName;
                traces.ServiceName = serviceName;
            }
            return traces;
        }

        private static TracesInfo parseV2(JToken j, Dictionary<string, TracesInfo> dic)
        {
            TracesInfo traces = new TracesInfo();
            traces.Id = getStr(j, "id");
            if (traces.Id == null)
            {
                return null;
            }
            dic[traces.Id] = traces;
            traces.Name = getStr(j, "name");
            traces.Pid = getStr(j, "parentId");
            traces.Timestamp = getLong(j, "timestamp");
            traces.Duration = getLong(j, "duration");
            if (j["tags"] != null)
            {
                Dictionary<string, string> details = j["tags"].ToObject<Dictionary<string, string>>();
                if (details.ContainsKey("query.sql"))
                {
                    details["query.sql"] = Regex.Replace(details["query.sql"], @"(\n\s{0,}\n){1,}", "\n");
                }
                traces.Details = details;
            }
            JToken local = j["localEndpoint"];
            traces.ClientName = getStr(local, "serviceName");
            JToken remote = j["remoteEndpoint"];
            traces.ServiceName = getStr(remote, "serviceName");
            return traces;
        }

        private static void children(TracesInfo p, Dictionary<string, TracesInfo> dic)
        {
            TracesInfo c;
            List<TracesInfo> list;
            foreach (var key in dic.Keys)
            {
                c = dic[key];
                if (c.Pid != null && p.Id.Equals(c.Pid))
                {
                    list = p.Children;
                    if (list == null)
                    {
                        list = new List<TracesInfo>();
                        p.Children = list;
                    }
                    list.Add(c);
                    sort(list);
                    children(c, dic);
                }
            }
        }

        public static void sort(List<TracesInfo> list)
        {
            list.Sort((o1, o2) =>
            {
                if (o1.Timestamp != null && o1.Timestamp != null)
                {
                    return o1.Timestamp > o2.Timestamp ? 1 : -1;
                }
                else if (o1.Timestamp != null)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            });
        }

        private static string getStr(JToken j, string key)
        {
            return j.Value<string>(key);
        }

        private static long? getLong(JToken j, string key)
        {
            return j.Value<long?>(key);
        }

    }
}
