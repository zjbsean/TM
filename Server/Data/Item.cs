using System;
using System.Collections.Generic;
using System.Data;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;

namespace Server
{
    class Item : DBItemBase<TItem>
    {
        public Item(TItem i)
        {
            Update(i);
        }

        protected override void updateSql()
        {
            m_operSql = string.Format(ItemsManager.SQL_Replace, m_Data.id, m_Data.name, m_Data.desc, ItemsManager.MarksListToString(m_Data.marks.GetElements()));
        }

        protected override void deleteSql()
        {
            m_operSql = string.Format(ItemsManager.SQL_Delete, m_Data.id);
        }
    }

    class ItemsManager : IDataDBOper
    {
        public static ItemsManager Instance
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
                SvLogger.Error("DB Access Fail : Op=Load Item, ErrMsg={0}, Data={1}.", dbi.Result.ErrMsg, dbi.SpName);
            else
            {
                DataTable dt = dbi.OutDs.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    TItem t = new TItem();
                    t.id = HTBaseFunc.NullToLong(dr["ID"]);
                    t.name = HTBaseFunc.NullToStr(dr["Name"]);
                    t.desc = HTBaseFunc.NullToStr(dr["Desc"]);
                    string marksStr = HTBaseFunc.NullToStr(dr["Marks"]);
                    List<string> marksList = MarksStringToList(marksStr);
                    t.marks = new TStrList();
                    t.marks.GetElements().AddRange(marksList);
                    Item i = new Item(t);
                    m_itemDic[t.id] = i;
                }

                DataLoadManager.Instance.AddLoadSuccTableCount();
            }

            OnLoadDataFromDBFinish(dbi.Result.ErrCode == 0);
        }

        public const string SQL_Select = "select * from info_item";

        public const string SQL_Replace = "replace into info_item(ID, Name, Desc, Marks) vaules ({0}, \"{1}\", \"{2}\", \"{3}\");";

        public const string SQL_Delete = "delete from info_item where ID={0};";

        public static string MarksListToString(List<string> markList)
        {
            if (markList == null || markList.Count == 0)
                return "";
            return string.Join(";", markList);
        }

        public static List<string> MarksStringToList(string marks)
        {
            if (String.IsNullOrEmpty(marks))
            {
                return new List<string>();
            }
            return new List<string>(marks.Split(';'));
        }

        private Dictionary<long, Item> m_itemDic = new Dictionary<long, Item>(); 

        private static ItemsManager m_instance = new ItemsManager();
        private ItemsManager() { }  
    }
}
