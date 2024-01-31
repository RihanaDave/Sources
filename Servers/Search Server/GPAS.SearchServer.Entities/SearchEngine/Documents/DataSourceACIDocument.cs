using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
  public class DataSourceAciDocument
    {
        public static readonly string Name = "DataSourceAci_Collection";
        public static readonly string ConfigSetName = "DataSourceAci_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrDataSourceAciURL"];

        public long dsid { get; set; }

        public string GroupName { get; set; }

        public long Permission { get; set; }
    }
}
