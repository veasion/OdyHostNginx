using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class WindowsNginxImpl : Nginx
    {

        public static string nginxPath = FileHelper.getCurrentDirectory() + "\\bin\\nginx";
        public static string nginxConfigDir = nginxPath + "\\conf";
        public static string nginxExePath = nginxPath + "\\nginx.exe";
        public static string nginxLogPath = nginxPath + "\\logs\\error.log";
        public static string nginxConfigPath = nginxConfigDir + "\\nginx.conf";

        public static Encoding confEncoding = new UTF8Encoding(false);

        private const string startConfig = "# OdyHostNginx(start)";
        private const string endConfig = "# OdyHostNginx(end)";

        public void restart()
        {
            stop();
            CmdHelper.Cmd(new string[] { "cd " + nginxPath, nginxExePath });
        }

        public void stop()
        {
            CmdHelper.CloseProcess("nginx");
        }

        public void include(List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(startConfig);
            foreach (var path in list)
            {
                sb.AppendLine("include " + path + ";");
            }
            sb.AppendLine(endConfig);
            sb = readConfig(sb.ToString());
            FileHelper.writeFile(nginxConfigPath, confEncoding, sb.ToString());
        }

        public void reset()
        {
            StringBuilder sb = readConfig(null);
            FileHelper.writeFile(nginxConfigPath, confEncoding, sb.ToString());
        }

        private StringBuilder readConfig(string include)
        {
            Flag f = new Flag
            {
                count = 0,
                flag = false
            };
            ArrayList list = new ArrayList();
            FileHelper.readTextFile(nginxConfigPath, confEncoding, (index, line) =>
            {
                if (line.StartsWith(startConfig))
                {
                    f.flag = true;
                    return;
                }
                else if (line.EndsWith(endConfig))
                {
                    f.flag = false;
                    return;
                }
                if (!f.flag)
                {
                    list.Add(line);
                    if (include != null && "}".Equals(line.Trim()))
                    {
                        f.count = list.Count - 1;
                    }
                }
            });
            if (include != null && f.count > 0)
            {
                list.Insert(f.count, include);
            }

            bool isBlank = false;
            StringBuilder sb = new StringBuilder();
            foreach (string item in list)
            {
                if (isBlank && StringHelper.isBlank(item))
                {
                    continue;
                }
                else
                {
                    isBlank = false;
                }
                if (StringHelper.isBlank(item))
                {
                    isBlank = true;
                }
                sb.AppendLine(item);
            }
            return sb;
        }

    }
}
