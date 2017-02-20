using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{
    [DataContract]
    public class JsonBatchItem
    {
        [DataMember(Order = 0)]
        public int gen_num { get; set; }

        [DataMember(Order = 1)]
        public Item_Config[] item_config { get; set; }
    }

}
