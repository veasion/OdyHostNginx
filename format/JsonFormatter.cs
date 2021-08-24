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
            text = text.Trim();
            if ((text.StartsWith("\"{") && text.EndsWith("}\"")) || (text.StartsWith("\"[") && text.EndsWith("]\"")))
            {
                try
                {
                    text = StringHelper.stringFormat(text);
                    return StringHelper.jsonFormat(text);
                }
                catch (Exception) { }
            }
            return StringHelper.jsonFormat(text);
        }

    }
}
