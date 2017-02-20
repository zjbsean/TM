using System;
using System.Collections.Generic;
using System.Text;
using com.tieao.mmo.interval;

namespace ServerCommon
{
    public delegate void ServerInfoDelegate(PtServerInfo regSvrInfo, string param);

    public delegate void CommandDealDelegate(string command);

    public delegate void OneIntParamDelegate(int param);

    public delegate void ServerRegistDelegate(eServerType svrType, int sessionID);

    //内部协议处理代理
    public class InternalProtocolDealDelegate
    {
        public static InternalProtocolDealDelegate Instance
        {
            get
            {
                return m_instance;
            }
        }

        public void OnRegistServer(PtServerInfo svrInfo, string param)
        {
            if (m_onRegistServer != null)
            {
                m_onRegistServer(svrInfo, param);
            }
        }

        public void OnRegistServerSucc(PtServerInfo svrInfo, string param)
        {
            if(m_onRegistServerSucc != null)
            {
                m_onRegistServerSucc(svrInfo, param);
            }
        }

        public void OnRegistServerFail(PtServerInfo svrInfo, string param)
        {
            if (m_onRegistServerFail != null)
            {
                m_onRegistServerFail(svrInfo, param);
            }
        }

        public void OnReregistServer(PtServerInfo svrInfo, string param)
        {
            if (m_onReregistServer != null)
            {
                m_onReregistServer(svrInfo, param);
            }
        }

        public void OnReregistServerSucc(PtServerInfo svrInfo, string param)
        {
            if (m_onReregistServerSucc != null)
            {
                m_onReregistServerSucc(svrInfo, param);
            }
        }

        public void OnReregistServerFail(PtServerInfo svrInfo, string param)
        {
            if (m_onReregistServerFail != null) 
                m_onReregistServerFail(svrInfo, param);
        }

        public void OnConnectServer(PtServerInfo svrInfo, string param)
        {
            if (m_onConnectServer != null)
                m_onConnectServer(svrInfo, param);
        }

        public void OnConnectServerSucc(PtServerInfo svrInfo, string param)
        {
            if (m_onConnectServerSucc != null)
                m_onConnectServerSucc(svrInfo, param);
        }

        public void OnConnectServerFail(PtServerInfo svrInfo, string param)
        {
            if (m_onConnectServerFail != null)
                m_onConnectServerFail(svrInfo, param);
        }

        public void OnHadCommand(string command)
        {
            if (m_commandDeal != null)
                m_commandDeal(command);
        }

        public void OnGetServerID(int serverID)
        {
            if (m_getServerIDDeal != null)
                m_getServerIDDeal(serverID);
        }

        public void OnServerRegist(eServerType svrType, int sessionID)
        {
            if (m_svrRegistDeal != null)
                m_svrRegistDeal(svrType, sessionID);
        }

        public void SetOnRegistServerDeal(ServerInfoDelegate dealFunc)
        {
            m_onRegistServer = dealFunc;
        }

        public void SetOnRegistServerSucc(ServerInfoDelegate dealFunc)
        {
            m_onRegistServerSucc = dealFunc;
        }

        public void SetOnRegistServerFail(ServerInfoDelegate dealFunc)
        {
            m_onRegistServerFail = dealFunc;
        }

        public void SetOnConnectServer(ServerInfoDelegate dealFunc)
        {
            m_onConnectServer = dealFunc;
        }

        public void SetOnConnectServerSucc(ServerInfoDelegate dealFunc)
        {
            m_onConnectServerSucc = dealFunc;
        }

        public void SetOnConnectServerFail(ServerInfoDelegate dealFunc)
        {
            m_onConnectServerFail = dealFunc;
        }

        public void SetOnReregistServer(ServerInfoDelegate dealFunc)
        {
            m_onReregistServer = dealFunc; 
        }

        public void SetOnReregistServerSucc(ServerInfoDelegate dealFunc)
        {
            m_onReregistServerSucc = dealFunc;
        }

        public void SetOnReregistServerFail(ServerInfoDelegate dealFunc)
        {
            m_onReregistServerFail = dealFunc;
        }

        public void SetCommandDeal(CommandDealDelegate dealFunc)
        {
            m_commandDeal = dealFunc;
        }

        public void SetGetServerIDDeal(OneIntParamDelegate dealFunc)
        {
            m_getServerIDDeal = dealFunc;
        }

        public void SetSvrRegistDeal(ServerRegistDelegate dealFunc)
        {
            m_svrRegistDeal = dealFunc;
        }

        private ServerInfoDelegate m_onRegistServer = null;
        private ServerInfoDelegate m_onRegistServerSucc = null;
        private ServerInfoDelegate m_onRegistServerFail = null;

        private ServerInfoDelegate m_onReregistServer = null;
        private ServerInfoDelegate m_onReregistServerSucc = null;
        private ServerInfoDelegate m_onReregistServerFail = null;

        private ServerInfoDelegate m_onConnectServer = null;
        private ServerInfoDelegate m_onConnectServerSucc = null;
        private ServerInfoDelegate m_onConnectServerFail = null;

        private CommandDealDelegate m_commandDeal = null;
        private OneIntParamDelegate m_getServerIDDeal = null;

        private ServerRegistDelegate m_svrRegistDeal = null;

        private static InternalProtocolDealDelegate m_instance = new InternalProtocolDealDelegate();
        private InternalProtocolDealDelegate() { }
    }
}
