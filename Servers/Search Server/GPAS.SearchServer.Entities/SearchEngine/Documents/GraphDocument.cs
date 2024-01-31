using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
  public  class GraphDocument
    {
        public static readonly string Name = "Graph_Collection";
        public static readonly string ConfigSetName = "Graph_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrGraphURL"];

        public long dsid { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string TimeCreated { get; set; }

        public string GraphImage { get; set; }

        public string GraphArrangement { get; set; }

        public long NodesCount { get; set; }
    }
}
