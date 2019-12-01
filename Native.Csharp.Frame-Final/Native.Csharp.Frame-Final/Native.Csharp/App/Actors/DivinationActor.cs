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
        Dictionary<string, string[,]> yaoci = new Dictionary<string, string[,]>();

        Random rand;
        string path = "";
        string zhouyiName = "zhouyi.txt";

        public void init(string path)
        {
            
            rand = new Random();
            this.path = path;
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
                    guaci[nowGuaNum] = new string[7];
                    yaoci[nowGuaNum] = new string[6, 4];
                    nowline = 0;
                }
                else
                {
                    if (nowline == 1)
                    {
                        items = line.Trim().Split(' ');
                        guaci[nowGuaNum][0] = items[0];
                        guaci[nowGuaNum][1] = items[1];
                        guaci[nowGuaNum][2] = items[2];
                    }
                    else if (nowline == 2) guaci[nowGuaNum][3] = line.Trim().Substring(guaci[nowGuaNum][0].Length + 1);
                    else if (nowline == 3) guaci[nowGuaNum][4] = line.Trim().Substring(3);
                    else if (nowline == 4) guaci[nowGuaNum][5] = line.Trim().Substring(3);
                    else if (nowline == 5) guaci[nowGuaNum][6] = line.Trim().Substring(3);
                    else if (nowline >= 6) yaoci[nowGuaNum][(nowline - 6) / 4, (nowline - 6) % 4] = line.Trim().Substring(3);
                }
            }
        }

        
        public string getGuaming(string gua)
        {
            return $"{guaci[gua][0]}({guaci[gua][1]}，{guaci[gua][2]})";
        }
        public string getGuaci(string gua)
        {
            return $"★{guaci[gua][3]}\r\n{guaci[gua][4]}";
            //return $"★{guaci[gua][3]}\r\n{guaci[gua][4]}\r\n{guaci[gua][6]}";
        }

        public string getYaoci(string gua, int yao)
        {
            return $"★{getYaoPos(yao, gua[yao])}，{yaoci[gua][yao, 0]}\r\n{yaoci[gua][yao, 1]}";
            //return $"★{getYaoPos(yao, gua[yao])}，{yaoci[gua][yao, 0]}\r\n{yaoci[gua][yao, 1]}\r\n{yaoci[gua][yao, 3]}";
        }

        public string getZhouYi()
        {
            string yao = "";
            string gua1 = "";
            string gua2 = "";
            for (int i = 0; i < 6; i++)
            {
                yao += getYao();
            }

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
                    result = $"主卦：{getGuaming(gua1)}，无变卦\r\n" +
                        $"{getGuaci(gua1)}";
                    break;
                case 1:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有一：{getYaoPos(changes[0], gua1[changes[0]])}\r\n" +
                        $"{getYaoci(gua1, changes[0])}";
                    break;
                case 2:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有二：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}\r\n" +
                        $"{getYaoci(gua1, changes[0])}\r\n{getYaoci(gua1, changes[1])}";
                    break;
                case 3:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有三：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}\r\n" +
                        $"{getGuaci(gua1)}\r\n{getGuaci(gua2)}"; break;
                case 4:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有四：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}、{getYaoPos(changes[3], gua1[changes[3]])}\r\n" +
                        $"{getYaoci(gua2, changes[0])}\r\n{getYaoci(gua2, changes[1])}";
                    break;
                case 5:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n" +
                        $"变爻有五：{getYaoPos(changes[0], gua1[changes[0]])}、{getYaoPos(changes[1], gua1[changes[1]])}、{getYaoPos(changes[2], gua1[changes[2]])}、{getYaoPos(changes[3], gua1[changes[3]])}、{getYaoPos(changes[4], gua1[changes[4]])}\r\n" +
                        $"{getYaoci(gua2, changes[0])}";
                    break;
                case 6:
                    result = $"主卦：{getGuaming(gua1)}\r\n变卦：{getGuaming(gua2)}\r\n六爻皆变\r\n" +
                        $"{getGuaci(gua2)}";
                    break;
                default:
                    break;
            }
            return result;
        }

        public string getYaoPos(int num, char yinyang)
        {
            if (yinyang == '1')
            {
                switch (num)
                {
                    case 0: return "初九";
                    case 1: return "九二";
                    case 2: return "九三";
                    case 3: return "九四";
                    case 4: return "九五";
                    case 5: return "上九";
                    default: break;
                }
            }
            else if (yinyang == '0')
            {
                switch (num)
                {
                    case 0: return "初六";
                    case 1: return "六二";
                    case 2: return "六三";
                    case 3: return "六四";
                    case 4: return "六五";
                    case 5: return "上六";
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
