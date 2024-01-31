using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.Transformation;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DAM = GPAS.Workspace.DataAccessManager;

namespace GPAS.Workspace.Logic.Publish
{
    /// <summary>
    /// این کلاس مدیریت مربوط به عملیات انتشار را بر عهده دارد
    /// </summary>
    public class PublishManager
    {
        private static Ontology.Ontology ontology = OntologyProvider.GetOntology();

        private PublishManager()
        { }
        public static void DiscardAllChanges()
        {
            DAM.ObjectManager.DiscardChanges();
            DAM.LinkManager.DiscardChanges();
            DAM.PropertyManager.DiscardChanges();
            DAM.MediaManager.DiscardChanges();
        }

        public static async Task<Tuple<KWObject, PublishResultMetadata>> 
            PublishImportedUnstructuredDataSourceAsync(string path, GPAS.DataImport.DataMapping.Unstructured.TypeMapping typeMapping, GPAS.AccessControl.ACL acl)
        {
            UnstructuredDataTransformer transformer = new UnstructuredDataTransformer(OntologyProvider.GetOntology());
            transformer.TransformConcepts(typeMapping);

            if (transformer.GeneratingObjects.Count > 0)
            {
                ImportingObject generatedImportingObject = transformer.GeneratingObjects.First();
                string label = generatedImportingObject.LabelProperty.Value;
                string type = generatedImportingObject.TypeUri;

                // 1
                KWObject obj = await ObjectManager.CreateNewObject(type, label);

                // 2         
                byte[] fileBytes = File.ReadAllBytes(path);

                // 3
                DataSourceProvider dsProvider = new DataSourceProvider();
                long dataSourceID = dsProvider.RegisterNewImportedUnstructuredDataSource(fileBytes, obj, acl);

                DataAccessManager.ObjectManager.SetDocumentSourceAsUploaded(obj);

                // 4
                List<KWProperty> objProperties = new List<KWProperty>();

                foreach (var currentProperty in generatedImportingObject.Properties)
                {
                    if (!currentProperty.TypeURI.Equals(OntologyProvider.GetOntology().GetDefaultDisplayNamePropertyTypeUri()))
                    {
                        KWProperty kWProperty = DataAccessManager.PropertyManager.CreateNewProperty(currentProperty.TypeURI, currentProperty.Value, obj);
                        objProperties.Add(kWProperty);
                    }
                }

                if (obj.DisplayName != null)
                {
                    objProperties.Add(obj.DisplayName);
                }


                PublishResultMetadata publishResult = await PublishSelectedConceptsAndSyncWorkspaceCacheAsync
                    (new List<KWObject>() { obj }
                    , objProperties, new List<KWMedia>()
                    , new List<KWRelationship>()
                    , dataSourceID);

                return new Tuple<KWObject, PublishResultMetadata>(obj, publishResult);
            }
            else
            {
                return null;
            }
        }

        public static async Task<PublishResultMetadata> PublishSpecifiedManuallyEnteredConcepts
            (List<KWObject> objectsForPublish, List<KWProperty> propertiesForPublish
            , List<KWMedia> mediasForPublish, List<KWRelationship> relationshipsForPublish)
        {
            DataSourceProvider dsProvider = new DataSourceProvider();
            long dataSourceID = dsProvider.RegisterNewManualyEnteredDataSource();
            return await PublishSelectedConceptsAndSyncWorkspaceCacheAsync
                (objectsForPublish, propertiesForPublish
                , mediasForPublish, relationshipsForPublish
                , dataSourceID);
        }

        private static async Task<PublishResultMetadata> PublishSelectedConceptsAndSyncWorkspaceCacheAsync
            (List<KWObject> objectsForPublish, List<KWProperty> propertiesForPublish
            , List<KWMedia> mediasForPublish, List<KWRelationship> relationshipsForPublish, long dataSourceID)
        {
            var objectChanges = DAM.ObjectManager.GetUnpublishedChanges();
            var propertyChanges = DAM.PropertyManager.GetUnpublishedChanges();
            var mediaChanges = DAM.MediaManager.GetUnpublishedChanges();
            var relationshipChanges = await DAM.LinkManager.GetUnpublishedChangesAsync();

            if (objectsForPublish.Count > 0 && !ObjectManager.AreDocumentsSourcesUploaded(objectsForPublish))
                throw new InvalidOperationException("One or more object selected for publish, are documents with not uploaded sources");

            AddedConcepts addedConcept = new AddedConcepts();
            addedConcept.AddedObjects
                = DAM.ObjectManager.ConvertKWObjectsToKObjectList
                    (objectsForPublish.Where(o => DAM.ObjectManager.IsUnpublishedObject(o)));
            addedConcept.AddedProperties
                = await DAM.PropertyManager.GetKPropertiesFromKWProperties
                    (propertiesForPublish.Where(p => DAM.PropertyManager.IsUnpublishedProperty(p)));
            addedConcept.AddedRelationships
                = await DAM.LinkManager.GetRelationshipBaseKlinksFromRelationships
                    (relationshipsForPublish.Where(r => DAM.LinkManager.IsUnpublishedRelationship(r)));
            addedConcept.AddedMedias
                = await DAM.MediaManager.GetKMediasFromKWMedias
                    (mediasForPublish.Where(m => DAM.MediaManager.IsUnpublishedMedia(m)));

            ModifiedConcepts modifiedConcept = new ModifiedConcepts();
            modifiedConcept.ModifiedProperties
                = await DAM.PropertyManager.GetModifiedPropertiesFromKWProperties
                    (propertiesForPublish.Where(p => DAM.PropertyManager.IsModifiedProperty(p)), false);
            modifiedConcept.DeletedMedias
                = await DAM.MediaManager.GetKMediasFromKWMedias
                    (mediasForPublish.Where(m => DAM.MediaManager.IsDeletedMedia(m)), false);

            return await PublishConceptsAndSyncWorkspaceCache(addedConcept, modifiedConcept, dataSourceID);
        }

        private static async Task<PublishResultMetadata> PublishConceptsAndSyncWorkspaceCache(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID)
        {
            WorkspaceServiceClient sc = null;
            Dispatch.Entities.Publish.PublishResult publishStatus = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                publishStatus = await sc.PublishAsync(addedConcept, modifiedConcept, dataSourceID, false);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            if (publishStatus == null)
            {
                throw new InvalidOperationException("Invalid Server Response");
            }
            ApplyPublishResultOnCache(addedConcept, modifiedConcept, dataSourceID);
            return new PublishResultMetadata(publishStatus.HorizonServerSynchronized, publishStatus.SearchServerSynchronized);
        }

        private static void ApplyPublishResultOnCache(AddedConcepts addedConcept, ModifiedConcepts modifiedConcept, long dataSourceID)
        {
            DAM.ObjectManager.CommitUnpublishedChanges
                (addedConcept.AddedObjects.Select(o => o.Id));

            DAM.PropertyManager.CommitUnpublishedChanges
                (addedConcept.AddedProperties.Select(p => p.Id)
                , modifiedConcept.ModifiedProperties.Select(p => p.Id)
                , dataSourceID);

            DAM.MediaManager.CommitUnpublishedChanges
                (addedConcept.AddedMedias.Select(m => m.Id)
                , modifiedConcept.DeletedMedias.Select(dm => dm.Id)
                , dataSourceID);

            DAM.LinkManager.CommitUnpublishedChanges
                (addedConcept.AddedRelationships.Select(r => r.Relationship.Id)
                , dataSourceID);
        }
    }
}