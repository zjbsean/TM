using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerCommon
{
    public class DailyFlashTime
    {
        public DailyFlashTime(int offsetTime)
        {
            m_curFlashTime = ServerCommon.SvrCommCfg.CurDayBeginTimeInSecend(offsetTime);
            m_nextFlashTime = m_curFlashTime + 24 * 3600;
        }

        public bool CheckNeedFlash(ref int lastFlashTime)
        {
            int curSvrTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;
            if(curSvrTime > m_nextFlashTime)
            {
                m_curFlashTime = m_nextFlashTime;
                m_nextFlashTime = m_curFlashTime + 24 * 3600;
            }
            bool isNeedFlash = false;
            if (curSvrTime >= m_curFlashTime && lastFlashTime <= m_curFlashTime)
            {
                isNeedFlash = true;
                lastFlashTime = curSvrTime;
            }
            return isNeedFlash;
        }

        private int m_curFlashTime;
        private int m_nextFlashTime;
    }

    public class DailyFlashTimeManager
    {
        public static DailyFlashTimeManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public bool CheckNeedFlash(int offsetTime, ref int lastFlashTime)
        {
            DailyFlashTime ft;
            if (m_flashTimeDic.TryGetValue(offsetTime, out ft) == false)
            {
                ft = new DailyFlashTime(offsetTime);
                m_flashTimeDic.Add(offsetTime, ft);
            }
            return ft.CheckNeedFlash(ref lastFlashTime);
        }

        private Dictionary<int, DailyFlashTime> m_flashTimeDic = new Dictionary<int, DailyFlashTime>();
        private static DailyFlashTimeManager m_instance = new DailyFlashTimeManager();
        private DailyFlashTimeManager() { }
    }

    public class WeeklyFlashTime
    {
        public WeeklyFlashTime(DayOfWeek startWeek, int offsetTime)
        {
            DayOfWeek curWeek = DateTime.Now.DayOfWeek;

            m_curFlashTime = ServerCommon.SvrCommCfg.CurDayBeginTimeInSecend(offsetTime);
            int disDay = Math.Abs(curWeek - startWeek);
            m_curFlashTime -= disDay * 24 * 3600;
            m_nextFlashTime = m_curFlashTime + 7 * 24 * 3600;
        }

        public bool CheckNeedFlash(ref int lastFlashTime)
        {
            int curSvrTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;
            if (curSvrTime > m_nextFlashTime)
            {
                m_curFlashTime = m_nextFlashTime;
                m_nextFlashTime = m_curFlashTime + 7 * 24 * 3600;
            }
            bool isNeedFlash = false;
            if (lastFlashTime <= m_curFlashTime)
            {
                isNeedFlash = true;
                lastFlashTime = curSvrTime;
            }
            return isNeedFlash;
        }

        private int m_curFlashTime;
        private int m_nextFlashTime;
    }

    public class WeeklyFlashTimeManager
    {
        public static WeeklyFlashTimeManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public bool CheckNeedFlash(DayOfWeek week, int offsetTime, ref int lastFlashTime)
        {
            Dictionary<int, WeeklyFlashTime> ftDic;
            WeeklyFlashTime wft;
            if (m_flashTimeDic.TryGetValue(week, out ftDic) == false)
            {
                Dictionary<int, WeeklyFlashTime> flashDic = new Dictionary<int, WeeklyFlashTime>();
                wft = new WeeklyFlashTime(week, offsetTime);
                flashDic.Add(offsetTime, wft);
                m_flashTimeDic.Add(week, flashDic);
            }
            else
            {
                if (ftDic.TryGetValue(offsetTime, out wft) == false)
                {
                    wft = new WeeklyFlashTime(week, offsetTime);
                    ftDic.Add(offsetTime, wft);
                }
            }
            return wft.CheckNeedFlash(ref lastFlashTime);
        }

        private Dictionary<DayOfWeek, Dictionary<int, WeeklyFlashTime>> m_flashTimeDic = new Dictionary<DayOfWeek, Dictionary<int, WeeklyFlashTime>>();
        private static WeeklyFlashTimeManager m_instance = new WeeklyFlashTimeManager();
        private WeeklyFlashTimeManager() { }
    }
}
