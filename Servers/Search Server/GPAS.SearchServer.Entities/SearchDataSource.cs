using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
  public  class SearchDataSource
    {

        [DataMember]
        public long id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public string CreateBy { get; set; }

        [DataMember]
        public string CreateTime{ get; set; }

        [DataMember]
        public string ClassificationIdentifier { get; set; }

        [DataMember]
        public string Administrators { get; set; }
    }
}
