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

        public static TracesInfo traces(string url)
        {
            TracesInfo root = null;
            url = url.Replace("/zipkin/traces/", "/zipkin/api/v1/trace/");
            string jsonStr = HttpPacketHelper.get(url);
            if (StringHelper.isEmpty(jsonStr)) return null;
            TracesInfo traces;
            Dictionary<string, TracesInfo> dic = new Dictionary<string, TracesInfo>();
            JToken jsonArr = JToken.Parse(jsonStr);
            foreach (var j in jsonArr)
            {
                traces = new TracesInfo();
                traces.Id = getStr(j, "id");
                if (traces.Id == null) continue;
                dic[traces.Id] = traces;
                if (root == null && traces.Id.Equals(getStr(j, "traceId")))
                {
                    root = traces;
                }
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
            }
            children(root, dic);
            return root;
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
