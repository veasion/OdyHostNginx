using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class NginxConfig
    {

        private string fileName;
        private string listen;
        private string serverName;
        private string body;

        private EnvConfig env;

        public string FileName { get => fileName; set => fileName = value; }
        public string Listen { get => listen; set => listen = value; }
        public string ServerName { get => serverName; set => serverName = value; }
        public string Body { get => body; set => body = value; }
        internal EnvConfig Env { get => env; set => env = value; }

    }
}
