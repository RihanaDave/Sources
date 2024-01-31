using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public struct ImportFileRequest
    {
        [DataMember]
        public byte[] mapping { set; get; }

        [DataMember]
        public string path { set; get; }
    }
}
