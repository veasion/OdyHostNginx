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
        private string server_name;
        private string body;

        public string FileName { get => fileName; set => fileName = value; }
        public string Listen { get => listen; set => listen = value; }
        public string Server_name { get => server_name; set => server_name = value; }
        public string Body { get => body; set => body = value; }

    }
}
