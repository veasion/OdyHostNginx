using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class SSHConnInfo
    {

        string user;
        string password;
        string ip;
        int port = 22;

        string contextPath;
        int serverPort;

        public string User { get => user; set => user = value; }
        public string Password { get => password; set => password = value; }
        public string Ip { get => ip; set => ip = value; }
        public int Port { get => port; set => port = value; }
        public string ContextPath { get => contextPath; set => contextPath = value; }
        public int ServerPort { get => serverPort; set => serverPort = value; }

    }
}
