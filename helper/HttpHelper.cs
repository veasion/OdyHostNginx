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
        /// get请求
        /// </summary>
        public static string get(string url)
        {
            return get(url, FileHelper.UTF_8);
        }

        public static string get(string url, Encoding encoding)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
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
