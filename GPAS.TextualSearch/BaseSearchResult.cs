using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
    [DataContract]
    [KnownType(typeof(DocumentBasedSearchResult))]
    [KnownType(typeof(ObjectBasedSearchResult))]
    public abstract class BaseSearchResult
    {
        [DataMember]
        public long FoundNumber
        {
            get;
            set;
        }

        [DataMember]
        public long TotalRow
        {
            get;
            set;
        }

        [DataMember]
        public long ObjectId
        {
            get;
            set;
        }
    }
}
