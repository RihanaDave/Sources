using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts.SearchAroundResult
{
    [DataContract]
    public class RelationshipBasedResultsPerSearchedObjects
    {
        [DataMember]
        public long SearchedObjectID { set; get; }

        [DataMember]
        public RelationshipBasedNotLoadedResult[] NotLoadedResults { set; get; }
    }
}