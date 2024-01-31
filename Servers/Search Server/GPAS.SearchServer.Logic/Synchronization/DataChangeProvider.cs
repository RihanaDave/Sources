using GPAS.AccessControl;
using GPAS.Logger;
using GPAS.Ontology;
using GPAS.SearchServer.Access.DataClient;
using GPAS.SearchServer.Access.SearchEngine;
using GPAS.SearchServer.Access.SearchEngine.ApacheSolr;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using GPAS.SearchServer.Entities.Sync;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;

namespace GPAS.SearchServer.Logic.Synchronization
{
    internal class DataChangeProvider
    {
        private readonly Ontology.Ontology ontology = OntologyProvider.GetOntology();
        private static DateTime lastCommitTimeStamp = DateTime.MinValue;
        internal static readonly long SearchableMediaMaxSizeInMB = long.Parse(ConfigurationManager.AppSettings["SearchableMediaMaxSizeInMB"]);

        internal TimeSpan MaxCommitInterval
        {
            get;
            private set;
        }

        internal DataChangeProvider()
        {
            int maxCommitIntervalInSeconds;
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxCommitIntervalInSeconds"], out maxCommitIntervalInSeconds))
            {
                maxCommitIntervalInSeconds = 120;
            }
            MaxCommitInterval = new TimeSpan(0, 0, maxCommitIntervalInSeconds);
        }

        internal void Synchronize(AddedConceptsWithAcl addedConcepts, ModifiedConcepts modifiedConcepts, bool isContinousPublish = false)
        {
            IAccessClient engineAccess = SearchEngineProvider.GetNewDefaultSearchEngineClient();

            PrepairAddedObjects_PropertiesAndRelationships(addedConcepts, engineAccess);           
            PrepairAddedDocuments(addedConcepts.AddedDocuments, engineAccess);
            PrepairAddedMedias(addedConcepts.AddedMedias, engineAccess);
            PrepairModifiedProperties(modifiedConcepts.ModifiedProperties, engineAccess);
            PrepaitDeletedMedias(modifiedConcepts.DeletedMedias, engineAccess);

            ApplyPrepairedChanges(engineAccess, isContinousPublish);
        }
        internal void Synchronize(List<DataSourceInfo> dataSourceInfos)
        {
            IAccessClient engineAccess = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            SearchEngineDocumentConvertor searchEngineDocumentConvertor = new SearchEngineDocumentConvertor();
            foreach (var dataSourceInfo in dataSourceInfos)
            {
                engineAccess.AddDataSourceDocument
                    (searchEngineDocumentConvertor.ConvertDataSourceInfoToDataSource(dataSourceInfo));
            }
            bool isCommitNeeded = DateTime.Now - lastCommitTimeStamp >= MaxCommitInterval;
            engineAccess.ApplyChanges(DataSourceCollection.SolrUrl, isCommitNeeded);
        }

        private void ApplyPrepairedChanges(IAccessClient engineAccess, bool isContinousPublish)
        {
            bool isCommitNeeded = !isContinousPublish || DateTime.Now - lastCommitTimeStamp >= MaxCommitInterval;
            engineAccess.ApplyChanges(ObjectCollection.SolrUrl, isCommitNeeded);
        }

        private void PrepaitDeletedMedias(List<SearchMedia> deletedMedias, IAccessClient engineAccess)
        {
            foreach (var media in deletedMedias)
            {
                engineAccess.RemoveValueFromFileDocumentMultiField
                    (EncodingConverter.GetBase64Encode(media.URI.ToString()), nameof(File.OwnerObjectIds), media.OwnerObjectId.ToString(CultureInfo.InvariantCulture), true);
            }
        }

        private void PrepairModifiedProperties(ModifiedProperty[] modifiedProperties, IAccessClient engineAccess)
        {
            foreach (ModifiedProperty modifiedProp in modifiedProperties)
            {
                BaseDataTypes propBaseType = ontology.GetBaseDataTypeOfProperty(modifiedProp.TypeUri);
                if (modifiedProp.TypeUri == ontology.GetLocationPropertyTypeUri())
                {
                    propBaseType = BaseDataTypes.GeoPoint;
                }
                string valueFieldName = Property.GetPropertyValueFieldNameByBaseType(propBaseType);
                Property property = new Property();
                if (valueFieldName == nameof(property.StringValue))
                {
                    engineAccess.UpdatePropertyDocumentField
                    (modifiedProp.ID.ToString(CultureInfo.InvariantCulture)
                        , modifiedProp.OwnerObjectID.ToString(CultureInfo.InvariantCulture)
                        , nameof(property.KeywordTokenizedStringValue), modifiedProp.newValue);
                }

                engineAccess.UpdatePropertyDocumentField
                   (modifiedProp.ID.ToString(CultureInfo.InvariantCulture)
                   , modifiedProp.OwnerObjectID.ToString(CultureInfo.InvariantCulture)
                   , valueFieldName, modifiedProp.newValue);
            }
        }

        private void PrepairAddedMedias(List<AccessControled<SearchMedia>> addedMedias, IAccessClient engineAccess)
        {
            var docConvertor = new SearchEngineDocumentConvertor();
            if (addedMedias.Any())
            {
                Dictionary<string, File> addedMediasRelatedDocumentsByUri = new Dictionary<string, File>(addedMedias.Count);

                foreach (AccessControled<SearchMedia> addedMedia in addedMedias)
                {
                    if (CanIndexFile(addedMedia.ConceptInstance.URI))
                    {
                        if (engineAccess.IsFileDocumentExistWithID(EncodingConverter.GetBase64Encode(addedMedia.ConceptInstance.URI)))
                        {
                            List<string> multiValuesToAdd = new List<string>();
                            multiValuesToAdd.Add(addedMedia.ConceptInstance.OwnerObjectId.ToString(CultureInfo.InvariantCulture));
                            engineAccess.AddValueToFileDocumentMultiValue
                                (EncodingConverter.GetBase64Encode(addedMedia.ConceptInstance.URI)
                                , nameof(File.OwnerObjectIds)
                                , multiValuesToAdd);
                        }
                        else
                        {
                            if (!addedMediasRelatedDocumentsByUri.ContainsKey(addedMedia.ConceptInstance.URI))
                            {
                                FileRepositoryDataClient fileRepositoryDataClient = new FileRepositoryDataClient();
                                byte[] mediaContent = fileRepositoryDataClient.GetMediaContentFromFileRepository(addedMedia.ConceptInstance.URI);
                                File relatedDoc = docConvertor.GetFileDocument(addedMedia, mediaContent);
                                addedMediasRelatedDocumentsByUri.Add(addedMedia.ConceptInstance.URI, relatedDoc);
                            }
                            else
                            {
                                addedMediasRelatedDocumentsByUri[addedMedia.ConceptInstance.URI].OwnerObjectIds.Add(addedMedia.ConceptInstance.OwnerObjectId.ToString(CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }

                foreach (File fileToAdd in addedMediasRelatedDocumentsByUri.Values)
                {
                    engineAccess.AddFileDocument(fileToAdd);
                }
            }
        }

        private void PrepairAddedDocuments(List<AccessControled<SearchObject>> addedDocuments, IAccessClient engineAccess)
        {
            foreach (AccessControled<SearchObject> addedDoc in addedDocuments)
            {
                // TODO: آیا استخراج محتوا برای تصاویر نیاز نیست؟
                if (ontology.IsDocument(addedDoc.ConceptInstance.TypeUri))
                {
                    var fileRepoDataClient = new FileRepositoryDataClient();
                    var docConvertor = new SearchEngineDocumentConvertor();
                    byte[] documentContent = fileRepoDataClient.GetDocumentContentFromFileRepository(addedDoc.ConceptInstance.Id);
                    File relatedDoc = docConvertor.GetFileDocument(addedDoc, documentContent);
                    engineAccess.AddFileDocument(relatedDoc);

                    if (ontology.IsImageDocument(addedDoc.ConceptInstance.TypeUri)
                        && ImageProcessingProvider.IsVisonServiceInstalled)
                    {
                        ImageProcessingProvider imageProcessingProvider = new ImageProcessingProvider();
                        ImageDocument imageDocument = imageProcessingProvider.GetImageDocument(addedDoc);
                        engineAccess.AddImageDocument(imageDocument);
                    }
                }
            }
        }

        private void PrepairAddedObjects_PropertiesAndRelationships(AddedConceptsWithAcl addedConcepts, IAccessClient engineAccess)
        {
            AdministrativeEventReporter administrativeEventReporter = new AdministrativeEventReporter();
            string notIndexedPropertyIDs = null;
            var docConvertor = new SearchEngineDocumentConvertor();
            var addedObjectsToIndex = new Dictionary<long, ObjectDocument>(addedConcepts.AddedNonDocumentObjects.Count + addedConcepts.AddedDocuments.Count);
            var addedPropertiesWhereNotOwnedByAddedObjectsByOwnerID = new Dictionary<SearchObject, List<Property>>();
            var addedRelationshipsWhereNotOwnedByAddedObjectsByOwnerID = new Dictionary<long, List<Relationship>>();

            // Prepair Added Objects
            List<long> resolvedObjectIds = new List<long>();
            foreach (SearchObject addedObj
                in addedConcepts.AddedNonDocumentObjects
                .Concat(addedConcepts.AddedDocuments.Select(d => d.ConceptInstance)))
            {
                ObjectDocument relatedDoc = docConvertor.GetObjectDocument(addedObj);
                if (!addedObjectsToIndex.ContainsKey(addedObj.Id))
                    addedObjectsToIndex.Add(addedObj.Id, relatedDoc);
            }

            // Prepair Added Properties
            foreach (AccessControled<SearchProperty> addedProp in addedConcepts.AddedProperties)
            {
                if (resolvedObjectIds.Contains(addedProp.ConceptInstance.OwnerObject.Id))
                {
                    continue;
                }
                Property relatedDoc = null;
                bool isParsed = docConvertor.GetPropertyDocument(addedProp, ontology, out relatedDoc);
                if (!isParsed)
                {
                    notIndexedPropertyIDs += " " + addedProp.ConceptInstance.Id.ToString();
                    continue;
                }
                if (addedObjectsToIndex.ContainsKey(addedProp.ConceptInstance.OwnerObject.Id))
                {
                    addedObjectsToIndex[addedProp.ConceptInstance.OwnerObject.Id].Properties.Add(relatedDoc);
                }
                else
                {
                    if (!addedPropertiesWhereNotOwnedByAddedObjectsByOwnerID.ContainsKey(addedProp.ConceptInstance.OwnerObject))
                    {
                        addedPropertiesWhereNotOwnedByAddedObjectsByOwnerID.Add(addedProp.ConceptInstance.OwnerObject, new List<Property>());
                    }
                    addedPropertiesWhereNotOwnedByAddedObjectsByOwnerID[addedProp.ConceptInstance.OwnerObject].Add(relatedDoc);
                }
            }
            if (!string.IsNullOrEmpty(notIndexedPropertyIDs))
            {
                administrativeEventReporter.Report(string.Format
                      ("Propery with id(s): [{0}] was(were) not indexed in search server because an error occurred in validation proccess"
                      , notIndexedPropertyIDs)
                      );
            }
            // Prepair Added Relationships
            foreach (AccessControled<SearchRelationship> addedRel in addedConcepts.AddedRelationships)
            {
                if (resolvedObjectIds.Contains(addedRel.ConceptInstance.SourceObjectId))
                {
                    continue;
                }
                Relationship relatedDoc = docConvertor.GetRelationshipDocument(addedRel);
                if (addedObjectsToIndex.ContainsKey(addedRel.ConceptInstance.SourceObjectId))
                {
                    addedObjectsToIndex[addedRel.ConceptInstance.SourceObjectId].Relationships.Add(relatedDoc);
                }
                else
                {
                    if (!addedRelationshipsWhereNotOwnedByAddedObjectsByOwnerID.ContainsKey(addedRel.ConceptInstance.SourceObjectId))
                    {
                        addedRelationshipsWhereNotOwnedByAddedObjectsByOwnerID.Add(addedRel.ConceptInstance.SourceObjectId, new List<Relationship>());
                    }
                    addedRelationshipsWhereNotOwnedByAddedObjectsByOwnerID[addedRel.ConceptInstance.SourceObjectId].Add(relatedDoc);
                }
            }
            
            // Append Added Objects And Added Properties
            foreach (var item in addedObjectsToIndex)
            {
                engineAccess.AddObjectDocument(item.Value);
            }
            foreach (var item in addedPropertiesWhereNotOwnedByAddedObjectsByOwnerID)
            {
                engineAccess.AddPropertyDocument(item.Key, item.Value);
            }
            foreach (var item in addedRelationshipsWhereNotOwnedByAddedObjectsByOwnerID)
            {
                engineAccess.AddRelationshipDocument(item.Value);
            }
            if (resolvedObjectIds.Count > 0)
            {
                List<string> resolvedObjectIdsString = resolvedObjectIds.Select(o => o.ToString()).ToList();
                engineAccess.DeleteObjectDocument(resolvedObjectIdsString);
            }
        }

        internal void CommitDocumentChanges()
        {
            IAccessClient engineAccess = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            engineAccess.Commit();

            lastCommitTimeStamp = DateTime.Now;
        }

        private bool CanIndexFile(string fileRepositoryUri)
        {
            // File is an known Document
            int extensionStartIndex = fileRepositoryUri.LastIndexOf('.') + 1;
            if (fileRepositoryUri.Length <= extensionStartIndex)
                return false;
            string extension = fileRepositoryUri.Substring(extensionStartIndex).ToUpper();
            if (!ontology.IsDocument(extension))
                return false;
            //Check Size For Inedexing 
            double fileSizeInMb = GetFileSizeInMegabytes(fileRepositoryUri);
            if (fileSizeInMb > SearchableMediaMaxSizeInMB)
                return false;
            return true;
        }

        private double GetFileSizeInMegabytes(string fileRepositoryUri)
        {
            FileRepositoryDataClient fileRepositoryDataClient = new FileRepositoryDataClient();
            double fileSizeInMb = Math.Ceiling(fileRepositoryDataClient.GetFileSizeInBytes(fileRepositoryUri) / 1048576.0);
            return fileSizeInMb;
        }
    }
}