using GPAS.Workspace.Entities;
using GPAS.Workspace.ViewModel.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GPAS.Workspace.DataAccessManager.LinkManager;

namespace GPAS.Workspace.Logic.Publish
{
    public class PendingChangesPublishManager
    {        

        private static List<KWObject> GetChangedObjectFromChangedProperties(List<KWProperty> unpublishedPropertyChanges)
        {
            List<KWObject> result = new List<KWObject>();
            foreach (var item in unpublishedPropertyChanges)
            {
                result.Add(item.Owner);
            }
            return result;
        }

        private static List<KWObject> GetChangedObjectFromChangedMedias(List<KWMedia> unpublishedMediaChanges)
        {
            List<KWObject> result = new List<KWObject>();
            foreach (var item in unpublishedMediaChanges)
            {
                result.Add(item.Owner);
            }
            return result;
        }

        public static async Task<Tuple<List<UnpublishedObject>, List<UnpublishedObject>, List<UnpublishedObject>>> GetUnpublishedObjects(UnpublishedConcepts unpublishedConcepts)
        {
            List<UnpublishedObject> unpublishedObjects = new List<UnpublishedObject>();
            
            List<KWObject> changedObjects = await GenerateUnpublishObjects(unpublishedConcepts);

            unpublishedObjects = GenerateChangedUnpublishedObject(unpublishedConcepts, changedObjects);

            List<UnpublishedLink> unpublishedLinks = await GenerateUnpublishedLinks(unpublishedConcepts.unpublishedRelationshipChanges);

            foreach (var currentUnpublishedObject in unpublishedObjects)
            {
                foreach (var currentUnpublishedLink in unpublishedLinks)
                {
                    if (currentUnpublishedLink.relatedSource == currentUnpublishedObject.RelatedKWObject ||
                        currentUnpublishedLink.relatedTarget == currentUnpublishedObject.RelatedKWObject)
                    {
                        currentUnpublishedObject.UnpublishedLinks.Add(currentUnpublishedLink);
                    }
                }
            }

            Tuple<List<UnpublishedObject>, List<UnpublishedObject>, List<UnpublishedObject>> sepetatedUnpublishedObjects = sepetateUnpublishedObjects(unpublishedObjects);

            return sepetatedUnpublishedObjects;
        }

        private static Tuple<List<UnpublishedObject>, List<UnpublishedObject>, List<UnpublishedObject>> sepetateUnpublishedObjects(List<UnpublishedObject> unpublishedObjects)
        {
            List<UnpublishedObject> changedUnpublishedObject = new List<UnpublishedObject>();
            List<UnpublishedObject> addedUnpublishedObject = new List<UnpublishedObject>();
            List<UnpublishedObject> deletedUnpublishedObject = new List<UnpublishedObject>();
            foreach (var currentUnpublishedObject in unpublishedObjects)
            {
                if (currentUnpublishedObject.ChangeType == UnpublishConceptChangeType.Added)
                {
                    addedUnpublishedObject.Add(currentUnpublishedObject);
                }
                else if (currentUnpublishedObject.ChangeType == UnpublishConceptChangeType.Changed)
                {
                    changedUnpublishedObject.Add(currentUnpublishedObject);
                }
                else if (currentUnpublishedObject.ChangeType == UnpublishConceptChangeType.Deleted)
                {
                    deletedUnpublishedObject.Add(currentUnpublishedObject);
                }
            }
            return new Tuple<List<UnpublishedObject>, List<UnpublishedObject>, List<UnpublishedObject>>(changedUnpublishedObject, addedUnpublishedObject, deletedUnpublishedObject);
        }

        private static List<UnpublishedObject> GenerateChangedUnpublishedObject(UnpublishedConcepts unpublishedConcepts, List<KWObject> changedObjects)
        {
            List<UnpublishedObject> result = new List<UnpublishedObject>();
            foreach (var currentObject in changedObjects)
            {
                result.Add(GenerateUnpublishedObject(unpublishedConcepts, currentObject));
            }            
            
            return result;
        }

        private static UnpublishedObject GenerateUnpublishedObject(UnpublishedConcepts unpublishedConcepts, KWObject currentObject)
        {
            UnpublishedObject result = new UnpublishedObject();

            if (ObjectManager.IsUnpublishObject(currentObject))
            {
                result.ChangeType = UnpublishConceptChangeType.Added;
            }

            else if (IsObjectPropertiesChanged(unpublishedConcepts.unpublishedPropertyChanges ,currentObject) ||
                IsObjectLinksChanged(unpublishedConcepts.unpublishedRelationshipChanges, currentObject) ||
                IsMediasChanged(unpublishedConcepts.unpublishedMediaChanges, currentObject))
            {
                result.ChangeType = UnpublishConceptChangeType.Changed;
            }            

            result.DisplayName = currentObject.GetObjectLabel();
            result.TypeURI = currentObject.TypeURI;
            result.UnpublishedProperties = GenerateUnpublishedProperties(unpublishedConcepts.unpublishedPropertyChanges, currentObject);

            //result.ResolvedObjects = GenerateResolvedObjects(unpublishedConcepts.ResolvedObjects, currentObject);

            result.UnpublishedMedias = GenerateUnpublishedMedias(unpublishedConcepts.unpublishedMediaChanges, currentObject);
            
            result.RelatedKWObject = currentObject;
            return result;
        }

        private static bool IsMediasChanged(List<KWMedia> unpublishedMediaChanges, KWObject currentObject)
        {
            bool result = false;
            if (unpublishedMediaChanges.Select(p => p.Owner.ID).Contains(currentObject.ID))
            {
                result = true;
            }
            return result;
        }

        //private static List<ResolvedObjectsVM> GenerateResolvedObjects(List<ObjectResolutionMap> resolvedObjects, KWObject currentObject)
        //{
        //    List<ResolvedObjectsVM> result = new List<ResolvedObjectsVM>();
        //    foreach (var currentresolvedObject in resolvedObjects)
        //    {
        //        if (currentresolvedObject.ResolveMaster.ID == currentObject.ID)
        //        {
        //            ResolvedObjectsVM resolvedObjectsVM = GenerateResolvedObjectsVM(currentresolvedObject);
        //            resolvedObjectsVM.relatedKWObject = currentresolvedObject;
        //            result.Add(unpublishedProperty);
        //        }
        //    }
        //    return result;
        //}

        //private static ResolvedObjectsVM GenerateResolvedObjectsVM(ObjectResolutionMap currentresolvedObject)
        //{
        //    UnpublishedProperty result = new UnpublishedProperty();
        //    if (DataAccessManager.PropertyManager.IsUnpublishedProperty(currentProperty))
        //    {
        //        result.ChangeType = UnpublishConceptChangeType.Added;
        //    }
        //    else if (PropertyManager.IsPropertyModified(currentProperty) && !DataAccessManager.PropertyManager.IsUnpublishedProperty(currentProperty))
        //    {
        //        result.ChangeType = UnpublishConceptChangeType.Changed;
        //    }
        //    else
        //    {
        //        result.ChangeType = UnpublishConceptChangeType.Unchanged;
        //    }
        //    result.UnpublishedPropertyType = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentProperty.TypeURI);
        //    result.UnpublishedPropertyValue = currentProperty.Value;
        //    return result;
        //}

        private static bool IsObjectLinksChanged(List<KWRelationship> unpublishedRelationshipChanges, KWObject currentObject)
        {
            bool result = false;
            foreach (var currentRelationship in unpublishedRelationshipChanges)
            {
                CachedRelationshipMetadata relationshipMetadata = GetCachedMetadataById(currentRelationship.ID);
                if (relationshipMetadata.RelationshipSourceId == currentObject.ID ||
                    relationshipMetadata.RelationshipTargetId == currentObject.ID)
                {
                    result = true;
                }
            }
            return result;
        }

        private static bool IsObjectPropertiesChanged(List<KWProperty> unpublishedPropertyChanges, KWObject currentObject)
        {
            bool result = false;
            if (unpublishedPropertyChanges.Select(p=>p.Owner.ID).Contains(currentObject.ID))
            {
                result = true;
            }
            return result;
        }

        public static Dictionary<KWRelationship, UnpublishedLink> KWRelationshipToUnpublishedLinkDictionary = new Dictionary<KWRelationship, UnpublishedLink>();

        //private async static Task<List<UnpublishedLink>> GenerateUnpublishedLinks(List<KWRelationship> unpublishedRelationshipChanges, KWObject currentObject)
        //{
        //    List<UnpublishedLink> result = new List<UnpublishedLink>();            
        //    foreach (var currentRelationship in unpublishedRelationshipChanges)
        //    {
        //        var cachedMetadata = GetCachedMetadataById(currentRelationship.ID);
        //        var source = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipSourceId);
        //        var target = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipTargetId);
        //        if (source.ID == currentObject.ID ||
        //            target.ID == currentObject.ID)
        //        {
        //            result.Add(GetRelatedUnpublishedLink(currentRelationship));                    
        //        }
        //    }
        //    return result;
        //}

        private async static Task<List<UnpublishedLink>> GenerateUnpublishedLinks(List<KWRelationship> unpublishedRelationshipChanges)
        {
            List<UnpublishedLink> result = new List<UnpublishedLink>();
            foreach (var currentRelationship in unpublishedRelationshipChanges)
            {
                var cachedMetadata = GetCachedMetadataById(currentRelationship.ID);
                var source = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipSourceId);
                var target = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipTargetId);
                
                result.Add(GetRelatedUnpublishedLink(currentRelationship, source,target));
                
            }
            return result;
        }

        //private static UnpublishedLink GetRelatedUnpublishedLink(KWRelationship currentRelationship)
        //{
        //    UnpublishedLink result = null;
        //    if (KWRelationshipToUnpublishedLinkDictionary.ContainsKey(currentRelationship))
        //    {
        //        result = KWRelationshipToUnpublishedLinkDictionary[currentRelationship];
        //        result.relatedKWRelationship = currentRelationship;
        //    }
        //    else
        //    {
        //        result = GenerateUnpublishedLink(currentRelationship);
        //        result.relatedKWRelationship = currentRelationship;
        //        KWRelationshipToUnpublishedLinkDictionary.Add(currentRelationship, result);
        //    }            
        //    return result;
        //}

        private static UnpublishedLink GetRelatedUnpublishedLink(KWRelationship currentRelationship, KWObject source, KWObject target)
        {
            UnpublishedLink result = null;
            if (KWRelationshipToUnpublishedLinkDictionary.ContainsKey(currentRelationship))
            {
                result = KWRelationshipToUnpublishedLinkDictionary[currentRelationship];
                result.relatedKWRelationship = currentRelationship;
            }
            else
            {
                result = GenerateUnpublishedLink(currentRelationship);
                result.relatedKWRelationship = currentRelationship;
                result.relatedSource = source;
                result.relatedTarget = target;
                KWRelationshipToUnpublishedLinkDictionary.Add(currentRelationship, result);
            }
            return result;
        }

        private static UnpublishedLink GenerateUnpublishedLink(KWRelationship currentRelationship)
        {
            UnpublishedLink result = new UnpublishedLink();
            if (DataAccessManager.LinkManager.IsUnpublishedRelationship(currentRelationship))
            {
                result.ChangeType = UnpublishConceptChangeType.Added;
            }
            else
            {
                result.ChangeType = UnpublishConceptChangeType.Changed;
            }
            result.TypeURI = currentRelationship.TypeURI;
            result.Description = currentRelationship.Description;
            result.TimeBegin = currentRelationship.TimeBegin;
            result.TimeEnd = currentRelationship.TimeEnd;            
            return result;
        }

        private static List<UnpublishedMedia> GenerateUnpublishedMedias(List<KWMedia> unpublishedMediaChanges, KWObject currentObject)
        {
            List<UnpublishedMedia> result = new List<UnpublishedMedia>();
            foreach (var currentMedia in unpublishedMediaChanges)
            {
                if (currentMedia.Owner.ID == currentObject.ID)
                {
                    UnpublishedMedia unpublishedMedia = GenerateUnpublishedMedia(currentMedia);
                    unpublishedMedia.relatedKWMedia = currentMedia;
                    result.Add(unpublishedMedia);
                }
            }
            return result;
        }        

        private static List<UnpublishedProperty> GenerateUnpublishedProperties(List<KWProperty> unpublishedPropertyChanges, KWObject currentObject)
        {
            List<UnpublishedProperty> result = new List<UnpublishedProperty>();
            foreach (var currentProperty in unpublishedPropertyChanges)
            {
                if (currentProperty.Owner.ID == currentObject.ID)
                {
                    UnpublishedProperty unpublishedProperty = GenerateUnpublishedProperty(currentProperty);
                    unpublishedProperty.relatedKWProperty = currentProperty;
                    result.Add(unpublishedProperty);
                }
            }
            return result;
        }
        
        private static UnpublishedMedia GenerateUnpublishedMedia(KWMedia currentMedia)
        {
            UnpublishedMedia result = new UnpublishedMedia();
            var isMediaDeleteted = MediaManager.IsDeletedMedia(currentMedia);
            if (isMediaDeleteted)
            {
                result.ChangeType = UnpublishConceptChangeType.Deleted;
            }
            else
            {
                result.ChangeType = UnpublishConceptChangeType.Added;
            }
            result.UnpublishedMediaFilePath = currentMedia.MediaUri.ToString();
            return result;
        }

        private static UnpublishedProperty GenerateUnpublishedProperty(KWProperty currentProperty)
        {
            UnpublishedProperty result = new UnpublishedProperty();
            if (DataAccessManager.PropertyManager.IsUnpublishedProperty(currentProperty))
            {
                result.ChangeType = UnpublishConceptChangeType.Added;
            }
            else if (PropertyManager.IsPropertyModified(currentProperty) && !DataAccessManager.PropertyManager.IsUnpublishedProperty(currentProperty))
            {
                result.ChangeType = UnpublishConceptChangeType.Changed;
            }
            else
            {
                result.ChangeType = UnpublishConceptChangeType.Unchanged;
            }
            result.UnpublishedPropertyType = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentProperty.TypeURI);
            if (currentProperty.TypeURI.Equals(OntologyProvider.GetOntology().GetDateRangeAndLocationPropertyTypeUri()))
            {
                result.UnpublishedPropertyValue = (currentProperty as GeoTimeKWProperty).GeoTimeValue.ToString();
            }
            else
            {
                result.UnpublishedPropertyValue = currentProperty.Value;
            }
            
            return result;
        }
        
        private async static Task<List<KWObject>> GenerateUnpublishObjects(UnpublishedConcepts unpublishedConcepts)
        {
            List<KWObject> changedObjects = new List<KWObject>();
            foreach (var currentObject in GetChangedObjectFromChangedProperties(unpublishedConcepts.unpublishedPropertyChanges))
            {
                if (!changedObjects.Contains(currentObject))
                {
                    changedObjects.Add(currentObject);
                }
            }
            foreach (var currentObject in GetChangedObjectFromChangedMedias(unpublishedConcepts.unpublishedMediaChanges))
            {
                if (!changedObjects.Contains(currentObject))
                {
                    changedObjects.Add(currentObject);
                }
            }
            foreach (var currentObject in unpublishedConcepts.unpublishedObjectChanges)
            {
                if (!changedObjects.Contains(currentObject))
                {
                    changedObjects.Add(currentObject);
                }
            }

            foreach (var item in unpublishedConcepts.unpublishedRelationshipChanges)
            {
                var cachedMetadata = GetCachedMetadataById(item.ID);
                var source = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipSourceId);
                var target = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedMetadata.RelationshipTargetId);
                if (!changedObjects.Contains(source))
                {
                    changedObjects.Add(source);
                }
                if (!changedObjects.Contains(target))
                {
                    changedObjects.Add(target);
                }
            }
            return changedObjects;
        }



    }
}