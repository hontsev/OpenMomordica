using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 简易骰子功能模块
    /// </summary>
    class DiceActor
    {
        Random rand;
        public DiceActor()
        {
            rand = new Random();
        }

        public string getRollString(string cmd)
        {
            Regex reg = new Regex(@"^r(\d*)?d(\d*)?(.*)?$");
            var result = reg.Match(cmd);
            if(result.Success)
            {
                int dicenum = 1;
                int facenum = 100;
                string desc = "";
                try
                {
                    if (result.Groups.Count == 4)
                    {
                        try
                        {
                            dicenum = int.Parse(result.Groups[1].ToString());
                        }
                        catch { }
                        try
                        {
                            facenum = int.Parse(result.Groups[2].ToString());
                        }
                        catch { }
                        try
                        {
                            desc = result.Groups[3].ToString();
                        }
                        catch { }     
                    }
                }
                catch { }
                string resdesc = "";
                long res = getRoll(facenum, dicenum, out resdesc);
                return $"{desc} {dicenum}d{facenum} = {resdesc}";
            }
            return "";
        }

        public long getRoll(int faceNum, int DiceNum, out string resdesc)
        {
            long res = 0;
            List<int> ress = new List<int>();
            for (int i = 0; i < DiceNum; i++)
            {
                ress.Add(faceNum > 1 ? rand.Next(1, faceNum) : 1);
            }
            res = ress.Sum();
            if (DiceNum == 1) resdesc = $"{res}";
            else resdesc = $"{string.Join("+", ress)} = {res}";
            return res;
        }
    }
}
