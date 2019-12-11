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
        public bool useGroupMsgBuf = false;        // 如果bot的qq号被腾讯限制群聊，可以尝试用这个模式突破之
        public DateTime startTime;
        public string startTimeString;
        public long beginTimes;
        public DateTime thisStartTime = DateTime.Now;
        public long playTimePrivate = 0;
        public long playTimeGroup = 0;
        public long errTime = 0;
        public string askName;


        public string path;

        public static string configFile = "config.txt";
        public static string groupLevelListFile = "level_group.txt";
        public static string personLevelListFile = "level_person.txt";
        public Dictionary<long, List<string>> groupLevel;
        public Dictionary<long, List<string>> personLevel;

        public Configs()
        {

        }

        public void init(string path)
        {
            this.path = path;
            Dictionary<string, string> configs = new Dictionary<string, string>();
            try
            {
                List<string> configlines = FileIOActor.readLines(path + configFile).ToList();
                foreach (var line in configlines)
                {
                    var item = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (item.Length == 2)
                    {
                        configs[item[0]] = item[1];
                    }
                }
                if (configs.ContainsKey("master")) masterQQ = long.Parse(configs["master"]);
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
                if (configs.ContainsKey("askname")) askName = configs["askname"];

            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }

            try
            {
                groupLevel = new Dictionary<long, List<string>>();
                var lines = FileIOActor.readLines($"{this.path}{groupLevelListFile}");
                foreach(var line in lines)
                {
                    var items = line.Trim().Split('\t');
                    if (items.Length >= 2)
                    {
                        groupLevel[long.Parse(items[0])] = items[1].Trim().Split(new char[] { ',', '，' },StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }

                personLevel = new Dictionary<long, List<string>>();
                var lines2 = FileIOActor.readLines($"{this.path}{personLevelListFile}");
                foreach (var line in lines2)
                {
                    var items = line.Trim().Split('\t');
                    if (items.Length >= 2)
                    {
                        personLevel[long.Parse(items[0])] = items[1].Trim().Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
            }
            catch
            {

            }
        }

        public void save()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"master={masterQQ}");
                sb.AppendLine($"groupmsgbuff={(useGroupMsgBuf?1:0)}");
                sb.AppendLine($"starttime={startTimeString}");
                sb.AppendLine($"startnum={beginTimes}");
                sb.AppendLine($"playtimeprivate={playTimePrivate}");
                sb.AppendLine($"playtimegroup={playTimeGroup}");
                sb.AppendLine($"errtime={errTime}");
                sb.AppendLine($"askname={askName}");
                FileIOActor.write(path + configFile, sb.ToString());

                sb = new StringBuilder();
                foreach (var pair in groupLevel)
                {
                    sb.AppendLine($"{pair.Key}\t{string.Join("，", pair.Value)}");
                }
                FileIOActor.write(path + groupLevelListFile, sb.ToString());

                sb = new StringBuilder();
                foreach (var pair in personLevel)
                {
                    sb.AppendLine($"{pair.Key}\t{string.Join("，", pair.Value)}");
                }
                FileIOActor.write(path + personLevelListFile, sb.ToString());
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        public bool groupIs(long group, string state)
        {
            if (groupLevel.ContainsKey(group)) return groupLevel[group].Contains(state);
            else return false;
        }

        public bool personIs(long group, string state)
        {
            if (personLevel.ContainsKey(group)) return personLevel[group].Contains(state);
            else return false;
        }

        /// <summary>
        /// 判断是否回复特定qq号的消息
        /// 根据personlevel配置来作判断
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool allowuser(long user)
        {
            if (!personIs(user, "屏蔽")) return true;
            else
            {
                if (personLevel.ContainsKey(user))
                {
                    for(int i = 0; i < personLevel[user].Count; i++)
                    {
                        if (personLevel[user][i].StartsWith("剩余："))
                        {
                            try
                            {
                                int lefttime = int.Parse(personLevel[user][i].Substring(3));
                                if (lefttime > 0)
                                {
                                    lefttime -= 1;
                                    personLevel[user][i] = $"剩余：" + lefttime;
                                    // TODO:整小时，重置互乐次数
                                    return true;
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
            return false;
        }
    }

}
