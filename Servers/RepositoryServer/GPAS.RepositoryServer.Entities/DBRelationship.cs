using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBRelationship: IComparable
    {
        [BsonId]
        public ObjectId _id;

        [DataMember]
        public DBObject Source { set; get; }

        [DataMember]
        public DBObject Target { set; get; }

        [DataMember]
        public long Id { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }
        [DataMember]
        public DateTime? TimeBegin { set; get; }

        [DataMember]
        public DateTime? TimeEnd { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public string TypeURI { set; get; }

        [DataMember]
        public RepositoryLinkDirection Direction { set; get; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherPersonHobby = obj as DBRelationship;
            return Id.CompareTo(otherPersonHobby.Id);
        }
    }
}
