using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Horizon.Entities
{
    [DataContract]
    public class IndexModel
    {
        [DataMember]
        public string NodeType { set; get; }

        [DataMember]
        public string[] PropertiesType { set; get; }
    }
}
