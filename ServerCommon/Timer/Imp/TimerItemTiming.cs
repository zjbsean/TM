/********************************************************************
    created:	2015/11/27 14:45:06
    author:		donghuiqi
    email:		
	
    purpose:	TimerItem 定时执行
*********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerCommon
{
    [Flags]
    public enum EWeek
    {
        Monday = 1,         //星期一
        Tuesday = 2,        //星期二
        Wednesday = 4,      //星期三
        Thursday = 8,       //星期四
        Friday = 16,        //星期五
        Saturday = 32,      //星期六
        Sunday = 64,        //星期日

        Workday = Monday | Tuesday | Wednesday | Thursday | Friday, //工作日
        DayOff = Saturday | Sunday,     //休息日
        Everyday = Workday | DayOff,    //每日
    }

    public class TimerItemTiming : TimerItemBasic
    {
        /// <summary>
        /// 定时执行TimerItem
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="callback">回调函数</param>
        /// <param name="paras">回调函数参数</param>
        /// <param name="timePoints">时间点列表，时间点数值为某日初到指定时间点所间隔的秒数</param>
        /// <param name="week">执行的周日</param>
        /// <param name="times">执行的次数</param>
        public TimerItemTiming(object id, Action<object[]> callback, object[] paras,List<Double> timePoints, EWeek week = EWeek.Everyday, int times = 1)
            :base(id,callback,paras,times)
        {
            if (timePoints.Count == 0)
                throw new Exception("至少需要设置一个时间点");

            m_timePoints = timePoints;
            timePoints.Sort();
            m_week = week;
        }

        protected override DateTime GenNextTime()
        {
            if (m_nextTime == null)
            {
                DateTime weekDay;
                DateTime now = DateTime.Now;
                if ((DayOfWeekConvertToEWeek(now.DayOfWeek) & m_week) != 0)
                {
                    weekDay = now;
                }
                else
                {
                    weekDay = GetNextWeekDay(now);
                }

                DateChange(weekDay);
            }

            return TakeExecTime();
        }

        DateTime GetNextWeekDay(DateTime date)
        {
            for (int i = 1; i <= 7; i++)
            {
                DateTime timeItem = date.AddDays(i);
                if ((DayOfWeekConvertToEWeek(timeItem.DayOfWeek) & m_week) != 0)
                {
                    return timeItem;
                }
            }

            throw new Exception("获取下一个执行日失败。");
        }

        EWeek DayOfWeekConvertToEWeek(DayOfWeek dayOfWeek)
        {
            EWeek week = EWeek.Monday;

            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    week = EWeek.Friday;
                    break;
                case DayOfWeek.Monday:
                    week = EWeek.Monday;
                    break;
                case DayOfWeek.Saturday:
                    week = EWeek.Saturday;
                    break;
                case DayOfWeek.Sunday:
                    week = EWeek.Sunday;
                    break;
                case DayOfWeek.Thursday:
                    week = EWeek.Thursday;
                    break;
                case DayOfWeek.Tuesday:
                    week = EWeek.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    week = EWeek.Wednesday;
                    break;
            }

            return week;
        }

        #region 执行日期的时间点
        void DateChange(DateTime date)
        {
            if (m_date != date.Date)
            {
                m_date = date.Date;

                m_surplusPoints.Clear();

                if (DateEquips(date.Date,DateTime.Now.Date))
                {
                    //添加还没有过时的时间点
                    double nowTimePoint = (DateTime.Now - DateTime.Now.Date).TotalSeconds;
                    for (int i = 0; i < m_timePoints.Count; i++)
                    {
                        if (m_timePoints[i] >= nowTimePoint)
                        {
                            m_surplusPoints.Add(m_timePoints[i]);
                        }
                    }
                }
                else
                {
                    m_surplusPoints.AddRange(m_timePoints);
                }
            }
        }

        bool DateEquips(DateTime dt1, DateTime dt2)     //是否是同一天
        {
            if (dt1.Year == dt2.Year
                && dt1.Month == dt2.Month
                && dt1.Day == dt2.Day)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        DateTime TakeExecTime()
        {
            if (m_surplusPoints.Count == 0)
            {
                DateChange(GetNextWeekDay(m_date));
            }

            DateTime result = m_date.AddSeconds(m_surplusPoints[0]);
            m_surplusPoints.RemoveAt(0);
            return result;
        }


        DateTime m_date = DateTime.MinValue;
        List<double> m_surplusPoints = new List<double>();   //剩余的时间点
        #endregion

        List<Double> m_timePoints;
        EWeek m_week;
    }
}
