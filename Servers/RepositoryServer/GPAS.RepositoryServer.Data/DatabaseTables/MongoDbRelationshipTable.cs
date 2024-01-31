using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Data.DatabaseTables
{
  public  class MongoDbRelationshipTable
    {
        public static readonly string tableName = "dbrelationship";
        public static readonly string id = "Id";
        public static readonly string target = "Target";
        public static readonly string source = "Source";
        public static readonly string timebegin = "TimeBegin";
        public static readonly string timeend = "TimeEnd";
        public static readonly string typeuri = "TypeUri";
        public static readonly string description = "Description";
        public static readonly string direction = "Direction";
        public static readonly string dsid = "DataSourceID";
    }
}
