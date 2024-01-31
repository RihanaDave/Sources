using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataSynchronization.SynchronizingConcepts.Counting
{
    internal class Counter
    {
        SynchronizingConceptsBase SynchronizingConcepts;
        ISynchronizationAdaptor SynchronizationAdaptor;

        private CounterMode CountingMode;

        private long RangeCountingNextStepFirstID;
        private long RangeCountingLastID;

        private List<long> CollectionCountingNotAssignedIDs;

        internal Counter(SynchronizingConceptsBase synchronizingConcepts, ISynchronizationAdaptor adaptor)
        {
            SynchronizingConcepts = synchronizingConcepts ?? throw new ArgumentNullException(nameof(synchronizingConcepts));
            SynchronizationAdaptor = adaptor ?? throw new ArgumentNullException(nameof(adaptor));
            PrepareCountingMode();
        }

        private void PrepareCountingMode()
        {
            Type syncConcptesType = SynchronizingConcepts.GetType();
            if (syncConcptesType == typeof(AllStoredConcpts))
            {
                CountingMode = CounterMode.RangeCounting;
                RangeCountingNextStepFirstID = 1;
                RangeCountingLastID = long.MinValue;
            }
            else if (syncConcptesType == typeof(ConceptsWithIDsInSpecificRange))
            {
                CountingMode = CounterMode.RangeCounting;
                RangeCountingNextStepFirstID = (SynchronizingConcepts as ConceptsWithIDsInSpecificRange).FirstID;
                RangeCountingLastID = (SynchronizingConcepts as ConceptsWithIDsInSpecificRange).LastID;

                if (RangeCountingNextStepFirstID > RangeCountingLastID)
                {
                    throw new InvalidOperationException("First ID > Last ID");
                }
            }
            else if (syncConcptesType == typeof(UnsynchronizeConcepts))
            {
                CountingMode = CounterMode.CollectionCounting;
            }
            else if (syncConcptesType == typeof(ConceptsWithIDsInSpecificCollection))
            {
                CountingMode = CounterMode.CollectionCounting;
                CollectionCountingNotAssignedIDs = (SynchronizingConcepts as ConceptsWithIDsInSpecificCollection).IDs;
            }
            else
            {
                throw new NotSupportedException("Specified 'Synchronizing Concpets' is not supported");
            }
        }

        public async Task<SynchronizingConceptIDsBatch> GetNextStepIDs(int stepSize)
        {
            SynchronizingConceptIDsBatch result = new SynchronizingConceptIDsBatch();
            if (CountingMode == CounterMode.RangeCounting)
            {
                if (RangeCountingLastID == long.MinValue
                    && SynchronizingConcepts is AllStoredConcpts)
                {
                    RangeCountingLastID = await SynchronizationAdaptor.RetrieveLastAssignedID();
                    if (RangeCountingLastID < 1)
                    {
                        return result;
                    }
                }
                while (result.Size < stepSize && RangeCountingNextStepFirstID <= RangeCountingLastID)
                {
                    result.AppendID(RangeCountingNextStepFirstID);
                    RangeCountingNextStepFirstID++;
                }
            }
            else if (CountingMode == CounterMode.CollectionCounting)
            {
                if (SynchronizingConcepts is UnsynchronizeConcepts)
                {
                    result.AppendIDs(await SynchronizationAdaptor.GetOldestUnsyncConceptIDs(stepSize));
                }
                else
                {
                    result.AppendIDs(CollectionCountingNotAssignedIDs.Take(stepSize));
                    CollectionCountingNotAssignedIDs.RemoveRange(0, stepSize);
                }
            }
            else
            {
                throw new NotSupportedException("Specified 'Counting Mode' is not supported");
            }
            return result;
        }
    }
}
