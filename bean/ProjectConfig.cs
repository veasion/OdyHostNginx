using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    public class ProjectConfig
    {

        private string name;
        private List<EnvConfig> envs;

        private bool hostGroup;

        public string Name { get => name; set => name = value; }
        public bool HostGroup { get => hostGroup; set => hostGroup = value; }
        internal List<EnvConfig> Envs { get => envs; set => envs = value; }

    }
}
