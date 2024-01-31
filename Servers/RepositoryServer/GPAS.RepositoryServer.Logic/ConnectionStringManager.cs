using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic
{
    public class ConnectionStringManager
    {
        public static string GetRepositoryDBConnectionString()
        {
            string RepositorydbIP = ConfigurationManager.AppSettings["RepositorydbIP"];
            string RepositorydbPort = ConfigurationManager.AppSettings["RepositorydbPort"];
            string RepositorydbDatabase = ConfigurationManager.AppSettings["RepositorydbDatabase"];
            string RepositorydbUser = ConfigurationManager.AppSettings["RepositorydbUser"];
            string RepositorydbPassword = ConfigurationManager.AppSettings["RepositorydbPassword"];
            return $"Server={RepositorydbIP};Port={RepositorydbPort};Database={RepositorydbDatabase};User Id={RepositorydbUser};Password={RepositorydbPassword};No Reset On Close=true;";
        }
        public static string GetSystemDBConnectionString()
        {
            string RepositorydbIP = ConfigurationManager.AppSettings["RepositorydbIP"];
            string RepositorydbPort = ConfigurationManager.AppSettings["RepositorydbPort"];
            string RepositorydbUser = ConfigurationManager.AppSettings["RepositorydbUser"];
            string RepositorydbPassword = ConfigurationManager.AppSettings["RepositorydbPassword"];
            return $"Server={RepositorydbIP};Port={RepositorydbPort};Database=system;User Id={RepositorydbUser};Password={RepositorydbPassword};No Reset On Close=true;";
        }
    }
}
