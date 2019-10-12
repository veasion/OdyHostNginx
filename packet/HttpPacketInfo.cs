using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class HttpPacketInfo
    {

        private int number;
        private int id;
        private int pid;
        private string uri;
        private string pool;
        private string fullUrl;
        private string clientIp;
        private string reqMethod;
        private string hostname;
        private string reqBody;
        private string response;
        private int responseCode;
        private Dictionary<string, string> reqHeaders;
        private Dictionary<string, List<string>> reqCookies;
        private Dictionary<string, string> respHeaders;
        private Dictionary<string, List<string>> respCookies;

        public int Number { get => number; set => number = value; }
        public int Id { get => id; set => id = value; }
        public int Pid { get => pid; set => pid = value; }
        public string Uri { get => uri; set => uri = value; }
        public string Pool { get => pool; set => pool = value; }
        public string FullUrl { get => fullUrl; set => fullUrl = value; }
        public string ClientIp { get => clientIp; set => clientIp = value; }
        public string ReqMethod { get => reqMethod; set => reqMethod = value; }
        public string Hostname { get => hostname; set => hostname = value; }
        public string ReqBody { get => reqBody; set => reqBody = value; }
        public string Response { get => response; set => response = value; }
        public int ResponseCode { get => responseCode; set => responseCode = value; }
        public Dictionary<string, string> ReqHeaders { get => reqHeaders; set => reqHeaders = value; }
        public Dictionary<string, List<string>> ReqCookies { get => reqCookies; set => reqCookies = value; }
        public Dictionary<string, string> RespHeaders { get => respHeaders; set => respHeaders = value; }
        public Dictionary<string, List<string>> RespCookies { get => respCookies; set => respCookies = value; }

        public bool reqIsJson()
        {
            if (reqHeaders.TryGetValue("Content-Type", out string type))
            {
                if (reqBody != null && type != null && type.Contains("application/json"))
                {
                    return true;
                }
            }
            return false;
        }

        public bool respIsJson()
        {
            if (respHeaders.TryGetValue("Content-Type", out string type))
            {
                if (response != null && type != null && type.Contains("application/json"))
                {
                    return true;
                }
            }
            return false;
        }

        public string reqCookie(string name)
        {
            if (reqCookies != null && reqCookies.TryGetValue(name, out List<string> value))
            {
                return value != null && value.Count > 0 ? value[0] : null;
            }
            return null;
        }

        public string respCookie(string name)
        {
            if (respCookies != null && respCookies.TryGetValue(name, out List<string> value))
            {
                return value != null && value.Count > 0 ? value[0] : null;
            }
            return null;
        }

        public List<string> queryParams()
        {
            int s = fullUrl.IndexOf("?");
            if (s != -1)
            {
                List<string> paramList = new List<string>();
                string[] paramArr = fullUrl.Substring(s + 1).Split('&');
                foreach (var item in paramArr)
                {
                    paramList.Add(item);
                }
                return paramList;
            }
            return null;
        }

        public Dictionary<string, string> queryParamMap()
        {
            List<string> paramList = queryParams();
            if (paramList != null)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (var item in paramList)
                {
                    string[] eq = item.Split('=');
                    if (eq.Length > 1)
                    {
                        dic[eq[0]] = eq[1];
                    }
                }
                return dic;
            }
            return null;
        }

        public string ut()
        {
            return reqCookie("ut");
        }

        public string trace()
        {
            if (respHeaders.TryGetValue("Trace-Full-Info", out string trace))
            {
                int index = trace.IndexOf("http://");
                if (index == -1)
                {
                    index = trace.IndexOf("https://");
                }
                if (index != -1)
                {
                    return trace.Substring(index);
                }
            }
            return null;
        }

        public bool show()
        {
            if (uri != null && uri.StartsWith("/zipkin/"))
            {
                return false;
            }
            if (respHeaders.TryGetValue("Content-Type", out string type))
            {
                if (response != null && type != null)
                {
                    return type.Contains("application/json") || type.Contains("text/plain");
                }
            }
            return false;
        }

        public bool isError()
        {
            if (responseCode == 500) return true;
            if (respIsJson())
            {
                try
                {
                    JToken json = JToken.Parse(response);
                    if (json != null)
                    {
                        if (json["success"] != null && !json.Value<bool>("success"))
                        {
                            return true;
                        }
                        if (json["success"] == null && json["message"] != null && json["code"] != null && !"0".Equals(json.Value<string>("code")))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception) { }
            }
            return false;
        }

    }
}
