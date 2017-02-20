using System;
using System.Collections.Generic;
using System.Threading;
using GsTechLib;
using com.tieao.mmo.interval;
using System.Diagnostics;

namespace ServerCommon
{
    //服务器服务基类
    public class BasicService
    {
        #region BasicService IsRunning Code.

        protected static BasicServiceRunner _Runner = null;

        public static void SetRunner(BasicServiceRunner runner)
        {
            _Runner = runner;
        }

        /// <summary>
        /// 停止运行服务
        /// </summary>
        public static void StopRunnerService()
        {
            //Network.NetworkManager.Instance.StopRunNetworkUpdate();
            if (_Runner != null)
            {
                _Runner.StopService();

                //if (SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.CENTER)
                //{
                //    //向各个Server发送服务关闭消息
                //    com.tieao.mmo.interval.PtServerList regSvrList = RegServerManager.Instance.GetAllServerList();
                //    for (int i = 0; i < regSvrList.GetElements().Count; ++i)
                //    {
                //        int sessionID = regSvrList.GetElements()[i].m_SessionID;
                //        if( regSvrList.GetElements()[i].m_Type != eServerType.DATABASE && 
                //            regSvrList.GetElements()[i].m_Type != eServerType.CROSSREALM &&
                //            regSvrList.GetElements()[i].m_Type != eServerType.PROTAL &&
                //            regSvrList.GetElements()[i].m_Type != eServerType.GM &&
                //            regSvrList.GetElements()[i].m_Type != eServerType.PLATFORM_DOCKING &&
                //            regSvrList.GetElements()[i].m_Type != eServerType.MONITEORNODE &&
                //            regSvrList.GetElements()[i].m_Type != eServerType.MONITEORSERVER &&
                //            regSvrList.GetElements()[i].m_Type != eServerType.COMMANDSENDER)
                //            Network.NetworkManager.Instance.SendMessageToServer(sessionID, com.tieao.mmo.interval.client.InternalProtocolClientHelper.ShutDownSvr("Server Close Nomal!"));
                //    }
                //}
                //else if (SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.CROSSREALM)
                //{
                //    com.tieao.mmo.interval.PtServerList regSvrList = RegServerManager.Instance.GetAllServerList();
                //    for (int i = 0; i < regSvrList.GetElements().Count; ++i)
                //    {
                //        int sessionID = regSvrList.GetElements()[i].m_SessionID;
                //        if (regSvrList.GetElements()[i].m_Type != eServerType.CENTER)
                //            Network.NetworkManager.Instance.SendMessageToServer(sessionID, com.tieao.mmo.interval.client.InternalProtocolClientHelper.ShutDownSvr("Server Close Nomal!"));
                //    }
                //}
            }
        }

        /// <summary>
        /// 收到CenterServer关闭命令
        /// </summary>
        //public static void RecvCenterServerCloseCommand()
        //{
        //    if (m_isStop == false)
        //    {
        //        m_isStop = true;
        //        Network.NetworkManager.Instance.StopRunNetworkUpdate();
        //        if (_Runner != null)
        //            _Runner.StopService();
        //    }
        //}

        //服务运行开关
        private bool _bRunning = false;
        public bool IsRunning
        {
            get
            {
                return _bRunning;
            }
            set
            {
                _bRunning = value;
            }
        }

        /// <summary>
        /// 获取服务名字
        /// </summary>
        /// <returns></returns>
        public string GetAssemblyName()
        {
            string name = System.Reflection.Assembly.GetEntryAssembly().FullName;
            return name.Split(',')[0];
        }

        #endregion

        #region Basic Virtual Functions

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <returns></returns>
        virtual public void InitInstance(string[] args)
        {
            //修改引用TITLE
            if (SvrCommCfg.Instance.ServerInfo.m_Name == "")
            {
                SvLogger.Error("ServerName cannot be empty.");
            }
            SvLogger.Info("ServerName = {0}.", SvrCommCfg.Instance.ServerInfo.m_Name);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Process processes = Process.GetCurrentProcess();
                PInvoker.SetConsoleTitle(GetAssemblyName() + " - " + processes.Id);
                SvrCommCfg.Instance.ProcessID = processes.Id;
            }
        }

        /// <summary>
        /// 结束服务
        /// </summary>
        /// <returns></returns>
        virtual public void ExitInstance()
        {
            //while (true)
            //{
            //    Thread.Sleep(10);
            //}
        }

        /// <summary>
        /// 服务更新
        /// </summary>
        /// <returns></returns>
        virtual public int Update()
        {


            return 0;
        }

        private static bool m_isStop = false;
        #endregion
    }

    //服务运行管理类
    public sealed class BasicServiceRunner
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="service">服务类</param>
        /// <param name="args">服务参数</param>
        public BasicServiceRunner(BasicService service, string[] args)
        {
            #region 去掉关闭菜单

            try
            {
                int hwnd = (int)System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                int hmenu = PInvoker.GetSystemMenu(hwnd, false);
                bool bsucc = PInvoker.RemoveMenu(hmenu, 0xF060, 0);
            }
            catch
            {
            }

            #endregion

            #region 初始化

            try
            {
                _Service = service;
                _Service.InitInstance(args);
                _Service.IsRunning = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Abnormal Application Termination :{0}\r\nStackTrace : \r\n{1}", e.Message, e.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Abnormal Application Termination : {0}\r\nStackTrace : \r\n{1}", e.Message, e.StackTrace), _Service.GetAssemblyName());
                }
                catch
                {
                }
                //Network.NetworkManager.Instance.StopRunNetworkUpdate();
                _Service.IsRunning = false;
                return;
            }

            #endregion
        }

        /// <summary>
        /// 运行服务
        /// </summary>
        public void RunService(bool hadInterval)
        {
            try
            {
                int join0num = 0;
                if (hadInterval == true)
                {
                    while (_Service.IsRunning/* || Network.NetworkManager.Instance.CheckNetworkThreadIsAlive()*/)
                    {
                        int startTick = Environment.TickCount;
                        
                        m_stopwatch.Restart();
                        
                        _Service.Update();

                        m_stopwatch.Stop();
                        if (m_stopwatch.Elapsed < m_maxSleepTS)
                            Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                        else
                            Thread.Sleep(1);
                    }
                }
                else
                {
                    while (_Service.IsRunning/* || Network.NetworkManager.Instance.CheckNetworkThreadIsAlive()*/)
                    {
                        m_stopwatch.Restart();

                        _Service.Update();

                        m_stopwatch.Stop();
                        if (m_stopwatch.Elapsed < m_maxSleepTS)
                            Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                        else
                            Thread.Sleep(1);
                    }
                }
                _Service.ExitInstance();
            }
            catch (Exception e)
            {
                Console.WriteLine("Abnormal Application Termination :{0}\r\nStackTrace : \r\n{1}", e.Message, e.StackTrace);
                try
                {
                    HTEventLog.InitFileLog(@"ServerEvents.log");
                    HTEventLog.SaveLog(HTEventLog.LogTypeEnum.错误, string.Format("Abnormal Application Termination : {0}\r\nStackTrace : \r\n{1}", e.Message, e.StackTrace), _Service.GetAssemblyName());
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void StopService()
        {
            if (_Service != null)
            {
                _Service.IsRunning = false;
            }
        }

        /// <summary>
        /// 获取运行的服务
        /// </summary>
        /// <returns></returns>
        public BasicService GetService()
        {
            return _Service;
        }

        /// <summary>
        /// 运行的服务对象
        /// </summary>
        private BasicService _Service = null;
        private TimeSpan m_maxSleepTS = new TimeSpan(10000 * 20);
        private Stopwatch m_stopwatch = new Stopwatch();
    }
}
