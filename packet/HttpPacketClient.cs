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
        public static string name = "OdyHtppPacketClient";

        private HashSet<string> whiteList;
        private HttpPacketHandler handler;

        public delegate void HttpPacketHandler(HttpPacketInfo info);

        public void Start(HttpPacketHandler handler, bool https, HashSet<string> whiteList)
        {
            ssl = https;
            this.handler = handler;
            this.whiteList = whiteList;
            if (IsStarted())
            {
                Shutdown();
            }

            FiddlerApplication.SetAppDisplayName(name);
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
                Encoding encoding;
                HttpPacketInfo info = new HttpPacketInfo
                {
                    Id = session.id + "",
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
                if (encoding != null && session.ResponseBody != null)
                {
                    info.Response = encoding.GetString(session.ResponseBody);
                }
                else
                {
                    info.Response = session.GetResponseBodyAsString();
                }
                info.RespHeaders = header(session.ResponseHeaders);

                info.ReqCookies = cookies(info.ReqHeaders);
                info.RespCookies = cookies(info.RespHeaders);

                // info.Id = session.Timers.ClientBeginRequest.Ticks + "-" + session.fullUrl.GetHashCode();

                new HttpPacketHandler(handler)(info);
            }
            catch (Exception e)
            {
                Logger.error("解析抓包数据发生错误！", e);
            }
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

        private Dictionary<string, List<string>> cookies(Dictionary<string, string> headers)
        {
            Dictionary<string, List<string>> cookies = new Dictionary<string, List<string>>();
            if (headers.TryGetValue("Cookie", out string cookie))
            {
                string[] cookieArr = cookie.Split(';');
                if (cookieArr != null && cookieArr.Length > 0)
                {
                    foreach (var item in cookieArr)
                    {
                        if (item.IndexOf("=") > 0)
                        {
                            string[] kv = item.Split('=');
                            string key = kv[0].Trim(), val = kv[1].Trim();
                            if (cookies.TryGetValue(key, out List<string> value))
                            {
                                value.Add(val);
                                cookies[key] = value;
                            }
                            else
                            {
                                value = new List<string>
                                {
                                    val
                                };
                                cookies[key] = value;
                            }
                        }
                    }
                }
            }
            return cookies;
        }

        private void BeforeRequest(Session session)
        {
            /*
            session.bBufferResponse = true;
            session.utilCreateResponseAndBypassServer();  
            session.oResponse.headers.SetStatus(200, "Ok");  
            string str = session.GetResponseBodyAsString();
            session.utilSetResponseBody("修改后的：" + str);
            */
        }

        private void BeforeResponse(Session session)
        {
            // 修改任何http响应
            // session.bBufferResponse = true;
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
