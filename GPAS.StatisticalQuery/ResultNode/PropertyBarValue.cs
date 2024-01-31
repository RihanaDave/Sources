using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]
    public class PropertyBarValue
    {
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public double Start { get; set; }
        [DataMember]
        public double End { get; set; }
    }
}
