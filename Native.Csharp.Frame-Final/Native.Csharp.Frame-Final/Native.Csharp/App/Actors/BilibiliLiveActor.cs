using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    class BilibiliLiveActor
    {
        public BilibiliLiveActor()
        {

        }

        public class LiveInfo
        {
            public string uname;
            public int roomid;
            public int uid;
            public int online;
            public string title;
        }

        public string getLiveNum()
        {
            int page = 1;
            string sort = "online";
            int area = 199;
            string url = $"https://api.live.bilibili.com/room/v3/area/getRoomList?platform=web&parent_area_id=1&cate_id=0&area_id={area}&sort_type={sort}&page={page}&page_size=30&tag_version=1";

            string resstr = WebConnectActor.getData(url, Encoding.UTF8);
            JObject jo = JObject.Parse(resstr);
            try
            {
                int sum = int.Parse(jo["data"]["count"].ToString());
                List<LiveInfo> infos = new List<LiveInfo>();
                int num = jo["data"]["list"].Count();
                for(int i = 0; i < num; i++)
                {
                    try
                    {
                        LiveInfo info = new LiveInfo();
                        info.roomid = int.Parse(jo["data"]["list"][i]["roomid"].ToString());
                        info.uid = int.Parse(jo["data"]["list"][i]["uid"].ToString());
                        info.uname = jo["data"]["list"][i]["uname"].ToString();
                        info.online= int.Parse(jo["data"]["list"][i]["online"].ToString());
                        info.title = jo["data"]["list"][i]["title"].ToString();
                        infos.Add(info);
                    }
                    catch { }
                }
                StringBuilder sb = new StringBuilder();

                sb.Append($"虚拟主播区还有{sum}个人播，");
                if (sum > 0)
                {
                    sb.Append($"第一是{infos[0].uname}，{infos[0].online}人气，在播{infos[0].title}");
                }
                else
                {
                    sb.Append("惊了。");
                }

                return sb.ToString();
            }
            catch
            {
                return "";
            }

        }
    }
}
