using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class JsonFormatter : Formatter
    {
        public string Format(string text)
        {
            return StringHelper.jsonFormat(text);
        }

    }
}
