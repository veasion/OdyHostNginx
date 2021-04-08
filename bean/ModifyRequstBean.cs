using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class ModifyRequstBean : INotifyPropertyChanged
    {

        private int interceptType = 0; // 0 请求 1 响应
        private int matchType = 0; // 0 包含 1 等于 2 正则
        private string matchText;
        private string paramsStr; // URL参数 name=veasion&age=18
        private Dictionary<string, string> headers; // 请求头
        private string body; // body

        public int InterceptType
        {
            get => interceptType;
            set
            {
                bool update = interceptType != value;
                interceptType = value;
                if (update)
                {
                    Notify("interceptType", value);
                }
            }
        }
        public int MatchType
        {
            get => matchType;
            set
            {
                bool update = matchType != value;
                matchType = value;
                if (update)
                {
                    Notify("matchType", value);
                }
            }
        }
        public string MatchText
        {
            get => matchText;
            set
            {
                bool update = matchText != value;
                matchText = value;
                if (update)
                {
                    Notify("matchText", value);
                }
            }
        }
        public string ParamsStr
        {
            get => paramsStr;
            set
            {
                bool update = paramsStr != value;
                paramsStr = value;
                if (update)
                {
                    Notify("paramsStr", value);
                }
            }
        }
        public Dictionary<string, string> Headers
        {
            get => headers;
            set
            {
                bool update = headers != value;
                headers = value;
                if (update)
                {
                    Notify("headers", value);
                }
            }
        }
        public string Body
        {
            get => body;
            set
            {
                bool update = body != value;
                body = value;
                if (update)
                {
                    Notify("body", value);
                }
            }
        }

        private void Notify(string name, object value)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool matchFullUrl(string fullUrl)
        {
            // 0 包含 1 等于 2 正则
            if (matchType == 0)
            {
                return fullUrl.Contains(matchText);
            }
            else if (matchType == 1)
            {
                return fullUrl.Trim().Equals(matchText.Trim());
            }
            else if (matchType == 2)
            {
                return Regex.IsMatch(fullUrl, matchText);
            }
            return false;
        }

        public string getFullUrl(string fullUrl)
        {
            if (StringHelper.isBlank(paramsStr))
            {
                return fullUrl;
            }
            paramsStr = paramsStr.Trim();
            if (paramsStr.StartsWith("http://") || paramsStr.StartsWith("https://"))
            {
                // 修改整个url
                return paramsStr;
            }
            else if (paramsStr.StartsWith("/") && fullUrl.Contains("//"))
            {
                // 修改uri
                int idx = fullUrl.IndexOf("/", fullUrl.IndexOf("//") + 2);
                if (idx == -1)
                {
                    return fullUrl + paramsStr;
                }
                else
                {
                    return fullUrl.Substring(0, idx) + paramsStr;
                }
            }
            else if (paramsStr.StartsWith("&"))
            {
                // 拼接参数
                if (fullUrl.Contains("?"))
                {
                    return fullUrl + paramsStr;
                }
                else
                {
                    return fullUrl + "?" + paramsStr.Substring(1);
                }
            }
            else
            {
                // 修改参数
                string url = fullUrl;
                int index = url.IndexOf("?");
                if (index > -1)
                {
                    url = url.Substring(0, index);
                }
                url = url + (paramsStr.StartsWith("?") ? "" : "?") + paramsStr;
                return url;
            }
        }
    }
}
