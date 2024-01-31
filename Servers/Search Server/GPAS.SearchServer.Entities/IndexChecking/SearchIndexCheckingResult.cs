using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.IndexChecking
{
    [DataContract]
    public class SearchIndexCheckingResult
    {
        public SearchIndexCheckingResult()
        {
            PropertiesIndexStatus= new List<IndexingStatus>();
            RelationsIndexStatus = new List<IndexingStatus>();
        }

        [DataMember]
        public bool ObjectIndexStatus { set; get; }

        [DataMember]
        public bool? DocumentIndexStatus { set; get; }

        [DataMember]
        public bool? ImageIndexStatus { set; get; }

        [DataMember]
        public string ImageIndexCount { set; get; }

        [DataMember]
        public List<IndexingStatus> PropertiesIndexStatus { set; get; }

        [DataMember]
        public List<IndexingStatus> RelationsIndexStatus { set; get; }
    }
}
