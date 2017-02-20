using System;
using System.Collections.Generic;
using GsTechLib;
using ServerCommon;
using ServerCommon.Network;
using RedisLib;

namespace Server
{
    partial class ServerMain : BasicServer
    {
        #region Public Functions

        /// <summary>
        /// 初始化服务实例
        /// </summary>
        /// <returns></returns>
        public override bool Init(string[] args)
        {
            if (base.Init(args) == false)
                return false;

            #region 初始化服务配置

            //初始化LOG日志配置
            SvLogger.Init(XmlConfigure.Instance.GetAppConfigString("LogFile"), XmlConfigure.Instance.GetAppConfigString("LogFileLevel"), XmlConfigure.Instance.GetAppConfigString("LogConsoleLevel"));

            ServerConfigData.ListenPort = XmlConfigure.Instance.GetAppConfigInt("ListenPort");
            ServerConfigData.DBIP = XmlConfigure.Instance.GetAppConfigString("DBIP");
            ServerConfigData.DBPort = XmlConfigure.Instance.GetAppConfigInt("DBPort");
            ServerConfigData.DBName = XmlConfigure.Instance.GetAppConfigString("DBName");
            ServerConfigData.DBUser = XmlConfigure.Instance.GetAppConfigString("DBUser");
            ServerConfigData.DBPassword = XmlConfigure.Instance.GetAppConfigString("DBPassword");

            ServerConfigData.OSSPath = XmlConfigure.Instance.GetAppConfigString("OSSPath");
            ServerConfigData.OSSUser = XmlConfigure.Instance.GetAppConfigString("OSSUser");
            ServerConfigData.OSSPassword = XmlConfigure.Instance.GetAppConfigString("OSSPassword");
            
            #endregion

            #region 网络组更新启动
            try
            {
                SvLogger.Info("<< Start Network Listen : OutSidePort={0}.", ServerConfigData.ListenPort);
                ServerCommon.Network.XDNetworkManager.Instance.InitConfig(ServerConfigData.ListenPort);
                ServerCommon.Network.XDNetworkManager.Instance.StartRun();
            }
            catch (Exception ex)
            {
                SvLogger.Fatal(ex, ">> ****** Network Listen Fail! Msg={0}", ex.Message);
                return false;
            }
            SvLogger.Info(">> Network Listen Succ !!!");
            #endregion

            #region 数据库连接

            bool dbConnectBack = false;
            dbConnectBack = DBAccessCfg.Instance.InitDBConnection(eDbConnFlag.Game,
                                                                        ServerConfigData.DBIP,
                                                                        ServerConfigData.DBPort,
                                                                        ServerConfigData.DBName,
                                                                        ServerConfigData.DBUser,
                                                                        ServerConfigData.DBPassword);
            if (dbConnectBack == false)
            {
                SvLogger.Error("DB Access Config Deal Error!");
                return false;
            }

            #endregion

            #region 从数据库加载服务器组配置数据

            //LoadDataFromMySqlManager.Instance.m_OnLoadAllTableSucc = onLoadConfigFromDBFinish;
            //LoadDataFromMySqlManager.Instance.LoadData();

            #endregion

            return true;
        }

        /// <summary>
        /// 服务逻辑更新
        /// </summary>
        public void Update()
        {
            try
            {
                #region 协议处理
                ClientProtocolDealManager.Instance.TransferRecvClientData(ref m_protocolList);
                if (m_protocolList.Count > 0)
                {
                    for (int i = 0; i < m_protocolList.Count; i++)
                    {
                        try
                        {
                            //ServerProtocolDealManager.Instance.ServerSend2ClientData(clientSessionID, protocolData);
                        }
                        catch (Exception ex)
                        {
                            SvLogger.Fatal(ex, string.Format("Param Proto Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace));
                        }    
                    }
                    m_protocolList.Clear();
                }
                #endregion

                #region 数据库回调处理

                List<DbAccessPoolVistor> dapvistorlist = DbAccessPool.GetVistorList();
                for (int i = 0; i < dapvistorlist.Count; i++)
                {
                    DbAccessPoolVistor dapvistor = dapvistorlist[i];
                    int callbackNum = dapvistor.GetCallbackNum();
                    int doBackNum = 0;
                    int vStartTick = Environment.TickCount;
                    int doStartTick;
                    DateTime doStartTime;
                    while (doBackNum < callbackNum)
                    {
                        doStartTick = Environment.TickCount;
                        doStartTime = HTBaseFunc.GetTime(0);
                        DbAccessItem dbi = dapvistor.PopupCallback();
                        if (dbi != null)
                        {
                            dbi.OnFinish(dbi);
                        }
                        ++doBackNum;
                        int doEndTick = Environment.TickCount;
                        int doSpendTicks = (doEndTick >= doStartTick ? doEndTick - doStartTick : (int.MaxValue + doEndTick) + (int.MaxValue - doStartTick));
                        if (doSpendTicks >= 1000)
                            SvLogger.Warn("Update TimeOut : DbAccessPoolCallback {0}, SpendMs = {1}, SpName = {2}, StartTime = {3}.", dapvistor.Flag, doSpendTicks, ((dbi != null) ? dbi.SpName : ""), doStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        int vSpendTicks = (doEndTick >= vStartTick ? doEndTick - vStartTick : doEndTick + (int.MaxValue - vStartTick));
                        if (vSpendTicks >= SvrCommCfg.Instance.DbAccessPoolCallbackDealInterval)
                            break;
                    }
                }

                #endregion

                #region 定时打印服务器监控信息

                m_svrCheckEndTicket = Environment.TickCount;
                m_svrCheckSpendTicket = (m_svrCheckEndTicket >= m_svrCheckStartTicket ? m_svrCheckEndTicket - m_svrCheckStartTicket : (int.MaxValue + m_svrCheckEndTicket) + (int.MaxValue - m_svrCheckStartTicket));
                if (m_svrCheckSpendTicket >= 60000)
                {
                    m_svrCheckStartTicket = m_svrCheckEndTicket;
                    ServerCommon.DbAccess.Instance.ShowDBAccCountInPool();

                }

                #endregion

                #region 内存回收

                if (m_lastMemoryCheck.AddSeconds(30) < HTBaseFunc.GetTime(0))
                {
                    if (m_lastGcTime.AddSeconds(30) < HTBaseFunc.GetTime(0) && System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 > 128 * 1024 * 1024)
                    {
                        GC.Collect();
                        m_lastGcTime = HTBaseFunc.GetTime(0);
                    }

                    m_lastMemoryCheck = HTBaseFunc.GetTime(0);
                }

                #endregion
            }
            catch(Exception ex)
            {
                SvLogger.Fatal(ex, string.Format("Main Logic Exception : {0}\r\nStackTrace : \r\n{1}", ex.Message, ex.StackTrace));
            }
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// 构造
        /// </summary>
        private ServerMain() { }

        #endregion

        #region Property

        public static ServerMain Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region Data Member

        private static ServerMain _instance = new ServerMain();

        private int m_svrCheckSpendTicket = 0;
        private int m_svrCheckStartTicket = Environment.TickCount;
        private int m_svrCheckEndTicket = 0;

        private DateTime m_lastMemoryCheck = DateTime.Now;
        private DateTime m_lastGcTime = DateTime.Now;

        private List<ClientProtocolDealManager.RecvClientProtocol> m_protocolList = new List<ClientProtocolDealManager.RecvClientProtocol>();

        #endregion
    }
}
