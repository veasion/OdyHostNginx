using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class HttpPacketInfo
    {

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
            string type = reqHeaders["Content-Type"];
            if (reqBody != null && type != null && type.Contains("application/json"))
            {
                return true;
            }
            return false;
        }

        public bool respIsJson()
        {
            string type = respHeaders["Content-Type"];
            if (response != null && type != null && type.Contains("application/json"))
            {
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

    }
}
