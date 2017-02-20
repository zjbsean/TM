using System;
using System.Collections.Generic;
using ServerCommon.Network;
using GsTechLib;
using com.tieao.mmo.interval;

namespace ServerCommon
{
    //服务器分配信息
    //[Serializable]
    //public class UserSvrInfo
    //{
    //    public string m_LoginName;
    //    public long m_PlayerFlagID;
    //    public bool m_IsUserID;             //是否是玩家ID
    //    public byte m_IsGM = 0;             //是否GM
    //    public byte m_VipLevel = 0;         //VIP等级
        
    //    //public Dictionary<eServerType, string> m_AllocSvrList = new Dictionary<eServerType, string>();    //为该玩家分配的服务器列表
    //    public string m_GameServerName;
    //    public string m_GateWayName;

    //    //获取分配的某类型服务器名
    //    public string GetAllocSvrName(eServerType serverType)
    //    {
    //        switch (serverType)
    //        {
    //            case eServerType.GATEWAY:
    //                return m_GateWayName;
    //            case eServerType.GAME:
    //                return m_GameServerName;
    //        }
    //        return "";
    //    }

    //    //获取分配的某类型服务器的Sid
    //    public int GetAllocSvrSid(eServerType servertype)
    //    {
    //        string svrname = GetAllocSvrName(servertype);
    //        return RegServerManager.Instance.GetServerSid(svrname);
    //    }
    //}

    //玩家分配管理
    //public class AllocSvrMap
    //{
    //    #region Public Functions

    //    /// <summary>
    //    /// 添加一个UserSvrInfo
    //    /// </summary>
    //    /// <param name="info">用户所在服务器信息</param>
    //    /// <returns>返回是否成功</returns>
    //    public void AddPlayerSvrInfo(UserSvrInfo info)
    //    {
    //        if (info.m_PlayerFlagID != 0)
    //        {
    //            RemovePlayerSvrInfo(info.m_PlayerFlagID);

    //            m_playerIDMap[info.m_PlayerFlagID] = info;

    //            string svrName = info.m_GameServerName;
    //            addPlayerSvrInfo(eServerType.GAME, svrName, info);

    //            svrName = info.m_GateWayName;
    //            addPlayerSvrInfo(eServerType.GATEWAY, svrName, info);
    //        }

    //        m_loginnameMap[info.m_LoginName] = info;
    //    }

    //    /// <summary>
    //    /// 更新玩家ID
    //    /// </summary>
    //    /// <param name="loginName"></param>
    //    /// <param name="playerID"></param>
    //    public void UpdatePlayerID(string loginName,long playerID)
    //    {
    //        UserSvrInfo info;
    //        if(m_loginnameMap.TryGetValue(loginName,out info))
    //        {
    //            if (info.m_PlayerFlagID != 0)
    //            {
    //                RemovePlayerSvrInfo(info.m_PlayerFlagID);
    //            }
    //            info.m_PlayerFlagID = playerID;
    //            if(playerID != 0)
    //            {
    //                m_playerIDMap[playerID] = info;

    //                string svrName = info.m_GameServerName;
    //                addPlayerSvrInfo(eServerType.GAME, svrName, info);

    //                svrName = info.m_GateWayName;
    //                addPlayerSvrInfo(eServerType.GATEWAY, svrName, info);
    //            }
    //        }
    //    }

    //    private void addPlayerSvrInfo(eServerType svrType, string svrName, UserSvrInfo info)
    //    {
    //        Dictionary<string, Dictionary<long, UserSvrInfo>> nameGuidSvrInfoDic;
    //        if (m_allocSvrMap.TryGetValue(svrType, out nameGuidSvrInfoDic))
    //        {
    //            Dictionary<long, UserSvrInfo> guidSvrInfoDic;
    //            if (nameGuidSvrInfoDic.TryGetValue(svrName, out guidSvrInfoDic))
    //                guidSvrInfoDic[info.m_PlayerFlagID] = info;
    //            else
    //            {
    //                guidSvrInfoDic = new Dictionary<long, UserSvrInfo>();
    //                guidSvrInfoDic.Add(info.m_PlayerFlagID, info);
    //                nameGuidSvrInfoDic.Add(svrName, guidSvrInfoDic);
    //            }
    //        }
    //        else
    //        {
    //            Dictionary<long, UserSvrInfo> guidSvrInfoDic = new Dictionary<long, UserSvrInfo>();
    //            guidSvrInfoDic.Add(info.m_PlayerFlagID, info);

    //            nameGuidSvrInfoDic = new Dictionary<string, Dictionary<long, UserSvrInfo>>();
    //            nameGuidSvrInfoDic.Add(svrName, guidSvrInfoDic);

    //            m_allocSvrMap.Add(svrType, nameGuidSvrInfoDic);
    //        }
    //    }

    //    /// <summary>
    //    /// 添加一群玩家、角色服务分配信息
    //    /// </summary>
    //    /// <param name="infoList"></param>
    //    public void AddPlayerListSvrInfo(List<UserSvrInfo> infoList)
    //    {
    //        foreach (UserSvrInfo userSvr in infoList)
    //        {
    //            AddPlayerSvrInfo(userSvr);
    //        }
    //    }

    //    /// <summary>
    //    /// 删除一个UserSvrInfo
    //    /// </summary>
    //    /// <param name="playerID">玩家ID</param>
    //    public void RemovePlayerSvrInfo(long playerFlagID)
    //    {
    //        //Dictionary<eServerType, Dictionary<string, Dictionary<Guid, UserSvrInfo>>> 
    //        UserSvrInfo userSvrInfo;
    //        if (m_playerIDMap.TryGetValue(playerFlagID, out userSvrInfo))
    //        {
    //            userSvrInfo.m_PlayerFlagID = 0;
    //            m_playerIDMap.Remove(playerFlagID);

    //            removePlayerSvrInfo(eServerType.GAME, userSvrInfo.m_GameServerName, playerFlagID);
    //            removePlayerSvrInfo(eServerType.GATEWAY, userSvrInfo.m_GateWayName, playerFlagID);
    //        }
    //    }
    //    private void removePlayerSvrInfo(eServerType svrType, string svrName, long playerID)
    //    {
    //        Dictionary<string, Dictionary<long, UserSvrInfo>> nameGuidSvrInfoDic;
    //        if (m_allocSvrMap.TryGetValue(svrType, out nameGuidSvrInfoDic))
    //        {
    //            Dictionary<long, UserSvrInfo> guidSvrInfoDic;
    //            if (nameGuidSvrInfoDic.TryGetValue(svrName, out guidSvrInfoDic))
    //            {
    //                if (guidSvrInfoDic.ContainsKey(playerID))
    //                    guidSvrInfoDic.Remove(playerID);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 获取一个UserSvrInfo
    //    /// </summary>
    //    /// <param name="userid">玩家ID</param>
    //    /// <returns>返回UserSvrInfo</returns>
    //    public UserSvrInfo GetPlayerSvrInfo(long playerFlagID)
    //    {
    //        UserSvrInfo svrInfo;
    //        m_playerIDMap.TryGetValue(playerFlagID, out svrInfo);
    //        return svrInfo;
    //    }

    //    public UserSvrInfo GetPlayerSvrInfo(string loginName)
    //    {
    //        UserSvrInfo svrInfo;
    //        m_loginnameMap.TryGetValue(loginName, out svrInfo);
    //        return svrInfo;
    //    }

    //    /// <summary>
    //    /// 删除角色信息
    //    /// </summary>
    //    /// <param name="charaID"></param>
    //    public void DeleteCharaSvrInfo(int playerFlagID)
    //    {
    //        m_playerIDMap.Remove(playerFlagID);
    //    }

    //    /// <summary>
    //    /// 更新玩家分配的服务器信息
    //    /// </summary>
    //    /// <param name="userid">玩家ID</param>
    //    /// <param name="servertype">服务器类型</param>
    //    /// <param name="servername">服务器名</param>
    //    /// <returns>成功返回true，否则返回false</returns>
    //    public bool UpdatePlayerAllocedSvr(long playerFlagID, eServerType serverType, string serverName)
    //    {
    //        UserSvrInfo oldSvrInfo;
    //        if (m_playerIDMap.TryGetValue(playerFlagID, out oldSvrInfo))
    //        {
    //            string oldSvrName = "";

    //            if (eServerType.GATEWAY == serverType)
    //            {
    //                oldSvrName = oldSvrInfo.m_GateWayName;
    //                oldSvrInfo.m_GateWayName = serverName;
    //            }
    //            else if (eServerType.GAME == serverType)
    //            {
    //                oldSvrName = oldSvrInfo.m_GameServerName;
    //                oldSvrInfo.m_GameServerName = serverName;
    //            }

    //            //Dictionary<eServerType, Dictionary<string, Dictionary<Guid, UserSvrInfo>>> 
    //            Dictionary<string, Dictionary<long, UserSvrInfo>> nameGuidSvrDic;
    //            if (m_allocSvrMap.TryGetValue(serverType, out nameGuidSvrDic))
    //            {
    //                Dictionary<long, UserSvrInfo> guidSvrDic;
    //                if (oldSvrName != "")
    //                {
    //                    if (nameGuidSvrDic.TryGetValue(oldSvrName, out guidSvrDic))
    //                        guidSvrDic.Remove(playerFlagID);
    //                }
                    
    //                if (nameGuidSvrDic.TryGetValue(serverName, out guidSvrDic))
    //                    guidSvrDic[playerFlagID] = oldSvrInfo;
    //            }
    //            else
    //            {
    //                Dictionary<long, UserSvrInfo> guidSvrDic = new Dictionary<long, UserSvrInfo>();
    //                guidSvrDic.Add(playerFlagID, oldSvrInfo);
    //                nameGuidSvrDic = new Dictionary<string, Dictionary<long, UserSvrInfo>>();
    //                nameGuidSvrDic.Add(serverName, guidSvrDic);
    //                m_allocSvrMap.Add(serverType, nameGuidSvrDic);
    //            }
                
    //            return true;
    //        }
    //        else
    //            return false;
    //    }

    //    /// <summary>
    //    /// 更新玩家列表服务器分配信息
    //    /// </summary>
    //    /// <param name="playerIDList"></param>
    //    /// <param name="serverType"></param>
    //    /// <param name="serverName"></param>
    //    public void UpdatePlayerListAllocSvr(List<long> playerIDList, eServerType serverType, string serverName)
    //    {
    //        foreach (long playerID in playerIDList)
    //        {
    //            if (UpdatePlayerAllocedSvr(playerID, serverType, serverName) == false)
    //                SvLogger.Error("The PlayerID Update Server Alloc Fail : PlayerID={0}, ServerType={1}, ServerName={2}.", playerID, serverType, serverName);
    //        }
    //    }

    //    /// <summary>
    //    /// 获取某个服务器上分配的玩家UserID列表
    //    /// </summary>
    //    /// <param name="servertype">服务器类型</param>
    //    /// <param name="servername">服务器名</param>
    //    /// <returns>返回玩家UserID列表</returns>
    //    public List<long> GetSvrAllocPlayerIDList(eServerType serverType, string serverName)
    //    {
    //        List<long> guidList = new List<long>();
    //        //Dictionary<eServerType, Dictionary<string, Dictionary<Guid, UserSvrInfo>>> 
    //        Dictionary<string, Dictionary<long, UserSvrInfo>> nameGuidSvrDic;
    //        if (m_allocSvrMap.TryGetValue(serverType, out nameGuidSvrDic))
    //        {
    //            Dictionary<long, UserSvrInfo> guidSvrDic;
    //            if (nameGuidSvrDic.TryGetValue(serverName, out guidSvrDic))
    //                guidList.AddRange(guidSvrDic.Keys);
    //        }
    //        return guidList;
    //    }

    //    /// <summary>
    //    /// 获取某个服务器上分配的玩家UserSvrInfo列表
    //    /// </summary>
    //    /// <param name="servertype">服务器类型</param>
    //    /// <param name="servername">服务器名</param>
    //    /// <returns>返回玩家UserSvrInfo列表</returns>
    //    public List<UserSvrInfo> GetSvrAllocPlayerList(eServerType serverType, string serverName)
    //    {
    //        List<UserSvrInfo> userSvrList = new List<UserSvrInfo>();
    //        //Dictionary<eServerType, Dictionary<string, Dictionary<Guid, UserSvrInfo>>> 
    //        Dictionary<string, Dictionary<long, UserSvrInfo>> nameGuidSvrDic;
    //        if (m_allocSvrMap.TryGetValue(serverType, out nameGuidSvrDic))
    //        {
    //            Dictionary<long, UserSvrInfo> guidSvrDic;
    //            if (nameGuidSvrDic.TryGetValue(serverName, out guidSvrDic))
    //                userSvrList.AddRange(guidSvrDic.Values);
    //        }
    //        return userSvrList;
    //    }

    //    /// <summary>
    //    /// 获取压力最小的某类型服务器名
    //    /// </summary>
    //    /// <param name="servertype">服务器类型</param>
    //    /// <returns>返回服务器名</returns>
    //    public string GetMinPressureSvrName(eServerType serverType)
    //    {
    //        string minCountSvrName = "";
    //        //Dictionary<eServerType, Dictionary<string, Dictionary<Guid, UserSvrInfo>>> 
    //        Dictionary<string, Dictionary<long, UserSvrInfo>> nameGuidSvrDic;
    //        Dictionary<long, UserSvrInfo> guidSvrDic;
    //        PtServerList gsSvrList = RegServerManager.Instance.GetServerList(eServerType.GAME);
    //        int userCount = int.MaxValue;
    //        foreach (PtServerInfo gsSvr in gsSvrList.GetElements())
    //        {
    //            if (gsSvr.m_SessionID != -1)
    //            {
    //                if (m_allocSvrMap.TryGetValue(serverType, out nameGuidSvrDic))
    //                {
    //                    if (nameGuidSvrDic.TryGetValue(gsSvr.m_Name, out guidSvrDic))
    //                    {
    //                        int tempCount = guidSvrDic.Count;
    //                        if (userCount > tempCount)
    //                        {
    //                            minCountSvrName = gsSvr.m_Name;
    //                            userCount = tempCount;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        minCountSvrName = gsSvr.m_Name;
    //                        break;
    //                    }
    //                }
    //                else
    //                {
    //                    minCountSvrName = gsSvr.m_Name;
    //                    break;
    //                }
    //            }
    //        }
            
    //        return minCountSvrName;
    //    }

    //    //检查玩家是否分配在指定服务器
    //    public bool IsAllocOnTheServer(int playerFlagID, eServerType serverType, string serverName)
    //    {
    //        UserSvrInfo svrInfo;
    //        if (m_playerIDMap.TryGetValue(playerFlagID, out svrInfo))
    //        {
    //            if(serverType == eServerType.GAME)
    //            {
    //                if(svrInfo.m_GameServerName == serverName)
    //                    return true;
    //            }
    //            else if (serverType == eServerType.GATEWAY)
    //            {
    //                if (svrInfo.m_GateWayName == serverName)
    //                    return true;
    //            }
    //        }
    //        return false;
    //    }

    //    /// <summary>
    //    /// 获取玩家目标服务器连接SessionID
    //    /// </summary>
    //    /// <param name="userID">玩家ID</param>
    //    /// <param name="serverType">服务类型</param>
    //    /// <returns>链接SessionID</returns>
    //    public int GetPlayerSvrSid(long playerFlagID, eServerType serverType)
    //    {
    //        int sessionID = -1;

    //        UserSvrInfo svrInfo;
    //        if (m_playerIDMap.TryGetValue(playerFlagID, out svrInfo))
    //        {
    //            if (serverType == eServerType.GATEWAY)
    //                sessionID = RegServerManager.Instance.GetServerSid(svrInfo.m_GateWayName);
    //            else if (serverType == eServerType.GAME)
    //                sessionID = RegServerManager.Instance.GetServerSid(svrInfo.m_GameServerName);
    //        }

    //        return sessionID;
    //    }

    //    public int GetPlayerSvrSid(string loginName,eServerType serverType)
    //    {
    //        int sessionID = -1;

    //        UserSvrInfo svrInfo;
    //        if (m_loginnameMap.TryGetValue(loginName, out svrInfo))
    //        {
    //            if (serverType == eServerType.GATEWAY)
    //                sessionID = RegServerManager.Instance.GetServerSid(svrInfo.m_GateWayName);
    //            else if (serverType == eServerType.GAME)
    //                sessionID = RegServerManager.Instance.GetServerSid(svrInfo.m_GameServerName);
    //        }

    //        return sessionID;
    //    }


    //    /// <summary>
    //    /// 获取玩家目标服务器名字
    //    /// </summary>
    //    /// <param name="userID">玩家GUID</param>
    //    /// <param name="serverType">目标服务器类型</param>
    //    /// <returns>名字</returns>
    //    public string GetPlayerSvrName(int playerFlagID, eServerType serverType)
    //    {
    //        UserSvrInfo svrInfo;
    //        playerFlagID = 0;
    //        if (m_playerIDMap.TryGetValue(playerFlagID, out svrInfo))
    //        {
    //            if (serverType == eServerType.GAME)
    //                return svrInfo.m_GameServerName;
    //            else if (serverType == eServerType.GATEWAY)
    //                return svrInfo.m_GateWayName;
    //        }

    //        return "";
    //    }


    //    #endregion

    //    #region Private Functions

    //    /*私有化构造*/
    //    private AllocSvrMap() { }

    //    #endregion

    //    #region Property

    //    //单例属性
    //    public static AllocSvrMap Instance
    //    {
    //        get
    //        {
    //            return m_instance;
    //        }
    //    }

    //    #endregion

    //    #region Data Member

    //    //单例对象
    //    private static AllocSvrMap m_instance = new AllocSvrMap();

    //    //玩家服务分配字典 key=登录名
    //    Dictionary<string, UserSvrInfo> m_loginnameMap = new Dictionary<string, UserSvrInfo>();

    //    //玩家服务分配字典
    //    private Dictionary<long, UserSvrInfo> m_playerIDMap = new Dictionary<long, UserSvrInfo>();
    //    //服务中分配玩家字典 key2=serverName key3=playerID
    //    private Dictionary<eServerType, Dictionary<string, Dictionary<long, UserSvrInfo>>> m_allocSvrMap = new Dictionary<eServerType, Dictionary<string, Dictionary<long, UserSvrInfo>>>();

    //    #endregion
    //}
}
