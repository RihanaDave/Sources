using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Data.DatabaseTables
{
   public class RelationshipTable
    {
        public static readonly string tableName = "dbrelationship";
        public static readonly string id = "id";
        public static readonly string target = "target";
        public static readonly string source = "source";
        public static readonly string timebegin = "timebegin";
        public static readonly string timeend = "timeend";
        public static readonly string typeuri = "typeuri";
        public static readonly string description = "description";
        public static readonly string direction = "direction";
        public static readonly string dsid = "dsid";
    }
}
