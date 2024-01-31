using System.Configuration;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public class ResolveCollection
    {
        public static readonly string Name = "Resolve_Collection";
        public static readonly string ConfigSetName = "Resolve_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrResolveURL"];
    }
}
