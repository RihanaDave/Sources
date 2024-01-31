using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Data.DatabaseTables
{
    public class MongoDbPropertyTable
    {
        public static readonly string tableName = "dbproperty";
        public static readonly string id = "Id";
        public static readonly string typeuri = "TypeUri";
        public static readonly string value = "Value";
        public static readonly string objectid = "ObjectId";
        public static readonly string dsid = "DataSourceID";
    }
}
