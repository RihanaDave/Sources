using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBProperty: IComparable
    {
        [BsonId]
        public ObjectId _id;

        [DataMember]
        public string TypeUri { set; get; }

        [DataMember]
        public string Value { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public DBObject Owner { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }

        public long ObjectId { set; get; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherPersonHobby = obj as DBProperty;
            return Id.CompareTo(otherPersonHobby.Id);
        }
    }
}
