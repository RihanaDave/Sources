using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
    [DataContract]
    public class DocumentBasedSearchResult : BaseSearchResult
    {
        [DataMember]
        public long FileSize
        {
            get;
            set;
        }

        [DataMember]
        public string TypeURI
        {
            get;
            set;
        }


        [DataMember]
        public string FileName
        {
            get;
            set;
        }

        [DataMember]
        public TextResult TextResult
        {
            get;
            set;
        }
    }
}
