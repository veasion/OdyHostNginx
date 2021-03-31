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
                request.Method = "POST";
                request.ContentType = contentType;
                request.ContentLength = buf.Length;
                request.Headers = new WebHeaderCollection();
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

    }
}
