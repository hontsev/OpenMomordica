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
        string weathercodeListName = "weathercode.txt";
        Dictionary<string, string> citycodes = new Dictionary<string, string>();

        public void init(string path)
        {
            try
            {
                citycodes = new Dictionary<string, string>();
                var lines = FileIOActor.readLines(path + weathercodeListName);
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    if (items.Length >= 2) citycodes[items[1]] = items[0];
                }
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }

        }

        string appcode = "70fba3150023486c984b213b296531ce";

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
            string resstr = WebConnectActor.getSecurityData($"https://jisutqybmf.market.alicloudapi.com/weather/query?city={city}", appcode);
            //string resstr = WebConnectActor.getData($"http://jisutqybmf.market.alicloudapi.com/weather/query?city={city}");
            //string resstr = WebConnectActor.getData($"https://www.tianqiapi.com/api/?version={version}&cityid={cityid}&appid=1001&appsecret=5566", Encoding.UTF8);
            FileIOActor.log("getweather ~ " + resstr);
            JObject o = JObject.Parse(resstr);
            if(o["status"].ToString() != "0")
            {
                // not found city
                return "";
            }
            int day = 7;
            WeatherInfo[] infos = new WeatherInfo[7];
            for (int i = 0; i < day; i++)
            {
                try
                {
                    WeatherInfo info = new WeatherInfo();
                    var witem = o["result"]["daily"][i];
                    //FileIOActor.log(witem.ToString());
                    info.date = DateTime.Parse(witem["date"].ToString());
                    info.maxtem = int.Parse(witem["day"]["temphigh"].ToString().Replace("℃",""));
                    info.mintem = int.Parse(witem["night"]["templow"].ToString().Replace("℃", ""));
                    //info.avetem = int.Parse(o["result"]["daily"][i]["temp"].ToString().Replace("℃", ""));
                    info.wea = witem["day"]["weather"].ToString();
                    info.win = new string[2];
                    info.win[0]= witem["night"]["winddirect"].ToString();
                    try
                    {
                        info.win[1] = witem["day"]["winddirect"].ToString();
                    }
                    catch { }
                    info.winspeed = witem["day"]["windpower"].ToString();

                    infos[i] = info;
                }
                catch(Exception e)
                {
                    FileIOActor.log($"{e.Message}\r\n{e.StackTrace}");
                    //return "*NOTFOUNDCITY*";
                    //return $"{e.Message}\r\n{e.StackTrace}";
                }
                
            }
            if ("今天".Contains(daystr))
            {
                return infos[0].getDescription();
            }
            else if ("明天 明日".Contains(daystr))
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

             return infos[0].getDescription();
        }

       
    }
}
