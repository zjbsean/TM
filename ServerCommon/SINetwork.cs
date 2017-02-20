using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using com.ideadynamo.foundation;
using com.ideadynamo.foundation.game;
using com.ideadynamo.foundation.channels;
using com.ideadynamo.foundation.buffer;
using com.tieao.mmo.interval;
using com.tieao.mmo.interval.server;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;

namespace ServerCommon
{
    namespace Network
    {
#if SocketConnectMode
        public class SINetworkManager
        {
            public static SINetworkManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            #region Public Functions

            public void InitMoniteorConnectInfo(string ip, int port)
            {
                m_connectMoniteroNodeIP = ip;
                m_connectMoniteorNodePort = port;
            }

            public void InitCenterServerConnectInfo(string ip, int port)
            {
                m_connectCenterServerIP = ip;
                m_connectCenterServerPort = port;
            }

            public void StartRun()
            {
                if(m_networkThread != null)
                {
                    m_threadRun = false;
                    m_networkThread.Abort();
                    m_networkThread = null;
                }

                m_threadRun = true;
                m_networkThread = new Thread(networkRun);
                m_networkThread.Start();
            }

            public void StopRun()
            {
                m_threadRun = false;
            }

            #endregion

            #region Thread Logic 

            private void networkRun()
            {
                m_serverListen = new SIServerListen(SvrCommCfg.Instance.ServerInfo.m_Port);
                if(m_serverListen.Listen() == false)
                {
                    SvLogger.Error("Network Thread : Open Listen Fail, Port={0}.", SvrCommCfg.Instance.ServerInfo.m_Port);
                    return;
                }

                if (m_connectMoniteroNodeIP != "")
                {
                    if (SIConnServiceManager.Instance.CreateConnService(m_connectMoniteroNodeIP, m_connectMoniteorNodePort) == false)
                        throw new Exception("Init Moniteor Server Connection Fail !");
                }

                if(m_connectCenterServerIP != "")
                {
                    if(SIConnServiceManager.Instance.CreateConnService(m_connectCenterServerIP, m_connectCenterServerPort) == false)
                        throw new Exception("Init Center Server Connection Fail !");
                }

                m_threadRun = true;
                int logPrintIndex = 0;
                while(m_threadRun)
                {
                    m_serverListen.Update();
                    SIConnServiceManager.Instance.Update();

                    SIProtocolDataDealManager.Instance.DealData();

                    m_svrCheckEndTicket = Environment.TickCount;
                    m_svrCheckSpendTicket = (m_svrCheckEndTicket >= m_svrCheckStartTicket ? m_svrCheckEndTicket - m_svrCheckStartTicket : m_svrCheckEndTicket + (int.MaxValue - m_svrCheckStartTicket));
                    if (m_svrCheckSpendTicket >= 1000)
                    {
                        int curSvrTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;
                        SIReconnectManager.Instance.Update(curSvrTime);
                        
                        m_svrCheckStartTicket = m_svrCheckEndTicket;
                        ++logPrintIndex;
                        if (logPrintIndex >= 60)
                        {
                            logPrintIndex = 0;

                            //TODO:
                        }
                    }

                    Thread.Sleep(50);
                }

                onThreadClose();
            }

            private void onThreadClose()
            {
                if(m_serverListen != null)
                    m_serverListen.CloseServerListen();

                SIConnServiceManager.Instance.CloseAllConnect();
            }

            #endregion

            

            #region Thread Obj & Connection Config

            private Thread m_networkThread = null;
            private bool m_threadRun = false;

            private int m_listenPort4Server;

            private string m_connectMoniteroNodeIP;
            private int m_connectMoniteorNodePort;

            private string m_connectCenterServerIP;
            private int m_connectCenterServerPort;

            private bool m_moniteorConnIsOpened = false;
            private ConnectService m_moniteorConnection = null;

            private SIServerListen m_serverListen = null;

            #endregion

            private int m_svrCheckSpendTicket = 0;
            private int m_svrCheckStartTicket = 0;
            private int m_svrCheckEndTicket = 0;

            private static SINetworkManager m_instance = new SINetworkManager();
            private SINetworkManager() { }
        }

        public class SIConnectionSession
        {
            public SIConnectionSession(ConnectionSession connSession, int connServiceID)
            {
                m_connSession = connSession;
                m_connServiceID = connServiceID;
                m_createTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;
            }

            public void SetServerInfo(PtServerInfo svrInfo)
            {
                m_svrInfo = svrInfo;
            }

            public bool IsChecked
            {
                get
                {
                    if (m_svrInfo == null)
                        return false;
                    return true;
                }
            }

            public string ServerName
            {
                get
                {
                    return m_svrInfo.m_Name;
                }
            }

            public eServerType ServerType
            {
                get
                {
                    return m_svrInfo.m_Type;
                }
            }

            public int SessionID
            {
                get
                {
                    return m_connSession.LinkId;
                }
            }

            public void Close()
            {
                m_connSession.Connection.Channel.Close();
            }

            public bool IsTimeOut(int curSvrTime)
            {
                if (m_createTime + m_timeOurSec < curSvrTime)
                    return true;
                return false;
            }

            public void SendData(ByteArray data)
            {
                m_connSession.Send(data);
            }

            public int ConnServiceID
            {
                get
                {
                    return m_connServiceID;
                }
            }

            private PtServerInfo m_svrInfo;
            private ConnectionSession m_connSession;
            private int m_connServiceID;
            private int m_createTime;
            private const int m_timeOurSec = 15;
        }

        public class SIServerListen
        {
            public SIServerListen(int listenPort)
            {
                m_listenPort = listenPort;
            }

            public bool Listen()
            {
                CloseServerListen();

                try
                {
                    m_serverListen = new ListenService( new ProtocolDealDelegate(onRecvServerData),
                                                        new OpenNewConnectDelegate(onOpenNewServerConnection),
                                                        new ConnectOnCloseDelegate(onServerConnectionClose), false, 0);
                    bool result = m_serverListen.Listion(m_listenPort, 10000);
                    if (result == false)
                        m_serverListen = null;
                    return result;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public void Update()
            {
                m_serverListen.Update();
            }

            private bool onRecvServerData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                if (buffer._readerIdx == buffer._writerIdx)
                    return false;

                SIReceiveDealManager.Instance.AddData(sessionID, buffer);
                return true;
            }

            private void onOpenNewServerConnection(ConnectionSession connSession)
            {
                SIConnectionSession SIConnSession = new SIConnectionSession(connSession, 0);
                SIConnSessionsManager.Instance.AddConnSession(SIConnSession);

                connSession.Send(com.tieao.mmo.interval.client.InternalProtocolClientHelper.ConnectSvrSucc(SvrCommCfg.Instance.ServerInfo));
            }

            private void onServerConnectionClose(ConnectionSession connSession)
            {
                SIConnSessionsManager.Instance.OnConnectionClose(connSession.LinkId);
            }

            public void CloseServerListen()
            {
                if (m_serverListen != null)
                {
                    m_serverListen.Close();
                    m_serverListen.Dispose();
                    m_serverListen = null;
                }
            }

            private ListenService m_serverListen = null;
            private int m_listenPort;
        }

        public class SIConnServiceManager
        {
            public static SIConnServiceManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            class SIConnService
            {
                public SIConnService(int id, string ip, int port)
                {
                    m_id = id;
                    m_ip = ip;
                    m_port = port;
                }

                public SIConnService(int id, PtServerInfo svrInfo)
                {
                    m_id = id;
                    m_svrInfo = svrInfo;
                    m_ip = m_svrInfo.m_Address;
                    m_port = m_svrInfo.m_Port;
                }

                public int ID
                {
                    get
                    {
                        return m_id;
                    }
                }

                public bool Connect()
                {
                    CloseConnectionService();

                    try
                    {
                        m_connService = new ConnectService(OnRecvServerData,
                                                            OnServerConnectionClose,
                                                            OnOpenNewServerConnection,
                                                            OnOpenConnectionWithSvrInfoFail);
                        //m_connService.RegisterInternalProtocolDealer(InternalProtocolDealer.Instance.Parse);

                        bool result = m_connService.Connect(m_ip, m_port);
                        if (result == true)
                        {
                            SvLogger.Info("Start Connect : Address={0}, Port={1}", m_ip, m_port);
                            return true;
                        }
                        else
                        {
                            m_connService.Channel.Close();
                            m_connService = null;
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        SvLogger.Fatal(ex, "OpenConnectionFail : IP={0}, Port={1}, ErrMsg={2}.", m_ip, m_port, ex.Message);
                    }
                    return false;
                }

                public void Update()
                {
                    m_connService.Update();
                }

                /// <summary>
                /// 接收到服务器协议
                /// </summary>
                /// <param name="buffer">服务器协议</param>
                /// <param name="sessionID">连接SessionID</param>
                private bool OnRecvServerData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
                {
                    if (buffer._readerIdx == buffer._writerIdx)
                        return false;

                    SIReceiveDealManager.Instance.AddData(sessionID, buffer);
                    return true;
                }

                /// <summary>
                /// 当服务组连接断开
                /// </summary>
                /// <param name="sessionID">服务组连接sessionID</param>
                private void OnServerConnectionClose(ConnectionSession connSesion)
                {
                    PtServerInfo svrInfo = null;
                    if (m_connService.GetConnectServerInfo(ref svrInfo))
                    {
                        ReconnectServerInfo reconnSvrInfo = new ReconnectServerInfo(svrInfo);
                        SIReconnectManager.Instance.AddReconnectInfo(reconnSvrInfo);
                    }
                    else
                    {
                        ConnectIPPort ipPort = new ConnectIPPort(m_ip, m_port);
                        SIReconnectManager.Instance.AddReconnectInfo(ipPort);
                    }

                    SIConnServiceManager.Instance.RemoveConnService(m_id);
                }

                /// <summary>
                /// 当有新的服务连接建立
                /// </summary>
                /// <param name="connSession">连接session</param>
                private void OnOpenNewServerConnection(ConnectionSession connSession)
                {
                    SIConnectionSession siconn = new SIConnectionSession(connSession, m_id);
                    SIConnSessionsManager.Instance.AddConnSession(siconn);
                }

                /// <summary>
                /// 当用服务信息连接失败处理
                /// </summary>
                /// <param name="ex">异常信息</param>
                /// <param name="errMsg">错误提示</param>
                /// <param name="svrInfo">服务信息</param>
                /// <param name="connService">失败的连接服务</param>
                private void OnOpenConnectionWithSvrInfoFail(Exception ex, string errMsg, PtServerInfo svrInfo, ConnectService failConnService)
                {
                    SvLogger.Error("Open Connection ServerInfo Fail : SvrType={0}, IP={1}, Port={2}, ErrMsg={3}.", svrInfo.m_Type, svrInfo.m_Address, svrInfo.m_Port, errMsg);

                    //重连
                    ConnectIPPort ipPort = new ConnectIPPort(m_ip, m_port);
                    SIReconnectManager.Instance.AddReconnectInfo(ipPort);

                    SIConnServiceManager.Instance.RemoveConnService(m_id);
                }

                public void CloseConnectionService()
                {
                    if (m_connService != null)
                    {
                        m_connService.Channel.Close();
                        m_connService.Dispose();
                        m_connService = null;
                    }
                }

                private int m_id;
                private string m_ip = null;
                private int m_port = 0;
                private PtServerInfo m_svrInfo;
                private ConnectService m_connService;
            }

            public bool CreateConnService(string ip, int port)
            {
                SIConnService connService = new SIConnService(genID, ip, port);
                bool result = connService.Connect();
                if(result == true)
                    m_connServiceDic[connService.ID] = connService;
                return result;
            }

            public bool CreateConnService(PtServerInfo svrInfo)
            {
                SIConnService connService = new SIConnService(genID, svrInfo);
                bool result = connService.Connect();
                if (result == true)
                    m_connServiceDic[connService.ID] = connService;
                return result;
            }

            public void RemoveConnService(int id)
            {
                m_connServiceDic.Remove(id);
            }

            public void Update()
            {
                foreach (var connService in m_connServiceDic.Values)
                    connService.Update();
            }

            public void CloseAllConnect()
            {
                foreach (var connService in m_connServiceDic.Values)
                    connService.CloseConnectionService();
            }

            private int genID
            {
                get
                {
                    return ++m_curMaxID;
                }
            }

            private Dictionary<int, SIConnService> m_connServiceDic = new Dictionary<int, SIConnService>();

            private int m_curMaxID = 0;

            private static SIConnServiceManager m_instance = new SIConnServiceManager();
            private SIConnServiceManager() { }
        }

        public class SIReconnectManager
        {
            public static SIReconnectManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            public void AddReconnectInfo(ReconnectServerInfo reconnSvrInfo)
            {
                m_reconnSvrInfoList.AddLast(reconnSvrInfo);
            }

            public void AddReconnectInfo(ConnectIPPort reconnIPPort)
            {
                m_reconnIPPortList.AddLast(reconnIPPort);
            }

            public void Update(int curSvrTime)
            {
                while (m_reconnSvrInfoList.First != null)
                {
                    if (m_reconnSvrInfoList.First.Value.ReconnectTime < curSvrTime)
                    {
                        //重连
                        if(SIConnServiceManager.Instance.CreateConnService(m_reconnSvrInfoList.First.Value.m_SvrInfo) == false)
                        {
                            LinkedListNode<ReconnectServerInfo> reconnSvrInfoNode = m_reconnSvrInfoList.First;
                            ReconnectServerInfo newInfo = new ReconnectServerInfo(reconnSvrInfoNode.Value.m_SvrInfo);
                            m_reconnSvrInfoList.AddLast(newInfo);
                        }
                        m_reconnSvrInfoList.RemoveFirst();
                    }
                    else
                        break;
                }

                while(m_reconnIPPortList.First != null)
                {
                    if (m_reconnIPPortList.First.Value.ReconnectTime < curSvrTime)
                    {
                        //重连
                        if(SIConnServiceManager.Instance.CreateConnService(m_reconnIPPortList.First.Value.m_IP, m_reconnIPPortList.First.Value.m_Port) == false)
                        {
                            LinkedListNode<ConnectIPPort> reconnIPPortNode = m_reconnIPPortList.First;
                            ConnectIPPort newInfo = new ConnectIPPort(reconnIPPortNode.Value.m_IP, reconnIPPortNode.Value.m_Port);
                            m_reconnIPPortList.AddLast(newInfo);
                        }
                        m_reconnIPPortList.RemoveFirst();
                    }
                    else
                        break;
                }
            }

            private LinkedList<ReconnectServerInfo> m_reconnSvrInfoList = new LinkedList<ReconnectServerInfo>();
            private LinkedList<ConnectIPPort> m_reconnIPPortList = new LinkedList<ConnectIPPort>();

            private static SIReconnectManager m_instance = new SIReconnectManager();
            private SIReconnectManager() { }
        }

        public class SIConnSessionsManager
        {
            public static SIConnSessionsManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            public void AddConnSession(SIConnectionSession connSession)
            {
                m_sidConnSessionDic[connSession.SessionID] = connSession;
            }

            public void OnConnectionClose(int sessionID)
            {
                m_sidConnSessionDic.Remove(sessionID);
            }

            public void UpdateServerConnSession(int sessionID, PtServerInfo svrInfo)
            {
                SIConnectionSession connSession;
                if(m_sidConnSessionDic.TryGetValue(sessionID, out connSession))
                    connSession.SetServerInfo(svrInfo);
            }

            public void Send2Server(int sid, ByteArray sendData)
            {
                SIConnectionSession connSession;
                if(m_sidConnSessionDic.TryGetValue(sid, out connSession))
                    connSession.SendData(sendData);
                else
                    SvLogger.Error("Send2Server : Not Find Server Connection Session : SessionID={0}.", sid);
            }

            private Dictionary<int, SIConnectionSession> m_sidConnSessionDic = new Dictionary<int, SIConnectionSession>();

            private static SIConnSessionsManager m_instance = new SIConnSessionsManager();
            private SIConnSessionsManager() { }
        }

        /// <summary>
        /// 接收协议数据管理
        /// </summary>
        public class SIReceiveDealManager
        {
            public static SIReceiveDealManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            public void AddData(int sessionID, ByteArray data)
            {
                ReceiveData receData = new ReceiveData(sessionID, data);
                lock (m_netDataListLockObj)
                    m_netDataList.Add(receData);
            }

            public List<ReceiveData> DealData()
            {
                lock (m_netDataListLockObj)
                {
                    m_mainDataList.AddRange(m_netDataList);
                    m_netDataList.Clear();
                }
                return m_mainDataList;
            }

            private object m_netDataListLockObj = new object();
            private List<ReceiveData> m_netDataList = new List<ReceiveData>();
            public List<ReceiveData> m_mainDataList = new List<ReceiveData>();
            private static SIReceiveDealManager m_instance = new SIReceiveDealManager();
            private SIReceiveDealManager() { }
        }

        /// <summary>
        /// 协议处理后数据管理
        /// </summary>
        public class SIProtocolDataDealManager
        {
            public static SIProtocolDataDealManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            public void AddData(ProtocolDealDataBase data)
            {
                lock (m_dataListLockObj)
                    m_mainDataList.Add(data);
            }

            public void DealData()
            {
                lock (m_dataListLockObj)
                {
                    m_netDataList.AddRange(m_mainDataList);
                    m_mainDataList.Clear();
                }

                foreach (var dealDataBase in m_netDataList)
                {
                    switch(dealDataBase.m_DataType)
                    {
                        case eProtocolDealDataType.DataSend:
                            {
                                DataSendInfo sendInfo = (DataSendInfo)dealDataBase;
                                SIConnSessionsManager.Instance.Send2Server(sendInfo.SessionID, sendInfo.SendData);
                            }
                            break;
                        case eProtocolDealDataType.MakeConnectWithIPPort:
                            {
                                MakeConnWithIPPort connIPPort = (MakeConnWithIPPort)dealDataBase;

                                SIConnServiceManager.Instance.CreateConnService(connIPPort.IP, connIPPort.Port);
                            }
                            break;
                        case eProtocolDealDataType.MakeConnectWithSvrInfo:
                            {
                                MakeConnWithSvrInfo connSvrInfo = (MakeConnWithSvrInfo)dealDataBase;

                                SIConnServiceManager.Instance.CreateConnService(connSvrInfo.m_SvrInfo);
                            }
                            break;
                        case eProtocolDealDataType.SetServerInfo:
                            {
                                SetServerInfo setSvrInfo = (SetServerInfo)dealDataBase;
                                SIConnSessionsManager.Instance.UpdateServerConnSession(setSvrInfo.SessionID, setSvrInfo.m_SvrInfo);
                            }
                            break;
                    }
                }

                m_netDataList.Clear();
            }

            private object m_dataListLockObj = new object();
            private List<ProtocolDealDataBase> m_mainDataList = new List<ProtocolDealDataBase>();
            private List<ProtocolDealDataBase> m_netDataList = new List<ProtocolDealDataBase>();
            private static SIProtocolDataDealManager m_instance = new SIProtocolDataDealManager();
            private SIProtocolDataDealManager() { }
        }

        #region ReceiveData

        public class ReceiveData
        {
            public ReceiveData(int sessionID, ByteArray data)
            {
                m_sessionID = sessionID;
                Data = data;
            }

            public int SessionID
            {
                get
                {
                    return m_sessionID;
                }
            }

            private int m_sessionID;
            public ByteArray Data;
        }

        #endregion

        #region Protocol Deal Data

        public enum eProtocolDealDataType
        {
            MakeConnectWithIPPort = 1,      //通过IP、端口建立连接
            MakeConnectWithSvrInfo = 2,     //通过服务信息建立连接
            SetServerInfo = 3,              //设置服务信息
            DataSend = 4,                   //数据发送
        }

        public class ProtocolDealDataBase
        {
            public ProtocolDealDataBase(eProtocolDealDataType dataType)
            {
                m_DataType = dataType;
            }

            public eProtocolDealDataType m_DataType;
        }

        public class MakeConnWithIPPort : ProtocolDealDataBase
        {
            public MakeConnWithIPPort(string ip, int port)
                : base(eProtocolDealDataType.MakeConnectWithIPPort)
            {
                m_ip = ip;
                m_port = port;
            }

            public string IP
            {
                get
                {
                    return m_ip;
                }
            }

            public int Port
            {
                get
                {
                    return m_port;
                }
            }

            private string m_ip;
            private int m_port;
        }

        public class MakeConnWithSvrInfo : ProtocolDealDataBase
        {
            public MakeConnWithSvrInfo(PtServerInfo svrInfo)
                : base(eProtocolDealDataType.MakeConnectWithSvrInfo)
            {
                m_SvrInfo = svrInfo;
            }

            public PtServerInfo m_SvrInfo;
        }

        public class SetServerInfo : ProtocolDealDataBase
        {
            public SetServerInfo(int sessionID, PtServerInfo svrInfo)
                : base(eProtocolDealDataType.SetServerInfo)
            {
                m_sessionID = sessionID;
                m_SvrInfo = svrInfo;
            }

            public int SessionID
            {
                get
                {
                    return m_sessionID;
                }
            }

            private int m_sessionID;
            public PtServerInfo m_SvrInfo;
        }

        public class DataSendInfo : ProtocolDealDataBase
        {
            public DataSendInfo(int sid, ByteArray sendData)
                : base(eProtocolDealDataType.DataSend)
            {
                m_sid = sid;
                SendData = sendData;
            }

            public int SessionID
            {
                get
                {
                    return m_sid;
                }
            }

            private int m_sid = 0;
            public ByteArray SendData;
        }

        #endregion
#endif
    }
}
