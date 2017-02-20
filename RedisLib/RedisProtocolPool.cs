using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GsTechLib;

namespace RedisLib
{
    public class RedisProtocolPool
    {
        public RedisProtocolPool(RedisAccess redisAccess, List<string> channelNameList, int privateChannelID)
        {
            m_redisAccess = redisAccess;
            foreach(string channelName in channelNameList)
                m_channelNameSet.Add(channelName);

            m_privateChannelID = privateChannelID;
        }

        public RedisProtocolPool(RedisAccess redisAccess, string channelName, int privateChannelID)
        {
            m_redisAccess = redisAccess;
            m_channelNameSet.Add(channelName);

            m_privateChannelID = privateChannelID;
        }

        public void AddChannelName(string channelName)
        {
            if(m_channelNameSet.Contains(channelName) == false)
                m_channelNameSet.Add(channelName);
        }

        public int PrivateChannelID
        {
            get
            {
                return m_privateChannelID;
            }
        }

        public void Start()
        {
            if(m_runThread == false)
            {
                m_runThread = true;
                m_channelNameArr = m_channelNameSet.ToArray<string>();
                m_thread = new Thread(run);
                m_thread.Start();
            }
        }

        public void Stop()
        {
            if(m_thread != null && m_runThread == true)
            {
                m_runThread = false;
            }
        }

        private void run()
        {
            while(m_runThread)
            {
                if (m_redisCliDisconnect == false)
                {
                    try
                    {
                        byte[][] datas = m_redisAccess.RedisCli.BRPop(m_channelNameArr, 1);
                        if(datas.Length > 1)
                        {
                            RedisProtocolData protocolData = new RedisProtocolData(ref datas[1]);
                            lock (m_protocolDatasLockObj)
                                m_selfThreadProtocolDataList.Add(protocolData);
                        }
                    }
                    catch (Exception ex)
                    {
                        if(m_redisAccess.RedisCli.IsSocketConnected() == false)
                            m_redisCliDisconnect = true;

                        SvLogger.Fatal(ex, "{0}\n{1}.", ex.Message, ex.StackTrace);
                    }
                }
                else
                {
                    try
                    {
                        if (m_redisAccess.Reconnect())
                            m_redisCliDisconnect = false;
                        else
                            Thread.Sleep(1000);
                    }
                    catch(Exception ex)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        public void PopAllProtocolDatas(ref List<RedisProtocolData> protocolList)
        {
            lock(m_protocolDatasLockObj)
            {
                if(m_selfThreadProtocolDataList.Count > 0)
                {
                    protocolList.AddRange(m_selfThreadProtocolDataList);
                    m_selfThreadProtocolDataList.Clear();
                }
            }
        }

        private bool m_runThread = false;
        private bool m_redisCliDisconnect = false;

        private object m_protocolDatasLockObj = new object();
        private List<RedisProtocolData> m_selfThreadProtocolDataList = new List<RedisProtocolData>();
        
        private HashSet<string> m_channelNameSet = new HashSet<string>();
        private int m_privateChannelID = 0;
        private string[] m_channelNameArr;
        private RedisAccess m_redisAccess;
        private Thread m_thread = null;
    }
}
