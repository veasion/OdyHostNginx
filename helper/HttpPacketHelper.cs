using System;
using System.Collections.Generic;
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
            // TODO
            return hostIps()[0];
        }

    }
}
