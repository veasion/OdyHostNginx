using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class ConnectionParams
    {

        private string ip;
        private int port;
        private string user;
        private string pwd;

        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public string User { get => user; set => user = value; }
        public string Pwd { get => pwd; set => pwd = value; }

    }
}
