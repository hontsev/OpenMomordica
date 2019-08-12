using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 获取天气信息模块
    /// </summary>
    class WeatherActor
    {
        class WeatherInfo
        {
            public DateTime date;
            public int mintem;
            public int maxtem;
            public int avetem;
            public string wea;
            public string[] win;
            public string winspeed;

            public string getDescription()
            {
                return $"{date.ToString("M月d日ddd")} {wea}，{mintem}~{maxtem}℃，{win[0]}{winspeed}";
            }
        }

        public WeatherActor()
        {

        }

        Dictionary<string, string> citycodes = new Dictionary<string, string>();

        public void init(string citycodePath)
        {
            citycodes = new Dictionary<string, string>();
            var lines = FileIOActor.readTxtList(citycodePath);
            foreach(var line in lines)
            {
                var items = line.Trim().Split('\t');
                if (items.Length >= 2) citycodes[items[1]] = items[0].Trim();
            }
        }

        public string getWeather(string city, string daystr="今天")
        {
            string version = "v1";
            string cityid = "";
            if (citycodes.ContainsKey(city))
            {
                cityid = citycodes[city];
            }
            else
            {
                return "";
            }
            string resstr = WebConnectActor.getData($"https://www.tianqiapi.com/api/?version={version}&cityid={cityid}", Encoding.UTF8);
            JObject o = JObject.Parse(resstr);
            if(o["cityid"].ToString() != cityid)
            {
                // not found city
                return "";
            }
            int day = 7;
            WeatherInfo[] infos = new WeatherInfo[7];
            for(int i = 0; i < day; i++)
            {
                try
                {
                    WeatherInfo info = new WeatherInfo();
                    info.date = DateTime.Parse(o["data"][i]["date"].ToString());
                    info.mintem = int.Parse(o["data"][i]["tem2"].ToString().Replace("℃",""));
                    info.maxtem = int.Parse(o["data"][i]["tem1"].ToString().Replace("℃", ""));
                    info.avetem = int.Parse(o["data"][i]["tem"].ToString().Replace("℃", ""));
                    info.wea = o["data"][i]["wea"].ToString();
                    info.win = new string[2];
                    info.win[0]= o["data"][i]["win"][0].ToString();
                    info.win[1] = o["data"][i]["win"][1].ToString();
                    info.winspeed = o["data"][i]["win_speed"].ToString();

                    infos[i] = info;
                }
                catch(Exception e)
                {
                    return "";
                    //return $"{e.Message}\r\n{e.StackTrace}";
                }
                
            }
            if ("今天".Contains(daystr))
            {
                return infos[0].getDescription();
            }
            else if ("明天".Contains(daystr))
            {
                return infos[1].getDescription();
            }
            else if ("后天".Contains(daystr))
            {
                return infos[2].getDescription();
            }
            else if ("大后天".Contains(daystr))
            {
                return infos[3].getDescription();
            }

            return "";
        }

       
    }
}
