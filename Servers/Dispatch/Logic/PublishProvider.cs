using GPAS.AccessControl;
using GPAS.DataImport.Publish;
using GPAS.Dispatch.DataAccess;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
//using GPAS.Dispatch.ServiceAccess.RepositoryService;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using SearchService = GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    public class PublishProvider
    {
        static readonly int MaximumAcceptableUnsynchronized = int.Parse(ConfigurationManager.AppSettings["MaximumAcceptableUnsynchronized"]);
        private TimeSpan horizonServerSyncDuration = new TimeSpan();
        private TimeSpan searchServerSyncDuration = new TimeSpan();
        private TimeSpan repositoryStoreDuration = new TimeSpan();

        //private DBAddedConcepts ConvertKAddedConceptToDBAddedConcept(AddedConcepts kAddedConcept)
        //{
        //    DBAddedConcepts dbAddedConcept = new DBAddedConcepts();
        //    if (kAddedConcept == null)
        //        return dbAddedConcept;
        //    if (kAddedConcept.AddedMedias != null)
        //    {
        //        dbAddedConcept.AddedMediaList = new DBMedia[kAddedConcept.AddedMedias.Count];
        //        for (int i = 0; i < kAddedConcept.AddedMedias.Count; i++)
        //        {
        //            DBMedia dbMedia = ConvertKMediaToDBMedia(kAddedConcept.AddedMedias[i]);
        //            dbAddedConcept.AddedMediaList[i] = dbMedia;
        //        }
        //    }

        //    if (kAddedConcept.AddedObjects != null)
        //    {
        //        dbAddedConcept.AddedObjectList = new DBObject[kAddedConcept.AddedObjects.Count];
        //        for (int i = 0; i < kAddedConcept.AddedObjects.Count; i++)
        //        {
        //            DBObject dbObject = ConvertKObjectToDBObject(kAddedConcept.AddedObjects[i]);
        //            dbAddedConcept.AddedObjectList[i] = dbObject;
        //        }
        //    }
        //    if (kAddedConcept.AddedProperties != null)
        //    {
        //        dbAddedConcept.AddedPropertyList = new DBProperty[kAddedConcept.AddedProperties.Count];
        //        for (int i = 0; i < kAddedConcept.AddedProperties.Count; i++)
        //        {
        //            DBProperty dbProperty = ConvertKPropertyToDBProperty(kAddedConcept.AddedProperties[i]);
        //            dbAddedConcept.AddedPropertyList[i] = dbProperty;
        //        }
        //    }
        //    if (kAddedConcept.AddedRelationships != null)
        //    {
        //        dbAddedConcept.AddedRelationshipList = new DBRelationship[kAddedConcept.AddedRelationships.Count];
        //        for (int i = 0; i < kAddedConcept.AddedRelationships.Count; i++)
        //        {
        //            DBRelationship dbRelationship = ConvertDBRelationshipRelationshipBaseKlink(kAddedConcept.AddedRelationships[i]);
        //            dbAddedConcept.AddedRelationshipList[i] = dbRelationship;
        //        }
        //    }
        //    return dbAddedConcept;
        //}

        //public KMedia ConvertDBMediaToKMedia(long dbMedia)
        //{
        //    return new KMedia()
        //    {

        //    };
        //    //return new KMedia()
        //    //{
        //    //    Description = dbMedia.Description,
        //    //    Id = dbMedia.Id,
        //    //    URI = dbMedia.URI,
        //    //    ObjectId = dbMedia.ObjectId
        //    //};
        //}
        //public KMedia ConvertDBMediaToKMedia(DBMedia dbMedia)
        //{
        //    //return new KMedia()
        //    //{
        //    //};
        //    return new KMedia()
        //    {
        //        Description = dbMedia.Description,
        //        Id = dbMedia.Id,
        //        URI = dbMedia.URI,
        //        OwnerObjectId = dbMedia.ObjectId
        //    };
        //}

        //private DBMedia ConvertKMediaToDBMedia(KMedia kMedia)
        //{
        //    return new DBMedia()
        //    {
        //        Description = kMedia.Description,
        //        Id = kMedia.Id,
        //        URI = kMedia.URI,
        //        ObjectId = kMedia.OwnerObjectId
        //    };
        //}

        //private KProperty ConvertDBPropertyToKProperty(DBProperty dbproperty, string callerUserName)
        //{
        //    return new KProperty()
        //    {
        //        Id = dbproperty.Id,
        //        TypeUri = dbproperty.TypeUri,
        //        Value = dbproperty.Value,
        //        Owner = ConvertDBObjectToKObject(dbproperty.Owner, callerUserName)
        //    };
        //}

        //private DBProperty ConvertKPropertyToDBProperty(KProperty kMedia)
        //{
        //    return new DBProperty()
        //    {
        //        Id = kMedia.Id,
        //        TypeUri = kMedia.TypeUri,
        //        Value = kMedia.Value,
        //        Owner = ConvertKObjectToDBObject(kMedia.Owner)
        //    };
        //}

        //private DBRelationship ConvertDBRelationshipRelationshipBaseKlink(RelationshipBaseKlink relationshipBaseKlink)
        //{
        //    return new DBRelationship()
        //    {
        //        Description = relationshipBaseKlink.Relationship.Description,
        //        Id = relationshipBaseKlink.Relationship.Id,
        //        Direction = (RepositoryLinkDirection)relationshipBaseKlink.Relationship.Direction,
        //        TimeBegin = relationshipBaseKlink.Relationship.TimeBegin,
        //        TimeEnd = relationshipBaseKlink.Relationship.TimeEnd,
        //        TypeURI = relationshipBaseKlink.TypeURI,
        //        Source = ConvertKObjectToDBObject(relationshipBaseKlink.Source),
        //        Target = ConvertKObjectToDBObject(relationshipBaseKlink.Target)
        //    };
        //}

        //private RelationshipBaseKlink ConvertDBRelationshipRelationshipBaseKlink(DBRelationship dbRelationship, string callerUserName)
        //{
        //    return new RelationshipBaseKlink()
        //    {
        //        TypeURI = dbRelationship.TypeURI,
        //        Relationship = new KRelationship()
        //        {
        //            Direction = (LinkDirection)dbRelationship.Direction,
        //            Description = dbRelationship.Description,
        //            Id = dbRelationship.Id,
        //            TimeBegin = dbRelationship.TimeBegin,
        //            TimeEnd = dbRelationship.TimeEnd
        //        },
        //        Source = ConvertDBObjectToKObject(dbRelationship.Source, callerUserName),
        //        Target = ConvertDBObjectToKObject(dbRelationship.Target, callerUserName)
        //    };

        //}

        //private KObject ConvertDBObjectToKObject(DBObject dbObject, string callerUserName)
        //{
        //    return new KObject()
        //    {
        //        LabelPropertyID = dbObject.LabelPropertyID,
        //        Id = dbObject.Id,
        //        IsGroup = dbObject.IsGroup,
        //        ResolvedTo = null, //(new RepositoryProvider(callerUserName)).GetResolveMasterKObjectForDBObject(dbObject),
        //        TypeUri = dbObject.TypeUri
        //    };
        //}

        //private DBObject ConvertKObjectToDBObject(KObject kObject)
        //{
        //    return new DBObject()
        //    {
        //        LabelPropertyID = kObject.LabelPropertyID,
        //        Id = kObject.Id,
        //        IsGroup = kObject.IsGroup,
        //        TypeUri = kObject.TypeUri,
        //        ResolvedTo = RepositoryProvider.GetDBObjectResolvedToFieldForKObject(kObject)
        //    };
        //}

        //private DBModifiedProperty ConvertKModifiedPropertyToDBModifiedProperty(ModifiedProperty kModifiedProperty)
        //{
        //    return new DBModifiedProperty()
        //    {
        //        Id = kModifiedProperty.Id,
        //        NewValue = kModifiedProperty.NewValue
        //    };
        //}

        //private ModifiedProperty ConvertKModifiedPropertyToDBModifiedProperty(DBModifiedProperty dbModifiedProperty)
        //{
        //    return new ModifiedProperty()
        //    {
        //        Id = dbModifiedProperty.Id,
        //        NewValue = dbModifiedProperty.NewValue
        //    };
        //}

        //private DBModifiedConcepts ConvertKModifiedConceptToDBModifiedConcept(ModifiedConcepts kModifiedConcept)
        //{
        //    DBModifiedConcepts dbModifiedConcept = new DBModifiedConcepts();
        //    List<DBModifiedProperty> dbModifiedPropertyList = new List<DBModifiedProperty>();

        //    if (kModifiedConcept == null)
        //        return dbModifiedConcept;

        //    if (kModifiedConcept.ModifiedProperties != null)
        //        for (int i = 0; i < kModifiedConcept.ModifiedProperties.Count; i++)
        //        {
        //            dbModifiedPropertyList.Add(ConvertKModifiedPropertyToDBModifiedProperty(kModifiedConcept.ModifiedProperties[i]));
        //        }
        //    dbModifiedConcept.ModifiedPropertyList = (dbModifiedPropertyList != null) ? dbModifiedPropertyList.ToArray() : null;
        //    dbModifiedConcept.DeletedMediaIDList = (kModifiedConcept.DeletedMedias != null) ? kModifiedConcept.DeletedMedias.Select(m => m.Id).ToArray() : null;

        //    return dbModifiedConcept;
        //}

        public PublishResult Publish(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID = -1, bool isContinousPublish = false)
        {
            if (addedConcept == null)
                throw new ArgumentNullException("addedConcept");
            if (modifiedConcept == null)
                throw new ArgumentNullException("modifiedConcept");

            if (!AreConceptsIdValid(addedConcept.AddedObjects.Select(o => o.Id))
                || !AreConceptsIdValid(addedConcept.AddedProperties.Select(p => p.Id))
                || !AreConceptsIdValid(addedConcept.AddedProperties.Select(p => p.Owner.Id))
                || !AreConceptsIdValid(addedConcept.AddedRelationships.Select(r => r.Relationship.Id))
                || !AreConceptsIdValid(addedConcept.AddedRelationships.Select(r => r.Source.Id))
                || !AreConceptsIdValid(addedConcept.AddedRelationships.Select(r => r.Target.Id))
                || !AreConceptsIdValid(addedConcept.AddedMedias.Select(m => m.Id))
                || !AreConceptsIdValid(addedConcept.AddedMedias.Select(m => m.OwnerObjectId))
                || !AreConceptsIdValid(modifiedConcept.ModifiedProperties.Select(p => p.Id))
                || !AreConceptsIdValid(modifiedConcept.DeletedMedias.Select(m => m.Id))
                || !AreConceptsIdValid(modifiedConcept.DeletedMedias.Select(m => m.OwnerObjectId))
                )
            {
                throw new ArgumentOutOfRangeException();
            }

            //SaveChangesInRepository(addedConcept, modifiedConcept, resolvedObjects, dataSourceID);

            Task<bool> horizonSyncTask = SynchronizeHorizonServer(addedConcept, modifiedConcept, dataSourceID, isContinousPublish);
            Task<bool> searchSyncTask = SynchronizeSearchServer(addedConcept, modifiedConcept, dataSourceID, isContinousPublish);
            Task.WaitAll(new Task[] { horizonSyncTask, searchSyncTask });

            bool horizonServerSynchronized = horizonSyncTask.Result;
            bool searchServerSynchronized = searchSyncTask.Result;
            if (!horizonServerSynchronized)
            {
                //Add new and modified unsync objects
                List<long> unsyncObjectIds = new List<long>();
                unsyncObjectIds.AddRange(addedConcept.AddedObjects.Select(o => o.Id));
                unsyncObjectIds.AddRange(addedConcept.AddedProperties.Select(o => o.Owner.Id));
                unsyncObjectIds.AddRange(modifiedConcept.ModifiedProperties.Select(o => o.OwnerObjectID));
                AddUnsyncObjectsToHorizonServerUnsyncTable(unsyncObjectIds.Distinct().ToList());
                //Add new unsync relationships
                List<long> unsyncRelationshipIds = new List<long>();
                unsyncRelationshipIds.AddRange(addedConcept.AddedRelationships.Select(o => o.Relationship.Id));
                AddUnsyncRelationshipsToHorizonServerUnsyncTable(unsyncRelationshipIds);
            }
            if (!searchServerSynchronized)
            {
                List<long> unsyncObjectIds = new List<long>();
                unsyncObjectIds.AddRange(addedConcept.AddedObjects.Select(o => o.Id));
                unsyncObjectIds.AddRange(addedConcept.AddedProperties.Select(o => o.Owner.Id));
                unsyncObjectIds.AddRange(addedConcept.AddedRelationships.Select(o => o.Source.Id));
                unsyncObjectIds.AddRange(modifiedConcept.ModifiedProperties.Select(o => o.OwnerObjectID));
                AddUnsyncObjectsToSearchServerUnsyncTable(unsyncObjectIds.Distinct().ToList());
            }
            return new PublishResult()
            {
                HorizonServerSynchronized = horizonServerSynchronized,
                HorizonServerSyncDuration = horizonServerSyncDuration,
                SearchServerSynchronized = searchServerSynchronized,
                SearchServerSyncDuration = searchServerSyncDuration,
                RepositoryStoreDuration = repositoryStoreDuration
            };
        }

        public void SynchronizeDataSourceInSearchServer(DataSourceInfo dataSourceInfo)
        {
            SearchService.ServiceClient proxy = null;
            SynchronizationResult result;
            try
            {
                proxy = new SearchService.ServiceClient();
                result = proxy.SynchronizeDataSource(dataSourceInfo);
            }
            catch (Exception ex)
            {
                AddUnsyncDataSourceToSearchServerUnsyncTable(dataSourceInfo.Id);
                ReportSearchServerDataSourceSyncExceptionAdminEvent(ex, dataSourceInfo.Id);
                throw;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            if (!result.IsCompletelySynchronized)
            {
                ReportSearchServerObjectIncompletedSyncAdminEvent(result.SyncronizationLog);
                throw new ServerException($"Unable to Sync Data-Source in 'Search Server'; Sync log:{Environment.NewLine}{result.SyncronizationLog}");
            }
        }

        private void AddUnsyncDataSourceToSearchServerUnsyncTable(long unSyncId)
        {
            List<long> unSyncIds = new List<long>();
            unSyncIds.Add(unSyncId);
            SearchIndexesSynchronizationDatabaseAccess searchIndexesSynchronizationDatabaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            searchIndexesSynchronizationDatabaseAccess.RegisterUnpublishedConcepts(unSyncIds, SearchIndecesSynchronizationTables.SearchServerUnsyncDataSources);
        }

        private void AddUnsyncObjectsToHorizonServerUnsyncTable(List<long> unSyncIds)
        {
            SearchIndexesSynchronizationDatabaseAccess searchIndexesSynchronizationDatabaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            searchIndexesSynchronizationDatabaseAccess.RegisterUnpublishedConcepts(unSyncIds, SearchIndecesSynchronizationTables.HorizonServerUnsyncObjects);
        }
        private void AddUnsyncRelationshipsToHorizonServerUnsyncTable(List<long> unSyncIds)
        {
            SearchIndexesSynchronizationDatabaseAccess searchIndexesSynchronizationDatabaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            searchIndexesSynchronizationDatabaseAccess.RegisterUnpublishedConcepts(unSyncIds, SearchIndecesSynchronizationTables.HorizonServerUnsyncRelationships);
        }
        private void AddUnsyncObjectsToSearchServerUnsyncTable(List<long> unSyncIds)
        {
            SearchIndexesSynchronizationDatabaseAccess searchIndexesSynchronizationDatabaseAccess = new SearchIndexesSynchronizationDatabaseAccess();
            searchIndexesSynchronizationDatabaseAccess.RegisterUnpublishedConcepts(unSyncIds, SearchIndecesSynchronizationTables.SearchServerUnsyncObjects);
        }

        //private void SaveChangesInRepository(AddedConcepts kAddedConcept, ModifiedConcepts modifiedConcept, ResolvedObject[] resolvedObjects, long dataSourceID)
        //{
        //    DateTime SaveStartTimeStamp = DateTime.Now;
        //    // تبدیل کلاس ها در Dispatch Server به کلاس های موجود در Repository server.
        //    DBAddedConcepts dbAddedConcept = ConvertKAddedConceptToDBAddedConcept(kAddedConcept);
        //    DBModifiedConcepts dbModifiedConcept = ConvertKModifiedConceptToDBModifiedConcept(modifiedConcept);
        //    DBResolvedObject[] dbResolvedOBjects = ConvertKResolvedObjectsToDBResolvedObjects(resolvedObjects);

        //    ServiceAccess.RepositoryService.ServiceClient repositoryProxy = null;
        //    try
        //    {
        //        repositoryProxy = new ServiceAccess.RepositoryService.ServiceClient();
        //        repositoryProxy.Publish(dbAddedConcept, dbModifiedConcept, dbResolvedOBjects, dataSourceID);
        //    }
        //    finally
        //    {
        //        if (repositoryProxy != null)
        //            repositoryProxy.Close();
        //    }
        //    repositoryStoreDuration = DateTime.Now - SaveStartTimeStamp;
        //}

        //private DBResolvedObject[] ConvertKResolvedObjectsToDBResolvedObjects(ResolvedObject[] resolvedObjects)
        //{
        //    DBResolvedObject[] dbResObjs = new DBResolvedObject[resolvedObjects.Length];
        //    for (int i = 0; i < resolvedObjects.Length; i++)
        //    {
        //        dbResObjs[i] = new DBResolvedObject()
        //        {
        //            ResolutionMasterObjectID = resolvedObjects[i].ResolutionMasterObjectID,
        //            ResolvedObjectIDs = resolvedObjects[i].ResolutionCondidateObjectIDs,
        //            MatchedProperties = ConvertKMatchedPropertiesToDBMatchProperties(resolvedObjects[i].MatchedProperties)
        //        };
        //    }
        //    return dbResObjs;
        //}

        //private DBMatchedProperty[] ConvertKMatchedPropertiesToDBMatchProperties(MatchedProperty[] matchedProperties)
        //{
        //    DBMatchedProperty[] result = new DBMatchedProperty[matchedProperties.Length];
        //    for (int i = 0; i < matchedProperties.Length; i++)
        //    {
        //        result[i] = new DBMatchedProperty()
        //        {
        //            TypeUri = matchedProperties[i].TypeUri,
        //            Value = matchedProperties[i].Value
        //        };
        //    }
        //    return result;
        //}

        private async Task<bool> SynchronizeHorizonServer(AddedConcepts addedConcepts, ModifiedConcepts modifiedConcepts, long dataSourceID, bool isContinousPublish)
        {
            DateTime SyncStartTimeStamp = DateTime.Now;
            bool synchronized = false;

            if (!addedConcepts.AddedObjects.Any() &&
                !addedConcepts.AddedProperties.Any() &&
                !addedConcepts.AddedRelationships.Any() &&
                !addedConcepts.AddedMedias.Any() &&
                !modifiedConcepts.ModifiedProperties.Any() &&
                !modifiedConcepts.DeletedMedias.Any())
            {
                horizonServerSyncDuration = DateTime.Now - SyncStartTimeStamp;
                return true;
            }

            ServiceAccess.HorizonService.ServiceClient horizonProxy = null;
            try
            {
                horizonProxy = new ServiceAccess.HorizonService.ServiceClient();
                Entities.Publish.SynchronizationResult result = await horizonProxy.SyncPublishChangesAsync
                    (addedConcepts, modifiedConcepts, dataSourceID, isContinousPublish);

                if (result.IsCompletelySynchronized)
                {
                    synchronized = true;
                }
                else
                {
                    synchronized = false;
                    ReportHorizonServerIncompleteSyncAdminEvent(result.SyncronizationLog);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    ExceptionHandler errorLogger = new ExceptionHandler();
                    errorLogger.WriteErrorLog(ex);
                }
                catch
                { }

                List<long> unsyncObjectIDs = new List<long>();
                unsyncObjectIDs.AddRange(addedConcepts.AddedObjects.Select(o => o.Id));

                List<long> unsyncPropertyIDs = new List<long>();
                unsyncPropertyIDs.AddRange(addedConcepts.AddedProperties.Select(p => p.Id));
                unsyncPropertyIDs.AddRange(modifiedConcepts.ModifiedProperties.Select(p => p.Id));

                List<long> unsyncRelationshipIDs = new List<long>
                    (addedConcepts.AddedRelationships.Select(r => r.Relationship.Id));

                ReportHorizonServerSyncExceptionAdminEvent(ex, unsyncObjectIDs, unsyncPropertyIDs, unsyncRelationshipIDs);
            }
            finally
            {
                if (horizonProxy != null)
                    horizonProxy.Close();
            }

            horizonServerSyncDuration = DateTime.Now - SyncStartTimeStamp;

            return synchronized;
        }

        private void ReportHorizonServerIncompleteSyncAdminEvent(string log)
        {
            var reporter = new AdministrativeEventReporter();
            reporter.Report(string.Format("Synchronizing 'Horizon Server' was not completed.{0}Sync log:{0}{1}", Environment.NewLine, log));
        }
        private void ReportHorizonServerSyncExceptionAdminEvent(Exception ex, List<long> unsyncObjectIDs, List<long> unsyncPropertyIDs, List<long> unsyncRelationshipIDs)
        {
            var reporter = new AdministrativeEventReporter();
            reporter.Report(string.Format("Unable to sync 'Horizon Server' during Publish process.{0}Exception:{0}{1}{0}Un-Sync. object IDs: {2}{0}Un-Sync. property IDs: {3}{0}Un-Sync. relationship IDs: {4}{0}Objects with un-sync."
                    , Environment.NewLine, (new ExceptionDetailGenerator()).GetDetails(ex)
                    , GetCommaSeparatedStringForIDs(unsyncObjectIDs)
                    , GetCommaSeparatedStringForIDs(unsyncPropertyIDs)
                    , GetCommaSeparatedStringForIDs(unsyncRelationshipIDs)));
        }

        private void ReportSearchServerObjectIncompletedSyncAdminEvent(string log)
        {
            var reporter = new AdministrativeEventReporter();
            reporter.Report(string.Format("Synchronizing Object(s) in 'Search Server' was not completed.{0}Sync log:{0}{1}", Environment.NewLine, log));
        }
        private void ReportSearchServerObjectSyncExceptionAdminEvent(Exception ex, IEnumerable<long> unsyncObjectIDs, IEnumerable<long> unsyncProprtiesIDs)
        {
            var reporter = new AdministrativeEventReporter();
            reporter.Report(string.Format("Unable to sync 'Search Server' during Publish process.{0}Exception:{0}{1}{0}Un-Sync. object IDs: {2}{0}Un-Sync. property IDs: {3}{0}"
                    , Environment.NewLine, (new ExceptionDetailGenerator()).GetDetails(ex)
                    , GetCommaSeparatedStringForIDs(unsyncObjectIDs)
                    , GetCommaSeparatedStringForIDs(unsyncProprtiesIDs)));
        }

        private void ReportSearchServerDataSourceIncompleteSyncAdminEvent(string log)
        {
            var reporter = new AdministrativeEventReporter();
            reporter.Report(string.Format("Synchronizing Data-Source(s) in 'Search Server' was not completed.{0}Sync log:{0}{1}", Environment.NewLine, log));
        }
        private void ReportSearchServerDataSourceSyncExceptionAdminEvent(Exception ex, long unsyncDataSourceID)
        {
            var reporter = new AdministrativeEventReporter();
            reporter.Report(string.Format("Unable to sync 'Search Server' during DataSource Register process.{0}Exception:{0}{1}{0}Un-Sync. DataSource ID: {2} Un-Sync."
                    , Environment.NewLine, (new ExceptionDetailGenerator()).GetDetails(ex)
                    , unsyncDataSourceID));
        }

        private object GetCommaSeparatedStringForIDs(IEnumerable<long> ids)
        {
            string result = "";
            foreach (int id in ids)
            {
                result += id.ToString() + ',';
            }
            // حذف آخرین کاما (که اضافه است) درصورت وجود
            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }

        private async Task<bool> SynchronizeSearchServer(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID, bool isContinousPublish)
        {
            DateTime SyncStartTimeStamp = DateTime.Now;
            bool synchronized = false;

            var added = new SearchService.AddedConcepts();
            added.AddedObjects
                = addedConcept.AddedObjects
                    .Select(o => ConvertKObjectToSearchObject(o))
                    .ToArray();
            added.AddedProperties
                = addedConcept.AddedProperties
                    .Select(p => ConvertKPropertyToSearchProperty(p))
                    .ToArray();
            added.AddedRelationships
                = addedConcept.AddedRelationships
                    .Select(r => ConvertKRelationshipToSearchRelationship(r))
                    .ToArray();
            added.AddedMedias
                = addedConcept.AddedMedias
                    .Select(m => ConvertKMediaToSearchMedia(m))
                    .ToArray();

            var modified = new SearchService.ModifiedConcepts();
            modified.ModifiedProperties
                = modifiedConcept.ModifiedProperties
                    .Select(mp => new SearchService.ModifiedProperty() { ID = mp.Id, TypeUri = mp.TypeUri, newValue = mp.NewValue, OwnerObjectID = mp.OwnerObjectID })
                    .ToArray();
            modified.DeletedMedias
                = modifiedConcept.DeletedMedias
                    .Select(dm => new SearchService.SearchMedia() { Description = dm.Description, Id = dm.Id, OwnerObjectId = dm.OwnerObjectId, URI = dm.URI })
                    .ToArray();

            if (!added.AddedObjects.Any() &&
                !added.AddedProperties.Any() &&
                !added.AddedMedias.Any() &&
                !modified.ModifiedProperties.Any() &&
                !modified.DeletedMedias.Any())
            {
                searchServerSyncDuration = DateTime.Now - SyncStartTimeStamp;
                return true;
            }
            SearchService.ServiceClient searchProxy = null;
            try
            {
                searchProxy = new SearchService.ServiceClient();
                SynchronizationResult result = await searchProxy.SyncPublishChangesAsync
                    (added, modified, dataSourceID, isContinousPublish);

                if (result.IsCompletelySynchronized)
                {
                    synchronized = true;
                }
                else
                {
                    synchronized = false;
                    ReportSearchServerObjectIncompletedSyncAdminEvent(result.SyncronizationLog);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    ExceptionHandler errorLogger = new ExceptionHandler();
                    errorLogger.WriteErrorLog(ex);
                }
                catch
                { }

                List<long> unsyncObjectsIDs = new List<long>();
                unsyncObjectsIDs.AddRange(added.AddedObjects.Select(o => o.Id));
                unsyncObjectsIDs.AddRange(added.AddedProperties.Select(p => p.OwnerObject.Id));
                unsyncObjectsIDs.AddRange(added.AddedMedias.Select(m => m.OwnerObjectId));
                unsyncObjectsIDs.AddRange(modifiedConcept.ModifiedProperties.Select(p => p.OwnerObjectID));
                unsyncObjectsIDs.AddRange(modified.DeletedMedias.Select(m => m.OwnerObjectId));

                // از آنجایی که در سرور جستجو ویژگی اشیا به تنهایی موضوعیت ندارند
                // تمام اشیایی که خود یا ویژگی‌هایشان در فرایند انتشار همگام نشده‌اند
                // در گزارش ذکر می‌شوند
                ReportSearchServerObjectSyncExceptionAdminEvent(ex, unsyncObjectsIDs.Distinct(), new long[] { });
            }
            finally
            {
                if (searchProxy != null)
                    searchProxy.Close();
            }

            searchServerSyncDuration = DateTime.Now - SyncStartTimeStamp;

            return synchronized;
        }

        private SearchService.SearchMedia ConvertKMediaToSearchMedia(KMedia kMedia)
        {
            return new ServiceAccess.SearchService.SearchMedia()
            {
                Description = kMedia.Description,
                Id = kMedia.Id,
                OwnerObjectId = kMedia.OwnerObjectId,
                URI = kMedia.URI
            };
        }

        private SearchService.SearchRelationship ConvertKRelationshipToSearchRelationship(RelationshipBaseKlink r)
        {
            return new SearchService.SearchRelationship()
            {
                Id = r.Relationship.Id,
                TypeUri = r.TypeURI,
                SourceObjectId = r.Source.Id,
                SourceObjectTypeUri = r.Source.TypeUri,
                TargetObjectId = r.Target.Id,
                TargetObjectTypeUri = r.Target.TypeUri,
                Direction = (int)r.Relationship.Direction,
                DataSourceID = r.Relationship.DataSourceID
            };
        }

        private SearchService.SearchProperty ConvertKPropertyToSearchProperty(KProperty p)
        {
            return new SearchService.SearchProperty()
            {
                Id = p.Id,
                OwnerObject = ConvertKObjectToSearchObject(p.Owner),
                TypeUri = p.TypeUri,
                Value = p.Value
            };
        }

        private SearchService.SearchObject ConvertKObjectToSearchObject(KObject kObject)
        {
            return new SearchService.SearchObject()
            {
                LabelPropertyID = kObject.LabelPropertyID.GetValueOrDefault(),
                Id = kObject.Id,
                TypeUri = kObject.TypeUri
            };
        }

        private bool AreConceptsIdValid(IEnumerable<long> ids)
        {
            foreach (var id in ids)
            {
                if (id <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void FinalizeContinousPublish()
        {
            var searchProxy = new SearchService.ServiceClient();
            searchProxy.FinalizeContinousPublish();
        }
        public bool CanPerformNewPublish()
        {
            try
            {
                SearchIndexesSynchronization searchIndexesSynchronization = new SearchIndexesSynchronization();
                if (searchIndexesSynchronization.GetUnsyncConceptsCount() < MaximumAcceptableUnsynchronized)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new PublishCurrentlyImpossible(ex.Message, ex);
            }
        }
    }
}
