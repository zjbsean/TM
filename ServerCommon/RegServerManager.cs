using System;
using System.Collections;
using System.Collections.Generic;
using GsTechLib;
using ServerCommon.Network;
using com.tieao.mmo.interval;

namespace ServerCommon
{
    //服务器注册管理
    //public class RegServerManager
    //{

    //    #region Public Functions

    //    /// <summary>
    //    /// 注册本服服务信息
    //    /// </summary>
    //    /// <param name="svrInfo">服务信息</param>
    //    public void RegServerSelf(ref PtServerInfo svrInfo)
    //    {
    //        m_SelfSvrInfo = svrInfo;
    //    }

    //    /// <summary>
    //    /// 注册服务器
    //    /// </summary>
    //    /// <param name="svrinfo">服务器信息</param>
    //    /// <returns>返回错误ID</returns>
    //    public int RegServer(ref PtServerInfo svrinfo)
    //    {
    //        int errCode = 0;
    //        if (svrinfo.m_Type == eServerType.UNKNOW)
    //            errCode = 1;//服务器类型错误
    //        else
    //        {
    //            PtServerInfo checkSvrInfo;
    //            if (m_serverDic.TryGetValue(svrinfo.m_Name, out checkSvrInfo))
    //                Network.NetworkManager.Instance.CloseServerConnection(checkSvrInfo.m_ServerID);

    //            m_serverDic[svrinfo.m_Name] = svrinfo;
    //            m_serverSidDic[svrinfo.m_SessionID] = svrinfo;
    //            PtServerList psl;
    //            m_serverTypeDic.TryGetValue(svrinfo.m_Type, out psl);
    //            if (psl == null)
    //            {
    //                psl = new PtServerList();
    //                m_serverTypeDic.Add(svrinfo.m_Type, psl);
    //            }
    //            psl.Add(svrinfo);

    //            InternalProtocolDealDelegate.Instance.OnServerRegist(svrinfo.m_Type, svrinfo.m_SessionID);
    //        }
            
    //        return errCode;
    //    }

    //    /// <summary>
    //    /// 获取SessionID
    //    /// </summary>
    //    /// <param name="eType"></param>
    //    /// <param name="serverID"></param>
    //    /// <returns></returns>
    //    public int GetSessionID(eServerType eType, int serverID)
    //    {
    //        PtServerList psl;
    //        if (m_serverTypeDic.TryGetValue(eType, out psl))
    //        {
    //            for (int i = 0; i < psl.GetElements().Count; i++)
    //            {
    //                if (psl.GetElements()[i].m_ServerID == serverID)
    //                    return psl.GetElements()[i].m_SessionID;
    //            }
    //        }
    //        return -1;
    //    }

    //    public PtServerInfo GetServerInfo(eServerType eType, string serverName)
    //    {
    //        PtServerList psl;
    //        if (m_serverTypeDic.TryGetValue(eType, out psl))
    //        {
    //            for (int i = 0; i < psl.GetElements().Count; i++)
    //            {

    //                if (psl.GetElements()[i].m_Name == serverName)
    //                    return psl.GetElements()[i];
    //            }
    //        }
    //        return null;
    //    }


    //    /// <summary>
    //    /// 反注册服务器
    //    /// </summary>
    //    /// <param name="svrname">服务器名</param>
    //    /// <param name="svrtype">服务器类型</param>
    //    public void UnregServer(string svrName)
    //    {
    //        PtServerInfo psi;
    //        if (m_serverDic.TryGetValue(svrName, out psi))
    //        {
    //            m_serverDic.Remove(svrName);
    //            m_serverSidDic.Remove(psi.m_SessionID);
    //            PtServerList psl;
    //            if (m_serverTypeDic.TryGetValue(psi.m_Type, out psl))
    //            {
    //                foreach (var ps in psl.GetElements())
    //                {
    //                    if (ps.m_Name == svrName)
    //                    {
    //                        psl.GetElements().Remove(ps);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }

        /// <summary>
        /// 更新服务器
        /// </summary>
        /// <param name="svrinfo">服务器信息</param>
        /// <param name="allowRegNew">如果不存在，是否允许注册新的</param>
        /// <returns>返回错误ID</returns>
        //public int UpdateServer(ref PtServerInfo svrinfo)
        //{
            //int errCode = 0;

    //        PtServerInfo psi;
    //        if (m_serverDic.TryGetValue(svrinfo.m_Name, out psi))
    //        {
    //            m_serverSidDic.Remove(psi.m_SessionID);
    //        }
    //        m_serverDic[svrinfo.m_Name] = svrinfo;
    //        m_serverSidDic[svrinfo.m_SessionID] = svrinfo;
            
            
    //        PtServerList psl;
    //        if (m_serverTypeDic.TryGetValue(svrinfo.m_Type, out psl) == false)
    //        {
    //            psl = new PtServerList();
    //            m_serverTypeDic.Add(svrinfo.m_Type, psl);
    //        }
    //        int index = 0;
    //        for (; index < psl.GetElements().Count; ++index)
    //        {
    //            if (psl.GetElements()[index].m_Name == svrinfo.m_Name)
    //            {
    //                psl.GetElements()[index] = svrinfo;
    //                break;
    //            }
    //        }
    //        if (index == psl.GetElements().Count)
    //        {
    //            psl.Add(svrinfo);
    //        }

    //        return errCode;
    //    }

    //    /// <summary>
    //    /// 是否存在服务器
    //    /// </summary>
    //    /// <param name="svrname">服务器名</param>
    //    /// <param name="svrtype">服务器类型</param>
    //    /// <returns>存在返回true，否则返回false</returns>
    //    public bool ExistServerInfo(string svrName)
    //    {
    //        return m_serverDic.ContainsKey(svrName);
    //    }

    //    /// <summary>
    //    /// 是否存在服务器
    //    /// </summary>
    //    /// <param name="sessionID">服务器连接ID</param>
    //    /// <returns>存在返回true，否则返回false</returns>
    //    public bool ExistServerInfo(int sessionID)
    //    {
    //        return m_serverSidDic.ContainsKey(sessionID);
    //    }

    //    /// <summary>
    //    /// 获取名称相符服务器信息
    //    /// </summary>
    //    /// <param name="svrname">服务器名</param>
    //    /// <param name="svrtype">服务器类型</param>
    //    /// <param name="svrinfo">输出服务器信息</param>
    //    /// <returns>返回是否存在</returns>
    //    public bool GetServerInfo(string svrName, out PtServerInfo svrInfo)
    //    {
    //        return m_serverDic.TryGetValue(svrName, out svrInfo);
    //    }

    //    /// <summary>
    //    /// 获取类型相符的第一个服务器信息
    //    /// </summary>
    //    /// <param name="svrtype">服务器类型</param>
    //    /// <param name="svrinfo">输出服务器信息</param>
    //    /// <returns>返回是否存在</returns>
    //    public bool GetServerInfo(eServerType svrType, out PtServerInfo svrInfo)
    //    {
    //        PtServerList psl;
    //        if (m_serverTypeDic.TryGetValue(svrType, out psl))
    //        {
    //            if (psl.GetElements().Count > 0)
    //            {
    //                svrInfo = psl.GetElements()[0];
    //                return true;
    //            }
    //        }
    //        svrInfo = new PtServerInfo();
    //        return false;
    //    }

    //    /// <summary>
    //    /// 获取服务器信息
    //    /// </summary>
    //    /// <param name="sessionID">服务器连接ID</param>
    //    /// <param name="svrinfo">输出服务器信息</param>
    //    /// <returns>返回是否存在</returns>
    //    public bool GetServerInfo(int sessionID, out PtServerInfo svrinfo)
    //    {
    //        return m_serverSidDic.TryGetValue(sessionID, out svrinfo);
    //    }

    //    /// <summary>
    //    /// 获取指定类型的服务器类别
    //    /// </summary>
    //    /// <param name="svrtype">服务器类型</param>
    //    /// <returns>返回列表</returns>
    //    public PtServerList GetServerList(eServerType svrType)
    //    {
    //        PtServerList svrlist;
    //        m_serverTypeDic.TryGetValue(svrType, out svrlist);
    //        if (svrlist == null)
    //            svrlist = new PtServerList();
    //        return svrlist;
    //    }

    //    /// <summary>
    //    /// 获取所有服务器列表
    //    /// </summary>
    //    /// <returns>返回列表</returns>
    //    public PtServerList GetAllServerList()
    //    {
    //        PtServerList psl = new PtServerList();
    //        psl.GetElements().AddRange(m_serverDic.Values);
    //        return psl;
    //    }

    //    /// <summary>
    //    /// 获取除指定服务外的服务列表
    //    /// </summary>
    //    /// <param name="svrInfo">服务信息</param>
    //    /// <returns>服务列表</returns>
    //    public PtServerList GetServerListWithout(ref PtServerInfo svrInfo)
    //    {
    //        PtServerList ptSvrList = new PtServerList();
    //        foreach (var psi in m_serverDic.Values)
    //        {
    //            if (psi.m_Name == svrInfo.m_Name)
    //                continue;
    //            ptSvrList.Add(psi);
    //        }
    //        return ptSvrList;
    //    }

    //    /// <summary>
    //    /// 获取有效的服务器SessionID数组
    //    /// </summary>
    //    /// <returns>返回数组</returns>
    //    public ArrayList GetAllServerSidList()
    //    {
    //        ArrayList sidList = new ArrayList();
    //        sidList.AddRange(m_serverSidDic.Keys);
    //        return sidList;
    //    }

    //    /// <summary>
    //    /// 获取类型相符的第一个服务器的连接ID
    //    /// </summary>
    //    /// <param name="svrtype">服务器类型</param>
    //    /// <returns>返回服务器连接ID</returns>
    //    public int GetServerSid(eServerType svrType)
    //    {
    //        PtServerList psl;
    //        if(m_serverTypeDic.TryGetValue(svrType, out psl))
    //        {
    //            if (psl.GetElements().Count > 0)
    //                return psl.GetElements()[0].m_SessionID;
    //        }
    //        return -1;
    //    }

    //    /// <summary>
    //    /// 获取名称和类型相符的第一个服务器的连接ID
    //    /// </summary>
    //    /// <param name="svrname">服务器名</param>
    //    /// <param name="svrtype">服务器类型</param>
    //    /// <returns>返回服务器连接ID</returns>
    //    public int GetServerSid(string svrName)
    //    {
    //        PtServerInfo psi;
    //        if (m_serverDic.TryGetValue(svrName, out psi))
    //            return psi.m_SessionID;
    //        return -1;
    //    }

    //    #endregion


    //    #region Private Functions

    //    //私有构造
    //    private RegServerManager() { }

    //    #endregion

    //    #region Data Member

    //    public static RegServerManager Instance
    //    {
    //        get
    //        {
    //            return _instance;
    //        }
    //    }

    //    private static RegServerManager _instance = new RegServerManager();

    //    //服务器注册列表
    //    //private PtServerList m_serverList = new PtServerList();
    //    private Dictionary<string, PtServerInfo> m_serverDic = new Dictionary<string, PtServerInfo>();
    //    private Dictionary<eServerType, PtServerList> m_serverTypeDic = new Dictionary<eServerType, PtServerList>();
    //    private Dictionary<int, PtServerInfo> m_serverSidDic = new Dictionary<int, PtServerInfo>();

    //    public PtServerInfo m_SelfSvrInfo;

    //    #endregion
    //}
}
