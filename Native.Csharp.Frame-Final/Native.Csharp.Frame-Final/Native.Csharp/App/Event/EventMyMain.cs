using Native.Csharp.App.Actors;
using Native.Csharp.App.EventArgs;
using Native.Csharp.App.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.Event
{
    /// <summary>
    /// 苦瓜bot主要处理类
    /// </summary>
    public class MomordicaMain
    {
        public delegate void sendStringHandler(string str);
        public delegate void sendQQPrivateMsgHandler(long targetUser, string msg);
        public delegate void sendQQGroupMsgHandler(long group, long targetUser, string msg);

        public sendStringHandler log;
        public sendQQPrivateMsgHandler sendPrivate;
        public sendQQGroupMsgHandler sendGroup;
        public long myQQ;   // bot的qq
        public long masterQQ;   // 主人的qq，可能响应特殊指令，并私发一些调试消息
        public string rootDict; // 资源根目录

        private static MomordicaMain _mmdk;
        
        MD5 md5 = MD5.Create();
        public Random rand = new Random();
        bool inited = false;
        List<string> sgn = new List<string>();
        List<string> sgnover = new List<string>();
        List<string[]> words = new List<string[]>();
        Dictionary<long, long> userignore = new Dictionary<long, long>();
        Dictionary<long, string> privatemode = new Dictionary<long, string>();
        Dictionary<long, string> groupmode = new Dictionary<long, string>();
        Dictionary<string, List<string>> modedict = new Dictionary<string, List<string>>();

        List<string> motion = new List<string>();
        List<string> xwb = new List<string>();
        List<string> askname = new List<string>();

        BaiduSearchActor baidu = new BaiduSearchActor();
        BeastProofActor proof = new BeastProofActor();
        DiceActor dice = new DiceActor();
        WeatherActor weather = new WeatherActor();
        TranslateActor trans = new TranslateActor();
        BilibiliLiveActor bilibili = new BilibiliLiveActor();

        object mainmutex = new object();

        static MomordicaMain()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();//生成字节数组
            int iRoot = BitConverter.ToInt32(buffer, 0);//利用BitConvert方法把字节数组转换为整数
            Random rand = new Random(iRoot);//以这个生成的整数为种子
            _mmdk = new MomordicaMain();
        }

        public static MomordicaMain getMomordicaMain()
        {
            return _mmdk;
        }

        /// <summary>
        /// bot各类所需资源的初始化
        /// 这里会保证全局只初始化一遍
        /// </summary>
        public void tryInit()
        {
            lock (mainmutex)
            {
                if (!inited)
                {
                    baidu.init(rootDict + "\\BaiduReplace.txt");
                    xwb = new List<string>();
                    List<string> xwbstr = FileIOActor.readTxtList(rootDict + "\\xwb.txt").ToList();
                    foreach (var item in xwbstr)
                    {
                        xwb.Add(item.Trim());
                    }


                    userignore = new Dictionary<long, long>();
                    List<string> userignorestr = FileIOActor.readTxtList(rootDict + "\\ignoreuser.txt").ToList();
                    foreach (var items in userignorestr)
                    {
                        var uitem = items.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (uitem.Length < 2) continue;
                        userignore[long.Parse(uitem[0])] = long.Parse(uitem[1]);
                    }

                    sgn = FileIOActor.readTxtList(rootDict + "\\sgn.txt").ToList();
                    sgn.Add("\r\n");


                    words = new List<string[]>();
                    List<string> wordstmp = FileIOActor.readTxtList(rootDict + "\\dict.txt").ToList();
                    foreach (var word in wordstmp)
                    {
                        words.Add(word.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    motion = new List<string>();
                    List<string> motionstr = FileIOActor.readTxtList(rootDict + "\\motions.txt").ToList();
                    foreach (var item in motionstr)
                    {
                        motion.Add(item.Trim());
                    }

                    askname = new List<string>();
                    List<string> asknamestr = FileIOActor.readTxtList(rootDict + "\\askname.txt").ToList();
                    foreach (var item in asknamestr)
                    {
                        askname.Add(item.Trim());

                    }

                    groupmode = new Dictionary<long, string>();
                    List<string> modestr = FileIOActor.readTxtList(rootDict + "\\groupmode.txt").ToList();
                    foreach (var items in modestr)
                    {
                        var item = items.Split('\t');
                        if (item.Length >= 2)
                        {
                            groupmode[long.Parse(item[0])] = item[1].Trim();
                        }
                    }

                    privatemode = new Dictionary<long, string>();
                    List<string> privatemodestr = FileIOActor.readTxtList(rootDict + "\\privatemode.txt").ToList();
                    foreach (var items in privatemodestr)
                    {
                        var item = items.Split('\t');
                        if (item.Length >= 2)
                        {
                            privatemode[long.Parse(item[0])] = item[1].Trim();
                        }
                    }

                    //mode select
                    modedict = new Dictionary<string, List<string>>();
                    List<string> modeindexstr = FileIOActor.readTxtList(rootDict + "\\mode\\index.txt").ToList();
                    foreach (var line in modeindexstr)
                    {
                        string modename = line.Trim();
                        string file = $"{rootDict}\\mode\\{modename}.txt";
                        modedict[modename] = new List<string>();
                        if (File.Exists(file))
                        {
                            modedict[modename] = FileIOActor.readTxtList(file).ToList();
                        }
                    }

                    proof.init(rootDict + "\\han_bh.txt");
                    weather.init(rootDict + "\\weathercode.txt");

                    inited = true;
                }
            }

        }

        /// <summary>
        /// 处理指令句子
        /// 应当优先执行，然后看如果未按指令处理，再走后续处理逻辑
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        /// <returns>是否已按照指令进行了处理</returns>
        bool dealCmd(long group, long user, string msg)
        {
            if (user == masterQQ)
            {
                // super admin
                if (msg.Contains("remove "))
                {
                    string userstr = msg.Replace("remove ", "").Trim();
                    //Common.CqApi.AddLoger(Sdk.Cqp.Enum.LogerLevel.Info, "test", userstr);
                    long userqq = 0;
                    long.TryParse(userstr, out userqq);
                    if (userqq != 0 && !userignore.Keys.Contains(userqq))
                    {
                        try
                        {
                            userignore[userqq] = 0;
                            sendGroup(group, user, $"~好，我立即屏蔽{userqq}~");
                            File.AppendAllText(rootDict + "\\ignoreuser.txt", $"{userqq}\t{userignore[userqq]}\r\n");

                        }
                        catch { }
                        return true;
                    }
                }
            }
            bool isGroup = true;
            if (group <= 0) isGroup = false;
            msg = msg.Trim();
            if (msg.Contains("模式列表"))
            {
                string modeindexs = "";
                foreach (var modename in modedict.Keys) modeindexs += $"{modename}模式\r\n";
                modeindexs += "~输入“xx模式on”即可切换模式~";
                if (isGroup)
                {
                    sendGroup(group, user, modeindexs);
                }
                else
                {
                    sendPrivate(user, modeindexs);
                }
                return true;
            }
            foreach (var mode in modedict.Keys)
            {
                if (msg.Contains($"{mode}模式on") || msg.Contains($"{mode}模式 on"))
                {
                    if (isGroup)
                    {
                        groupmode[group] = mode;
                        sendGroup(group, 0, $"~苦瓜的{mode}模式启动~");
                        try
                        {
                            List<string> refreshMode = new List<string>();
                            foreach (var k in groupmode.Keys) refreshMode.Add($"{k}\t{groupmode[k]}");
                            File.WriteAllLines(rootDict + "\\groupmode.txt", refreshMode.ToArray());

                        }
                        catch { }
                    }
                    else
                    {
                        privatemode[user] = mode;
                        sendPrivate(user, $"~苦瓜的{mode}模式启动~");
                        try
                        {
                            List<string> refreshMode = new List<string>();
                            foreach (var k in privatemode.Keys) refreshMode.Add($"{k}\t{privatemode[k]}");
                            File.WriteAllLines(rootDict + "\\privatemode.txt", refreshMode.ToArray());

                        }
                        catch { }
                    }
                    return true;
                }
            }
            if (msg.Contains("模式on") || msg.StartsWith("模式"))
            {
                string modeindexs = "苦瓜还没有这个模式（小声）";
                if (isGroup)
                {
                    sendGroup(group, user, modeindexs);
                }
                else
                {
                    sendPrivate(user, modeindexs);
                }

                modeindexs = "";
                foreach (var modename in modedict.Keys) modeindexs += $"{modename}模式\r\n";
                modeindexs += "~输入“xx模式on”即可切换模式~";
                if (isGroup)
                {
                    sendGroup(group, user, modeindexs);
                }
                else
                {
                    sendPrivate(user, modeindexs);
                }
                return true;
            }

            // 数字论证
            if(msg.StartsWith("数字论证"))
            {
                bool proofsuccess = proof.getProofString(msg.Replace("数字论证", "").Trim());
                if (proofsuccess)
                {
                    //sendPrivate(masterQQ, proof.finalproof);
                    if (isGroup) sendGroup(group, user, proof.finalproof);
                    else sendPrivate(user, proof.finalproof);
                }
                else
                {
                    string resspeak = "论不出来，我紫菜";
                    if (isGroup) sendGroup(group, user, resspeak);
                    else sendPrivate(user, resspeak);
                }
                return true;
            }
            
            // 功能介绍
            if(msg=="功能" || msg == "设置")
            {
                if (isGroup)
                {
                    sendGroup(group, -1, getWelcomeString());
                }
                else
                {
                    sendPrivate(user, getWelcomeString());
                }
                return true;
            }

            // 天气 
            if (msg.EndsWith("天气"))
            {
                msg = msg.Substring(0, msg.Length - 2);
                string daystr = "今天";
                var daystrs = new string[] { "今天", "明天", "大后天" ,"后天" };
                foreach(var ds in daystrs)
                {
                    if (msg.EndsWith(ds))
                    {
                        daystr = ds;
                        msg = msg.Substring(0, msg.Length - ds.Length);
                        break;
                    }

                    if (msg.StartsWith(ds))
                    {
                        daystr = ds;
                        msg = msg.Substring(ds.Length);
                        break;
                    }
                }
                string wres = weather.getWeather(msg, daystr);
                if (!string.IsNullOrWhiteSpace(wres))
                {
                    wres = msg + wres;
                    if (isGroup)  sendGroup(group, user, wres);
                    else  sendPrivate(user, wres);
                    return true;
                }

            }

            // 翻译
            if (msg.StartsWith("翻译")){
                msg = msg.Substring(2);
                if (!string.IsNullOrWhiteSpace(msg))
                { 
                    string transstr = trans.Translation(msg);
                    if (isGroup) sendGroup(group, user, transstr);
                    else sendPrivate(user, transstr);
                    return true;
                }
            }

            if (msg == "虚拟区谁在播")
            {
                string xnq = bilibili.getLiveNum();
                if (isGroup) sendGroup(group, user, xnq);
                else sendPrivate(user, xnq);
                return true;
            }

            // 骰子
            string diceres = dice.getRollString(msg.Trim());
            if (!string.IsNullOrWhiteSpace(diceres))
            {
                if (isGroup) sendGroup(group, user, diceres);
                else sendPrivate(user, diceres);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 正常模式的回复
        /// 正常模式不会进行随机拼句回复，而是尽量爬取网上的有用信息来回应
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        string getAnswerNormal(long user, string question)
        {
            string answer = "";
            string msg = "";
            answer = getKGAnswer(question);
            if (answer.Length > 0)
            {
                msg = answer;
            }
            else
            {

                answer = getZhidaoAnswer(question);

                if (answer.Length > 0)
                {
                    msg = answer + "...";

                }

                if (rand.Next(0, 100) > 75 || msg.Length <= 0)
                {
                    try
                    {
                        var tiebares = baidu.getBaiduTiebaAnswers(question);
                        if (tiebares.Length > 0)
                        {
                            string tiebaanswer = tiebares[rand.Next(0, tiebares.Length)].Trim();
                            msg = tiebaanswer;
                            //sendPrivate(masterQQ, question + "\r\n\r\n" + tiebaanswer);
                        }
                    }
                    catch { }
                }


                if (msg.Length <= 0)
                {
                    msg = getSaoHua();
                }



            }
            msg = getHexie(msg);
            return msg;
        }

        /// <summary>
        /// 混沌模式的回复
        /// 混沌模式依然保留知识图谱基本查询功能
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        string getAnswerChaos(long user, string question)
        {
            string answer = "";
            string msg = "";

            answer = getKGAnswer(question);
            if (answer.Length > 0)
            {
                msg = answer + getMotionString();
            }
            else
            {
                if (rand.Next(0, 100) < 85)
                {
                    msg = getRandomSentence(question) + getMotionString();
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

            }
            msg = getHexie(msg);
            //msg = getSaoHua();
            return msg;
        }

        /// 
        /// 面对输入的逻辑：
        /// -0 若未初始化，先初始化
        /// 0 记录聊天内容
        /// 1 先检查群聊中是否需要我响应该消息，并过滤掉at的指令等（如果私聊则跳过这一步）
        /// 2 检查对方权限，作对应忽略
        /// 3 按内容处理
        /// 4 记录输出的聊天内容
        /// 

        object mlock = new object();
        /// <summary>
        /// 判断是否回复特定qq号的消息
        /// 根据ignore文件内的配置来作判断
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool allowuser(long user)
        {
            if (DateTime.Now.Minute == 0)
            {
                // 整小时，重置互乐次数
                lock (mlock)
                {
                    try
                    {
                        userignore = new Dictionary<long, long>();
                        List<string> userignorestr = FileIOActor.readTxtList(rootDict + "\\ignoreuser.txt").ToList();
                        foreach (var items in userignorestr)
                        {
                            var uitem = items.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (uitem.Length < 2) continue;
                            userignore[long.Parse(uitem[0])] = long.Parse(uitem[1]);
                        }
                    }
                    catch { }
                }
            }

            if (userignore.Keys.Contains(user))
            {
                long lefttime = userignore[user];
                log(lefttime.ToString());
                if (lefttime > 0)
                {
                    userignore[user] = lefttime - 1;
                    log(userignore[user].ToString());
                    return true;
                }
                log("igore..");
                return false;
            }
            log("no ignore" + user.ToString());
            return true;
        }

        /// <summary>
        /// 处理群消息
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="question"></param>
        public void dealGroupMsg(long group, long user, string question)
        {
            tryInit();
            saveMsg(group, user, DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss"), question.Trim());
            if (!askme(ref question)) return;
            if (!allowuser(user)) return;
            if (dealCmd(group, user, question)) return;

            string msg = "";

            if (groupmode.ContainsKey(group))
            {
                switch (groupmode[group])
                {
                    case "正常": msg += getAnswerNormal(user, question); break;
                    case "混沌": msg += getAnswerChaos(user, question); break;
                    default: msg += getAnswerWithMode(user, question, groupmode[group]); break;
                }
            }
            else
            {
                msg += getAnswerChaos(user, question);
            }
            sendGroup(group, user, msg);
            saveMsg(group, myQQ, DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss"), msg.Trim());

        }

        /// <summary>
        /// 处理私聊信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        public void dealPrivateMsg(long user, string question)
        {
            tryInit();
            saveMsg(0, user, DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss"), question.Trim());
            if (!allowuser(user)) return;
            if (user == masterQQ)
            {
                // Common.CqApi.SendPrivateMessage(user, "[CQ: rich, url = https://i.y.qq.com/v8/playsong.html?songid=201243685&amp;source=yqq#wechat_redirect,text=来自QQ音乐的分享 《Lightbreaker》]");
            }
            if (dealCmd(0, user, question)) return;

            string msg = "";
            if (privatemode.ContainsKey(user))
            {
                switch (privatemode[user])
                {
                    case "正常": msg += getAnswerNormal(user, question); break;
                    case "混沌": msg += getAnswerChaos(user, question); break;
                    default: msg += getAnswerWithMode(user, question, privatemode[user]); break;
                }
            }
            else
            {
                msg += getAnswerNormal(user, question);
            }
            sendPrivate(user, msg);
            saveMsg(0, user, DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss") + "[me]", msg.Trim());
        }

        object savemsgLock = new object();

        /// <summary>
        /// 记录群/私人聊天信息到文件中
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="time"></param>
        /// <param name="msg"></param>
        void saveMsg(long group, long user, string time, string msg)
        {
            lock (savemsgLock)
            {
                try
                {
                    if (group <= 0)
                    {
                        // private
                        File.AppendAllText(
                            $"{rootDict}\\history\\private\\{user}.txt",
                            $"{time}\t{msg}\r\n",
                            Encoding.UTF8
                            );
                    }
                    else
                    {
                        // group
                        string gfile = $"{rootDict}\\history\\group\\{group}.txt";
                        if (!File.Exists(gfile))
                        {
                            sendGroup(group, -1, getWelcomeString());
                        }
                        File.AppendAllText(
                            gfile,
                            $"{time}\t{user}\t{msg}\r\n",
                            Encoding.UTF8
                            );
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 按照模式随机生成回复
        /// 模式是在配置文件里添加的，bot初始化时会从中读取要加载的模式，然后把句子都扔进内存来缓存
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        string getAnswerWithMode(long user, string question, string mode)
        {
            if (modedict.ContainsKey(mode) && modedict[mode] != null && modedict[mode].Count > 0)
            {
                string result = "";
                //byte[] md5data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                int sentencemaxnum = 6;
                int sentencemaxlen = 7;
                int sentencemaxwordnum = 4;

                int sentences = rand.Next(sentencemaxnum);

                for (int i = 0; i < sentences; i++)
                {
                    int thislen = rand.Next(1, sentencemaxlen);
                    StringBuilder thissentence = new StringBuilder();
                    int wordnum = 0;
                    while (thissentence.Length < thislen && wordnum < sentencemaxwordnum)
                    {
                        wordnum++;
                        thissentence.Append(modedict[mode][rand.Next(0, modedict[mode].Count - 1)]);
                    }
                    if (thissentence.Length>0 && !sgn.Contains(thissentence.ToString().Last().ToString()))
                    {
                        string[] noSgnModes = new string[] { "佛", "emoji" };
                        if (noSgnModes.Contains(mode)) thissentence.Append(" ");
                        else thissentence.Append("，");
                        result += thissentence.ToString();
                        if (result.Length > 0 && !noSgnModes.Contains(mode)) result = result.Substring(0, result.Length - 1) + "。";
                    }
                    else
                    {
                        result += thissentence.ToString();
                    }
                }
                result = getHexie(result);
                if (string.IsNullOrWhiteSpace(result)) result = sgn[rand.Next(sgn.Count)];
                return result;
            }
            else
            {
                return getAnswerChaos(user, question);
            }
        }

        

        /// <summary>
        /// 检查这个句子是否是在问bot，如果是才作回应
        /// 用于群聊天
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        bool askme(ref string question)
        {
            foreach (var name in askname)
            {
                //Common.CqApi.AddLoger(Sdk.Cqp.Enum.LogerLevel.Info, "name", name);
                if (question.StartsWith(name))
                {
                    question = question.Substring(name.Length).Trim();
                    if (question.StartsWith("，")) question = question.Substring(1);
                    if (question.StartsWith(",")) question = question.Substring(1);
                    int maxnum = 100;
                    do
                    {
                        int begin = question.IndexOf("[CQ:emoji");
                        if (begin < 0) break;
                        int end = question.IndexOf("]");
                        if (end < 0) break;
                        try
                        {
                            question = question.Substring(0, begin) + question.Substring(end + 1);
                        }
                        catch
                        {
                            break;
                        }
                    } while (maxnum-- > 0);
                    return true;
                }
            }

            if (question.Contains(CqCode_At(myQQ)))
            {
                question = question.Replace(CqCode_At(myQQ), "");
                return true;
            }
            question = question.Trim();
            return false;
        }

        /// <summary>
        /// 获取随机的括弧情绪文本。就是（悲）（大嘘）这种
        /// </summary>
        /// <returns></returns>
        string getMotionString()
        {
            string res = "";

            if (motion.Count <= 0) return res;
            if (rand.Next(0, 100) > 66)
            {
                res = $"({motion[rand.Next(0, motion.Count - 1)]})";
            }

            return res;
        }

        /// <summary>
        /// 某些字段的和谐
        /// 输出前的必备步骤
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getHexie(string str)
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
        string CqCode_At(long qqId = -1, bool addSpacing = true)
        {
            return string.Format("[CQ:at,qq={0}]{1}", (qqId == -1) ? "all" : qqId.ToString(), addSpacing ? " " : string.Empty);
        }

        /// <summary>
        /// 去除HTML标记 
        /// </summary>
        /// <param name="strHtml">包括HTML的源码 </param>
        /// <returns>已经去除后的文字</returns>
        static string StripHTML(string strHtml)
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
        /// 从百度知道的问答中找回复
        /// 提取出多条搜索结果，然后从中随机选一个
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getZhidaoAnswer(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            string result = "";
            var res1 = baidu.getBaiduZhidaoAnswers(str, 5);
            if (res1.Length > 0)
            {
                int maxlen = 150;
                int findwidth = 20;
                var tmp = res1[rand.Next(0, res1.Length)].Replace("展开全部", "").Replace("\r", "").Trim();
                tmp = StripHTML(tmp);
                try
                {
                    File.WriteAllText(rootDict + "\\answer\\" + str + ".txt", tmp);
                }
                catch { }

                if (tmp.Length <= maxlen)
                    result = tmp;
                else
                {
                    var tmp2 = tmp;//.Split(new char[] { '\n' },StringSplitOptions.RemoveEmptyEntries)[0];
                    if (tmp2.Length >= maxlen)
                    {
                        int cutPos = tmp2.IndexOfAny(new char[] { '。', '！', '？', '…', '!', '?' }, maxlen - findwidth);
                        if (cutPos > 0 && cutPos < maxlen + findwidth)
                        {
                            result = tmp2.Substring(0, cutPos);
                        }
                        else
                        {
                            cutPos = tmp2.IndexOfAny(new char[] { ',', '；', '、', ',', '.', '》', '”', '"', '\'' }, maxlen - findwidth);
                            if (cutPos > 0 && cutPos < maxlen + findwidth)
                            {
                                result = tmp2.Substring(0, cutPos);
                            }
                            else
                            {
                                result = tmp2.Substring(0, maxlen);
                            }
                        }
                    }
                    else
                        result = tmp2;
                }
            }

            return result;
        }

        /// <summary>
        /// 从百度知识图谱数据中取得问题的答案
        /// 百度知识图谱包括一些常识信息，也能数学运算、查汇率之类的。
        /// 和百度搜索结果中的“智能”显示的知识部分一致
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getKGAnswer(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return "";
            var res = baidu.getBaiduKGResult(str);
            if (res.Length > 0)
            {
                return res[0].Trim();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 获取骚话（情话）
        /// </summary>
        /// <returns></returns>
        string getSaoHua()
        {
            string res = "";

            var list = FileIOActor.readTxtList(rootDict + "\\saohua.txt");
            res = list[rand.Next(0, list.Length - 1)].Trim();

            return res;
        }

        /// <summary>
        /// 混沌模式的组句，比其他模式稍复杂些。从2个库中按概率抽取内容，整体上接近小万邦的同时加入新词
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string getRandomSentence(string str)
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
                        thissentence.Append(words[rand.Next(0, words.Count - 1)][0]);
                    }
                    else
                    {
                        thissentence.Append(xwb[rand.Next(0, xwb.Count - 1)]);
                    }
                }
                thissentence.Append(sgn[rand.Next(0, sgn.Count - 1)]);
                result += thissentence.ToString();
            }

            return result;
        }

        /// <summary>
        /// bot的欢迎文本
        /// </summary>
        /// <returns></returns>
        public string getWelcomeString()
        {
            tryInit();
            return "我是苦瓜bot。用法：\r\n" +
                "~在群里回复：at我或者打字开头加“苦瓜”\r\n" +
                "~切换模式：输入“xx模式on”\r\n" +
                "~翻译：输入“翻译xxx”\r\n" +
                "~天气预报：输入“北京明天天气”\r\n" +
                "~掷骰：输入“r1d100 xxx”\r\n" +
                "~数字论证：输入“数字论证xxx”";
        }
    }


    class EventMyMain : 
        IReceiveGroupMessage, 
        IReceiveFriendMessage, 
        IReceiveFriendAddRequest, 
        IReceiveAddGroupBeInvitee,
        IReceiveFriendIncrease
    {
        MomordicaMain mmdk;

        private void log(string str)
        {
            tryInit();
            Common.CqApi.AddLoger(Sdk.Cqp.Enum.LogerLevel.Info, "debuglog", str);
        }

        private void tryInit()
        {
            try
            {
                if(mmdk==null)
                {
                    mmdk = MomordicaMain.getMomordicaMain();
                    mmdk.myQQ = Common.CqApi.GetLoginQQ();
                    mmdk.masterQQ = 287859992;
                    mmdk.rootDict = Common.AppDirectory;
                    mmdk.log = log;
                    mmdk.sendGroup = sendGroup;
                    mmdk.sendPrivate = sendPrivate;
                    mmdk.tryInit();
                    
                }
                
            }
            catch { }
        }

        private void sendPrivate(long user, string msg)
        {
            Common.CqApi.SendPrivateMessage(user, msg);
        }

        private void sendGroup(long group, long user, string msg)
        {
            if (user > 0)
            {
                msg = Common.CqApi.CqCode_At(user) + msg;
            }
            
            Common.CqApi.SendGroupMessage(group, msg);
        }

        public void ReceiveGroupMessage(object sender, CqGroupMessageEventArgs e)
        {
            tryInit();
            try
            {
                mmdk.dealGroupMsg(e.FromGroup, e.FromQQ, e.Message);
            }catch(Exception ex)
            {
                sendPrivate(mmdk.masterQQ, ex.Message + "\r\n" + ex.StackTrace);
            }
            
         }

        public void ReceiveFriendMessage(object sender, CqPrivateMessageEventArgs e)
        {
            tryInit();
            try
            {
                mmdk.dealPrivateMsg(e.FromQQ, e.Message);
            }
            catch (Exception ex)
            {
                sendPrivate(mmdk.masterQQ, ex.Message + "\r\n" + ex.StackTrace);
            }
            
        }

        public void ReceiveAddGroupBeInvitee(object sender, CqAddGroupRequestEventArgs e)
        {
            Common.CqApi.SetGroupAddRequest(e.ResponseFlag, Sdk.Cqp.Enum.RequestType.GroupInvitation, Sdk.Cqp.Enum.ResponseType.PASS,"");
        }

        public void ReceiveFriendAddRequest(object sender, CqAddFriendRequestEventArgs e)
        {
            Common.CqApi.SetFriendAddRequest("", Sdk.Cqp.Enum.ResponseType.PASS);
        }

        public void ReceiveFriendIncrease(object sender, CqFriendIncreaseEventArgs e)
        {
            //sendPrivate(e.FromQQ, mmdk.getWelcomeString());
        }
    }
}
