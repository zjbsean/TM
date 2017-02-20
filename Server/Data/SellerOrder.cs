using System;
using System.Collections.Generic;
using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;
using System.Data;
using System.Text;

namespace Server
{
    class SellerOrder : DBItemBase<TSellerOrder>
    {
        public SellerOrder(TSellerOrder order)
        {
            Update(order);
        }

        protected override void updateSql()
        {
            m_operSql = string.Format(SellerOrderDBHelper.SQL_Replace, m_Data.orderID, m_Data.sellerID, m_Data.theBuyerOrderID, m_Data.makeTime, SellerOrderDBHelper.ItemsListToString(m_Data.sellItems));
        }

        protected override void deleteSql()
        {
            m_operSql = string.Format(SellerOrderDBHelper.SQL_Delete, m_Data.orderID);
        }
    }

    class SellerOrderDBHelper
    {
        public static string ItemsListToString(TBusinessItemList itemList)
        {
            List<string> itemStrList = new List<string>();
            foreach (TBusinessItem item in itemList.GetElements())
                itemStrList.Add(string.Format("{0},{1},{2},{3}", item.itemID, item.count, item.mark, item.price));
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
                item.price = Convert.ToSingle(itemPropStrList[3]);
                itemList.Add(item);
            }
            return itemList;
        }

        public const string SQL_Select_WithOriderID = "select * from info_sellerorder where OrderID=\"{0}\";";
        public const string SQL_Select_WithSellerID = "select * from info_sellerorder where SellerID={0};";
        public const string SQL_Select_WithBuyerOriderID = "select * from info_sellerorder where TheBuyerOrderID=\"{0}\";";
        public const string SQL_Select_WithMakeTime = "select * from info_sellerorder where \"{0}\" <= MakeTime and MakeTime <= \"{1}\";";

        public const string SQL_Replace = "replace into info_sellerorder(OrderID,SellerID,TheBuyerOrderID,MakeTime,Items,State) values (\"{0}\",{1},\"{2}\",\"{3}\",\"{4}\",{5});";
        public const string SQL_Delete = "delete from info_sellerorder where OrderID=\"{0}\";";
    }
}
