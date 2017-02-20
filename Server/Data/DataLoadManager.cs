using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GsTechLib;

namespace Server
{
    class DataLoadManager
    {
        public static DataLoadManager Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// 载入数据
        /// </summary>
        public void LoadData()
        {
            foreach (IDataDBOper load in m_loadSendList)
            {
                load.LoadDataFromDB();
            }
        }

        /// <summary>
        /// 添加载入成功表数量
        /// </summary>
        public void AddLoadSuccTableCount()
        {
            ++m_succLoadTableCount;

            Console.WriteLine("Load Succ Count = {0}.", m_succLoadTableCount);

            if (m_succLoadTableCount >= m_needSuccLoadTableCount)
                OnLoadAllDataSucc();
        }

        public void OnLoadAllDataSucc()
        {
            
        }

        private List<IDataDBOper> m_loadSendList = new List<IDataDBOper>();
        
        private int m_needSuccLoadTableCount = 0;       //需要成功载入数据库表数量
        private int m_succLoadTableCount = 0;           //成功载入数据库表数量

        private static DataLoadManager m_instance = new DataLoadManager();

        private DataLoadManager()
        {
            m_loadSendList.Add(ItemsManager.Instance);
            m_loadSendList.Add(AccountManager.Instance);
            m_loadSendList.Add(BuyerManager.Instance);
            m_loadSendList.Add(SellerManager.Instance);
            m_loadSendList.Add(ShippingManager.Instance);
        }
    }
}
