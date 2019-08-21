using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 文件操作
    /// </summary>
    class FileIOActor
    {
        static string logFile = "log.txt";
        public static void log(string content)
        {
            try
            {
                File.AppendAllText(logFile, $"[{DateTime.Now.ToString("yyyyMMdd HH:mm:ss")}]{content}\r\n", Encoding.UTF8);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 读取单个txt文件内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string readTxtFile(string fileName, Encoding encoding)
        {
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    using (StreamReader reader = new StreamReader(file, encoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch(Exception e)
            {
                log(e.Message + "\r\n" + e.StackTrace);
                return null;
            }

        }

        /// <summary>
        /// 读取单个txt文件内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string readTxtFile(string fileName)
        {
            return readTxtFile(fileName, Encoding.UTF8);
        }

        /// <summary>
        /// 读取txt文件列表
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ICollection<string> readLines(string fileName)
        {
            return readLines(fileName, Encoding.UTF8);
        }

        public static ICollection<string> readLines(string fileName, Encoding encoding)
        {
            List<string> res = new List<string>();
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    using (StreamReader r = new StreamReader(file, encoding))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if(!string.IsNullOrWhiteSpace(line))
                            {
                                res.Add(line);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log(e.Message + "\r\n" + e.StackTrace);
            }
            return res;
        }

        public static void writeLines(string fileName, ICollection<string> lines)
        {
            try
            {
                File.WriteAllLines(fileName, lines);
            }
            catch (Exception e)
            {
                log(e.Message + "\r\n" + e.StackTrace);
            }
        }
        
        public static void clearFile(string fileName)
        {
            try
            {
                File.WriteAllText(fileName, "");
            }
            catch (Exception e)
            {
                log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public static void appendLine(string fileName, string line)
        {
            try
            {
                File.AppendAllText(fileName, line + "\r\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                log(e.Message + "\r\n" + e.StackTrace);
            }
        }

    }
}
