using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class ProjectConfig
    {

        private bool use;
        private string name;
        private List<EnvConfig> envs;

        public bool Use { get => use; set => use = value; }
        public string Name { get => name; set => name = value; }
        internal List<EnvConfig> Envs { get => envs; set => envs = value; }

    }
}
