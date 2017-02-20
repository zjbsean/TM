using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using ServiceStack.Redis;
using System.Diagnostics;

namespace ServerCommon
{
    public enum eRedisVisitErr
    {
        None = 0,
        VisiterThreadIsRun = 1,
        VisiterThreadStartException = 2,
        VisiterServerIPEmpty = 3,
        VisiterServerFail = 4,
    }

    public class RedisSetData<T> : IHadKeyNodeBase<string>
    {
        public RedisSetData(string key, T obj, Action<bool, T> saveBack)
        {
            m_key = key;
            m_obj = obj;
            m_saveBackFun = saveBack;
        }

        public string Key()
        {
            return m_key;
        }

        public T Obj
        {
            get
            {
                return m_obj;
            }
        }

        public void SetSetResult(bool result)
        {
            m_isSetSucc = result;
        }

        public void DoSaveBack()
        {
            m_saveBackFun(m_isSetSucc, m_obj);
        }

        public byte[] BinaryData
        {
            get
            {
                try
                {
                    return serialize(m_obj);
                }
                catch(Exception ex)
                {
                    return new byte[0];
                }
            }
        }

        private byte[] serialize(T obj)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            return buffer;
        }

        private T m_obj;
        private string m_key;
        private bool m_isSetSucc = false;
        private Action<bool, T> m_saveBackFun;
    }

    public class RedisGetData<T> : IHadKeyNodeBase<string>
    {
        public RedisGetData(string key, Action<T> getBack)
        {
            m_key = key;
            m_getBackFun = getBack;
        }

        public string Key()
        {
            return m_key;
        }

        public void GetObj(T obj)
        {
            m_obj = obj;
        }

        public void GetObjBinary(byte[] objData)
        {
            try
            {
                m_obj = desrialize(objData);
            }
            catch(Exception ex)
            {

            }
        }

        public void DoGetBack()
        {
            m_getBackFun(m_obj);
        }

        private T desrialize(byte[] buffer)
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(buffer);
            m_obj = (T)formatter.Deserialize(stream);
            stream.Close();
            return m_obj;
        }

        private T m_obj;
        private string m_key;
        private Action<T> m_getBackFun;
    }

    public class RedisVisiter<T>
    {
        public void SetConnectParam(string ip, int port = 0)
        {
            m_ipAddress = ip;
            m_port = port;
        }

        public eRedisVisitErr Run()
        {
            if (m_runThread != null)
                return eRedisVisitErr.VisiterThreadIsRun;

            m_threadRunFlag = true;

            onBeforeStartRun();

            connectServer();

            m_runThread = new Thread(visitUpdate);
            try
            {
                m_runThread.Start();
            }
            catch(Exception ex)
            {
                startRunFail(ex);
                return eRedisVisitErr.VisiterThreadStartException;
            }

            onStartRun();

            return eRedisVisitErr.None;
        }

        public void Stop()
        {
            if (m_runThread != null)
            {
                m_threadRunFlag = false;

                m_runThread.Abort();
                
                m_runThread = null;
            }

            if(m_visiter != null)
            {
                m_visiter.Dispose();
                m_visiter = null;
            }

            onStopRun();
        }

        private eRedisVisitErr connectServer()
        {
            if (m_ipAddress.Trim() == "")
                return eRedisVisitErr.VisiterServerIPEmpty;

            try
            {
                if (m_port == 0)
                    m_visiter = new RedisClient(m_ipAddress);
                else
                    m_visiter = new RedisClient(m_ipAddress, m_port);
            }
            catch(Exception ex)
            {
                return eRedisVisitErr.VisiterServerFail;
            }

            return eRedisVisitErr.None;
        }

        private void visitUpdate()
        {
            while(m_threadRunFlag)
            {
                m_stopwatch.Restart();

                //Get 优先
                tempGetData = null;
                lock (m_getDataLink)
                {
                    tempGetData = m_getDataLink.PopFirst();
                }
                
                if(tempGetData != null)
                {
                    byte[] objData = m_visiter.Get(tempGetData.Value.Key());
                    tempGetData.Value.GetObjBinary(objData);
                    lock (m_getDataBackLockObj)
                        m_getDataBackLink.AddLast(tempGetData);
                }
                //

                //Set
                tempSetData = null;
                lock (m_setDataLockObj)
                    tempSetData = m_setDataLink.PopFirst();

                if (tempSetData != null)
                {
                    byte[] binaryData = tempSetData.Value.BinaryData;
                    bool isSucc = false;
                    if(binaryData.Length > 0)
                        isSucc = m_visiter.Set(tempSetData.Value.Key(), binaryData);
                    tempSetData.Value.SetSetResult(isSucc);
                    lock (m_setDataBackLockObj)
                        m_setDataBackLink.AddLast(tempSetData);
                }
                //

                m_stopwatch.Stop();
                if (m_stopwatch.Elapsed < m_maxSleepTS)
                    Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                else
                    Thread.Sleep(1);
            }
        }

        public void SetObj(string key, T obj, Action<bool, T> setBackFun)
        {
            lock(m_setDataLockObj)
            {
                if (m_setDataLink.PeekData(key) == null)
                {
                    RedisSetData<T> redisData = new RedisSetData<T>(key, obj, setBackFun);
                    m_setDataBackLink.AddLast(redisData);
                }
            }
        }

        public void GetObj(string key, Action<T> getBackFun)
        {
            lock(m_setDataLink)
                tempSetData = m_setDataLink.PopData(key);

            if (tempSetData != null)
            {
                RedisGetData<T> redisData = new RedisGetData<T>(key, getBackFun);
                redisData.GetObj(tempSetData.Value.Obj);

                lock (m_getDataBackLockObj)
                    m_getDataBackLink.AddLast(redisData);
            }
        }

        public void Update()
        {
            lock(m_getDataBackLockObj)
            {
                LinkedListNode<RedisGetData<T>> getDataNode = m_getDataBackLink.PopFirst();
                while (getDataNode != null)
                {
                    try
                    {
                        getDataNode.Value.DoGetBack();
                    }
                    catch(Exception ex)
                    {

                    }
                    getDataNode = m_getDataLink.PopFirst();
                }
            }

            lock(m_setDataBackLink)
            {
                LinkedListNode<RedisSetData<T>> setDataNode = m_setDataBackLink.PopFirst();
                while(setDataNode != null)
                {
                    try
                    {
                        setDataNode.Value.DoSaveBack();
                    }
                    catch(Exception ex)
                    {

                    }
                    setDataNode = m_setDataBackLink.PopFirst();
                }
            }
        }

        private void onBeforeStartRun()
        {

        }

        private void onStartRun()
        {

        }
        private void onStopRun()
        {

        }

        private void startRunFail(Exception ex)
        {

        }

        private string m_ipAddress;
        private int m_port;
        private Thread m_runThread = null;
        private RedisClient m_visiter = null;
        private bool m_threadRunFlag = false;

        private LinkedListNode<RedisSetData<T>> tempSetData = null;
        private LinkedListNode<RedisGetData<T>> tempGetData = null;

        private QuickSearchLink<string, RedisSetData<T>> m_setDataLink = new QuickSearchLink<string, RedisSetData<T>>();
        private object m_setDataLockObj = new object();

        private QuickSearchLink<string, RedisSetData<T>> m_setDataBackLink = new QuickSearchLink<string, RedisSetData<T>>();
        private object m_setDataBackLockObj = new object();

        private QuickSearchLink<string, RedisGetData<T>> m_getDataLink = new QuickSearchLink<string, RedisGetData<T>>();
        private object m_getDataLockObj = new object();

        private QuickSearchLink<string, RedisGetData<T>> m_getDataBackLink = new QuickSearchLink<string, RedisGetData<T>>();
        private object m_getDataBackLockObj = new object();

        private TimeSpan m_maxSleepTS = new TimeSpan(10000 * 50);
        private Stopwatch m_stopwatch = new Stopwatch();
    }
}