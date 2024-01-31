using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public class DataSourceCollection
    {
        public static readonly string Name = "DataSource_Collection";
        public static readonly string ConfigSetName = "DataSource_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrDataSourceURL"];

    }
}
