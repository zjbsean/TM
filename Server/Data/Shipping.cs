using System;
using System.Collections.Generic;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;
using System.Data;

namespace Server
{
    class Shipping : DBItemBase<TShipping>
    {
        public Shipping(TShipping data)
        {
            Update(data);
        }

        protected override void updateSql()
        {
            m_operSql = string.Format(ShippingManager.SQL_Replace, m_Data.id, m_Data.name, m_Data.desc, ShippingManager.LinkmanListToString(m_Data.linkMans));
        }

        protected override void deleteSql()
        {
            m_operSql = string.Format(ShippingManager.SQL_Delete, m_Data.id);
        }
    }

    class ShippingManager : IDataDBOper
    {
        public static ShippingManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public override void LoadDataFromDB()
        {
            DBVisiter.Exec(SQL_Select, loadBack, null);
        }

        public override void OnLoadDataFromDBFinish(bool isSucc)
        {

        }

        /// <summary>
        /// 载入道具返回
        /// </summary>
        /// <param name="dbi"></param>
        private void loadBack(DbAccessItem dbi)
        {
            if (dbi.Result.ErrCode != 0)
                SvLogger.Error("DB Access Fail : Op=Load Shipping, ErrMsg={0}, Data={1}.", dbi.Result.ErrMsg, dbi.SpName);
            else
            {
                DataTable dt = dbi.OutDs.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    TShipping t = new TShipping();
                    t.id = HTBaseFunc.NullToLong(dr["ID"]);
                    t.name = HTBaseFunc.NullToStr(dr["Name"]);
                    t.desc = HTBaseFunc.NullToStr(dr["Desc"]);
                    t.country = HTBaseFunc.NullToStr(dr["Country"]);
                    t.address = HTBaseFunc.NullToStr(dr["Addres"]);
                    t.tel = HTBaseFunc.NullToStr(dr["Tel"]);
                    t.linkMans = LinkmanStringToList(HTBaseFunc.NullToStr(dr["linkman"]));
                    Shipping s = new Shipping(t);
                    m_shippingDic[s.m_Data.id] = s;
                }

                DataLoadManager.Instance.AddLoadSuccTableCount();
            }

            OnLoadDataFromDBFinish(dbi.Result.ErrCode == 0);
        }

        public const string SQL_Select = "select * from info_shipping";

        public const string SQL_Replace = "replace into info_shipping(ID, Name, Desc, Country, Addres, Tel, linkman) vaules ({0}, \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\");";

        public const string SQL_Delete = "delete from info_shipping where ID={0};";

        public static string LinkmanListToString(TStrKeyValueList linkmanList)
        {
            if (linkmanList == null || linkmanList.GetElements().Count == 0)
                return "";
            List<string> strList = new List<string>();
            foreach (TStrKeyValue linkman in linkmanList.GetElements())
            {
                strList.Add(string.Format("{0},{1}", linkman.key, linkman.value));
            }
            return string.Join(";", strList);
        }

        public static TStrKeyValueList LinkmanStringToList(string linkmansStr)
        {
            if (String.IsNullOrEmpty(linkmansStr))
            {
                return new TStrKeyValueList();
            }
            TStrKeyValueList linkmanList = new TStrKeyValueList();
            string[] linkmanStrList = linkmansStr.Split(';');
            for (int i = 0; i < linkmanStrList.Length; i++)
            {
                string[] tmpStrArr = linkmanStrList[i].Split(',');
                TStrKeyValue t = new TStrKeyValue() { key = tmpStrArr[0], value = tmpStrArr[1] };
                linkmanList.Add(t);
            }
            return linkmanList;
        }

        private Dictionary<long, Shipping> m_shippingDic = new Dictionary<long, Shipping>();

        private static ShippingManager m_instance = new ShippingManager();
        private ShippingManager() { }
    }
}
