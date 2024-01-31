using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
    public class SearchRelationship
    {
        [DataMember]
        public long Id { set; get; }
        [DataMember]
        public string TypeUri { set; get; }
        [DataMember]
        public long SourceObjectId { set; get; }
        [DataMember]
        public string SourceObjectTypeUri { set; get; }
        [DataMember]
        public long TargetObjectId { set; get; }
        [DataMember]
        public string TargetObjectTypeUri { set; get; }
        [DataMember]
        public long DataSourceID { get; set; }

        [DataMember]
        public int Direction { get; set; }
    }
}
