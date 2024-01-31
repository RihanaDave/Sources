using GPAS.DataSynchronization;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.SearchServer.Logic.Synchronization
{
    internal class ObjectsSynchronizationCachedConcepts : CachedConcepts
    {
        internal AddedConceptsWithAcl TotallyAvailableConcepts { get; private set; }
        internal ModifiedConcepts ModifiedConcepts { get; private set; }

        public ObjectsSynchronizationCachedConcepts
            (AddedConceptsWithAcl totallyAvailableConcepts
            , ModifiedConcepts modifiedConcepts)
        {
            TotallyAvailableConcepts = totallyAvailableConcepts ?? throw new ArgumentNullException(nameof(totallyAvailableConcepts));
            ModifiedConcepts = modifiedConcepts ?? throw new ArgumentNullException(nameof(modifiedConcepts));
        }

        public override IEnumerable<long> GetCachedConceptIDs()
        {
            return TotallyAvailableConcepts.AddedNonDocumentObjects.Select(o => o.Id)
                .Concat(TotallyAvailableConcepts.AddedDocuments.Select(d => d.ConceptInstance.Id))
                .Concat(TotallyAvailableConcepts.AddedProperties.Select(p => p.ConceptInstance.OwnerObject.Id))
                .Concat(TotallyAvailableConcepts.AddedRelationships.Select(r => r.ConceptInstance.SourceObjectId))
                .Concat(TotallyAvailableConcepts.AddedMedias.Select(m => m.ConceptInstance.OwnerObjectId))
                .Concat(ModifiedConcepts.ModifiedProperties.Select(mp => mp.OwnerObjectID))
                .Concat(ModifiedConcepts.DeletedMedias.Select(dm => dm.OwnerObjectId))
                .Distinct();
        }

        public List<SearchObject> GetAddedNonDocumentObjectForSpecificObject(long objID)
        {
            foreach (SearchObject obj in TotallyAvailableConcepts.AddedNonDocumentObjects)
            {
                if (obj.Id.Equals(objID))
                {
                    return new List<SearchObject>() { obj };
                }
            }
            return new List<SearchObject>();
        }
        public List<AccessControled<SearchObject>> GetAddedDocumentObjectForSpecificObject(long objID)
        {
            foreach (AccessControled<SearchObject> acDoc in TotallyAvailableConcepts.AddedDocuments)
            {
                if (acDoc.ConceptInstance.Id.Equals(objID))
                {
                    return new List<AccessControled<SearchObject>>() { acDoc };
                }
            }
            return new List<AccessControled<SearchObject>>();
        }
        public List<AccessControled<SearchProperty>> GetAddedPropertiesForSpecificObject(long objID)
        {
            return TotallyAvailableConcepts.AddedProperties.Where(p => p.ConceptInstance.OwnerObject.Id == objID).ToList();
        }
        public List<AccessControled<SearchRelationship>> GetAddedRelationshipsSourcedSpecificObject(long objID)
        {
            return TotallyAvailableConcepts.AddedRelationships.Where(r => r.ConceptInstance.SourceObjectId == objID).ToList();
        }
        public List<AccessControled<SearchMedia>> GetAddedMediasForSpecificObject(long objID)
        {
            return TotallyAvailableConcepts.AddedMedias.Where(m => m.ConceptInstance.OwnerObjectId == objID).ToList();
        }
        public ModifiedProperty[] GetModifiedPropertiesForSpecificObject(long objID)
        {
            return ModifiedConcepts.ModifiedProperties.Where(mp => mp.OwnerObjectID == objID).ToArray();
        }
        public List<SearchMedia> GetDeletedMediasForSpecificObject(long objID)
        {
            return ModifiedConcepts.DeletedMedias.Where(dm => dm.OwnerObjectId == objID).ToList();
        }
    }
}