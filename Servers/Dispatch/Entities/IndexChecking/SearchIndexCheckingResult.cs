using System.Collections.Generic;

namespace GPAS.Dispatch.Entities.IndexChecking
{
    public class SearchIndexCheckingResult
    {
        public bool ObjectIndexStatus { set; get; }
        
        public bool? DocumentIndexStatus { set; get; }
        
        public bool? ImageIndexStatus { set; get; }

        public string ImageIndexCount { get; set; }
        
        public List<IndexingStatus> PropertiesIndexStatus { set; get; }
        
        public List<IndexingStatus> RelationsIndexStatus { set; get; }
    }
}
