using GPAS.DataSynchronization.SynchronizingConcepts;
using GPAS.DataSynchronization.SynchronizingConcepts.Counting;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.DataSynchronization
{
    public partial class Synchronizer
    {
        public ProcessLogger Logger { get; set; }
        public int ConcurrencyLevel { get; set; }
        public int BatchSize { get; set; }
        public int MaximumRetryTimes { get; set; }
        public ISynchronizationAdaptor Adaptor { get; private set; }

        public Synchronizer(ISynchronizationAdaptor adaptor, ProcessLogger logger = null)
        {
            Adaptor = adaptor ?? throw new ArgumentNullException(nameof(adaptor));
            Logger = logger;
            ConcurrencyLevel = 8;
            BatchSize = 1000;
            MaximumRetryTimes = 4;
        }

        public async Task Synchronize(SynchronizingConceptsBase synchronizingConcepts)
        {
            if (synchronizingConcepts == null)
            {
                throw new ArgumentNullException(nameof(synchronizingConcepts));
            }
            if (synchronizingConcepts is UnsynchronizeConcepts && ConcurrencyLevel > 1)
            {
                ConcurrencyLevel = 1;
                WriteLog($"Synchronizing unsync. {Adaptor.ConceptsTypeTitle}s in concurrent mode will start concurrent batches that try sync. unique data, while all of them done of fail! 'Concurrency Level' changed to '1'");
            }

            if (synchronizingConcepts is SpecificConcepts)
            {
                if ((synchronizingConcepts as SpecificConcepts).LoadedConcepts == null)
                    throw new ArgumentNullException("synchronizingConcepts.LoadedConcepts");
                (synchronizingConcepts as SpecificConcepts).LoadedConcepts.SetCachedConceptIDsAsNotSynchronize();
                if ((synchronizingConcepts as SpecificConcepts).LoadedConcepts.NotSynchronizeIDs.Count == 0)
                {
                    WriteLog($"No {Adaptor.ConceptsTypeTitle} specified as 'Not-Synchronized'");
                    return;
                }
                await SynchronizeIndexesForSpecificConcepts(synchronizingConcepts as SpecificConcepts);
                if (!(synchronizingConcepts as SpecificConcepts).IsContinousSynchronization)
                {
                    Adaptor.FinalizeContinousSynchronization();
                }
            }
            else
            {
                WriteLog($"Synchronizing {Adaptor.ConceptsTypeTitle}s with concurrency level {ConcurrencyLevel} and Batch size {BatchSize}...");
                Counter counter = new Counter(synchronizingConcepts, Adaptor);
                List<Task> syncTasks = new List<Task>();
                //
                do
                {
                    SynchronizingConceptIDsBatch batch = await counter.GetNextStepIDs(BatchSize);
                    if (batch.Size > 0)
                    {
                        syncTasks.Add(SynchronizeIndexesByID(batch));
                    }
                    else
                    {
                        break;
                    }
                } while (syncTasks.Count < ConcurrencyLevel);
                while (syncTasks.Count > 0)
                {
                    Task finishedSyncTask = null;
                    try
                    {
                        finishedSyncTask = await Task.WhenAny(syncTasks);
                        await finishedSyncTask;
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex);
                    }
                    finally
                    {
                        if (finishedSyncTask != null)
                        {
                            syncTasks.Remove(finishedSyncTask);
                        }
                    }

                    SynchronizingConceptIDsBatch batch = await counter.GetNextStepIDs(BatchSize);
                    if (batch.Size > 0)
                    {
                        syncTasks.Add(SynchronizeIndexesByID(batch));
                    }
                }
                Adaptor.FinalizeContinousSynchronization();
            }
            WriteLog($"Synchronization completed. Synchronized: {SynchronizedConceptsCount} | Unable to synchronize: {StayNotSynchronizeConceptsCount}");
        }

        private string retrieveDuration = string.Empty;

        private async Task SynchronizeIndexesByID(SynchronizingConceptIDsBatch batch)
        {
            CachedConcepts processingConcepts = await TryRetrieveConcepts(batch);

            if (processingConcepts == null)
            {
                WriteLog($"Unable to retrieve {batch.Description}; Last try duration: {retrieveDuration}");
            }
            else
            {
                processingConcepts.SetCachedConceptIDsAsNotSynchronize();
                if (processingConcepts.NotSynchronizeIDs.Count <= 0)
                {
                    WriteLog($"Nothing to retrieve with given ID(s) ({batch.Description}); Duration: {retrieveDuration}");
                }
                else
                {
                    TrySynchronizeCachedConcepts(batch, processingConcepts);
                    AppendStatistics(processingConcepts);
                    await Adaptor.FinalizeSynchronization(processingConcepts);
                }
            }
        }

        private async Task SynchronizeIndexesForSpecificConcepts(SpecificConcepts syncConcepts)
        {
            SynchronizingConceptIDsBatch batch = new SynchronizingConceptIDsBatch(syncConcepts.LoadedConcepts.NotSynchronizeIDs);
            TrySynchronizeCachedConcepts(batch, syncConcepts.LoadedConcepts);
            AppendStatistics(syncConcepts.LoadedConcepts);
            await Adaptor.FinalizeSynchronization(syncConcepts.LoadedConcepts);
        }

        private readonly object AppendStatistics_LockObject = new object();
        public long SynchronizedConceptsCount { get; private set; }
        public long StayNotSynchronizeConceptsCount { get; private set; }

        private void AppendStatistics(CachedConcepts processingConcepts)
        {
            lock (AppendStatistics_LockObject)
            {
                SynchronizedConceptsCount += processingConcepts.SynchronizedIDs.Count;
                StayNotSynchronizeConceptsCount += processingConcepts.NotSynchronizeIDs.Count;
            }
        }

        /// <summary></summary>
        /// <returns>Returns retrieve results or 'null' that means unable to retrieve</returns>
        private async Task<CachedConcepts> TryRetrieveConcepts(SynchronizingConceptIDsBatch batch)
        {
            int retrieveTryTimes = 0;
            DateTime retrieveStartTimeStamp;
            CachedConcepts retrieveResult = null;
            while (true)
            {
                retrieveStartTimeStamp = DateTime.Now;
                try
                {
                    retrieveResult = await Adaptor.RetrieveConceptsByIDs(batch.GetIDs());
                    break;
                }
                catch (Exception ex)
                {
                    string exDetail = GetExceptionDetails(ex);
                    string duration = GetDuration(retrieveStartTimeStamp);
                    WriteLog($"Retrievation failed; Duration: {duration}; Try {retrieveTryTimes + 1} of {MaximumRetryTimes + 1};\r\nException:{exDetail}");

                    retrieveTryTimes++;

                    if (retrieveTryTimes <= MaximumRetryTimes)
                    {
                        WaitAccordingRetryTime(retrieveTryTimes);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            retrieveDuration = GetDuration(retrieveStartTimeStamp);
            return retrieveResult;
        }
        private void TrySynchronizeCachedConcepts
            (SynchronizingConceptIDsBatch totalBatch, CachedConcepts cachedConcepts
            , SynchronizeRetrievedConceptsMode mode = SynchronizeRetrievedConceptsMode.AllRetrievedConcepts
            , long idOfConceptToSync = 0)
        {
            if (mode == SynchronizeRetrievedConceptsMode.SingleRetrievedConcept
                && idOfConceptToSync <= 0)
            {
                throw new InvalidOperationException("An ID must specified in 'SingleRetrievedConcept' mode");
            }

            int syncTryTimes = 0;
            DateTime syncStartTimeStamp;
            int numberOfUnsyncConceptsBeforeProcess = cachedConcepts.NotSynchronizeIDs.Count;
            while (true)
            {
                syncStartTimeStamp = DateTime.Now;
                try
                {
                    if (mode == SynchronizeRetrievedConceptsMode.AllRetrievedConcepts)
                    {
                        Adaptor.SynchronizeAllCachedConcepts(cachedConcepts);
                        cachedConcepts.SetAllIDsAsSynchronized();
                        string syncDuration = GetDuration(syncStartTimeStamp);
                        WriteLog($"{numberOfUnsyncConceptsBeforeProcess} {Adaptor.ConceptsTypeTitle}(s) Synnchronized ({totalBatch.Description}); Retrieve: {retrieveDuration}, Sync: {syncDuration}");
                    }
                    else if (mode == SynchronizeRetrievedConceptsMode.SingleRetrievedConcept)
                    {
                        if (cachedConcepts.IsConceptSynchronizedBefore(idOfConceptToSync))
                        {
                            WriteLog($"ID '{idOfConceptToSync}' was synchronized before!");
                            break;
                        }

                        Adaptor.SynchronizeSpecificCachedConcept(idOfConceptToSync, cachedConcepts);
                        cachedConcepts.SetConceptAsSynchronized(idOfConceptToSync);
                        string syncDuration = GetDuration(syncStartTimeStamp);
                        WriteLog($"ID '{idOfConceptToSync}' synnchronized; Sync: {syncDuration}");
                    }
                    else
                        throw new NotSupportedException("Mode is not supported");
                    break;
                }
                catch (Exception ex)
                {
                    string exDetail = GetExceptionDetails(ex);
                    string syncDuration = GetDuration(syncStartTimeStamp);
                    if (mode == SynchronizeRetrievedConceptsMode.AllRetrievedConcepts)
                    {
                        WriteLog($"{numberOfUnsyncConceptsBeforeProcess} {Adaptor.ConceptsTypeTitle}(s) synchronizing failed ({totalBatch.Description}); Duration: {syncDuration}; Try {syncTryTimes + 1} of {MaximumRetryTimes + 1};\r\nException:{exDetail}");
                    }
                    else if (mode == SynchronizeRetrievedConceptsMode.SingleRetrievedConcept)
                    {
                        WriteLog($"Synchronizing ID '{idOfConceptToSync}' failed; Duration: {syncDuration}; Try {syncTryTimes + 1} of {MaximumRetryTimes + 1};\r\nException:{exDetail}");
                    }

                    syncTryTimes++;

                    if (syncTryTimes <= MaximumRetryTimes)
                    {
                        WaitAccordingRetryTime(syncTryTimes);
                    }
                    else if (mode == SynchronizeRetrievedConceptsMode.AllRetrievedConcepts
                            && (cachedConcepts.SynchronizedIDs.Count + cachedConcepts.NotSynchronizeIDs.Count) > 1)
                    {
                        WriteLog($"Try Synchronizing {Adaptor.ConceptsTypeTitle}s one-by-one... (Total Retrieve duration was: {retrieveDuration})");
                        long[] currentNotSyncIDs = new long[cachedConcepts.NotSynchronizeIDs.Count];
                        cachedConcepts.NotSynchronizeIDs.CopyTo(currentNotSyncIDs);
                        foreach (long conceptID in currentNotSyncIDs)
                        {
                            TrySynchronizeCachedConcepts(totalBatch, cachedConcepts, SynchronizeRetrievedConceptsMode.SingleRetrievedConcept, conceptID);
                        }
                        WriteLog($"One-by-one Synchronization completed; IDs: {totalBatch.Description}, Synchronized: {cachedConcepts.SynchronizedIDs.Count}, Stay unsync.: {cachedConcepts.NotSynchronizeIDs.Count}");
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void WaitAccordingRetryTime(int retryTimes)
        {
            int waitMiliseconds = (int)(Math.Pow(2, retryTimes) * 250);
            WriteLog(string.Format("Wait {0} miliseconds...", waitMiliseconds));
            Task.Delay(waitMiliseconds).Wait();
            WriteLog("Retry...");
        }

        private static string GetExceptionDetails(Exception ex)
        {
            ExceptionDetailGenerator detialGen = new ExceptionDetailGenerator();
            string exDetail = detialGen.GetDetails(ex);
            return exDetail;
        }

        private string GetDuration(DateTime startTimeStamp)
        {
            TimeSpan processDuration = DateTime.Now - startTimeStamp;
            return processDuration.ToString(@"h\:mm\:ss\.fff");
        }

        private void WriteLog(string message)
        {
            if (Logger != null)
            {
                Logger.WriteLog(message);
            }
        }
        private void WriteLog(Exception ex)
        {
            if (Logger != null)
            {
                Logger.WriteLog(ex);
            }
        }
    }
}
