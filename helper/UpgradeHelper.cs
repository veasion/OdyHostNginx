using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OdyHostNginx
{
    class UpgradeHelper
    {

        public const string version = "v2.0.7";
        public static string reqUrl = "https://veasion.oss-cn-shanghai.aliyuncs.com/ody/OdyHostNginx/upgrade.json";

        private static UpgradeVo upgrade;
        public static string upgradeDir = FileHelper.getCurrentDirectory() + "_Upgrade";
        private static string updatePath = FileHelper.getCurrentDirectory() + "\\update.exe";
        private static string jsonPath = FileHelper.getCurrentDirectory() + "\\bin\\upgrade\\upgrade.json";
        private static string alipayPath = FileHelper.getCurrentDirectory() + "\\bin\\upgrade\\alipay.png";
        private static string lockPath = FileHelper.getCurrentDirectory() + "\\bin\\upgrade\\lock";


        public static UpgradeVo getUpgrade(bool? latest)
        {
            if (latest == true && (!File.Exists(lockPath) && File.Exists(jsonPath)))
            {
                return reqUpgrade();
            }
            if (upgrade == null)
            {
                try
                {
                    string json = FileHelper.readTextFile(jsonPath);
                    if (!StringHelper.isEmpty(json))
                    {
                        upgrade = parse(json);
                    }
                }
                catch (Exception e)
                {
                    Logger.error("解析升级json数据", e);
                }
            }
            return upgrade;
        }

        private static UpgradeVo reqUpgrade()
        {
            try
            {
                Logger.info("检查更新...");
                string json = HttpHelper.get(reqUrl);
                if (StringHelper.isEmpty(json))
                {
                    return upgrade;
                }
                FileInfo file = new FileInfo(jsonPath);
                if (!file.Directory.Exists)
                {
                    FileHelper.mkdir(file.DirectoryName);
                }
                FileHelper.writeFile(jsonPath, json);
                upgrade = parse(json);
                FileInfo fileInfo = new FileInfo(alipayPath);
                if (!fileInfo.Exists || isNeedUpdate(upgrade))
                {
                    if (!StringHelper.isEmpty(upgrade.PayImageUrl))
                    {
                        HttpHelper.downloadFile(upgrade.PayImageUrl, alipayPath);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.error("下载更新文件", e);
            }
            return upgrade;
        }

        private static UpgradeVo parse(string json)
        {
            JToken jt = JToken.Parse(json);
            UpgradeVo upgrade = jt.ToObject<UpgradeVo>();
            if (upgrade != null)
            {
                if (jt["adds"] != null)
                {
                    upgrade.Adds = jt["adds"].ToObject<List<UpgradeDetails>>();
                }
                if (jt["deletes"] != null)
                {
                    upgrade.Deletes = jt["deletes"].ToObject<List<UpgradeDetails>>();
                }
            }
            return upgrade;
        }

        public static bool preUpgrade(UpgradeVo upgrade)
        {
            try
            {
                string dir = upgradeDir;
                FileHelper.mkdirAndDel(dir, true);
                FileHelper.copyDirectory(FileHelper.getCurrentDirectory(), dir, true);
                if (!Directory.Exists(dir))
                {
                    return false;
                }
                delFiles(dir, upgrade.Deletes);
                addFiles(dir, upgrade.Adds);
                return true;
            }
            catch (Exception e)
            {
                Logger.error("预升级", e);
                return false;
            }
        }

        public static bool isNeedUpdate(UpgradeVo upgrade)
        {
            if (upgrade != null && !version.Equals(upgrade.Version))
            {
                return version.CompareTo(upgrade.Version) < 0;
            }
            return false;
        }

        public static void doUpdate()
        {
            FileInfo fileInfo = new FileInfo(updatePath);
            if (!fileInfo.Exists)
            {
                Logger.info("更新程序缺失！updatePath: " + updatePath);
                MessageBox.Show("更新程序缺失！", "错误");
                return;
            }
            CmdHelper.Cmd(new string[] { "cd " + fileInfo.DirectoryName, updatePath });
        }

        private static void addFiles(string dir, List<UpgradeDetails> adds)
        {
            if (adds != null)
            {
                foreach (var item in adds)
                {
                    string systemDir = dir + (item.Dir ?? "");
                    if (item.ClearDir)
                    {
                        FileHelper.delDir(systemDir, true);
                    }
                    if (!StringHelper.isEmpty(item.DownloadUrl))
                    {
                        if (StringHelper.isEmpty(item.FileName))
                        {
                            int s = item.DownloadUrl.LastIndexOf("/");
                            item.FileName = item.DownloadUrl.Substring(s + 1);
                        }
                        string path = systemDir + "\\" + item.FileName;
                        bool suc = HttpHelper.downloadFile(item.DownloadUrl, path);
                        if (!suc)
                        {
                            Logger.error("下载升级文件失败！url = " + item.DownloadUrl);
                            throw new ServiceException("下载升级文件失败！");
                        }
                        if (item.Run)
                        {
                            CmdHelper.Cmd(new string[] { "cd " + systemDir, item.FileName });
                        }
                        if (item.ZipOutDir != null)
                        {
                            // 解压
                            ZipHelper.UnZipFile(path, dir + item.ZipOutDir);
                        }
                    }
                }
            }
        }

        private static void delFiles(string dir, List<UpgradeDetails> deletes)
        {
            if (deletes != null)
            {
                foreach (var item in deletes)
                {
                    string d = dir + item.Dir;
                    if (item.ClearDir)
                    {
                        FileHelper.delDir(d, true);
                    }
                    if (!StringHelper.isEmpty(item.FileName))
                    {
                        string path = d + "\\" + item.FileName;
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
            }
        }
    }
}
