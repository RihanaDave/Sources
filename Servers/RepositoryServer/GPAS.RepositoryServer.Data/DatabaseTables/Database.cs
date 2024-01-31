using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Data.DatabaseTables
{
   public class Database
    {
        public static readonly string databaseName = ConfigurationManager.AppSettings["RepositorydbDatabase"];
    }
}
