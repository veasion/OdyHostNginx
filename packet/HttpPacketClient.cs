using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        public static int listenPort = 8888;
        public static string name = "OdyHtppPacketClient";

        private HashSet<string> whiteList;
        private HttpPacketHandler handler;

        public delegate void HttpPacketHandler(HttpPacketInfo info);

        public void Start(HttpPacketHandler handler, HashSet<string> whiteList)
        {
            this.handler = handler;
            this.whiteList = whiteList;
            if (IsStarted())
            {
                Shutdown();
            }

            CONFIG.IgnoreServerCertErrors = true;
            FiddlerApplication.SetAppDisplayName(name);
            FiddlerApplication.Startup(listenPort, true, false, true);
            // FiddlerApplication.Startup(listenPort, true, true, true);

            FiddlerApplication.BeforeRequest += BeforeRequest;
            FiddlerApplication.BeforeResponse += BeforeResponse;
            FiddlerApplication.AfterSessionComplete += AfterSessionComplete;
        }

        private void AfterSessionComplete(Session session)
        {
            if (session.RequestMethod == "CONNECT")
            {
                return;
            }
            Console.WriteLine("req => " + session.fullUrl);
            if (whiteList != null && !whiteList.Contains(session.hostname))
            {
                return;
            }
            Encoding encoding;
            HttpPacketInfo info = new HttpPacketInfo();
            info.Id = session.id;
            info.Uri = session.PathAndQuery;
            info.Pid = session.LocalProcessID;
            info.ClientIp = session.clientIP;
            info.FullUrl = session.fullUrl;
            info.Hostname = session.hostname;
            session.utilDecodeRequest();
            info.ReqMethod = session.RequestMethod;
            encoding = session.GetRequestBodyEncoding();
            if (encoding != null)
            {
                info.ReqBody = encoding.GetString(session.RequestBody);
            }
            else
            {
                info.ReqBody = session.GetRequestBodyAsString();
            }
            info.ReqHeaders = header(session.RequestHeaders.ToArray());
            session.utilDecodeResponse();
            info.ResponseCode = session.responseCode;
            encoding = session.GetResponseBodyEncoding();
            if (encoding != null)
            {
                info.Response = encoding.GetString(session.ResponseBody);
            }
            else
            {
                info.Response = session.GetResponseBodyAsString();
            }
            info.RespHeaders = header(session.ResponseHeaders.ToArray());

            info.ReqCookies = cookies(info.ReqHeaders);
            info.RespCookies = cookies(info.RespHeaders);

            new HttpPacketHandler(handler)(info);

        }

        private Dictionary<string, string> header(HTTPHeaderItem[] array)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var item in array)
            {
                headers[item.Name] = item.Value;
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
