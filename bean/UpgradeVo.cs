using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class UpgradeVo
    {

        // 更新版本
        private string version;
        // 更新类型：1软件，2环境，3软件和环境
        private int updateType;
        // 是否强制升级
        private bool force;
        // 描述
        private string desc;
        // 打赏
        private string payImageUrl;
        // 新增
        private List<UpgradeDetails> adds;
        // 删除
        private List<UpgradeDetails> deletes;
        // 软件手动下载地址
        private string odyHostNginxDownload;
        // 项目手动下载地址
        private string odyHostNginxProjectDownload;

        public string Version { get => version; set => version = value; }
        public int UpdateType { get => updateType; set => updateType = value; }
        public bool Force { get => force; set => force = value; }
        public string Desc { get => desc; set => desc = value; }
        public string PayImageUrl { get => payImageUrl; set => payImageUrl = value; }
        internal List<UpgradeDetails> Adds { get => adds; set => adds = value; }
        internal List<UpgradeDetails> Deletes { get => deletes; set => deletes = value; }
        public string OdyHostNginxDownload { get => odyHostNginxDownload; set => odyHostNginxDownload = value; }
        public string OdyHostNginxProjectDownload { get => odyHostNginxProjectDownload; set => odyHostNginxProjectDownload = value; }

    }
}
