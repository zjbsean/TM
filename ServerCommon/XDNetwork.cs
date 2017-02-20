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
using System.Net;

namespace ServerCommon
{
#if BW
    namespace Network
    {
        //连接管理
        public class XDNetworkManager
        {
            public static XDNetworkManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            #region Public Functions

            public void SetHadNewServerRegistOK(OpenNewConnectDelegate registOKDelegate)
            {
                m_registOKDelegate = registOKDelegate;
            }

            public void InitConfig(int outSidePort, int inSidePort, string moniteorNodeIP, int moniteorNodePort)
            {
                m_listenPort4Client = outSidePort;
                m_listenPort4Server = inSidePort;

                m_connectMoniteorNodeIP = moniteorNodeIP;
                m_connectMoniteorNodePort = moniteorNodePort;
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
                if (m_listenPort4Client == 0 || openClientListen() == false)
                    throw new Exception("Open Client Listen Fail!");

                if (m_listenPort4Server == 0 || openServerListen() == false)
                    throw new Exception("Open Server Listen Fail!");

                if (m_connectMoniteorNodeIP != "")
                {
                    if (openMoniteorSvrConnection() == false)
                        throw new Exception("Init Moniteor Server Connection Fail!");
                }
                
                m_threadRun = true;
                int logPrintIndex = 0;
                while(m_threadRun)
                {
                    m_doNetworkUpdate = true;
                    m_doNetworkUpdateTimesWithoutSleep = 0;

                    m_stopwatch.Restart();

                    while (m_doNetworkUpdate)
                    {
                        m_doNetworkUpdate = false;
                        ++m_doNetworkUpdateTimesWithoutSleep;
                        m_clientListen.Update();
                        m_serverListen.Update();
                        if (m_moniteorConnection != null)
                            m_moniteorConnection.Update();
                        if (m_doNetworkUpdateTimesWithoutSleep > 100)
                            break;

                        //SvLogger.Debug("DoNetworkUpdateTimesWithoutSleep Times={0}", m_doNetworkUpdateTimesWithoutSleep);

                        ServerProtocolDealManager.Instance.DealedProtocolTransferData(ref m_closeClientConnSessionIDList,
                                                                                        ref m_checkClientConnList,
                                                                                        ref m_forwardProtocolList,
                                                                                        ref m_serverRegistList,
                                                                                        ref m_serverBoardcastList);

                        dealCheckClientConnList();
                        dealForwardProtocolList();
                        dealServerRegistList();

                        ClientProtocolDealManager.Instance.TransferForward2ClientData(ref m_forward2ClientProtocolList);
                        dealForward2ClientList();

                        ClientProtocolDealManager.Instance.TransferForward2ServerData(ref m_forward2ServerProtocolList);
                        dealForward2ServerList();

                        dealServerBoardcastProtocolList();

                        dealCloseClientConnSession();

                    }

                    m_svrCheckEndTicket = Environment.TickCount;
                    m_svrCheckSpendTicket = (m_svrCheckEndTicket >= m_svrCheckStartTicket ? m_svrCheckEndTicket - m_svrCheckStartTicket : (int.MaxValue + m_svrCheckEndTicket) + (int.MaxValue - m_svrCheckStartTicket));
                    if (m_svrCheckSpendTicket >= 1000)
                    {
                        m_svrCheckStartTicket = m_svrCheckEndTicket;

                        int curSvrTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond;

                        m_clientConnMgr.UncheckConnCheckUpdate(m_clientListen, curSvrTime);
                        m_clientConnMgr.CheckedShortConnCheckUpdate(m_clientListen, curSvrTime);

                        ++logPrintIndex;
                        if(logPrintIndex >= 60)
                        {
                            logPrintIndex = 0;
                            //打印连接数情况
                            m_clientConnMgr.LogConnectionCount();
                        }
                    }

                    m_stopwatch.Stop();
                    double sleepMillSec = (m_maxSleepTS - m_stopwatch.Elapsed).TotalMilliseconds;
                    if(m_doNetworkUpdateTimesWithoutSleep > 1)
                    {
                        //SvLogger.Debug("Sleep : 1");
                        Thread.Sleep(1);
                    }
                    else
                    {
                        if (m_stopwatch.Elapsed < m_maxSleepTS)
                        {
                            //SvLogger.Debug("Sleep : {0}", sleepMillSec);
                            Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                        }
                        else
                        {
                            //SvLogger.Debug("Sleep : 1");
                            Thread.Sleep(1);
                        }
                    }
                    //if (m_stopwatch.Elapsed < m_maxSleepTS)
                    //{
                    //    Thread.Sleep(m_maxSleepTS - m_stopwatch.Elapsed);
                    //}
                    //else
                    //{
                    //    Thread.Sleep(1);
                    //}
                }

                closeClientListen();
                closeServerListen();
                closeMoniteorSvrConnection();
            }

            #region Client Listen

            private bool openClientListen()
            {
                try
                {
                    closeClientListen();

                    m_clientListen = new ListenService(new ProtocolDealDelegate(onRecvConnectionClientData),
                                                new OpenNewConnectDelegate(onOpenNewClientConnection),
                                                new ConnectOnCloseDelegate(onClientConnectionClose),
                                                false, 0);

                    bool result = m_clientListen.Listion(m_listenPort4Client, 100000);
                    if (result == false)
                        m_clientListen = null;
                    return result;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }

            private bool onRecvConnectionClientData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                if (buffer._readerIdx == buffer._writerIdx)
                    return false;

                ClientProtocolDealManager.Instance.HadRecvClientProtocol(sessionID, buffer, connType);
                m_doNetworkUpdate = true;

                return true;
            }

            private void onOpenNewClientConnection(ConnectionSession connSession)
            {
                m_clientConnMgr.AddUncheckConnection(connSession);
                m_doNetworkUpdate = true;
                //connSession.EnableHeartBeat(false, true);
            }

            private void onClientConnectionClose(ConnectionSession connSession)
            {
                m_clientConnMgr.ClientConnClose(m_clientListen, m_serverListen, connSession);
                m_doNetworkUpdate = true;
            }

            private void closeClientListen()
            {
                if(m_clientListen != null)
                {
                    m_clientListen.Close();
                    m_clientListen.Dispose();

                    m_clientListen = null;
                }
            }

            #endregion

            #region Server Listen

            private bool openServerListen()
            {
                closeServerListen();
                try
                {
                    m_serverListen = new ListenService(new ProtocolDealDelegate(onRecvServerData),
                                            new OpenNewConnectDelegate(onOpenNewServerConnection),
                                            new ConnectOnCloseDelegate(onServerConnectionClose), false, 0);
                    bool result = m_serverListen.Listion(m_listenPort4Server, 10000);
                    if (result == false)
                        m_serverListen = null;
                    return result;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }

            private bool onRecvServerData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                //SvLogger.Debug("OnReceiveServerData : ReaderIdx={0}, WriterIdx={1}, ConnType={2}.", buffer._readerIdx, buffer._writerIdx, connType);
                if (buffer._readerIdx == buffer._writerIdx)
                    return false;

                ServerProtocolDealManager.Instance.RecvServerData(sessionID, buffer, connType);
                m_doNetworkUpdate = true;
                return true;
            }

            private void onOpenNewServerConnection(ConnectionSession connSession)
            {
                ConnectService connection = connSession.Service as ConnectService;
                if (connection == null)
                {
                    connSession.Send(com.tieao.mmo.interval.client.InternalProtocolClientHelper.ConnectSvrSucc(SvrCommCfg.Instance.ServerInfo));
                    m_doNetworkUpdate = true;
                }
            }

            private void onServerConnectionClose(ConnectionSession connSession)
            {
                m_serverConnMgr.ConnectionClose(connSession);
                m_doNetworkUpdate = true;
            }

            private void closeServerListen()
            {
                if (m_serverListen != null)
                {
                    m_serverListen.Close();
                    m_serverListen.Dispose();
                    m_serverListen = null;
                }
            }

            #endregion

            #region Server Connection

            private bool openMoniteorSvrConnection()
            {
                closeMoniteorSvrConnection();

                try
                {
                    m_moniteorConnection = new ConnectService(onRecvMoniteorData,
                                                    onMoniteorConnectionClose,
                                                    onOpenNewMoniteorConnection,
                                                    onOpenMoniteorFail);

                    bool result = m_moniteorConnection.Connect(m_connectMoniteorNodeIP, m_connectMoniteorNodePort);
                    if (result == false)
                        m_moniteorConnection = null;
                    return result;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }

            private bool onRecvMoniteorData(ByteArray buffer, int sessionID, ClientProtocolDealManager.eConnectionType connType)
            {
                ServerProtocolDealManager.Instance.RecvServerData(sessionID, buffer, connType);
                return true;
            }

            private void onMoniteorConnectionClose(ConnectionSession connSession)
            {
                m_moniteorConnection.Dispose();
                m_moniteorConnIsOpened = false;
                m_moniteorConnection = null;

                //TODO:重连
            }

            private void onOpenNewMoniteorConnection(ConnectionSession connSession)
            {
                m_moniteorConnIsOpened = true;
            }

            private void onOpenMoniteorFail(Exception ex, string errMsg, string ip, int port, ConnectService failConnService)
            {
                m_moniteorConnection = null;

                //TODO:重连
            }

            private void closeMoniteorSvrConnection()
            {
                if (m_moniteorConnection != null)
                {
                    m_moniteorConnIsOpened = false;

                    m_moniteorConnection.Close();
                    m_moniteorConnection.Dispose();
                    
                    m_moniteorConnection = null;
                }
            }

            #endregion

            #region Protocol Deal

            private void dealCloseClientConnSession()
            {
                if (m_closeClientConnSessionIDList.Count > 0)
                {
                    foreach (var sessionID in m_closeClientConnSessionIDList)
                    {
                        ClientConnectionsManager.CheckedConnectionInfo checkedConn = m_clientConnMgr.RemoveCheckedShortConnection(sessionID);
                        if (checkedConn == null)
                            checkedConn = m_clientConnMgr.RemoveCheckedShortConnection(sessionID);

                        if (checkedConn != null)
                            checkedConn.m_ClientSession.Connection.Close();
                    }
                    m_closeClientConnSessionIDList.Clear();
                }
            }

            private void dealCheckClientConnList()
            {
                if (m_checkClientConnList.Count > 0)
                {
                    foreach (var connCheck in m_checkClientConnList)
                    {
                        m_clientConnMgr.RemoveUncheckConnection(connCheck.clientSessionID);
                        ConnectionSession connSession = m_clientListen.GetSession(connCheck.clientSessionID);
                        if (connSession != null && connSession.Connected == true)
                        {
                            if (connCheck.isPassedCheck)
                            {
                                if (connCheck.isLongConnection == true)
                                {
                                    m_clientConnMgr.AddCheckedLongConnection(m_serverListen, connSession, connCheck.gatewaySessionID);
                                    ConnectionSession gatewaySession = m_serverListen.GetSession(connCheck.gatewaySessionID);
                                    if(gatewaySession != null)
                                    {
                                        //通知Gateway连接的IP
                                        (connSession.Connection.Reactor.RemoteEndPoint as IPEndPoint).Address.ToString();

                                    }
                                }
                                else
                                    m_clientConnMgr.AddCheckedShortConnection(connSession, connCheck.gatewaySessionID);
                            }
                            else
                            {
                                connSession.Connection.Close();
                            }
                        }
                    }

                    m_checkClientConnList.Clear();
                }
            }

            private void dealForwardProtocolList()
            {
                if (m_forwardProtocolList.Count > 0)
                {
                    foreach (var forwardProtocol in m_forwardProtocolList)
                    {
                        ConnectionSession connSession = m_clientListen.GetSession(forwardProtocol.m_ClientSessionID);
                        if (connSession != null)
                        {
                            if (connSession.Connected == true)
                            {
                                connSession.Send(forwardProtocol.m_ProtocolData);
                                //SvLogger.Info("ForwardProtocolList!!!!!!!!!!");
                            }
                            else
                            {
                                connSession.Connection.Close();
                                m_clientConnMgr.RemoveCheckedLongConnection(forwardProtocol.m_ClientSessionID);
                            }
                        }
                    }

                    m_forwardProtocolList.Clear();
                }
            }

            private void dealServerRegistList()
            {
                if (m_serverRegistList.Count > 0)
                {
                    foreach (var svrRegist in m_serverRegistList)
                    {
                        ConnectionSession connSession = m_serverListen.GetSession(svrRegist.m_ServerSessionID);
                        if (connSession != null)
                        {
                            ServerConnectionsManager.ServerConnection svrConn = new ServerConnectionsManager.ServerConnection();
                            svrConn.m_Port = svrRegist.m_Port;
                            svrConn.m_ConnSession = connSession;
                            if (svrRegist.m_ServerType == eServerType.GATEWAY)
                            {
                                m_serverConnMgr.AddGatewayConnection(svrRegist.m_ServerID, svrConn);
                                connSession.m_GatewaySid = svrRegist.m_ServerSessionID;
                            }
                                
                            else if (svrRegist.m_ServerType == eServerType.PROTAL)
                                m_serverConnMgr.AddPortalConnection(svrConn);

                            ByteArray sendDatas = com.tieao.mmo.interval.client.InternalProtocolClientHelper.RegisterSvrOK(SvrCommCfg.Instance.ServerInfo, new PtServerList());
                            connSession.Send(sendDatas);
                            SvLogger.Info("OnRegisterServer OK : ServerType={0}.", svrRegist.m_ServerType);

                            if(m_registOKDelegate != null)
                                m_registOKDelegate(connSession);
                        }
                    }

                    m_serverRegistList.Clear();
                }
            }

            private void dealForward2ClientList()
            {
                if(m_forward2ClientProtocolList.Count > 0)
                {
                    foreach (var clientProto in m_forward2ClientProtocolList)
                    {
                        switch(clientProto.m_ConnType)
                        {
                            case ClientProtocolDealManager.eConnectionType.Uncheck:
                                {
                                    ConnectionSession connSession = m_clientListen.GetSession(clientProto.m_ClientSessionID);
                                    if (connSession != null && connSession.Connected == true)
                                    {
                                        connSession.Send(clientProto.m_ProtocolData);
                                        //SvLogger.Info("Forward2Client Uncheck : ClientSessionID={0}.", clientProto.m_ClientSessionID);
                                    }
                                    else
                                        SvLogger.Error("Not Find Uncheck Connection : SessionID={0}", clientProto.m_ClientSessionID);
                                }
                                break;
                            case ClientProtocolDealManager.eConnectionType.Short:
                                {
                                    ConnectionSession connSession = m_clientListen.GetSession(clientProto.m_ClientSessionID);
                                    if (connSession != null)
                                    {
                                        connSession.Send(clientProto.m_ProtocolData);
                                        //SvLogger.Info("Forward2Client Short : ClientSessionID={0}.", clientProto.m_ClientSessionID);
                                    }
                                    else
                                        SvLogger.Error("Not Find Short Connection : SessionID={0}", clientProto.m_ClientSessionID);
                                }
                                break;
                            case ClientProtocolDealManager.eConnectionType.Long:
                                {
                                    ConnectionSession connSession = m_clientListen.GetSession(clientProto.m_ClientSessionID);
                                    if (connSession != null)
                                    {
                                        connSession.Send(clientProto.m_ProtocolData);
                                        //SvLogger.Info("Forward2Client Long : ClientSessionID={0}.", clientProto.m_ClientSessionID);
                                    }
                                    else
                                        SvLogger.Error("Not Find Long Connection : SessionID={0}", clientProto.m_ClientSessionID);
                                }
                                break;
                            default:
                                SvLogger.Error("Not Find Connection Type : Type={0}", clientProto.m_ConnType);
                                break;
                        }
                    }

                    m_forward2ClientProtocolList.Clear();
                }
            }

            private void dealForward2ServerList()
            {
                if(m_forward2ServerProtocolList.Count > 0)
                {
                    foreach (var serverProto in m_forward2ServerProtocolList)
                    {
                        switch(serverProto.m_4What)
                        {
                            case ClientProtocolDealManager.eForward2Server4What.AccountCheckWithPassword:
                            case ClientProtocolDealManager.eForward2Server4What.AccountCheckWithPlatformToken:
                                {
                                    ConnectionSession connSession = m_clientListen.GetSession(serverProto.m_ClientSessionID);
                                    if (connSession != null)
                                    {
                                        if (connSession.m_ConnType == ClientProtocolDealManager.eConnectionType.Uncheck)
                                        {
                                            ConnectionSession gatewaySession = m_serverConnMgr.GetServerGateway(serverProto.m_ServerID, serverProto.m_Port);
                                            if (gatewaySession != null)
                                            {
                                                gatewaySession.Send(serverProto.m_ProtocolData);
                                                //SvLogger.Info("Forward2Server AccountCheckWithPassword/AccountCheckWithPlatformToken: ServerID={0}, Port={1}.", serverProto.m_ServerID, serverProto.m_Port);
                                            }
                                            else
                                            {
                                                string errMsg = string.Format("Not Find The Server Gateway! ServerID={0}, 4What={1}", serverProto.m_ServerID, connSession.m_ConnType);
                                                SvLogger.Error(errMsg);

                                                //通知Gateway没找到
                                                ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                                connSession.Send(sendData);
                                                connSession.Connection.Close();
                                            }
                                        }
                                        else
                                        {
                                            string errMsg = string.Format("Client Connection Is Not Uncheck : 4What={0}", serverProto.m_4What);
                                            SvLogger.Error(errMsg);

                                            //通知并断开连接
                                            ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                            connSession.Send(sendData);
                                            connSession.Connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        SvLogger.Error("Client Connection Had Close : 4What={0}", serverProto.m_4What);
                                    }
                                }
                                break;
                            case ClientProtocolDealManager.eForward2Server4What.LongConnRegist:
                                {
                                    ConnectionSession connSession = m_clientListen.GetSession(serverProto.m_ClientSessionID);
                                    if (connSession != null)
                                    {
                                        if (connSession.m_ConnType == ClientProtocolDealManager.eConnectionType.Uncheck)
                                        {
                                            ConnectionSession gatewaySession = m_serverConnMgr.GetServerGateway(serverProto.m_ServerID, serverProto.m_Port);
                                            if (gatewaySession != null)
                                            {
                                                ByteArray sendData = com.tieao.mmo.gateway4server.server.XDGatewayProtocolServerHelper.XDPlayerRegistLongConnection(serverProto.m_UserID, serverProto.m_ClientSessionID);
                                                gatewaySession.Send(sendData);
                                                //SvLogger.Info("Forward2Server LongConnRegist: ServerID={0}, Port={1}.", serverProto.m_ServerID, serverProto.m_Port);
                                            }
                                            else
                                            {
                                                string errMsg = string.Format("Not Find The Server Gateway! ServerID={0}, 4What={1}", serverProto.m_ServerID, connSession.m_ConnType);
                                                SvLogger.Error(errMsg);

                                                //通知Gateway没找到
                                                ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                                connSession.Send(sendData);
                                                connSession.Connection.Close();
                                            }
                                        }
                                        else
                                        {
                                            string errMsg = string.Format("Client Connection Is Not Uncheck : 4What={0}", serverProto.m_4What);
                                            SvLogger.Error(errMsg);

                                            //通知并断开连接
                                            ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                            connSession.Send(sendData);
                                            connSession.Connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        SvLogger.Error("Client Connection Had Close : 4What={0}", serverProto.m_4What);
                                    }
                                }
                                break;
                            case ClientProtocolDealManager.eForward2Server4What.PortalInfo:
                                {
                                    ConnectionSession clientSession = m_clientListen.GetSession(serverProto.m_ClientSessionID);
                                    if (clientSession != null)
                                    {
                                        if (clientSession.m_ConnType == ClientProtocolDealManager.eConnectionType.Uncheck)
                                        {
                                            ConnectionSession portalSession = m_serverConnMgr.RadomServerPortal();
                                            if(portalSession == null)
                                            {
                                                string errMsg = string.Format("The Radom Portal Server Session Is Null : 4What={0}", serverProto.m_4What);
                                                //报错
                                                ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                                clientSession.Send(sendData);
                                                clientSession.Connection.Close();
                                            }
                                            else
                                            {
                                                portalSession.Send(serverProto.m_ProtocolData);
                                                //SvLogger.Info("Forward2Server PortalInfo: ServerID={0}, Port={1}.", serverProto.m_ServerID, serverProto.m_Port);
                                            }
                                        }
                                        else
                                        {
                                            //通知并断开连接
                                            string errMsg = string.Format("Client Connection Is Not Uncheck : 4What={0}", serverProto.m_4What);
                                            SvLogger.Error(errMsg);

                                            ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                            clientSession.Send(sendData);
                                            clientSession.Connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        SvLogger.Error("Client Connection Had Close : 4What={0}", serverProto.m_4What);
                                    }
                                }
                                break;
                            case ClientProtocolDealManager.eForward2Server4What.Server:
                                {
                                    bool isLongConn = false;
                                    if (serverProto.m_ConnType == ClientProtocolDealManager.eConnectionType.Long)
                                        isLongConn = true;

                                    ConnectionSession connSession = m_serverConnMgr.GetServerGateway(serverProto.m_ServerID, serverProto.m_Port);
                                    if (connSession != null)
                                    {
                                        ByteArray sendData = com.tieao.mmo.gateway4server.server.XDGatewayProtocolServerHelper.XDClientMessage(serverProto.m_UserID, serverProto.m_ServerType, serverProto.m_ClientSessionID, isLongConn, serverProto.m_ProtocolIndex, serverProto.m_ProtocolData);
                                        connSession.Send(sendData);
                                        //SvLogger.Info("Forward2Server Server: ServerID={0}, Port={1}.", serverProto.m_ServerID, serverProto.m_Port);
                                    }
                                    else
                                    {
                                        ConnectionSession clientSession = m_clientListen.GetSession(serverProto.m_ClientSessionID);
                                        if(clientSession != null)
                                        {
                                            string errMsg = string.Format("Lost Gateway Connection : 4What={0}", serverProto.m_4What);
                                            SvLogger.Error(errMsg);

                                            ByteArray sendData = com.tieao.mmo.xdgateway4client.client.XDGateWay4ClientClientHelper.XDNoticeMessage(errMsg);
                                            clientSession.Send(sendData);
                                            clientSession.Connection.Close();
                                        }
                                    }
                                }
                                break;
                            default:
                                SvLogger.Error("Not Find Send Target Type : Type={0}", serverProto.m_4What);
                                break;
                        }
                    }

                    m_forward2ServerProtocolList.Clear();
                }
            }

            private void dealServerBoardcastProtocolList()
            {
                if (m_serverBoardcastList.Count > 0)
                {
                    foreach (var bp in m_serverBoardcastList)
                    {
                        foreach (var sid in bp.m_clientSessionIDList.GetElements())
                        {
                            ConnectionSession connSid = m_clientListen.GetSession(sid);
                            if (connSid != null &&
                                connSid.Connected == true &&
                                connSid.m_ConnType == ClientProtocolDealManager.eConnectionType.Long)
                            {
                                connSid.Send(bp.m_ProtocolData);
                            }
                        }
                    }
                    m_serverBoardcastList.Clear();
                }
            }

            #endregion

            #endregion

            #region Thread Obj & Connection Config

            private Thread m_networkThread = null;
            private bool m_threadRun = false;

            private int m_listenPort4Client;

            private int m_listenPort4Server;

            private string m_connectMoniteorNodeIP;
            private int m_connectMoniteorNodePort;

            private ClientConnectionsManager m_clientConnMgr = new ClientConnectionsManager();
            private ServerConnectionsManager m_serverConnMgr = new ServerConnectionsManager();

            private bool m_moniteorConnIsOpened = false;
            private ConnectService m_moniteorConnection = null;

            private ListenService m_clientListen = null;
            private ListenService m_serverListen = null;

            #endregion

            #region Protocol Copy Data

            private List<int> m_closeClientConnSessionIDList = new List<int>();
            private List<TConnectionCheck> m_checkClientConnList = new List<TConnectionCheck>();
            private List<ServerProtocolDealManager.ServerForwardProtocol> m_forwardProtocolList = new List<ServerProtocolDealManager.ServerForwardProtocol>();
            private List<ClientProtocolDealManager.Forward2ClientProtocol> m_forward2ClientProtocolList = new List<ClientProtocolDealManager.Forward2ClientProtocol>();
            private List<ClientProtocolDealManager.Forward2ServerProtocol> m_forward2ServerProtocolList = new List<ClientProtocolDealManager.Forward2ServerProtocol>();
            private List<ServerProtocolDealManager.ServerRegistInfo> m_serverRegistList = new List<ServerProtocolDealManager.ServerRegistInfo>();
            private List<ServerProtocolDealManager.ServerBoardcastProtocol> m_serverBoardcastList = new List<ServerProtocolDealManager.ServerBoardcastProtocol>();

            #endregion

            private int m_svrCheckSpendTicket = 0;
            private int m_svrCheckStartTicket = Environment.TickCount;
            private int m_svrCheckEndTicket = 0;

            private OpenNewConnectDelegate m_registOKDelegate = null;

            private TimeSpan m_maxSleepTS = new TimeSpan(10000 * 50);
            private Stopwatch m_stopwatch = new Stopwatch();

            private bool m_doNetworkUpdate = false;
            private int m_doNetworkUpdateTimesWithoutSleep = 0;

            private static XDNetworkManager m_instance = new XDNetworkManager();
            private XDNetworkManager() { }
        }

        public class ClientConnectionsManager
        {
            class UncheckConnectionInfo
            {
                public ConnectionSession m_ClientSession;
                public int m_OpenTime;
            }

            public class CheckingConnectionInfo
            {
                public TConnectionCheck m_ConnectionCheck = new TConnectionCheck();
                public int m_CheckTime;
            }

            public class CheckedConnectionInfo
            {
                public ConnectionSession m_ClientSession;
                public int m_GatewaySessionID;
            }

            public enum eConnectionState
            {
                Uncheck,
                Checking,
                CheckedShort,
                CheckedLong
            }

            #region 未验证连接

            public void AddUncheckConnection(ConnectionSession connSession)
            {
                if (m_uncheckConnLinkNodeDic.ContainsKey(connSession.LinkId) == false)
                {
                    UncheckConnectionInfo checkInfo = new UncheckConnectionInfo()
                    {
                        m_ClientSession = connSession,
                        m_OpenTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond + m_uncheckLimitSec
                    };
                    LinkedListNode<UncheckConnectionInfo> checkInfoNode = m_uncheckConnLink.AddLast(checkInfo);
                    m_uncheckConnLinkNodeDic.Add(connSession.LinkId, checkInfoNode);
                    connSession.m_ConnType = ClientProtocolDealManager.eConnectionType.Uncheck;
                }
            }

            public void RemoveUncheckConnection(int sessionID)
            {
                LinkedListNode<UncheckConnectionInfo> checkInfoNode;
                if (m_uncheckConnLinkNodeDic.TryGetValue(sessionID, out checkInfoNode))
                {
                    m_uncheckConnLinkNodeDic.Remove(sessionID);
                    m_uncheckConnLink.Remove(checkInfoNode);
                }
            }
            #endregion

            #region 已验证的短连接链表

            public void AddCheckedShortConnection(ConnectionSession connSession, int gatewaySessionID)
            {
                if (m_checkedClientShortConnLinkNodeDic.ContainsKey(connSession.LinkId) == false)
                {
                    CheckedConnectionInfo checkedInfo = new CheckedConnectionInfo()
                    {
                        m_ClientSession = connSession,
                        m_GatewaySessionID = gatewaySessionID,
                    };
                    checkedInfo.m_ClientSession.m_CreateTime = ServerCommon.SvrCommCfg.CurrentServerTimeInSecond + m_checdedShortConnLimitSec;
                    LinkedListNode<CheckedConnectionInfo> checkedNode = m_checkedClientShortConnLink.AddLast(checkedInfo);
                    m_checkedClientShortConnLinkNodeDic.Add(connSession.LinkId, checkedNode);
                    connSession.m_ConnType = ClientProtocolDealManager.eConnectionType.Short;
                }
            }

            public CheckedConnectionInfo RemoveCheckedShortConnection(int sessionID)
            {
                LinkedListNode<CheckedConnectionInfo> checkedNode;
                if (m_checkedClientShortConnLinkNodeDic.TryGetValue(sessionID, out checkedNode))
                {
                    m_checkedClientShortConnLinkNodeDic.Remove(sessionID);
                    m_checkedClientShortConnLink.Remove(checkedNode);

                    return checkedNode.Value;
                }
                return null;
            }

            #endregion

            #region 已验证的长连接链表

            public void AddCheckedLongConnection(ListenService serverListen, ConnectionSession connSession, int gatewaySessionID)
            {
                if (m_checkedClientLongConnLinkNodeDic.ContainsKey(connSession.LinkId) == false)
                {
                    CheckedConnectionInfo checkedInfo = new CheckedConnectionInfo()
                    {
                        m_ClientSession = connSession,
                        m_GatewaySessionID = gatewaySessionID
                    };

                    connSession.EnableHeartBeat(true, true);

                    LinkedListNode<CheckedConnectionInfo> checkedNode = m_checkedClientLongConnLink.AddLast(checkedInfo);
                    m_checkedClientLongConnLinkNodeDic.Add(connSession.LinkId, checkedNode);
                    connSession.m_ConnType = ClientProtocolDealManager.eConnectionType.Long;
                }
                else
                {
                    //通知长连接断开
                    ConnectionSession gatewaySid = serverListen.GetSession(gatewaySessionID);
                    if (gatewaySid != null)
                    {
                        ByteArray sendData = com.tieao.mmo.gateway4server.server.XDGatewayProtocolServerHelper.XDLongConnectionClose(connSession.LinkId);
                        gatewaySid.Send(sendData);
                    }
                }
            }

            public CheckedConnectionInfo RemoveCheckedLongConnection(int sessionID)
            {
                LinkedListNode<CheckedConnectionInfo> checkedNode;
                if (m_checkedClientLongConnLinkNodeDic.TryGetValue(sessionID, out checkedNode))
                {
                    m_checkedClientLongConnLinkNodeDic.Remove(sessionID);
                    m_checkedClientLongConnLink.Remove(checkedNode);
                    return checkedNode.Value;
                }
                return null;
            }

            #endregion

            #region 客户端连接断开

            public void ClientConnClose(ListenService clientListen, ListenService serverListen, ConnectionSession connSession)
            {
                switch (connSession.m_ConnType)
                {
                    case ClientProtocolDealManager.eConnectionType.Uncheck:
                        {
                            if(unknowConeectionClose(clientListen, serverListen, connSession) == false)
                            {
                                if(shortConeectionClose(clientListen, serverListen, connSession) == false)
                                    longConnectionClose(clientListen, serverListen, connSession);
                            }
                        }
                        break;
                    //case ClientProtocolDealManager.eConnectionType:
                    //    {
                    //        LinkedListNode<CheckingConnectionInfo> checkingConnNode;
                    //        if (m_checkingConnLinkNodeDic.TryGetValue(sessionID, out checkingConnNode) == true)
                    //        {
                    //            m_checkingConnLinkNodeDic.Remove(sessionID);
                    //            m_checkingConnLink.Remove(checkingConnNode);
                    //        }
                    //    }
                    //    break;
                    case ClientProtocolDealManager.eConnectionType.Short:
                        {
                            if (shortConeectionClose(clientListen, serverListen, connSession) == false)
                            {
                                if (longConnectionClose(clientListen, serverListen, connSession) == false)
                                    unknowConeectionClose(clientListen, serverListen, connSession);
                            }
                        }
                        break;
                    case ClientProtocolDealManager.eConnectionType.Long:
                        {
                            if (longConnectionClose(clientListen, serverListen, connSession) == false)
                            {
                                if(shortConeectionClose(clientListen, serverListen, connSession) == false)
                                    unknowConeectionClose(clientListen, serverListen, connSession);
                            }
                        }
                        break;
                }
            }

            private bool unknowConeectionClose(ListenService clientListen, ListenService serverListen, ConnectionSession connSession)
            {
                LinkedListNode<UncheckConnectionInfo> uncheckConnNode;
                if (m_uncheckConnLinkNodeDic.TryGetValue(connSession.LinkId, out uncheckConnNode) == true)
                {
                    m_uncheckConnLinkNodeDic.Remove(connSession.LinkId);
                    m_uncheckConnLink.Remove(uncheckConnNode);
                    return true;
                }
                return false;
            }

            private bool shortConeectionClose(ListenService clientListen, ListenService serverListen, ConnectionSession connSession)
            {
                LinkedListNode<CheckedConnectionInfo> checkedConnNode;
                if (m_checkedClientShortConnLinkNodeDic.TryGetValue(connSession.LinkId, out checkedConnNode) == true)
                {
                    m_checkedClientShortConnLinkNodeDic.Remove(connSession.LinkId);
                    m_checkedClientShortConnLink.Remove(checkedConnNode);
                    return true;
                }
                return false;
            }

            private bool longConnectionClose(ListenService clientListen, ListenService serverListen, ConnectionSession connSession)
            {
                LinkedListNode<CheckedConnectionInfo> checkedConnNode;
                if (m_checkedClientLongConnLinkNodeDic.TryGetValue(connSession.LinkId, out checkedConnNode) == true)
                {
                    m_checkedClientLongConnLinkNodeDic.Remove(connSession.LinkId);
                    m_checkedClientLongConnLink.Remove(checkedConnNode);
                    if (checkedConnNode.Value.m_GatewaySessionID >= 0)
                    {
                        ConnectionSession gatewaySid = serverListen.GetSession(checkedConnNode.Value.m_GatewaySessionID);
                        if (gatewaySid != null)
                        {
                            ByteArray sendData = com.tieao.mmo.gateway4server.server.XDGatewayProtocolServerHelper.XDLongConnectionClose(connSession.LinkId);
                            gatewaySid.Send(sendData);
                        }
                    }
                    return true;
                }
                return false;
            }

            #endregion

            #region Gateway失联

            public void LoseGateway(int gatewaySid)
            {
                LinkedListNode<CheckedConnectionInfo> checkedConnLinkNode = m_checkedClientShortConnLink.First;
                LinkedListNode<CheckedConnectionInfo> tmpcheckedConnLinkNode;
                while(checkedConnLinkNode != null)
                {
                    if (checkedConnLinkNode.Value.m_GatewaySessionID == gatewaySid)
                    {
                        tmpcheckedConnLinkNode = checkedConnLinkNode.Next;
                        m_checkedClientShortConnLink.Remove(checkedConnLinkNode);
                        m_checkedClientShortConnLinkNodeDic.Remove(checkedConnLinkNode.Value.m_ClientSession.LinkId);
                        checkedConnLinkNode = tmpcheckedConnLinkNode;
                    }
                    else
                        checkedConnLinkNode = checkedConnLinkNode.Next;
                }

                checkedConnLinkNode = m_checkedClientLongConnLink.First;
                while(checkedConnLinkNode != null)
                {
                    if (checkedConnLinkNode.Value.m_GatewaySessionID == gatewaySid)
                    {
                        tmpcheckedConnLinkNode = checkedConnLinkNode.Next;
                        m_checkedClientLongConnLink.Remove(checkedConnLinkNode);
                        m_checkedClientLongConnLinkNodeDic.Remove(checkedConnLinkNode.Value.m_ClientSession.LinkId);
                        checkedConnLinkNode = tmpcheckedConnLinkNode;
                    }
                    else
                        checkedConnLinkNode = checkedConnLinkNode.Next;
                }
            }

            #endregion

            #region Logic Update

            public void LogConnectionCount()
            {
                SvLogger.Info("### Uncheck Connection Count={0}, Checked Short Connection Count={1}, Checked Long Connection Count={2}", m_uncheckConnLink.Count, m_checkedClientShortConnLink.Count, m_checkedClientLongConnLink.Count);
            }

            public void UncheckConnCheckUpdate(ListenService clientListen, int curSvrTime)
            {
                //if (m_isUseLock == true)
                //{
                //    lock (m_uncheckLockObj)
                //    {
                //        uncheckConnCheckUpdate(tickCount);
                //    }
                //}
                //else
                uncheckConnCheckUpdate(clientListen, curSvrTime);
            }

            private void uncheckConnCheckUpdate(ListenService clientListen, int curSvrTime)
            {
                LinkedListNode<UncheckConnectionInfo> uncheckConnNode = m_uncheckConnLink.First;
                while(uncheckConnNode != null)
                {
                    if (uncheckConnNode.Value.m_OpenTime <= curSvrTime)
                    {
                        LinkedListNode<UncheckConnectionInfo> tmpNode = uncheckConnNode.Next;
                        m_uncheckConnLink.Remove(uncheckConnNode);
                        m_uncheckConnLinkNodeDic.Remove(uncheckConnNode.Value.m_ClientSession.LinkId);

                        //超时断开连接
                        ConnectionSession connSession = clientListen.GetSession(uncheckConnNode.Value.m_ClientSession.LinkId);
                        if (connSession != null)
                            connSession.Connection.Close();
                        
                        uncheckConnNode = tmpNode;
                    }
                    else
                        break;
                }
            }

            public void CheckedShortConnCheckUpdate(ListenService clientListen, int curSvrTime)
            {
                //if (m_isUseLock == true)
                //{
                //    lock (m_uncheckLockObj)
                //    {
                //        checkedShortConnCheckUpdate(tickCount);
                //    }
                //}
                //else
                checkedShortConnCheckUpdate(clientListen, curSvrTime);
            }

            private void checkedShortConnCheckUpdate(ListenService clientListen, int curSvrTime)
            {

                LinkedListNode<CheckedConnectionInfo> checkedShortConnNode = m_checkedClientShortConnLink.First;
                while (checkedShortConnNode != null)
                {
                    if (checkedShortConnNode.Value.m_ClientSession.m_CreateTime + m_checdedShortConnLimitSec <= curSvrTime)
                    {
                        LinkedListNode<CheckedConnectionInfo> tmpNode = checkedShortConnNode.Next;
                        m_checkedClientShortConnLink.Remove(checkedShortConnNode);
                        m_checkedClientShortConnLinkNodeDic.Remove(checkedShortConnNode.Value.m_ClientSession.LinkId);

                        //超时断开连接
                        ConnectionSession connSession = clientListen.GetSession(checkedShortConnNode.Value.m_ClientSession.LinkId);
                        if (connSession != null)
                            connSession.Connection.Close();

                        checkedShortConnNode = tmpNode;
                    }
                    else
                        break;
                }
            }

            #endregion

            private int m_uncheckLimitSec = 15;
            private int m_checkingLimitSec = 30;
            private int m_checdedShortConnLimitSec = 15;

            #region Connection Datas

            //private bool m_isUseLock = false;

            private object m_uncheckLockObj = new object();
            //未验证的连接链表
            private LinkedList<UncheckConnectionInfo> m_uncheckConnLink = new LinkedList<UncheckConnectionInfo>();
            //KEY:连接SessionID
            private Dictionary<int, LinkedListNode<UncheckConnectionInfo>> m_uncheckConnLinkNodeDic = new Dictionary<int, LinkedListNode<UncheckConnectionInfo>>();

            private object m_checkedShortLockObj = new object();
            //已验证的短连接链表
            private LinkedList<CheckedConnectionInfo> m_checkedClientShortConnLink = new LinkedList<CheckedConnectionInfo>();
            //已验证的短连接字典： KEY:对外SessionID; VALUE:Gateway SessionID
            private Dictionary<int, LinkedListNode<CheckedConnectionInfo>> m_checkedClientShortConnLinkNodeDic = new Dictionary<int, LinkedListNode<CheckedConnectionInfo>>();

            private object m_checkedLongLockObj = new object();
            //已验证的长连接链表
            private LinkedList<CheckedConnectionInfo> m_checkedClientLongConnLink = new LinkedList<CheckedConnectionInfo>();
            //已验证的长连接字典： KEY:对外SessionID; VALUE:Gateway SessionID
            private Dictionary<int, LinkedListNode<CheckedConnectionInfo>> m_checkedClientLongConnLinkNodeDic = new Dictionary<int, LinkedListNode<CheckedConnectionInfo>>();

            #endregion
        }

        public class ServerConnectionsManager
        {
            public class ServerConnection
            {
                public int m_Port;
                public ConnectionSession m_ConnSession;
            }

            public void AddGatewayConnection(int serverID, ServerConnection svrConn)
            {
                Dictionary<int, ServerConnection> svrConnDic;
                if (m_serverGatewaySessionsDic.TryGetValue(serverID, out svrConnDic) == false)
                {
                    svrConnDic = new Dictionary<int, ServerConnection>();
                    m_serverGatewaySessionsDic.Add(serverID, svrConnDic);
                }

                ServerConnection oldSvrConn;
                if (svrConnDic.TryGetValue(svrConn.m_Port, out oldSvrConn))
                {
                    m_gatewaySidPortDic.Remove(oldSvrConn.m_ConnSession.LinkId);
                    svrConnDic[svrConn.m_Port] = svrConn;
                }
                else
                    svrConnDic.Add(svrConn.m_Port, svrConn);

                m_gatewaySidPortDic.Add(svrConn.m_ConnSession.LinkId, serverID);
            }

            public ConnectionSession GetServerGateway(int serverID, int port)
            {
                Dictionary<int, ServerConnection> gatewaySidDic;
                if (m_serverGatewaySessionsDic.TryGetValue(serverID, out gatewaySidDic))
                {
                    ServerConnection svrConn;
                    if(gatewaySidDic.TryGetValue(port, out svrConn))
                        return svrConn.m_ConnSession;
                }
                return null;
            }

            public void RemoveGatewayConnection(int sessionID)
            {
                int serverID = 0;
                if (m_gatewaySidPortDic.TryGetValue(sessionID, out serverID))
                {
                    Dictionary<int, ServerConnection> svrConnDic = new Dictionary<int, ServerConnection>();
                    if(m_serverGatewaySessionsDic.TryGetValue(serverID, out svrConnDic))
                    {
                        foreach (var svrConnEle in svrConnDic)
                        {
                            if (svrConnEle.Value.m_ConnSession.LinkId == sessionID)
                            {
                                svrConnDic.Remove(svrConnEle.Key);
                                break;
                            }
                        }
                    }
                    m_gatewaySidPortDic.Remove(sessionID);
                }
            }

            public void AddPortalConnection(ServerConnection svrConn)
            {
                bool hadThisPortal = false;
                foreach (var sc in m_portalConnList)
                {
                    if (sc.m_ConnSession.LinkId == svrConn.m_ConnSession.LinkId)
                    {
                        hadThisPortal = true;
                        break;
                    }
                }
                if (hadThisPortal == false)
                    m_portalConnList.Add(svrConn);
            }

            public ConnectionSession RadomServerPortal()
            {
                if(m_portalConnList.Count == 0)
                    return null;

                int rdmIndex = HTBaseFunc.random.Next(0, m_portalConnList.Count);
                return m_portalConnList[rdmIndex].m_ConnSession;
            }

            public void RemovePortalConnection(int sessionID)
            {
                for (int i = 0; i < m_portalConnList.Count; ++i )
                {
                    if (m_portalConnList[i].m_ConnSession.LinkId == sessionID)
                    {
                        m_portalConnList.RemoveAt(i);
                        break;
                    }  
                }
            }

            public void ConnectionClose(ConnectionSession connSession)
            {
                for (int i = 0; i < m_portalConnList.Count; ++i)
                {
                    if (m_portalConnList[i].m_ConnSession.LinkId == connSession.LinkId)
                    {
                        m_portalConnList.RemoveAt(i);
                        break;
                    }
                }
            }

            private Dictionary<int, int> m_gatewaySidPortDic = new Dictionary<int, int>();
            private Dictionary<int, Dictionary<int, ServerConnection>> m_serverGatewaySessionsDic = new Dictionary<int, Dictionary<int, ServerConnection>>();

            private List<ServerConnection> m_portalConnList = new List<ServerConnection>();
        }

        //服务器协议处理管理
        public class ServerProtocolDealManager
        {
            public class ServerForwardProtocol
            {
                public int m_ClientSessionID;
                public ByteArray m_ProtocolData;
            }

            public class ServerRecvProtocol
            {
                public int m_ServerSessionID;
                public ByteArray m_ProtocolData;
                public ClientProtocolDealManager.eConnectionType m_ConnType;
            }

            public class ServerRegistInfo
            {
                public eServerType m_ServerType;
                public int m_ServerID;
                public int m_Port;
                public int m_ServerSessionID;
            }

            public class ServerBoardcastProtocol
            {
                public TIntList m_clientSessionIDList;
                public ByteArray m_ProtocolData;
            }

            //public class ClientForwardProtocol
            //{
            //    public eServerType m_ServerType;
            //    public int m_
            //}

            #region Gateway通知连接断开协议处理

            public void CloseClientConnSessionID(int clientSessionID)
            {
                lock (m_closeClientConnSessionIDListLockObj)
                {
                    m_closeClientConnSessionIDList.Add(clientSessionID);
                }
            }

            #endregion

            #region 玩家连接去Gateway验证返回

            public void ConnectionCheckBack(TConnectionCheck connCheck)
            {
                lock (m_checkClientConnListLockObj)
                {
                    m_checkClientConnList.Add(connCheck);
                }
            }

            #endregion

            #region Server转发数据给客户端

            public void ServerSend2ClientData(int clientSid, ByteArray protocolData)
            {
                ServerForwardProtocol svrForward = new ServerForwardProtocol()
                {
                    m_ClientSessionID = clientSid,
                    m_ProtocolData = protocolData
                };

                lock(m_forwardProtocolListLockObj)
                {
                    m_forwardProtocolList.Add(svrForward);
                }
            }

            #endregion

            #region Server广播数据给客户端

            public void ServerBoardcast2ClientData(ServerBoardcastProtocol boardcastData)
            {
                lock(m_serverBoardcast2ClientProtocolListLockObj)
                {
                    m_serverBoardcast2ClientProtocolList.Add(boardcastData);
                }
            }

            #endregion

            #region 收到服务端数据

            /// <summary>
            /// 收到服务器数据
            /// </summary>
            /// <param name="ServerSessionID"></param>
            /// <param name="protocolData"></param>
            public void RecvServerData(int ServerSessionID, ByteArray protocolData, ClientProtocolDealManager.eConnectionType connType)
            {
                ServerRecvProtocol recvSvrProtocol = new ServerRecvProtocol()
                {
                    m_ServerSessionID = ServerSessionID,
                    m_ProtocolData = protocolData,
                    m_ConnType = connType
                };

                lock (m_serverRecvProtocolListLockObj)
                {
                    //SvLogger.Debug("##########AddSvrProtocol TO List");
                    m_serverRecvProtocolList.Add(recvSvrProtocol);
                }
            }
            
            #endregion

            #region 收到服务器来注册

            public void ServerRegist(eServerType svrType, int serverID, int port, int sessionID)
            {
                ServerRegistInfo registInfo = new ServerRegistInfo()
                {
                    m_ServerType = svrType,
                    m_ServerID = serverID,
                    m_Port = port,
                    m_ServerSessionID = sessionID
                };

                lock (m_serverRegestListLockObj)
                {
                    m_serverRegistInfoList.Add(registInfo);
                }
            }

            #endregion

            #region 数据转移

            public void RecvProtocolTransferData(ref List<ServerRecvProtocol> serverRecvProtocolList)
            {
                lock(m_serverRecvProtocolListLockObj)
                {
                    if (m_serverRecvProtocolList.Count > 0)
                    {
                        //SvLogger.Debug("RecvProtocolTransferData : ProtocolCount={0}.", m_serverRecvProtocolList.Count);
                        serverRecvProtocolList.AddRange(m_serverRecvProtocolList);
                        m_serverRecvProtocolList.Clear();
                    }
                }
            }

            public void DealedProtocolTransferData( ref List<int> closeClientConnSessionIDList,
                                                    ref List<TConnectionCheck> checkClientConnList,
                                                    ref List<ServerForwardProtocol> forwardProtocolList,
                                                    ref List<ServerRegistInfo> serverRegistList,
                                                    ref List<ServerBoardcastProtocol> boardcastList)
            {
                lock (m_closeClientConnSessionIDListLockObj)
                {
                    if (m_closeClientConnSessionIDList.Count > 0)
                    {
                        closeClientConnSessionIDList.AddRange(m_closeClientConnSessionIDList);
                        m_closeClientConnSessionIDList.Clear();
                    }
                }

                lock (m_checkClientConnListLockObj)
                {
                    if (m_checkClientConnList.Count > 0)
                    {
                        checkClientConnList.AddRange(m_checkClientConnList);
                        m_checkClientConnList.Clear();
                    }
                }

                lock(m_forwardProtocolListLockObj)
                {
                    if (m_forwardProtocolList.Count > 0)
                    {
                        forwardProtocolList.AddRange(m_forwardProtocolList);
                        m_forwardProtocolList.Clear();
                    }
                }

                lock(m_serverRegestListLockObj)
                {
                    if(m_serverRegistInfoList.Count > 0)
                    {
                        serverRegistList.AddRange(m_serverRegistInfoList);
                        m_serverRegistInfoList.Clear();
                    }
                }

                lock(m_serverBoardcast2ClientProtocolListLockObj)
                {
                    if(m_serverBoardcast2ClientProtocolList.Count > 0)
                    {
                        boardcastList.AddRange(m_serverBoardcast2ClientProtocolList);
                        m_serverBoardcast2ClientProtocolList.Clear();
                    }
                }
            }

            #endregion

            #region Connection Datas

            private object m_closeClientConnSessionIDListLockObj = new object();
            private List<int> m_closeClientConnSessionIDList = new List<int>();

            private object m_checkClientConnListLockObj = new object();
            private List<TConnectionCheck> m_checkClientConnList = new List<TConnectionCheck>();

            private object m_forwardProtocolListLockObj = new object();
            private List<ServerForwardProtocol> m_forwardProtocolList = new List<ServerForwardProtocol>();

            private object m_serverRegestListLockObj = new object();
            private List<ServerRegistInfo> m_serverRegistInfoList = new List<ServerRegistInfo>();

            private object m_serverRecvProtocolListLockObj = new object();
            private List<ServerRecvProtocol> m_serverRecvProtocolList = new List<ServerRecvProtocol>();

            private object m_serverBoardcast2ClientProtocolListLockObj = new object();
            private List<ServerBoardcastProtocol> m_serverBoardcast2ClientProtocolList = new List<ServerBoardcastProtocol>();

            #endregion

            public static ServerProtocolDealManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            private static ServerProtocolDealManager m_instance = new ServerProtocolDealManager();
            private ServerProtocolDealManager() { }
        }

        //客户端协议处理管理
        public class ClientProtocolDealManager
        {
            public enum eConnectionType
            {
                Uncheck,
                Short,
                Long,
            }

            public enum eForwardServerType
            {
                Portal = 0,
                Center = 1,
                Gateway = 2,
                Game = 3,
                Chat = 4,
                WorldBoss = 5,
            }

            public enum eForward2Server4What
            {
                PortalInfo,
                AccountCheckWithPassword,
                AccountCheckWithPlatformToken,
                LongConnRegist,
                Server,
            }

            public class Forward2ClientProtocol
            {
                public int m_ClientSessionID;
                public eConnectionType m_ConnType;
                public ByteArray m_ProtocolData;
            }

            public class Forward2ServerProtocol
            {
                public int m_ClientSessionID;
                public eForward2Server4What m_4What;
                public int m_ServerID;
                public int m_Port;
                public int m_ProtocolIndex;
                public byte m_ServerType;
                public PtGuid m_UserID;
                public ByteArray m_ProtocolData;
                public ClientProtocolDealManager.eConnectionType m_ConnType;
            }

            public class RecvClientProtocol
            {
                public int m_ClientSessionID;
                public ByteArray m_ProtocolData;
                public ClientProtocolDealManager.eConnectionType m_ConnType;
            }

            public void HadRecvClientProtocol(int clientSessionID, ByteArray protocolData, ClientProtocolDealManager.eConnectionType connType)
            {
                RecvClientProtocol rcp = new RecvClientProtocol()
                {
                    m_ClientSessionID = clientSessionID,
                    m_ProtocolData = protocolData,
                    m_ConnType = connType
                };

                lock (m_recvClientProtocolListLockObj)
                {
                    m_recvClientProtocolList.Add(rcp);
                }
            }

            public void HadForward2ClientProtocol(Forward2ClientProtocol forwardProto)
            {
                lock (m_forward2ClientProtocolListLockObj)
                {
                    m_forward2ClientProtocolList.Add(forwardProto);
                }
            }

            public void HadForward2ServerProtocol(Forward2ServerProtocol forwardProto)
            {
                lock(m_forward2ServerProtocolListLockObj)
                {
                    m_forward2ServerProtocolList.Add(forwardProto);
                }
            }

            public void TransferRecvClientData(ref List<RecvClientProtocol> recvProtoList)
            {
                lock (m_recvClientProtocolListLockObj)
                {
                    if(m_recvClientProtocolList.Count > 0)
                    {
                        recvProtoList.AddRange(m_recvClientProtocolList);
                        m_recvClientProtocolList.Clear();
                    }
                }
            }

            public void TransferForward2ClientData(ref List<Forward2ClientProtocol> clientProtocolList)
            {
                lock (m_forward2ClientProtocolListLockObj)
                {
                    if (m_forward2ClientProtocolList.Count > 0)
                    {
                        clientProtocolList.AddRange(m_forward2ClientProtocolList);
                        m_forward2ClientProtocolList.Clear();
                    }
                }
            }

            public void TransferForward2ServerData(ref List<Forward2ServerProtocol> serverProtocolList)
            {
                lock(m_forward2ServerProtocolListLockObj)
                {
                    if(m_forward2ServerProtocolList.Count > 0)
                    {
                        serverProtocolList.AddRange(m_forward2ServerProtocolList);
                        m_forward2ServerProtocolList.Clear();
                    }
                }
            }

            private object m_forward2ClientProtocolListLockObj = new object();
            private List<Forward2ClientProtocol> m_forward2ClientProtocolList = new List<Forward2ClientProtocol>();

            private object m_forward2ServerProtocolListLockObj = new object();
            private List<Forward2ServerProtocol> m_forward2ServerProtocolList = new List<Forward2ServerProtocol>();

            private object m_forward2PortalListLockObj = new object();
            private List<Forward2ServerProtocol> m_forward2PortalList = new List<Forward2ServerProtocol>();

            private object m_forward4AccountCheckListLockObj = new object();
            private List<Forward2ServerProtocol> m_forward4AccountCheckProtocolList = new List<Forward2ServerProtocol>();

            private object m_forward4LongConnRegistListLockObj = new object();
            private List<Forward2ServerProtocol> m_forward4LongConnRegistList = new List<Forward2ServerProtocol>();
            
            private object m_recvClientProtocolListLockObj = new object();
            private List<RecvClientProtocol> m_recvClientProtocolList = new List<RecvClientProtocol>();

            public static ClientProtocolDealManager Instance
            {
                get
                {
                    return m_instance;
                }
            }

            private static ClientProtocolDealManager m_instance = new ClientProtocolDealManager();
            private ClientProtocolDealManager() { }
        }
    }
#endif
}
