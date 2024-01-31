using GPAS.DataSynchronization;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Access.DataClient;
using GPAS.Horizon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Horizon.Logic.Synchronization
{
    internal class RelationshipsSynchronizationAdaptor : ISynchronizationAdaptor
    {
        public string ConceptsTypeTitle { get => "Relationship"; }

        public void FinalizeContinousSynchronization()
        {
        }

        public async Task FinalizeSynchronization(CachedConcepts cachedConcepts)
        {
            RelationshipsSynchronizationCachedConcepts cachedRelationships = ValidateFilledCachedConcepts(cachedConcepts);
            if (cachedRelationships.SynchronizedIDs.Count == 0 && cachedRelationships.NotSynchronizeIDs.Count == 0)
                return;
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();
                var syncResult = new Access.DispatchService.SynchronizationChanges()
                {
                    SynchronizedConceptsIDs = cachedRelationships.SynchronizedIDs.ToArray(),
                    StayUnsynchronizeConceptsIDs = cachedRelationships.NotSynchronizeIDs.ToArray()
                };
                await client.ApplyHorizonRelationshipsSynchronizationResultAsync(syncResult);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        private RelationshipsSynchronizationCachedConcepts ValidateFilledCachedConcepts(CachedConcepts cachedConcepts)
        {
            if (!(cachedConcepts is RelationshipsSynchronizationCachedConcepts))
            {
                throw new NotSupportedException("Cached Concepts instance not supported");
            }
            RelationshipsSynchronizationCachedConcepts cachedObjects = (cachedConcepts as RelationshipsSynchronizationCachedConcepts);
            if (cachedObjects.RetrievedLinks == null)
            {
                throw new InvalidOperationException("Relationships not cached (correctly)!");
            }
            return cachedObjects;
        }

        public async Task<List<long>> GetOldestUnsyncConceptIDs(int stepSize)
        {
            Access.DispatchService.InfrastructureServiceClient client = null;
            try
            {
                client = new Access.DispatchService.InfrastructureServiceClient();

                return (await client.GetOldestHorizonUnsyncRelatioinshipsAsync(stepSize)).ToList();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        public async Task<CachedConcepts> RetrieveConceptsByIDs(IEnumerable<long> IDsToSync)
        {
            var retrieveDataClient = new RetrieveDataClient();
            List<AccessControled<RelationshipBaseKlink>> retrievedLinks = await retrieveDataClient.GetRelationshipsByIDsAsync(IDsToSync.Distinct().ToList());
            var cachedConcepts = new RelationshipsSynchronizationCachedConcepts(retrievedLinks);
            return cachedConcepts;
        }

        public async Task<long> RetrieveLastAssignedID()
        {
            Access.DispatchService.InfrastructureServiceClient sc = null;
            try
            {
                sc = new Access.DispatchService.InfrastructureServiceClient();
                return await sc.GetLastAssignedRelatioshshipIDAsync();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public void SynchronizeAllCachedConcepts(CachedConcepts cachedConcepts)
        {
            RelationshipsSynchronizationCachedConcepts cachedRelationship = ValidateFilledCachedConcepts(cachedConcepts);
            SynchronizeIndexesWithRetrievedRelationships(cachedRelationship.RetrievedLinks);
        }

        public void SynchronizeSpecificCachedConcept(long conceptID, CachedConcepts cachedConcepts)
        {
            RelationshipsSynchronizationCachedConcepts cachedRelationships = ValidateFilledCachedConcepts(cachedConcepts);

            bool linkFound = false;
            foreach (AccessControled<RelationshipBaseKlink> link in cachedRelationships.RetrievedLinks)
            {
                if (link.ConceptInstance.Relationship.Id.Equals(conceptID))
                {
                    SynchronizeIndexesWithRetrievedRelationships
                        (new List<AccessControled<RelationshipBaseKlink>>() { link });
                    linkFound = true;
                    break;
                }
            }

            if (!linkFound)
            {
                throw new InvalidOperationException($"Relationship '{conceptID}' not found in cached Relationships!");
            }
        }

        private void SynchronizeIndexesWithRetrievedRelationships(List<AccessControled<RelationshipBaseKlink>> retrievedRelationship)
        {
            AddedConceptsWithAcl addedConceptsWithAcl = new AddedConceptsWithAcl()
            {
                AddedObjects = new List<KObject>(),
                AddedProperties = new List<KProperty>(),
                AddedMedias = new List<KMedia>(),
                AddedRelationshipsWithAcl = retrievedRelationship
            };
            ModifiedConcepts modifiedConcepts = new ModifiedConcepts()
            {
                ModifiedProperties = new List<ModifiedProperty>(),
                DeletedMedias = new List<KMedia>()
            };
            DataChangeProvider dataSynchronizer = new DataChangeProvider();
            dataSynchronizer.SynchronizePublishChanges(addedConceptsWithAcl, modifiedConcepts);
        }
    }
}
