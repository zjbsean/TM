using System;
using System.Collections.Generic;
using System.Data;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;

namespace Server
{
    class Buyer : DBItemBase<TBuyer>
    {
        public Buyer(TBuyer data)
        {
            Update(data);
        }

        protected override void updateSql()
        {
            m_operSql = string.Format(BuyerManager.SQL_Replace, m_Data.id, m_Data.name, m_Data.desc, BuyerManager.LinkmanListToString(m_Data.linkMans));
        }

        protected override void deleteSql()
        {
            m_operSql = string.Format(BuyerManager.SQL_Delete, m_Data.id);
        }
    }

    class BuyerManager : IDataDBOper
    {
        public static BuyerManager Instance
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
                SvLogger.Error("DB Access Fail : Op=Load Buyer, ErrMsg={0}, Data={1}.", dbi.Result.ErrMsg, dbi.SpName);
            else
            {
                DataTable dt = dbi.OutDs.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    TBuyer t = new TBuyer();
                    t.id = HTBaseFunc.NullToLong(dr["ID"]);
                    t.name = HTBaseFunc.NullToStr(dr["Name"]);
                    t.desc = HTBaseFunc.NullToStr(dr["Desc"]);
                    t.country = HTBaseFunc.NullToStr(dr["Country"]);
                    t.address = HTBaseFunc.NullToStr(dr["Addres"]);
                    t.tel = HTBaseFunc.NullToStr(dr["Tel"]);
                    t.linkMans = LinkmanStringToList(HTBaseFunc.NullToStr(dr["linkman"]));
                    Buyer b = new Buyer(t);
                    m_buyerDic[b.m_Data.id] = b;
                }

                DataLoadManager.Instance.AddLoadSuccTableCount();
            }

            OnLoadDataFromDBFinish(dbi.Result.ErrCode == 0);
        }

        public const string SQL_Select = "select * from info_buyers";

        public const string SQL_Replace = "replace into info_buyers(ID, Name, Desc, Country, Addres, Tel, linkman) vaules ({0}, \"{1}\", \"{2}\", \"{3}\", \"{4}\", \"{5}\", \"{6}\");";

        public const string SQL_Delete = "delete from info_buyers where ID={0};";

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
                TStrKeyValue t = new TStrKeyValue(){key = tmpStrArr[0], value = tmpStrArr[1]};
                linkmanList.Add(t);
            }
            return linkmanList;
        }

        private Dictionary<long, Buyer> m_buyerDic = new Dictionary<long, Buyer>(); 

        private static BuyerManager m_instance = new BuyerManager();
        private BuyerManager() { }  
    }
}
