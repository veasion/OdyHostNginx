using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class UpstreamDetails
    {

        private string serverName;
        private HashSet<string> uris;
        private HashSet<string> contextPaths;

        public string ServerName { get => serverName; set => serverName = value; }
        public HashSet<string> Uris { get => uris; set => uris = value; }
        public HashSet<string> ContextPaths { get => contextPaths; set => contextPaths = value; }

    }
}
