using System;
using System.Collections.Generic;

namespace GPAS.DataSynchronization
{
    public abstract class CachedConcepts
    {
        public List<long> SynchronizedIDs { get; protected set; }
        public List<long> NotSynchronizeIDs { get; protected set; }
        public abstract IEnumerable<long> GetCachedConceptIDs();

        public void SetCachedConceptIDsAsNotSynchronize()
        {
            NotSynchronizeIDs = new List<long>(GetCachedConceptIDs());
            SynchronizedIDs = new List<long>(NotSynchronizeIDs.Count);
        }

        public void SetAllIDsAsSynchronized()
        {
            SynchronizedIDs.AddRange(NotSynchronizeIDs);
            NotSynchronizeIDs.Clear();
        }

        public bool IsConceptSynchronizedBefore(long conceptID)
        {
            if (SynchronizedIDs.Contains(conceptID))
            {
                return true;
            }
            else if (!NotSynchronizeIDs.Contains(conceptID))
            {
                throw new InvalidOperationException($"Concept '{conceptID}' is not cached before.");
            }
            return false;
        }

        public void SetConceptAsSynchronized(long conceptID)
        {
            SynchronizedIDs.Add(conceptID);
            NotSynchronizeIDs.Remove(conceptID);
        }
    }
}
