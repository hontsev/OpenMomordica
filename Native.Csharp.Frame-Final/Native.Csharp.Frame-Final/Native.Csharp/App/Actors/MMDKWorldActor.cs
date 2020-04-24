using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Native.Csharp.App.Event.MomordicaMain;
using System.IO;
using System.Drawing;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 游戏内单元
    /// </summary>
    public class MWItem
    {

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

        public MWServer(MMDKWorldActor _world, long _group)
        {
            world = _world;
            group = _group;


        }

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

        //Dictionary<string,>
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
        }

        public bool cmd(long group, string cmd)
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
                FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
            }
            return false;
            
        }

        public void dealImg()
        {

        }

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
