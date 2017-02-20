using System;
using System.Collections.Generic;
using System.Text;
using ServiceStack.Redis;
using System.Threading;
using ServiceStack.Text;
using GsTechLib;
using System.Diagnostics;

namespace RedisLib
{
    public class RedisAccessPool
    {
        public RedisAccessPool(RedisAccess redisAccess)
        {
            m_redisAccess = redisAccess;

            m_thread = new Thread(run);
        }

        public int ID
        {
            get
            {
                return m_redisAccess.RedisID;
            }
        }

        public string IP
        {
            get
            {
                return m_redisAccess.IP;
            }
        }

        public int Port
        {
            get
            {
                return m_redisAccess.Port;
            }
        }

        public string Password
        {
            get
            {
                return m_redisAccess.Password;
            }
        }

        public bool IsRun
        {
            get
            {
                return m_runThread;
            }
        }

        public void Start()
        {
            if(m_runThread == false)
            {
                m_runThread = true;
                m_thread.Start();
            }
        }

        public void Stop()
        {
            if(m_thread != null && m_runThread == true)
            {
                m_runThread = false;
                //m_thread.Abort();
                //m_thread = null;
            }
        }

        public eRedisAccessError Push(RedisCommandBase command)
        {
            if (m_runThread == true)
            {
                lock (m_mainThreadCommandListLuckObj)
                {
                    m_mainThreadCommandList.Add(command);
                }
            }
            else
            {
                command.m_Err = eRedisAccessError.Access_Thread_Not_Run;
            }

            return command.m_Err;
        }

        public eRedisAccessError Push(List<RedisCommandBase> commandList)
        {
            if (m_runThread == true)
            {
                lock (m_mainThreadCommandListLuckObj)
                {
                    m_mainThreadCommandList.AddRange(commandList);
                }
            }
            else
            {
                return eRedisAccessError.Access_Thread_Not_Run;
            }

            return eRedisAccessError.None;
        }

        private void run()
        {
            while (m_runThread)
            {
                m_stopwatch.Restart();
                if (m_redisCliDisconnect == false)
                {
                    try 
                    {
                        if (commandDeal() > 0)
                        {
                            try
                            {
                                if (m_redisAccess.Reconnect() == true)
                                    m_redisCliDisconnect = false;
                            }
                            catch (Exception ex)
                            {
                                m_redisCliDisconnect = true;
                                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        if (m_redisAccess.RedisCli.IsSocketConnected() == false)
                            m_redisCliDisconnect = true;

                        SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
                    }
                }
                else
                {
                    try
                    {
                        if (m_redisAccess.Reconnect())
                            m_redisCliDisconnect = false;
                        else
                            Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
                m_stopwatch.Stop();
                if (m_stopwatch.Elapsed < m_maxSleepTS)
                    Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                else
                    Thread.Sleep(1);
            }

            commandDeal();

            m_redisAccess.Close();
        }

        private eRedisAccessError commandDeal()
        {
            if (m_redisCliDisconnect == true)
                return eRedisAccessError.Connection_Is_Disconnect;

            eRedisAccessError errID = eRedisAccessError.None;
            lock (m_mainThreadCommandListLuckObj)
            {
                if (m_mainThreadCommandList.Count > 0)
                {
                    m_selfThreadCommandList.AddRange(m_mainThreadCommandList);
                    m_mainThreadCommandList.Clear();
                }
            }

            if (m_selfThreadCommandList.Count > 0)
            {
                int i = 0;
                for (; i < m_selfThreadCommandList.Count; ++i)
                {
                    try
                    {
                        errID = m_selfThreadCommandList[i].DoCommand(m_redisAccess.RedisCli);
                        if (errID != 0)
                        {
                            if (m_redisAccess.RedisCli.Ping() == false)
                            {
                                m_redisCliDisconnect = true;
                                if (i > 0)
                                {
                                    m_selfThreadCommandList.RemoveRange(0, i);
                                    i = 0;
                                }
                                break;
                            }
                        }
                        if (m_selfThreadCommandList[i].NeedDealCallBack == true)
                            m_selfThreadDealedCommandList.Add(m_selfThreadCommandList[i]);
                    }
                    catch (Exception ex)
                    {
                        errID = eRedisAccessError.Command_Exec_Exception;
                        SvLogger.Fatal(ex, "Redis Command Deal Exception: {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                if(i == m_selfThreadCommandList.Count)
                    m_selfThreadCommandList.Clear();
            }

            if(m_selfThreadDealedCommandList.Count > 0)
            {
                lock(m_mainThreadDealdCommandListLuckObj)
                {
                    m_mainThreadDealedCommandList.AddRange(m_selfThreadDealedCommandList);
                }

                m_selfThreadDealedCommandList.Clear();
            }

            return errID;
        }

        public void PopAllMainThreadDealedCommandList(ref List<RedisCommandBase> commandList)
        {
            lock(m_mainThreadDealdCommandListLuckObj)
            {
                if (m_mainThreadDealedCommandList.Count > 0)
                {
                    commandList.AddRange(m_mainThreadDealedCommandList);
                    m_mainThreadDealedCommandList.Clear();
                }
            }
        }

        #region Need Deal Commands
        
        private List<RedisCommandBase> m_selfThreadCommandList = new List<RedisCommandBase>();

        private object m_mainThreadCommandListLuckObj = new object();
        private List<RedisCommandBase> m_mainThreadCommandList = new List<RedisCommandBase>();
        
        #endregion

        #region Dealed Commands

        private List<RedisCommandBase> m_selfThreadDealedCommandList = new List<RedisCommandBase>();

        private object m_mainThreadDealdCommandListLuckObj = new object();
        private List<RedisCommandBase> m_mainThreadDealedCommandList = new List<RedisCommandBase>();
        
        #endregion

        private bool m_runThread = false;
        private int m_id;
        private bool m_redisCliDisconnect = false;
        private RedisAccess m_redisAccess;
        private Thread m_thread = null;
        private TimeSpan m_maxSleepTS = new TimeSpan(10000 * 50);
        private Stopwatch m_stopwatch = new Stopwatch();
    }
}
