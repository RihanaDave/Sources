using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.FilterSearch;
using GPAS.SearchAround;
using GPAS.SearchAround.DataMapping;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.FeatureTest.SearchAroundTests
{
    [TestClass]
    public class CustomSearchAround
    {
        private bool isInitialized = false;
        private const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff ";

        [TestInitialize]
        public async Task Init()
        {
            if (!isInitialized)
            {
                var authentication = new UserAccountControlProvider();
                bool result = await authentication.AuthenticateAsync("admin", "admin");
                await Workspace.Logic.System.InitializationAsync();
                isInitialized = true;
            }
        }

        // تست‌های جستجوی پیرامونی سفارشی شده‌
        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRepation()
        {
            // Assign
            string relationType = "شبیه";
            string personType = "شخص";
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personType, $"{Guid.NewGuid().ToString()}Person 1");
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(personType, $"{Guid.NewGuid().ToString()}Person 2");

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, relationType, LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());            

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();            
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds(relationType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(personType), "Person_1");
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newRelation.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds("شخص");
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().SearchedObject.ID);
        }        

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRepationWithStringPropertyAndEqualComparator()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType2 = "نام";
            string personPropertyName2 = $"{personPropertyId}name";
            
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);            
            
            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);
            
            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyName2);
            unpublishedProperties.Add(newUnpublishProperty1);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds("شبیه").ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyName2))
            {
                Comparator = new StringPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyName2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newRelation.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRepationWithLongPropertyAndEqualComparator()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyType2 = "سن";
            int personPropertyName2 = 28;

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyName2.ToString());
            unpublishedProperties.Add(newUnpublishProperty1);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds("شبیه").ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyName2.ToString()))
            {
                Comparator = new LongPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyName2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newRelation.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r=>r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);

        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRepationWithFloatPropertyAndEqualComparator()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyType2 = "قد";
            double personPropertyName2 = 12.5;

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyName2.ToString());
            unpublishedProperties.Add(newUnpublishProperty1);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds("شبیه").ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyName2.ToString()))
            {
                Comparator = new FloatPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = (float)personPropertyName2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newRelation.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r=>r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRepationWithMilliSecondDateTimePropertyAndEqualComparator()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyType2 = "تاریخ_ایجاد_کاربر_شبکه_اجتماعی";           
            string dateString = "7/10/1974 7:10:24.987 AM";
            DateTime personPropertyValue2 =
                DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2.ToString(DefaultDateTimeFormat));
            unpublishedProperties.Add(newUnpublishProperty1);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds("شبیه").ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2.ToString(DefaultDateTimeFormat)))
            {
                Comparator = new DateTimePropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newRelation.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r=>r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRepationDateTimePropertyAndEqualComparator()
        {
            // Assign
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyType2 = "تاریخ_ایجاد_کاربر_شبکه_اجتماعی";
            string dateString = "7/10/1974 7:10:24 AM";
            DateTime personPropertyValue2 =
                DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2.ToString(DefaultDateTimeFormat));
            unpublishedProperties.Add(newUnpublishProperty1);

            RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishPerson2, "شبیه", LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelation.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds("شبیه").ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2.ToString(DefaultDateTimeFormat)))
            {
                Comparator = new DateTimePropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newRelation.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Contains(newRelation.Relationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomRelationsForMultipleObjectAndMultipleProperty()
        {
            // Assign
            string relationType = "حضور_در";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()} Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()} Person 2";
            string newOrganizationLabel1 = $"{Guid.NewGuid().ToString()} Organization 1";
            string newOrganizationLabel2 = $"{Guid.NewGuid().ToString()} Organization 2";
            string personType = "شخص";           
            string organizationType = "سازمان";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType = "نام";
            string personPropertyValue = $"{personPropertyId} name"; ;
            string organizationPropertyId = Guid.NewGuid().ToString();
            string organizationPropertyType = "نام";
            string organizationPropertyValue = $"{organizationPropertyId} name";

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personType, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(personType, newPersonLabel2);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(organizationType, newOrganizationLabel1);
            KWObject newUnpublishOrganization2 = await ObjectManager.CreateNewObject(organizationType, newOrganizationLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);
            unpublishedObjects.Add(newUnpublishOrganization1);
            unpublishedObjects.Add(newUnpublishOrganization2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishPersonProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType, personPropertyValue.ToString());
            KWProperty newUnpublishOrganizationProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, organizationPropertyType, organizationPropertyValue.ToString());
            unpublishedProperties.Add(newUnpublishPersonProperty);
            unpublishedProperties.Add(newUnpublishOrganizationProperty);

            RelationshipBasedKWLink newRelationForPerson1 = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson1, newUnpublishOrganization1, relationType, LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelationForPerson1.Relationship);
            RelationshipBasedKWLink newRelationForPerson2 = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson2, newUnpublishOrganization2, relationType, LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newRelationForPerson2.Relationship);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            ObjectMapping sourceObjectMappingByPerson1 = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping sourceObjectMappingByOrganization2 = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishOrganization2.TypeURI), newUnpublishOrganization2.GetObjectLabel());

            SourceSetObjectMapping sourceSetObjectMapping = new SourceSetObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI),
                newUnpublishPerson1.GetObjectLabel(),
                new List<ObjectMapping>() { sourceObjectMappingByPerson1, sourceObjectMappingByOrganization2 });

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            // SearchAroundStep1
            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds(relationType).ToArray();

            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(personType), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType), new ConstValueMappingItem(personPropertyValue.ToString()))
            {
                Comparator = new StringPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue
                }
            });

            //targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("label"), new ConstValueMappingItem(newPersonLabel2.ToString()))
            //{
            //    Comparator = new StringPropertyCriteriaOperatorValuePair()
            //    {
            //        CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
            //        CriteriaValue = newPersonLabel2
            //    }
            //});
            LinkMapping linkMappingForStep1 = new LinkMapping(sourceSetObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(relationType));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(personType);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMappingForStep1.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            // SearchAroundStep2
            SearchAroundStep searchAroundStep2 = new SearchAroundStep();
            searchAroundStep2.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds(relationType).ToArray();

            ObjectMapping targetObjectMappingForOrganization2 = new ObjectMapping(new OntologyTypeMappingItem(organizationType), "Organization_1");
            targetObjectMappingForOrganization2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(organizationPropertyType), new ConstValueMappingItem(organizationPropertyValue.ToString()))
            {
                Comparator = new StringPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = organizationPropertyValue
                }
            });
            LinkMapping linkMappingForStep2 = new LinkMapping(sourceSetObjectMapping, targetObjectMappingForOrganization2, new OntologyTypeMappingItem(relationType));
            
            List<PropertyValueCriteria> criteriasForStep2 = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUriForOrganization = OntologyProvider.GetOntology().GetAllChilds(organizationType);
            searchAroundStep2.TargetObjectTypeUri = targetObjectTypeUriForOrganization.ToArray();
            foreach (var currentTargetProperty in linkMappingForStep2.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep2, criteriasForStep2, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep2);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI, newUnpublishOrganization2.TypeURI},
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1, newUnpublishOrganization2 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(2, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(newRelationForPerson1.Relationship.ID, customSearchAroundResult.RalationshipBasedResult.Where(r=>r.SearchedObject.ID == newUnpublishPerson1.ID).Select(r => r.LoadedResults).FirstOrDefault().FirstOrDefault().RelationshipIDs.FirstOrDefault());
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).Select(r => r.SearchedObject).FirstOrDefault().ID);
            Assert.AreEqual(2, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Where(r=>r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Where(r => r.SearchedObject.ID == newUnpublishOrganization2.ID).FirstOrDefault().LoadedResults.Count);            
            Assert.AreEqual(newUnpublishOrganization1.ID, customSearchAroundResult.RalationshipBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(newUnpublishPerson2.ID, customSearchAroundResult.RalationshipBasedResult.Where(r => r.SearchedObject.ID == newUnpublishOrganization2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task CheckCustomRelationsNumber()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();

            string relationType = "شبیه";
            string newPersonLabel0 = $"{Guid.NewGuid().ToString()}Person 0";
            string objectType = "شخص";

            KWObject newUnpublishPerson0 = await ObjectManager.CreateNewObject(objectType, newPersonLabel0);
            unpublishedObjects.Add(newUnpublishPerson0);

            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            for (int i = 1; i < 101; i++)
            {
                KWObject newUnpublishPerson = await ObjectManager.CreateNewObject(objectType, $"{Guid.NewGuid().ToString()}Person {i}");
                unpublishedObjects.Add(newUnpublishPerson);
            }

            foreach (var currentUnpublishedObject in unpublishedObjects.ToList())
            {
                RelationshipBasedKWLink newRelation = await LinkManager.CreateRelationshipBaseLinkAsync(newUnpublishPerson0, currentUnpublishedObject, relationType, LinkDirection.SourceToTarget, null, null, string.Empty);
                unpublishedRelations.Add(newRelation.Relationship);
            }

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllRelationshipChilds(relationType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson0.TypeURI), newUnpublishPerson0.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType), "Person_1");
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(relationType));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds("شخص");
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties.ToList())
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson0.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson0 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.Count);
            Assert.AreEqual(Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().RelationshipIDs.Count);
            Assert.AreEqual(100 - Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize, customSearchAroundResult.RalationshipBasedResult.FirstOrDefault().NotLoadedResults.Count);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]

        public async Task GetCustomEvent()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string newPersonLabel3 = $"{Guid.NewGuid().ToString()}Person 3";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string objectType3 = "شخص";            
            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();
            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);
            KWObject newUnpublishPerson3 = await ObjectManager.CreateNewObject(objectType3, newPersonLabel3);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);
            unpublishedObjects.Add(newUnpublishPerson3);

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelations.Add(newEvent.FirstRelationship);
            unpublishedRelations.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();            
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newEvent.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir=>ir.FirstRelationshipID).Contains(newEvent.FirstRelationship.ID));
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRelationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(e => e.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomEventWithStringPropertyAndEqualComparator()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType2 = "نام";
            string personPropertyValue2 = $"{personPropertyId}name";

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2);
            unpublishedProperties.Add(newUnpublishProperty1);

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelations.Add(newEvent.FirstRelationship);
            unpublishedRelations.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2))
            {
                Comparator = new StringPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newEvent.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.FirstRelationshipID).Contains(newEvent.FirstRelationship.ID));
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRelationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(e => e.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomEventWithLongPropertyAndEqualComparator()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType2 = "سن";
            long personPropertyValue2 = 28;

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2.ToString());
            unpublishedProperties.Add(newUnpublishProperty1);

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelations.Add(newEvent.FirstRelationship);
            unpublishedRelations.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2.ToString()))
            {
                Comparator = new LongPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newEvent.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.FirstRelationshipID).Contains(newEvent.FirstRelationship.ID));
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRelationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(e => e.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomEventWithFloatPropertyAndEqualComparator()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType2 = "قد";
            double personPropertyValue2 = 12.5;

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2.ToString());
            unpublishedProperties.Add(newUnpublishProperty1);

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelations.Add(newEvent.FirstRelationship);
            unpublishedRelations.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2.ToString()))
            {
                Comparator = new FloatPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = (float)personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newEvent.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.FirstRelationshipID).Contains(newEvent.FirstRelationship.ID));
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRelationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(e => e.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomEventWithMilliSecondDateTimePropertyAndEqualComparator()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType2 = "تاریخ_ایجاد_کاربر_شبکه_اجتماعی";
            string dateString = "7/10/1974 7:10:24.987 AM";
            DateTime personPropertyValue2 =
                DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2.ToString(DefaultDateTimeFormat));
            unpublishedProperties.Add(newUnpublishProperty1);

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelations.Add(newEvent.FirstRelationship);
            unpublishedRelations.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2.ToString(DefaultDateTimeFormat)))
            {
                Comparator = new DateTimePropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newEvent.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.FirstRelationshipID).Contains(newEvent.FirstRelationship.ID));
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRelationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(e => e.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomEventWithDateTimePropertyAndEqualComparator()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string objectType1 = "شخص";
            string objectType2 = "شخص";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType2 = "تاریخ_ایجاد_کاربر_شبکه_اجتماعی";
            string dateString = "7/10/1974 7:10:24 AM";
            DateTime personPropertyValue2 =
                DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(objectType1, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(objectType2, newPersonLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishProperty1 = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType2, personPropertyValue2.ToString(DefaultDateTimeFormat));
            unpublishedProperties.Add(newUnpublishProperty1);

            EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishPerson2, eventType, LinkDirection.Bidirectional, null, null, string.Empty);
            unpublishedRelations.Add(newEvent.FirstRelationship);
            unpublishedRelations.Add(newEvent.SecondRelationship);
            unpublishedObjects.Add(newEvent.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType2), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType2), new ConstValueMappingItem(personPropertyValue2.ToString(DefaultDateTimeFormat)))
            {
                Comparator = new DateTimePropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue2
                }
            });
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(newEvent.TypeURI));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType2);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.FirstRelationshipID).Contains(newEvent.FirstRelationship.ID));
            Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRelationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(e => e.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
        }


        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task GetCustomEventsForMultipleObjectAndMultipleProperty()
        {
            // Assign
            string eventType = "ایمیل";
            string newPersonLabel1 = $"{Guid.NewGuid().ToString()}Person 1";
            string newPersonLabel2 = $"{Guid.NewGuid().ToString()}Person 2";
            string newOrganizationLabel1 = $"{Guid.NewGuid().ToString()}Organization 1";
            string newOrganizationLabel2 = $"{Guid.NewGuid().ToString()}Organization 2";
            string personType = "شخص";
            string organizationType = "سازمان";
            string personPropertyId = Guid.NewGuid().ToString();
            string personPropertyType = "نام";
            string personPropertyValue = $"{personPropertyId} name"; ;
            string organizationPropertyId = Guid.NewGuid().ToString();
            string organizationPropertyType = "نام";
            string organizationPropertyValue = $"{organizationPropertyId} name";

            List<KWObject> unpublishedObjects = new List<KWObject>();
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();

            KWObject newUnpublishPerson1 = await ObjectManager.CreateNewObject(personType, newPersonLabel1);
            KWObject newUnpublishPerson2 = await ObjectManager.CreateNewObject(personType, newPersonLabel2);
            KWObject newUnpublishOrganization1 = await ObjectManager.CreateNewObject(organizationType, newOrganizationLabel1);
            KWObject newUnpublishOrganization2 = await ObjectManager.CreateNewObject(organizationType, newOrganizationLabel2);

            unpublishedObjects.Add(newUnpublishPerson1);
            unpublishedObjects.Add(newUnpublishPerson2);
            unpublishedObjects.Add(newUnpublishOrganization1);
            unpublishedObjects.Add(newUnpublishOrganization2);

            List<KWProperty> unpublishedProperties = new List<KWProperty>();
            KWProperty newUnpublishPersonProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishPerson2, personPropertyType, personPropertyValue.ToString());
            KWProperty newUnpublishOrganizationProperty = PropertyManager.CreateNewPropertyForObject(newUnpublishOrganization1, organizationPropertyType, organizationPropertyValue.ToString());
            unpublishedProperties.Add(newUnpublishPersonProperty);
            unpublishedProperties.Add(newUnpublishOrganizationProperty);

            EventBasedKWLink newEventForPerson1 = LinkManager.CreateEventBaseLink(newUnpublishPerson1, newUnpublishOrganization1, eventType, LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newEventForPerson1.FirstRelationship);
            unpublishedRelations.Add(newEventForPerson1.SecondRelationship);
            unpublishedObjects.Add(newEventForPerson1.IntermediaryEvent);

            EventBasedKWLink newEventForPerson2 = LinkManager.CreateEventBaseLink(newUnpublishPerson2, newUnpublishOrganization2, eventType, LinkDirection.SourceToTarget, null, null, string.Empty);
            unpublishedRelations.Add(newEventForPerson2.FirstRelationship);
            unpublishedRelations.Add(newEventForPerson2.SecondRelationship);
            unpublishedObjects.Add(newEventForPerson2.IntermediaryEvent);

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                unpublishedProperties, new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            ObjectMapping sourceObjectMappingByPerson1 = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI), newUnpublishPerson1.GetObjectLabel());
            ObjectMapping sourceObjectMappingByOrganization2 = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishOrganization2.TypeURI), newUnpublishOrganization2.GetObjectLabel());

            SourceSetObjectMapping sourceSetObjectMapping = new SourceSetObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson1.TypeURI),
                newUnpublishPerson1.GetObjectLabel(),
                new List<ObjectMapping>() { sourceObjectMappingByPerson1, sourceObjectMappingByOrganization2 });

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            // SearchAroundStep1
            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();

            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(personType), "Person_1");
            targetObjectMapping.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(personPropertyType), new ConstValueMappingItem(personPropertyValue.ToString()))
            {
                Comparator = new StringPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = personPropertyValue
                }
            });
            LinkMapping linkMappingForStep1 = new LinkMapping(sourceSetObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(eventType));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(personType);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMappingForStep1.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            // SearchAroundStep2
            SearchAroundStep searchAroundStep2 = new SearchAroundStep();
            searchAroundStep2.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();

            ObjectMapping targetObjectMappingForOrganization2 = new ObjectMapping(new OntologyTypeMappingItem(organizationType), "Organization_1");
            targetObjectMappingForOrganization2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(organizationPropertyType), new ConstValueMappingItem(organizationPropertyValue.ToString()))
            {
                Comparator = new StringPropertyCriteriaOperatorValuePair()
                {
                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                    CriteriaValue = organizationPropertyValue
                }
            });
            LinkMapping linkMappingForStep2 = new LinkMapping(sourceSetObjectMapping, targetObjectMappingForOrganization2, new OntologyTypeMappingItem(eventType));

            List<PropertyValueCriteria> criteriasForStep2 = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUriForOrganization = OntologyProvider.GetOntology().GetAllChilds(organizationType);
            searchAroundStep2.TargetObjectTypeUri = targetObjectTypeUriForOrganization.ToArray();
            foreach (var currentTargetProperty in linkMappingForStep2.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep2, criteriasForStep2, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep2);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson1.TypeURI, newUnpublishOrganization2.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson1, newUnpublishOrganization2 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(2, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(newUnpublishPerson1.ID, customSearchAroundResult.EventBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().SearchedObject.ID);
            Assert.AreEqual(newUnpublishOrganization2.ID, customSearchAroundResult.EventBasedResult.Where(r => r.SearchedObject.ID == newUnpublishOrganization2.ID).FirstOrDefault().SearchedObject.ID);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Where(r => r.SearchedObject.ID == newUnpublishOrganization2.ID).FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(newUnpublishOrganization1.ID, customSearchAroundResult.EventBasedResult.Where(r => r.SearchedObject.ID == newUnpublishPerson1.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
            Assert.AreEqual(newUnpublishPerson2.ID, customSearchAroundResult.EventBasedResult.Where(r => r.SearchedObject.ID == newUnpublishOrganization2.ID).FirstOrDefault().LoadedResults.FirstOrDefault().TargetObject.ID);
        }

        [TestCategory("جستجوی پیرامونی سفارشی شده")]
        [TestMethod]
        public async Task CheckCustomEventsNumber()
        {
            // Assign
            List<KWObject> unpublishedObjects = new List<KWObject>();

            string eventType = "ایمیل";

            string newPersonLabel0 = $"{Guid.NewGuid().ToString()}Person 0";
            string objectType0 = "شخص";

            KWObject newUnpublishPerson0 = await ObjectManager.CreateNewObject(objectType0, newPersonLabel0);
            unpublishedObjects.Add(newUnpublishPerson0);
            string objectType = "شخص";
            
            List<KWRelationship> unpublishedRelations = new List<KWRelationship>();
            List<KWObject> newUnpublishPersons = new List<KWObject>();

            for (int i = 1; i < 101; i++)
            {
                KWObject newUnpublishPerson = await ObjectManager.CreateNewObject(objectType, $"{Guid.NewGuid().ToString()}Person {i}");
                newUnpublishPersons.Add(newUnpublishPerson);
                unpublishedObjects.Add(newUnpublishPerson);
            }

            for (int i = 0; i < newUnpublishPersons.Count; i++)
            {
                EventBasedKWLink newEvent = LinkManager.CreateEventBaseLink(newUnpublishPerson0, newUnpublishPersons.ElementAt(i), eventType, LinkDirection.Bidirectional, null, null, string.Empty);
                unpublishedRelations.Add(newEvent.FirstRelationship);
                unpublishedRelations.Add(newEvent.SecondRelationship);
                unpublishedObjects.Add(newEvent.IntermediaryEvent);
            }            

            PublishResultMetadata publishResultMetadata = await Workspace.Logic.Publish.PublishManager.PublishSpecifiedManuallyEnteredConcepts(unpublishedObjects,
                new List<KWProperty>(), new List<KWMedia>(), unpublishedRelations, new List<Workspace.Entities.GraphResolution.ObjectResolutionMap>());

            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            SearchAroundStep searchAroundStep = new SearchAroundStep();
            searchAroundStep.LinkTypeUri = OntologyProvider.GetOntology().GetAllChilds(eventType).ToArray();
            ObjectMapping sourceObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(newUnpublishPerson0.TypeURI), newUnpublishPerson0.GetObjectLabel());
            ObjectMapping targetObjectMapping = new ObjectMapping(new OntologyTypeMappingItem(objectType), "Person_1");
            LinkMapping linkMapping = new LinkMapping(sourceObjectMapping, targetObjectMapping, new OntologyTypeMappingItem(eventType));

            List<PropertyValueCriteria> criterias = new List<PropertyValueCriteria>();
            List<string> targetObjectTypeUri = OntologyProvider.GetOntology().GetAllChilds(objectType);
            searchAroundStep.TargetObjectTypeUri = targetObjectTypeUri.ToArray();
            foreach (var currentTargetProperty in linkMapping.Target.Properties)
            {
                GenerateSearchAroundStep(searchAroundStep, criterias, currentTargetProperty);
            }

            searchAroundSteps.Add(searchAroundStep);

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { newUnpublishPerson0.TypeURI },
                LinksFromSearchSet = searchAroundSteps.ToArray()
            };

            //Act
            KWCustomSearchAroundResult customSearchAroundResult = await Workspace.Logic.Search.SearchAround.PerformCustomSearchAround(new KWObject[] { newUnpublishPerson0 }, customSearchAroundCriteria);

            //Assert
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.Count);
            Assert.AreEqual(Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.Count);
            Assert.AreEqual(1, customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Count);
            //Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.FirstRealationshipID).Contains(newEvent.FirstRelationship.ID));
            //Assert.IsTrue(customSearchAroundResult.EventBasedResult.FirstOrDefault().LoadedResults.FirstOrDefault().InnerRelationshipIDs.Select(ir => ir.SecondRealationshipID).Contains(newEvent.SecondRelationship.ID));
            Assert.AreEqual(100 - Workspace.Logic.Search.SearchAround.LoadingDefaultBatchSize, customSearchAroundResult.EventBasedResult.FirstOrDefault().NotLoadedResults.Count);
        }

        private static void GenerateSearchAroundStep(SearchAroundStep searchAroundStep, List<PropertyValueCriteria> criterias, PropertyMapping currentSourceProperty)
        {
            PropertyValueCriteria propertyValueCriteria = new PropertyValueCriteria()
            {
                PropertyTypeUri = currentSourceProperty.PropertyType.TypeUri
            };
            switch (OntologyProvider.GetBaseDataTypeOfProperty(currentSourceProperty.PropertyType.TypeUri))
            {
                case Ontology.BaseDataTypes.String:
                case Ontology.BaseDataTypes.HdfsURI:
                    propertyValueCriteria.OperatorValuePair = new StringPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = (currentSourceProperty.Value as ConstValueMappingItem).ConstValue,
                        CriteriaOperator = (currentSourceProperty.Comparator as StringPropertyCriteriaOperatorValuePair).CriteriaOperator
                    };
                    break;
                case Ontology.BaseDataTypes.Double:
                    propertyValueCriteria.OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = float.Parse((currentSourceProperty.Value as ConstValueMappingItem).ConstValue),
                        CriteriaOperator = (currentSourceProperty.Comparator as FloatPropertyCriteriaOperatorValuePair).CriteriaOperator
                    };
                    break;
                case Ontology.BaseDataTypes.Long:
                case Ontology.BaseDataTypes.Int:
                    propertyValueCriteria.OperatorValuePair = new LongPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = long.Parse((currentSourceProperty.Value as ConstValueMappingItem).ConstValue),
                        CriteriaOperator = (currentSourceProperty.Comparator as LongPropertyCriteriaOperatorValuePair).CriteriaOperator
                    };
                    break;
                case Ontology.BaseDataTypes.DateTime:
                    propertyValueCriteria.OperatorValuePair = new DateTimePropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = DateTime.Parse((currentSourceProperty.Value as ConstValueMappingItem).ConstValue),
                        CriteriaOperator = (currentSourceProperty.Comparator as DateTimePropertyCriteriaOperatorValuePair).CriteriaOperator
                    };
                    break;
                case Ontology.BaseDataTypes.Boolean:
                    propertyValueCriteria.OperatorValuePair = new BooleanPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = bool.Parse((currentSourceProperty.Value as ConstValueMappingItem).ConstValue),
                        CriteriaOperator = (currentSourceProperty.Comparator as BooleanPropertyCriteriaOperatorValuePair).CriteriaOperator
                    };
                    break;
                default:
                    break;
            }
            criterias.Add(propertyValueCriteria);

            searchAroundStep.TargetObjectPropertyCriterias = criterias.ToArray();
        }

    }
}
