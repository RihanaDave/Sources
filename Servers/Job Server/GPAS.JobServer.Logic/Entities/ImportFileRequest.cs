using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.DataImport.DataMapping.SemiStructured;
using System.Runtime.Serialization;

namespace GPAS.JobServer.Logic.SemiStructuredDataImport
{
    [DataContract]
    public class ImportFileRequest
    {
        [DataMember]
        public byte[] mapping { set; get; }

        [DataMember]
        public string path { set; get; }
    }
}
