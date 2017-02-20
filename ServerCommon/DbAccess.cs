using System;
using System.Collections.Generic;
using System.Linq;
using GsTechLib;

namespace ServerCommon
{
    //数据库访问
    public class DbAccess
    {
        #region Variables

        private static DbAccess _instance = new DbAccess();
        public static DbAccess Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// 初始化数据库连接
        /// </summary>
        /// <param name="dbConnFlag">连接名</param>
        /// <param name="serverType">数据库类型</param>
        /// <param name="serverIp">数据库服务器名</param>
        /// <param name="serverPort">数据库端口</param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="userName">数据库用户名</param>
        /// <param name="userPass">数据库用户密码</param>
        /// <param name="childConnCount">子连接个数</param>
        /// <returns>返回错误信息</returns>
        public int InitilizeDbConn(eDbConnFlag dbConnFlag, string serverType, string serverIp, string serverPort, string databaseName, string userName, string userPass, int childConnCount)
        {
            int errCode = 0;
            //最终Connect Flag生成
            m_connFlagMakeLines[dbConnFlag] += 1;
            int lineIndex = Convert.ToInt32(m_connFlagMakeLines[dbConnFlag]);
            int connFlag = Convert.ToInt32(dbConnFlag) | lineIndex;

            DbAccessPool dapool = DbAccessPool.GetPool(connFlag);

            if (dapool != null)
            {
                errCode = 1111;
                SvLogger.Error("Had Same DB Connections : ConnectType={0}, ConnectFlag={1}.", dbConnFlag, connFlag);
                return errCode;
            }

            //获取SQL日志记录池
            DbAccessPool sqlogdapool = null;
            //if (dbConnFlag != eDbConnFlag.SqlLog)
            //    sqlogdapool = DbAccessPool.GetPool(EnumDbConnName.SqlLog.ToString());

            //建立数据库连接
            DbAccessPoolVistor vistor = new DbAccessPoolVistor(connFlag);
            dapool = new DbAccessPool(connFlag, SvLogger.GetLogger(), vistor, sqlogdapool);
            try
            {
                dapool.Start(serverType, serverIp, serverPort, databaseName, userName, userPass);
            }
            catch (Exception ex)
            {
                SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
                errCode = 1111;
                return errCode;
            }
            int mainFlag = dapool.Flag;
            if (dbConnFlag == eDbConnFlag.Game)
                m_gameDBMainConnFlagList.Add(dapool.Flag);
            else if (dbConnFlag == eDbConnFlag.Log)
                m_logDbAccPool = dapool;
            else if (dbConnFlag == eDbConnFlag.Secd)
                m_secdDbAccPool = dapool;
            else if (dbConnFlag == eDbConnFlag.GM)
                m_gmDBAccPool = dapool;
            else if (dbConnFlag == eDbConnFlag.Platform)
                m_platformDBAccPool = dapool;
            else if (dbConnFlag == eDbConnFlag.Moniteor)
                m_moniteorDBAccPool = dapool;
            else if (dbConnFlag == eDbConnFlag.Entry)
                m_entryDBAccPool = dapool;
            else if (dbConnFlag == eDbConnFlag.GiftCode)
                m_giftCodeDBAccPool = dapool;

            //加入池
            DbAccessPool.AddPool(dapool);

            //Game数据库子连接建立
            if (dbConnFlag == eDbConnFlag.Game)
            {
                for (int i = 0; i < childConnCount; ++i)
                {
                    //最终Connect Flag生成
                    m_connFlagMakeLines[dbConnFlag] += 1;
                    lineIndex = Convert.ToInt32(m_connFlagMakeLines[dbConnFlag]);
                    connFlag = Convert.ToInt32(dbConnFlag) | lineIndex;

                    dapool = DbAccessPool.GetPool(connFlag);
                    if (dapool != null)
                    {
                        errCode = 1111;
                        SvLogger.Error("Had Same DB Connections : ConnectType={0}, ConnectFlag={1}.", dbConnFlag, connFlag);
                        return errCode;
                    }

                    //获取SQL日志记录池
                    sqlogdapool = null;
                    //if (dbConnFlag != eDbConnFlag.SqlLog)
                    //    sqlogdapool = DbAccessPool.GetPool(EnumDbConnName.SqlLog.ToString());

                    //建立数据库连接
                    vistor = new DbAccessPoolVistor(connFlag);
                    dapool = new DbAccessPool(connFlag, SvLogger.GetLogger(), vistor, sqlogdapool);
                    try
                    {
                        dapool.Start(serverType, serverIp, serverPort, databaseName, userName, userPass);
                    }
                    catch (Exception ex)
                    {
                        SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
                        errCode = 1111;
                        return errCode;
                    }
                    List<int> childList;
                    if (m_gameDBChildConnFlagDic.TryGetValue(mainFlag, out childList) == false)
                    {
                        childList = new List<int>();
                        m_gameDBChildConnFlagDic.Add(mainFlag, childList);
                    }
                    childList.Add(dapool.Flag);

                    //加入池
                    DbAccessPool.AddPool(dapool);
                }
            }

            return errCode;
        }

        /// <summary>
        /// 销毁数据库连接
        /// </summary>
        /// <param name="connFlag">连接标记</param>
        public void DestroyDbConn(int connFlag)
        {
            DbAccessPool dapool = DbAccessPool.GetPool(connFlag);
            if (dapool != null)
            {
                dapool.Stop(true);
                DbAccessPool.RemovePool(connFlag);
            }
        }

        /// <summary>
        /// 销毁所有数据库连接
        /// </summary>
        public void DestroyAllDbConn()
        {
            Dictionary<int, DbAccessPool> dbAccessPoolDic = DbAccessPool.GetPoolList();
            List<int> connFlagList = new List<int>();
            foreach (DbAccessPool accessPool in dbAccessPoolDic.Values)
                accessPool.Stop(true);
        }

        /// <summary>
        /// 获取数据库主访问池
        /// </summary>
        /// <param name="dbConnFlag">数据库类型</param>
        /// <returns>数据库访问池</returns>
        public DbAccessPool GetDBMainPool(eDbConnFlag dbConnFlag, int searchFlag = 0)
        {
            switch (dbConnFlag)
            {
                case eDbConnFlag.Game:
                    if (searchFlag == 0 && m_gameDBMainConnFlagList.Count > 0)
                    {
                        return DbAccessPool.GetPool(m_gameDBMainConnFlagList[0]);
                    }
                    foreach (int gameFlag in m_gameDBMainConnFlagList)
                    {
                        if (gameFlag == searchFlag)
                            return DbAccessPool.GetPool(searchFlag);
                    }
                    return null;
                case eDbConnFlag.Log:
                    return m_logDbAccPool;
                case eDbConnFlag.Secd:
                    return m_secdDbAccPool;
                case eDbConnFlag.GM:
                    return m_gmDBAccPool;
                case eDbConnFlag.Platform:
                    return m_platformDBAccPool;
                case eDbConnFlag.Moniteor:
                    return m_moniteorDBAccPool;
                case eDbConnFlag.Entry:
                    return m_entryDBAccPool;
                case eDbConnFlag.GiftCode:
                    return m_giftCodeDBAccPool;
            }
            return null;
        }

        /// <summary>
        /// 获取数据库访问池
        /// </summary>
        /// <param name="searchFlag">访问池ID</param>
        /// <returns>访问池</returns>
        public DbAccessPool GetDBPool(int searchFlag)
        {
            return DbAccessPool.GetPool(searchFlag);
        }

        public DbAccessPool GetGMDBPool()
        {
            return m_gmDBAccPool;
        }

        /// <summary>
        /// 获取游戏数据主访问池列表
        /// </summary>
        /// <returns></returns>
        public List<DbAccessPool> GetGameDbMainPoolList()
        {
            List<DbAccessPool> dbPoolList = new List<DbAccessPool>();
            DbAccessPool dbPool;
            foreach (int gameFlag in m_gameDBMainConnFlagList)
            {
                dbPool = DbAccessPool.GetPool(gameFlag);
                if (dbPool != null)
                    dbPoolList.Add(dbPool);
            }

            return dbPoolList;
        }

        /// <summary>
        /// 获取一个游戏数据主访问持
        /// </summary>
        /// <returns></returns>
        public int GetOneGameDbMainPool()
        {
            int index = HTBaseFunc.random.Next(0, m_gameDBMainConnFlagList.Count);
            return m_gameDBMainConnFlagList[index];
        }

        /// <summary>
        /// 获取主访问池下的所有子访问池ID列表
        /// </summary>
        /// <param name="mainFlag">主访问池ID</param>
        /// <returns>子访问池ID列表</returns>
        public List<int> GetGameDbChildPoolFlagList(int mainFlag)
        {
            List<int> childFlagList;
            m_gameDBChildConnFlagDic.TryGetValue(mainFlag, out childFlagList);
            return childFlagList;
        }

        /// <summary>
        /// 随机一个访问池ID
        /// </summary>
        /// <param name="mainFlag">主访问池ID</param>
        /// <returns></returns>
        public int RadomGameDbChildPoolFlag(int mainFlag)
        {
            List<int> childFlagList;
            if (m_gameDBChildConnFlagDic.TryGetValue(mainFlag, out childFlagList))
            {
                if (childFlagList.Count > 0)
                    return childFlagList[HTBaseFunc.random.Next(0, childFlagList.Count)];
            }

            return mainFlag;
        }

        /// <summary>
        /// 显示缓存中数据库访问数量
        /// </summary>
        /// <returns></returns>
        public void ShowDBAccCountInPool()
        {
            if (m_logDbAccPool != null)
                SvLogger.Info(string.Format("### Main DBType=LogDB, AccCountInPool={0}.", m_logDbAccPool.GetDBCountInPool()));

            if (m_secdDbAccPool != null)
                SvLogger.Info(string.Format("### Main DBType=SecdDB, AccCountInPool={0}.", m_secdDbAccPool.GetDBCountInPool()));

            for (int i = 0; i < m_gameDBMainConnFlagList.Count; ++i )
            {
                DbAccessPool dbPool = GetDBPool(m_gameDBMainConnFlagList[i]);
                if (dbPool != null)
                {
                    SvLogger.Info(string.Format("### Main DBType=GameDB, AccCountInPool={0}.", dbPool.GetDBCountInPool()));
                    List<int> childFlagList;
                    m_gameDBChildConnFlagDic.TryGetValue(m_gameDBMainConnFlagList[i], out childFlagList);
                    if (childFlagList != null)
                    {
                        for (int j = 0; j < childFlagList.Count; ++j)
                        {
                            dbPool = GetDBPool(childFlagList[j]);
                            if (dbPool != null)
                            {
                                SvLogger.Info(string.Format("###        Child DBFlag={0}, AccCountInPool={1}.", childFlagList[j], dbPool.GetDBCountInPool()));
                            }
                        }
                    }
                }
            }
            
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// 构造
        /// </summary>
        private DbAccess()
        {
            m_connFlagMakeLines.Add(eDbConnFlag.Game, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.Log, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.Secd, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.SqlLog, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.GM, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.Platform, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.Moniteor, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.Entry, 0x0000);
            m_connFlagMakeLines.Add(eDbConnFlag.GiftCode, 0x0000);
        }

        #endregion

        #region Data Member

        //数据建立连接线路字典
        private Dictionary<eDbConnFlag, int> m_connFlagMakeLines = new Dictionary<eDbConnFlag, int>();

        //游戏数据数据库主连接列表
        private List<int> m_gameDBMainConnFlagList = new List<int>();
        //游戏数据数据库子连接字典
        private Dictionary<int, List<int>> m_gameDBChildConnFlagDic = new Dictionary<int, List<int>>();
        //LOG数据库连接
        private DbAccessPool m_logDbAccPool;
        //邮件数据库连接
        private DbAccessPool m_secdDbAccPool;
        //GM服务器数据库
        private DbAccessPool m_gmDBAccPool;
        //平台数据库
        private DbAccessPool m_platformDBAccPool;
        //监控数据库
        private DbAccessPool m_moniteorDBAccPool;
        //Entry服务器数据库
        private DbAccessPool m_entryDBAccPool;
        //激活码服务器数据库
        private DbAccessPool m_giftCodeDBAccPool;
        #endregion
    }

    



}
