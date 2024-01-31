using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts.SearchAroundResult
{
    [DataContract]
    public class EventBasedResultsPerSearchedObjects
    {
        [DataMember]
        public long SearchedObjectID { set; get; }

        [DataMember]
        public EventBasedNotLoadedResult[] NotLoadedResults { set; get; }
    }
}