using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisLib
{
    public class RedisAccessPoolManager
    {
        public static RedisAccessPoolManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public bool AddAccessPool(RedisAccessPool redisAccessPool)
        {
            if (m_accessPoolDic.ContainsKey(redisAccessPool.ID))
                return false;

            m_accessPoolDic.Add(redisAccessPool.ID, redisAccessPool);
            return true;
        }

        public bool StopAccessPool(int flagID)
        {
            RedisAccessPool accessPool;
            if(m_accessPoolDic.TryGetValue(flagID, out accessPool))
            {
                if(accessPool.IsRun == true)
                    accessPool.Stop();
                return true;
            }
            return false;
        }

        public bool StartAccessPool(int flagID)
        {
            RedisAccessPool accessPool;
            if(m_accessPoolDic.TryGetValue(flagID, out accessPool))
            {
                if (accessPool.IsRun == false)
                    accessPool.Start();

                return true;
            }
            return false;
        }

        public void StopAllAccessPool()
        {
            foreach(RedisAccessPool accessPool in m_accessPoolDic.Values)
                accessPool.Stop();
        }

        public void StartAllAccessPool()
        {
            foreach (RedisAccessPool accessPool in m_accessPoolDic.Values)
                accessPool.Start();
        }

        public List<RedisCommandBase> CommandBackDataList
        {
            get
            {
                foreach(RedisAccessPool accessPool in m_accessPoolDic.Values)
                    accessPool.PopAllMainThreadDealedCommandList(ref m_commandBackDataList);
                return m_commandBackDataList;
            }
        }

        private List<RedisCommandBase> m_commandBackDataList = new List<RedisCommandBase>();
        private Dictionary<int, RedisAccessPool> m_accessPoolDic = new Dictionary<int, RedisAccessPool>();
        private static RedisAccessPoolManager m_instance = new RedisAccessPoolManager();
        private RedisAccessPoolManager() { }
    }
}
