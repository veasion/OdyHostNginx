using Newtonsoft.Json.Linq;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    public class SSHClientHelper
    {

        static JToken sshJson;
        static string jsonPath = "logssh.cf";
        static HashSet<SshClient> sshClients = new HashSet<SshClient>();

        private static JToken getSSHJson()
        {
            if (sshJson != null) return sshJson;
            if (!File.Exists(jsonPath)) return null;
            try
            {
                sshJson = JToken.Parse(FileHelper.readTextFile(jsonPath));
                return sshJson;
            }
            catch (Exception e)
            {
                Logger.error("加载logssh.cf文件", e);
                throw new Exception("logssh文件加载失败！" + e.Message);
            }
        }

        public static bool isOpenSshLog(bool reload)
        {
            if (reload)
            {
                sshJson = null;
            }
            JToken j = null;
            try
            {
                j = getSSHJson();
            }
            catch (Exception) { }
            return j != null && j["open"] != null && "true".Equals(j["open"].ToString().ToLower());
        }

        public static SSHConnInfo getConnInfo(NginxUpstream up)
        {
            JToken j = getSSHJson();
            JToken sshCf = j["ssh"];
            SSHConnInfo conn = new SSHConnInfo();
            conn.ContextPath = up.ContextPath;
            conn.ServerPort = up.OldPort;
            conn.Ip = up.OldIp;

            JToken cf = sshCf[conn.Ip] != null ? sshCf[conn.Ip] : sshCf["common"];
            conn.User = cf["user"].ToString();
            conn.Password = cf["password"].ToString();
            if (cf["port"] != null)
            {
                conn.Port = int.Parse(cf["port"].ToString());
            }
            return conn;
        }

        public static SshClient open(SSHConnInfo conn)
        {
            if (conn.Port <= 0)
            {
                conn.Port = 22;
            }
            SshClient ssh = new SshClient(conn.Ip, conn.Port, conn.User, conn.Password);
            try
            {
                Logger.info("正在连接ssh: " + conn.Ip + ":" + conn.Port);
                ssh.Connect();
                sshClients.Add(ssh);
            }
            catch (Exception e)
            {
                throw new Exception("连接失败，" + e.Message, e);
            }
            return ssh;
        }

        public static string tryGetCommand(SshClient ssh, SSHConnInfo conn)
        {
            JToken j = getSSHJson();
            JToken logComm = j["serverLogCommand"];
            string contextPath = conn.ContextPath;
            if (logComm != null && logComm[contextPath] != null)
            {
                return logComm[contextPath].ToString();
            }
            string logPath = null;
            string dir = execCommandFirstResult(ssh, "find /data/logdir/ -name '" + contextPath + "' -type d");

            if (dir == null && contextPath.IndexOf("-") > 0)
            {
                dir = execCommandFirstResult(ssh, "find /data/logdir/ -name '" + contextPath.Split('-')[0] + "' -type d");
            }
            if (dir != null)
            {
                logPath = execCommandFirstResult(ssh, "find " + dir + " -name info.log");
                if (logPath == null)
                {
                    logPath = execCommandFirstResult(ssh, "find " + dir + " -name *.log -mtime -1");
                }
            }
            if (logPath == null)
            {
                throw new Exception("日志文件查找失败");
            }
            return "tail -50f " + logPath;
        }

        private static string execCommandFirstResult(SshClient ssh, string command)
        {
            SshCommand ls = ssh.CreateCommand(command);
            string result = ls.Execute();
            string[] strs = result.Split('\n');
            if (strs != null && strs.Length > 0 && !StringHelper.isBlank(strs[0]))
            {
                return strs[0];
            }
            return null;
        }

        public static void close(SshClient sshClient)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                sshClient.Disconnect();
                sshClients.Remove(sshClient);
            }
        }

        public static void closeAll()
        {
            sshClients.ToList().ForEach(close);
        }
    }
}
