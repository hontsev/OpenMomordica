using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 游戏内单元
    /// </summary>
    class MWItem
    {
        public string name;
        public string desc;
        public ulong level;
        public ulong quality;// 白，绿，蓝，黄，紫，彩


        public MWItem()
        {

        }

        public MWItem(MWItem target)
        {
            name = target.name;
            desc = target.desc;
            level = target.level;
            quality = target.quality;
        }

        public void parse(string line)
        {
            var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length >= 4)
            {
                try
                {
                    name = items[0].Trim();
                    desc = items[1].Trim();
                    level = ulong.Parse(items[2].Trim());
                    quality = ulong.Parse(items[3].Trim());
                }
                catch
                {

                }
            }
        }

        public string getQualityString()
        {
            string res = "";
            for(ulong i = 0; i < quality; i++)
            {
                res += "★";
            }
            return res;
        }

        public uint getValue()
        {
            uint res = 0;

            res = (uint)(5 * quality * level);

            return res;
        }

        public override string ToString()
        {
            return $"{name}\t{desc.Replace("\r\n","\\r\\n")}\t{level}\t{quality}";
        }
    }

    class MWItemPool
    {
        Random rand = new Random();
        List<MWItem> items = new List<MWItem>();
        List<int> pers = new List<int>();

        Dictionary<string, MWItem> allItems = null;

        public string name;
        public string desc;
        public long cost;
        public int maxper = 0;
        
        public MWItemPool(Dictionary<string, MWItem> _allItems)
        {
            allItems = _allItems;
        }

        public void parse(string line)
        {
            var items = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length >= 4)
            {
                name = items[0].Trim();
                desc = items[1].Trim();
                cost = long.Parse(items[2].Trim());
                var pairs = items[3].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var p in pairs)
                {
                    var pitem = p.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pitem.Length >= 2)
                    {
                        try{
                            string name = pitem[0];
                            int per = int.Parse(pitem[1]);
                            if (allItems != null && allItems.ContainsKey(name))
                            {
                                addItem(allItems[name], per);
                            }
                        }
                        catch { }

                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{name}\t{desc}\t{cost}\t");
            for(int i = 0; i < items.Count; i++)
            {
                sb.Append($"{items[i].name},{pers[i]};");
            }
            //sb.Append("\r\n");

            return sb.ToString();
        }

        public void addItem(MWItem _item, int _per)
        {
            items.Add(_item);
            pers.Add(_per);
            maxper += _per;
        }

        /// <summary>
        /// 抽卡
        /// </summary>
        /// <returns></returns>
        public MWItem getItem()
        {
            int thisnum = rand.Next(maxper);

            for(int i = 0; i < items.Count; i++)
            {
                if (thisnum > pers[i])
                {
                    thisnum -= pers[i];
                }
                else
                {
                    return items[i];
                }
            }

            return null;
        }
    }

    class MWUser
    {
        public long userid;
        public List<MWItem> cards=new List<MWItem>();

        public long userGetCardNum = 0;
        public long userSpendMoney = 0;

        //public int cardNum
        //{
        //    get
        //    {
        //        return cards.Count;
        //    }
        //}

        //BTCActor btc = null;
        //BTCUser btcuser = null;
        Dictionary<string, MWItem> allItems;

        public MWUser(long uid, Dictionary<string, MWItem> _allItems)
        {
            userid = uid;
            allItems = _allItems;
           // btc = _btc;
            //btcuser = btc.get(userid);
        }

        public void parse(string line)
        {
            try
            {
                var pitems = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (pitems.Length >= 2)
                {
                    userid = long.Parse(pitems[0].Trim());

                    var mitems = pitems[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var itemstrs in mitems)
                    {
                        var itemm = itemstrs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (itemm.Length >= 2)
                        {
                            string name = itemm[0].Trim();
                            ulong level = ulong.Parse(itemm[1].Trim());
                            if (allItems.ContainsKey(name))
                            {
                                MWItem thiscard = new MWItem(allItems[name]);
                                thiscard.name = name;
                                thiscard.level = level;
                                cards.Add(thiscard);
                            }
                        }
                    }

                    if (pitems.Length >= 3)
                    {
                        try
                        {
                            userGetCardNum = long.Parse(pitems[2].Trim());
                        }
                        catch { }
                    }
                    if (pitems.Length >= 4)
                    {
                        try
                        {
                            userSpendMoney = long.Parse(pitems[3].Trim());
                        }
                        catch { }
                    }
                }
            }
            catch
            {

            }
           
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"{userid}\t");
            for(int i = 0; i < cards.Count; i++)
            {
                sb.Append($"{cards[i].name},{cards[i].level};");
            }
            sb.Append($"\t{userGetCardNum}\t{userSpendMoney}");

            return sb.ToString();
        }

        public void addCard(MWItem card)
        {
            userGetCardNum += 1;
            cards.Add(card);
        }

        public uint deleteCard(MWItem card)
        {
            uint val = (uint)(0.8 * card.getValue());
            if (val <= 0) val = 1;
            cards.Remove(card);
            return val;
        }

        public int getCardNum(string name)
        {
            int num = 0;
            foreach(var card in cards)
            {
                if (card.name == name) num++;
            }
            return num;
        }

        public int getCardNumByQuality(ulong quality)
        {
            int num = 0;
            foreach (var card in cards)
            {
                if (card.quality == quality) num++;
            }
            return num;
        }

        public List<MWItem> getSortCardsByQuality()
        {
            List<MWItem> res = cards.ToList();
            res.Sort((left, right) =>
            {
                return -1 * left.quality.CompareTo(right.quality);
            });
            return res;
        }
        
    }



    /// <summary>
    /// 视频
    /// </summary>
    public class MWVideo
    {
        public string source = "";
        public string name;
        public List<string> frame;
        //public long flen;
        public int fps;
        public int fw;
        public int fh;

        public int nowf;
        public MWVideo()
        {
            source = "";
            name = "";
            frame = new List<string>();
            fps = 15;
            nowf = 0;
        }

        public MWVideo(string _source)
        {
            source = _source;
            name = Path.GetFileNameWithoutExtension(source);
            frame = new List<string>();
            fps = 15;
            nowf = 0;
        }

        public MWVideo(MWVideo mwv)
        {
            source = mwv.source;
            name = mwv.name;
            frame = new List<string>();
            fps = mwv.fps;
            nowf = mwv.nowf;
        }

        public string play()
        {
            if (nowf < 0)
            {
                // broken video.
                return $"PLAY ({source}) ERROR.";
            }
            if (frame.Count == 0)
            {
                // init
                try
                {
                    var tmp = File.ReadAllLines(source, Encoding.UTF8);
                    int fnum = int.Parse(tmp[0]);
                    int fh=(tmp.Length-1)/ fnum;
                    int fw = tmp[1].Length;
                    for(int f = 0; f < fnum; f++)
                    {
                        StringBuilder sb = new StringBuilder();
                        for(int i = 0; i < fh; i++)
                        {
                            sb.Append(tmp[f * fh + i + 1]+"\r\n");
                        }
                        frame.Add(sb.ToString());
                    }
                    nowf = 0;
                }
                catch
                {
                    //source = "";
                    nowf = -1;
                    return "PLAY ERROR!";
                }
            }
            if (nowf >= frame.Count)
            {
                // over.
                return frame.Last();
            }

            return frame[nowf++];
        }

        public bool isEnd()
        {
            return nowf >= frame.Count ;
        }
    }

    /// <summary>
    /// 服务器
    /// </summary>
    class MWServer
    {
        public long group;
        public MWVideo nowvideo = null;
        public List<MWVideo> videos = new List<MWVideo>();
        public MMDKWorldActor world;

        public int loopWait = 0;
        public object videoLock=new object();

        public MWItemPool pool;

        public MWServer(MMDKWorldActor _world, long _group = -1)
        {
            world = _world;
            group = _group;


            // dcard deal
            if (world.pools.Count > 0) pool = world.pools.First().Value;
        }

        public void parse(string line)
        {
            var pitems = line.Trim().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (pitems.Length >= 2)
            {
                group = long.Parse(pitems[0].Trim());
                string poolName = pitems[1].Trim();
                if (world.pools.ContainsKey(poolName))
                {
                    pool = world.pools[poolName];
                }
            }
        }

        public override string ToString()
        {
            return $"{group}\t{pool.name}";
        }

        #region draw cards

        public void getCard(long user)
        {

        }

        #endregion



        #region videos

        public bool playVideo(string videoName)
        {
            if (world.videos.ContainsKey(videoName))
            {
                lock (videoLock)
                {
                    nowvideo = new MWVideo(world.videos[videoName]);
                    return true;
                }
                
            }
            return false;
            //if (videoName)
            //{

            //}
        }

        public void stopVideo()
        {
            lock (videoLock)
            {
                nowvideo = null;
            }
               
        }

        public string getFrame()
        {
            if (nowvideo != null)
            {
                lock (videoLock)
                {
                    if (nowvideo != null)
                    {
                        string res = nowvideo.play();
                        if (nowvideo.isEnd())
                        {
                            nowvideo = null;
                        }
                        return res;
                    }
                }
            }
            return "";
        }

        #endregion
    
    
    }

    class ImageConvertHelper
    {
        private static readonly string grayForMiddle = "圞厵虪虈屭囍囅嚚噩懿嘂茻芔网玆米王立爻爪父卄卝乄川巛巜丷丶　";//"MNHQ&OC?7>!:-;. ";
        //private static  readonly string grayForMax = "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ";

        /// <summary>
        /// 算法2.0
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="colSize"></param>
        /// <param name="rowSize"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Generate(Bitmap bitmap, int colSize, int rowSize)
        {
            StringBuilder result = new StringBuilder();
            int bitmapH = bitmap.Height;
            int bitmapW = bitmap.Width;
            //for(int h = 0; h < colSize; h++)
            //{
            //    for(int w = 0; w < rowSize; w++)
            //    {
            //        int offsetY = bitmapH / rowSize;
            //        int offsetX = bitmapW / colSize;
            //        double averBright = 0;
            //        for (int j = 0; j < rowSize && offsetY + j < bitmapH; j++)
            //        {

            //        }
            //    }
            //}


            for (int h = 0; h < bitmapH / rowSize; h++)
            {
                int offsetY = h * rowSize;
                for (int w = 0; w < bitmapW / colSize; w++)
                {
                    int offsetX = w * colSize;
                    double averBright = 0;
                    for (int j = 0; j < rowSize && offsetY + j < bitmapH; j++)
                    {
                        for (int i = 0; i < colSize && offsetX + i < bitmapW; i++)
                        {
                            Color color = bitmap.GetPixel(offsetX + i, offsetY + j);
                            averBright += (color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                        }
                    }
                    averBright /= (rowSize * colSize);
                    int index = (int)(averBright / (256.0 / (double)grayForMiddle.Length));
                    if (index >= grayForMiddle.Length)
                    {
                        result.Append(grayForMiddle[grayForMiddle.Length - 1]);
                    }
                    else
                    {
                        result.Append(grayForMiddle[index]);
                    }
                }
                result.Append("\r\n");
            }
            return result.ToString();
        }

    }

    class MMDKWorldActor
    {
        public sendQQGroupMsgHandler outputMessage;
        public getQQNickHandler getQQNick;

        string serverPath = "server\\";
        string videoPath = "video\\";
        
        string path = "";
        public static Random rand = new Random();
        object matchMutex = new object();
        public static Thread LoopThread;
        public static bool run = false;

        public int fps = 5;

        public Dictionary<string, MWVideo> videos = new Dictionary<string, MWVideo>();
        public Dictionary<long, MWServer> servers = new Dictionary<long, MWServer>();
        
        public BTCActor btc=new BTCActor();

        // dcards
        object dcardMutex = new object();
        string dcardPath = "dcards\\";
        string cardf = "cards.txt";
        string poolf = "pools.txt";
        string userf = "users.txt";
        string dcardserverf = "servers.txt";
        public Dictionary<string, MWItem> cards = new Dictionary<string, MWItem>();
        public Dictionary<string, MWItemPool> pools = new Dictionary<string, MWItemPool>();
        public Dictionary<long, MWUser> users = new Dictionary<long, MWUser>();

        public void init(sendQQGroupMsgHandler _showScene, getQQNickHandler _getQQNick, BTCActor _btc, string _path)
        {
            outputMessage = _showScene;
            getQQNick = _getQQNick;
            btc = _btc;
            path = _path;

            // init videos;
            var videofiles = Directory.GetFiles(path + videoPath,"*.txt");
            foreach(var vf in videofiles)
            {
                string name = Path.GetFileNameWithoutExtension(vf);
                videos[name] = new MWVideo(vf);
            }

            //LoopThread = new Thread(mainloop);
            //run = true;
            //LoopThread.Start();

            lock (dcardMutex)
            {
                // init cards
                var lines = FileIOActor.readLines(path + dcardPath + cardf, Encoding.UTF8);
                foreach (var line in lines)
                {
                    try
                    {
                        MWItem card = new MWItem();
                        card.parse(line);
                        if(!string.IsNullOrWhiteSpace(card.name)) cards[card.name] = card;
                    }
                    catch (Exception ex)
                    {
                        FileIOActor.log(ex);
                    }
                }

                // init pools
                lines = FileIOActor.readLines(path + dcardPath + poolf, Encoding.UTF8);
                foreach (var line in lines)
                {
                    try
                    {
                        MWItemPool pool = new MWItemPool(cards);
                        pool.parse(line);
                        if (!string.IsNullOrWhiteSpace(pool.name)) pools[pool.name] = pool;
                    }
                    catch(Exception ex)
                    {
                        FileIOActor.log(ex);
                    }
                }

                // init users
                lines = FileIOActor.readLines(path + dcardPath + userf, Encoding.UTF8);
                foreach (var line in lines)
                {
                    try
                    {
                        MWUser user = new MWUser(-1, cards);
                        user.parse(line);
                        if (user.userid > 0) users[user.userid] = user;
                    }
                    catch (Exception ex)
                    {
                        FileIOActor.log(ex);
                    }
                }

                // init servers
                lines = FileIOActor.readLines(path + dcardPath + dcardserverf, Encoding.UTF8);
                foreach (var line in lines)
                {
                    try
                    {
                        MWServer server = new MWServer(this);
                        server.parse(line);
                        if (server.group>0) servers[server.group] = server;
                    }
                    catch (Exception ex)
                    {
                        FileIOActor.log(ex);
                    }
                }
            }
              
        }
        #region video
        public bool videoCmd(long group, string cmd)
        {
            try
            {
                if (!servers.ContainsKey(group)) servers[group] = new MWServer(this, group);
                if (cmd.StartsWith("播放"))
                {
                    cmd = cmd.Substring(2).Trim();
                    if (!servers[group].playVideo(cmd))
                    {
                        outputMessage(group, -1, $"没有名为{cmd}的视频。");
                    }
                    return true;
                }
                if (cmd == "停止")
                {
                    servers[group].stopVideo();
                    outputMessage(group, -1, "已停止播放视频。");
                    return true;
                }
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
            }
            return false;
        }
        #endregion
       
        #region cards
        public string getAllPool()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("可选卡池：\r\n");
            foreach(var p in pools)
            {
                sb.Append($"{p.Key}：【{p.Value.desc}】\r\n");
            }

            return sb.ToString();
        }

        public string getNowPool(MWServer server)
        {
            return $"目前卡池是{server.pool.name} 【{server.pool.desc}】，每抽消耗{server.pool.cost}{BTCActor.unitName}";
        }

        /// <summary>
        /// 保存抽卡结果
        /// </summary>
        public void savePlayData()
        {
            lock (dcardMutex)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var user in users.Values)
                    {
                        sb.Append(user.ToString()+"\r\n");
                    }
                    File.WriteAllText(path + dcardPath + userf, sb.ToString(), Encoding.UTF8);

                    sb = new StringBuilder();
                    foreach (var server in servers.Values)
                    {
                        sb.Append(server.ToString()+"\r\n");
                    }
                    File.WriteAllText(path + dcardPath + dcardserverf, sb.ToString(), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    FileIOActor.log(ex);
                }
            }

        }

        /// <summary>
        /// 保存（新增的）卡片信息
        /// </summary>
        public void saveCardsData()
        {
            lock (dcardMutex)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var pool in pools.Values)
                    {
                        sb.Append(pool.ToString()+"\r\n");
                    }
                    File.WriteAllText(path + dcardPath + poolf, sb.ToString(), Encoding.UTF8);

                    sb = new StringBuilder();
                    foreach (var card in cards.Values)
                    {
                        sb.Append(card.ToString()+"\r\n");
                    }
                    File.WriteAllText(path + dcardPath + cardf, sb.ToString(), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    FileIOActor.log(ex);
                }
            }

        }

        public bool deleteCardsByQuality(MWServer server, MWUser user, ulong maxquality)
        {
            try
            {
                if (maxquality <= 0) return false;
                Dictionary<string, List<MWItem>> deleteItems = new Dictionary<string, List<MWItem>>();
                foreach (var card in user.cards)
                {
                    if (card.quality<=maxquality)
                    {
                        if (!deleteItems.ContainsKey(card.name)) deleteItems[card.name] = new List<MWItem>();
                        deleteItems[card.name].Add(card);
                    }
                }
                uint dmoney = 0;
                string res = "";
                if (deleteItems.Count <= 0)
                {
                    // no cards
                    res = $"你手里没有 {maxquality}星或以下卡片";
                }
                else
                {
                    res = $"你卖掉了";
                    foreach (var itemp in deleteItems)
                    {
                        foreach (var item in itemp.Value)
                        {
                            dmoney += user.deleteCard(item);
                        }
                        res += $"{itemp.Value.Count}张{itemp.Key},";
                    }

                    btc.getUser(user.userid).addMoney(dmoney);
                    res += $"获得{dmoney}{BTCActor.unitName}";
                }
                outputMessage(server.group, user.userid, res.Trim());
                savePlayData();
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
                return false;
            }
            return true;
        }

        public bool deleteCards(MWServer server, MWUser user, string cname, int maxnum)
        {
            try
            {
                if (maxnum <= 0) return false;
                List<MWItem> deleteItems = new List<MWItem>();
                foreach (var card in user.cards)
                {
                    if (card.name == cname)
                    {
                        deleteItems.Add(card);
                        if (deleteItems.Count >= maxnum) break;
                    }
                }
                int dnum = deleteItems.Count;
                uint dmoney = 0;
                foreach (var item in deleteItems)
                {
                    dmoney += user.deleteCard(item);
                }
                
                string res = "";
                if (dnum <= 0)
                {
                    // no cards
                    res = $"你手里没有 {cname} 卡";
                }
                else
                {
                    btc.getUser(user.userid).addMoney(dmoney);
                    if (dnum < maxnum)
                    {
                        res = $"你卖掉了全部{dnum}张 {cname} 卡，获得{dmoney}{BTCActor.unitName}";
                    }
                    else
                    {
                        res = $"你卖掉了{dnum}张 {cname} 卡，获得{dmoney}{BTCActor.unitName}";
                    }
                }
                outputMessage(server.group, user.userid, res.Trim());
                savePlayData();
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
                return false;
            }
            return true;
        }

        public bool getCards(MWServer server, MWUser user, int num = 1)
        {
            try
            {
                if (btc.getUser(user.userid).Money < server.pool.cost * num)
                {
                    outputMessage(server.group, user.userid, 
                        $"没{BTCActor.unitName}抽你🐎呢？{(num == 1 ? "单" : $"{num}连")}抽消耗{server.pool.cost * num}枚{BTCActor.unitName}"
                    );
                }
                else
                {
                    string res = "";
                    if (num == 1)
                    {
                        //单抽
                        var card = server.pool.getItem();
                        btc.getUser(user.userid).addMoney(-1 * server.pool.cost);
                        user.userSpendMoney += server.pool.cost;
                        users[user.userid].addCard(card);
                        res = $"单抽抽到了{card.getQualityString()}{card.name}\r\n“{card.desc}”";
                    }
                    else
                    {
                        // 多抽
                        res = $"{num}连抽结果：\r\n";
                        for (int i = 0; i < num; i++)
                        {
                            var card = server.pool.getItem();
                            btc.getUser(user.userid).addMoney(-1 * server.pool.cost);
                            user.userSpendMoney += server.pool.cost;
                            users[user.userid].addCard(card);
                            res += $"{card.getQualityString()}{card.name} “{card.desc}”\r\n";
                        }
                    }
                    outputMessage(server.group, user.userid, res.Replace("【用户名】", getQQNick(user.userid)).Trim());
                }
                savePlayData();
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
                return false;
            }
            return true;
        }

        public string getDesc()
        {
            return $"苦瓜世界（MMDKWorld）抽卡指令介绍：\r\n" +
                        $"抽卡/单抽：从当前卡池抽取1张卡\r\n" +
                        $"十连/十连抽/10连：从当前卡池一次抽取10张卡（无保底）\r\n" +
                        $"卡池列表：查看一共有哪些卡池\r\n" +
                        $"当前卡池：查看本群现在是什么卡池，以及每抽耗费的{BTCActor.unitName}\r\n" +
                        $"换池xxx：将本群卡池换成xxx\r\n" +
                        $"物品栏：查看已有卡牌\r\n" +
                        $"卖a张b：从物品栏里卖掉a张b类型卡牌，换成少量{BTCActor.unitName}\r\n" +
                        $"卖所有b/卖全部b：从物品栏里卖掉所有b类型卡牌\r\n" +
                        $"卖n星及以下：从物品栏里卖掉所有小于等于n星的卡牌\r\n" +
                        $"加卡 name 3 desc：向【自定义】卡池新增一张卡，名为name，星级为3，描述为desc。参数用空格分割"
            ;
        }

        public bool showMyCards(MWServer server, MWUser user)
        {
            try
            {
                if (user.cards.Count <= 0)
                {
                    outputMessage(server.group, user.userid, $"你还没有卡");
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"你共有{user.cards.Count}张卡");
                    for(ulong i = 10; i > 0; i--)
                    {
                        int n = user.getCardNumByQuality(i);
                        if (n > 0)
                        {
                            sb.Append($"，{user.getCardNumByQuality(i)}张{i}星");
                        }
                    }
                    sb.Append("\r\n");
                    foreach (var card in user.getSortCardsByQuality())
                    {
                        sb.Append($"{card.name}({card.getQualityString()}),");
                    }
                    outputMessage(server.group, user.userid, sb.ToString().Substring(0, sb.Length - 1));
                }
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
                return false;

            }
            return true;
        }

        public string getUserInfo(long uid)
        {
            string res = "";
            try
            {
                if (!users.ContainsKey(uid)) users[uid] = new MWUser(uid, cards);
                var user = users[uid];
                res += $"你在抽卡上花了{user.userSpendMoney}{BTCActor.unitName}，";
                if (user.cards.Count <= 0)
                {
                    res += "你手上现在没有卡。";
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"你共有{user.cards.Count}张卡");
                    for (ulong i = 5; i > 0; i--)
                    {
                        int n = user.getCardNumByQuality(i);
                        if (n > 0)
                        {
                            sb.Append($"，{user.getCardNumByQuality(i)}张{i}星");
                        }
                    }
                    res += sb.ToString() + "。";
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }

            return res;
        }

        public MWItemPool addPool(string name, string desc, long money)
        {
            try
            {
                name = name.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
                desc = desc.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');

                if (pools.ContainsKey(name))
                {
                    // exist.
                    return null;
                }
                if (string.IsNullOrWhiteSpace(name)
                    || name.Length > 30
                    || string.IsNullOrWhiteSpace(desc)
                    || desc.Length > 300
                    || money <= 0)
                {
                    // illegal
                    return null;
                }
                MWItemPool pool = new MWItemPool(cards);
                pool.name = name;
                pool.desc = desc;
                pool.cost = money;

                pools[pool.name] = pool;
                return pool;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return null;
        }

        public MWItem addCard(string name, ulong quality, string desc)
        {
            try
            {
                name = name.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
                desc = desc.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');

                if (cards.ContainsKey(name))
                {
                    // exist.
                    return null;
                }
                if (string.IsNullOrWhiteSpace(name)
                    || name.Length > 30
                    || string.IsNullOrWhiteSpace(desc) 
                    || desc.Length > 300
                    || quality<=0 
                    || quality >= 7)
                {
                    // illegal
                    return null;
                }
                MWItem card = new MWItem();
                card.name = name;
                card.quality = quality;
                card.desc = desc;
                card.level = 1;
                cards[card.name] = card;
                return card;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return null;
        }

        public bool addCardIntoPool(MWItemPool pool,  MWItem card)
        {
            try
            {
                int per = 1;
                if (card.quality > 0 && card.quality <= 7) per += (int)((7 - card.quality) * 5);
                pool.addItem(card, per);
                return true;
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return false;
        }

        public bool dcardCmd(long uid, long group, string cmd)
        {
            try
            {
                if (!servers.ContainsKey(group)) servers[group] = new MWServer(this, group);
                if (!users.ContainsKey(uid)) users[uid] = new MWUser(uid, cards);
                MWServer server = servers[group];
                MWUser user = users[uid];
                
                if (cmd == "查看卡池" || cmd == "卡池" || cmd == "什么池" || cmd == "当前卡池")
                {
                    outputMessage(group, -1, getNowPool(server));
                    return true;
                }
                else if (cmd == "卡池列表" )
                {
                    outputMessage(group, -1, getAllPool());
                    return true;
                }
                else if (cmd == "抽卡介绍")
                {
                    outputMessage(group, -1, getDesc());
                    return true;
                }
                else if (cmd.StartsWith("换池"))
                {
                    cmd = cmd.Substring(2).Trim();
                    
                    if (!pools.ContainsKey(cmd))
                    {
                        outputMessage(group, -1, $"没有名为{cmd}的卡池。");
                        outputMessage(group, -1, getAllPool());
                    }
                    else
                    {
                        server.pool = pools[cmd];
                        outputMessage(group, -1, $"已换为{cmd}卡池。");
                    }
                    outputMessage(group, -1, getNowPool(server));

                    savePlayData();
                    return true;
                }
                if (cmd == "抽卡" || cmd == "单抽")
                {
                    return getCards(server, user, 1);
                }
                else if (cmd == "五连抽" || cmd == "五连" || cmd == "5连")
                {
                    return getCards(server, user, 5);
                }
                else if (cmd == "十连抽" || cmd == "十连" || cmd == "10连")
                {
                    return getCards(server, user, 10);
                }
                else if (cmd == "二十连抽" || cmd == "二十连" || cmd == "20连")
                {
                    return getCards(server, user, 20);
                }
                else if (cmd == "物品栏")
                {
                    return showMyCards(server, user);
                }
                else if(cmd.StartsWith("卖"))
                {
                    if (cmd.StartsWith("卖所有") || cmd.StartsWith("卖全部"))
                    {
                        cmd = cmd.Substring(3).Trim();
                        if (cmd.Length > 0)
                        {
                            return deleteCards(server, user, cmd, int.MaxValue);
                        }
                    }
                    Regex zzs = new Regex("卖(\\d+)张(.+)");
                    var matchzzs = zzs.Match(cmd);
                    if (matchzzs.Success)
                    {
                        try
                        {
                            int num = int.Parse(matchzzs.Groups[1].ToString());
                            string target = matchzzs.Groups[2].ToString();
                            return deleteCards(server, user, target, num);
                        }
                        catch (Exception ex)
                        {
                            FileIOActor.log(ex);
                            return false;
                        }
                    }

                    if (cmd == "卖一星及以下") return deleteCardsByQuality(server, user, 1);
                    if (cmd == "卖二星及以下") return deleteCardsByQuality(server, user, 2);
                    if (cmd == "卖三星及以下") return deleteCardsByQuality(server, user, 3);
                    if (cmd == "卖四星及以下") return deleteCardsByQuality(server, user, 4);
                    if (cmd == "卖五星及以下") return deleteCardsByQuality(server, user, 5);
                    if (cmd == "卖六星及以下") return deleteCardsByQuality(server, user, 6);
                    if (cmd == "卖七星及以下") return deleteCardsByQuality(server, user, 7);

                    zzs = new Regex("卖(\\d+)星及以下");
                    matchzzs = zzs.Match(cmd);
                    if (matchzzs.Success)
                    {
                        try
                        {
                            uint star = uint.Parse(matchzzs.Groups[1].ToString());
                            return deleteCardsByQuality(server, user, star);
                        }
                        catch (Exception ex)
                        {
                            FileIOActor.log(ex);
                            return false;
                        }
                    }
                }
                else if (cmd.StartsWith("加卡"))
                {
                    try
                    {
                        cmd = cmd.Substring(2);
                        var items = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (items.Length >= 3)
                        {
                            long paymoney = 20;
                            if(btc.getUser(uid).Money< paymoney)
                            {
                                outputMessage(group, uid, $"自定义卡片需要花费{paymoney}{BTCActor.unitName}，余额不足。");
                            }
                            else
                            {
                                addPool("自定义", "由用户自行上传的卡组。卡牌稀有度自动根据星级计算。", 15);
                                string name = items[0];
                                ulong quality = ulong.Parse(items[1]);
                                string desc = "";
                                for (int i = 2; i < items.Length; i++) desc += items[i];
                                var newcard = addCard(name, quality, desc);
                                if (newcard!=null)
                                {
                                    // success
                                    if(pools.ContainsKey("自定义") && addCardIntoPool(pools["自定义"], newcard))
                                    {
                                        btc.getUser(uid).addMoney(-1 * paymoney);

                                        outputMessage(group, uid, $"您成功向【自定义】卡池新增1张卡。花费{paymoney}{BTCActor.unitName}");
                                        saveCardsData();
                                    }
                                }
                                else
                                {
                                    outputMessage(group, uid, $"自定义卡片出了问题，可能是卡片重名。（失败不收费）");
                                }
                            }
                            return true;
                        }
                    }
                    catch(Exception ex)
                    {
                        FileIOActor.log(ex);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
            return false;

        }
        #endregion

        public void mainloop()
        {
            while (run)
            {
                long[] serverid = servers.Keys.ToArray();
                foreach (var sid in serverid)
                {
                    try
                    {
                        string res = servers[sid].getFrame();
                        if(!string.IsNullOrWhiteSpace(res))  outputMessage(sid, -1, res);
                    }
                    catch { }
                }
                Thread.Sleep(1000 / fps);
            }

        }
    }
}
