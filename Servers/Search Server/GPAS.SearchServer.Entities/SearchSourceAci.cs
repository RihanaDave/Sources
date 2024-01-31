using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
  public  class SearchDataSourceAci
    {
        [DataMember]
        public long dsid { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public int Permission { get; set; }
    }
}
