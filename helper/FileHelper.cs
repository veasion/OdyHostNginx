using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace OdyHostNginx
{
    public delegate bool LineHandle(int line, string text);

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
                return true;
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
                    bool r = lineHandle(++count, line);
                    if (!r) break;
                }
            }
            catch (Exception e)
            {
                Logger.error("读取文件", e);
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
            Logger.info("正在写文件: " + path);
            File.WriteAllText(path, context, encoding);
            return true;
        }

        public static T readJsonFile<T>(string path)
        {
            string json = readTextFile(path);
            if (!StringHelper.isEmpty(json))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception e)
                {
                    Logger.error("读取json文件", e);
                }
            }
            return default(T);
        }

        public static bool writeJsonFile(object obj, string path)
        {
            if (obj == null)
            {
                return false;
            }
            mkdir(new FileInfo(path).DirectoryName);
            string json = JsonConvert.SerializeObject(obj);
            return writeFile(path, json);
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
                Logger.error("追加文件", e);
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

        public static void copyFiles(string[] sourceFilePath, string targetDir, bool replace)
        {
            mkdir(targetDir);
            if (sourceFilePath != null && sourceFilePath.Length > 0)
            {
                foreach (var filePath in sourceFilePath)
                {
                    FileInfo file = new FileInfo(filePath);
                    if (file.Exists)
                    {
                        file.CopyTo(targetDir + "\\" + file.Name, replace);
                    }
                }
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
                    Directory.Delete(dirPath);
                }
                else
                {
                    Directory.Delete(dirPath);
                }
            }
        }

        public static void copyDirectory(string sourceDir, string targetDir, bool replace)
        {
            copyDirectory(sourceDir, targetDir, replace, false);
        }

        public static void copyDirectory(string sourceDir, string targetDir, bool replace, bool ignoreDeleted)
        {
            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                string[] files = Directory.GetFiles(sourceDir);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string pFilePath = targetDir + "\\" + fileName;
                    if (File.Exists(pFilePath))
                    {
                        if (replace)
                        {
                            File.Delete(pFilePath);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (ignoreDeleted && fileName.StartsWith(OdyConfigHelper.deleteStartsWith))
                    {
                        continue;
                    }
                    File.Copy(file, pFilePath, replace);
                }
                string[] dirs = Directory.GetDirectories(sourceDir);
                foreach (string dir in dirs)
                {
                    if (ignoreDeleted && dir.StartsWith(OdyConfigHelper.deleteStartsWith))
                    {
                        continue;
                    }
                    copyDirectory(dir, targetDir + "\\" + Path.GetFileName(dir), replace);
                }
            }
            catch (Exception e)
            {
                throw new ServiceException("复制文件发生错误", e);
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
