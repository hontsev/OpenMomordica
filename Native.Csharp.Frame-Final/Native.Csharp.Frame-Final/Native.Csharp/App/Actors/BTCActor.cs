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
        string walletf = "btclist.txt";
        string benefitf = "benefitlist.txt";
        string recordf = "record.txt";

        public static string unitName = "比特币";

        public sendQQGroupMsgHandler outputMessage = null;
        public getQQNickHandler getQQNick = null;
        public static Random rand = new Random();
        object btcMutex = new object();
        object recordMutex = new object();
        public Dictionary<long, BTCUser> users = new Dictionary<long, BTCUser>();
        
        public BTCActor()
        {

        }



        public void init(sendQQGroupMsgHandler _outputMessage, getQQNickHandler _getQQNick, string _path)
        {
            outputMessage = _outputMessage;
            getQQNick = _getQQNick;
            path = _path;

            users = new Dictionary<long, BTCUser>();
            // read users
            // wallet
            var lines = FileIOActor.readLines(path + walletf);
            foreach (var line in lines)
            {
                BTCWallet wallet = new BTCWallet(line);
                if (wallet.uid > 0)
                {
                    // success
                    if (!users.ContainsKey(wallet.uid)) users[wallet.uid] = new BTCUser(wallet.uid);
                    users[wallet.uid]._wallet = wallet;
                }
            }
            // benefit
            lines = FileIOActor.readLines(path + benefitf);
            foreach (var line in lines)
            {
                BTCBenefit benefit = new BTCBenefit(line);
                if (benefit.uid > 0)
                {
                    // success
                    if (!users.ContainsKey(benefit.uid)) users[benefit.uid] = new BTCUser(benefit.uid);
                    users[benefit.uid]._benefit = benefit;
                }
            }
        }

        /// <summary>
        /// 获取用户，如果不存在，就为该号码开户
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public BTCUser getUser(long uid)
        {
            if (!users.ContainsKey(uid))
            {
                try
                {
                    users[uid] = new BTCUser(uid);
                }
                catch (Exception ex)
                {
                    FileIOActor.log(ex);
                    users[uid] = null;
                }
            }
            return users[uid];
        }


        void save()
        {
            lock (btcMutex)
            {
                try
                {
                    StringBuilder sbwallet = new StringBuilder();
                    StringBuilder sbbenefit = new StringBuilder();

                    foreach(var user in users.Values)
                    {
                        sbwallet.Append($"{user._wallet.ToString()}\r\n");
                        sbbenefit.Append($"{user._benefit.ToString()}\r\n");
                    }
                    FileIOActor.write(path + walletf, sbwallet.ToString());
                    FileIOActor.write(path + benefitf, sbbenefit.ToString());
                }
                catch (Exception ex)
                {
                    FileIOActor.log(ex);
                }
            }
        }

        /// <summary>
        /// 写资金变动日志
        /// </summary>
        /// <param name="record"></param>
        void writeRecord(BTCRecord record)
        {
            lock (recordMutex)
            {
                try
                {
                    FileIOActor.appendLine(path + recordf, record.ToString());
                }
                catch(Exception ex)
                {
                    FileIOActor.log(ex);
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
            var u = getUser(userqq);
            if (u._benefit.getDailyBenefit())
            {
                int maxmoney = 114;
                int minmoney = 30;
                // success
                long money = rand.Next(minmoney, maxmoney);
                money = u.addMoney(money);

                outputMessage(group, userqq, $"您今日领取失业补助{money}枚{unitName}，现在账上一共{u.Money}枚");
                save();
            }
            else
            {
                outputMessage(group, userqq, $"在？领过了");
            }
        }

        /// <summary>
        /// 转账
        /// </summary>
        /// <param name="fromqq"></param>
        /// <param name="targetqq"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public string transMoney(long fromqq, long targetqq, long money)
        {
            string res = "";
            
            try
            {
                var user1 = getUser(fromqq);
                var user2 = getUser(targetqq);
                if (user1 == null || user2 == null)
                {
                    return $"用户{targetqq}没有{unitName}账户";
                }
                if (money <= 0)
                {
                    return $"只允许正向转账";
                }
                if (user1.Money < money)
                {
                    return $"您的余额不足。当前余额{user1.Money}{unitName}";
                }

                res = $"您向{user2.UserId}发起转账{money}枚{BTCActor.unitName}，";

                
                long realMoney = user2.addMoney(money);

                if (realMoney != money)
                {
                    if (realMoney == 0)
                    {
                        res += $"但对方钱包满了，没转成。";
                    }
                    else
                    {
                        //  上溢
                        res += $"由于对方钱包已满，仅成功转账{realMoney}枚。";
                    }
                }
                user1.addMoney(-1 * realMoney);
                writeRecord(new BTCRecord(fromqq, targetqq, realMoney, "转账", realMoney == money ? "成功" : "成功。达到钱包上限"));
                res += $"余额{user1.Money}{unitName}";
                save();
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
                res += $"转账系统被橄榄了，你钱没了！请带截图联系bot作者";
            }
            return res;
        }

        /// <summary>
        /// 富人榜
        /// </summary>
        /// <returns></returns>
        public string showRichest()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int maxnum = 10;
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    return -1 * left.Money.CompareTo(right.Money);
                });

                sb.Append("富 豪 榜 \r\n");
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{getQQNick(users[i].UserId)},{users[i].Money}枚\r\n");
                }
                return sb.ToString();
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
                return "";
            }
        }

        /// <summary>
        /// 穷人榜
        /// </summary>
        /// <returns></returns>
        public string showPoorest()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int maxnum = 10;
                var users = this.users.Values.ToList();
                users.Sort((left, right) =>
                {
                    return left.Money.CompareTo(right.Money);
                });

                sb.Append("穷 人 榜 \r\n");
                for (int i = 0; i < Math.Min(users.Count, maxnum); i++)
                {
                    sb.Append($"{i + 1}:{getQQNick(users[i].UserId)},{users[i].Money}枚\r\n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
                return "";
            }
        }

        public string getUserInfo(long userqq)
        {
            var u = getUser(userqq);
            return $"您的账上共有{u.Money}枚{unitName}。共领取失业补助{u._benefit.sum}次，今日失业补助{(u._benefit.isTodayAlreadyGet() ? "已领取" : "还未领取")}";
        }
    }

    class BTCWallet
    {
        public long uid = -1;

        /// <summary>
        /// 不要直接操作这个参数，请通过User对象来安全转账
        /// </summary>
        public long money = 0;

        public BTCWallet()
        {
            uid = -1;
            money = 0;
        }

        public BTCWallet(string line)
        {
            parse(line);
        }

        public BTCWallet(long _uid, long _money = 0)
        {
            uid = _uid;
            money = _money;
            //benefit = new Benefit(0, new DateTime(2019, 1, 1));
        }

        public void parse(string line)
        {
            try
            {
                var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 2)
                {
                    uid = long.Parse(items[0]);
                    money = long.Parse(items[1]);
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

        public override string ToString()
        {
            return $"{uid}\t{money}";
        }

    }

    class BTCUser
    {
        long qq = -1;
        object moneyMutex = new object();

        public BTCWallet _wallet = new BTCWallet();
        public BTCBenefit _benefit = new BTCBenefit();

        public long UserId
        {
            get
            {
                return qq;
            }
        }

        public long Money
        {
            get
            {
                return _wallet.money;
            }

            //set
            //{
                
            //}
        }

        /// <summary>
        /// 设置钱数。覆盖原有钱数，请谨慎。
        /// </summary>
        /// <param name="money"></param>
        public void setMoney(long money)
        {
            try
            {
                _wallet.money = money;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

        /// <summary>
        /// 安全修改货币数目
        /// </summary>
        /// <param name="money"></param>
        public long addMoney(long money)
        {
            try
            {
                lock (moneyMutex)
                {
                    long realMoney = money;
                    if (money>=0 && Money >= 0) realMoney = Math.Min(money, long.MaxValue - Money);
                    else if(money<=0 && Money<=0) realMoney = Math.Max(money, long.MinValue - Money);

                    _wallet.money += realMoney;

                    if (realMoney != money)
                    {
                        FileIOActor.log($"{UserId}的{BTCActor.unitName}溢出，{_wallet.money}+{money}");
                    }

                    return realMoney;
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return 0;
        }


        //public BTCUser()
        //{
        //    qq = -1;
        //    wallet = new BTCWallet();
        //    benefit = new BTCBenefit();
        //}

        public BTCUser(long _qq)
        {
            qq = _qq;
            _wallet = new BTCWallet(qq);
            _benefit = new BTCBenefit(qq);
        }

    }


    class BTCBenefit
    {
        public long uid;
        public long sum;
        public DateTime lastGetTime;

        //public int alltime;
        //public int monthtime;
        //public DateTime lastGetBenefitTime;


        public BTCBenefit()
        {
            uid = -1;
            sum = 0;
            lastGetTime = new DateTime(2019, 1, 1);
        }

        public BTCBenefit(long qq)
        {
            uid = qq;
            sum = 0;
            lastGetTime = new DateTime(2019, 1, 1);
        }

        public BTCBenefit(string str)
        {
            parse(str);
        }

        public void parse(string line)
        {
            try
            {
                var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 3)
                {
                    uid = long.Parse(items[0]);
                    sum = long.Parse(items[1]);
                    lastGetTime = Configs.toDateTime(items[2]);// DateTime.ParseExact(items[2], "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

        public override string ToString()
        {
            try
            {
                return $"{uid}\t{sum}\t{Configs.toTimestamp(lastGetTime)}";
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return "";
        }

        public bool isTodayAlreadyGet()
        {
            if (lastGetTime.Day < DateTime.Now.Day
             || lastGetTime.Month < DateTime.Now.Month
             || lastGetTime.Year < DateTime.Now.Year)
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
            try
            {
                if (!isTodayAlreadyGet())
                {
                    // success
                    lastGetTime = DateTime.Now;
                    sum += 1;
                    //monthtime += 1;
                    return true;
                }
                else
                {
                    //showScene(group, user, $"在？领过了");
                    return false;
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
                return false;
            }

        }
    }



    class BTCRecord
    {
        public long src;
        public long tar;
        public DateTime time;
        public long money;
        public string reason;
        public string result;

        public BTCRecord()
        {
            src = -1;
            tar = -1;
            time = DateTime.Now;
            money = 0;
            reason = "";
            result = "";
        }

        public BTCRecord(long _src, long _tar, long _money, string _reason, string _result)
        {
            src = _src;
            tar = _tar;
            money = _money;
            time = DateTime.Now;
            reason = _reason;
            result = _result;
        }

        public BTCRecord(string line)
        {
            parse(line);
        }

        public void parse(string line)
        {
            try
            {
                var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (items.Length >= 6)
                {
                    src = long.Parse(items[0]);
                    tar = long.Parse(items[1]);
                    time = DateTime.ParseExact(items[2], "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    money = long.Parse(items[3]);
                    reason = items[4];
                    result = items[5];
                }
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

        public override string ToString()
        {
            return $"{src}\t{tar}\t{time.ToString("yyyy-MM-dd HH:mm:ss")}\t{money}\t{reason}\t{result}";
        }


    }

}
