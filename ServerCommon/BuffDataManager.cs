using com.tieao.mmo.CustomTypeInProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class BuffDataItem
    {
        public BuffDataItem(Guid id, Action<object[]> calback, object[] data)
        {
            m_id = id;
            m_data = data;
            m_callback = calback;
        }

        public Guid Id
        {
            get { return m_id; }
        }


        public void Call(params object[] dataPara)
        {
            if (dataPara == null || dataPara.Length == 0)
            {
                m_callback(m_data);
            }
            else
            {
                List<object> paras = new List<object>();
                paras.AddRange(m_data);
                paras.AddRange(dataPara);
                m_callback(paras.ToArray());
            }
        }            

        Guid m_id;
        object[] m_data;
        Action<object[]> m_callback;
    }

    public class BuffDataManager
    {
        BuffDataManager() { }
        static BuffDataManager m_instance = new BuffDataManager();
        public static BuffDataManager Instance { get { return m_instance; } }


        public Guid PutData(Action<object[]> calback,params object[] data)
        {
            var newGuid = Guid.NewGuid();

            int maxCount = 1000;
            for (int i = 0; i < maxCount; i++)
            {
                if (m_dataDic.ContainsKey(newGuid))
                    newGuid = Guid.NewGuid();
                else
                    break;

                if (i == maxCount - 1)
                {
                    throw new Exception("DataBuffManager.PutData Bad for");
                }
            }


            BuffDataItem item = new BuffDataItem(newGuid, calback, data);
            m_dataDic.Add(newGuid, item);

            return newGuid;
        }

        public BuffDataItem TakeData(Guid guid)
        {
            BuffDataItem result;
            if (m_dataDic.TryGetValue(guid, out result))
            {
                m_dataDic.Remove(guid);
            }
            
            return result;
        }

        Dictionary<Guid, BuffDataItem> m_dataDic = new Dictionary<Guid, BuffDataItem>();
    }
}
