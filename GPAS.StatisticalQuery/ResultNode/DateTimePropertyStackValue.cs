using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]

    public class DateTimePropertyStackValue
    {
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public DateTime Start { get; set; }
    }
}
