using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPAS.Workspace.DataAccessManager.LinkManager;
using static GPAS.Workspace.DataAccessManager.MediaManager;
using static GPAS.Workspace.DataAccessManager.ObjectManager;
using static GPAS.Workspace.DataAccessManager.PropertyManager;

namespace GPAS.Workspace.DataAccessManager
{
    public class UnpublishedChangesManager
    {
        internal CachedMetadatas GetAllCachedMetadata()
        {
            CachedMetadatas cachedMetadatas = new CachedMetadatas();

            List<CachedObjectMetadata> cachedObjectMetadatas = (DataAccessManager.ObjectManager.GetCachedObjectMetadatas());

            List<CachedPropertyMetadata> cachedPropertyMetadatas = DataAccessManager.PropertyManager.GetCachedPropertyMetadatas();

            List<CachedMediaMetadata> cachedMediaMetadatas = DataAccessManager.MediaManager.GetCachedMediaMetadatas();

            List<CachedRelationshipMetadata> unpublishedKWRelationships = DataAccessManager.LinkManager.GetCachedRelationshipMetadatas();

            cachedMetadatas.objectMetadatas = cachedObjectMetadatas;
            cachedMetadatas.propertyMetadatas = cachedPropertyMetadatas;
            cachedMetadatas.mediaMetadatas = cachedMediaMetadatas;
            cachedMetadatas.relationshipMetadatas = unpublishedKWRelationships;

            return cachedMetadatas;
        }

        public async Task<GPAS.Workspace.Entities.Investigation.SaveInvestigationUnpublishedConcepts> GetAllUnpublishedChangesAsync()
        {
            GPAS.Workspace.Entities.Investigation.SaveInvestigationUnpublishedConcepts unpublishedConcepts = new GPAS.Workspace.Entities.Investigation.SaveInvestigationUnpublishedConcepts();
            CachedMetadatas cachedMetadatas = GetAllCachedMetadata();

            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedObjectMetadata> cachedObjectMetadatas = ConvertCashedMetadataToSaveInvestigationObjectMetadata(cachedMetadatas.objectMetadatas);
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedPropertyMetadata> cachedPropertyMetadatas = await ConvertPropertyMetadataToSaveInvestigationPropertyMetadata(cachedMetadatas.propertyMetadatas);
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedMediaMetadata> cachedMediaMetadatas = await ConvertMediaMetadataToSaveInvestigationMediaMetadata(cachedMetadatas.mediaMetadatas);
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedRelationshipMetadata> cachedRelationshipMetadatas = ConvertRelationshipMetadataToSaveInvestigationRelationshipMetadata(cachedMetadatas.relationshipMetadatas);

            unpublishedConcepts.unpublishedObjectChanges = cachedObjectMetadatas;
            unpublishedConcepts.unpublishedPropertyChanges = cachedPropertyMetadatas;
            unpublishedConcepts.unpublishedMediaChanges = cachedMediaMetadatas;
            unpublishedConcepts.unpublishedRelationshipChanges = cachedRelationshipMetadatas;
            return unpublishedConcepts;
        }

        private List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedRelationshipMetadata> ConvertRelationshipMetadataToSaveInvestigationRelationshipMetadata(List<CachedRelationshipMetadata> relationshipMetadatas)
        {
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedRelationshipMetadata> result = new List<Entities.Investigation.UnpublishedChanges.CachedRelationshipMetadata>();

            foreach (var currentRelation in relationshipMetadatas)
            {
                result.Add(new Entities.Investigation.UnpublishedChanges.CachedRelationshipMetadata()
                {
                    CachedRelationship = LinkManager.GetKRelationshipFromKWRelationship(currentRelation.CachedRelationship),
                    IsPublished = currentRelation.IsPublished,
                    RelationshipSourceId = currentRelation.RelationshipSourceId,
                    RelationshipTargetId = currentRelation.RelationshipTargetId,
                    TypeURI = currentRelation.CachedRelationship.TypeURI
                });
            }

            return result;
        }

        private async Task<List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedMediaMetadata>> ConvertMediaMetadataToSaveInvestigationMediaMetadata(List<CachedMediaMetadata> mediaMetadatas)
        {
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedMediaMetadata> result = new List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedMediaMetadata>();

            List<KMedia> kMedias = await DataAccessManager.MediaManager.GetKMediasFromKWMedias(mediaMetadatas.Select(m => m.CachedMedia).ToList());
            foreach (var currentMediaMetadata in mediaMetadatas)
            {
                result.Add(new Entities.Investigation.UnpublishedChanges.CachedMediaMetadata()
                {
                    CachedMedia = kMedias.Where(m => m.Id == currentMediaMetadata.CachedMedia.ID).First(),
                    IsPublished = currentMediaMetadata.IsPublished,
                    IsDeleted = currentMediaMetadata.IsDeleted
                });
            }

            return result;
        }

        private async Task<List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedPropertyMetadata>> ConvertPropertyMetadataToSaveInvestigationPropertyMetadata(List<CachedPropertyMetadata> propertyMetadatas)
        {
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedPropertyMetadata> result = new List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedPropertyMetadata>();

            List<KProperty> kProperties = await DataAccessManager.PropertyManager.GetKPropertiesFromKWProperties(propertyMetadatas.Select(p => p.CachedProperty).ToList());
            foreach (var currentPropertyMetadata in propertyMetadatas)
            {
                result.Add(new Entities.Investigation.UnpublishedChanges.CachedPropertyMetadata()
                {
                    CachedProperty = kProperties.Where(p => p.Id == currentPropertyMetadata.CachedProperty.ID).First(),
                    IsModified = currentPropertyMetadata.IsModified,
                    IsPublished = currentPropertyMetadata.IsPublished
                });
            }

            return result;
        }

        private List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedObjectMetadata> ConvertCashedMetadataToSaveInvestigationObjectMetadata(List<CachedObjectMetadata> objectMetadatas)
        {
            List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedObjectMetadata> result = new List<GPAS.Workspace.Entities.Investigation.UnpublishedChanges.CachedObjectMetadata>();
            List<KObject> kObjects = DataAccessManager.ObjectManager.GetKObjectsFromKWObjects(objectMetadatas.Select(o => o.CachedObject).ToList());
            foreach (var currentObjectMetadata in objectMetadatas)
            {
                result.Add(new Entities.Investigation.UnpublishedChanges.CachedObjectMetadata()
                {
                    CachedObject = kObjects.Where(o => o.Id == currentObjectMetadata.CachedObject.ID).First(),
                    IsNotUploadedSourceDocument = currentObjectMetadata.IsNotUploadedSourceDocument,
                    ObjectsWhereLocallyResolvedTo = DataAccessManager.ObjectManager.GetKObjectsFromKWObjects(currentObjectMetadata.ObjectsWhereLocallyResolvedTo.ToList()).ToArray(),
                    IsLocallyResolved = currentObjectMetadata.IsLocallyResolved,
                    IsPublished = currentObjectMetadata.IsPublished,
                });
            }

            return result;



            //List<KMedia> kMedias = await DataAccessManager.MediaManager.GetKMediasFromKWMedias(unpublishedConcepts.unpublishedMediaChanges);

            //return new SaveInvestigationUnpublishedConcepts()
            //{
            //    unpublishedObjectChanges = kObjects,
            //    unpublishedPropertyChanges = kProperties,
            //    unpublishedMediaChanges = kMedias,
            //    unpublishedRelationshipChanges = await DataAccessManager.LinkManager.GetRelationshipBaseKlinksFromRelationships(unpublishedConcepts.unpublishedRelationshipChanges),
            //    ResolvedObjects = unpublishedConcepts.ResolvedObjects

            //};
        }

        public async Task AddUnpublishedConceptsToCache(GPAS.Workspace.Entities.Investigation.SaveInvestigationUnpublishedConcepts savedUnpublishedConcepts)
        {
            ObjectManager.ClearCache();
            PropertyManager.ClearCache();
            LinkManager.ClearCache();
            MediaManager.ClearCache();

            List<CachedObjectMetadata> objectsMetadata = await ConvertSavedInvestigationObjectMetadataToCachedMetadata(savedUnpublishedConcepts.unpublishedObjectChanges);
            ObjectManager.AddSavedMetadataToCache(objectsMetadata);

            List<CachedPropertyMetadata> propertiesMetadata = await ConvertSavedInvestigationPropertyMetadataToCachedMetadata(savedUnpublishedConcepts.unpublishedPropertyChanges);
            PropertyManager.AddSavedMetadataToCache(propertiesMetadata);

            SetKWObjectLableProperty(savedUnpublishedConcepts.unpublishedObjectChanges.Select(o=>o.CachedObject).ToList(),
                objectsMetadata.Select(o=>o.CachedObject).ToList(),
                propertiesMetadata.Select(p=>p.CachedProperty).ToList());

            List<CachedMediaMetadata> mediasMetadata = ConvertSavedInvestigationMediaMetadataToCachedMetadata(savedUnpublishedConcepts.unpublishedMediaChanges);
            MediaManager.AddSavedMetadataToCache(mediasMetadata);

            List<CachedRelationshipMetadata> relationsMetadata = ConvertSavedInvestigationRelationshipMetadataToCachedMetadata(savedUnpublishedConcepts.unpublishedRelationshipChanges);
            LinkManager.AddSavedMetadataToCache(relationsMetadata);
        }

        private void SetKWObjectLableProperty(List<KObject> kObjects, List<KWObject> kWObjects, List<KWProperty> kWProperties)
        {
            foreach (var currentObj in kObjects)
            {
                kWObjects.Where(o => o.ID == currentObj.Id).First().DisplayName =
                kWProperties.Where(p => p.ID == currentObj.LabelPropertyID).First();
            }
        }

        private List<LinkManager.CachedRelationshipMetadata> ConvertSavedInvestigationRelationshipMetadataToCachedMetadata(List<Entities.Investigation.UnpublishedChanges.CachedRelationshipMetadata> unpublishedRelationChanges)
        {
            List<LinkManager.CachedRelationshipMetadata> result = new List<CachedRelationshipMetadata>();

            foreach (var currentRelationMetadata in unpublishedRelationChanges)
            {
                result.Add(new CachedRelationshipMetadata()
                {
                    CachedRelationship = LinkManager.GetRelationshipFromReterievedData(currentRelationMetadata.CachedRelationship,
                    currentRelationMetadata.TypeURI, currentRelationMetadata.RelationshipSourceId, currentRelationMetadata.RelationshipTargetId),
                    IsPublished = currentRelationMetadata.IsPublished,
                    RelationshipSourceId = currentRelationMetadata.RelationshipSourceId,
                    RelationshipTargetId = currentRelationMetadata.RelationshipTargetId
                });
            }
            return result;
        }

        private List<MediaManager.CachedMediaMetadata> ConvertSavedInvestigationMediaMetadataToCachedMetadata(List<Entities.Investigation.UnpublishedChanges.CachedMediaMetadata> unpublishedMediaChanges)
        {
            List<MediaManager.CachedMediaMetadata> result = new List<CachedMediaMetadata>();
            ////await DataAccessManager.MediaManager.GetMediasFromRetrievedDataAsync
            ////    (savedUnpublishedConcepts.unpublishedMediaChanges.ToArray);
            ///
            return result;
        }

        private async Task<List<PropertyManager.CachedPropertyMetadata>> ConvertSavedInvestigationPropertyMetadataToCachedMetadata(List<Entities.Investigation.UnpublishedChanges.CachedPropertyMetadata> unpublishedPropertyChanges)
        {
            List<PropertyManager.CachedPropertyMetadata> result = new List<CachedPropertyMetadata>();

            List<KWProperty> kWProperties = (await DataAccessManager.PropertyManager.GetPropertyFromRetrievedDataArrayAsync
                (unpublishedPropertyChanges.Select(pc => pc.CachedProperty).ToArray())).ToList();

            foreach (var currentPropertyMetadata in unpublishedPropertyChanges)
            {
                result.Add(new CachedPropertyMetadata()
                {
                    CachedProperty = kWProperties.Where(p => p.ID == currentPropertyMetadata.CachedProperty.Id).First(),
                    IsModified = currentPropertyMetadata.IsModified,
                    IsPublished = currentPropertyMetadata.IsPublished
                });
            }
            return result;
        }

        private async Task<List<ObjectManager.CachedObjectMetadata>> ConvertSavedInvestigationObjectMetadataToCachedMetadata(List<Entities.Investigation.UnpublishedChanges.CachedObjectMetadata> unpublishedObjectChanges)
        {
            List<ObjectManager.CachedObjectMetadata> result = new List<ObjectManager.CachedObjectMetadata>();

            List<KWObject> kWObjects = await DataAccessManager.ObjectManager.GetObjectsFromRetrievedDataAsync
               (unpublishedObjectChanges.Select(o => o.CachedObject).ToArray());

            foreach (var currentObjectMetadata in unpublishedObjectChanges)
            {
                result.Add(new CachedObjectMetadata()
                {
                    CachedObject = kWObjects.Where(o => o.ID == currentObjectMetadata.CachedObject.Id).First(),
                    IsPublished = currentObjectMetadata.IsPublished,
                    IsLocallyResolved = currentObjectMetadata.IsLocallyResolved,
                    IsNotUploadedSourceDocument = currentObjectMetadata.IsNotUploadedSourceDocument,
                    ObjectsWhereLocallyResolvedTo = (await DataAccessManager.ObjectManager.GetObjectsFromRetrievedDataAsync
                    (currentObjectMetadata.ObjectsWhereLocallyResolvedTo.ToArray())).ToArray()
                });
            }
            return result;
        }
    }
}
