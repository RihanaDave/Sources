using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBObject: IComparable
    {
        [BsonId]
        public ObjectId _id;

        [DataMember]
        public string TypeUri { set; get; }
        
        [DataMember]
        public long? LabelPropertyID { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public bool IsGroup { set; get; }

        [DataMember]
        public long? ResolvedTo { set; get; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherPersonHobby = obj as DBObject;
            return Id.CompareTo(otherPersonHobby.Id);
        }
    }
}
