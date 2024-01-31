using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Publish
{
    [DataContract]
    public class SynchronizationResult
    {
        [DataMember]
        public bool IsCompletelySynchronized { get; set; }
        [DataMember]
        public string SyncronizationLog { get; set; }
    }
}
