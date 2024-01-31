using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public class EventBaseKlink
    {
        [DataMember]
        public RelationshipBaseKlink SourceRelationship { set; get; }

        [DataMember]
        public RelationshipBaseKlink TargetRelationship { set; get; }
    }
}
