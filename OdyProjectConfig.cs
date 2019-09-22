using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class OdyProjectConfig
    {

        private bool use;
        private ProjectConfig[] projects;

        public bool Use { get => use; set => use = value; }
        internal ProjectConfig[] Projects { get => projects; set => projects = value; }

    }
}
