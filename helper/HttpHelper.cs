using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OdyHostNginx
{
    class HttpHelper
    {

        /// <summary>
        /// post请求
        /// </summary>
        public static string post(string url, string json)
        {
            return post(url, json, null);
        }

        public static string post(string url, string json, Dictionary<string, string> header)
        {
            return post(url, json, "application/json; charset=UTF-8", header, FileHelper.UTF_8);
        }

        public static string post(string url, string body, string contentType, Dictionary<string, string> header, Encoding encoding)
        {
            try
            {
                if (encoding == null)
                {
                    encoding = Encoding.GetEncoding("UTF-8");
                }
                byte[] buf = encoding.GetBytes(body);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers = new WebHeaderCollection();
                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = buf.Length;
                if (header != null)
                {
                    foreach (var k in header.Keys)
                    {
                        request.Headers.Add(k, header[k]);
                    }
                }
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(buf, 0, buf.Length);
                    reqStream.Close();
                }
                WebResponse response = request.GetResponse();
                Stream respStream = response.GetResponseStream();
                string responseStr = null;
                if (respStream != null)
                {
                    StreamReader reader = new StreamReader(respStream, encoding);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                    respStream.Close();
                }
                response.Close();
                return responseStr;
            }
            catch (Exception e)
            {
                Logger.error("Post请求", e);
                return null;
            }
        }

        /// <summary>
        /// get请求
        /// </summary>
        public static string get(string url)
        {
            return get(url, null, FileHelper.UTF_8);
        }

        public static string get(string url, Dictionary<string, string> header, Encoding encoding)
        {
            try
            {
                if (encoding == null)
                {
                    encoding = Encoding.GetEncoding("UTF-8");
                }
                WebRequest request = WebRequest.Create(url);
                request.Headers = new WebHeaderCollection();
                if (header != null)
                {
                    foreach (var k in header.Keys)
                    {
                        request.Headers.Add(k, header[k]);
                    }
                }
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                string responseStr = null;
                if (stream != null)
                {
                    StreamReader reader = new StreamReader(stream, encoding);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                    stream.Close();
                }
                response.Close();
                return responseStr;
            }
            catch (Exception e)
            {
                Logger.error("Get请求", e);
                return null;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public static bool downloadFile(string url, string path)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                FileHelper.mkdir(fileInfo.DirectoryName);
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                Stream stream = new FileStream(path, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, bArr.Length);
                while (size > 0)
                {
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, bArr.Length);
                }
                stream.Close();
                responseStream.Close();
                return true;
            }
            catch (Exception e)
            {
                Logger.error("下载文件", e);
            }
            return false;
        }

        public static Dictionary<string, List<string>> cookies(Dictionary<string, string> headers)
        {
            string cookie;
            Dictionary<string, List<string>> cookies = new Dictionary<string, List<string>>();
            if (headers.TryGetValue("Cookie", out cookie) || headers.TryGetValue("cookie", out cookie))
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

        public static bool ReplayXHR(HttpPacketInfo info)
        {
            try
            {
                HttpPacketInfo packetInfo = (HttpPacketInfo)info.Clone();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(packetInfo.FullUrl);
                request.Timeout = 8000;
                request.ContinueTimeout = 8000;
                request.ReadWriteTimeout = 8000;
                request.Headers = new WebHeaderCollection();
                request.Method = packetInfo.ReqMethod ?? "GET";
                request.ContentType = HttpPacketInfo.contentType(packetInfo.ReqHeaders);
                request.ServicePoint.Expect100Continue = false;

                byte[] body = null;
                if (!StringHelper.isEmpty(packetInfo.ReqBody))
                {
                    body = (packetInfo.ReqEncoding ?? Encoding.Default).GetBytes(packetInfo.ReqBody);
                    request.ContentLength = body.Length;
                }
                if (packetInfo.ReqHeaders != null)
                {
                    foreach (var k in packetInfo.ReqHeaders.Keys)
                    {
                        try
                        {
                            string v = packetInfo.ReqHeaders[k];
                            if ("Accept".Equals(k))
                            {
                                request.Accept = v;
                            }
                            else if ("Host".Equals(k))
                            {
                                request.Host = v;
                            }
                            else if ("User-Agent".Equals(k))
                            {
                                request.UserAgent = v;
                            }
                            else if ("Referer".Equals(k))
                            {
                                request.Referer = v;
                            }
                            else if ("Content-Type".Equals(k))
                            {
                                request.ContentType = v;
                            }
                            request.Headers.Add(k, v);
                        }
                        catch (Exception e)
                        {
                            Logger.error("ReplayXHR.Headers.Add", e);
                        }
                    }
                }
                if (body != null && body.Length > 0)
                {
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(body, 0, body.Length);
                    }
                }
                // 请求
                WebResponse response = request.GetResponse();
                Stream respStream = response.GetResponseStream();
                string responseStr = null;
                if (respStream != null)
                {
                    StreamReader reader = new StreamReader(respStream, packetInfo.RespEncoding ?? Encoding.GetEncoding("UTF-8"));
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                    respStream.Close();
                }
                response.Close();
                return true;
            }
            catch (Exception e)
            {
                Logger.error("ReplayXHR请求", e);
                return false;
            }
        }

        public static string getIp(string domain)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(domain);
                IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
                return ipEndPoint.Address.ToString();
            }
            catch (Exception e)
            {
                Logger.error("获取ip失败 domain=" + domain, e);
                return null;
            }
        }

    }
}
