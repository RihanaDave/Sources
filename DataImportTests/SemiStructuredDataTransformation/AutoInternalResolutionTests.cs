using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Transformation;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImportTests.SemiStructuredDataTransformation
{
    [TestClass()]
    public class AutoInternalResolutionTests
    {
        private static void SetNeededAppsettingsFakeAssigns()
        {
            System.Configuration.Fakes.ShimConfigurationManager.AppSettingsGet = () =>
            {
                var nvc = new System.Collections.Specialized.NameValueCollection();
                nvc.Add("ReportFullDetailsInImportLog", "False");
                nvc.Add("MinimumIntervalBetwweenIterrativeLogsReportInSeconds", "30");
                return nvc;
            };
        }

        private Ontology.Ontology fakeOntology = new Ontology.Ontology();

        [TestMethod()]
        public void ObjectMappingAndRealtionMappingWithSimilarPropertyType()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "4" },
                new string[] { "5", "5" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            RelationshipMapping relMap = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(6, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectMappingWithDiffrentObjectTypeAndRealtionMappingWithSimilarPropertyType()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "4" },
                new string[] { "5", "5" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("سازمان"), "سازمان");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            RelationshipMapping relMap = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(9, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectMappingAndRealtionMappingWithDifferentPropertyType()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "4" },
                new string[] { "5", "5" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            RelationshipMapping relMap = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(9, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectMappingAndRealtionMappingWithSimilarPropertyTypeWithFindMatchAndMustMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "4" },
                new string[] { "5", "5" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
            );

            RelationshipMapping relMap = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(11, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectMappingAndMultiRealtionMappingWithSimilarPropertyType()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "4" },
                new string[] { "5", "5" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            RelationshipMapping relMap1 = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            RelationshipMapping relMap2 = new RelationshipMapping(
                objMap2, objMap1,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("friend")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap1);
            testMapping.AddRelationshipMapping(relMap2);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(6, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(14, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void MultiObjectMappingAndMultiRealtionMappingWithSimilarPropertyType()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2", "id_3" },
                new string[] { "1", "5", "2" },
                new string[] { "2", "6", "7" },
                new string[] { "2", "7", "7" },
                new string[] { "4", "6", "4" },
                new string[] { "5", "1", "1" },
                new string[] { "1", "4", "6" },
                new string[] { "2", "2", "3" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap3 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap3.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(2) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            RelationshipMapping relMap1 = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            RelationshipMapping relMap2 = new RelationshipMapping(
                objMap3, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("email")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddObjectMapping(objMap3);
            testMapping.AddRelationshipMapping(relMap1);
            testMapping.AddRelationshipMapping(relMap2);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(7, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(14, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ObjectMappingAndRealtionMappingWithSimilarPropertyTypeAndEmptyData()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"}
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            RelationshipMapping relMap = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            testMapping.InterTypeAutoInternalResolution = true;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(0, trnasformer.GeneratingObjects.Count);
            }
        }
    }
}
