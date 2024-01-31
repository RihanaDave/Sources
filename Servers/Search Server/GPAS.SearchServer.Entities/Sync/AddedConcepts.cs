using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.Sync
{
    [DataContract]
    public class AddedConcepts
    {
        [DataMember]
        public SearchObject[] AddedObjects { set; get; }

        [DataMember]
        public SearchProperty[] AddedProperties { set; get; }
        
        [DataMember]
        public SearchRelationship[] AddedRelationships { set; get; }

        [DataMember]
        public SearchMedia[] AddedMedias { get; set; }
    }
}
