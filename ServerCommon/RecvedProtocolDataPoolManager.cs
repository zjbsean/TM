using System;
using System.Threading;
using System.Collections.Generic;
using com.ideadynamo.foundation.buffer;
using GsTechLib;

namespace ServerCommon
{
    //网络更新代理
    public delegate void NetworkUpdateDelegate();

    //接收协议缓存数据
    public class RecvedProtocolDataInfo
    {
        public RecvedProtocolDataInfo(bool isClientData, bool isLongConnection, int sessionID, ByteArray datas)
        {
            m_IsClientData = isClientData;
            m_IsLongConnection = isLongConnection;
            m_SessionID = sessionID;
            m_Datas = datas;

            m_RecordTime = Environment.TickCount;
        }

        public bool m_IsClientData = false; //是否是客户端协议
        public bool m_IsLongConnection = false;
        public int m_SessionID = -1;    //连接SessionID
        public ByteArray m_Datas;       //协议数据
        public int m_RecordTime;        //记录时间
    }

    //接收到的协议数据缓存池管理
    public class RecvedProtocolDataPoolManager
    {
        #region Public Functions

        /// <summary>
        /// 压入数据
        /// </summary>
        /// <param name="protoData">协议数据</param>
        public void PushData(RecvedProtocolDataInfo protoData)
        {
            lock (m_cachePoolLock)
            {
                if (protoData.m_IsClientData == true)
                {
                    ++m_curClientCacheItems;

                    if (m_curClientCacheItems > m_maxClientCacheItems)
                    {
                        --m_curClientCacheItems;
                        
                        //协议丢弃提示
                        SvLogger.Error("Protocol Pool Is Full!!!");
                        return;
                    }
                }

                m_cachePool.Enqueue(protoData);
            }
        }

        /// <summary>
        /// 推出数据
        /// </summary>
        /// <returns>协议数据</returns>
        public List<RecvedProtocolDataInfo> PopAllData()
        {
            lock (m_cachePoolLock)
            {
                if (m_cachePool.Count > 0)
                {
                    m_dealPool.AddRange(m_cachePool);
                    m_cachePool.Clear();
                    m_curClientCacheItems = 0;
                }
            }
            return m_dealPool;
        }

        /// <summary>
        /// 打印协议缓存池中排队协议数
        /// </summary>
        /// <returns></returns>
        public void ShowProtocolCountInPool()
        {
            //SvLogger.Info("*** ShowProtocolCountInPool Before Lock.");
            lock (m_cachePoolLock)
            {
                SvLogger.Info("### Protocol Count In Pool={0}.", m_cachePool.Count);
            }
            //SvLogger.Info("*** ShowProtocolCountInPool After Lock.");
        }

        /// <summary>
        /// 打印协议平均等待时间
        /// </summary>
        public void ShowProtocolWaitingTimeAverage()
        {
            //SvLogger.Info("*** ShowProtocolWaitingTimeAverage Before Lock.");
            lock (m_cachePoolLock)
            {
                int aveTime = 0;
                if (m_protoTotalCount != 0)
                    aveTime = m_protoTotalWaitTime / m_protoTotalCount;

                SvLogger.Info("### Protocol Wait Time Average={0}.", aveTime);
                SvLogger.Info("### Protocol Count Deal This Minutes={0}.", m_protoTotalCount);

                m_protoTotalWaitTime = 0;
                m_protoTotalCount = 0;
            }
            //SvLogger.Info("*** ShowProtocolWaitingTimeAverage After Lock.");
        }

        #endregion


        #region Private Functions

        

        #endregion

        #region Property

        public static RecvedProtocolDataPoolManager Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        #region Data Member

        private int m_protoTotalWaitTime = 0;           //协议总等待时间
        private int m_protoTotalCount = 0;              //协议总数

        private object m_cachePoolLock = new object();  //缓存池锁
        private Queue<RecvedProtocolDataInfo> m_cachePool = new Queue<RecvedProtocolDataInfo>();    //缓存池

        private List<RecvedProtocolDataInfo> m_dealPool = new List<RecvedProtocolDataInfo>();

        private int m_curClientCacheItems = 0;          //当前客户端缓存条数
        private int m_maxClientCacheItems = 10000;      //最大缓存客户端协议量（暂定客户端协议包大小平均1KB，缓存区大概在10M的缓存容量。设置太大没意义，玩家等不起）

        private static RecvedProtocolDataPoolManager _instance = new RecvedProtocolDataPoolManager();
        private RecvedProtocolDataPoolManager() { }

        #endregion
    }
}
