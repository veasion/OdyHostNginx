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
        private List<string> uris;
        private List<string> contextPaths;

        public string ServerName { get => serverName; set => serverName = value; }
        public List<string> Uris { get => uris; set => uris = value; }
        public List<string> ContextPaths { get => contextPaths; set => contextPaths = value; }

    }
}
