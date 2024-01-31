using System.Configuration;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public class ObjectCollection
    {
        public static readonly string Name = "Object_Collection";
        public static readonly string ConfigSetName = "Object_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrObjectURL"];
    }
}
