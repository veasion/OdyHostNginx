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
        // zip解压目录
        private string zipOutDir;
        // 文件名称
        private string fileName;
        // 是否运行
        private bool run;
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
        public string FileName { get => fileName; set => fileName = value; }
        public string DownloadUrl { get => downloadUrl; set => downloadUrl = value; }
        public string ZipOutDir { get => zipOutDir; set => zipOutDir = value; }
        public bool Run { get => run; set => run = value; }
    }
}
