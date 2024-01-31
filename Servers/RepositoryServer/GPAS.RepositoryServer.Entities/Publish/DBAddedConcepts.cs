using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBAddedConcepts
    {
        [DataMember]
        public List<DBObject> AddedObjectList { set; get; }

        [DataMember]
        public List<DBProperty> AddedPropertyList { set; get; }

        [DataMember]
        public List<DBRelationship> AddedRelationshipList { set; get; }

        [DataMember]
        public List<DBMedia> AddedMediaList { set; get; }
    }
}