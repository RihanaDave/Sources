using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.AccessControl
{
    [DataContract]
    public class ClassificationEntry
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string IdentifierString { get; set; }

    }
}
