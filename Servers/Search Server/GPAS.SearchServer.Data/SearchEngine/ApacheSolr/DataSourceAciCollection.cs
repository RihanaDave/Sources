using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
 public   class DataSourceAciCollection
    {
        public static readonly string Name = "DataSourceAci_Collection";
        public static readonly string ConfigSetName = "DataSourceAci_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrDataSourceAciURL"];
    }
}
