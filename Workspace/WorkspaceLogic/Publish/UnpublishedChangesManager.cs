using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Investigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.Publish
{
    /// <summary>
    /// این کلاس مدیریت مربوط به اشیاء منتشر شده را به عهده دارد
    /// </summary>
    public class UnpublishedChangesManager
    {
        public static async Task<SaveInvestigationUnpublishedConcepts> GetAllSaveInvestigationUnpublishedChanges()
        {
            SaveInvestigationUnpublishedConcepts unpublishedConcepts = new SaveInvestigationUnpublishedConcepts();
            DataAccessManager.UnpublishedChangesManager unpublishedChangesManager = new DataAccessManager.UnpublishedChangesManager();
            unpublishedConcepts = await unpublishedChangesManager.GetAllUnpublishedChangesAsync();
            return unpublishedConcepts;
        }
        public static async Task<UnpublishedConcepts> GetAllUnpublishedChangesAsync()
        {
            UnpublishedConcepts unpublishedConcepts = new UnpublishedConcepts();
            List<KWObject> unpublishedObjectChanges = new List<KWObject>();
            var unpublishObjects = (DataAccessManager.ObjectManager.GetUnpublishedChanges());
            unpublishedObjectChanges = unpublishedObjectChanges.Concat(unpublishObjects.AddedObjects).ToList();


            List<KWProperty> unpublishedProperyChanges = new List<KWProperty>();
            var unpublishProperties = DataAccessManager.PropertyManager.GetUnpublishedChanges();
            unpublishedProperyChanges = unpublishedProperyChanges.Concat(unpublishProperties.AddedProperties).ToList();
            unpublishedProperyChanges = unpublishedProperyChanges.Concat(unpublishProperties.ModifiedProperties).ToList();

            List<KWMedia> unpublishedMediaChanges = new List<KWMedia>();
            var unpublishedMedias = DataAccessManager.MediaManager.GetUnpublishedChanges();
            unpublishedMediaChanges = unpublishedMediaChanges.Concat(unpublishedMedias.AddedMedias).ToList();
            unpublishedMediaChanges = unpublishedMediaChanges.Concat(unpublishedMedias.DeletedMedias).ToList();

            List<KWRelationship> unpublishedKWRelationshipChanges = new List<KWRelationship>();
            var unpublishedKWRelationships = await DataAccessManager.LinkManager.GetUnpublishedChangesAsync();
            foreach (var item in unpublishedKWRelationships.AddedRelationships)
            {
                unpublishedKWRelationshipChanges.Add(item.Relationship);
            }
            unpublishedConcepts.unpublishedObjectChanges = unpublishedObjectChanges;
            unpublishedConcepts.unpublishedPropertyChanges = unpublishedProperyChanges;
            unpublishedConcepts.unpublishedMediaChanges = unpublishedMediaChanges;
            unpublishedConcepts.unpublishedRelationshipChanges = unpublishedKWRelationshipChanges;
            return unpublishedConcepts;
        }

        public static async Task<bool> IsAnyUnpublishedChangesAsync()
        {
            var propertyChanges = DataAccessManager.PropertyManager.GetUnpublishedChanges();
            if (propertyChanges.AddedProperties.Any() || propertyChanges.ModifiedProperties.Any())
                return true;

            var relationshipChanges = await DataAccessManager.LinkManager.GetUnpublishedChangesAsync();
            if (relationshipChanges.AddedRelationships.Any())
                return true;

            var objectChanges = DataAccessManager.ObjectManager.GetUnpublishedChanges();
            if (objectChanges.AddedObjects.Any())
                return true;

            var mediaChanges = DataAccessManager.MediaManager.GetUnpublishedChanges();
            if (mediaChanges.AddedMedias.Any() || mediaChanges.DeletedMedias.Any())
                return true;

            return false;
        }

        public static async Task<UnpublishedConcepts> GetSpecifiedUnpublishedChangesAsync(List<KWObject> kwObjects, List<KWLink> kwLinks)
        {

            UnpublishedConcepts unpublishedConcepts = new UnpublishedConcepts();
            var unpublishedObjectChanges = new List<KWObject>();
            var unpublishedPropertyChanges = new List<KWProperty>();
            var unpublishedMediaChanges = new List<KWMedia>();
            var unpublishedRelationshipChanges = new List<KWRelationship>();
            List<KWObject> kwObjectsWithIntermediaryEventS = AddLinksInnerObjectsToObjectsList(kwLinks, kwObjects);



            var ConceptList =
                new Tuple<List<KWObject>, List<KWProperty>, List<KWMedia>>
                (new List<KWObject>(), new List<KWProperty>(), new List<KWMedia>());

            foreach (var item in kwObjectsWithIntermediaryEventS)
            {
                //-------------Object-----------
                if (DataAccessManager.ObjectManager.IsUnpublishedObject(item))
                    unpublishedObjectChanges.Add(item);
                //------------------------------
                //------------Property----------

                var ps = await DataAccessManager.PropertyManager.GetPropertiesForObjectAsync(item);
                foreach (var prop in ps)
                {
                    if (DataAccessManager.PropertyManager.IsUnpublishedProperty(prop))
                        unpublishedPropertyChanges.Add(prop);
                    else if (DataAccessManager.PropertyManager.IsModifiedProperty(prop))
                        unpublishedPropertyChanges.Add(prop);
                }
                //------------------------------
                //-------------Media----------
                var ms = await DataAccessManager.MediaManager.GetMediaForObjectAsync(item);
                foreach (var media in ms)
                {
                    if (DataAccessManager.MediaManager.IsUnpublishedMedia(media))
                        unpublishedMediaChanges.Add(media);
                    else if (DataAccessManager.MediaManager.IsDeletedMedia(media))
                        unpublishedMediaChanges.Add(media);
                }
                //------------------------------

                // TODO: Resolved Objects may set here.
            }
            var kwRelationships = GetSelectedUnPublishedRelationship(kwObjectsWithIntermediaryEventS, kwLinks);
            foreach (var kwRelationship in kwRelationships)
            {
                unpublishedRelationshipChanges.Add(kwRelationship);
            }

            unpublishedConcepts.unpublishedObjectChanges = unpublishedObjectChanges;
            unpublishedConcepts.unpublishedPropertyChanges = unpublishedPropertyChanges;
            unpublishedConcepts.unpublishedMediaChanges = unpublishedMediaChanges;
            unpublishedConcepts.unpublishedRelationshipChanges = unpublishedRelationshipChanges;
            return unpublishedConcepts;
        }


        private UnpublishedChangesManager()
        {

        }


        private static List<KWObject> AddLinksInnerObjectsToObjectsList(List<KWLink> kwLinks, List<KWObject> kwObjects)
        {
            foreach (var link in kwLinks)
            {
                if (link is EventBasedKWLink)
                {
                    if (IsSourceOrTargetOfLinkWillPublishedAfterPublishingTheLink(link, kwObjects))
                    {
                        kwObjects.Add((link as EventBasedKWLink).IntermediaryEvent);
                    }
                }
            }
            return kwObjects;
        }


        private static bool IsSourceOrTargetOfLinkWillPublishedAfterPublishingTheLink(KWLink kwLink, List<KWObject> kwObjects)
        {
            if (kwObjects.Contains(kwLink.Source))
            {
                if (kwObjects.Contains(kwLink.Target) || (!DataAccessManager.ObjectManager.IsUnpublishedObject(kwLink.Target)))
                {
                    return true;
                }
                else
                    if (!DataAccessManager.ObjectManager.IsUnpublishedObject(kwLink.Target))
                {
                    return true;
                }
            }
            else if (kwObjects.Contains(kwLink.Target))
            {
                if (kwObjects.Contains(kwLink.Source) || (!DataAccessManager.ObjectManager.IsUnpublishedObject(kwLink.Source)))
                {
                    return true;
                }
                else
                    if (!DataAccessManager.ObjectManager.IsUnpublishedObject(kwLink.Source))
                {
                    return true;
                }
            }
            else if ((!DataAccessManager.ObjectManager.IsUnpublishedObject(kwLink.Source)) &&
                (!DataAccessManager.ObjectManager.IsUnpublishedObject(kwLink.Target)))
            {
                return true;
            }
            return false;
        }

        private static List<KWRelationship> GetSelectedUnPublishedRelationship(List<KWObject> kwObjects, List<KWLink> kwLinks)
        {
            var kwRelationships = new List<KWRelationship>();
            foreach (var item in kwLinks)
            {
                if ((item is EventBasedKWLink))
                {
                    if (DataAccessManager.LinkManager.IsUnpublishedRelationship((item as EventBasedKWLink).FirstRelationship))
                    {
                        if (IsSourceOrTargetOfLinkWillPublishedAfterPublishingTheLink(item, kwObjects))
                        {
                            kwRelationships.Add((item as EventBasedKWLink).FirstRelationship);
                            kwRelationships.Add((item as EventBasedKWLink).SecondRelationship);
                        }
                    }
                }
                else if (item is RelationshipBasedKWLink)
                {
                    if (DataAccessManager.LinkManager.IsUnpublishedRelationship((item as RelationshipBasedKWLink).Relationship))
                    {
                        if (IsSourceOrTargetOfLinkWillPublishedAfterPublishingTheLink(item, kwObjects))
                            kwRelationships.Add((item as RelationshipBasedKWLink).Relationship);
                    }
                }
            }
            return kwRelationships;
        }

        public static void ClearCache()
        {
            DataAccessManager.MediaManager.DiscardChanges();
            DataAccessManager.PropertyManager.DiscardChanges();
            DataAccessManager.LinkManager.DiscardChanges();
            DataAccessManager.ObjectManager.DiscardChanges();
        }
    }
}