using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]
    public class TypeBasedStatistic
    {
        [DataMember]
        public string TypeUri { get; set; }
        [DataMember]
        public long Frequency { get; set; }
    }
}
