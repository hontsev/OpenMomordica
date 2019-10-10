using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    public class Configs
    {
        public long myQQ;                // bot的qq

        public long masterQQ;           // 主人的qq，可能响应特殊指令，并私发一些调试消息
        public long testGroup;        //  测试用群，会拥有一些调试用的权限
        public bool useGroupMsgBuf = false;        // 如果bot的qq号被腾讯限制群聊，可以尝试用这个模式突破之
        public DateTime startTime;
        public string startTimeString;
        public long beginTimes;
        public DateTime thisStartTime = DateTime.Now;
        public long playTimePrivate = 0;
        public long playTimeGroup = 0;
        public long errTime = 0;


        public string filepath;
        

        public Configs()
        {

        }

        public void init(string filePath)
        {
            filepath = filePath;
            Dictionary<string, string> configs = new Dictionary<string, string>();
            try
            {
                List<string> configlines = FileIOActor.readLines(filepath).ToList();
                foreach (var line in configlines)
                {
                    var item = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (item.Length == 2)
                    {
                        configs[item[0]] = item[1];
                    }
                }
                if (configs.ContainsKey("master")) masterQQ = long.Parse(configs["master"]);
                if (configs.ContainsKey("testgroup")) testGroup = long.Parse(configs["testgroup"]);
                if (configs.ContainsKey("groupmsgbuff")) useGroupMsgBuf = configs["groupmsgbuff"] == "1" ? true : false;
                if (configs.ContainsKey("starttime"))
                {
                    startTime = DateTime.ParseExact(configs["starttime"], "yyyy-MM-dd hh:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                    startTimeString = configs["starttime"].Trim();
                }
                if (configs.ContainsKey("startnum")) beginTimes = long.Parse(configs["startnum"]) + 1;
                if (configs.ContainsKey("playtimeprivate")) playTimePrivate = long.Parse(configs["playtimeprivate"]);
                if (configs.ContainsKey("playtimegroup")) playTimeGroup = long.Parse(configs["playtimegroup"]);
                if (configs.ContainsKey("errtime")) errTime = long.Parse(configs["errtime"]);

            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public void save()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"master={masterQQ}");
                sb.AppendLine($"testgroup={testGroup}");
                sb.AppendLine($"groupmsgbuff={(useGroupMsgBuf?1:0)}");
                sb.AppendLine($"starttime={startTimeString}");
                sb.AppendLine($"startnum={beginTimes}");
                sb.AppendLine($"playtimeprivate={playTimePrivate}");
                sb.AppendLine($"playtimegroup={playTimeGroup}");
                sb.AppendLine($"errtime={errTime}");
                FileIOActor.write(filepath, sb.ToString());
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }
    }

    class CmdActor
    {

        public CmdActor()
        {

        }

        public void dealCmd(string str)
        {

        }
    }
}
