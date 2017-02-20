using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    public class Activity_Basic_Json
    {
        [DataMember(Order = 0)]
        public int activity_id { get; set; }

        [DataMember(Order = 1)]
        public int activity_type { get; set; }

        [DataMember(Order = 2)]
        public int entry_flag { get; set; }

        [DataMember(Order = 3)]
        public int show_priority { get; set; }

        [DataMember(Order = 4)]
        public int show_time { get; set; }

        [DataMember(Order = 5)]
        public int start_time { get; set; }

        [DataMember(Order = 6)]
        public int end_time { get; set; }

        [DataMember(Order = 7)]
        public int disappear_time { get; set; }

        [DataMember(Order = 8)]
        public string title { get; set; }


        public Activity_Basic_Json Clone()
        {
            return (Activity_Basic_Json)this.MemberwiseClone();
        }

        public string GetBasicJsonStr()
        {
            Activity_Basic_Json basic = new Activity_Basic_Json();
            basic.activity_id = this.activity_id;
            basic.activity_type = this.activity_type;
            basic.entry_flag = this.entry_flag;
            basic.show_priority = this.show_priority;
            basic.show_time = this.show_time;
            basic.start_time = this.start_time;
            basic.end_time = this.end_time;
            basic.disappear_time = this.disappear_time;
            basic.title = this.title;

            return JsonTool.GetJsonStr(basic);
        }

        public virtual string GetRewardJsonStr()
        {
            return "";
        }
    }
}
