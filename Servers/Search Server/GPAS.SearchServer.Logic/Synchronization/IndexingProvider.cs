using GPAS.AccessControl;
using GPAS.DataSynchronization;
using GPAS.DataSynchronization.SynchronizingConcepts;
using GPAS.Logger;
using GPAS.SearchServer.Access.DataClient;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.Sync;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic.Synchronization
{
    public class IndexingProvider
    {
        private ProcessLogger logger;
        private bool IsStable;
        private string stabilityStatusFilePath;
        private bool IsObjectRangeSpecified = false;
        private bool IsDataSourceRangeSpecified = false;

        private static readonly object UseStabilityStatusFile_LockObject = new object();
        private readonly Ontology.Ontology ontology = OntologyProvider.GetOntology();

        public bool DeleteExistingIndexes { get; set; }
        public int FirstObjectID { get; set; }
        public int FirstDataSourceID { get; set; }
        public int LastObjectID { get; set; }
        public int LastDataSourceID { get; set; }
        public int ResetNumberOfConceptsToGetSequential { get; set; }
        public int ResetMaximumConcurrentSynchronizations { get; set; }
        public int SyncDefaultNumberOfConceptsToGetSequential { get; set; }
        public int MaximumRetryTimes { get; set; }

        public IndexingProvider()
        {
            DeleteExistingIndexes = false;
            FirstObjectID = 1;
            FirstDataSourceID = 1;
            LastObjectID = 0;
            LastDataSourceID = 0;

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
                ResetNumberOfConceptsToGetSequential = int.Parse(ConfigurationManager.AppSettings["ResetProcessDefaultNumberOfConceptsForGetSequential"]);
                ResetMaximumConcurrentSynchronizations = int.Parse(ConfigurationManager.AppSettings["ResetProcessDefaultMaximumConcurrentSynchronizations"]);
                MaximumRetryTimes = int.Parse(ConfigurationManager.AppSettings["MaximumRetryTimesOnFailureOfSingleBatchInResetProcess"]);
                SyncDefaultNumberOfConceptsToGetSequential = int.Parse(ConfigurationManager.AppSettings["SynchronizationProcessDefaultNumberOfConcepts"]);
                stabilityStatusFilePath = ConfigurationManager.AppSettings["StablityStatusFilePath"];

                InitiateStablityStatusFile();
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
                client.DeleteSearchServerUnsyncConcepts();
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
            // در حال حاضر امکان همگام‌سازی هستان‌شناسی حین بازنشانی داده‌ای وجود ندارد
            WriteLog("MAKE SURE THE SEARCH INFRASTRUCTURE IS SYNCHRONIZE WITH THE ONTOLOGY, this process is not provide it.");

            ClearAclCache();
            DeleteRegisteredUnsyncConcepts();

            if (IsObjectRangeSpecified && !IsDataSourceRangeSpecified)
            {
                await ResetObjectIndexes();
            }
            else if (IsDataSourceRangeSpecified && !IsObjectRangeSpecified)
            {
                await ResetDataSourceIndexes();
            }
            else
            {
                await ResetDataSourceIndexes();
                await ResetObjectIndexes();
            }

            IsStable = true;
            SetStatusOfStablity(IsStable);
        }
        private async Task ResetObjectIndexes()
        {
            ObjectsSynchronizationAdaptor adaptor = new ObjectsSynchronizationAdaptor();
            SynchronizingConceptsBase synchronizingConcepts;
            if (FirstObjectID >= 1 && LastObjectID >= FirstObjectID)
            {
                synchronizingConcepts = new ConceptsWithIDsInSpecificRange()
                {
                    FirstID = FirstObjectID,
                    LastID = LastObjectID
                };
            }
            else
            {
                synchronizingConcepts = new AllStoredConcpts();
            }
            Synchronizer synchronizer = new Synchronizer(adaptor, logger)
            {
                BatchSize = ResetNumberOfConceptsToGetSequential,
                ConcurrencyLevel = ResetMaximumConcurrentSynchronizations,
                MaximumRetryTimes = MaximumRetryTimes
            };
            await synchronizer.Synchronize(synchronizingConcepts);
        }
        private async Task ResetDataSourceIndexes()
        {
            DataSourcesSynchronizationAdaptor adaptor = new DataSourcesSynchronizationAdaptor();
            Synchronizer synchronizer = new Synchronizer(adaptor, logger)
            {
                BatchSize = ResetNumberOfConceptsToGetSequential,
                ConcurrencyLevel = ResetMaximumConcurrentSynchronizations,
                MaximumRetryTimes = MaximumRetryTimes
            };
            await synchronizer.Synchronize(new AllStoredConcpts());
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
                await SynchronizeNotSyncDataSourceIndexes();
                await SynchronizeNotSyncObjectIndexes();
            }
        }
        private async Task SynchronizeNotSyncObjectIndexes()
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
        private async Task SynchronizeNotSyncDataSourceIndexes()
        {
            DataSourcesSynchronizationAdaptor adaptor = new DataSourcesSynchronizationAdaptor();
            Synchronizer synchronizer = new Synchronizer(adaptor, logger)
            {
                BatchSize = SyncDefaultNumberOfConceptsToGetSequential,
                ConcurrencyLevel = 1,
                MaximumRetryTimes = MaximumRetryTimes
            };
            await synchronizer.Synchronize(new UnsynchronizeConcepts());
        }

        public async Task<bool> SynchronizePublishChanges
            (AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts
            , long dataSourceID, bool isContinousPublish = false, ProcessLogger logger = null)
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

            var dataClient = new RetrieveDataClient();
            ACL dataSourceAcl = dataClient.GetDataSourceACLsMapping(new List<long>() { dataSourceID })[dataSourceID];

            AddedConceptsWithAcl addedConceptsWithAcl = new AddedConceptsWithAcl()
            {
                AddedNonDocumentObjects = new List<SearchObject>(addedConcepts.AddedObjects.Length),
                AddedDocuments = new List<AccessControled<SearchObject>>(),
                AddedProperties = new List<AccessControled<SearchProperty>>(addedConcepts.AddedProperties.Length),
                AddedRelationships = new List<AccessControled<SearchRelationship>>(addedConcepts.AddedRelationships.Length),
                AddedMedias = new List<AccessControled<SearchMedia>>(addedConcepts.AddedMedias.Length)
            };

            foreach (SearchObject addedObj in addedConcepts.AddedObjects)
            {
                if (ontology.IsDocument(addedObj.TypeUri))
                {
                    addedConceptsWithAcl.AddedDocuments.Add(new AccessControled<SearchObject>() { ConceptInstance = addedObj, Acl = dataSourceAcl });
                }
                else
                {
                    addedConceptsWithAcl.AddedNonDocumentObjects.Add(addedObj);
                }
            }
            //addedConceptsWithAcl.AddedProperties.AddRange(addedConcepts.AddedProperties.Select(p => new AccessControled<SearchProperty>() { ConceptInstance = p, Acl = dataSourceAcl  }));
            addedConceptsWithAcl.AddedProperties.AddRange(addedConcepts.AddedProperties.Select(p => new AccessControled<SearchProperty>() { ConceptInstance = new SearchProperty() { Id = p.Id, TypeUri = p.TypeUri, Value = p.Value, DataSourceID = dataSourceID, OwnerObject = p.OwnerObject }  , Acl = dataSourceAcl }));
            //addedConceptsWithAcl.AddedRelationships.AddRange(addedConcepts.AddedRelationships.Select(r => new AccessControled<SearchRelationship>() { ConceptInstance = r, Acl = dataSourceAcl }));
            addedConceptsWithAcl.AddedRelationships.AddRange(addedConcepts.AddedRelationships.Select(r => new AccessControled<SearchRelationship>() { ConceptInstance = new SearchRelationship() { Id = r.Id, DataSourceID = dataSourceID, SourceObjectId= r.SourceObjectId, SourceObjectTypeUri = r.SourceObjectTypeUri, TargetObjectId = r.TargetObjectId, TargetObjectTypeUri  =r.TargetObjectTypeUri,  TypeUri = r.TypeUri, Direction= r.Direction } , Acl = dataSourceAcl }));
            addedConceptsWithAcl.AddedMedias.AddRange(addedConcepts.AddedMedias.Select(m => new AccessControled<SearchMedia>() { ConceptInstance = m, Acl = dataSourceAcl }));
            
            SpecificConcepts syncConcepts = new SpecificConcepts
            {
                LoadedConcepts = new ObjectsSynchronizationCachedConcepts(addedConceptsWithAcl, modifiedConcepts),
                IsContinousSynchronization = isContinousPublish
            };
            ObjectsSynchronizationAdaptor adaptor = new ObjectsSynchronizationAdaptor();
            Synchronizer synchronizer = new Synchronizer(adaptor, logger);
            await synchronizer.Synchronize(syncConcepts);

            if (synchronizer.StayNotSynchronizeConceptsCount == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<bool> SynchronizeDataSource(DataSourceInfo dataSourceInfo, ProcessLogger logger = null)
        {
            if (dataSourceInfo == null)
                throw new ArgumentNullException("dataSourceInfo");

            SpecificConcepts syncConcepts = new SpecificConcepts
            {
                LoadedConcepts = new DataSourceSynchronizationCachedConcepts(new List<DataSourceInfo>() { dataSourceInfo })
            };
            DataSourcesSynchronizationAdaptor adaptor = new DataSourcesSynchronizationAdaptor();
            Synchronizer synchronizer = new Synchronizer(adaptor, logger);
            await synchronizer.Synchronize(syncConcepts);

            if (synchronizer.StayNotSynchronizeConceptsCount == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void InitiateStablityStatusFile()
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
        
        public void FinalizeContinousIndexing()
        {
            DataChangeProvider dataChangeProvider = new DataChangeProvider();
            dataChangeProvider.CommitDocumentChanges();
        }

        public void DeleteExistIndexes()
        {
            WriteLog("Delete existing data indexes...");
            SearchEngineProvider.GetNewDefaultSearchEngineClient().DeleteAllDocuments();
            WriteLog("Indexes were deleted.");
        }

        private void ValidateParameters()
        {
            if (FirstObjectID < 0)
                throw new ArgumentOutOfRangeException(nameof(FirstObjectID));
            if (LastObjectID != 0)
            {
                if (LastObjectID < FirstObjectID)
                    throw new InvalidOperationException($"{nameof(LastObjectID)} < {nameof(FirstObjectID)}");
                else
                    IsObjectRangeSpecified = true;
            }

            if (FirstDataSourceID < 0)
                throw new ArgumentOutOfRangeException(nameof(FirstDataSourceID));
            if (LastDataSourceID != 0)
            {
                if (LastDataSourceID < FirstDataSourceID)
                    throw new InvalidOperationException($"{nameof(LastDataSourceID)} < {nameof(FirstDataSourceID)}");
                else
                    IsDataSourceRangeSpecified = true;
            }

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