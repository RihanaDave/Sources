using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]

    public class DateTimePropertyStackValues
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public DateTime Start { get; set; }
        [DataMember]
        public DateTime End { get; set; }
        [DataMember]
        public long BucketCount { get; set; }
        [DataMember]
        public List<List<DateTimePropertyStackValue>> Bars { get; set; }
    }

}
