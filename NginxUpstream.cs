﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class NginxUpstream
    {

        private string serverName;
        private string ip;
        private int port;

        private List<string> uris;
        private List<string> contextPaths;

        private EnvConfig env;

        public string ServerName { get => serverName; set => serverName = value; }
        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public List<string> Uris { get => uris; set => uris = value; }
        public List<string> ContextPaths { get => contextPaths; set => contextPaths = value; }
        internal EnvConfig Env { get => env; set => env = value; }

    }
}
