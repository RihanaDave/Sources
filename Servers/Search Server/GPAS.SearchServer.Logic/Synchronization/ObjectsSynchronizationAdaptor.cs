using GPAS.DataSynchronization;
using GPAS.PropertiesValidation;
using GPAS.SearchServer.Access.DataClient;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic.Synchronization
{
    internal class ObjectsSynchronizationAdaptor : ISynchronizationAdaptor
    {
        public string ConceptsTypeTitle { get => "Object"; }

        public void FinalizeContinousSynchronization()
        {
            IndexingProvider indexingProvider = new IndexingProvider();
            indexingProvider.FinalizeContinousIndexing();
        }

        public async Task<List<long>> GetOldestUnsyncConceptIDs(int stepSize)
        {
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();

                return (await client.GetOldestSearchUnsyncObjectsAsync(stepSize)).ToList();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }
        public bool convertValidationToBoolean(Ontology.BaseDataTypes baseDataTypes, string bucketCount)
        {
            if (ValueBaseValidation.IsValidPropertyValue(baseDataTypes, bucketCount).Status != PropertiesValidation.ValidationStatus.Invalid)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<CachedConcepts> RetrieveConceptsByIDs(IEnumerable<long> IDsToSync)
        {
            //var retrieveDataClient = new RetrieveDataClient();
            //var propertyComparer = new ResetProcessPropertyEqualityComparer();
            //var retrievedConcepts = new AddedConceptsWithAcl();
            //List<SearchObject> retrievedObjects = await retrieveDataClient.GetObjectsByIDsAsync(IDsToSync.Distinct().ToArray());
            //if (retrievedObjects.Count > 0)
            //{
            //    List<long> retrievedObjectIDs = retrievedObjects.Select(r => r.Id).Distinct().ToList();
            //    retrievedConcepts = new AddedConceptsWithAcl()
            //    {
            //        AddedProperties = retrieveDataClient.RetrievePropertiesOfObjects(retrievedObjectIDs).Where(sp
            //                    => !string.IsNullOrWhiteSpace(sp.ConceptInstance.Value)
            //                    && convertValidationToBoolean(ontology.GetBaseDataTypeOfProperty(sp.ConceptInstance.TypeUri), sp.ConceptInstance.Value))
            //            .Distinct(propertyComparer)
            //            .ToList(),
            //        AddedRelationships = retrieveDataClient.RetrieveRelationshipsSourcedByObjects(retrievedObjectIDs),
            //        AddedMedias = retrieveDataClient.RetrieveMediasOfObjects(retrievedObjectIDs.ToArray())
            //    };
            //    SetRetrievedObjectsToAddedConcepts(retrievedObjects, retrievedConcepts);
            //}
            //else
            //{
            //    retrievedConcepts = new AddedConceptsWithAcl()
            //    {
            //        AddedDocuments = new List<AccessControled<SearchObject>>(),
            //        AddedNonDocumentObjects = new List<SearchObject>(),
            //        AddedProperties = new List<AccessControled<SearchProperty>>(),
            //        AddedRelationships = new List<AccessControled<SearchRelationship>>(),
            //        AddedMedias = new List<AccessControled<SearchMedia>>()
            //    };
            //}
            //var cachedConcepts = new ObjectsSynchronizationCachedConcepts
            //    (retrievedConcepts
            //    , new ModifiedConcepts() { ModifiedProperties = new ModifiedProperty[] { }, DeletedMedias = new List<SearchMedia>() }
            //    , new List<ResolveMasterObject>());
            //return cachedConcepts;

            return null;
        }

        private readonly Ontology.Ontology ontology = OntologyProvider.GetOntology();

        private void SetRetrievedObjectsToAddedConcepts(List<SearchObject> retrievedObjects, AddedConceptsWithAcl cachedConcepts)
        {
            cachedConcepts.AddedNonDocumentObjects = new List<SearchObject>();
            cachedConcepts.AddedDocuments = new List<AccessControled<SearchObject>>();
            foreach (SearchObject obj in retrievedObjects)
            {
                if (!ontology.IsDocument(obj.TypeUri))
                {
                    cachedConcepts.AddedNonDocumentObjects.Add(obj);
                }
                else
                {
                    bool docAclFound = false;
                    foreach (var propWithAcl in cachedConcepts.AddedProperties)
                    {
                        if (propWithAcl.ConceptInstance.OwnerObject.Id.Equals(obj.Id))
                        {
                            cachedConcepts.AddedDocuments.Add(new AccessControled<SearchObject>()
                            {
                                ConceptInstance = obj,
                                Acl = propWithAcl.Acl
                            });
                            docAclFound = true;
                            break;
                        }
                    }
                    if (!docAclFound)
                    {
                        throw new InvalidOperationException($"Unable to retrieve ACL for Document: {obj.Id}");
                    }
                }
            }
        }

        public void SynchronizeAllCachedConcepts(CachedConcepts cachedConcepts)
        {
            ObjectsSynchronizationCachedConcepts cachedObjects = ValidateFilledCachedConcepts(cachedConcepts);
            SynchronizeIndexesWithRetrievedConcepts
                (cachedObjects.TotallyAvailableConcepts, cachedObjects.ModifiedConcepts);
        }

        private ObjectsSynchronizationCachedConcepts ValidateFilledCachedConcepts(CachedConcepts cachedConcepts)
        {
            if (!(cachedConcepts is ObjectsSynchronizationCachedConcepts))
            {
                throw new NotSupportedException("Cached Concepts instance not supported");
            }
            ObjectsSynchronizationCachedConcepts cachedObjects = (cachedConcepts as ObjectsSynchronizationCachedConcepts);
            if (cachedObjects.TotallyAvailableConcepts == null
                || cachedObjects.TotallyAvailableConcepts.AddedNonDocumentObjects == null
                || cachedObjects.TotallyAvailableConcepts.AddedDocuments == null
                || cachedObjects.TotallyAvailableConcepts.AddedProperties == null
                || cachedObjects.TotallyAvailableConcepts.AddedRelationships == null
                || cachedObjects.TotallyAvailableConcepts.AddedMedias == null
                || cachedObjects.ModifiedConcepts == null
                || cachedObjects.ModifiedConcepts.ModifiedProperties == null
                || cachedObjects.ModifiedConcepts.DeletedMedias == null)
            {
                throw new InvalidOperationException("Objects (or their components) not cached (correctly)!");
            }
            return cachedObjects;
        }

        public void SynchronizeSpecificCachedConcept(long objectID, CachedConcepts cachedConcepts)
        {
            ObjectsSynchronizationCachedConcepts cachedObjects = ValidateFilledCachedConcepts(cachedConcepts);
            AddedConceptsWithAcl totallyAvailableConcepts = new AddedConceptsWithAcl()
            {
                AddedNonDocumentObjects = cachedObjects.GetAddedNonDocumentObjectForSpecificObject(objectID),
                AddedDocuments = cachedObjects.GetAddedDocumentObjectForSpecificObject(objectID),
                AddedProperties = cachedObjects.GetAddedPropertiesForSpecificObject(objectID),
                AddedRelationships = cachedObjects.GetAddedRelationshipsSourcedSpecificObject(objectID),
                AddedMedias = cachedObjects.GetAddedMediasForSpecificObject(objectID)
            };
            ModifiedConcepts modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = cachedObjects.GetModifiedPropertiesForSpecificObject(objectID),
                DeletedMedias = cachedObjects.GetDeletedMediasForSpecificObject(objectID)
            };
            SynchronizeIndexesWithRetrievedConcepts(totallyAvailableConcepts, modifiedConcepts);
        }

        private static void SynchronizeIndexesWithRetrievedConcepts
            (AddedConceptsWithAcl addedConcepts, ModifiedConcepts modifiedConcepts)
        {
            var dataSynchronizer = new DataChangeProvider();
            dataSynchronizer.Synchronize(addedConcepts, modifiedConcepts, true);
        }
        
        public async Task FinalizeSynchronization(CachedConcepts cachedConcepts)
        {
            ObjectsSynchronizationCachedConcepts cachedObjects = ValidateFilledCachedConcepts(cachedConcepts);
            if (cachedObjects.SynchronizedIDs.Count == 0 && cachedObjects.NotSynchronizeIDs.Count == 0)
                return;
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();
                //var syncResult = new Access.DispatchService.SynchronizationChanges()
                var syncResult = new Dispatch.Entities.SynchronizationChanges()
                {
                    SynchronizedConceptsIDs = cachedObjects.SynchronizedIDs.ToArray(),
                    StayUnsynchronizeConceptsIDs = cachedObjects.NotSynchronizeIDs.ToArray()
                };
                await client.ApplySearchObjectsSynchronizationResultAsync(syncResult);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        public async Task<long> RetrieveLastAssignedID()
        {
            Access.DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new Access.DispatchService.InfrastructureServiceClient();
                return await sc.GetLastAssignedObjectIDAsync();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
    }
}
