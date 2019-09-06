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
        public delegate string getQQNickHandler(long qq);

        public sendStringHandler log;
        public sendQQPrivateMsgHandler sendPrivate;
        public sendQQGroupMsgHandler sendGroup;
        public getQQNickHandler getQQNick;

        private static MomordicaMain _mmdk;

        public long myQQ;                // bot的qq
        public long masterQQ;           // 主人的qq，可能响应特殊指令，并私发一些调试消息

        public string rootDict;         // 资源根目录
        string asknameFile = "askname.txt";
        string configFile = "config.txt";
        string historyPath = "_history\\";
        string groupBlacklistFile = "group_blacklist.txt";
        string groupWhitelistFile = "group_whitelist.txt";
        string userBlacklistFile = "user_blacklist.txt";
        string DataBaiduPath = "\\DataBaidu\\";
        string DataProofPath = "\\DataProof\\";
        string DataWeatherPath = "\\DataWeather\\";
        string DataModePath = "\\DataMode\\";
        string DataBilibiliPath = "\\DataBilibili\\";
        string DataRacehorsePath = "\\DataRacehorse\\";

        bool inited = false;
        object dealmsgMutex = new object();
        object savemsgMutex = new object();
        object userblackMutex = new object();

        Dictionary<long, long> userBlacklist = new Dictionary<long, long>();
        Dictionary<long, long> groupBlacklist = new Dictionary<long, long>();
        Dictionary<long, long> groupWhitelist = new Dictionary<long, long>();
        List<string> askname = new List<string>();

        
        BaiduSearchActor baidu = new BaiduSearchActor();
        BeastProofActor proof = new BeastProofActor();
        DiceActor dice = new DiceActor();
        WeatherActor weather = new WeatherActor();
        TranslateActor trans = new TranslateActor();
        BilibiliLiveActor bilibili = new BilibiliLiveActor();
        ModeActor modes = new ModeActor();
        RacehorseActor racehorse = new RacehorseActor();

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
            lock (dealmsgMutex)
            {
                if (!inited)
                {
                    try
                    {
                        if (Directory.Exists(rootDict + historyPath)) Directory.CreateDirectory(rootDict + historyPath);

                        modes.init(rootDict + DataModePath);
                        baidu.init(rootDict + DataBaiduPath);
                        proof.init(rootDict + DataProofPath);
                        weather.init(rootDict + DataWeatherPath);
                        bilibili.init(rootDict + DataBilibiliPath);
                        racehorse.init(sendGroup, getQQNick, rootDict + DataRacehorsePath);

                        userBlacklist = new Dictionary<long, long>();
                        groupBlacklist = new Dictionary<long, long>();
                        groupWhitelist = new Dictionary<long, long>();
                        List<string> userblacklistlines = FileIOActor.readLines(rootDict + userBlacklistFile).ToList();
                        foreach (var line in userblacklistlines)
                        {
                            var uitem = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (uitem.Length >= 2) userBlacklist[long.Parse(uitem[0])] = long.Parse(uitem[1]);
                        }
                        List<string> groupblacklistlines = FileIOActor.readLines(rootDict + groupBlacklistFile).ToList();
                        foreach (var line in groupblacklistlines)
                        {
                            groupBlacklist[long.Parse(line)] = 0;
                        }
                        List<string> groupwhitelistlines = FileIOActor.readLines(rootDict + groupWhitelistFile).ToList();
                        foreach (var line in groupwhitelistlines)
                        {
                            groupWhitelist[long.Parse(line)] = 0;
                        }
                        askname = FileIOActor.readLines(rootDict + asknameFile).ToList();
                        inited = true;
                    }
                    catch (Exception e)
                    {
                        FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                        inited = false;
                    }
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
                    if (userqq != 0 && !userBlacklist.Keys.Contains(userqq))
                    {
                        try
                        {
                            userBlacklist[userqq] = 0;
                            sendGroup(group, user, $"~好，我立即屏蔽{userqq}~");
                            File.AppendAllText(rootDict + "\\ignoreuser.txt", $"{userqq}\t{userBlacklist[userqq]}\r\n");

                        }
                        catch { }
                        return true;
                    }
                }
            }

            bool isGroup = (group <= 0) ? false : true;
            msg = msg.Trim();

            // 模式配置
            if (msg.Contains("模式列表"))
            {
                string modeindexs = modes.printModeList();
                modeindexs += "~输入“xx模式on”即可切换模式~";
                if (isGroup) sendGroup(group, user, modeindexs);
                else sendPrivate(user, modeindexs);
                return true;
            }
            foreach (var mode in modes.modedict.Keys)
            {
                if (msg.Contains($"{mode}模式on"))
                {
                    if (isGroup)
                    {
                        sendGroup(group, 0, $"~苦瓜的{mode}模式启动~");
                        modes.setGroupMode(group, mode);
                    }
                    else
                    {
                        sendPrivate(user, $"~苦瓜的{mode}模式启动~");
                        modes.setUserMode(user, mode);
                    }
                    return true;
                }
            }
            if (msg.Contains("模式on") || msg.StartsWith("模式"))
            {
                string modeindexs = "苦瓜还没有这个模式（小声）";
                if (isGroup) sendGroup(group, user, modeindexs);
                else sendPrivate(user, modeindexs);

                modeindexs = modes.printModeList();
                modeindexs += "~输入“xx模式on”即可切换模式~";
                if (isGroup) sendGroup(group, user, modeindexs);
                else sendPrivate(user, modeindexs);
                return true;
            }

            // 数字论证
            if (msg.StartsWith("数字论证"))
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
            if (msg == "功能" || msg == "设置" || msg == "帮助" || msg == "设定")
            {
                if (isGroup) sendGroup(group, -1, getWelcomeString());
                else sendPrivate(user, getWelcomeString());
                return true;
            }

            // 天气 
            if (msg.EndsWith("天气"))
            {
                msg = msg.Substring(0, msg.Length - 2);
                string daystr = "今天";
                var daystrs = new string[] { "今天", "明天", "大后天", "后天" };
                foreach (var ds in daystrs)
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
                    if (isGroup) sendGroup(group, user, wres);
                    else sendPrivate(user, wres);
                    return true;
                }

            }

            // 翻译
            if (msg.StartsWith("翻译"))
            {
                msg = msg.Substring(2);
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    string transstr = trans.Translation(msg);
                    if (isGroup) sendGroup(group, user, transstr);
                    else sendPrivate(user, transstr);
                    return true;
                }
            }

            // bilibili 功能
            if (msg == "虚拟区谁在播")
            {
                string xnq = bilibili.getLiveNum();
                if (isGroup) sendGroup(group, user, xnq);
                else sendPrivate(user, xnq);
                return true;
            }
            if (msg.Contains("在播吗"))
            {
                string test = msg.Replace("在播吗", "");
                log(test);
                string res = bilibili.getLiveInfo(test);
                if (isGroup) sendGroup(group, user, res);
                else sendPrivate(user, res);
                return true;
            }
            if (msg.Contains("播了吗"))
            {
                string test = msg.Replace("播了吗", "");
                log(test);
                string res = bilibili.getLiveInfo(test);
                if (isGroup) sendGroup(group, user, res);
                else sendPrivate(user, res);
                return true;
            }
            if (msg.StartsWith("设置别名"))
            {
                var items = msg.Replace("设置别名","").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 2)
                {
                    bilibili.setReplaceName(items[0], items[1]);
                    string res = "好";
                    if (isGroup) sendGroup(group, user, res);
                    else sendPrivate(user, res);
                    return true;
                }
            }
            if (msg.StartsWith("设置房间号"))
            {
                var items = msg.Replace("设置房间号", "").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 2)
                {
                    bilibili.setRoomId(items[0], items[1]);
                    string res = "好";
                    if (isGroup) sendGroup(group, user, res);
                    else sendPrivate(user, res);
                    return true;
                }
            }

            // 骰子
            string diceres = dice.getRollString(msg.Trim());
            if (!string.IsNullOrWhiteSpace(diceres))
            {
                if (isGroup) sendGroup(group, user, diceres);
                else sendPrivate(user, diceres);
                return true;
            }

            // 赛马
            if (isGroup && (msg == "赛马介绍" || msg == "赛马玩法" || msg == "赛马说明"))
            {
                sendGroup(group, user, "苦瓜赛🐎游戏介绍：\r\n输入“赛马”开始一局比赛\r\n在比赛开始时会有下注时间，输入x号y可以向x号马下注y元\r\n比赛开始后自动演算，期间不接收指令\r\n其他查询指令包括“个人信息”“富豪榜”“胜率榜”");
                return true;
             }
            if (isGroup && msg == "签到")
            {
                racehorse.dailyAttendance(group, user);
                return true;
            }
            if (isGroup && msg == "学习强国")
            {
                sendGroup(group, user, "*敬请期待*");
                return true;
                //racehorse.dailyAttendance(group, user);
            }
            if (isGroup && (msg == "赛马" || msg== "賽馬"))
            {
                if (racehorse.isAllow(group))
                {
                    int num = 5;
                    racehorse.initMatch(group, num);
                    return true;
                }
                else
                {
                    return true;
                    //sendGroup(group, user, "*由于相关法律法规原因，该功能暂时无法使用*");
                }
            }
            if (isGroup && (msg == "富豪榜" || msg =="富人榜"))
            {
                racehorse.showRichest(group);
                return true;
            }
            if (isGroup && msg == "胜率榜")
            {
                racehorse.showBigWinner(group);
                return true;
            }
            if (isGroup && msg == "穷人榜")
            {
                racehorse.showPoorest(group);
                return true;
            }
            if (isGroup && msg == "败率榜")
            {
                racehorse.showBigLoser(group);
                return true;
            }
            if (isGroup && msg == "求求你借我一点钱")
            {
                sendGroup(group, user, "滚");
                return true;
                //racehorse.addMoney(group, user, 1);
                //string res = "好";
                //if (isGroup) sendGroup(group, user, res);
                //else sendPrivate(user, res);
                //return true;
            }
           
            if (isGroup && msg == "个人信息")
            {
                racehorse.showMyInfo(group, user);
                return true;
            }
            if (isGroup)
            {
                var trygetbet = Regex.Match(msg, @"(\d+)号\s*(\d+)");
                if (trygetbet.Success)
                {
                    try
                    {
                        int roadnum = int.Parse(trygetbet.Groups[1].ToString());
                        int money = int.Parse(trygetbet.Groups[2].ToString());
                        racehorse.addBet(group, user, roadnum, money);
                        return true;
                    }
                    catch
                    {
                    }
                }
                
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
            // 知识图谱功能
            var kganswer = baidu.getKGAnswer(question);
            if (kganswer.Length > 0)
            {
                kganswer = kganswer + modes.getMotionString();
                return kganswer;
            }

            answer = baidu.getZhidaoAnswer(question);
            if (answer.Length > 0)
            {
                msg = answer + "...";
            }
            if (modes.rand.Next(0, 100) > 75 || msg.Length <= 0)
            {
                try
                {
                    var tiebares = baidu.getBaiduTiebaAnswers(question);
                    if (tiebares.Length > 0)
                    {
                        string tiebaanswer = tiebares[modes.rand.Next(0, tiebares.Length)].Trim();
                        msg = tiebaanswer;
                        //sendPrivate(masterQQ, question + "\r\n\r\n" + tiebaanswer);
                    }
                }
                catch (Exception e)
                {
                    FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                }
            }
            if (msg.Length <= 0)
            {
                msg = modes.getSaoHua();
            }

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
                lock (userblackMutex)
                {
                    try
                    {
                        userBlacklist = new Dictionary<long, long>();
                        List<string> userlines = FileIOActor.readLines(rootDict + userBlacklistFile).ToList();
                        foreach (var line in userlines)
                        {
                            var item = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (item.Length >= 2) userBlacklist[long.Parse(item[0])] = long.Parse(item[1]);
                        }
                    }
                    catch (Exception e)
                    {
                        FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                    }
                }
            }

            if (userBlacklist.Keys.Contains(user))
            {
                long lefttime = userBlacklist[user];
                log(lefttime.ToString());
                if (lefttime > 0)
                {
                    userBlacklist[user] = lefttime - 1;
                    log(userBlacklist[user].ToString());
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
            saveMsg(group, user, question.Trim());
            if (!askme(ref question)) return;
            if (!allowuser(user)) return;
            if (dealCmd(group, user, question)) return;

            string msg = "";
            string modeName = modes.getGroupMode(group);
            switch (modeName)
            {
                case "正常": msg += getAnswerNormal(user, question); break;
                case "混沌": msg += modes.getAnswerChaos(user, question); break;
                default: msg += modes.getAnswerWithMode(user, question, modeName); break;
            }
            msg = ItemParser.getHexie(msg);
            sendGroup(group, user, msg);
            saveMsg(group, myQQ, msg.Trim());

        }

        /// <summary>
        /// 处理私聊信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="question"></param>
        public void dealPrivateMsg(long user, string question)
        {
            tryInit();
            saveMsg(0, user, question.Trim());
            if (!allowuser(user)) return;
            if (user == masterQQ)
            {
                // Common.CqApi.SendPrivateMessage(user, "[CQ: rich, url = https://i.y.qq.com/v8/playsong.html?songid=201243685&amp;source=yqq#wechat_redirect,text=来自QQ音乐的分享 《Lightbreaker》]");
            }
            if (dealCmd(0, user, question)) return;

            string msg = "";
            string modeName = modes.getUserMode(user);
            switch (modeName)
            {
                case "正常": msg += getAnswerNormal(user, question); break;
                case "混沌": msg += modes.getAnswerChaos(user, question); break;
                default: msg += modes.getAnswerWithMode(user, question, modeName); break;
            }
            msg = ItemParser.getHexie(msg);
            sendPrivate(user, msg);
            saveMsg(0, user, msg.Trim());
        }

        

        /// <summary>
        /// 记录群/私人聊天信息到文件中
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        void saveMsg(long group, long user, string msg)
        {
            lock (savemsgMutex)
            {
                try
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
                    if (user == myQQ) time += "[me]";
                    if (group <= 0)
                    {
                        // private
                        string ppath = $"{rootDict}{historyPath}\\private\\";
                        if (!Directory.Exists(ppath)) Directory.CreateDirectory(ppath);
                        File.AppendAllText( $"{ppath}{user}.txt", $"{time}\t{msg}\r\n",  Encoding.UTF8 );
                    }
                    else
                    {
                        // group
                        string ppath = $"{rootDict}{historyPath}\\group\\";
                        if (!Directory.Exists(ppath)) Directory.CreateDirectory(ppath);
                        string gfile = $"{ppath}{group}.txt";
                        if (!File.Exists(gfile))
                        {
                            // 第一次入群，主动发一下自我介绍
                            sendGroup(group, -1, getWelcomeString());
                        }
                        File.AppendAllText(gfile, $"{time}\t{user}\t{msg}\r\n", Encoding.UTF8 );
                    }
                }
                catch (Exception e)
                {
                    FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                }
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
                    if (question.StartsWith("，") || question.StartsWith(","))
                    {
                        question = question.Substring(1);
                    }
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

            if (question.Contains(ItemParser.CqCode_At(myQQ)))
            {
                question = question.Replace(ItemParser.CqCode_At(myQQ), "");
                return true;
            }
            question = question.Trim();
            return false;
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
                    mmdk.getQQNick = getQQNick;
                    mmdk.tryInit();
                    
                }
                
            }
            catch { }
        }

        private void sendPrivate(long user, string msg)
        {
            Common.CqApi.SendPrivateMessage(user, msg);
        }

        private string getQQNick(long qq)
        {
            try
            {
               return Common.CqApi.GetQQInfo(qq).Nick;
            }
            catch (Exception e)
            {
                return qq.ToString();
            }
        }

        private void sendGroup(long group, long user, string msg)
        {

            if (user > 0)
            {
                msg = Common.CqApi.CqCode_At(user) + msg;// Common.CqApi.GetMemberInfo(group, user).Nick + " " + msg;// Common.CqApi.CqCode_At(user) + msg;

            }
            for (int i = 0; i < 55; i++)    // 33
            {
                msg = Common.CqApi.CqCode_Face(Sdk.Cqp.Enum.Face.拳头) + msg;
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
