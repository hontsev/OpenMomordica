using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 百度内容爬取模块
    /// </summary>
    class BaiduSearchActor
    {
        ItemParser parser = new ItemParser();
        public BaiduSearchActor()
        {
        }

        public void init(string replacefile)
        {
            parser.init(replacefile);
        }

        /// <summary>
        /// 从万恶的百度知道的答案中删掉那些万恶的图片格式的文字。嗯。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string replaceImageWords(string str)
        {
            //str = ItemParser.removeBlank(str);
            List<List<string>> baiduWordList = FileIOActor.readBaiduWords(Common.AppDirectory + "\\BaiduWord.txt");
            foreach (var match in baiduWordList)
            {
                str = str.Replace("<img class=\"word-replace\" src=\"https://zhidao.baidu.com/api/getdecpic?picenc=" + match[0] + "\">", match[1]);
            }
            return str;
        }

        public static string convertUnicodeChinese(string str)
        {
            string res = "";



            return res;
        }

        /// <summary>
        /// 暂时不可用
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public string getAsklibResult(string question)
        {
            string url = string.Format("http://www.asklib.com/s/{0}", WebConnectActor.UrlEncode(question));
            string res = "";
            //List<string> res = new List<string>();
            string html = WebConnectActor.getData(url, Encoding.UTF8);
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);
            try
            {
                HtmlNode favurl = null;
                try
                {
                    //res = html; return res;
                    favurl = hdoc.DocumentNode.SelectSingleNode("//div[@class=\"p15 right\"]").ChildNodes[1];
                    url = ItemParser.removeBlank(favurl.GetAttributeValue("href", ""), true);
                    url = "http://www.asklib.com/" + url;
                    html = WebConnectActor.getData(url, Encoding.UTF8);
                    hdoc = new HtmlDocument();
                    hdoc.LoadHtml(html);
                    var tmp = getText(hdoc.DocumentNode.SelectSingleNode("//div[@class=\"listtip\"]").InnerHtml);
                    StringBuilder sb = new StringBuilder();
                    foreach (var t in tmp) if (!string.IsNullOrWhiteSpace(t.Trim())) sb.Append(t + "\r\n");
                    sb.Replace("\r\n\r\n", "\r\n");
                    res = sb.ToString();
                }
                catch {  }                
            }
            catch { }

            return res;
        }


        /// <summary>
        /// 从百度知识图谱中寻找答案
        /// 基本就是把关键词放入百度搜索，然后看百度有没有智能返回的结果
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public string[] getBaiduKGResult(string words)
        {
            List<string> reslist = new List<string>();
            string askUrl = "http://www.baidu.com/s?wd=" + WebConnectActor.UrlEncode(words);
            string html = WebConnectActor.getData(askUrl, Encoding.UTF8);
            //var html1 = HttpUtility.UrlDecode(html);
            //var html2 = Regex.Unescape(html);
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            //统计数值
            try
            {
                string gdp = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_gdp_subtitle\"]").InnerText);
                reslist.Add(gdp);
            }
            catch { }

            //图谱常识
            try
            {
                string common = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_exactqa_s_answer\"]").InnerText);
                reslist.Add(common);
            }
            catch { }

            //股票
            //try
            //{
            //    string gp = ItemParser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_stockweakdemand_cur_num c-gap-right-small\"]").InnerText);
            //    reslist.Add(gp);
            //    string gpzf = ItemParser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_stockweakdemand_cur_info c-gap-icon-right-small\"]").InnerText);
            //    reslist.Add(gpzf);
            //}
            //catch { }
            try
            {
                string gp = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op-stockdynamic-moretab-cur-num c-gap-right-small\"]").InnerText);
                reslist.Add(gp);
                string gpzf = hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op-stockdynamic-moretab-cur-unit\"]").InnerText.Trim();
                gpzf = hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op-stockdynamic-moretab-cur\"]").InnerText.Trim();
                foreach (var s in new string[]{"美元","元","镑" }) if (gpzf.Contains(s)) { gpzf = s; break; }
                reslist.Add(gpzf);
            }
            catch { }

            //热线电话
            try
            {
                string rx = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_kefupoly_td2\"]").InnerText);
                reslist.Add(rx);
            }
            catch { }


            //翻译
            try
            {
                string trans = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_sp_fanyi_line_two\"]").InnerText);
                reslist.Add(trans);
            }
            catch { }

            //数学运算
            try
            {
                string trans = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_new_val_screen_result\"]").InnerText);
                //string trans = ItemParser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@style=\"font-size:1.4em;line-height:22px;padding-bottom:2px;width:474px;\"]").InnerText);
                reslist.Add(trans.Trim());
            }
            catch { }

            //汇率换算
            try
            {
                string hl = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_exrate_result\"]").InnerText);
                reslist.Add(hl);
            }
            catch { }

            //单位换算
            try
            {
                var res = Regex.Match(html, " tab\\:(.*?),\\\n        rank", RegexOptions.Singleline).Groups[1].Value.Trim();
                string num = Regex.Match(res, "numres\\\":\\\"(.*?)\\\",").Groups[1].Value.Trim();
                string dw = Regex.Match(res, "to_syn\\\":\\\"(.*?)\\\",").Groups[1].Value.Trim();
                dw = Regex.Unescape(dw);
                if (num.Length <= 0) throw new Exception("not answer");
                //string dw = ItemParser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op-unit-result c-clearfix\"]").InnerText);
                //reslist.Add(dw);
                reslist.Add(num + dw);
            }
            catch { }

            //邮编
            try
            {
                string dw = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_post_content \"]").InnerText);
                reslist.Add(dw);
            }
            catch { }


            // 百度知道最佳答案
            // 去所跳转的页面上找答案
            try
            {
                string dw = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_best_answer_question_link\"]").GetAttributeValue("href", ""));
                var answers = getBaiduZhidaoAnswers(dw)[0];
                reslist.Add(answers);
            }
            catch { }

            // 百度知道的推荐答案的第一个
            // 去所跳转的页面上找答案
            try
            {
                string dw = parser.removeUnText(hdoc.DocumentNode.SelectSingleNode("//*[@class=\"op_generalqa_answer c-gap-bottom-small op_generalqa_answer_first\"]").ChildNodes[3].FirstChild.GetAttributeValue("href", ""));
                var answers = getBaiduZhidaoAnswersByUrl(dw)[0];
                reslist.Add(answers);
            }
            catch { }

            // 百度日历上的日子
            // 由于它是js生成的日历，所以需要从js里正则匹配一下
            try
            {
                Regex reg = new Regex("\"selectday\":\"([^\"]*?)\"", RegexOptions.None);
                string date = reg.Match(html).Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(date)) reslist.Add(date);
            }
            catch { }


            return reslist.ToArray();
        }

        /// <summary>
        /// 从html文档中找出文本部分
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string[] getText(string html)
        {
            if (string.IsNullOrWhiteSpace(html) || html[0] != '<')
            {
                return new string[] { parser.removeUnText(html) };
            }
            List<string> res = new List<string>();
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            foreach (var node in hdoc.DocumentNode.ChildNodes)
            {
                var text = parser.removeUnText(node.InnerText).Trim();
                if (!string.IsNullOrWhiteSpace(text)) res.Add(text);
                //if (node.NodeType == HtmlNodeType.Text)
                //{
                //    res.Add(ItemParser.removeUnText(node.InnerText));
                //}
                ////else if (node.Name == "br")
                ////{
                ////    res.Add("\r\n");
                ////}
                //else if (node.NodeType == HtmlNodeType.Element)
                //{
                //    var tmp = getText(node.InnerHtml);
                //    foreach (var t in tmp) res.Add(t);
                //}

            }

            return res.ToArray();
        }


        public string[] getBaiduTiebaAnswers(string sentence, int num = 10)
        {
            string url = string.Format("http://tieba.baidu.com/f/search/res?ie=utf-8&qw=%20{0}", WebConnectActor.UrlEncode(sentence));
            List<string> res = new List<string>();
            string html = WebConnectActor.getData(url, Encoding.GetEncoding("gb2312"));
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            try
            {
                HtmlNodeCollection tiezinodes = null;
                try
                {
                    tiezinodes = hdoc.DocumentNode.SelectNodes("//div[@class=\"p_content\"]");
                    
                    int numm = 0;
                    foreach(var node in tiezinodes)
                    {
                        string content = removeReplyWords(node.InnerText);
                        content = string.Join("\r\n",getText(content)).Trim();
                        //bool useful = true;
                        int dontmatch = 0;
                        foreach(char c in sentence)
                        {
                            if (!content.Contains(c))
                            {
                                dontmatch ++;
                                //useful = false;
                                //break;
                            }
                        }
                        if(dontmatch <= 2)
                        {
                            res.Add(content);
                            numm++;
                            if (numm >= num) break;
                        }
                        
                    }
                }
                catch { }
            }
            catch { }

            return res.ToArray();
        }

        string removeReplyWords(string str)
        {
            str = str.Replace("??", "");
            if (str.StartsWith("回复"))
            {
                int f = str.IndexOf(":");
                if (f > 3 && f <20)
                {
                    return str.Substring(f + 1);
                }
            }
            return str;
        }

        /// <summary>
        /// 在百度知道查询答案。
        /// </summary>
        /// <param name="sentence">要查询的句子</param>
        /// <param name="num">获取的答案数</param>
        /// <returns></returns>
        public string[] getBaiduZhidaoAnswers(string sentence, int num = 10)
        {
            string url = string.Format("http://zhidao.baidu.com/search?word={0}", WebConnectActor.UrlEncode(sentence));

            List<string> res = new List<string>();
            string html = WebConnectActor.getData(url, Encoding.GetEncoding("gb2312"));
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            try
            {
                HtmlNode favurl = null;
                try
                {
                    favurl = hdoc.DocumentNode.SelectSingleNode("//dt[@class=\"dt mb-8\"]").ChildNodes[1];
                } catch  { }

                var urls = hdoc.DocumentNode.SelectNodes("//a[@class=\"ti\"]");
                if (favurl != null)
                    urls.Insert(0, favurl);
                foreach (var aurl in urls)
                {
                    string dw = ItemParser.removeBlank(aurl.GetAttributeValue("href", ""), true);
                    var areslist = getBaiduZhidaoAnswersByUrl(dw);
                    if (areslist.Length > 0)
                    {
                        res.Add(areslist[0].Trim());
                    }
                    if (res.Count > num) break;

                }
            }
            catch { }

            return res.ToArray();
        }

        //public string[] getBaiduBaikeAnswer(string sentence)
        //{
        //    string url = string.Format("https://baike.baidu.com/item/{0}", WebConnectActor.UrlEncode(sentence));
        //    List<string> res = new List<string>();
        //    string html = WebConnectActor.getData(url, Encoding.GetEncoding("gb2312"));
        //    HtmlDocument hdoc = new HtmlDocument();
        //    hdoc.LoadHtml(html);

        //}

        /// <summary>
        /// 根据百度知道的页面来查找是否有最佳答案或者用户认可答案之类的
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string[] getBaiduZhidaoAnswersByUrl(string url)
        {

            List<string> res = new List<string>();

            // 弱智百度的编码是gb2312
            string html = WebConnectActor.getData(url, Encoding.GetEncoding("gb2312"));
            HtmlDocument hdoc = new HtmlDocument();
            hdoc.LoadHtml(html);

            // 新版最佳答案
            try
            {
                string dw = hdoc.DocumentNode.SelectSingleNode("//*[@class=\"best-text mb-10\"]").InnerHtml;
                dw = replaceImageWords(dw).Trim();

                var tmp = getText(dw);
                StringBuilder sb = new StringBuilder();
                foreach (var t in tmp) if (!string.IsNullOrWhiteSpace(t.Trim())) sb.Append(t + "\r\n");
                sb.Replace("\r\n\r\n", "\r\n");
                res.Add(sb.ToString());
            }
            catch { }

            return res.ToArray();

            //res = reg.Match(res).Groups[1].ToString();
            //reg = new Regex("href=\"(.*?)\"");
            //if (reg.IsMatch(res))
            //{
            //    //从知道首页找到最接近的答案的url
            //    askUrl = reg.Match(res).Groups[1].ToString();
            //    res = WebConnectHelper.getData(askUrl).Replace("\n", "").Replace("\r", "").Replace(" ", "");

            //    Regex[] regs = new Regex[]{
            //                        //被采纳答案
            //                        new Regex("wgt-best(.*?)i-quality-icon"),
            //                        new Regex("wgt-best(.*?)answer-share-widget"),
            //                        //尝试优质答案
            //                        new Regex("quality-content-detailcontent\">(.*?)</div>"),
            //                        //尝试网友推荐答案
            //                        new Regex("wgt-recommend(.*?)i-quality-icon")

            //                    };
            //    bool ismatch = false;
            //    foreach (var treg in regs)
            //    {
            //        if (treg.IsMatch(res))
            //        {
            //            //tmpOutputSentence.Add(res);
            //            res = treg.Match(res).Groups[1].ToString();
            //            ismatch = true;
            //            break;
            //        }
            //    }
            //    if (ismatch)
            //    {
            //        reg = new Regex("<pre(.*?)>(.*?)</pre>");
            //        if (reg.IsMatch(res))
            //        {
            //            res = reg.Match(res).Groups[2].ToString();
            //            res = res.Replace("<br>", "\r\n");
            //            res = res.Replace("<br/>", "\r\n");
            //            res = replaceImageWords(res);
            //            answer.Add(res);
            //        }
            //    }

            //}
        }


        //public static string[] getSearchResult(string words, int pagenum = 10)
        //{
        //    List<string> reslist = new List<string>();
        //    for (int i = 0; i < pagenum; i++)
        //    {

        //        string askUrl = "http://www.baidu.com/s?wd=" + WebConnectHelper.UrlEncode(words) + "&pn=" + (i * 10);
        //        string res = WebConnectHelper.getData(askUrl, Encoding.UTF8);
        //        res = res.Replace("\n", "").Replace("\r", "");
        //        HtmlDocument hdoc = new HtmlDocument();
        //        hdoc.LoadHtml(res);

        //        HtmlNodeCollection collection = hdoc.DocumentNode.SelectNodes("//*[@class=\"c-abstract\"]");
        //        if (collection != null)
        //        {
        //            foreach (HtmlNode node in collection)
        //            {
        //                reslist.Add(node.InnerText);
        //            }
        //            collection = hdoc.DocumentNode.SelectNodes("//*[@class=\"t\"]");
        //            foreach (HtmlNode node in collection)
        //            {
        //                reslist.Add(node.InnerText);
        //            }
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }

        //    return reslist.ToArray();
        //}

        public string[] getWebsiteAnswer(string question)
        {
            List<string> answer = new List<string>();
            string askUrl = "http://www.baidu.com/s?wd=" + WebConnectActor.UrlEncode(question);
            string res = WebConnectActor.getData(askUrl, Encoding.UTF8);
            res = res.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            Regex reg = new Regex("class=\"op_exactqa_s_answer\">(.*?)</div>");
            if (reg.IsMatch(res))
            {
                //说明百度首页给出了智能答案
                res = reg.Match(res).Groups[1].ToString();
                reg = new Regex("target=\"_blank\">(.*?)</a>");
                if (reg.IsMatch(res))
                {
                    res = reg.Match(res).Groups[1].ToString();
                    answer.Add(res);
                }

            }
            else
            {
                //判断是否是百度统计相关答案
                reg = new Regex("<p class='op_gdp_subtitle'>(.*?)</p>");
                if (reg.IsMatch(res))
                {
                    res = reg.Match(res).Groups[1].ToString();
                    answer.Add(res);
                }
                else
                {
                    //判断是否是计算题答案
                    reg = new Regex("line-height:22px;padding-bottom:2px;width:474px;\">(.*?)</div>");
                    if (reg.IsMatch(res))
                    {
                        res = reg.Match(res).Groups[1].ToString().Replace("&nbsp;", " ");
                        answer.Add(res);
                    }
                    else
                    {
                        string tmpstr = "正在百度问题：" + question;
                        answer.Add(tmpstr);
                        //去百度知道查一波
                        askUrl = "http://zhidao.baidu.com/search?word=" + question;
                        res = WebConnectActor.getData(askUrl);
                        res = res.Replace("\n", "").Replace("\r", "").Replace(" ", "");

                        //如果rank较低就舍弃
                        reg = new Regex("data-rank=\"(.*?)\"");
                        if (reg.IsMatch(res))
                        {
                            string rank = reg.Match(res).Groups[1].ToString().Split(':')[0];
                            int rankvalue = Int32.Parse(rank);
                            if (rankvalue <= 500)
                            {
                                //rank太低了，不再查询答案。
                                //return false;
                            }
                        }

                        reg = new Regex("data-log-area=\"list\">(.*?)</a>");
                        if (reg.IsMatch(res))
                        {
                            res = reg.Match(res).Groups[1].ToString();
                            reg = new Regex("href=\"(.*?)\"");
                            if (reg.IsMatch(res))
                            {
                                //从知道首页找到最接近的答案的url
                                askUrl = reg.Match(res).Groups[1].ToString();
                                res = WebConnectActor.getData(askUrl).Replace("\n", "").Replace("\r", "").Replace(" ", "");

                                Regex[] regs = new Regex[]{
                                    //被采纳答案
                                    new Regex("wgt-best(.*?)i-quality-icon"),
                                    new Regex("wgt-best(.*?)answer-share-widget"),
                                    //尝试优质答案
                                    new Regex("quality-content-detailcontent\">(.*?)</div>"),
                                    //尝试网友推荐答案
                                    new Regex("wgt-recommend(.*?)i-quality-icon")

                                };
                                bool ismatch = false;
                                foreach (var treg in regs)
                                {
                                    if (treg.IsMatch(res))
                                    {
                                        //tmpOutputSentence.Add(res);
                                        res = treg.Match(res).Groups[1].ToString();
                                        ismatch = true;
                                        break;
                                    }
                                }
                                if (ismatch)
                                {
                                    reg = new Regex("<pre(.*?)>(.*?)</pre>");
                                    if (reg.IsMatch(res))
                                    {
                                        res = reg.Match(res).Groups[2].ToString();
                                        res = res.Replace("<br>", "\r\n");
                                        res = res.Replace("<br/>", "\r\n");
                                        res = replaceImageWords(res);
                                        answer.Add(res);
                                    }
                                }



                                //{

                                //    reg = ;
                                //    if (reg.IsMatch(res))
                                //    {
                                //        res = reg.Match(res).Groups[1].ToString();
                                //        res = res.Replace("<br>", "\r\n");
                                //        res = res.Replace("<br/>", "\r\n");
                                //        res = replaceImageWords(res);
                                //        tmpOutputSentence.Add(res);
                                //        isa = true;
                                //    }
                                //    else
                                //    {
                                //        ;
                                //        if (reg.IsMatch(res))
                                //        {
                                //            res = reg.Match(res).Groups[1].ToString();
                                //            reg = new Regex("<pre(.*?)>(.*?)</pre>");
                                //            if (reg.IsMatch(res))
                                //            {
                                //                res = reg.Match(res).Groups[2].ToString();
                                //                res = res.Replace("<br>", "\r\n");
                                //                res = res.Replace("<br/>", "\r\n");
                                //                res = replaceImageWords(res);
                                //                tmpOutputSentence.Add(res);
                                //                isa = true;
                                //            }
                                //        }
                                //    }
                                //}

                            }
                        }
                    }
                }
            }




            return answer.ToArray();
        }

    }
}
