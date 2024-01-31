using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts.SearchAroundResult
{
    [DataContract]
    public class EventBasedNotLoadedResult
    {
        [DataMember]
        public long FirstRealationshipID { set; get; }

        [DataMember]
        public long SecondRealationshipID { set; get; }

        [DataMember]
        public long TargetObjectID { set; get; }
    }
}