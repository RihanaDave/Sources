using GPAS.SearchServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic.Synchronization
{
    internal class ResetProcessPropertyEqualityComparer : IEqualityComparer<AccessControled<SearchProperty>>
    {
        public bool Equals(AccessControled<SearchProperty> x, AccessControled<SearchProperty> y)
        {
            return x.ConceptInstance.OwnerObject.Id.Equals(y.ConceptInstance.OwnerObject.Id)
                && x.ConceptInstance.TypeUri.Equals(y.ConceptInstance.TypeUri)
                && x.ConceptInstance.Value.Equals(y.ConceptInstance.Value);
        }

        public int GetHashCode(AccessControled<SearchProperty> obj)
        {
            return obj.ConceptInstance.OwnerObject.Id.GetHashCode() ^ obj.ConceptInstance.TypeUri.GetHashCode() ^ obj.ConceptInstance.Value.GetHashCode();
        }
    }
}
