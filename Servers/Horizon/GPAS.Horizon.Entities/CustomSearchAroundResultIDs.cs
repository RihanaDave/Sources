using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using System;
using System.Runtime.Serialization;

namespace GPAS.Horizon.Entities
{
    [DataContract]
    public class CustomSearchAroundResultIDs
    {
        [DataMember]
        public Guid SearchAroundStepGuid { get; set; }
        [DataMember]
        public RelationshipBasedResultsPerSearchedObjects[] RelationshipNotLoadedResultIDs;
        [DataMember]
        public EventBasedResultsPerSearchedObjects[] EventBasedNotLoadedResults;
    }
}
