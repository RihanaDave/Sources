using GPAS.DataSynchronization;
using GPAS.DataSynchronization.SynchronizingConcepts;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Access.DataClient;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Sync;
using GPAS.Horizon.GraphRepository;
using GPAS.Logger;
using GPAS.PropertiesValidation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Horizon.Logic.Synchronization
{
    public partial class IndexingProvider
    {
        private ProcessLogger logger;
        private bool IsStable;
        private string stabilityStatusFilePath;

        private static readonly object UseStabilityStatusFile_LockObject = new object();

        public bool ApplyOntologyLastChanges { get; set; }
        public bool DeleteExistingIndexes { get; set; }
        public int ResetNumberOfConceptsToGetSequential { get; set; }
        public int ResetMaximumConcurrentSynchronizations { get; set; }
        public int SyncDefaultNumberOfConceptsToGetSequential { get; set; }
        public int MaximumRetryTimes { get; set; }

        public IndexingProvider()
        {
            ApplyOntologyLastChanges = false;
            DeleteExistingIndexes = false;

            string logFolderPath = ConfigurationManager.AppSettings["ManagerLogsFolderPath"];
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }
            string logFilePath = string.Format("{0}{1}", logFolderPath, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff"));
            logger = new ProcessLogger();
            logger.Initialization(logFilePath);

            try
            {
                ResetNumberOfConceptsToGetSequential = int.Parse(ConfigurationManager.AppSettings["ResetProcessDefaultNumberOfObjectsForGetSequential"]);
                ResetMaximumConcurrentSynchronizations = int.Parse(ConfigurationManager.AppSettings["ResetProcessDefaultMaximumConcurrentSynchronizations"]);
                MaximumRetryTimes = int.Parse(ConfigurationManager.AppSettings["MaximumRetryTimesOnFailureOfSingleBatchInResetProcess"]);
                SyncDefaultNumberOfConceptsToGetSequential = int.Parse(ConfigurationManager.AppSettings["SynchronizationProcessDefaultNumberOfObjects"]);
                stabilityStatusFilePath = ConfigurationManager.AppSettings["StablityStatusFilePath"];

                InitiateStablilityStatusFile();
            }
            catch (Exception ex)
            {
                WriteLog(ex);
                throw;
            }
        }

        private void DeleteRegisteredUnsyncConcepts()
        {
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();
                client.DeleteHorizonServerUnsyncConcepts();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }
        private static void ClearAclCache()
        {
            RetrieveDataClient retrieveClient = new RetrieveDataClient();
            retrieveClient.PrepairDataSourceCachedForReset();
        }

        public async Task ResetAllIndexes()
        {
            IsStable = false;
            SetStatusOfStablity(IsStable);

            ValidateParameters();
            if (DeleteExistingIndexes)
            {
                DeleteExistIndexes();
            }
            if (ApplyOntologyLastChanges)
            {
                UpdateSchemaWithLastVersionOfOntology();
            }

            ClearAclCache();
            DeleteRegisteredUnsyncConcepts();

            await ResetObjectIndexes();
            await ResetRelationshipIndexes();

            IsStable = true;
            SetStatusOfStablity(IsStable);
        }
        private async Task ResetObjectIndexes()
        {
            if (ResetMaximumConcurrentSynchronizations > 1)
            {
                ResetMaximumConcurrentSynchronizations = 1;
                WriteLog($"Parallel data synchronization currently is not supported by Horizon Server! 'Concurrency Level' changed to '1'");
            }
            ObjectsSynchronizationAdaptor adaptor = new ObjectsSynchronizationAdaptor();
            SynchronizingConceptsBase synchronizingConcepts = new AllStoredConcpts();
            Synchronizer synchronizer = new Synchronizer(adaptor, logger)
            {
                BatchSize = ResetNumberOfConceptsToGetSequential,
                ConcurrencyLevel = ResetMaximumConcurrentSynchronizations,
                MaximumRetryTimes = MaximumRetryTimes
            };
            await synchronizer.Synchronize(synchronizingConcepts);
        }
        private async Task ResetRelationshipIndexes()
        {
            if (ResetMaximumConcurrentSynchronizations > 1)
            {
                ResetMaximumConcurrentSynchronizations = 1;
                WriteLog($"Parallel data synchronization currently is not supported by Horizon Server! 'Concurrency Level' changed to '1'");
            }
            RelationshipsSynchronizationAdaptor adaptor = new RelationshipsSynchronizationAdaptor();
            SynchronizingConceptsBase synchronizingConcepts = new AllStoredConcpts();
            Synchronizer synchronizer = new Synchronizer(adaptor, logger)
            {
                BatchSize = ResetNumberOfConceptsToGetSequential,
                ConcurrencyLevel = ResetMaximumConcurrentSynchronizations,
                MaximumRetryTimes = MaximumRetryTimes
            };
            await synchronizer.Synchronize(synchronizingConcepts);
        }

        public async Task SynchronizeNotSyncIndexes()
        {
            IsStable = GetStatusOfStablity();
            if (!IsStable)
            {
                throw new InvalidOperationException("Server indexes status is not stable, a Reset process is required");
            }
            else
            {
                UpdateSchemaWithLastVersionOfOntology();
                await SynchronizeNotSyncObjectIndexes();
                await SynchronizeNotSyncRelationshipIndexes();
            }
        }
        private async Task SynchronizeNotSyncObjectIndexes()
        {
            IsStable = GetStatusOfStablity();
            if (!IsStable)
            {
                throw new InvalidOperationException("Server indexes status is not stable, a Reset process is required");
            }
            else
            {
                ObjectsSynchronizationAdaptor adaptor = new ObjectsSynchronizationAdaptor();
                Synchronizer synchronizer = new Synchronizer(adaptor, logger)
                {
                    BatchSize = SyncDefaultNumberOfConceptsToGetSequential,
                    ConcurrencyLevel = 1,
                    MaximumRetryTimes = MaximumRetryTimes
                };
                await synchronizer.Synchronize(new UnsynchronizeConcepts());
            }
        }
        private async Task SynchronizeNotSyncRelationshipIndexes()
        {
            IsStable = GetStatusOfStablity();
            if (!IsStable)
            {
                throw new InvalidOperationException("Server indexes status is not stable, a Reset process is required");
            }
            else
            {
                RelationshipsSynchronizationAdaptor adaptor = new RelationshipsSynchronizationAdaptor();
                Synchronizer synchronizer = new Synchronizer(adaptor, logger)
                {
                    BatchSize = SyncDefaultNumberOfConceptsToGetSequential,
                    ConcurrencyLevel = 1,
                    MaximumRetryTimes = MaximumRetryTimes
                };
                await synchronizer.Synchronize(new UnsynchronizeConcepts());
            }
        }

        public async Task<bool> SynchronizePublishChanges
            (AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts
            , long dataSourceID, bool isContinousPublish, ProcessLogger logger = null)
        {
            if (addedConcepts == null)
                throw new ArgumentNullException("addedConcepts");
            if (addedConcepts.AddedObjects == null)
                throw new ArgumentNullException("addedConcepts.AddedObjects");
            if (addedConcepts.AddedProperties == null)
                throw new ArgumentNullException("addedConcepts.AddedProperties");
            if (addedConcepts.AddedMedias == null)
                throw new ArgumentNullException("addedConcepts.AddedMedias");
            if (modifiedConcepts == null)
                throw new ArgumentNullException("modifiedConcepts");
            if (modifiedConcepts.ModifiedProperties == null)
                throw new ArgumentNullException("modifiedConcepts.ModifiedProperties");
            if (modifiedConcepts.DeletedMedias == null)
                throw new ArgumentNullException("modifiedConcepts.DeletedMedias");
            if (dataSourceID <= 0)
                throw new ArgumentOutOfRangeException(nameof(dataSourceID));
            
            var objSyncAdaptor = new ObjectsSynchronizationAdaptor();
            var objSynchronizer = new Synchronizer(objSyncAdaptor, logger);
            var syncObjects = new SpecificConcepts
            {
                LoadedConcepts = new ObjectsSynchronizationCachedConcepts
                    (addedConcepts.AddedObjects
                    , addedConcepts.AddedProperties
                    , modifiedConcepts.ModifiedProperties
                    ),
                IsContinousSynchronization = true
            };
            await objSynchronizer.Synchronize(syncObjects);
            
            var relSyncAdaptor = new RelationshipsSynchronizationAdaptor();
            var relSynchronizer = new Synchronizer(relSyncAdaptor, logger);
            var dataProvider = new DataChangeProvider();
            AddedConceptsWithAcl addedConceptsWithAcl = dataProvider.GetAddedConceptsWithAcl(addedConcepts, dataSourceID);
            var syncLinks = new SpecificConcepts
            {
                LoadedConcepts = new RelationshipsSynchronizationCachedConcepts(addedConceptsWithAcl.AddedRelationshipsWithAcl),
                IsContinousSynchronization = isContinousPublish
            };
            await relSynchronizer.Synchronize(syncLinks);

            if (objSynchronizer.StayNotSynchronizeConceptsCount == 0 && relSynchronizer.StayNotSynchronizeConceptsCount == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitiateStablilityStatusFile()
        {
            if (!File.Exists(stabilityStatusFilePath))
            {
                StreamWriter stabilityStream = new StreamWriter(stabilityStatusFilePath);
                lock (UseStabilityStatusFile_LockObject)
                {
                    stabilityStream.WriteLine(true.ToString(CultureInfo.InvariantCulture));
                    stabilityStream.Flush();
                    stabilityStream.Close();
                }
            }
        }
        private void SetStatusOfStablity(bool isStable)
        {
            StreamWriter stabilityStream = new StreamWriter(stabilityStatusFilePath);
            lock (UseStabilityStatusFile_LockObject)
            {
                stabilityStream.WriteLine(isStable.ToString(CultureInfo.InvariantCulture));
                stabilityStream.Flush();
                stabilityStream.Close();
            }
        }
        public bool GetStatusOfStablity()
        {

            if (File.Exists(stabilityStatusFilePath))
            {
                StreamReader stabilityStream = new StreamReader(stabilityStatusFilePath);
                bool isStable = bool.Parse(stabilityStream.ReadLine());
                stabilityStream.Close();
                return isStable;
            }
            else throw new InvalidOperationException("StabilityStatusFile not created.");
        }
        
        public void UpdateSchemaWithLastVersionOfOntology()
        {
            WriteLog("Synchronize ontology last changes...");
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            WriteLog("Ontology synchronized.");
        }
        public void DeleteExistIndexes()
        {
            WriteLog("Delete existing data indexes...");
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            IAccessClient accessClient = graphRepositoryProvider.GetNewSearchEngineClient();
            accessClient.OpenConnetion();
            accessClient.TruncateDatabase();
            WriteLog("Indexes were deleted.");
        }

        private void ValidateParameters()
        {
            if (ResetNumberOfConceptsToGetSequential < 1)
                throw new ArgumentOutOfRangeException(nameof(ResetMaximumConcurrentSynchronizations));
            if (ResetMaximumConcurrentSynchronizations < 1)
                throw new ArgumentOutOfRangeException(nameof(ResetMaximumConcurrentSynchronizations));
            if (MaximumRetryTimes < 0)
                throw new ArgumentOutOfRangeException(nameof(MaximumRetryTimes));
        }
        
        private void WriteLog(string message)
        {
            logger.WriteLog(message);
        }
        private void WriteLog(Exception ex)
        {
            logger.WriteLog(ex);
        }
    }
}
