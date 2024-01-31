using GPAS.DataSynchronization;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Access.DataClient;
using GPAS.Horizon.Entities;
using GPAS.Horizon.GraphRepository;
using GPAS.PropertiesValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Horizon.Logic.Synchronization
{
    internal class ObjectsSynchronizationAdaptor : ISynchronizationAdaptor
    {
        public string ConceptsTypeTitle { get => "Object"; }

        public void FinalizeContinousSynchronization()
        {
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
                var syncResult = new Access.DispatchService.SynchronizationChanges()
                {
                    SynchronizedConceptsIDs = cachedObjects.SynchronizedIDs.ToArray(),
                    StayUnsynchronizeConceptsIDs = cachedObjects.NotSynchronizeIDs.ToArray()
                };
                await client.ApplyHorizonObjectsSynchronizationResultAsync(syncResult);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }
        private ObjectsSynchronizationCachedConcepts ValidateFilledCachedConcepts(CachedConcepts cachedConcepts)
        {
            if (!(cachedConcepts is ObjectsSynchronizationCachedConcepts))
            {
                throw new NotSupportedException("Cached Concepts instance not supported");
            }
            ObjectsSynchronizationCachedConcepts cachedObjects = (cachedConcepts as ObjectsSynchronizationCachedConcepts);
            if (cachedObjects.AddedObjects == null
                || cachedObjects.AddedProperties == null)
            {
                throw new InvalidOperationException("Objects (or their components) not cached (correctly)!");
            }
            return cachedObjects;
        }

        public async Task<List<long>> GetOldestUnsyncConceptIDs(int stepSize)
        {
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();

                return (await client.GetOldestHorizonUnsyncObjectsAsync(stepSize)).ToList();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        private readonly Ontology.Ontology ontology = OntologyProvider.GetOntology();

        public bool ConvertValidationToBoolean(string p1,string p2)
        {
            if (ValueBaseValidation.IsValidPropertyValue(ontology.GetBaseDataTypeOfProperty(p1), p2).Status==ValidationStatus.Valid)
            {
                return true;
            }
            else
            {
                return false;
            }           
        }
 
        public async Task<CachedConcepts> RetrieveConceptsByIDs(IEnumerable<long> IDsToSync)
        {
            var retrieveDataClient = new RetrieveDataClient();
            var propertyComparer = new ResetProcessPropertyEqualityComparer();
            List<KObject> retrievedObjects = await retrieveDataClient.RetrieveObjectsByIDsAsync(IDsToSync.Distinct().ToArray());
            List<KProperty> retrievedProperties;
            if (retrievedObjects.Count > 0)
            {
                List<long> retrievedObjectIDs = retrievedObjects.Select(r => r.Id).Distinct().ToList();
                retrievedProperties = retrieveDataClient.RetrievePropertiesOfObjects(retrievedObjectIDs).Where(p
                                => !string.IsNullOrWhiteSpace(p.Value)
                                && ConvertValidationToBoolean(p.TypeUri, p.Value))
                        .Distinct(propertyComparer)
                        .ToList();
            }
            else
            {
                retrievedProperties = new List<KProperty>();
            }
            var cachedConcepts = new ObjectsSynchronizationCachedConcepts
                (retrievedObjects, retrievedProperties, new List<ModifiedProperty>());
            return cachedConcepts;
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

        public void SynchronizeAllCachedConcepts(CachedConcepts cachedConcepts)
        {
            ObjectsSynchronizationCachedConcepts cachedObjects = ValidateFilledCachedConcepts(cachedConcepts);
            SynchronizeIndexesWithRetrievedConcepts
                (cachedObjects.AddedObjects
                , cachedObjects.AddedProperties
                , cachedObjects.ModifiedProperties);
        }

        public void SynchronizeSpecificCachedConcept(long objectID, CachedConcepts cachedConcepts)
        {
            ObjectsSynchronizationCachedConcepts cachedObjects = ValidateFilledCachedConcepts(cachedConcepts);
            SynchronizeIndexesWithRetrievedConcepts
                (cachedObjects.GetAddedObjectForSpecificObject(objectID)
                , cachedObjects.GetAddedPropertiesForSpecificObject(objectID)
                , cachedObjects.GetModifiedPropertiesForSpecificObject(objectID));
        }
        
        private void SynchronizeIndexesWithRetrievedConcepts
            (List<KObject> addedObjects, List<KProperty> addedProperties
            , List<ModifiedProperty> modifiedProperties)
        {
            if (addedObjects.Count > 0)
            {
                DeleteObjectIndexes(addedObjects);
            }
            AddedConceptsWithAcl addedConceptsWithAcl = new AddedConceptsWithAcl()
            {
                AddedObjects = addedObjects,
                AddedProperties = addedProperties,
                AddedMedias = new List<KMedia>(),
                AddedRelationshipsWithAcl = new List<AccessControled<RelationshipBaseKlink>>()
            };
            ModifiedConcepts modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = modifiedProperties,
                DeletedMedias = new List<KMedia>()
            };
            DataChangeProvider changeProvider = new DataChangeProvider();
            changeProvider.SynchronizePublishChanges(addedConceptsWithAcl, modifiedConcepts);
        }

        private void DeleteObjectIndexes(List<KObject> objects)
        {
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            IAccessClient accessClient = graphRepositoryProvider.GetNewSearchEngineClient();
            accessClient.OpenConnetion();
            foreach (var item in objects.GroupBy(o => o.TypeUri))
            {
                accessClient.DeleteVertices(item.Key, item.Select(o => o.Id).ToList());
            }
            accessClient.ApplyChanges();
        }
    }
}
