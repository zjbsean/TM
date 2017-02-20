using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ServerCommon
{
    /// <summary>
    /// 定时检测
    /// </summary>
    public class TimingDetection
    {
        public delegate void TimingDetectionDelegate(int curSvrTime);

        public TimingDetection(int intervalSec, bool isDoItWhenStart)
        {
            m_intervalSec = intervalSec;

            m_lastCheckTime = 0;
        }

        public int IntervalSec
        {
            get
            {
                return m_intervalSec;
            }
        }

        public void AddEvent(TimingDetectionDelegate dealFunc)
        {
            m_intervalDealEvent += dealFunc;
        }

        public void RemoveEvent(TimingDetectionDelegate dealFunc)
        {
            m_intervalDealEvent -= dealFunc;
        }

        public void Update(int curServerTime)
        {
            if(curServerTime - m_lastCheckTime >= m_intervalSec)
            {
                m_lastCheckTime = curServerTime;

                if(m_intervalDealEvent != null)
                    m_intervalDealEvent(curServerTime);
            }
        }

        private int m_intervalSec;
        private event TimingDetectionDelegate m_intervalDealEvent;
        private int m_lastCheckTime = SvrCommCfg.CurrentServerTimeInSecond;
    }

    /// <summary>
    /// 定时检测管理
    /// </summary>
    public class TimingDetectionManager
    {
        public static TimingDetectionManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        private static TimingDetectionManager m_instance = new TimingDetectionManager();
        private TimingDetectionManager() { }

        public void Clear()
        {
            m_intervalEventsDic.Clear();
        }

        public void RegistEvent(int intervalSec, TimingDetection.TimingDetectionDelegate dealFunc, bool isDoWhenStart = true)
        {
            TimingDetection td;
            if(m_intervalEventsDic.TryGetValue(intervalSec, out td) == false)
            {
                td = new TimingDetection(intervalSec, isDoWhenStart);
                m_intervalEventsDic.Add(intervalSec, td);
            }
            td.AddEvent(dealFunc);
        }

        public void Update()
        {
            m_checkEndTicket = Environment.TickCount;
            m_checkSpendTicket = (m_checkEndTicket >= m_checkStartTicket ? m_checkEndTicket - m_checkStartTicket : (int.MaxValue + m_checkEndTicket) + (int.MaxValue - m_checkStartTicket));
            if (m_checkSpendTicket >= 1000)
            {
                m_checkStartTicket = m_checkEndTicket;
                int curServerTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;

                foreach(TimingDetection td in m_intervalEventsDic.Values)
                    td.Update(curServerTime);
            }
        }

        private int m_checkSpendTicket = 0;
        private int m_checkStartTicket = Environment.TickCount;
        private int m_checkEndTicket = 0;

        private Dictionary<int, TimingDetection> m_intervalEventsDic = new Dictionary<int, TimingDetection>();
    }
}
