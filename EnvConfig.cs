using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class EnvConfig
    {

        private bool use;
        private string envName;
        private List<NginxConfig> configs;
        private String upstreamFileName;
        private List<NginxUpstream> upstreams;

        public bool Use { get => use; set => use = value; }
        public string EnvName { get => envName; set => envName = value; }
        internal List<NginxConfig> Configs { get => configs; set => configs = value; }
        public string UpstreamFileName { get => upstreamFileName; set => upstreamFileName = value; }
        internal List<NginxUpstream> Upstreams { get => upstreams; set => upstreams = value; }

    }
}
