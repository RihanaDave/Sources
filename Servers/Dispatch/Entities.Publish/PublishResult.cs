using System;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Publish
{
    [DataContract]
   public class PublishResult
    {
        [DataMember]
        public bool HorizonServerSynchronized { set; get; }
        [DataMember]
        public bool SearchServerSynchronized { set; get; }
        [DataMember]
        public TimeSpan HorizonServerSyncDuration { get; set; }
        [DataMember]
        public TimeSpan SearchServerSyncDuration { get; set; }
        [DataMember]
        public TimeSpan RepositoryStoreDuration { get; set; }
    }
}
