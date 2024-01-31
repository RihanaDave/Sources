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
  public   class DBDataSource: IComparable
    {
        [BsonId]
        public ObjectId _id;

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public string dsname { set; get; }

        [DataMember]
        public string description { set; get; }

        [DataMember]
        public string classification { set; get; }

        [DataMember]
        public int sourcetype { set; get; }

        [DataMember]
        public string createdBy { set; get; }

        [DataMember]
        public DateTime createdTime { set; get; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherPersonHobby = obj as DBDataSource;
            return Id.CompareTo(otherPersonHobby.Id);
        }
    }
}
