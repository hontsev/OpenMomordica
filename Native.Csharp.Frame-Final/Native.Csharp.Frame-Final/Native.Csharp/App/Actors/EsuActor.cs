using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    class EsuActor
    {
        string path = "";
        string firstName = "firstname.txt";
        string lastName = "lastname.txt";


        public EsuActor()
        {

        }

        public void init(string path)
        {
            this.path = path;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            //var lines = FileIOActor.readLines(path + imageWords);
            //foreach (var line in lines)
            //{
            //    var items = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //    if (items.Length >= 2) baiduWordReplaceDict[items[0]] = items[1];
            //}
        }

        /// <summary>
        /// 18位身份证号码验证
        /// </summary>
        /// <param name="idNumber"></param>
        /// <returns></returns>
        public bool CheckIDCard18(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证  
            }
            string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = idNumber.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            Console.WriteLine("Y的理论值: " + y);
            if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证  
            }
            return true;//符合GB11643-1999标准  
        }


    }
}
