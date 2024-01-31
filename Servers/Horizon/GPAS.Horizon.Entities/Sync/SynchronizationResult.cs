using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities.Sync
{
    [DataContract]
    public class SynchronizationResult
    {
        [DataMember]
        public long[] SynchronizedConceptsIDs { set; get; }
        [DataMember]
        public long[] StayUnsynchronizeConceptsIDs { set; get; }
    }
}
