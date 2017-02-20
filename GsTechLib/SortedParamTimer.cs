using System;
using System.Collections;

namespace GsTechLib
{
    /// <summary>
    /// 有序参数化定时器
    /// </summary>
    public class SortedParamTimer
    {
        #region Class Init

        public delegate void ParamTimerCallback(int times, long elapsed, long TimerID, object param);

        static SortedParamTimer _instance = new SortedParamTimer();
        static IGsLogger _logger = null;
        static long TimerID = 0;
        int _precision = 10;
        SortedList _timerPool = new SortedList();//以TimerID为Key存储ParamTimerData
        SortedList _sortedTimerPool = new SortedList();//以到达时间为Key存储ArrayList(以ParamTimerData为元素)
        SortedList _addList = new SortedList();//以TimerID为Key存储ParamTimerData
        ArrayList _delList = new ArrayList();
        class ParamTimerData
        {
            public long id;
            public ParamTimerCallback cb;
            public long interval;
            public DateTime lastTime;
            public DateTime timeOut;
            public int totalTimes;
            public int times;
            public object param;
        }

        public static SortedParamTimer GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        /// <param name="logger">日志打印接口</param>
        public static void Init(IGsLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取当前时间加上addInterval以后的时间
        /// </summary>
        /// <param name="addInterval">增加间隔，毫秒级</param>
        /// <returns>返回当前时间加上addInterval以后的时间</returns>
        public static DateTime GetTime(long addInterval)
        {
            return HTBaseFunc.GetTime(addInterval);
        }

        /// <summary>
        /// 获取2个时间之间的间隔
        /// </summary>
        /// <param name="minuendTime">被减的时间</param>
        /// <param name="subtrahendTime">减的时间</param>
        /// <returns>返回间隔</returns>
        public long GetInterval(DateTime minuendTime, DateTime subtrahendTime)
        {
            return HTBaseFunc.GetInterval(minuendTime, subtrahendTime);
        }

        #endregion

        #region Timer Maintenance

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="cb">回调函数</param>
        /// <param name="interval">间隔ms</param>
        /// <param name="times">调用次数，-1表示一直循环</param>
        /// <param name="param">回调参数</param>
        /// <returns>返回定时器ID</returns>
        public long AddTimer(ParamTimerCallback cb, long interval, int times, object param)
        {
            lock (this)
            {
                if (interval < _precision)
                    interval = _precision;
                ParamTimerData data = new ParamTimerData();
                data.id = ++TimerID;
                data.interval = interval;
                data.lastTime = GetTime(0);
                data.timeOut = GetTime(interval);
                data.times = 0;
                data.totalTimes = times;
                data.cb = cb;
                data.param = param;
                _addList.Add(data.id, data);
                return data.id;
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="cb">回调函数(不带参数的回调)</param>
        /// <param name="interval">间隔ms</param>
        /// <param name="times">调用次数，-1表示一直循环</param>
        /// <returns>返回定时器ID</returns>
        public long AddTimer(ParamTimerCallback cb, long interval, int times)
        {
            return AddTimer(cb, interval, times, null);
        }

        /// <summary>
        /// 添加定时器(仅调用一次)
        /// </summary>
        /// <param name="cb">回调函数(不带参数的回调)</param>
        /// <param name="interval">间隔ms</param>
        /// <returns>返回定时器ID</returns>
        public long AddTimer(ParamTimerCallback cb, long interval)
        {
            return AddTimer(cb, interval, 1);
        }

        /// <summary>
        /// 添加定时器(仅调用一次)
        /// </summary>
        /// <param name="cb">回调函数</param>
        /// <param name="interval">间隔ms</param>
        /// <param name="param">回调参数</param>
        /// <returns>返回定时器ID</returns>
        /*
        public long AddTimer(ParamTimerCallback cb, long interval, object param)
        {
            return AddTimer(cb, interval, 1, param);
        }
        */

        /// <summary>
        /// 添加定时器(仅调用一次)
        /// </summary>
        /// <param name="cb">回调函数</param>
        /// <param name="tiggerTime">调用时间</param>
        /// <param name="param">回调参数</param>
        /// <returns>返回定时器ID</returns>
        public long AddTimer(ParamTimerCallback cb, DateTime tiggerTime, object param)
        {
            long interval = (long)((TimeSpan)(tiggerTime - DateTime.Now)).TotalMilliseconds;
            if (interval >= 0)
                return AddTimer(cb, interval, 1, param);
            else
                return AddTimer(cb, 0, 1, param);
        }

        /// <summary>
        /// 添加定时器(仅调用一次)
        /// </summary>
        /// <param name="cb">回调函数</param>
        /// <param name="tiggerTime">调用时间</param>
        /// <returns>返回定时器ID</returns>
        public long AddTimer(ParamTimerCallback cb, DateTime tiggerTime)
        {
            return AddTimer(cb, tiggerTime, null);
        }

        /// <summary>
        /// 删除定时器
        /// </summary>
        /// <param name="timerID">定时器ID</param>
        public void KillTimer(long timerID)
        {
            lock (this)
            {
                if (_addList.ContainsKey(timerID))
                    _addList.Remove(timerID);
                _delList.Add(timerID);
            }
        }

        /// <summary>
        /// 获取定时器触发的剩余时间ms
        /// </summary>
        /// <param name="timerID">定时器ID</param>
        /// <returns>返回剩余时间，毫秒级</returns>
        public long GetTimerTime(long timerID)
        {
            lock (this)
            {
                if (_addList.ContainsKey(timerID))
                {
                    ParamTimerData t = (ParamTimerData)_addList[timerID];
                    return GetInterval(t.timeOut, GetTime(0));
                }
                if (_timerPool.Contains(timerID))
                {
                    ParamTimerData t = (ParamTimerData)_timerPool[timerID];
                    return GetInterval(t.timeOut, GetTime(0));
                }
                return 0;
            }
        }

        /// <summary>
        /// 调整定时器的触发时
        /// </summary>
        /// <param name="timerID">定时器ID</param>
        /// <param name="prolongTime">调整的时间长度，毫秒级</param>
        public void ProlongTimer(long timerID, long prolongTime)
        {
            lock (this)
            {
                if (_addList.ContainsKey(timerID))
                {
                    ParamTimerData t = (ParamTimerData)_addList[timerID];
                    t.timeOut = t.timeOut.AddMilliseconds(prolongTime);
                    return;
                }

                if (_timerPool.Contains(timerID))
                {
                    ParamTimerData t = (ParamTimerData)_timerPool[timerID];
                    DateTime oldTimeOut = t.timeOut;
                    t.timeOut = t.timeOut.AddMilliseconds(prolongTime);

                    if (oldTimeOut != t.timeOut)
                    {
                        ArrayList timerList = (ArrayList)_sortedTimerPool[oldTimeOut];
                        if (timerList != null)
                        {
                            for (int i = timerList.Count - 1; i >= 0; i--)
                            {
                                if (((ParamTimerData)timerList[i]).id == timerID)
                                    timerList.RemoveAt(i);
                            }
                            if (timerList.Count == 0)
                                _sortedTimerPool.Remove(oldTimeOut);
                        }

                        timerList = (ArrayList)_sortedTimerPool[t.timeOut];
                        if (timerList == null)
                        {
                            timerList = new ArrayList();
                            _sortedTimerPool.Add(t.timeOut, timerList);
                        }
                        timerList.Insert(0, t);//倒着插入
                    }

                    return;
                }
            }
        }

        /// <summary>
        /// 调整定时器的触发时
        /// </summary>
        /// <param name="timerID">定时器ID</param>
        /// <param name="newTimeOut">新的时间</param>
        public void ProlongTimer(long timerID, DateTime newTimeOut)
        {
            lock (this)
            {
                if (_addList.ContainsKey(timerID))
                {
                    ParamTimerData t = (ParamTimerData)_addList[timerID];
                    t.timeOut = newTimeOut;
                    return;
                }

                if (_timerPool.Contains(timerID))
                {
                    ParamTimerData t = (ParamTimerData)_timerPool[timerID];
                    DateTime oldTimeOut = t.timeOut;
                    t.timeOut = newTimeOut;

                    if (oldTimeOut != t.timeOut)
                    {
                        ArrayList timerList = (ArrayList)_sortedTimerPool[oldTimeOut];
                        if (timerList != null)
                        {
                            for (int i = timerList.Count - 1; i >= 0; i--)
                            {
                                if (((ParamTimerData)timerList[i]).id == timerID)
                                    timerList.RemoveAt(i);
                            }
                            if (timerList.Count == 0)
                                _sortedTimerPool.Remove(oldTimeOut);
                        }

                        timerList = (ArrayList)_sortedTimerPool[t.timeOut];
                        if (timerList == null)
                        {
                            timerList = new ArrayList();
                            _sortedTimerPool.Add(t.timeOut, timerList);
                        }
                        timerList.Insert(0, t);//倒着插入
                    }

                    return;
                }
            }
        }

        #endregion

        #region Schedule

        public delegate void PrintMemoryFluxDele(string place);

        /// <summary>
        /// 定时器调度，有执行限制时长ms
        /// </summary>
        /// <param name="exceptions">输出异常队列</param>
        /// <param name="restrictInterval">执行限制时长，毫秒级</param>
        /// <param name="pmdel">内存统计接口</param>
        public void Schedule(ArrayList exceptions, long restrictInterval, PrintMemoryFluxDele pmdel)
        {
            lock (this)
            {
                DateTime time = GetTime(0);

                foreach (ParamTimerData t in _addList.Values)
                {
                    _timerPool.Add(t.id, t);

                    ArrayList timerList = (ArrayList)_sortedTimerPool[t.timeOut];
                    if (timerList == null)
                    {
                        timerList = new ArrayList();
                        _sortedTimerPool.Add(t.timeOut, timerList);
                    }
                    timerList.Insert(0, t);//倒着插入
                }
                _addList.Clear();

                foreach (long timerID in _delList)
                {
                    ParamTimerData t = (ParamTimerData)_timerPool[timerID];
                    if (t != null)
                    {
                        _timerPool.Remove(timerID);

                        ArrayList timerList = (ArrayList)_sortedTimerPool[t.timeOut];
                        if (timerList != null)
                        {
                            for (int i = timerList.Count - 1; i >= 0; i--)
                            {
                                if (((ParamTimerData)timerList[i]).id == timerID)
                                    timerList.RemoveAt(i);
                            }
                            if (timerList.Count == 0)
                                _sortedTimerPool.Remove(t.timeOut);
                        }
                    }
                }
                _delList.Clear();

                bool bTimerOut = false;
                ArrayList delTimerList = new ArrayList();
                foreach(DateTime timeOut in _sortedTimerPool.Keys)
                {
                    if (timeOut <= time)
                    {
                        ArrayList timerList = (ArrayList)_sortedTimerPool[timeOut];
                        if (timerList == null || timerList.Count <= 0)
                        {
                            delTimerList.Add(timeOut);
                        }
                        else
                        {
                            for (int i = timerList.Count - 1; i >= 0; i--)
                            {
                                ParamTimerData t = (ParamTimerData)timerList[i];
                                
                                try
                                {
                                    if (pmdel != null)
                                        pmdel("SortedParamTimer.Before." + t.cb.Method.Name);
                                    if (t == null || t.cb == null)
                                        SvLogger.Info("Schedule Exception : ID={0}, LastTime={1}, TimeOut={2}.", 
                                                                            t == null ? -1 : t.id, 
                                                                            t == null ? "1900-01-01 00:00:00" : t.lastTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                            t == null ? "1900-01-01 00:00:00" : t.timeOut.ToString("yyyy-MM-dd HH:mm:ss"));
                                    t.cb(t.times++, GetInterval(time, t.lastTime), t.id, t.param);

                                    if (pmdel != null)
                                        pmdel("SortedParamTimer.After." + t.cb.Method.Name);
                                }
                                catch (Exception ex)
                                {
                                    if (exceptions != null)
                                        exceptions.Add(ex);
                                }
                                
                                t.lastTime = time;
                                t.timeOut = time.AddMilliseconds(-(GetInterval(time, t.timeOut) % t.interval) + t.interval);

                                timerList.RemoveAt(i);
                                _timerPool.Remove(t.id);

                                if (t.times < t.totalTimes || t.totalTimes == -1)
                                    _addList.Add(t.id, t);

                                if (restrictInterval > 0 && time.AddMilliseconds(restrictInterval) <= GetTime(0))
                                {
                                    bTimerOut = true;
                                    break;
                                }
                            }

                            if (timerList.Count == 0)
                                delTimerList.Add(timeOut);
                        }

                        if (bTimerOut)
                            break;
                    }
                    else
                        break;
                }
                foreach (DateTime timeOut in delTimerList)
                {
                    _sortedTimerPool.Remove(timeOut);
                }

            }
        }

        /// <summary>
        /// 定时器调度
        /// </summary>
        /// <param name="exceptions">输出异常队列</param>
        /// <param name="pmdel">内存统计接口</param>
        public void Schedule(ArrayList exceptions, PrintMemoryFluxDele pmdel)
        {
            Schedule(exceptions, 0, pmdel);
        }

        /// <summary>
        /// 定时器调度，有执行限制时长ms
        /// </summary>
        /// <param name="restrictInterval">执行限制时长，毫秒级</param>
        public void Schedule(int restrictInterval)
        {
            Schedule(null, restrictInterval, null);
        }

        /// <summary>
        /// 定时器调度
        /// </summary>
        public void Schedule()
        {
            Schedule(0);
        }

        #endregion

    }
}