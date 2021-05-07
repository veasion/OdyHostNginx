using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OdyHostNginx
{
    public class StringHelper
    {

        static Regex blankReg = new Regex(@"[\s]+");
        static string chinesePattern = @"[\u4e00-\u9fa5]+";
        static string envNamePattern = @"^[-a-zA-Z0-9_]+$";
        static string ipPattern = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$";
        static string domainPattern = @"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$";

        public static bool isEmpty(string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static bool hasChinese(string str)
        {
            return !isEmpty(str) && Regex.IsMatch(str, chinesePattern);
        }

        public static bool isBlank(string str)
        {
            return isEmpty(str) || "".Equals(str.Trim());
        }

        public static bool isDomain(string domain)
        {
            return !isEmpty(domain) && Regex.IsMatch(domain, domainPattern);
        }

        public static bool isEnvName(string envName)
        {
            return !isEmpty(envName) && Regex.IsMatch(envName, envNamePattern);
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

        public static string replaceMultipleBlank(string str)
        {
            if (str == null) return str;
            return blankReg.Replace(str, " ");
        }

        public static string timeStampFormat(long jsTimeStamp, string format)
        {
            if (format == null)
            {
                format = "yyyy/MM/dd HH:mm:ss.fff";
            }
            if (jsTimeStamp.ToString().Length >= 16)
            {
                jsTimeStamp = jsTimeStamp / 1000;
            }
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dt = startTime.AddMilliseconds(jsTimeStamp);
            return dt.ToString(format);
        }

        public static string hash(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 1)
            {
                return "";
            }
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
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
            catch (Exception e)
            {
                Logger.error("json格式化异常", e);
                return null;
            }
        }

        public static string upFirst(string str)
        {
            if (isEmpty(str))
            {
                return str;
            }
            str = str.Trim();
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        public static string toVar(string str)
        {
            if (isEmpty(str))
            {
                return str;
            }
            str = str.Trim().ToLower();
            bool has_ = str.Contains("_");
            StringBuilder result = new StringBuilder();
            string[] chars = str.Split('_');
            foreach (string _char in chars)
            {
                if (_char == null || "".Equals(_char.Trim())) continue;
                if (!has_)
                {
                    result.Append(_char);
                    continue;
                }
                if (result.Length == 0)
                {
                    result.Append(_char.ToLower());
                }
                else
                {
                    result.Append(_char.Substring(0, 1).ToUpper());
                    result.Append(_char.Substring(1).ToLower());
                }
            }
            return result.ToString();
        }

        public static HashSet<string> GetPlaceHolders(string s, string placeHolderPrefix, string placeHolderSuffix)
        {
            HashSet<string> phs = new HashSet<string>();
            string placeHolder = null;
            string sc;
            for (int beginIndex = 0; (placeHolder = DoGetPlaceHolder(s, beginIndex, placeHolderPrefix, placeHolderSuffix)) != null; beginIndex = s.IndexOf(sc, beginIndex) + sc.Length)
            {
                phs.Add(placeHolder);
                sc = placeHolderPrefix + placeHolder + placeHolderSuffix;
            }
            return phs;
        }

        private static string DoGetPlaceHolder(string s, int beginIndex, string placeHolderPrefix, string placeHolderSuffix)
        {
            string str = s.Substring(beginIndex);
            int start = str.IndexOf(placeHolderPrefix);
            if (start != -1)
            {
                int end = str.IndexOf(placeHolderSuffix, start + 1);
                if (end != -1)
                {
                    return Substring(str, start + placeHolderPrefix.Length, end);
                }
            }
            return null;
        }

        public static string Substring(string str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }

        public static string md5(string Text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(Text);
            try
            {
                MD5CryptoServiceProvider check;
                check = new MD5CryptoServiceProvider();
                byte[] somme = check.ComputeHash(buffer);
                string ret = "";
                foreach (byte a in somme)
                {
                    if (a < 16)
                        ret += "0" + a.ToString("X");
                    else
                        ret += a.ToString("X");
                }
                return ret.ToLower();
            }
            catch
            {
                throw;
            }
        }

    }
}
