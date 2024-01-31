using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
   public class SearchObjectMaster
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public long MasterId { get; set; }

        [DataMember]
        public long[] ResolveTo { get; set; }
    }
}
