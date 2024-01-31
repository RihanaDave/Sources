namespace GPAS.DataSynchronization.SynchronizingConcepts
{
    public class SpecificConcepts : SynchronizingConceptsBase
    {
        public CachedConcepts LoadedConcepts { get; set; }
        public bool IsContinousSynchronization { get; set; }
    }
}
