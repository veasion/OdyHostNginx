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
            if (url.IndexOf("/zipkin/") > 0)
            {
                return zipkin(url);
            }
            else if (url.IndexOf("/trace") > 0)
            {
                return skywalking(url);
            }
            return null;
        }

        private static TracesInfo skywalking(string url)
        {
            int idx1 = url.IndexOf("/trace");
            int idx2 = url.IndexOf("traceid=");
            string baseUrl = url.Substring(0, idx1);
            string traceid = url.Substring(idx2).Replace("traceid=", "");
            string graphqlUrl = baseUrl + "/graphql";
            return skywalkingTrace(graphqlUrl, traceid);
        }

        private static TracesInfo skywalkingTrace(string graphqlUrl, string traceid)
        {
            // Dictionary<string, string> servicesMap = services(graphqlUrl, traceid);
            string reqJson = "{\"query\":\"query queryTraces($condition: TraceQueryCondition) {\\n  data: queryBasicTraces(condition: $condition) {\\n    traces {\\n      key: segmentId\\n      endpointNames\\n      duration\\n      start\\n      isError\\n      traceIds\\n    }\\n    total\\n  }}\",\"variables\":{\"condition\":{\"queryDuration\":{\"start\":\"2000-01-01\",\"end\":\"2099-01-01\",\"step\":\"MINUTE\"},\"traceState\":\"ALL\",\"paging\":{\"pageNum\":1,\"pageSize\":15,\"needTotal\":true},\"queryOrder\":\"BY_DURATION\",\"traceId\":\"" + traceid + "\"}}}";
            string response = HttpHelper.post(graphqlUrl, reqJson);
            if (StringHelper.isEmpty(response)) return null;
            JToken jsonObj = JToken.Parse(response);
            if (jsonObj == null || jsonObj["data"] == null)
            {
                return null;
            }
            JToken traces = jsonObj["data"]["data"]["traces"];
            int total;
            int.TryParse(jsonObj["data"]["data"]["total"].ToString(), out total);
            if (traces == null || total <= 0)
            {
                return null;
            }
            List<TracesInfo> list = new List<TracesInfo>();
            foreach (var dj in traces)
            {
                TracesInfo trace = new TracesInfo();
                trace.Id = getStr(dj, "key");
                trace.Pid = null;
                trace.Name = getArrStr(dj, "endpointNames");
                trace.Timestamp = getLong(dj, "start");
                trace.Error = dj.Value<bool?>("isError");
                trace.Duration = getLong(dj, "duration");
                List<string> traceIds = getArray(dj, "traceIds");
                trace.Children = new List<TracesInfo>();
                // trace.ClientName, trace.ServiceName, trace.Details
                foreach (var id in traceIds)
                {
                    loadTraceSpans(graphqlUrl, trace, id);
                }
                list.Add(trace);
                // break;
            }
            if (list.Count <= 0)
            {
                return null;
            }
            TracesInfo result = list[0];
            if (list.Count > 1)
            {
                TracesInfo trace = new TracesInfo();
                trace.Id = "-1";
                trace.Pid = "-1";
                trace.Name = "root";
                trace.Children = new List<TracesInfo>();
                foreach (var item in list)
                {
                    trace.Children.Add(item);
                }
                result = trace;
            }
            return result;
        }

        private static void loadTraceSpans(string graphqlUrl, TracesInfo parent, string traceId)
        {
            string reqJson = "{\"query\":\"query queryTrace($traceId: ID!) {\\n  trace: queryTrace(traceId: $traceId) {\\n    spans {\\n      traceId\\n      segmentId\\n      spanId\\n      parentSpanId\\n      refs {\\n        traceId\\n        parentSegmentId\\n        parentSpanId\\n        type\\n      }\\n      serviceCode\\n      startTime\\n      endTime\\n      endpointName\\n      type\\n      peer\\n      component\\n      isError\\n      layer\\n      tags {\\n        key\\n        value\\n      }\\n      logs {\\n        time\\n        data {\\n          key\\n          value\\n        }\\n      }\\n    }\\n  }\\n  }\",\"variables\":{\"traceId\":\"" + traceId + "\"}}";
            string response = HttpHelper.post(graphqlUrl, reqJson);
            JToken jsonObj = JToken.Parse(response);
            if (jsonObj == null || jsonObj["data"] == null)
            {
                return;
            }
            JToken spans = jsonObj["data"]["trace"]["spans"];
            if (spans == null || spans.Count() <= 0)
            {
                return;
            }
            string startId = null;
            TracesInfo trace;
            Dictionary<string, string> nextMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> children = new Dictionary<string, List<string>>();
            Dictionary<string, TracesInfo> segmentSpanIdMap = new Dictionary<string, TracesInfo>();
            foreach (var span in spans)
            {
                string segmentId = getStr(span, "segmentId");
                long spanId = (long)getLong(span, "spanId");
                long startTime = (long)getLong(span, "startTime");
                long endTime = (long)getLong(span, "endTime");
                long parentSpanId = (long)getLong(span, "parentSpanId");
                trace = new TracesInfo();
                trace.Id = segmentId + "_" + spanId;
                trace.Pid = segmentId + "_" + parentSpanId;
                trace.Name = getStr(span, "endpointName");
                trace.Timestamp = startTime;
                trace.Duration = endTime - startTime;
                trace.ServiceName = getStr(span, "serviceCode");
                trace.ClientName = getStr(span, "layer") + " - " + getStr(span, "component");
                trace.Error = span.Value<bool?>("isError");
                Dictionary<string, string> details = new Dictionary<string, string>();
                details.Add("endpointName", trace.Name);
                details.Add("serviceCode", trace.ServiceName);
                details.Add("component", trace.ClientName);
                details.Add("peer", getStr(span, "peer"));
                details.Add("type", getStr(span, "type"));
                if (span["tags"] != null)
                {
                    foreach (var dj in span["tags"])
                    {
                        string key = getStr(dj, "key");
                        string value = getStr(dj, "value");
                        if (key != null)
                        {
                            if (("query.sql".Equals(key) || "db.statement".Equals(key)) && value != null)
                            {
                                value = Regex.Replace(value, @"(\n\s{0,}\n){1,}", "\n");
                            }
                            details[key] = value;
                        }
                    }
                }
                trace.Details = details;

                JToken refs = span["refs"];
                if (refs != null && refs.Count() > 0)
                {
                    JToken refToken = refs.ElementAt(0);
                    string pId = getStr(refToken, "parentSegmentId") + "_" + getStr(refToken, "parentSpanId");
                    if (!children.ContainsKey(pId))
                    {
                        children[pId] = new List<string>();
                    }
                    if (!children[pId].Contains(trace.Id))
                    {
                        children[pId].Add(trace.Id);
                    }
                }

                if (startId == null)
                {
                    startId = trace.Id;
                }

                if (parentSpanId >= 0)
                {
                    if (!children.ContainsKey(trace.Pid))
                    {
                        children[trace.Pid] = new List<string>();
                    }
                    if (!children[trace.Pid].Contains(trace.Id))
                    {
                        children[trace.Pid].Add(trace.Id);
                    }
                }

                segmentSpanIdMap[trace.Id] = trace;
            }
            // 组装数据
            if (startId == null)
            {
                return;
            }
            buildSegmentSpan(parent, startId, segmentSpanIdMap, children, nextMap);
        }

        private static void buildSegmentSpan(TracesInfo parent, string id,
            Dictionary<string, TracesInfo> segmentSpanIdMap,
            Dictionary<string, List<string>> children,
            Dictionary<string, string> nextMap)
        {
            TracesInfo trace;
            do
            {
                if (!segmentSpanIdMap.ContainsKey(id))
                {
                    break;
                }
                trace = segmentSpanIdMap[id];
                if (parent.Children == null)
                {
                    parent.Children = new List<TracesInfo>();
                }
                if (children.ContainsKey(id))
                {
                    foreach (var childId in children[id])
                    {
                        buildSegmentSpan(trace, childId, segmentSpanIdMap, children, nextMap);
                    }
                }
                parent.Children.Add(trace);
            } while (nextMap.TryGetValue(id, out id));
        }

        private static Dictionary<string, string> services(string graphqlUrl, string traceid)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string reqJson = "{\"query\":\"query queryServices($duration: Duration!,$keyword: String!) {\\n    services: getAllServices(duration: $duration, group: $keyword) {\\n      key: id\\n      label: name\\n    }\\n  }\",\"variables\":{\"duration\":{\"start\":\"2000-01-01\",\"end\":\"2099-01-01\",\"step\":\"MINUTE\"},\"keyword\":\"\"}}";
            string response = HttpHelper.post(graphqlUrl, reqJson);
            if (!StringHelper.isEmpty(response))
            {
                JToken jsonObj = JToken.Parse(response);
                foreach (var dj in jsonObj["data"]["services"])
                {
                    string key = getStr(dj, "key");
                    string value = getStr(dj, "label");
                    dic.Add(key, value);
                }
            }
            return dic;
        }

        private static TracesInfo zipkin(string url)
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
            traces.Duration = getLong(j, "duration") / 1000;
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
            traces.Duration = getLong(j, "duration") / 1000;
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

        private static List<string> getArray(JToken j, string key)
        {
            return j[key].ToObject<List<string>>();
        }

        private static string getArrStr(JToken j, string key)
        {
            List<string> array = getArray(j, key);
            if (array != null && array.Count > 0)
            {
                string val = null;
                foreach (var item in array)
                {
                    if (val != null && val != "")
                    {
                        val = val + " " + item;
                    }
                    else
                    {
                        val = item;
                    }
                }
                return val;
            }
            return null;
        }

        private static long? getLong(JToken j, string key)
        {
            return j.Value<long?>(key);
        }

    }
}
