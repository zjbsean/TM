using com.tieao.mmo.CustomTypeInProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class MathHelper
    {
        public static Random MyRandom = new Random();

        public static bool DateEqual(DateTime date1,DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month && date1.Day == date2.Day;
        }

        public static string TIntList2Str(TIntList intList,char split)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < intList.GetElements().Count; i++)
            {
                sb.Append(intList.GetElements()[i]);
                if (i != intList.GetElements().Count - 1)
                    sb.Append(split);
            }

            return sb.ToString();
        }

        public static TIntList Str2TIntList(string str, char split)
        {
            string[] strArray = str.Split(split);
            TIntList list = new TIntList();
            foreach (var item in strArray)
            {
                if (item == string.Empty)
                    continue;

                list.Add(Convert.ToInt32(item));
            }
            return list;
        }

        public static List<string> Str2StrList(string str, char split)
        {
            List<string> tList = new List<string>();
            string[] strArray = str.Split(split);
            foreach (var item in strArray)
            {
                if (item == string.Empty)
                    continue;

                tList.Add(item);
            }
            return tList;
        }

        /// <summary>
        /// 权重计算
        /// </summary>
        /// <param name="pl">数值列表</param>
        /// <returns>随机到的列表索引</returns>
        public static int WeightCalc(List<int> pl)
        {
            if (pl.Count == 0)
                return -1;

            int total = 0;
            for (int i = 0; i < pl.Count; i++)
            {
                total += pl[i];
            }
            int ramdomNum = MyRandom.Next(1, total + 1);
            int currTotal = 0;
            for (int i = 0; i < pl.Count; i++)
            {
                currTotal += pl[i];
                if (currTotal >= ramdomNum)
                {
                    return i;
                }
            }

            return -2;
        }

        /// <summary>
        /// 获取最后一个刷新时间点
        /// </summary>
        /// <param name="hoursPoint">时间点（小时）</param>
        public static DateTime GetLastRefreshTime(int hoursPoint)
        {
            var now = DateTime.Now;

            if (now.Hour >= hoursPoint)
            {
                return now.Date.AddHours(hoursPoint);
            }
            else
                return now.Date.AddDays(-1).AddHours(hoursPoint);
        }
    }
}
