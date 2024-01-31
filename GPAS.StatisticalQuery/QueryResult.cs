using GPAS.StatisticalQuery.ResultNode;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery
{
    [DataContract]
    public class QueryResult
    {
        [DataMember]
        public List<TypeBasedStatistic> ObjectTypePreview { get; set; }
        [DataMember]
        public List<TypeBasedStatistic> PropertyTypePreview { get; set; }
    }
}
