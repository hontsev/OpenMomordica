using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// *未完工*
    /// 文本内容分析
    /// </summary>
    public class Link
    {
        public string id1;
        public string id2;
        public DateTime date;
        public string author;
        public string type;
        public Link(string id1, string id2, string type, string date, string author)
        {
            this.id1 = id1;
            this.id2 = id2;
            this.type = type;
            this.author = author.Replace("-", " ");
            string strdate = date;
            string format = "yyyyMMddHHmmss";
            DateTime.TryParseExact(strdate, format,
                               System.Globalization.CultureInfo.InvariantCulture,
                               System.Globalization.DateTimeStyles.None, out this.date);

        }

    }

    public class Area
    {
        public string id;

        public Area()
        {

        }
    }


    class ItemParser
    {
        Random rand;
        public string replacefile = @"replacewords.txt";
        public string repfile = @"replaces.txt";
        public string expfile = @"exps.txt";
        string path = "";
        Dictionary<string, string[]> replaces = new Dictionary<string, string[]>();
        Dictionary<string, string> replacesC = new Dictionary<string, string>();
        List<string[]> exps = new List<string[]>(); 

        Dictionary<string, Link> link1 = new Dictionary<string, Link>();
        Dictionary<string, Link> link2 = new Dictionary<string, Link>();
        Dictionary<string, Area> areas = new Dictionary<string, Area>();

        Dictionary<string, string> wordReplace=new Dictionary<string, string>();

        public ItemParser()
        {

            rand = new Random();
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

                var lines1 = FileIOActor.readLines(path + repfile);
                foreach (var line in lines1)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        var r1 = items[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        replaces[items[0]] = r1;
                        foreach(var sr in r1) replacesC[sr] = items[0];
                    }
                }

                var lines2 = FileIOActor.readLines(path + expfile);
                foreach (var line in lines2)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    exps.Add(items);
                }
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }

        }

        public void save()
        {
            try
            {
                List<string> res = new List<string>();
                StringBuilder sb = new StringBuilder();
                foreach(var exp in exps)
                {
                    sb.Append($"{string.Join("\t", exp)}\r\n");
                }
                File.WriteAllText(path + expfile, sb.ToString(), Encoding.UTF8);

                sb = new StringBuilder();
                foreach (var rep in replaces)
                {
                    sb.Append($"{rep.Key}\t{string.Join(",",rep.Value)}\r\n");
                }
                File.WriteAllText(path + repfile, sb.ToString(), Encoding.UTF8);

            }
            catch
            {

            }
        }

        public string getResult(string input)
        {
            string res = "";
            foreach(var exp in exps)
            {
                try
                {
                    string ine = exp[0];
                    string oute = exp[1];

                    int maxtime = 20;
                    int time = 0;
                    bool find = false;
                    List<string> initems = new List<string>();
                    while (ine.Contains('【'))
                    {
                        time++;
                        if (time > maxtime) break;
                        int begin = ine.IndexOf('【');
                        int end = ine.IndexOf('】');
                        string k = ine.Substring(begin + 1, end - begin - 1);
                        if (replaces.ContainsKey(k))
                        {
                            string[] target = replaces[k];
                            foreach(var tar in target)
                            {
                                string t = ine.Replace($"【{k}】", tar);
                                if (input == t)
                                {
                                    find = true;
                                    initems.Add(tar);
                                    ine = t;
                                    //break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (input == ine)
                    {
                        find = true;
                    }
                    time = 0;
                    if (find)
                    {
                        while (oute.Contains('【'))
                        {
                            time++;
                            if (time > maxtime) break;
                            int begin = ine.IndexOf('【');
                            int end = ine.IndexOf('】');
                            string k = ine.Substring(begin + 1, end - begin - 1);
                            if (replaces.ContainsKey(k))
                            {
                                string[] tartmp = replaces[k];
                                oute=oute.Replace($"【{k}】", tartmp[rand.Next(tartmp.Length)]);
                            }
                            else
                            {
                                int tryint = -1;
                                int.TryParse(k, out tryint);
                                if (tryint > 0)
                                {
                                    //tryint -= 1;
                                    try
                                    {
                                        oute = oute.Replace($"【{k}】", initems[tryint - 1]);
                                    }
                                    catch
                                    {

                                    }   
                                }
                            }
                        }
                        res = oute;
                    }
                }
                catch
                {

                }
            }

            return res;
        }


        public static string DealInput(string input)
        {
            input = input.Trim();
            string[] options;

            return input;
        }

        public string removeUnText(string ori)
        {
            string res = ori;
            foreach(var k in wordReplace.Keys)
            {
                res = res.Replace(k, wordReplace[k]);
            }
            return res;
        }

        public static string removeBlank(string ori, bool ignoreNewline = false)
        {
            string blanks = " \t";
            if (!ignoreNewline) blanks += "\r\n";

            StringBuilder sb = new StringBuilder();

            foreach (var c in ori)
            {
                if (!blanks.Contains(c)) sb.Append(c);
            }
            return sb.ToString();
        }

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
        /// 某些字段的和谐
        /// 输出前的必备步骤
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string getHexie(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            str = str.Replace("习近平", "🐻");
            str = str.Replace("江泽民", "🐸");
            str = str.Replace("毛泽东", "🐱");
            str = str.Replace("毛主席", "🐱");
            str = str.Replace("彭丽媛", "🐎🐎");
            str = str.Replace("法轮功", "⭕");
            str = str.Replace("共产党", "☭");
            str = str.Replace("共产主义", "☭");
            str = str.Replace("革命", "gm");
            return str;
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

    }
}
