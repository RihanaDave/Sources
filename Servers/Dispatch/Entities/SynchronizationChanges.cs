using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public class SynchronizationChanges
    {
        [DataMember]
        public long[] SynchronizedConceptsIDs { set; get; }
        [DataMember]
        public long[] StayUnsynchronizeConceptsIDs { set; get; }
    }
}
