using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    [DataContract]
    public class FirstPay_Activity_Json : Activity_Basic_Json
    {

        [DataMember(Order = 0)]
        public Item_Config[] reward_list { get; set; }

        public override string GetRewardJsonStr()
        {
            return JsonTool.GetJsonStr(reward_list);
        }
    }
}
