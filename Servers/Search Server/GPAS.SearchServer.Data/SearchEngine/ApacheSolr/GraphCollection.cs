using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
   public class GraphCollection
    {
        public static readonly string Name = "Graph_Collection";
        public static readonly string ConfigSetName = "Graph_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrGraphURL"];
    }
}
