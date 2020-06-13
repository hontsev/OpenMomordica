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

        /// <summary>
        /// 将异常详情写入日志文件
        /// </summary>
        /// <param name="ex"></param>
        public static void log(Exception ex)
        {
            try
            {
                log($"{ex.Message}\r\n{ex.StackTrace}");
            }
            catch { }
        }

        /// <summary>
        /// 写入日志文件
        /// </summary>
        /// <param name="content"></param>
        public static void log(string content)
        {
            try
            {
                File.AppendAllText(logFile, $"[{DateTime.Now.ToString("yyyyMMdd HH:mm:ss")}]{content}\r\n", Encoding.UTF8);
            }
            catch { }
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
            catch(Exception ex)
            {
                log(ex);
            }
            return "";
        }

        /// <summary>
        /// 读取单个txt文件内容，默认utf-8编码
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string readTxtFile(string fileName)
        {
            try
            {
                return readTxtFile(fileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                log(ex);
            }
            return "";
        }

        /// <summary>
        /// 读取txt文件中的行字符串，默认utf-8编码
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ICollection<string> readLines(string fileName)
        {
            try
            {
                return readLines(fileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                log(ex);
            }
            return null;
        }


        /// <summary>
        /// 读取所有行
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
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
            catch (Exception ex)
            {
                log(ex);
            }
            return res;
        }

        /// <summary>
        /// 用utf-8编码覆盖写入
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lines"></param>
        public static void writeLines(string fileName, ICollection<string> lines)
        {
            try
            {
                File.WriteAllLines(fileName, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        /// <summary>
        /// 用utf-8编码覆盖写入
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public static void write(string fileName, string data)
        {
            try
            {
                File.WriteAllText(fileName, data, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        public static void clearFile(string fileName)
        {
            try
            {
                write(fileName, "");
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }

        public static void appendLine(string fileName, string line)
        {
            try
            {
                File.AppendAllText(fileName, line + "\r\n", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }







        


    }
}
