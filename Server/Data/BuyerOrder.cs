using System;
using System.Collections.Generic;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;
using System.Data;
using System.Text;

namespace Server
{
    class BuyerOrder : DBItemBase<TBuyerOrder>
    {
        public BuyerOrder(TBuyerOrder order)
        {
            Update(order);
        }

        protected override void updateSql()
        {
            m_operSql = string.Format(BuyerOrderDBHelper.SQL_Replace, m_Data.orderID, m_Data.buyerID, m_Data.makeTime, BuyerOrderDBHelper.ItemsListToString(m_Data.buyItems), m_Data.state);
        }

        protected override void deleteSql()
        {
            m_operSql = string.Format(BuyerOrderDBHelper.SQL_Delete, m_Data.orderID);
        }
    }

    class BuyerOrderDBHelper
    {
        public static string ItemsListToString(TBusinessItemList itemList)
        {
            List<string> itemStrList = new List<string>();
            foreach (TBusinessItem item in itemList.GetElements())
                itemStrList.Add(string.Format("{0},{1},{2}", item.itemID, item.count, item.mark));
            return string.Join(";", itemStrList);
        }

        public static TBusinessItemList ItemsStringToList(string itemsStr)
        {
            TBusinessItemList itemList = new TBusinessItemList();
            string[] orderItemStrList = itemsStr.Split(';');
            for (int i = 0; i < orderItemStrList.Length; i++)
            {
                string[] itemPropStrList = orderItemStrList[i].Split(',');
                TBusinessItem item = new TBusinessItem();
                item.itemID = Convert.ToInt64(itemPropStrList[0]);
                item.count = Convert.ToInt32(itemPropStrList[1]);
                item.mark = itemPropStrList[2];
                item.price = 0.0f;
                itemList.Add(item);
            }
            return itemList;
        }

        public const string SQL_Select_WithOriderID = "select * from info_buyerorder where OrderID=\"{0}\";";
        public const string SQL_Select_WithBuyerID = "select * from info_buyerorder where BuyerID={0};";
        public const string SQL_Select_WithMakeTime = "select * from info_buyerorder where \"{0}\" <= MakeTime and MakeTime <= \"{1}\";";

        public const string SQL_Replace = "replace into info_buyerorder(OrderID,BuyerID,MakeTime,Items,State) values (\"{0}\",{1},\"{2}\",\"{3}\",{4});";
        public const string SQL_Delete = "delete from info_buyerorder where OrderID=\"{0}\";";
    }
}
