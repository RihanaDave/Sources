using GPAS.Dispatch.Entities.Concepts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Publish
{
    [DataContract]
    public class AddedConcepts
    {
        [DataMember]
        public List<KObject> AddedObjects { set; get; }

        [DataMember]
        public List<KProperty> AddedProperties { set; get; }

        [DataMember]
        public List<RelationshipBaseKlink> AddedRelationships { set; get; }

        [DataMember]
        public List<KMedia> AddedMedias { set; get; }
    }
}