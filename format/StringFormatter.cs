using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class StringFormatter : Formatter
    {

        public string Format(string text)
        {
            try
            {
                return StringHelper.stringFormat(text);
            }
            catch (Exception) { return null; }
        }

    }
}
