using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class JsFormatter : Formatter
    {

        private static string space4 = "    ";

        public string Format(string text)
        {
            try
            {
                return formatJavaScript(text);
            }
            catch (Exception) { return null; }
        }

        private string formatJavaScript(string js)
        {
            js = js.Replace("\r\n", " ").Replace("\n", " ");
            js = StringHelper.replaceMultipleBlank(js);
            js = js.Replace("{ ", "{").Replace("; ", ";").Replace("} ", "}");
            string jsStr = js.Replace("{", "{\r").Replace("}", "}\r").Replace(";", ";\r");
            int indentIndex = 0;
            for (int i = 0; i < jsStr.Length; i++)
            {
                if (jsStr[i] == '\r' && jsStr[i - 1] == '{')
                {
                    indentIndex++;
                    jsStr = jsStr.Insert(i + 1, getSpace(indentIndex));
                    i = i + 4 * indentIndex + 1;
                }
                else if (jsStr[i] == '\r' && jsStr[i - 1] == '}')
                {
                    indentIndex--;
                    jsStr = jsStr.Remove(i - 5, 4);
                    i = i - 4;
                }

                if (jsStr[i] == '\r')
                {
                    jsStr = jsStr.Insert(i + 1, getSpace(indentIndex));
                    i = i + 4 * indentIndex + 1;
                }
            }

            return jsStr;
        }

        private string getSpace(int index)
        {
            string str = string.Empty;
            for (int i = 0; i < index; i++)
            {
                str += space4;
            }
            return str;
        }
    }
}
