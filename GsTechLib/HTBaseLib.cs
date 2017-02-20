namespace GsTechLib
{
    #region 引入类库

    using System;
    using System.IO;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Threading;
    using Microsoft.Win32;
    using System.Windows.Forms;
    using System.Drawing.Printing;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using System.Diagnostics;

    using System.Security;
    using System.Security.Cryptography;

    using System.Data;
    using System.Data.SqlClient;
    using System.Data.OleDb;
    using System.Data.OracleClient;
    using System.Data.Odbc;
    using MySql.Data.MySqlClient;
    using MySql.Data;

    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Runtime.Remoting.Channels.Http;
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Serialization.Formatters;
    using System.Net;

    using System.Xml;

    using System.Web;
    using System.Configuration;

    #endregion

    #region 辅助类库

    /// <summary>
    /// 文件查找结构参数
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class FileInfos
    {
        public int fileAttributes = 0;
        // creationTime was an embedded FILETIME structure.
        public int creationTime_lowDateTime = 0;
        public int creationTime_highDateTime = 0;
        // lastAccessTime was an embedded FILETIME structure.
        public int lastAccessTime_lowDateTime = 0;
        public int lastAccessTime_highDateTime = 0;
        // lastWriteTime was an embedded FILETIME structure.
        public int lastWriteTime_lowDateTime = 0;
        public int lastWriteTime_highDateTime = 0;
        public int nFileSizeHigh = 0;
        public int nFileSizeLow = 0;
        public int dwReserved0 = 0;
        public int dwReserved1 = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String fileName = null;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public String alternateFileName = null;
    }

    //远程服务基础接口
    public interface IRemotingBaseInterface
    {
    }
    //远程服务基类
    [Serializable]
    public class RemotingBaseClass : MarshalByRefObject, IRemotingBaseInterface
    {
        //目的是起用无限租约
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }

    #endregion

    #region 功能类库

    /// <summary>
    /// 常用函数库
    /// </summary>
    public class HTBaseFunc
    {
        #region 数值处理类

        /// <summary>
        /// 舍入算法
        /// </summary>
        /// <param name="val"></param>
        /// <param name="RoundZero">最小值</param>
        /// <param name="RoundFrom">舍入值</param>
        /// <param name="RoundUnit">单位值</param>
        /// <returns>舍入后结果</returns>
        public static double RoundReal(double val, double RoundZero, double RoundFrom, double RoundUnit)
        {
            double AbsVal = Math.Abs(val);

            if (AbsVal < RoundZero)
            {
                AbsVal = 0.0;
            }
            else if (AbsVal <= RoundUnit)
            {
                AbsVal = RoundUnit;
            }
            else
            {
                AbsVal = Math.Floor((AbsVal + RoundUnit - RoundFrom + RoundFrom / 10000) / RoundUnit) * RoundUnit;
            }
            if (val < 0.0)
            {
                AbsVal = -AbsVal;
            }
            return AbsVal;
        }

        //		public static String UpperMoney(decimal num)
        //		{
        //			string ch1;
        //			string ch2;
        //			string Tmp;
        //			int Length;
        //			int i;
        //			string j;
        //			string sResult;
        //
        //			sResult = "";
        //			ch1 = "仟佰拾万仟佰拾元点角分";
        //			ch2 = "零壹贰叁肆伍陆柒捌玖";
        //			Tmp = (System.Math.Abs(num)).ToString("0.00");
        //			if ( Tmp.Length > 11 ) { Tmp = RightStr(Tmp, 11); }
        //			Length = Tmp.Length;		
        //			for ( i = 1 ; i <= Length;i++)
        //			{
        //				if ( i != 3 ) 
        //				{   //不为小数点位					
        //					j = Tmp.Substring(Length - i, 1) ;//取Money数值;
        //					if ( Func.NullToInt(j) > 0 ) 
        //					{	//sResult = Mid(ch2, ValInt(j) + 1, 1) + Mid(ch1, ch1.Length - i + 1, 1)+sResult;
        //						sResult = ch2.Substring(Func.NullToInt(j), 1) + ch1.Substring(ch1.Length - i, 1)+sResult;
        //					} 
        //					else 
        //					{ //j=0
        //						if ( i == 4 && Length > 4 ) 
        //						{  //个位为0，加元字
        //							sResult = "元" + sResult;
        //						}
        //						if ( i == 8 ) 
        //						{ 	//当万位为0时，加万字
        //							sResult = "万" + sResult;
        //						}
        //						if (( num > 1) && (sResult != "") && (("零元万").IndexOf(sResult.Substring(0,1)) <0) )
        //						{ 	//加零字
        //							sResult = "零" + sResult;
        //						}
        //					}
        //				}				
        //			}
        //			return sResult;
        //		}

        /// <summary>
        /// 将数值或人民币转化为大写
        /// </summary>
        /// <param name="num"></param>
        /// <param name="rmbb"></param>
        /// <returns></returns>
        public static String UpperMoney(Double num, bool rmbb)
        {
            String[] hz = new String[16];
            String[] money = new String[3];
            //hz[16]="零"
            if (!rmbb)
            {
                hz[1] = "一";
                hz[2] = "二";
                hz[3] = "三";
                hz[4] = "四";
                hz[5] = "五";
                hz[6] = "六";
                hz[7] = "七";
                hz[8] = "八";
                hz[9] = "九";
                hz[10] = "十";
                hz[11] = "百";
                hz[12] = "千";
                hz[13] = "万";
                hz[14] = "亿";
                hz[15] = "点";
            }
            else
            {
                hz[1] = "壹";
                hz[2] = "贰";
                hz[3] = "叁";
                hz[4] = "肆";
                hz[5] = "伍";
                hz[6] = "陆";
                hz[7] = "柒";
                hz[8] = "捌";
                hz[9] = "玖";
                hz[10] = "拾";
                hz[11] = "佰";
                hz[12] = "仟";
                hz[13] = "万";
                hz[14] = "亿";
                hz[15] = "元";
                money[1] = "角";
                money[2] = "分";
            }
            String val, num1, num2;
            num1 = num.ToString().Trim();
            val = num1;
            while (HTBaseFunc.LeftStr(val, 1) == "0")
            {
                val = HTBaseFunc.RightStr(val, val.Length - 1);
            }
            if (HTBaseFunc.PosStr(val, ".") >= 0)
            {
                num1 = HTBaseFunc.LeftStr(val, HTBaseFunc.PosStr(val, "."));
                num2 = HTBaseFunc.MidStr(val, HTBaseFunc.PosStr(val, ".") + 1);
            }
            else
            {
                num2 = "";
            }

            String strs = "";
            String str_tmp;
            int i;
            for (i = 0; i < num1.Length; i++)
            {
                str_tmp = HTBaseFunc.MidStr(num1, i, 1);
                switch (str_tmp)
                {
                    case "1":
                        strs = strs + hz[1];
                        break;
                    case "2":
                        strs = strs + hz[2];
                        break;
                    case "3":
                        strs = strs + hz[3];
                        break;
                    case "4":
                        strs = strs + hz[4];
                        break;
                    case "5":
                        strs = strs + hz[5];
                        break;
                    case "6":
                        strs = strs + hz[6];
                        break;
                    case "7":
                        strs = strs + hz[7];
                        break;
                    case "8":
                        strs = strs + hz[8];
                        break;
                    case "9":
                        strs = strs + hz[9];
                        break;
                    case "0":
                        strs = strs + "";
                        break;
                }
                if (str_tmp == "0")
                    if ((HTBaseFunc.RightStr(strs, 1) == "零") || (strs == "") || (i == num1.Length - 1))
                        strs = strs;
                    else
                        strs = strs + "零";
                else
                    switch (num1.Length - i)
                    {
                        case 1:
                            strs = strs;
                            break;
                        case 2:
                        case 6:
                        case 10:
                            strs = strs + hz[10]; //十
                            break;
                        case 3:
                        case 7:
                        case 11:
                            strs = strs + hz[11]; //百
                            break;
                        case 4:
                        case 8:
                        case 12:
                            strs = strs + hz[12]; //千  
                            break;
                        case 5:
                        case 13:
                            strs = strs + hz[13]; //万
                            break;
                        case 9:
                            strs = strs + "亿";
                            break;
                    }

                if ((num1.Length - i == 9) && (HTBaseFunc.MidStr(num1, num1.Length - 9, 1) == "0"))
                {
                    if (HTBaseFunc.RightStr(strs, 1) == "零") strs = HTBaseFunc.LeftStr(strs, strs.Length - 1);
                    strs = strs + "亿";
                }
                if ((num1.Length - i == 5) && (HTBaseFunc.MidStr(num1, num1.Length - 5, 1) == "0"))
                {
                    if (HTBaseFunc.RightStr(strs, 1) == "零") strs = HTBaseFunc.LeftStr(strs, strs.Length - 1);
                    strs = strs + "万";
                }
            }
            if (HTBaseFunc.RightStr(strs, 1) == "零") strs = HTBaseFunc.LeftStr(strs, strs.Length - 1);
            if ((HTBaseFunc.LeftStr(strs, 2) == "一十") || (HTBaseFunc.LeftStr(strs, 2) == "壹拾")) strs = HTBaseFunc.RightStr(strs, strs.Length - 1);

            int lpos = HTBaseFunc.PosStr(strs, "亿万");
            if (lpos >= 0) strs = HTBaseFunc.LeftStr(strs, lpos + 1) + HTBaseFunc.MidStr(strs, lpos + 2);
            if (HTBaseFunc.PosStr(val, ".") < 0)
            {
                if (rmbb)
                    return strs + hz[15];
                else
                    return strs;
            }
            strs = strs + hz[15];  //点or 元
            int il_i = num2.Length;
            if (rmbb && (il_i > 2)) il_i = 2;
            for (i = 0; i < il_i; i++)
            {
                str_tmp = HTBaseFunc.MidStr(num2, i, 1);
                switch (str_tmp)
                {
                    case "1":
                        strs = strs + hz[1];
                        break;
                    case "2":
                        strs = strs + hz[2];
                        break;
                    case "3":
                        strs = strs + hz[3];
                        break;
                    case "4":
                        strs = strs + hz[4];
                        break;
                    case "5":
                        strs = strs + hz[5];
                        break;
                    case "6":
                        strs = strs + hz[6];
                        break;
                    case "7":
                        strs = strs + hz[7];
                        break;
                    case "8":
                        strs = strs + hz[8];
                        break;
                    case "9":
                        strs = strs + hz[9];
                        break;
                    case "0":
                        strs = strs + "零";
                        break;
                }
                if (rmbb && (HTBaseFunc.RightStr(strs, 1) != "零") && (((HTBaseFunc.RightStr(strs, 1) != "角") && (i == 1)) || ((HTBaseFunc.RightStr(strs, 1) != "元") && (i == 0)))) strs = strs + money[i + 1];
            }
            while (HTBaseFunc.RightStr(strs, 1) == "零")
            {
                strs = HTBaseFunc.LeftStr(strs, strs.Length - 1);
            }
            //if (rmbb) strs=strs+"人民币";
            return strs;
        }

        /// <summary>
        /// 在oldselect语句基础上组合alwaysflt和newflt以重新生成SQL语句
        /// alwaysflt是该语句检索的常用条件
        /// newflt是该语句要组合的新的条件
        /// linkoper是与原有条件的连接符，可以是and或or
        /// </summary>
        /// <param name="oldselect"></param>
        /// <param name="alwaysflt"></param>
        /// <param name="newflt"></param>
        /// <param name="linkoper"></param>
        /// <returns></returns>
        public static String RegenerateSql(String oldselect, String alwaysflt, String newflt, String linkoper)
        {
            //重新生成数据窗口的SQL语法
            int ll_numofselect, ll_numoffrom, ll_posofselect = 0, ll_posoffrom = 0, ll_posofwhere = 0, ll_posofgroup = 0, ll_posoforder = 0;
            String ls_beforewhere, ls_followwhere, ls_beforegroup, ls_followgroup, ls_beforeorder, ls_followorder, ls_newsql;
            bool lb_gotoloopgroup = false, lb_gotolooporder = false;
            if (linkoper == "") linkoper = "and";
            //合并生成新条件
            if (newflt == "")
            {
                if (alwaysflt != "")
                    newflt = alwaysflt.Trim();
                else
                    newflt = "";
            }
            else
            {
                if (alwaysflt != "")
                    newflt = newflt.Trim() + " and " + alwaysflt.Trim();
                else
                    newflt = newflt.Trim();
            }
            //生成新SQL语法
            if (newflt == "")
                ls_newsql = oldselect;
            else
            {
                String ls_prechar, ls_endchar;
                int ll_comppos = -1;
                ll_posofwhere = -1;
            loopwhere:
                do
                {
                    ll_posofwhere = HTBaseFunc.PosStr(oldselect.ToUpper(), "WHERE", ll_posofwhere + 1);
                    if (ll_posofwhere >= 0)
                    {
                        ls_prechar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posofwhere - 1, 1);
                        if (ls_prechar == "") ls_prechar = " ";
                        ls_endchar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posofwhere + 5, 1);
                        if (ls_endchar == "") ls_endchar = " ";
                    }
                    else
                    {
                        ls_prechar = " ";
                        ls_endchar = " ";
                    }
                }
                while (!((ls_prechar == " ") || (ls_prechar == "	") || (ls_prechar == HTBaseFunc.Chr(13).ToString()) || (ls_prechar == HTBaseFunc.Chr(10).ToString()) || (ls_endchar == " ") || (ls_endchar == "	") || (ls_endchar == HTBaseFunc.Chr(13).ToString()) || (ls_endchar == HTBaseFunc.Chr(10).ToString())));

            loopgrouporder:
                if ((ll_posofwhere < 0) || lb_gotoloopgroup || lb_gotolooporder)
                {
                    if ((ll_posofwhere < 0) || lb_gotoloopgroup)
                    {
                        if (!lb_gotoloopgroup) ll_posofgroup = -1;
                        lb_gotoloopgroup = false;
                        do
                        {
                            ll_posofgroup = HTBaseFunc.PosStr(oldselect.ToUpper(), "GROUP", ll_posofgroup + 1);
                            if (ll_posofgroup >= 0)
                            {
                                ls_prechar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posofgroup - 1, 1);
                                if (ls_prechar == "") ls_prechar = " ";
                                ls_endchar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posofgroup + 5, 1);
                                if (ls_endchar == "") ls_endchar = " ";
                            }
                            else
                            {
                                ls_prechar = " ";
                                ls_endchar = " ";
                            }
                        }
                        while (!((ls_prechar == " ") || (ls_prechar == "	") || (ls_prechar == HTBaseFunc.Chr(13).ToString()) || (ls_prechar == HTBaseFunc.Chr(10).ToString()) || (ls_endchar == " ") || (ls_endchar == "	") || (ls_endchar == HTBaseFunc.Chr(13).ToString()) || (ls_endchar == HTBaseFunc.Chr(10).ToString())));
                    }

                    if ((ll_posofgroup < 0) || lb_gotolooporder)
                    {
                        if (!lb_gotolooporder) ll_posoforder = -1;
                        lb_gotolooporder = false;
                        do
                        {
                            ll_posoforder = HTBaseFunc.PosStr(oldselect.ToUpper(), "ORDER", ll_posoforder + 1);
                            if (ll_posoforder >= 0)
                            {
                                ls_prechar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posoforder - 1, 1);
                                if (ls_prechar == "") ls_prechar = " ";
                                ls_endchar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posoforder + 5, 1);
                                if (ls_endchar == "") ls_endchar = " ";
                            }
                            else
                            {
                                ls_prechar = " ";
                                ls_endchar = " ";
                            }
                        }
                        while (!((ls_prechar == " ") || (ls_prechar == "	") || (ls_prechar == HTBaseFunc.Chr(13).ToString()) || (ls_prechar == HTBaseFunc.Chr(10).ToString()) || (ls_endchar == " ") || (ls_endchar == "	") || (ls_endchar == HTBaseFunc.Chr(13).ToString()) || (ls_endchar == HTBaseFunc.Chr(10).ToString())));

                        if (ll_posoforder < 0)
                            ll_comppos = oldselect.Length;
                        else
                            ll_comppos = ll_posoforder;
                    }
                    else
                        ll_comppos = ll_posofgroup;
                }
                else
                    ll_comppos = ll_posofwhere;

                ll_numofselect = 0;
                ll_posofselect = -1;
                do
                {
                    do
                    {
                        ll_posofselect = HTBaseFunc.PosStr(oldselect.ToUpper(), "SELECT", ll_posofselect + 1);
                        if (ll_posofselect >= 0)
                        {
                            ls_prechar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posofselect - 1, 1);
                            if (ls_prechar == "") ls_prechar = " ";
                            ls_endchar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posofselect + 6, 1);
                            if (ls_endchar == "") ls_endchar = " ";
                        }
                        else
                        {
                            ls_prechar = " ";
                            ls_endchar = " ";
                        }
                    }
                    while (!((ls_prechar == " ") || (ls_prechar == "	") || (ls_prechar == HTBaseFunc.Chr(13).ToString()) || (ls_prechar == HTBaseFunc.Chr(10).ToString()) || (ls_endchar == " ") || (ls_endchar == "	") || (ls_endchar == HTBaseFunc.Chr(13).ToString()) || (ls_endchar == HTBaseFunc.Chr(10).ToString())));
                    if ((ll_posofselect >= 0) && (ll_posofselect + 1 < ll_comppos)) ll_numofselect += 1;
                }
                while (!((ll_posofselect + 1 >= ll_comppos) || (ll_posofselect < 0)));

                ll_numoffrom = 0;
                ll_posoffrom = -1;
                do
                {
                    do
                    {
                        ll_posoffrom = HTBaseFunc.PosStr(oldselect.ToUpper(), "FROM", ll_posoffrom + 1);
                        if (ll_posoffrom >= 0)
                        {
                            ls_prechar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posoffrom - 1, 1);
                            if (ls_prechar == "") ls_prechar = " ";
                            ls_endchar = HTBaseFunc.MidStr(oldselect.ToUpper(), ll_posoffrom + 4, 1);
                            if (ls_endchar == "") ls_endchar = " ";
                        }
                        else
                        {
                            ls_prechar = " ";
                            ls_endchar = " ";
                        }
                    }
                    while (!((ls_prechar == " ") || (ls_prechar == "	") || (ls_prechar == HTBaseFunc.Chr(13).ToString()) || (ls_prechar == HTBaseFunc.Chr(10).ToString()) || (ls_endchar == " ") || (ls_endchar == "	") || (ls_endchar == HTBaseFunc.Chr(13).ToString()) || (ls_endchar == HTBaseFunc.Chr(10).ToString())));
                    if ((ll_posoffrom >= 0) && (ll_posoffrom + 1 < ll_comppos)) ll_numoffrom += 1;
                }
                while (!((ll_posoffrom + 1 >= ll_comppos) || (ll_posoffrom < 0)));

                if (ll_numoffrom < ll_numofselect)
                {
                    if (ll_posofwhere >= 0)
                        goto loopwhere;
                    else if (ll_posofgroup >= 0)
                    {
                        lb_gotoloopgroup = true;
                        goto loopgrouporder;
                    }
                    else if (ll_posoforder >= 0)
                    {
                        lb_gotolooporder = true;
                        goto loopgrouporder;
                    }
                }

                //组装SQL
                if (ll_posofwhere >= 0)
                {
                    ll_posofwhere = ll_posofwhere + 4;
                    //取WHERE前的SQL
                    ls_beforewhere = HTBaseFunc.LeftStr(oldselect, ll_posofwhere + 1);
                    //取WHERE后的SQL
                    ls_followwhere = HTBaseFunc.MidStr(oldselect, ll_posofwhere + 1);
                    //合并SQL
                    ls_newsql = ls_beforewhere.Trim() + " " + newflt + " " + linkoper + " " + ls_followwhere.Trim();
                }
                else
                {
                    if (ll_posofgroup >= 0)
                    {
                        //取GROUP前的SQL
                        ls_beforegroup = HTBaseFunc.LeftStr(oldselect, ll_posofgroup - 1);
                        //取GROUP后的SQL
                        ls_followgroup = HTBaseFunc.MidStr(oldselect, ll_posofgroup);
                        //合并SQL
                        ls_newsql = ls_beforegroup.Trim() + " where " + newflt + " " + ls_followgroup.Trim();
                    }
                    else
                    {
                        if (ll_posoforder >= 0)
                        {
                            //取ORDER前的SQL
                            ls_beforeorder = HTBaseFunc.LeftStr(oldselect, ll_posoforder - 1);
                            //取ORDER后的SQL
                            ls_followorder = HTBaseFunc.MidStr(oldselect, ll_posoforder);
                            //合并SQL
                            ls_newsql = ls_beforeorder.Trim() + " where " + newflt + " " + ls_followorder.Trim();
                        }
                        else
                            ls_newsql = oldselect + " where " + newflt;
                    }
                }
            }
            return ls_newsql;
        }

        /// <summary>
        /// 在oldselect语句基础上用and组合alwaysflt和newflt以重新生成SQL语句
        /// </summary>
        /// <param name="oldselect"></param>
        /// <param name="alwaysflt"></param>
        /// <param name="newflt"></param>
        /// <returns></returns>
        public static String RegenerateSql(String oldselect, String alwaysflt, String newflt)
        {
            //重新生成SQL
            return RegenerateSql(oldselect, alwaysflt, newflt, "and");
        }

        /// <summary>
        /// 判断对象是否为null，若是则返回dftval，否则返回原值
        /// </summary>
        /// <param name="srcvar"></param>
        /// <param name="dftval"></param>
        /// <returns></returns>
        public static Object IsNull(Object srcvar, Object dftval)
        {
            if ((srcvar == DBNull.Value) || (srcvar == null))
                return dftval;
            else
                return srcvar;
        }

        /// <summary>
        /// 转换为字符串,空对象转换为空字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string NullToStr(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return "";
            }
            else
            {
                return obj.ToString();
            }
        }

        /// <summary>
        /// 转换为字符串,空对象转换为空字符串,并带单引号
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string NullToStrWithExpr(object obj)
        {
            string sResult = "";
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                sResult = "";
            }
            else
            {
                sResult = obj.ToString();
            }
            return Expr(sResult);
        }

        /// <summary>
        /// 转换为byte,空对象转换为0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte NullToByte(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return 0;
            }
            else
            {
                return (byte)StringToInt32(obj.ToString());
            }
        }

        /// <summary>
        /// 转换为int,空对象转换为0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int NullToInt(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return 0;
            }
            else
            {
                return StringToInt32(obj.ToString());
            }
        }

        /// <summary>
        /// 转换为long,空对象转换为0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long NullToLong(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return 0;
            }
            else
            {
                return StringToInt64(obj.ToString());
            }
        }

        /// <summary>
        /// 转换为guid
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Guid NullToGuid(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return Guid.Empty;
            }
            else
            {
                try
                {
                    return new Guid(obj.ToString());
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        /// <summary>
        /// 转换为Decimal,空对象转换为0.0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Decimal NullToDecimal(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return 0.0M;
            }
            else
            {
                return HTBaseFunc.StringToDecimal(obj.ToString());
            }
        }

        /// <summary>
        /// 转换为double,空对象转换为0.0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double NullToDouble(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return 0.0;
            }
            else
            {
                return HTBaseFunc.StringToDouble(obj.ToString());
            }
        }

        /// <summary>
        /// 转换为float,空对象转换为0.0
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static float NullToFloat(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return 0.0f;
            }
            else
            {
                return (float)HTBaseFunc.StringToDecimal(obj.ToString());
            }
        }

        /// <summary>
        /// 转换为日期,空对象转换为1900-01-01
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DateTime NullToDateTime(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)
            {
                return Convert.ToDateTime("1900-01-01");
            }
            else
            {
                try
                {
                    return Convert.ToDateTime(obj);
                }
                catch
                {
                    return Convert.ToDateTime("1900-01-01");
                }
            }
        }

        /// <summary>
        /// 转换为bool,空对象转换为false
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool NullToBool(object obj)
        {
            if (obj == null || obj == System.DBNull.Value)	//数据库空可以不判断,系统会自动转换为null
            {
                return false;
            }
            else
            {
                return Convert.ToBoolean(obj);
            }
        }

        /// <summary>
        /// 获取用于SQL值的DateTime数据,1900-01-01将被转换为null,其他有效时间将加上引号
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetDateTimeSqlValue(DateTime dt)
        {
            if (dt <= Convert.ToDateTime("1900-01-01"))
            {
                return "null";
            }
            else
            {
                return Expr(GetDatetimeStr(dt));
            }
        }

        /// <summary>
        /// 把一个字符串类型转化为可能的byte
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static byte StringToByte(string S)
        {
            S = S.Trim();
            if (S.Trim() == "") return (0);

            byte ival = 0;
            if (byte.TryParse(S, out ival))
                return ival;
            else
                return 0;
        }

        /// <summary>
        /// 把一个字符串类型转化为可能的整型
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static int StringToInt32(string S)
        {
            S = S.Trim();
            if (S.Trim() == "") return (0);

            int ival = 0;
            if (int.TryParse(S, out ival))
                return ival;
            else
                return 0;

            //if (S.IndexOf("-") >= 0)
            //{   //有负号，但不是开头，则转换为0
            //    if (S.StartsWith("-") == false) return (0);
            //    if (S.StartsWith("--")) return (0);
            //}
            //for (int i = 0; i < S.Length; i++)
            //{
            //    switch (S.Substring(i, 1))
            //    {
            //        case "0":
            //        case "1":
            //        case "2":
            //        case "3":
            //        case "4":
            //        case "5":
            //        case "6":
            //        case "7":
            //        case "8":
            //        case "9":
            //            break;
            //        case "-":
            //            if (S.Length == 1) return (0);
            //            break;
            //        default:
            //            if (i == 0)
            //                return (0);
            //            else
            //                try
            //                {
            //                    return (Convert.ToInt32(S.Substring(0, i)));
            //                }
            //                catch
            //                {
            //                    return 0;
            //                }
            //            break;
            //    }
            //}
            //try
            //{
            //    return (Convert.ToInt32(S));
            //}
            //catch
            //{
            //    return 0;
            //}
        }

        /// <summary>
        /// 把一个字符串类型转化为可能的长整型
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static long StringToInt64(string S)
        {
            S = S.Trim();
            if (S.Trim() == "") return (0);

            long lval = 0;
            if (long.TryParse(S, out lval))
                return lval;
            else
                return 0;

            //if (S.IndexOf("-") >= 0)
            //{   //有负号，但不是开头，则转换为0
            //    if (S.StartsWith("-") == false) return (0);
            //    if (S.StartsWith("--")) return (0);
            //}
            //for (int i = 0; i < S.Length; i++)
            //{
            //    switch (S.Substring(i, 1))
            //    {
            //        case "0":
            //        case "1":
            //        case "2":
            //        case "3":
            //        case "4":
            //        case "5":
            //        case "6":
            //        case "7":
            //        case "8":
            //        case "9":
            //            break;
            //        case "-":
            //            if (S.Length == 1) return (0);
            //            break;
            //        default:
            //            if (i == 0)
            //                return (0);
            //            else
            //                try
            //                {
            //                    return (Convert.ToInt64(S.Substring(0, i)));
            //                }
            //                catch
            //                {
            //                    return 0;
            //                }
            //            break;
            //    }
            //}
            //try
            //{
            //    return (Convert.ToInt64(S));
            //}
            //catch
            //{
            //    return 0;
            //}
        }

        /// <summary>
        /// 把一个字符串类型转化为可能的俘点型
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static double StringToDouble(string S)
        {
            S = S.Trim();
            if (S.Trim() == "") return (0);

            double dval = 0;
            if (double.TryParse(S, out dval))
                return dval;
            else
                return 0;

            //bool Flag = false;
            //if (S.IndexOf("-") >= 0)
            //{   //有负号，但不是开头，则转换为0
            //    if (S.StartsWith("-") == false) return (0);
            //    if (S.StartsWith("--")) return (0);
            //}
            //for (int i = 0; i < S.Length; i++)
            //{
            //    switch (S.Substring(i, 1))
            //    {
            //        case "0":
            //        case "1":
            //        case "2":
            //        case "3":
            //        case "4":
            //        case "5":
            //        case "6":
            //        case "7":
            //        case "8":
            //        case "9":
            //            break;
            //        case "-":
            //            if (S.Length == 1) return (0);      //只有一个点的情况
            //            break;
            //        case ".":
            //            if (S.Length == 1) return (0);      //只有一个点的情况
            //            if (Flag == false)
            //            {
            //                Flag = true;
            //            }
            //            else
            //            {   //如果出现2个点
            //                if (i == 0)  //实际不可能 I=0
            //                    return (0);
            //                else         //取这个点之前的内容来进行计算
            //                    try
            //                    {
            //                        return (Convert.ToDouble(S.Substring(0, i)));
            //                    }
            //                    catch
            //                    {
            //                        return (0);
            //                    }
            //            }
            //            break;
            //        default:   //出现非法数字
            //            if (i == 0)
            //                return (0);
            //            else
            //                try
            //                {
            //                    return (Convert.ToDouble(S.Substring(0, i)));
            //                }
            //                catch
            //                {
            //                    return (0);
            //                }
            //    }
            //}

            //try
            //{
            //    return (Convert.ToDouble(S));
            //}
            //catch
            //{
            //    return 0;
            //}

        }

        /// <summary>
        /// 把一个字符串类型转化为可能的俘点型
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static decimal StringToDecimal(string S)
        {
            S = S.Trim();
            if (S.Trim() == "") return (0);

            decimal dval = 0;
            if (decimal.TryParse(S, out dval))
                return dval;
            else
                return 0;

            //bool Flag = false;
            //if (S.IndexOf("-") >= 0)
            //{   //有负号，但不是开头，则转换为0
            //    if (S.StartsWith("-") == false) return (0);
            //    if (S.StartsWith("--")) return (0);
            //}
            //for (int i = 0; i < S.Length; i++)
            //{
            //    switch (S.Substring(i, 1))
            //    {
            //        case "0":
            //        case "1":
            //        case "2":
            //        case "3":
            //        case "4":
            //        case "5":
            //        case "6":
            //        case "7":
            //        case "8":
            //        case "9":
            //            break;
            //        case "-":
            //            if (S.Length == 1) return (0M);
            //            break;
            //        case ".":
            //            if (S.Length == 1) return (0M);      //只有一个点的情况
            //            if (Flag == false)
            //            {
            //                Flag = true;
            //            }
            //            else
            //            {   //如果出现2个点
            //                if (i == 0)  //实际不可能 I=0
            //                    return (0M);
            //                else         //取这个点之前的内容来进行计算
            //                    try
            //                    {
            //                        return (Convert.ToDecimal(S.Substring(0, i)));
            //                    }
            //                    catch
            //                    {
            //                        return 0m;
            //                    }
            //            }
            //            break;
            //        default:   //出现非法数字
            //            if (i == 0)
            //                return (0M);
            //            else
            //                try
            //                {
            //                    return (Convert.ToDecimal(S.Substring(0, i)));
            //                }
            //                catch
            //                {
            //                    return 0m;
            //                }
            //    }
            //}
            //try
            //{
            //    return (Convert.ToDecimal(S));
            //}
            //catch
            //{
            //    return 0m;
            //}
        }

        /// <summary>
        /// 把一个字符串类型转化为可能的日期
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static DateTime StringToDateTime(string S)
        {
            S = S.Trim();
            if (S.Trim() == "") return NullDateTime;

            DateTime dtval = NullDateTime;
            if (DateTime.TryParse(S, out dtval))
                return dtval;
            else
                return NullDateTime;
        }

        /// <summary>
        /// 根据类型字符串转换数据
        /// </summary>
        /// <param name="sData">要转换的数据</param>
        /// <param name="sType">要转换的类型</param>
        /// <returns>转换后的数据</returns>
        public static object ConvertDataByType(string sData, string sType)
        {
            try
            {
                switch (sType.ToLower().Trim())
                {
                    case "tinyint":
                        return (Convert.ToByte(sData));
                    case "smallint":
                        return (Convert.ToInt16(sData));
                    case "int":
                        return (Convert.ToInt32(sData));
                    case "smalldatetime":
                        return (Convert.ToDateTime(sData));
                    case "real":
                        return (Convert.ToDouble(sData));
                    case "money":
                        return (Convert.ToDecimal(sData));
                    case "datetime":
                        return (Convert.ToDateTime(sData));
                    case "float":
                        return (Convert.ToSingle(sData));
                    case "bit":
                        return (Convert.ToInt16(sData));
                    case "decimal":
                        return (Convert.ToDecimal(sData));
                    case "numeric":
                        return (Convert.ToDecimal(sData));
                    case "smallmoney":
                        return (Convert.ToDecimal(sData));
                    case "bigint":
                        return (Convert.ToInt64(sData));
                    case "timestamp":
                        return (Convert.ToDateTime(sData));
                    default:
                        return (sData);
                }
            }
            catch
            {
                return (sData);
            }
        }

        /// <summary>
        /// 根据类型字符串转换为SqlDbType
        /// </summary>
        /// <param name="sType">数据库中的类型</param>
        /// <returns>转换后的类型</returns>
        public static SqlDbType ConvertSqlDbType(string sType)
        {
            switch (sType.ToLower().Trim())
            {
                case "image":
                    return (SqlDbType.Image);
                case "text":
                    return (SqlDbType.Text);
                case "uniqueidentifier":
                    return (SqlDbType.UniqueIdentifier);
                case "tinyint":
                    return (SqlDbType.TinyInt);
                case "smallint":
                    return (SqlDbType.SmallInt);
                case "int":
                    return (SqlDbType.Int);
                case "smalldatetime":
                    return (SqlDbType.SmallDateTime);
                case "real":
                    return (SqlDbType.Real);
                case "money":
                    return (SqlDbType.Money);
                case "datetime":
                    return (SqlDbType.DateTime);
                case "float":
                    return (SqlDbType.Float);
                case "ntext":
                    return (SqlDbType.NText);
                case "bit":
                    return (SqlDbType.Bit);
                case "decimal":
                    return (SqlDbType.Decimal);
                case "numeric":
                    return (SqlDbType.Decimal);
                case "smallmoney":
                    return (SqlDbType.SmallMoney);
                case "bigint":
                    return (SqlDbType.BigInt);
                case "varbinary":
                    return (SqlDbType.VarBinary);
                case "varchar":
                    return (SqlDbType.VarChar);
                case "binary":
                    return (SqlDbType.Binary);
                case "char":
                    return (SqlDbType.Char);
                case "timestamp":
                    return (SqlDbType.Timestamp);
                case "nvarchar":
                    return (SqlDbType.NVarChar);
                case "nchar":
                    return (SqlDbType.NChar);
                default:
                    return (SqlDbType.Variant);
            }
        }

        /// <summary>
        /// 根据SqlDbType转换为类型字符串
        /// </summary>
        /// <param name="sqlType"></param>
        /// <returns></returns>
        public static string ConvertSqlDbType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.Image:
                    return "image";
                case SqlDbType.Text:
                    return "text";
                case SqlDbType.UniqueIdentifier:
                    return "uniqueidentifier";
                case SqlDbType.TinyInt:
                    return "tinyint";
                case SqlDbType.SmallInt:
                    return "smallint";
                case SqlDbType.Int:
                    return "int";
                case SqlDbType.SmallDateTime:
                    return "smalldatetime";
                case SqlDbType.Real:
                    return "real";
                case SqlDbType.Money:
                    return "money";
                case SqlDbType.DateTime:
                    return "datetime";
                case SqlDbType.Float:
                    return "float";
                case SqlDbType.NText:
                    return "ntext";
                case SqlDbType.Bit:
                    return "bit";
                case SqlDbType.Decimal:
                    return "decimal";
                case SqlDbType.SmallMoney:
                    return "smallmoney";
                case SqlDbType.BigInt:
                    return "bigint";
                case SqlDbType.VarBinary:
                    return "varbinary";
                case SqlDbType.VarChar:
                    return "varchar";
                case SqlDbType.Binary:
                    return "binary";
                case SqlDbType.Char:
                    return "char";
                case SqlDbType.Timestamp:
                    return "timestamp";
                case SqlDbType.NVarChar:
                    return "nvarchar";
                case SqlDbType.NChar:
                    return "nchar";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 根据类型字符串转换为OleDbType
        /// </summary>
        /// <param name="sType">数据库中的类型</param>
        /// <returns>转换后的类型</returns>
        public static OleDbType ConvertOleDbType(string sType)
        {
            switch (sType.ToLower().Trim())
            {
                case "image":
                    return (OleDbType.LongVarBinary);
                case "text":
                    return (OleDbType.LongVarChar);
                case "uniqueidentifier":
                    return (OleDbType.Guid);
                case "tinyint":
                    return (OleDbType.TinyInt);
                case "smallint":
                    return (OleDbType.SmallInt);
                case "int":
                    return (OleDbType.Integer);
                case "smalldatetime":
                    return (OleDbType.DBTimeStamp);
                case "real":
                    return (OleDbType.Double);
                case "money":
                    return (OleDbType.Currency);
                case "datetime":
                    return (OleDbType.DBTimeStamp);
                case "float":
                    return (OleDbType.Single);
                case "ntext":
                    return (OleDbType.LongVarWChar);
                case "bit":
                    return (OleDbType.TinyInt);
                case "decimal":
                    return (OleDbType.Decimal);
                case "numeric":
                    return (OleDbType.Numeric);
                case "smallmoney":
                    return (OleDbType.Currency);
                case "bigint":
                    return (OleDbType.BigInt);
                case "varbinary":
                    return (OleDbType.VarBinary);
                case "varchar":
                    return (OleDbType.VarChar);
                case "binary":
                    return (OleDbType.Binary);
                case "char":
                    return (OleDbType.Char);
                case "timestamp":
                    return (OleDbType.DBTimeStamp);
                case "nvarchar":
                    return (OleDbType.VarWChar);
                case "nchar":
                    return (OleDbType.WChar);
                default:
                    return (OleDbType.Variant);
            }
        }

        /// <summary>
        /// 根据类型决定值是否需要在前后加上引号
        /// </summary>
        /// <param name="oType">类型</param>
        /// <param name="oValue">原值</param>
        /// <returns>处理后的字符串值</returns>
        public static string GetSqledValue(Type oType, object oValue)
        {
            switch (oType.ToString())
            {
                case "System.String":
                case "System.Char":
                    return HTBaseFunc.Expr(oValue.ToString());
                case "System.DateTime":
                    return GetDateTimeSqlValue(Convert.ToDateTime(oValue));
                case "System.Byte[]":
                    byte[] bs = (byte[])oValue;
                    string s = "";
                    for (long i = 0; i < bs.LongLength; i++)
                    {
                        s += @"\0x" + bs[i].ToString("X");
                    }
                    return s;
                default:
                    return oValue.ToString();
            }
        }

        /// <summary>
        /// 根据序号从Hashtable中取值
        /// </summary>
        /// <param name="hashtable"></param>
        /// <param name="iPos"></param>
        /// <returns></returns>
        public static object GetHashtableObject(Hashtable hashtable, int iPos)
        {
            if (hashtable == null) return null;
            if (hashtable.Count <= iPos) return null;

            System.Collections.IDictionaryEnumerator ide = hashtable.GetEnumerator() as System.Collections.IDictionaryEnumerator;
            ide.Reset();
            int i = 0;
            while (ide.MoveNext())
            {
                if (i == iPos)
                    return ide.Value;
                else
                    i++;
            }
            return null;
        }

        /// <summary>
        /// 将Hashtable转化为ArrayList
        /// </summary>
        /// <param name="hashtable"></param>
        /// <returns></returns>
        public static ArrayList Hashtable2ArrayList(Hashtable hashtable)
        {
            if (hashtable == null) return null;

            System.Collections.IDictionaryEnumerator ide = hashtable.GetEnumerator() as System.Collections.IDictionaryEnumerator;
            ArrayList arrList = new ArrayList();
            ide.Reset();
            while (ide.MoveNext())
            {
                arrList.Add(ide.Value);
            }
            return arrList;
        }

        /// <summary>
        /// 从两个完整路径中比较出相对路径
        /// </summary>
        /// <param name="asSrcPath"></param>
        /// <param name="asComparePath"></param>
        /// <returns></returns>
        public static string GetRelativelyPath(string asSrcPath, string asComparePath)
        {
            if (HTBaseFunc.RightStr(asSrcPath, 1) == @"\")
                asSrcPath = HTBaseFunc.LeftStrL(asSrcPath, 1);
            if (HTBaseFunc.RightStr(asComparePath, 1) == @"\")
                asComparePath = HTBaseFunc.LeftStrL(asComparePath, 1);

            string[] sSrcPathSplits = asSrcPath.Split('\\');
            string[] sComparePathSplits = asComparePath.Split('\\');
            if (sSrcPathSplits[0].ToUpper() != sComparePathSplits[0].ToUpper())
                return asSrcPath;

            string sDstPath = "";
            int iComparePathPos = -1;
            bool bNotEqual = false;
            for (int i = 0; i < sSrcPathSplits.Length; i++)
            {
                string sSrcPathSect = sSrcPathSplits[i];
                if (bNotEqual)
                {
                    if (sDstPath != "")
                        sDstPath += @"\";
                    sDstPath += sSrcPathSect;
                }
                else
                {
                    if (sComparePathSplits.Length > i)
                    {
                        string sComparePathSect = sComparePathSplits[i];
                        iComparePathPos++;
                        if (sComparePathSect.ToUpper() != sSrcPathSect.ToUpper())
                        {
                            bNotEqual = true;
                            sDstPath += @"..\" + sSrcPathSect;
                        }
                    }
                    else
                    {
                        if (sDstPath != "")
                            sDstPath += @"\";
                        sDstPath += sSrcPathSect;
                    }
                }
            }

            iComparePathPos++;
            if (iComparePathPos < sComparePathSplits.Length)
            {
                for (int i = iComparePathPos; i < sComparePathSplits.Length; i++)
                {
                    sDstPath = @"..\" + sDstPath;
                }
            }

            return sDstPath;

        }

        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetDeepCopy(object obj)
        {
            Object DeepCopyObj;
            if (obj == null)
                DeepCopyObj = null;
            else if (obj.GetType().IsValueType)
                DeepCopyObj = obj;
            else
            {
                DeepCopyObj = System.Activator.CreateInstance(obj.GetType());
                System.Reflection.MemberInfo[] memberCollection = obj.GetType().GetMembers();
                foreach (System.Reflection.MemberInfo member in memberCollection)
                {
                    if (member.MemberType == System.Reflection.MemberTypes.Field)
                    {
                        System.Reflection.FieldInfo field = (System.Reflection.FieldInfo)member;
                        Object fieldValue = field.GetValue(obj);
                        if (fieldValue != null)
                        {
                            if (fieldValue is IDictionary)
                            {
                                Object fieldValueCopy = System.Activator.CreateInstance(field.FieldType);
                                field.SetValue(DeepCopyObj, fieldValueCopy);
                                foreach (DictionaryEntry de in (IDictionary)fieldValue)
                                {
                                    ((IDictionary)fieldValueCopy).Add(de.Key, GetDeepCopy(de.Value));
                                }
                            }
                            else if (fieldValue is IList)
                            {
                                Object fieldValueCopy;
                                if (fieldValue is Array)
                                    fieldValueCopy = System.Activator.CreateInstance(field.FieldType, ((IList)fieldValue).Count);
                                else
                                    fieldValueCopy = System.Activator.CreateInstance(field.FieldType);
                                field.SetValue(DeepCopyObj, fieldValueCopy);
                                for (int i = 0; i < ((IList)fieldValue).Count; i++)
                                {
                                    object le = ((IList)fieldValue)[i];
                                    if (fieldValue is Array)
                                        ((IList)fieldValueCopy)[i] = GetDeepCopy(le);
                                    else
                                        ((IList)fieldValueCopy).Add(GetDeepCopy(le));
                                }
                            }
                            else if (fieldValue is ICloneable)
                                field.SetValue(DeepCopyObj, (fieldValue as ICloneable).Clone());
                            else
                                field.SetValue(DeepCopyObj, GetDeepCopy(fieldValue));
                        }
                        else
                            field.SetValue(DeepCopyObj, null);
                    }
                }

            }
            return DeepCopyObj;
        }
        
        #endregion

        #region 控件操作类

        /// <summary>
        /// 定位当前控件所在容器中的下一个控件
        /// ScrollToFirst表示当当前控件是最后一个时是否定位第一个控件
        /// </summary>
        /// <param name="currentcontrol"></param>
        /// <param name="ScrollToFirst"></param>
        /// <returns></returns>
        public static Control FocusNextControl(Control currentcontrol, bool ScrollToFirst)
        {
            Control ct;
            ct = currentcontrol;
        loop1:
            ct = currentcontrol.Parent.GetNextControl(ct, true);
            if (ct == null)
            {
                if (ScrollToFirst)
                    return FocusFirstControl(currentcontrol.Parent);
                else
                    return null;
            }
            else
            {
                if (ct.TabStop && ct.Visible && ct.Enabled)
                {
                    ct.Focus();
                    return ct;
                }
                else
                {
                    goto loop1;
                }
            }
        }

        /// <summary>
        /// 定位当前控件所在容器中的前一个控件
        /// </summary>
        /// <param name="currentcontrol"></param>
        /// <returns></returns>
        public static Control FocusPriorControl(Control currentcontrol)
        {
            Control ct;
            ct = currentcontrol;
        loop1:
            ct = currentcontrol.Parent.GetNextControl(ct, false);
            if (ct == null)
                return null;
            else
            {
                if (ct.TabStop && ct.Visible && ct.Enabled)
                {
                    ct.Focus();
                    return ct;
                }
                else
                {
                    goto loop1;
                }
            }
        }

        /// <summary>
        /// 定位当前控件所在容器中第一个控件
        /// </summary>
        /// <param name="Parent"></param>
        /// <returns></returns>
        public static Control FocusFirstControl(Control Parent)
        {
            //定位到第一个控件
            Control ct, lastct = null;
            if (Parent.Controls.Count > 0)
            {
                ct = Parent.Controls[0];
                if (ct.TabStop && ct.Visible && ct.Enabled) lastct = ct;
            loop1:
                ct = Parent.GetNextControl(ct, false);
                if (ct == null)
                {
                    if (lastct != null) lastct.Focus();
                    return lastct;
                }
                else
                {
                    if (ct.TabStop && ct.Visible && ct.Enabled) lastct = ct;
                    goto loop1;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ComboBox文本改变时自动选取后面的内容
        /// </summary>
        /// <param name="comb"></param>
        public static void ComboboxTextChanged(ComboBox comb)
        {
            try
            {
                int fin = comb.FindString(comb.Text);
                comb.SelectedIndex = fin;
                if (comb.Text.Length > 0)
                {
                    int startpo = comb.Text.Length - 1;
                    int setlength = comb.Text.Length - comb.SelectionStart;
                    comb.Select(startpo, setlength);
                }
            }
            catch { }
        }

        #endregion

        #region 检查数据类

        /// <summary>
        /// 检查Email是否有效
        /// </summary>
        /// <param name="sText">Email地址</param>
        /// <param name="是否必须"></param>
        /// <returns></returns>
        public static bool ChkEmail(string sText, bool 是否必须, bool 显示错误)
        {
            if (sText == "")
            {
                if (是否必须 == true)
                {
                    _lasterror = "Email地址不允许为空！";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
            }
            else
            {
                if (sText.IndexOf("@") <= 0 || sText.IndexOf(".") <= 0)
                {
                    _lasterror = "Email地址必须包括@和. 请确认！";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
                else
                {
                    string sReg = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                    if (Regex.IsMatch(sText, sReg) == false)
                    {
                        _lasterror = "无效的Email地址！请确认。";
                        if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 检查Email是否有效
        /// </summary>
        /// <param name="sText">Email地址</param>
        /// <returns></returns>
        public static bool ChkEmail(string sText)
        {
            return ChkEmail(sText, false, true);
        }

        /// <summary>
        /// 检查邮政编码是否有效
        /// </summary>
        /// <param name="sText">邮政编码</param>
        /// <param name="是否必须"></param>
        /// <returns></returns>
        public static bool ChkPostalCode(string sText, bool 是否必须, bool 显示错误)
        {
            if (sText == "")
            {
                if (是否必须 == true)
                {
                    _lasterror = "邮政编码不允许为空！";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
            }
            else
            {
                if (sText.Length != 6)
                {
                    _lasterror = "邮政编码长度必须为6位！请确认。";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
                else
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if ("0123456789".IndexOf(sText.Substring(i, 1)) < 0)
                        {
                            _lasterror = "无效的邮政编码！请确认。";
                            if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 检查邮编是否有效
        /// </summary>
        /// <param name="sText">邮编</param>
        /// <returns></returns>
        public static bool ChkPostalCode(string sText)
        {
            return ChkPostalCode(sText, false, true);
        }

        /// <summary>
        /// 检查电话号码是否有效
        /// </summary>
        /// <param name="sText">电话号码</param>
        /// <param name="是否必须"></param>
        /// <returns></returns>
        public static bool ChkPhoneCode(string sText, bool 是否必须, bool 显示错误)
        {
            if (sText == "")
            {
                if (是否必须 == true)
                {
                    _lasterror = "电话号码不允许为空！";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < sText.Length; i++)
                {
                    if ("0123456789 (-*/,;:?%#)\\+".IndexOf(sText.Substring(i, 1)) < 0)
                    {
                        _lasterror = "无效的电话号码！请确认。";
                        if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 检查电话号码是否有效
        /// </summary>
        /// <param name="sText">电话号码</param>
        /// <returns></returns>
        public static bool ChkPhoneCode(string sText)
        {
            return ChkPhoneCode(sText, false, true);
        }

        /// <summary>
        /// 检查身份证号是否有效
        /// </summary>
        /// <param name="sText">身份证号</param>
        /// <param name="是否必须"></param>
        /// <returns></returns>
        public static bool ChkIdentityCard(string sText, bool 是否必须, bool 显示错误)
        {
            if (sText == "")
            {
                if (是否必须 == true)
                {
                    _lasterror = "身份证号不允许为空！";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
            }
            else
            {
                if (sText.Length != 15 && sText.Length != 18)
                {
                    _lasterror = "身份证号长度不正确！请确认。";
                    if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                    return false;
                }
                else
                {
                    //身份证号前17位数字最后一位字母
                    for (int i = 0; i < sText.Length - 1; i++)
                    {
                        if ("0123456789".IndexOf(sText.Substring(i, 1)) < 0)
                        {
                            _lasterror = "无效的身份证号！请确认。";
                            if (显示错误) HTBaseFunc.MsgWarning(_lasterror);
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 检查身份证号是否有效
        /// </summary>
        /// <param name="sText">身份证号</param>
        /// <returns></returns>
        public static bool ChkIdentityCard(string sText)
        {
            return ChkIdentityCard(sText, false, true);
        }

        #endregion

        #region 字符处理类

        /// <summary>
        /// 按实际长度连接字符串,若长度不够右边补空格 Flag 默认 true
        /// </summary>
        /// <param name="Str">原来的字符串</param>
        /// <param name="Length">长度</param>
        /// <returns></returns>
        public static string LStrB(string Str, int Length)
        {
            return LStrB(Str, Length, true);
        }

        /// <summary>
        /// 按实际长度连接字符串,若长度不够右边补空格 Flag是否补空格
        /// </summary>
        /// <param name="Str">待处理字符串</param>
        /// <param name="Length">长度</param>
        /// <param name="Flag">是否补空格</param>
        /// <returns></returns>
        public static string LStrB(string Str, int Length, bool Flag)
        {
            string T = MidStrB(Str, 0, Length);
            int L = LenB(T);
            if (L < Length && Flag)
                return T.PadRight(Length - L + T.Length, ' ');
            else
                return T;
        }

        /// <summary>
        /// 连续按实际长度获取字符串中的数据 从第1位开始计算start
        /// </summary>
        /// <param name="Str">字符串</param>
        /// <param name="Start">起始位</param>
        /// <param name="Length">截取长度</param>
        /// <returns></returns>
        public static string MidStrB(string Str, int Start, int Length)
        {
            int L = LenB(Str);
            if (L <= Start) return "";

            byte[] strBytes = System.Text.Encoding.Default.GetBytes(Str);
            if (Start + Length > L)
                return System.Text.Encoding.Default.GetString(strBytes, Start, L - Start);
            else
                return System.Text.Encoding.Default.GetString(strBytes, Start, Length);
        }

        /// <summary>
        /// //字符串长度包括汉字两位
        /// </summary>
        /// <param name="Str">字符串</param>
        /// <returns>汉字字符串的字节长度</returns>
        public static int LenB(string Str)
        {
            return System.Text.Encoding.Default.GetByteCount(Str);
        }

        /// <summary>
        /// 字符串统一去空格 大写转换
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String UTrim(string Str)
        {
            return Str.Trim().ToUpper();
        }

        /// <summary>
        /// 字符串统一去空格 小写转换
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String LTrim(string Str)
        {
            return Str.Trim().ToLower();
        }

        /// <summary>
        /// 从字符串strs中以dpts为分隔符读取第nos个分割串
        /// strs的格式为：<str>[<dpts><str>]
        /// strs中第一个分割串的nos为0
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="nos"></param>
        /// <returns></returns>
        public static String DepartStr(String strs, String dpts, int nos)
        {
            String departedstr = "";
            int i, pos1, pos2, lenofdpts;
            //strs = strs.Trim();
            //dpts = dpts.Trim();
            lenofdpts = dpts.Length;
            if (nos > 0)
            {
                i = 0;
                pos2 = -lenofdpts;
            loop1:
                pos1 = PosStr(strs, dpts, pos2 + lenofdpts);
                if (pos1 >= 0)
                {
                    i = i + 1;
                    pos2 = pos1;
                    if (i == nos)
                    {
                        pos1 = PosStr(strs, dpts, pos2 + lenofdpts);
                        if (pos1 >= 0)
                            departedstr = MidStr(strs, pos2 + lenofdpts, pos1 - pos2 - lenofdpts);
                        else
                            departedstr = MidStr(strs, pos2 + lenofdpts, strs.Length - pos2 - lenofdpts);
                    }
                    else if (i < nos)
                        goto loop1;
                }
                else
                    departedstr = "";
            }
            else if (nos == 0)
            {
                pos1 = PosStr(strs, dpts);
                if (pos1 >= 0)
                    departedstr = MidStr(strs, 0, pos1);
                else
                    departedstr = strs;
            }
            else
                departedstr = "";
            return departedstr;
        }

        /// <summary>
        /// 从字符串strs中以分号为分隔符读取第nos个分割串
        /// strs的格式为：<str>[;<str>]
        /// strs中第一个分割串的nos为0
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="nos"></param>
        /// <returns></returns>
        public static String DepartStr(String strs, int nos)
        {
            return DepartStr(strs, ";", nos);
        }

        /// <summary>
        /// 从字符串strs中取从startp位置开始遇到的第一个subs所在的位置
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="subs"></param>
        /// <param name="startp"></param>
        /// <returns></returns>
        public static int PosStr(String strs, String subs, int startp)
        {
            if (startp >= strs.Length)
                return -1;
            else
                return strs.IndexOf(subs, startp);
        }

        /// <summary>
        /// 从字符串strs中取遇到的第一个subs所在的位置
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="subs"></param>
        /// <returns></returns>
        public static int PosStr(String strs, String subs)
        {
            return PosStr(strs, subs, 0);
        }

        /// <summary>
        /// 从字符串strs中取从startp位置开始遇到的最后一个subs所在的位置
        /// 该函数其实是取subs从右边开始的位置
        /// startp表示strs中左边有效的字符位置，结果位置一定小于等于startp
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="subs"></param>
        /// <param name="startp"></param>
        /// <returns></returns>
        public static int DownPosStr(String strs, String subs, int startp)
        {
            int li_pos1;
            int li_pos2 = -1;
            do
            {
                li_pos1 = li_pos2;
                li_pos2 = PosStr(strs, subs, li_pos2 + 1);
            }
            while ((li_pos2 >= 0) && (li_pos2 <= startp));
            return li_pos1;
        }

        /// <summary>
        /// 从字符串strs中取遇到的最后一个subs所在的位置
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="subs"></param>
        /// <returns></returns>
        public static int DownPosStr(String strs, String subs)
        {
            int li_pos1;
            int li_pos2 = -1;
            do
            {
                li_pos1 = li_pos2;
                li_pos2 = PosStr(strs, subs, li_pos2 + 1);
            }
            while (li_pos2 >= 0);
            return li_pos1;
        }

        /// <summary>
        /// 累加lenn个字符串srcs返回，同RepeatStr
        /// 如FillStr("0",3)返回"000"
        /// </summary>
        /// <param name="srcs"></param>
        /// <param name="lenn"></param>
        /// <returns></returns>
        public static String FillStr(String srcs, int lenn)
        {
            int i;
            String rtns = "";
            for (i = 1; i <= lenn; i++)
            {
                rtns = rtns + srcs;
            }
            return rtns;
        }

        /// <summary>
        /// 累加lenn个字符串srcs返回，同FillStr
        /// 如RepeatStr("0",3)返回"000"
        /// </summary>
        /// <param name="srcs"></param>
        /// <param name="lenn"></param>
        /// <returns></returns>
        public static String RepeatStr(String srcs, int lenn)
        {
            return FillStr(srcs, lenn);
        }

        /// <summary>
        /// 取字符串srcs左边lenn个字符组成的串
        /// </summary>
        /// <param name="srcs"></param>
        /// <param name="lenn"></param>
        /// <returns></returns>
        public static String LeftStr(String srcs, int lenn)
        {
            return MidStr(srcs, 0, lenn);
        }

        /// <summary>
        /// 取字符串srcs右边lenn个字符组成的串
        /// </summary>
        /// <param name="srcs"></param>
        /// <param name="lenn"></param>
        /// <returns></returns>
        public static String RightStr(String srcs, int lenn)
        {
            if (srcs.Length >= lenn)
                return MidStr(srcs, srcs.Length - lenn);
            else
                return srcs;
        }

        /// <summary>
        /// 取字符串srcs从startp开始的lenn个长度字符组成的字符串
        /// </summary>
        /// <param name="srcs"></param>
        /// <param name="startp"></param>
        /// <param name="lenn"></param>
        /// <returns></returns>
        public static String MidStr(String srcs, int startp, int lenn)
        {
            if ((startp >= 0) && (lenn > 0) && (srcs.Length >= startp))
            {
                if (lenn > srcs.Length) lenn = srcs.Length;
                return srcs.Substring(startp, lenn);
            }
            else
                return "";
        }

        /// <summary>
        /// 取字符串srcs从startp开始到结尾的字符组成的字符串
        /// </summary>
        /// <param name="srcs"></param>
        /// <param name="startp"></param>
        /// <returns></returns>
        public static String MidStr(String srcs, int startp)
        {
            if ((startp >= 0) && (srcs.Length >= startp))
                return srcs.Substring(startp);
            else
                return "";
        }

        /// <summary>
        /// 将数值代码字符串codestr前面添加"0"使其达到codelen个字符长度后输出
        /// 如果codestr的长度大于等于codelen则直接输出codestr
        /// </summary>
        /// <param name="codestr"></param>
        /// <param name="codelen"></param>
        /// <returns></returns>
        public static String FillCode(String codestr, int codelen)
        {
            long codeint;
            String codetmp;
            codeint = Convert.ToInt64(codestr);
            codestr = codeint.ToString().Trim();
            codetmp = FillStr("0", codelen - codestr.Length) + codestr;
            return codetmp;
        }

        /// <summary>
        /// 在字符串sText的前或后补字符cPadChar
        /// bUntilWidth表示补字符的长度iWidth是要达到的长度，还是要补的长度
        /// bPadTail表示是否将字符补到sText的后面
        /// </summary>
        /// <param name="sText"></param>
        /// <param name="cPadChar"></param>
        /// <param name="iWidth"></param>
        /// <param name="bUntilWidth"></param>
        /// <param name="bPadTail"></param>
        /// <returns></returns>
        public static String PadChar(String sText, char cPadChar, int iWidth, bool bUntilWidth, bool bPadTail)
        {
            if (!bPadTail)
            {
                if (bUntilWidth)
                    return FillStr(cPadChar.ToString(), iWidth - sText.Length) + sText;
                else
                    return FillStr(cPadChar.ToString(), iWidth) + sText;
            }
            else
            {
                if (bUntilWidth)
                    return sText + FillStr(cPadChar.ToString(), iWidth - sText.Length);
                else
                    return sText + FillStr(cPadChar.ToString(), iWidth);
            }
        }

        /// <summary>
        /// 在字符串sText的前补空格
        /// bUntilWidth表示补字符的长度iWidth是要达到的长度，还是要补的长度
        /// bPadTail表示是否将字符补到sText的后面
        /// </summary>
        /// <param name="sText"></param>
        /// <param name="iWidth"></param>
        /// <returns></returns>
        public static String PadSpace(String sText, int iWidth, bool bUntilWidth, bool bPadTail)
        {
            return PadChar(sText, Chr(" "), iWidth, bUntilWidth, bPadTail);
        }

        /// <summary>
        /// 从字符串text中截取除右边leftlength个长度的字符串
        /// </summary>
        /// <param name="atext"></param>
        /// <param name="aleftlength"></param>
        /// <returns></returns>
        public static String LeftStrL(String text, int leftlength)
        {
            //获取多字节左边串，但长度参数是右边被排除串的长度
            if (text.Length > leftlength)
                return MidStr(text, 0, text.Length - leftlength);
            else
                return "";
        }

        /// <summary>
        /// 从字符串text中截取除左边leftlength个长度的字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="leftlength"></param>
        /// <returns></returns>
        public static String RightStrL(String text, int leftlength)
        {
            return MidStr(text, leftlength);
        }

        /// <summary>
        /// 从字符串strs中取遇到的最后一个subs所在的位置
        /// </summary>
        /// <param name="parentstr"></param>
        /// <param name="substr"></param>
        /// <returns></returns>
        public static int PosStrL(String parentstr, String substr)
        {
            return PosStrL(parentstr, substr, parentstr.Length - 1);
        }

        /// <summary>
        /// 从字符串parentstr中取从start位置开始遇到的最后一个substr所在的位置，同DownPosStr
        /// 该函数其实是取substr从右边开始的位置
        /// start表示parentstr中左边有效的字符位置，结果位置一定小于等于start
        /// </summary>
        /// <param name="parentstr"></param>
        /// <param name="substr"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int PosStrL(String parentstr, String substr, int start)
        {
            int li_pos1;
            int li_pos2 = -1;
            parentstr = LeftStr(parentstr, start + 1);
            do
            {
                li_pos1 = li_pos2;
                li_pos2 = PosStr(parentstr, substr, li_pos2 + 1);
            }
            while ((li_pos2 >= 0) && (li_pos2 <= start));
            return li_pos1;
        }

        /// <summary>
        /// 判断字符串substr是否是parentstr的字串
        /// </summary>
        /// <param name="parentstr"></param>
        /// <param name="substr"></param>
        /// <returns></returns>
        public static bool IsSubString(String parentstr, String substr)
        {
            if (PosStr(parentstr, substr) >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 判断是否空字符串，若是则返回dftstr，否则返回原值
        /// 本函数将为null和只有空格的字符串都看做空字符串
        /// </summary>
        /// <param name="srcstr"></param>
        /// <param name="dftstr"></param>
        /// <returns></returns>
        public static String IsEmptyString(String srcstr, String dftstr)
        {
            if ((srcstr == null) || (srcstr.Trim() == ""))
                return dftstr;
            else
                return srcstr;
        }

        /// <summary>
        /// 从字符串srcstr中替换从startpos开始的replacelen个长度的字符为replaces
        /// </summary>
        /// <param name="srcstr"></param>
        /// <param name="startpos"></param>
        /// <param name="replacelen"></param>
        /// <param name="replaces"></param>
        /// <returns></returns>
        public static String ReplaceStr(String srcstr, int startpos, int replacelen, String replaces)
        {
            if (startpos < 0) startpos = 0;
            if ((startpos >= srcstr.Length) || (replacelen <= 0)) return srcstr;
            if (startpos + replacelen >= srcstr.Length)
                return LeftStr(srcstr, startpos) + replaces;
            else
                return LeftStr(srcstr, startpos) + replaces + MidStr(srcstr, startpos + replacelen);
        }

        /// <summary>
        /// 从字符串srcstr中将从startpos开始的所有replaceds替换为replaces
        /// </summary>
        /// <param name="srcstr"></param>
        /// <param name="replaceds"></param>
        /// <param name="replaces"></param>
        /// <param name="startpos"></param>
        /// <returns></returns>
        public static String ReplaceStrAll(String srcstr, String replaceds, String replaces, int startpos)
        {
            return ReplaceStrAll(srcstr, replaceds, replaces, startpos, -1);
        }

        /// <summary>
        /// 从字符串srcstr中将所有replaceds替换为replaces
        /// </summary>
        /// <param name="srcstr"></param>
        /// <param name="replaceds"></param>
        /// <param name="replaces"></param>
        /// <returns></returns>
        public static String ReplaceStrAll(String srcstr, String replaceds, String replaces)
        {
            return ReplaceStrAll(srcstr, replaceds, replaces, 0);
        }

        /// <summary>
        /// 从字符串srcstr中将从startpos开始的所有replaceds替换为replaces，但替换数量不能超过replacecount
        /// </summary>
        /// <param name="srcstr"></param>
        /// <param name="replaceds"></param>
        /// <param name="replaces"></param>
        /// <param name="startpos"></param>
        /// <param name="replacecount"></param>
        /// <returns></returns>
        public static String ReplaceStrAll(String srcstr, String replaceds, String replaces, int startpos, int replacecount)
        {
            String leftstrs = LeftStr(srcstr, startpos);
            srcstr = MidStr(srcstr, startpos);
            int pos1 = -1;
            int pos2;
            int cnt = 0;
            int lenofreplaces = replaces.Length;
            int lenofreplaceds = replaceds.Length;
            while ((cnt < replacecount) || (replacecount < 0))
            {
                pos2 = PosStr(srcstr, replaceds, pos1 + 1);
                if (pos2 >= 0)
                {
                    pos1 = pos2 + lenofreplaces;
                    srcstr = LeftStr(srcstr, pos2) + replaces + MidStr(srcstr, pos2 + lenofreplaceds);
                    cnt++;
                }
                else
                    break;
            }

            return leftstrs + srcstr;
        }

        /// <summary>
        /// 将一个整数Ascii码转化为一个字符
        /// </summary>
        /// <param name="ascode"></param>
        /// <returns></returns>
        public static char Chr(int ascode)
        {
            return (char)ascode;
        }

        /// <summary>
        /// 从字符串str中取出第一个字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char Chr(string str)
        {
            return Chr(str, 0);
        }

        /// <summary>
        /// 从字符串str中取出第chrpos个字符
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chrpos"></param>
        /// <returns></returns>
        public static char Chr(string str, int chrpos)
        {
            return str.ToCharArray(chrpos, 1)[0];
        }

        /// <summary>
        /// 获取字符chr的Ascii码
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public static int Asc(char chr)
        {
            return Convert.ToInt32(chr);
        }

        /// <summary>
        /// 获取字符串str中第一个字符的Ascii码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int Asc(string str)
        {
            return Asc(str, 0);
        }

        /// <summary>
        /// 获取字符串str中第chrpos个字符的Ascii码
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chrpos"></param>
        /// <returns></returns>
        public static int Asc(string str, int chrpos)
        {
            return Asc(str.ToCharArray(chrpos, 1)[0]);
        }

        /// <summary>
        /// 从字符串strs中取出标识为ids的值，其中dpts为分隔符，equs为等值符 
        /// 字符串strs的格式为：<name><equs><value>[<dpts><name><equs><value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="equs"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static String GetParmStr(String strs, String dpts, String equs, String ids)
        {
            int pos0, pos1, pos2;
            String poss;
            strs = dpts + strs;
            pos0 = PosStr(strs.ToUpper(), (dpts + ids + equs).ToUpper());
            if (pos0 >= 0)
            {
                pos1 = pos0 + ((string)(dpts + ids + equs)).Length;
                pos2 = PosStr(strs, dpts, pos1);
                if (pos2 >= 0)
                    poss = MidStr(strs, pos1, pos2 - pos1);
                else
                    poss = MidStr(strs, pos1);
            }
            else
                poss = ""; //"<empty>"
            return poss;
        }

        /// <summary>
        /// 从字符串strs中取出标识为ids的值，其中dpts为分隔符，等值符为"="
        /// 字符串strs的格式为：<name>=<value>[<dpts><name>=<value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static String GetParmStr(String strs, String dpts, String ids)
        {
            return GetParmStr(strs, dpts, "=", ids);
        }

        /// <summary>
        /// 从字符串strs中取出标识为ids的值，其中分隔符为";"，等值符为"="
        /// 字符串strs的格式为：<name>=<value>[;<name>=<value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static String GetParmStr(String strs, String ids)
        {
            return GetParmStr(strs, ";", ids);
        }

        /// <summary>
        /// 将字符串srs中标识为ids的值设置为vals，其中dpts为分隔符，equs为等值符
        /// 字符串strs的格式为：<name><equs><value>[<dpts><name><equs><value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="equs"></param>
        /// <param name="ids"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static String SetParmStr(String strs, String dpts, String equs, String ids, String vals)
        {
            int pos0, pos1, pos2;
            strs = dpts + strs;
            pos0 = PosStr(strs.ToUpper(), (dpts + ids + equs).ToUpper());
            if (pos0 >= 0)
            {
                pos1 = pos0 + ((string)(dpts + ids + equs)).Length;
                pos2 = PosStr(strs, dpts, pos1);
                if (pos2 >= 0)
                    strs = LeftStr(strs, pos1) + vals + MidStr(strs, pos2);
                else
                    strs = LeftStr(strs, pos1) + vals;
            }
            else
            {
                if (strs == dpts)
                    strs = dpts + ids + equs + vals;
                else
                    strs = strs + dpts + ids + equs + vals;
            }
            strs = MidStr(strs, dpts.Length);
            return strs;
        }

        /// <summary>
        /// 将字符串srs中标识为ids的值设置为vals，其中dpts为分隔符，等值符为"="
        /// 字符串strs的格式为：<name>=<value>[<dpts><name>=<value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="ids"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static String SetParmStr(String strs, String dpts, String ids, String vals)
        {
            return SetParmStr(strs, dpts, "=", ids, vals);
        }

        /// <summary>
        /// 将字符串srs中标识为ids的值设置为vals，其中分隔符为";"，等值符为"="
        /// 字符串strs的格式为：<name>=<value>[;<name>=<value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="ids"></param>
        /// <param name="vals"></param>
        /// <returns></returns>
        public static String SetParmStr(String strs, String ids, String vals)
        {
            return SetParmStr(strs, ";", ids, vals);
        }

        /// <summary>
        /// 将字符串srs中标识为ids的值删除，其中dpts为分隔符，equs为等值符
        /// 字符串strs的格式为：<name><equs><value>[<dpts><name><equs><value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="equs"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static String DelParmStr(String strs, String dpts, String equs, String ids)
        {
            int pos0, pos1, pos2;
            strs = dpts + strs;
            pos0 = PosStr(strs.ToUpper(), (dpts + ids + equs).ToUpper());
            if (pos0 >= 0)
            {
                pos1 = pos0 + ((string)(dpts + ids + equs)).Length;
                pos2 = PosStr(strs, dpts, pos1);
                if (pos2 >= 0)
                {
                    if (pos0 >= 1)
                        strs = LeftStr(strs, pos0) + MidStr(strs, pos2);
                    else
                        strs = MidStr(strs, pos2);
                }
                else
                {
                    if (pos0 >= 1)
                        strs = LeftStr(strs, pos0);
                    else
                        strs = dpts;
                }
            }
            strs = MidStr(strs, dpts.Length);
            return strs;
        }

        /// <summary>
        /// 将字符串srs中标识为ids的值删除，其中dpts为分隔符，等值符为"="
        /// 字符串strs的格式为：<name>=<value>[<dpts><name>=<value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="dpts"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static String DelParmStr(String strs, String dpts, String ids)
        {
            return DelParmStr(strs, dpts, "=", ids);
        }

        /// <summary>
        /// 将字符串srs中标识为ids的值删除，其中分隔符为";"，等值符为"="
        /// 字符串strs的格式为：<name>=<value>[;<name>=<value>]
        /// </summary>
        /// <param name="strs"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static String DelParmStr(String strs, String ids)
        {
            return DelParmStr(strs, ";", ids);
        }

        /// <summary>
        /// 将汉字字符串hzs转化为拼音首拼，如果中间有非汉字则直接输出
        /// </summary>
        /// <param name="hzs"></param>
        /// <returns></returns>
        public static String GetHzPY(String hzs)
        {
            if (hzs.Trim() == "") return "";

            String[] pys = new String[27];
            pys[1] = "ＡａAa吖阿啊锕嗄哎哀唉埃挨锿捱皑癌嗳矮蔼霭艾爱砹隘嗌嫒碍暧瑷安桉氨庵谙鹌鞍俺埯铵揞犴岸按案胺暗黯肮昂盎凹坳敖嗷廒獒遨熬翱聱螯鳌鏖拗袄媪岙傲奥骜澳懊鏊谄腌";
            pys[2] = "ＢｂBb八巴叭扒吧岜芭疤捌笆粑拔茇菝跋魃把钯靶坝爸罢鲅霸灞掰白百佰柏捭摆呗败拜稗扳班般颁斑搬瘢癍阪坂板版钣舨办半伴扮拌绊瓣邦帮梆浜绑榜膀蚌傍棒谤蒡磅镑包孢苞胞煲龅褒雹宝饱保鸨堡葆褓报抱豹趵鲍暴爆陂卑杯悲碑鹎北贝狈邶备背钡倍悖被惫焙辈碚蓓褙鞴鐾奔贲锛本苯畚坌笨崩绷嘣甭泵迸甏蹦逼荸鼻匕比吡妣彼秕俾笔舭鄙币必毕闭庇畀哔毖荜陛毙狴铋婢庳敝萆弼愎筚滗痹蓖裨跸弊碧箅蔽壁篦薜避濞臂髀璧襞边砭笾编煸蝙鳊鞭贬扁窆匾碥褊卞弁忭汴苄便变缏遍辨辩辫杓彪标飑髟骠膘瘭镖飙飚镳表婊裱鳔憋鳖别蹩瘪宾彬傧斌滨缤槟镔濒豳摈殡膑髌鬓冰兵丙邴秉柄炳饼禀并病摒拨波玻剥钵饽啵脖菠播伯孛驳帛泊勃亳钹铂舶博渤鹁搏箔膊踣薄礴跛簸擘檗逋钸晡醭卜卟补哺捕不布步怖钚部埠瓿簿玢缭耙芘埤";
            pys[3] = "ＣｃCc嚓擦礤猜才材财裁采彩睬踩菜蔡参骖餐残蚕惭惨黪灿粲璨仓伧沧苍舱藏操糙曹嘈漕槽艚螬草册侧厕恻测策岑涔噌层蹭叉杈插馇锸查茬茶搽猹槎察碴檫衩镲汊岔诧姹差拆钗侪柴豺虿瘥觇掺搀婵谗孱禅馋缠蝉廛潺镡蟾躔产铲阐蒇冁忏颤羼伥昌娼猖菖阊鲳长肠苌尝偿常徜嫦厂场昶惝敞怅畅倡鬯唱抄怊钞焯超晁巢朝嘲潮吵炒耖车砗扯彻坼掣撤澈抻郴琛嗔尘臣忱沉辰陈宸晨谌碜闯衬称龀趁榇谶柽蛏铛撑瞠丞成呈承枨诚城乘埕铖惩程裎塍酲澄橙逞骋秤吃哧蚩鸱眵笞嗤媸痴螭魑弛池驰迟茌持匙墀踟篪尺侈齿耻豉褫彳叱斥赤饬炽翅敕啻傺瘛充冲忡茺舂憧艟虫崇宠铳抽瘳仇俦帱绸畴愁稠筹酬踌雠丑瞅臭出初樗刍除厨滁锄蜍雏橱躇蹰杵础储楮楚褚亍处怵绌搐触憷黜矗搋揣啜嘬踹巛川氚穿传舡船遄椽舛喘串钏囱疮窗床创怆吹炊垂陲捶棰槌锤春椿蝽纯唇莼淳鹑醇蠢踔戳绰辍龊呲疵词祠茈茨瓷慈辞磁雌鹚糍此次刺赐从匆苁枞葱骢璁聪丛淙琮凑楱腠辏粗徂殂促猝酢蔟醋簇蹙蹴汆撺镩蹿窜篡爨崔催摧榱璀脆啐悴淬萃毳瘁粹翠村皴存忖寸搓磋撮蹉嵯痤矬鹾脞厝挫措锉错坻廾浒诿忄楂膪澶骣幢隹";
            pys[4] = "ＤｄDd哒耷搭嗒褡达妲怛沓笪答瘩靼鞑打大呆呔歹傣代岱甙绐迨带待怠殆玳贷埭袋逮戴黛丹单担眈耽郸聃殚瘅箪儋胆疸掸旦但诞啖弹惮淡萏蛋氮澹当裆挡党谠凼宕砀荡档菪刀叨忉氘导岛倒捣祷蹈到悼焘盗道稻纛得锝德的灯登噔簦蹬等戥邓凳嶝瞪磴镫低羝堤嘀滴镝狄籴迪敌涤荻笛觌嫡氐诋邸底抵柢砥骶地弟帝娣递第谛棣睇缔蒂碲嗲掂滇颠巅癫典点碘踮电佃甸阽坫店垫玷钿惦淀奠殿靛癜簟刁叼凋貂碉雕鲷吊钓调掉铞爹跌迭垤瓞谍喋堞揲耋叠牒碟蝶蹀鲽丁仃叮玎疔盯钉耵酊顶鼎订定啶腚碇锭丢铥东冬咚岽氡鸫董懂动冻侗垌峒恫栋洞胨胴硐都兜蔸篼斗抖陡蚪豆逗痘窦嘟督毒读渎椟牍犊黩髑独笃堵赌睹芏妒杜肚度渡镀蠹端短段断缎椴煅锻簖堆队对兑怼碓憝镦吨敦墩礅蹲盹趸囤沌盾砘钝顿遁多咄哆裰夺铎掇踱朵哚垛缍躲剁沲堕舵惰跺冂缈肀肜骀赕绨铫町铤柁佚翟丶";
            pys[5] = "ＥｅEe屙讹俄娥峨莪锇鹅蛾额婀厄呃扼苊轭垩恶饿谔鄂阏愕萼遏腭锷鹗颚噩鳄恩蒽摁儿而鸸鲕尔耳迩洱饵珥铒二佴贰诶";
            pys[6] = "ＦｆFf勹发乏伐垡罚阀筏法砝珐帆番幡翻藩凡矾钒烦樊蕃燔繁蹯蘩反返犯泛饭范贩畈梵方邡坊芳枋钫防妨房肪鲂仿访彷纺舫放飞妃非啡绯菲扉蜚霏鲱肥淝腓匪诽悱斐榧翡篚吠废沸狒肺费痱镄分吩纷芬氛酚坟汾棼焚鼢粉份奋忿偾愤粪鲼瀵丰风沣枫封疯砜峰烽葑锋蜂酆冯逢缝讽唪凤奉俸佛缶否夫呋肤趺麸稃跗孵敷弗伏凫孚扶芙芾怫拂服绂绋苻俘氟祓罘茯郛浮砩莩蚨匐桴涪符艴菔袱幅福蜉辐幞蝠黻抚甫府拊斧俯釜脯辅腑滏腐黼父讣付妇负附咐阜驸复赴副傅富赋缚腹鲋赙蝮鳆覆馥";
            pys[7] = "ＧｇGg旮钆尜嘎噶尕尬该陔垓赅改丐钙盖溉戤概干甘杆肝坩泔苷柑竿疳酐尴秆赶敢感澉橄擀旰矸绀淦赣冈刚岗纲肛缸钢罡港杠筻戆皋羔高槔睾膏篙糕杲搞缟槁稿镐藁告诰郜锆戈圪纥疙哥胳袼鸽割搁歌阁革格鬲葛蛤隔嗝塥搿膈镉骼哿舸个各虼硌铬给根跟哏亘艮茛更庚耕赓羹哽埂绠耿梗鲠工弓公功攻供肱宫恭蚣躬龚觥巩汞拱珙共贡勾佝沟钩缑篝鞲岣狗苟枸笱构诟购垢够媾彀遘觏估咕姑孤沽轱鸪菇菰蛄觚辜酤毂箍鹘古汩诂谷股牯骨罟钴蛊鹄鼓嘏臌瞽固故顾崮梏牿雇痼锢鲴瓜刮胍鸹呱剐寡卦诖挂褂乖拐怪关观官冠倌棺鳏馆管贯惯掼涫盥灌鹳罐光咣桄胱广犷逛归圭妫龟规皈闺硅瑰鲑宄轨庋匦诡癸鬼晷簋刽刿柜炅贵桂跪鳜衮绲辊滚磙鲧棍呙埚郭崞聒锅蝈国帼掴虢馘果猓椁蜾裹过颌桧卩矜栝饣莞仡";
            pys[8] = "ＨｈHh惆铪哈嗨孩骸海胲醢亥骇害氦顸蚶酣憨鼾邗含邯函晗涵焓寒韩罕喊汉汗旱悍捍焊菡颔撖憾撼翰瀚夯杭绗航颃沆蒿嚆薅蚝毫嗥豪嚎壕濠好郝号昊浩耗皓颢灏诃呵喝嗬禾合何劾和河曷阂核盍荷涸盒菏蚵貉阖翮贺褐赫鹤壑黑嘿痕很狠恨亨哼恒桁珩横衡蘅轰哄訇烘薨弘红宏闳泓洪荭虹鸿蕻黉讧侯喉猴瘊篌糇骺吼后厚後逅候堠鲎乎呼忽烀轷唿惚滹囫弧狐胡壶斛湖猢葫煳瑚鹕槲糊蝴醐觳虎唬琥互户冱护沪岵怙戽祜笏扈瓠鹱花华哗骅铧滑猾化划画话桦怀徊淮槐踝坏欢獾还环郇洹桓萑锾寰缳鬟缓幻奂宦唤换浣涣患焕逭痪豢漶鲩擐肓荒慌皇凰隍黄徨惶湟遑煌潢璜篁蝗癀磺簧蟥鳇恍晃谎幌灰诙咴恢挥虺晖珲辉麾徽隳回洄茴蛔悔卉汇会讳哕浍绘荟诲恚烩贿彗晦秽喙惠缋毁慧蕙蟪昏荤婚阍浑馄魂诨混溷耠锪劐豁攉活火伙钬夥或货获祸惑霍镬嚯藿蠖阚溃凵砉讠圜";
            pys[9] = "ＩｉIi";
            pys[10] = "ＪｊJj艹氅炖伽丌讥击叽饥乩圾机玑肌芨矶鸡咭迹剞唧姬屐积笄基绩嵇犄缉赍畸跻箕畿齑墼激羁及吉岌汲级即极亟佶急笈疾戢棘殛集嫉楫蒺辑瘠蕺籍几己虮挤脊掎戟嵴麂计记伎纪妓忌技芰际剂季哜既洎济继觊偈寂寄悸祭蓟暨跽霁鲚稷鲫冀髻骥加夹佳迦枷浃珈家痂笳袈袷葭跏嘉镓岬郏荚恝戛铗蛱颊甲胛贾钾瘕价驾架假嫁稼戋奸尖坚歼间肩艰兼监笺菅湔犍缄搛煎缣蒹鲣鹣鞯囝拣枧俭柬茧捡笕减剪检趼睑硷裥锏简谫戬碱翦謇蹇见件建饯剑牮荐贱健涧舰渐谏楗毽溅腱践鉴键僭槛箭踺江姜将茳浆豇僵缰礓疆讲奖桨蒋耩匠降洚绛酱犟糨艽交郊姣娇浇茭骄胶椒焦蛟跤僬鲛蕉礁鹪角佼侥挢狡绞饺皎矫脚铰搅湫剿敫徼缴叫峤轿较教窖酵噍醮阶疖皆接秸喈嗟揭街孑节讦劫杰诘拮洁结桀婕捷颉睫截碣竭鲒羯姐解介戒芥届界疥诫借蚧骱藉巾今斤金津衿筋襟仅卺紧堇谨锦廑馑槿瑾尽劲妗近进荩晋浸烬赆缙禁靳觐噤京泾经茎荆惊旌菁晶腈睛粳兢精鲸井阱刭肼颈景儆憬警净弪径迳胫痉竞婧竟敬靓靖境獍静镜扃迥炯窘纠究鸠赳阄啾揪鬏九久灸玖韭酒旧臼咎疚柩桕厩救就舅僦鹫居拘狙苴驹疽掬椐琚趄锔裾雎鞠鞫局桔菊橘咀沮举矩莒榘龃踽句巨讵拒苣具炬钜俱倨剧惧据距犋飓锯窭聚屦踞遽醵娟捐涓鹃镌蠲卷锩倦桊狷绢隽眷鄄噘撅孓决诀抉珏绝觉倔崛掘桷觖厥劂谲獗蕨噱橛爵镢蹶嚼矍爝攫军君均钧皲菌筠麇俊郡峻捃浚骏竣冖荠榍";
            pys[11] = "ＫｋKk咔咖喀卡佧胩开揩锎凯剀垲恺铠慨蒈楷锴忾刊勘龛堪戡坎侃砍莰看瞰康慷糠扛亢伉抗闶炕钪尻考拷栲烤铐犒靠坷苛柯珂科轲疴钶棵颏稞窠颗瞌磕蝌髁壳咳可岢渴克刻客恪课氪骒缂嗑溘锞肯垦恳啃裉吭坑铿空倥崆箜孔恐控抠芤眍口叩扣寇筘蔻刳枯哭堀窟骷苦库绔喾裤酷夸侉垮挎胯跨蒯块快侩郐哙狯脍筷宽髋款匡诓哐筐狂诳夼邝圹纩况旷矿贶框眶亏岿悝盔窥奎逵馗喹揆葵暌魁睽蝰夔傀跬匮喟愦愧蒉馈篑聩坤昆琨锟髡醌鲲悃捆阃困扩括蛞阔廓喱咯隗龈";
            pys[12] = "ＬｌLl虍钅垃拉啦邋旯砬喇剌腊瘌蜡辣来崃徕涞莱铼赉睐赖濑癞籁兰岚拦栏婪阑蓝谰澜褴斓篮镧览揽缆榄漤罱懒烂滥啷郎狼莨廊琅榔稂锒螂朗阆浪蒗捞劳牢唠崂痨铹醪老佬姥栳铑潦涝烙耢酪仂乐叻泐勒鳓雷嫘缧檑镭羸耒诔垒磊蕾儡肋泪类累酹擂嘞塄棱楞冷愣厘梨狸离莉骊犁鹂漓缡蓠蜊嫠璃鲡黎篱罹藜黧蠡礼李里俚哩娌逦理锂鲤澧醴鳢力历厉立吏丽利励呖坜沥苈例戾枥疠隶俐俪栎疬荔轹郦栗猁砺砾莅唳笠粒粝蛎傈痢詈跞雳溧篥俩奁连帘怜涟莲联裢廉鲢濂臁镰蠊敛琏脸裣蔹练娈炼恋殓链楝潋良凉梁椋粮粱墚踉两魉亮谅辆晾量辽疗聊僚寥廖嘹寮撩獠燎镣鹩钌蓼了尥料撂咧列劣冽洌埒烈捩猎裂趔躐鬣邻林临啉淋琳粼嶙遴辚霖瞵磷鳞麟凛廪懔檩吝赁蔺膦躏拎伶灵囹岭泠苓柃玲瓴凌铃陵棂绫羚翎聆菱蛉零龄鲮酃领令另呤溜熘刘浏流留琉硫旒遛馏骝榴瘤镏鎏柳绺锍六鹨龙咙泷茏栊珑胧砻笼聋隆癃窿陇垄垅拢娄偻喽蒌楼耧蝼髅嵝搂篓陋漏瘘镂露噜撸卢庐芦垆泸炉栌胪轳鸬舻颅鲈卤虏掳鲁橹镥陆录赂辂渌逯鹿禄滤碌路漉戮辘潞璐簏鹭麓氇驴闾榈吕侣旅稆铝屡缕膂褛履律虑率绿氯孪峦挛栾鸾脔滦銮卵乱掠略锊抡仑伦囵沦纶轮论捋罗猡脶萝逻椤锣箩骡镙螺倮裸瘰蠃泺洛络荦骆珞落摞漯雒宀疒亻弋丬";
            pys[13] = "ＭｍMm冫呒妈嬷麻蟆马犸玛码蚂杩骂唛吗嘛埋霾买荬劢迈麦卖脉颟蛮馒瞒鞔鳗满螨曼谩墁幔慢漫缦蔓熳镘邙忙盲茫硭莽漭蟒猫毛矛牦茅旄蛑锚髦蝥蟊卯峁泖茆昴铆茂冒贸耄袤帽瑁瞀貌懋么没枚玫眉莓梅媒嵋湄猸楣煤酶镅鹛霉每美浼镁妹昧袂媚寐魅门扪钔闷焖懑们氓虻萌盟甍瞢朦檬礞艨勐猛蒙锰艋蜢懵蠓孟梦咪弥祢迷猕谜醚糜縻麋靡蘼米芈弭敉脒眯糸汨宓泌觅秘密幂谧嘧蜜眠绵棉免沔黾勉眄娩冕湎缅腼面喵苗描瞄鹋杪眇秒淼渺藐邈妙庙乜咩灭蔑篾蠛民岷玟苠珉缗皿闵抿泯闽悯敏愍鳘名明鸣茗冥铭溟暝瞑螟酩命谬缪摸谟嫫馍摹模膜麽摩磨蘑魔抹末殁沫茉陌秣莫寞漠蓦貊墨瘼镆默貘耱哞牟侔眸谋鍪某母毪亩牡姆拇木仫目沐坶牧苜钼募墓幕睦慕暮穆渑礻";
            pys[14] = "ＮｎNn灬阝拿镎哪内那纳肭娜衲钠捺乃奶艿氖奈柰耐萘鼐囡男南难喃楠赧腩蝻囔囊馕曩攮孬呶挠硇铙猱蛲垴恼脑瑙闹淖讷呐呢馁嫩能嗯妮尼坭怩泥倪铌猊霓鲵伲你拟旎昵逆匿溺睨腻拈年鲇鲶黏捻辇撵碾廿念埝娘酿鸟茑袅嬲尿脲捏陧涅聂臬啮嗫镊镍颞蹑孽蘖您宁咛拧狞柠聍凝佞泞甯妞牛忸扭狃纽钮农侬哝浓脓弄耨奴孥驽努弩胬怒女钕恧衄疟虐暖挪傩诺喏搦锘懦糯恁扌慝蔫苎";
            pys[15] = "ＯｏOo噢哦讴欧殴瓯鸥呕偶耦藕怄沤喔衤";
            pys[16] = "ＰｐPp辟拚趴啪葩杷爬琶筢帕怕拍俳徘排牌哌派湃蒎潘攀爿盘磐蹒蟠判泮叛盼畔袢襻乓滂庞逄旁螃耪胖抛脬刨咆庖狍炮袍匏跑泡疱呸胚醅陪培赔锫裴沛佩帔旆配辔霈喷盆湓怦抨砰烹嘭朋堋彭棚硼蓬鹏澎篷膨蟛捧碰丕批纰邳坯披砒铍劈噼霹皮枇毗疲蚍郫陴啤琵脾罴蜱貔鼙匹庀仳圮痞擗癖屁淠媲睥僻甓譬片偏犏篇翩骈胼蹁谝骗剽漂缥飘螵瓢殍瞟票嘌嫖氕撇瞥丿苤姘拼贫嫔频颦品榀牝娉聘乒俜平评凭坪苹屏枰瓶萍鲆钋坡泼颇婆鄱皤叵钷笸迫珀破粕魄剖掊裒仆攴扑铺噗匍莆菩葡蒲璞濮镤朴圃埔浦普溥谱氆镨蹼瀑曝疋";
            pys[17] = "ＱｑQq稽瞿七沏妻柒凄栖桤戚萋期欺嘁槭漆蹊亓祁齐圻岐芪其奇歧祈耆脐颀崎淇畦萁骐骑棋琦琪祺蛴旗綦蜞蕲鳍麒乞企屺岂芑启杞起绮綮气讫汔迄弃汽泣契砌葺碛器憩掐葜恰洽髂千仟阡扦芊迁佥岍钎牵悭铅谦愆签骞搴褰前荨钤虔钱钳乾掮箝潜黔浅肷慊遣谴缱欠芡茜倩堑嵌椠歉呛羌戕戗枪跄腔蜣锖锵镪强墙嫱蔷樯抢羟襁炝悄硗跷劁敲锹橇缲乔侨荞桥谯憔鞒樵瞧巧愀俏诮峭窍翘撬鞘切茄且妾怯窃挈惬箧锲亲侵钦衾芩芹秦琴禽勤嗪溱噙擒檎螓锓寝吣沁揿青氢轻倾卿圊清蜻鲭情晴氰擎檠黥苘顷请謦庆箐磬罄跫銎邛穷穹茕筇琼蛩丘邱秋蚯楸鳅囚犰求虬泅俅酋逑球赇巯遒裘蝤鼽糗区曲岖诎驱屈祛蛆躯蛐趋麴黢劬朐鸲渠蕖磲璩蘧氍癯衢蠼取娶龋去阒觑趣悛圈全权诠泉荃拳辁痊铨筌蜷醛鬈颧犬畎绻劝券炔缺瘸却悫雀确阕阙鹊榷逡裙群俟覃郄欹";
            pys[18] = "ＲｒRr蚺然髯燃冉苒染禳瓤穰嚷壤攘让荛饶桡扰娆绕惹热人仁壬忍荏稔刃认仞任纫妊轫韧饪衽葚扔仍日戎狨绒茸荣容嵘溶蓉榕熔蝾融冗柔揉糅蹂鞣肉如茹铷儒嚅孺濡薷襦蠕颥汝乳辱入洳溽缛蓐褥阮朊软蕤蕊芮枘蚋锐瑞睿闰润若偌弱箬";
            pys[19] = "ＳｓSs嬖丨彐攵犭仨撒洒卅飒脎萨塞腮噻鳃赛三叁毵伞散糁馓桑嗓搡磉颡丧搔骚缫臊鳋扫嫂埽瘙色涩啬铯瑟穑森僧杀沙纱刹砂莎铩痧裟鲨傻唼啥歃煞霎筛晒山删杉芟姗衫钐埏珊舢跚煽潸膻闪陕讪汕疝苫剡扇善骟鄯缮嬗擅膳赡蟮鳝伤殇商觞墒熵裳垧晌赏上尚绱捎梢烧稍筲艄蛸勺芍苕韶少劭邵绍哨潲奢猞赊畲舌佘蛇舍厍设社射涉赦慑摄滠麝申伸身呻绅诜娠砷深神沈审哂矧谂婶渖肾甚胂渗慎椹蜃升生声牲胜笙甥绳省眚圣晟盛剩嵊尸失师虱诗施狮湿蓍酾鲺十什石时识实拾炻蚀食埘莳鲥史矢豕使始驶屎士氏世仕市示式事侍势视试饰室恃拭是柿贳适舐轼逝铈弑谥释嗜筮誓噬螫收手守首艏寿受狩兽售授绶瘦书殳抒纾叔枢姝倏殊梳淑菽疏舒摅毹输蔬秫孰赎塾熟暑黍署鼠蜀薯曙术戍束沭述树竖恕庶数腧墅漱澍刷唰耍衰摔甩帅蟀闩拴栓涮双霜孀爽谁水税睡吮顺舜瞬说妁烁朔铄硕嗍搠蒴嗽槊厶丝司私咝思鸶斯缌蛳厮锶嘶撕澌死巳四寺汜伺似兕姒祀泗饲驷笥耜嗣肆忪松凇崧淞菘嵩怂悚耸竦讼宋诵送颂嗖搜溲馊飕锼艘螋叟嗾瞍擞薮苏酥稣俗夙诉肃涑素速宿粟谡嗉塑愫溯僳蔌觫簌狻酸蒜算虽荽眭睢濉绥隋随髓岁祟谇遂碎隧燧穗邃孙狲荪飧损笋隼榫唆娑挲桫梭睃嗦羧蓑缩所唢索琐锁歙霰莘属";
            pys[20] = "ＴｔTt辶钭她他它趿铊塌溻塔獭鳎挞闼遢榻踏蹋胎台邰抬苔炱跆鲐薹太汰态肽钛泰酞坍贪摊滩瘫坛昙谈郯痰锬谭潭檀忐坦袒钽毯叹炭探碳汤铴羰镗饧唐堂棠塘搪溏瑭樘膛糖螗螳醣帑倘淌傥耥躺烫趟涛绦掏滔韬饕洮逃桃陶啕淘萄鼗讨套忑忒特铽疼腾誊滕藤剔梯锑踢啼提缇鹈题蹄醍体屉剃倜悌涕逖惕替裼嚏天添田恬畋甜填阗忝殄腆舔掭佻挑祧条迢笤龆蜩髫鲦窕眺粜跳贴萜铁帖餮厅汀听烃廷亭庭莛停婷葶蜓霆挺梃艇通嗵仝同佟彤茼桐砼铜童酮潼瞳统捅桶筒恸痛偷头投骰透凸秃突图徒涂荼途屠酴土吐钍兔堍菟湍团抟疃彖推颓腿退煺蜕褪吞暾屯饨豚臀氽乇托拖脱驮佗陀坨沱驼砣鸵跎酡橐鼍妥庹椭拓柝唾箨荑";
            pys[21] = "ＵｕUu";
            pys[22] = "ＶｖVv";
            pys[23] = "ＷｗWw屮芒哇娃挖洼娲蛙瓦佤袜腽歪崴外弯剜湾蜿豌丸纨芄完玩顽烷宛挽晚婉惋绾脘菀琬皖畹碗万腕汪亡王网往枉罔惘辋魍妄忘旺望危威偎逶隈葳微煨薇巍囗为韦圩围帏沩违闱桅涠唯帷惟维嵬潍伟伪尾纬苇委炜玮洧娓萎猥痿艉韪鲔卫未位味畏胃軎尉谓喂渭猬蔚慰魏温瘟文纹闻蚊阌雯刎吻紊稳问汶璺翁嗡蓊瓮蕹挝倭涡莴窝蜗我沃肟卧幄握渥硪斡龌乌圬污邬呜巫屋诬钨无毋吴吾芜唔梧浯蜈鼯五午仵伍坞妩庑忤怃迕武侮捂牾鹉舞兀勿务戊阢杌芴物误悟晤焐婺痦骛雾寤鹜鋈尢於夂";
            pys[24] = "ＸｘXx氵亠夕兮汐西吸希昔析矽穸郗唏奚息浠牺悉惜欷淅烯硒菥晰犀稀粞翕舾溪皙锡僖熄熙蜥嘻嬉膝樨熹羲螅蟋醯曦鼷习席袭觋媳隰檄洗玺徙铣喜葸屣蓰禧戏系饩细阋舄隙禊呷虾瞎匣侠狎峡柙狭硖遐暇瑕辖霞黠下吓夏厦罅仙先纤氙祆籼莶掀跹酰锨鲜暹闲弦贤咸涎娴舷衔痫鹇嫌冼显险猃蚬筅跣藓燹县岘苋现线限宪陷馅羡献腺乡芗相香厢湘缃葙箱襄骧镶详庠祥翔享响饷飨想鲞向巷项象像橡蟓枭削哓枵骁宵消绡逍萧硝销潇箫霄魈嚣崤淆小晓筱孝肖哮效校笑啸些楔歇蝎协邪胁挟偕斜谐携勰撷缬鞋写泄泻绁卸屑械亵渫谢榭廨懈獬薤邂燮瀣蟹躞心忻芯辛昕欣锌新歆薪馨鑫囟信衅兴星惺猩腥刑行邢形陉型硎醒擤杏姓幸性荇悻凶兄匈芎汹胸雄熊休修咻庥羞鸺貅馐髹朽秀岫绣袖锈溴戌盱胥须顼虚嘘需墟徐许诩栩糈醑旭序叙恤洫畜勖绪续酗婿溆絮嗅煦蓄蓿轩宣谖喧揎萱暄煊儇玄痃悬旋漩璇选癣泫炫绚眩铉渲楦碹镟靴薛穴学泶踅雪鳕血谑勋埙熏窨獯薰曛醺寻巡旬驯询峋恂洵浔荀循鲟训讯汛迅徇逊殉巽蕈荥";
            pys[25] = "ＹｙYy刂匚榉彡丫压呀押鸦桠鸭牙伢岈芽琊蚜崖涯睚衙哑痖雅亚讶迓垭娅砑氩揠咽恹烟胭崦淹焉菸阉湮鄢嫣延闫严妍芫言岩沿炎研盐阎筵蜒颜檐兖奄俨衍偃厣掩眼郾琰罨演魇鼹厌彦砚唁宴晏艳验谚堰焰焱雁滟酽谳餍燕赝央泱殃秧鸯鞅扬羊阳杨炀佯疡徉洋烊蛘仰养氧痒怏恙样漾幺夭吆妖腰邀爻尧肴姚轺珧窑谣徭摇遥瑶繇鳐杳咬窈舀崾药要鹞曜耀椰噎爷耶揶铘也冶野业叶曳页邺夜晔烨掖液谒腋靥一伊衣医依咿猗铱壹揖漪噫黟仪圯夷沂诒宜怡迤饴咦姨贻眙胰酏痍移遗颐疑嶷彝乙已以钇矣苡舣蚁倚椅旖义亿刈忆艺议亦屹异呓役抑译邑佾峄怿易绎诣驿奕弈疫羿轶悒挹益谊埸翊翌逸意溢缢肄裔瘗蜴毅熠镒劓殪薏翳翼臆癔镱懿因阴姻洇茵荫音殷氤铟喑堙吟垠狺寅淫银鄞夤霪廴尹引吲饮蚓隐瘾印茚胤应英莺婴瑛嘤撄缨罂樱璎鹦膺鹰迎茔盈荧莹萤营萦楹滢蓥潆蝇嬴赢瀛郢颍颖影瘿映硬媵哟唷佣拥痈邕庸雍墉慵壅镛臃鳙饔喁永甬咏泳俑勇涌恿蛹踊用优忧攸呦幽悠尤由犹邮油柚疣莜莸铀蚰游鱿猷蝣友有卣酉莠铕牖黝又右幼佑侑囿宥诱蚴釉鼬纡迂淤渝瘀于予余妤欤盂臾鱼俞禺竽舁娱狳谀馀渔萸隅雩嵛愉揄腴逾愚榆瑜虞觎窬舆蝓与伛宇屿羽雨俣禹语圄圉庾瘐窳龉玉驭吁聿芋妪饫育郁昱狱峪浴钰预域欲谕阈喻寓御裕遇鹆愈煜蓣誉毓蜮豫燠鹬鬻鸢冤眢鸳渊箢元员园沅垣爰原圆袁援缘鼋塬源猿辕橼螈远苑怨院垸媛掾瑗愿曰约月刖岳钥悦钺阅跃粤越樾龠瀹云匀纭芸昀郧耘氲允狁陨殒孕运郓恽晕酝愠韫韵熨蕴";
            pys[26] = "ＺｚZz纟僮匝咂拶杂砸灾甾哉栽宰载崽再在糌簪咱昝攒趱暂赞錾瓒赃臧驵奘脏葬遭糟凿早枣蚤澡藻灶皂唣造噪燥躁则择泽责迮啧帻笮舴箦赜仄昃贼怎谮曾增憎缯罾锃甑赠吒咋哳喳揸渣齄扎札轧闸铡眨砟乍诈咤栅炸痄蚱榨斋摘宅窄债砦寨瘵沾毡旃粘詹谵瞻斩展盏崭搌辗占战栈站绽湛蘸张章鄣嫜彰漳獐樟璋蟑仉涨掌丈仗帐杖胀账障嶂幛瘴钊招昭啁找沼召兆诏赵笊棹照罩肇蜇遮折哲辄蛰谪摺磔辙者锗赭褶这柘浙蔗鹧贞针侦浈珍桢真砧祯斟甄蓁榛箴臻诊枕胗轸畛疹缜稹圳阵鸩振朕赈镇震争征怔峥挣狰钲睁铮筝蒸徵拯整正证诤郑帧政症之支卮汁芝吱枝知织肢栀祗胝脂蜘执侄直值埴职植殖絷跖摭踯止只旨址纸芷祉咫指枳轵趾黹酯至志忮豸制帙帜治炙质郅峙栉陟挚桎秩致贽轾掷痔窒鸷彘智滞痣蛭骘稚置雉膣觯踬中忠终盅钟舯衷锺螽肿种冢踵仲众重州舟诌周洲粥妯轴碡肘帚纣咒宙绉昼胄荮皱酎骤籀朱侏诛邾洙茱株珠诸猪铢蛛槠潴橥竹竺烛逐舳瘃躅主拄渚煮嘱麈瞩伫住助杼注贮驻柱炷祝疰著蛀筑铸箸翥抓爪拽专砖颛转啭赚撰篆馔妆庄桩装壮状撞追骓椎锥坠缀惴缒赘肫窀谆准卓拙倬捉桌涿灼茁斫浊浞诼酌啄着琢禚擢濯镯仔孜兹咨姿赀资淄缁谘孳嵫滋粢辎觜趑锱龇髭鲻籽子姊秭耔笫梓紫滓訾字自恣渍眦宗综棕腙踪鬃总偬纵粽邹驺诹陬鄹鲰走奏揍租菹足卒族镞诅阻组俎祖躜缵纂钻攥嘴最罪蕞醉尊遵樽鳟撙昨左佐作坐阼怍柞祚胙唑座做";

            int i, j;
            String rtns, hz;
            rtns = "";
            for (i = 0; i < hzs.Length; i++)
            {
                hz = MidStr(hzs, i, 1);
                if ((Asc(hz) > 160) || (Asc(hz) <= 0))
                {
                    for (j = 1; j <= 26; j++)
                    {
                        if (PosStr(pys[j], hz) >= 0) break;
                    }
                    if ((j <= 26) && (j >= 1))
                        rtns += Chr(64 + j);
                    else
                        rtns += hz;
                }
                else
                    rtns += hz;
            }
            return rtns;
        }

        /// <summary>
        /// 将汉字字符串hzs转化为五笔首字母，如果中间有非汉字则直接输出
        /// </summary>
        /// <param name="hzs"></param>
        /// <returns></returns>
        public static String GetHzWB(String hzs)
        {
            if (hzs.Trim() == "") return "";

            String[] wbs = new String[27];
            wbs[1] = "ＡａAa蔼艾鞍芭茇菝靶蒡苞葆蓓鞴苯荸荜萆蓖蔽薜鞭匾苄菠薄菜蔡苍藏艹草茬茶蒇菖苌臣茌茺莼茈茨苁葱蔟萃靼鞑甙萏荡菪荻蒂东鸫董蔸芏莪苊萼蒽贰藩蕃蘩范匚芳菲匪芬葑芙芾苻茯莩菔甘苷藁戈革葛茛工功攻恭廾巩汞共贡鞲苟菇菰鹳匦邯菡蒿薅荷菏蘅薨荭蕻葫花划萑荒黄茴荟蕙荤劐或获惑藿芨基蒺蕺芰蓟葭荚菅蒹鞯茧荐茳蒋匠艽茭蕉节戒芥藉堇荩靳觐茎荆菁警敬苴鞠鞫菊莒巨苣蕨菌蒈勘戡莰苛恐芤蔻苦蒯匡葵匮蒉莱蓝莨蒗劳勒蕾莉蓠藜苈荔莅莲蔹蓼蔺苓菱茏蒌芦萝荦落荬颟鞔蔓芒茫莽茅茆茂莓萌甍瞢蒙蘼苗鹋藐蔑苠茗摹蘑茉莫蓦某苜募墓幕慕暮艿萘匿廿茑孽蘖欧殴瓯鸥藕葩蒎蓬芘匹苤苹萍叵莆菩葡蒲七萋期欺芪其萁綦蕲芑荠葺葜芊荨芡茜蔷荞鞒巧翘鞘切茄芩芹勤擎檠苘跫銎邛茕蛩区蕖蘧荃颧鹊苒荛惹荏葚戎茸荣蓉鞣茹薷蓐蕤蕊芮若萨散莎芟苫芍苕甚蓍莳世式贳菽蔬薯蒴斯菘薮苏蔌蒜荽荪蓑苔薹萄忒慝藤萜莛葶茼荼菟芄莞菀葳薇苇萎蔚蓊蕹莴卧巫芜芴昔菥熙觋葸蓰匣莶藓苋芗葙巷项萧邪鞋薤芯莘薪荇芎蓄蓿萱靴薛薰荀蕈鸦牙芽雅迓菸蔫芫郾燕鞅尧药医荑颐苡弋艺薏翳茵荫鄞茚英莺茔荥荧莹萤营萦蓥莜莸莠萸芋蓣鸢苑芸蕴匝葬藻赜蘸蔗斟蓁蒸芝芷荮茱苎著茁菹蕞";
            wbs[2] = "ＢｂBb阿隘阪孢陂陛屮陈丞承蚩耻出除陲聪耽聃阽耵陡队堕耳防阝附陔隔耿孤聒孩函隍隳亟际降阶卩孑卺阱聚孓孔聩联辽聊了陵聆隆陇陋陆勐孟陌陧聂颞聍陪陴聘阡凵取娶孺阮陕隋随祟隧孙陶粜陀隈隗卫阢隰隙险限陷陉逊阳耶也阴隐隅院陨障阵职陟骘坠孜子陬鄹阻阼";
            wbs[3] = "ＣｃCc巴畚弁骠驳参骖叉骋驰骢皴迨怠邓叠对怼驸观骇骅欢鸡骥艰骄矜劲刭颈迳驹骏骒垒骊骝驴骡骆马矛蝥蟊瞀牟鍪难能骈骗骐骑巯驱劝逡柔叁毵桑颡骚骟圣驶双厶驷骀台邰炱通驮驼婺骛鹜戏骧骁熊驯验矣驿甬勇恿又予驭预豫鹬允驵蚤骣骤驻骓驺";
            wbs[4] = "ＤｄDd砹碍鹌百邦帮磅悲碑辈碚奔泵砭碥髟飙鬓礴布礤厕碴虿厂耖砗辰碜成舂厨础春唇蠢磁蹙存磋厝耷达大砀焘磴砥碲碘碉碟碇硐碓礅趸砘夺厄而鸸砝矾非蜚斐翡奋丰砜酆奉砩尬尴感矸硌耕龚鸪辜古嘏故顾硅磙夯耗厚胡鹕瓠鬟磺灰彗慧耠矶剞髻恝戛硷碱礓耩礁碣兢鬏韭厩厥劂砍磕克刳夸夼矿盔奎髡砬耢耒磊厘历厉励砺砾奁鹩尥鬣磷硫龙砻聋垄耧碌码劢迈硭髦礞面耱奈耐硇碾耨恧耦耙耪匏裴砰硼碰砒破戚奇契砌碛牵硗挈秦磲鬈犬确髯辱三磉砂奢厍砷蜃盛石寿戍耍爽硕厮耜肆碎太态泰碳耥套髫厅砼砣碗万威硪戊矽硒袭硖夏厦咸厢硝硎雄髹戌砉碹压砑研奄厣魇厌砚艳雁餍赝页靥欹硬尢尤友有右郁原愿耘砸在臧仄砟丈磔砧碡砖斫髭耔鬃奏左";
            wbs[5] = "ＥｅEe爱胺肮膀胞豹膘豳膑脖膊采彩豺肠塍腠脆脞胆貂腚胨胴肚腭肪肥腓肺肤孚服郛脯腑腹尕戤肝肛胳膈肱股臌胍胱虢胲貉肌及胛腱胶脚腈肼胫雎爵胩胯脍腊肋臁脸膦胧胪脶脉貌朦脒觅腼邈膜貊貘肭乃鼐腩脑腻脲脓胖脬胚朋鹏膨脾貔胼脐肷腔且朐肜乳朊脎腮臊彡膻膳胂胜豕受腧甩舜胎肽膛腾滕腆腿豚脱妥腽脘腕肟奚膝燹县腺胁腥胸貅须悬胭腌腰遥繇舀鹞腋胰臆盈媵臃用腴爰月刖孕脏膪胀胗朕肢胝脂豸膣肿肘逐助肫腙胙";
            wbs[6] = "ＦｆFf埃霭埯坳坝霸坂雹贲甏孛勃博鹁埠才裁场超朝坼趁城埕墀赤翅亍矗寸埭戴堤觌坻地颠坫垤堞耋动垌都堵堆墩垛二坊霏坟封夫赴垓干坩赶圪塥埂垢彀遘觏毂鼓瞽卦圭规埚过顸邗韩翰壕郝盍赫堠壶觳坏卉恚魂霍击圾赍吉戟霁嘉教劫颉截进井境赳救趄均垲刊堪坎考坷壳坑堀垮块款圹亏逵坤垃老雷塄嫠坜雳墚埒趔霖零酃垅露垆埋霾卖墁耄霉坶南赧垴坭霓辇埝培霈堋彭坯霹埤鼙圮坪坡埔亓圻耆起乾墙謦磬罄求逑裘趋麴去趣却悫壤韧颥丧埽啬霎埏墒垧赦声十埘士示螫霜寺索塌塔坍坛坦塘趟韬替填霆土堍坨顽韦圩违未雯斡圬无坞雾熹喜霞献霰霄孝协馨幸需墟雪埙垭盐堰壹圯埸懿堙垠霪墉雩雨域元垣袁鼋塬远垸越云运韫哉栽载趱增赵者赭真圳震支直埴址志煮翥专趑走";
            wbs[7] = "ＧｇGg瑷敖獒遨熬聱螯鳌骜鏊班斑甭逼碧表殡丙邴玻逋不残蚕璨曹琛豉敕刺璁琮殂璀歹带殆玳殚到纛玷靛玎豆逗毒蠹顿垩恶噩珥珐玢否麸敷甫副丐鬲亘更珙瑰翮珩瑚琥互画还环璜虺珲惠丌玑墼棘殛夹珈郏颊戋歼柬戬豇瑾晋靓静玖琚珏开珂琨剌来赉赖琅鹂璃逦理吏丽郦琏殓两列烈裂琳玲琉珑璐珞玛麦瑁玫芈灭玟珉末殁囊孬瑙弄琶丕邳琵殍平珀璞妻琦琪琴青琼球璩融瑞卅瑟珊殇事殊束死素速琐瑭忑天忝殄餮吞屯橐瓦歪豌玩琬王玮軎吾五武鹉兀瑕下现刑邢形型顼璇殉琊亚焉鄢严琰殃珧瑶一夷殪瑛璎迂于欤盂瑜与玉瑗殒再瓒遭枣责盏璋珍臻整正政殖至郅致珠赘琢";
            wbs[8] = "ＨｈHh龅彪卜步睬餐粲柴觇龀瞠眵齿瞅龊雌此鹾眈瞪睇点盯鼎督睹盹丨壑虍虎乩睑睫睛旧龃具遽瞿矍卡瞰瞌肯眍眶睽睐瞵龄卢鸬颅卤虏虑瞒眯眠眄瞄眇瞑眸目睦睨虐盼皮睥瞟频颦颇攴歧虔瞧氍龋觑睿上叔睡瞬瞍眭睢睃忐龆眺瞳凸龌瞎些盱虚眩睚眼眙龈卣虞龉眨砦瞻占战贞睁止瞩卓桌赀觜龇紫訾眦";
            wbs[9] = "ＩｉIi澳灞浜滗濞汴滨濒波泊渤沧漕测涔汊潺尝常敞氅潮澈尘沉澄池滁淳淙淬沓淡澹当党凼滴涤滇淀洞渎渡沌沲洱法泛淝沸汾瀵沣浮涪滏尜溉泔澉淦港沟沽汩涫灌光滚海涵汉汗瀚沆濠浩灏河涸泓洪鸿黉鲎滹湖浒沪滑淮洹浣涣漶湟潢辉洄汇浍浑混溷活激汲脊洎济浃尖湔涧渐溅江洚浇湫洁津浸泾酒沮举涓觉浚渴溘喾溃涞濑澜漤滥浪潦涝泐泪漓澧沥溧涟濂潋梁粱劣洌淋泠溜浏流鎏泷漏泸渌滤漉潞滦沦泺洛漯满漫漭泖没湄浼懑汨泌沔湎淼渺泯溟沫漠沐淖泥溺涅泞浓沤派湃潘泮滂泡沛湓澎淠漂泼婆濮浦溥瀑沏柒漆淇汔汽泣洽潜浅溱沁清泅渠雀染溶濡汝洳溽润洒涩沙裟鲨潸汕裳赏尚少潲涉滠深沈渖渗渑省湿淑沭漱澍涮氵水澌汜泗淞溲涑溯濉娑挲溻汰滩潭汤堂棠溏淌烫涛滔洮逃淘鼗涕添汀潼涂湍沱洼湾汪沩涠潍洧渭温汶涡沃渥污浯鋈汐浠淅溪洗涎湘削消逍潇淆小肖泄泻渫瀣兴汹溴洫溆漩泫渲学泶洵浔汛涯淹湮沿演滟泱洋漾耀液漪沂溢洇淫滢潆瀛泳涌油游淤渝渔浴誉渊沅源瀹澡泽渣沾澶湛漳涨掌沼兆浙浈汁治滞洲洙潴渚注涿浊浞濯淄滋滓渍";
            wbs[10] = "ＪｊJj暧暗昂蚌暴蝙晡螬蝉蟾昌畅晁晨蛏螭匙虫蜍蝽旦刂戥电蝶蚪蛾遏蜂蚨蜉蝠蝮旰杲蛤虼蚣蛄蛊归晷炅蝈果蜾蚶晗旱蚝昊颢曷蚵虹蝴蝗蟥晃晖蛔晦蟪夥蠖虮蛱坚监鉴蛟蚧紧晶景颗蝌旷暌蝰昆蛞旯蜡览螂蜊里蛎蠊晾量临蛉蝼螺蟆蚂螨曼蟒蛑昴冒昧虻盟蜢蠓冕蠛明暝螟蝻曩蛲昵暖蟠螃蟛蚍蜱螵曝蛴蜞蜣螓蜻晴蚯虬蝤蛆蛐蠼蜷蚺日蝾蠕蚋晒蟮晌蛸蛇申肾晟师时是暑曙竖墅帅蟀蛳螋遢昙螗螳剔题蜩蜓蜕暾蛙蜿晚旺韪蚊蜗蜈晤晰蜥螅蟋曦虾暇暹贤显蚬蟓晓歇蝎昕星勖煦暄曛蚜蜒晏蛘曜野曳晔蚁易蜴蚓蝇影映蛹蚰蝣蚴禺愚蝓昱遇蜮螈曰昀晕早昃蚱蟑昭照蜘蛭蛛蛀最昨";
            wbs[11] = "ＫｋKk吖啊嗄哎唉嗳嗌嗷叭吧跋呗趵嘣蹦吡鄙哔跸别啵踣跛卟哺嚓踩嘈噌蹭躔唱嘲吵嗔呈逞吃哧嗤踟叱踌躇蹰啜嘬踹川喘串吹踔呲蹴蹿啐蹉哒嗒呆呔啖叨蹈噔蹬嘀嗲踮叼吊跌喋蹀叮啶咚嘟吨蹲咄哆踱哚跺呃鄂鹗颚蹯啡吠吩唪呋趺跗呒咐嘎噶嗝跟哏哽咕呱剐咣贵跪呙哈嗨喊嚆嗥嚎号呵喝嗬嘿哼哄喉吼呼唿唬哗踝唤患咴哕喙嚯叽咭唧跻戢哜跽跏趼践踺跤叫噍喈嗟噤啾咀踽距踞鹃噘噱蹶嚼咔咖喀咳嗑啃吭口叩哭跨哙哐喹跬喟啦喇啷唠叻嘞喱哩呖唳跞踉嘹咧躐啉躏另呤咯咙喽噜路鹭吕骂唛吗嘛咪嘧黾喵咩鸣哞哪喃囔呶呐呢嗯啮嗫蹑咛哝喏噢哦呕趴啪哌蹒咆跑呸喷嘭噼啤蹁嘌品噗蹼嘁蹊器遣呛跄跷嗪噙吣嚷蹂嚅噻嗓唼啥跚哨呻哂史嗜噬唰吮顺嗍嗽咝嘶嗣嗖嗾嗉虽唆嗦唢趿踏蹋跆叹饕啕踢啼蹄嚏跳听嗵吐跎鼍唾哇唯味喂吻嗡喔呜吴唔吸唏嘻呷吓跹跣响哓嚣哮啸躞兄咻嘘嗅喧勋呀哑咽唁吆咬噎叶咿噫咦遗呓邑喑吟吲嘤郢哟唷喁咏踊呦吁喻员跃郧咂咱唣噪躁啧吒咋哳喳咤啁吱跖踯只趾踬中忠盅踵咒躅嘱啭啄踪足躜嘴唑";
            wbs[12] = "ＬｌLl黯罢办畀边黪车畴黜辍辏黩囤轭恩罚畈罘辐辅罡哿轱罟固轨辊国贺黑轰轷囫回畸羁辑加迦袈甲驾架囝轿较界轲困罱累罹力轹詈连辆辚囹轳辂辘略囵轮罗逻皿墨默囡男嬲畔毗罴圃畦黔堑椠轻圊黥囚黢圈辁畎轫软轼输署蜀思四田畋町图团疃畹辋囗围畏胃辖黠勰轩鸭罨轺黟轶因黝囿圄圉园圆辕圜暂錾罾轧斩辗罩辄辙轸畛轵轾置轴转辎罪";
            wbs[13] = "ＭｍMm岸盎凹岜败贝崩髀贬飑飚髌财册岑崇帱遄赐崔嵯丹嶝迪骶巅典雕岽峒髑赌朵剁峨帆幡凡贩风峰凤幅幞赋赙赅冈刚岗骼岣购鹘骨崮刿崞帼骸骺岵凰幌贿岌几嵴觊岬见贱峤骱巾赆冂迥飓崛峻凯剀髁岢崆骷髋贶岿崃岚崂嶙岭髅嵝赂幔峁帽嵋岷内帕赔帔岐崎屺岂髂岍嵌峭赇曲岖冉嵘肉山删赡赊嵊殳赎兕崧嵩飕夙髓岁炭赕贴帖同彤骰崴网罔巍帏帷嵬幄峡岘崤岫峋岈崖崦岩央鸯崾贻嶷屹峄婴罂鹦由邮嵛屿峪崽赃则帻贼赠崭帐账嶂幛赈峥帧帙帜峙周胄贮颛赚幢嵫";
            wbs[14] = "ＮｎNn懊悖鐾必愎辟壁嬖避臂璧襞忭擘檗怖惭惨恻层孱忏羼惝怅怊忱迟尺忡憧惆丑怵憷怆戳悴翠忖怛惮蛋忉导悼惦殿刁懂恫惰屙愕发飞悱愤怫改敢怪惯憨悍憾恨恒惚怙怀慌惶恍恢悔屐己忌悸届尽惊憬居局剧惧屦恺慨忾慷尻恪快悝愦愧悃懒愣怜懔鹨戮屡履买慢忙眉鹛懵乜民悯愍恼尼怩尿忸懦怄怕爿怦劈屁甓譬屏恰悭慊戕悄憔愀怯惬情屈悛慑慎尸虱屎恃收书疏刷司巳忪悚愫屉悌惕恬恸屠臀惋惘惟尾尉慰屋忤怃悟惜犀习屣遐屑懈忄心忻惺性悻胥恤恂迅巽疋恹怏怡乙已以忆异怿羿悒翌翼慵忧愉羽悦恽愠熨奘憎翟展怔咫忮昼属惴怍";
            wbs[15] = "ＯｏOo粑爆焙煸灬炳灿糙焯炒炽炊糍粗粹灯断煅炖烦燔粉粪烽黻黼糕焓焊烘糇烀煳糊焕煌烩火糨烬粳精炯炬爝糠炕烤烂烙类粒粝炼粮燎料粼遴熘娄炉熳煤焖迷米敉糯炮粕炝糗炔燃熔糅糁煽剡熵烧炻数烁燧郯糖烃煺烷煨炜焐烯粞熄籼燮糈煊炫烟炎焰焱炀烊业邺烨熠煜燠糌糟凿灶燥炸粘黹烛炷灼籽粽";
            wbs[16] = "ＰｐPp安案袄宝褓被褙裨窆褊裱宾补察衩禅宸衬裎褫宠初褚穿窗辶祠窜褡裆宕祷定窦裰额祓袱福富袼割宫寡褂官冠宄害寒罕褐鹤宏祜寰宦逭豁祸寂寄家袷裥謇蹇窖衿襟窘究裾窭军皲窠客裉空寇窟裤宽窥褴牢礼帘裢裣寥寮窿禄褛裸袂寐祢冖宓密幂蜜宀冥寞衲宁甯农袢襻袍祁祈祺骞搴褰襁窍窃寝穷穹祛裙禳衽容冗襦褥塞赛衫社神审实礻视室守祀宋宿邃它袒裼祧窕突褪袜剜完宛窝寤穸禧禊祆宪祥宵写袖宣穴窨宴窑窈衤宜寅廴宥窬宇窳寓裕冤郓灾宰宅窄寨褶祯鸩之祗祉窒冢宙祝窀禚字宗祖祚";
            wbs[17] = "ＱｑQq锕锿铵犴钯鲅钣镑勹包饱鲍狈钡锛狴铋鳊镖镳鳔镔饼钵饽钹铂钸钚猜馇锸猹镲钗馋镡铲猖鲳鬯钞铛铖鸱饬铳刍锄雏触舛钏锤匆猝镩锉错岛锝镫镝狄氐邸甸钿鲷钓铞鲽钉锭铥兜钭独镀锻镦钝多铎锇饿锷鳄儿鲕尔迩饵铒钒犯饭钫鲂鲱狒镄鲼锋孵凫匐负鲋鳆钆钙钢镐锆镉铬鲠觥勾钩狗够觚钴锢鲴鳏馆盥犷逛龟鲑鳜鲧锅猓铪狠訇猴忽狐斛猢鹱铧猾獾郇锾奂鲩鳇昏馄锪钬镬饥急鲚鲫镓铗钾鲣锏饯键鲛角狡饺铰桀鲒解钅金锦馑鲸獍镜久灸狙锔句钜锯镌锩狷觖獗镢钧锎铠锴钪铐钶锞铿狯狂馈锟鲲铼镧狼锒铹铑乐鳓镭狸鲡锂鲤鳢猁鲢镰链獠镣钌猎鳞铃鲮留遛馏镏锍镂鲈鲁镥铝卵锊猡锣镙犸馒鳗镘猫锚卯铆贸猸镅镁钔猛锰猕免勉名铭馍镆钼镎钠馕铙猱馁铌猊鲵鲇鲶鸟袅镊镍狞狃钮钕锘刨狍锫铍鲆钋钷铺匍镤镨鳍钎铅钤钱钳欠锖锵镪锹锲钦锓卿鲭鳅犰劬鸲铨犭然饶饪狨铷锐鳃馓鳋色铯杀刹铩煞钐鳝觞勺猞狮鲺饣蚀鲥氏饰铈弑狩铄锶饲馊锼稣觫狻狲飧锁铊獭鳎鲐钛锬钽铴镗饧铽锑逖鲦铫铁铤铜钍兔饨鸵外危猥鲔猬刎乌邬钨勿夕希郗欷锡玺铣饩郄狎狭锨鲜猃馅镶饷象枭销獬邂蟹锌鑫猩凶匈馐锈铉镟鳕獯旬鲟爻肴鳐铘猗铱饴钇刈逸镒镱铟狺银夤饮印迎镛鳙犹铀鱿铕鱼狳馀饫狱钰眢鸳猿怨钥钺匀狁锃铡詹獐钊锗针镇争狰钲铮炙觯钟锺皱猪铢橥铸馔锥镯锱鲻邹鲰镞钻鳟";
            wbs[18] = "ＲｒRr挨捱皑氨揞按翱拗扒捌拔魃把掰白捭摆拜扳搬扮拌报抱卑鹎拚摈兵摒拨播帛搏捕擦操插搽拆掺搀抄扯掣撤抻撑魑持斥抽搐搋揣氚捶撺摧搓撮挫措搭打担掸氮挡氘捣的抵掂垫掉迭瓞揲氡抖盾遁掇扼摁反返氛缶扶拂氟抚拊擀缸皋搞搁搿拱瓜挂拐掼罐皈鬼掴氦捍撖撼皓后逅护换擐皇遑挥攉挤掎技搛拣捡挢皎搅敫接揭拮捷斤近揪拘掬拒据捐撅抉掘攫捃揩看扛抗拷氪控抠扣挎揆魁捆扩括拉拦揽捞擂魉撩撂捩拎拢搂撸掳氯掠抡捋摞魅扪描抿摸抹拇捺氖攮挠拟拈年捻撵捏拧牛扭挪搦爬拍排乓抛抨捧批披郫擗氕撇拼乒皤迫魄掊扑颀气掐扦掮抢撬擒揿氢氰丘邱泉缺攘扰热扔揉撒搡搔扫擅捎摄失拾势拭逝誓手扌授抒摅摔拴搠撕搜擞损所挞抬摊探搪掏提掭挑挺捅投抟推托拖拓挖挽皖魍挝握捂舞罅氙掀魈挟携撷卸欣擤揎踅押氩揠掩扬氧邀摇揶掖揖抑挹殷氤撄拥揄援掾岳氲拶攒皂择揸扎摘搌招找蜇折哲蛰摺振挣拯卮执絷摭指制质挚贽掷鸷朱邾拄抓爪拽撰撞拙捉擢揍攥撙";
            wbs[19] = "ＳｓSs桉柏板梆榜棒杯本杓标彬槟柄醭材槽杈查槎檫郴榇柽枨酲橙酬樗橱杵楮楚椽棰槌椿醇枞楱酢醋榱村档柢棣丁酊顶栋椟杜椴樊梵枋榧酚棼焚枫桴覆概杆柑酐橄杠槔槁哥歌格根梗枸构酤梏棺桄柜桂棍椁醢酣杭核桁横槲醐桦槐桓桧机极楫枷贾枧检楗槛椒酵醮杰槿禁柩桕椐桔橘榉醵鄄桷橛楷栲柯棵可枯酷框醌栝栏婪榄榔醪栳酪檑酹棱楞李醴枥栎栗楝椋林檩柃棂榴柳栊楼栌橹麓榈椤杩懋枚梅楣酶檬梦醚棉杪酩模木柰楠酿柠杷攀醅配棚枇剽飘瓢票榀枰朴栖桤槭棋杞枪樯橇桥樵檎楸权醛榷桡榕枘森杉梢椹酾柿枢梳术述树栓松酥粟酸榫桫梭榻酞覃檀樘醣桃梯醍梃桐酮桶酴柁酡椭柝枉桅梧杌西析皙樨醯檄柙酰相想橡枵校楔械榍榭醒杏朽栩醑酗楦醺桠檐酽杨样杳要椰酏椅裔樱楹柚酉榆橼樾酝楂札栅榨栈樟杖棹柘桢甄榛枕枝栀植枳酯栉桎酎株槠杼柱桩椎酌梓棕醉樽柞";
            wbs[20] = "ＴｔTt矮岙奥笆稗般版舨备惫笨鼻彼秕笔舭币筚箅篦笾秉舶箔簸簿舱艚策长徜彻称乘惩程秤笞篪彳艟愁稠筹臭处舡船囱垂辞徂簇汆篡毳矬笪答待箪稻得德簦等籴敌笛第簟牒丢冬篼牍犊笃短簖躲舵鹅乏筏番翻繁彷舫篚逢稃符复馥竿秆筻睾篙稿告郜舸各躬篝笱箍牯鹄牿刮鸹乖管簋鼾航禾和很衡篌後乎笏徊徨篁簧徽秽积笄嵇犄箕稽笈籍季稷笳稼笺犍笕简舰毽箭矫徼秸街筋径咎矩榘犋筠犒靠科稞箜筘筷筐篑徕籁篮稂梨犁黎篱黧利笠篥笼篓舻簏氇稆律乱箩雒毛牦么每艨艋秘秒篾敏鳘秣毪牡牧穆黏臬衄筢徘牌盘磐逄篷片犏篇丿牝鄱笸攵氆乞迄憩千迁愆签箝乔箧箐筇秋鼽躯衢筌穰壬稔入箬穑歃筛舢稍筲艄舌射身矧升生牲笙甥眚剩矢适舐释筮艏秫黍税私笥艘簌算穗笋毯躺特甜舔条笤廷艇筒透秃徒颓乇箨往逶微委艉魏稳我午迕牾务物息牺悉稀舾徙系先舷衔筅香箱向箫筱笑囟衅行秀徐选血熏循徇衙延筵衍秧徉夭徭迤移舣役劓胤牖釉竽禹御毓箢粤簪昝赞造迮笮舴箦怎齄乍毡笊箴稹征筝徵知夂秩智稚雉舯螽种重舟籀竹竺舳筑箸篆秭笫自租纂";
            wbs[21] = "ＵｕUu癌疤瘢癍半瓣北邶背迸闭敝痹弊辨辩辫瘭憋鳖蹩瘪冫冰并病部瓿差瘥产阐冁阊闯痴啻瘛冲瘳疮疵瓷慈鹚次凑瘁痤瘩单郸瘅疸盗道羝弟帝递癫奠癜凋疔冻斗痘端兑阏阀痱疯冯盖疳赣戆羔疙阁羹痼关闺衮馘阂阖痕闳瘊冱痪豢癀阍疾瘠冀痂瘕间兼煎鹣减剪翦姜将浆奖桨酱交郊疖竭羯疥净痉竞竟靖阄疚疽蠲卷桊眷决竣阚闶疴况夔阃阔瘌辣癞兰阑阆痨冷立疠疬痢凉疗冽凛凌羚瘤六癃瘘闾瘰美门闷闵闽瘼闹疒逆凝疟判叛旁疱疲痞癖瞥瓶剖普凄前歉羌羟妾亲酋遒癯阒拳痊券瘸阕阙闰飒瘙痧闪疝善鄯商韶首兽瘦闩朔槊凇竦送塑遂羧闼瘫痰羰疼誊鹈剃阗童痛头闱痿瘟闻阌问痦羲阋闲痫鹇冼羡翔鲞效辛新歆羞痃癣丫痖阉闫阎颜兖彦羊疡养痒恙冶痍疫益翊意瘗毅癔音瘾瘿痈疣猷瘀瘐阈阅韵曾甑闸痄瘵站章鄣彰瘴疹郑症痔痣瘃疰妆装丬壮状准着兹咨姿资孳粢恣总尊遵";
            wbs[22] = "ＶｖVv嫒媪妣婢婊剥姹婵娼嫦巢媸巛妲逮刀嫡娣妒娥婀妨妃鼢妇旮艮媾姑妫好毁婚姬即嫉彐妓既暨嫁奸建姣娇剿婕她姐妗婧鸠九臼舅娟君郡垦恳馗邋姥嫘娌隶灵录逯妈嬷媒妹媚娩妙嫫姆那娜奶嫩妮娘肀妞奴孥驽努弩胬怒女媲嫖姘嫔娉嫱群娆忍刃妊如嫂姗嬗劭邵娠婶始姝鼠恕孀妁姒叟肃帑迢婷退娃娲丸婉娓鼯妩嬉鼷媳舄娴嫌姓旭婿絮寻巡娅嫣妍鼹妖姚姨姻尹邕鼬妤臾舁娱聿妪媛杂甾嫜召妯帚姊";
            wbs[23] = "ＷｗWw俺傲八爸佰颁伴傍煲保堡倍坌俾便傧伯仓伧侧岔侪伥偿倡侈傺仇俦雠储传创从丛促爨催傣代岱贷袋黛儋但倒登凳低佃爹仃侗段俄佴伐垡仿分份忿偾俸佛伏俘斧俯釜父付阜傅伽鸽个公供佝估谷倌癸刽含颔合何盒颌侯候华化会伙货佶集伎偈祭佳价假俭件剑牮健僭僵焦僬鹪佼侥介借今仅儆僦俱倨倦隽倔俊佧龛侃伉倥侉侩郐傀佬仂儡俚例俐俪傈俩敛僚邻赁伶瓴翎领令偻侣仑伦倮们命侔仫拿倪伲你念您佞侬傩偶俳佩盆仳僻偏贫俜凭仆企仟佥倩戗劁侨俏侵衾禽倾俅全人亻仁仞任恁仍儒偌仨伞僧傻伤畲佘舍伸什食使仕侍售倏舒毹伺似俟怂耸颂俗僳隼他贪倘傥体倜佻停仝佟僮偷途氽佗佤偎伟伪位璺翁瓮倭仵伍侮兮翕僖歙侠仙像偕斜信休修鸺叙儇伢俨偃佯仰爷伊依仪倚亿仡佚佾佣俑优攸悠佑侑余俞逾觎舆伛俣欲鹆愈龠债仉仗侦侄值仲众侏伫住隹追倬仔偬俎佐作坐做";
            wbs[24] = "ＸｘXx绊绑鸨绷匕比毕毖毙弼编缏缤缠弛绸绌纯绰绐弹缔缎缍纺绯费纷缝弗绂绋艴缚绀纲缟纥给绠弓缑贯绲绗弘红弧缳缓幻绘缋绩缉畿级纪继缄缣缰疆绛犟绞缴皆结缙经弪纠绢绝缂绔纩缆缧缡蠡练缭绫绺缕绿纶络缦弥弭糸绵缅缈缗缪母纳纽辔纰缥绮缱强缲顷绻绕纫绒缛弱缫纱缮绱绍绅绳绶纾纟丝鸶缌绥缩绦绨缇统彖纨绾维纬纹毋细纤弦线乡缃飨绡缬绁绣绪续绚幺疑彝绎缢肄引缨颍颖幽幼纡鬻缘约纭缯绽张缜织旨纸彘终粥纣绉缀缒缁综纵组缵";
            wbs[25] = "ＹｙYy哀庵谙廒鏖谤褒庇庳扁卞变遍斌禀亳诧谗廛谄颤昶谌谶诚充床鹑词诞谠诋底谛店调谍订读度憝敦讹谔方邡房访放扉诽废讽府腐讣该高膏诰庚赓诟诂雇诖广庋诡郭裹亥颃毫豪诃劾亨讧户戽扈话肓谎诙麾讳诲诨讥迹齑麂计记剂肩谫谏讲讦诘诫谨廑京旌扃就鹫讵诀谲麇康亢颏刻课库诓诳邝廓谰斓郎廊朗羸诔离戾廉娈恋良亮谅廖麟廪吝刘旒庐鹿旅膂率孪峦挛栾鸾脔銮论蠃麻蛮谩邙盲旄袤氓谜糜縻麋靡谧庙谬谟麽摩磨魔谋亩讷旎诺讴庞庖旆烹庀翩谝评裒谱齐旗麒启綮讫弃谦谴敲谯诮请庆诎诠瓤让认讪扇设麝诜谂诗施识市试谥孰塾熟庶衰谁说讼诵诉谡谇谈谭唐讨亭庭亠庹弯亡妄忘望为诿谓文紊诬庑误诶席襄详庠享谐亵谢廨庥许诩序畜谖玄旋谑询训讯讶讠言谚谳谣夜谒衣诒旖义议亦译诣奕弈谊应膺鹰嬴赢庸雍壅饔永诱於谀语庾育谕谮诈斋旃谵诏肇遮谪这鹧诊证诤衷州诌诛诸丶主麈庄谆诼谘诹卒族诅座";
            wbs[26] = "ＺｚZz";

            int i, j;
            String rtns, hz;
            rtns = "";
            for (i = 0; i < hzs.Length; i++)
            {
                hz = MidStr(hzs, i, 1);
                if ((Asc(hz) > 160) || (Asc(hz) <= 0))
                {
                    for (j = 1; j <= 26; j++)
                    {
                        if (PosStr(wbs[j], hz) >= 0) break;
                    }
                    if ((j <= 26) && (j >= 1))
                        rtns += Chr(64 + j);
                    else
                        rtns += hz;
                }
                else
                    rtns += hz;
            }
            return rtns;
        }

        /// <summary>
        /// 为字符串加单引号
        /// </summary>
        /// <param name="Str">要处理的字符串</param>
        /// <returns></returns>
        public static string Expr(string Str)
        {
            return "'" + Str.Replace("'", "''") + "'";
        }

        /// <summary>
        /// 判断是否分隔字符，包括空格、回车、TAB
        /// </summary>
        /// <param name="sStr"></param>
        /// <returns></returns>
        public static bool IsSplittedString(string sStr)
        {
            for (int i = 0; i < sStr.Length; i++)
            {
                if (sStr[i] == ' ' || sStr[i] == '\n' || sStr[i] == Convert.ToChar(13) || sStr[i] == Convert.ToChar(10) || sStr[i] == '\t' || sStr[i] == '	')
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取两个被分隔的字符串的的实际内容和起始位置
        /// </summary>
        /// <param name="sSrc"></param>
        /// <param name="sFirst"></param>
        /// <param name="sSecond"></param>
        /// <param name="iPos"></param>
        /// <returns></returns>
        public static string GetSplittedStrPos(string sSrc, string sFirst, string sSecond, ref int iPos)
        {
            int iFirstPos = -1, iSecondPos;
        loopgo:
            iFirstPos = PosStr(sSrc, sFirst, iFirstPos + 1);
            if (iFirstPos >= 0)
            {
                iSecondPos = PosStr(sSrc, sSecond, iFirstPos + 1);
                if (iSecondPos >= 0)
                {
                    if (IsSplittedString(MidStr(sSrc, iFirstPos + sFirst.Length, iSecondPos - iFirstPos - sFirst.Length)))
                    {
                        iPos = iFirstPos;
                        return MidStr(sSrc, iFirstPos, iSecondPos + sSecond.Length - iFirstPos);
                    }
                }
                goto loopgo;
            }

            iPos = -1;
            return "";
        }

        //转换一个字符串中的特殊字符
        public static string ConvertXmlSpecialChar(string oldStr)
        {
            oldStr = HTBaseFunc.ReplaceStrAll(oldStr, "&", "&amp;");
            oldStr = HTBaseFunc.ReplaceStrAll(oldStr, "<", "&lt;");
            oldStr = HTBaseFunc.ReplaceStrAll(oldStr, "<", "&gt;");
            oldStr = HTBaseFunc.ReplaceStrAll(oldStr, "'", "&apos;");
            oldStr = HTBaseFunc.ReplaceStrAll(oldStr, "\"", "&quot;");
            oldStr = HTBaseFunc.ReplaceStrAll(oldStr, " ", "&nbsp;");
            return oldStr;
        }

        #endregion

        #region 日期处理类

        [StructLayout(LayoutKind.Sequential)]
        public class SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        public class LibWrap
        {
            // Declares a managed prototype for the unmanaged function.
            [DllImport("Kernel32.dll")]
            public static extern void GetSystemTime([In, Out] SystemTime st);
            [DllImport("Kernel32.dll")]
            public static extern long SetSystemTime([In, Out] SystemTime Dt);
        }

        private static DateTime _nulldatetime = Convert.ToDateTime("1900-01-01");
        public static DateTime NullDateTime
        {
            get
            {
                return _nulldatetime;
            }
        }

        private static DateTime _maxdatetime = Convert.ToDateTime("9999-12-31");
        public static DateTime MaxDateTime
        {
            get
            {
                return _maxdatetime;
            }
        }

        public static DateTime GetTime(double interval)
        {
            return DateTime.Now.AddMilliseconds(interval);
        }

        public static DateTime GetTime(long interval)
        {
            return DateTime.Now.AddMilliseconds(interval);
        }

        /// <summary>
        /// 计算年龄
        /// </summary>
        /// <param name="sDate">出生日期,yyyyMMdd</param>
        /// <returns>整数年龄</returns>
        public static int Age(string sDate)
        {
            return Age(sDate, DateTime.Now.Date);
        }
        public static int Age(string sDate, DateTime DD)
        {
            long Years;
            DateTime DDD;

            if (IsDate(sDate) == false) return (-1);

            DDD = Convert.ToDateTime(sDate.Substring(0, 4) + "-" + sDate.Substring(4, 2) + "-" + sDate.Substring(6, 2));
            Years = (DD.Ticks - DDD.Ticks) / 10000000 / 24 / 3600 / 365;
            return ((int)Years);
        }

        /// <summary>
        /// 计算详细年龄 到天月
        /// </summary>
        /// <param name="sDate"></param>
        /// <returns></returns>
        public static string AgeText(string sDate)
        {
            return AgeText(sDate, DateTime.Now.Date);
        }

        /// <summary>
        /// 计算详细年龄 到天月
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="DD"></param>
        /// <returns></returns>
        public static string AgeText(string sDate, DateTime DD)
        {
            long Days;
            long Years;
            DateTime DDD;

            if (IsDate(sDate) == false) return ("");

            DDD = Convert.ToDateTime(sDate.Substring(0, 4) + "-" + sDate.Substring(4, 2) + "-" + sDate.Substring(6, 2));
            Days = (DD.Ticks - DDD.Ticks) / 10000000 / 24 / 3600;
            Years = Days / 365;

            if (Days >= 0 && Days < 60)
                return (Days.ToString() + "天");
            else if (Days >= 60 && Years < 2)
            {
                Days = Days / 30;
                return (Days.ToString() + "月");
            }
            return (Years.ToString());
        }

        /// <summary>
        /// 获取某年第weekp周第weekdayp周天的日期，这里从该年的第一天算第一周的第一个周天。
        /// </summary>
        /// <param name="yearp"></param>
        /// <param name="weekp"></param>
        /// <param name="weekdayp"></param>
        /// <returns></returns>
        public static DateTime GetDateOfWeekDay(int yearp, int weekp, int weekdayp)
        {
            DateTime caldate;
            caldate = Convert.ToDateTime(yearp.ToString("0000") + "/01" + "/01");
            caldate = caldate.AddDays(7 * (weekp - 1));
            caldate = caldate.AddDays(weekdayp - 1);
            return caldate;
        }

        /// <summary>
        /// 获取某日是该年的第几周
        /// </summary>
        /// <param name="dttime"></param>
        /// <returns></returns>
        public static int GetWeekOfYear(DateTime dttime)
        {
            int firstweekend = 7 - Convert.ToInt32(DateTime.Parse(dttime.Year.ToString() + "-01-01").DayOfWeek);
            int currentday = dttime.DayOfYear;
            return Convert.ToInt32(Math.Ceiling((currentday - firstweekend) / 7.0)) + 1;
        }

        /// <summary>
        /// 获取指定日期所在星期的第一天（周一）
        /// </summary>
        /// <param name="dttime"></param>
        /// <returns></returns>
        public static DateTime GetMondayOfWeek(DateTime dttime)
        {
            int weekday = GetDayOfWeek(dttime);
            DateTime monday = dttime.Date.AddDays(1 - weekday);
            return monday;
        }

        /// <summary>
        /// 获取指定日期所在星期的第一天（周日）
        /// </summary>
        /// <param name="dttime"></param>
        /// <returns></returns>
        public static DateTime GetSundayOfWeek(DateTime dttime)
        {
            int weekday = (int)dttime.DayOfWeek;
            DateTime sunday = dttime.Date.AddDays(weekday * (-1));
            return sunday;
        }

        /// <summary>
        /// 获取指定时间的周天（周日算最后一天，为7）
        /// </summary>
        /// <param name="dttime"></param>
        /// <returns></returns>
        public static int GetDayOfWeek(DateTime dttime)
        {
            if (dttime.DayOfWeek == DayOfWeek.Sunday)
                return 7;
            else
                return (int)dttime.DayOfWeek;
        }

        /// <summary>
        /// 是否是同一天
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static bool IsSameDay(DateTime dt1, DateTime dt2)
        {
            if (dt1.Day != dt2.Day || dt1.Month != dt2.Month || dt1.Year != dt2.Year)
                return false;
            return true;
        }

        /// <summary>
        /// 是否在同一个游戏天内（凌晨5点到第二天凌晨5点为一天）
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static bool IsSameGameDay(DateTime dt1, DateTime dt2)
        {
            DateTime dt1StartTime;
            DateTime dt1EndTime;

            const int hoursPoint = 5;

            if (dt1.Hour <= hoursPoint)
            {
                dt1StartTime = dt1.Date.AddDays(-1).AddHours(hoursPoint);
                dt1EndTime = dt1.Date.AddHours(hoursPoint);
            }
            else
            {
                dt1StartTime = dt1.Date.AddHours(hoursPoint);
                dt1EndTime = dt1.Date.AddDays(1).AddHours(hoursPoint);
            }

            if (dt2 >= dt1StartTime && dt2 <= dt1EndTime)
                return true;

            return false;
        }

        /// <summary>
        /// 是否为合法日期类型。"YYYYMMDD"
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static bool IsDate(string S)
        {
            string T;
            int Num;

            S = S.Trim();
            Num = HTBaseFunc.StringToInt32(S);
            T = Num.ToString();

            if ((T.Length != 8) || (S.Length != 8)) return (false);

            Num = Convert.ToInt32(S.Substring(0, 4));
            if (Num < 0 || Num > 3000) return (false);

            Num = Convert.ToInt32(S.Substring(4, 2));
            if (Num < 0 || Num > 12) return (false);

            Num = Convert.ToInt32(S.Substring(6, 2));
            switch (S.Substring(4, 2))
            {
                case "02":
                    if (Num < 0 || Num > 29) return (false);
                    if (Num == 29 && DateTime.IsLeapYear(Convert.ToInt32(S.Substring(0, 4))) == false) return (false);
                    break;
                case "04":
                case "06":
                case "09":
                case "11":
                    if (Num < 0 || Num > 30) return (false);
                    break;
                default:
                    if (Num < 0 || Num > 31) return (false);
                    break;
            }
            return (true);
        }

        /// <summary>
        /// 获取上个月的第一天的日期
        /// </summary>
        /// <param name="todaydt"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfLastMonth(DateTime todaydt)
        {
            int li_year, li_month;
            DateTime ld_date;
            li_year = todaydt.Year;
            li_month = todaydt.Month;

            if (li_month == 1)
            {
                li_year = li_year - 1;
                li_month = 12;
            }
            else
                li_month = li_month - 1;

            ld_date = Convert.ToDateTime(li_year.ToString("0000") + "/" + li_month.ToString("00") + "/01");
            return ld_date;
        }

        /// <summary>
        /// 获取下个月的第一天的日期
        /// </summary>
        /// <param name="todaydt"></param>
        /// <returns></returns>
        public static DateTime GetFirstDayOfNextMonth(DateTime todaydt)
        {
            int li_year, li_month;
            DateTime ld_date;
            li_year = todaydt.Year;
            li_month = todaydt.Month;

            if (li_month == 12)
            {
                li_year = li_year + 1;
                li_month = 1;
            }
            else
                li_month = li_month + 1;
            ld_date = Convert.ToDateTime(li_year.ToString("0000") + "/" + li_month.ToString("00") + "/01");
            return ld_date;
        }

        /// <summary>
        /// 判断是否闰年？
        /// </summary>
        /// <param name="a_year"></param>
        /// <returns></returns>
        public static bool IsLeapYear(int a_year)
        {
            String s;
            DateTime dt;
            try
            {
                s = (a_year.ToString() + "-02-29");
                dt = Convert.ToDateTime(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取上个月的最后一天的日期
        /// </summary>
        /// <param name="todaydt"></param>
        /// <returns></returns>
        public static DateTime GetEndDayOfLastMonth(DateTime todaydt)
        {
            int li_year, li_month;
            DateTime ld_date;
            li_year = todaydt.Year;
            li_month = todaydt.Month;
            ld_date = Convert.ToDateTime(li_year.ToString("0000") + "/" + li_month.ToString("00") + "/01");
            ld_date = ld_date.AddDays(-1);
            return ld_date;
        }

        /// <summary>
        /// 获取下个月的最后一天的日期
        /// </summary>
        /// <param name="todaydt"></param>
        /// <returns></returns>
        public static DateTime GetEndDayOfNextMonth(DateTime todaydt)
        {
            DateTime ld_date;
            ld_date = GetFirstDayOfNextMonth(GetFirstDayOfNextMonth(todaydt));
            ld_date = ld_date.AddDays(-1);
            return ld_date;
        }

        /// <summary>
        /// 获取一天的最早时刻，这里的时间就是返回00:00:00
        /// </summary>
        /// <param name="todaydt"></param>
        /// <returns></returns>
        public static DateTime GetFirstTimeOfADay(DateTime todaydt)
        {
            return Convert.ToDateTime(todaydt.ToString("yyyy-MM-dd") + " 00:00:00");
        }

        /// <summary>
        /// 获取一天的最后时刻，这里的时间就是返回23:59:59
        /// </summary>
        /// <param name="todaydt"></param>
        /// <returns></returns>
        public static DateTime GetEndTimeOfADay(DateTime todaydt)
        {
            return Convert.ToDateTime(todaydt.ToString("yyyy-MM-dd") + " 23:59:59");
        }

        /// <summary>
        /// 按as_rtnfmt重新格式化一个按as_oldfmt格式化的日期字符串as_date
        /// 其中as_date必须是一个严格按as_oldfmt格式化的日期字符串
        /// </summary>
        /// <param name="as_date"></param>
        /// <param name="as_oldfmt"></param>
        /// <param name="as_rtnfmt"></param>
        /// <returns></returns>
        public static String ReFormatDateString(String as_date, String as_oldfmt, String as_rtnfmt)
        {
            String ls_rtn, s, ms;
            int i, mi;
            as_oldfmt = as_oldfmt.Trim().ToLower();
            as_rtnfmt = as_rtnfmt.Trim().ToLower();
            if (as_oldfmt == as_rtnfmt) return as_date;

            ls_rtn = as_rtnfmt;
            bool byed, bmed, bded;
            byed = false;
            bmed = false;
            bded = false;
            i = -1;
            mi = -1;
            s = "";
            ms = "";
            while (i < as_date.Length)
            {
                i = i + 1;
                if (i < as_oldfmt.Length)
                    s = HTBaseFunc.MidStr(as_oldfmt, i, 1);
                else
                    s = "";
                if ((HTBaseFunc.PosStr("ymd", s) < 0) || (s != HTBaseFunc.RightStr(ms, 1)))
                {
                    if (ms != "")
                    {
                        switch (HTBaseFunc.LeftStr(ms, 1))
                        {
                            case "y":
                                if (!byed)
                                {
                                    ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "yyyy", HTBaseFunc.MidStr(as_date, mi, ms.Length));
                                    ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "yy", HTBaseFunc.RightStr(HTBaseFunc.MidStr(as_date, mi, ms.Length), 2));
                                }
                                byed = true;
                                break;
                            case "m":
                                if (!bmed)
                                {
                                    ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "mm", HTBaseFunc.RightStr("0" + HTBaseFunc.MidStr(as_date, mi, ms.Length), 2));
                                    ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "m", Convert.ToString(Convert.ToInt32(HTBaseFunc.MidStr(as_date, mi, ms.Length))));
                                }
                                bmed = true;
                                break;
                            case "d":
                                if (!bded)
                                {
                                    ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "dd", HTBaseFunc.RightStr("0" + HTBaseFunc.MidStr(as_date, mi, ms.Length), 2));
                                    ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "d", Convert.ToString(Convert.ToInt32(HTBaseFunc.MidStr(as_date, mi, ms.Length))));
                                }
                                bded = true;
                                break;
                        }
                    }
                    mi = -1;
                    ms = "";
                }
                if (HTBaseFunc.PosStr("ymd", s) >= 0)
                {
                    if ((mi == -1) && (ms == "")) mi = i;
                    ms = ms + s;
                }
            }
            ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "dd", "01");
            ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "d", "1");
            ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "mm", "01");
            ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "m", "1");
            ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "yyyy", "1900");
            ls_rtn = HTBaseFunc.ReplaceStrAll(ls_rtn, "yy", "00");
            return ls_rtn;
        }

        /// <summary>
        /// 返回日期时间型数据的格式字符串
        /// 该格式为yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static String GetDatetimeStr(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回日期时间型数据的日期格式字符串
        /// 该格式为yyyy-MM-dd
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static String GetDateStr(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 返回日期时间型数据的时间格式字符串
        /// 该格式为HH:mm:ss
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static String GetTimeStr(DateTime dt)
        {
            return dt.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 返回日期型数据的格式 
        /// </summary>
        /// <param name="sDateStr"></param>
        /// <returns></returns>
        public static DateTime CovertToDate(string sDateStr)
        {
            if (sDateStr.Length == 10)
            {
                return Convert.ToDateTime(sDateStr);
            }
            else if (sDateStr.Length == 8)
            {
                return Convert.ToDateTime(TranDateStr(sDateStr));
            }
            return new DateTime(1900, 1, 1);
        }

        /// <summary>
        /// 返回日期时间型数据的格式 
        /// </summary>
        /// <param name="sDateStr"></param>
        /// <returns></returns>
        public static DateTime CovertToTime(string sDateStr)
        {
            if (sDateStr.Length == 8)
            {
                return Convert.ToDateTime(sDateStr);
            }
            else if (sDateStr.Length == 6)
            {
                return Convert.ToDateTime(TranTimeStr(sDateStr));
            }
            return new DateTime(1900, 1, 1);
        }

        /// <summary>
        /// 日期格式转换 yyyyMMdd 转换为 yyyy-MM-dd
        /// </summary>
        /// <param name="sDateStr"></param>
        /// <returns></returns>
        public static string TranDateStr(string sDateStr)
        {
            return TranDateStr(sDateStr, "-");
        }

        /// <summary>
        /// 日期格式转换 yyyyMMdd 转换为 其他分割符分割的格式
        /// </summary>
        /// <param name="sDateStr"></param>
        /// <param name="sSplit"></param>
        /// <returns></returns>
        public static string TranDateStr(string sDateStr, string sSplit)
        {
            if (sDateStr.Length == 8)
            {
                return sDateStr.Substring(0, 4) + sSplit + sDateStr.Substring(4, 2) + sSplit + sDateStr.Substring(6, 2);
            }
            else if (sDateStr.Length == 6)
            {
                return sDateStr.Substring(0, 2) + sSplit + sDateStr.Substring(2, 2) + sSplit + sDateStr.Substring(4, 2);
            }
            else
            {
                return "1900" + sSplit + "01" + sSplit + "01";
            }
        }

        /// <summary>
        /// 日期格式转换 yyyyMMdd HHmmss 转换为 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="sDateStr"></param>
        /// <param name="sTimeStr"></param>
        /// <returns></returns>
        public static string TranDateTimeStr(string sDateStr, string sTimeStr)
        {
            return TranDateStr(sDateStr, "-") + " " + TranTimeStr(sTimeStr);
        }

        /// <summary>
        /// 时间格式转换 HHmmss 转换为 HH:mm:ss
        /// </summary>
        /// <param name="sTimeStr"></param>
        /// <returns></returns>
        public static string TranTimeStr(string sTimeStr)
        {
            if (sTimeStr.Length == 6)
            {
                return sTimeStr.Substring(0, 2) + ":" + sTimeStr.Substring(2, 2) + ":" + sTimeStr.Substring(4, 2);
            }
            else
            {
                return "00:00:00";
            }
        }

        /// <summary>
        /// 设置当前时间，传入需要设置的时间
        /// </summary>
        /// <param name="Dt"></param>
        public static void SetLocalTime(DateTime Dt)
        {
            try
            {
                SystemTime st = new SystemTime();
                st.wYear = (ushort)Dt.ToUniversalTime().Year;
                st.wMonth = (ushort)Dt.ToUniversalTime().Month;
                st.wDay = (ushort)Dt.ToUniversalTime().Day;
                st.wHour = (ushort)Dt.ToUniversalTime().Hour;
                st.wMinute = (ushort)Dt.ToUniversalTime().Minute;
                st.wSecond = (ushort)Dt.ToUniversalTime().Second;
                st.wMilliseconds = (ushort)Dt.ToUniversalTime().Millisecond;
                LibWrap.SetSystemTime(st);
            }
            catch
            {
                //屏蔽错误
            }
        }

        public static string GetDateTimeSqlVal(DateTime Dt)
        {
            if (Dt == Convert.ToDateTime("1900-01-01"))
                return "null";
            else
                return Expr(Dt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public static string GetDateSqlVal(DateTime Dt)
        {
            if (Dt == Convert.ToDateTime("1900-01-01"))
                return "null";
            else
                return Expr(Dt.ToString("yyyy-MM-dd"));
        }

        public static int GetCurrentTickCount()
        {
            //return DateTime.Now.Ticks / 10000;
            return Environment.TickCount;
        }

        /// <summary>
        /// 获取2个时间之间的间隔
        /// </summary>
        /// <param name="minuendTime">被减的时间</param>
        /// <param name="subtrahendTime">减的时间</param>
        /// <returns>返回间隔</returns>
        public static long GetInterval(DateTime minuendTime, DateTime subtrahendTime)
        {
            return (long)(((TimeSpan)(minuendTime - subtrahendTime)).TotalMilliseconds);
        }

        #endregion

        #region 系统操作类

        //配置文件读写的两个动态DLL
        [DllImport("KERNEL32.DLL")]
        static extern int WritePrivateProfileString(String section, String key, String val, String filePath);
        [DllImport("KERNEL32.DLL")]
        static extern int GetPrivateProfileString(String section, String key, String def, StringBuilder retVal, int size, String filePath);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr FindFirstFile(String fileName, [In, Out] FileInfos findFileData);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool FindNextFile(IntPtr hFindFile, [In, Out] FileInfos findFileData);
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        static extern bool FindClose(IntPtr hFindFile);

        public static bool EnableWriteRegistry = true;


        /// <summary>
        /// 获取操作系统版本
        /// 这里只区分两种操作系统，分别返回为WinNT和Win9x
        /// </summary>
        /// <returns></returns>
        public static String GetOSType()
        {
            String s;
            s = Environment.OSVersion.ToString();
            if (HTBaseFunc.PosStr(s.ToUpper(), "WINDOWS NT") >= 0)
                return "WinNT";
            else if (HTBaseFunc.PosStr(s.ToUpper(), "WINDOWS 9") >= 0)
                return "Win9x";
            else
                return "";
        }

        /// <summary>
        /// 获取一些系统路径
        /// S：系统SYSTEM路径
        /// W：系统WINDOWS路径
        /// C或A：当前应用程序路径
        /// D：桌面路径
        /// P：programfiles路径
        /// </summary>
        /// <param name="special"></param>
        /// <returns></returns>
        public static String GetPath(String special)
        {
            String s;
            switch (special.Trim().ToUpper())
            {
                case "SYSTEM":
                case "SYS":
                case "S":
                    return Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\";
                case "WINDOWS":
                case "WINDOW":
                case "WIN":
                case "W":
                    s = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    return HTBaseFunc.LeftStr(s, HTBaseFunc.DownPosStr(s, @"\"));
                case "CURRENT":
                case "CURR":
                case "C":
                    return Environment.CurrentDirectory + @"\";
                case "APPLICATION":
                case "APP":
                case "A":
                    return Application.StartupPath + @"\";
                case "DESKTOP":
                case "DESK":
                case "D":
                    return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
                case "PROGRAM":
                case "PROG":
                case "P":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取注册表路径对象
        /// writeable表示获取的对象可否修改
        /// </summary>
        /// <param name="path"></param>
        /// <param name="writeable"></param>
        /// <returns></returns>
        public static RegistryKey GetRegistryPath(String path, bool writeable)
        {
            if (EnableWriteRegistry == false)
            {
                return (null);
            }

            RegistryKey rk = null;
            String rs = HTBaseFunc.DepartStr(path, @"\", 0);
            switch (rs.Trim().ToUpper())
            {
                case "HKEY_CLASSES_ROOT":
                    rk = Registry.ClassesRoot.OpenSubKey(HTBaseFunc.MidStr(path, HTBaseFunc.PosStr(path, @"\") + 1), writeable);
                    break;
                case "HKEY_CURRENT_USER":
                    rk = Registry.CurrentUser.OpenSubKey(HTBaseFunc.MidStr(path, HTBaseFunc.PosStr(path, @"\") + 1), writeable);
                    break;
                case "HKEY_LOCAL_MACHINE":
                    rk = Registry.LocalMachine.OpenSubKey(HTBaseFunc.MidStr(path, HTBaseFunc.PosStr(path, @"\") + 1), writeable);
                    break;
                case "HKEY_USERS":
                    rk = Registry.Users.OpenSubKey(HTBaseFunc.MidStr(path, HTBaseFunc.PosStr(path, @"\") + 1), writeable);
                    break;
                case "HKEY_CURRENT_CONFIG":
                    rk = Registry.CurrentConfig.OpenSubKey(HTBaseFunc.MidStr(path, HTBaseFunc.PosStr(path, @"\") + 1), writeable);
                    break;
            }
            return rk;
        }

        /// <summary>
        /// 以缺省不可修改的方式获取注册表路径对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static RegistryKey GetRegistryPath(String path)
        {
            return GetRegistryPath(path, false);
        }

        /// <summary>
        /// 获取注册表中的值
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="dftdata"></param>
        /// <returns></returns>
        public static String GetRegistryString(String path, String name, String dftdata)
        {
            RegistryKey rk;
            String rs = null;
            rk = GetRegistryPath(path);
            if (rk != null)
            {
                try
                {
                    rs = rk.GetValue(name, dftdata).ToString();
                }
                catch (Exception ex)
                {
                    _lasterror = ex.Message;
                }
                finally
                {
                    rk.Close();
                }
            }
            return rs;
        }

        /// <summary>
        /// 设置注册表的值
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static String SetRegistryString(String path, String name, String data)
        {
            if (EnableWriteRegistry == false)
            {
                _lasterror = "不允许写注册表！";
                return null;
            }

            RegistryKey rk;
            String rs = null;
            rk = GetRegistryPath(path, true);
            if (rk != null)
            {
                try
                {
                    rk.SetValue(name, data);
                }
                catch (Exception ex)
                {
                    _lasterror = ex.Message;
                }
                finally
                {
                    rk.Close();
                }
            }
            return rs;
        }

        /// <summary>
        /// 创建注册表的子键
        /// </summary>
        /// <param name="path"></param>
        /// <param name="subkey"></param>
        /// <returns></returns>
        public static String CreateRegistrySubKey(String path, String subkey)
        {
            if (EnableWriteRegistry == false)
            {
                _lasterror = "不允许写注册表！";
                return "";
            }
            RegistryKey rk, rsk;
            String rs = null;
            rk = GetRegistryPath(path, true);
            if (rk != null)
            {
                try
                {
                    rsk = rk.CreateSubKey(subkey);
                    rsk.Close();
                }
                catch (Exception ex)
                {
                    _lasterror = ex.Message;
                }
                finally
                {
                    rk.Close();
                }
            }
            else
                _lasterror = "获取路径失败！";
            return rs;
        }

        /// <summary>
        /// 删除注册表的子键
        /// 这里将路径和子键写在一个参数中
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static String DeleteRegistrySubKey(String path)
        {
            if (EnableWriteRegistry == false)
            {
                _lasterror = "不允许写注册表！";
                return "";
            }

            String subkey;
            int posi;
            String rs = null;
            posi = HTBaseFunc.DownPosStr(path, @"\");
            if (posi >= 0)
            {
                subkey = HTBaseFunc.MidStr(path, posi + 1);
                rs = DeleteRegistrySubKey(HTBaseFunc.LeftStr(path, posi), subkey);
            }
            else
                _lasterror = "传递的路径无效！";
            return rs;
        }

        /// <summary>
        /// 删除注册表的子键
        /// </summary>
        /// <param name="path"></param>
        /// <param name="subkey"></param>
        /// <returns></returns>
        public static String DeleteRegistrySubKey(String path, String subkey)
        {
            if (EnableWriteRegistry == false)
            {
                _lasterror = "不允许写注册表！";
                return "";
            }

            RegistryKey rk;
            String rs = null;
            rk = GetRegistryPath(path, true);
            if (rk != null)
            {
                try
                {
                    rk.DeleteSubKey(subkey, false);
                }
                catch (Exception ex)
                {
                    _lasterror = ex.Message;
                }
                finally
                {
                    rk.Close();
                }
            }
            else
                _lasterror = "获取路径失败！";
            return rs;
        }

        /// <summary>
        /// 删除注册表值
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String DeleteRegistryValue(String path, String value)
        {
            if (EnableWriteRegistry == false)
            {
                _lasterror = "不允许写注册表！";
                return "";
            }

            RegistryKey rk;
            String rs = null;
            rk = GetRegistryPath(path, true);
            if (rk != null)
            {
                try
                {
                    rk.DeleteValue(value, false);
                }
                catch (Exception ex)
                {
                    _lasterror = ex.Message;
                }
                finally
                {
                    rk.Close();
                }
            }
            else
                _lasterror = "获取路径失败！";
            return rs;
        }

        /// <summary>
        /// 设置配置文件值
        /// </summary>
        /// <param name="file"></param>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static bool SetIniFileString(String file, String Section, String Key, String Value)
        {
            //设置配置文件
            try
            {
                WritePrivateProfileString(Section, Key, Value, file);
                return true;
            }
            catch (Exception ex)
            {
                _lasterror = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取配置文件的值
        /// </summary>
        /// <param name="file"></param>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="dftvalue"></param>
        /// <returns></returns>
        public static String GetIniFileString(String file, String Section, String Key, String dftvalue)
        {
            StringBuilder temp = new StringBuilder(6000);
            int i;
            i = GetPrivateProfileString(Section, Key, dftvalue, temp, 6000, file);
            if (i > 0)
                return temp.ToString();
            else
                return "";
        }

        /// <summary>
        /// 设置缺省打印机，printername是系统中安装的打印机名
        /// </summary>
        /// <param name="printername"></param>
        /// <returns></returns>
        public static bool SetDefaultPrinter(String printername)
        {
            if (EnableWriteRegistry == false)
            {
                _lasterror = "当前不能修改注册表，无法设置缺省打印机！";
                return false;
            }
            char[] s = new char[65];
            String inifile, ostype, ls_driver, ls_port, ls_printer, ls_key, ls_new, ls_new2;
            ls_new = printername;
            ls_driver = "";
            ls_port = "";

            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = ls_new;
            if (!pd.PrinterSettings.IsValid)
            {
                _lasterror = "选择的打印机无效！";
                return false;
            }

            ostype = GetOSType();
            if (ostype == "Win9x")
            {
                inifile = GetPath("WINDOWS") + "win.ini";
                if (SetRegistryString(@"HKEY_LOCAL_MACHINE\Config\0001\System\CurrentControlSet\Control\Print\Printers", "default", ls_new) != "")
                {
                    _lasterror = "无法选择打印机！";
                    return false;
                }
                ls_printer = ls_new;
                ls_key = @"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Print\Printers\" + ls_printer;
                ls_driver = GetRegistryString(ls_key, "Printer Driver", ls_driver);
                ls_port = GetRegistryString(ls_key, "Port", ls_port);
                ls_new2 = ls_driver + "," + ls_port;
                ls_printer = ls_new + "," + ls_new2;
                if (!SetIniFileString(inifile, "Windows", "device", ls_printer))
                {
                    _lasterror = "无法选择打印机！";
                    return false;
                }
                else
                    return true;
            }
            else if (ostype == "WinNT")
            {
                ls_port = GetRegistryString(@"HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\Devices", ls_new, ls_port);
                ls_printer = ls_new + "," + ls_port;
                if (SetRegistryString(@"HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\Windows", "Device", ls_printer) != "")
                {
                    _lasterror = "无法选择打印机！";
                    return false;
                }
                else
                    return true;
            }
            else
            {
                _lasterror = "无法选择打印机！";
                return false;
            }
        }

        /// <summary>
        /// 获取屏幕分辨率
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetDisp(out int width, out int height)
        {
            width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        }

        /// <summary>
        /// 判断是否已有同样的实例启动
        /// </summary>
        /// <returns>如果找到返回该实例否则返回null</returns>
        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();

            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        return process;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 查找文件，返回一个FileInfos的ArrayList
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ArrayList FindFile(string fileName)
        {
            ArrayList arr = new ArrayList();
            FileInfos fd;
            IntPtr handle;

            fd = new FileInfos();
            handle = FindFirstFile(fileName, fd);
            if (handle.ToInt32() >= 0)
            {
                bool bFind = true;
                while (bFind)
                {
                    arr.Add(fd);

                    fd = new FileInfos();
                    bFind = FindNextFile(handle, fd);
                }
            }

            FindClose(handle);

            return arr;
        }

        /// <summary>
        /// 获取总的内存
        /// </summary>
        /// <returns></returns>
        public static long GetMemoryTotal()
        {
            long totalbytes = 0;
            System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_ComputerSystem");
            foreach (System.Management.ManagementObject mo in mc.GetInstances())
            {
                if (mo["TotalPhysicalMemory"] != null)
                {
                    totalbytes = long.Parse(mo["TotalPhysicalMemory"].ToString());
                    break;
                }
            }
            return totalbytes;
        } 

        /// <summary>
        /// 获取有效内存
        /// </summary>
        /// <returns></returns>
        public static long GetMemoryAvailable()
        {
            long availablebytes = 0;
            try
            {
                System.Management.ManagementClass mos = new System.Management.ManagementClass("Win32_OperatingSystem");
                foreach (System.Management.ManagementObject mo in mos.GetInstances())
                {
                    if (mo["FreePhysicalMemory"] != null)
                    {
                        availablebytes = 1024 * long.Parse(mo["FreePhysicalMemory"].ToString());
                    }
                }
            }
            catch
            {
            }
            return availablebytes;
        } 

        #endregion

        #region 获取网络地址

        /// <summary>
        /// 获取本机所有的IP
        /// </summary>
        /// <param name="ipFormat">满足的ip格式串</param>
        /// <returns></returns>
        public static ArrayList GetIPs(string ipFormat)
        {
            ArrayList ips = new ArrayList();

            System.Net.IPAddress[] ipList = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
            if (ipList.Length > 0)
            {
                for (int i = 0; i < ipList.Length; i++)
                {
                    if (ipFormat != "")
                    {
                        string[] ipFormatSplit = ipFormat.Split('.');
                        string[] ipSplit = ipList[i].ToString().Split('.');
                        bool bEqual = true;
                        for (int j = 0; j < ipFormatSplit.Length; j++)
                        {
                            if (ipFormatSplit[j] == "*")
                                continue;
                            else
                            {
                                try
                                {
                                    int ipFormatVal = int.Parse(ipFormatSplit[j]);
                                    int ipVal = int.Parse(ipSplit[j]);
                                    if (ipFormatVal == ipVal)
                                        continue;
                                    else
                                    {
                                        bEqual = false;
                                        break;
                                    }
                                }
                                catch
                                {
                                    bEqual = false;
                                    break;
                                }
                            }
                        }
                        if (bEqual)
                            ips.Add(ipList[i].ToString());
                    }
                    else
                        ips.Add(ipList[i].ToString());
                }
            }
            else
                ips.Add("127.0.0.1");

            return ips;
        }

        public static ArrayList GetIPs()
        {
            return GetIPs("");
        }

        /// <summary>
        /// 获得本机IP地址
        /// </summary>
        /// <param name="ipFormat">满足的ip格式串</param>
        /// <returns></returns>
        public static string GetIP(string ipFormat)
        {
            System.Net.IPAddress[] ipList = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
            if (ipFormat != "")
            {
                string[] ipFormatSplit = ipFormat.Split('.');
                for (int i = 0; i < ipList.Length; i++)
                {
                    string[] ipSplit = ipList[i].ToString().Split('.');
                    bool bEqual = true;
                    for (int j = 0; j < ipFormatSplit.Length; j++)
                    {
                        if (ipFormatSplit[j] == "*")
                            continue;
                        else
                        {
                            try
                            {
                                int ipFormatVal = int.Parse(ipFormatSplit[j]);
                                int ipVal = int.Parse(ipSplit[j]);
                                if (ipFormatVal == ipVal)
                                    continue;
                                else
                                {
                                    bEqual = false;
                                    break;
                                }
                            }
                            catch
                            {
                                bEqual = false;
                                break;
                            }
                        }
                    }
                    if (bEqual)
                        return ipList[i].ToString();
                }
            }

            if (ipList.Length > 0)
                return ipList[0].ToString();
            else
                return "127.0.0.1";
        }

        /// <summary>
        /// 获得本机IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            return GetIP("");
        }

        /// <summary>
        /// 处理本地ip，如果参数是127.0.0.1则自行获取
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string DealLocalIP(string ip, string ipFormat)
        {
            if (ip == "127.0.0.1")
                return GetIP(ipFormat);
            else
                return ip;
        }

        public static string DealLocalIP(string ip)
        {
            return ip;
        }

        /// <summary>
        /// 获取本机MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMac()
        {
            string mac = "";
            System.Management.ManagementObjectSearcher query = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
            System.Management.ManagementObjectCollection queryCollection = query.Get();
            foreach (System.Management.ManagementObject mo in queryCollection)
            {
                if (mo["IPEnabled"].ToString() == "True")
                {
                    mac = mo["MacAddress"].ToString();
                }
            }
            return (mac);
        }

        /// <summary>
        /// 获取所有网卡的MAC地址
        /// </summary>
        /// <returns></returns>
        public static ArrayList GetMacs()
        {
            ArrayList macs = new ArrayList();
            System.Management.ManagementClass mc = new System.Management.ManagementClass("Win32_NetworkAdapterConfiguration");
            System.Management.ManagementObjectCollection moc = mc.GetInstances();

            foreach (System.Management.ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    macs.Add(mo["MacAddress"].ToString());
                }
            }
            return macs;
        }

        #endregion

        #region 消息显示类

        /// <summary>
        /// 基本的消息显示类，如果要自定义消息显示的界面可以修改本函数
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <param name="defaultButton"></param>
        /// <returns></returns>
        private static DialogResult MsgBase(IWin32Window MsgOwner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            if (MsgOwner == null)
            {
                return MessageBox.Show(text, caption, buttons, icon, defaultButton);
            }
            else
            {
                return MessageBox.Show(MsgOwner, text, caption, buttons, icon, defaultButton);
            }
        }

        /// <summary>
        /// 基本的消息显示类的一个重载
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        private static DialogResult MsgBase(IWin32Window MsgOwner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MsgBase(MsgOwner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// 显示提示对话框，只有ok按钮
        /// </summary>
        /// <param name="infos"></param>
        public static void MsgOK(IWin32Window MsgOwner, Object infos)
        {
            MsgBase(MsgOwner, infos.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static void MsgOK(Object infos)
        {
            MsgOK(null, infos);
        }

        /// <summary>
        /// 显示错误对话框，只有ok按钮
        /// </summary>
        /// <param name="infos"></param>
        public static void MsgError(IWin32Window MsgOwner, Object infos)
        {
            MsgBase(MsgOwner, infos.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
        public static void MsgError(Object infos)
        {
            MsgError(null, infos);
        }

        /// <summary>
        /// 显示警告对话框，只有ok按钮
        /// </summary>
        /// <param name="infos"></param>
        public static void MsgWarning(IWin32Window MsgOwner, Object infos)
        {
            MsgBase(MsgOwner, infos.ToString(), "警告", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        public static void MsgWarning(Object infos)
        {
            MsgWarning(null, infos);
        }

        /// <summary>
        /// 显示yesno选择对话框
        /// defbts为缺省按钮，1表示缺省为yes，2表示缺省为no
        /// </summary>
        /// <param name="MsgOwner"></param>
        /// <param name="infos"></param>
        /// <param name="defbts"></param>
        /// <param name="sTitle"></param>
        /// <returns></returns>
        public static DialogResult MsgYesNo(IWin32Window MsgOwner, Object infos, int defbts, string sTitle)
        {
            MessageBoxDefaultButton mbs;
            if (defbts == 1)
                mbs = MessageBoxDefaultButton.Button1;
            else
                mbs = MessageBoxDefaultButton.Button2;
            return MsgBase(MsgOwner, infos.ToString(), sTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question, mbs);
        }
        public static DialogResult MsgYesNo(Object infos, int defbts, string sTitle)
        {
            return MsgYesNo(null, infos, defbts, sTitle);
        }

        /// <summary>
        /// 显示yesno选择对话框
        /// defbts为缺省按钮，1表示缺省为yes，2表示缺省为no
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="defbts"></param>
        /// <returns></returns>
        public static DialogResult MsgYesNo(IWin32Window MsgOwner, Object infos, int defbts)
        {
            return MsgYesNo(MsgOwner, infos, defbts, "请选择");
        }
        public static DialogResult MsgYesNo(Object infos, int defbts)
        {
            return MsgYesNo(null, infos, defbts);
        }

        /// <summary>
        /// 显示yesno选择对话框，缺省按钮为no
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static DialogResult MsgYesNo(IWin32Window MsgOwner, Object infos, string sTitle)
        {
            return MsgYesNo(MsgOwner, infos, 2, sTitle);
        }
        public static DialogResult MsgYesNo(Object infos, string sTitle)
        {
            return MsgYesNo(null, infos, sTitle);
        }

        public static DialogResult MsgYesNo(IWin32Window MsgOwner, Object infos)
        {
            return MsgYesNo(MsgOwner, infos, 2, "请选择");
        }
        public static DialogResult MsgYesNo(Object infos)
        {
            return MsgYesNo(null, infos);
        }

        /// <summary>
        /// 显示okcancel选择对话框
        /// defbts为缺省按钮，1表示缺省为ok，2表示缺省为cancel
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="defbts"></param>
        /// <returns></returns>
        public static DialogResult MsgOkCancel(IWin32Window MsgOwner, Object infos, int defbts, string Title)
        {
            MessageBoxDefaultButton mbs;
            if (defbts == 1)
                mbs = MessageBoxDefaultButton.Button1;
            else
                mbs = MessageBoxDefaultButton.Button2;
            return MsgBase(MsgOwner, infos.ToString(), Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, mbs);
        }
        public static DialogResult MsgOkCancel(Object infos, int defbts, string Title)
        {
            return MsgOkCancel(null, infos, defbts, Title);
        }

        public static DialogResult MsgOkCancel(IWin32Window MsgOwner, Object infos, int defbts)
        {
            return MsgOkCancel(MsgOwner, infos, defbts, "请确认");
        }
        public static DialogResult MsgOkCancel(Object infos, int defbts)
        {
            return MsgOkCancel(null, infos, defbts);
        }

        /// <summary>
        /// 显示okcancel选择对话框，缺省按钮为cancel
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static DialogResult MsgOkCancel(IWin32Window MsgOwner, Object infos)
        {
            return MsgOkCancel(MsgOwner, infos, 2);
        }
        public static DialogResult MsgOkCancel(Object infos)
        {
            return MsgOkCancel(null, infos);
        }

        public static DialogResult MsgOkCancel(IWin32Window MsgOwner, Object infos, string Title)
        {
            return MsgOkCancel(MsgOwner, infos, 2, Title);
        }
        public static DialogResult MsgOkCancel(Object infos, string Title)
        {
            return MsgOkCancel(null, infos, Title);
        }

        /// <summary>
        /// 显示yesnocancel选择对话框
        /// defbts为缺省按钮，1表示缺省为yes，2表示缺省为no，3表示缺省为cancel
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="defbts"></param>
        /// <returns></returns>
        public static DialogResult MsgYesNoCancel(IWin32Window MsgOwner, Object infos, int defbts, string Title)
        {
            MessageBoxDefaultButton mbs;
            if (defbts == 1)
                mbs = MessageBoxDefaultButton.Button1;
            else if (defbts == 2)
                mbs = MessageBoxDefaultButton.Button2;
            else
                mbs = MessageBoxDefaultButton.Button3;
            return MsgBase(MsgOwner, infos.ToString(), Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, mbs);
        }
        public static DialogResult MsgYesNoCancel(Object infos, int defbts, string Title)
        {
            return MsgYesNoCancel(null, infos, defbts, Title);
        }

        public static DialogResult MsgYesNoCancel(IWin32Window MsgOwner, Object infos, int defbts)
        {
            return MsgYesNoCancel(MsgOwner, infos, defbts, "请确认");
        }
        public static DialogResult MsgYesNoCancel(Object infos, int defbts)
        {
            return MsgYesNoCancel(null, infos, defbts);
        }

        /// <summary>
        /// 显示yesnocancel选择对话框，缺省按钮为cancel
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static DialogResult MsgYesNoCancel(IWin32Window MsgOwner, Object infos)
        {
            return MsgYesNoCancel(MsgOwner, infos, 3, "请确认");
        }
        public static DialogResult MsgYesNoCancel(Object infos)
        {
            return MsgYesNoCancel(null, infos);
        }

        public static DialogResult MsgYesNoCancel(IWin32Window MsgOwner, Object infos, string Title)
        {
            return MsgYesNoCancel(MsgOwner, infos, 3, Title);
        }
        public static DialogResult MsgYesNoCancel(Object infos, string Title)
        {
            return MsgYesNoCancel(null, infos, Title);
        }

        /// <summary>
        /// 显示retrycancel选择对话框
        /// defbts为缺省按钮，1表示缺省为retry，2表示缺省为cancel
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="defbts"></param>
        /// <returns></returns>
        public static DialogResult MsgRetryCancel(IWin32Window MsgOwner, Object infos, int defbts, string Title)
        {
            MessageBoxDefaultButton mbs;
            if (defbts == 1)
                mbs = MessageBoxDefaultButton.Button1;
            else
                mbs = MessageBoxDefaultButton.Button2;
            return MsgBase(MsgOwner, infos.ToString(), Title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Question, mbs);
        }
        public static DialogResult MsgRetryCancel(Object infos, int defbts, string Title)
        {
            return MsgRetryCancel(null, infos, defbts, Title);
        }

        public static DialogResult MsgRetryCancel(IWin32Window MsgOwner, Object infos, int defbts)
        {
            return MsgRetryCancel(MsgOwner, infos, defbts, "请确认");
        }
        public static DialogResult MsgRetryCancel(Object infos, int defbts)
        {
            return MsgRetryCancel(null, infos, defbts);
        }

        /// <summary>
        /// 显示retrycancel选择对话框，缺省按钮为retry
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static DialogResult MsgRetryCancel(IWin32Window MsgOwner, Object infos)
        {
            return MsgRetryCancel(MsgOwner, infos, 1, "请确认");
        }
        public static DialogResult MsgRetryCancel(Object infos)
        {
            return MsgRetryCancel(null, infos);
        }

        public static DialogResult MsgRetryCancel(IWin32Window MsgOwner, Object infos, string Title)
        {
            return MsgRetryCancel(MsgOwner, infos, 1, Title);
        }
        public static DialogResult MsgRetryCancel(Object infos, string Title)
        {
            return MsgRetryCancel(null, infos, Title);
        }

        /// <summary>
        /// 显示abortretryignore选择对话框
        /// defbts为缺省按钮，1表示缺省为abort，2表示缺省为retry，3表示缺省为ignore
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="defbts"></param>
        /// <returns></returns>
        public static DialogResult MsgAbortRetryIgnore(IWin32Window MsgOwner, Object infos, int defbts, string Title)
        {
            MessageBoxDefaultButton mbs;
            if (defbts == 1)
                mbs = MessageBoxDefaultButton.Button1;
            else if (defbts == 2)
                mbs = MessageBoxDefaultButton.Button2;
            else
                mbs = MessageBoxDefaultButton.Button3;
            return MsgBase(MsgOwner, infos.ToString(), Title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question, mbs);
        }
        public static DialogResult MsgAbortRetryIgnore(Object infos, int defbts, string Title)
        {
            return MsgAbortRetryIgnore(null, infos, defbts, Title);
        }

        public static DialogResult MsgAbortRetryIgnore(IWin32Window MsgOwner, Object infos, int defbts)
        {
            return MsgAbortRetryIgnore(MsgOwner, infos, defbts, "询问");
        }
        public static DialogResult MsgAbortRetryIgnore(Object infos, int defbts)
        {
            return MsgAbortRetryIgnore(null, infos, defbts);
        }

        /// <summary>
        /// 显示abortretryignore选择对话框，缺省按钮为retry
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static DialogResult MsgAbortRetryIgnore(IWin32Window MsgOwner, Object infos)
        {
            return MsgAbortRetryIgnore(MsgOwner, infos, 2);
        }
        public static DialogResult MsgAbortRetryIgnore(Object infos)
        {
            return MsgAbortRetryIgnore(null, infos);
        }

        public static DialogResult MsgAbortRetryIgnore(IWin32Window MsgOwner, Object infos, string Title)
        {
            return MsgAbortRetryIgnore(MsgOwner, infos, 2, Title);
        }
        public static DialogResult MsgAbortRetryIgnore(Object infos, string Title)
        {
            return MsgAbortRetryIgnore(null, infos, Title);
        }

        #endregion

        #region 安全处理类

        private static byte[] IV ={ 0x11, 0x22, 0x33, 0x44, 0x55, 0xAA, 0xBB, 0xDD };
        private static string _EncryptKey = "(!@#$%^&*)";

        /// <summary>
        /// 取全局密钥
        /// </summary>
        /// <returns></returns>
        private static string GetEncryptKey()
        {
            //msKey的长度必须小于等于与IV的元素数量相等
            string msKey = _EncryptKey;
            _EncryptKey = "(!@#$%^&*)";
            return msKey;
        }

        public static void SetEncryptKey(string newEncryptKey)
        {
            _EncryptKey = newEncryptKey;
        }

        /// <summary> 
        /// DES加密字符串
        /// </summary> 
        /// <param name="EncryptString">加密后的字符串</param> 
        /// <param name="strText">要加密的字符串</param> 
        /// <returns>是否成功</returns> 
        public static bool EncryptString(ref string encryptString, string strText)
        {
            bool bResult = false;
            byte[] byKey = null;
            byte[] byInput = null;
            string sKey = GetEncryptKey();

            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byInput = Encoding.UTF8.GetBytes(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(byInput, 0, byInput.Length);
                cs.FlushFinalBlock();
                encryptString = Convert.ToBase64String(ms.ToArray());
                bResult = true;
            }
            catch (System.Exception error)
            {
                _lasterror = "加密字符串出错！" + error.Message;
            }
            return bResult;
        }

        public static bool EncryptString(ref string encryptString, string strText, string encryptKey)
        {
            string lastEncryptKey = GetEncryptKey();
            if (encryptKey != "")
                SetEncryptKey(encryptKey);
            bool bResult = EncryptString(ref encryptString, strText);
            SetEncryptKey(lastEncryptKey);
            return bResult;
        }

        /// <summary> 
        /// DES解密字符串
        /// </summary> 
        /// <param name="DeEncryptString">解密后的字符串</param> 
        /// <param name="strText">要解密的字符串</param> 
        /// <returns>是否成功</returns> 
        public static bool DecryptString(ref string decryptString, string strText)
        {
            bool bResult = false;
            byte[] byKey = null;
            byte[] byInput = new Byte[strText.Length];
            string sKey = GetEncryptKey();
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byInput = Convert.FromBase64String(strText);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(byInput, 0, byInput.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                decryptString = encoding.GetString(ms.ToArray());
                bResult = true;
            }
            catch (System.Exception error)
            {
                _lasterror = "解密字符串出错！" + error.Message;
            }
            return bResult;
        }

        public static bool DecryptString(ref string decryptString, string strText, string encryptKey)
        {
            string lastEncryptKey = GetEncryptKey();
            if (encryptKey != "")
                SetEncryptKey(encryptKey);
            bool bResult = DecryptString(ref decryptString, strText);
            SetEncryptKey(lastEncryptKey);
            return bResult;
        }

        /// <summary> 
        /// DES加密文件
        /// </summary> 
        /// <param name="SourceFile">被加密文件</param> 
        /// <param name="EncryptFile">加密后文件</param> 
        public static bool EncryptFile(string sourceFile, string encryptFile)
        {
            bool bResult = false;
            byte[] byKey = null;

            string sKey = GetEncryptKey();
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8));
                FileStream fin = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(encryptFile, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);
                //Create variables to help with read and write. 
                byte[] bin = new byte[100]; //This is intermediate storage for the encryption. 
                long rdlen = 0; //This is the total number of bytes written. 
                long totlen = fin.Length; //This is the total length of the input file. 
                int len; //This is the number of bytes to be written at a time. 

                DES des = new DESCryptoServiceProvider();
                CryptoStream encStream = new CryptoStream(fout, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);


                //Read from the input file, then encrypt and write to the output file. 
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    encStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }

                encStream.Close();
                fout.Close();
                fin.Close();
                bResult = true;
            }
            catch (System.Exception error)
            {
                _lasterror = "加密文件出错！" + error.Message;
            }

            return bResult;
        }

        public static bool EncryptFile(string sourceFile, string encryptFile, string encryptKey)
        {
            string lastEncryptKey = GetEncryptKey();
            if (encryptKey != "")
                SetEncryptKey(encryptKey);
            bool bResult = EncryptFile(sourceFile, encryptFile);
            SetEncryptKey(lastEncryptKey);
            return bResult;
        }

        /// <summary> 
        /// DES解密文件
        /// </summary> 
        /// <param name="EncryptFile">被解密文件</param> 
        /// <param name="DecryptFile">解密后的文件</param> 
        public static bool DecryptFile(string encryptFile, string decryptFile)
        {
            bool bResult = false;
            byte[] byKey = null;

            string sKey = GetEncryptKey();
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8));
                FileStream fin = new FileStream(encryptFile, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(decryptFile, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);
                //Create variables to help with read and write. 
                byte[] bin = new byte[100]; //This is intermediate storage for the encryption. 
                long rdlen = 0; //This is the total number of bytes written. 
                long totlen = fin.Length; //This is the total length of the input file. 
                int len; //This is the number of bytes to be written at a time. 

                DES des = new DESCryptoServiceProvider();
                CryptoStream encStream = new CryptoStream(fout, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);


                //Read from the input file, then encrypt and write to the output file. 
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    encStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }

                encStream.Close();
                fout.Close();
                fin.Close();

                bResult = true;
            }
            catch (System.Exception error)
            {
                _lasterror = "解密文件出错！" + error.Message;
            }

            return bResult;
        }

        public static bool DecryptFile(string encryptFile, string decryptFile, string encryptKey)
        {
            string lastEncryptKey = GetEncryptKey();
            if (encryptKey != "")
                SetEncryptKey(encryptKey);
            bool bResult = DecryptFile(encryptFile, decryptFile);
            SetEncryptKey(lastEncryptKey);
            return bResult;
        }

        /// <summary> 
        /// 产生MD5校验码,用于产生口令等敏感信息的校验码
        /// </summary> 
        /// <param name="SourceString">被校验的字符串</param> 
        /// <returns>MD5校验码</returns> 
        public static string GetVerifyMD5(string SourceString)
        {
            SourceString = SourceString + GetEncryptKey();

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] byResult = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(SourceString));
            return System.Text.Encoding.UTF8.GetString(byResult);
        }

        /// <summary> 
        /// 产生SHA1校验码,用于产生口令等敏感信息的校验码
        /// </summary> 
        /// <param name="SourceString">被校验的字符串</param> 
        /// <returns>SHA1校验码</returns> 
        public static string GetVerifySHA1(string SourceString)
        {
            SourceString = SourceString + GetEncryptKey();

            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] byResult = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(SourceString));
            return System.Text.Encoding.UTF8.GetString(byResult);
        }

        /// <summary>
        /// 产生UTF8字符串校验码,用于产生口令等敏感信息的校验码
        /// </summary>
        /// <param name="SourceString">被校验的UTF8字符串</param>
        /// <param name="SplitChar">返回校验码时使用的分割符,例如:"-" </param>
        /// <returns>校验码</returns>
        public static string GetVerifyUTF8(string SourceString, string SplitChar)
        {
            SourceString = SourceString + GetEncryptKey();

            string sResult = "";
            System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
            Byte[] btResult = enc.GetBytes(SourceString);
            btResult = HashAlgorithm.Create().ComputeHash(btResult);

            for (int i = 0; i < btResult.Length; i++)
            {
                sResult = sResult + btResult[i].ToString("X") + SplitChar;
            }
            if (SplitChar != "")
            {
                sResult = sResult.Substring(0, sResult.Length - SplitChar.Length);
            }

            return sResult;
        }

        /// <summary>
        /// 产生Unicode字符串校验码,用于产生口令等敏感信息的校验码
        /// </summary>
        /// <param name="SourceString">被校验的Unicode字符串</param>
        /// <param name="SplitChar">返回校验码时使用的分割符,例如:"-" </param>
        /// <returns>校验码</returns>
        public static string GetVerifyUnicode(string SourceString, string SplitChar)
        {
            SourceString = SourceString + GetEncryptKey();

            string sResult = "";
            System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
            Byte[] btResult = enc.GetBytes(SourceString);
            btResult = HashAlgorithm.Create().ComputeHash(btResult);

            for (int i = 0; i < btResult.Length; i++)
            {
                sResult = sResult + btResult[i].ToString("X") + SplitChar;
            }
            if (SplitChar != "")
            {
                sResult = sResult.Substring(0, sResult.Length - SplitChar.Length);
            }

            return sResult;
        }

        #endregion

        #region MD5加密

        /// <summary>
        /// MD5加密,和动网上的16/32位MD5加密结果相同
        /// </summary>
        /// <param name="strSource">待加密字串</param>
        /// <param name="length">16或32值之一,其它则采用.net默认MD5加密算法</param>
        /// <returns>加密后的字串</returns>
        public static string MD5Encrypt(string strSource, int length)
        {
            try
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(strSource);
                byte[] hashValue = ((System.Security.Cryptography.HashAlgorithm)System.Security.Cryptography.CryptoConfig.CreateFromName("MD5")).ComputeHash(bytes);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                switch (length)
                {
                    case 16:
                        for (int i = 4; i < 12; i++)
                            sb.Append(hashValue[i].ToString("x2"));
                        break;
                    case 32:
                        for (int i = 0; i < 16; i++)
                        {
                            sb.Append(hashValue[i].ToString("x2"));
                        }
                        break;
                    default:
                        for (int i = 0; i < hashValue.Length; i++)
                        {
                            sb.Append(hashValue[i].ToString("x2"));
                        }
                        break;
                }
                return sb.ToString();
            }
            catch
            {
                return String.Empty;
            }
        }

        public static string MD5Encrypt(string strSource)
        {
            try
            {
                byte[] hashvalue = (new System.Security.Cryptography.MD5CryptoServiceProvider()).ComputeHash(System.Text.Encoding.UTF8.GetBytes(strSource));
                return BitConverter.ToString(hashvalue);
            }
            catch
            {
                return String.Empty;
            }
        }

        #endregion

        #region SHA加密

        public string SHA1Encrypt(string strIN, bool isReturnNum)
        {
            byte[] tmpByte;
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            tmpByte = sha1.ComputeHash(GetKeyByteArray(strIN));
            sha1.Clear();

            return GetStringValue(tmpByte, isReturnNum);
        }

        public string SHA256Encrypt(string strIN, bool isReturnNum)
        {
            byte[] tmpByte;
            SHA256 sha256 = new SHA256Managed();

            tmpByte = sha256.ComputeHash(GetKeyByteArray(strIN));
            sha256.Clear();

            return GetStringValue(tmpByte, isReturnNum);
        }

        public string SHA512Encrypt(string strIN, bool isReturnNum)
        {
            byte[] tmpByte;
            SHA512 sha512 = new SHA512Managed();

            tmpByte = sha512.ComputeHash(GetKeyByteArray(strIN));
            sha512.Clear();

            return GetStringValue(tmpByte, isReturnNum);
        }

        private string GetStringValue(byte[] Byte, bool isReturnNum)
        {
            string tmpString = "";

            if (isReturnNum == false)
            {
                ASCIIEncoding Asc = new ASCIIEncoding();
                tmpString = Asc.GetString(Byte);
            }
            else
            {
                int iCounter;
                for(iCounter = 0; iCounter < Byte.Length; iCounter++)
                {
                    tmpString = tmpString + Byte[iCounter].ToString();
                }
            }

            return tmpString;
        }

        private byte[] GetKeyByteArray(string strKey)
        {
            ASCIIEncoding Asc = new ASCIIEncoding();

            int tmpStrLen = strKey.Length;
            byte[] tmpByte = new byte[tmpStrLen - 1];

            tmpByte = Asc.GetBytes(strKey);

            return tmpByte;
        }

        #endregion

        #region DES加解密

        /// <summary>
        /// 使用DES加密（Added by niehl 2005-4-6）
        /// </summary>
        /// <param name="originalValue">待加密的字符串</param>
        /// <param name="key">密钥(最大长度8)</param>
        /// <param name="IV">初始化向量(最大长度8)</param>
        /// <returns>加密后的字符串</returns>
        public string DESEncrypt(string originalValue, string key, string IV)
        {
            //将key和IV处理成8个字符
            key += "12345678";
            IV += "12345678";
            key = key.Substring(0, 8);
            IV = IV.Substring(0, 8);

            SymmetricAlgorithm sa;
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            sa = new DESCryptoServiceProvider();
            sa.Key = Encoding.UTF8.GetBytes(key);
            sa.IV = Encoding.UTF8.GetBytes(IV);
            ct = sa.CreateEncryptor();

            byt = Encoding.UTF8.GetBytes(originalValue);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();

            return Convert.ToBase64String(ms.ToArray());

        }

        public string DESEncrypt(string originalValue, string key)
        {
            return DESEncrypt(originalValue, key, key);
        }

        /// <summary>
        /// 使用DES解密（Added by niehl 2005-4-6）
        /// </summary>
        /// <param name="encryptedValue">待解密的字符串</param>
        /// <param name="key">密钥(最大长度8)</param>
        /// <param name="IV">m初始化向量(最大长度8)</param>
        /// <returns>解密后的字符串</returns>
        public string DESDecrypt(string encryptedValue, string key, string IV)
        {
            //将key和IV处理成8个字符
            key += "12345678";
            IV += "12345678";
            key = key.Substring(0, 8);
            IV = IV.Substring(0, 8);

            SymmetricAlgorithm sa;
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            sa = new DESCryptoServiceProvider();
            sa.Key = Encoding.UTF8.GetBytes(key);
            sa.IV = Encoding.UTF8.GetBytes(IV);
            ct = sa.CreateDecryptor();

            byt = Convert.FromBase64String(encryptedValue);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct,
         CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();

            return Encoding.UTF8.GetString(ms.ToArray());

        }

        public string DESDecrypt(string encryptedValue, string key)
        {
            return DESDecrypt(encryptedValue, key, key);
        }

        #endregion

        #region 错误处理类

        private static string _lasterror = "";

        public static string GetLastError()
        {
            return _lasterror;
        }

        #endregion

        #region 数据压缩

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            //byte[] buffer = new byte[2000];
            //int len;
            //while ((len = input.Read(buffer, 0, 2000)) > 0)
            //{
            //    output.Write(buffer, 0, len);
            //}
            //output.Flush();
            int copyLength = (int)input.Length;
            byte[] buffer = new byte[copyLength];
            input.Read(buffer, 0, copyLength);
            output.Write(buffer, 0, copyLength);
            output.Flush();
        }

        public static bool CompressStr(string inStr, out byte[] outBytes)
        {
            bool result = true;
            outBytes = null;

            MemoryStream outStream = new MemoryStream();
            ComponentAce.Compression.Libs.zlib.ZOutputStream outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream, ComponentAce.Compression.Libs.zlib.zlibConst.Z_DEFAULT_COMPRESSION);

            byte[] inBytes = System.Text.Encoding.UTF8.GetBytes(inStr);
            MemoryStream inStream = new MemoryStream(inBytes);

            try
            {
                CopyStream(inStream, outZStream);
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                outZStream.Close();
                outStream.Close();
                inStream.Close();
            }

            if (result)
            {
                outBytes = outStream.ToArray();
                if (outBytes.Length >= inBytes.Length)
                    result = false;
            }
            return result;
        }

        public static bool DecompressStr(byte[] inBytes, out string outStr)
        {
            bool result = true;
            outStr = "";

            MemoryStream outStream = new MemoryStream();
            ComponentAce.Compression.Libs.zlib.ZOutputStream outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream);

            MemoryStream inStream = new MemoryStream(inBytes);

            try
            {
                CopyStream(inStream, outZStream);
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                outZStream.Close();
                outStream.Close();
                inStream.Close();
            }

            if (result)
            {
                byte[] outBytes = outStream.ToArray();
                outStr = System.Text.Encoding.UTF8.GetString(outBytes);
            }
            return result;
        }

        public static bool CompressBin(byte[] inBytes, out byte[] outBytes)
        {
            bool result = true;
            outBytes = null;

            MemoryStream outStream = new MemoryStream();
            ComponentAce.Compression.Libs.zlib.ZOutputStream outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream, ComponentAce.Compression.Libs.zlib.zlibConst.Z_DEFAULT_COMPRESSION);

            MemoryStream inStream = new MemoryStream(inBytes);

            try
            {
                CopyStream(inStream, outZStream);
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                outZStream.Close();
                outStream.Close();
                inStream.Close();
            }

            if (result)
            {
                outBytes = outStream.ToArray();
                if (outBytes.Length >= inBytes.Length)
                    result = false;
            }
            return result;
        }

        public static bool DecompressBin(byte[] inBytes, out byte[] outBytes)
        {
            bool result = true;
            outBytes = null;

            MemoryStream outStream = new MemoryStream();
            ComponentAce.Compression.Libs.zlib.ZOutputStream outZStream = new ComponentAce.Compression.Libs.zlib.ZOutputStream(outStream);

            MemoryStream inStream = new MemoryStream(inBytes);

            try
            {
                CopyStream(inStream, outZStream);
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                outZStream.Close();
                outStream.Close();
                inStream.Close();
            }

            if (result)
                outBytes = outStream.ToArray();
            return result;
        }

        #endregion

        #region 其他

        public static object BytesToStruct(byte[] bytes, Type strcutType)
        {
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static unsafe byte[] GetBytes<T>(object obj)
        {
            byte[] arr = new byte[Marshal.SizeOf(obj)];

            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            void* pVer = handle.AddrOfPinnedObject().ToPointer();

            fixed (byte* parr = arr)
            {
                *parr = *(byte*)pVer;
            }

            handle.Free();
            return arr;
        }

        public static unsafe T GetObject<T>(byte[] bytes) where T : new()
        {
            T rect = new T();
            GCHandle handle = GCHandle.Alloc(rect, GCHandleType.Pinned);
            void* pVer = handle.AddrOfPinnedObject().ToPointer();

            fixed (byte* b = bytes)
            {
                *(byte*)pVer = *b;
            }

            handle.Free();
            return rect;
        }

        public static unsafe int ReserveInt(int srcint)
        {
            try
            {
                byte[] bytes = new byte[4];
                fixed (byte* pointer = bytes)
                {
                    *((int*)pointer) = srcint;
                }
                byte t = bytes[0]; bytes[0] = bytes[3]; bytes[3] = t;
                t = bytes[1]; bytes[1] = bytes[2]; bytes[2] = t;
                int dstint = 0;
                fixed (byte* pointer = bytes)
                {
                    dstint = *((int*)pointer);
                }
                return dstint;
            }
            catch
            {
                return srcint;
            }
        }

        public static unsafe byte[] TransInt2ByteArr(int srcint)
        {
            try
            {
                byte[] bytes = new byte[4];
                fixed (byte* pointer = bytes)
                {
                    *((int*)pointer) = srcint;
                }
                byte t = bytes[0]; bytes[0] = bytes[3]; bytes[3] = t;
                t = bytes[1]; bytes[1] = bytes[2]; bytes[2] = t;
                return bytes;
            }
            catch
            {
                return null;
            }

        }

        public static unsafe int TransByteArr2Int(byte[] arr)
        {
            if (arr.Length < 4)
                return 0;

            try
            {
                byte[] bytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    bytes[i] = arr[i];
                }
                byte t = bytes[0]; bytes[0] = bytes[3]; bytes[3] = t;
                t = bytes[1]; bytes[1] = bytes[2]; bytes[2] = t;
                int dstint = 0;
                fixed (byte* pointer = bytes)
                {
                    dstint = *((int*)pointer);
                }
                return dstint;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region 随机值

        public static Random random = new Random();

        #endregion

    }

    /// <summary>
    /// 数据基本访问
    /// </summary>
    public class HTDBBaseFunc
    {
        #region 公有变量

        //连接类型枚举
        public enum ConnectionTypeEnum
        {
            Sql = 1,
            Oracle = 2,
            SqlOleDB = 3,
            SybaseOleDB = 4,
            MySql = 5,
            MySqlUseTran = 6
        }

        //连接类型属性
        public ConnectionTypeEnum ConnectionType
        {
            get
            {
                return _conntype;
            }
            set
            {
                _conntype = value;
            }
        }

        //连接字符串属性
        public string ConnectionString
        {
            //Provider=SQLOLEDB.1;data source=bzy,1433;user id=sa;password=;initial catalog=HTINFO;
            get
            {
                return _connstring;
            }
            set
            {
                _connstring = value;
            }
        }

        //错误处理模式枚举
        public struct ProcessErrorModeEnum
        {
            public static int None = 0;
            public static int ShowError = 1;
            public static int WriteConsole = 2;
            public static int WriteWinLog = 4;
        }

        //错误处理模式
        public int ProcessErrorMode
        {
            get
            {
                return _processErrorMode;
            }
            set
            {
                _processErrorMode = value;
            }
        }

        //存储过程参数结构
        public class StoredProcedureParam
        {
            public string Name;
            public string Type;
            public object Value;
            public ParameterDirection Direction;
        }

        //判断数据库是否已经连接
        public bool DataBaseConnected
        {
            get
            {
                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        return (mSqlCn != null && mSqlCn.State != ConnectionState.Closed);
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        return (mOleCn != null && mOleCn.State != ConnectionState.Closed);
                    case ConnectionTypeEnum.Oracle:
                        return (mOraCn != null && mOraCn.State != ConnectionState.Closed);
                    case ConnectionTypeEnum.MySql:
                        return (mySqlCn != null && mySqlCn.State != ConnectionState.Closed);
                    case ConnectionTypeEnum.MySqlUseTran:
                        return (mySqlCn != null && mySqlCn.State != ConnectionState.Closed);
                }
                return false;
            }
        }

        //是否WEB处理模式,在WEB处理模式下脚本执行后连接就关闭
        public bool WebProcessMode
        {
            get
            {
                return _webProcessMode;
            }
            set
            {
                _webProcessMode = value;
            }
        }
        //在一次数据库操作后保持一次连接不关闭,仅用于WebProcessMode下
        public bool HoldDbConnectOneTimeForWebProcessMode = false;

        //保持事务不提交
        public bool HoldDbTransNoCommit = false;
        //执行存储过程时无需事务
        public bool ExecWithoutTrans = false;

        #endregion

        #region 私有变量

        //对应连接类型枚举中的Provider
        private string[] _connProvider ={ "", "", "", "SQLOLEDB.1", "Sybase.ASEOLEDBProvider.2", "MySql" };

        private string _connstring = "";//连接字符串属性的本地变量
        private ConnectionTypeEnum _conntype = ConnectionTypeEnum.SqlOleDB;//连接类型属性的本地变量
        private ArrayList _SqlCmdList = new ArrayList();//SQL批处理集合
        private int _processErrorMode = ProcessErrorModeEnum.ShowError;//错误处理模式
        private string _lastError = "";
        private bool _webProcessMode = false;//Web处理模式
        private bool _InTrans = false; //是否在事务中

        //声明处理SQL SERVER的对象
        private SqlConnection mSqlCn;
        private SqlCommand mSqlCmd;
        private SqlDataAdapter mSqlDA;
        private SqlTransaction mSqlTrans;

        //声明处理OleDB的对象
        private OleDbConnection mOleCn;
        private OleDbCommand mOleCmd;
        private OleDbDataAdapter mOleDA;
        private OleDbTransaction mOleTrans;

        //声明处理Oracle的对象
        private OracleConnection mOraCn;
        private OracleCommand mOraCmd;
        private OracleDataAdapter mOraDA;
        private OracleTransaction mOraTrans;

        //声明处理MySQL的对象
        private MySqlConnection mySqlCn;
        //private MySqlCommand mySqlCmd;
        //private MySqlDataAdapter mySqlDA;
        private MySqlTransaction mySqlTrans;

        #endregion

        #region 私有函数

        //拼接连接字符串
        private string GenConnectionString(string provider, string dbServerName, string dbServerPort, string dbName, string userName, string userPassword, bool integratedSecurity)
        {
            if (provider == "MySql" || provider == "MySqlUseTran")
                return String.Format("server={0}{1};uid={2};pwd={3};database={4};charset=utf8;allow user variables=true;pooling=false;Default Command Timeout=60;Connect Timeout=60", dbServerName, ((dbServerPort != "") ? ";port=" + dbServerPort : ""), userName, userPassword, dbName);

            string lsConnStr = "data source=" + dbServerName + ((dbServerPort != "") ? "," + dbServerPort : "");
            if (HTBaseFunc.NullToStr(provider) != "")
                lsConnStr = "Provider=" + ((provider == "MySqlUseTran") ? "MySql" : provider) + ";" + lsConnStr;
            if (HTBaseFunc.NullToStr(dbName) != "")
                lsConnStr += ";initial catalog=" + dbName;
            if (integratedSecurity == true)
                lsConnStr += ";Integrated Security=SSPI";
            else
                lsConnStr += ";user id=" + userName + ";password=" + userPassword;
            return lsConnStr;
        }

        //拼接连接字符串
        private string GenConnectionString(ConnectionTypeEnum connType, string dbServerName, string dbServerPort, string dbName, string userName, string userPassword, bool integratedSecurity)
        {
            return GenConnectionString(_connProvider[Convert.ToInt16(connType)], dbServerName, dbServerPort, dbName, userName, userPassword, integratedSecurity);
        }

        //连接数据库
        private bool Connect()
        {
            switch (_conntype)
            {
                case ConnectionTypeEnum.Sql:
                    try
                    {
                        if (mSqlCn != null)
                        {
                            mSqlCn.Close();
                            //mSqlCn.Dispose();
                        }
                        mSqlCn = new SqlConnection(_connstring);
                        mSqlCn.Open();

                        mSqlCmd = new SqlCommand("", mSqlCn);
                        mSqlCmd.CommandTimeout = 300;

                        mSqlDA = new SqlDataAdapter("", mSqlCn);
                        mSqlDA.SelectCommand.CommandTimeout = 300;
                    }
                    catch (SqlException e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    break;
                case ConnectionTypeEnum.SqlOleDB:
                case ConnectionTypeEnum.SybaseOleDB:
                    try
                    {
                        if (mOleCn != null)
                        {
                            mOleCn.Close();
                            //mOleCn.Dispose();
                        }
                        mOleCn = new OleDbConnection(_connstring);
                        mOleCn.Open();

                        mOleCmd = new OleDbCommand("", mOleCn);
                        mOleCmd.CommandTimeout = 300;

                        mOleDA = new OleDbDataAdapter("", mOleCn);
                        mOleDA.SelectCommand.CommandTimeout = 300;
                    }
                    catch (SqlException e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    break;
                case ConnectionTypeEnum.Oracle:
                    try
                    {
                        if (mOraCn != null)
                        {
                            mOraCn.Close();
                            //mOraCn.Dispose();
                        }
                        mOraCn = new OracleConnection(_connstring);
                        mOraCn.Open();

                        mOraCmd = new OracleCommand("", mOraCn);

                        mOraDA = new OracleDataAdapter("", mOraCn);
                    }
                    catch (SqlException e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    break;
                case ConnectionTypeEnum.MySql:
                case ConnectionTypeEnum.MySqlUseTran:
                    try
                    {
                        if (mySqlCn != null)
                        {
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                        }
                        mySqlCn = new MySqlConnection(_connstring);
                        mySqlCn.Open();

                        //mySqlCmd = new MySqlCommand("", mySqlCn);

                        //mySqlDA = new MySqlDataAdapter("", mySqlCn);
                    }
                    catch (MySqlException e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("连接数据库错误！" + e.Message);
                        return (false);
                    }
                    break;
            }

            return (true);

        }

        //断开数据库连接
        private bool DisConnect()
        {
            switch (_conntype)
            {
                case ConnectionTypeEnum.Sql:
                    try
                    {
                        mSqlCmd.Cancel();
                        //mSqlCmd.Dispose();
                        //mSqlDA.Dispose();
                        mSqlCn.Close();
                        //mSqlCn.Dispose();
                    }
                    catch (SqlException e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    break;
                case ConnectionTypeEnum.SqlOleDB:
                case ConnectionTypeEnum.SybaseOleDB:
                    try
                    {
                        mOleCmd.Cancel();
                        //mOleCmd.Dispose();
                        //mOleDA.Dispose();
                        mOleCn.Close();
                        //mOleCn.Dispose();
                    }
                    catch (SqlException e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    break;
                case ConnectionTypeEnum.Oracle:
                    try
                    {
                        mOraCmd.Cancel();
                        //mOraCmd.Dispose();
                        //mOraDA.Dispose();
                        mOraCn.Close();
                        //mOraCn.Dispose();
                    }
                    catch (SqlException e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    break;
                case ConnectionTypeEnum.MySql:
                case ConnectionTypeEnum.MySqlUseTran:
                    try
                    {
                        //mySqlCmd.Cancel();
                        //mySqlCmd.Dispose();
                        //mySqlDA.Dispose();
                        mySqlCn.Close();
                        //mySqlCn.Dispose();
                    }
                    catch (MySqlException e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    catch (Exception e)
                    {
                        ProcessError("断开数据库连接错误！" + e.Message);
                        return (false);
                    }
                    break;
            }

            return (true);
        }

        //错误处理
        private void ProcessError(string errMsg)
        {
            _lastError = errMsg;

            if (_processErrorMode == 1 || _processErrorMode == 3 || _processErrorMode == 5 || _processErrorMode == 7)
                HTBaseFunc.MsgError(errMsg);
            if (_processErrorMode == 2 || _processErrorMode == 3 || _processErrorMode == 6 || _processErrorMode == 7)
                Console.WriteLine(errMsg);
            if (_processErrorMode == 4 || _processErrorMode == 5 || _processErrorMode == 6 || _processErrorMode == 7)
                HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, errMsg, "HTDBBaseFunc");
        }

        #endregion

        #region 操作函数

        public HTDBBaseFunc()
        {
        }

        public HTDBBaseFunc(HTDBBaseFunc dbConn)
        {
            _conntype = dbConn.ConnectionType;
            _connstring = dbConn.ConnectionString;
        }

        //获取当前错误信息
        public string GetLastError()
        {
            return GetErrPerfix() + _lastError;
        }

        public void ClearLastError()
        {
            _lastError = "";
        }

        public string GetErrPerfix()
        {
            return "数据库访问错误：";
        }

        //连接数据库
        public bool ConnectDatabase(ConnectionTypeEnum connType, string dbServerName, string dbServerPort, string dbName, string userName, string userPassword, bool integratedSecurity)
        {
            ClearLastError();

            _conntype = connType;
            _connstring = GenConnectionString(connType, dbServerName, dbServerPort, dbName, userName, userPassword, integratedSecurity);
            return Connect();
        }

        public bool ConnectDatabase(ConnectionTypeEnum connType, string dbServerName, string dbServerPort, string dbName, string userName, string userPassword)
        {
            return ConnectDatabase(connType, dbServerName, dbServerPort, dbName, userName, userPassword, false);
        }

        public bool ConnectDatabase(ConnectionTypeEnum connType, string dbServerName, string dbServerPort, string dbName, bool integratedSecurity)
        {
            return ConnectDatabase(connType, dbServerName, dbServerPort, dbName, "", "", true);
        }

        public bool ConnectDatabase()
        {
            ClearLastError();

            if (HTBaseFunc.NullToStr(_connstring) == "")
            {
                ProcessError("连接数据库错误！没有设置数据库连接字符串！");
                return (false);
            }

            return Connect();
        }

        //断开数据库连接
        public void DisConnectDatabase()
        {
            HoldDbTransNoCommit = false;
            _InTrans = false;

            DisConnect();
        }

        //提交事务
        public bool CommitTransaction(string errMsg)
        {
            ClearLastError();

            lock (this)
            {
                HoldDbTransNoCommit = false;
                if (!_InTrans)
                    return true;

                _InTrans = false;

                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        try
                        {
                            mSqlTrans.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            mSqlTrans.Rollback();
                            ProcessError(errMsg + e.Message);
                            return false;
                        }
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        try
                        {
                            mOleTrans.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            mOleTrans.Rollback();
                            ProcessError(errMsg + e.Message);
                            return false;
                        }
                    case ConnectionTypeEnum.Oracle:
                        try
                        {
                            mOraTrans.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            mOraTrans.Rollback();
                            ProcessError(errMsg + e.Message);
                            return false;
                        }
                    case ConnectionTypeEnum.MySql:
                        return true;
                    case ConnectionTypeEnum.MySqlUseTran:
                        try
                        {
                            mySqlTrans.Commit();
                            return true;
                        }
                        catch (Exception e)
                        {
                            mySqlTrans.Rollback();
                            ProcessError(errMsg + e.Message);
                            return false;
                        }
                    default:
                        ProcessError(errMsg + "未知的连接类型！");
                        return false;
                }
            }
        }

        //回滚事务
        public void RollbackTransaction()
        {
            ClearLastError();

            lock (this)
            {
                HoldDbTransNoCommit = false;
                if (!_InTrans)
                    return;

                _InTrans = false;

                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        mSqlTrans.Rollback();
                        break;
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        mOleTrans.Rollback();
                        break;
                    case ConnectionTypeEnum.Oracle:
                        mOraTrans.Rollback();
                        break;
                    case ConnectionTypeEnum.MySql:
                        break;
                    case ConnectionTypeEnum.MySqlUseTran:
                        mySqlTrans.Rollback();
                        break;
                }
            }
        }

        //执行单SQL语句，返回影响的函数
        public int ExecuteNoQuery(string execSql, string errMsg)
        {
            lock (this)
            {
                ClearLastError();

                bool lbHoldDbTransNoCommit = HoldDbTransNoCommit;
                HoldDbTransNoCommit = false;
                bool lbExecWithoutTrans = ExecWithoutTrans;
                ExecWithoutTrans = false;

                //如果没有任何脚本可执行则返回0
                if (HTBaseFunc.NullToStr(execSql) == "")
                {
                    HoldDbConnectOneTimeForWebProcessMode = false;
                    if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                        RollbackTransaction();
                    return 0;
                }

                //连接数据库
                if (DataBaseConnected == false)
                {
                    if (ConnectDatabase() == false)
                    {
                        HoldDbConnectOneTimeForWebProcessMode = false;
                        return -1;
                    }
                }

                //执行脚本
                int iExecuteRowEffect = -1;
                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mSqlTrans = mSqlCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mSqlCmd.Transaction = mSqlTrans;
                            mSqlCmd.CommandText = execSql;
                            mSqlCmd.CommandType = CommandType.Text;
                            mSqlCmd.Parameters.Clear();
                            iExecuteRowEffect = mSqlCmd.ExecuteNonQuery();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Commit();
                            }
                            iExecuteRowEffect = 0;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOleTrans = mOleCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOleCmd.Transaction = mOleTrans;
                            mOleCmd.CommandText = execSql;
                            mOleCmd.CommandType = CommandType.Text;
                            mOleCmd.Parameters.Clear();
                            iExecuteRowEffect = mOleCmd.ExecuteNonQuery();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Commit();
                            }
                            iExecuteRowEffect = 0;
                        }
                        catch (OleDbException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.Oracle:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOraTrans = mOraCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOraCmd.Transaction = mOraTrans;
                            mOraCmd.CommandText = execSql;
                            mOraCmd.CommandType = CommandType.Text;
                            mOraCmd.Parameters.Clear();
                            iExecuteRowEffect = mOraCmd.ExecuteNonQuery();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Commit();
                            }
                            iExecuteRowEffect = 0;
                        }
                        catch (OracleException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.MySql:
                        MySqlCommand mySqlCmd1 = new MySqlCommand("", mySqlCn);
                        try
                        {
                            mySqlCmd1.CommandText = execSql;
                            mySqlCmd1.CommandType = CommandType.Text;
                            mySqlCmd1.Parameters.Clear();
                            iExecuteRowEffect = mySqlCmd1.ExecuteNonQuery();
                            iExecuteRowEffect = 0;
                        }
                        catch (MySqlException e)
                        {
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlCmd1 = null;
                            //if (mySqlCmd1 != null)
                            //    mySqlCmd1.Dispose();
                        }
                        break;
                    case ConnectionTypeEnum.MySqlUseTran:
                        MySqlCommand mySqlCmd2 = new MySqlCommand("", mySqlCn);
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mySqlTrans = mySqlCn.BeginTransaction();
                                _InTrans = true;
                            }

                            mySqlCmd2.Transaction = mySqlTrans;
                            mySqlCmd2.CommandText = execSql;
                            mySqlCmd2.CommandType = CommandType.Text;
                            mySqlCmd2.Parameters.Clear();
                            iExecuteRowEffect = mySqlCmd2.ExecuteNonQuery();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Commit();
                            }
                            iExecuteRowEffect = 0;
                        }
                        catch (MySqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlCmd2 = null;
                            //if (mySqlCmd2 != null)
                            //    mySqlCmd2.Dispose();
                        }
                        break;
                }

                //断开数据库连接
                if (_webProcessMode && (HoldDbConnectOneTimeForWebProcessMode == false))
                    DisConnectDatabase();
                HoldDbConnectOneTimeForWebProcessMode = false;

                return (iExecuteRowEffect);
            }
        }

        //执行单SQL语句，返回结果
        public object ExecuteScalar(string execSql, string errMsg)
        {
            lock (this)
            {
                ClearLastError();

                bool lbHoldDbTransNoCommit = HoldDbTransNoCommit;
                HoldDbTransNoCommit = false;
                bool lbExecWithoutTrans = ExecWithoutTrans;
                ExecWithoutTrans = false;

                //如果没有任何脚本可执行则返回null
                if (HTBaseFunc.NullToStr(execSql) == "")
                {
                    HoldDbConnectOneTimeForWebProcessMode = false;
                    if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                        RollbackTransaction();
                    return null;
                }

                //连接数据库
                if (DataBaseConnected == false)
                {
                    if (ConnectDatabase() == false)
                    {
                        HoldDbConnectOneTimeForWebProcessMode = false;
                        return null;
                    }
                }

                //执行脚本
                object oResult = null;
                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mSqlTrans = mSqlCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mSqlCmd.Transaction = mSqlTrans;
                            mSqlCmd.CommandText = execSql;
                            mSqlCmd.CommandType = CommandType.Text;
                            mSqlCmd.Parameters.Clear();
                            oResult = mSqlCmd.ExecuteScalar();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Commit();
                            }
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOleTrans = mOleCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOleCmd.Transaction = mOleTrans;
                            mOleCmd.CommandText = execSql;
                            mOleCmd.CommandType = CommandType.Text;
                            mOleCmd.Parameters.Clear();
                            oResult = mOleCmd.ExecuteScalar();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Commit();
                            }
                        }
                        catch (OleDbException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.Oracle:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOraTrans = mOraCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOraCmd.Transaction = mOraTrans;
                            mOraCmd.CommandText = execSql;
                            mOraCmd.CommandType = CommandType.Text;
                            mOraCmd.Parameters.Clear();
                            oResult = mOraCmd.ExecuteScalar();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Commit();
                            }
                        }
                        catch (OracleException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.MySql:
                        MySqlCommand mySqlCmd1 = new MySqlCommand("", mySqlCn);
                        try
                        {
                            mySqlCmd1.CommandText = execSql;
                            mySqlCmd1.CommandType = CommandType.Text;
                            mySqlCmd1.Parameters.Clear();
                            oResult = mySqlCmd1.ExecuteScalar();
                        }
                        catch (MySqlException e)
                        {
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlCmd1 = null;
                            //if (mySqlCmd1 != null)
                            //    mySqlCmd1.Dispose();
                        }
                        break;
                    case ConnectionTypeEnum.MySqlUseTran:
                        MySqlCommand mySqlCmd2 = new MySqlCommand("", mySqlCn);
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mySqlTrans = mySqlCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mySqlCmd2.Transaction = mySqlTrans;
                            mySqlCmd2.CommandText = execSql;
                            mySqlCmd2.CommandType = CommandType.Text;
                            mySqlCmd2.Parameters.Clear();
                            oResult = mySqlCmd2.ExecuteScalar();
                            if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Commit();
                            }
                        }
                        catch (MySqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlCmd2 = null;
                            //if (mySqlCmd2 != null)
                            //    mySqlCmd2.Dispose();
                        }
                        break;
                }

                //断开数据库连接
                if (_webProcessMode && (HoldDbConnectOneTimeForWebProcessMode == false))
                    DisConnectDatabase();
                HoldDbConnectOneTimeForWebProcessMode = false;

                return (oResult);
            }
        }

        //打开数据表
        public bool OpenDataTable(ref DataTable aDataTable, string execSql, string errMsg, bool bAutoClear)
        {
            lock (this)
            {
                ClearLastError();

                bool lbHoldDbTransNoCommit = HoldDbTransNoCommit;
                HoldDbTransNoCommit = false;
                bool lbExecWithoutTrans = ExecWithoutTrans;
                ExecWithoutTrans = false;

                //先清空
                if (aDataTable == null)
                {
                    aDataTable = new DataTable();
                }
                else
                {
                    if (bAutoClear) aDataTable.Clear();
                }

                //连接数据库
                if (DataBaseConnected == false)
                {
                    if (ConnectDatabase() == false)
                    {
                        HoldDbConnectOneTimeForWebProcessMode = false;
                        return false;
                    }
                }

                //执行脚本
                bool bResult = false;
                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mSqlTrans = mSqlCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mSqlDA.SelectCommand.Transaction = mSqlTrans;
                            mSqlDA.SelectCommand.CommandType = CommandType.Text;
                            mSqlDA.SelectCommand.CommandText = execSql;
                            mSqlDA.Fill(aDataTable);
                            if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOleTrans = mOleCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOleDA.SelectCommand.Transaction = mOleTrans;
                            mOleDA.SelectCommand.CommandType = CommandType.Text;
                            mOleDA.SelectCommand.CommandText = execSql;
                            mOleDA.Fill(aDataTable);
                            if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.Oracle:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOraTrans = mOraCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOraDA.SelectCommand.Transaction = mOraTrans;
                            mOraDA.SelectCommand.CommandType = CommandType.Text;
                            mOraDA.SelectCommand.CommandText = execSql;
                            mOraDA.Fill(aDataTable);
                            if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.MySql:
                        MySqlDataAdapter mySqlDA1 = new MySqlDataAdapter("", mySqlCn);
                        try
                        {
                            mySqlDA1.SelectCommand.CommandType = CommandType.Text;
                            mySqlDA1.SelectCommand.CommandText = execSql;
                            mySqlDA1.Fill(aDataTable);
                            bResult = true;
                        }
                        catch (MySqlException e)
                        {
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlDA1 = null;
                            //if (mySqlDA1 != null)
                            //    mySqlDA1.Dispose();
                        }
                        break;
                    case ConnectionTypeEnum.MySqlUseTran:
                        MySqlDataAdapter mySqlDA2 = new MySqlDataAdapter("", mySqlCn);
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = true;
                            }

                            mySqlDA2.SelectCommand.Transaction = mySqlTrans;
                            mySqlDA2.SelectCommand.CommandType = CommandType.Text;
                            mySqlDA2.SelectCommand.CommandText = execSql;
                            mySqlDA2.Fill(aDataTable);
                            if (lbHoldDbTransNoCommit == false && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (MySqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlDA2 = null;
                            //if (mySqlDA2 != null)
                            //    mySqlDA2.Dispose();
                        }
                        break;
                }

                //断开数据库连接
                if (_webProcessMode && (HoldDbConnectOneTimeForWebProcessMode == false))
                    DisConnectDatabase();
                HoldDbConnectOneTimeForWebProcessMode = false;

                return (bResult);
            }
        }

        //打开数据表，缺省总是清空原表
        public bool OpenDataTable(ref DataTable aDataTable, string execSql, string errMsg)
        {
            return OpenDataTable(ref aDataTable, execSql, errMsg, true);
        }

        //打开数据集
        public bool OpenDataSet(ref DataSet aDataSet, string execSql, string errMsg)
        {
            lock (this)
            {
                ClearLastError();

                bool lbHoldDbTransNoCommit = HoldDbTransNoCommit;
                HoldDbTransNoCommit = false;
                bool lbExecWithoutTrans = ExecWithoutTrans;
                ExecWithoutTrans = false;

                //连接数据库
                if (DataBaseConnected == false)
                {
                    if (ConnectDatabase() == false)
                    {
                        HoldDbConnectOneTimeForWebProcessMode = false;
                        return false;
                    }
                }

                //执行脚本
                bool bResult = false;
                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mSqlTrans = mSqlCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mSqlDA.SelectCommand.Transaction = mSqlTrans;
                            mSqlDA.SelectCommand.CommandType = CommandType.Text;
                            mSqlDA.SelectCommand.CommandText = execSql;
                            mSqlDA.Fill(aDataSet);
                            if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mSqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOleTrans = mOleCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOleDA.SelectCommand.Transaction = mOleTrans;
                            mOleDA.SelectCommand.CommandType = CommandType.Text;
                            mOleDA.SelectCommand.CommandText = execSql;
                            mOleDA.Fill(aDataSet);
                            if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOleTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.Oracle:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOraTrans = mOraCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOraDA.SelectCommand.Transaction = mOraTrans;
                            mOraDA.SelectCommand.CommandType = CommandType.Text;
                            mOraDA.SelectCommand.CommandText = execSql;
                            mOraDA.Fill(aDataSet);
                            if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mOraTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.MySql:
                        MySqlDataAdapter mySqlDA1 = new MySqlDataAdapter("", mySqlCn);
                        try
                        {
                            mySqlDA1.SelectCommand.CommandType = CommandType.Text;
                            mySqlDA1.SelectCommand.CommandText = execSql;
                            mySqlDA1.Fill(aDataSet);
                            bResult = true;
                        }
                        catch (MySqlException e)
                        {
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                            if (ConnectDatabase())
                            {
                                mySqlDA1 = new MySqlDataAdapter("", mySqlCn);
                                try
                                {
                                    mySqlDA1.SelectCommand.CommandType = CommandType.Text;
                                    mySqlDA1.SelectCommand.CommandText = execSql;
                                    mySqlDA1.Fill(aDataSet);
                                    bResult = true;
                                }
                                catch (MySqlException e1)
                                {
                                    ProcessError(errMsg + e1.Message + " " + e1.ErrorCode);
                                    mySqlCn.Close();
                                }
                                catch (Exception e2)
                                {
                                    ProcessError(errMsg + e2.Message);
                                    mySqlCn.Close();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlDA1 = null;
                            //if (mySqlDA1 != null)
                            //    mySqlDA1.Dispose();
                        }
                        break;
                    case ConnectionTypeEnum.MySqlUseTran:
                        MySqlDataAdapter mySqlDA2 = new MySqlDataAdapter("", mySqlCn);
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mySqlTrans = mySqlCn.BeginTransaction();
                                _InTrans = true;
                            }

                            mySqlDA2.SelectCommand.Transaction = mySqlTrans;
                            mySqlDA2.SelectCommand.CommandType = CommandType.Text;
                            mySqlDA2.SelectCommand.CommandText = execSql;
                            mySqlDA2.Fill(aDataSet);
                            if (lbHoldDbTransNoCommit == false && _InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Commit();
                            }
                            bResult = true;
                        }
                        catch (MySqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                mySqlTrans.Rollback();
                            }
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlDA2 = null;
                            //if (mySqlDA2 != null)
                            //    mySqlDA2.Dispose();
                        }
                        break;
                }

                //断开数据库连接
                if (_webProcessMode && (HoldDbConnectOneTimeForWebProcessMode == false))
                    DisConnectDatabase();
                HoldDbConnectOneTimeForWebProcessMode = false;

                return (bResult);
            }
        }

        //生成存储过程参数
        public StoredProcedureParam GenStoredProcedureParam(string paramName, string sqlType, object value, bool isOutput)
        {
            StoredProcedureParam spParam = new StoredProcedureParam();
            spParam.Name = paramName;
            spParam.Type = sqlType;
            spParam.Value = value;
            if (isOutput)
                spParam.Direction = ParameterDirection.InputOutput;
            else
                spParam.Direction = ParameterDirection.Input;
            return spParam;
        }

        //获取存储过程参数
        public static StoredProcedureParam GetStoredProcedureParam(ArrayList oParams, string paramName)
        {
            if (oParams == null)
                return null;

            foreach (StoredProcedureParam spParam in oParams)
            {
                if (spParam != null && spParam.Name.ToUpper() == paramName.ToUpper())
                {
                    return spParam;
                }
            }
            return null;
        }

        public int ExecStoredProcedure(string sStoredProcedureName, ref ArrayList oParams, string errMsg)
        {
            object oDataTableSet = null;
            return ExecStoredProcedure(sStoredProcedureName, ref oParams, ref oDataTableSet, 0, errMsg);
        }
        public int ExecStoredProcedure(string sStoredProcedureName, ref ArrayList oParams, ref DataTable oDataTable, string errMsg)
        {
            object oDataTableSet = oDataTable;
            return ExecStoredProcedure(sStoredProcedureName, ref oParams, ref oDataTableSet, 1, errMsg);
        }
        public int ExecStoredProcedure(string sStoredProcedureName, ref ArrayList oParams, ref DataSet oDataSet, string errMsg)
        {
            object oDataTableSet = oDataSet;
            return ExecStoredProcedure(sStoredProcedureName, ref oParams, ref oDataTableSet, 2, errMsg);
        }

        //执行存储过程,其中oParams中元素是StoredProcedureParam类型的
        //iRtnDataTableSet指示填充DataTable(1)、DataSet(2)还是不填充(0)，如果填充则aDataTableSet必须为对应对象
        private int ExecStoredProcedure(string sStoredProcedureName, ref ArrayList oParams, ref object aDataTableSet, int iRtnDataTableSet, string errMsg)
        {            
            lock (this)
            {
                ClearLastError();

                bool lbHoldDbTransNoCommit = HoldDbTransNoCommit;
                HoldDbTransNoCommit = false;
                bool lbExecWithoutTrans = ExecWithoutTrans;
                ExecWithoutTrans = false;

                //如果没有任何脚本可执行则返回-9999
                if (HTBaseFunc.NullToStr(sStoredProcedureName) == "")
                {
                    HoldDbConnectOneTimeForWebProcessMode = false;
                    if (!lbHoldDbTransNoCommit && _InTrans && !lbExecWithoutTrans)
                        RollbackTransaction();
                    ProcessError("未指定存储过程名！");
                    return (-9999);
                }

                //连接数据库
                if (DataBaseConnected == false)
                {
                    if (ConnectDatabase() == false)
                    {
                        HoldDbConnectOneTimeForWebProcessMode = false;
                        return -9999;
                    }
                }

                //执行脚本
                int iResult = -9999;
                string sParamIndex = "";
                switch (_conntype)
                {
                    case ConnectionTypeEnum.Sql:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mSqlTrans = mSqlCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mSqlCmd.Transaction = mSqlTrans;
                            mSqlCmd.CommandText = sStoredProcedureName;
                            mSqlCmd.CommandType = CommandType.StoredProcedure;
                            mSqlCmd.Parameters.Clear();

                            SqlParameter oSqlParam;
                            StoredProcedureParam spParam;
                            for (int i = 0; i < oParams.Count; i++)
                            {
                                spParam = (StoredProcedureParam)oParams[i];
                                oSqlParam = mSqlCmd.Parameters.Add(spParam.Name, HTBaseFunc.ConvertSqlDbType(spParam.Type));
                                oSqlParam.Value = spParam.Value;
                                oSqlParam.Direction = spParam.Direction;
                                sParamIndex = HTBaseFunc.SetParmStr(sParamIndex, spParam.Name, i.ToString());
                            }
                            oSqlParam = mSqlCmd.Parameters.Add("RETURN_VALUE", SqlDbType.Int);
                            oSqlParam.Direction = ParameterDirection.ReturnValue;

                            if (iRtnDataTableSet == 1)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataTable();
                                mSqlDA.SelectCommand = mSqlCmd;
                                mSqlDA.Fill((DataTable)aDataTableSet);
                            }
                            else if (iRtnDataTableSet == 2)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataSet();
                                mSqlDA.SelectCommand = mSqlCmd;
                                mSqlDA.Fill((DataSet)aDataTableSet);
                            }
                            else
                            {
                                mSqlCmd.ExecuteReader().Close();
                            }

                            iResult = HTBaseFunc.StringToInt32(HTBaseFunc.NullToStr(mSqlCmd.Parameters["RETURN_VALUE"].Value));
                            if (iResult == 0)
                            {
                                if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                                {
                                    _InTrans = false;
                                    mSqlTrans.Commit();
                                }

                                int j;
                                foreach (SqlParameter param in mSqlCmd.Parameters)
                                {
                                    if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                                    {
                                        j = HTBaseFunc.StringToInt32(HTBaseFunc.GetParmStr(sParamIndex, param.ParameterName));
                                        spParam = (StoredProcedureParam)oParams[j];
                                        spParam.Value = param.Value;
                                        oParams[j] = spParam;
                                    }
                                }
                            }
                            else if (!lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mSqlTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch (SqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mSqlTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);

                            mSqlCn.Close();
                            //mSqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mSqlTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);

                            mSqlCn.Close();
                            //mSqlCn.Dispose();
                            //GC.Collect();
                        }
                        break;
                    case ConnectionTypeEnum.SqlOleDB:
                    case ConnectionTypeEnum.SybaseOleDB:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOleTrans = mOleCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOleCmd.Transaction = mOleTrans;
                            mOleCmd.CommandText = sStoredProcedureName;
                            mOleCmd.CommandType = CommandType.StoredProcedure;
                            mOleCmd.Parameters.Clear();

                            OleDbParameter oOleParam;
                            StoredProcedureParam spParam;
                            for (int i = 0; i < oParams.Count; i++)
                            {
                                spParam = (StoredProcedureParam)oParams[i];
                                oOleParam = mOleCmd.Parameters.Add(spParam.Name, HTBaseFunc.ConvertOleDbType(spParam.Type));
                                oOleParam.Value = spParam.Value;
                                oOleParam.Direction = spParam.Direction;
                                sParamIndex = HTBaseFunc.SetParmStr(sParamIndex, spParam.Name, i.ToString());
                            }
                            oOleParam = mOleCmd.Parameters.Add("RETURN_VALUE", OleDbType.Integer);
                            oOleParam.Direction = ParameterDirection.ReturnValue;

                            if (iRtnDataTableSet == 1)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataTable();
                                mOleDA.SelectCommand = mOleCmd;
                                mOleDA.Fill((DataTable)aDataTableSet);
                            }
                            else if (iRtnDataTableSet == 2)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataSet();
                                mOleDA.SelectCommand = mOleCmd;
                                mOleDA.Fill((DataSet)aDataTableSet);
                            }
                            else
                            {
                                mOleCmd.ExecuteReader().Close();
                            }

                            iResult = HTBaseFunc.StringToInt32(HTBaseFunc.NullToStr(mOleCmd.Parameters["RETURN_VALUE"].Value));
                            if (iResult == 0)
                            {
                                if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                                {
                                    _InTrans = false;
                                    mOleTrans.Commit();
                                }

                                int j;
                                foreach (OleDbParameter param in mOleCmd.Parameters)
                                {
                                    if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                                    {
                                        j = HTBaseFunc.StringToInt32(HTBaseFunc.GetParmStr(sParamIndex, param.ParameterName));
                                        spParam = (StoredProcedureParam)oParams[j];
                                        spParam.Value = param.Value;
                                        oParams[j] = spParam;
                                    }
                                }
                            }
                            else if (!lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mOleTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch (OleDbException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mOleTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mOleTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.Oracle:
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mOraTrans = mOraCn.BeginTransaction();
                                _InTrans = true;
                            }
                            mOraCmd.Transaction = mOraTrans;
                            mOraCmd.CommandText = sStoredProcedureName;
                            mOraCmd.CommandType = CommandType.StoredProcedure;
                            mOraCmd.Parameters.Clear();

                            OracleParameter oOraParam;
                            StoredProcedureParam spParam;
                            for (int i = 0; i < oParams.Count; i++)
                            {
                                spParam = (StoredProcedureParam)oParams[i];
                                oOraParam = mOraCmd.Parameters.Add(spParam.Name, spParam.Value);
                                oOraParam.Direction = spParam.Direction;
                                sParamIndex = HTBaseFunc.SetParmStr(sParamIndex, spParam.Name, i.ToString());
                            }
                            oOraParam = mOraCmd.Parameters.Add("RETURN_VALUE", OracleType.Int32);
                            oOraParam.Direction = ParameterDirection.ReturnValue;

                            if (iRtnDataTableSet == 1)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataTable();
                                mOraDA.SelectCommand = mOraCmd;
                                mOraDA.Fill((DataTable)aDataTableSet);
                            }
                            else if (iRtnDataTableSet == 2)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataSet();
                                mOraDA.SelectCommand = mOraCmd;
                                mOraDA.Fill((DataSet)aDataTableSet);
                            }
                            else
                            {
                                mOraCmd.ExecuteReader().Close();
                            }

                            iResult = HTBaseFunc.StringToInt32(HTBaseFunc.NullToStr(mOraCmd.Parameters["RETURN_VALUE"].Value));
                            if (iResult == 0)
                            {
                                if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                                {
                                    _InTrans = false;
                                    mOraTrans.Commit();
                                }

                                int j;
                                foreach (OracleParameter param in mOraCmd.Parameters)
                                {
                                    if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                                    {
                                        j = HTBaseFunc.StringToInt32(HTBaseFunc.GetParmStr(sParamIndex, param.ParameterName));
                                        spParam = (StoredProcedureParam)oParams[j];
                                        spParam.Value = param.Value;
                                        oParams[j] = spParam;
                                    }
                                }
                            }
                            else if (!lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mOraTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch (OracleException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mOraTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mOraTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);
                        }
                        break;
                    case ConnectionTypeEnum.MySql:
                        MySqlCommand mySqlCmd1 = new MySqlCommand("", mySqlCn);
                        MySqlDataAdapter mySqlDA1 = new MySqlDataAdapter();
                        try
                        {
                            mySqlCmd1.CommandText = sStoredProcedureName;
                            mySqlCmd1.CommandType = CommandType.StoredProcedure;
                            mySqlCmd1.Parameters.Clear();

                            MySqlParameter myParam;

                            StoredProcedureParam spParam;
                            for (int i = 0; i < oParams.Count; i++)
                            {
                                spParam = (StoredProcedureParam)oParams[i];
                                myParam = mySqlCmd1.Parameters.Add(spParam.Name, spParam.Value);
                                myParam.Direction = spParam.Direction;
                                sParamIndex = HTBaseFunc.SetParmStr(sParamIndex, spParam.Name, i.ToString());
                            }
                            myParam = mySqlCmd1.Parameters.Add("RETURN_VALUE", 0);
                            myParam.Direction = ParameterDirection.Output;

                            if (iRtnDataTableSet == 1)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataTable();

                                mySqlDA1.SelectCommand = mySqlCmd1;
                                mySqlDA1.Fill((DataTable)aDataTableSet);
                            }
                            else if (iRtnDataTableSet == 2)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataSet();
                                mySqlDA1.SelectCommand = mySqlCmd1;
                                mySqlDA1.Fill((DataSet)aDataTableSet);
                            }
                            else
                            {
                                MySqlDataReader mySqlDR = mySqlCmd1.ExecuteReader();
                                if (mySqlDR != null)
                                {
                                    mySqlDR.Close();
                                    mySqlDR = null;
                                    //mySqlDR.Dispose();
                                }
                            }

                            iResult = HTBaseFunc.StringToInt32(HTBaseFunc.NullToStr(mySqlCmd1.Parameters["RETURN_VALUE"].Value));

                            if (iResult == 0)
                            {
                                int j;
                                foreach (MySqlParameter param in mySqlCmd1.Parameters)
                                {
                                    if (param.ParameterName != "RETURN_VALUE")
                                    {
                                        if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                                        {
                                            j = HTBaseFunc.StringToInt32(HTBaseFunc.GetParmStr(sParamIndex, param.ParameterName));
                                            spParam = (StoredProcedureParam)oParams[j];
                                            spParam.Value = param.Value;
                                            oParams[j] = spParam;
                                        }
                                    }
                                }
                            }
                        }
                        catch (MySqlException e)
                        {
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlCmd1 = null;
                            //if (mySqlCmd1 != null)
                            //    mySqlCmd1.Dispose();
                            mySqlDA1 = null;
                            //if (mySqlDA1 != null)
                            //    mySqlDA1.Dispose();
                        }
                        break;
                    case ConnectionTypeEnum.MySqlUseTran:
                        MySqlCommand mySqlCmd2 = new MySqlCommand("", mySqlCn);
                        MySqlDataAdapter mySqlDA2 = new MySqlDataAdapter("", mySqlCn);
                        try
                        {
                            if (!_InTrans && !lbExecWithoutTrans)
                            {
                                mySqlTrans = mySqlCn.BeginTransaction();
                                _InTrans = true;
                            }

                            mySqlCmd2.Transaction = mySqlTrans;
                            mySqlCmd2.CommandText = sStoredProcedureName;
                            mySqlCmd2.CommandType = CommandType.StoredProcedure;
                            mySqlCmd2.Parameters.Clear();

                            MySqlParameter myParam;

                            StoredProcedureParam spParam;
                            for (int i = 0; i < oParams.Count; i++)
                            {
                                spParam = (StoredProcedureParam)oParams[i];
                                myParam = mySqlCmd2.Parameters.Add(spParam.Name, spParam.Value);
                                myParam.Direction = spParam.Direction;
                                sParamIndex = HTBaseFunc.SetParmStr(sParamIndex, spParam.Name, i.ToString());
                            }
                            myParam = mySqlCmd2.Parameters.Add("RETURN_VALUE", 0);
                            myParam.Direction = ParameterDirection.Output;

                            if (iRtnDataTableSet == 1)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataTable();

                                mySqlDA2.SelectCommand = mySqlCmd2;
                                mySqlDA2.Fill((DataTable)aDataTableSet);
                            }
                            else if (iRtnDataTableSet == 2)
                            {
                                if (aDataTableSet == null)
                                    aDataTableSet = new DataSet();
                                mySqlDA2.SelectCommand = mySqlCmd2;
                                mySqlDA2.Fill((DataSet)aDataTableSet);
                            }
                            else
                            {
                                MySqlDataReader mySqlDR = mySqlCmd2.ExecuteReader();
                                if (mySqlDR != null)
                                {
                                    mySqlDR.Close();
                                    mySqlDR = null;
                                    //mySqlDR.Dispose();
                                }
                            }

                            iResult = HTBaseFunc.StringToInt32(HTBaseFunc.NullToStr(mySqlCmd2.Parameters["RETURN_VALUE"].Value));

                            if (iResult == 0)
                            {
                                if (!lbHoldDbTransNoCommit && !lbExecWithoutTrans)
                                {
                                    _InTrans = false;
                                    mySqlTrans.Commit();
                                }

                                int j;
                                foreach (MySqlParameter param in mySqlCmd2.Parameters)
                                {
                                    if (param.ParameterName != "RETURN_VALUE")
                                    {
                                        if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                                        {
                                            j = HTBaseFunc.StringToInt32(HTBaseFunc.GetParmStr(sParamIndex, param.ParameterName));
                                            spParam = (StoredProcedureParam)oParams[j];
                                            spParam.Value = param.Value;
                                            oParams[j] = spParam;
                                        }
                                    }
                                }
                            }
                            else if (!lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mySqlTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch (MySqlException e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mySqlTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message + " " + e.ErrorCode);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        catch (Exception e)
                        {
                            if (_InTrans && !lbExecWithoutTrans)
                            {
                                _InTrans = false;
                                try
                                {
                                    mySqlTrans.Rollback();
                                }
                                catch
                                {
                                }
                            }
                            ProcessError(errMsg + e.Message);
                            mySqlCn.Close();
                            //mySqlCn.Dispose();
                            //GC.Collect();
                        }
                        finally
                        {
                            mySqlCmd2 = null;
                            //if (mySqlCmd2 != null)
                            //    mySqlCmd2.Dispose();
                            mySqlDA2 = null;
                            //if (mySqlDA2 != null)
                            //    mySqlDA2.Dispose();
                        }
                        break;
                }

                //断开数据库连接
                if (_webProcessMode && (HoldDbConnectOneTimeForWebProcessMode == false))
                    DisConnectDatabase();
                HoldDbConnectOneTimeForWebProcessMode = false;

                return (iResult);
            }
        }

        //初始化SQL批处理集合
        public void InitSQLCmd()
        {
            _SqlCmdList.Clear();
        }

        // 在SQL批处理集合中添加SQL语句
        public void AddSQLCmd(string singleSql)
        {
            _SqlCmdList.Add(singleSql);
        }

        /// <summary>
        ///  执行SQL批命令，返回是否执行成功。错误可以自己写进@_ErrMsg中,并GOTO ERR。
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool ExecSQLCmd(string errMsg)
        {
            string execSql = ViewSQLCmd();

            return (ExecuteNoQuery(execSql, errMsg) >= 0);
        }

        // 查看SQL批命令，对于不同数据库生成的脚本不一样
        public string ViewSQLCmd()
        {
            return ViewSQLCmd(_SqlCmdList);
        }

        private string ViewSQLCmd(ArrayList sqlList)
        {
            string execSql = "";
            int i;

            if (sqlList.Count == 0) return ("");

            if (sqlList.Count == 1) return (sqlList[0].ToString());

            execSql = "DECLARE @_ErrMsg varchar(255) \n";
            execSql = execSql + "SELECT @_ErrMsg='' \n";
            //			execSql = execSql + "BEGIN TRAN\n";
            for (i = 0; i < sqlList.Count; i++)
            {
                execSql = execSql + sqlList[i].ToString() + " \n";
                execSql = execSql + "IF @@ERROR <> 0 GOTO ERR \n";
            }
            //			execSql = execSql + "COMMIT TRAN\n";
            execSql = execSql + "GOTO QUIT\n";
            //			execSql = execSql + "ERR:ROLLBACK TRAN\n";
            execSql = execSql + "ERR:\n";
            if (_conntype == ConnectionTypeEnum.Sql || _conntype == ConnectionTypeEnum.SqlOleDB)
            {
                execSql = execSql + "RAISERROR(@_ErrMsg, 16, 1) \n";
            }
            else if (_conntype == ConnectionTypeEnum.SybaseOleDB)
            {
                execSql = execSql + "RAISERROR 13000, @_ErrMsg \n";
            }
            else
            {
            }
            execSql = execSql + "QUIT:\n";

            return (execSql);
        }

        #endregion

        #region 脚本生成

        /// <summary>
        /// 根据数据表中的修改情况自动生成更新脚本
        /// </summary>
        /// <param name="sTableName">要更新的数据表名</param>
        /// <param name="oDT">发生修改的DataTable</param>
        /// <param name="oUpdateColList">可更新的字段列表，如果为null表示所有字段可更新</param>
        /// <returns>返回更新脚本</returns>
        public string GenDataTableProcessSql(string sTableName, DataTable oDT, ArrayList oUpdateColList)
        {
            ArrayList sqlList = new ArrayList();
            string sSql, sColName;
            object oValue;
            DataTable dt;
            DataView dv;

            //转换小写
            if (oUpdateColList != null)
            {
                for (int i = 0; i < oUpdateColList.Count; i++)
                {
                    oUpdateColList[i] = ((string)oUpdateColList[i]).ToLower();
                }
            }

            //生成条件模板
            string sWhere = "";
            if (oDT.PrimaryKey.Length > 0)
            {
                for (int i = 0; i < oDT.PrimaryKey.Length; i++)
                {
                    sColName = oDT.PrimaryKey[i].ColumnName;
                    if (oUpdateColList == null || oUpdateColList.IndexOf(sColName.ToLower()) > 0)
                    {
                        sWhere += " and " + sTableName + "." + sColName + "{#" + sColName + "#}";
                    }
                }
            }
            else
            {
                for (int i = 0; i < oDT.Columns.Count; i++)
                {
                    sColName = oDT.Columns[i].ColumnName;
                    if (oUpdateColList == null || oUpdateColList.IndexOf(sColName.ToLower()) > 0)
                    {
                        sWhere += " and " + sTableName + "." + sColName + "{#" + sColName + "#}";
                    }
                }
            }

            //生成删除操作的脚本
            string sWhereListSql;
            dt = oDT.GetChanges(DataRowState.Deleted);
            if (dt != null)
            {
                dv = new DataView(dt, "", dt.DefaultView.Sort, DataViewRowState.OriginalRows);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sWhereListSql = sWhere;

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (dt.Columns[j].AutoIncrement == false)
                        {
                            sColName = dt.Columns[j].ColumnName;
                            if (oUpdateColList == null || oUpdateColList.IndexOf(sColName.ToLower()) > 0)
                            {
                                oValue = dv[i][j];
                                sWhereListSql = HTBaseFunc.ReplaceStrAll(sWhereListSql, "{#" + sColName + "#}", ((oValue == DBNull.Value || oValue == null) ? " is null" : "=" + HTBaseFunc.GetSqledValue(dt.Columns[j].DataType, oValue)));
                            }
                        }
                    }

                    if (sWhereListSql != "")
                    {
                        sSql = "delete from [" + sTableName + "] where 1=1" + sWhereListSql;
                        sqlList.Add(sSql);
                    }
                }
            }

            //生成更新操作的脚本
            string sUpdateListSql;
            dt = oDT.GetChanges(DataRowState.Modified);
            if (dt != null)
            {
                dv = new DataView(dt, "", dt.DefaultView.Sort, DataViewRowState.OriginalRows);
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sWhereListSql = sWhere;
                    sUpdateListSql = "";

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (dt.Columns[j].AutoIncrement == false)
                        {
                            sColName = dt.Columns[j].ColumnName;
                            if (oUpdateColList == null || oUpdateColList.IndexOf(sColName.ToLower()) > 0)
                            {
                                if (HTBaseFunc.NullToStr(dt.Rows[i][j]) != HTBaseFunc.NullToStr(dv[i][j]))
                                {
                                    oValue = dt.Rows[i][j];
                                    sUpdateListSql += ", " + sColName;
                                    if (oValue == DBNull.Value || oValue == null)
                                        sUpdateListSql += "=null";
                                    else
                                        sUpdateListSql += "=" + HTBaseFunc.GetSqledValue(dt.Columns[j].DataType, oValue);
                                }

                                oValue = dv[i][j];
                                sWhereListSql = HTBaseFunc.ReplaceStrAll(sWhereListSql, "{#" + sColName + "#}", ((oValue == DBNull.Value || oValue == null) ? " is null" : "=" + HTBaseFunc.GetSqledValue(dt.Columns[j].DataType, oValue)));
                            }
                        }
                    }

                    if (sUpdateListSql != "" && sWhereListSql != "")
                    {
                        sUpdateListSql = HTBaseFunc.MidStr(sUpdateListSql, 1);
                        sSql = "update [" + sTableName + "] set " + sUpdateListSql + " where 1=1" + sWhereListSql;
                        sqlList.Add(sSql);
                    }
                }
            }

            //生成插入新记录的脚本
            string sColListSql, sValListSql;
            dt = oDT.GetChanges(DataRowState.Added);
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sColListSql = "";
                    sValListSql = "";
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (dt.Columns[j].AutoIncrement == false)
                        {
                            sColName = dt.Columns[j].ColumnName;
                            if (oUpdateColList == null || oUpdateColList.IndexOf(sColName.ToLower()) > 0)
                            {
                                oValue = dt.Rows[i][j];
                                sColListSql += ", " + sColName;
                                if (oValue == DBNull.Value || oValue == null)
                                    sValListSql += ", null";
                                else
                                    sValListSql += ", " + HTBaseFunc.GetSqledValue(dt.Columns[j].DataType, oValue);
                            }
                        }
                    }

                    if (sColListSql != "" && sValListSql != "")
                    {
                        sColListSql = HTBaseFunc.MidStr(sColListSql, 1);
                        sValListSql = HTBaseFunc.MidStr(sValListSql, 1);
                        sSql = "insert into [" + sTableName + "](" + sColListSql + ") values(" + sValListSql + ")";
                        sqlList.Add(sSql);
                    }
                }
            }

            return ViewSQLCmd(sqlList);
        }

        public string GenDataTableProcessSql(string sTableName, DataTable oDT)
        {
            return GenDataTableProcessSql(sTableName, oDT, null);
        }

        /// <summary>
        /// 根据数据表中的修改情况自动生成更新脚本
        /// 要更新的表名和可更新的字段都从DATATABLE的
        /// </summary>
        /// <param name="oDT"></param>
        /// <returns></returns>
        public string GenDataTableProcessSqlByColumnPrefix(string sTableName, DataTable oDT)
        {
            sTableName = sTableName.ToLower().Trim();

            //获取要更新的字段列表
            bool bNotSetPrefix = true;
            string sPrefix;
            ArrayList oColList = new ArrayList();
            for (int i = 0; i < oDT.Columns.Count; i++)
            {
                sPrefix = oDT.Columns[i].Prefix.ToLower().Trim();
                if (sPrefix == "")
                {
                    continue;
                }
                else
                {
                    bNotSetPrefix = false;

                    if (sPrefix == sTableName)
                    {
                        oColList.Add(oDT.Columns[i].ColumnName);
                    }
                }
            }

            //获取更新脚本
            if (bNotSetPrefix)
                return GenDataTableProcessSql(sTableName, oDT);
            else if (oColList.Count > 0)
                return GenDataTableProcessSql(sTableName, oDT, oColList);
            else
            {
                _lastError = "DataTable上没有通过Prefix指定可更新的列！";
                return "";
            }
        }

        #endregion

    }

    /// <summary>
    /// 事件日志记录
    /// </summary>
    public class HTEventLog
    {
        #region 公用变量

        public enum LogTypeEnum
        {
            信息 = 0,
            警告 = 1,
            错误 = 2,
            成功审核 = 3,
            失败审核 = 4
        }

        #endregion

        #region 私有变量

        private static string _lasterror = "";

        private static bool _windowsLogMode = false;
        private static bool _fileLogMode = false;
        private static bool _xmlLogMode = false;
        private static bool _databaseLogMode = false;


        //Windows日志
        private static string _serverName = ".";
        private static string _kind = "应用程序";
        private static string _source = Application.ProductName;

        //文件日志
        private static string _logFileName = Application.StartupPath + "\\" + Application.ProductName + ".Log";

        //XML日志
        private static string _logXmlFileName = Application.StartupPath + "\\" + Application.ProductName + "_Log.Xml";

        //数据库日志
        private static HTDBBaseFunc _htDBF = null;
        private static string _logTable = "EventLog";

        #endregion

        #region 错误处理

        public static string GetLastError()
        {
            return _lasterror;
        }

        #endregion

        #region 日志初始化

        public static bool InitWindowsLog(string serverName, string kind, string source)
        {
            if (HTBaseFunc.NullToStr(serverName) != "") _serverName = serverName;
            if (HTBaseFunc.NullToStr(kind) != "") _kind = kind;
            if (HTBaseFunc.NullToStr(source) != "") _source = source;

            _windowsLogMode = true;

            return true;
        }

        public static bool InitFileLog(string sLogFileName)
        {
            if (HTBaseFunc.NullToStr(sLogFileName) != "") _logFileName = sLogFileName;

            _fileLogMode = true;

            return true;
        }

        public static bool InitXmlLog(string sXmlFileName)
        {
            if (HTBaseFunc.NullToStr(_logXmlFileName) != "") _logXmlFileName = sXmlFileName;

            _xmlLogMode = true;

            return true;
        }

        public static bool InitDatabaseLog(HTDBBaseFunc.ConnectionTypeEnum connType, string dbServerName, string dbName, string userName, string userPassword, bool integratedSecurity, string logTable)
        {
            _htDBF = new HTDBBaseFunc();
            if (_htDBF.ConnectDatabase(connType, dbServerName, "", dbName, userName, userPassword, integratedSecurity))
            {
                if (HTBaseFunc.NullToStr(logTable) != "") _logTable = logTable;

                string sSql
                    = "if not exists (select 1 from sysobjects where type='U' and name='" + _logTable + "')"
                    + "  begin"
                    + "    create table " + _logTable + "("
                    + "      id int identity(1,1) not null,"
                    + "      type nvarchar(10) not null,"
                    + "      context nvarchar(1000) not null,"
                    + "      logtime datetime not null,"
                    + "      source nvarchar(30) not null,"
                    + "      computer nvarchar(30) null"
                    + "    )"
                    + "  end";
                if (_htDBF.ExecuteNoQuery(sSql, "初始化数据库日志出错！") >= 0)
                {
                    _databaseLogMode = true;

                    return true;
                }
                else
                {
                    _lasterror = _htDBF.GetLastError();
                    return false;
                }
            }
            else
            {
                _lasterror = _htDBF.GetLastError();
                return false;
            }
        }

        public static void DestroyLog()
        {
            _windowsLogMode = false;
            _fileLogMode = false;
            _xmlLogMode = false;
            _databaseLogMode = false;

            if (_htDBF != null)
            {
                if (_htDBF.DataBaseConnected) _htDBF.DisConnectDatabase();
            }
        }

        #endregion

        #region 记录日志

        public static string SaveLog(LogTypeEnum logType, string logText, string source)
        {
            string sErrList = "";

            if (_windowsLogMode)
                if (SaveWindowsLog(_kind, logType, logText, source, _serverName) == false) sErrList += "|WindowsLog";
            if (_fileLogMode)
                if (SaveFileLog(logType, logText, source, Environment.MachineName) == false) sErrList += "|FileLog";
            if (_xmlLogMode)
                if (SaveXmlLog(logType, logText, source, Environment.MachineName) == false) sErrList += "|XmlLog";
            if (_databaseLogMode)
                if (SaveDatabaseLog(logType, logText, source, Environment.MachineName) == false) sErrList += "|DatabaseLog";

            if (_windowsLogMode || _fileLogMode || _xmlLogMode || _databaseLogMode)
            {
                if (sErrList != "")
                {
                    sErrList = HTBaseFunc.MidStr(sErrList, 1);
                    return sErrList;
                }
                else
                    return "";
            }
            else
            {
                _lasterror = "不存在任何日志记录方式！";
                return "Error";
            }

        }

        #endregion

        #region Windows日志

        private static bool SaveWindowsLog(string kind, LogTypeEnum logType, string logText, string source, string serverName)
        {
            try
            {
                EventLog elCustom = new EventLog();

                EventLogEntryType elType = EventLogEntryType.Information;
                int iEventID = 0;

                switch (logType)
                {
                    case LogTypeEnum.信息:
                        elType = EventLogEntryType.Information;
                        iEventID = 0;
                        break;

                    case LogTypeEnum.警告:
                        elType = EventLogEntryType.Warning;
                        iEventID = 1000;
                        break;

                    case LogTypeEnum.错误:
                        elType = EventLogEntryType.Error;
                        iEventID = 9999;
                        break;

                    case LogTypeEnum.成功审核:
                        elType = EventLogEntryType.SuccessAudit;
                        iEventID = 5000;
                        break;

                    case LogTypeEnum.失败审核:
                        elType = EventLogEntryType.FailureAudit;
                        iEventID = 5999;
                        break;
                }

                if (HTBaseFunc.NullToStr(logText) == "")
                {
                    _lasterror = "日志内容不能为空！";
                    return false;
                }

                if (HTBaseFunc.NullToStr(serverName) != "")
                    elCustom.MachineName = serverName;
                else
                    elCustom.MachineName = ".";

                if (HTBaseFunc.NullToStr(source) != "")
                    elCustom.Source = source;
                else if (HTBaseFunc.NullToStr(_source) != "")
                    elCustom.Source = _source;
                else
                    elCustom.Source = Application.ProductName;

                if (HTBaseFunc.NullToStr(kind) == "应用程序" || HTBaseFunc.NullToStr(kind) == "") kind = "AppEvent";

                if (!EventLog.SourceExists(source, serverName)) EventLog.CreateEventSource(source, kind, serverName);
                elCustom.Log = kind;

                elCustom.WriteEntry(logText, elType, iEventID);
                elCustom.Close();
                return true;
            }
            catch (Exception ex)
            {
                _lasterror = "记录日志出错！" + ex.Message;
                return false;
            }
        }

        #endregion

        #region 文件日志

        public static bool SaveFileLog(LogTypeEnum logType, string logText, string source, string machineName)
        {
            try
            {
                string sLogType = "信息";
                switch (logType)
                {
                    case LogTypeEnum.信息:
                        sLogType = "信息";
                        break;
                    case LogTypeEnum.警告:
                        sLogType = "警告";
                        break;
                    case LogTypeEnum.错误:
                        sLogType = "错误";
                        break;
                    case LogTypeEnum.成功审核:
                        sLogType = "成功审核";
                        break;
                    case LogTypeEnum.失败审核:
                        sLogType = "失败审核";
                        break;
                }

                if (HTBaseFunc.NullToStr(logText) == "")
                {
                    _lasterror = "日志内容不能为空！";
                    return false;
                }
                if (HTBaseFunc.NullToStr(source) == "") source = Application.ProductName;

                string sLogStr
                    = "\r\n【类型】" + sLogType
                    + "\r\n【时间】" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + "\r\n【来源】 " + source
                    + "\r\n【机器】 " + HTBaseFunc.NullToStr(machineName)
                    + "\r\n【描述】" + logText
                    + "\r\n****************************************************************************************\r\n";

                StreamWriter sw = File.AppendText(_logFileName);
                sw.Write(sLogStr);
                sw.Close();

                return true;
            }
            catch (Exception ex)
            {
                _lasterror = "记录日志出错！" + ex.Message;
                return false;
            }
        }

        #endregion

        #region XML日志

        public static bool SaveXmlLog(LogTypeEnum logType, string logText, string source, string machineName)
        {
            try
            {
                string sLogType = "信息";
                switch (logType)
                {
                    case LogTypeEnum.信息:
                        sLogType = "信息";
                        break;
                    case LogTypeEnum.警告:
                        sLogType = "警告";
                        break;
                    case LogTypeEnum.错误:
                        sLogType = "错误";
                        break;
                    case LogTypeEnum.成功审核:
                        sLogType = "成功审核";
                        break;
                    case LogTypeEnum.失败审核:
                        sLogType = "失败审核";
                        break;
                }

                if (HTBaseFunc.NullToStr(logText) == "")
                {
                    _lasterror = "日志内容不能为空！";
                    return false;
                }
                if (HTBaseFunc.NullToStr(source) == "") source = Application.ProductName;

                HTXmlBaseFunc htXBF;
                htXBF = new HTXmlBaseFunc(_logXmlFileName, !(File.Exists(_logXmlFileName)), "EventLog");
                XmlDocument xmlDoc = htXBF.GetXmlDocument();
                XmlNode rootNode = htXBF.GetRootNode();

                XmlNode parentNode;
                parentNode = xmlDoc.CreateNode(XmlNodeType.Element, "LogRecord", "");
                rootNode.AppendChild(parentNode);

                XmlNode xmlNode;
                xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "类型", "");
                xmlNode.InnerText = sLogType;
                parentNode.AppendChild(xmlNode);

                xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "时间", "");
                xmlNode.InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                parentNode.AppendChild(xmlNode);

                xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "来源", "");
                xmlNode.InnerText = source;
                parentNode.AppendChild(xmlNode);

                xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "机器", "");
                xmlNode.InnerText = HTBaseFunc.NullToStr(machineName);
                parentNode.AppendChild(xmlNode);

                xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "描述", "");
                xmlNode.InnerText = logText;
                parentNode.AppendChild(xmlNode);

                if (htXBF.Save())
                    return true;
                else
                {
                    _lasterror = htXBF.GetLastError();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _lasterror = "记录日志出错！" + ex.Message;
                return false;
            }
        }

        #endregion

        #region 数据库日志

        public static bool SaveDatabaseLog(LogTypeEnum logType, string logText, string source, string machineName)
        {
            try
            {
                string sLogType = "信息";
                switch (logType)
                {
                    case LogTypeEnum.信息:
                        sLogType = "信息";
                        break;
                    case LogTypeEnum.警告:
                        sLogType = "警告";
                        break;
                    case LogTypeEnum.错误:
                        sLogType = "错误";
                        break;
                    case LogTypeEnum.成功审核:
                        sLogType = "成功审核";
                        break;
                    case LogTypeEnum.失败审核:
                        sLogType = "失败审核";
                        break;
                }

                if (HTBaseFunc.NullToStr(logText) == "")
                {
                    _lasterror = "日志内容不能为空！";
                    return false;
                }
                if (HTBaseFunc.NullToStr(source) == "") source = Application.ProductName;

                string sSql = "insert into " + _logTable + "(type, context, logtime, source, computer) values("
                    + HTBaseFunc.Expr(sLogType)
                    + ", " + HTBaseFunc.Expr(logText)
                    + ", getdate()"
                    + ", " + HTBaseFunc.Expr(source)
                    + ", " + HTBaseFunc.Expr(HTBaseFunc.NullToStr(machineName))
                    + ")";
                if (_htDBF.ExecuteNoQuery(sSql, "保存日志到数据库出错！") >= 0)
                    return true;
                else
                {
                    _lasterror = _htDBF.GetLastError();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _lasterror = "记录日志出错！" + ex.Message;
                return false;
            }
        }

        #endregion

    }

    /// <summary>
    /// XML文件处理
    /// </summary>
    public class HTXmlBaseFunc
    {
        #region 公有变量

        public bool XMLFileOpened = false;

        #endregion

        #region 私有变量

        private string _xmlFile = "";
        private XmlDocument _xdoc = null;
        private XmlNode _root = null;

        private string _lasterror = "";

        #endregion

        #region 私有函数

        //打开Xml文件
        private string OpenXmlFile(string sXmlFile)
        {
            if (HTBaseFunc.NullToStr(sXmlFile).Trim() == "") return "指定要打开的XML文件无效！";

            _xdoc = new XmlDocument();
            _root = null;
            try
            {
                _xdoc.Load(sXmlFile);
                _root = _xdoc.DocumentElement;
                return "";
            }
            catch (Exception ex)
            {
                return ("XML文件打开失败！" + ex.Message);
            }
        }

        //新建XML文件
        private string NewXmlFile(string rootName)
        {
            if (HTBaseFunc.NullToStr(rootName).Trim() == "") return "指定要新建的XML根结点无效！";

            try
            {
                _xdoc = new XmlDocument();
                _xdoc.LoadXml("<" + rootName + "></" + rootName + ">");
                _root = _xdoc.DocumentElement;
                return "";
            }
            catch (Exception ex)
            {
                return ("XML文件新建失败！" + ex.Message);
            }
        }

        //保存XML文件
        private string SaveXmlFile(string sXmlFile)
        {
            if (HTBaseFunc.NullToStr(sXmlFile).Trim() == "") return "指定要保存的XML文件无效！";

            try
            {
                _xdoc.Save(sXmlFile);
                return "";
            }
            catch (Exception ex)
            {
                return "保存XML文件出错！" + ex.Message;
            }
        }

        #endregion

        #region 基本操作

        public string GetLastError()
        {
            return _lasterror;
        }

        public HTXmlBaseFunc(string sXmlFile, bool bToNewXmlFile, string sRootName)
        {
            string sErr;
            if (bToNewXmlFile)
                sErr = NewXmlFile(sRootName);
            else
                sErr = OpenXmlFile(sXmlFile);

            if (sErr != "")
            {
                _lasterror = sErr;
                throw new Exception(sErr);
            }
            else
            {
                XMLFileOpened = true;
                _xmlFile = sXmlFile;
            }
        }

        public HTXmlBaseFunc(string sXmlData)
        {
            string sErr = "";
            try
            {
                _xdoc = new XmlDocument();
                sXmlData = sXmlData.Trim();
                _xdoc.LoadXml(sXmlData);
                _root = _xdoc.DocumentElement;
            }
            catch (Exception ex)
            {
                sErr = ex.Message;
            }

            if (sErr != "")
            {
                _lasterror = sErr;
                throw new Exception(sErr);
            }
        }

        public XmlDocument GetXmlDocument()
        {
            return _xdoc;
        }

        public XmlNode GetRootNode()
        {
            return _root;
        }

        //获取XML子结点列表
        public XmlNodeList GetXmlChildNodes(XmlNode parentNode, string xPath)
        {
            if (HTBaseFunc.NullToStr(xPath) == "")
                if (parentNode != null)
                    return parentNode.ChildNodes;
                else
                    return _xdoc.ChildNodes;
            else
                if (parentNode != null)
                    return parentNode.SelectNodes(xPath);
                else
                    return _xdoc.SelectNodes(xPath);
        }

        //设置或新增一个子结点
        public XmlNode SetXmlChildNode(XmlNode parentNode, string xName, string sInnerText, bool bForceNew)
        {
            XmlNode xmlNode = null;
            try
            {
                XmlNode tempNode = null;
                if (bForceNew == false)
                {
                    tempNode = parentNode.SelectSingleNode(xName);
                }
                if (tempNode == null)
                {
                    tempNode = _xdoc.CreateNode(XmlNodeType.Element, xName, "");
                }

                tempNode.InnerText = sInnerText;
                parentNode.AppendChild(tempNode);

                xmlNode = tempNode;
            }
            catch (Exception ex)
            {
                _lasterror = ex.Message;
            }

            return xmlNode;
        }

        //从结点获取指定属性的值
        public string GetAttributeValue(XmlNode xmlNode, string sAttributeName)
        {
            if (xmlNode == null) return null;
            if (sAttributeName == null || sAttributeName == "") return null;
            string sValue = null;
            for (int i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (xmlNode.Attributes[i].Name == sAttributeName)
                {
                    sValue = xmlNode.Attributes.GetNamedItem(sAttributeName).Value;
                    break;
                }
            }
            return sValue;
        }

        //设置结点指定属性的值
        public bool SetAttributeValue(XmlNode xmlNode, string sAttributeName, string sValue)
        {
            if (xmlNode == null) return false;
            if (sAttributeName == null || sAttributeName == "") return false;
            bool bFinded = false;
            for (int i = 0; i < xmlNode.Attributes.Count; i++)
            {
                if (xmlNode.Attributes[i].Name == sAttributeName)
                {
                    xmlNode.Attributes[sAttributeName].Value = sValue;
                    bFinded = true;
                    break;
                }
            }
            if (bFinded == false)
            {
                XmlAttribute xmlAttr = _xdoc.CreateAttribute(sAttributeName);
                xmlAttr.Value = sValue;
                xmlNode.Attributes.Append(xmlAttr);
            }
            return true;
        }

        //根据指定属性名的值来获取结点列表：内部函数
        private ArrayList GetChildNodesByAttributeValue(XmlNode parentNode, string sChildNodeName, string sAttributeName, string sValue, bool bValueCaseMatch, bool bOnlyGetOne)
        {
            ArrayList arrList = new ArrayList();

            if (sAttributeName == null || sAttributeName == "") return arrList;

            XmlNodeList xmlNodeList = GetXmlChildNodes(parentNode, sChildNodeName);

            bool bFinded;
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                bFinded = false;
                for (int i = 0; i < xmlNode.Attributes.Count; i++)
                {
                    if (xmlNode.Attributes[i].Name == sAttributeName)
                    {
                        if (bValueCaseMatch)
                            bFinded = (xmlNode.Attributes.GetNamedItem(sAttributeName).Value == sValue);
                        else
                            bFinded = (xmlNode.Attributes.GetNamedItem(sAttributeName).Value.ToLower() == sValue.ToLower());
                        break;
                    }
                }
                if (bFinded)
                {
                    arrList.Add(xmlNode);

                    if (bOnlyGetOne) break;
                }
            }

            return arrList;
        }

        //根据指定属性名的值获取所有结点
        public ArrayList GetChildNodesByAttributeValue(XmlNode parentNode, string sChildNodeName, string sAttributeName, string sValue, bool bValueCaseMatch)
        {
            return GetChildNodesByAttributeValue(parentNode, sChildNodeName, sAttributeName, sValue, bValueCaseMatch, false);
        }

        //根据指定属性名的值获取第一个结点
        public XmlNode GetChildNodeByAttributeValue(XmlNode parentNode, string sChildNodeName, string sAttributeName, string sValue, bool bValueCaseMatch)
        {
            ArrayList arrList = GetChildNodesByAttributeValue(parentNode, sChildNodeName, sAttributeName, sValue, bValueCaseMatch, true);
            if (arrList.Count == 0)
            {
                return null;
            }
            else
            {
                return (XmlNode)arrList[0];
            }
        }

        //从XML文件中读到数据表中
        public DataTable ReadXmlFileToDataTable(string sXmlFile)
        {
            if (HTBaseFunc.NullToStr(sXmlFile).Trim() == "")
            {
                _lasterror = "指定的XML文件无效！";
                return null;
            }

            try
            {
                DataSet dst = new DataSet();
                dst.ReadXml(sXmlFile);
                DataTable dt = dst.Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                _lasterror = "读取XML文件到数据表出错！" + ex.Message;
                return null;
            }
        }

        //将数据表写入XML文件
        public bool SaveDataTableToXmlFile(DataTable dt, string sXmlFile)
        {
            if (dt == null)
            {
                _lasterror = "要保存的DataTable无效！";
                return false;
            }
            if (HTBaseFunc.NullToStr(sXmlFile).Trim() == "")
            {
                _lasterror = "指定的XML文件无效！";
                return false;
            }

            try
            {
                DataSet dst = new DataSet();
                dst.Tables.Add(dt);
                dst.WriteXml(sXmlFile, XmlWriteMode.WriteSchema);
                return true;
            }
            catch (Exception ex)
            {
                _lasterror = "将数据表保存到XML文件出错！" + ex.Message;
                return false;
            }
        }

        //用指定文件保存XML
        public bool Save(string sXmlFile)
        {
            string sErr = SaveXmlFile(sXmlFile);
            if (sErr != "")
            {
                _lasterror = sErr;
                return false;
            }
            else
            {
                _xmlFile = sXmlFile;
                return true;
            }
        }

        //用当前文件保存XML
        public bool Save()
        {
            return Save(_xmlFile);
        }

        #endregion

        #region 配置读取

        //读取配置信息,配置的值用InnerText记录
        //这些配置必须通过xpath限定后无任何上级和同名结点
        /*格式如：
         * <xpath>
         * <Key1>Value1</Key1>
         * <Key2>Value2</Key2>
         * </xpath>
         */
        public Hashtable ReadConfig(XmlNode parentNode, string xpath)
        {
            XmlNode singleXmlNode;
            if (parentNode != null)
                if (HTBaseFunc.NullToStr(xpath) != "")
                    singleXmlNode = parentNode.SelectSingleNode(xpath);
                else
                    singleXmlNode = parentNode;
            else
                if (HTBaseFunc.NullToStr(xpath) != "")
                    singleXmlNode = _xdoc.SelectSingleNode(xpath);
                else
                    singleXmlNode = _root;
            XmlNodeList xmlNodeList = GetXmlChildNodes(singleXmlNode, "");
            Hashtable haseTable = new Hashtable();
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                haseTable.Add(xmlNode.Name, xmlNode.InnerText);
            }

            return haseTable;
        }

        //读取有一级集合的配置信息
        //这些配置必须通过xpath限定后只有一级上级,并且上级都是同名结点
        /*格式如：
         * <xpath>
         * <Key1>Value1</Key1>
         * <Key2>Value2</Key2>
         * </xpath>
         * <xpath>
         * <Key1>Value1</Key1>
         * <Key3>Value2</Key3>
         * </xpath>
         */
        public Hashtable ReadGroupConfig(XmlNode parentNode, string xpath)
        {
            XmlNodeList xmlNodeList = GetXmlChildNodes(parentNode, xpath);
            Hashtable haseTable = new Hashtable();
            int i = 0;
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                haseTable.Add("#" + i.ToString() + "#_" + xmlNode.Name, ReadConfig(xmlNode, ""));
                i++;
            }

            return haseTable;
        }

        //根据指定属性名的值读取配置信息,配置的值用Attribute记录,通过属性限定
        //这些配置必须通过xpath限定后无任何上级,并且都是同名结点
        public Hashtable ReadConfigByAttribute(XmlNode parentNode, string xpath, string sRestrictAttrName, string sRestrictAttrValue)
        {
            XmlNode singleXmlNode;
            if (HTBaseFunc.NullToStr(sRestrictAttrName) != "")
                singleXmlNode = GetChildNodeByAttributeValue(parentNode, xpath, sRestrictAttrName, sRestrictAttrValue, true);
            else
                if (parentNode != null)
                    if (HTBaseFunc.NullToStr(xpath) != "")
                        singleXmlNode = parentNode.SelectSingleNode(xpath);
                    else
                        singleXmlNode = parentNode;
                else
                    if (HTBaseFunc.NullToStr(xpath) != "")
                        singleXmlNode = _xdoc.SelectSingleNode(xpath);
                    else
                        singleXmlNode = _root;
            if (singleXmlNode != null)
            {
                XmlNodeList xmlNodeList = GetXmlChildNodes(singleXmlNode, "");
                Hashtable haseTable = new Hashtable();
                int i = 0;
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    Hashtable haseTable2 = new Hashtable();
                    foreach (XmlAttribute xmlAttr in xmlNode.Attributes)
                    {
                        haseTable2.Add(xmlAttr.Name, xmlAttr.Value);
                    }

                    haseTable.Add("#" + i.ToString() + "#_" + xmlNode.Name, haseTable2);
                    i++;
                }

                return haseTable;
            }
            else
                return null;
        }

        //提供几个对应的静态方法
        //xpath必须是完全限定的路径
        public static Hashtable ReadConfiguration(string xmlfile, string xpath)
        {
            HTXmlBaseFunc xmlbf = new HTXmlBaseFunc(xmlfile, false, "");
            return xmlbf.ReadConfig(null, xpath);
        }
        public static Hashtable ReadGroupConfiguration(string xmlfile, string xpath)
        {
            HTXmlBaseFunc xmlbf = new HTXmlBaseFunc(xmlfile, false, "");
            return xmlbf.ReadGroupConfig(null, xpath);
        }
        public static Hashtable ReadConfigurationByAttribute(string xmlfile, string xpath, string sRestrictAttrName, string sRestrictAttrValue)
        {
            HTXmlBaseFunc xmlbf = new HTXmlBaseFunc(xmlfile, false, "");
            return xmlbf.ReadConfigByAttribute(null, xpath, sRestrictAttrName, sRestrictAttrValue);
        }

        #endregion

    }

    public class HTTimerTask
    {
        #region 变量定义

        private System.Threading.Timer timer = null;

        private ArrayList timerTasks = new ArrayList();
        private ArrayList timerTaskParams = new ArrayList();
        private bool isRunning = false;

        private string lastError = "";

        #endregion

        #region 类初始化

        public HTTimerTask()
        {
        }

        public HTTimerTask(TimerCallback timeCallback, object timeCallbackParam, int timedMilliSeconds, int periodMilliSeconds)
        {
            AddTimerTask(timeCallback, timeCallbackParam);
            if (Start(timedMilliSeconds, periodMilliSeconds) == false)
            {
                throw new Exception(lastError);
            }
        }

        #endregion

        #region 操作函数

        //获取最新错误
        public string GetLastError()
        {
            return lastError;
        }

        //执行任务
        private void ExecuteTimerTasks(object data)
        {
            if (isRunning) return;
            isRunning = true;

            try
            {
                TimerCallback timerCallback;
                object timerCallbackParam;
                for (int i = 0; i < timerTasks.Count; i++)
                {
                    timerCallback = timerTasks[i] as TimerCallback;
                    timerCallbackParam = timerTaskParams[i];
                    timerCallback(timerCallbackParam);
                }

                isRunning = false;
            }
            catch (Exception ex)
            {
                isRunning = false;
                lastError = ex.Message;
                throw new Exception(ex.Message);
            }
        }

        //添加任务
        public void AddTimerTask(TimerCallback timeCallback, object timeCallbackParam)
        {
            timerTasks.Add(timeCallback);
            timerTaskParams.Add(timeCallbackParam);
        }

        //开始计时
        public bool Start(int timedMilliSeconds, int periodMilliSeconds)
        {
            try
            {
                if (isRunning == false)
                {
                    timer = new System.Threading.Timer(new TimerCallback(ExecuteTimerTasks), null, timedMilliSeconds, ((periodMilliSeconds == 0) ? System.Threading.Timeout.Infinite : periodMilliSeconds));
                }
                else
                {
                    timer.Change(timedMilliSeconds, periodMilliSeconds);
                }
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        //结束计时
        public bool Stop()
        {
            try
            {
                timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }

        public void Destory()
        {
            if (timer != null)
            {
                Stop();

                timer.Dispose();
                timer = null;
            }
        }

        #endregion

    }

    /// <summary>
    /// 远程服务操作
    /// </summary>
    public class HTRemotingFunc
    {

        #region 调用帮助

        /*服务器端调用方法
		先启动远程服务
		bool bSucc=StartRemotingServer("localhost", serverport);
		然后
        (1)注册反射对象
		Operate mo=new Operate();
		bool bSucc=RegisterMarshalObject(mo, "IOperate_LinkName")
        (2)注册反射服务
        RegisterMarshalService(typeof(Operate), "IOperate_LinkName");
		结束时终止远程服务
		EndRemotingServer();
        注意:Operate是用户自己的类,必须从MarshalByRefObject和让客户端调用的接口IOperate继承
		*/


        /*客户端调用方法
        在合适的地方调用
        IOperate mo=(IOperate)HTRemotingFunc.GetInterface(servername, serverport, "IOperate_LinkName");
        然后就可以调用mo中的接口方法
        注意:这里的接口IOperate名称必须与服务器端一致,包括namespace.
        */

        #endregion

        #region 远程服务

        private ArrayList _mashaledObjectList = new ArrayList();
        private ArrayList _mashaledServiceList = new ArrayList();
        private TcpChannel _channel;

        //启动远程服务
        public string StartRemotingServer(string channelName, string serverip, int serverport)
        {
            try
            {
                bool bFind = false;
                foreach (IChannel ichannel in ChannelServices.RegisteredChannels)
                {
                    if (ichannel.ChannelName == channelName)
                    {
                        bFind = true;
                    }
                }

                if (bFind)
                {
                    return "";
                }
                else
                {
                    BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                    BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
                    serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                    IDictionary props = new Hashtable();
                    if (channelName != null && channelName != "")
                        props["name"] = channelName;
                    props["port"] = serverport;

                    _channel = new TcpChannel(props, clientProvider, serverProvider);
                    ChannelDataStore channelData = (ChannelDataStore)_channel.ChannelData;
                    SetChannelUris(channelData, serverip);
                    ChannelServices.RegisterChannel(_channel);
                    return "";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string StartRemotingServer(int serverport)
        {
            string sChannelName = "localhost@" + serverport.ToString();
            string serverip = "";
            return StartRemotingServer(sChannelName, serverip, serverport);
        }

        //注册反射对象
        //mbr必须是一个继承自MarshalByRefObject和特定接口的类的静态实例
        //interfacename一般是该接口的名字
        public string RegisterMarshalObject(MarshalByRefObject mbr, string interfacename)
        {
            try
            {
                ObjRef objRef = RemotingServices.Marshal(mbr, interfacename);
                _mashaledObjectList.Add(mbr);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        //断开反射对象
        private void UnRegisterMarshalObject(MarshalByRefObject mbr)
        {
            try
            {
                ObjRef objref = RemotingServices.GetObjRefForProxy(mbr);
                if (objref != null)
                    RemotingServices.Unmarshal(objref);
            }
            catch
            {
            }
            _mashaledObjectList.Remove(mbr);

        }

        //注册反射服务
        public bool RegisterMarshalService(Type serviceType, string interfacename)
        {
            if (_mashaledServiceList.Contains(interfacename))
                return false;

            RemotingConfiguration.RegisterWellKnownServiceType(serviceType, interfacename, WellKnownObjectMode.SingleCall);
            _mashaledServiceList.Add(interfacename);
            return true;
        }

        //结束远程服务
        public void EndRemotingServer()
        {
            for (int i = _mashaledObjectList.Count - 1; i >= 0; i--)
            {
                UnRegisterMarshalObject((MarshalByRefObject)_mashaledObjectList[i]);
            }

            _mashaledServiceList.Clear();

            try
            {
                ChannelServices.UnregisterChannel(_channel);
            }
            catch
            {
            }
        }

        #endregion

        #region 远程客户

        //要将返回结果转化为interfacename定义的接口来使用
        public static object GetInterface(string serverip, int serverport, string interfacename)
        {
            string uri = string.Format("{0}://{1}:{2}/{3}", "tcp", serverip, serverport, interfacename);
            return Activator.GetObject(typeof(RemotingBaseClass), uri);
        }

        #endregion

        #region Common Function

        private static void SetChannelUris(ChannelDataStore channelData, string newIpAddress)
        {
            if (newIpAddress != "(HOLD)")
            {
                if (newIpAddress == null || newIpAddress.Trim() == "")
                {
                    //获得本机IP地址(最后一个) 即:如果有外网地址使用外网地址,否则使用内网地址 
                    System.Net.IPAddress[] ipList = System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList;
                    string localIPAddress;
                    if (ipList.Length > 1)
                        localIPAddress = ipList[1].ToString();
                    else
                        localIPAddress = ipList[0].ToString();

                    newIpAddress = localIPAddress;
                }

                string localPoint = channelData.ChannelUris[0];
                //取得协议 
                int i = localPoint.IndexOf(":");
                string confer = localPoint.Substring(0, i + 3);
                //取得信道的端口号 
                i = localPoint.LastIndexOf(":");
                string localPort = localPoint.Substring(i, localPoint.Length - i);
                //重设信道IP地址 
                string[] IpAndPort = { confer + newIpAddress + localPort };
                channelData.ChannelUris = IpAndPort;
            }
        }

        #endregion
    }

    #endregion

}