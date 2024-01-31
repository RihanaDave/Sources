using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Horizon.Entities.IndexChecking
{
    [DataContract]
    public class HorizonIndexCheckingResult
    {
        public HorizonIndexCheckingResult()
        {
            ObjectIndexStatus = false;
            PropertiesIndexStatus = new List<IndexingStatus>();
            RelationsIndexStatus = new List<IndexingStatus>();
        }

        [DataMember]
        public bool ObjectIndexStatus { set; get; }

        [DataMember]
        public List<IndexingStatus> PropertiesIndexStatus { set; get; }

        [DataMember]
        public List<IndexingStatus> RelationsIndexStatus { set; get; }
    }
}
