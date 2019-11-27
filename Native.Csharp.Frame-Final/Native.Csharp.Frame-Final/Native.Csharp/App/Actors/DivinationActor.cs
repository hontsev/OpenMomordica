using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    class DivinationActor
    {
        Dictionary<string, string[]> guaci = new Dictionary<string, string[]>();
        Dictionary<string, string[]> yaoci = new Dictionary<string, string[]>();

        Random rand;
        string path = "";
        string zhouyiName = "zhouyi.txt";

        public void init(string path)
        {
            
            rand = new Random();

            var lines = FileIOActor.readLines(path + zhouyiName);
            string nowGuaNum = "";
            int nowline = 0;
            string[] items;
            foreach (var line in lines)
            {
                nowline += 1;
                if (line.StartsWith("0") || line.StartsWith("1"))
                {
                    nowGuaNum = line.Trim();
                    guaci[nowGuaNum] = new string[5];
                    yaoci[nowGuaNum] = new string[6];
                    nowline = 0;
                }
                else
                {
                    if (line.StartsWith("初九：") || line.StartsWith("初六："))
                        yaoci[nowGuaNum][0] = line.Substring(3).Replace(" ", "").Trim();
                    else if (line.StartsWith("九二：") || line.StartsWith("六二："))
                        yaoci[nowGuaNum][1] = line.Substring(3).Replace(" ", "").Trim();
                    else if (line.StartsWith("九三：") || line.StartsWith("六三："))
                        yaoci[nowGuaNum][2] = line.Substring(3).Replace(" ", "").Trim();
                    else if (line.StartsWith("九四：") || line.StartsWith("六四："))
                        yaoci[nowGuaNum][3] = line.Substring(3).Replace(" ", "").Trim();
                    else if (line.StartsWith("九五：") || line.StartsWith("六五："))
                        yaoci[nowGuaNum][4] = line.Substring(3).Replace(" ", "").Trim();
                    else if (line.StartsWith("上九：") || line.StartsWith("上六："))
                        yaoci[nowGuaNum][5] = line.Substring(3).Replace(" ", "").Trim();
                    else if (line.StartsWith("彖曰：") || line.StartsWith("象曰："))
                        guaci[nowGuaNum][4] += line.Substring(3).Replace(" ", "").Trim() + "\r\n";
                    else if (nowline == 1)
                    {
                        items = line.Trim().Split(' ');
                        guaci[nowGuaNum][0] = items[0];
                        guaci[nowGuaNum][1] = items[1];
                        guaci[nowGuaNum][2] = items[2];
                    }
                    else if (nowline == 2)
                    {
                        items = line.Trim().Split('：');
                        guaci[nowGuaNum][3] = items[1];
                    }
                }
            }
        }



        public string getZhouYi()
        {
            string yao = "";
            string gua1 = "";
            string gua2 = "";
            for (int i = 0; i < 6; i++)
            {
                yao = getYao() + yao;
            }
            //Debug.WriteLine(yao);

            List<int> notchanges = new List<int>();
            List<int> changes = new List<int>();
            string yao2 = "";
            for (int i = 0; i < 6; i++)
            {
                gua1 += (yao[i] == '6' || yao[i] == '8') ? '0' : '1';
                if (yao[i] == '6')
                {
                    changes.Add(i);
                    yao2 = yao2 + "7";
                }
                else if (yao[i] == '9')
                {
                    changes.Add(i);
                    yao2 = yao2 + "8";
                }
                else
                {
                    notchanges.Add(i);
                    yao2 += yao[i];
                }
                gua2 += (yao2[i] == '6' || yao2[i] == '8') ? '0' : '1';
            }

            string result = "";
            switch (changes.Count)
            {
                case 0:
                    result = $"主卦：{guaci[gua1][0]}，无变卦\r\n" +
                        $"{guaci[gua1][3]}";
                    break;
                case 1:
                    result = $"主卦：{guaci[gua1][0]}，变卦：{guaci[gua2][0]}，" +
                        $"变爻：{getYaoPos(changes[0], gua1[changes[0]])}\r\n" +
                        $"{yaoci[gua1][changes[0]]}";
                    break;
                case 2:
                    result = $"主卦：{guaci[gua1][0]}，变卦：{guaci[gua2][0]}，" +
                        $"变爻：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}\r\n" +
                        $"{yaoci[gua1][changes[0]]}\r\n{yaoci[gua1][changes[1]]}";
                    break;
                case 3:
                    result = $"主卦：{guaci[gua1][0]}，变卦：{guaci[gua2][0]}，" +
                        $"变爻：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}\r\n" +
                        $"{guaci[gua1][3]}\r\n{guaci[gua2][3]}"; break;
                case 4:
                    result = $"主卦：{guaci[gua1][0]}，变卦：{guaci[gua2][0]}，" +
                        $"变爻：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}、{getYaoPos(changes[3], gua1[changes[3]])}\r\n" +
                        $"{yaoci[gua1][notchanges[0]]}\r\n{yaoci[gua1][notchanges[1]]}";
                    break;
                case 5:
                    result = $"主卦：{guaci[gua1][0]}，变卦：{guaci[gua2][0]}，" +
                        $"变爻：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}、{getYaoPos(changes[3], gua1[changes[3]])}、{getYaoPos(changes[4], gua1[changes[4]])}\r\n" +
                        $"{yaoci[gua2][notchanges[0]]}";
                    break;
                case 6:
                    result = $"主卦：{guaci[gua1][0]}，变卦：{guaci[gua2][0]}，六爻皆变\r\n" +
                        $"{guaci[gua2][3]}";
                    break;
                default:
                    break;
            }
            return result;
        }

        string getYaoPos(int num, char yinyang)
        {
            if (yinyang == '0')
            {
                switch (num)
                {
                    case 5: return "初九";
                    case 4: return "九二";
                    case 3: return "九三";
                    case 2: return "九四";
                    case 1: return "九五";
                    case 0: return "上九";
                    default: break;
                }
            }
            else if (yinyang == '1')
            {
                switch (num)
                {
                    case 5: return "初六";
                    case 4: return "六二";
                    case 3: return "六三";
                    case 2: return "六四";
                    case 1: return "六五";
                    case 0: return "上六";
                    default: break;
                }
            }
            return "";
        }

        int getYao()
        {
            int allnum = 49;
            allnum = Bian(allnum);
            //Debug.WriteLine(allnum);
            allnum = Bian(allnum);
            //Debug.WriteLine(allnum);
            allnum = Bian(allnum);
            //Debug.WriteLine(allnum);
            allnum /= 4;

            return allnum;
        }

        int Bian(int allnum)
        {
            int minnum = 3;
            int left = 0;
            int right = 0;
            int middle = 0;
            for (int i = 0; i < allnum; i++)
            {
                if (rand.Next(100) < 50) left++;
                else right++;
            }
            if (left < minnum)
            {
                left += minnum;
                right -= minnum;
            }
            else if (right < minnum)
            {
                right += minnum;
                left -= minnum;
            }
            right -= 1;
            middle += 1;
            int leftmod = left % 4;
            if (leftmod == 0) leftmod = 4;
            left -= leftmod;
            middle += leftmod;
            int rightmod = right % 4;
            if (rightmod == 0) rightmod = 4;
            right -= rightmod;
            middle += rightmod;

            return left + right;
        }
    }
}
