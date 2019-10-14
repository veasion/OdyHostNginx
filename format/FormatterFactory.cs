using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class FormatterFactory
    {

        public static Formatter GetFormatter(string type)
        {
            switch (type)
            {
                case "json": return new JsonFormatter();
                case "sql": return new SqlFormatter();
                case "js": return new JsFormatter();
                case "html": return new HtmlFormatter();
                case "xml": return new XmlFormatter();
            }
            return new JsonFormatter();
        }

    }
}
