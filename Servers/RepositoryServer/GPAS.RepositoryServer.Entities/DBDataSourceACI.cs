using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Entities
{
   public  class DBDataSourceACI: IComparable
    {
        [BsonId]
        public ObjectId _id;

        public long dsid { get; set; }

        public string  groupname { get; set; }

        public int permission { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var otherPersonHobby = obj as DBDataSourceACI;
            return dsid.CompareTo(otherPersonHobby.dsid);
        }
    }
}
