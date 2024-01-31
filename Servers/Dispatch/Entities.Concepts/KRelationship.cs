using System;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public class KRelationship
    {
        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public DateTime? TimeBegin { set; get; }

        [DataMember]
        public DateTime? TimeEnd { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public LinkDirection Direction { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }
    }
}
