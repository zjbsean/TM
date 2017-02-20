using System;
using System.Collections.Generic;
using System.Text;

namespace RedisLib
{
    class RedisProtocolSenderAccessPoolManager
    {
        class ChannelData
        {
            public int ChannelID;
            public string ChannelName;
            public RedisAccessPool AccessPool;
        }

        public static RedisProtocolSenderAccessPoolManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public void SetChannel(string channelName, int channelID, RedisAccessPool accessPool)
        {
            ChannelData channelData = new ChannelData() 
            {   ChannelID = channelID, 
                ChannelName = channelName, 
                AccessPool = accessPool };

            m_channelByIDDic[channelData.ChannelID] = channelData;
            m_channelByNameDic[channelData.ChannelName] = channelData;
        }

        public RedisAccessPool GetAccessPool(int channelID)
        {
            ChannelData channel;
            if(m_channelByIDDic.TryGetValue(channelID, out channel))
                return channel.AccessPool;
            return null;
        }

        public RedisAccessPool GetAccessPool(string channelName)
        {
            ChannelData channel;
            if (m_channelByNameDic.TryGetValue(channelName, out channel))
                return channel.AccessPool;
            return null;
        }

        private Dictionary<int, ChannelData> m_channelByIDDic = new Dictionary<int, ChannelData>();
        private Dictionary<string, ChannelData> m_channelByNameDic = new Dictionary<string, ChannelData>();

        private static RedisProtocolSenderAccessPoolManager m_instance = new RedisProtocolSenderAccessPoolManager();
        private RedisProtocolSenderAccessPoolManager() { }
    }
}
