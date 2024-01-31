using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.Formula
{
    [DataContract]
    public class PropertyValueRangeStatistics
    {
        [DataMember]
        public double MaxValue { get; set; }
        [DataMember]
        public double MinValue { get; set; }
        [DataMember]
        public string NumericPropertyTypeUri { get; set; }
        [DataMember]
        public long BucketCount { get; set; }
        [DataMember]
        public List<PropertyValueRangeStatistic> Bars { get; set; }



    }
}
