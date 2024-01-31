using GPAS.Logger;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.DataSynchronization
{
    public interface ISynchronizationAdaptor
    {
        string ConceptsTypeTitle { get; }

        Task<long> RetrieveLastAssignedID();
        Task<List<long>> GetOldestUnsyncConceptIDs(int stepSize);
        
        Task<CachedConcepts> RetrieveConceptsByIDs(IEnumerable<long> IDsToSync);
        void SynchronizeAllCachedConcepts(CachedConcepts cachedConcepts);
        void SynchronizeSpecificCachedConcept(long conceptID, CachedConcepts cachedConcepts);
        Task FinalizeSynchronization(CachedConcepts cachedConcepts);

        void FinalizeContinousSynchronization();
    }
}