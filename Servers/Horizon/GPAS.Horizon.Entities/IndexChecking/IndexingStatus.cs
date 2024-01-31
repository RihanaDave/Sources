using System.Runtime.Serialization;

namespace GPAS.Horizon.Entities.IndexChecking
{
    [DataContract]
    public class IndexingStatus
    {
        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public bool IndexStatus { set; get; }
    }
}
