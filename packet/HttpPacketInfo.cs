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
        private string id;
        private int pid;
        private string uri;
        private string pool;
        private string fullUrl;
        private string clientIp;
        private string reqMethod;
        private string hostname;
        private string reqBody;
        private string response;
        private int status;
        private bool isModify;
        private JToken respJson;
        private Dictionary<string, string> reqHeaders;
        private Dictionary<string, List<string>> reqCookies;
        private Dictionary<string, string> respHeaders;
        private Dictionary<string, List<string>> respCookies;

        public int Number { get => number; set => number = value; }
        public string Id { get => id; set => id = value; }
        public int Pid { get => pid; set => pid = value; }
        public string Uri { get => uri; set => uri = value; }
        public string Pool { get => pool; set => pool = value; }
        public string FullUrl { get => fullUrl; set => fullUrl = value; }
        public string ClientIp { get => clientIp; set => clientIp = value; }
        public string ReqMethod { get => reqMethod; set => reqMethod = value; }
        public string Hostname { get => hostname; set => hostname = value; }
        public string ReqBody { get => reqBody; set => reqBody = value; }
        public string Response { get => response; set => response = value; }
        public int Status { get => status; set => status = value; }
        public Dictionary<string, string> ReqHeaders { get => reqHeaders; set => reqHeaders = value; }
        public Dictionary<string, List<string>> ReqCookies { get => reqCookies; set => reqCookies = value; }
        public Dictionary<string, string> RespHeaders { get => respHeaders; set => respHeaders = value; }
        public Dictionary<string, List<string>> RespCookies { get => respCookies; set => respCookies = value; }

        private static string contentType(Dictionary<string, string> header)
        {
            header.TryGetValue("Content-Type", out string type);
            if (type == null)
            {
                header.TryGetValue("content-type", out type);
            }
            return type;
        }

        private static bool isJson(Dictionary<string, string> header, string body)
        {
            string type = contentType(header);
            if (type != null && body != null && type.ToLower().Contains("application/json"))
            {
                return true;
            }
            return false;
        }

        public bool reqIsJson()
        {
            return isJson(reqHeaders, reqBody);
        }

        public bool respIsJson()
        {
            if (respJson != null)
            {
                return true;
            }
            if (isJson(respHeaders, response))
            {
                try
                {
                    respJson = JToken.Parse(response);
                }
                catch (Exception) { }
                return true;
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

        public bool show(bool https)
        {
            if (uri != null && uri.StartsWith("/zipkin/"))
            {
                return false;
            }
            if (isModify || respJson != null)
            {
                return true;
            }
            string type = contentType(respHeaders);
            if (response != null && type != null)
            {
                type = type.ToLower();
                return type.Contains("application/json") || type.Contains("text/plain");
            }
            else if (https && "CONNECT".Equals(reqMethod))
            {
                return true;
            }
            return false;
        }

        public string Code
        {
            get
            {
                if (respIsJson() && respJson != null && respJson.Type == JTokenType.Object && respJson["code"] != null)
                {
                    return respJson.Value<string>("code");
                }
                return "";
            }
        }

        public bool IsModify { get => isModify; set => isModify = value; }

        public bool isError()
        {
            if (status == 500) return true;
            if (respIsJson() && respJson != null)
            {
                if (respJson.Type != JTokenType.Object)
                {
                    return false;
                }
                if (respJson["success"] != null)
                {
                    string success = respJson["success"].ToString().ToLower();
                    if ("true".Equals(success) || "成功".Equals(success) || "ok".Equals(success))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                if (respJson["success"] == null && respJson["message"] != null && respJson["code"] != null && !"0".Equals(respJson.Value<string>("code")))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
