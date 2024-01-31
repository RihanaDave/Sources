using System.Collections.Generic;

namespace GPAS.DataSynchronization.SynchronizingConcepts
{
    public class ConceptsWithIDsInSpecificCollection : SynchronizingConceptsBase
    {
        public List<long> IDs { get; set; }
    }
}
