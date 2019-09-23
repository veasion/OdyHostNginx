using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OdyHostNginx
{
    class NginxConfig
    {

        private string fileName;
        private string listen;
        private string serverName;
        private string body;

        private string filePath;

        private EnvConfig env;

        public string FileName { get => fileName; set => fileName = value; }
        public string Listen { get => listen; set => listen = value; }
        public string ServerName { get => serverName; set => serverName = value; }
        public string Body
        {
            get
            {
                // body 懒加载
                if (body != null)
                {
                    return body;
                }
                else if (filePath != null)
                {
                    return FileHelper.readTextFile(filePath);
                }
                return body;
            }
            set => body = value;
        }
        public string FilePath { get => filePath; set => filePath = value; }
        internal EnvConfig Env { get => env; set => env = value; }

    }
}
