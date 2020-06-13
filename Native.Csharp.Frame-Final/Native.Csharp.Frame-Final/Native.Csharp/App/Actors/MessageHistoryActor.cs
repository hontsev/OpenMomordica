using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Native.Csharp.Sdk.Cqp.Model;

namespace Native.Csharp.App.Actors
{
    class MessageHistory
    {
        public long userid;
        public string message;
        public DateTime date;

        public MessageHistory()
        {
           // groupid = -1;
            userid = -1;
            message = "";
        }

        public MessageHistory(long _uid, string _message)
        {
            date = DateTime.Now;
            userid = _uid;
            message = _message;
        }

        public override string ToString()
        {
            return $"{date:yyyy-MM-dd_HH:mm:ss}\t{userid}\t{message}";
        }
    }

    class MessageHistoryGroup
    {
        public string filePath = "";
        public long uid;
        public bool isGroup;
        public Queue<MessageHistory> history = new Queue<MessageHistory>();

        public MessageHistoryGroup(string _rootpath, long _gid, bool _isGroup)
        {
            isGroup = _isGroup;
            uid = _gid;
            filePath = $"{_rootpath}\\{(isGroup ? "group" : "private")}\\{_gid}.txt";
        }

        public void addMessage(long user, string message)
        {
            MessageHistory h = new MessageHistory(user, message);
            history.Enqueue(h);
        }


        static int maxWriteTime = 100;
        public void write()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                
                int nowtime = 0;
                while (history.Count > 0)
                {
                    sb.AppendLine(history.Dequeue().ToString());
                    if (nowtime++ >= maxWriteTime) break;
                }
                if (sb.Length > 0)
                {
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
            catch(Exception ex)
            {
                FileIOActor.log(ex);
            }
        }
    }


    class MessageHistoryActor
    {
        public string path;
        public Thread DataSavingThread;
        public object savemsgMutex = new object();

        Dictionary<string, MessageHistoryGroup> history = new Dictionary<string, MessageHistoryGroup>();

        public void init(string _path)
        {
            path = _path;

            DataSavingThread = new Thread(workDealHistory);
            DataSavingThread.Start();
        }

        public void workDealHistory()
        {
            while (true)
            {
                try
                {
                    foreach(var item in history.Values)
                    {
                        item.write();
                    }
                    Thread.Sleep(1000);
                }
                catch(Exception ex)
                {
                    FileIOActor.log(ex);
                }
            }
        }



        /// <summary>
        /// 记录群/私人聊天信息到文件中
        /// </summary>
        /// <param name="group"></param>
        /// <param name="user"></param>
        /// <param name="msg"></param>
        public void saveMsg(long group, long user, string msg)
        {
            try
            {
                bool isGroup = (group <= 0 ? false : true);
                long uid = (isGroup ? group : user);
                string key = (isGroup ? $"G{group}" : $"P{user}");
                if (!history.ContainsKey(key)) history[key] = new MessageHistoryGroup(path, uid, isGroup);
                history[key].addMessage(user, msg);
            }
            catch (Exception ex)
            {
                FileIOActor.log(ex);
            }
        }

    }
}
