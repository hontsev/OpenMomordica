using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{

    class ItemActor
    {
        Random rand;
        public string repfile = @"replaces.txt";
        public string expfile = @"exps.txt";
        string path = "";
        Dictionary<string, List<string>> replaces = new Dictionary<string, List<string>>();
        Dictionary<string, string> replacesC = new Dictionary<string, string>();
        List<string[]> exps = new List<string[]>();

        public ItemActor()
        {

            rand = new Random();
        }

        public void init(string path)
        {
            try
            {
                this.path = path;


                var lines1 = FileIOActor.readLines(path + repfile);
                foreach (var line in lines1)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items.Length >= 2)
                    {
                        var r1 = items[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        replaces[items[0]] = r1.ToList();
                        foreach (var sr in r1) replacesC[sr] = items[0];
                    }
                }

                var lines2 = FileIOActor.readLines(path + expfile);
                foreach (var line in lines2)
                {
                    var items = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    exps.Add(items);
                }
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
                List<string> res = new List<string>();
                StringBuilder sb = new StringBuilder();
                foreach (var exp in exps)
                {
                    sb.Append($"{string.Join("\t", exp)}\r\n");
                }
                File.WriteAllText(path + expfile, sb.ToString(), Encoding.UTF8);

                sb = new StringBuilder();
                foreach (var rep in replaces)
                {
                    sb.Append($"{rep.Key}\t{string.Join(",", rep.Value)}\r\n");
                }
                File.WriteAllText(path + repfile, sb.ToString(), Encoding.UTF8);

            }
            catch
            {

            }
        }
        
        public void inputRep(string a,string b)
        {
            if (!replaces.ContainsKey(a)) replaces[a] = new List<string>();
            replaces[a].Add(b);
            save();
        }

        public void inputExp(string a, string b)
        {
            exps.Add(new string[] { a, b });
            save();
        }

        public void delRep(string a, string b)
        {
            if (!replaces.ContainsKey(a)) replaces[a] = new List<string>();
            try
            {
                replaces[a].Remove(b);
            }
            catch { }
            save();
        }

        public string showExps()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var exp in exps)
            {
                sb.Append($"{string.Join("\t", exp)}\r\n");
            }
            File.WriteAllText(path + expfile, sb.ToString(), Encoding.UTF8);

            return sb.ToString();
        }

        public string showReps()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var rep in replaces)
            {
                sb.Append($"{rep.Key}\t{string.Join(",", rep.Value)}\r\n");
            }
            File.WriteAllText(path + repfile, sb.ToString(), Encoding.UTF8);

            return sb.ToString();
        }

        public string getResult(string input)
        {
            string res = "";
            foreach (var exp in exps)
            {
                try
                {
                    string ine = exp[0];
                    string oute = exp[1];

                    int maxtime = 20;
                    int time = 0;
                    bool find = false;
                    List<string> initems = new List<string>();
                    while (ine.Contains('【'))
                    {
                        time++;
                        if (time > maxtime) break;
                        int begin = ine.IndexOf('【');
                        int end = ine.IndexOf('】');
                        string k = ine.Substring(begin + 1, end - begin - 1);
                        if (replaces.ContainsKey(k))
                        {
                            var target = replaces[k];
                            foreach (var tar in target)
                            {
                                string t = ine.Replace($"【{k}】", tar);
                                if (input == t)
                                {
                                    find = true;
                                    initems.Add(tar);
                                    ine = t;
                                    //break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (input == ine)
                    {
                        find = true;
                    }
                    time = 0;
                    if (find)
                    {
                        while (oute.Contains('【'))
                        {
                            time++;
                            if (time > maxtime) break;
                            int begin = oute.IndexOf('【');
                            int end = oute.IndexOf('】');
                            string k = oute.Substring(begin + 1, end - begin - 1);
                            if (replaces.ContainsKey(k))
                            {
                                var tartmp = replaces[k];
                                oute = oute.Replace($"【{k}】", tartmp[rand.Next(tartmp.Count)]);
                            }
                            else
                            {
                                int tryint = -1;
                                int.TryParse(k, out tryint);
                                if (tryint > 0)
                                {
                                    //tryint -= 1;
                                    try
                                    {
                                        oute = oute.Replace($"【{k}】", initems[tryint - 1]);
                                    }
                                    catch (Exception ex)
                                    {
                                        FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
                                    }
                                }
                            }
                        }
                        res = oute;
                    }
                }
                catch(Exception ex)
                {
                    FileIOActor.log(ex.Message + "\r\n" + ex.StackTrace);
                }
            }

            return res;
        }


        public static string DealInput(string input)
        {
            input = input.Trim();
            string[] options;

            return input;
        }

    }
}
