using GPAS.StatisticalQuery.ResultNode;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery
{
    [DataContract]
    public class PropertyValueStatistics
    {
        [DataMember]
        public List<PropertyValueStatistic> Results { get; set; }
        [DataMember]
        public int TotalResultsCount { get; set; }
    }
}
