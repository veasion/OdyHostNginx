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

        public string ServerName { get => serverName; set => serverName = value; }
        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }

    }
}
