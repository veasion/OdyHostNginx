using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    public class OdyProjectConfig
    {

        private bool use;
        private List<ProjectConfig> projects;
        private JToken config = JToken.Parse("{}");

        public bool Use { get => use; set => use = value; }
        public JToken Config { get => config; set => config = value; }
        internal List<ProjectConfig> Projects { get => projects; set => projects = value; }

    }
}
