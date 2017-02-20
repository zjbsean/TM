using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    /// <summary>
    /// 山庄膳房
    /// </summary>
    public class Dining_Activity_Json : Activity_Basic_Json
    {
        [DataMember(Order = 0)]
        public int tiLi { get; set; }

        public override string GetRewardJsonStr()
        {
            return tiLi.ToString();
        }
    }

}
