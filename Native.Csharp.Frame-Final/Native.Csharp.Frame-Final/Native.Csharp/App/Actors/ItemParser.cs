using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public string dataDir = @"./Data/";
        Dictionary<string, Link> link1 = new Dictionary<string, Link>();
        Dictionary<string, Link> link2 = new Dictionary<string, Link>();
        Dictionary<string, Area> areas = new Dictionary<string, Area>();

        Dictionary<string, string> wordReplace=new Dictionary<string, string>();

        public ItemParser()
        {
            

        }

        public void init(string replacefile)
        {
            try
            {
                wordReplace = new Dictionary<string, string>();
                var wlist = File.ReadAllLines(replacefile, Encoding.UTF8);
                foreach (var line in wlist)
                {
                    var items = line.TrimEnd().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        wordReplace[items[1]] = items[0];
                    }
                    //wordReplace[]
                }
            }
            catch { }

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

        public static string[] splitSentence(string str)
        {
            string[] splits = { " ", "\t", "\n", "\r", ",", ".", "?", " ", "!", ";", ":", "，", "。", "”", "“", "‘", "’", "：", "；", "？", "！", "、", "（", "）", "(", ")", "\"", "'", "—", "《", "》", "【", "】", "…" };
            return str.Split(splits, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
