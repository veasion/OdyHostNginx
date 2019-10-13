using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class UpgradeDetails
    {

        // 目录（相对路径）
        private string dir;
        // 是否清空目录
        private bool clearDir;
        // 是否替换文件
        private bool replace;
        // 文件名称
        private string fileName;
        // 下载地址
        private string downloadUrl;

        public string Dir
        {
            get
            {
                if (StringHelper.isBlank(dir))
                {
                    return "";
                }
                else if (dir.StartsWith("\\") || dir.StartsWith("/"))
                {
                    return dir;
                }
                else
                {
                    return "\\" + dir;
                }
            }
            set => dir = value;
        }
        public bool ClearDir { get => clearDir; set => clearDir = value; }
        public bool Replace { get => replace; set => replace = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string DownloadUrl { get => downloadUrl; set => downloadUrl = value; }

    }
}
