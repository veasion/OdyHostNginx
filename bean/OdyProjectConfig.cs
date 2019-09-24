using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class OdyProjectConfig
    {

        private bool use;
        private List<ProjectConfig> projects;

        public bool Use { get => use; set => use = value; }
        internal List<ProjectConfig> Projects { get => projects; set => projects = value; }

    }
}
