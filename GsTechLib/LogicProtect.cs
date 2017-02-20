using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GsTechLib
{
    /// <summary>
    /// 调用保护类
    /// </summary>
    public class CallProtect
    {
        private static CallProtect _instance = new CallProtect();
        public static CallProtect Instance
        {
            get
            {
                return _instance;
            }
        }

        //频繁调用保护
        private Hashtable _callProtectList = new Hashtable();//以campid为key存储Hashtable(以操作名为key存储上次操作时间)

        /// <summary>
        /// 添加调用保护
        /// </summary>
        /// <param name="campid">匹配ID</param>
        /// <param name="calledOper">调用操作</param>
        public void AddCall(string campid, string calledOper)
        {
            Hashtable callHash = (Hashtable)_callProtectList[campid];
            if (callHash == null)
            {
                callHash = new Hashtable();
                _callProtectList.Add(campid, callHash);
            }

            callHash[calledOper] = HTBaseFunc.GetTime(0);
        }

        /// <summary>
        /// 删除调用保护
        /// </summary>
        /// <param name="campid">匹配ID</param>
        /// <param name="calledOper">调用操作</param>
        public void RemoveCall(string campid, string calledOper)
        {
            Hashtable callHash = (Hashtable)_callProtectList[campid];
            if (callHash != null)
            {
                callHash.Remove(calledOper);

                if (callHash.Count == 0)
                    _callProtectList.Remove(campid);
            }
        }

        /// <summary>
        /// 检查调用保护，返回剩余秒数
        /// </summary>
        /// <param name="campid">匹配ID</param>
        /// <param name="calledOper">调用操作</param>
        /// <param name="allowInterval">允许的间隔，毫秒级</param>
        /// <returns>返回剩余的间隔</returns>
        public int CheckCall(string campid, string calledOper, int allowInterval)
        {
            Hashtable callHash = (Hashtable)_callProtectList[campid];
            if (callHash == null)
                return 0;
            else
            {
                if (callHash.Contains(calledOper))
                {
                    DateTime endCall = HTBaseFunc.NullToDateTime(callHash[calledOper]).AddMilliseconds(allowInterval);
                    DateTime dtNow = HTBaseFunc.GetTime(0);
                    if (endCall > dtNow)
                    {
                        TimeSpan ts = (TimeSpan)(endCall - dtNow);
                        return (int)ts.TotalMilliseconds;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

    }

    /// <summary>
    /// 状态保护类
    /// </summary>
    public class StatusProtect
    {
        private static StatusProtect _instance = new StatusProtect();
        public static StatusProtect Instance
        {
            get
            {
                return _instance;
            }
        }

        //状态保护
        private Hashtable _statusProtectList = new Hashtable();//以campid为key存储StatusData

        /// <summary>
        /// 添加状态保护
        /// </summary>
        /// <param name="campid">匹配ID</param>
        /// <param name="calledOper">调用操作</param>
        /// <param name="callTime">调用时间</param>
        /// <returns>如果有保护则返回前一操作名</returns>
        private string AddStatusProtect(string campid, string calledOper, DateTime callTime)
        {
            lock (_statusProtectList)
            {
                StatusData lastStatusData = (StatusData)_statusProtectList[campid];
                if (lastStatusData == null)
                {
                    _statusProtectList[campid] = new StatusData(campid, calledOper, callTime);
                    return "";
                }
                else
                {
                    return lastStatusData.CalledOper;
                }
            }
        }

        public string AddStatusProtect(string campid, string calledOper)
        {
            return AddStatusProtect(campid, calledOper, HTBaseFunc.GetTime(0));
        }

        /// <summary>
        /// 删除状态保护
        /// </summary>
        /// <param name="sessionID">匹配ID</param>
        public void DelStatusProtect(string campid)
        {
            lock (_statusProtectList)
            {
                _statusProtectList.Remove(campid);
            }
        }

        class StatusData
        {
            public StatusData(string campid, string calledOper, DateTime callTime)
            {
                CampID = campid;
                CalledOper = calledOper;
                CallTime = callTime;
            }

            public string CampID;
            public string CalledOper;
            public DateTime CallTime;
        }
    }

    /// <summary>
    /// 请求保护类
    /// </summary>
    public class RequestProtect
    {
        private static RequestProtect _instance = new RequestProtect();
        public static RequestProtect Instance
        {
            get
            {
                return _instance;
            }
        }

        //请求保护
        private long _currprotectid = 0;
        private Dictionary<int, Dictionary<string, object[]>> _requestProtectList = new Dictionary<int, Dictionary<string, object[]>>();//以userid为key存储列表（以一个操作名为Key存储参数列表，倒数第二个参数是ProtectID，最后一个参数是过期时间，这2个参数在生成该数组时保留位置）
        private Dictionary<long, object[]> _requestidlist = new Dictionary<long, object[]>();//以ProtectID为Key存储UserID和操作名组成的一个数组
        private SortedList<DateTime, List<long>> _requestaddlist = new SortedList<DateTime, List<long>>();//以记录时间为Key存储ProtectID的列表

        public RequestProtect()
        {
            SortedParamTimer.GetInstance().AddTimer(new SortedParamTimer.ParamTimerCallback(CheckTimeOut), 1 * 60 * 1000, -1, null);
        }

        private void CheckTimeOut(int times, long elapsed, long TimerID, object param)
        {
            if (_requestaddlist.Count > 0)
            {
                DateTime dtnow = HTBaseFunc.GetTime(0);

            looprpt:
                DateTime expiretime = _requestaddlist.Keys[0];
                List<long> protectidlist = _requestaddlist[expiretime];
                if (expiretime <= dtnow)
                {
                    for (int i = protectidlist.Count - 1; i >= 0; i--)
                    {
                        long pid = protectidlist[i];
                        DelRequestProtect(pid);
                    }
                    _requestaddlist.Remove(expiretime);
                    if (_requestaddlist.Count > 0)
                        goto looprpt;
                }
            }
        }

        private long GetProtectID()
        {
            long lastprotectid = _currprotectid;
            bool bfind = false;
            while (_currprotectid <= long.MaxValue)
            {
                if (_requestidlist.ContainsKey(_currprotectid) || _currprotectid == 0)
                    _currprotectid++;
                else
                {
                    bfind = true;
                    break;
                }
            }
            if (!bfind)
            {
                _currprotectid = 0;
                while (_currprotectid < lastprotectid)
                {
                    if (_requestidlist.ContainsKey(_currprotectid) || _currprotectid == 0)
                        _currprotectid++;
                    else
                    {
                        bfind = true;
                        break;
                    }
                }
            }

            if (!bfind)
            {
                DateTime expiretime = _requestaddlist.Keys[0];
                List<long> protectidlist = _requestaddlist[expiretime];
                DateTime dtnow = HTBaseFunc.GetTime(0);
                if (expiretime <= dtnow)
                {
                    long pid = protectidlist[0];
                    DelRequestProtect(pid);
                    _currprotectid = pid;
                    bfind = true;
                }
            }

            long protectid = 0;
            if (bfind)
            {
                protectid = _currprotectid;
                if (_currprotectid == int.MaxValue)
                    _currprotectid = 0;
                else
                    _currprotectid++;
            }
            return protectid;
        }

        /// <summary>
        /// 添加请求保护
        /// </summary>
        /// <param name="userid">玩家ID</param>
        /// <param name="calledOper">调用操作</param>
        /// <param name="callTime">调用时间</param>
        /// <param name="holdIntervals">保持时长，毫秒级</param>
        /// <param name="args">参数列表，注意最后面要留2个空位以内部填入保护ID和调用时间</param>
        /// <returns>成功返回大于0的ProtectID，否则返回0</returns>
        private long AddRequestProtect(int userid, string calledOper, DateTime callTime, int holdIntervals, object[] args)
        {
            long protectid = 0;
            DateTime dtnow = HTBaseFunc.GetTime(0);
            DateTime expiretime;
            if (holdIntervals < 0)
                expiretime = HTBaseFunc.NullDateTime;
            else
                expiretime = callTime.AddMilliseconds(holdIntervals);

            Dictionary<string, object[]> operlist = null;
            if (_requestProtectList.ContainsKey(userid))
                operlist = (Dictionary<string, object[]>)_requestProtectList[userid];
            else
            {
                operlist = new Dictionary<string, object[]>();
                _requestProtectList.Add(userid, operlist);
            }

            object[] arglist = null;
            if (operlist.ContainsKey(calledOper))
            {
                arglist = operlist[calledOper];
                long oldprotectid = (long)arglist[arglist.Length - 2];
                DateTime oldexpiretime = Convert.ToDateTime(arglist[arglist.Length - 1]);
                if (oldexpiretime <= dtnow && oldexpiretime > HTBaseFunc.NullDateTime)
                {
                    //删除原来过期的
                    operlist.Remove(calledOper);
                    _requestidlist.Remove(oldprotectid);
                    if (_requestaddlist.ContainsKey(oldexpiretime))
                    {
                        _requestaddlist[oldexpiretime].Remove(oldprotectid);
                        if (_requestaddlist[oldexpiretime].Count == 0)
                            _requestaddlist.Remove(oldexpiretime);
                    }

                    //添加新的
                    arglist = args;
                    protectid = GetProtectID();
                    if (protectid > 0)
                    {
                        arglist[arglist.Length - 2] = protectid;
                        arglist[arglist.Length - 1] = expiretime;
                        operlist.Add(calledOper, arglist);
                    }
                }
            }
            else
            {
                arglist = args;
                protectid = GetProtectID();
                if (protectid > 0)
                {
                    arglist[arglist.Length - 2] = protectid;
                    arglist[arglist.Length - 1] = expiretime;
                    operlist.Add(calledOper, arglist);
                }
            }

            if (operlist.Count == 0)
                _requestProtectList.Remove(userid);

            if (protectid > 0)
            {
                _requestidlist[protectid] = new object[] { userid, calledOper };

                if (expiretime > HTBaseFunc.NullDateTime)
                {
                    List<long> protectidlist = null;
                    if (_requestaddlist.ContainsKey(expiretime))
                        protectidlist = _requestaddlist[expiretime];
                    else
                    {
                        protectidlist = new List<long>();
                        _requestaddlist.Add(expiretime, protectidlist);
                    }
                    protectidlist.Add(protectid);
                }
            }

            return protectid;
        }

        /// <summary>
        /// 添加请求保护(持久保护直到删除)
        /// </summary>
        /// <param name="userid">玩家ID</param>
        /// <param name="calledOper">调用操作</param>
        /// <param name="callTime">调用时间</param>
        /// <returns>成功返回大于0的ProtectID，否则返回0</returns>
        private long AddRequestProtect(int userid, string calledOper, DateTime callTime, object[] args)
        {
            return AddRequestProtect(userid, calledOper, callTime, -1, args);
        }

        /// <summary>
        /// 添加请求保护
        /// </summary>
        /// <param name="userid">玩家ID</param>
        /// <param name="calledOper">调用操作</param>
        /// <param name="holdIntervals">保持时长，毫秒级</param>
        /// <returns>成功返回大于0的ProtectID，否则返回0</returns>
        public long AddRequestProtect(int userid, string calledOper, int holdIntervals, object[] args)
        {
            return AddRequestProtect(userid, calledOper, HTBaseFunc.GetTime(0), holdIntervals, args);
        }

        /// <summary>
        /// 添加请求保护(持久保护直到删除)
        /// </summary>
        /// <param name="userid">玩家ID</param>
        /// <param name="calledOper">调用操作</param>
        /// <returns>成功返回大于0的ProtectID，否则返回0</returns>
        public long AddRequestProtect(int userid, string calledOper, object[] args)
        {
            return AddRequestProtect(userid, calledOper, HTBaseFunc.GetTime(0), -1, args);
        }

        /// <summary>
        /// 删除请求保护
        /// </summary>
        /// <param name="protectid">请求保护ID</param>
        public void DelRequestProtect(long protectid)
        {
            if (_requestidlist.ContainsKey(protectid))
            {
                object[] arrlist = _requestidlist[protectid];
                _requestidlist.Remove(protectid);
                int userid = (int)arrlist[0];
                string calledoper = arrlist[1].ToString();
                arrlist = null;

                DateTime expiretime = HTBaseFunc.NullDateTime;
                if (_requestProtectList.ContainsKey(userid))
                {
                    Dictionary<string, object[]> operlist = _requestProtectList[userid];
                    if (operlist.ContainsKey(calledoper))
                    {
                        object[] arglist = operlist[calledoper];
                        expiretime = Convert.ToDateTime(arglist[arglist.Length - 1]);
                        operlist.Remove(calledoper);
                        if (operlist.Count == 0)
                            _requestProtectList.Remove(userid);
                    }
                }

                if (expiretime > HTBaseFunc.NullDateTime && _requestaddlist.ContainsKey(expiretime))
                {
                    _requestaddlist[expiretime].Remove(protectid);
                    if (_requestaddlist[expiretime].Count == 0)
                        _requestaddlist.Remove(expiretime);
                }
            }
        }

        /// <summary>
        /// 获取请求保护的参数
        /// </summary>
        /// <param name="protectid">请求保护ID</param>
        /// <param name="ispop">是否popup操作，如果是则取出的同时删除</param>
        /// <returns>返回参数列表</returns>
        public object[] GetRequestArgs(long protectid, bool ispop)
        {
            object[] arglist = null;
            if (_requestidlist.ContainsKey(protectid))
            {

                object[] arrlist = _requestidlist[protectid];
                int userid = (int)arrlist[0];
                string calledoper = arrlist[1].ToString();
                arrlist = null;

                if (_requestProtectList.ContainsKey(userid))
                {
                    Dictionary<string, object[]> operlist = _requestProtectList[userid];
                    if (operlist.ContainsKey(calledoper))
                        arglist = operlist[calledoper];
                }
            }

            if (ispop && arglist != null)
                DelRequestProtect(protectid);

            return arglist;
        }

    }
}
