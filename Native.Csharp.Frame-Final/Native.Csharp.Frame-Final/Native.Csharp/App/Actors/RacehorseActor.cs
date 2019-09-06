using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;

namespace Native.Csharp.App.Actors
{
    class UserInfo
    {
        public long qq;
        public int money;
        public int wintime;
        public int losetime;
        public DateTime timestamp;

        public UserInfo(long _qq, int _money, int _wintime, int _losetime, DateTime _timestamp)
        {
            qq = _qq;
            money = _money;
            wintime = _wintime;
            losetime = _losetime;
            timestamp = _timestamp;
        }



        public override string ToString()
        {
            return $"{qq}\t{money}\t{wintime}\t{losetime}\t{RacehorseActor.toTimestamp(timestamp)}";
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
    }

    class HorseInfo
    {
        public string emoji;
        public string name;
        public int minspeed;
        public int maxspeed;
        public int triggerType;
        public int triggerParam;
        public string triggerEmoji;

        public HorseInfo(string _emoji, string _name, int _minspeed, int _maxspeed, int _triggerType, int _triggerParam, string _triggerEmoji)
        {
            name = _name;
            emoji = _emoji;
            minspeed = _minspeed;
            maxspeed = _maxspeed;
            triggerType = _triggerType;
            triggerParam = _triggerParam;
            triggerEmoji = _triggerEmoji;
        }

        public override string ToString()
        {
            return $"{emoji}\t{name}\t{minspeed}\t{maxspeed}\t{triggerType}\t{triggerParam}\t{triggerEmoji}";
        }

        public int getNextStep()
        {
            return RacehorseActor.rand.Next(minspeed, maxspeed);
        }
    }

    class Buff
    {
        public string emoji;
        public int type;
        public int para;
        public int lefttime;
        public int speedAdd = 0;
        
        public Buff(string _emoji, int _type, int _para, int _lefttime)
        {
            emoji = _emoji;
            type = _type;
            para = _para;
            lefttime = _lefttime;
        }
    }

    class Road
    {
        public HorseInfo horse;
        public int num;
        public int nowlen;
        public Buff buff;

        public Road(int _num, HorseInfo _horse)
        {
            num = _num;
            horse = _horse;
            nowlen = 0;
        }
    }
    
    class MatchInfo
    {
        getQQNickHandler getQQNick;
        public sendQQGroupMsgHandler showScene;

        public Dictionary<UserInfo, Dictionary<int, int>> bets = new Dictionary<UserInfo, Dictionary<int, int>>();
        public Dictionary<int, Road> roads = new Dictionary<int, Road>();

        public long id;  //用qq群号作为比赛唯一标识，避免同一个群同时多局
        public int roadnum;
        public int roadlen;
       // public int maxTurn;
        //public int turn;
        /// <summary>
        /// 比赛状态
        /// 0 未开始
        /// 1 下注中
        /// 2 开赛中
        /// 3 比赛结束
        /// </summary>
        public int status = 0;

        public const int betWaitTime = 30;    // 单位是秒
        public const int turnWaitTime = 5;
        public const int GameoverTime = 1;
        public int nowTime = 0;
        public int winnerRoad = 0;
        string skillDescription = "";

        public void begin(long _id, int _roadnum, int _roadlen, List<HorseInfo> _horses, sendQQGroupMsgHandler handle, getQQNickHandler getqq)
        {
            //horses = _horses;
            showScene = handle;
            getQQNick = getqq;
            id = _id;
            roadnum = _roadnum;
            roadlen = _roadlen;
            roads.Clear();
            bets.Clear();
            initHorses(_horses);
            status = 1;
            nowTime = 0;
            skillDescription = "";
        }

        public void initHorses(List<HorseInfo> _horses)
        {
            //FileIOActor.log("init horses. horse type " + _horses.Count);
            if (roadnum > 0 && _horses.Count > 0)
            {
                for(int i = 1; i <= roadnum; i++)
                { 
                    roads[i] = new Road(i, _horses[RacehorseActor.rand.Next(_horses.Count)]);
              //      FileIOActor.log("road " + i + " horse " + roads[i].horse.emoji);
                }
            }
        }

        public string bet(UserInfo _user, int _roadnum, int _money)
        {
            if (status != 1) return "";
            if (!bets.ContainsKey(_user))
            {
                bets[_user] = new Dictionary<int, int>();
            }
            if(_roadnum<=0 || _roadnum > roadnum)
            {
                return $"在？没有第{_roadnum}条赛道";
            }
            if (_user.money <= 0)
            {
                return $"一分钱都没有，下你🐎的注呢？";
            }
            if(bets[_user].Keys.Count>= 2 && !bets[_user].ContainsKey(_roadnum))
            {
                return $"最多押两匹，你已经下了{string.Join("、", bets[_user].Keys)}。";
            }
            string res = "";
            if (_money >= _user.money)
            {
                res = $"all in!把手上的{_user.money}枚比特币都押了{_roadnum}号马";
                _money = _user.money;
                _user.money = 0;
            }
            else
            {
                _user.money -= _money;
                res = $"成功在{_roadnum}号马下注{_money}枚比特币，账户余额{_user.money}";

            }
            if (!bets[_user].ContainsKey(_roadnum))
            {
                bets[_user][_roadnum] = 0;
            }
            bets[_user][_roadnum] += _money;
            
            return res;
        }

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
                int skillNum = RacehorseActor.rand.Next(0, roads.Count);

                var road = roads[skillNum];
                if (road.horse.triggerType != 0)
                {
                    switch (road.horse.triggerType)
                    {
                        case 1:
                            // 自身加速
                            skillDescription = $"{road.num}号马突然开始加速！";
                            road.buff = new Buff(road.horse.triggerEmoji, road.horse.triggerType, road.horse.triggerParam, 1);
                            road.buff.speedAdd = road.horse.triggerParam;
                            break;
                        case 2:
                            // 第一减速
                            skillDescription = $"{road.num}号马累了！";
                            road.buff = new Buff(road.horse.triggerEmoji, road.horse.triggerType, road.horse.triggerParam, 1);
                            road.buff.speedAdd = road.horse.triggerParam;
                            break;
                        default: break;
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

        public string calBetResult(int winnerroad)
        {
            StringBuilder sb = new StringBuilder();

            int allmoney = 0;
            foreach (var bet in bets.Values) foreach (var money in bet.Values) allmoney += money;
            List<UserInfo> winners = new List<UserInfo>();
            double pl = RacehorseActor.rand.Next(1000, 6666);
            int othermoneys = 0;
            int winnermoneys = 0;
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
                double bl = othermoneys * 0.97 / winnermoneys + 1.2;
                foreach (var winner in winners)
                {
                    int money = (int)(Math.Ceiling(bets[winner][winnerroad] * bl)) + 1;
                    winner.money += money;
                    sb.Append($"{getQQNick(winner.qq)}赢了{money}枚比特币！恭喜\r\n");
                    winner.wintime += 1;
                }
            }
                
            

            return sb.ToString();
        }

        public void run()
        {
            switch (status)
            {
                case 0:
                    //  未开始
                    return;
                case 1:
                    // 下注中
                    if (nowTime == 0)
                    {
                        // 输出马的介绍信息
                        string s = "";
                        s += $"现在是赛🐎比赛下注时间，请下注您看好的马（输入赛道对应数字）。比赛将于{betWaitTime}秒后自动开始\r\n";
                        //showScene(id, -1, s);
                        //s = "";
                        foreach(var road in roads.Values)
                        {
                            s += $"{road.num}号：{road.horse.emoji} {road.horse.name}\r\n";
                        }
                        showScene(id, -1, s);
                    }
                    else if(nowTime >= betWaitTime)
                    {
                        status = 2;
                        nowTime = 0;
                    }
                    break;
                case 2:
                    // 比赛中
                    if (nowTime == 1)
                    {
                        // 输出比赛开始场景，初始化各赛道
                        showScene(id, -1, "赛🐎比赛正式开始！！");
                        showScene(id, -1, getMatchScene());
                    }
                    else if (nowTime >= turnWaitTime)
                    {
                        nextLoop();
                        showScene(id, -1, getMatchScene());
                        if (winnerRoad > 0)
                        {
                            showScene(id, -1, $"比赛结束！{winnerRoad}号马赢了！");
                            showScene(id, -1, calBetResult(winnerRoad));
                            winnerRoad = -1;
                            status = 3;
                        }
                        nowTime = 1;
                    }
                    break;
                case 3:
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

    }

    class RacehorseActor
    {
        public sendQQGroupMsgHandler showScene;
        public getQQNickHandler getQQNick;

        string userinfoFile = "userinfo.txt";
        string horseinfoFile = "horseinfo.txt";
        string matchPath = "match\\";
        string path = "";
        public static Random rand = new Random();
        object matchMutex = new object();
        public static Thread raceLoopThread ;
        public static bool run = false;

        public Dictionary<long, UserInfo> userinfo = new Dictionary<long, UserInfo>();
        public Dictionary<string, HorseInfo> horseinfo = new Dictionary<string, HorseInfo>();
        public Dictionary<long , MatchInfo> matchinfo = new Dictionary<long, MatchInfo>();

        public RacehorseActor()
        {
        }

        public void raceLoop()
        {
            int sleepTime = 1000;
            while(run)
            {
                var matchs = matchinfo.Values;
                foreach (var match in matchs)
                {
                    try
                    {
                        match.run();
                    }
                    catch(Exception e)
                    {
                        FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                    }
                }
                Thread.Sleep(sleepTime);
            }
        }

        public void show()
        {
            
        }

        public void init(sendQQGroupMsgHandler _showScene, getQQNickHandler _getQQNick,string path)
        {
            showScene = _showScene;
            getQQNick = _getQQNick;
            this.path = path;
            lock (matchMutex)
            {
                try
                {
                    var lines = FileIOActor.readLines(path + userinfoFile);
                    foreach (var line in lines)
                    {
                        var items = line.Split('\t');
                        if (items.Length >= 5)
                        {
                            userinfo[long.Parse(items[0])] = new UserInfo(
                                long.Parse(items[0]),
                                int.Parse(items[1]),
                                int.Parse(items[2]),
                                int.Parse(items[3]),
                                toDateTime(items[4])
                            );
                        }
                    }

                    lines = FileIOActor.readLines(path + horseinfoFile);
                    foreach (var line in lines)
                    {
                        var items = line.Split('\t');
                        if (items.Length >= 7)
                        {
                            horseinfo[items[0]] = new HorseInfo(
                                items[0],
                                items[1],
                                int.Parse(items[2]),
                                int.Parse(items[3]),
                                int.Parse(items[4]),
                                int.Parse(items[5]),
                                items[6]
                            );
                        }
                    }
                    try
                    {
                        run = true;
                        if (raceLoopThread == null) raceLoopThread = new Thread(raceLoop);
                        raceLoopThread.Start();
                    }
                    catch
                    {

                    }

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
                    FileIOActor.clearFile(path + userinfoFile);
                    foreach (var user in userinfo.Values)
                    {
                        FileIOActor.appendLine(path + userinfoFile, user.ToString());
                    }
                    //FileIOActor.clearFile(path + horseinfoFile);
                    //foreach (var horse in horseinfo.Values)
                    //{
                    //    FileIOActor.appendLine(path + horseinfoFile, horse.ToString());
                    //}
                }
                catch (Exception e)
                {
                    FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                }
            }
        }


        public void initMatch(long group, int num)
        {
            if (!matchinfo.ContainsKey(group)) matchinfo[group] = new MatchInfo();
            else if (matchinfo[group].status != 0) return;
            matchinfo[group].begin(group, num, 100, horseinfo.Values.ToList(), showScene, getQQNick);
            save();
        }

        public void addBet(long group, long user, int roadnum, int money)
        {
            if (!matchinfo.ContainsKey(group)) return;// matchinfo[group] = new MatchInfo();
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0, new DateTime(2019,1,1));
            string res = matchinfo[group].bet(userinfo[user], roadnum, money);
            if (!string.IsNullOrWhiteSpace(res)) showScene(group, user, res);
            save();
        }

        #region 转换时间为unix时间戳
        /// <summary>
        /// 转换时间为unix时间戳
        /// </summary>
        /// <param name="date">需要传递UTC时间,避免时区误差,例:DataTime.UTCNow</param>
        /// <returns></returns>
        public static double toTimestamp(DateTime date)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan diff = date - dateTimeStart;
            return Math.Floor(diff.TotalSeconds);
        }
        #endregion

        #region 时间戳转换为时间

        public static DateTime toDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        #endregion

        public bool isAllow(long group)
        {
            DateTime time = DateTime.Now;
            DateTime nightRaceBegin = new DateTime(time.Year, time.Month, time.Day, 21, 0, 0);
            DateTime nightRaceEnd = new DateTime(time.Year, time.Month, time.Day, 23, 0, 0);

            if (time >= nightRaceBegin && time <= nightRaceEnd)
            {
                return true;
            }

            if (group == 884550792)
            {
                return true;
            }

            showScene(group, -1, $"夜间赛事起止时间为{nightRaceBegin.ToString("HH:mm")}-{nightRaceEnd.ToString("HH:mm")}");
            return false;
        }

        public void dailyAttendance(long group, long user)
        {
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0, new DateTime(2019, 1, 1));
            var u = userinfo[user];
            if(u.timestamp.Day<DateTime.Now.Day || u.timestamp.Month < DateTime.Now.Month || u.timestamp.Year < DateTime.Now.Year)
            {
                // success
                u.timestamp = DateTime.Now;
                int money = rand.Next(10, 100);
                u.money += money;
                showScene(group, user, $"您今日领取失业补助{money}枚比特币，现在账上一共{u.money}枚");
                save();
            }
            else
            {
                showScene(group, user, $"在？领过了");
            }
        }

        public void showBigWinner(long group)
        {
            var users = userinfo.Values.ToList();
            users.Sort((left, right) =>
            {
                if (left.getWinPercent() < right.getWinPercent())
                    return 1;
                else if (left.getWinPercent() == right.getWinPercent())
                    return 0;
                else
                    return -1;
            });

            StringBuilder sb = new StringBuilder();
            sb.Append("赛 🐎 胜 率 榜 \r\n");
            for (int i = 0; i < Math.Min(users.Count, 10); i++)
            {
                sb.Append($"{i + 1}:{getQQNick(users[i].qq)},{Math.Round(users[i].getWinPercent(),2)}%({users[i].wintime}/{users[i].wintime+users[i].losetime})\r\n");
            }
            showScene(group, -1, sb.ToString());
            save();
        }

        public void showBigLoser(long group)
        {
            var users = userinfo.Values.ToList();
            users.Sort((left, right) =>
            {
                if (left.getLosePercent() < right.getLosePercent())
                    return 1;
                else if (left.getLosePercent() == right.getLosePercent())
                    return 0;
                else
                    return -1;
            });

            StringBuilder sb = new StringBuilder();
            sb.Append("赛 🐎 败 率 榜 \r\n");
            for (int i = 0; i < Math.Min(users.Count, 10); i++)
            {
                sb.Append($"{i + 1}:{getQQNick(users[i].qq)},{Math.Round(users[i].getLosePercent(), 2)}%({users[i].losetime}/{users[i].wintime + users[i].losetime})\r\n");
            }
            showScene(group, -1, sb.ToString());
            save();
        }

        public void showRichest(long group)
        {
            var users = userinfo.Values.ToList();
            users.Sort((left, right) =>
            {
                if (left.money < right.money)
                    return 1;
                else if (left.money == right.money)
                    return 0;
                else
                    return -1;
            });

            StringBuilder sb = new StringBuilder();
            sb.Append("赛 🐎 富 豪 榜 \r\n");
            for(int i = 0; i < Math.Min(users.Count,10); i++)
            {
                sb.Append($"{i + 1}:{getQQNick(users[i].qq)},{users[i].money}枚\r\n");
            }
            showScene(group, -1, sb.ToString());
            save();
        }


        public void showPoorest(long group)
        {
            var users = userinfo.Values.ToList();
            users.Sort((left, right) =>
            {
                if (left.money > right.money)
                    return 1;
                else if (left.money == right.money)
                    return 0;
                else
                    return -1;
            });

            StringBuilder sb = new StringBuilder();
            sb.Append("赛 🐎 穷 人 榜 \r\n");
            for (int i = 0; i < Math.Min(users.Count, 10); i++)
            {
                sb.Append($"{i + 1}:{getQQNick(users[i].qq)},{users[i].money}枚\r\n");
            }
            showScene(group, -1, sb.ToString());
            save();
        }

        public void showMyInfo(long group, long user)
        {
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0, new DateTime(2019, 1, 1));
            var u = userinfo[user];
            showScene(group, user, $"您的账上有{u.money}枚比特币，共下注{u.losetime+u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%");
            save();
        }
        
        public void addMoney(long group, long user, int money)
        {
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0, new DateTime(2019, 1, 1));
            userinfo[user].money += money;
            save();
        }
    }
}
