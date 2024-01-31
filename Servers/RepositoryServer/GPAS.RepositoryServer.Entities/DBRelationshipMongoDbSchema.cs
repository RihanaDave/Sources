using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBRelationshipMongoDbSchema : IComparable
    {
        [BsonId]
        public ObjectId _id;

        [DataMember]
        public long Source { set; get; }

        [DataMember]
        public long Target { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public long DataSourceID { set; get; }

        [DataMember]
        public string TimeBegin { set; get; }

        [DataMember]
        public string TimeEnd { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public string TypeURI { set; get; }

       [DataMember]
       public long Direction { set; get; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherPersonHobby = obj as DBRelationshipMongoDbSchema;
            return Id.CompareTo(otherPersonHobby.Id);
        }
    }
}
