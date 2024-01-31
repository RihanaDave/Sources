using GPAS.DataSynchronization;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Horizon.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Horizon.Logic.Synchronization
{
    internal class RelationshipsSynchronizationCachedConcepts : CachedConcepts
    {
        internal List<AccessControled<RelationshipBaseKlink>> RetrievedLinks { get; private set; }

        internal RelationshipsSynchronizationCachedConcepts(List<AccessControled<RelationshipBaseKlink>> retrievedLinks)
        {
            RetrievedLinks = retrievedLinks;
        }

        public override IEnumerable<long> GetCachedConceptIDs()
        {
            return RetrievedLinks.Select(r => r.ConceptInstance.Relationship.Id);
        }
    }
}
