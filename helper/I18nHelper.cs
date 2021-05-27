using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class I18nHelper
    {
        private string appId;
        private string appSecret;

        private string i18nPrefix = "$t('";
        private string i18nSuffix = "')";
        private string staticPool = "vue-static";
        private string enJsFilePath = "packages\\lang\\en_US.js";

        public I18nHelper(string appId, string appSecret)
        {
            this.appId = appId;
            this.appSecret = appSecret;
        }

        public I18nHelper()
        {
            string configPath = FileHelper.getCurrentDirectory() + "\\bin\\config\\i18n.json";
            if (!File.Exists(configPath))
            {
                Logger.error("\\bin\\config\\i18n.json" + "配置不存在");
            }
            try
            {
                string json = FileHelper.readTextFile(configPath);
                Dictionary<string, string> config = JToken.Parse(json).ToObject<Dictionary<string, string>>();
                this.appId = config["appId"];
                this.appSecret = config["appSecret"];
                if (config.TryGetValue("i18nPrefix", out string i18nPrefix))
                {
                    this.i18nPrefix = i18nPrefix;
                }
                if (config.TryGetValue("i18nSuffix", out string i18nSuffix))
                {
                    this.i18nSuffix = i18nSuffix;
                }
                if (config.TryGetValue("enJsFilePath", out string enJsFilePath))
                {
                    this.enJsFilePath = enJsFilePath;
                }
                if (config.TryGetValue("staticPool", out string staticPool))
                {
                    this.staticPool = staticPool;
                }
            }
            catch (Exception e)
            {
                Logger.error("加载" + configPath, e);
                throw e;
            }
        }

        public HashSet<string> getPlaceHolders(string file, string context, bool skipNonChinese)
        {
            HashSet<string> result = getPlaceHolders(context, skipNonChinese);
            try
            {
                if (file != null && file.EndsWith(".js") && file.Contains("router"))
                {
                    // router > title: ''
                    int startIndex = 0;
                    while ((startIndex = context.IndexOf("title: '", startIndex)) != -1)
                    {
                        string title = StringHelper.substring(context, "title: '", "'", startIndex);
                        if (StringHelper.hasChinese(title))
                        {
                            result.Add(title);
                        }
                        startIndex = context.IndexOf("'", startIndex + 1);
                    }
                }
                int startData = context.IndexOf("data() {");
                if (startData > 0)
                {
                    string dataVue = context.Substring(startData);
                    // label: ''
                    int startIndex = 0;
                    while ((startIndex = dataVue.IndexOf("label: '", startIndex)) != -1)
                    {
                        string label = StringHelper.substring(dataVue, "label: '", "'", startIndex);
                        if (StringHelper.hasChinese(label))
                        {
                            result.Add(label);
                        }
                        startIndex = dataVue.IndexOf("'", startIndex + 1);
                    }
                }
            }
            catch (Exception) { }
            return result;
        }

        public HashSet<string> getPlaceHolders(string context, bool skipNonChinese)
        {
            HashSet<string> result = StringHelper.GetPlaceHolders(context, i18nPrefix, i18nSuffix);
            if (result != null && skipNonChinese)
            {
                HashSet<string> hasChineseResult = new HashSet<string>();
                foreach (var item in result)
                {
                    if (StringHelper.hasChinese(item))
                    {
                        hasChineseResult.Add(item);
                    }
                }
                return hasChineseResult;
            }
            else
            {
                return result;
            }
        }

        public string translate(string zh)
        {
            int salt = new Random().Next(1000, 10000);

            StringBuilder url = new StringBuilder();
            url.Append("http://api.fanyi.baidu.com/api/trans/vip/translate?from=zh&to=en");
            url.Append("&appid=").Append(appId);
            url.Append("&q=").Append(StringHelper.UrlEncode(zh));
            url.Append("&salt=").Append(salt);
            url.Append("&sign=").Append(StringHelper.md5(appId + zh + salt + appSecret));

            Logger.info(url.ToString());

            string json = HttpHelper.get(url.ToString());
            Logger.info("翻译：" + zh + " > " + json);
            JToken jt = JToken.Parse(json);
            if (jt["error_code"] != null && "54003".Equals(jt["error_code"].ToString()))
            {
                System.Threading.Thread.Sleep(1000);
                return translate(zh);
            }
            else
            {
                return jt["trans_result"][0]["dst"].ToString();
            }
        }

        public string getEnJsFilePath(string vuePath)
        {
            FileInfo file = new FileInfo(vuePath);
            DirectoryInfo dir = file.Directory;
            while ((dir = dir.Parent) != null && dir.Exists)
            {
                if (staticPool.Equals(dir.Name))
                {
                    return dir.FullName + "\\" + enJsFilePath;
                }
            }
            return null;
        }

        public bool hasKey(string context, string key)
        {
            int index = -1;
            int len = context.Length;
            while ((index = context.IndexOf(key, index + 1)) > -1)
            {
                if (index > 0 && (context[index - 1] != '\'' && context[index - 1] != '"'))
                {
                    continue;
                }
                if (index + key.Length < len && (context[index + key.Length] == '\'' || context[index + key.Length] == '"'))
                {
                    return true;
                }
            }
            return false;
        }

        public string getJSON(Dictionary<string, string> enMap)
        {
            int count = enMap.Count;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            foreach (var key in enMap.Keys)
            {
                count--;
                sb.Append("  '").Append(key.Replace("'", @"\'"));
                sb.Append("': '");
                sb.Append(enMap[key].Replace("'", @"\'"));
                sb.Append("'");
                if (count > 0)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }

    }
}
