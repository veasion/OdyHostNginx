using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace OdyHostNginx
{
    class HttpPacketHelper
    {

        /// <summary>
        /// 获取本机ip
        /// </summary>
        public static List<string> hostIps()
        {
            List<string> ips = new List<string>();
            string strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            foreach (var item in ipEntry.AddressList)
            {
                if (item.ToString().IndexOf('.') > 0)
                {
                    ips.Add(item.ToString());
                }
            }
            return ips;
        }

        /// <summary>
        /// 获取本机代理ip
        /// </summary>
        public static string proxyIp()
        {
            List<string> ips = hostIps();
            if (ips != null && ips.Count > 0)
            {
                return ips.LastOrDefault();
            }
            return "127.0.0.1";
        }

    }
}
