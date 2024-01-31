using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities
{
    [DataContract]
    public class EventBasedLinksID
    {
        [DataMember]
        public long firstLinkID;
        [DataMember]
        public long secondLinkID;
    }
}
