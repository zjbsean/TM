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
using System.Diagnostics;

namespace ServerCommon
{
    namespace Network
    {
        //错误类型枚举
        public enum eErrType
        {
            None = 0,
            Not_Set_Protocol_Dealer,
            Listen_Fail,
        }

        /// <summary>
        /// 协议处理委托
        /// </summary>
        /// <param name="buffer">协议数据</param>
        /// <param name="sessionID">消息来源SessionID</param>
        /// <returns>是否解析处理成功</returns>
        public delegate bool ProtocolDealDelegate(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType);
        public delegate bool ProtocolDealClassDelegate(RecvedProtocolDataInfo protoData);

        /// <summary>
        /// 连接断开处理委托
        /// </summary>
        /// <param name="sessionID">连接ID</param>
        public delegate void ConnectOnCloseDelegate(ConnectionSession connSession);

        /// <summary>
        /// 连接建立处理委托
        /// </summary>
        /// <param name="connSession">连接session</param>
        public delegate void OpenNewConnectDelegate(ConnectionSession connSession);

        /// <summary>
        /// 用IP、端口打开连接失败代理
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="errMsg">错误信息</param>
        /// <param name="ip">连接IP</param>
        /// <param name="port">连接端口</param>
        /// <param name="connService">失败的连接服务</param>
        public delegate void OpenConnectionFailWithIP_PortDelegate(Exception ex, string errMsg, string ip, int port, ConnectService connService);

        /// <summary>
        /// 用服务信息打开连接失败代理
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="errMsg">错误信息</param>
        /// <param name="svrInfo">连接服务信息</param>
        /// <param name="connService">失败的连接服务</param>
        public delegate void OpenConnectionFailWithSvrInfoDelegate(Exception ex, string errMsg, PtServerInfo svrInfo, ConnectService connService);

        /// <summary>
        /// 重连服务信息
        /// </summary>
        public class ReconnectServerInfo
        {
            public ReconnectServerInfo(PtServerInfo svrInfo)
            {
                m_SvrInfo = svrInfo;
                m_reconnectTime = Environment.TickCount + 3000;
            }

            public int ReconnectTime
            {
                get
                {
                    return m_reconnectTime;
                }
            }

            private int m_reconnectTime;
            public PtServerInfo m_SvrInfo;
        }

        public class ConnectIPPort
        {
            public ConnectIPPort(string ip, int port)
            {
                m_IP = ip;
                m_Port = port;
                m_reconnectTime = Environment.TickCount + 3000;
            }

            public int ReconnectTime
            {
                get
                {
                    return m_reconnectTime;
                }
            }

            private int m_reconnectTime;
            public string m_IP;
            public int m_Port;
        }

        //连接Session
        public class ConnectionSession : com.ideadynamo.foundation.game.SimpleNetworkSession
        {
            public ConnectionSession()
            {
                m_CreateTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;
                OnClose += new VoidDelegate(OnConnectionClose);

                m_checker.Start();
            }

            //协议处理委托对象
            private ProtocolDealDelegate m_protocolDealer = null;

            //连接断开处理委托对象
            private ConnectOnCloseDelegate m_connectOnCloseDealer = null;

            /// <summary>
            /// 注册协议处理者
            /// </summary>
            /// <param name="protocolDealer">协议处理者</param>
            public void RegisterProtocolDealer(ProtocolDealDelegate protocolDealer)
            {
                m_protocolDealer = protocolDealer;
            }

            public void SetStratFrequentlyCheckWhenReceiveCount(byte datda)
            {

            }

            /// <summary>
            /// 注册连接断开处理者
            /// </summary>
            /// <param name="onCloseDealer">连接断开处理者</param>
            public void RegisterOnCloseDealer(ConnectOnCloseDelegate onCloseDealer)
            {
                m_connectOnCloseDealer = onCloseDealer;
            }

            /// <summary>
            /// 连接断开处理
            /// </summary>
            public void OnConnectionClose()
            {
                if (m_connectOnCloseDealer != null)
                    m_connectOnCloseDealer(this);
            }

            /// <summary>
            /// 收到协议数据
            /// </summary>
            /// <param name="buffer">协议数据</param>
            /// <returns>是否解析处理成功</returns>
            public override bool HandleProtocol(com.ideadynamo.foundation.buffer.ByteArray buffer)
            {
                if (m_startFrequentlyCheckWhenReceiveCount > 0)
                {
                    ++m_curFrequentlyCheckReceiveCount;
                    if (m_curFrequentlyCheckReceiveCount >= m_startFrequentlyCheckWhenReceiveCount)
                    {
                        m_checker.Stop();

                        if (m_checker.ElapsedMilliseconds / m_startFrequentlyCheckWhenReceiveCount < m_maxFrequently)
                            Connection.Channel.Close();

                        m_curFrequentlyCheckReceiveCount = 0;
                        m_checker.Restart();
                    }
                }

                if (m_protocolDealer != null)
                    return m_protocolDealer(buffer, LinkId, m_ConnType);
                return false;
            }

            /// <summary>
            /// 心跳超时
            /// </summary>
            /// <param name="slowHeartbeat"></param>
            protected override void OnDeadHeartbeat(int slowHeartbeat)
            {
                SvLogger.Debug("OnDeadHeartbeat: SessionID={0}.", LinkId);
                Connection.Close();
            }

            protected override int UpdateImpl()
            {
                return 0;
            }

            //连接建立时间
            public int m_CreateTime = -1;

            public int m_GatewaySid = -1;

            //连接类型
            public ClientProtocolDealManager.eConnectionType m_ConnType = ClientProtocolDealManager.eConnectionType.Uncheck;

            private byte m_startFrequentlyCheckWhenReceiveCount = 0;        //当收到几条数据后开始频繁检测
            private byte m_curFrequentlyCheckReceiveCount = 0;              //当前平凡检测收到数据次数
            private Stopwatch m_checker = new Stopwatch();
            private const byte m_maxFrequently = 100;                       //协议接受最高频率
        }

        //侦听服务
        public class ListenService : SimpleNetworkService<ConnectionSession>
        {
            //协议处理代理对象
            public ProtocolDealDelegate m_ProtocolDealer = null;

            //新连接建立处理委托对象
            private OpenNewConnectDelegate m_openNewConnectDealer = null;

            //连接断开处理委托对象
            private ConnectOnCloseDelegate m_connectOnCloseDealer = null;

            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="protocolDealer"></param>
            public ListenService(ProtocolDealDelegate protocolDealer, OpenNewConnectDelegate newConnOpen, ConnectOnCloseDelegate closeDealer)
                : this(protocolDealer, newConnOpen, closeDealer, false, 0)
            {

            }

            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="protocolDealer">协议解析处理者</param>
            /// <param name="newConnOpen">新连接建立处理者</param>
            /// <param name="isOpenCheck">是否开启验证超时</param>
            /// <param name="checkLimitTime">超时验证时间</param>
            public ListenService(ProtocolDealDelegate protocolDealer, OpenNewConnectDelegate newConnOpen, ConnectOnCloseDelegate closeDealer, bool isOpenCheck, int checkLimitTime)
            {
                m_ProtocolDealer = protocolDealer;
                //m_isOpenCheck = isOpenCheck;
                //m_checkLimitTime = checkLimitTime;
                m_openNewConnectDealer = newConnOpen;
                m_connectOnCloseDealer = closeDealer;
            }

            /// <summary>
            /// 创建侦听
            /// </summary>
            /// <param name="port">侦听端口</param>
            /// <param name="maxConnection">侦听结果</param>
            /// <returns> 侦听是否异常 </returns>
            public bool Listion(int port, int maxConnection)
            {
                NetworkConfiguration = new TcpServerConfig(port, maxConnection);
                return Initialize();
            }

            /// <summary>
            /// 暂时不用的功能
            /// </summary>
            /// <returns></returns>
            protected override int UpdateImpl()
            {
                return 1;
            }

            /// <summary>
            /// 有一个新的连接
            /// </summary>
            /// <param name="param">连接接口</param>
            protected override void OnOpenNewConnection(IConnection param)
            {
                ConnectionSession connectSession = param.SessionPolicy as ConnectionSession;
                connectSession.Service = this;
                connectSession.RegisterProtocolDealer(m_ProtocolDealer);
                connectSession.RegisterOnCloseDealer(new ConnectOnCloseDelegate(OnOneConnectionClose));

                if (m_openNewConnectDealer != null)
                    m_openNewConnectDealer(connectSession);

                //if (m_isOpenCheck == true)
                //    AddUnCheckSession(connectSession);

                connectSession.EnableHeartBeat(true, true);
                //SvLogger.Debug("####### Open New Connection!");
            }

            /// <summary>
            /// 添加未连接验证的连接
            /// </summary>
            /// <param name="connectSession">连接ID</param>
            //public void AddUnCheckSession(ConnectionSession connectSession)
            //{
            //    LinkedListNode<ConnectionSession> connectNode = m_unCheckSessionList.AddLast(connectSession);
            //    m_unCheckSessionNodeDic.Remove(connectSession.LinkId);
            //    m_unCheckSessionNodeDic.Add(connectSession.LinkId, connectNode);
            //}

            /// <summary>
            /// 删除未连接验证的连接
            /// </summary>
            /// <param name="sessionID">连接ID</param>
            //public void RemoveUnCheckSession(int sessionID)
            //{
            //    LinkedListNode<ConnectionSession> connectNode;
            //    if (m_unCheckSessionNodeDic.TryGetValue(sessionID, out connectNode))
            //    {
            //        m_unCheckSessionList.Remove(connectNode);
            //        m_unCheckSessionNodeDic.Remove(sessionID);
            //    }
            //}

            /// <summary>
            /// 有连接断开
            /// </summary>
            /// <param name="sessionID"> 连接SessionID </param>
            private void OnOneConnectionClose(ConnectionSession connSession)
            {
                //if (m_isOpenCheck == true)
                //    RemoveUnCheckSession(sessionID);

                if (m_connectOnCloseDealer != null)
                    m_connectOnCloseDealer(connSession);
            }

            /// <summary>
            /// 连接验证超时更新
            /// </summary>
            //public void ConnectionCheckLimitUpdate()
            //{
            //    if (m_isOpenCheck == true)
            //    {
            //        //while (m_unCheckSessionList.First != null)
            //        //{
            //        //    if (Convert.ToInt32((DateTime.Now - m_unCheckSessionList.First.Value.m_CreateTime).TotalSeconds) > m_checkLimitTime)
            //        //        m_unCheckSessionList.First.Value.Connection.Close();
            //        //    else
            //        //        break;
            //        //}
            //    }
            //}

            //未验证的连接队列
            //private LinkedList<ConnectionSession> m_unCheckSessionList = new LinkedList<ConnectionSession>();
            //未验证的连接队列节点字典
            //private Dictionary<int, LinkedListNode<ConnectionSession>> m_unCheckSessionNodeDic = new Dictionary<int,LinkedListNode<ConnectionSession>>();

            //是否开启连接验证超时
            //private bool m_isOpenCheck = false;
            //连接验证超时时间
            private int m_checkLimitTime = 0;
        }

        //连接服务
        public class ConnectService : SimpleNetworkService<ConnectionSession>
        {
            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="protocolDealer">协议处理代理对象</param>
            /// <param name="serverConnCloseDealer">连接关闭处理</param>
            /// <param name="serverConnectionOpenDealer">连接建立处理</param>
            /// <param name="openWithIP_PortFailDealer">用IP、端口连接失败处理者</param>
            public ConnectService(ProtocolDealDelegate protocolDealer, ConnectOnCloseDelegate serverConnCloseDealer, OpenNewConnectDelegate serverConnectionOpenDealer, OpenConnectionFailWithIP_PortDelegate openWithIP_PortFailDealer)
            {
                m_ProtocolDealer = protocolDealer;
                m_openServerConnectionDealer = serverConnectionOpenDealer;
                m_connectOnCloseDealer = serverConnCloseDealer;
                m_openWithIP_PortFailDealer = openWithIP_PortFailDealer;
            }

            /// <summary>
            /// 构造
            /// </summary>
            /// <param name="protocolDealer">协议处理代理对象</param>
            /// <param name="serverConnCloseDealer">连接关闭处理</param>
            /// <param name="serverConnectionOpenDealer">连接建立处理</param>
            /// <param name="openWithSvrInfoFailDealer">用服务信息连接失败处理者</param>
            public ConnectService(ProtocolDealDelegate protocolDealer, ConnectOnCloseDelegate serverConnCloseDealer, OpenNewConnectDelegate serverConnectionOpenDealer, OpenConnectionFailWithSvrInfoDelegate openWithSvrInfoFailDealer)
            {
                m_ProtocolDealer = protocolDealer;
                m_openServerConnectionDealer = serverConnectionOpenDealer;
                m_connectOnCloseDealer = serverConnCloseDealer;
                m_openWithSvrInfoFailDealer = openWithSvrInfoFailDealer;
            }

            /// <summary>
            /// 设置连接服务器信息
            /// </summary>
            /// <param name="svrInfo">服务信息</param>
            public void SetConnectServerInfo(ref PtServerInfo svrInfo)
            {
                m_isSetConnSvrInfo = true;
                m_connSvrInfo = svrInfo;
            }

            /// <summary>
            /// 获取连接服务的信息
            /// </summary>
            /// <param name="svrInfo">服务信息</param>
            /// <returns>是否存在</returns>
            public bool GetConnectServerInfo(ref PtServerInfo svrInfo)
            {
                if (m_isSetConnSvrInfo == false)
                    return false;

                svrInfo = m_connSvrInfo;

                return true;
            }

            /// <summary>
            /// 注册内部协议处理者
            /// </summary>
            /// <param name="internalDealer"></param>
            public void RegisterInternalProtocolDealer(ProtocolDealClassDelegate internalDealer)
            {
                m_internalProtocolDealer = internalDealer;
            }

            /// <summary>
            /// 连接
            /// </summary>
            /// <param name="ip"> 连接IP </param>
            /// <param name="port"> 连接端口 </param>
            /// <returns> 连接操作是否异常 </returns>
            public bool Connect(string ip, int port)
            {
                NetworkConfiguration = new TcpClientConfig(ip, port);
                m_ip = ip;
                m_port = port;
                bool initResult = Initialize();
                Channel.OnOpenFailed += new ParamedDelegate<Exception, string>(openConnectionFail);
                return initResult;
            }

            /// <summary>
            /// 暂时不用的功能
            /// </summary>
            /// <returns></returns>
            protected override int UpdateImpl()
            {
                return 1;
            }

            /// <summary>
            /// 有一个新的连接
            /// </summary>
            /// <param name="param"> 连接接口 </param>
            protected override void OnOpenNewConnection(IConnection param)
            {
                ConnectionSession connectSession = param.SessionPolicy as ConnectionSession;
                connectSession.RegisterProtocolDealer(m_ProtocolDealer);
                connectSession.RegisterOnCloseDealer(m_connectOnCloseDealer);

                //if (m_openServerConnectionDealer != null)
                //    m_openServerConnectionDealer(connectSession);

                //if (m_isSetConnSvrInfo == true)
                //{
                //    if (m_connSvrInfo.m_Type != eServerType.CENTER &&
                //        m_connSvrInfo.m_Type != eServerType.PROTAL &&
                //        m_connSvrInfo.m_Type != eServerType.GM &&
                //        m_connSvrInfo.m_Type != eServerType.PLATFORM_DOCKING &&
                //        m_connSvrInfo.m_Type != eServerType.MONITEORNODE &&
                //        m_connSvrInfo.m_Type != eServerType.MONITEORSERVER &&
                //        m_connSvrInfo.m_Type != eServerType.GIFTCODE)
                //        NetworkManager.Instance.SendMessageToServer(connectSession.LinkId, InternalProtocolServerHelper.NotifyConnectorSvr(SvrCommCfg.Instance.ServerInfo));
                //}

                connectSession.EnableHeartBeat(false, true);
            }

            /// <summary>
            /// 连接开启失败
            /// </summary>
            /// <param name="ex">异常对象</param>
            /// <param name="errMsg">错误信息</param>
            private void openConnectionFail(Exception ex, string errMsg)
            {
                if (m_openWithIP_PortFailDealer != null)
                    m_openWithIP_PortFailDealer(ex, errMsg, m_ip, m_port, this);
                if (m_openWithSvrInfoFailDealer != null)
                    m_openWithSvrInfoFailDealer(ex, errMsg, m_connSvrInfo, this);
            }

            //是否设置服务连接信息
            private bool m_isSetConnSvrInfo = false;

            //连接服务信息
            private PtServerInfo m_connSvrInfo;

            //连接IP
            private string m_ip;

            //连接端口
            private int m_port;

            //服务内部协议处理委托对象
            private ProtocolDealClassDelegate m_internalProtocolDealer = null;

            //协议处理代理对象
            public ProtocolDealDelegate m_ProtocolDealer = null;

            //新服务连接处理对象
            private OpenNewConnectDelegate m_openServerConnectionDealer = null;

            //连接断开处理委托对象
            private ConnectOnCloseDelegate m_connectOnCloseDealer = null;

            //用IP、端口连接失败处理委托对象
            OpenConnectionFailWithIP_PortDelegate m_openWithIP_PortFailDealer = null;

            //用服务信息连接失败处理委托对象
            OpenConnectionFailWithSvrInfoDelegate m_openWithSvrInfoFailDealer = null;
        }

        //连接超时检测信息
        class ConnectTimeOutCheckInfo
        {
            public ConnectTimeOutCheckInfo(int sid, int openTicket)
            {
                m_SessionID = sid;
                m_OpenTicket = openTicket;
            }
            public int m_SessionID;
            public int m_OpenTicket;
        }


        /// <summary>
        /// 当有服务前来注册
        /// </summary>
        /// <param name="svrInfo"></param>
        public delegate void OnServerRegistDelegate(PtServerInfo svrInfo);

        /// <summary>
        /// 当有服务失联
        /// </summary>
        /// <param name="svrInfo"></param>
        public delegate void OnServerDisConnectDelegate(PtServerInfo svrInfo);

        //public class NetworkManager
        //{
        //    /// <summary>
        //    /// GUID转PtGuid
        //    /// </summary>
        //    /// <param name="id">Guid</param>
        //    /// <param name="ptID">PTGuid</param>
        //    public static PtGuid GuidConvertToPtGuid(Guid id)
        //    {
        //        PtGuid ptID = new PtGuid();
        //        ValBytes vbs = new ValBytes();
        //        byte[] idBytes = id.ToByteArray();
        //        vbs._b0 = idBytes[0];
        //        vbs._b1 = idBytes[1];
        //        vbs._b2 = idBytes[2];
        //        vbs._b3 = idBytes[3];
        //        ptID.a = vbs._intVal;

        //        vbs._b0 = idBytes[4];
        //        vbs._b1 = idBytes[5];
        //        ptID.b = vbs._shortVal;

        //        vbs._b0 = idBytes[6];
        //        vbs._b1 = idBytes[7];
        //        ptID.c = vbs._shortVal;

        //        ptID.d = idBytes[8];
        //        ptID.e = idBytes[9];
        //        ptID.f = idBytes[10];
        //        ptID.g = idBytes[11];
        //        ptID.h = idBytes[12];
        //        ptID.i = idBytes[13];
        //        ptID.j = idBytes[14];
        //        ptID.k = idBytes[15];

        //        return ptID;
        //    }

        //    /// <summary>
        //    /// PtGuid转Guid
        //    /// </summary>
        //    /// <param name="ptID"></param>
        //    /// <returns></returns>
        //    public static Guid PtGuidConvertToGuid(PtGuid ptID)
        //    {
        //        return new Guid(ptID.a, ptID.b, ptID.c, ptID.d, ptID.e, ptID.f, ptID.g, ptID.h, ptID.i, ptID.j, ptID.k);
        //    }
        //}

//#if SocketConnectMode
        //连接管理
        public class NetworkManager
        {
            #region Public Functions

            /// <summary>
            /// 开启客户端侦听服务
            /// </summary>
            /// <param name="port">端口</param>
            /// <param name="isOpenCheck">是否开启连接验证超时处理</param>
            /// <param name="checkLimitTime">验证限制时间</param>
            /// <param name="maxConnections">最大连接数</param>
            /// <param name="isAsync">协议是否是异步处理</param>
            /// <returns>侦听建立返回</returns>
            public bool OpenClientListen(int port, bool isOpenCheck, int checkLimitTime, int maxConnections, bool isDefaultLongConnect, bool isSync, OpenNewConnectDelegate longConnectionOpen, ConnectOnCloseDelegate longConnectionClose)
            {
                lock (m_networkLock)
                {
                    if (isDefaultLongConnect == false)
                    {
                        m_outShortConnectionListen = new ListenService(new ProtocolDealDelegate(OnRecvShortConnectionClientData),
                                                                        new OpenNewConnectDelegate(OnOpenNewClientShortConnection),
                                                                        new ConnectOnCloseDelegate(OnClientShortConnectionClose),
                                                                        isOpenCheck, checkLimitTime);
                        bool result = m_outShortConnectionListen.Listion(port, maxConnections);
                        if (result == false)
                        {
                            //TODO:状态LOG记录显示
                            m_outShortConnectionListen = null;
                            return false;
                        }
                        m_isShortConnSync = isSync;
                    }
                    else
                    {
                        m_outLongConnectionListen = new ListenService(new ProtocolDealDelegate(OnRecvLongConnectionClientData),
                                                                        new OpenNewConnectDelegate(OnOpenNewClientLongConnection),
                                                                        new ConnectOnCloseDelegate(OnClientLongConnectionClose),
                                                                        isOpenCheck, checkLimitTime);
                        bool result = m_outLongConnectionListen.Listion(port, maxConnections);
                        if (result == false)
                        {
                            m_outLongConnectionListen = null;
                            return false;
                        }
                        m_isLongConnSync = isSync;
                    }

                    m_serverLogicOnOpenNewConnectionDeal = longConnectionOpen;
                    m_serverLogicOnConnectionCloseDeal = longConnectionClose;

                    return true;
                }
            }
            
            /// <summary>
            /// 关闭客户端侦听服务
            /// </summary>
            public void CloseClientListen()
            {
                lock (m_networkLock)
                {
                    if (m_outShortConnectionListen != null)
                    {
                        m_outShortConnectionListen.Close();
                        m_outShortConnectionListen = null;
                    }
                    if (m_outLongConnectionListen != null)
                    {
                        m_outLongConnectionListen.Close();
                        m_outLongConnectionListen = null;
                    }
                }
            }

            /// <summary>
            ///  开启服务组侦听服务
            /// </summary>
            /// <param name="protocolDealer">协议处理代理对象</param>
            /// <param name="port">端口</param>
            /// <returns>侦听建立返回</returns>
            //public bool OpenServerListen(   int port,
            //                                OnServerRegistDelegate registDeal,
            //                                OnServerDisConnectDelegate disconnectDeal)
            //{
            //    lock (m_networkLock)
            //    {
            //        m_inListen = new ListenService(new ProtocolDealDelegate(OnRecvServerData),
            //                                new OpenNewConnectDelegate(OnOpenNewServerConnection),
            //                                new ConnectOnCloseDelegate(OnServerConnectionClose), false, 0);

            //        m_serverRegistDelegate += registDeal;
            //        m_serverDisconnectDelegate += disconnectDeal;

            //        bool result = m_inListen.Listion(port, 1000);
            //        if (result == false)
            //            //TODO:状态LOG记录显示
            //            return false;
            //    }
            //    return true;
            //}

            /// <summary>
            /// 关闭服务组侦听服务
            /// </summary>
            public void CloseServerListen()
            {
                lock (m_networkLock)
                {
                    if (m_inListen != null)
                    {
                        m_inListen.Close();
                        m_inListen = null;
                    }
                }
            }

            /// <summary>
            /// 开启服务组连接
            /// </summary>
            /// <param name="svrInfo">服务信息</param>
            /// <returns>连接建立返回</returns>
            //public bool OpenServerConnection(string address, int port)
            //{
            //    lock(m_networkLock)
            //    {
            //        ConnectService connection = new ConnectService( OnRecvServerData, 
            //                                                        OnServerConnectionClose, 
            //                                                        OnOpenNewServerConnection, 
            //                                                        OnOpenConnectionWithIP_PortFail);
            //        connection.RegisterInternalProtocolDealer(InternalProtocolDealer.Instance.Parse);

            //        bool result = connection.Connect(address, port);
            //        if (result == true)
            //        //TODO:连接请求状态LOG
            //        {
            //            m_tempConnectList.Add(connection);
            //            return true;
            //        }
            //        return false;
            //    }
            //}

            /// <summary>
            /// 开启服务组连接
            /// </summary>
            /// <param name="svrInfo">服务信息</param>
            /// <returns>连接建立返回</returns>
            //public bool OpenRegServerConnection(PtServerInfo svrInfo)
            //{
            //    lock (m_networkLock)
            //    {
            //        ConnectService connection = new ConnectService( OnRecvServerData, 
            //                                                        OnServerConnectionClose, 
            //                                                        OnOpenNewServerConnection, 
            //                                                        OnOpenConnectionWithSvrInfoFail);
            //        connection.RegisterInternalProtocolDealer(InternalProtocolDealer.Instance.Parse);
            //        connection.SetConnectServerInfo(ref svrInfo);

            //        bool result = connection.Connect(svrInfo.m_Address, svrInfo.m_Port);
            //        if (result == true)
            //        //TODO:连接请求状态LOG
            //        {
            //            SvLogger.Info("Start Reconnect : ServerType={0}, Address={1}, Port={2}", svrInfo.m_Type, svrInfo.m_Address, svrInfo.m_Port);
            //            m_tempConnectList.Add(connection);
            //            return true;
            //        }
            //        else
            //        {
            //            connection.Channel.Close();
            //            connection = null;
            //        }
            //        return false;
            //    }
            //}

            /// <summary>
            /// 关闭服务组连接
            /// </summary>
            /// <param name="sessionID">连接SessionID</param>
            //public void CloseServerConnection(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        ConnectionSession closeConnection = null;
            //        if (m_serverConnectSessionDic.TryGetValue(sessionID, out closeConnection))
            //        {
            //            closeConnection.Connection.Channel.Close();
            //            ConnectService connService = closeConnection.Service as ConnectService;
            //            connService.Dispose();
            //            //m_tempLostConnectList.Add(connService);
            //            m_serverConnectSessionDic.Remove(sessionID);
            //        }
            //    }
            //}


            /// <summary>
            /// 检测服务器连接关闭
            /// </summary>
            /// <param name="sessionID"></param>
            /// <returns></returns>
            //public bool CheckServerConnectionAlive(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        return m_serverConnectSessionDic.ContainsKey(sessionID);
            //    }
            //}

            /// <summary>
            /// 模拟服务器连接丢失(测试用)
            /// </summary>
            /// <param name="sessionID">连接SessionID</param>
            //public void SimulatedServerConnectionLost(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        ConnectionSession closeConnection = null;
            //        if (m_serverConnectSessionDic.TryGetValue(sessionID, out closeConnection))
            //            closeConnection.Connection.Close();
            //    }
            //}

            /// <summary>
            ///  关闭服务组侦听到的连接
            /// </summary>
            /// <param name="sessionID">连接SessionID</param>
            //public void CloseServerListenConnect(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        ConnectionSession conn = null;
            //        if (m_serverConnectSessionDic.TryGetValue(sessionID, out conn))
            //        {
            //            conn.Connection.Close();
            //            m_serverConnectSessionDic.Remove(sessionID);
            //        }
            //    }
            //}

            /// <summary>
            /// 关闭指定客户端长连接
            /// </summary>
            /// <param name="sessionID">连接SessionID</param>
            //public void CloseClientListenLongConnect(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        Guid guid;
            //        if (m_playerSessionIDGuidLongConnDic.TryGetValue(sessionID, out guid))
            //        {
            //            m_playerSessionIDGuidLongConnDic.Remove(sessionID);
            //            m_playerGuidSessionIDLongConnDic.Remove(guid);
                        
            //            m_longConnSidSet.Remove(sessionID);
            //        }

            //        int playerFlagID;
            //        if (m_playerSessionIDFlagIDLongConnDic.TryGetValue(sessionID, out playerFlagID))
            //        {
            //            m_playerSessionIDFlagIDLongConnDic.Remove(sessionID);
            //            m_playerFlagIDSessionIDLongConnDic.Remove(playerFlagID);
            //        }
                    
            //        m_outLongConnectionListen.DropSession(sessionID);
            //    }
            //}

            //public void CloseClientListenLongConnect(Guid userID)
            //{
            //    lock (m_networkLock)
            //    {
            //        int sid;
            //        if (m_playerGuidSessionIDLongConnDic.TryGetValue(userID, out sid))
            //        {
            //            m_playerSessionIDGuidLongConnDic.Remove(sid);
            //            m_playerGuidSessionIDLongConnDic.Remove(userID);
            //            m_longConnSidSet.Remove(sid);

            //            int playerFlagID;
            //            if(m_playerSessionIDFlagIDLongConnDic.TryGetValue(sid, out playerFlagID))
            //            {
            //                m_playerSessionIDFlagIDLongConnDic.Remove(sid);
            //                m_playerFlagIDSessionIDLongConnDic.Remove(playerFlagID);
            //            }
            //        }
                    
            //        m_outLongConnectionListen.DropSession(sid);
            //    }
            //}

            /// <summary>
            /// 关闭指定客户端短连接
            /// </summary>
            /// <param name="sessionID"></param>
            //public void CloseClientListenShortConnect(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        Guid guid;
            //        if (m_playerSessionIDGuidShortConnDic.TryGetValue(sessionID, out guid))
            //        {
            //            m_playerSessionIDGuidShortConnDic.Remove(sessionID);
            //            m_playerGuidSessionIDShortConnDic.Remove(guid);
            //            m_shortConnSidSet.Remove(sessionID);

            //            int playerFlagID;
            //            if(m_playerSessionIDFlagIDShortConnDic.TryGetValue(sessionID ,out playerFlagID))
            //            {
            //                m_playerSessionIDFlagIDShortConnDic.Remove(sessionID);
            //                m_playerFlagIDSessionIDShortConnDic.Remove(playerFlagID);
            //            }
            //        }
            //        m_outShortConnectionListen.DropSession(sessionID);
            //    }
            //}

            //public void CloseClientListenShortConnect(Guid userID)
            //{
            //    lock (m_networkLock)
            //    {
            //        int sid;
            //        if (m_playerGuidSessionIDShortConnDic.TryGetValue(userID, out sid))
            //        {
            //            m_playerSessionIDGuidShortConnDic.Remove(sid);
            //            m_playerGuidSessionIDShortConnDic.Remove(userID);
            //            m_shortConnSidSet.Remove(sid);

            //            int playerFlagID;
            //            if(m_playerSessionIDFlagIDShortConnDic.TryGetValue(sid, out playerFlagID))
            //            {
            //                m_playerSessionIDFlagIDShortConnDic.Remove(sid);
            //                m_playerFlagIDSessionIDShortConnDic.Remove(playerFlagID);
            //            }
            //        }
            //        m_outShortConnectionListen.DropSession(sid);
            //    }
            //}

            /// <summary>
            /// 运行网络更新
            /// </summary>
            public void RunNetworkUpdate()
            {
                m_networkUpdateThread = new Thread(Update);
                m_networkUpdateThread.Start();
            }

            /// <summary>
            /// 停止运行网络更新
            /// </summary>
            public void StopRunNetworkUpdate()
            {
                lock (m_networkLock)
                {
                    m_run = false;
                    if (m_networkUpdateThread != null)
                    {
                        m_networkUpdateThread.Abort();
                        m_networkUpdateThread.Join(1000);
                    }
                }
            }

            /// <summary>
            /// 检测网络线程是否存活
            /// </summary>
            /// <returns></returns>
            public bool CheckNetworkThreadIsAlive()
            {
                if (m_networkUpdateThread == null)
                    return false;
                return m_networkUpdateThread.IsAlive;
            }

            /// <summary>
            /// 发送消息到服务器
            /// </summary>
            /// <param name="sessionID">服务器</param>
            /// <param name="data">发送数据</param>
            public void SendMessageToServer(int sessionID, ByteArray data)
            {
                lock (m_networkLock)
                {
                    ConnectionSession serverConn;
                    if (m_serverConnectSessionDic.TryGetValue(sessionID, out serverConn))
                    {
                        serverConn.Connection.Send(data);
                    }
                    else
                    {
                        SvLogger.Error("SendMessageToServer Fail : Not Find The Session - SessionID={0}.", sessionID);
                    }
                }
            }

            /// <summary>
            /// 发送消息到服务
            /// </summary>
            /// <param name="svrType">服务类型</param>
            /// <param name="datas">消息数据</param>
            //public void SendMessageToServer(eServerType svrType, ByteArray datas)
            //{
            //    int sessionID = RegServerManager.Instance.GetServerSid(svrType);
            //    SendMessageToServer(sessionID, datas);
            //}

            /// <summary>
            /// 服务发送消息给客户端
            /// </summary>
            /// <param name="userID">玩家GUID</param>
            /// <param name="datas">协议数据</param>
            //public void ServerSendMsgToClientWithPlayerFlagID(long playerFlagID, ByteArray datas, bool isLongConn = false)
            //{
            //    if (SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.GAME &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CENTER &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.WORLDBOSS &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CHAT)
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : This Function Just Be Use In GameServer/CenterServer/WorldBoss/Chat!!!");
            //        return;
            //    }

            //    int sessionID = AllocSvrMap.Instance.GetPlayerSvrSid(playerFlagID, eServerType.GATEWAY);
            //    if (sessionID != -1)
            //    {
            //        SendMessageToServer(sessionID, com.tieao.mmo.gateway4server.client.GateWay4ServerClientHelper.AS_SendMsg2ClientWithPlayerFlagID(playerFlagID, datas, isLongConn));
            //    }
            //    else
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : Not Find The Connection SessionID The GateWay Player In. UserID={0}.", playerFlagID);
            //        //try
            //        //{
            //        //    throw new Exception("Not Find The Connection SessionID The GateWay Player In");
            //        //}
            //        //catch (Exception ex)
            //        //{
            //        //    SvLogger.Error(ex.StackTrace);
            //        //}
            //    }
            //}

            /// <summary>
            /// 服务发送消息给客户端
            /// </summary>
            /// <param name="playerFlagID"></param>
            /// <param name="datas"></param>
            /// <param name="isLongConn"></param>
            //public void ServerSendMsgToClientWithLoginName(string loginName, ByteArray datas, bool isLongConn = false)
            //{
            //    if (SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.GAME &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CENTER &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.WORLDBOSS &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CHAT)
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : This Function Just Be Use In GameServer/CenterServer/WorldBoss/Chat!!!");
            //        return;
            //    }

            //    int sessionID = AllocSvrMap.Instance.GetPlayerSvrSid(loginName, eServerType.GATEWAY);
            //    if (sessionID != -1)
            //    {
            //        SendMessageToServer(sessionID, com.tieao.mmo.gateway4server.client.GateWay4ServerClientHelper.AS_SendMsg2ClientWithLoginName(loginName, datas, isLongConn));
            //    }
            //    else
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : Not Find The Connection SessionID The GateWay Player In. loginName={0}.", loginName);
            //        //try
            //        //{
            //        //    throw new Exception("Not Find The Connection SessionID The GateWay Player In");
            //        //}
            //        //catch (Exception ex)
            //        //{
            //        //    SvLogger.Error(ex.StackTrace);
            //        //}
            //    }
            //}

            /// <summary>
            /// 服务发送消息给客户端（仅供CenterServer使用）
            /// </summary>
            /// <param name="gateWaySessionID">网关服务链接SessionID</param>
            /// <param name="clientSessionID">网关服务与该客户端链接SessionID</param>
            /// <param name="datas">发送给客户端的协议数据</param>
            //public void ServerSendMsgToClientWithClientSid(int gatewaySessionID, int XDGatewaySessionID, int clientSessionID, ByteArray datas)
            //{
            //    if (SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CENTER)
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : This Function Just Be Use In CenterServer!!!");
            //        return;
            //    }

            //    SendMessageToServer(gatewaySessionID, com.tieao.mmo.gateway4server.client.GateWay4ServerClientHelper.AS_CSSendMsg2ClientWithClientSid(XDGatewaySessionID, clientSessionID, datas));
            //}

            /// <summary>
            /// 服务器发送消息给客户端
            /// </summary>
            /// <param name="gatewaySessionID"></param>
            /// <param name="playerFlagID"></param>
            /// <param name="datas"></param>
            //public void ServerSendMsgToClientWithPlayerFlagID(int gatewaySessionID, long playerFlagID, ByteArray datas, bool isLongConn = true)
            //{
            //    if (SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.WORLDBOSS &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CHAT &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.GAME)
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : This Function Just Be Use In WorldBossServer/ChatSrver!!!");
            //        return;
            //    }

            //    SendMessageToServer(gatewaySessionID, com.tieao.mmo.gateway4server.client.GateWay4ServerClientHelper.AS_SendMsg2ClientWithPlayerFlagID(playerFlagID, datas, isLongConn));
            //}

            /// <summary>
            /// 广播数据到客户端
            /// </summary>
            /// <param name="gatewaySessionID"></param>
            /// <param name="playerFlagIDList"></param>
            /// <param name="datas"></param>
            //public void ServerBroadcastMsgToClient(int gatewaySessionID, TLongList playerFlagIDList, ByteArray datas)
            //{
            //    if (SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.WORLDBOSS &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CHAT)
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : This Function Just Be Use In WorldBossServer/ChatServer!!!");
            //        return;
            //    }

            //    SendMessageToServer(gatewaySessionID, com.tieao.mmo.gateway4server.client.GateWay4ServerClientHelper.AS_Broadcast2ClientWithPlayers(playerFlagIDList, datas));
            //}

            /// <summary>
            /// 广播数据到客户端
            /// </summary>
            /// <param name="gatewaySessionID"></param>
            /// <param name="datas"></param>
            //public void ServerBroadcastMsgToClient(int gatewaySessionID, ByteArray datas)
            //{
            //    if (SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.WORLDBOSS &&
            //        SvrCommCfg.Instance.ServerInfo.m_Type != eServerType.CHAT)
            //    {
            //        SvLogger.Error("ServerSendMsgToClient Fail : This Function Just Be Use In WorldBossServer/ChatServer!!!");
            //        return;
            //    }

            //    SendMessageToServer(gatewaySessionID, com.tieao.mmo.gateway4server.client.GateWay4ServerClientHelper.AS_Broadcast2ClientAllPlayer(datas));
            //}

            /// <summary>
            /// 发送数据给客户端（仅供直接对客户端开放的服务使用）
            /// </summary>
            /// <param name="sessionID"></param>
            /// <param name="datas"></param>
            public void ServerSendMsgToClientWithSessionID(int sessionID, ByteArray datas)
            {
                lock (m_networkLock)
                {
                    ConnectionSession connSession = m_outLongConnectionListen.GetSession(sessionID);

                    if (connSession != null)
                        connSession.Send(datas);
                }
            }

            /// <summary>
            /// 广播消息到客户端
            /// </summary>
            /// <param name="datas"></param>
            public void BroadMsgToClinet(ByteArray datas)
            {
                lock (m_networkLock)
                {
                    ConnectionSession connSession = null;
                    foreach (var sessionID in m_longConnSidSet)
                    {
                        connSession = m_outLongConnectionListen.GetSession(sessionID);
                        if (connSession != null)
                            connSession.Send(datas);
                    }
                }
            }

            /// <summary>
            /// 注册到CenterServer成功
            /// </summary>
            /// <param name="sessionID">连接SessionID</param>
            /// <param name="centerSvrInfo">Center服务信息</param>
            public void RegistToServerSucc(int sessionID, ref PtServerInfo svrInfo)
            {
                lock (m_networkLock)
                {
                    ConnectionSession connSession = null;
                    if (m_serverConnectSessionDic.TryGetValue(sessionID, out connSession))
                    {
                        ConnectService connService = connSession.Service as ConnectService;
                        if (connService != null)
                        {
                            connService.SetConnectServerInfo(ref svrInfo);
                            m_hadRegistServerSet.Add(svrInfo.m_Type);
                        }
                    }
                }
            }

            /// <summary>
            /// 是否已经注册过CenterServer
            /// </summary>
            /// <returns></returns>
            public bool HadRegistedServer(eServerType svrType)
            {
                return m_hadRegistServerSet.Contains(svrType);
            }

            /// <summary>
            /// 设置玩家ID和客户端连接SessionID
            /// </summary>
            /// <param name="userID">玩家ID</param>
            /// <param name="sessionID">链接SessionID</param>
            //public void SetShortConnUserIDAndSessionID(Guid userID, int playerFlagID, int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        m_playerGuidSessionIDShortConnDic[userID] = sessionID;
            //        m_playerFlagIDSessionIDShortConnDic[playerFlagID] = sessionID;

            //        m_playerSessionIDGuidShortConnDic[sessionID] = userID;
            //        m_playerSessionIDFlagIDShortConnDic[sessionID] = playerFlagID;
            //    }
            //}
            //public void SetLongConnUserIDAndSessionID(Guid userID, int playerFlagID, int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        m_playerGuidSessionIDLongConnDic[userID] = sessionID;
            //        m_playerFlagIDSessionIDLongConnDic[playerFlagID] = sessionID;

            //        m_playerSessionIDGuidLongConnDic[sessionID] = userID;
            //        m_playerSessionIDFlagIDLongConnDic[sessionID] = playerFlagID;
            //    }
            //}

            /// <summary>
            /// 根据玩家ID获取客户端连接SessonID
            /// </summary>
            /// <param name="userID"></param>
            /// <returns></returns>
            //public int GetShortConnSessionIDByUserID(Guid userID)
            //{
            //    lock (m_networkLock)
            //    {
            //        if (m_playerGuidSessionIDShortConnDic.ContainsKey(userID))
            //            return m_playerGuidSessionIDShortConnDic[userID];
            //        return -1;
            //    }
            //}
            //public Guid GetShortConnUserIDBySessionID(int sessionID)
            //{
            //    lock (m_networkLock)
            //    {
            //        Guid userID;
            //        if (m_playerSessionIDGuidShortConnDic.TryGetValue(sessionID, out userID))
            //            return userID;
            //        return new Guid();
            //    }
            //}
            //public int GetLongConnSessionIDByUserID(Guid userID)
            //{
            //    lock (m_networkLock)
            //    {
            //        if (m_playerGuidSessionIDLongConnDic.ContainsKey(userID))
            //            return m_playerGuidSessionIDLongConnDic[userID];
            //        return -1;
            //    }
            //}
            //public Guid GetLongConnUserIDBySessionID(int sessionID, ref int playerFlagID)
            //{
            //    lock (m_networkLock)
            //    {
            //        if (m_playerSessionIDFlagIDLongConnDic.TryGetValue(sessionID, out playerFlagID) == false)
            //            playerFlagID = 0;

            //        Guid userID;
            //        if (m_playerSessionIDGuidLongConnDic.TryGetValue(sessionID, out userID))
            //        {
            //            return userID;
            //        }
            //        return new Guid();
            //    }
            //}

            //public int GetLongConnPlayerFlagIDBySessionID(int sessionID)
            //{
            //    lock(m_networkLock)
            //    {
            //        int playerFlagID;
            //        if (m_playerSessionIDFlagIDLongConnDic.TryGetValue(sessionID, out playerFlagID))
            //            return playerFlagID;

            //        return 0;
            //    }
            //}

            /// <summary>
            /// GUID转PtGuid
            /// </summary>
            /// <param name="id">Guid</param>
            /// <param name="ptID">PTGuid</param>
            public static PtGuid GuidConvertToPtGuid(Guid id)
            {
                PtGuid ptID = new PtGuid();
                ValBytes vbs = new ValBytes();
                byte[] idBytes = id.ToByteArray();
                vbs._b0 = idBytes[0];
                vbs._b1 = idBytes[1];
                vbs._b2 = idBytes[2];
                vbs._b3 = idBytes[3];
                ptID.a = vbs._intVal;

                vbs._b0 = idBytes[4];
                vbs._b1 = idBytes[5];
                ptID.b = vbs._shortVal;

                vbs._b0 = idBytes[6];
                vbs._b1 = idBytes[7];
                ptID.c = vbs._shortVal;

                ptID.d = idBytes[8];
                ptID.e = idBytes[9];
                ptID.f = idBytes[10];
                ptID.g = idBytes[11];
                ptID.h = idBytes[12];
                ptID.i = idBytes[13];
                ptID.j = idBytes[14];
                ptID.k = idBytes[15];

                return ptID;
            }

            /// <summary>
            /// PtGuid转Guid
            /// </summary>
            /// <param name="ptID"></param>
            /// <returns></returns>
            public static Guid PtGuidConvertToGuid(PtGuid ptID)
            {
                return new Guid(ptID.a, ptID.b, ptID.c, ptID.d, ptID.e, ptID.f, ptID.g, ptID.h, ptID.i, ptID.j, ptID.k);
            }

            /// <summary>
            /// 获取客户端同步连接
            /// </summary>
            /// <param name="sessionID"></param>
            /// <returns></returns>
            public ConnectionSession GetSyncClinetShortConnection(int sessionID)
            {
                if(m_isShortConnSync == false)
                    return null;

                ConnectionSession connSession;
                m_clientSyncShortConnDic.TryGetValue(sessionID, out connSession);
                return connSession;
            }
            public ConnectionSession GetSyncClinetLongConnection(int sessionID)
            {
                if (m_isLongConnSync == false)
                    return null;

                ConnectionSession connSession;
                m_clientSyncLongConnDic.TryGetValue(sessionID, out connSession);
                return connSession;
            }

            /// <summary>
            /// 是否是需要注册服务器
            /// </summary>
            /// <param name="svrType"></param>
            /// <returns></returns>
            public bool IsNeedRegistServer(eServerType svrType)
            {
                return m_needRegistServerSet.Contains(svrType);
            }

            /// <summary>
            /// 当有服务来注册
            /// </summary>
            /// <param name="svrInfo"></param>
            public void OnServerRegist(PtServerInfo svrInfo)
            {
                if (m_serverRegistDelegate != null)
                    m_serverRegistDelegate(svrInfo);
            }

            #endregion

            #region Private Functions

            private NetworkManager() 
            {
                m_needRegistServerSet.Add(eServerType.CENTER);
                m_needRegistServerSet.Add(eServerType.CROSSREALM);
                m_needRegistServerSet.Add(eServerType.PROTAL);
                m_needRegistServerSet.Add(eServerType.GM);
                m_needRegistServerSet.Add(eServerType.PLATFORM_DOCKING);
                m_needRegistServerSet.Add(eServerType.MONITEORNODE);
                m_needRegistServerSet.Add(eServerType.MONITEORSERVER);
                m_needRegistServerSet.Add(eServerType.XDGATEWAY);
                m_needRegistServerSet.Add(eServerType.GIFTCODE);
            }

            /// <summary>
            /// 侦听、连接刷新
            /// </summary>
            private void Update()
            {
                while (m_run)
                {
                    m_stopwatch.Restart();

                    lock (m_networkLock)
                    {
                        try
                        {
                            m_hadDoNetwork = true;
                            m_doNetWorkTimes = 0;
                            while (m_hadDoNetwork == true)
                            {
                                ++m_doNetWorkTimes;

                                if (m_outShortConnectionListen != null)
                                    m_outShortConnectionListen.Update();
                                if (m_outLongConnectionListen != null)
                                    m_outLongConnectionListen.Update();
                                if (m_inListen != null)
                                    m_inListen.Update();
                                foreach (ConnectService connect in m_connectList)
                                    connect.Update();

                                m_curConnTimeOuntCheckTime = Environment.TickCount;
                                m_connTimeOutCheckSpendTicket = (m_curConnTimeOuntCheckTime >= m_lastConnTimeOutCheckTime ? m_curConnTimeOuntCheckTime - m_lastConnTimeOutCheckTime : m_curConnTimeOuntCheckTime + (int.MaxValue - m_lastConnTimeOutCheckTime));
                                if (m_connTimeOutCheckSpendTicket >= 1000)
                                {
                                    if (m_needConnectSvrInfoQue.Count > 0)
                                    {
                                        ConnectIPPort ipPort = m_needConnectSvrInfoQue.Peek();
                                        if (ipPort.ReconnectTime <= m_curConnTimeOuntCheckTime)
                                        {
                                            m_needConnectSvrInfoQue.Dequeue();
                                            //bool result = OpenServerConnection(ipPort.m_IP, ipPort.m_Port);
                                            //if (result == false)
                                            //    SvLogger.Error("Make Connect Server Fail : Address={0}, Port={1}.", ipPort.m_IP, ipPort.m_Port);
                                        }
                                    }

                                    if (m_needReconnectSvrInfoQue.Count > 0)
                                    {
                                        ReconnectServerInfo reconnSvrInfo = m_needReconnectSvrInfoQue.Peek();
                                        if (reconnSvrInfo.ReconnectTime <= m_curConnTimeOuntCheckTime)
                                        {
                                            m_needReconnectSvrInfoQue.Dequeue();
                                            //bool result = OpenRegServerConnection(reconnSvrInfo.m_SvrInfo);
                                            //if (result == false)
                                            //    SvLogger.Error("Make Reconnect Server Fail And Stop Reconnect: SvrType={0}, Address={1}, Port={2}.", reconnSvrInfo.m_SvrInfo.m_Type, reconnSvrInfo.m_SvrInfo.m_Address, reconnSvrInfo.m_SvrInfo.m_Port);
                                        }
                                    }
                                }

                                //连接超时断开处理
                                if (SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.GATEWAY ||
                                    SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.CROSSREALM_BATTLE ||
                                    SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.PROTAL)
                                {
                                    if (m_connTimeOutCheckSpendTicket >= 30000)
                                    {
                                        if (m_outShortConnectionListen != null)
                                        {
                                            foreach (ConnectTimeOutCheckInfo checkInfo in m_shortConnectTimeOutCheckDic.Values)
                                            {
                                                if (checkInfo.m_OpenTicket + 180000 <= m_curConnTimeOuntCheckTime)
                                                {
                                                    ConnectionSession cs = m_outShortConnectionListen.GetSession(checkInfo.m_SessionID);
                                                    if (cs != null)
                                                        cs.Connection.Close();
                                                    m_timeOutConnSidQueue.Enqueue(checkInfo.m_SessionID);
                                                    if (m_isShortConnSync == true)
                                                        m_clientSyncShortConnDic.Remove(checkInfo.m_SessionID);
                                                }
                                            }

                                            int count = m_timeOutConnSidQueue.Count;
                                            for (int i = 0; i < count; ++i)
                                                m_shortConnectTimeOutCheckDic.Remove(m_timeOutConnSidQueue.Dequeue());
                                        }
                                        if (m_outLongConnectionListen != null)
                                        {
                                            foreach (ConnectTimeOutCheckInfo checkInfo in m_longConnectTimeOutCheckDic.Values)
                                            {
                                                if (checkInfo.m_OpenTicket + 180000 <= m_curConnTimeOuntCheckTime)
                                                {
                                                    ConnectionSession cs = m_outLongConnectionListen.GetSession(checkInfo.m_SessionID);
                                                    if (cs != null)
                                                        cs.Connection.Close();
                                                    m_timeOutConnSidQueue.Enqueue(checkInfo.m_SessionID);
                                                    if (m_isLongConnSync == true)
                                                        m_clientSyncLongConnDic.Remove(checkInfo.m_SessionID);
                                                }
                                            }

                                            int count = m_timeOutConnSidQueue.Count;
                                            for (int i = 0; i < count; ++i)
                                                m_longConnectTimeOutCheckDic.Remove(m_timeOutConnSidQueue.Dequeue());
                                        }

                                        m_lastConnTimeOutCheckTime = m_curConnTimeOuntCheckTime;
                                    }
                                }

                                if(m_doNetWorkTimes > 10)
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            SvLogger.Error("Connections Update Exception : Connected Process Lost. Msg={0}, StackInfo={1}.", ex.Message, ex.StackTrace);
                        }

                        if (m_tempConnectList.Count != 0)
                        {
                            foreach (ConnectService connService in m_tempConnectList)
                                m_connectList.Add(connService);
                            m_tempConnectList.Clear();
                        }

                        if (m_tempLostConnectList.Count != 0)
                        {
                            foreach (ConnectService connService in m_tempLostConnectList)
                                m_connectList.Remove(connService);
                            m_tempLostConnectList.Clear();
                        }
                        //Thread.Sleep(1);
                    }

                    m_stopwatch.Stop();
                    if (m_stopwatch.Elapsed < m_maxSleepTS)
                        Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                    else
                        Thread.Sleep(1);
                }
            }

            /// <summary>
            /// 接收到客户端短连接数据
            /// </summary>
            /// <param name="buffer">协议数据</param>
            /// <param name="sessionID">连接SessionID</param>
            private bool OnRecvShortConnectionClientData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                RecvedProtocolDataInfo recvProtoData = new RecvedProtocolDataInfo(true, false, sessionID, buffer);
                recvProtoData.m_IsClientData = true;
                recvProtoData.m_IsLongConnection = false;
                RecvedProtocolDataPoolManager.Instance.PushData(recvProtoData);
                m_hadDoNetwork = true;
                return true;
            }

            /// <summary>
            /// 接受到客户端长连接数据
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="sessionID"></param>
            /// <returns></returns>
            private bool OnRecvLongConnectionClientData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                RecvedProtocolDataInfo recvProtoData = new RecvedProtocolDataInfo(true, true, sessionID, buffer);
                recvProtoData.m_IsClientData = true;
                recvProtoData.m_IsLongConnection = true;
                RecvedProtocolDataPoolManager.Instance.PushData(recvProtoData);
                m_hadDoNetwork = true;
                return true;
            }

            /// <summary>
            /// 接收到服务器协议
            /// </summary>
            /// <param name="buffer">服务器协议</param>
            /// <param name="sessionID">连接SessionID</param>
            private bool OnRecvServerData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                RecvedProtocolDataInfo recvProtoData = new RecvedProtocolDataInfo(false, false, sessionID, buffer);
                RecvedProtocolDataPoolManager.Instance.PushData(recvProtoData);
                m_hadDoNetwork = true;
                return true;
            }

            /// <summary>
            /// 当服务组连接断开
            /// </summary>
            /// <param name="sessionID">服务组连接sessionID</param>
            //private void OnServerConnectionClose(ConnectionSession connSesion)
            //{
            //    lock (m_networkLock)
            //    {
            //        ConnectionSession conn = null;
            //        if (m_serverConnectSessionDic.TryGetValue(connSesion.LinkId, out conn))
            //        {
            //            PtServerInfo svrInfo = new PtServerInfo();
            //            ConnectService connService = conn.Service as ConnectService;
            //            if (connService != null)
            //            {
            //                //从连接服务列表移除
            //                m_tempLostConnectList.Add(connService);

            //                if (connService.GetConnectServerInfo(ref svrInfo))
            //                {
            //                    SvLogger.Error("Server Internal Connection Close : ServerType={0}, ServerName={1}, ProcessID={2}, Address={3}, Port={4}.", svrInfo.m_Type, svrInfo.m_Name, svrInfo.m_ProcessID, svrInfo.m_Address, svrInfo.m_Port);

            //                    //服务器连接断开上层逻辑处理
            //                    if (m_serverDisconnectDelegate != null)
            //                        m_serverDisconnectDelegate(svrInfo);
            //                    //

            //                    //检测目标进程是否还存活
            //                    //try
            //                    //{
            //                    //    System.Diagnostics.Process targetProcess = System.Diagnostics.Process.GetProcessById(svrInfo.m_ProcessID);
            //                    //}
            //                    //catch (System.ArgumentException)
            //                    //{
            //                    //    SvLogger.Error("The Target Server Is Crash : ServerType={0}, ServerName={1}, ProcessID={2}, Address={3}, Port={4}.", svrInfo.m_Type, svrInfo.m_Name, svrInfo.m_ProcessID, svrInfo.m_Address, svrInfo.m_Port);

            //                    //    //TODO:如果目标是CenterServer则服务启动自动关闭流程
            //                    //    if (svrInfo.m_Type == eServerType.CENTER)
            //                    //    {
            //                    //        //TODO:
            //                    //    }
            //                    //}

            //                    //重新开启连接
            //                    ReconnectServerInfo reconnSvrInfo = new ReconnectServerInfo(svrInfo);
            //                    m_needReconnectSvrInfoQue.Enqueue(reconnSvrInfo);
            //                }
            //                else
            //                    SvLogger.Error("ConnectService Not Find Connect Server Info!");
            //            }
            //            else
            //            {
            //                if (RegServerManager.Instance.GetServerInfo(connSesion.LinkId, out svrInfo))
            //                    SvLogger.Error("Server Internal Connection Close : SvrType={0}, Address={1}, Port={2}.", svrInfo.m_Type, svrInfo.m_Address, svrInfo.m_Port);
            //                else
            //                    SvLogger.Error("Server Internal Connection Close And Unknow ServerInfo.");
            //            }

            //            if (RegServerManager.Instance.GetServerInfo(connSesion.LinkId, out svrInfo))
            //                RegServerManager.Instance.UnregServer(svrInfo.m_Name);
            //            m_serverConnectSessionDic.Remove(connSesion.LinkId);
            //            SvLogger.Debug("Server Internal Connection Close: SvrName={0}.", svrInfo == null ? "UnKnow" : svrInfo.m_Name);
            //        }
            //        else
            //            SvLogger.Error("Server Internal Connection Close And Unknow ServerInfo(2).");
            //    }
            //}

            /// <summary>
            /// 当客户端短连接断开
            /// </summary>
            /// <param name="sessionID">玩家连接sessionID</param>
            private void OnClientShortConnectionClose(ConnectionSession connSession)
            {
                //Guid userID;
                //lock (m_networkLock)
                //{
                //    if (m_playerSessionIDGuidShortConnDic.TryGetValue(sessionID, out userID))
                //    {
                //        m_playerSessionIDGuidShortConnDic.Remove(sessionID);
                //        m_playerGuidSessionIDShortConnDic.Remove(userID);
                //        m_shortConnSidSet.Remove(sessionID);

                //        int playerFlagID;
                //        if (m_playerSessionIDFlagIDShortConnDic.TryGetValue(sessionID, out playerFlagID))
                //        {
                //            m_playerSessionIDFlagIDShortConnDic.Remove(sessionID);
                //            m_playerFlagIDSessionIDShortConnDic.Remove(playerFlagID);
                //        }

                //        SvLogger.Debug("Client Connection Close : UserID={0}, Sid={1}.", userID.ToString(), sessionID);
                //    }
                //    else
                //    {
                //        SvLogger.Debug("Client Connection Close : Sid={0}.", sessionID);
                //    }

                //    m_timeOutConnSidQueue.Enqueue(sessionID);
                //    if (m_isShortConnSync == true)
                //        m_clientSyncShortConnDic.Remove(sessionID);
                //}
            }

            /// <summary>
            /// 当客户端长连接断开
            /// </summary>
            /// <param name="sessionID"></param>
            private void OnClientLongConnectionClose(ConnectionSession connSession)
            {
                m_longConnSidSet.Remove(connSession.LinkId);

                //Guid userID;
                //lock (m_networkLock)
                //{
                //    if (m_playerSessionIDGuidLongConnDic.TryGetValue(sessionID, out userID))
                //    {
                //        if (m_serverLogicOnConnectionCloseDeal != null)
                //            m_serverLogicOnConnectionCloseDeal(sessionID);

                //        m_playerSessionIDGuidLongConnDic.Remove(sessionID);
                //        m_playerGuidSessionIDLongConnDic.Remove(userID);
                        
                //        m_longConnSidSet.Remove(sessionID);

                //        int playerFlagID;
                //        if(m_playerSessionIDFlagIDLongConnDic.TryGetValue(sessionID, out playerFlagID))
                //        {
                //            m_playerSessionIDFlagIDLongConnDic.Remove(sessionID);
                //            m_playerFlagIDSessionIDLongConnDic.Remove(playerFlagID);
                //        }

                //        SvLogger.Debug("Client Connection Close : UserID={0}, Sid={1}.", userID.ToString(), sessionID);
                //    }
                //    else
                //    {
                //        SvLogger.Debug("Client Connection Close : Sid={0}.", sessionID);
                //    }

                //    m_timeOutConnSidQueue.Enqueue(sessionID);
                //    if (m_isLongConnSync == true)
                //        m_clientSyncLongConnDic.Remove(sessionID);
                //}
            }

            /// <summary>
            /// 当有新的服务连接建立
            /// </summary>
            /// <param name="connSession">连接session</param>
            private void OnOpenNewServerConnection(ConnectionSession connSession)
            {
                lock (m_networkLock)
                {
                    m_serverConnectSessionDic.Remove(connSession.LinkId);
                    m_serverConnectSessionDic.Add(connSession.LinkId, connSession);

                    ConnectService connection = connSession.Service as ConnectService;
                    if (connection == null)
                    {
                        if (IsNeedRegistServer(SvrCommCfg.Instance.ServerInfo.m_Type))
                            connSession.Send(com.tieao.mmo.interval.client.InternalProtocolClientHelper.ConnectSvrSucc(SvrCommCfg.Instance.ServerInfo));

                        m_hadDoNetwork = true;
                    }
                }
            }

            /// <summary>
            /// 当有新的客户端短连接建立
            /// </summary>
            /// <param name="connSession">连接session</param>
            private void OnOpenNewClientShortConnection(ConnectionSession connSession)
            {
                lock (m_networkLock)
                {
                    //SvLogger.Debug("Open New Short Connection");
                    ConnectTimeOutCheckInfo ctoci = new ConnectTimeOutCheckInfo(connSession.LinkId, Environment.TickCount);
                    m_shortConnectTimeOutCheckDic[ctoci.m_SessionID] = ctoci;
                    if (m_isShortConnSync == true)
                        m_clientSyncShortConnDic[ctoci.m_SessionID] = connSession;
                    //m_shortConnSidSet.Add(connSession.LinkId);

                    m_hadDoNetwork = true;
                }
            }

            /// <summary>
            /// 当有新的客户端长连接建立
            /// </summary>
            /// <param name="connSession"></param>
            private void OnOpenNewClientLongConnection(ConnectionSession connSession)
            {
                lock (m_networkLock)
                {
                    //SvLogger.Debug("Open New Long Connection");
                    ConnectTimeOutCheckInfo ctoci = new ConnectTimeOutCheckInfo(connSession.LinkId, Environment.TickCount + 600000);
                    m_longConnectTimeOutCheckDic[ctoci.m_SessionID] = ctoci;
                    if (m_isLongConnSync == true)
                        m_clientSyncLongConnDic[ctoci.m_SessionID] = connSession;
                    m_longConnSidSet.Add(connSession.LinkId);

                    if (m_serverLogicOnOpenNewConnectionDeal != null)
                        m_serverLogicOnOpenNewConnectionDeal(connSession);

                    m_hadDoNetwork = true;
                }
            }

            /// <summary>
            /// 当用IP、端口连接失败处理
            /// </summary>
            /// <param name="ex">异常信息</param>
            /// <param name="errMsg">错误提示</param>
            /// <param name="ip">连接IP</param>
            /// <param name="port">连接端口</param>
            /// <param name="connService">失败的连接服务</param>
            private void OnOpenConnectionWithIP_PortFail(Exception ex, string errMsg, string ip, int port, ConnectService failConnService)
            {
                SvLogger.Error("Open Connection With IP&Port Fail : IP={0}, Port={1}, ErrMsg={2}.", ip, port, errMsg);

                lock (m_networkLock)
                {
                    m_tempLostConnectList.Add(failConnService);
                    ConnectIPPort ipPort = new ConnectIPPort(ip, port);
                    m_needConnectSvrInfoQue.Enqueue(ipPort);
                    //if (OpenServerConnection(ip, port) == true)
                    //    SvLogger.Info("ReOpen Connection With IP&Port : IP={0}, Port={1}.", ip, port);
                    //else
                    //    SvLogger.Info("ReOpen Connection With IP&Port Fail : IP={0}, Port={1}.", ip, port);
                }
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

                lock (m_networkLock)
                {
                    m_tempLostConnectList.Add(failConnService);
                    ReconnectServerInfo reconnSvrInfo = new ReconnectServerInfo(svrInfo);
                    m_needReconnectSvrInfoQue.Enqueue(reconnSvrInfo);
                    //if (OpenRegServerConnection(svrInfo) == true)
                    //    SvLogger.Info("ReOpen Connection With ServerInfo : SvrType={0}, IP={1}, Port={2}.", svrInfo.m_Type, svrInfo.m_Address, svrInfo.m_Port);
                    //else
                    //    SvLogger.Info("ReOpen Connection With ServerInfo Fail : SvrType={0}, IP={1}, Port={2}.", svrInfo.m_Type, svrInfo.m_Address, svrInfo.m_Port);
                }
            }

            #endregion

            #region Data Member

            public static NetworkManager Instance
            {
                get
                {
                    return _instance;
                }
            }

            private static NetworkManager _instance = new NetworkManager();

            
            #region Lock Targets

            //网络活动锁
            private object m_networkLock = new object();
            //客户端侦听服务
            private ListenService m_outShortConnectionListen = null;
            private ListenService m_outLongConnectionListen = null;
            //服务器组侦听服务
            private ListenService m_inListen = null;
            //服务器组连接服务
            private List<ConnectService> m_connectList = new List<ConnectService>();
            //服务器连接Session字典；KEY: SessionID; VALUES: 连接Session
            private Dictionary<int, ConnectionSession> m_serverConnectSessionDic = new Dictionary<int, ConnectionSession>();

            //玩家连接SessionID字典；KEY: Guid; VALUES: 连接的SessionID,SessionID=-1为无效ID
            //private Dictionary<Guid, int> m_playerGuidSessionIDShortConnDic = new Dictionary<Guid, int>();
            //private Dictionary<int, int> m_playerFlagIDSessionIDShortConnDic = new Dictionary<int, int>();
            //private Dictionary<Guid, int> m_playerGuidSessionIDLongConnDic = new Dictionary<Guid, int>();
            //private Dictionary<int, int> m_playerFlagIDSessionIDLongConnDic = new Dictionary<int, int>();
            //private HashSet<int> m_shortConnSidSet = new HashSet<int>();
            //玩家连接Guid字典；KEY:sessionID; VALUES：该连接所属玩家的GUID
            //private Dictionary<int, Guid> m_playerSessionIDGuidShortConnDic = new Dictionary<int, Guid>();
            //private Dictionary<int, int> m_playerSessionIDFlagIDShortConnDic = new Dictionary<int, int>();
            //private Dictionary<int, Guid> m_playerSessionIDGuidLongConnDic = new Dictionary<int, Guid>();
            //private Dictionary<int, int> m_playerSessionIDFlagIDLongConnDic = new Dictionary<int, int>();
            private HashSet<int> m_longConnSidSet = new HashSet<int>();

            private Queue<ReconnectServerInfo> m_needReconnectSvrInfoQue = new Queue<ReconnectServerInfo>();
            private Queue<ConnectIPPort> m_needConnectSvrInfoQue = new Queue<ConnectIPPort>(); 


            //连接超时检测字典
            private Dictionary<int, ConnectTimeOutCheckInfo> m_shortConnectTimeOutCheckDic = new Dictionary<int, ConnectTimeOutCheckInfo>();
            private Dictionary<int, ConnectTimeOutCheckInfo> m_longConnectTimeOutCheckDic = new Dictionary<int, ConnectTimeOutCheckInfo>();

            private OpenNewConnectDelegate m_serverLogicOnOpenNewConnectionDeal;
            private ConnectOnCloseDelegate m_serverLogicOnConnectionCloseDeal;

            #endregion
            
            //服务器组连接服务临时缓存
            private List<ConnectService> m_tempConnectList = new List<ConnectService>();
            //服务器组失连服务临时缓存
            private List<ConnectService> m_tempLostConnectList = new List<ConnectService>();

            //已经注册过的服务器
            private HashSet<eServerType> m_hadRegistServerSet = new HashSet<eServerType>();

            //网络活动线程
            private Thread m_networkUpdateThread = null;
            //连接超时检测间隔
            private int m_connTimeOutCheckSpendTicket = 0;
            //当前连接超时检测时间
            private int m_curConnTimeOuntCheckTime = 0;
            //连接超时最后检测时间
            private int m_lastConnTimeOutCheckTime = 0;
            //超时连接队列
            private Queue<int> m_timeOutConnSidQueue = new Queue<int>();

            private bool m_run = true;

            //有服务来注册代理
            private OnServerRegistDelegate m_serverRegistDelegate = null;

            //有服务断开连接代理
            private OnServerDisConnectDelegate m_serverDisconnectDelegate = null;

            //需要注册服务器类型字典
            private HashSet<eServerType> m_needRegistServerSet = new HashSet<eServerType>();

            #region 协议非异步处理连接相关

            //协议是否同步处理
            private bool m_isShortConnSync = false;
            private bool m_isLongConnSync = false;
            //协议非异步处理连接字典
            private Dictionary<int, ConnectionSession> m_clientSyncShortConnDic = new Dictionary<int, ConnectionSession>();
            private Dictionary<int, ConnectionSession> m_clientSyncLongConnDic = new Dictionary<int, ConnectionSession>();

            #endregion

            #endregion

            private TimeSpan m_maxSleepTS = new TimeSpan(10000 * 50);
            private Stopwatch m_stopwatch = new Stopwatch();

            private bool m_hadDoNetwork = false;
            private int m_doNetWorkTimes = 0;
        }

//#endif
    }
}
