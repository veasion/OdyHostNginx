using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class DbCombox
    {

        private string label;
        private string value;

        public DbCombox() { }
        public DbCombox(string label, string value)
        {
            this.label = label;
            this.value = value;
        }

        public string Label { get => label; set => label = value; }
        public string Value { get => value; set => this.value = value; }

    }
}
