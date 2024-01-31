using RestSharp;
using System.Configuration;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public class FileCollection
    {
        public static readonly string Name = "File_Collection";
        public static readonly string ConfigSetName = "File_Collection";
        public static readonly string SolrUrl = ConfigurationManager.AppSettings["SolrFileURL"];
    }
}
