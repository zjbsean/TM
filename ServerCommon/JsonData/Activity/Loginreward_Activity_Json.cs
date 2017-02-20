using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    [DataContract]
    public class Loginreward_Activity_Json : Activity_Basic_Json
    {
        [DataMember(Order = 0)]
        public Day_Reward_Item[] day_reward_list { get; set; }

        public override string GetRewardJsonStr()
        {
            return JsonTool.GetJsonStr(day_reward_list);
        }
    }

    [DataContract]
    public class Day_Reward_Item
    {
        [DataMember(Order = 0)]
        public Item_Config[] rewards { get; set; }
    }
}
