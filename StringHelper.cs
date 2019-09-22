using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class StringHelper
    {

        public static bool isEmpty(string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static bool isBlank(string str)
        {
            return isEmpty(str) || "".Equals(str.Trim());
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
            if (isEmpty(str))
            {
                return null;
            }
            int sIndex = str.IndexOf(start, startIndex);
            int eIndex = -1;
            if (sIndex > -1)
            {
                sIndex += start.Length;
                eIndex = str.IndexOf(end, sIndex);
                if (eIndex > 0)
                {
                    return str.Substring(sIndex, eIndex - sIndex);
                }
            }
            return null;
        }

    }
}
