using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class HostConfig
    {

        private bool use;
        private string ip;
        private string domain;
        private string pingIp;

        public bool Use { get => use; set => use = value; }
        public string Ip { get => ip; set => ip = value; }
        public string Domain { get => domain; set => domain = value; }
        public string PingIp { get => pingIp; set => pingIp = value; }

    }
}
