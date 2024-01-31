using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.AccessControl
{
    [DataContract]
    public class AuthorizationParametters
    {
        [DataMember]
        public List<string> permittedGroupNames { set; get; }

        [DataMember]
        public List<string> readableClassifications { set; get; }
    }
}
