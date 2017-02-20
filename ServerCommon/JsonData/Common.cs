using com.tieao.mmo.CustomTypeInProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerCommon
{

    [DataContract]
    public class Item_Config
    {
        [DataMember(Order = 0)]
        public int item_id { get; set; }
        [DataMember(Order = 1)]
        public int item_num { get; set; }

        public TIntKeyValue TransferData 
        {
            get
            {
                return new TIntKeyValue() { key = item_id, value = item_num };
            }
        }
    }
}
