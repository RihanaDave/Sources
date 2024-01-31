using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public class RelationshipBaseKlink
    {
        [DataMember]
        public KRelationship Relationship { set; get; }

        [DataMember]
        public KObject Source { set; get; }

        [DataMember]
        public KObject Target { set; get; }

        [DataMember]
        public string TypeURI { set; get; }
    }
}
