using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public class Timestamp
    {
        public Timestamp(DateTime serverTimeZero,int debugAddTime = 0)
        {
            m_serverTimeZero = serverTimeZero;
            m_debugAddTime = debugAddTime;
        }

        //服务器上大部分的逻辑都以秒为单位
        public int CurrentServerTimeInSecond
        {
            get
            {
                return Convert.ToInt32(CurrentServerTimeTick / (10000 * 1000) + m_debugAddTime);
            }
        }

        //服务器时间，精确到毫秒
        public double CurrentServerTimeInMicrotime
        {
            get
            {
                return (CurrentServerTimeTick / (10000) + m_debugAddTime) / 1000.0f;
            }
        }


        //服务器相对于基准时间
        public long CurrentServerTimeTick
        {
            get
            {
                return (DateTime.Now - m_serverTimeZero).Ticks;
            }
        }

        /// <summary>
        /// 获取某个时间点的时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int GetTimeInSecond(DateTime time)
        {
            return (int)(time - ServerTimeZero).TotalSeconds;
        }

        /// <summary>
        /// 时间戳起点
        /// </summary>
        public DateTime ServerTimeZero
        {
            get { return m_serverTimeZero; }
        }

        //调试增加时间
        int m_debugAddTime = 0;
        //服务器基准时间
        DateTime m_serverTimeZero;
    }
}
