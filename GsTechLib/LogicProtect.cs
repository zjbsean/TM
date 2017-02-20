using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GsTechLib
{
    /// <summary>
    /// ���ñ�����
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

        //Ƶ�����ñ���
        private Hashtable _callProtectList = new Hashtable();//��campidΪkey�洢Hashtable(�Բ�����Ϊkey�洢�ϴβ���ʱ��)

        /// <summary>
        /// ��ӵ��ñ���
        /// </summary>
        /// <param name="campid">ƥ��ID</param>
        /// <param name="calledOper">���ò���</param>
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
        /// ɾ�����ñ���
        /// </summary>
        /// <param name="campid">ƥ��ID</param>
        /// <param name="calledOper">���ò���</param>
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
        /// �����ñ���������ʣ������
        /// </summary>
        /// <param name="campid">ƥ��ID</param>
        /// <param name="calledOper">���ò���</param>
        /// <param name="allowInterval">����ļ�������뼶</param>
        /// <returns>����ʣ��ļ��</returns>
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
    /// ״̬������
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

        //״̬����
        private Hashtable _statusProtectList = new Hashtable();//��campidΪkey�洢StatusData

        /// <summary>
        /// ���״̬����
        /// </summary>
        /// <param name="campid">ƥ��ID</param>
        /// <param name="calledOper">���ò���</param>
        /// <param name="callTime">����ʱ��</param>
        /// <returns>����б����򷵻�ǰһ������</returns>
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
        /// ɾ��״̬����
        /// </summary>
        /// <param name="sessionID">ƥ��ID</param>
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
    /// ���󱣻���
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

        //���󱣻�
        private long _currprotectid = 0;
        private Dictionary<int, Dictionary<string, object[]>> _requestProtectList = new Dictionary<int, Dictionary<string, object[]>>();//��useridΪkey�洢�б���һ��������ΪKey�洢�����б������ڶ���������ProtectID�����һ�������ǹ���ʱ�䣬��2�����������ɸ�����ʱ����λ�ã�
        private Dictionary<long, object[]> _requestidlist = new Dictionary<long, object[]>();//��ProtectIDΪKey�洢UserID�Ͳ�������ɵ�һ������
        private SortedList<DateTime, List<long>> _requestaddlist = new SortedList<DateTime, List<long>>();//�Լ�¼ʱ��ΪKey�洢ProtectID���б�

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
        /// ������󱣻�
        /// </summary>
        /// <param name="userid">���ID</param>
        /// <param name="calledOper">���ò���</param>
        /// <param name="callTime">����ʱ��</param>
        /// <param name="holdIntervals">����ʱ�������뼶</param>
        /// <param name="args">�����б�ע�������Ҫ��2����λ���ڲ����뱣��ID�͵���ʱ��</param>
        /// <returns>�ɹ����ش���0��ProtectID�����򷵻�0</returns>
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
                    //ɾ��ԭ�����ڵ�
                    operlist.Remove(calledOper);
                    _requestidlist.Remove(oldprotectid);
                    if (_requestaddlist.ContainsKey(oldexpiretime))
                    {
                        _requestaddlist[oldexpiretime].Remove(oldprotectid);
                        if (_requestaddlist[oldexpiretime].Count == 0)
                            _requestaddlist.Remove(oldexpiretime);
                    }

                    //����µ�
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
        /// ������󱣻�(�־ñ���ֱ��ɾ��)
        /// </summary>
        /// <param name="userid">���ID</param>
        /// <param name="calledOper">���ò���</param>
        /// <param name="callTime">����ʱ��</param>
        /// <returns>�ɹ����ش���0��ProtectID�����򷵻�0</returns>
        private long AddRequestProtect(int userid, string calledOper, DateTime callTime, object[] args)
        {
            return AddRequestProtect(userid, calledOper, callTime, -1, args);
        }

        /// <summary>
        /// ������󱣻�
        /// </summary>
        /// <param name="userid">���ID</param>
        /// <param name="calledOper">���ò���</param>
        /// <param name="holdIntervals">����ʱ�������뼶</param>
        /// <returns>�ɹ����ش���0��ProtectID�����򷵻�0</returns>
        public long AddRequestProtect(int userid, string calledOper, int holdIntervals, object[] args)
        {
            return AddRequestProtect(userid, calledOper, HTBaseFunc.GetTime(0), holdIntervals, args);
        }

        /// <summary>
        /// ������󱣻�(�־ñ���ֱ��ɾ��)
        /// </summary>
        /// <param name="userid">���ID</param>
        /// <param name="calledOper">���ò���</param>
        /// <returns>�ɹ����ش���0��ProtectID�����򷵻�0</returns>
        public long AddRequestProtect(int userid, string calledOper, object[] args)
        {
            return AddRequestProtect(userid, calledOper, HTBaseFunc.GetTime(0), -1, args);
        }

        /// <summary>
        /// ɾ�����󱣻�
        /// </summary>
        /// <param name="protectid">���󱣻�ID</param>
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
        /// ��ȡ���󱣻��Ĳ���
        /// </summary>
        /// <param name="protectid">���󱣻�ID</param>
        /// <param name="ispop">�Ƿ�popup�������������ȡ����ͬʱɾ��</param>
        /// <returns>���ز����б�</returns>
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
