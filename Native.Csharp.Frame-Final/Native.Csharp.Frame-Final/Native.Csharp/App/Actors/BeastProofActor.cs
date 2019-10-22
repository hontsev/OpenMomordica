using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App.Actors
{
    /// <summary>
    /// 恶臭数学论证模块
    /// </summary>
    class BeastProofActor
    {
        string bihuaDictName = "bihua.txt";
        Dictionary<string, int> bhdict = new Dictionary<string, int>();
        List<double> tbase = new List<double>();

        /* Maintains the number of ways to decompose the given number. */
        int counter = 0;

        List<string> proofres = new List<string>();
        public string finalproof = "";
        Random rand = new Random();

        /* Maintains the number of calculations performed. */
        int calculation = 0;
        double desired = 0;

        object proofLock = new object();

        public BeastProofActor()
        {
        }

        public void init(string path)
        {
            try
            {
                bhdict = new Dictionary<string, int>();
                var lines = FileIOActor.readLines(path + bihuaDictName);
                foreach (var line in lines)
                {
                    string[] vitem = line.Split('\t');
                    if (vitem.Length >= 2) bhdict[vitem[0]] = int.Parse(vitem[1]);
                }
            }
            catch (Exception e)
            {
                FileIOActor.log(e.Message + "\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 获取汉字笔画数
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public int getbh(string word)
        {
            if (bhdict.ContainsKey(word)) return bhdict[word];
            return -1;
        }

        /// <summary>
        /// 英文字母序列的数字论证
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool getProofEngIndex(string str)
        {
            string eng = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            List<int> bhs = new List<int>();
            foreach (var word in str)
            {
                if ("\t\r\n []【】！@#￥%…&*（）+=-—_!@#$%^&*()|/\\。、，？?“”\"',".Contains(word)) continue;
                int index = eng.IndexOf(word) % 26 + 1;
                if (index <= 0)
                {
                    // not find!
                    return false;
                }
                bhs.Add(index);
            }
            string desc1 = $"{str} 的字母序号是 {string.Join(",", bhs)}\r\n";
            desc1 += $"{string.Join("+", bhs)} = {bhs.Sum()}\r\n";

            bool proofsuccess = getProof(bhs.Sum());
            if (proofsuccess)
            {
                finalproof = desc1 + finalproof;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 汉字笔画的数字论证
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool getProofBh(string str)
        {
            List<int> bhs = new List<int>();
            foreach (var word in str)
            {
                if ("\t\r\n []【】！@#￥%…&*（）+=-—_!@#$%^&*()|/\\。、，？?“”\"',".Contains(word)) continue;
                int bh = getbh(word.ToString());
                if (bh < 0)
                {
                    // not find!
                    return false;
                }
                bhs.Add(bh);
            }
            string desc1 = $"{str} 的笔划是 {string.Join(",", bhs)}\r\n";
            desc1 += $"{string.Join("+", bhs)} = {bhs.Sum()}\r\n";

            bool proofsuccess = getProof(bhs.Sum());
            if (proofsuccess)
            {
                finalproof = desc1 + finalproof;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 字符串数字论证
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public bool getProofString(string str1, string str2)
        {
            lock (proofLock)
            {
                if (!string.IsNullOrWhiteSpace(str1) && string.IsNullOrWhiteSpace(str2))
                {
                    long trynum;
                    if (long.TryParse(str1, out trynum)) return getProof(trynum);
                    else if (getProofEngIndex(str1)) return true;
                    else if (getProofBh(str1)) return true;

                }
                else if(!string.IsNullOrWhiteSpace(str1) && !string.IsNullOrWhiteSpace(str2))
                {

                }
            }
            return false;
        }

        /// <summary>
        /// 数字求和，并打印求和表达式字符串
        /// </summary>
        /// <param name="num"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public long getNumSum(long num, out string description)
        {
            string ns = num.ToString();
            long sum = 0;
            description = "";
            foreach (var c in ns)
            {
                description += c + " + ";
                sum += long.Parse(c.ToString());
            }
            if (description.EndsWith("+ ")) description = description.Substring(0, description.Length - 2);
            description += "= " + sum;
            return sum;
        }

        /// <summary>
        /// 整数的数字论证
        /// </summary>
        /// <param name="desired1"></param>
        /// <returns></returns>
        public bool getProof(long desired1)
        {
            desired = desired1;
            string result = "";

            if (strongProof() > 0)
            {
                // have strong.
                string p1 = proofres[rand.Next(proofres.Count)];
                finalproof = $"{desired} = {p1}\r\nQ.E.D";
                return true;
            }
            else
            {
                // try sum
                try
                {
                    finalproof = "";
                    int time = 5;
                    while (time > 0)
                    {
                        time--;
                        string desc = "";
                        desired = getNumSum((long)desired, out desc);
                        if (finalproof.Length > 0) finalproof += " = ";
                        finalproof += desc;
                        if (strongProof() > 0)
                        {
                            string p1 = proofres[rand.Next(proofres.Count)];
                            finalproof += $" = {p1}\r\nQ.E.D";
                            return true;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return false;

        }

        int strongProof()
        {
            proofres = new List<string>();
            calculation = 0;
            counter = 0;
            tbase.Clear();
            tbase.Add(1.0);
            tbase.Add(1.0);
            tbase.Add(4.0);
            tbase.Add(5.0);
            tbase.Add(1.0);
            tbase.Add(4.0);
            List<int> array = new List<int>();
            array.Add(0);
            array.Add(0);

            put(array, 11);

            array.Clear();
            array.Add(-1);
            array.Add(0);
            put(array, 11);

            return counter;
        }

        //string weakProof()
        //{
        //    string result = "";
        //    calculation = 0;
        //    counter = 0;
        //    Queue<char> q = new Queue<char>();
        //    q.Enqueue('1');
        //    q.Enqueue('1');
        //    q.Enqueue('4');
        //    q.Enqueue('5');
        //    q.Enqueue('1');
        //    q.Enqueue('4');
        //    List<double> v = new List<double>();
        //    choose(q, v);
        //    if (counter >= 1)
        //    {
        //        result += "Strong numerical proof found " + counter.ToString() + " solutions.\r\n";
        //    }
        //    else
        //    {
        //        result += "Strong numerical proof found " + counter.ToString() + " solution.\r\n";
        //    }
        //    result += "Performed " + calculation + " calculations.";
        //    return result;
        //}

        void choose(Queue<char> remain, List<double> current)
        {
            string input = "";
            if (remain.Count <= 0)
            {
                tbase = current;
                int max_length = current.Count * 2 - 1;
                if (max_length > 2 && max_length < 11)
                {
                    List<int> array = new List<int>();
                    array.Add(0);
                    array.Add(0);
                    put(array, max_length);
                    array.Clear();
                    array.Add(-1);
                    array.Add(0);
                    put(array, max_length);
                }
                return;
            }
            while (remain.Count > 0)
            {
                input += remain.Peek();
                remain.Dequeue();
                current.Add(double.Parse(input));
                // used++;

                choose(remain, current);
                current.RemoveAt(current.Count - 1);
            }
        }

        void put(List<int> v, int max_length)
        {
            put(v, 2, 2, 0, max_length);
        }

        //recursively put in numbers and symbols in Reverse Polish Notation (RPN).
        //-1: -1, 0: number, 1: +, 2: -, 3: *, 4: /, 5: ^.
        void put(List<int> v, int pos, int numCounter, int symCounter, int length_)
        {
            //cout << "put called" << endl;
            if (pos == length_)
            {
                if (checker(v))
                {
                    counter++;
                    print(v);
                }
                calculation++;
                return;
            }
            int lower = 0;
            int upper = 6;
            if (numCounter == (length_ + 1) / 2)
            {
                lower = 1;
            }
            if (symCounter == (length_ + 1) / 2 - 1 || symCounter == numCounter - 1)
            {
                upper = 1;
            }
            while (lower < upper)
            {
                if (pos == (int)v.Count)
                    v.Add(lower);
                else if (pos > (int)v.Count)
                    return;
                //cout << "something went wrong" << endl;
                else
                    v[pos] = lower;
                if (lower == 0)
                    put(v, pos + 1, numCounter + 1, symCounter, length_);
                else
                    put(v, pos + 1, numCounter, symCounter + 1, length_);
                lower++;

            }

        }

        //check if the RPN stored in the array gives the desired result.
        bool checker(List<int> seed)
        {

            // double a = 1.0, b = 1.0, c = 4.0, d = 5.0, e = 1.0, f = 4.0;
            //cout << "checker called" << endl;
            Stack<double> myStack = new Stack<double>();
            int sign;
            double firstNum, secondNum;
            int numCount = 0;
            foreach (int i in seed)
            {
                if (i == 0)
                {
                    myStack.Push(tbase[numCount]);
                    numCount++;
                }
                else if (i == -1)
                {
                    myStack.Push(-1 * tbase[0]);
                    numCount++;
                }
                else
                {
                    sign = i;
                    secondNum = myStack.Peek();
                    myStack.Pop();
                    firstNum = myStack.Peek();
                    myStack.Pop();
                    //cout << "myStack size: " << myStack.size() << ", i = " << i << endl;
                    switch (sign)
                    {
                        case 1:
                            myStack.Push(firstNum + secondNum);
                            break;
                        case 2:
                            myStack.Push(firstNum - secondNum);
                            break;
                        case 3:
                            myStack.Push(firstNum * secondNum);
                            break;
                        case 4:
                            myStack.Push(firstNum / secondNum);
                            break;
                        case 5:
                            myStack.Push(Math.Pow(firstNum, secondNum));
                            break;
                    }
                }
            }
            // cout << "checker done" << endl;
            if (myStack.Peek() == desired)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        string translate(string input)
        {
            // cout << "translate called" << endl;

            string symbol = "+-*/^";

            Stack<string> output = new Stack<string>();
            string str1;
            string str2;
            string outstr = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                {
                    continue;
                }
                if ((int)symbol.IndexOf(input[i]) == -1)
                {
                    string total = "";
                    while (input[i] != ' ')
                    {
                        if (i == 1)
                        {
                            if (input[i] == '-')
                            {
                                total = "-1";
                                break;
                            }
                        }
                        total += input[i].ToString();
                        i++;
                    }
                    output.Push(total);
                }
                else
                {
                    str2 = output.Peek();
                    output.Pop();
                    str1 = output.Peek();
                    output.Pop();
                    if (i == input.Length - 2)
                    {
                        output.Push(str1 + input[i].ToString() + str2);
                    }
                    else
                    {
                        output.Push("(" + str1 + input[i].ToString() + str2 + ")");
                    }
                }
            }
            return output.Peek();
        }

        //print RPN.
        string print(List<int> seed)
        {
            // cout << "print called" << endl;

            int numCount = 0;
            // int i;
            string output = "";
            foreach (int i in seed)
            {
                switch (i)
                {
                    case -1:
                        output += ((int)tbase[0]).ToString();
                        output += "- ";
                        numCount++;
                        break;
                    case 0:
                        output += ((int)tbase[numCount]).ToString();
                        output += " ";
                        numCount++;
                        break;
                    case 1:
                        output += "+ ";
                        break;
                    case 2:
                        output += "- ";
                        break;
                    case 3:
                        output += "* ";
                        break;
                    case 4:
                        output += "/ ";
                        break;
                    case 5:
                        output += "^ ";
                        break;
                }
            }
            //cout << output << endl;
            // cout << std::to_string(desired) + "=" + output << endl;
            string resstr = $"{translate(output)}";
            proofres.Add(resstr);
            return $"{desired} = {translate(output)} \r\n";
        }
    }





















    /// <summary>
    /// *暂时没调通*
    /// 这个论证模块来自https://github.com/farteryhr/labs/blob/master/123456789.cpp
    /// </summary>
    class BeastProofActor1
    {
        class node : IComparable<node>
        {
            public double num;
            public node l;
            public node r;
            public short optype, opneg;

            public int CompareTo(node obj)
            {
                return num.CompareTo(obj.num);
            }
        };

        //problem size. 9 with +-*/^ takes 1~2 gb ram and several minutes.
        SortedSet<node>[,] m = new SortedSet<node>[9, 10];

        const double cmnfac = 2 * 2 * 2 * 3 * 3 * 5 * 7;
        const string numstr = "123456789";

        //initial concatenation
        node leafnode(int l, int r)
        {
            node n = new node();
            n.num = 0.0;
            for (int i = l; i < r; i++)
            {
                n.num = n.num * 10.0 + (double)(numstr[i] - '0');
            }
            n.l = n.r = null;
            n.optype = 0;
            n.opneg = 0;
            return n;
        }
        void printnode(node n, bool brkt)
        {
            if (n.optype == 0)
            {
                if (n.num < 0.0)
                {
                    Debug.Write($"({n.num})");
                    //printf("(%.10lg)", (double)n.num);
                }
                else
                {
                    Debug.Write($"{n.num}");
                    //printf("%.10lg", (double)n.num);
                }
            }
            else
            {
                if (n.optype == 4)
                {
                    Debug.Write($" +");
                    //printf(" +");
                }
                if (brkt || n.optype == 4)
                {
                    Debug.Write($"(");
                    //putchar('(');
                }
                printnode(n.l, n.l.optype > 0 && n.l.optype < n.optype);
                if (n.optype == 1)
                {
                    if (n.opneg == 0)
                    {
                        Debug.Write($" + ");
                        // printf(" + ");
                    }
                    else
                    {
                        Debug.Write($" - ");
                        //printf(" - ");
                    }
                }
                else if (n.optype == 2)
                {
                    if (n.opneg == 0)
                    {
                        Debug.Write($" * ");
                        // printf(" * ");
                    }
                    else
                    {
                        Debug.Write($" / ");
                        //printf(" / ");
                    }
                }
                else if (n.optype == 3)
                {
                    // right associativity
                    Debug.Write($" ^ ");
                    //printf(" ^ ");
                }
                else if (n.optype == 4)
                {
                    Debug.Write($" +\"\"+ ");
                    //printf(" +\"\"+ ");
                }
                printnode(n.r, n.r.optype > 0 && n.r.optype < n.optype);
                if (brkt || n.optype == 4)
                {
                    Debug.Write($")");
                    //putchar(')');
                }
                //printf(" /*%.10lg*/",(double)n.num);
            }
        }
        void insertnodup(SortedSet<node> m, node n)
        {
            var find = m.Contains(n);
            if (!find)
            {
                m.Add(n);
            }
            else
            {
                return;
                //printf("dup %.17lg ", (double)n.num);
                //printnode(*find, false);
                //printf(" = ");
                //printnode(n, false);
                //printf("\n");
            }
        }
        double idigits(double n)
        {
            n = Math.Round(n);
            if (n < 10)
            {
                return 10.0;
            }
            else if (n < 100.0)
            {
                return 100;
            }
            else if (n < 1000.0)
            {
                return 1000;
            }
            else if (n < 10000.0)
            {
                return 10000;
            }
            else if (n < 100000.0)
            {
                return 100000;
            }
            return 0;
        }

        void numfix(node n)
        {
            //try to fix FP errors.
            //may do wrong things with extremely small probability.
            //needs CAS to for absolute corectness.
            if (Math.Abs(n.num - Math.Round(n.num)) < 1e-15)
            {
                n.num = Math.Round(n.num);
            }
            else if (Math.Abs(n.num * cmnfac - Math.Round(n.num * cmnfac)) < 1e-12)
            {
                n.num = Math.Round(n.num * cmnfac) / cmnfac;
            }
        }

        //public string getProof(double din)
        //{
        //    string result = "";
        //    //ensure long double
        //    for (int i = 1; i <= 9; i++)
        //    {
        //        for (int j = 0; j + i <= 9; j++)
        //        {
        //            Debug.Write($"width:{i} offset:{j}\r\n");
        //            //printf("width:%d offset:%d\n", i, j);
        //            if (m[j, j + i] == null) m[j, j + i] = new SortedSet<node>();
        //            SortedSet<node> tout = m[j, j + i];
        //            node n;
        //            //if(i==1){ //only single digits
        //            n = leafnode(j, j + i);
        //            insertnodup(tout, n);
        //            if (j == 0 && n.num != 0.0)
        //            {
        //                //only negate the first number
        //                //for positive initials and positive desired answers
        //                //negation sign can be transformed out of brackets
        //                n.num = -n.num;
        //                insertnodup(tout, n);
        //            }
        //            //}
        //            for (int k = 1; k <= i - 1; k++)
        //            {
        //                Debug.Write($"split:{k} len-a:{m[j, j + k].Count} len-b:{m[j + k, j + i].Count}\n");
        //                //printf("split:%d len-a:%d len-b:%d\n", k, m[j][j + k].size(), m[j + k][j + i].size());


        //                foreach (var ita in m[j, j + k])
        //                {
        //                    foreach (var itb in m[j + k, j + i])
        //                    {

        //                        node na = ita;
        //                        node nb = itb;
        //                        n.l = na;
        //                        n.r = nb;
        //                        if (nb.optype != 1)
        //                        {
        //                            n.optype = 1;
        //                            if (nb.optype != 0 || nb.num >= 0)
        //                            {
        //                                n.opneg = 0;
        //                                n.num = na.num + nb.num;
        //                                if (Math.Abs(n.num - Math.Round(n.num)) < 1e-15 ||
        //                                         Math.Abs(n.num - Math.Round(n.num)) > 1e-3)
        //                                {
        //                                    //numfix(n);
        //                                    insertnodup(tout, n);
        //                                }
        //                            }
        //                            if (nb.optype != 0 || nb.num >= 0)
        //                            {
        //                                n.opneg = 1;
        //                                n.num = na.num - nb.num;
        //                                if (Math.Abs(n.num - Math.Round(n.num)) < 1e-15 ||
        //                                    Math.Abs(n.num - Math.Round(n.num)) > 1e-3)
        //                                {
        //                                    //numfix(n);
        //                                    insertnodup(tout, n);
        //                                }
        //                            }
        //                        }
        //                        if (nb.optype != 2)
        //                        {
        //                            n.optype = 2;
        //                            n.opneg = 0;
        //                            n.num = na.num * nb.num;
        //                            if (Math.Abs(n.num) < 1e10)
        //                            { //pruning
        //                                numfix(n);
        //                                insertnodup(tout, n);
        //                            }
        //                        }
        //                        if (na.optype != 3 && // (a^b)^c=a^(b*c)
        //                            na.num >= 0 &&
        //                            true)
        //                        {
        //                            n.optype = 3;
        //                            n.opneg = 0;
        //                            n.num = Math.Pow(na.num, nb.num);
        //                            if (n.num == n.num && Math.Abs(n.num) < 1e10 && Math.Abs(n.num) > 1e-3)
        //                            {
        //                                numfix(n);
        //                                insertnodup(tout, n);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    SortedSet<node> ans = m[0, 9];
        //    Debug.Write($"OK! total {ans.Count}\n");
        //    double prec = 1e-15;
        //    node ntmp = new node();


        //    bool res = false;
        //    prec = 1e-15;

        //    while (!res)
        //    {
        //        ntmp.num = din - prec;
        //        node smallest = new node();
        //        smallest.num = 0;
        //        var it = ans.GetViewBetween(smallest, ntmp).First();
        //        while(true)
        //        {
        //            printnode(it, false);
        //            Debug.Write($" = {it.num}\n");
        //            res = true;
        //        }
        //        //int index = ans.Select
        //        //for (; it != ans.end() && Math.Abs(it.num - din) <= prec; it++)
        //        //{
        //        //    printnode(it, false);
        //        //    Debug.Write($" = {it.num}\n");
        //        //    res = true;
        //        //}
        //        prec *= 2;
        //    }

        //    return result;

        //}

        public string getProof(double input)
        {
            string result = "";
            //ensure long double
            for (int i = 1; i <= 9; i++)
            {
                for (int j = 0; j + i <= 9; j++)
                {
                    Debug.Write($"width:{i} offset:{j}\r\n");
                    //printf("width:%d offset:%d\n", i, j);
                    if (m[j, j + i] == null) m[j, j + i] = new SortedSet<node>();
                    SortedSet<node> tout = m[j, j + i];
                    node n;
                    //if(i==1){ //only single digits
                    n = leafnode(j, j + i);
                    insertnodup(tout, n);
                    if (j == 0 && n.num != 0.0)
                    {
                        //only negate the first number
                        //for positive initials and positive desired answers
                        //negation sign can be transformed out of brackets
                        n.num = -n.num;
                        insertnodup(tout, n);
                    }
                    //}
                    for (int k = 1; k <= i - 1; k++)
                    {
                        Debug.Write($"split:{k} len-a:{m[j, j + k].Count} len-b:{m[j + k, j + i].Count}\n");
                        //printf("split:%d len-a:%d len-b:%d\n", k, m[j][j + k].size(), m[j + k][j + i].size());


                        foreach (var ita in m[j, j + k])
                        {
                            foreach (var itb in m[j + k, j + i])
                            {

                                node na = ita;
                                node nb = itb;
                                n.l = na;
                                n.r = nb;
                                if (nb.optype != 1)
                                {
                                    n.optype = 1;
                                    if (nb.optype != 0 || nb.num >= 0)
                                    {
                                        n.opneg = 0;
                                        n.num = na.num + nb.num;
                                        if (Math.Abs(n.num - Math.Round(n.num)) < 1e-15 ||
                                                 Math.Abs(n.num - Math.Round(n.num)) > 1e-3)
                                        {
                                            //numfix(n);
                                            insertnodup(tout, n);
                                        }
                                    }
                                    if (nb.optype != 0 || nb.num >= 0)
                                    {
                                        n.opneg = 1;
                                        n.num = na.num - nb.num;
                                        if (Math.Abs(n.num - Math.Round(n.num)) < 1e-15 ||
                                            Math.Abs(n.num - Math.Round(n.num)) > 1e-3)
                                        {
                                            //numfix(n);
                                            insertnodup(tout, n);
                                        }
                                    }
                                }
                                if (nb.optype != 2)
                                {
                                    n.optype = 2;
                                    n.opneg = 0;
                                    n.num = na.num * nb.num;
                                    if (Math.Abs(n.num) < 1e10)
                                    { //pruning
                                        numfix(n);
                                        insertnodup(tout, n);
                                    }
                                }
                                //power, produces many useless nodes with division enabled 
                                if (na.optype != 3 && // (a^b)^c=a^(b*c)
                                    na.num >= 0 &&
                                    //nb.num >= 0 &&
                                    //Math.Abs(Math.Round(na.num)-na.num)<1e-15l &&
                                    //Math.Abs(Math.Round(nb.num)-nb.num)<1e-15l &&
                                    true)
                                {
                                    n.optype = 3;
                                    n.opneg = 0;
                                    //n.num=powl(Math.Round(na.num),Math.Round(nb.num));
                                    n.num = Math.Pow(na.num, nb.num);
                                    if (n.num == n.num && Math.Abs(n.num) < 1e10 && Math.Abs(n.num) > 1e-3)
                                    {
                                        numfix(n);
                                        insertnodup(tout, n);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            SortedSet<node> ans = m[0, 9];
            Debug.Write($"OK! total {ans.Count}\n");

            //printf("OK! total %d\n", ans.size());
            if (false)
            { //list all
              //		for(mapt::iterator it=ans.begin();it!=ans.end();it++){
              //			printf("res %.17lg = ", (double)(it->num));
              //			printnode(*it,false);
              //printf("\n");
              //}
            }
            if (true)
            { //do something meaningful
                double din = 0.0;
                double prec = 1e-15;
                node ntmp = new node();

                //if (true)
                //{ //enumerate integer results
                //    for (int i = 0; ; i++)
                //    {
                //        bool res = false;
                //        din = (double)i;
                //        ntmp.num = din - 1e-15;
                //        node smallnode = new node();
                //        smallnode.num = -1;
                //        var it = ans.GetViewBetween(smallnode, ntmp).First();
                //        for (; it != ans.end() && Math.Abs(it->num - din) <= prec; it++)
                //        {
                //            //printnode(*it,false);
                //            //printf(" = %.17lg\n",(double)(it->num));
                //            res = true;
                //            break; // get rid of multiple answers due to FP error
                //        }
                //        if (!res)
                //        {
                //            printf("%d no solution!\n", i);
                //            break;
                //        }
                //    }
                //}


            }
            return result;
        }



    }
}
