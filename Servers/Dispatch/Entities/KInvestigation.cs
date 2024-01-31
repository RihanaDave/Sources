using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public class KInvestigation: InvestigationInfo
    {
        [DataMember]
        public byte[] InvestigationImage { set; get; }

        [DataMember]
        public byte[] InvestigationStatus { set; get; }
    }
}
