using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows;
using Fiddler;

namespace OdyHostNginx
{
    /// <summary>
    /// Http Packet Client
    /// </summary>
    class HttpPacketClient
    {

        public static bool ssl = false;
        public static int listenPort = 8888;
        public static string name = "OdyHttpPacketClient";

        private HashSet<string> whiteList;
        private HttpPacketHandler handler;
        private List<ModifyRequstBean> modifys;

        public delegate void HttpPacketHandler(HttpPacketInfo info);

        public void Start(HttpPacketHandler handler, List<ModifyRequstBean> modifys, bool https, HashSet<string> whiteList)
        {
            ssl = https;
            this.modifys = modifys;
            this.handler = handler;
            this.whiteList = whiteList;

            FiddlerApplication.SetAppDisplayName(name);
            if (IsStarted())
            {
                Shutdown();
            }
            if (ssl)
            {
                openSSL();
            }
            FiddlerApplication.Startup(listenPort, true, ssl, true);
            FiddlerApplication.BeforeRequest += BeforeRequest;
            FiddlerApplication.BeforeResponse += BeforeResponse;
            FiddlerApplication.AfterSessionComplete += AfterSessionComplete;
        }

        private void openSSL()
        {
            try
            {
                X509Certificate2 oRootCert = CertMaker.GetRootCertificate();
                X509Store certStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                certStore.Open(OpenFlags.ReadWrite);
                try
                {
                    certStore.Add(oRootCert);
                }
                finally
                {
                    certStore.Close();
                }
                FiddlerApplication.oDefaultClientCertificate = oRootCert;
            }
            catch (CryptographicException e)
            {
                MessageBox.Show("加载本地证书失败，请退出程序，右键以管理员身份运行！", "错误：" + e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show("证书ssl操作失败！", "错误：" + e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CONFIG.IgnoreServerCertErrors = true;
        }

        private void AfterSessionComplete(Session session)
        {
            if (session.RequestMethod == "CONNECT")
            {
                return;
            }
            try
            {
                Logger.info("抓包，req => " + session.fullUrl);
                if (whiteList != null && !whiteList.Contains(session.hostname))
                {
                    return;
                }
                // 处理请求&响应
                this.SessionHandler(session, false);
            }
            catch (Exception e)
            {
                Logger.error("解析抓包数据发生错误！", e);
            }
        }

        private void SessionHandler(Session session, bool isModify)
        {
            Encoding encoding;
            HttpPacketInfo info = new HttpPacketInfo
            {
                Id = session.id + "",
                IsModify = isModify,
                FullUrl = session.fullUrl,
                Uri = session.PathAndQuery,
                Hostname = session.hostname,
                ClientIp = session.clientIP,
                Pid = session.LocalProcessID,
                Status = session.responseCode,
                ReqMethod = session.RequestMethod
            };
            session.utilDecodeRequest();
            encoding = session.GetRequestBodyEncoding();
            info.ReqEncoding = encoding;
            if (encoding != null && session.RequestBody != null)
            {
                info.ReqBody = encoding.GetString(session.RequestBody);
            }
            else
            {
                info.ReqBody = session.GetRequestBodyAsString();
            }
            info.ReqHeaders = header(session.RequestHeaders);
            session.utilDecodeResponse();
            encoding = session.GetResponseBodyEncoding();
            info.RespEncoding = encoding;
            if (encoding != null && session.ResponseBody != null)
            {
                info.Response = encoding.GetString(session.ResponseBody);
            }
            else
            {
                info.Response = session.GetResponseBodyAsString();
            }
            info.RespHeaders = header(session.ResponseHeaders);

            info.ReqCookies = HttpHelper.cookies(info.ReqHeaders);
            info.RespCookies = HttpHelper.cookies(info.RespHeaders);

            // info.Id = session.Timers.ClientBeginRequest.Ticks + "-" + session.fullUrl.GetHashCode();

            new HttpPacketHandler(handler)(info);
        }

        private Dictionary<string, string> header(HTTPHeaders header)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (header != null)
            {
                foreach (var item in header.ToArray())
                {
                    headers[item.Name] = item.Value;
                }
            }
            return headers;
        }

        private void BeforeRequest(Session session)
        {
            if (!ConfigDialogData.modifyResponse || modifys == null)
            {
                return;
            }
            List<ModifyRequstBean> list = new List<ModifyRequstBean>(modifys);
            foreach (var item in list)
            {
                if (!item.matchFullUrl(session.fullUrl))
                {
                    continue;
                }
                if (item.InterceptType == 0)
                {
                    // 修改http请求
                    modifyRequest(session, item);
                }
                else if (item.InterceptType == 1)
                {
                    // 修改http响应
                    modifyResponse(session, item);
                }
                break;
            }
        }

        private void modifyRequest(Session session, ModifyRequstBean bean)
        {
            Logger.info("修改请求: " + session.fullUrl);
            if (!StringHelper.isBlank(bean.ParamsStr))
            {
                string url = session.fullUrl;
                int index = url.IndexOf("?");
                if (index > -1)
                {
                    url = url.Substring(0, index);
                }
                url = url + "?" + bean.ParamsStr;
                session.fullUrl = url;
                Logger.info("修改后的请求: " + session.fullUrl);
            }
            if (bean.Headers != null && bean.Headers.Count > 0)
            {
                session.oRequest.headers.RemoveAll();
                foreach (var key in bean.Headers.Keys)
                {
                    if ("Content-Length".Equals(key))
                    {
                        continue;
                    }
                    session.oRequest.headers.Add(key, bean.Headers[key]);
                }
            }
            if (!StringHelper.isBlank(bean.Body))
            {
                session.utilSetRequestBody(bean.Body);
            }
        }

        private void modifyResponse(Session session, ModifyRequstBean bean)
        {
            Logger.info("修改响应: " + session.fullUrl);
            session.bBufferResponse = true;
            session.utilCreateResponseAndBypassServer();
            if (bean.Headers != null)
            {
                foreach (var key in bean.Headers.Keys)
                {
                    session.oResponse.headers.Add(key, bean.Headers[key]);
                }
            }
            session.oResponse.headers.SetStatus(200, "Ok");
            session.utilSetResponseBody(bean.Body);
            // 处理请求&响应
            this.SessionHandler(session, true);
        }

        private void BeforeResponse(Session session)
        {
            // BeforeResponse
        }

        public static bool InstallCertificate()
        {
            // http://127.0.0.1:8888/FiddlerRoot.cer
            if (!CertMaker.rootCertExists())
            {
                if (!CertMaker.createRootCert())
                    return false;

                if (!CertMaker.trustRootCert())
                    return false;
            }
            return true;
        }

        public static bool UninstallCertificate()
        {
            if (CertMaker.rootCertExists())
            {
                if (!CertMaker.removeFiddlerGeneratedCerts(true))
                    return false;
            }
            return true;
        }

        public bool IsStarted()
        {
            return FiddlerApplication.IsStarted();
        }

        public void Shutdown()
        {
            FiddlerApplication.Shutdown();
            Thread.Sleep(200);
        }

    }
}
