using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]
    public class PropertyBarValues
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public double Start { get; set; }
        [DataMember]
        public double End { get; set; }
        [DataMember]
        public string Unit { get; set; }
        [DataMember]
        public long BucketCount { get; set; }
        [DataMember]
        public List<PropertyBarValue> Bars { get; set; }
    }
}
