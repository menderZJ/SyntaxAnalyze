using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;

namespace Common
{

    /// <summary>
    /// Description:主要用于分析代码字符串，并分离出操作符、变量、函数、常量等单元，以对不同单元进行不同处理。带有括号（花括号、方括号和圆括号）匹配检测功能，检测结果可通过共成员Errors获得。
    /// Author：menderZJ
    /// Date:2020-05-10
    /// </summary>
    public class SyntaxAnalyze
    {
        /// <summary>
        /// 定界符栈
        /// </summary>
        private Stack<StackUnit> ss;
        /// <summary>
        /// 语法单元结果序列
        /// </summary>
        private ArrayList p_list;
        /// <summary>
        /// 错误序列
        /// </summary>
        private ArrayList err_list;

        public ArrayList Result
        {
            get
            {
                return p_list;
            }
        }

        public ArrayList Errors
        {
            get
            {
                return err_list;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public SyntaxAnalyze(string statement="")
        {
            if (statement != "")
            {
                Analyze(statement);
            }
        }
        private void Analyze(string statement)
        {
            if (statement == "") {
                return;
            }
            SyntaxUnit s_item;
            SyntaxUnit s_itemC;
            StackUnit ss_item;
            StackUnit ss_itemC;
            ss = new Stack<StackUnit>();
            p_list = new ArrayList();
            err_list = new ArrayList();



            //字符串长度
            int sLen = statement.Length;
            //已扫描且未进入单元的段
            string preStr = "";
            //操作符
            string optS = "+-*/!^<>=&,% ";
            //定界符
            string delimiter = "{}()[]";
            //zi字符串定界符
            string stringS = "\"'";

            for (int i = 0; i < sLen; i++)
            {
                //当前扫描到的字符
                string currChar = statement.Substring(i, 1);
                int stackLen = ss.Count;

                //在字符串中  （字符串最优先，不匹配字符串定界符时，就认为还是在字符串里面）
                if (stackLen > 0 && stringS.IndexOf(ss.Peek().value) != -1 && ss.Peek().value != GetPairDelimiter(currChar))
                {
                    preStr = preStr + currChar;
                    continue;
                }

                if (stringS.IndexOf(currChar) != -1)
                { //字符串定界符
                    if (stackLen > 0 && ss.Peek().value == GetPairDelimiter(currChar))
                    {
                        //存入之前的字符串
                        s_item = new SyntaxUnit();
                        s_item.pos = i - preStr.Length;
                        s_item.type = SyntaxUnitType.constStrValue;
                        s_item.len = preStr.Length;
                        s_item.value = preStr;
                        p_list.Add(s_item);

                        //存入当前字符到结果序列
                        s_itemC = new SyntaxUnit();
                        s_itemC.pos = i;
                        s_itemC.type = SyntaxUnitType.delimiterR;
                        s_itemC.pairPos = ss.Pop().pos;
                        s_itemC.len = 1;
                        s_itemC.value = currChar;
                        p_list.Add(s_itemC);
                        //弹出对应的字符串定界符
                        //ss.Pop();
                    }
                    else
                    {
                        //存入当前字符到栈
                        ss_item = new StackUnit();
                        ss_item.pos = i;
                        ss_item.value = currChar;
                        ss.Push(ss_item);

                        //存入当前字符到结果序列
                        s_itemC = new SyntaxUnit();
                        s_itemC.pos = i;
                        s_itemC.type = SyntaxUnitType.delimiterL;
                        s_itemC.len = 1;
                        s_itemC.value = currChar;
                        p_list.Add(s_itemC);
                    }
                    preStr = "";
                }
                else if (optS.IndexOf(currChar) != -1)
                { //操作符
                    if (preStr.Length > 0)
                    {
                        s_item = new SyntaxUnit();
                        s_item.pos = i - preStr.Length;
                        if (IsVar(preStr))
                        {
                            s_item.type = SyntaxUnitType.variable;
                        }
                        else if (IsNum(preStr))
                        {
                            s_item.type = SyntaxUnitType.constValue;
                        }
                        else
                        {
                            s_item.type = SyntaxUnitType.unknown;
                            ErrLog("错误的变量或常量 " + preStr + "", i - preStr.Length);
                        }
                        s_item.len = preStr.Length;
                        s_item.value = preStr;
                        p_list.Add(s_item);
                        preStr = "";

                    }

                    //存入当前操作符到结果序列
                    s_itemC = new SyntaxUnit();
                    s_itemC.pos = i - preStr.Length;
                    s_itemC.type = SyntaxUnitType.opt;
                    s_itemC.len = 1;
                    s_itemC.value = currChar;
                    p_list.Add(s_itemC);

                }
                else if (delimiter.IndexOf(currChar) != -1)
                { //一般定界符
                    //有左定界符相匹配
                    if (stackLen > 0 && ss.Peek().value == GetPairDelimiter(currChar))
                    {
                        if (preStr.Length > 0)
                        {
                            s_item = new SyntaxUnit();
                            s_item.pos = i - preStr.Length;
                            if (currChar == "(" || currChar == "[")
                            {
                                if (IsVar(preStr))
                                {
                                    s_item.type = SyntaxUnitType.fnName;
                                }
                                else
                                {
                                    s_item.type = SyntaxUnitType.unknown;
                                    ErrLog("错误的变量或函数名称[" + preStr + "]", i - preStr.Length);
                                }
                            }
                            else
                            {
                                if (IsVar(preStr))
                                {
                                    s_item.type = SyntaxUnitType.variable;
                                }
                                else if (IsNum(preStr))
                                {
                                    s_item.type = SyntaxUnitType.constValue;
                                }
                                else
                                {
                                    s_item.type = SyntaxUnitType.unknown;
                                    ErrLog("错误的变量或常量：" + preStr + "", i - preStr.Length);
                                }


                            }

                            s_item.len = preStr.Length;
                            s_item.value = preStr;
                            p_list.Add(s_item);

                            s_itemC = new SyntaxUnit();
                            s_itemC.pos = i - preStr.Length;
                            s_itemC.type = SyntaxUnitType.delimiterR;
                            s_itemC.len = 1;
                            s_itemC.pairPos = ss.Pop().pos;
                            s_itemC.value = currChar;
                            p_list.Add(s_itemC);
                            
                        }
                        else
                        {

                            s_itemC = new SyntaxUnit();
                            s_itemC.pos = i - preStr.Length;
                            s_itemC.type = SyntaxUnitType.delimiterL;
                            s_itemC.len = 1;
                            s_itemC.value = currChar;
                            p_list.Add(s_itemC);
                            ss.Pop();
                        }
                    }
                    else
                    {

                        if (preStr.Length > 0)
                        {
                            s_item = new SyntaxUnit();
                            s_item.pos = i - preStr.Length;
                            if (currChar == "(" || currChar == "[")
                            {
                                if (IsVar(preStr))
                                {
                                    s_item.type = SyntaxUnitType.fnName;
                                }
                                else
                                {
                                    s_item.type = SyntaxUnitType.unknown;
                                    ErrLog("错误的变量或函数名称[" + preStr + "]", i - preStr.Length);
                                }
                            }
                            else
                            {
                                if (IsVar(preStr))
                                {
                                    s_item.type = SyntaxUnitType.variable;
                                }
                                else if (IsNum(preStr))
                                {
                                    s_item.type = SyntaxUnitType.constValue;
                                }
                                else
                                {
                                    s_item.type = SyntaxUnitType.unknown;
                                    ErrLog("错误的变量或函数名称[" + preStr + "]", i - preStr.Length);
                                }
                            }

                            s_item.len = preStr.Length;
                            s_item.value = preStr;
                            p_list.Add(s_item);
                        }




                        s_itemC = new SyntaxUnit();

                        if (((string) ")]}").IndexOf(currChar) > -1)
                        {
                            ErrLog("不能找到匹配的定界符\"" + currChar + "\"", i);
                            s_itemC.type = SyntaxUnitType.delimiterR;
                        }
                        else
                        {
                            s_itemC.type = SyntaxUnitType.delimiterL;
                        }
                        s_itemC.pos = i;

                        s_itemC.len = 1;
                        s_itemC.value = currChar;
                        p_list.Add(s_itemC);

                        if (((string) "{[(").IndexOf(currChar) > -1)
                        {
                            ss_itemC = new StackUnit();
                            ss_itemC.pos = i;
                            ss_itemC.value = currChar;
                            ss.Push(ss_itemC);
                        }
                    }

                    preStr = "";
                }
                else
                {
                    preStr = preStr + currChar;

                }
            }

            //若全部循环完preStr还有值，则存入结果序列
            if (preStr != "")
            {
                s_item = new SyntaxUnit();
                s_item.pos = sLen - preStr.Length - 1;
                if (IsVar(preStr))
                {
                    s_item.type = SyntaxUnitType.variable;

                }
                else
                {
                    s_item.type = SyntaxUnitType.constValue;
                }
                s_item.value = preStr;
                s_item.len = preStr.Length;
                p_list.Add(s_item);
            }

            //符号栈还有残留，说明成对的定界符右错误
            if (ss.Count > 0)
            {
                Stack<StackUnit> ssr = new Stack<StackUnit>();
                while (ss.Count > 0)
                {
                    ssr.Push(ss.Pop());
                }


                while (ssr.Count > 0)
                {
                    StackUnit d = ssr.Pop();
                    ErrLog("未匹配的符号:" + d.value, d.pos);
                }
            }

        }

        /// <summary>
        /// 获取配对的定界符
        /// </summary>
        /// <param name="Delimiter"></param>
        /// <returns></returns>
        private string GetPairDelimiter(string Delimiter)
        {
            string rs = "";
            switch (Delimiter)
            {
                //case "{":
                //    rs = "}";
                //    break;
                case "}":
                    rs = "{";
                    break;
                //case "<":
                //    rs = ">";
                //    break;
                case ">":
                    rs = "<";
                    break;
                //case "[":
                //    rs = "]";
                //    break;
                case "]":
                    rs = "[";
                    break;
                case "(":
                    rs = ")";
                    break;
                case ")":
                    rs = "(";
                    break;
                case "\"":
                    rs = "\"";
                    break;
                case "'":
                    rs = "'";
                    break;
            }
            return rs;
        }

        /// <summary>
        /// 存入错误列表
        /// </summary>
        /// <param name="msg">信息</param>
        /// <param name="pos">位置</param>
        private void ErrLog(string msg, int pos)
        {
            err_list.Add(msg + " at [" + pos + "]");
        }
        /// <summary>
        /// 判定是否合法的变量名称
        /// </summary>
        /// <param name="var">需要检测的变量</param>
        /// <returns></returns>
        private Boolean IsVar(string var)
        {
            Regex r = new Regex(@"^[_a-zA-ZΑ-Ωα-ω][_a-zA-Z0-9Α-Ωα-ω\.]*$", RegexOptions.Singleline);
            return r.IsMatch(var);
        }
        //判断是否为数字
        private Boolean IsNum(string v)
        {
            Regex r = new Regex(@"^[0-9]+([\.][[0-9]+)?$", RegexOptions.Singleline);
            return r.IsMatch(v);

        }

        public override string ToString()
        {
            string rs = "";
            foreach (SyntaxUnit item in p_list)
            {
                rs += item.value;
            }
            return rs;
        }



        

        /// <summary>
        /// 使用walkFun函数数处理每个指定类型的单元，并返回整个处理后的字串
        /// </summary>
        /// <param name="walkFun">委托函数，会传入指定类型的语法单元</param>
        /// <param name="unitType">SyntaxUnitType类型，若要全部类型都执行，则不指定本参数，默认为SyntaxUnitType.all</param>
        /// <returns></returns>
        public string Walk(DelegateWalkSyntaxUnitFun walkFun, SyntaxUnitType unitType = SyntaxUnitType.all)
        {
            string rs="";
            DelegateWalkSyntaxUnitFun cFun = new DelegateWalkSyntaxUnitFun(walkFun);
                foreach (SyntaxUnit item in p_list)
                {
                if (item.type == unitType || unitType == SyntaxUnitType.all)
                {
                    rs += cFun(item);
                }
                else
                {
                    rs += item.value;
                }
                }
            return rs;
        }

    }

    public delegate string DelegateWalkSyntaxUnitFun(SyntaxUnit v);

    /// <summary>
    /// 语法单元
    /// </summary>
    public class SyntaxUnit
    {
        ///单元类型
        public SyntaxUnitType type;
        //开始位置
        public int pos;
        //长度
        public int len;
        //为定界符时，另一个定界符的位置
        public int pairPos;
        //字符（串值）
        public string value;
    }

    
    /// <summary>
    /// 栈单元
    /// </summary>
    public class StackUnit
    {
        public int pos;
        public string value;
    }
    /// <summary>
    /// 单元类型
    /// </summary>
    public enum SyntaxUnitType
    {
        //全部
        all = -1,
        //未知
        unknown = 0,
        //操作符
        opt = 1,
        //变量
        variable = 2,
        //左定界符
        delimiterL = 3,
        //右定界符
        delimiterR = 4,
        //常量
        constValue = 5,
        //函数
        fnName = 6,
        //字符串常量
        constStrValue=7


    }

}