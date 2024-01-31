using System;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]

    public class DateTimePropertyBarValue
    {
        [DataMember]
        public long Count { get; set; }
        [DataMember]
        public DateTime Start { get; set; }
    }
}
