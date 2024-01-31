using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.ConceptsToGenerate.Serialization;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPAS.DataImport.Publish
{
    public class ConceptsPublisher
    {
        private string importingObjAndKobjMappingFilePath = "C:/JobWorkerLogs/Mappings/Mappings-{0}.txt";
        string importingObjAndKobjMappingFolderPath = "C:/JobWorkerLogs/Mappings/";
        public int PublishMaximumRetryTimes { get; set; }
        public bool ReportFullDetailsInLog { get; set; }
        public int PublishMinimumIntervalBetweenLogsInSeconds { get; set; }
        public double PublishAcceptableFailsPercentage { get; set; }
        public List<long> AddedObjectIDs { get; private set; }
        public bool IsAnySearchIndexSynchronizationFailureOccured { get; private set; }

        private const int MaximumAcceptableGlobalResolutionCandidates = 200;
        private int maximumNumberOfGlobalResolutionCandidates = 50;
        public int MaximumNumberOfGlobalResolutionCandidates
        {
            get { return maximumNumberOfGlobalResolutionCandidates; }
            set
            {
                if (value > MaximumAcceptableGlobalResolutionCandidates)
                    throw new ArgumentOutOfRangeException(nameof(MaximumNumberOfGlobalResolutionCandidates));
                else
                    maximumNumberOfGlobalResolutionCandidates = value;
            }
        }

        private const int ComponentsPublishDefaultBatchSize = 1000;

        public ConceptsPublisher()
        {
            PublishMaximumRetryTimes = 5;
            ReportFullDetailsInLog = false;
            PublishMinimumIntervalBetweenLogsInSeconds = 30;
            PublishAcceptableFailsPercentage = 5;
            IsAnySearchIndexSynchronizationFailureOccured = false;
        }

        int totalConcept = 0;
        List<ImportingObject> importingObjects;
        List<ImportingRelationship> importingRelationships;
        PublishAdaptor adaptor;
        long masterDataSourceID = -1;
        ACL acl;
        ProcessLogger logger;

        long kobjectsIdCounter;
        long kpropertiesIdCounter;
        long krelationshipsIdCounter;
        AddedConcepts addedConcepts;
        ModifiedConcepts modifiedConcepts;

        Dictionary<ImportingObject, KObject> publishedObjectPreImportingObjects;
        Dictionary<long, List<ImportingObject>> importingObjectsPerPublishedObjID;
        HashSet<ImportingObject> nonPublishableObjects = new HashSet<ImportingObject>();

        bool isInBatchMode = false;
        bool isInitialized = false;

        // This event report last published object index 
        public event EventHandler<PublishedEventArgs> ImportingObjectBatchPublished;

        protected void OnImportingObjectBatchPublished(int lastIndexOfImportingObj, int totalImportingObjects)
        {
            if (lastIndexOfImportingObj < 0)
                throw new ArgumentNullException(nameof(lastIndexOfImportingObj));
            if (totalImportingObjects < 0)
                throw new ArgumentNullException(nameof(totalImportingObjects));

            //ImportingObjectBatchPublished?.Invoke(this, new PublishedEventArgs(lastIndexOfImportingObj, totalImportingObjects));
            ImportingObjectBatchPublished?.Invoke(this, new PublishedEventArgs(lastIndexOfImportingObj, totalConcept));
        }

        // This event report last published relationship index 
        public event EventHandler<PublishedEventArgs> ImportingRelationBatchPublished;

        protected void OnImportingRelationBatchPublished(int lastIndexOfImportingRelation, int totalImportingRelations)
        {
            if (lastIndexOfImportingRelation < 0)
                throw new ArgumentNullException(nameof(lastIndexOfImportingRelation));
            if (totalImportingRelations < 0)
                throw new ArgumentNullException(nameof(totalImportingRelations));

            //ImportingRelationBatchPublished?.Invoke(this, new PublishedEventArgs(lastIndexOfImportingRelation,totalImportingRelations));
            ImportingRelationBatchPublished?.Invoke(this, new PublishedEventArgs(lastIndexOfImportingRelation, totalConcept));
        }

        /// <summary>
        /// کلاس را برای انتشار از منبع غیرساختیافته آماده می‌کند؛
        /// در این حالت، منبع داده‌ای در طول فرایند ایجاد و ثبت می‌شود و نیاز به ثبت آن از قبل نیست
        /// </summary>
        public void InitToPublishFromUnstructuredSource
            (ImportingDocument doc
            , PublishAdaptor publishAdaptor
            , ACL dataSourceAcl
            , ProcessLogger processLogger = null)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            Init
                (new List<ImportingObject>() { doc }, new List<ImportingRelationship>()
                , publishAdaptor, -1, dataSourceAcl, processLogger);
        }

        public void InitToPublishFromSemiStructuredSource
            (List<ImportingObject> objects
            , List<ImportingRelationship> relationships
            , PublishAdaptor publishAdaptor
            , long registeredDataSourceID
            , ACL dataSourceAcl
            , ProcessLogger processLogger = null)
        {
            if (registeredDataSourceID < 1)
                throw new ArgumentOutOfRangeException(nameof(dataSourceAcl));

            Init
                (objects, relationships, publishAdaptor, registeredDataSourceID
                , dataSourceAcl, processLogger);
        }

        private void Init(List<ImportingObject> objects
            , List<ImportingRelationship> relationships
            , PublishAdaptor publishAdaptor
            , long dataSourceID
            , ACL dataSourceAcl
            , ProcessLogger processLogger = null)
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));
            if (relationships == null)
                throw new ArgumentNullException(nameof(relationships));
            if (publishAdaptor == null)
                throw new ArgumentNullException(nameof(publishAdaptor));
            if (dataSourceAcl == null)
                throw new ArgumentNullException(nameof(dataSourceAcl));

            if (objects.Count == 0 && relationships.Count == 0)
                throw new InvalidOperationException("No concept to init publisher");
            if (isInitialized)
                throw new InvalidOperationException("Unable to init. more than once");

            importingObjects = objects;
            importingRelationships = relationships;
            adaptor = publishAdaptor;
            masterDataSourceID = dataSourceID;
            acl = dataSourceAcl;
            logger = processLogger;

            kobjectsIdCounter = -1;
            kpropertiesIdCounter = -1;
            krelationshipsIdCounter = -1;
            addedConcepts = new AddedConcepts()
            {
                AddedObjects = new List<KObject>(),
                AddedProperties = new List<KProperty>(),
                AddedRelationships = new List<RelationshipBaseKlink>(),
                AddedMedias = new List<KMedia>()
            };
            modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = new List<ModifiedProperty>(),
                DeletedMedias = new List<KMedia>()
            };

            publishedObjectPreImportingObjects = new Dictionary<ImportingObject, KObject>();
            importingObjectsPerPublishedObjID = new Dictionary<long, List<ConceptsToGenerate.ImportingObject>>();

            AddedObjectIDs = new List<long>();

            isInitialized = true;

            totalConcept = objects.Count + relationships.Count;
        }

        public void PublishConcepts()
        {
            isInBatchMode = false;

            if (importingObjects.Count > 0 || importingRelationships.Count > 0)
            {
                WriteLog("Prepair publishing concepts ...", false);

                kobjectsIdCounter = firstKobjectsId = GetFirstIdOfReservedObjectIdRange();

                kpropertiesIdCounter = GetFirstIdOfReservedPropertyIdRange();

                krelationshipsIdCounter = firstKrelationshipId = GetFirstIdOfReservedRelationshipIdRange();

                lastPublishLogReportedPercentage = 0;
                lastPublishLogStoreTime = DateTime.MinValue;

                WriteLog("Prepair publishing ...");
                PublishObjectsAndRelationships();
                WriteLog("Publish compeleted.");
            }
            else
            {
                WriteLog("Nothing to publish!");
            }
        }

        private long GetFirstIdOfReservedRelationshipIdRange()
        {
            return GetFirstIdOfReservedConceptIdRange(importingRelationships.Count, adaptor.GetFirstIdOfReservedRelationshipIdRange, "Relationship");
        }

        private long GetFirstIdOfReservedPropertyIdRange()
        {
            long totalPropertiesCount = importingObjects.Sum(o => o.GetProperties().Count());
            return GetFirstIdOfReservedConceptIdRange(totalPropertiesCount, adaptor.GetFirstIdOfReservedPropertyIdRange, "Property");
        }

        private long GetFirstIdOfReservedObjectIdRange()
        {
            return GetFirstIdOfReservedConceptIdRange(importingObjects.Count, adaptor.GetFirstIdOfReservedObjectIdRange, "Object");
        }

        private long GetFirstIdOfReservedConceptIdRange(long rangeLength, Func<long, long> GetIdRangeMethod, string titleOfConcept)
        {
            if (rangeLength <= 0)
            {
                return -1;
            }

            WriteLog($"Getting new {titleOfConcept} IDs...");

            bool retrieved = false;
            long firstIdOfReservedRange = -1;

            int retryTimes = 0;
            while (!retrieved && retryTimes < PublishMaximumRetryTimes)
            {
                try
                {
                    firstIdOfReservedRange = GetIdRangeMethod(rangeLength);
                    retrieved = true;
                    WriteLog($"{titleOfConcept} ID range retrieved: {firstIdOfReservedRange:N0} - {(firstIdOfReservedRange + rangeLength - 1):N0}", false);
                }
                catch (Exception ex)
                {
                    retryTimes++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to get ID range (try {retryTimes} of {PublishMaximumRetryTimes}); Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTimes < PublishMaximumRetryTimes)
                    {
                        WaitProcessForRetry(retryTimes, "Retry getting IDs...");
                    }
                    else
                    {
                        string message = string.Format("Unable to get IDs after {0} retri(es); Inner-exception contains the last try exception", PublishMaximumRetryTimes);
                        WriteLog(message);
                        throw new UnableToPublishException(message, ex);
                    }
                }
            }
            return firstIdOfReservedRange;
        }

        /// <summary>
        /// انتشار مفاهیم به صورت یکجا
        /// </summary>
        public void PublishConceptsInBatchMode(int lastImportObjectIndex = -1,
            int lastImportRelationIndex = -1)
        {
            isInBatchMode = true;

            int startImportingObjectIndex = 0;
            int startImportingRelationIndex = 0;

            InitResumeStatus();

            if (lastImportObjectIndex != -1)
            {
                startImportingObjectIndex = lastImportObjectIndex;
            }
            if (lastImportRelationIndex != -1)
            {
                startImportingRelationIndex = lastImportRelationIndex;
            }

            WriteLog("Prepair publishing concepts ...", false);
            if (PublishAcceptableFailsPercentage < 0
                || PublishAcceptableFailsPercentage > 100)
            {
                throw new ArgumentOutOfRangeException("PublishAcceptableFailsPercentage");
            }

            if (importingObjects.Count > 0)
            {
                kobjectsIdCounter = firstKobjectsId = GetFirstIdOfReservedObjectIdRange();

                kpropertiesIdCounter = GetFirstIdOfReservedPropertyIdRange();

                lastPublishLogReportedPercentage = 0;
                lastPublishLogStoreTime = DateTime.MinValue;

                int totalFailedImportingObjectsCount = 0;

                List<ImportingObject> objectBatch = new List<ImportingObject>(ComponentsPublishDefaultBatchSize);
                int objectBatchPropertiesCount = 0;
                for (int i = startImportingObjectIndex; i < importingObjects.Count; i++)
                {
                    ImportingObject imObj = importingObjects[i];
                    objectBatch.Add(imObj);
                    objectBatchPropertiesCount += imObj.Properties.Count;

                    if (i == importingObjects.Count - 1
                        || objectBatchPropertiesCount + importingObjects[i + 1].Properties.Count > ComponentsPublishDefaultBatchSize
                        || objectBatch.Count >= ComponentsPublishDefaultBatchSize)
                    {
                        try
                        {
                            lastImportObjectIndex = i + 1;
                            OnImportingObjectBatchPublished(lastImportObjectIndex, importingObjects.Count());
                            WriteLog(string.Format("Prepair publishing {0} object(s) [Last imported object index: {1}]", objectBatch.Count, lastImportObjectIndex));
                            WriteObjectsPublishProgressLog();
                            PublishObjects(objectBatch);
                        }
                        catch (UnableToPublishException)
                        {
                            lastImportObjectIndex = i + 1 - objectBatch.Count;
                            OnImportingObjectBatchPublished(lastImportObjectIndex, importingObjects.Count());
                            totalFailedImportingObjectsCount += objectBatch.Count;

                            double failedImportingObjectsPercentage = (totalFailedImportingObjectsCount * 100) / importingObjects.Count;
                            if (failedImportingObjectsPercentage < PublishAcceptableFailsPercentage)
                            {
                                WriteLog(string.Format("Batch publish failed; Failed publishes {0}%. Import will continue...", failedImportingObjectsPercentage));
                            }
                            else
                            {
                                WriteLog(string.Format("Batch publish failed; Failed publishes are {0}% and exceed threshould. Import failed.", failedImportingObjectsPercentage));
                                throw;
                            }
                            foreach (ImportingObject obj in objectBatch)
                            {
                                nonPublishableObjects.Add(obj);
                            }
                        }
                        finally
                        {
                            objectBatch.Clear();
                            objectBatchPropertiesCount = 0;
                        }
                    }
                }

                if (totalFailedImportingObjectsCount == 0)
                {
                    WriteLog("Objects and their Properties Publish compeleted without any fail publish.");
                }
                else
                {
                    WriteLog(string.Format("Objects and their Properties Publish compeleted without {0}% fails", (totalFailedImportingObjectsCount * 100) / importingObjects.Count));
                }
            }
            else
            {
                WriteLog("No object for publish");
            }

            // لینک هایی که یکی یا هر دو گره ی دو سر آن به صورت موفقیت آمیزی منتشر نشده اند از لیست حذف می شوند.
            RemoveRelationshipsWithNonPublishableEnd();

            if (importingRelationships.Count > 0)
            {
                krelationshipsIdCounter = firstKrelationshipId = GetFirstIdOfReservedRelationshipIdRange();

                lastPublishLogStoreTime = DateTime.MinValue;
                lastPublishLogReportedPercentage = 0;

                int totalFailedRelationshipsCount = 0;

                for (int batchIndex = startImportingRelationIndex; batchIndex <= ((importingRelationships.Count - 1) / ComponentsPublishDefaultBatchSize); batchIndex++)
                {
                    int batchStartIndex = batchIndex * ComponentsPublishDefaultBatchSize;
                    int batchCount = Math.Min(ComponentsPublishDefaultBatchSize, importingRelationships.Count - batchStartIndex);

                    List<ImportingRelationship> relationshipsToPublish = importingRelationships.GetRange(batchStartIndex, batchCount);
                    WriteLog(string.Format("Prepair publishing {0} relationships(s)", relationshipsToPublish.Count));
                    try
                    {
                        lastImportRelationIndex = batchIndex;
                        //OnImportingRelationBatchPublished(lastImportRelationIndex, importingRelationships.Count());
                        OnImportingRelationBatchPublished(lastImportRelationIndex, totalConcept);
                        WriteLog(string.Format("Prepair publishing {0} relationships(s) [Last imported relationship index is {1}]", relationshipsToPublish.Count, lastImportRelationIndex));
                        WriteRelationshipsPublishProgressLog();
                        PublishRelationships(relationshipsToPublish);
                    }
                    catch (UnableToPublishException)
                    {
                        totalFailedRelationshipsCount += batchCount;

                        double failedImportingRelationshipsPercentage = (totalFailedRelationshipsCount * 100) / importingRelationships.Count;
                        if (failedImportingRelationshipsPercentage < PublishAcceptableFailsPercentage)
                        {
                            WriteLog(string.Format("Batch publish failed; Failed relationship publishes {0}%. Import will continue...", failedImportingRelationshipsPercentage));
                        }
                        else
                        {
                            WriteLog(string.Format("Batch publish failed; Failed relationship publishes are {0}% and exceed threshould. Import failed.", failedImportingRelationshipsPercentage));
                            throw;
                        }
                    }
                    WriteLog("Relationships Publish compeleted.");
                }
            }
            else
            {
                WriteLog("No relationship for publish");
            }

            if (File.Exists(string.Format(importingObjAndKobjMappingFilePath, masterDataSourceID)))
            {
                File.Delete(string.Format(importingObjAndKobjMappingFilePath, masterDataSourceID));
            }

            adaptor.FinalizeContinousPublish();
        }

        private void InitResumeStatus()
        {
            publishedObjectPreImportingObjects = new Dictionary<ImportingObject, KObject>();
            string currentMappingFilePath = string.Format(importingObjAndKobjMappingFilePath, masterDataSourceID);
            if (File.Exists(currentMappingFilePath))
            {
                Serializer serializer = new Serializer();
                Dictionary<ImportingObject, KObject> currentMapping
                    = serializer.DeserializeMappingFromFile(currentMappingFilePath);

                foreach (var currentRow in currentMapping)
                {
                    AddImportingObjectMapping(currentRow.Key, currentRow.Value);
                }
            }
        }

        private void RemoveRelationshipsWithNonPublishableEnd()
        {
            if (nonPublishableObjects.Count > 0)
            {
                WriteLog("Removing relationships with not-published source and/or target...");
                int relationshipsCountBeforeRemove = importingRelationships.Count;
                importingRelationships
                    .RemoveAll(r => nonPublishableObjects.Contains(r.Source)
                                 || nonPublishableObjects.Contains(r.Target));
                int relationshipsCountAfterRemove = importingRelationships.Count;
                WriteLog(string.Format("{0} relationships removed from publish queue.", relationshipsCountBeforeRemove - relationshipsCountAfterRemove));
                nonPublishableObjects.Clear();
            }
        }

        private void PublishObjectsAndRelationships()
        {
            PrepairObjectsToAddInPublishProcess(importingObjects);
            RemoveRelationshipsWithNonPublishableEnd();
            PrepairRelationshipsToAddInPublishProcess(importingRelationships);
            PublishPrepairedObjectsAndRelationshipsChanges();
        }

        private void PublishRelationships(List<ImportingRelationship> relationshipsToPublish)
        {
            PrepairRelationshipsToAddInPublishProcess(relationshipsToPublish);
            PublishPrepairedRelationshipsChanges();
        }

        private void PrepairRelationshipsToAddInPublishProcess(List<ImportingRelationship> relationshipsToPublish)
        {
            foreach (ImportingRelationship relationship in relationshipsToPublish)
            {

                var source = GetRelatedKObjectFromDictionary(relationship.Source);
                var target = GetRelatedKObjectFromDictionary(relationship.Target);

                //var source = publishedObjectPreImportingObjects[relationship.Source];
                //var target = publishedObjectPreImportingObjects[relationship.Target];


                long newRelId = GetNewRelationshipID();
                addedConcepts.AddedRelationships.Add(new RelationshipBaseKlink()
                {
                    Source = source,
                    Target = target,
                    TypeURI = relationship.TypeURI,
                    Relationship = new KRelationship()
                    {
                        Id = newRelId,
                        Description = relationship.Description,
                        TimeBegin = relationship.TimeBegin,
                        TimeEnd = relationship.TimeEnd,
                        Direction = ConvertImportingDirectionToDispatchDirection(relationship.Direction)
                    }
                });
            }
        }

        private KObject GetRelatedKObjectFromDictionary(ImportingObject source)
        {
            KObject relatedKObject = null;
            foreach (var currentRow in publishedObjectPreImportingObjects)
            {
                ImportingObject xImportObj = currentRow.Key;
                ImportingObject yImportObj = source;

                if (xImportObj.GetHashCode() == yImportObj.GetHashCode())
                {
                    relatedKObject = currentRow.Value;
                    break;
                }
            }

            return relatedKObject;
        }

        private bool IsImportingObjectExistInDictionary(ImportingObject importingObject)
        {
            bool result = false;
            foreach (var currentRow in publishedObjectPreImportingObjects)
            {
                ImportingObject xImportObj = currentRow.Key;
                ImportingObject yImportObj = importingObject;

                if (xImportObj.GetHashCode() == yImportObj.GetHashCode())
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        private bool IsPropertiesEqual(ImportingObject xImportObj, ImportingObject yImportObj)
        {
            bool result = false;
            foreach (var currentXProperty in xImportObj.Properties)
            {
                if (yImportObj.Properties.Where(p => p.TypeURI == currentXProperty.TypeURI && p.Value == currentXProperty.Value).Count() == 1)
                {
                    result = true;
                    break;
                }

            }

            return result;
        }

        //private static object relationshipIdGenerationLockObject = new object();
        private long GetNewRelationshipID()
        {
            //lock (relationshipIdGenerationLockObject)
            //{
            return krelationshipsIdCounter++;
            //}
        }

        private static object logReportLockObject = new object();
        DateTime lastPublishLogStoreTime = DateTime.MinValue;
        double lastPublishLogReportedPercentage = 0;
        long firstKobjectsId = -1;
        private void WriteObjectsPublishProgressLog()
        {
            if (logger != null)
            {
                lock (logReportLockObject)
                {
                    double percentages = (kobjectsIdCounter - firstKobjectsId) * 100 / importingObjects.Count;
                    if (lastPublishLogReportedPercentage != percentages
                        && lastPublishLogStoreTime.AddSeconds(PublishMinimumIntervalBetweenLogsInSeconds) <= DateTime.Now)
                    {
                        WriteLog(string.Format("Publishing Objects and their Properties: {0}%", percentages), false);
                        lastPublishLogStoreTime = DateTime.Now;
                        lastPublishLogReportedPercentage = percentages;
                    }
                }
            }
        }

        long firstKrelationshipId = -1;
        private void WriteRelationshipsPublishProgressLog()
        {
            if (logger != null)
            {
                lock (logReportLockObject)
                {
                    double percentages = (krelationshipsIdCounter - firstKrelationshipId) * 100 / importingRelationships.Count;
                    if (lastPublishLogReportedPercentage != percentages
                        && lastPublishLogStoreTime.AddSeconds(PublishMinimumIntervalBetweenLogsInSeconds) <= DateTime.Now)
                    {
                        WriteLog(string.Format("Publishing Relationships: {0}%", percentages), false);
                        lastPublishLogStoreTime = DateTime.Now;
                        lastPublishLogReportedPercentage = percentages;
                    }
                }
            }
        }

        private void PublishObjects(List<ImportingObject> objectsToPublish)
        {
            PrepairObjectsToAddInPublishProcess(objectsToPublish);

            if (!File.Exists(importingObjAndKobjMappingFolderPath))
            {
                Directory.CreateDirectory(importingObjAndKobjMappingFolderPath);
            }

            Serializer serializer = new Serializer();
            serializer.SerializeMappingDictionaryToFile(string.Format(importingObjAndKobjMappingFilePath, masterDataSourceID), publishedObjectPreImportingObjects);

            PublishPrepairedObjectsChanges();
        }

        private void PublishPrepairedRelationshipsChanges()
        {
            if (!IsAnyChangePrepaired())
            {
                WriteLog("Nothing to publish!");
                return;
            }

            bool published;
            PublishResult publishResult;
            Exception publishLastException;
            WriteLog("Publishing ...");
            PublishTotalPrepairedChanges(out published, out publishResult, out publishLastException);
            if (published)
            {
                WriteLog(string.Format
                    ("{0:N0} Relationship(s) Published; Store duration: {1} | Horizon synchronized: {2} | Search synchronized: {3}"
                    , addedConcepts.AddedRelationships.Count
                    , publishResult.RepositoryStoreDuration.ToString(@"h\:mm\:ss\.fff")
                    , !(publishResult.HorizonServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.HorizonServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))
                    , !(publishResult.SearchServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.SearchServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))), true);
            }
            addedConcepts.AddedRelationships.Clear();
            if (!published)
            {
                string message = string.Format("Unable to Publish Relationship(s) after {0} retri(es)", PublishMaximumRetryTimes);
                WriteLog(message);
                throw new UnableToPublishException(message, publishLastException);
            }
        }

        private bool IsAnyChangePrepaired()
        {
            return addedConcepts.AddedObjects.Count > 0
                || addedConcepts.AddedProperties.Count > 0
                || addedConcepts.AddedRelationships.Count > 0
                || addedConcepts.AddedMedias.Count > 0
                || modifiedConcepts.ModifiedProperties.Count > 0
                || modifiedConcepts.DeletedMedias.Count > 0;
        }

        private void PublishPrepairedObjectsAndRelationshipsChanges()
        {
            if (!IsAnyChangePrepaired())
            {
                WriteLog("Nothing to publish!");
                return;
            }

            bool published;
            PublishResult publishResult;
            Exception publishLastException;
            WriteLog("Publishing ...");
            PublishTotalPrepairedChanges(out published, out publishResult, out publishLastException);
            if (published)
            {
                WriteLog(string.Format
                    ("{0:N0} added Object(s), {1:N0} resolved Object(s), {2:N0} added Properti(es) and {3:N0} added Relatinship(s) Published; Store duration: {4} | Horizon synchronized: {5} | Search syncronized: {6}"
                    , addedConcepts.AddedObjects.Count
                    , 0
                    , addedConcepts.AddedProperties.Count
                    , addedConcepts.AddedRelationships.Count
                    , publishResult.RepositoryStoreDuration.ToString(@"h\:mm\:ss\.fff")
                    , !(publishResult.HorizonServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.HorizonServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))
                    , !(publishResult.SearchServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.SearchServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))), true);
            }
            else
            {
                string message = string.Format("Unable to Publish concept(s) after {0} retri(es); Inner-exception contains Publish last exception", PublishMaximumRetryTimes);
                WriteLog(message);
                throw new Exception(message, publishLastException);
            }

            addedConcepts.AddedObjects.Clear();
            addedConcepts.AddedProperties.Clear();
        }

        private void PublishPrepairedObjectsChanges()
        {
            if (!IsAnyChangePrepaired())
            {
                WriteLog("Nothing to publish!");
                return;
            }

            bool published;
            PublishResult publishResult;
            Exception publishLastException;
            WriteLog("Publishing ...");
            PublishTotalPrepairedChanges(out published, out publishResult, out publishLastException);
            if (published)
            {
                WriteLog(string.Format
                    ("{0:N0} added Object(s), {1:N0} resolved Object(s) and {2:N0} Properti(es) Published; Store duration: {3} | Horizon synchronized: {4} | Search synchronized: {5}"
                    , addedConcepts.AddedObjects.Count
                    , 0
                    , addedConcepts.AddedProperties.Count
                    , publishResult.RepositoryStoreDuration.ToString(@"h\:mm\:ss\.fff")
                    , !(publishResult.HorizonServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.HorizonServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))
                    , !(publishResult.SearchServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.SearchServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))), true, true);
            }
            addedConcepts.AddedObjects.Clear();
            addedConcepts.AddedProperties.Clear();
            if (!published)
            {
                string message = string.Format("Unable to Publish Objects and their Properties after {0} retri(es); Inner-exception contains Publish last exception", PublishMaximumRetryTimes);
                WriteLog(message);
                throw new UnableToPublishException(message, publishLastException);
            }
        }

        private void PublishTotalPrepairedChanges(out bool published, out PublishResult publishResult, out Exception publishLastException)
        {
            published = false;
            publishResult = null;
            publishLastException = null;

            int publishRetryTimes = 0;
            while (!published && publishRetryTimes < PublishMaximumRetryTimes)
            {
                try
                {
                    publishResult = adaptor.PublshConcepts(addedConcepts, modifiedConcepts, masterDataSourceID, isInBatchMode);
                    //OnSomeConceptsPublished();
                    AddedObjectIDs.AddRange(addedConcepts.AddedObjects.Select(o => o.Id));
                    if (!publishResult.HorizonServerSynchronized || !publishResult.SearchServerSynchronized)
                    {
                        IsAnySearchIndexSynchronizationFailureOccured = true;
                    }
                    published = true;
                }
                catch (Exception ex)
                {
                    publishRetryTimes++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog(string.Format("Unable to publish prepaired concepts (try {0} of {1}); Exception:{2}{3}"
                        , publishRetryTimes.ToString(), PublishMaximumRetryTimes.ToString(), Environment.NewLine, exceptionDetails), false);
                    publishLastException = ex;
                    WaitProcessForRetry(publishRetryTimes, "Retry publish...");
                }
            }
        }

        private void PrepairObjectsToAddInPublishProcess(List<ImportingObject> objectsToPublish)
        {
            if (objectsToPublish.Count == 0)
                return;

            foreach (ImportingObject obj in objectsToPublish)
            {
                try
                {
                    PrepairObjectToAddInPublishProcess(obj);
                }
                catch
                {
                    WriteLog("Unable to prepair object to publish; Related components will ignore from publish process.");
                    continue;
                }
            }
        }
        /// <summary>
        /// شئی که قرار است وارد شود آیدی مربوطه ساخته می شود و در صورتی که از نوع داکیومنت باشد پس از آپلود داکیومنت مربوطه 
        /// KObject
        /// .برگردانده می شود
        /// </summary>
        /// <returns>
        /// در صورتی که عملیات ساخت شیء با موفقیت انجام شود شیء بازگردانده می شود در غیر اینصورت مقدار نال بازگردانده می شود
        /// </returns>
        private KObject PrepairObjectToAddInPublishProcess(ImportingObject obj)
        {
            KObject generatingObject = GenerateNewKObject(obj);
            if (obj is ImportingDocument)
            {
                try
                {
                    PublishDocument(obj as ImportingDocument, generatingObject);
                }
                catch
                {
                    nonPublishableObjects.Add(obj);
                    WriteLog($"Faild to publish a document from batch objects (Document: \"{obj.LabelProperty.Value}\").", false);
                    throw;
                }
            }
            AddImportingObjectMapping(obj, generatingObject);
            if (!(obj is ImportingDocument))
            {
                addedConcepts.AddedObjects.Add(generatingObject);
                PrepairPropertiesToAddForExistObjectInPublishProcess(generatingObject, obj.GetProperties(), obj.LabelProperty);
            }
            return generatingObject;
        }
        private void PublishDocument(ImportingDocument importingDoc, KObject generatingDoc)
        {
            byte[] documentContent = ReadDocumentContent(importingDoc);

            var dataSource = new DataSourceMetadata()
            {
                Name = importingDoc.LabelProperty.Value,
                Type = DataSourceType.Document,
                Content = documentContent,
                Acl = acl,
                Description = string.Empty
            };
            var regProvider = new DataSourceRegisterationProvider(dataSource, adaptor, logger);
            regProvider.ProcessMaximumRetryTimes = PublishMaximumRetryTimes;
            regProvider.ReportFullDetailsInLog = ReportFullDetailsInLog;
            regProvider.Register(generatingDoc.Id);
            PublishDocumentIndivisually(importingDoc, generatingDoc, regProvider.DataSourceID);
        }

        private void PublishDocumentIndivisually(ImportingDocument importingDoc, KObject generatingDoc, long dataSourceID)
        {
            List<KObject> objectsToPublish = new List<KObject>() { generatingDoc };
            List<KProperty> propertiesToPublish = GenerateKPropertiesByImportingProperties(importingDoc.GetProperties(), importingDoc.LabelProperty, ref generatingDoc);
            AddedConcepts addedConceptsToPublish = new AddedConcepts()
            {
                AddedObjects = objectsToPublish,
                AddedProperties = propertiesToPublish,
                AddedRelationships = new List<RelationshipBaseKlink>(),
                AddedMedias = new List<KMedia>()
            };
            ModifiedConcepts modifiedConceptsToPublish = new ModifiedConcepts()
            {
                ModifiedProperties = new List<ModifiedProperty>(),
                DeletedMedias = new List<KMedia>()
            };

            WriteLog("Publishing document indivisually...");

            PublishResult publishResult = null;
            bool published = false;
            int retryTimes = 0;
            while (!published && retryTimes < PublishMaximumRetryTimes)
            {
                try
                {
                    publishResult = adaptor.PublshConcepts(addedConceptsToPublish, modifiedConceptsToPublish, dataSourceID, true);
                    AddedObjectIDs.Add(generatingDoc.Id);
                    published = true;
                }
                catch (Exception ex)
                {
                    retryTimes++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to publish document indivisually (try {retryTimes} of {PublishMaximumRetryTimes}); Document ID: \"{dataSourceID}\" Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (retryTimes < PublishMaximumRetryTimes)
                    {
                        WaitProcessForRetry(retryTimes, "Retry publishing document...");
                    }
                    else
                    {
                        WriteLog($"Document publish failed.");
                        throw;
                    }
                }
            }
            WriteLog(string.Format("Dcoument published; Store duration: {0} | Horizon synchronized: {1} | Search synchronized: {2}"
                , publishResult.RepositoryStoreDuration.ToString(@"h\:mm\:ss\.fff")
                , !(publishResult.HorizonServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.HorizonServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))
                , !(publishResult.SearchServerSynchronized) ? "No" : string.Format("Yes ({0})", publishResult.SearchServerSyncDuration.ToString(@"h\:mm\:ss\.fff"))));
        }

        private byte[] ReadDocumentContent(ImportingDocument doc)
        {
            WriteLog($"Reading document: \"{doc.DocumentPath}\"");

            if (string.IsNullOrEmpty(doc.DocumentPath))
                throw new ArgumentNullException(nameof(doc.DocumentPath));

            bool bytesRead = false;
            int readBytesRetryTime = 0;
            byte[] documentContent = new byte[] { };
            while (!bytesRead && readBytesRetryTime < PublishMaximumRetryTimes)
            {
                try
                {
                    documentContent = System.IO.File.ReadAllBytes(doc.DocumentPath);
                    bytesRead = true;
                    WriteLog($"Read completed.");
                }
                catch (Exception ex)
                {
                    readBytesRetryTime++;
                    string exceptionDetails = GetExceptionDetailsString(ex);
                    WriteLog($"Unable to read the document file (try {readBytesRetryTime} of {PublishMaximumRetryTimes}); Document path: \"{doc.DocumentPath}\" Exception:{Environment.NewLine}{exceptionDetails}", false);
                    if (readBytesRetryTime < PublishMaximumRetryTimes)
                    {
                        WaitProcessForRetry(readBytesRetryTime, "Retry reading document...");
                    }
                    else
                    {
                        WriteLog($"Read failed.");
                        throw;
                    }
                }
            }
            return documentContent;
        }

        private void WaitProcessForRetry(int uploadRetryTime, string postWaitPrompt)
        {
            int waitMiliseconds = (int)(Math.Pow(2, uploadRetryTime) * 250);
            WriteLog(string.Format("Wait {0} miliseconds...", waitMiliseconds), false);
            System.Threading.Tasks.Task.Delay(waitMiliseconds).Wait();
            WriteLog(postWaitPrompt);
        }

        private static string GetExceptionDetailsString(Exception ex)
        {
            var detailGenerator = new ExceptionDetailGenerator();
            string exceptionDetails = detailGenerator.GetDetails(ex);
            return exceptionDetails;
        }

        private void PrepairPropertiesToAddForExistObjectInPublishProcess(KObject generatingObject, IEnumerable<ImportingProperty> properties, ImportingProperty generatingObjectLabelProperty = null)
        {
            List<KProperty> generatingProperties = GenerateKPropertiesByImportingProperties(properties, generatingObjectLabelProperty, ref generatingObject);
            addedConcepts.AddedProperties.AddRange(generatingProperties);
        }

        private List<KProperty> GenerateKPropertiesByImportingProperties(IEnumerable<ImportingProperty> properties, ImportingProperty generatingObjectLabelProperty, ref KObject generatingObject)
        {
            List<KProperty> generatingProperties = new List<KProperty>();
            foreach (var prop in properties)
            {
                KProperty generatedProperty = GenerateNewKProperty(generatingObject, prop);
                generatingProperties.Add(generatedProperty);
                if (generatingObjectLabelProperty != null
                    && generatingObjectLabelProperty.TypeURI == prop.TypeURI
                    && generatingObjectLabelProperty.Value == prop.Value
                    )

                //generatingObjectLabelProperty.Equals(prop))
                {
                    generatingObject.LabelPropertyID = generatedProperty.Id;
                }
            }
            return generatingProperties;
        }

        //private static object propertyIdGenerationLockObject = new object();
        private long GetNewPropertyID()
        {
            //lock (propertyIdGenerationLockObject)
            //{
            return kpropertiesIdCounter++;
            //}
        }
        //private static object objectIdGenerationLockObject = new object();
        private long GetNewObjectID()
        {
            //lock (objectIdGenerationLockObject)
            //{
            return kobjectsIdCounter++;
            //}
        }
        //private static object AddImportingObjectMappingLockObject = new object();
        private void AddImportingObjectMapping(ImportingObject obj, KObject relatedExistingObject)
        {
            //lock (AddImportingObjectMappingLockObject)
            //{       
            if (!IsImportingObjectExistInDictionary(obj))
            {
                publishedObjectPreImportingObjects.Add(obj, relatedExistingObject);
            }

            if (!importingObjectsPerPublishedObjID.ContainsKey(relatedExistingObject.Id))
            {
                importingObjectsPerPublishedObjID.Add(relatedExistingObject.Id, new List<ImportingObject>());
            }
            importingObjectsPerPublishedObjID[relatedExistingObject.Id].Add(obj);
            //}
        }

        private string GetLongsCommaSpearatedString(IEnumerable<long> longs)
        {
            string result = "";
            long[] longsArray = longs.ToArray();
            for (int i = 0; i < longsArray.Length; i++)
            {
                result += string.Format("{0}{1}", (i == 0) ? "" : ", ", longsArray[i].ToString());
            }
            return result;
        }

        private void WriteLog(string message, bool isDetailedLog = true, bool reportInuseMemory = false)
        {
            if (logger != null)
            {
                if (isDetailedLog && !ReportFullDetailsInLog)
                    return;
                logger.WriteLog(message, reportInuseMemory);
            }
        }

        private KProperty GenerateNewKProperty(KObject generatingObject, ImportingProperty prop)
        {
            long newPropId = GetNewPropertyID();
            return new KProperty()
            {
                Id = newPropId,
                Owner = generatingObject,
                TypeUri = prop.TypeURI,
                Value = prop.Value
            };
        }

        private KObject GenerateNewKObject(ImportingObject obj)
        {
            long newObjId = GetNewObjectID();
            KObject newKobject = new KObject()
            {
                Id = newObjId,
                TypeUri = obj.TypeUri,
                LabelPropertyID = null
            };
            return newKobject;
        }

        public static LinkDirection ConvertImportingDirectionToDispatchDirection(ImportingRelationshipDirection direction)
        {
            switch (direction)
            {
                case ImportingRelationshipDirection.SourceToTarget:
                    return LinkDirection.SourceToTarget;
                case ImportingRelationshipDirection.TargetToSource:
                    return LinkDirection.TargetToSource;
                case ImportingRelationshipDirection.Bidirectional:
                    return LinkDirection.Bidirectional;
                default:
                    throw new NotSupportedException();
            }
        }

        public static int GetImportObjectIndexInPauseTime()
        {
            throw new NotImplementedException();
            //return ImportObjectIndexInPauseTime;
        }
    }

    public class PublishedEventArgs
    {
        public PublishedEventArgs(int lastIndexOfConcept, int totalImportingConcept)
        {
            LastIndexOfConcept = lastIndexOfConcept;
            TotalImportingConcept = totalImportingConcept;
        }
        public int TotalImportingConcept
        {
            get;
        }
        public int LastIndexOfConcept
        {
            get;
        }
    }
}