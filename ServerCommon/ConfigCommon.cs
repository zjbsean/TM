using com.tieao.mmo.CustomTypeInProtocol;
using GsTechLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    /// <summary>
    /// 性别
    /// </summary>
    public enum ESex
    {
        /// <summary>
        /// 男
        /// </summary>
        Boy,
        /// <summary>
        /// 女
        /// </summary>
        Girl,
    }

    public static class ConfigCommon
    {
        /// <summary>
        /// 道具配置解析
        /// </summary>
        /// <param name="itemStr"></param>
        /// <returns></returns>
        public static TIntKeyValueList ItemCfgparse(string itemStr)
        {
            TIntKeyValueList result = new TIntKeyValueList();
            if (!string.IsNullOrEmpty(itemStr))
            {
                string[] strArr1 = itemStr.Split('|');
                foreach (var str1 in strArr1)
                {
                    if (string.Empty != str1)
                    {
                        string[] itemKeyVal = str1.Split(',');
                        int itemID = Convert.ToInt32(itemKeyVal[0]);
                        int num = Convert.ToInt32(itemKeyVal[1]);
                        result.Add(new TIntKeyValue() { key = itemID, value = num });
                    }
                }
            }
            
            return result;
        }


        public static string ConvertToItemStr(this TIntKeyValueList kvList)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < kvList.GetElements().Count; i++)
            {
                sb.Append(kvList.GetElements()[i].key);
                sb.Append(',');
                sb.Append(kvList.GetElements()[i].value);

                if (i != kvList.GetElements().Count - 1)
                    sb.Append('|');
            }

            return sb.ToString();
        }

    }
}
