using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;

namespace Native.Csharp.App.Actors
{
    class RHUser
    {
        public long userid;
        //public BTCUser user;
        public ulong hrmoney = 0;
        public ulong wintime = 0;
        public ulong losetime = 0;

        public RHUser(long _userid = -1)
        {
            userid = _userid;
            //user = _user;
            //hrmoney = _hrmoney;
            //wintime = _wintime;
            //losetime = _losetime;
        }

        public RHUser(string str)
        {
            parse(str);
        }

        public void parse(string line)
        {
            try
            {
                var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 4)
                {
                    userid = long.Parse(items[0]);
                    hrmoney = ulong.Parse(items[1]);
                    wintime= ulong.Parse(items[2]);
                    losetime = ulong.Parse(items[3]);
                }
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
            }
            
        }
               
        public override string ToString()
        {
            return $"{userid}\t{hrmoney}\t{wintime}\t{losetime}";
        }

        public double getWinPercent()
        {
            if (wintime + losetime <= 0) return 0;
            return (double)100 * wintime / (double)(wintime + losetime);
        }

        public double getLosePercent()
        {
            if (wintime + losetime <= 0) return 0;
            return (double)100 * losetime / (double)(wintime + losetime);
        }

        public ulong getPlayTime()
        {
            return wintime + losetime;
        }
    }

    class RHHorse
    {
        public string emoji = "";
        public string name = "";
        public int minspeed = 0;
        public int maxspeed = 0;
        public int triggerType = 0;
        public int triggerParam = 0;
        public string triggerEmoji = "";

        public RHHorse()
        {
            //name = _name;
            //emoji = _emoji;
            //minspeed = _minspeed;
            //maxspeed = _maxspeed;
            //triggerType = _triggerType;
            //triggerParam = _triggerParam;
            //triggerEmoji = _triggerEmoji;
        }

        public RHHorse(string str)
        {
            parse(str);
        }

        public void parse(string line)
        {
            try
            {
                var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 7)
                {
                    emoji = items[0];
                    name = items[1];
                    minspeed = int.Parse(items[2]);
                    maxspeed = int.Parse(items[3]);
                    triggerType = int.Parse(items[4]);
                    triggerParam = int.Parse(items[5]);
                    triggerEmoji = items[6];
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

        public override string ToString()
        {
            return $"{name}\t{emoji}\t{minspeed}\t{maxspeed}\t{triggerType}\t{triggerParam}\t{triggerEmoji}";
        }

        public int getNextStep()
        {
            return RacehorseActor.rand.Next(minspeed, maxspeed);
        }
    }

    class RHBuff
    {
        public string emoji;
        public int type;
        public int para;
        public int lefttime;
        public int speedAdd = 0;
        
        public RHBuff(string _emoji, int _type, int _para, int _lefttime)
        {
            emoji = _emoji;
            type = _type;
            para = _para;
            lefttime = _lefttime;
        }
    }

    class RHRoad
    {
        public RHHorse horse;
        public int num;
        public int nowlen;
        public RHBuff buff;

        public RHRoad(int _num, RHHorse _horse)
        {
            num = _num;
            horse = _horse;
            nowlen = 0;
        }
    }

    enum RHStatus
    {
        None, Bet, Run, End
    }
    
    class RHMatch
    {
        RacehorseActor ra = null;
       // getQQNickHandler getQQNick;
      //  public sendQQGroupMsgHandler showScene;

        public Dictionary<RHUser, Dictionary<int, long>> bets = new Dictionary<RHUser, Dictionary<int, long>>();
        public Dictionary<int, RHRoad> roads = new Dictionary<int, RHRoad>();

        public long id = -1;  //用qq群号作为比赛唯一标识，避免同一个群同时多局
        public int roadnum = 0;
        public int roadlen = 0;
       // public int maxTurn;
        //public int turn;
        /// <summary>
        /// 比赛状态
        /// 0 未开始
        /// 1 下注中
        /// 2 开赛中
        /// 3 比赛结束
        /// </summary>
        public RHStatus status = RHStatus.None;

        public const int betWaitTime = 30;    // 单位是秒
        public const int turnWaitTime = 3;
        public const int GameoverTime = 1;
        public int nowTime = 0;
        public int winnerRoad = 0;
        string skillDescription = "";

        public RHMatch(long _id, RacehorseActor _ra)
        {
            id = _id;
            ra = _ra;
        }

        public void begin(int _roadnum, int _roadlen)
        {
            //horses = _horses;
            //showScene = handle;
            //getQQNick = getqq;
            roadnum = _roadnum;
            roadlen = _roadlen;
            roads.Clear();
            bets.Clear();
            initHorses(ra.horses.Values.ToList());
            status = RHStatus.Bet;
            nowTime = 0;
            skillDescription = "";
        }

        /// <summary>
        /// 给赛道分配🐎
        /// </summary>
        /// <param name="_horses"></param>
        public void initHorses(List<RHHorse> _horses)
        {
            //FileIOActor.log("init horses. horse type " + _horses.Count);
            if (roadnum > 0 && _horses.Count > 0)
            {
                for(int i = 1; i <= roadnum; i++)
                { 
                    roads[i] = new RHRoad(i, _horses[RacehorseActor.rand.Next(_horses.Count)]);
              //      FileIOActor.log("road " + i + " horse " + roads[i].horse.emoji);
                }
            }
        }

        /// <summary>
        /// 下注
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roadnum"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public string bet(RHUser user, int roadnum, long money)
        {
            try
            {
                int maxbet = 2;
                if (status != RHStatus.Bet || money <= 0) return "";

                var btcuser = ra.btc.getUser(user.userid);
                if (user == null) return "";

                if (roadnum <= 0 || roadnum > this.roadnum) return $"在？没有第{roadnum}条赛道";

                if (btcuser.Money <= 0) return $"一分钱都没有，下你🐎的注呢？";

                if (!bets.ContainsKey(user)) bets[user] = new Dictionary<int, long>();

                if (bets[user].Keys.Count >= maxbet && !bets[user].ContainsKey(roadnum))
                {
                    return $"最多押{maxbet}匹，你已经押了{string.Join("、", bets[user].Keys)}。";
                }
                
                string res = "";
                if (money >= btcuser.Money)
                {
                    money = btcuser.Money;
                    res = $"all in!把手上的{money}枚{BTCActor.unitName}都押了{roadnum}号马";
                }
                else
                {
                    res = $"成功在{roadnum}号马下注{money}枚{BTCActor.unitName}";
                }

                btcuser.addMoney(-1 * money);
                user.hrmoney += (ulong)money;
                if (!bets[user].ContainsKey(roadnum)) bets[user][roadnum] = 0;
                bets[user][roadnum] += money;

                res += $"，账户余额{btcuser.Money}";
                return res;
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
                return $"ERROR:{ex.Message}";
            }
          
        }

        /// <summary>
        /// 当前赛场画面
        /// </summary>
        /// <returns></returns>
        public string getMatchScene()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("🏁\r\n");
            int len = 40;
            for(int i = 1; i <= roadnum; i++)
            {
                sb.Append(i);
                if (i!= winnerRoad)  sb.Append("|");
                int space = (int)(len * (1 - (double)roads[i].nowlen / roadlen));
                if (space > 0) sb.Append(' ', space);
                sb.Append(roads[i].horse.emoji);
                if (roads[i].buff != null) sb.Append(roads[i].buff.emoji);
                sb.Append("\r\n");
            }
            if (!string.IsNullOrWhiteSpace(skillDescription)) sb.Append(skillDescription + "\r\n");
            skillDescription = "";

            return sb.ToString();
        }


        /// <summary>
        /// 计算当前帧的比赛进度
        /// </summary>
        private void nextLoop()
        {
            winnerRoad = 0;
            int winnerlen = -1;

            // clear old buffs
            foreach(var road in roads)
            {
                if (road.Value.buff == null) continue;
                road.Value.buff.lefttime -= 1;
                if(road.Value.buff.lefttime <= 0)
                {
                    road.Value.buff = null;
                }
            }
            // skill test
            if (RacehorseActor.rand.Next(100) < 20)
            {
                // play a skill!
                int skillNum = RacehorseActor.rand.Next(1, roadnum + 1);
                if (roads.ContainsKey(skillNum))
                {
                    var road = roads[skillNum];
                    if (road.horse.triggerType != 0)
                    {
                        switch (road.horse.triggerType)
                        {
                            case 1:
                                // 自身加速
                                skillDescription = $"{road.num}号马突然开始加速！";
                                road.buff = new RHBuff(road.horse.triggerEmoji, road.horse.triggerType, road.horse.triggerParam, 1);
                                road.buff.speedAdd = road.horse.triggerParam;
                                break;
                            case 2:
                                // 第一减速
                                skillDescription = $"{road.num}号马累了！";
                                road.buff = new RHBuff(road.horse.triggerEmoji, road.horse.triggerType, road.horse.triggerParam, 1);
                                road.buff.speedAdd = road.horse.triggerParam;
                                break;
                            default: break;
                        }
                    }
                }
               

            }

            for (int i = 1; i <= roadnum; i++)
            {
                var road = roads[i];
                int oristep = road.horse.getNextStep();
                int addstep = road.buff == null ? 0 : road.buff.speedAdd;
                int realstep;
                realstep = oristep + addstep;
                road.nowlen += realstep;
                if (road.nowlen > roadlen && road.nowlen > winnerlen)
                {
                    winnerRoad = i;
                    winnerlen = road.nowlen;
                }
            }
        }

        /// <summary>
        /// 结算
        /// </summary>
        /// <param name="winnerroad"></param>
        /// <returns></returns>
        public string calBetResult(int winnerroad)
        {
            StringBuilder sb = new StringBuilder();

            long allmoney = 0;
            foreach (var bet in bets.Values) foreach (var money in bet.Values) allmoney += money;
            List<RHUser> winners = new List<RHUser>();
            double pl = RacehorseActor.rand.Next(1000, 6666);
            long othermoneys = 0;
            long winnermoneys = 0;
            foreach (var bet in bets)
            {
                bool win = false;
                foreach(var betpair in bet.Value)
                {
                    if(betpair.Key == winnerroad)
                    {
                        winnermoneys += betpair.Value;
                        winners.Add(bet.Key);
                        win = true;
                    }
                    else
                    {
                        othermoneys += betpair.Value;
                    }
                }
                if (!win)
                {
                    bet.Key.losetime += 1;
                }
            }
            if (winners.Count <= 0)
            {
                sb.Append("很遗憾，本场无人猜中！");
            }
            else
            {
                double bl = othermoneys / winnermoneys + 2;
                foreach (var winner in winners)
                {
                    long money = (long)(Math.Ceiling(bets[winner][winnerroad] * bl)) + 1;
                    var btcuser = ra.btc.getUser(winner.userid);
                    long realMoney = btcuser.addMoney(money);
                    if (realMoney == 0) sb.Append($"{ra.getQQNick(winner.userid)}赢了！恭喜！但可惜他钱包满了，没有新的入账\r\n");
                    else sb.Append($"{ra.getQQNick(winner.userid)}赢了{realMoney}枚{BTCActor.unitName}！恭喜\r\n");
                    winner.wintime += 1;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 进行下一帧
        /// </summary>
        public void run()
        {
            try
            {
                switch (status)
                {
                    case RHStatus.None:
                        //  未开始
                        return;
                    case RHStatus.Bet:
                        // 下注中
                        if (nowTime == 0)
                        {
                            // 输出马的介绍信息
                            string s = "";
                            s += $"现在是赛🐎比赛下注时间，请下注您看好的马（输入赛道对应数字）。比赛将于{betWaitTime}秒后自动开始\r\n";
                            //showScene(id, -1, s);
                            //s = "";
                            foreach (var road in roads.Values)
                            {
                                s += $"{road.num}号：{road.horse.emoji} {road.horse.name}\r\n";
                            }
                            ra.outputMessage(id, -1, s);
                        }
                        else if (nowTime >= betWaitTime)
                        {
                            status = RHStatus.Run;
                            nowTime = 0;
                        }
                        break;
                    case RHStatus.Run:
                        // 比赛中
                        if (nowTime == 1)
                        {
                            // 输出比赛开始场景，初始化各赛道
                            ra.outputMessage(id, -1, "赛🐎比赛正式开始！！");
                            ra.outputMessage(id, -1, getMatchScene());
                        }
                        else if (nowTime >= turnWaitTime)
                        {
                            nextLoop();
                            ra.outputMessage(id, -1, getMatchScene());
                            if (winnerRoad > 0)
                            {
                                status = RHStatus.End;
                                ra.outputMessage(id, -1, $"比赛结束！{winnerRoad}号马赢了！");
                                ra.outputMessage(id, -1, calBetResult(winnerRoad));
                                winnerRoad = -1;
                            }
                            nowTime = 1;
                        }
                        break;
                    case RHStatus.End:
                        // 比赛结束
                        // 重置
                        status = 0;
                        nowTime = 0;
                        break;
                    default:
                        break;
                }
                nowTime += 1;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }

        }

    }

    class RacehorseActor 
    {
        public sendQQGroupMsgHandler outputMessage;
        public getQQNickHandler getQQNick;

        string userinfoFile = "userinfo.txt";
        string horseinfoFile = "horseinfo.txt";
        string matchPath = "match\\";
        string path = "";
        public static Random rand = new Random();
        object matchMutex = new object();
        public static Thread raceLoopThread;
        public static bool run = false;
        public BTCActor btc;

        public Dictionary<long, RHUser> users = new Dictionary<long, RHUser>();
        public Dictionary<string, RHHorse> horses = new Dictionary<string, RHHorse>();
        public Dictionary<long , RHMatch> matches = new Dictionary<long, RHMatch>();

        public TimeSpan raceBegin = new TimeSpan(21, 0, 0);
        public TimeSpan raceEnd = new TimeSpan(23, 0, 0);

        int loopSpanMs = 1000;
        public RacehorseActor()
        {

        }

        /// <summary>
        /// 赛马主循环
        /// </summary>
        public void raceLoop()
        {
            while (run)
            {
                try
                {
                    var matchs = matches.Values.ToArray();
                    for (int i = 0; i < matchs.Length; i++)
                    {
                        var match = matchs[i];
                        try
                        {
                            match.run();
                        }
                        catch (Exception ex)
                        {
                            FileIOActor.log(ex);
                        }
                    }
                    Thread.Sleep(loopSpanMs);
                }
                catch (Exception ex)
                {
                    FileIOActor.log(ex);
                }
            }
        }

        public void init(sendQQGroupMsgHandler _showScene, getQQNickHandler _getQQNick, BTCActor _btc, string _path)
        {
            outputMessage = _showScene;
            getQQNick = _getQQNick;
            btc = _btc;
            path = _path;
            lock (matchMutex)
            {
                try
                {
                    var lines = FileIOActor.readLines(path + userinfoFile);
                    foreach (var line in lines)
                    {
                        RHUser user = new RHUser(line);
                        users[user.userid] = user;
                    }
                    lines = FileIOActor.readLines(path + horseinfoFile);
                    foreach (var line in lines)
                    {
                        RHHorse horse = new RHHorse(line);
                        horses[horse.name] = horse;
                    }
                    run = true;
                    if (raceLoopThread == null) raceLoopThread = new Thread(raceLoop);
                    raceLoopThread.Start();
                }
                catch (Exception e)
                {
                    FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                }
            }

        }

        public void save()
        {
            lock (matchMutex)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    FileIOActor.clearFile(path + userinfoFile);
                    foreach (var user in users.Values)
                    {
                        sb.Append($"{user.ToString()}\r\n");
                    }
                    FileIOActor.appendLine(path + userinfoFile, sb.ToString());
                }
                catch (Exception ex)
                {
                    FileIOActor.log(ex);
                }
            }
        }


        public void initMatch(long group, int num)
        {
            if (!matches.ContainsKey(group)) matches[group] = new RHMatch(group, this);
            if (matches[group].status == RHStatus.None)
            {
                // can restart
                matches[group].begin(num, 100);
            }
        }

        /// <summary>
        /// 下注
        /// </summary>
        /// <param name="matchid"></param>
        /// <param name="user"></param>
        /// <param name="road"></param>
        /// <param name="money"></param>
        public bool addBet(long matchid, RHUser user, int road, long money)
        {
            try
            {
                if (matches.ContainsKey(matchid))
                {
                    string res = matches[matchid].bet(user, road, money);
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        outputMessage(matchid, user.userid, res);
                        return true;
                    }
                }
            }catch(Exception ex)
            {
                FileIOActor.log(ex);
            }
            return false;
        }



        public bool isAllow(long group)
        {
            DateTime time = DateTime.Now;
            DateTime nightRaceBegin = new DateTime(time.Year, time.Month, time.Day) + raceBegin;
            DateTime nightRaceEnd = new DateTime(time.Year, time.Month, time.Day) + raceEnd;

            if (time >= nightRaceBegin && time <= nightRaceEnd)
            {
                return true;
            }

            outputMessage(group, -1, $"夜间赛事起止时间为{nightRaceBegin.ToString("HH:mm")}-{nightRaceEnd.ToString("HH:mm")}");
            return false;
        }



        public string showBigWinner()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    if (left.getWinPercent() < right.getWinPercent())
                        return 1;
                    else if (left.getWinPercent() == right.getWinPercent())
                    {
                        if (left.getPlayTime() < right.getPlayTime()) return 1;
                        else if (left.getPlayTime() > right.getPlayTime()) return -1;
                        else return 0;
                    }
                    else
                        return -1;
                });
                sb.Append("赛 🐎 胜 率 榜 \r\n");
                int showtime = 0;
                int index = 0;
                int maxnum = 10;
                ulong mintime = 5;
                while (showtime < maxnum && index < users.Count)
                {
                    ulong playtime = users[index].wintime + users[index].losetime;
                    if (playtime > mintime)
                    {
                        sb.Append($"{showtime + 1}:{getQQNick(users[index].userid)},{Math.Round(users[index].getWinPercent(), 2)}%({users[index].wintime}/{playtime})\r\n");
                        showtime += 1;
                    }
                    index += 1;
                }
                return sb.ToString();
            }catch(Exception ex)
            {
                FileIOActor.log(ex);
            }
            return "";
            //save();
        }

        public string showBigLoser()
        {
            try
            {
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    if (left.getLosePercent() < right.getLosePercent())
                        return 1;
                    else if (left.getWinPercent() == right.getWinPercent())
                    {
                        if (left.getPlayTime() < right.getPlayTime()) return 1;
                        else if (left.getPlayTime() > right.getPlayTime()) return -1;
                        else return 0;
                    }
                    else
                        return -1;
                });

                StringBuilder sb = new StringBuilder();
                sb.Append("赛 🐎 败 率 榜 \r\n");
                int showtime = 0;
                int index = 0;
                int maxnum = 10;
                ulong mintime = 5;
                while (showtime < maxnum && index < users.Count)
                {
                    ulong playtime = users[index].wintime + users[index].losetime;
                    if (playtime > mintime)
                    {
                        sb.Append($"{showtime + 1}:{getQQNick(users[index].userid)},{Math.Round(users[index].getLosePercent(), 2)}%({users[index].losetime}/{playtime})\r\n");
                        showtime += 1;
                    }
                    index += 1;
                }

                //for (int i = 0; i < Math.Min(users.Count, 10); i++)
                //{
                //    sb.Append($"{i + 1}:{getQQNick(users[i].qq)},{Math.Round(users[i].getLosePercent(), 2)}%({users[i].losetime}/{users[i].wintime + users[i].losetime})\r\n");
                //}
                return sb.ToString();
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return "";

        }


        /// <summary>
        /// 赌狗榜
        /// </summary>
        public string showMostPlayTime()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("赛 🐎 赌 狗 榜 \r\n");

                int maxnum = 10;
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    if (left.getPlayTime() < right.getPlayTime())
                        return 1;
                    else if (left.getPlayTime() == right.getPlayTime())
                        return 0;
                    else
                        return -1;
                });
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{getQQNick(users[i].userid)},赌了{users[i].wintime + users[i].losetime}次\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return "";
        }

        /// <summary>
        /// 个人赌马记录
        /// </summary>
        /// <param name="userqq"></param>
        /// <returns></returns>
        public string getRHInfo(long userqq)
        {
            if (!users.ContainsKey(userqq)) users[userqq] = new RHUser(userqq);
            var u = users[userqq];
            return $"您在赌马上消费过{u.hrmoney}枚{BTCActor.unitName}，共下注{u.losetime + u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%";
            //outputMessage(group, userqq, $"您在赌马上消费过{u.hrmoney}枚{BTCActor.unitName}，共下注{u.losetime+u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%");
        }

        public string getIntroduction()
        {
            return $"赛🐎游戏介绍：\r\n" +
                $"输入“赛马”开始一局比赛\r\n" +
                $"在比赛开始时会有下注时间，输入x号y可以向x号马下注y元\r\n" +
                $"比赛开始后自动演算，比赛期间不接收指令\r\n" +
                $"其他指令包括“签到”“个人信息”“富豪榜”“穷人榜”“胜率榜”“败率榜”“赌狗榜”";
        }

        public bool dealCmd(long uid, long group, string cmd)
        {
            try
            {
                //if (!matches.ContainsKey(group)) matches[group] = new RHMatch(this, group);
                if (!users.ContainsKey(uid)) users[uid] = new RHUser(uid);
                RHUser user = users[uid];

                if (cmd == "赛马介绍" || cmd == "赛马玩法" || cmd == "赛马说明")
                {
                    outputMessage(group, -1, getIntroduction());
                    return true;
                }
                else if (cmd == "赛马")
                {
                     int num = 5;
                     initMatch(group, num);
                     return true;
                }
                else if (cmd == "胜率榜")
                {
                    outputMessage(group, -1, showBigWinner());
                    return true;
                }
                else if (cmd == "败率榜")
                {
                    outputMessage(group, -1, showBigLoser());
                    return true;
                }
                else if (cmd== "赌狗榜")
                {
                    outputMessage(group, -1, showMostPlayTime());
                    return true;
                }
                else
                {
                    var trygetbet = Regex.Match(cmd, @"(\d+)号\s*(\d+)");
                    if (trygetbet.Success)
                    {
                        try
                        {
                            int roadnum = int.Parse(trygetbet.Groups[1].ToString());
                            int money = int.Parse(trygetbet.Groups[2].ToString());
                            addBet(group, user, roadnum, money);
                            return true;
                        }
                        catch(Exception ex)
                        {
                            FileIOActor.log(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return false;
        }
    }
}
