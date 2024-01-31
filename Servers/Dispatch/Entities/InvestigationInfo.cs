using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public class InvestigationInfo
    {
        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public string Title { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public string CreatedBy { set; get; }

        [DataMember]
        public string CreatedTime { set; get; }
    }
}
