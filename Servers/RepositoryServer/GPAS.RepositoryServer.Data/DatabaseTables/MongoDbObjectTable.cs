using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Data.DatabaseTables
{
   public class MongoDbObjectTable
    {
        public static readonly string tableName = "dbobject";
        public static readonly string id = "Id";
        public static readonly string labelPropertyID = "LabelPropertyID";
        public static readonly string typeuri = "TypeUri";
        public static readonly string isgroup = "IsGroup";
        public static readonly string resolvedto = "ResolvedTo";
    }
}
