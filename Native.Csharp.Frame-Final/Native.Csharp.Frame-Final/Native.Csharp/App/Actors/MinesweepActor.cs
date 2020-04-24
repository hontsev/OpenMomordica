using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;

namespace Native.Csharp.App.Actors
{
    class MSUserInfo
    {
        public long user;
        public long wintime;
        public long playtime;
        public long mintime;

        public MSUserInfo()
        {
            user = 0;
            wintime = 0;
            playtime = 0;
            mintime = -1;
        }
    }

    class MSItem
    {
        public bool hide = true;
        public bool mine = false;
        public MSItem()
        {
            hide = true;
            mine = false;
        }
    }
    class MSServerInfo
    {
        MSItem[] items;
        public bool running = false;
        public int width;
        public int height;
        public int maxWidth = 100;
        public int maxHeight = 30;
        public DateTime beginTime;
        public DateTime endTime;

        public MSServerInfo()
        {

        }

        public void init(int _width = 10,int _height=10)
        {
            if(!running && _width>0 && _height>0 && _width<maxWidth && _height < maxHeight)
            {
                running = true;
                width = _width;
                height = _height;
                items = new MSItem[width * height];
                
            }

        }

        public string getFrame()
        {
            StringBuilder sb = new StringBuilder();



            return sb.ToString();
        }
    }


    class MinesweepActor
    {
        sendQQGroupMsgHandler outputMessage;

        getQQNickHandler getQQNick;

        BTCActor btc;
        private object matchMutex=new object();

        string path = "";
        string userinfoFile = "userinfo.txt";

        Dictionary<long, MSUserInfo> users=new Dictionary<long, MSUserInfo>();
        Dictionary<long, MSServerInfo> servers = new Dictionary<long, MSServerInfo>();

        public void init(sendQQGroupMsgHandler _showScene, getQQNickHandler _getQQNick, BTCActor _btc, string _path)
        {
            outputMessage = _showScene;
            getQQNick = _getQQNick;
            btc = _btc;
            path = _path;
            //lock (matchMutex)
            //{
            //    try
            //    {
            //        var lines = FileIOActor.readLines(path + userinfoFile);
            //        foreach (var line in lines)
            //        {
            //            var items = line.Split('\t');
            //            if (items.Length >= 4)
            //            {
            //                BTCUser user = btc.get(items[0]);
            //                ruuserinfo[user.qq] = new RHUserInfo(
            //                    user,
            //                    int.Parse(items[1]),
            //                    int.Parse(items[2]),
            //                    int.Parse(items[3])
            //                );
            //            }
            //        }

            //        lines = FileIOActor.readLines(path + horseinfoFile);
            //        foreach (var line in lines)
            //        {
            //            var items = line.Split('\t');
            //            if (items.Length >= 7)
            //            {
            //                horseinfo[items[0]] = new HorseInfo(
            //                    items[0],
            //                    items[1],
            //                    int.Parse(items[2]),
            //                    int.Parse(items[3]),
            //                    int.Parse(items[4]),
            //                    int.Parse(items[5]),
            //                    items[6]
            //                );
            //            }
            //        }
            //        try
            //        {
            //            run = true;
            //            if (raceLoopThread == null) raceLoopThread = new Thread(raceLoop);
            //            raceLoopThread.Start();
            //        }
            //        catch
            //        {

            //        }

            //    }
            //    catch (Exception e)
            //    {
            //        FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            //    }
            //}

        }
    }
}
