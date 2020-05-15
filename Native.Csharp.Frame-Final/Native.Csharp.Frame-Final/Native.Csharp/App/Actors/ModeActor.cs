using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static Native.Csharp.App.Event.MomordicaMain;

namespace Native.Csharp.App.Actors
{
    class ModeInfo
    {
        Random rand = new Random((int)DateTime.Now.Ticks);
        public string name;
        List<string> config;
        List<string> sentences;
        // public int 
        public ModeInfo()
        {
            config = new List<string>();
            sentences = new List<string>();
        }

        public ModeInfo(string _name, ICollection<string> _config, ICollection<string> _sentences)
        {
            name = _name;
            config = _config.ToList();
            sentences = _sentences.ToList();
        }

        public string getRandomSentence(string seed = "")
        {

            int maxsnum = 5;
            int maxslen = 7;
            int maxwordnum = 4;

            if (config.Contains("单句"))
            {
                maxslen = 1;
                maxwordnum = 1;
                maxsnum = 1;
            }
            if (config.Contains("句内不拼接"))
            {
                maxwordnum = 1;
                maxslen = 1;
            }


            string result = "";
            //byte[] md5data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            string[] sgn1 = new string[] { ",", "，", "；", "、" };
            string[] sgn2 = new string[] { "\r\n", "。", "。", "。", "？", "！", "…","——" };
            string[] sgn3 = new string[] { "\r\n", "。", "？", "！", "…", "——","??","...","：","?!","???","!!","！！！" };

            int sn = rand.Next(1, maxsnum);

            for (int i = 0; i < sn; i++)
            {
                int thislen = rand.Next(1, maxslen);
                StringBuilder thissentence = new StringBuilder();
                int wordnum = 0;
                while (thissentence.Length < thislen && wordnum < maxwordnum)
                {
                    wordnum++;
                    thissentence.Append(sentences[rand.Next(0, sentences.Count - 1)]);
                }
                if (thissentence.Length > 0 && !sgn1.Contains(thissentence.ToString().Last().ToString()) && !sgn2.Contains(thissentence.ToString().Last().ToString()))
                {
                    if (config.Contains("无标点")) thissentence.Append(" ");
                    else thissentence.Append(sgn1[rand.Next(sgn1.Length)]);
                    result += thissentence.ToString();
                    if (result.Length > 0)
                    {
                        if (config.Contains("无标点")) ;
                        else if(config.Contains("乱打标点"))result = result.Substring(0, result.Length - 1) + sgn3[rand.Next(sgn3.Length)];
                        else result = result.Substring(0, result.Length - 1) + sgn2[rand.Next(sgn2.Length)];
                    }
                        
                }
                else
                {
                    result += thissentence.ToString();
                }
            }
            if (string.IsNullOrWhiteSpace(result))
            {
                if (config.Contains("无标点")) result = " ";
                else if (config.Contains("乱打标点")) result = result.Substring(0, result.Length - 1) + sgn3[rand.Next(sgn3.Length)];
                else result = result.Substring(0, result.Length - 1) + sgn2[rand.Next(sgn2.Length)];
            }


            return result;
        }
    }
    class ModeActor
    {
        string path = "";
        string modeIndexName = "_index.txt";
        string modePrivateName = "_mode_private.txt";
        string modeGroupName = "_mode_group.txt";
        string defaultAnswerName = "_defaultanswer.txt";

        string sstvName = "sstv.jpg";
        public List<string> sstv = new List<string>();

        public Dictionary<string, ModeInfo> modedict = new Dictionary<string, ModeInfo>();
        List<string> defaultAnswers = new List<string>();
        public Dictionary<long, string> privatemode = new Dictionary<long, string>();
        public Dictionary<long, string> groupmode = new Dictionary<long, string>();
        public Random rand = new Random();
        MD5 md5 = MD5.Create();

        string chaosv = "混沌-名词.txt";
        string chaosm = "混沌-情绪词.txt";
        string chaosw = "混沌-小万邦部分.txt";
        List<string[]> chaosWord = new List<string[]>();
        List<string> chaosMotion = new List<string>();
        List<string> chaosXwb = new List<string>();

        string yunjief = "云杰说道.txt";
        List<string> yjsd = new List<string>();

        string randomch = "随机-随机汉字.txt";
        string randomChar = "";

        string gongshouName = "gongshou.txt";
        List<string> gongshou = new List<string>();

        string qianzeName = "gengshuang.txt";
        List<string> qianze1 = new List<string>();
        List<string> qianze2 = new List<string>();

        string jokeName = "jokes.txt";
        List<string> jokes = new List<string>();
        List<string> jokesEvent = new List<string>();
        List<string> jokesOrg = new List<string>();
        List<string> jokesEnemy = new List<string>();

        string penName = "pen.txt";
        List<string> penlist = new List<string>();

        string duiP2f = "pairc2.txt";
        string duiP1f = "pairc.txt";
        Dictionary<string, string[]> cf = new Dictionary<string, string[]>();
        Dictionary<string, string[]> cf2 = new Dictionary<string, string[]>();

        string junkf = "spam.txt";
        List<List<string>> junks = new List<List<string>>();

        string symbolf = "symboltemplate.txt";
        Dictionary<string, List<string>> symbollist = new Dictionary<string, List<string>>();

        string pyf = "pinyin.txt";
        string cangtou5f = "allline5.txt";
        string cangtou7f = "allline7.txt";
        Dictionary<char, List<string>> py = new Dictionary<char, List<string>>();
        Dictionary<char, List<string>> cangtou5 = new Dictionary<char, List<string>>();
        Dictionary<char, List<string>> cangtou7 = new Dictionary<char, List<string>>();
        Dictionary<string, List<string>> cangtou5py = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> cangtou7py = new Dictionary<string, List<string>>();
        Dictionary<char, List<string>> cangwei5 = new Dictionary<char, List<string>>();
        Dictionary<char, List<string>> cangwei7 = new Dictionary<char, List<string>>();
        Dictionary<string, List<string>> cangwei5py = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> cangwei7py = new Dictionary<string, List<string>>();

        public sendQQGroupMsgHandler outputMessage;


        public ModeActor()
        {

        }

        /// <summary>
        /// 模式配置初始化，读取目前各群各人的模式配置，刷新目前支持的模式列表
        /// </summary>
        /// <param name="path"></param>
        public void init(sendQQGroupMsgHandler _outputMessage, string path)
        {
            try
            {
                outputMessage = _outputMessage;
                this.path = path;
                // load modes
                modedict = new Dictionary<string, ModeInfo>();
                List<string> modelines = FileIOActor.readLines(path + modeIndexName).ToList();
                foreach (var line in modelines)
                {
                    var items = line.Split('\t');
                    string modeName = items[0].Trim();
                    try
                    {
                        string[] modeConfigs;
                        if (items.Length >= 2)
                        {
                            modeConfigs = items[1].Trim().Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);

                        }
                        else
                        {
                            modeConfigs = new string[1] { "默认" };
                        }
                        modedict[modeName] = new ModeInfo(modeName, modeConfigs, FileIOActor.readLines($"{path}\\{modeName}.txt").ToList());
                    }
                    catch(Exception ex)
                    {
                        FileIOActor.log($"模式行[ {line} ]加载失败，{ex.Message}\r\n{ex.StackTrace}");
                    }
                }

            
                // group mode config
                groupmode = new Dictionary<long, string>();
                List<string> grouplines = FileIOActor.readLines(path + modeGroupName).ToList();
                foreach (var line in grouplines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2)
                    {
                        groupmode[long.Parse(items[0])] = items[1].Trim();
                    }
                }
                // private mode config
                privatemode = new Dictionary<long, string>();
                List<string> privatelines = FileIOActor.readLines(path + modePrivateName).ToList();
                foreach (var line in privatelines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2)
                    {
                        privatemode[long.Parse(items[0])] = items[1].Trim();
                    }
                }

                // motions
                chaosMotion =  FileIOActor.readLines(path + chaosm).ToList();
                // verb
                var wordlines = FileIOActor.readLines(path + chaosv).ToList();
                foreach(var line in wordlines)
                {
                    chaosWord.Add(line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
                // xwb
                chaosXwb = FileIOActor.readLines(path + chaosw).ToList();

                // yunjieshuodao
                yjsd = FileIOActor.readLines(path + yunjief).ToList();

                // random
                randomChar = FileIOActor.readTxtFile(path + randomch, Encoding.UTF8).Trim();

                // gongshou
                gongshou = new List<string>();
                var res = FileIOActor.readLines(path + gongshouName, Encoding.UTF8);
                string thistmp = "";
                foreach(var line in res)
                {
                    if (line.Trim() == "$$$$$$$$" && !string.IsNullOrWhiteSpace(thistmp))
                    {
                        gongshou.Add(thistmp);
                        thistmp = "";
                    }
                    else
                    {
                        thistmp += line + "\r\n";
                    }
                }
                if (!string.IsNullOrWhiteSpace(thistmp)) gongshou.Add(thistmp);

                // qianze
                qianze1 = new List<string>();
                qianze2 = new List<string>();
                int pos = 0;
                res = FileIOActor.readLines(path + qianzeName, Encoding.UTF8);
                foreach(var line in res)
                {
                    if (line.Trim().StartsWith("#1"))
                    {
                        pos = 1;
                        continue;
                    }
                    else if (line.Trim().StartsWith("#2"))
                    {
                        pos = 2;
                        continue;
                    }

                    if (pos == 1) qianze1.Add(line.Trim());
                    else if (pos == 2) qianze2.Add(line.Trim());
                }

                // joke
                jokes = new List<string>();
                jokesOrg = new List<string>();
                jokesEvent = new List<string>();
                jokesEnemy = new List<string>();
                res = FileIOActor.readLines(path + jokeName, Encoding.UTF8);
                string tmpline = "";
                foreach (var line in res)
                {
                    if (line.Trim().StartsWith("#"))
                    {
                        if (!string.IsNullOrEmpty(tmpline))
                        {
                            bool find = false;
                            if (tmpline.Contains("【部门】")) { jokesOrg.Add(tmpline);find = true; }
                            if (tmpline.Contains("【事件】")) { jokesEvent.Add(tmpline);find = true; }
                            if (tmpline.Contains("【敌国】")) { jokesEnemy.Add(tmpline); find = true; }
                            if(!find)jokes.Add(tmpline);
                        }
                        tmpline = "";
                        continue;
                    }
                    else
                    {
                        tmpline += $"{line.Trim()}\r\n";
                    }
                }

                // pen
                penlist = FileIOActor.readLines(path + penName, Encoding.UTF8).ToList();

                // duilian
                var lines = FileIOActor.readLines(path+duiP1f, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    var items2 = items[1].Split(',');
                    cf[items[0]] = items2;
                }
                lines = FileIOActor.readLines(path + duiP2f, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    var items2 = items[1].Split(',');
                    cf2[items[0]] = items2;
                }

                // junk
                if(File.Exists(path + junkf))
                {
                    lines = File.ReadAllLines(path + junkf, Encoding.UTF8);

                    List<string> nowline = new List<string>();
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            if (nowline.Count > 0)
                            {
                                junks.Add(nowline);
                                nowline = new List<string>();
                            }
                        }
                        else
                        {
                            nowline.Add(line.Trim());
                        }
                    }
                    if (nowline.Count > 0)
                    {
                        junks.Add(nowline);
                    }
                }
              


                // symbols
                lines = FileIOActor.readLines(path + symbolf, Encoding.UTF8);
                symbollist = new Dictionary<string, List<string>>();
                foreach(var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("/"))
                    {
                        var items = line.Trim().Split('\t');
                        if (items.Length >= 2)
                        {
                            if (!symbollist.ContainsKey(items[0])) symbollist[items[0]] = new List<string>();
                            symbollist[items[0]].Add(items[1]);
                        }
                    }
                }

                // cangtou
                lines = FileIOActor.readLines(path + pyf, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var items = line.Trim().Split(' ');
                    if (items.Length >= 2)
                    {
                        char ch = items[0][0];
                        for (int i = 1; i < items.Length; i++)
                        {
                            string pyall = items[i];
                            string pyori = pyall;
                            if ("12345".Contains(pyori.Last()))
                            {
                                pyori = pyori.Substring(0, pyori.Length - 1);
                            }
                            if (!py.ContainsKey(ch)) py[ch] = new List<string>();
                            py[ch].Add(pyall);
                            //py[ch]
                        }
                    }
                }
                lines = FileIOActor.readLines(path + cangtou5f, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var ttmp = line.Trim();
                    if (ttmp.Length > 0)
                    {
                        char targetch = ttmp[0];
                        if (!cangtou5.ContainsKey(targetch)) cangtou5[targetch] = new List<string>();
                        cangtou5[targetch].Add(ttmp);
                        if (py.ContainsKey(targetch))
                        {
                            foreach (var pyi in py[targetch])
                            {
                                if (!cangtou5py.ContainsKey(pyi)) cangtou5py[pyi] = new List<string>();
                                cangtou5py[pyi].Add(ttmp);
                                string pyiori = pyi.Substring(0, pyi.Length - 1);
                                if (!cangtou5py.ContainsKey(pyiori)) cangtou5py[pyiori] = new List<string>();
                                cangtou5py[pyiori].Add(ttmp);
                            }
                        }
                        targetch = ttmp[ttmp.Length-1];
                        if (!cangwei5.ContainsKey(targetch)) cangwei5[targetch] = new List<string>();
                        cangwei5[targetch].Add(ttmp);
                        if (py.ContainsKey(targetch))
                        {
                            foreach (var pyi in py[targetch])
                            {
                                if (!cangwei5py.ContainsKey(pyi)) cangwei5py[pyi] = new List<string>();
                                cangwei5py[pyi].Add(ttmp);
                                string pyiori = pyi.Substring(0, pyi.Length - 1);
                                if (!cangwei5py.ContainsKey(pyiori)) cangwei5py[pyiori] = new List<string>();
                                cangwei5py[pyiori].Add(ttmp);
                            }
                        }
                    }
                }
                lines = FileIOActor.readLines(path + cangtou7f, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var ttmp = line.Trim();
                    if (ttmp.Length > 0)
                    {
                        char targetch = ttmp[0];
                        if (!cangtou7.ContainsKey(targetch)) cangtou7[targetch] = new List<string>();
                        cangtou7[targetch].Add(ttmp);
                        if (py.ContainsKey(targetch))
                        {
                            foreach (var pyi in py[targetch])
                            {
                                if (!cangtou7py.ContainsKey(pyi)) cangtou7py[pyi] = new List<string>();
                                cangtou7py[pyi].Add(ttmp);
                                string pyiori = pyi.Substring(0, pyi.Length - 1);
                                if (!cangtou7py.ContainsKey(pyiori)) cangtou7py[pyiori] = new List<string>();
                                cangtou7py[pyiori].Add(ttmp);
                            }
                        }
                        targetch = ttmp[ttmp.Length-1];
                        if (!cangwei7.ContainsKey(targetch)) cangwei7[targetch] = new List<string>();
                        cangwei7[targetch].Add(ttmp);
                        if (py.ContainsKey(targetch))
                        {
                            foreach (var pyi in py[targetch])
                            {
                                if (!cangwei7py.ContainsKey(pyi)) cangwei7py[pyi] = new List<string>();
                                cangwei7py[pyi].Add(ttmp);
                                string pyiori = pyi.Substring(0, pyi.Length - 1);
                                if (!cangwei7py.ContainsKey(pyiori)) cangwei7py[pyiori] = new List<string>();
                                cangwei7py[pyiori].Add(ttmp);
                            }
                        }
                    }
                }


                // default
                defaultAnswers = FileIOActor.readLines(path + defaultAnswerName).ToList();

                // sstv
                sstv = FileIOActor.readLines(path + sstvName).ToList();
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public bool modeExist(string modeName)
        {
            if (!modedict.ContainsKey(modeName))
            {
                // mode not exist!
                FileIOActor.log("mode " + modeName + " not exist.");
                return false;
            }
            return true;
        }

        public void setUserMode(long user, string modeName)
        {
           // if (!modeExist(modeName)) return;
            privatemode[user] = modeName;
            try
            {
                List<string> refreshMode = new List<string>();
                foreach (var k in privatemode.Keys) refreshMode.Add($"{k}\t{privatemode[k]}");
                File.WriteAllLines(path + modePrivateName, refreshMode.ToArray());
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public void setGroupMode(long group, string modeName)
        {
            //if (!modeExist(modeName)) return;
            groupmode[group] = modeName;
            try
            {
                List<string> refreshMode = new List<string>();
                foreach (var k in groupmode.Keys) refreshMode.Add($"{k}\t{groupmode[k]}");
                File.WriteAllLines(path + modeGroupName, refreshMode.ToArray());
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }


        public string printModeList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var modename in modedict.Keys) sb.Append($"{modename}模式\r\n");
            return sb.ToString();
        }


        public string getGroupMode(long group)
        {
            string modeName = "混沌";
            if (groupmode.ContainsKey(group))
            {
                modeName = groupmode[group];
            }
           // if(!modeExist(modeName)) modeName = "混沌";
            return modeName;
        }

        public string getUserMode(long user)
        {
            string modeName = "混沌";
            if (privatemode.ContainsKey(user))
            {
                modeName = privatemode[user];
            }
            //if (!modeExist(modeName)) modeName = "混沌";
            return modeName;
        }

        /// <summary>
        /// 按照模式随机生成回复
        /// 模式是在配置文件里添加的，bot初始化时会从中读取要加载的模式，然后把句子都扔进内存来缓存
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string getAnswerWithMode(long user, string question, string mode)
        {
            if (modedict.ContainsKey(mode))
            {
                return modedict[mode].getRandomSentence(question);
            }
            return "";
        }

        /// <summary>
        /// 龚诗 bot 特有的模拟
        /// </summary>
        /// <returns></returns>
        public string getGong()
        {
            StringBuilder sb = new StringBuilder();
            
            int snum = rand.Next(1, 5);
            for(int i = 0; i < snum; i++)
            {
                int wnum = rand.Next(1, 5);
                int nowlen = 0;
                for(int j = 0; j < wnum; j++)
                {
                    string s = chaosXwb[rand.Next(chaosXwb.Count)];
                    sb.Append(s);
                    nowlen += s.Length;
                    if (nowlen > 15) break;
                }
                if (i < snum - 1) sb.Append("，");
                else sb.Append("。？！"[rand.Next(3)]);
            }

            return sb.ToString();
        }
        /// <summary>
        /// 混沌模式的组句，比其他模式稍复杂些。从2个库中按概率抽取内容，整体上接近小万邦的同时加入新词
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getChaosRandomSentence(string str)
        {
            string[] sgn = new string[] { "\r\n", "。", "？", "！", "…", "——", "??", "...", "：", "?!", "???", "!!", "！！！" };
            string result = "";
            byte[] md5data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            int sentences = rand.Next(1, 6);

            for (int i = 0; i < sentences; i++)
            {
                int thislen = rand.Next(0, 11);
                StringBuilder thissentence = new StringBuilder();
                int wordnum = 0;
                while (thissentence.Length < thislen && wordnum < 5)
                {
                    wordnum++;
                    if (rand.Next(0, 100) > 80)
                    {
                        thissentence.Append(chaosWord[rand.Next(0, chaosWord.Count - 1)][0]);
                    }
                    else
                    {
                        thissentence.Append(chaosXwb[rand.Next(0, chaosXwb.Count - 1)]);
                    }
                }
                thissentence.Append(sgn[rand.Next(sgn.Length)]);
                result += thissentence.ToString();
            }

            return result;
        }

        public string getRandomCharSentence(int time, int num)
        {
            string result = "";

            for(int i = 0; i < time; i++)
            {
                for(int j = 0; j < num; j++)
                {
                    result += randomChar[rand.Next(randomChar.Length)];
                }
                result += " ";
            }

            return result;
        }

        public string getGongshou(string gong, string shou)
        {
            string result = "";

            try
            {
                if (!string.IsNullOrWhiteSpace(gong) && !string.IsNullOrWhiteSpace(shou) && gongshou.Count > 0)
                {
                    result = gongshou[rand.Next(gongshou.Count)];
                    result = result.Replace("<攻>", gong).Replace("<受>", shou);
                }
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
            }

            return result;
        }

        public string getJoke(Dictionary<string,string> pairs)
        {
            string result = "";

            try
            {
                List<string> usingjokes = new List<string>();
                if (pairs.ContainsKey("敌国")) usingjokes.AddRange(jokesEnemy);
                if (pairs.ContainsKey("部门")) usingjokes.AddRange(jokesOrg);
                if (pairs.ContainsKey("事件")) usingjokes.AddRange(jokesEvent);
                if (usingjokes.Count <= 0) usingjokes.AddRange(jokes);
                int find = 100;
                int index = rand.Next(usingjokes.Count);
                do
                {
                    result = usingjokes[index];
                    foreach(var pair in pairs)
                    {
                        result = result.Replace($"【{pair.Key}】", pair.Value);
                    }
                    if (result.Contains("【"))
                    {
                        index = (index + 1) % usingjokes.Count;
                        find -= 1;
                    }
                    else
                    {
                        break;
                    }
                } while (find >= 0);
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
            }

            return result;
        }

        public string getSymbolDeal(string str)
        {
            string res = "";

            try
            {
                foreach(var sb in symbollist)
                {
                    if (str.StartsWith(sb.Key))
                    {
                        str = str.Substring(sb.Key.Length);
                        if (string.IsNullOrWhiteSpace(str)) return "";

                        var temp = sb.Value[rand.Next(sb.Value.Count)];
                        if (temp.StartsWith("【W】"))     // num and english char
                        {
                            // total 10 + 26 + 26 = 62
                            temp = temp.Substring(3);
                            int singnum = temp.Length / 62;
                            foreach (var ch in str)
                            {
                                try
                                {
                                    int index = -1;
                                    if (ch >= '0' && ch <= '9') index = ch - '0';// res += temp[(int)(ch - '0')];
                                    else if (ch >= 'a' && ch <= 'z') index = 10 + ch - 'a'; 
                                    else if (ch >= 'A' && ch <= 'Z') index = 36 + ch - 'A'; 
                                    if(index< 0 || singnum <= 0)
                                    {
                                        res += ch;
                                    }
                                    else
                                    {
                                        res += temp.Substring(index * singnum, singnum);
                                    }
                                }
                                catch
                                {
                                    res += ch;
                                }
                            }
                        }
                        else if (temp.StartsWith("【N】"))        // just num
                        {
                            temp = temp.Substring(3); 
                            int maxnum = temp.Length - 1;
                            int trywholenum = -1;
                            int.TryParse(str, out trywholenum);
                            if (trywholenum >= 0 && trywholenum <= maxnum)
                            {
                                // whole num single sym
                                res = temp[trywholenum].ToString();
                            }
                            else
                            {
                                // each num single char
                                foreach (var ch in str)
                                {
                                    try
                                    {
                                        int index = -1;
                                        if (ch >= '0' && ch <= '9') index = ch - '0';
                                        if (index < 0)
                                        {
                                            res += ch;
                                        }
                                        else
                                        {
                                            res += temp.Substring(index, 1);
                                        }
                                    }
                                    catch
                                    {
                                        res += ch;
                                    }
                                }
                            }
                        }
                        else if (temp.StartsWith("【E】"))        // english char
                        {
                            // total 26 + 26 = 52
                            temp = temp.Substring(3);
                            int singnum = temp.Length / 52;
                            if (sb.Key.Contains("空心字母")) singnum = 4;
                            foreach (var ch in str)
                            {
                                try
                                {
                                    int index = -1;
                                    if (ch >= 'a' && ch <= 'z') index = ch - 'a';
                                    else if (ch >= 'A' && ch <= 'Z') index = 26 + ch - 'A';
                                    if (index < 0 || singnum <= 0)
                                    {
                                        res += ch;
                                    }
                                    else
                                    {
                                        
                                        res += temp.Substring(index * singnum, singnum);
                                    }
                                }
                                catch
                                {
                                    res += ch;
                                }
                            }
                        }
                        else if (temp.Contains("阿"))          // single word repeat
                        {
                            foreach (var ch in str)
                            {
                                try
                                {
                                    res += temp.Replace('阿', ch);
                                }
                                catch
                                {
                                    res += ch;
                                }
                            }
                        }
                        else if (temp.Contains("【1】"))
                        {
                            if (temp.Contains("【2】"))
                            {
                                // double content
                                res = temp.Replace("【1】", str.Substring(0, str.Length / 2)).Replace("【2】", str.Substring(str.Length / 2));

                            }
                            else
                            {
                                // single content
                                res = temp.Replace("【1】", str);
                            }
                        }


                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
            }

            return res;
        }

        public string getSpam(string key)
        {
            string result = "";
            try
            {
                foreach(var para in junks)
                {
                    if (para.Count > 0)
                    {
                        result += para[rand.Next(para.Count)]+"\r\n";
                    }
                }
                result = result.Replace("【E】", DateTime.Now.Year.ToString());
                result = result.Replace("【B】", new string[] { "朋友", "小伙伴", "网友" }[rand.Next(3)]);
                result = result.Replace("【A】", key);
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
            }

            return result;
        }

        public string getQianze(string mine, string character, string action)
        {
            string result = "";

            try
            {
                if (!string.IsNullOrWhiteSpace(mine) && !string.IsNullOrWhiteSpace(character) && !string.IsNullOrWhiteSpace(action) && qianze1.Count > 0 && qianze2.Count > 0)
                {
                    // begin
                    result += $"记者：{character}{action}，{mine}对此有何回应？\r\n";
                    result += $"发言人：";
                    // #1
                    result += $"{qianze1[rand.Next(qianze1.Count)]}";
                    // #2
                    List<int> indexs = new List<int>();
                    for (int i = 0; i < qianze2.Count; i++) indexs.Add(i);
                    for (int i = 0; i < rand.Next(3, 6); i++)
                    {
                        int get = rand.Next(indexs.Count);
                        result += $"{qianze2[indexs[get]]}";
                        indexs.RemoveAt(get);
                    }
                    result = result.Replace("#M", mine).Replace("#N", character).Replace("#B", action);
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
            }

            return result;
        }

        public string getDui(string sin)
        {
            sin = sin.Trim();
            string sout = "";

            for (int i = 0; i < sin.Length; i++)
            {
                if (i + 1 < sin.Length && cf2.ContainsKey(sin.Substring(i, 2)))
                {
                    sout += cf2[sin.Substring(i, 2)][rand.Next(cf2[sin.Substring(i, 2)].Length)];
                    i += 1;
                }
                else if (cf.ContainsKey(sin[i].ToString()))
                {
                    sout += cf[sin[i].ToString()][rand.Next(cf[sin[i].ToString()].Length)];
                }
                //else if("３")
                else if ("123456789".Contains(sin[i]))
                {
                    sout = $"{sout}{10-int.Parse(sin[i].ToString())}";
                }
                else if ("abcdefghijklmnopqrstuvwxyz".Contains(sin[i]))
                {
                    sout += "abcdefghijklmnopqrstuvwxyz"[rand.Next(26)];
                }
                else if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(sin[i]))
                {
                    sout += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[rand.Next(26)];
                }
                else if ("あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ".Contains(sin[i]))
                {
                    sout += "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをんがぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽ"[rand.Next(71)];
                }
                else if ("アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ".Contains(sin[i]))
                {
                    sout += "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ"[rand.Next(71)];
                }
                else
                {
                    sout += sin[i];
                }
            }

            return sout;
        }


        public string getZYJ(string sin)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("云杰说道：");
            int maxlong = 170;
            int snum = rand.Next(1, 7);
            for (int i = 0; i < snum; i++)
            {
                string tmp = yjsd[rand.Next(yjsd.Count)];
                sb.Append(tmp);
                if (sb.Length > maxlong) break;
            }

            return sb.ToString().Trim();
        }

        public string getCangtou(string target)
        {
            string res = "";
            if (target.Length <= 0) return "";
            try
            {
                if (target.Length > 50)
                {
                    target = target.Substring(0, 50);
                }
                string ct5 = "";
                string ct7 = "";
                foreach (var ch in target)
                {
                    if (cangtou5.ContainsKey(ch))
                    {
                        ct5 += $"{cangtou5[ch][rand.Next(cangtou5[ch].Count)]}\r\n";
                    }
                    else if (py.ContainsKey(ch))
                    {
                        bool find = false;
                        foreach (var p in py[ch])
                        {
                            if (cangtou5py.ContainsKey(p))
                            {
                                ct5 += $"{cangtou5py[p][rand.Next(cangtou5py[p].Count)]}\r\n";
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            ct5 = "";
                            //res += $"(5)not found:{ch}\r\n";
                            break;
                        }
                    }
                    else
                    {
                        ct5 = "";
                        //res += $"(5)not found:{ch}\r\n";
                        break;
                    }
                }

                foreach (var ch in target)
                {
                    if (cangtou7.ContainsKey(ch))
                    {
                        ct7 += $"{cangtou7[ch][rand.Next(cangtou7[ch].Count)]}\r\n";
                    }
                    else if (py.ContainsKey(ch))
                    {
                        bool find = false;
                        foreach (var p in py[ch])
                        {
                            if (cangtou7py.ContainsKey(p))
                            {
                                ct7 += $"{cangtou7py[p][rand.Next(cangtou7py[p].Count)]}\r\n";
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            ct7 = "";
                           // res += $"(7)not found:{ch}\r\n";
                            break;
                        }
                    }
                    else
                    {
                        ct7 = "";
                        res += $"(7)not found:{ch}\r\n";
                        break;
                    }
                }
                if (ct7.Length > 0 && ct5.Length > 0) res = "\r\n" + (rand.Next(2) > 0 ? ct7 : ct5);
                else if (ct7.Length > 0) res = "\r\n" + ct7;
                else if (ct5.Length > 0) res = "\r\n" + ct5;
                else
                {
                    // notfind
                    res = "我做不到。我紫菜";
                }
            }catch(Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
                res = "我做不到。我紫菜";
            }
            return res;
        }
        public string getCangwei(string target)
        {
            string res = "";
            if (target.Length <= 0) return "";
            try
            {
                if (target.Length > 50)
                {
                    target = target.Substring(0, 50);
                }
                string ct5 = "";
                string ct7 = "";
                foreach (var ch in target)
                {
                    if (cangwei5.ContainsKey(ch))
                    {
                        ct5 += $"{cangwei5[ch][rand.Next(cangwei5[ch].Count)]}\r\n";
                    }
                    else if (py.ContainsKey(ch))
                    {
                        bool find = false;
                        foreach (var p in py[ch])
                        {
                            if (cangwei5py.ContainsKey(p))
                            {
                                ct5 += $"{cangwei5py[p][rand.Next(cangwei5py[p].Count)]}\r\n";
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            ct5 = "";
                            break;
                        }
                    }
                    else
                    {
                        ct5 = "";
                        break;
                    }
                }

                foreach (var ch in target)
                {
                    if (cangwei7.ContainsKey(ch))
                    {
                        ct7 += $"{cangwei7[ch][rand.Next(cangwei7[ch].Count)]}\r\n";
                    }
                    else if (py.ContainsKey(ch))
                    {
                        bool find = false;
                        foreach (var p in py[ch])
                        {
                            if (cangwei7py.ContainsKey(p))
                            {
                                ct7 += $"{cangwei7py[p][rand.Next(cangwei7py[p].Count)]}\r\n";
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            ct7 = "";
                            break;
                        }
                    }
                    else
                    {
                        ct7 = "";
                        break;
                    }
                }
                if (ct7.Length > 0 && ct5.Length > 0) res = "\r\n" + (rand.Next(2) > 0 ? ct7 : ct5);
                else if (ct7.Length > 0) res = "\r\n" + ct7;
                else if (ct5.Length > 0) res = "\r\n" + ct5;
                else
                {
                    // notfind
                    res = "我做不到。我紫菜";
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
                res = "我做不到。我紫菜";
            }
            return res;
        }
        public string getPen(long group, long user)
        {
            try
            {
                int num = rand.Next(2, 10);
                while (num-- > 0)
                {
                    outputMessage(group, user, penlist[rand.Next(penlist.Count)].Trim());
                }
                return "";
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                return "";
            }
        }

        public string getHistoryReact(long group, long userqq)
        {
            string result = "";

            string historyPath = path.Replace("DataMode", "_history\\group");
            var files = Directory.GetFiles(historyPath, "*.txt");
            int maxtime = 10;
            try
            {
                if (files.Length <= 0) return "1";
                while (maxtime-- > 0)
                {   
                    int findex = rand.Next(files.Length);
                    string[] lines = FileIOActor.readLines(files[findex]).ToArray();
                    if (lines.Length < 100) continue;
                    int begin = rand.Next(lines.Length - 5);
                    int maxnum = rand.Next(1, 5);
                    int num = lines.Length - begin;// rand.Next(10, lines.Length - begin);
                    bool find = false;
                    string targetuser = "";
                    for(int i = 0; i < num; i++)
                    {
                        try
                        {
                            var items = lines[begin + i].Trim().Split('\t');
                            if (items.Length >= 3)
                            {
                                string ban = "2715126750 2045098852 188618935 2854196310 287859992 2963959417";
                                if (ban.Contains(items[1])) continue;
                                if (targetuser.Length > 0 && targetuser != items[1]) continue;
                                targetuser = items[1];
                                string msg = items[2].Trim();
                                if (msg.Contains("2715126750") || msg.Contains("2045098852")) continue;
                                bool isSstv = false;
                                foreach(var word in sstv)
                                {
                                    if (!string.IsNullOrWhiteSpace(word) && msg.Contains(word))
                                    {
                                        isSstv = true;
                                        break;
                                    }
                                }
                                if (isSstv) continue;
                                msg = Regex.Replace(msg, "\\[CQ\\:[^\\]]+\\]", "");
                                if (msg.Trim().StartsWith("我苦") || msg.Trim().StartsWith("苦瓜")) continue;
                                if (string.IsNullOrWhiteSpace(msg.Trim())) continue;
                                //msg = Regex.Replace(msg, "\\[CQ\\:image[^\\]]+\\]", "");
                                outputMessage(group, 0, msg);
                                find = true;
                            }
                        }
                        catch (Exception e)
                        {
                            FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                        }
                        maxnum -= 1;
                        if (maxnum <= 0) break;
                        
                    }
                    if(find)
                    break;
                }
            }
            catch(Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }


            return "2";
        }


        public string getAnswerGong(long user, string question)
        {
            string answer = "";
            string msg = "";
            if (rand.Next(0, 100) < 85)
            {
                msg = getChaosRandomSentence(question) + getMotionString();
            }
            else
            {
                if (msg.Length <= 0 || rand.Next(1, 100) < 40)
                {
                    msg = getSaoHua() + getMotionString();
                }
            }

            return msg;
        }


        /// <summary>
        /// 混沌模式的回复
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        public string getAnswerChaos(long user, string question)
        {
            string answer = "";
            string msg = "";
                if (rand.Next(0, 100) < 85)
                {
                    msg = getChaosRandomSentence(question) + getMotionString();
                }
                else
                {
                    //answer = getZhidaoAnswer(question);
                    //if (answer.Length > 0)
                    //{
                    //    msg = answer + "..." + getMotionString();
                    //}
                    if (msg.Length <= 0 || rand.Next(1, 100) < 40)
                    {
                        msg = getSaoHua() + getMotionString();
                    }
                }
            
            return msg;
        }


        /// <summary>
        /// 获取骚话（情话）
        /// </summary>
        /// <returns></returns>
        public string getSaoHua()
        {
            try
            {
               return defaultAnswers[rand.Next(defaultAnswers.Count)].Trim();
            }
            catch(Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// 获取随机的括弧情绪文本。例如（悲）（大嘘）这种
        /// </summary>
        /// <returns></returns>
        public string getMotionString()
        {
            string res = "";

            if (chaosMotion.Count <= 0) return res;
            if (rand.Next(0, 100) > 66)
            {
                res = $"({chaosMotion[rand.Next(0, chaosMotion.Count - 1)]})";
            }

            return res;
        }
    }
}
