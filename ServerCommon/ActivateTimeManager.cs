using System;
using System.Collections.Generic;
using System.Text;
using GsTechLib;

namespace ServerCommon
{
    //激活时间类型
    public enum eActivateTimeType
    {
        NONE = 0,                       //无

        EVERY_DAY = 1,                  //每日
        
        APPOINT_WEEK = 2,               //每周几

        APPOINT_DAY = 3,                //指定日期

        APPOINT_DAY_RANGE = 4,          //指定日期范围

        EVERY_DAY_DURATION = 5,         //每日持续

        APPOINT_WEEK_DURATION = 6,      //每周几持续

        APPOINT_DAY_DURATION = 7,       //指定日期持续

        APPOINT_DAY_RANGE_DURATION = 8, //指定日期范围持续
    }

    //月日
    public class MonthDay
    {
        public int Month
        {
            get
            {
                return m_month;
            }
        }
        public int Day
        {
            get
            {
                return m_day;
            }
        }

        public MonthDay(int month, int day)
        {
            m_month = month;
            m_day = day;
        }

        private int m_month;
        private int m_day;
    }

    //月日范围
    public class MonthDayRange
    {
        public MonthDay StartDay;
        public MonthDay EndDay;
        public MonthDayRange(MonthDay startDay, MonthDay endDay)
        {
            StartDay = startDay;
            EndDay = endDay;
        }

        /// <summary>
        /// 检测是否在月日范围
        /// </summary>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool CheckDayInRange(int month, int day)
        {
            //跨年
            if (StartDay.Month > EndDay.Month ||
                (StartDay.Month == EndDay.Month && StartDay.Day > EndDay.Day)
                )
            {
                if (month > StartDay.Month)
                    return true;
                else if (month == StartDay.Month)
                {
                    if (day >= StartDay.Day)
                        return true;
                }
                else
                {
                    if (month < EndDay.Month)
                        return true;
                    else if (month == EndDay.Month)
                    {
                        if (day <= EndDay.Day)
                            return true;
                    }
                }
            }
            //非跨年
            else if(StartDay.Month == month && StartDay.Month == EndDay.Month)
            {
                if(StartDay.Day <= day && day <= EndDay.Day)
                    return true;
            }
            else 
            {
                if (StartDay.Month < month && month < EndDay.Month)
                    return true;
                else 
                {
                    if (StartDay.Month == month && StartDay.Day <= day)
                        return true;
                    if (EndDay.Month == month && day <= EndDay.Day)
                        return true;
                }
            }

            return false;
        }
    }

    //天时
    public class DayTime
    {
        public int Hour
        {
            get
            {
                return m_hour;
            }
        }

        public int Min
        {
            get
            {
                return m_min;
            }
        }

        public int Sec
        {
            get
            {
                return m_sec;
            }
        }

        public int TotalSec
        {
            get
            {
                return m_totalSec;
            }
        }

        public int Duration
        {
            get
            {
                return m_duration;
            }
        }

        public DayTime(int hour, int min, int sec, int duration)
        {
            m_hour = hour;
            m_min = min;
            m_sec = sec;
            m_totalSec = m_hour * 3600 + m_min * 60 + m_sec;
            m_duration = duration;
        }

        private int m_hour;
        private int m_min;
        private int m_sec;
        private int m_totalSec;
        private int m_duration;
    }

    public interface IActiveTimeOperater
    {
        int CheckActiveStageNow(DateTime dt);

        int GetCurActiveEndTime();

        int GetCurActiveStartTime();
    }

    //激活时间范围基类
    public class ActiveTimeBase : IActiveTimeOperater
    {
        public ActiveTimeBase(eActivateTimeType activeType)
        {
            m_activeType = activeType;
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public virtual bool InitData(string strData) { return false; }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public virtual int CheckActiveStageNow(DateTime dt) { return 0; }

        /// <summary>
        /// 解析时间格式（HH:mm:ss）
        /// </summary>
        /// <param name="strTime"></param>
        /// <returns></returns>
        public bool ParseTime(string strTime)
        {
            int groupIndex = 0;
            bool result = true;
            while (true && groupIndex < 100)
            {
                string timeGroupStr = HTBaseFunc.DepartStr(strTime, ";", groupIndex);
                if (timeGroupStr == "")
                    break;

                List<DayTime> dtList = new List<DayTime>();
                int index = 0;
                while (true && index < 100)
                {
                    string timeStr = HTBaseFunc.DepartStr(timeGroupStr, ",", index);
                    if (timeStr == "")
                        break;
                    int hour, min, sec;
                    string strParam = HTBaseFunc.DepartStr(timeStr, ":", 0);
                    if (strParam == "")
                    {
                        result = false;
                        break;
                    }
                    hour = Convert.ToInt32(strParam);
                    if (hour < 0 || hour > 23)
                    {
                        result = false;
                        break;
                    }

                    strParam = HTBaseFunc.DepartStr(timeStr, ":", 1);
                    if (strParam == "")
                    {
                        result = false;
                        break;
                    }
                    min = Convert.ToInt32(strParam);
                    if (min < 0 || min > 59)
                    {
                        result = false;
                        break;
                    }

                    strParam = HTBaseFunc.DepartStr(timeStr, ":", 2);
                    if (strParam == "")
                    {
                        result = false;
                        break;
                    }
                    sec = Convert.ToInt32(strParam);
                    if (sec < 0 || sec > 59)
                    {
                        result = false;
                        break;
                    }

                    DayTime dt = new DayTime(hour, min, sec, 0);
                    dtList.Add(dt);
                    ++index;
                }

                if (result == false)
                    break;

                if (dtList.Count < 2)
                {
                    result = false;
                    break;
                }

                m_activeTimeRangeList.Add(dtList);

                ++groupIndex;
            }

            if (result == false)
                m_activeTimeRangeList.Clear();

            return result;
        }

        /// <summary>
        /// 解析日期列表
        /// </summary>
        /// <param name="strDay"></param>
        /// <returns></returns>
        public List<MonthDay> ParseDayList(string strDayList)
        {
            List<MonthDay> dataList = new List<MonthDay>();

            bool result = true;
            int index = 0;
            while (true && index < 100)
            {
                string strDay = HTBaseFunc.DepartStr(strDayList, ";", index);
                if (strDay == "")
                    break;

                MonthDay monthDay = ParseDay(strDay);
                if (monthDay == null)
                {
                    result = false;
                    break;
                }

                dataList.Add(monthDay);

                ++index;
            }

            if (result == false || dataList.Count == 0)
                return null;

            return dataList;
        }

        /// <summary>
        /// 解析日期
        /// </summary>
        /// <param name="strDay"></param>
        /// <returns></returns>
        public MonthDay ParseDay(string strDay)
        {
            string strParam = HTBaseFunc.DepartStr(strDay, "-", 0);
            if (strParam == "")
            {
                return null;
            }
            int month = Convert.ToInt32(strParam);

            strParam = HTBaseFunc.DepartStr(strDay, "-", 1);
            if (strParam == "")
            {
                return null;
            }
            int day = Convert.ToInt32(strParam);

            return new MonthDay(month, day);
        }

        /// <summary>
        /// 返回指定当天已经过去秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int SecSpendToday(DateTime dt)
        {
            return dt.Hour * 3600 + dt.Minute * 60 + dt.Second;
        }

        /// <summary>
        /// 获取当前互动结束时间
        /// </summary>
        /// <returns></returns>
        public int GetCurActiveEndTime()
        {
            if (m_curActiveTimeList.Count == 0)
                return int.MaxValue;

            return m_curActiveTimeList[m_curActiveTimeList.Count - 1];
        }

        /// <summary>
        /// 获取当前互动开始时间
        /// </summary>
        /// <returns></returns>
        public int GetCurActiveStartTime()
        {
            if (m_curActiveTimeList.Count == 0)
                return int.MaxValue;

            return m_curActiveTimeList[0];
        }

        /// <summary>
        /// 获取激活类型
        /// </summary>
        /// <returns></returns>
        public eActivateTimeType GetActiveType()
        {
            return m_activeType;
        }

        protected List<int> m_curActiveTimeList = new List<int>();                          //当前激活时间列表

        protected List<List<DayTime>> m_activeTimeRangeList = new List<List<DayTime>>();    //激活时间范围列表

        protected eActivateTimeType m_activeType = eActivateTimeType.NONE;                  //激活类型
    }

    //激活时间持续性基类
    public class ActiveTimeDurationBase : IActiveTimeOperater
    {
        public ActiveTimeDurationBase(eActivateTimeType activeType)
        {
            m_activeType = activeType;
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public virtual bool InitData(string strData) { return false; }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public virtual int CheckActiveStageNow(DateTime dt) { return 0; }

        /// <summary>
        /// 解析时间格式（HH:mm:ss）
        /// </summary>
        /// <param name="strTime"></param>
        /// <returns></returns>
        public bool ParseTime(string strTime)
        {
            int groupIndex = 0;
            bool result = true;
            while (true && groupIndex < 100)
            {
                string timeGroupStr = HTBaseFunc.DepartStr(strTime, ";", groupIndex);
                if (timeGroupStr == "")
                    break;

                string timeStr = HTBaseFunc.DepartStr(timeGroupStr, "-", 0);
                if (timeStr == "")
                    break;
                int hour, min, sec;
                string strParam = HTBaseFunc.DepartStr(timeStr, ":", 0);
                if (strParam == "")
                {
                    result = false;
                    break;
                }
                hour = Convert.ToInt32(strParam);
                if (hour < 0 || hour > 23)
                {
                    result = false;
                    break;
                }

                strParam = HTBaseFunc.DepartStr(timeStr, ":", 1);
                if (strParam == "")
                {
                    result = false;
                    break;
                }
                min = Convert.ToInt32(strParam);
                if (min < 0 || min > 59)
                {
                    result = false;
                    break;
                }

                strParam = HTBaseFunc.DepartStr(timeStr, ":", 2);
                if (strParam == "")
                {
                    result = false;
                    break;
                }
                sec = Convert.ToInt32(strParam);
                if (sec < 0 || sec > 59)
                {
                    result = false;
                    break;
                }

                timeStr = HTBaseFunc.DepartStr(timeGroupStr, "-", 1);
                int duration = Convert.ToInt32(timeStr);

                DayTime dt = new DayTime(hour, min, sec, duration);
                m_activeTimeList.Add(dt);

                if (result == false)
                    break;
            }

            if (result == false)
                m_activeTimeList.Clear();

            return result;
        }

        /// <summary>
        /// 解析日期列表
        /// </summary>
        /// <param name="strDay"></param>
        /// <returns></returns>
        public List<MonthDay> ParseDayList(string strDayList)
        {
            List<MonthDay> dataList = new List<MonthDay>();

            bool result = true;
            int index = 0;
            while (true && index < 100)
            {
                string strDay = HTBaseFunc.DepartStr(strDayList, ";", index);
                if (strDay == "")
                    break;

                MonthDay monthDay = ParseDay(strDay);
                if (monthDay == null)
                {
                    result = false;
                    break;
                }

                dataList.Add(monthDay);

                ++index;
            }

            if (result == false || dataList.Count == 0)
                return null;

            return dataList;
        }

        /// <summary>
        /// 解析日期
        /// </summary>
        /// <param name="strDay"></param>
        /// <returns></returns>
        public MonthDay ParseDay(string strDay)
        {
            string strParam = HTBaseFunc.DepartStr(strDay, "-", 0);
            if (strParam == "")
            {
                return null;
            }
            int month = Convert.ToInt32(strParam);

            strParam = HTBaseFunc.DepartStr(strDay, "-", 1);
            if (strParam == "")
            {
                return null;
            }
            int day = Convert.ToInt32(strParam);

            return new MonthDay(month, day);
        }

        /// <summary>
        /// 返回指定当天已经过去秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int SecSpendToday(DateTime dt)
        {
            return dt.Hour * 3600 + dt.Minute * 60 + dt.Second;
        }

        /// <summary>
        /// 获取当前互动结束时间
        /// </summary>
        /// <returns></returns>
        public int GetCurActiveEndTime()
        {
            if (m_curActiveTimeList.Count == 0)
                return int.MaxValue;

            return m_curActiveTimeList[m_curActiveTimeList.Count - 1];
        }

        /// <summary>
        /// 获取当前互动开始时间
        /// </summary>
        /// <returns></returns>
        public int GetCurActiveStartTime()
        {
            if (m_curActiveTimeList.Count == 0)
                return int.MaxValue;

            return m_curActiveTimeList[0];
        }

        /// <summary>
        /// 获取激活类型
        /// </summary>
        /// <returns></returns>
        public eActivateTimeType GetActiveType()
        {
            return m_activeType;
        }

        protected List<int> m_curActiveTimeList = new List<int>();                          //当前激活时间列表

        protected List<DayTime> m_activeTimeList = new List<DayTime>();                //激活时间范围列表

        protected eActivateTimeType m_activeType = eActivateTimeType.NONE;                  //激活类型
    }

    /// <summary>
    /// 每日激活
    ///     格式：HH:mm:ss,...;HH:mm:ss,...;...
    /// </summary>
    public class DayActivate : ActiveTimeBase
    {
        public DayActivate()
            : base(eActivateTimeType.EVERY_DAY)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData) 
        {
            return ParseTime(strData);
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt) 
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.DateTimeServerTimeInSecond(dt);
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);
                List<DayTime> timeGroupList = null;
                for (int i = 0; i < m_activeTimeRangeList.Count; ++i)
                {
                    int stages = m_activeTimeRangeList[i].Count;
                    if (m_activeTimeRangeList[i][0].TotalSec <= secSpend && secSpend <= m_activeTimeRangeList[i][stages - 1].TotalSec)
                    {
                        timeGroupList = m_activeTimeRangeList[i];
                        break;
                    }
                }
                if (timeGroupList == null)
                {
                    timeGroupList = m_activeTimeRangeList[0];
                    dtNow = dtNow.AddDays(1);
                }
                for (int i = 0; i < timeGroupList.Count; ++i)
                {
                    DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, timeGroupList[i].Hour, timeGroupList[i].Min, timeGroupList[i].Sec);

                    m_curActiveTimeList.Add(ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime));
                }
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// 指定周几激活
    ///     格式：周几;周几;...|HH:mm:ss,...;HH:mm:ss,...;...     (周日到周六：0-6)
    /// </summary>
    public class AppointWeekActivate : ActiveTimeBase
    {
        public AppointWeekActivate()
            : base(eActivateTimeType.APPOINT_WEEK)
        {
            
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData)
        {
            bool result = true;

            string weekList = HTBaseFunc.DepartStr(strData, "|", 0);
            string timeList = HTBaseFunc.DepartStr(strData, "|", 1);

            int weekIndex = 0;
            while (true && weekIndex < 100)
            {
                string weekStr = HTBaseFunc.DepartStr(weekList, ";", weekIndex);
                if (weekStr == "")
                    break;
                int week = Convert.ToInt32(weekStr);
                if (week < 0 || week > 6)
                {
                    result = false;
                    break;
                }

                m_weekList.Add((DayOfWeek)week);

                ++weekIndex;
            }
            if (m_weekList.Count < 1)
                result = false;

            if (result == false)
                m_weekList.Clear();
            else
            {
                result = ParseTime(timeList);
            }

            return result;
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt)
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.DateTimeServerTimeInSecond(dt);
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);
                
                List<DayTime> timeGroupList = null;
                bool todayIsActiveDay = false;
                for (int i = 0; i < m_weekList.Count; ++i)
                {
                    if (dtNow.DayOfWeek == m_weekList[i])
                    {
                        todayIsActiveDay = true;
                        break;
                    }
                }
                if (todayIsActiveDay == true)
                {
                    for (int i = 0; i < m_activeTimeRangeList.Count; ++i)
                    {
                        int stages = m_activeTimeRangeList[i].Count;
                        if (m_activeTimeRangeList[i][0].TotalSec <= secSpend && secSpend <= m_activeTimeRangeList[i][stages - 1].TotalSec)
                        {
                            timeGroupList = m_activeTimeRangeList[i];
                            break;
                        }
                    }
                }
                if (timeGroupList == null)
                {
                    timeGroupList = m_activeTimeRangeList[0];

                    int todayWeek = Convert.ToInt32(dtNow.DayOfWeek);
                    int addDays = 0;
                    for (int i = 0; i < m_weekList.Count; ++i)
                    {
                        int checkWeek = Convert.ToInt32(m_weekList[i]);
                        if (checkWeek > todayWeek)
                        {
                            addDays = checkWeek - todayWeek;
                            break;
                        }
                    }
                    if (addDays == 0)
                        addDays = Convert.ToInt32(m_weekList[0]) + 7 - todayWeek;
                    dtNow = dtNow.AddDays(addDays);
                }
                for (int i = 0; i < timeGroupList.Count; ++i)
                {
                    DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, timeGroupList[i].Hour, timeGroupList[i].Min, timeGroupList[i].Sec);
                    m_curActiveTimeList.Add(ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime));
                }
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }


        private List<DayOfWeek> m_weekList = new List<DayOfWeek>();
    }

    /// <summary>
    /// 指定月日激活
    ///     格式：月-日;月-日;...|HH:mm:ss,...;HH:mm:ss,...;...
    /// </summary>
    public class AppointDayActivate : ActiveTimeBase
    {
        public AppointDayActivate()
            : base(eActivateTimeType.APPOINT_DAY)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData)
        {
            string monthDayList = HTBaseFunc.DepartStr(strData, "|", 0);
            string timeList = HTBaseFunc.DepartStr(strData, "|", 1);

            m_monthDayList = ParseDayList(monthDayList);
            if (m_monthDayList == null)
                return false;

            return ParseTime(timeList);
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt)
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.DateTimeServerTimeInSecond(dt);
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);

                List<DayTime> timeGroupList = null;
                bool todayIsActiveDay = false;
                for (int i = 0; i < m_monthDayList.Count; ++i)
                {
                    if (m_monthDayList[i].Month == dtNow.Month && m_monthDayList[i].Day == dtNow.Day)
                    {
                        todayIsActiveDay = true;
                        break;
                    }
                }
                if (todayIsActiveDay == true)
                {
                    for (int i = 0; i < m_activeTimeRangeList.Count; ++i)
                    {
                        int stages = m_activeTimeRangeList[i].Count;
                        if (m_activeTimeRangeList[i][0].TotalSec <= secSpend && secSpend <= m_activeTimeRangeList[i][stages - 1].TotalSec)
                        {
                            timeGroupList = m_activeTimeRangeList[i];
                            break;
                        }
                    }
                }
                if (timeGroupList == null)
                {
                    timeGroupList = m_activeTimeRangeList[0];
                    int newMonth = -1, newDay = -1, newYear = 0;
                    for (int i = 0; i < m_monthDayList.Count; ++i)
                    {
                        if (    m_monthDayList[i].Month > dtNow.Month || 
                                (m_monthDayList[i].Month == dtNow.Month && m_monthDayList[i].Day > dtNow.Day)
                            )
                        {
                            newYear = dtNow.Year;
                            newMonth = m_monthDayList[i].Month;
                            newDay = m_monthDayList[i].Day;
                            break;
                        }
                    }
                    if (newMonth == -1)
                    {
                        newYear = dtNow.Year + 1;
                        newMonth = m_monthDayList[0].Month;
                        newDay = m_monthDayList[0].Day;
                    }

                    dtNow = new DateTime(newYear, newMonth, newDay);
                }
                for (int i = 0; i < timeGroupList.Count; ++i)
                {
                    DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, timeGroupList[i].Hour, timeGroupList[i].Min, timeGroupList[i].Sec);
                    m_curActiveTimeList.Add(ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime));
                }
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }

        private List<MonthDay> m_monthDayList = new List<MonthDay>();
    }

    /// <summary>
    /// 指定月日范围激活
    ///     格式：月-日,月-日;月-日,月-日;...|HH:mm:ss,...;HH:mm:ss,...;...
    /// </summary>
    public class AppointDayRange : ActiveTimeBase
    {
        public AppointDayRange()
            : base(eActivateTimeType.APPOINT_DAY_RANGE)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData)
        {
            string monthDayList = HTBaseFunc.DepartStr(strData, "|", 0);
            string timeList = HTBaseFunc.DepartStr(strData, "|", 1);

            int groupIndex = 0;
            bool result = true;
            while (true && groupIndex < 100)
            {
                string monthDayGroupStr = HTBaseFunc.DepartStr(monthDayList, ";", groupIndex);
                if (monthDayGroupStr == "")
                    break;

                string startDayStr = HTBaseFunc.DepartStr(monthDayGroupStr, ",", 0);
                string endDayStr = HTBaseFunc.DepartStr(monthDayGroupStr, ",", 1);
                MonthDay startDay = ParseDay(startDayStr);
                if (startDay == null)
                {
                    result = false;
                    break;
                }
                MonthDay endDay = ParseDay(endDayStr);
                if (endDay == null)
                {
                    result = false;
                    break;
                }

                m_monthDayRangeList.Add(new MonthDayRange(startDay, endDay));

                ++groupIndex;
            }
            if (result == false)
            {
                m_monthDayRangeList.Clear();
                return false;
            }

            if (m_monthDayRangeList.Count == 0)
                return false;

            return ParseTime(timeList);
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt)
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.CurrentServerTimeInSecond;
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);

                MonthDayRange monthDayRange = null;
                for (int i = 0; i < m_monthDayRangeList.Count; ++i)
                {
                    if (m_monthDayRangeList[i].CheckDayInRange(dtNow.Month, dtNow.Day))
                    {
                        monthDayRange = m_monthDayRangeList[i];
                        break;
                    }
                }

                List<DayTime> timeGroupList = null;
                if (monthDayRange != null)
                {
                    for (int i = 0; i < m_activeTimeRangeList.Count; ++i)
                    {
                        int stages = m_activeTimeRangeList[i].Count;
                        if (m_activeTimeRangeList[i][0].TotalSec <= secSpend && secSpend <= m_activeTimeRangeList[i][stages - 1].TotalSec)
                        {
                            timeGroupList = m_activeTimeRangeList[i];
                            break;
                        }
                    }
                }
                if (timeGroupList == null)
                {
                    timeGroupList = m_activeTimeRangeList[0];

                    bool finded = false;
                    if (monthDayRange != null)
                    {
                        dtNow = dtNow.AddDays(1);
                        if (monthDayRange.CheckDayInRange(dtNow.Month, dtNow.Day) == false)
                        {
                            for (int i = 0; i < m_monthDayRangeList.Count; ++i)
                            {
                                if (m_monthDayRangeList[i].StartDay.Month > monthDayRange.EndDay.Month ||
                                    (m_monthDayRangeList[i].StartDay.Month == monthDayRange.EndDay.Month && m_monthDayRangeList[i].StartDay.Day > monthDayRange.EndDay.Day))
                                {
                                    monthDayRange = m_monthDayRangeList[i];
                                    dtNow = new DateTime(dtNow.Year, monthDayRange.StartDay.Month, monthDayRange.StartDay.Day);
                                    finded = true;
                                    break;
                                }
                            }
                        }
                        else
                            finded = true;
                    }
                    if (finded == false)
                        dtNow = new DateTime(dtNow.Year + 1, m_monthDayRangeList[0].StartDay.Month, m_monthDayRangeList[0].StartDay.Day);

                }
                for (int i = 0; i < timeGroupList.Count; ++i)
                {
                    DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, timeGroupList[i].Hour, timeGroupList[i].Min, timeGroupList[i].Sec);
                    m_curActiveTimeList.Add(ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime));
                }
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }

        //月日范围列表
        private List<MonthDayRange> m_monthDayRangeList = new List<MonthDayRange>();
    }

    /// <summary>
    /// 每日激活N次，每次持续X秒
    ///     格式：HH:mm:ss-D;......
    /// </summary>
    public class DayActivateDuration : ActiveTimeDurationBase
    {
        public DayActivateDuration()
            : base(eActivateTimeType.EVERY_DAY_DURATION)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData) 
        {
            return ParseTime(strData);
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt) 
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.DateTimeServerTimeInSecond(dt);
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);
                DayTime dayTime = null;
                for (int i = 0; i < m_activeTimeList.Count; ++i)
                {
                    if (m_activeTimeList[i].TotalSec <= secSpend && secSpend <= m_activeTimeList[i].TotalSec + m_activeTimeList[i].Duration)
                    {
                        dayTime = m_activeTimeList[i];
                        break;
                    }
                }
                if (dayTime == null)
                {
                    dtNow = dtNow.AddDays(1);
                    dayTime = m_activeTimeList[0];
                }
                DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, dayTime.Hour, dayTime.Min, dayTime.Sec);
                int activeStartTime = ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime);
                m_curActiveTimeList.Add(activeStartTime);
                activeStartTime += dayTime.Duration;
                m_curActiveTimeList.Add(activeStartTime);
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }
    }

    /// <summary>
    /// 指定周几激活N次，每次持续X秒
    ///     格式：周几;周几;...|HH:mm:ss-D;......     (周日到周六：0-6)
    /// </summary>
    public class AppointWeekActivateDuration : ActiveTimeDurationBase
    {
        public AppointWeekActivateDuration()
            : base(eActivateTimeType.APPOINT_WEEK_DURATION)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData)
        {
            bool result = true;

            string weekList = HTBaseFunc.DepartStr(strData, "|", 0);
            string timeList = HTBaseFunc.DepartStr(strData, "|", 1);

            int weekIndex = 0;
            while (true && weekIndex < 100)
            {
                string weekStr = HTBaseFunc.DepartStr(weekList, ";", weekIndex);
                if (weekStr == "")
                    break;
                int week = Convert.ToInt32(weekStr);
                if (week < 0 || week > 6)
                {
                    result = false;
                    break;
                }

                m_weekList.Add((DayOfWeek)week);

                ++weekIndex;
            }
            if (m_weekList.Count < 1)
                result = false;

            if (result == false)
                m_weekList.Clear();
            else
            {
                result = ParseTime(timeList);
            }

            return result;
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt)
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.DateTimeServerTimeInSecond(dt);
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);

                DayTime dayTime = null;
                bool todayIsActiveDay = false;
                for (int i = 0; i < m_weekList.Count; ++i)
                {
                    if (dtNow.DayOfWeek == m_weekList[i])
                    {
                        todayIsActiveDay = true;
                        break;
                    }
                }
                if (todayIsActiveDay == true)
                {
                    for (int i = 0; i < m_activeTimeList.Count; ++i)
                    {
                        if (m_activeTimeList[i].TotalSec <= secSpend && secSpend <= m_activeTimeList[i].TotalSec + m_activeTimeList[i].Duration)
                        {
                            dayTime = m_activeTimeList[i];
                            break;
                        }
                    }
                }
                if (dayTime == null)
                {
                    dayTime = m_activeTimeList[0];

                    int todayWeek = Convert.ToInt32(dtNow.DayOfWeek);
                    int addDays = 0;
                    for (int i = 0; i < m_weekList.Count; ++i)
                    {
                        int checkWeek = Convert.ToInt32(m_weekList[i]);
                        if (checkWeek > todayWeek)
                        {
                            addDays = checkWeek - todayWeek;
                            break;
                        }
                    }
                    if (addDays == 0)
                        addDays = Convert.ToInt32(m_weekList[0]) + 7 - todayWeek;
                    dtNow = dtNow.AddDays(addDays);
                }

                DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, dayTime.Hour, dayTime.Min, dayTime.Sec);
                int curActiveTimeStart = ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime);
                m_curActiveTimeList.Add(curActiveTimeStart);
                curActiveTimeStart += dayTime.Duration;
                m_curActiveTimeList.Add(curActiveTimeStart);
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }


        private List<DayOfWeek> m_weekList = new List<DayOfWeek>();
    }

    /// <summary>
    /// 指定月日激活
    ///     格式：月-日;月-日;...|HH:mm:ss-D;......
    /// </summary>
    public class AppointDayActivateDuration : ActiveTimeDurationBase
    {
        public AppointDayActivateDuration()
            : base(eActivateTimeType.APPOINT_DAY_DURATION)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData)
        {
            string monthDayList = HTBaseFunc.DepartStr(strData, "|", 0);
            string timeList = HTBaseFunc.DepartStr(strData, "|", 1);

            m_monthDayList = ParseDayList(monthDayList);
            if (m_monthDayList == null)
                return false;

            return ParseTime(timeList);
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt)
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.DateTimeServerTimeInSecond(dt);
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);

                DayTime dayTime = null;
                bool todayIsActiveDay = false;
                for (int i = 0; i < m_monthDayList.Count; ++i)
                {
                    if (m_monthDayList[i].Month == dtNow.Month && m_monthDayList[i].Day == dtNow.Day)
                    {
                        todayIsActiveDay = true;
                        break;
                    }
                }
                if (todayIsActiveDay == true)
                {
                    for (int i = 0; i < m_activeTimeList.Count; ++i)
                    {
                        if (m_activeTimeList[i].TotalSec <= secSpend && secSpend <= m_activeTimeList[i].TotalSec + m_activeTimeList[i].Duration)
                        {
                            dayTime = m_activeTimeList[i];
                            break;
                        }
                    }
                }
                if (dayTime == null)
                {
                    dayTime = m_activeTimeList[0];
                    int newMonth = -1, newDay = -1, newYear = 0;
                    for (int i = 0; i < m_monthDayList.Count; ++i)
                    {
                        if (m_monthDayList[i].Month > dtNow.Month ||
                                (m_monthDayList[i].Month == dtNow.Month && m_monthDayList[i].Day > dtNow.Day)
                            )
                        {
                            newYear = dtNow.Year;
                            newMonth = m_monthDayList[i].Month;
                            newDay = m_monthDayList[i].Day;
                            break;
                        }
                    }
                    if (newMonth == -1)
                    {
                        newYear = dtNow.Year + 1;
                        newMonth = m_monthDayList[0].Month;
                        newDay = m_monthDayList[0].Day;
                    }

                    dtNow = new DateTime(newYear, newMonth, newDay);
                }
                DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, dayTime.Hour, dayTime.Min, dayTime.Sec);
                int startTime = ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime);
                m_curActiveTimeList.Add(startTime);
                startTime += dayTime.Duration;
                m_curActiveTimeList.Add(startTime);
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }

        private List<MonthDay> m_monthDayList = new List<MonthDay>();
    }

    /// <summary>
    /// 指定月日范围激活
    ///     格式：月-日,月-日;月-日,月-日;...|HH:mm:ss,...;HH:mm:ss,...;...
    /// </summary>
    public class AppointDayRangeDuration : ActiveTimeDurationBase
    {
        public AppointDayRangeDuration()
            : base(eActivateTimeType.APPOINT_DAY_RANGE_DURATION)
        {

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="strData"></param>
        /// <returns></returns>
        public override bool InitData(string strData)
        {
            string monthDayList = HTBaseFunc.DepartStr(strData, "|", 0);
            string timeList = HTBaseFunc.DepartStr(strData, "|", 1);

            int groupIndex = 0;
            bool result = true;
            while (true && groupIndex < 100)
            {
                string monthDayGroupStr = HTBaseFunc.DepartStr(monthDayList, ";", groupIndex);
                if (monthDayGroupStr == "")
                    break;

                string startDayStr = HTBaseFunc.DepartStr(monthDayGroupStr, ",", 0);
                string endDayStr = HTBaseFunc.DepartStr(monthDayGroupStr, ",", 1);
                MonthDay startDay = ParseDay(startDayStr);
                if (startDay == null)
                {
                    result = false;
                    break;
                }
                MonthDay endDay = ParseDay(endDayStr);
                if (endDay == null)
                {
                    result = false;
                    break;
                }

                m_monthDayRangeList.Add(new MonthDayRange(startDay, endDay));

                ++groupIndex;
            }
            if (result == false)
            {
                m_monthDayRangeList.Clear();
                return false;
            }

            if (m_monthDayRangeList.Count == 0)
                return false;

            return ParseTime(timeList);
        }

        /// <summary>
        /// 检测现在的激活阶段
        /// </summary>
        /// <returns></returns>
        public override int CheckActiveStageNow(DateTime dt)
        {
            int timeListCount = m_curActiveTimeList.Count;
            int curServerTime = SvrCommCfg.CurrentServerTimeInSecond;
            if (m_curActiveTimeList.Count == 0 || m_curActiveTimeList[timeListCount - 1] <= curServerTime)
            {
                //重新计算下一个
                m_curActiveTimeList.Clear();
                DateTime dtNow = dt;
                int secSpend = SecSpendToday(dtNow);

                MonthDayRange monthDayRange = null;
                for (int i = 0; i < m_monthDayRangeList.Count; ++i)
                {
                    if (m_monthDayRangeList[i].CheckDayInRange(dtNow.Month, dtNow.Day))
                    {
                        monthDayRange = m_monthDayRangeList[i];
                        break;
                    }
                }

                DayTime dayTime = null;
                if (monthDayRange != null)
                {
                    for (int i = 0; i < m_activeTimeList.Count; ++i)
                    {
                        if (m_activeTimeList[i].TotalSec <= secSpend && secSpend <= m_activeTimeList[i].TotalSec + m_activeTimeList[i].Duration)
                        {
                            dayTime = m_activeTimeList[i];
                            break;
                        }
                    }
                }
                if (dayTime == null)
                {
                    dayTime = m_activeTimeList[0];

                    bool finded = false;
                    if (monthDayRange != null)
                    {
                        dtNow = dtNow.AddDays(1);
                        if (monthDayRange.CheckDayInRange(dtNow.Month, dtNow.Day) == false)
                        {
                            for (int i = 0; i < m_monthDayRangeList.Count; ++i)
                            {
                                if (m_monthDayRangeList[i].StartDay.Month > monthDayRange.EndDay.Month ||
                                    (m_monthDayRangeList[i].StartDay.Month == monthDayRange.EndDay.Month && m_monthDayRangeList[i].StartDay.Day > monthDayRange.EndDay.Day))
                                {
                                    monthDayRange = m_monthDayRangeList[i];
                                    dtNow = new DateTime(dtNow.Year, monthDayRange.StartDay.Month, monthDayRange.StartDay.Day);
                                    finded = true;
                                    break;
                                }
                            }
                        }
                        else
                            finded = true;
                    }
                    if (finded == false)
                        dtNow = new DateTime(dtNow.Year + 1, m_monthDayRangeList[0].StartDay.Month, m_monthDayRangeList[0].StartDay.Day);

                }
                DateTime tmpTime = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, dayTime.Hour, dayTime.Min, dayTime.Sec);
                int startTime = ServerCommon.SvrCommCfg.DateTimeServerTimeInSecond(tmpTime);
                m_curActiveTimeList.Add(startTime);
                startTime += dayTime.Duration;
                m_curActiveTimeList.Add(startTime);
            }

            for (int i = 0; i < m_curActiveTimeList.Count - 1; ++i)
            {
                if (m_curActiveTimeList[i] <= curServerTime && curServerTime <= m_curActiveTimeList[i + 1])
                    return i + 1;
            }

            return 0;
        }

        //月日范围列表
        private List<MonthDayRange> m_monthDayRangeList = new List<MonthDayRange>();
    }

    /// <summary>
    /// 激活时间生成
    /// </summary>
    public class ActiveTimeMaker
    {
        public static ActiveTimeMaker Instance
        {
            get
            {
                return _instance;
            }
        }

        public IActiveTimeOperater Make(eActivateTimeType type, string timeData)
        {
            IActiveTimeOperater activeTimeOperater = null;
            switch (type)
            {
                case eActivateTimeType.APPOINT_DAY:
                    {
                        AppointDayActivate dayAppoint = new AppointDayActivate();
                        dayAppoint.InitData(timeData);
                        activeTimeOperater = dayAppoint;
                    }
                    break;
                case eActivateTimeType.APPOINT_DAY_RANGE:
                    {
                        AppointDayRange dayAppointRange = new AppointDayRange();
                        dayAppointRange.InitData(timeData);
                        activeTimeOperater = dayAppointRange;
                    }
                    break;
                case eActivateTimeType.APPOINT_WEEK:
                    {
                        AppointWeekActivate weekAppoint = new AppointWeekActivate();
                        weekAppoint.InitData(timeData);
                        activeTimeOperater = weekAppoint;
                    }
                    break;
                case eActivateTimeType.EVERY_DAY:
                    {
                        DayActivate day = new DayActivate();
                        day.InitData(timeData);
                        activeTimeOperater = day;
                    }
                    break;
                case eActivateTimeType.APPOINT_DAY_DURATION:
                    {
                        DayActivateDuration dayDuration = new DayActivateDuration();
                        dayDuration.InitData(timeData);
                        activeTimeOperater = dayDuration;
                    }
                    break;
                case eActivateTimeType.APPOINT_DAY_RANGE_DURATION:
                    {
                        AppointDayRangeDuration adrd = new AppointDayRangeDuration();
                        adrd.InitData(timeData);
                        activeTimeOperater = adrd;
                    }
                    break;
                case eActivateTimeType.APPOINT_WEEK_DURATION:
                    {
                        AppointWeekActivateDuration awad = new AppointWeekActivateDuration();
                        awad.InitData(timeData);
                        activeTimeOperater = awad;
                    }
                    break;
                case eActivateTimeType.EVERY_DAY_DURATION:
                    {
                        DayActivateDuration dad = new DayActivateDuration();
                        dad.InitData(timeData);
                        activeTimeOperater = dad;
                    }
                    break;
            }
            return activeTimeOperater;
        }

        private static ActiveTimeMaker _instance = new ActiveTimeMaker();
        private ActiveTimeMaker() { }
    }
}
