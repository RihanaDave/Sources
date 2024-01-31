using GPAS.StatisticalQuery.ResultNode;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery
{
    [DataContract]
    public class LinkTypeStatistics
    {
        [DataMember]
        public List<TypeBasedStatistic> LinkTypes { get; set; }
        [DataMember]
        public List<TypeBasedStatistic> LinkedObjectTypes { get; set; }
    }
}
