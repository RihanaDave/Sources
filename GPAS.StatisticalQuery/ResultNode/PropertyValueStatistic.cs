using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.ResultNode
{
    [DataContract]
    public class PropertyValueStatistic
    {
        [DataMember]
        public string PropertyValue { get; set; }
        [DataMember]
        public long Frequency { get; set; }
    }
}
