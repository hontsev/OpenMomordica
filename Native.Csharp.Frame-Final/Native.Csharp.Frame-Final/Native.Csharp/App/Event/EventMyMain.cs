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
    /// bot主要处理类
    /// </summary>
    public class MomordicaMain
    {
        public delegate void sendStringHandler(string str);
        public delegate void sendQQPrivateMsgHandler(long targetUser, string msg);
        public delegate void sendQQGroupMsgHandler(long group, long targetUser, string msg);
        public delegate string getQQNickHandler(long qq);
        public delegate string getQQNickFromGroupHandler(long group, long qq);
        public delegate long getQQNumFromGroupHandler(long group, string nick);
        public delegate long getQQGroupNumber();

        public sendStringHandler log;
        public sendQQPrivateMsgHandler sendPrivate;
        public sendQQGroupMsgHandler sendGroup;
        public getQQNickHandler getQQNick;
        public getQQNickFromGroupHandler getQQNickFromGroup;
        public getQQNumFromGroupHandler getQQNumFromGroup;
        public getQQGroupNumber getQQGroupNum;

        private static MomordicaMain _mmdk;

        public string rootDict;         // 资源根目录
        string historyPath = "_history\\";
        string DataBaiduPath = "\\DataBaidu\\";
        string DataProofPath = "\\DataProof\\";
        string DataWeatherPath = "\\DataWeather\\";
        string DataModePath = "\\DataMode\\";
        string DataBilibiliPath = "\\DataBilibili\\";
        string DataRacehorsePath = "\\DataRacehorse\\";
        string DataBTCPath = "\\DataBTC\\";
        string DataGoogleTransPath = "\\DataGoogleTrans\\";
        string DataDivinationPath = "\\DataDivination\\";

        bool inited = false;
        object dealmsgMutex = new object();
        object savemsgMutex = new object();

        public Configs config = new Configs();
        BaiduSearchActor baidu = new BaiduSearchActor();
        BeastProofActor proof = new BeastProofActor();
        DiceActor dice = new DiceActor();
        WeatherActor weather = new WeatherActor();
        TranslateActor trans = new TranslateActor();
        BilibiliLiveActor bilibili = new BilibiliLiveActor();
        ModeActor modes = new ModeActor();
        RacehorseActor racehorse = new RacehorseActor();
        BTCActor btc = new BTCActor();
        DivinationActor divi = new DivinationActor();

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

                        btc.init(sendGroup, getQQNick, rootDict + DataBTCPath);
                        modes.init(sendGroup, rootDict + DataModePath);
                        baidu.init(rootDict + DataBaiduPath);
                        proof.init(rootDict + DataProofPath);
                        weather.init(rootDict + DataWeatherPath);
                        bilibili.init(rootDict + DataBilibiliPath);
                        racehorse.init(sendGroup, getQQNick, btc, rootDict + DataRacehorsePath);
                        config.init(rootDict + "\\");
                        trans.init(rootDict + DataGoogleTransPath);
                        divi.init(rootDict + DataDivinationPath);

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
            //if (user == config.masterQQ)
            //{
            //    // super admin
            //    if (msg.Contains("remove "))
            //    {
            //        string userstr = msg.Replace("remove ", "").Trim();
            //        //Common.CqApi.AddLoger(Sdk.Cqp.Enum.LogerLevel.Info, "test", userstr);
            //        long userqq = 0;
            //        long.TryParse(userstr, out userqq);
            //        if (userqq != 0 && !userBlacklist.Keys.Contains(userqq))
            //        {
            //            try
            //            {
            //                userBlacklist[userqq] = 0;
            //                sendGroup(group, user, $"~好，我立即屏蔽{userqq}~");
            //                File.AppendAllText(rootDict + "\\ignoreuser.txt", $"{userqq}\t{userBlacklist[userqq]}\r\n");

            //            }
            //            catch { }
            //            return true;
            //        }
            //    }
            //}

            bool isGroup = (group <= 0) ? false : true;
            msg = msg.Trim();
            try
            {
                while (msg.EndsWith("?")) msg = msg.Substring(0, msg.Length - 1);
                while (msg.EndsWith("？")) msg = msg.Substring(0, msg.Length - 1);
            }
            catch { }


            //// 模式配置
            //if (msg.Contains("模式列表"))
            //{
            //    string modeindexs = modes.printModeList();
            //    modeindexs += "~输入“xx模式on”即可切换模式~";
            //    if (isGroup) sendGroup(group, user, modeindexs);
            //    else sendPrivate(user, modeindexs);
            //    return true;
            //}
            //Regex modereg = new Regex("(\\S+)模式\\s*(on|off)", RegexOptions.IgnoreCase);
            //var moderes = modereg.Match(msg);
            //if (moderes.Success)
            //{
            //    try
            //    {
            //        string mode = moderes.Groups[1].ToString();
            //        string swit = moderes.Groups[2].ToString().ToLower();
            //        if (swit == "off") mode = "正常";
            //        if (!modes.modedict.ContainsKey(mode))
            //        {
            //            if (config.groupIs(group, "测试") && (mode == "测试" || mode == "喷人"))
            //            {
            //                // pass
            //            }
            //            else
            //            {
            //                string modeindexs = "还没有这个模式（小声）";
            //                if (isGroup) sendGroup(group, user, modeindexs);
            //                else sendPrivate(user, modeindexs);

            //                modeindexs = modes.printModeList();
            //                modeindexs += "~输入“xx模式on”即可切换模式~";
            //                if (isGroup) sendGroup(group, user, modeindexs);
            //                else sendPrivate(user, modeindexs);
            //                return true;
            //            }
            //        }
            //        if (isGroup)
            //        {
            //            sendGroup(group, 0, $"~的{mode}模式启动~");
            //            modes.setGroupMode(group, mode);
            //        }
            //        else
            //        {
            //            sendPrivate(user, $"~的{mode}模式启动~");
            //            modes.setUserMode(user, mode);
            //        }
            //        return true;
            //    }
            //    catch { }
            //}

            //// 数字论证
            //Regex szlzreg = new Regex("数字论证\\s*(\\S+)");
            //var szlzres = szlzreg.Match(msg);
            //if (szlzres.Success)
            //{
            //    try
            //    {
            //        string lzdata = szlzres.Groups[1].ToString();
            //        string lz1, lz2;
            //        if (!lzdata.Contains("-"))
            //        {
            //            lz1 = lzdata.Trim();
            //            lz2 = "";
            //        }
            //        else
            //        {
            //            lz1 = lzdata.Split('-')[0].Trim();
            //            lz2 = lzdata.Split('-')[1].Trim();
            //        }
            //        bool proofsuccess = proof.getProofString(lz1, lz2);
            //        if (proofsuccess)
            //        {
            //            if (isGroup) sendGroup(group, user, proof.finalproof);
            //            else sendPrivate(user, proof.finalproof);
            //        }
            //        else
            //        {
            //            string resspeak = "论不出来，我紫菜";
            //            if (isGroup) sendGroup(group, user, resspeak);
            //            else sendPrivate(user, resspeak);
            //        }
            //        return true;
            //    }
            //    catch { }

            //}

            // 功能介绍
            if (new string[] { "用法", "介绍", "功能", "选项", "设置", "帮助", "配置", "设定", "菜单" }.Contains(msg))
            {
                if (isGroup) sendGroup(group, -1, getWelcomeString());
                else sendPrivate(user, getWelcomeString());
                return true;
            }

            if (msg.StartsWith("设置") && config.personIs(user, "管理员"))
            {
                string cmd = msg.Substring(2);
                try
                {
                    //if (cmd == "重发欢迎消息")
                    //{
                    //    var groups = Directory.GetFiles(rootDict + historyPath + "group\\", "*.txt");
                    //    foreach(var g in groups)
                    //    {
                    //        if(new FileInfo(g).Length > 1024 * 4)
                    //        {
                    //            long gnum = long.Parse(Path.GetFileNameWithoutExtension(g));
                    //            sendGroup(gnum, -1, getWelcomeString());
                    //        }

                    //    }

                    //    //var users = Directory.GetFiles(rootDict + historyPath + "private\\", "*.txt");
                    //    //foreach (var g in groups)
                    //    //{
                    //    //    long gnum = long.Parse(Path.GetFileNameWithoutExtension(g));
                    //    //    sendGroup(gnum, -1, getWelcomeString());
                    //    //}
                    //}
                }
                catch
                {

                }
            }

            if (msg == "状态" && config.personIs(user,"管理员"))
            {
                string rmsg = "";
                rmsg += $"首次启动时间：{config.startTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - config.startTime).TotalDays.ToString("0.00")}天)\r\n";
                rmsg += $"本次启动时间：{config.thisStartTime.ToString("yyyy-MM-dd HH:mm:ss")}(已运行{(DateTime.Now - config.thisStartTime).TotalDays.ToString("0.00")}天)\r\n";
                rmsg += $"重启次数：{config.beginTimes}次\r\n";
                rmsg += $"加了{getQQGroupNum()}个群\r\n";
                rmsg += $"在群里被乐{ config.playTimeGroup }次\r\n";
                rmsg += $"在私聊被乐{ config.playTimePrivate }次\r\n";
                if (isGroup) rmsg += $"在本群的配置是：{(config.groupLevel.ContainsKey(group) ? string.Join("，", config.groupLevel[group]) : "普通群")}\r\n";
                if (isGroup) rmsg += $"在本群是{ modes.getGroupMode(group)}模式\r\n";
                else rmsg += $"目前是{modes.getUserMode(user)}模式\r\n";

                if (isGroup) sendGroup(group, -1, rmsg);
                else sendPrivate(user, rmsg);
                return true;
            }
            if (msg == "存档" && config.personIs(user, "管理员"))
            {
                btc.save();
                racehorse.save();
                config.save();
                string rmsg = "好，已存档";
                if (isGroup) sendGroup(group, -1, rmsg);
                else sendPrivate(user, rmsg);
                return true;
            }

            if (msg.Contains("拳交"))
            {
                List<string> onMsg = new List<string> { "拳交on", "拳交ON", "开始拳交", "拳交马化腾", "拳交开始", "拳交启动", "拳交开启", "开启拳交" };
                List<string> offMsg = new List<string> { "拳交off", "拳交OFF", "停止拳交", "结束拳交", "拳交停止", "拳交结束", "拳交关闭" };
                List<string> qjusers = new List<string> { "807079241", "3345806534" };
                qjusers.Add(config.masterQQ.ToString());
                string rmsg = "";
                if (onMsg.Contains(msg) && (qjusers.Contains(user.ToString()) || config.groupIs(group, "测试")))
                {
                    config.useGroupMsgBuf = true;
                    rmsg = "开始拳交马化腾";
                }
                else if (offMsg.Contains(msg) && (qjusers.Contains(user.ToString()) || config.groupIs(group, "测试")))
                {
                    config.useGroupMsgBuf = false;
                    rmsg = "不再拳交马化腾";
                }
                if (!string.IsNullOrWhiteSpace(rmsg))
                {
                    if (isGroup) sendGroup(group, -1, rmsg);
                    else sendPrivate(user, rmsg);
                    return true;
                }
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

           

            // bilibili 功能
            Regex bsearchreg = new Regex("(\\S+)区有多少(\\S+)");
            var bseatchres = bsearchreg.Match(msg);
            if (bseatchres.Success)
            {
                try
                {
                    string barea = bseatchres.Groups[1].ToString().Trim() + "区";
                    string btar = bseatchres.Groups[2].ToString().Trim();

                    string res = bilibili.getTitleSearch(barea, btar);

                    if (isGroup) sendGroup(group, user, res);
                    else sendPrivate(user, res);
                    return true;
                }
                catch
                {

                }
            }
            if (msg.EndsWith("区谁在播") || msg.EndsWith("区有谁在播") || msg.EndsWith("区有谁") || msg.EndsWith("区都有谁"))
            {
                string areaname = msg.Substring(0, msg.LastIndexOf('区') + 1);
                string xnq = bilibili.getLiveNum(areaname);
                if (isGroup) sendGroup(group, user, xnq);
                else sendPrivate(user, xnq);
                return true;
            }
            if (msg.EndsWith("区谁最惨"))
            {
                string areaname = msg.Substring(0, msg.LastIndexOf('区') + 1);
                string xnq = bilibili.getPoorLives(areaname);
                if (isGroup) sendGroup(group, user, xnq);
                else sendPrivate(user, xnq);
                return true;
            }
            if (msg.Contains("在播吗") || msg.Contains("播了吗"))
            {
                string test = msg.Replace("在播吗", "").Replace("播了吗", "");
                //log(test);
                string res = bilibili.getLiveInfo(test);
                if (isGroup) sendGroup(group, user, res);
                else sendPrivate(user, res);
                return true;
            }
            if (msg.StartsWith("设置别名"))
            {
                var items = msg.Replace("设置别名", "").Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

            // 随机汉字
            if (msg.StartsWith("随机"))
            {
                msg = msg.Replace("随机", "").Trim();
                int time = 1;
                int num = 1;
                if (msg.Contains("*"))
                {
                    try
                    {
                        var item = msg.Split('*');
                        num = int.Parse(item[0]);
                        time = int.Parse(item[1]);
                    }
                    catch { }
                }
                try
                {
                    num = int.Parse(msg);
                }
                catch { }
                string res = "";
                if (time > 0 && time < 200 && num > 0 && num < 200 && num * time < 1000)
                {
                    res = modes.getRandomCharSentence(time, num);
                }
                else
                {
                    res = "太多了，溢出来了！";
                }
                if (isGroup) sendGroup(group, user, res);
                else sendPrivate(user, res);
                return true;
            }

            // 攻受
            Regex gs = new Regex("(.+)攻(.+)受");
            var matchgs = gs.Match(msg);
            if (matchgs.Success)
            {
                try
                {
                    string res = modes.getGongshou(matchgs.Groups[1].ToString(), matchgs.Groups[2].ToString());
                    if (res.Length > 0)
                    {
                        if (isGroup) sendGroup(group, user, res);
                        else sendPrivate(user, res);
                        return true;
                    }
                }
                catch { }
            }

            //// 谴责
            if (config.groupIs(group, "普通"))
            {
                // 翻译
                Regex transreg = new Regex("(\\S+)译(\\S+)\\s+");
                var transmatch = transreg.Match(msg);
                if (transmatch.Success)
                {
                    string msgyilist = transmatch.Groups[0].ToString().Trim();
                    string msgtar = msg.Substring(msgyilist.Length).Trim();
                    var lists = msgyilist.Split('译');
                    if (lists.Length >= 2 && msgtar.Length > 0)
                    {
                        string res = msgtar;
                        for (int i = 0; i < lists.Length - 1; i++)
                        {
                            res = trans.Translation(res, lists[i + 1], lists[i]);
                        }
                        if (isGroup) sendGroup(group, user, res);
                        else sendPrivate(user, res);
                        return true;
                    }
                }

                //Regex qz = new Regex("(.+)谴责(.+)的(.+)");
                //var matchqz = qz.Match(msg);
                //if (matchqz.Success)
                //{
                //    try
                //    {
                //        string res = modes.getQianze(matchqz.Groups[1].ToString(), matchqz.Groups[2].ToString(), matchqz.Groups[3].ToString());
                //        if (res.Length > 0)
                //        {
                //            if (isGroup) sendGroup(group, user, res);
                //            else sendPrivate(user, res);
                //            return true;
                //        }
                //    }
                //    catch { }
                //}
            }

            if (config.groupIsNot(group, "温和"))
            {
                if (msg.StartsWith("讽刺"))
                {
                    string res = "";
                    try
                    {
                        var items = msg.Substring(2).Trim().Split(new char[] { ',', '，' },StringSplitOptions.RemoveEmptyEntries);
                        if (items.Length >= 1)
                        {
                            Dictionary<string, string> pairs = new Dictionary<string, string>();
                            foreach(var item in items)
                            {
                                var pair = item.Split(new char[] { ':', '：', '=', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                if (pair.Length == 2) pairs[pair[0]] = pair[1];
                            }
                            if (pairs.Count > 0)
                            {
                                res = modes.getJoke(pairs);
                            }
                        }
                    }
                    catch { }
                    if (res.Length > 0)
                    {
                        if (isGroup) sendGroup(group, user, res);
                        else sendPrivate(user, res);
                        return true;
                    }
                }
            }


            // 才八点
            if(msg.StartsWith("现在几点") || msg.StartsWith("几点了"))
            {
                try
                {
                    string res = $"现在是{bilibili.getNowClockCountry(20)}";
                    if (res.Length > 0)
                    {
                        if (isGroup) sendGroup(group, user, res);
                        else sendPrivate(user, res);
                        return true;
                    }
                }
                catch { }
            }

            // BTC货币系统
            if (isGroup && msg == "签到")
            {
                btc.dailyAttendance(group, user);
                //racehorse.dailyAttendance(group, user);
                return true;
            }

            Regex zzs = new Regex("给(.+)转(\\d+)");
            var matchzzs = zzs.Match(msg);
            if (matchzzs.Success)
            {
                try
                {
                    string target = matchzzs.Groups[1].ToString();
                    long targetqq = -1;
                    if (!long.TryParse(target, out targetqq)) targetqq = getQQNumFromGroup(group, target.Trim());
                    string res = "";
                    if (targetqq <= 0)
                    {
                        res = $"群里好像没人叫 {target} ，转账失败。";
                    }
                    else
                    {
                        long money = long.Parse(matchzzs.Groups[2].ToString());
                        res = btc.transMoney(user, targetqq, money);

                    }
                    if (res.Length > 0)
                    {
                        if (isGroup) sendGroup(group, user, res);
                        else sendPrivate(user, res);
                        return true;
                    }
                }
                catch { }
            }


            // 占卜
            if (msg.StartsWith("占卜"))
            {
                try
                {
                    string res = divi.getZhouYi();
                    if (res.Length > 0)
                    {
                        if (isGroup) sendGroup(group, user, res);
                        else sendPrivate(user, res);
                        return true;
                    }
                }
                catch { }
            }

            // 赛马
            if(!config.groupIs(group, "禁赛马"))
            {
                if (isGroup && (msg == "赛马介绍" || msg == "赛马玩法" || msg == "赛马说明"))
                {
                    sendGroup(group, user, "赛🐎游戏介绍：\r\n输入“赛马”开始一局比赛\r\n在比赛开始时会有下注时间，输入x号y可以向x号马下注y元\r\n比赛开始后自动演算，期间不接收指令\r\n其他指令包括“签到”“个人信息”“富豪榜”“穷人榜”“胜率榜”“败率榜”“赌狗榜”");
                    return true;
                }
                if (isGroup && (msg == "赛马" ))
                {
                    if (config.groupIs(group, "测试") || racehorse.isAllow(group))
                    {
                        int num = 5;
                        racehorse.initMatch(group, num);
                        return true;
                    }
                }
                if (isGroup && (msg == "富豪榜" || msg == "富人榜"))
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
                if (isGroup && msg == "赌狗榜")
                {
                    racehorse.showMostPlayTime(group);
                    return true;
                }
            }
            //else
            //{
            //    sendGroup(group, user, "*由于相关法律法规原因，该功能暂时无法使用*");
            //}
            

            if (isGroup && msg == "个人信息")
            {
                string res = $"{btc.getUserInfo(user)}\r\n{racehorse.getRHInfo(group, user)}";
                if (res.Length > 0)
                {
                    if (isGroup) sendGroup(group, user, res);
                    else sendPrivate(user, res);
                    return true;
                }
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

            msg = baidu.getZhidaoAnswer(question);
            if (modes.rand.Next(0, 100) > 85 || msg.Length <= 0)
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
            if (!config.allowuser(user)) return;

            if (dealCmd(group, user, question))
            {
                config.playTimeGroup += 1;
                return;
            }
            config.playTimeGroup += 1;

            string msg = "";
            //string modeName = modes.getGroupMode(group);
            //switch (modeName)
            //{
            //    case "正常": msg += getAnswerNormal(user, question); break;
            //    case "混沌": msg += modes.getAnswerChaos(user, question); break;
            //    case "喷人": msg += modes.getPen(group, user); return; break;
            //    case "测试": msg += modes.getHistoryReact(group, user); return; break;
            //    default: msg += modes.getAnswerWithMode(user, question, modeName); break;
            //}
            msg = ItemParser.getHexie(msg);


            if (string.IsNullOrWhiteSpace(msg)) return;
            sendGroup(group, user, msg);
            saveMsg(group, config.myQQ, msg.Trim());
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
            if (!config.allowuser(user)) return;
            if (user == config.masterQQ)
            {
                // Common.CqApi.SendPrivateMessage(user, "[CQ: rich, url = https://i.y.qq.com/v8/playsong.html?songid=201243685&amp;source=yqq#wechat_redirect,text=来自QQ音乐的分享 《Lightbreaker》]");
            }
            if (dealCmd(0, user, question))
            {
                config.playTimePrivate += 1;
                return;
            }
            config.playTimePrivate += 1;

            string msg = "";
            string modeName = modes.getUserMode(user);
            switch (modeName)
            {
                case "正常": msg += getAnswerNormal(user, question); break;
                case "混沌": msg += modes.getAnswerChaos(user, question); break;
                //case "喷人": msg += modes.getPen(-1, user); return; break;
                //case "测试": msg += modes.getHistoryReact(-1, user); return; break;
                default: msg += modes.getAnswerWithMode(user, question, modeName); break;
            }
            msg = ItemParser.getHexie(msg);

            if (string.IsNullOrWhiteSpace(msg)) return;

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
                    if (user == config.myQQ) time += "[me]";
                    if (group <= 0)
                    {
                        // private
                        string ppath = $"{rootDict}{historyPath}\\private\\";
                        if (!Directory.Exists(ppath)) Directory.CreateDirectory(ppath);
                        File.AppendAllText($"{ppath}{user}.txt", $"{time}\t{msg}\r\n", Encoding.UTF8);
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
                        File.AppendAllText(gfile, $"{time}\t{user}\t{msg}\r\n", Encoding.UTF8);
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
            //Common.CqApi.AddLoger(Sdk.Cqp.Enum.LogerLevel.Info, "name", name);
            if (question.StartsWith(config.askName))
            {
                question = question.Substring(config.askName.Length).Trim();
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


            if (question.Contains(ItemParser.CqCode_At(config.myQQ)))
            {
                question = question.Replace(ItemParser.CqCode_At(config.myQQ), "");
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
            return "用法：\r\n" +
                $"~想在群里使用，就at我或者打字开头加“{config.askName}”，再加内容。私聊乐我的话直接发内容。\r\n" +
                "~以下是常用功能。根据群配置不同，有的功能可能无法提供。\r\n" +
                //"~状态查看：“状态”\r\n" +
                //"~模式更换：“模式列表”、“xx模式on”\r\n" +
                "~掷骰：“rd 成功率”“r3d10 攻击力”\r\n" +
                "~多语翻译：“汉译法译俄 xxxx”\r\n" +
                "~天气预报：“北京明天天气”\r\n" +
                "~B站直播搜索：“绘画区谁在播”“虚拟区有多少B限”“xxx在播吗”\r\n" +
                "~赛马：“赛马介绍”“签到”“个人信息”\r\n" +
                "~生成攻受文：“A攻B受”\r\n" +
                //"~生成谴责：“A谴责B的C”\r\n" +
               // "~生成笑话：“讽刺 本国=A国，好人=甲，坏人=乙，事件=xx”\r\n" +
                "~生成随机汉字：“随机5*4”\r\n" +
                "~周易占卜：“占卜 xxx”\r\n";
        }
    }


    class EventMyMain :
        IReceiveGroupMessage,
        IReceiveFriendMessage,
        IReceiveFriendAddRequest,
        IReceiveAddGroupBeInvitee,
        IReceiveFriendIncrease,
        IReceiveGroupPrivateMessage
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
                if (mmdk == null)
                {
                    mmdk = MomordicaMain.getMomordicaMain();
                    mmdk.config.myQQ = Common.CqApi.GetLoginQQ();
                    mmdk.rootDict = Common.AppDirectory;
                    mmdk.log = log;
                    mmdk.sendGroup = sendGroup;
                    mmdk.sendPrivate = sendPrivate;
                    mmdk.getQQNick = getQQNick;
                    mmdk.getQQNickFromGroup = getQQNickFromGroup;
                    mmdk.getQQNumFromGroup = getQQNumFromGroup;
                    mmdk.getQQGroupNum = getQQGroupNum;
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

        private string getQQNickFromGroup(long group, long qq)
        {
            try
            {
                return Common.CqApi.GetMemberInfo(group, qq).Nick;
            }
            catch (Exception e)
            {
                return qq.ToString();
            }
        }

        private long getQQNumFromGroup(long group, string nick)
        {
            try
            {
                var list = Common.CqApi.GetMemberList(group);
                foreach(var line in list)
                {
                    //log($"user:{line.QQId}  {line.Nick}  {line.Level}");
                    if (line.Nick.Trim().ToUpper() == nick.Trim().ToUpper()) return line.QQId;
                }
                return -1;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        private long getQQGroupNum()
        {
            try
            {
                return Common.CqApi.GetGroupList().Count;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private void sendGroup(long group, long user, string msg)
        {

            if (user > 0)
            {
                msg = Common.CqApi.CqCode_At(user) + msg;// Common.CqApi.GetMemberInfo(group, user).Nick + " " + msg;// Common.CqApi.CqCode_At(user) + msg;

            }
            if (mmdk.config.useGroupMsgBuf)
            {
                msg = "\r\n" + msg;
                for (int i = 0; i < 54; i++)    // 33  54
                {
                    msg = Common.CqApi.CqCode_Face(Sdk.Cqp.Enum.Face.拳头) + msg;
                }
            }
            Common.CqApi.SendGroupMessage(group, msg);
        }

        public void ReceiveGroupMessage(object sender, CqGroupMessageEventArgs e)
        {
            tryInit();
            try
            {
                mmdk.dealGroupMsg(e.FromGroup, e.FromQQ, e.Message);
            }
            catch (Exception ex)
            {
                sendPrivate(mmdk.config.masterQQ, ex.Message + "\r\n" + ex.StackTrace);
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
                sendPrivate(mmdk.config.masterQQ, ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void ReceiveAddGroupBeInvitee(object sender, CqAddGroupRequestEventArgs e)
        {
            Common.CqApi.SetGroupAddRequest(e.ResponseFlag, Sdk.Cqp.Enum.RequestType.GroupInvitation, Sdk.Cqp.Enum.ResponseType.PASS, "");
        }

        public void ReceiveFriendAddRequest(object sender, CqAddFriendRequestEventArgs e)
        {
            Common.CqApi.SetFriendAddRequest("", Sdk.Cqp.Enum.ResponseType.PASS);
        }

        public void ReceiveFriendIncrease(object sender, CqFriendIncreaseEventArgs e)
        {
            //sendPrivate(e.FromQQ, mmdk.getWelcomeString());
        }

        public void ReceiveGroupPrivateMessage(object sender, CqPrivateMessageEventArgs e)
        {
            tryInit();
            try
            {
                mmdk.dealPrivateMsg(e.FromQQ, e.Message);
            }
            catch (Exception ex)
            {
                sendPrivate(mmdk.config.masterQQ, ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
