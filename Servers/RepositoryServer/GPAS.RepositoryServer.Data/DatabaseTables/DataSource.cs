using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Data.DatabaseTables
{
  public class DataSource
    {
        public static readonly string tableName = "dbdatasource";
        public static readonly string id = "id";
        public static readonly string dsname = "dsname";
        public static readonly string description = "description";
        public static readonly string classification = "classification";
        public static readonly string sourceType = "sourcetype";
        public static readonly string createdBy = "createdBy";
        public static readonly string createdTime = "createdTime";
    }
}
