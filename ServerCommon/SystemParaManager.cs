using com.tieao.mmo.CustomTypeInProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class SystemParaManager
    {

        public const string C_SNAPSHOT = "snapshot";
        public const string C_GuildLastClearingData = "GuildLastClearingData";

        //PVP赛季
        public const string C_PVPSeasonID = "PVPSeasonID";
        public const string C_PVPSeasonState = "PVPSeasonState";
        public const string C_PVPSeasonStockDater = "PVPSeasonStockDater";
        public const string C_PVPSeasonOpeningTime = "PVPSeasonOpeningTime";

        //玩家数据最后刷新时间点
        public const string C_PlayerLastDayRefreshTime = "PlayerLastDayRefreshTime";


        public static void LoadData(TStrKeyValueList paraList)
        {
            foreach (var item in paraList.GetElements())
            {
                m_paras[item.key] = item.value;
            }
        }

        public static void Set(string name, string value)
        {
            m_paras[name] = value;
        }

        public static string Get(string name)
        {
            string value = null;
            m_paras.TryGetValue(name, out value);
            return value;
        }

        public static TStrKeyValueList TransformData
        {
            get 
            {
                TStrKeyValueList paraList = new TStrKeyValueList();
                foreach (var item in m_paras)
                {
                    paraList.Add(new TStrKeyValue() { key = item.Key, value = item.Value });
                }
                return paraList;
            } 
        }


        static Dictionary<string, string> m_paras = new Dictionary<string, string>();
    }

}
