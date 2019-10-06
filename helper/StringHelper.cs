using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OdyHostNginx
{
    class StringHelper
    {

        static string ipPattern = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$";
        static string domainPattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$";

        public static bool isEmpty(string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static bool isBlank(string str)
        {
            return isEmpty(str) || "".Equals(str.Trim());
        }

        public static bool isDomain(string domain)
        {
            return !isEmpty(domain) && Regex.IsMatch(domain, domainPattern);
        }

        public static bool isIp(string ip)
        {
            return !isEmpty(ip) && Regex.IsMatch(ip, ipPattern);
        }

        public static bool isLocalIp(string ip)
        {
            return "127.0.0.1".Equals(ip);
        }

        public static bool isPort(string port)
        {
            return !isEmpty(port) && port.Length <= 5 && isInt(port);
        }

        public static bool isInt(string str)
        {
            bool isNum;
            int vsNum;
            isNum = int.TryParse(str, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out vsNum);
            return isNum;
        }

        public static bool isNumberic(string str)
        {
            bool isNum;
            double vsNum;
            isNum = double.TryParse(str, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out vsNum);
            return isNum;
        }

        public static string substring(string str, string start, string end)
        {
            return substring(str, start, end, 0);
        }

        public static string substring(string str, string start, string end, int startIndex)
        {
            return substring(str, start, end, startIndex, -1);
        }

        public static string substring(string str, string start, string end, int startIndex, int maxDiffer)
        {
            if (isEmpty(str))
            {
                return null;
            }
            int sIndex = str.IndexOf(start, startIndex), eIndex;
            if (sIndex > -1)
            {
                sIndex += start.Length;
                eIndex = str.IndexOf(end, sIndex);
                if (eIndex > 0 && (maxDiffer == -1 || eIndex - startIndex < maxDiffer))
                {
                    return str.Substring(sIndex, eIndex - sIndex);
                }
            }
            return null;
        }

        public static string replaceLine(string str, string replace)
        {
            if (str == null) return str;
            if (str.IndexOf("\r\n") != -1)
            {
                str = str.Replace("\r\n", replace);
            }
            if (str.IndexOf("\n") != -1)
            {
                str = str.Replace("\n", replace);
            }
            return str;
        }

        public static string timeStampFormat(long jsTimeStamp, string format)
        {
            if (format == null)
            {
                format = "yyyy/MM/dd HH:mm:ss";
            }
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dt = startTime.AddMilliseconds(jsTimeStamp);
            return dt.ToString(format);
        }

        public static string jsonFormat(string json)
        {
            if (isBlank(json)) return null;
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                TextReader tr = new StringReader(json);
                JsonTextReader jtr = new JsonTextReader(tr);
                object obj = serializer.Deserialize(jtr);
                if (obj != null)
                {
                    StringWriter textWriter = new StringWriter();
                    JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, obj);
                    return textWriter.ToString();
                }
                else
                {
                    return json;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
