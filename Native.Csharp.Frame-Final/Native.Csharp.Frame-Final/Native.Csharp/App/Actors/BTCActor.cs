using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 金融系统
    /// </summary>
    class BTCActor
    {
        string path = "";
        string userDictName = "btclist.txt";
        string benefitDictName = "benefitlist.txt";
        public static string unitName = "比特币";

        public sendQQGroupMsgHandler outputMessage;
        public getQQNickHandler getQQNick;
        public static Random rand = new Random();
        object btcMutex = new object();
        public Dictionary<string, BTCUser> users = new Dictionary<string, BTCUser>();
        //Dictionary<string, string> benefitDict = new Dictionary<string, string>();

        public BTCActor()
        {

        }

        public static DateTime toDateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        public void init(sendQQGroupMsgHandler _outputMessage, getQQNickHandler _getQQNick, string path)
        {
            outputMessage = _outputMessage;
            getQQNick = _getQQNick;
            this.path = path;
            var lines = FileIOActor.readLines(path + userDictName);
            users = new Dictionary<string, BTCUser>();
            foreach (var line in lines)
            {
                var items = line.Split('\t');
                if (items.Length >= 2)
                {
                    try
                    {
                        BTCUser u = new BTCUser(long.Parse(items[0]), long.Parse(items[1]));
                        users[items[0]]=u;
                    }
                    catch (Exception ex)
                    {
                        FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
            }

            lines = FileIOActor.readLines(path + benefitDictName);
            foreach (var line in lines)
            {
                var items = line.Split('\t');
                if (items.Length >= 3)
                {
                    string qq = items[0];
                    if (users.ContainsKey(qq))
                    {
                        users[qq].benefit = new Benefit(
                            int.Parse(items[1]),
                            toDateTime(items[2])
                        );
                    }
                }
            }
        }

        public BTCUser get(string qq)
        {
            if (!users.ContainsKey(qq))
            {
                try
                {
                    BTCUser u = new BTCUser(long.Parse(qq), 10);
                    users[qq] = u;
                }
                catch
                {
                    users[qq] = null;
                }
            }

            return users[qq];
        }

        public BTCUser get(long qq)
        {
            return get(qq.ToString());
        }

        public void save()
        {
            lock (btcMutex)
            {
                try
                {
                    FileIOActor.clearFile(path + userDictName);
                    foreach (var user in users.Values)
                    {
                        FileIOActor.appendLine(path + userDictName, user.ToString());
                    }

                    FileIOActor.clearFile(path + benefitDictName);
                    foreach (var user in users.Values)
                    {
                        FileIOActor.appendLine(path + benefitDictName, user.ToStringBenefit());
                    }
                }
                catch (Exception e)
                {
                    FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
                }
            }
        }

        /// <summary>
        /// 每日签到，领取低保
        /// </summary>
        /// <param name="group"></param>
        /// <param name="userqq"></param>
        public void dailyAttendance(long group, long userqq)
        {
            var u = get(userqq);
            if(u.benefit.getDailyBenefit())
            {
                // success
                int money = rand.Next(30, 114);
                u.money += money;

                outputMessage(group, userqq, $"您今日领取失业补助{money}枚{unitName}，现在账上一共{u.money}枚");
                save();
            }
            else
            {
                outputMessage(group, userqq, $"在？领过了");
            }
        }

        public void addMoney(long group, long userqq, long money)
        {
            var user = get(userqq);
            user.money += money;
            save();
        }

        public string transMoney(long fromqq, long targetqq, long money)
        {
            var user1 = get(fromqq);
            var user2 = get(targetqq);
            if(user1==null || user2 == null)
            {
                return $"用户{targetqq}没有{unitName}账户";
            }
            if (user1.money < money)
            {
                return $"您的余额不足。当前余额{user1.money}{unitName}";
            }
            try
            {
                user1.money -= money;
                user2.money += money;
                save();
                return $"您向{user2.qq}成功转账{money}元。余额{user1.money}{unitName}";
            }
            catch
            {
                return $"转账系统被橄榄了，你钱没了！请联系bot作者";
            }
        }

        public string getUserInfo(long userqq)
        {
            var u = get(userqq);
            return $"您的账上共有{u.money}枚{unitName}。共领取失业补助{u.benefit.alltime}次，今日失业补助{(u.benefit.isTodayAlreadyGet() ? "已领取" : "还未领取")}";
        }
    }

    class BTCUser
    {
        public long qq;
        public long money;
        public Benefit benefit;


        public BTCUser(long _qq, long _money = 0)
        {
            qq = _qq;
            money = _money;
            benefit = new Benefit(0, new DateTime(2019, 1, 1));
        }



        public override string ToString()
        {
            return $"{qq}\t{money}";
        }

        public string ToStringBenefit()
        {
            return $"{qq}\t{benefit.ToString()}";
        }

    }

    class Benefit
    {
        public int alltime;
        //public int monthtime;
        public DateTime lastGetBenefitTime;

        public Benefit()
        {

        }

        public Benefit(int _alltime, DateTime _lasttime)
        {
            alltime = _alltime;
            //monthtime = _monthtime;
            lastGetBenefitTime = _lasttime;
        }

        public override string ToString()
        {
            return $"{alltime}\t{RacehorseActor.toTimestamp(lastGetBenefitTime)}";
        }

        public bool isTodayAlreadyGet()
        {
            if (lastGetBenefitTime.Day < DateTime.Now.Day
             || lastGetBenefitTime.Month < DateTime.Now.Month
             || lastGetBenefitTime.Year < DateTime.Now.Year)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool getDailyBenefit()
        {
            if (!isTodayAlreadyGet())
            {
                // success
                lastGetBenefitTime = DateTime.Now;
                alltime += 1;
                //monthtime += 1;
                return true;
            }
            else
            {
                //showScene(group, user, $"在？领过了");
                return false;
            }
        }
    }


}
