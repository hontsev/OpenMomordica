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

        public UserInfo(long _qq, int _money, int _wintime, int _losetime)
        {
            qq = _qq;
            money = _money;
            wintime = _wintime;
            losetime = _losetime;
        }



        public override string ToString()
        {
            return $"{qq}\t{money}\t{wintime}\t{losetime}";
        }

        public double getWinPercent()
        {
            if (wintime + losetime <= 0) return 0;
            return (double)100 * wintime / (double)(wintime + losetime);
        }
    }

    class HorseInfo
    {
        public string emoji;
        public int minspeed;
        public int maxspeed;
        /// <summary>
        ///  触发特效的类型
        ///  0 无
        ///  1 自身加速 - 加速度
        ///  2 自身减速 - 减速度
        ///  3 随机他人加速 - 加速度
        ///  4 随机他人减速 - 减速度
        ///  5 自杀
        ///  6 随机他人杀人
        ///  7 他人全体加速 - 加速度
        ///  8 他人全体减速 - 减速度
        /// </summary>
        public int triggerType;
        public int triggerParam;
        public string triggerEmoji;

        public HorseInfo(string _emoji, int _minspeed, int _maxspeed, int _triggerType, int _triggerParam, string _triggerEmoji)
        {
            emoji = _emoji;
            minspeed = _minspeed;
            maxspeed = _maxspeed;
            triggerType = _triggerType;
            triggerParam = _triggerParam;
            triggerEmoji = _triggerEmoji;
        }

        public override string ToString()
        {
            return $"{emoji}\t{minspeed}\t{maxspeed}\t{triggerType}\t{triggerParam}\t{triggerEmoji}";
        }

        public int getNextStep()
        {
            return RacehorseActor.rand.Next(minspeed, maxspeed);
        }

        public string showName()
        {
            switch (emoji)
            {
                case "🐴":return "普通马";
                case "🐎":return "骏马";
                default:return "不明生物";
            }
        }
    }


    class Road
    {
        public HorseInfo horse;
        public int num;
        public int nowlen;

        public Road(int _num, HorseInfo _horse)
        {
            num = _num;
            horse = _horse;
            nowlen = 0;
        }
    }
    
    class MatchInfo
    {
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

        public void begin(long _id, int _roadnum, int _roadlen, List<HorseInfo> _horses, sendQQGroupMsgHandler handle)
        {
            //horses = _horses;
            showScene = handle;
            id = _id;
            roadnum = _roadnum;
            roadlen = _roadlen;
            roads.Clear();
            bets.Clear();
            //this.horses.Clear();
            initHorses(_horses);
            status = 1;
            nowTime = 0;
        }

        public void initHorses(List<HorseInfo> _horses)
        {
            FileIOActor.log("init horses. horse type " + _horses.Count);
            if (roadnum > 0 && _horses.Count > 0)
            {
                for(int i = 1; i <= roadnum; i++)
                { 
                    roads[i] = new Road(i, _horses[RacehorseActor.rand.Next(_horses.Count)]);
                    FileIOActor.log("road " + i + " horse " + roads[i].horse.emoji);
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
            string res = "";
            if (_money >= _user.money)
            {
                res = $"all in!把手上的{_user.money}枚比特币都下注了{_roadnum}号马";
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
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        public string calBetResult(int winnerroad)
        {
            StringBuilder sb = new StringBuilder();

            int allmoney = 0;
            foreach (var bet in bets.Values) foreach (var money in bet.Values) allmoney += money;
            List<UserInfo> winners = new List<UserInfo>();
            foreach (var bet in bets)
            {
                if (bet.Value.ContainsKey(winnerroad))
                {
                    winners.Add(bet.Key);
                    bet.Key.wintime += 1;
                }
                else bet.Key.losetime += 1;
            }

            if (winners.Count > 0)
            {
                int money = allmoney / winners.Count;
                foreach(var w in winners)
                {
                    w.money += money;
                    sb.Append($"{w.qq}赢了{money}枚比特币！恭喜\r\n");
                }
            }
            else
            {
                sb.Append("很遗憾，本局无人猜中！\r\n");
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
                            s += $"{road.num}号：{road.horse.emoji}{road.horse.showName()}\r\n";
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
                    if (nowTime == 0)
                    {
                        // 输出比赛开始场景，初始化各赛道
                        showScene(id, -1, "赛🐎比赛正式开始！！");
                        showScene(id, -1, getMatchScene());
                    }
                    else if (nowTime >= turnWaitTime)
                    {
                        winnerRoad = 0;
                        int winnerlen = -1;
                        for(int i = 1; i <= roadnum; i++)
                        {
                            var road = roads[i];
                            road.nowlen += road.horse.getNextStep();
                            if(road.nowlen > roadlen && road.nowlen > winnerlen)
                            {
                                winnerRoad = i;
                                winnerlen = road.nowlen;
                            }
                        }
                        showScene(id, -1, getMatchScene());
                        if (winnerRoad > 0)
                        {
                            showScene(id, -1, $"比赛结束！{winnerRoad}号马赢了！");
                            showScene(id, -1, calBetResult(winnerRoad));
                            winnerRoad = -1;
                            status = 3;
                        }
                        nowTime = 0;
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
        string userinfoFile = "userinfo.txt";
        string horseinfoFile = "horseinfo.txt";
        string matchPath = "match\\";
        string path = "";
        public static Random rand = new Random();
        object matchMutex = new object();
        Thread raceLoopThread ;
        bool run = false;

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

        public void init(sendQQGroupMsgHandler _showScene, string path)
        {
            showScene = _showScene;
            this.path = path;
            lock (matchMutex)
            {
                try
                {
                    var lines = FileIOActor.readLines(path + userinfoFile);
                    foreach (var line in lines)
                    {
                        var items = line.Split('\t');
                        if (items.Length >= 4)
                        {
                            userinfo[long.Parse(items[0])] = new UserInfo(
                                long.Parse(items[0]),
                                int.Parse(items[1]),
                                int.Parse(items[2]),
                                int.Parse(items[3])
                            );
                        }
                    }

                    lines = FileIOActor.readLines(path + horseinfoFile);
                    foreach (var line in lines)
                    {
                        var items = line.Split('\t');
                        if (items.Length >= 6)
                        {
                            horseinfo[items[0]] = new HorseInfo(
                                items[0],
                                int.Parse(items[1]),
                                int.Parse(items[2]),
                                int.Parse(items[3]),
                                int.Parse(items[4]),
                                items[5]
                            );
                        }
                    }
                    run = true;
                    raceLoopThread = new Thread(raceLoop);
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
                    FileIOActor.clearFile(path + userinfoFile);
                    foreach (var user in userinfo.Values)
                    {
                        FileIOActor.appendLine(path + userinfoFile, user.ToString());
                    }
                    FileIOActor.clearFile(path + horseinfoFile);
                    foreach (var horse in horseinfo.Values)
                    {
                        FileIOActor.appendLine(path + horseinfoFile, horse.ToString());
                    }
                }
                catch (Exception e)
                {
                    FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                }
            }
        }


        public void initMatch(long group, int num = 5)
        {
            if (!matchinfo.ContainsKey(group)) matchinfo[group] = new MatchInfo();
            matchinfo[group].begin(group, num, 30, horseinfo.Values.ToList(), showScene);
            save();
        }

        public void addBet(long group, long user, int roadnum, int money)
        {
            if (!matchinfo.ContainsKey(group)) matchinfo[group] = new MatchInfo();
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0);
            string res = matchinfo[group].bet(userinfo[user], roadnum, money);
            if (!string.IsNullOrWhiteSpace(res)) showScene(group, user, res);
            save();
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
            sb.Append("胜 率 榜 \r\n");
            for (int i = 0; i < Math.Min(users.Count, 10); i++)
            {
                sb.Append($"{i + 1}:{users[i].qq},{Math.Round(users[i].getWinPercent(),2)}%({users[i].wintime}/{users[i].wintime+users[i].losetime})\r\n");
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
            sb.Append("富 豪 榜 \r\n");
            for(int i = 0; i < Math.Min(users.Count,10); i++)
            {
                sb.Append($"{i + 1}:{users[i].qq},{users[i].money}枚\r\n");
            }
            showScene(group, -1, sb.ToString());
            save();
        }

        public void showMyInfo(long group, long user)
        {
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0);
            var u = userinfo[user];
            showScene(group, user, $"您账户上有{u.money}枚比特币，共下注{u.losetime+u.wintime}场，赢{u.wintime}场，胜率{Math.Round(u.getWinPercent(), 2)}%");
            save();
        }
        
        public void addMoney(long group, long user, int money)
        {
            if (!userinfo.ContainsKey(user)) userinfo[user] = new UserInfo(user, 10, 0, 0);
            userinfo[user].money += money;
            save();
        }
    }
}
