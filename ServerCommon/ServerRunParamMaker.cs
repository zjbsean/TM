using System;
using System.Collections.Generic;
using System.Text;
using com.tieao.mmo.interval;
using com.tieao.mmo.CustomTypeInProtocol;
using com.ideadynamo.foundation.buffer;
using GsTechLib;

namespace ServerCommon
{
    //服务运行参数生成
    public class ServerRunParamConvert
    {
        public static ServerRunParamConvert Instance
        {
            get
            {
                return m_instance;
            }
        }

        #region CenterServer

        public string CenterServerRunParamConvert(string path, TCenterServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();
            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.serverGroupName).Append(" ");
            paramArgs.Append(cfgData.gameID.ToString()).Append(" ");
            paramArgs.Append(cfgData.serverID.ToString()).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort.ToString()).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.crossRealmServer.Key, cfgData.crossRealmServer.Value)).Append(" ");
            StringBuilder strBuild = new StringBuilder();
            foreach (var ele in cfgData.protalServerList.GetElements())
                strBuild.Append(string.Format("{0},{1};", ele.Key, ele.Value));
            if (strBuild.Length == 0)
                strBuild.Append(",");
            paramArgs.Append(strBuild.ToString()).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.gmServer.Key, cfgData.gmServer.Value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.platformDockingServer.Key, cfgData.platformDockingServer.Value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));
            return paramArgs.ToString();
        }

        public TCenterServerCfgData CenterServerRunParamConvert(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;

            TCenterServerCfgData cfgData = new TCenterServerCfgData();

            cfgData.serverGroupName = runParams[1];
            cfgData.gameID = Convert.ToInt32(runParams[2]);
            cfgData.serverID = Convert.ToInt32(runParams[3]);
            cfgData.lanIPAddr = runParams[4];
            cfgData.inPort = Convert.ToInt32(runParams[5]);

            TIDStrKeyValue idIPPort = new TIDStrKeyValue();
            idIPPort.Key = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            idIPPort.Value = HTBaseFunc.DepartStr(runParams[6], ",", 1);

            if (idIPPort.Value == "")
                idIPPort.Value = "0";
            cfgData.crossRealmServer = idIPPort;

            cfgData.protalServerList = new TIDStrKeyValueList();
            int index = 0;
            string curStr = HTBaseFunc.DepartStr(runParams[7], ";", index);
            while (curStr != "")
            {
                idIPPort = new TIDStrKeyValue();
                idIPPort.Key = HTBaseFunc.DepartStr(curStr, ",", 0);
                idIPPort.Value = HTBaseFunc.DepartStr(curStr, ",", 1);
                if (idIPPort.Value == "")
                    idIPPort.Value = "0";
                cfgData.protalServerList.Add(idIPPort);

                ++index;
                curStr = HTBaseFunc.DepartStr(runParams[7], ";", index);
            }

            idIPPort = new TIDStrKeyValue();
            idIPPort.Key = HTBaseFunc.DepartStr(runParams[8], ",", 0);
            idIPPort.Value = HTBaseFunc.DepartStr(runParams[8], ",", 1);
            if (idIPPort.Value == "")
                idIPPort.Value = "0";
            cfgData.gmServer = idIPPort;

            idIPPort = new TIDStrKeyValue();
            idIPPort.Key = HTBaseFunc.DepartStr(runParams[9], ",", 0);
            idIPPort.Value = HTBaseFunc.DepartStr(runParams[9], ",", 1);
            if (idIPPort.Value == "")
                idIPPort.Value = "0";
            cfgData.platformDockingServer = idIPPort;

            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[10], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[10], ",", 1));

            return cfgData;
        }

        #endregion

        #region DataServer

        public string DataServerRunParamConvert(string path, TDataBaseServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();
            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            foreach (var dbCfg in cfgData.dbCfgList.GetElements())
            {
                string dbConnStr = string.Format("{0},{1},{2},{3},{4},{5},{6};", dbCfg.dbType, dbCfg.lanIPAddr, dbCfg.dbName, dbCfg.loginName, dbCfg.passworld, dbCfg.port, dbCfg.childConnCount);
                paramArgs.Append(dbConnStr);
            }
            paramArgs.Append(" ").Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));
            return paramArgs.ToString();
        }

        public TDataBaseServerCfgData DataServerRunParamConvert(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;

            TDataBaseServerCfgData cfgData = new TDataBaseServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.lanIPAddr = runParams[3];
            cfgData.inPort = Convert.ToInt32(runParams[4]);

            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[5], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[5], ",", 1);

            cfgData.dbCfgList = new TDBCfgList();
            int index = 0;
            string curStr = HTBaseFunc.DepartStr(runParams[6], ";", index);
            while (curStr != "")
            {
                TDBCfg dbCfg = new TDBCfg();
                dbCfg.dbType = Convert.ToInt32(HTBaseFunc.DepartStr(curStr, ",", 0));
                dbCfg.lanIPAddr = HTBaseFunc.DepartStr(curStr, ",", 1);
                dbCfg.dbName = HTBaseFunc.DepartStr(curStr, ",", 2);
                dbCfg.loginName = HTBaseFunc.DepartStr(curStr, ",", 3);
                dbCfg.passworld = HTBaseFunc.DepartStr(curStr, ",", 4);
                dbCfg.port = Convert.ToInt32(HTBaseFunc.DepartStr(curStr, ",", 5));
                dbCfg.childConnCount = Convert.ToInt32(HTBaseFunc.DepartStr(curStr, ",", 6));
                
                cfgData.dbCfgList.Add(dbCfg);

                ++index;
                curStr = HTBaseFunc.DepartStr(runParams[6], ";", index);
            }
            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[7], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[7], ",", 1));
            return cfgData;
        }

        #endregion

        #region GameDataServer

        public string GameDataServerRunParamConvert(string path, TGameDataServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));
            return paramArgs.ToString();
        }

        public TGameDataServerCfgData GameDataServerRunParamConvert(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;
            TGameDataServerCfgData cfgData = new TGameDataServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.lanIPAddr = runParams[3];
            cfgData.inPort = Convert.ToInt32(runParams[4]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[5], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[5], ",", 1);
            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[6], ",", 1));

            return cfgData;
        }

        #endregion

        #region GameServer

        public string GameServerRunParamConvert(string path, TGameServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.index).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }

        public TGameServerCfgData GameServerRunParamConvert(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;
            TGameServerCfgData cfgData = new TGameServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.index = Convert.ToInt32(runParams[3]);
            cfgData.lanIPAddr = runParams[4];
            cfgData.inPort = Convert.ToInt32(runParams[5]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[6], ",", 1);
            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[7], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[7], ",", 1));

            return cfgData;
        }

        #endregion

        #region PublicLogicServer

        public string PublicServerRunParamConvert(string path, TPublicLogicServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }

        public TPublicLogicServerCfgData PublicServerRunParamConvert(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;
            TPublicLogicServerCfgData cfgData = new TPublicLogicServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.lanIPAddr = runParams[3];
            cfgData.inPort = Convert.ToInt32(runParams[4]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[5], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[5], ",", 1);
            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[6], ",", 1));

            return cfgData;
        }

        #endregion

        #region RankServer

        public string RankServerRunParamConvert(string path, TRankServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();
            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");

            foreach (var dbCfg in cfgData.dbCfgList.GetElements())
            {
                string dbConnStr = string.Format("{0},{1},{2},{3},{4},{5},{6};", dbCfg.dbType, dbCfg.lanIPAddr, dbCfg.dbName, dbCfg.loginName, dbCfg.passworld, dbCfg.port, dbCfg.childConnCount);
                paramArgs.Append(dbConnStr);
            }
            paramArgs.Append(" ").Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));
            return paramArgs.ToString();
        }

        public TRankServerCfgData RankServerRunParamConvert(string[] runParams, out string protalSvrAddr, out int protalSvrPort)
        {
            protalSvrAddr = "";
            protalSvrPort = 0;
            TRankServerCfgData cfgData = new TRankServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.lanIPAddr = runParams[3];
            cfgData.inPort = Convert.ToInt32(runParams[4]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[5], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[5], ",", 1);

            cfgData.dbCfgList = new TDBCfgList();
            int index = 0;
            string curStr = HTBaseFunc.DepartStr(runParams[6], ";", index);
            while (curStr != "")
            {
                TDBCfg dbCfg = new TDBCfg();
                dbCfg.dbType = Convert.ToInt32(HTBaseFunc.DepartStr(curStr, ",", 0));
                dbCfg.lanIPAddr = HTBaseFunc.DepartStr(curStr, ",", 1);
                dbCfg.dbName = HTBaseFunc.DepartStr(curStr, ",", 2);
                dbCfg.loginName = HTBaseFunc.DepartStr(curStr, ",", 3);
                dbCfg.passworld = HTBaseFunc.DepartStr(curStr, ",", 4);
                dbCfg.port = Convert.ToInt32(HTBaseFunc.DepartStr(curStr, ",", 5));
                dbCfg.childConnCount = Convert.ToInt32(HTBaseFunc.DepartStr(curStr, ",", 6));

                cfgData.dbCfgList.Add(dbCfg);

                ++index;
                curStr = HTBaseFunc.DepartStr(runParams[6], ";", index);
            }

            protalSvrAddr = HTBaseFunc.DepartStr(runParams[7], ",", 0);
            protalSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[7], ",", 1));

            return cfgData;
        }

        #endregion

        #region ChatServer
        
        public string ChatServerRunParamConver(string path, TChatServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }

        public TChatServerCfgData ChatServerRunParamConver(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;
            TChatServerCfgData cfgData = new TChatServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.lanIPAddr = runParams[3];
            cfgData.inPort = Convert.ToInt32(runParams[4]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[5], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[5], ",", 1);
            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[6], ",", 1));

            return cfgData;
        }

        #endregion

        #region WorldBossServer

        public string WorldBossServerRunParamConver(string path, TWorldBossServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.lanIPAddr).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }

        public TWorldBossServerCfgData WorldBossServerRunParamConver(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;
            TWorldBossServerCfgData cfgData = new TWorldBossServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.lanIPAddr = runParams[3];
            cfgData.inPort = Convert.ToInt32(runParams[4]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[5], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[5], ",", 1);
            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[6], ",", 1));

            return cfgData;
        }

        #endregion

        #region GateWayServer

        public string GateWayServerRunParamConvert(string path, TGateWayCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.serverID).Append(" ");
            paramArgs.Append(cfgData.index).Append(" ");
            paramArgs.Append(cfgData.wanIPAddr).Append(" ");
            paramArgs.Append(cfgData.outShortConnectionPort).Append(" ");
            paramArgs.Append(cfgData.outLongConnectionPort).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.centerServer.key, cfgData.centerServer.value)).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }

        public TGateWayCfgData GateWayServerRunParamConvert(string[] runParams, out string protalSvrAddr, out int protalSvrPort)
        {
            protalSvrAddr = "";
            protalSvrPort = 0;
            TGateWayCfgData cfgData = new TGateWayCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.serverID = Convert.ToInt32(runParams[2]);
            cfgData.index = Convert.ToInt32(runParams[3]);
            cfgData.wanIPAddr = runParams[4];
            cfgData.outShortConnectionPort = Convert.ToInt32(runParams[5]);
            cfgData.outLongConnectionPort = Convert.ToInt32(runParams[6]);
            cfgData.inPort = Convert.ToInt32(runParams[7]);
            cfgData.centerServer = new TStrKeyValue();
            cfgData.centerServer.key = HTBaseFunc.DepartStr(runParams[8], ",", 0);
            cfgData.centerServer.value = HTBaseFunc.DepartStr(runParams[8], ",", 1);
            protalSvrAddr = HTBaseFunc.DepartStr(runParams[9], ",", 0);
            protalSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[9], ",", 1));

            return cfgData;
        }

        #endregion

        #region XD Gateway

        public string GateWayServerRunParamConvert(string path, TXDGatewayCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.index).Append(" ");
            paramArgs.Append(cfgData.machineName).Append(" ");
            paramArgs.Append(cfgData.insideIPAddr).Append(" ");
            paramArgs.Append(cfgData.insidePort).Append(" ");
            paramArgs.Append(cfgData.outsidePort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }
        public TXDGatewayCfgData XDGatewayServerRunParamConver(string[] runParams, out string moniteorIP, out int moniteorPort)
        {
            TXDGatewayCfgData cfgData = new TXDGatewayCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.index = Convert.ToInt32(runParams[2]);
            cfgData.machineName = runParams[3];
            cfgData.insideIPAddr = runParams[4];
            cfgData.insidePort = Convert.ToInt32(runParams[5]);
            cfgData.outsidePort = Convert.ToInt32(runParams[6]);
            moniteorIP = HTBaseFunc.DepartStr(runParams[7], ",", 0);
            moniteorPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[7], ",", 1));
            return cfgData;
        }

        #endregion

        #region PortalServer

        public string PortalServerRunParamConvert(string path, TPortalServerCfgData cfgData)
        {
            StringBuilder paramArgs = new StringBuilder();

            paramArgs.Append(path).Append(" ");
            paramArgs.Append(cfgData.gameID).Append(" ");
            paramArgs.Append(cfgData.index).Append(" ");
            paramArgs.Append(cfgData.wanIPAddr).Append(" ");
            paramArgs.Append(cfgData.outPort).Append(" ");
            paramArgs.Append(cfgData.inPort).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", cfgData.platformDockingServer.Key, cfgData.platformDockingServer.Value)).Append(" ");
            paramArgs.Append(cfgData.pushNotice == "" ? "None" : cfgData.pushNotice).Append(" ");
            paramArgs.Append(string.Format("{0},{1}", "127.0.0.1", SvrCommCfg.Instance.ServerInfo.m_Port));

            return paramArgs.ToString();
        }

        public TPortalServerCfgData PortalServerRunParamConvert(string[] runParams, out string moniteorSvrAddr, out int moniteorSvrPort)
        {
            moniteorSvrAddr = "";
            moniteorSvrPort = 0;
            TPortalServerCfgData cfgData = new TPortalServerCfgData();

            cfgData.gameID = Convert.ToInt32(runParams[1]);
            cfgData.index = Convert.ToInt32(runParams[2]);
            cfgData.wanIPAddr = runParams[3];
            cfgData.outPort = Convert.ToInt32(runParams[4]);
            cfgData.inPort = Convert.ToInt32(runParams[5]);
            cfgData.platformDockingServer = new TIDStrKeyValue();
            cfgData.platformDockingServer.Key = HTBaseFunc.DepartStr(runParams[6], ",", 0);
            cfgData.platformDockingServer.Value = HTBaseFunc.DepartStr(runParams[6], ",", 1);
            cfgData.pushNotice = runParams[7];

            moniteorSvrAddr = HTBaseFunc.DepartStr(runParams[8], ",", 0);
            moniteorSvrPort = Convert.ToInt32(HTBaseFunc.DepartStr(runParams[8], ",", 1));

            return cfgData;
        }

        #endregion

        private static ServerRunParamConvert m_instance = new ServerRunParamConvert();
        private ServerRunParamConvert() { }
    }

    //服务器状态
    public enum eServerState
    {
        None = 0,

        ProcessRunning = 0x00000001,        //进程启动中
        ProcessCloseing = 0x00000002,       //进程关闭中
        ProcessKilling = 0x00000003,        //进程掐掉中

        ProcessRun = 0x00010000,            //逻辑运行
        ProcessClose = 0x00020000,          //逻辑关闭
    }

    //监控通用功能
    public class MoniteorCommonFunction
    {
        public static MoniteorCommonFunction Instance
        {
            get
            {
                return m_instance;
            }
        }

        /// <summary>
        /// 报告服务器状态到监控节点
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="state"></param>
        public void ReportServerState2MoniteorNode(string serverName, eServerState state)
        {
            //ByteArray sendData = com.tieao.mmo.interval.server.MoniteorServerDataProtocolServerHelper.ReportServerState(serverName, Convert.ToInt32(state));
            //Network.NetworkManager.Instance.SendMessageToServer(eServerType.MONITEORNODE, sendData);
        }

        /// <summary>
        /// 通知服务器启动
        /// </summary>
        /// <param name="serverName"></param>
        public void NotifyServerRun(string serverName)
        {
            //ByteArray sendData = com.tieao.mmo.interval.server.MoniteorServerDataProtocolServerHelper.NotifyServerRun(serverName, true);
            //Network.NetworkManager.Instance.SendMessageToServer(eServerType.MONITEORNODE, sendData);
        }

        /// <summary>
        /// 通知Portal启动
        /// </summary>
        /// <param name="serverName"></param>
        public void NotifyPortalRun(string serverName)
        {
            //ByteArray sendData = com.tieao.mmo.interval.server.MoniteorServerDataProtocolServerHelper.NotifyPortalRun(serverName, true);
            //Network.NetworkManager.Instance.SendMessageToServer(eServerType.MONITEORNODE, sendData);
        }

        private static MoniteorCommonFunction m_instance = new MoniteorCommonFunction();
        private MoniteorCommonFunction() { }
    }
}
