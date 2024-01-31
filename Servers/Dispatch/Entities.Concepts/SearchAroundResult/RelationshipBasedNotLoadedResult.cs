using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts.SearchAroundResult
{
    [DataContract]
    public class RelationshipBasedNotLoadedResult
    {
        [DataMember]
        public long RelationshipID { set; get; }

        [DataMember]
        public long TargetObjectID { set; get; }
    }
}