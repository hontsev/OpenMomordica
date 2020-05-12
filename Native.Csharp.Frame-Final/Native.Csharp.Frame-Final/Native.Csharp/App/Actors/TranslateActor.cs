using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 谷歌翻译模块
    /// </summary>
    class TranslateActor
    {
        string countryListName = "googletlist.txt";
        Dictionary<string, string> ctlist = new Dictionary<string, string>();


        public const string TranslateURL = "https://translate.google.cn/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8&sl=auto&tl=zh-CN";

        private Regex regex = new Regex("(?<=\\[\\\").*?(?=\\\")");
        //替换掉翻译结果中的id
        private Regex rreplaceid = new Regex("\\[\\[\\[\\\"[0-9a-z]+\\\"\\,\\\"\\\"\\]");

        public TranslateActor()
        {

        }

        public void init(string path)
        {
            try
            {
                ctlist = new Dictionary<string, string>();
                var lines = FileIOActor.readLines(path + countryListName);
                foreach (var line in lines)
                {
                    string[] vitem = line.Split('\t');
                    if (vitem.Length >= 2) ctlist[vitem[0]] = vitem[1];
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

        string getTranslateUrl(string from, string to)
        {
            try
            {
                string fn = "auto", tn = "zh-CN";
                if (ctlist.ContainsKey(from)) fn = ctlist[from];
                else if (ctlist.ContainsKey(from + "文")) fn = ctlist[from + "文"];
                else if (ctlist.ContainsKey(from + "语")) fn = ctlist[from + "语"];

                if (ctlist.ContainsKey(to)) tn = ctlist[to];
                else if (ctlist.ContainsKey(to + "文")) tn = ctlist[to + "文"];
                else if (ctlist.ContainsKey(to + "语")) tn = ctlist[to + "语"];

                return $"https://translate.google.cn/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8&sl={fn}&tl={tn}";
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
                return "";
            }

        }


        /// <summary>
        /// 获取翻译结果
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public string Translation(string src, string to="简体中文", string from="自动")
        {
            InitTkk();
            string tk = GetTK(src);
            string url = $"{getTranslateUrl(from, to)}&tk={tk}&q={UrlEncode(src)}";
            
            //return url;
            string httpresult = WebConnectActor.getData(url, Encoding.UTF8,"",true);
            FileIOActor.log(httpresult);
            string res = "";
            try
            {
                
                JArray jo = (JArray)JsonConvert.DeserializeObject(httpresult);
                //JObject jo = JObject.Parse(httpresult);
                //FileIOActor.log(jo[0].ToString());
                int resnum = jo[0].Count();
                if (resnum >= 1)
                {
                    foreach(var item in jo[0])
                    {
                        //FileIOActor.log(item.ToString());
                        //FileIOActor.log(item[0].ToString());
                        res += item[0].ToString() + Environment.NewLine;
                    }
                }
                return res.Trim();
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
                FileIOActor.log($"url:{url}\r\nres:{res}");
            }
            

            ////正则获取结果集
            //int begin = httpresult.IndexOf("[[[\"") + 4;
            //int end = httpresult.IndexOf("\",\"");
            //try
            //{
            //    httpresult = httpresult.Substring(begin, end - begin);
            //}
            //catch { }
            return httpresult;
        }



        /// <summary>
        /// URL编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString().Replace("%d%a", ""));
        }


        string tkk;
        void InitTkk()
        {
            string tkkHtml = string.Empty;
            string url = "https://translate.google.cn";
            Hashtable headers = new Hashtable();

#if UNITY_ANDROID
                headers.Add("User-Agent", "Mozilla/5.0 (Linux; U; Android 2.2.1; zh-cn; HTC_Wildfire_A3333 Build/FRG83D) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1");//android
#else
            headers.Add("User-Agent", "Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_3_3 like Mac OS X; en-us) AppleWebKit/533.17.9 (KHTML, like Gecko) Version/5.0.2 Mobile/8J2 Safari/6533.18.5");//ios 哈希表的数据格式
#endif
            try
            {
                tkkHtml = WebConnectActor.getData(url,Encoding.UTF8, "", true);
                tkk = GetTkk(tkkHtml);
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }
        private string GetTkk(string tkkHtml)
        {
            string tempStr = "";
            try
            {
                Regex reg = new Regex(@"tkk:'(?<key>.*?)',");
                var match = reg.Match(tkkHtml);
                tempStr = match.Groups["key"].Value;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return tempStr;
        }

        string GetTK(string a)
        {
            string[] e = tkk.Split('.');
            int d = 0;
            int h = 0;
            int[] g = new int[a.Length * 3];
            h = Number(e[0]);
            for (int f = 0; f < a.Length; f++)
            {
                int c = charCodeAt(a, f);// a.charCodeAt(f);
                if (128 > c)
                {
                    g[d++] = c;
                }
                else
                {
                    if (2048 > c)
                    {
                        g[d++] = c >> 6 | 192;
                    }
                    else
                    {
                        if (55296 == (c & 64512) && f + 1 < a.Length && 56320 == (charCodeAt(a, f + 1) & 64512))
                        {
                            c = 65536 + ((c & 1023) << 10) + charCodeAt(a, ++f) & 1023;
                            g[d++] = c >> 18 | 240;
                            g[d++] = c >> 12 & 63 | 128;
                        }
                        else
                        {
                            g[d++] = c >> 12 | 224;
                            g[d++] = c >> 6 & 63 | 128;

                        }
                    }
                    g[d++] = c & 63 | 128;
                }
            }

            List<int> g1 = new List<int>();
            foreach (int x in g)
            {
                if (x != 0)
                    g1.Add(x);
            }
            int[] g0 = g1.ToArray();

            long aa = h;
            for (d = 0; d < g0.Length; d++)
            {
                aa += g0[d];
                aa = Convert.ToInt64(b(aa, "+-a^+6"));
            }
            aa = Convert.ToInt64(b(aa, "+-3^+b+-f"));
            long bb = aa ^ Number(e[1]);
            aa = bb;
            aa = aa + bb;
            bb = aa - bb;
            aa = aa - bb;
            if (0 > aa)
            {
                aa = (aa & 2147483647) + 2147483648;
            }
            aa %= (long)1e6;
            return aa.ToString() + "." + (aa ^ h);
        }

        string b(long a, string b)
        {
            for (int d = 0; d < b.Length - 2; d += 3)
            {
                char c = charAt(b, d + 2);
                int c0 = 'a' <= c ? charCodeAt(c, 0) - 87 : Number(c);
                long c1 = '+' == charAt(b, d + 1) ? a >> c0 : a << c0;
                a = '+' == charAt(b, d) ? a + c1 & 4294967295 : a ^ c1;
            }
            a = Number(a);
            return a.ToString();
        }
        //实现js的charAt方法
        public static char charAt(object obj, int index)
        {
            char[] chars = obj.ToString().ToCharArray();
            return chars[index];
        }
        //实现js的charCodeAt方法
        public static int charCodeAt(object obj, int index)
        {
            char[] chars = obj.ToString().ToCharArray();
            return (int)chars[index];
        }

        //实现js的Number方法
        public static int Number(object cc)
        {
            try
            {
                long a = Convert.ToInt64(cc.ToString());
                int b = a > 2147483647 ? (int)(a - 4294967296) : a < -2147483647 ? (int)(a + 4294967296) : (int)a;
                return b;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
                return 0;
            }
        }
    }
}
