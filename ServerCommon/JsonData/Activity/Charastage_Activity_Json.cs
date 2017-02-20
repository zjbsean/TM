using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    [DataContract]
    public class Charastage_Activity_Json : Activity_Basic_Json
    {
        [DataMember(Order = 0)]
        public Charastage_Type_Reward_Item[] charastage_type_reward_list { get; set; }
        public override string GetRewardJsonStr()
        {
            return JsonTool.GetJsonStr(charastage_type_reward_list);
        }
    }

    [DataContract]
    public class Charastage_Type_Reward_Item
    {
        [DataMember(Order = 0)]
        public int big_id { get; set; }

        [DataMember(Order = 1)]
        public int small_id { get; set; }

        [DataMember(Order = 2)]
        public int diff { get; set; }

        /// <summary>
        /// 奖励数量限制,0=不限数量
        /// </summary>
        [DataMember(Order = 3)]
        public int limit_count { get; set; }

        /// <summary>
        /// 通关天数限制  0=不限天数
        /// </summary>
        [DataMember(Order = 4)]
        public int limit_day { get; set; }

        [DataMember(Order = 5)]
        public Item_Config[] rewards { get; set; }
    }

}
