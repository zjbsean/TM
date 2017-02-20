using System;
using System.Collections;
using System.Collections.Generic;
using com.tieao.mmo.interval;
using com.tieao.mmo.interval.client;
using com.tieao.mmo.interval.server;
using com.ideadynamo.foundation.buffer;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;

namespace ServerCommon
{
#if SocketConnectMode
    //内部通信注册协议处理类
    public class InternalProtocolDealer : IInternalProtocolClientService, IInternalProtocolServerService
    {
        //当前消息Sesssion
        private RecvedProtocolDataInfo m_protoData;

        /// <summary>
        /// 协议解析处理
        /// </summary>
        /// <param name="dates">协议数据</param>
        /// <param name="sessionID">连接SessionID</param>
        /// <returns>是否解析成功</returns>
        public bool Parse(RecvedProtocolDataInfo protoData)
        {
            m_protoData = protoData;
            if (InternalProtocolClientHelper.IntepretMessage(m_protoData.m_Datas, this)) return true;
            if (InternalProtocolServerHelper.IntepretMessage(m_protoData.m_Datas, this)) return true;
            return false;
        }


        #region IInternalProtocolClientService
        
        /// <summary>
        /// 活跃通知
        /// </summary>
        public void OnActivePingSvr()
        {

        }

        /// <summary>
        /// 有服务来注册
        /// </summary>
        /// <param name="svrInfo">服务信息</param>
        /// <param name="sessionID"></param>
        public void OnRegisterSvr(ref com.tieao.mmo.interval.PtServerInfo svrInfo)
        {
            SvLogger.Info("OnRegisterSvr Begin: ServerType={0}, ServerName={1}, sessionID={2}.", svrInfo.m_Type, svrInfo.m_Name, m_protoData.m_SessionID);
            svrInfo.m_SessionID = m_protoData.m_SessionID;

            int errCode = RegServerManager.Instance.RegServer(ref svrInfo);
            if (errCode == 0)
            {
                ServerCommon.Network.NetworkManager.Instance.OnServerRegist(svrInfo);

                PtServerList connectSvrList = null;
                //注册成功
                if(SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.CENTER)
                    connectSvrList = RegServerManager.Instance.GetServerListWithout(ref svrInfo);
                else
                    connectSvrList = new PtServerList();
                Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolClientHelper.RegisterSvrOK(RegServerManager.Instance.m_SelfSvrInfo, connectSvrList));
                SvLogger.Info("OnRegisterSvr OK: ServerType={0}, ServerName={1}, sessionID={2}.", svrInfo.m_Type, svrInfo.m_Name, m_protoData.m_SessionID);

                InternalProtocolDealDelegate.Instance.OnRegistServer(svrInfo, "Succ");

                if(SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.CENTER)
                {
                    ByteArray sendData = InternalProtocolClientHelper.ReportServerID(SvrCommCfg.Instance.ServerInfo.m_ServerID);
                    Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, sendData);
                }
            }
            else
            {
                //注册失败
                Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolClientHelper.RegisterSvrFail(errCode, RegServerManager.Instance.m_SelfSvrInfo));
                SvLogger.Info("OnRegisterSvr Fail: ServerType={0}, ServerName={1}, sessionID={2}, ErrCode={3}.", svrInfo.m_Type, svrInfo.m_Name, m_protoData.m_SessionID, errCode);

                InternalProtocolDealDelegate.Instance.OnRegistServer(svrInfo, "Fail");
            }
        }

        /// <summary>
        /// 有服务来重新注册（其他Server向CenterServer注册用）
        /// </summary>
        /// <param name="svrInfo">服务信息</param>
        public void OnReRegisterSvr(ref PtServerInfo svrInfo)
        {
            SvLogger.Info("OnReRegisterSvr Begin: ServerType={0}, ServerName={1}, sessionID={2}.", svrInfo.m_Type, svrInfo.m_Name, m_protoData.m_SessionID);
            svrInfo.m_SessionID = m_protoData.m_SessionID;
            int errCode = RegServerManager.Instance.RegServer(ref svrInfo);
            if (errCode == 0)
            {
                ServerCommon.Network.NetworkManager.Instance.OnServerRegist(svrInfo);

                //注册成功
                Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolClientHelper.ReRegisterSvrOK(RegServerManager.Instance.m_SelfSvrInfo));
                SvLogger.Info("OnReRegisterSvr OK: ServerType={0}, ServerName={1}, sessionID={2}.", svrInfo.m_Type, svrInfo.m_Name, m_protoData.m_SessionID);

                InternalProtocolDealDelegate.Instance.OnReregistServer(svrInfo, "Succ");
            }
            else
            {
                //注册失败
                Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolClientHelper.ReRegisterSvrFail(errCode, "Reregist Center Server Fail!"));
                SvLogger.Info("OnReRegisterSvr Fail: ServerType={0}, ServerName={1}, sessionID={2}, errCode={3}.", svrInfo.m_Type, svrInfo.m_Name, m_protoData.m_SessionID, errCode);

                InternalProtocolDealDelegate.Instance.OnReregistServer(svrInfo, "Fail");
            }
        }

        /// <summary>
        /// 有服务通知连接（除CenterSrever之外其他服务之间互联验证用）
        /// </summary>
        /// <param name="svrinfo">服务信息</param>
        /// <param name="sessionID"></param>
        public void OnNotifyConnectorSvr(ref com.tieao.mmo.interval.PtServerInfo svrinfo)
        {
            SvLogger.Info("OnNotifyConnectorSvr Begin: ServerType={0}, ServerName={1}, sessionID={2}.", svrinfo.m_Type, svrinfo.m_Name, m_protoData.m_SessionID);
            svrinfo.m_SessionID = m_protoData.m_SessionID;
            int errCode = RegServerManager.Instance.RegServer(ref svrinfo);
            if (errCode == 0)
            {
                //连接成功
                Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolClientHelper.ConnectSvrSucc(RegServerManager.Instance.m_SelfSvrInfo));
                SvLogger.Info("OnNotifyConnectorSvr OK: ServerType={0}, ServerName={1}, sessionID={2}.", svrinfo.m_Type, svrinfo.m_Name, m_protoData.m_SessionID);

                if (SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.GAMEDATA && svrinfo.m_Type == eServerType.DATABASE)
                {
                    //通知DBS可以发送数据过来
                    Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, com.tieao.mmo.database4server.server.GDSUpdate2DBSServerHelper.RequestAllPlayerData());
                }

                InternalProtocolDealDelegate.Instance.OnConnectServerSucc(svrinfo, "Succ");
            }
            else
            {
                //连接验证失败
                Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolClientHelper.ConnectSvrFail(errCode, RegServerManager.Instance.m_SelfSvrInfo));
                SvLogger.Info("OnNotifyConnectorSvr Fail: ServerType={0}, ServerName={1}, sessionID={2}, errCode={3}.", svrinfo.m_Type, svrinfo.m_Name, m_protoData.m_SessionID, errCode);

                InternalProtocolDealDelegate.Instance.OnConnectServerFail(svrinfo, "");
            }
        }

        /// <summary>
        /// 通知玩家服务分配信息
        /// </summary>
        /// <param name="userID">玩家ID</param>
        /// <param name="allocSvrList">服务分配信息</param>
        public void OnNotifyPlayerSvrAllocInfo(string loginName, ref PtPlayerAllocSvrList allocSvrList)
        {
            UserSvrInfo svrInfo = new UserSvrInfo();
            svrInfo.m_LoginName = loginName;
            //SvLogger.Debug("OnNotifyPlayerSvrAllocInfo Begin: PlayerFlagID={0}, AllocSvrCount={1}.", svrInfo.m_PlayerFlagID, allocSvrList.GetElements().Count);

            for (int i = 0; i < allocSvrList.GetElements().Count; ++i)
            {
                if (allocSvrList.GetElements()[i].svrType == eServerType.GAME)
                    svrInfo.m_GameServerName = allocSvrList.GetElements()[i].svrName;
                else if (allocSvrList.GetElements()[i].svrType == eServerType.GATEWAY)
                    svrInfo.m_GateWayName = allocSvrList.GetElements()[i].svrName;
            }
            AllocSvrMap.Instance.AddPlayerSvrInfo(svrInfo);

            //SvLogger.Debug("OnNotifyPlayerSvrAllocInfo Done.");
        }

        /// <summary>
        /// 通知玩家、角色服务分配信息
        /// </summary>
        /// <param name="playerIDList"></param>
        /// <param name="allocSvrList"></param>
        public void OnNotifyPlayerListSvrAllocInfo(ref TStrList loginNameList, ref PtPlayerAllocSvrList allocSvrList)
        {
            for (int i = 0; i < loginNameList.GetElements().Count; ++i)
            {
                UserSvrInfo svrInfo = new UserSvrInfo();
                svrInfo.m_LoginName = loginNameList.GetElements()[i];
                for (int j = 0; j < allocSvrList.GetElements().Count; ++j)
                {
                    if (allocSvrList.GetElements()[j].svrType == eServerType.GAME)
                        svrInfo.m_GameServerName = allocSvrList.GetElements()[j].svrName;
                    else if (allocSvrList.GetElements()[j].svrType == eServerType.GATEWAY)
                        svrInfo.m_GateWayName = allocSvrList.GetElements()[j].svrName;
                }
                AllocSvrMap.Instance.AddPlayerSvrInfo(svrInfo);
            }
        }

        /// <summary>
        /// 通知玩家服务分配信息改变
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="allocSvrInfo"></param>
        public void OnNotifyPlayerSvrAllocUpdate(ref PtGuid userID, ref PtPlayerAllocSvrInfo allocSvrInfo)
        {
            //Guid ugid = Network.NetworkManager.PtGuidConvertToGuid(userID);
            //SvLogger.Debug("OnNotifyPlayerSvrAllocUpdate Begin : UsreID={0}, SvrType={1}, SvrName={2}.", ugid.ToString(), allocSvrInfo.svrType, allocSvrInfo.svrName);

            //UserSvrInfo userSvrInfo = AllocSvrMap.Instance.GetPlayerSvrInfo(ugid);

            //if (userSvrInfo != null)
            //{
            //    if (allocSvrInfo.svrType == eServerType.GAME)
            //        userSvrInfo.m_GameServerName = allocSvrInfo.svrName;
            //    else if (allocSvrInfo.svrType == eServerType.GATEWAY)
            //        userSvrInfo.m_GateWayName = allocSvrInfo.svrName;
            //}

            //SvLogger.Debug("OnNotifyPlayerSvrAllocUpdate Done.");
        }

        /// <summary>
        /// 删除角色服务分配信息
        /// </summary>
        /// <param name="charaID"></param>
        public void OnNoticeDeleteCharacterAllocSvrInfo(ref PtGuid charaID)
        {
            //Guid cgid = Network.NetworkManager.PtGuidConvertToGuid(charaID);
            //AllocSvrMap.Instance.DeleteCharaSvrInfo(cgid);
        }

        /// <summary>
        /// 执行指令
        /// </summary>
        /// <param name="command"></param>
        public void OnDealCommand(string command)
        {
            InternalProtocolDealDelegate.Instance.OnHadCommand(command);
        }

        #endregion

        #region IInternalProtocolServerService

        /// <summary>
        /// 活跃通知超时，需马上发送
        /// </summary>
        /// <param name="sessionID"></param>
        public void OnActivePingSvrTimeOut()
        {

        }

        /// <summary>
        /// 注册服务成功
        /// </summary>
        /// <param name="svrinfo">注册服务信息</param>
        /// <param name="regedsvrlist">已经注册的服务信息列表</param>
        /// <param name="sessionID"></param>
        public void OnRegisterSvrOK(ref com.tieao.mmo.interval.PtServerInfo svrinfo, ref com.tieao.mmo.interval.PtServerList regedsvrlist)
        {
            SvLogger.Info("OnRegisterSvrOK Begin: ServerType={0}, ServerName={1}, RegedServerCount={2}, sessionID={3}.", svrinfo.m_Type, svrinfo.m_Name, regedsvrlist.GetElements().Count, m_protoData.m_SessionID);
            List<eServerType> connectSvrTypeList = new List<eServerType>();
            svrinfo.m_SessionID = m_protoData.m_SessionID;
            int errCode = RegServerManager.Instance.RegServer(ref svrinfo);
            if (errCode != 0)
            {
                SvLogger.Error("    RegisterFail : ErrCode={0}.", errCode);
                InternalProtocolDealDelegate.Instance.OnRegistServerSucc(svrinfo, errCode.ToString());
            }
            else
            {
                if (svrinfo.m_Type == eServerType.CENTER)
                    SvrCommCfg.Instance.ServerInfo.m_ServerID = svrinfo.m_ServerID;

                switch (RegServerManager.Instance.m_SelfSvrInfo.m_Type)
                {
                    case eServerType.GATEWAY:
                        {
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.WORLDBOSS);
                            connectSvrTypeList.Add(eServerType.CHAT);
                            //connectSvrTypeList.Add(eServerType.PUBLIC);
                        }
                        break;
                    case eServerType.GAME:
                        {
                            connectSvrTypeList.Add(eServerType.GATEWAY);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.RANK);
                            connectSvrTypeList.Add(eServerType.LOG);
                            connectSvrTypeList.Add(eServerType.WORLDBOSS);
                            connectSvrTypeList.Add(eServerType.CHAT);
                        }
                        break;
                    case eServerType.CENTER:
                        {
                        }
                        break;
                    case eServerType.GAMEDATA:
                        {
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.DATABASE);
                            connectSvrTypeList.Add(eServerType.LOG);
                            connectSvrTypeList.Add(eServerType.WORLDBOSS);
                            connectSvrTypeList.Add(eServerType.CHAT);
                        }
                        break;
                    case eServerType.PUBLIC:
                        {
                            //connectSvrTypeList.Add(eServerType.GATEWAY);
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.DATABASE);
                            connectSvrTypeList.Add(eServerType.RANK);
                            connectSvrTypeList.Add(eServerType.WORLDBOSS);
                            connectSvrTypeList.Add(eServerType.LOG);
                            connectSvrTypeList.Add(eServerType.CHAT);
                        }
                        break;
                    case eServerType.DATABASE:
                        {
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.LOG);
                        }
                        break;
                    case eServerType.RANK:
                        {
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                        }
                        break;
                    case eServerType.LOG:
                        {
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.DATABASE);
                            connectSvrTypeList.Add(eServerType.CHAT);
                        }
                        break;
                    case eServerType.CROSSREALM:
                        break;
                    case eServerType.CROSSREALM_BATTLE:
                        break;
                    case eServerType.PROTAL:
                        break;
                    case eServerType.MONITEORNODE:
                        break;
                    case eServerType.GM:
                        break;
                    case eServerType.PLATFORM_DOCKING:
                        break;
                    case eServerType.MONITEORSERVER:
                        break;
                    case eServerType.WORLDBOSS:
                        {
                            connectSvrTypeList.Add(eServerType.GATEWAY);
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.LOG);
                            connectSvrTypeList.Add(eServerType.CHAT);
                        }
                        break;
                    case eServerType.CHAT:
                        {
                            connectSvrTypeList.Add(eServerType.GATEWAY);
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.LOG);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.WORLDBOSS);
                        }
                        break;
                    case eServerType.COMMANDSENDER:
                        {
                            connectSvrTypeList.Add(eServerType.DATABASE);
                            connectSvrTypeList.Add(eServerType.GAMEDATA);
                            connectSvrTypeList.Add(eServerType.GAME);
                            connectSvrTypeList.Add(eServerType.PUBLIC);
                            connectSvrTypeList.Add(eServerType.RANK);
                            connectSvrTypeList.Add(eServerType.GATEWAY);
                            connectSvrTypeList.Add(eServerType.CHAT);
                            connectSvrTypeList.Add(eServerType.WORLDBOSS);
                        }
                        break;
                    case eServerType.UNKNOW:
                        SvLogger.Error("    This Server Type Unknow!");
                        break;
                }

                for (int i = 0; i < regedsvrlist.GetElements().Count; ++i)
                {
                    if (connectSvrTypeList.Contains(regedsvrlist.GetElements()[i].m_Type))
                    {
                        bool result = Network.NetworkManager.Instance.OpenRegServerConnection(regedsvrlist.GetElements()[i]);
                        if (result == true)
                        {
                            SvLogger.Info("     Start To Connect Server : ServerType={0}, IP={1}, Port={2}.",
                                                regedsvrlist.GetElements()[i].m_Type.ToString(),
                                                regedsvrlist.GetElements()[i].m_Address,
                                                regedsvrlist.GetElements()[i].m_Port);

                        }
                        else
                        {
                            SvLogger.Error("    Connect To Server Fail : ServerType={0}, IP={1}, Port={2}.",
                                                regedsvrlist.GetElements()[i].m_Type.ToString(),
                                                regedsvrlist.GetElements()[i].m_Address,
                                                regedsvrlist.GetElements()[i].m_Port);
                        }
                    }
                }

                if (Network.NetworkManager.Instance.IsNeedRegistServer(svrinfo.m_Type))
                    Network.NetworkManager.Instance.RegistToServerSucc(m_protoData.m_SessionID, ref svrinfo);

                InternalProtocolDealDelegate.Instance.OnRegistServerSucc(svrinfo, "0");
            }

            SvLogger.Info("OnRegisterSvrOK Done: ServerType={0}, ServerName={1}, NeedConnectServerCount={2}, sessionID={3}.", svrinfo.m_Type, svrinfo.m_Name, connectSvrTypeList.Count, m_protoData.m_SessionID);
        }

        /// <summary>
        /// 注册服务失败
        /// </summary>
        /// <param name="errcode">错误ID</param>
        /// <param name="svrinfo">注册服务信息</param>
        /// <param name="sessionID"></param>
        public void OnRegisterSvrFail(int errcode, ref com.tieao.mmo.interval.PtServerInfo svrinfo)
        {
            InternalProtocolDealDelegate.Instance.OnRegistServerFail(svrinfo, errcode.ToString());
            SvLogger.Error("OnRegisterSvrFail : ServerType={0}, IP={1}, Port={2}, ErrCode={3}.", svrinfo.m_Type, svrinfo.m_Address, svrinfo.m_Port, errcode);
        }

        /// <summary>
        ///  通知服务器关闭
        /// </summary>
        /// <param name="param">关闭参数</param>
        /// <param name="sessionID"></param>
        public void OnShutDownSvr(string param)
        {
            //关闭服务
            SvLogger.Info("OnShutDownSvr : param={0}", param);
            BasicService.RecvCenterServerCloseCommand();
        }

        /// <summary>
        /// 连接服务器成功
        /// </summary>
        /// <param name="svrinfo">服务器信息</param>
        /// <param name="sessionID"></param>
        public void OnConnectSvrSucc(ref com.tieao.mmo.interval.PtServerInfo svrinfo)
        {
            SvLogger.Info("OnConnectSvrSucc Begin : ServerType={0}, IP={1}, Port={2}.", svrinfo.m_Type, svrinfo.m_Address, svrinfo.m_Port);

            if (Network.NetworkManager.Instance.IsNeedRegistServer(svrinfo.m_Type))
            {
                SvLogger.Info("     RegServer To {2} Server : IP={0}, Port={1}.", svrinfo.m_Address, svrinfo.m_Port, svrinfo.m_Type);
                if (Network.NetworkManager.Instance.HadRegistedServer(svrinfo.m_Type) == true)
                {
                    //去重新注册
                    Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolServerHelper.ReRegisterSvr(SvrCommCfg.Instance.ServerInfo));
                }
                else
                {
                    //去注册
                    Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, InternalProtocolServerHelper.RegisterSvr(SvrCommCfg.Instance.ServerInfo));
                }
            }
            else
            {
                svrinfo.m_SessionID = m_protoData.m_SessionID;
                int errCode = RegServerManager.Instance.RegServer(ref svrinfo);
                if (errCode != 0)
                    SvLogger.Error("     RegServer Fail!");
                else
                {
                    if (SvrCommCfg.Instance.ServerInfo.m_Type == eServerType.GAMEDATA && svrinfo.m_Type == eServerType.DATABASE)
                    {
                        //通知DBS可以发送数据过来
                        Network.NetworkManager.Instance.SendMessageToServer(m_protoData.m_SessionID, com.tieao.mmo.database4server.server.GDSUpdate2DBSServerHelper.RequestAllPlayerData());
                    }
                }
            }
            InternalProtocolDealDelegate.Instance.OnConnectServerSucc(svrinfo, "");
            SvLogger.Info("OnConnectSvrSucc Done.");
        }

        /// <summary>
        /// 连接服务器失败
        /// </summary>
        /// <param name="errcode">错误信息</param>
        /// <param name="svrinfo">连接的服务器信息</param>
        public void OnConnectSvrFail(int errcode, ref com.tieao.mmo.interval.PtServerInfo svrinfo)
        {
            InternalProtocolDealDelegate.Instance.OnConnectServerFail(svrinfo, "");
            SvLogger.Info("OnConnectSvrFail : ServerType={0}, IP={1}, Port={2}, ErrCode={3}.", svrinfo.m_Type, svrinfo.m_Address, svrinfo.m_Port, errcode);
        }

        /// <summary>
        /// 重新向CenterServer注册成功
        /// </summary>
        /// <param name="svrInfo">CenterServer信息</param>
        public void OnReRegisterSvrOK(ref com.tieao.mmo.interval.PtServerInfo svrInfo)
        {
            SvLogger.Info("OnReRegisterSvrOK Begin : ServerType={0}, IP={1}, Port={2}.", svrInfo.m_Type, svrInfo.m_Address, svrInfo.m_Port);

            svrInfo.m_SessionID = m_protoData.m_SessionID;
            int errCode = RegServerManager.Instance.RegServer(ref svrInfo);
            if (errCode != 0)
                SvLogger.Error("    RegServer Fail : errCode={0}.", errCode);

            InternalProtocolDealDelegate.Instance.OnReregistServerSucc(svrInfo, errCode.ToString());
            
            SvLogger.Info("OnConnectSvrSucc Done.");
        }

        /// <summary>
        /// 重新向CenterServer注册失败
        /// </summary>
        /// <param name="errCode">错误ID</param>
        /// <param name="errMsg">错误消息</param>
        public void OnReRegisterSvrFail(int errCode, string errMsg)
        {
            SvLogger.Error("OnReRegisterSvrFail: ErrCode={0}, ErrMsg={1}.", errCode, errMsg);
            ServerCommon.Network.NetworkManager.Instance.CloseServerConnection(m_protoData.m_SessionID);
        }

        /// <summary>
        /// 报告ServerID
        /// </summary>
        /// <param name="serverID"></param>
        public void OnReportServerID(int serverID)
        {
            InternalProtocolDealDelegate.Instance.OnGetServerID(serverID);
        }

        #endregion

        public static InternalProtocolDealer Instance
        {
            get
            {
                return _instance;
            }
        }
        private InternalProtocolDealer() { }
        private static InternalProtocolDealer _instance = new InternalProtocolDealer();
      

    }
#endif
}
