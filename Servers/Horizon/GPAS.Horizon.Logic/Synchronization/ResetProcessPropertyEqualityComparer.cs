using GPAS.Dispatch.Entities.Concepts;
using System.Collections.Generic;

namespace GPAS.Horizon.Logic.Synchronization
{
    internal class ResetProcessPropertyEqualityComparer : IEqualityComparer<KProperty>
    {
        public bool Equals(KProperty x, KProperty y)
        {
            return x.Owner.Id.Equals(y.Owner.Id)
                && x.TypeUri.Equals(y.TypeUri)
                && x.Value.Equals(y.Value);
        }

        public int GetHashCode(KProperty obj)
        {
            return obj.Owner.Id.GetHashCode() ^ obj.TypeUri.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }
}
