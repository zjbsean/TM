using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Collections.Generic;
using GsTechLib;
using System.Threading.Tasks;
using System.Text;

namespace ServerCommon
{
    public enum eHttpHostType
    {
        Login,
        Recharge,
        Entry,
        EntryCfgUpdate,
        GM,
        ClientLogger,
    }

    public class HttpHost
    {
        public HttpHost(eHttpHostType hostType, string svrUrl, WaitCallback waitCallback, Action<string> logCallBack)
        {
            m_hostType = hostType;
            m_serviceUrl = svrUrl;
            m_waitCallback = waitCallback;
            m_logCallBack = logCallBack;
        }

        public bool AutoReRun { get; set; }

        public eHttpHostType HostType
        {
            get { return m_hostType; }
        }

        public void Stop()
        {
            lock (m_stopWorker)
            {
                m_stopWorker.Set();
            }

            if (m_workingThread != null)
            {
                m_workingThread.Abort();
                m_workingThread = null;
            }

            if (m_listerner != null)
            {
                m_listerner.Stop();
                m_listerner = null;
            }
        }

        public bool RunHost()
        {
            if (m_logCallBack == null)
            {
                return false;
            }

            if (m_serviceUrl == null || m_serviceUrl == string.Empty)
            {
                m_logCallBack(string.Format("{0} Service URL is not valid!", m_hostType));
                return false;
            }

            if (m_waitCallback == null)
            {
                m_logCallBack(string.Format("{0} No Charge Callback is set", m_hostType));
                return false;
            }
            if (m_workingThread != null)
            {
                Stop();
                int i = 5;
                m_logCallBack(string.Format("{0} Old Payment Server Shutting down!", m_hostType));
                while (i > 0)
                {
                    m_logCallBack(string.Format("\t\t{0}...", i));
                    Thread.Sleep(1000);
                    i--;
                }
            }

            m_stopWorker = new ManualResetEvent(false);
            m_workingThread = new Thread(DoWork);
            m_workingThread.Start();
            return true;
        }

        void DoWorkSafe()
        {
            try
            {
                DoWork();
            }
            catch (ThreadAbortException)
            {
                // if thread is aborted, do not restart it!
            }
            catch (Exception err)
            {
                m_logCallBack(string.Format("{0} Fatal error, restarting service", m_hostType));

                RunHost();
            }
        }
        void DoWork()
        {
            m_listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;//指定身份验证  Anonymous匿名访问
            m_listerner.Prefixes.Add(m_serviceUrl);
            m_logCallBack(string.Format("{1} Start payment server at {0}", m_serviceUrl, m_hostType));
            ThreadPool.SetMaxThreads(1000, 1000);
            m_listerner.Start();
            displayPrefixesAndState();
            bool stoped = false;
            while (!stoped)
            {
                try
                {
                    //等待请求连接
                    //没有请求则GetContext处于阻塞状态
                    //SvLogger.Debug("HTTP GetContext Begin.");
                    HttpListenerContext ctx = m_listerner.GetContext();
                    //SvLogger.Debug("HTTP GetContext End.");

                    //SvLogger.Debug("HTTP Enter Thread Pool Begin.");
                    ThreadPool.QueueUserWorkItem(m_waitCallback, ctx);
                    //SvLogger.Debug("HTTP Enter Thread Pool End.");
                }
                catch (Exception err)
                {
                    //there might be hostile requests, should we block from certain ip?
                    m_logCallBack(string.Format("{0} Error receiving exmsg="+err.Message + " hostType=", m_hostType));
                }

                //SvLogger.Debug("StopWorker Begin.");
                lock (m_stopWorker)
                {
                    //SvLogger.Debug("while StopWorker 1  hostType=", m_hostType);
                    stoped = m_stopWorker.WaitOne(0, false);
                    //SvLogger.Debug("while StopWorker 2.stoped = " + stoped + " hostType=", m_hostType);
                }
                //SvLogger.Debug("StopWorker End.");
            }

            m_listerner.Stop();
            m_listerner = null;
        }

        private void displayPrefixesAndState()
        {
            HttpListenerPrefixCollection prefixes = m_listerner.Prefixes;
            if(prefixes.Count == 0)
            {
                SvLogger.Debug("    There are no prefixes.");
            }

            StringBuilder tempStr = new StringBuilder();
            int index = 0;
            foreach (string prefix in prefixes)
            {
                tempStr.Clear();
                tempStr.AppendFormat("Prefix {0} : {1}.", ++index, prefix);
                SvLogger.Debug(tempStr.ToString());
            }

            if(m_listerner.IsListening)
            {
                SvLogger.Debug("    The Server Is Listening.");
            }
        }

        private eHttpHostType m_hostType;
        private ManualResetEvent m_stopWorker;
        private Thread m_workingThread;
        private HttpListener m_listerner = new HttpListener();
        private WaitCallback m_waitCallback = null;
        private Action<string> m_logCallBack = null;
        private string m_serviceUrl;
    }
}

