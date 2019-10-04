using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    public class EnvConfig
    {

        private bool use;
        private string envName;
        private List<NginxConfig> configs;
        private String upstreamFileName;
        private List<NginxUpstream> upstreams;
        private List<HostConfig> hosts;

        private ProjectConfig project;

        public bool Use { get => use; set => use = value; }
        public string EnvName { get => envName; set => envName = value; }
        internal List<NginxConfig> Configs { get => configs; set => configs = value; }
        public string UpstreamFileName { get => upstreamFileName; set => upstreamFileName = value; }
        internal List<NginxUpstream> Upstreams { get => upstreams; set => upstreams = value; }
        internal ProjectConfig Project { get => project; set => project = value; }
        internal List<HostConfig> Hosts { get => hosts; set => hosts = value; }

    }
}
