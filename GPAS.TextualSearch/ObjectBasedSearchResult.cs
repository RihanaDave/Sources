using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
    [DataContract]
    public class ObjectBasedSearchResult : BaseSearchResult
    {
        [DataMember]
        public string TypeURI
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
