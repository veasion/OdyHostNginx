using System;
using System.IO;
using System.Text;

namespace OdyHostNginx
{
    public delegate void LineHandle(int line, string text);

    /// <summary>
    /// 文件工具类
    /// </summary>
    class FileHelper
    {
        public static string readTextFile(string path)
        {
            return readTextFile(path, Encoding.UTF8);
        }

        public static string readTextFile(string path, Encoding encoding)
        {
            StringBuilder sb = new StringBuilder();
            readTextFile(path, encoding, (line, text) =>
            {
                sb.AppendLine(text);
            });
            return sb.ToString();
        }

        public static void readTextFile(string path, LineHandle lineHandle)
        {
            readTextFile(path, Encoding.UTF8, lineHandle);
        }

        public static void readTextFile(string path, Encoding encoding, LineHandle lineHandle)
        {
            StreamReader sr = null;
            try
            {
                String line;
                int count = 0;
                sr = new StreamReader(path, encoding);
                while ((line = sr.ReadLine()) != null)
                {
                    lineHandle(++count, line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }

        public static bool writeFile(string path, string context)
        {
            return writeFile(path, Encoding.UTF8, context);
        }

        public static bool writeFile(string path, Encoding encoding, string context)
        {
            File.WriteAllText(path, context, encoding);
            return true;
        }

        public static bool appendFile(string path, string line)
        {
            return appendFile(path, Encoding.UTF8, line);
        }

        public static bool appendFile(string path, Encoding encoding, string line)
        {
            FileStream fs = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs, encoding);
            bool success = false;
            try
            {
                sw.WriteLine(line);
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return success;
        }

        public static void mkdir(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public static void mkdirAndDel(string dirPath, bool del)
        {
            if (del)
            {
                delDir(dirPath, true);
                Directory.CreateDirectory(dirPath);
            }
            else
            {
                mkdir(dirPath);
            }
        }

        public static void delDir(string dirPath, bool recursive)
        {
            if (Directory.Exists(dirPath))
            {
                if (recursive)
                {
                    foreach (string path in Directory.GetFileSystemEntries(dirPath))
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                        else
                        {
                            delDir(path, true);
                        }
                    }
                }
                else
                {
                    Directory.Delete(dirPath);
                }
            }
        }

        public static string getCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public static string getDesktopDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

    }
}
