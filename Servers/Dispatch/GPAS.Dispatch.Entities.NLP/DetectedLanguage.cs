using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.NLP
{
    [DataContract]
    public class DetectedLanguage
    {
        [DataMember]
        public string LanguageName { get; set; }
        [DataMember]
        public int? Percent { get; set; }
    }
}
