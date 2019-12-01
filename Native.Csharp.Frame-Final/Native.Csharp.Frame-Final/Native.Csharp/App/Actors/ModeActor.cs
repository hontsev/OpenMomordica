using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;

namespace Native.Csharp.App.Actors
{
    class ModeInfo
    {
        public string name;
        public int minSentenceNum;
        public int maxSentenceNum;
        public int minWordNum;
        public int maxWordNum;
       // public int 
    }
    class ModeActor
    {
        string path = "";
        string modeIndexName = "_index.txt";
        string modeSgnName = "_sgn.txt";
        string modeSgnOverName = "_sgnover.txt";
        string modePrivateName = "_mode_private.txt";
        string modeGroupName = "_mode_group.txt";
        string defaultAnswerName = "_defaultanswer.txt";
        

        public Dictionary<string, List<string>> modedict = new Dictionary<string, List<string>>();
        List<string> sgn = new List<string>();
        List<string> sgnover = new List<string>();
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

        string randomch = "随机-随机汉字.txt";
        string randomChar = "";

        string gongshouName = "gongshou.txt";
        List<string> gongshou = new List<string>();

        string qianzeName = "gengshuang.txt";
        List<string> qianze1 = new List<string>();
        List<string> qianze2 = new List<string>();

        string penName = "pen.txt";
        List<string> penlist = new List<string>();

        public sendQQGroupMsgHandler outputMessage;


        public ModeActor()
        {

        }

        /// <summary>
        /// 模式配置初始化，读取目前各群各人的模式配置，刷新目前苦瓜支持的模式列表
        /// </summary>
        /// <param name="path"></param>
        public void init(sendQQGroupMsgHandler _outputMessage, string path)
        {
            try
            {
                outputMessage = _outputMessage;
                this.path = path;
                // load modes
                modedict = new Dictionary<string, List<string>>();
                List<string> modelines = FileIOActor.readLines(path + modeIndexName).ToList();
                foreach (var line in modelines)
                {
                    var items = line.Split('\t');
                    string modeName = items[0].Trim();
                    if (items.Length >= 2)
                    {
                        string modeConfigs = items[1].Trim();  
                    }
                    else
                    {

                    }
                    string file = $"{path}\\{modeName}.txt";
                    modedict[modeName] = FileIOActor.readLines(file).ToList();
                }

                // sgn
                sgn = FileIOActor.readLines(path + modeSgnName).ToList();
                sgn.Add("\r\n");

                // sgn over
                sgnover = FileIOActor.readLines(path + modeSgnOverName).ToList();
                sgnover.Add("\r\n");

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

                // pen
                penlist = FileIOActor.readLines(path + penName, Encoding.UTF8).ToList();

                // default
                defaultAnswers = FileIOActor.readLines(path + defaultAnswerName).ToList();
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
            string result = "";
            if (modeExist(mode))
            {
                //byte[] md5data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                int sentencemaxnum = 5;
                int sentencemaxlen = 7;
                int sentencemaxwordnum = 4;

                int sentences = rand.Next(sentencemaxnum);

                for (int i = 0; i <= sentences; i++)
                {
                    int thislen = rand.Next(1, sentencemaxlen);
                    StringBuilder thissentence = new StringBuilder();
                    int wordnum = 0;
                    while (thissentence.Length < thislen && wordnum < sentencemaxwordnum)
                    {
                        wordnum++;
                        thissentence.Append(modedict[mode][rand.Next(0, modedict[mode].Count - 1)]);
                    }
                    if (thissentence.Length > 0 && !sgn.Contains(thissentence.ToString().Last().ToString()))
                    {
                        string[] noSgnModes = new string[] { "佛", "emoji" };
                        if (noSgnModes.Contains(mode)) thissentence.Append(" ");
                        else thissentence.Append("，");
                        result += thissentence.ToString();
                        if (result.Length > 0 && !noSgnModes.Contains(mode)) result = result.Substring(0, result.Length - 1) + sgnover[rand.Next(sgnover.Count)];
                    }
                    else
                    {
                        result += thissentence.ToString();
                    }
                }
                if (string.IsNullOrWhiteSpace(result)) result = sgnover[rand.Next(sgnover.Count)];
               
            }
            return result;
        }



        /// <summary>
        /// 混沌模式的组句，比其他模式稍复杂些。从2个库中按概率抽取内容，整体上接近小万邦的同时加入新词
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getChaosRandomSentence(string str)
        {
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
                thissentence.Append(sgn[rand.Next(0, sgn.Count - 1)]);
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

        public string getQianze(string mine, string character, string action)
        {
            string result = "";

            try
            {
                if (!string.IsNullOrWhiteSpace(mine) && !string.IsNullOrWhiteSpace(character) && !string.IsNullOrWhiteSpace(action) && qianze1.Count > 0 && qianze2.Count > 0)
                {
                    // begin
                    result += $"记者：{character}{action}，{mine}对此有何回应？\r\n";
                    result += $"苦瓜：";
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
                                if (msg.Contains("维尼") || msg.Contains("支那") || 
                                    msg.Contains("本群") || msg.Contains("[CQ") || 
                                    msg.Contains("被管理员") || msg.Contains("你的QQ暂不支持") || msg.Contains("请使用新版手机QQ") || msg.Contains("☆西方苦瓜公主☆"))
                                    continue;
                                msg = Regex.Replace(msg, "\\[CQ\\:[^\\]]+\\]", "");
                                if (msg.Trim().StartsWith("苦瓜")) continue;
                                msg = msg.Trim();
                                if (msg.Length <= 0) continue;
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


        /// <summary>
        /// 混沌模式的回复
        /// 混沌模式依然保留知识图谱基本查询功能
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
