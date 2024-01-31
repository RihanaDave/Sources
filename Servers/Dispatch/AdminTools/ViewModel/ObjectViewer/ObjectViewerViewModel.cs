using System;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Dispatch.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Dispatch.Entities.IndexChecking;

namespace GPAS.Dispatch.AdminTools.ViewModel.ObjectViewer
{
    public class ObjectViewerViewModel : BaseViewModel
    {
        public ObjectViewerViewModel()
        {
            ObjectToShow = new ObjectModel
            {
                Properties = new ObservableCollection<PropertiesModel>(),
                RelatedEntities = new ObservableCollection<RelationModel>(),
                RelatedEvents = new ObservableCollection<RelationModel>(),
                RelatedDocuments = new ObservableCollection<RelationModel>()
            };

            ListOfSearchedObjects = new ObservableCollection<long>();
        }

        public ObjectModel ObjectToShow { get; set; }
        public ObservableCollection<long> ListOfSearchedObjects { get; set; }

        public async Task SearchById(long id, string typeUri)
        {
            ////ObjectToShow.Reset();

            // TODO |Search Around
            var searchedObject = await GetObjectById(id);

            if (searchedObject == null)
                return;

            var propertiesList = await GetPropertyForObject(searchedObject);
            var arg = new Dictionary<string, long[]>
            {
                { typeUri, new[] { id } }
            };
            var relationsDetailsList = await GetRelationsDetails(await FindRelatedItems(arg));
            DataSourceACLs = await GetDataSourceAcl(propertiesList, relationsDetailsList);
            var searchIndexResult = await SearchIndexChecking(searchedObject, propertiesList, relationsDetailsList);
            var horizonIndexResult = await HorizonIndexChecking(searchedObject, propertiesList, relationsDetailsList);

            PrepareObjectToShow(searchedObject, propertiesList, relationsDetailsList);
            PrepareIndexStatus(searchIndexResult, horizonIndexResult);
        }

        public async Task<KObject> GetObjectById(long objectId)
        {
            KObject searchedObject = new KObject();

            await Task.Run(() =>
            {
                try
                {
                    RepositoryProvider repositoryProvider = new RepositoryProvider(AccessControl.Users.NativeUser.Admin.ToString());
                    searchedObject = repositoryProvider.GetObjects(new[] { objectId }).First();
                }
                catch
                {
                    searchedObject = null;
                }
            });

            return searchedObject;
        }

        public async Task<List<KProperty>> GetPropertyForObject(KObject kObject)
        {
            List<KProperty> propertiesList = new List<KProperty>();

            await Task.Run(() =>
            {
                try
                {
                    RepositoryProvider repositoryProvider = new RepositoryProvider(AccessControl.Users.NativeUser.Admin.ToString());
                    propertiesList = repositoryProvider.GetPtoperty(kObject);
                }
                catch
                {
                    //Do nothing
                }
            });

            return propertiesList;
        }

        public async Task<List<DataSourceACL>> GetDataSourceAcl(List<KProperty> propertiesList,
                                                                List<List<RelationshipBaseKlink>> relationsDetailsList)
        {
            List<DataSourceACL> dataSourceAclList = new List<DataSourceACL>();

            List<long> dataSourceIds = new List<long>();

            foreach (var property in propertiesList)
            {
                dataSourceIds.Add(property.DataSourceID);
            }

            foreach (var relationDetails in relationsDetailsList)
            {
                foreach (var relation in relationDetails)
                {
                    dataSourceIds.Add(relation.Relationship.DataSourceID);
                }
            }

            await Task.Run(() =>
            {
                UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
                dataSourceAclList = userAccountControlProvider.RetriveDataSourceACLs(dataSourceIds.ToArray());
            });

            return dataSourceAclList;
        }

        public async Task<List<RelationshipBasedResult>> FindRelatedItems(Dictionary<string, long[]> searchedObjects)
        {
            List<RelationshipBasedResult> relationBasedResults = new List<RelationshipBasedResult>();

            await Task.Run(() =>
            {
                SearchAroundProvider searchAround = new SearchAroundProvider(AccessControl.Users.NativeUser.Admin.ToString());
                relationBasedResults.Add(searchAround.FindRelatedEntities(searchedObjects, 2000));
                relationBasedResults.Add(searchAround.FindRelatedEvents(searchedObjects, 2000));
                relationBasedResults.Add(searchAround.FindRelatedDocuments(searchedObjects, 2000));
            });

            return relationBasedResults;
        }

        public async Task<List<List<RelationshipBaseKlink>>> GetRelationsDetails(List<RelationshipBasedResult> relationshipBasedResults)
        {
            List<List<RelationshipBaseKlink>> relationshipsList = new List<List<RelationshipBaseKlink>>();

            await Task.Run(() =>
            {
                foreach (var relationshipBasedResult in relationshipBasedResults)
                {
                    List<long> relationsId = new List<long>();

                    foreach (var result in relationshipBasedResult.Results)
                    {
                        foreach (var notLoadedResult in result.NotLoadedResults)
                        {
                            relationsId.Add(notLoadedResult.RelationshipID);
                        }
                    }

                    RepositoryProvider repositoryProvider = new RepositoryProvider(AccessControl.Users.NativeUser.Admin.ToString());
                    relationshipsList.Add(repositoryProvider.GetRelationships(relationsId));
                }
            });

            return relationshipsList;
        }

        private async Task<SearchIndexCheckingResult> SearchIndexChecking(KObject searchedObject, List<KProperty> properties,
            List<List<RelationshipBaseKlink>> relationsDetailsList)
        {
            var result = new SearchIndexCheckingResult();

            SearchIndexCheckingInput searchInput = new SearchIndexCheckingInput
            {
                ObjectId = searchedObject.Id,
                ObjectType = searchedObject.TypeUri
            };

            foreach (var property in properties)
            {
                searchInput.Properties.Add(property);
            }

            foreach (var relationDetail in relationsDetailsList.SelectMany(relationDetails => relationDetails))
            {
                searchInput.RelationsIds.Add(relationDetail.Relationship.Id);
            }

            await Task.Run(() =>
            {
                IndexCheckingProvider indexCheckingProvider = new IndexCheckingProvider(AccessControl.Users.NativeUser.Admin.ToString());
                result = indexCheckingProvider.SearchIndexChecking(searchInput);
            });

            return result;
        }

        private async Task<HorizonIndexCheckingResult> HorizonIndexChecking(KObject searchedObject, List<KProperty> properties,
            List<List<RelationshipBaseKlink>> relationsDetailsList)
        {
            var result = new HorizonIndexCheckingResult();

            HorizonIndexCheckingInput horizonInput = new HorizonIndexCheckingInput
            {
                ObjectId = searchedObject.Id,
                ObjectTypeUri = searchedObject.TypeUri,
                ResultLimit = 2000
            };

            foreach (var property in properties)
            {
                horizonInput.Properties.Add(property);
            }

            foreach (var relationDetail in relationsDetailsList.SelectMany(relationDetails => relationDetails))
            {
                horizonInput.RelationsIds.Add(relationDetail.Relationship.Id);
            }

            await Task.Run(() =>
            {
                IndexCheckingProvider indexCheckingProvider = new IndexCheckingProvider(AccessControl.Users.NativeUser.Admin.ToString());
                result = indexCheckingProvider.HorizonIndexChecking(horizonInput);
            });

            return result;
        }

        private void PrepareIndexStatus(SearchIndexCheckingResult searchResult, HorizonIndexCheckingResult horizonResult)
        {
            ObjectToShow.ObjectIndex = searchResult.ObjectIndexStatus;
            ObjectToShow.DocumentIndex = searchResult.DocumentIndexStatus;
            ObjectToShow.ImageIndex = searchResult.ImageIndexStatus;
            ObjectToShow.ImageIndexCount = searchResult.ImageIndexCount;
            ObjectToShow.HorizonIndexed = horizonResult.ObjectIndexStatus;

            foreach (var property in ObjectToShow.Properties)
            {
                foreach (var propertyIndexStatus in searchResult.PropertiesIndexStatus.Where(propertyIndexStatus =>
                    property.Id == propertyIndexStatus.Id))
                {
                    property.SearchIndexed = propertyIndexStatus.IndexStatus;
                    break;
                }

                foreach (var propertyIndexStatus in horizonResult.PropertiesIndexStatus.Where(propertyIndexStatus =>
                    property.Id == propertyIndexStatus.Id))
                {
                    property.HorizonIndexed = propertyIndexStatus.IndexStatus;
                    break;
                }
            }

            foreach (var relation in ObjectToShow.RelatedEntities)
            {
                foreach (var relationIndexStatus in searchResult.RelationsIndexStatus.Where(relationIndexStatus =>
                    relation.Id == relationIndexStatus.Id))
                {
                    relation.SearchIndexed = relationIndexStatus.IndexStatus;
                    break;
                }

                foreach (var relationIndexStatus in horizonResult.RelationsIndexStatus.Where(relationIndexStatus =>
                    relation.Id == relationIndexStatus.Id))
                {
                    relation.HorizonIndexed = relationIndexStatus.IndexStatus;
                    break;
                }
            }

            foreach (var relation in ObjectToShow.RelatedDocuments)
            {
                foreach (var relationIndexStatus in searchResult.RelationsIndexStatus.Where(relationIndexStatus =>
                    relation.Id == relationIndexStatus.Id))
                {
                    relation.SearchIndexed = relationIndexStatus.IndexStatus;
                    break;
                }

                foreach (var relationIndexStatus in horizonResult.RelationsIndexStatus.Where(relationIndexStatus =>
                    relation.Id == relationIndexStatus.Id))
                {
                    relation.HorizonIndexed = relationIndexStatus.IndexStatus;
                    break;
                }
            }

            foreach (var relation in ObjectToShow.RelatedEvents)
            {
                foreach (var relationIndexStatus in searchResult.RelationsIndexStatus.Where(relationIndexStatus =>
                    relation.Id == relationIndexStatus.Id))
                {
                    relation.SearchIndexed = relationIndexStatus.IndexStatus;
                    break;
                }

                foreach (var relationIndexStatus in horizonResult.RelationsIndexStatus.Where(relationIndexStatus =>
                    relation.Id == relationIndexStatus.Id))
                {
                    relation.HorizonIndexed = relationIndexStatus.IndexStatus;
                    break;
                }
            }

            if (ListOfSearchedObjects.Count == 0)
            {
                AddObjectToObjectModelList(ObjectToShow);
                return;
            }

            bool existId = ListOfSearchedObjects.Any(x => x == ObjectToShow.Id);

            if (!existId)
            {
                AddObjectToObjectModelList(ObjectToShow);
            }
        }

        private void PrepareObjectToShow(KObject searchedObject, List<KProperty> propertiesList,
                                         List<List<RelationshipBaseKlink>> relationsDetailsList)
        {
            ObjectToShow.Id = searchedObject.Id;
            ObjectToShow.Type = searchedObject.TypeUri;

            if (ObjectToShow.Properties.Count != 0) ObjectToShow.Properties.Clear();

            foreach (var property in propertiesList)
            {
                if (property.Id == searchedObject.LabelPropertyID)
                {
                    ObjectToShow.Name = property.Value;
                    continue;
                }

                ObjectToShow.Properties.Add(new PropertiesModel
                {
                    Id = property.Id,
                    Type = property.TypeUri,
                    BaseType = OntologyLoader.OntologyLoader.GetOntology().GetBaseDataTypeOfProperty(property.TypeUri).ToString(),
                    Value = property.Value,
                    DataSourceId = property.DataSourceID
                });
            }

            int counter = 0;

            try
            {
                if (ObjectToShow.RelatedEntities.Count != 0)
                {
                    ObjectToShow.RelatedEntities.Clear();
                }

                if (ObjectToShow.RelatedEvents.Count != 0)
                {
                    ObjectToShow.RelatedEvents.Clear();
                }

                if (ObjectToShow.RelatedDocuments.Count != 0)
                {
                    ObjectToShow.RelatedDocuments.Clear();
                }
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (var relationDetails in relationsDetailsList)
            {
                foreach (var relationDetail in relationDetails)
                {
                    switch (counter)
                    {
                        case 0:
                            ObjectToShow.RelatedEntities.Add(new RelationModel
                            {
                                Id = relationDetail.Relationship.Id,
                                DataSourceId = relationDetail.Relationship.DataSourceID,
                                Label = relationDetail.Relationship.Description,
                                Type = relationDetail.TypeURI,
                                SourceId = relationDetail.Source.Id,
                                TargetId = relationDetail.Target.Id
                            });
                            continue;
                        case 1:
                            ObjectToShow.RelatedEvents.Add(new RelationModel
                            {
                                Id = relationDetail.Relationship.Id,
                                DataSourceId = relationDetail.Relationship.DataSourceID,
                                Label = relationDetail.Relationship.Description,
                                Type = relationDetail.TypeURI,
                                SourceId = relationDetail.Source.Id,
                                TargetId = relationDetail.Target.Id
                            });
                            continue;
                        case 2:
                            ObjectToShow.RelatedDocuments.Add(new RelationModel
                            {
                                Id = relationDetail.Relationship.Id,
                                DataSourceId = relationDetail.Relationship.DataSourceID,
                                Label = relationDetail.Relationship.Description,
                                Type = relationDetail.TypeURI,
                                SourceId = relationDetail.Source.Id,
                                TargetId = relationDetail.Target.Id
                            });
                            continue;
                    }
                }

                counter++;
            }
        }

        private void AddObjectToObjectModelList(ObjectModel objectModel)
        {
            var newObject = new ObjectModel
            {
                Id = objectModel.Id,
                Name = objectModel.Name,
                Type = objectModel.Type,
                ResolveTo = objectModel.ResolveTo,
                Properties = new ObservableCollection<PropertiesModel>(),
                RelatedEvents = new ObservableCollection<RelationModel>(),
                RelatedEntities = new ObservableCollection<RelationModel>(),
                RelatedDocuments = new ObservableCollection<RelationModel>()
            };

            foreach (var property in objectModel.Properties)
            {
                newObject.Properties.Add(new PropertiesModel
                {
                    Id = property.Id,
                    Type = property.Type,
                    Value = property.Value,
                    DataSourceId = property.DataSourceId
                });
            }

            foreach (var relatedEntity in objectModel.RelatedEntities)
            {
                newObject.RelatedEntities.Add(new RelationModel
                {
                    Id = relatedEntity.Id,
                    DataSourceId = relatedEntity.DataSourceId,
                    Label = relatedEntity.Label,
                    Type = relatedEntity.Type,
                    SourceId = relatedEntity.SourceId,
                    TargetId = relatedEntity.TargetId
                });
            }

            foreach (var relatedEntity in objectModel.RelatedEvents)
            {
                newObject.RelatedEvents.Add(new RelationModel
                {
                    Id = relatedEntity.Id,
                    DataSourceId = relatedEntity.DataSourceId,
                    Label = relatedEntity.Label,
                    Type = relatedEntity.Type,
                    SourceId = relatedEntity.SourceId,
                    TargetId = relatedEntity.TargetId
                });
            }

            foreach (var relatedEntity in objectModel.RelatedDocuments)
            {
                newObject.RelatedDocuments.Add(new RelationModel
                {
                    Id = relatedEntity.Id,
                    DataSourceId = relatedEntity.DataSourceId,
                    Label = relatedEntity.Label,
                    Type = relatedEntity.Type,
                    SourceId = relatedEntity.SourceId,
                    TargetId = relatedEntity.TargetId
                });
            }

            ListOfSearchedObjects.Add(newObject.Id);
        }
    }
}
