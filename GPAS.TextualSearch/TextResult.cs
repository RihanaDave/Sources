using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
    [DataContract]
   public class TextResult
    {
        [DataMember]
        public List<string> PartOfText
        {
            get;
            set;
        }
    }
}
