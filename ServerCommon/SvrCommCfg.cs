using System;
using GsTechLib;
using System.Collections.Generic;
using com.tieao.mmo.interval;

namespace ServerCommon
{
    //服务通用配置
    public class SvrCommCfg
    {
        public static SvrCommCfg Instance
        {
            get
            {
                return _instance;
            }
        }

        public string m_Envirment;

        //服务版本信息
        public string VersionFlage = "0.0.1";

        public int ProcessID = 0;

        /// <summary>
        /// 时间的字符串格式
        /// </summary>
        public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        //数据库回调处理间隔，单位ms
        public int DbAccessPoolCallbackDealInterval = 10;

        //服务器基准时间
        public static DateTime ServerTimeZero = new DateTime(2014, 1, 1);  

        //服务器相对于基准时间
        public static long CurrentServerTimeTick      
        {
            get
            {
                return (DateTime.Now - ServerTimeZero).Ticks;
            }
        }

        //调试增加时间
        private static int m_debugAddTime = 0;
        public static int DebugAddTime      
        {
            get
            {
                return m_debugAddTime;
            }
            set
            {
                if (m_debugAddTime < value)
                    m_debugAddTime = value;
            }
        }

        //服务器上大部分的逻辑都以秒为单位
        public static int CurrentServerTimeInSecond        
        {
            get
            {
                return Convert.ToInt32(CurrentServerTimeTick / (10000 * 1000) + DebugAddTime);
            }
        }

        //服务器时间，精确到毫秒
        public static double CurrentServerTimeInMicrotime
        {
            get 
            {
                return (CurrentServerTimeTick / (10000) + DebugAddTime) / 1000.0f;
            }
        }

        public static long CurrentServerTimeInTick
        {
            get
            {
                return CurrentServerTimeTick;
            }
        }

        //时间对应服务器时间
        public static int DateTimeServerTimeInSecond(DateTime dt)
        {
            return Convert.ToInt32((dt - ServerTimeZero).Ticks / (10000 * 1000));
        }

        public static DateTime SysDateTimeNow()
        {
            return DateTime.Now.AddSeconds(DebugAddTime);
        }

        /// <summary>
        /// 当天开始时间
        /// </summary>
        /// <param name="offsetSec"></param>
        /// <returns></returns>
        public static int CurDayBeginTimeInSecend(int offsetSec)
        {
            DateTime now = DateTime.Now.AddSeconds(DebugAddTime);
            DateTime dayStart = new DateTime(now.Year, now.Month, now.Day);
            dayStart = dayStart.AddSeconds(offsetSec);

            int dayStartSec = Convert.ToInt32((dayStart - ServerTimeZero).Ticks / (10000 * 1000));
            return dayStartSec;
        }

        /// <summary>
        /// 当前星期某天开始时间
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static int CurWeekDayBeginTimeInSecend(DayOfWeek day, int offsetSec)
        {
            DateTime now = DateTime.Now.AddSeconds(DebugAddTime);
            DateTime dayStart = new DateTime(now.Year, now.Month, now.Day);
            dayStart = dayStart.AddSeconds(offsetSec);
            int dayStartSec = Convert.ToInt32((dayStart - ServerTimeZero).Ticks / (10000 * 1000));

            dayStartSec -= ((7 + Convert.ToInt32(now.DayOfWeek) - Convert.ToInt32(day)) % 7) * 3600 * 24;
            return dayStartSec;
        }

        //明天开始时间
        public static int NextDayBeginTimeInSecend(int offsetSec)
        {
            DateTime now = DateTime.Now.AddSeconds(DebugAddTime);
            DateTime dayStart = new DateTime(now.Year, now.Month, now.Day);
            dayStart.AddDays(1);
            dayStart = dayStart.AddSeconds(offsetSec);

            int dayStartSec = Convert.ToInt32((dayStart - ServerTimeZero).Ticks / (10000 * 1000));
            if (dayStartSec < CurrentServerTimeInSecond)
                dayStartSec += 24 * 3600;
            return dayStartSec;
        }

        //秒数对应于日期时间
        public static DateTime ServerTimeSecondToDataTime(int second)
        {
            return ServerTimeZero.AddSeconds(second);
        }


        public const long PlayeridLimit = 10000000000;

        //服务信息
        public PtServerInfo ServerInfo = new PtServerInfo();


        public int Index = 0;

        public string MoniteorNodeIP = "";
        public int MoniteorNodePort = 0;

        public Guid ZeroGuid = new Guid();      //空的GUID，全是0
        public bool CShutDownMode = true;       //是否C关闭模式，C关闭模式是指由CS发送指令关闭的模式

        public static bool LogSend = false;     //日志发送开关

        /// <summary>
        /// 心动时间戳
        /// </summary>
        public static Timestamp XDTimestamp { get { return s_logTimestamp; } }

        /// <summary>
        /// 狂刀时间戳转为心动时间戳
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static int TimeStampConvert2XD(int timeStamp)
        {
            if (timeStamp == 0)
                return 0;

            return timeStamp + (int)(ServerTimeZero - XDTimestamp.ServerTimeZero).TotalSeconds;
        }

        /// <summary>
        /// 心动时间戳转为狂刀时间戳
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static int TimeStampConvert2KD(int timeStamp)
        {
            if (timeStamp == 0)
                return 0;

            return timeStamp - (int)(ServerTimeZero - XDTimestamp.ServerTimeZero).TotalSeconds;
        }

        static Timestamp s_logTimestamp = new Timestamp(DateTime.Parse("1970-1-1 8:00:00"));

        private SvrCommCfg() { }
        private static SvrCommCfg _instance = new SvrCommCfg();
    }
}
