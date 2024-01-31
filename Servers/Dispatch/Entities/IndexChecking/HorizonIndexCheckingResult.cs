using System.Collections.Generic;

namespace GPAS.Dispatch.Entities.IndexChecking
{
    public class HorizonIndexCheckingResult
    {
        public bool ObjectIndexStatus { set; get; }
        
        public List<IndexingStatus> PropertiesIndexStatus { set; get; }

        public List<IndexingStatus> RelationsIndexStatus { set; get; }
    }
}
