using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    public class NginxUpstream
    {

        private string serverName;
        private string ip;
        private int port;

        private string oldIp;
        private int oldPort;

        private EnvConfig env;
        private UpstreamDetails ud;

        private string contextPath;

        public string ServerName { get => serverName; set => serverName = value; }
        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public string OldIp { get => oldIp; set => oldIp = value; }
        public int OldPort { get => oldPort; set => oldPort = value; }
        public string ContextPath { get => contextPath; set => contextPath = value; }
        internal EnvConfig Env { get => env; set => env = value; }
        internal UpstreamDetails Ud { get => ud; set => ud = value; }
    }

}
