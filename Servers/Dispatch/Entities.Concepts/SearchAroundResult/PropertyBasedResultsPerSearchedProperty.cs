using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts.SearchAroundResult
{
    [DataContract]
    public class PropertyBasedResultsPerSearchedProperty
    {
        [DataMember]
        public KProperty SearchedProperty { set; get; }

        [DataMember]
        public KProperty[] LoadedResults { set; get; }

        [DataMember]
        public long[] NotLoadedResultPropertyIDs { set; get; }
    }
}