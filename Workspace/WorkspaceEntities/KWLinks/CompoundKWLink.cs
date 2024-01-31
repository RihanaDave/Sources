using System.Collections.Generic;

namespace GPAS.Workspace.Entities
{
    public abstract class CompoundKWLink : KWLink
    {
        public abstract bool ContainsLink(KWLink kwlinkToCompair);

        public abstract HashSet<long> GetAllLinks();
    }
}