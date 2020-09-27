# 代码分析类ForC#
在实践中，有时我们会遇到需要将原来是某种语言的代码转换成另外一种语言代码的情形，这种情况下，就需要对原来的代码进行提取分析，并根据两种语言的不同点进行对应处理。
另外一种常见场景就是，组装SQL语句时，有时不能预知计算单元的数据类型，需要对参与计算的每个变量或数字进行浮点化处理。如：SQl中计算3/5 得到的结果是0，但如果后面需要用到3/5的结果再参与到其他计算中，这时候，0这个结果显然不是我们想要，如果我们把式子改成：3\*1.00/5\*1.00 则可完美避免此问题。这时候，使用本类对原SQl代码解析，遍历所有常数，并将所有常数*1.00.

主要用于分析代码字符串，并分离出操作符、变量、函数、常量等单元，以对不同单元进行不同处理。带有括号（花括号、方括号和圆括号）匹配检测功能，检测结果可通过共成员Errors获得。
如：
~~~
a+b*c+fun(35+Xd)
~~~

解析后为：
~~~
a + b + c + fun ( 53 + Xd )
~~~

对应的，a,b,c，Xd 会被标示为变量，+,+,+,+会被标示为opt（操作符），fun 会被标示为函数，( 会被标示为 做定界符，53 会被标示为常量，) 会被标示为右定界符。
具体标示结构可参考下面SyntaxUnit类：
~~~
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
~~~

# API
## 提供了已个遍历全部或指定类型单元的 成员函数：Walk，原型如下：
~~~
	/// <summary>
        /// 使用walkFun函数数处理每个指定类型的单元，并返回整个处理后的字串
        /// </summary>
        /// <param name="walkFun">委托函数，会传入指定类型的语法单元</param>
        /// <param name="unitType">SyntaxUnitType类型，若要全部类型都执行，则不指定本参数，默认为SyntaxUnitType.all</param>
        /// <returns></returns>
		
        public string Walk(DelegateWalkSyntaxUnitFun walkFun, SyntaxUnitType unitType = SyntaxUnitType.all);
~~~

