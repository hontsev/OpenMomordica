using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    
    class ItemParser
    {
        public string replacefile = @"replacewords.txt";
        string path = "";

        Dictionary<string, string> wordReplace=new Dictionary<string, string>();

        public ItemParser()
        {

        }

        public void init(string path)
        {
            try
            {
                this.path = path;
                wordReplace = new Dictionary<string, string>();
                var lines = FileIOActor.readLines(path + replacefile);
                foreach(var line in lines)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        wordReplace[items[1]] = items[0];
                    }
                }
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }

        }

        /// <summary>
        /// 基于替换词典来替换文本中的关键词
        /// </summary>
        /// <param name="ori"></param>
        /// <returns></returns>
        public string replaceSymbolByDict(string ori)
        {
            string res = ori;
            foreach(var k in wordReplace.Keys)
            {
                res = res.Replace(k, wordReplace[k]);
            }
            return res;
        }

        /// <summary>
        /// 移除内部的各种换行、空格。
        /// </summary>
        /// <param name="ori"></param>
        /// <param name="ignoreNewline">true则保留换行符</param>
        /// <returns></returns>
        public static string removeBlank(string ori, bool ignoreNewline = false)
        {
            string blanks = " 　\t";
            if (!ignoreNewline) blanks += "\r\n";

            StringBuilder sb = new StringBuilder();

            foreach (var c in ori)
            {
                if (!blanks.Contains(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 去除字符串中的中英文标点和特殊字符
        /// </summary>
        /// <param name="ori"></param>
        /// <returns></returns>
        public static string removeSymbol(string ori)
        {
            StringBuilder sb = new StringBuilder();
            string sym = "，。、；：【】？“”‘’《》！￥…—{}[]()+=-/*!@#$%^&_|,.?:;/\\'\" \t";
            foreach(var c in ori)
            {
                if (!sym.Contains(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        public static string[] splitSentence(string str)
        {
            string[] splits = { " ", "\t", "\n", "\r", ",", ".", "?", " ", "!", ";", ":", "，", "。", "”", "“", "‘", "’", "：", "；", "？", "！", "、", "（", "）", "(", ")", "\"", "'", "—", "《", "》", "【", "】", "…" };
            return str.Split(splits, StringSplitOptions.RemoveEmptyEntries);
        }


        /// <summary>
        /// 去除HTML标记 
        /// </summary>
        /// <param name="strHtml">包括HTML的源码 </param>
        /// <returns>已经去除后的文字</returns>
        public static string StripHTML(string strHtml)
        {
            string[] aryReg = { @"<script[^>]*?>.*?</script>", @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>", @"([\r\n])[\s]+", @"&(quot|#34);", @"&(amp|#38);", @"&(lt|#60);", @"&(gt|#62);", @"&(nbsp|#160);", @"&(iexcl|#161);", @"&(cent|#162);", @"&(pound|#163);", @"&(copy|#169);", @"&#(\d+);", @"-->", @"<!--.*\n" };
            string[] aryRep = { "", "", "", "\"", "&", "<", ">", " ", "\xa1", "\xa2", "\xa3", "\xa9", "", "\r\n", "" };
            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            strOutput.Replace("<", ""); strOutput.Replace(">", "");
            strOutput.Replace("\r\n", ""); return strOutput;
        }




        /// <summary>
        /// 获取酷Q "At某人" 代码
        /// </summary>
        /// <param name="qqId">QQ号, 填写 -1 为At全体成员</param>
        /// <param name="addSpacing">默认为True, At后添加空格, 可使At更规范美观. 如果不需要添加空格, 请置本参数为False</param>
        /// <returns></returns>
        public static string CqCode_At(long qqId = -1, bool addSpacing = true)
        {
            return string.Format("[CQ:at,qq={0}]{1}", (qqId == -1) ? "all" : qqId.ToString(), addSpacing ? " " : string.Empty);
        }


        /// <summary>
        /// emoji转换成unicode编码
        /// </summary>
        /// <param name="emoji"></param>
        /// <returns></returns>
        public static int ConvertEmoji2Unicode(string emoji)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(emoji)) return 0;

                byte[] bytes = Encoding.UTF8.GetBytes(emoji);
                string firstItem = Convert.ToString(bytes[0], 2); //获取首字节二进制

                int iv;
                if (bytes.Length == 1)
                {
                    //单字节字符
                    iv = Convert.ToInt32(firstItem, 2);
                }
                else
                {
                    //多字节字符
                    StringBuilder sbBinary = new StringBuilder();
                    sbBinary.Append(firstItem.Substring(bytes.Length + 1).TrimStart('0'));
                    for (int i = 1; i < bytes.Length; i++)
                    {
                        string item = Convert.ToString(bytes[i], 2);
                        item = item.Substring(2);
                        sbBinary.Append(item);
                    }

                    iv = Convert.ToInt32(sbBinary.ToString(), 2);
                }
                return iv;
                //return Convert.ToString(iv, 10).PadLeft(4, '0');
            }
            catch
            {
                return 0;
            }

        }

        /// <summary>
        /// emoji的unicode编码值转换成utf8格式字符串emoji
        /// </summary>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string ConvertUnicode2Emoji(int iv)
        {
            try
            {
                string res = "";
                string s = Convert.ToString(iv, 2);
                if (iv <= 0x7F)
                {
                    // 1
                    s = s.PadLeft(7, '0');
                    res = Encoding.UTF8.GetString(new byte[] {
                    Convert.ToByte(s,2)
                });
                }
                else if (iv <= 0x07FF)
                {
                    // 2
                    s = s.PadLeft(11, '0');
                    res = Encoding.UTF8.GetString(new byte[]
                    {Convert.ToByte("110"+s.Substring(0,5),2),
                    Convert.ToByte("10"+s.Substring(5),2),


                    });
                }
                else if (iv <= 0xffff)
                {
                    // 3
                    s = s.PadLeft(16, '0');
                    res = Encoding.UTF8.GetString(new byte[]
                    {

                    Convert.ToByte("1110"+s.Substring(0,4),2),
                     Convert.ToByte("10"+s.Substring(4,6),2),
                     Convert.ToByte("10"+s.Substring(10),2),
                    });
                }
                else if (iv <= 0x1fffff)
                {
                    // 4
                    s = s.PadLeft(21, '0');
                    res = Encoding.UTF8.GetString(new byte[]
                    {
                    Convert.ToByte("11110"+s.Substring(0,3),2),
                     Convert.ToByte("10"+s.Substring(3,6),2),
                      Convert.ToByte("10"+s.Substring(9,6),2),
                      Convert.ToByte("10"+s.Substring(15),2),
                    });
                }
                else if (iv <= 0x3ffffff)
                {
                    // 5
                    s = s.PadLeft(26, '0');
                    res = Encoding.UTF8.GetString(new byte[]
                    {
                    Convert.ToByte("111110"+s.Substring(0,2),2),
                     Convert.ToByte("10"+s.Substring(2,6),2),
                     Convert.ToByte("10"+s.Substring(8,6),2),
                     Convert.ToByte("10"+s.Substring(14,6),2),
                     Convert.ToByte("10"+s.Substring(20),2),
                    });
                }
                else
                {
                    // 6
                    s = s.PadLeft(31, '0');
                    res = Encoding.UTF8.GetString(new byte[]
                    {
                    Convert.ToByte("1111110"+s.Substring(0,1),2),
                     Convert.ToByte("10"+s.Substring(1,6),2),
                     Convert.ToByte("10"+s.Substring(7,6),2),
                      Convert.ToByte("10"+s.Substring(13,6),2),
                     Convert.ToByte("10"+s.Substring(19,6),2),
                     Convert.ToByte("10"+s.Substring(25),2),
                    });
                }
                return res;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 从输入文本中删掉CQ码特有的表情、emoji等奇怪格式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string replaceCoolQEmojis(string str)
        {
            string res = str;
            try
            {
                // emoji
                Regex reg = new Regex(@"\[CQ\:emoji,id=(\d+)\]", RegexOptions.IgnoreCase);
                var regres = reg.Matches(res);
                for (int i = 0; i < regres.Count; i++)
                {
                    try
                    {
                        Match m = regres[i];
                        string matchori = m.Groups[0].ToString();
                        int emojicode = int.Parse(m.Groups[1].ToString());
                        string emoji = ConvertUnicode2Emoji(emojicode);
                        res = res.Replace(matchori, emoji);
                    }
                    catch { }
                }

                // bface
                reg = new Regex(@"\[CQ\:bface.*?\]", RegexOptions.IgnoreCase);
                regres = reg.Matches(res);
                for (int i = 0; i < regres.Count; i++)
                {
                    try
                    {
                        Match m = regres[i];
                        string matchori = m.Groups[0].ToString();
                        res = res.Replace(matchori, "");
                    }
                    catch { }
                }

                // face
                reg = new Regex(@"\[CQ\:face.*?\]", RegexOptions.IgnoreCase);
                regres = reg.Matches(res);
                for (int i = 0; i < regres.Count; i++)
                {
                    try
                    {
                        Match m = regres[i];
                        string matchori = m.Groups[0].ToString();
                        res = res.Replace(matchori, "");
                    }
                    catch { }
                }

                // sface
                reg = new Regex(@"\[CQ\:sface.*?\]", RegexOptions.IgnoreCase);
                regres = reg.Matches(res);
                for (int i = 0; i < regres.Count; i++)
                {
                    try
                    {
                        Match m = regres[i];
                        string matchori = m.Groups[0].ToString();
                        res = res.Replace(matchori, "");
                    }
                    catch { }
                }

                // rps
                reg = new Regex(@"\[CQ\:rps.*?\]", RegexOptions.IgnoreCase);
                regres = reg.Matches(res);
                for (int i = 0; i < regres.Count; i++)
                {
                    try
                    {
                        Match m = regres[i];
                        string matchori = m.Groups[0].ToString();
                        res = res.Replace(matchori, "");
                    }
                    catch { }
                }

                // dice
                reg = new Regex(@"\[CQ\:dice.*?\]", RegexOptions.IgnoreCase);
                regres = reg.Matches(res);
                for (int i = 0; i < regres.Count; i++)
                {
                    try
                    {
                        Match m = regres[i];
                        string matchori = m.Groups[0].ToString();
                        res = res.Replace(matchori, "");
                    }
                    catch { }
                }
            }
            catch
            {
               
            }

            return res;
        }

    }
}
