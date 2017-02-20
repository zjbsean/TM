using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class BatchExecItem
    {
        public BatchExecItem(Action<object[]> callback, List<object[]> parasList, int oneExecCount)
        {
            m_callback = callback;
            m_parasList = parasList;
            m_oneExecCount = oneExecCount;
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <returns>是否执行完毕</returns>
        public bool Exec()
        {

            for (int i = 0; i < m_oneExecCount; i++)
            {
                if (m_parasList.Count == 0)
                {
                    return true;
                }

                m_callback(m_parasList[0]);

                m_parasList.RemoveAt(0);
            }

            return false;
        }


        Action<object[]> m_callback;
        List<object[]> m_parasList;
        int m_oneExecCount;

    }
    public class BatchExecManager
    {
        BatchExecManager() { }
        static BatchExecManager m_instance = new BatchExecManager();
        public static BatchExecManager Instance { get { return m_instance; } }

        public void Update(int currTime)
        {
            List<BatchExecItem> completeList = null;

            for (int i = 0; i < m_execItemList.Count; i++)
            {
                if (m_execItemList[i].Exec())
                {
                    if (completeList == null)
                        completeList = new List<BatchExecItem>();

                    completeList.Add(m_execItemList[i]);
                }
            }

            if (completeList != null)
            {
                for (int i = 0; i < completeList.Count; i++)
                {
                    m_execItemList.Remove(completeList[i]);
                }
            }
        }

        public void AddExecItem(BatchExecItem item)
        {
            m_execItemList.Add(item);
        }


        List<BatchExecItem> m_execItemList = new List<BatchExecItem>();

    }
}
