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
    public class InternalResolutionTests
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
        public void SingleObjectMappingWithSingleFindMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id" },
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "7" },
                new string[] { "4", "6" }
            };
            const string Cell_phnoe_id_TypeUri = "Cell_phone_id";

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(3, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        public void SingleObjectMappingWithMultiFindMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"},
                new string[] { "1", "5", "1" },
                new string[] { "2", "6", "2" },
                new string[] { "2", "7", "3" },
                new string[] { "1", "5", "7" },
                new string[] { "4", "6", "4" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
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
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        public void SingleObjectMappingWithFindMatchAndMustMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"},
                new string[] { "1", "5", "1" },
                new string[] { "2", "6", "2" },
                new string[] { "2", "7", "3" },
                new string[] { "1", "5", "7" },
                new string[] { "4", "6", "4" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
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
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ObjectMappingWithMustMatchWithoutFindMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"},
                new string[] { "1", "5", "1" },
                new string[] { "2", "6", "2" },
                new string[] { "2", "7", "3" },
                new string[] { "1", "5", "7" },
                new string[] { "4", "6", "4" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
        }

        [TestMethod()]
        public void SingleObjectMappingWithFindMatchPropertyOptionWithSimilarProperties()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"},
                new string[] { "1", "5", "1" },
                new string[] { "2", "6", "2" },
                new string[] { "2", "7", "3" },
                new string[] { "1", "5", "1" },
                new string[] { "2", "6", "4" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
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
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(3, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ObjectMappingWithEmptyData()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"}
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

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

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ObjectMappingWithNullData()
        {
            // Assign
            string[][] rowData = null;

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

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

        [TestMethod()]
        public void ObjectMappingAndRealtionMappingWithAllTypePropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "person_id", "organization_id", "price" },
                new string[] { "1", "5", "1000" },
                new string[] { "2", "6", "2000" },
                new string[] { "2", "7", "3000" },
                new string[] { "4", "6", "1000" }
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
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );
            RelationshipMapping relMap = new RelationshipMapping(
                objMap1, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("friend")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(7, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectMappingAndEventRealtionMappingWithAllTypePropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "person_id", "organization_id", "price" },
                new string[] { "1", "5", "1000" },
                new string[] { "2", "6", "2000" },
                new string[] { "2", "7", "3000" },
                new string[] { "1", "5", "7000" },
                new string[] { "4", "6", "1000" }
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
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            ObjectMapping objMap3 = new ObjectMapping(new OntologyTypeMappingItem("تراکنش_بانکی"), "تراکنش بانکی");
            objMap3.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );
            RelationshipMapping relMap1 = new RelationshipMapping(
                objMap1, objMap3,
                RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("Banking_transaction")
            );
            RelationshipMapping relMap2 = new RelationshipMapping(
                objMap3, objMap2,
                RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("Banking_transaction")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddObjectMapping(objMap3);
            testMapping.AddRelationshipMapping(relMap1);
            testMapping.AddRelationshipMapping(relMap2);
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(11, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(10, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void SingleObjectMappingWithEmptyFieldFindMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"},
                new string[] { "1", "5", "1" },
                new string[] { "", "6", "2" },
                new string[] { "", "7", "3" },
                new string[] { "1", "5", "7" },
                new string[] { "4", "6", "4" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        public void SingleObjectMappingNonCaseSensitiveFieldFindMatchPropertyOption()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id", "field_3"},
                new string[] { "logan", "5", "1" },
                new string[] { "Jack", "6", "2" },
                new string[] { "Logan", "7", "3" },
                new string[] { "Jack", "5", "7" },
                new string[] { "LogaN", "6", "4" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarPropertyTypeAndFFAutoEmptyField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "" },
                new string[] { "", "" }
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
                Assert.AreEqual(10, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarPropertyTypeAndFFAutoNullField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { null, "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", null },
                new string[] { null, null }
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
                Assert.AreEqual(10, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarPropertyTypeAndFFWithoutAutoEmptyField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "" },
                new string[] { "", "" }
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
            testMapping.InterTypeAutoInternalResolution = false;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(12, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarPropertyTypeAndIIWithoutAutoEmptyField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id"},
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "", "7" },
                new string[] { "4", "6" },
                new string[] { "5", "1" },
                new string[] { "5", "" },
                new string[] { "", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );

            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
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
            testMapping.InterTypeAutoInternalResolution = false;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(14, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(7, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarProperiesAndFMMAutoEmptyField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "", "1", "1", "7", "", "" },
                new string[] { "2", "", "", "5", "6", "2" },
                new string[] { "", "1", "1", "4", "", "" },
                new string[] { "4", "", "", "2", "", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
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
                Assert.AreEqual(8, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarProperiesAndFMIAutoEmptyField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "1", "1", "", "7", "6", "" },
                new string[] { "2", "3", "", "5", "6", "2" },
                new string[] { "1", "1", "", "4", "3", "" },
                new string[] { "2", "5", "", "2", "5", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
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
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarProperiesAndFMMAutoNullAndEmptyField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "1", "", "1", "7", "", "" },
                new string[] { "2", null, "", "5", "6", "2" },
                new string[] { "1", "", "1", "4", null, "" },
                new string[] { "4", null, "", "2", "", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
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
                Assert.AreEqual(8, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarProperiesAndFMMAutoEmptyFieldAndRepeatedRows()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "1", "", "1", "7", "", "" },
                new string[] { "2", "", "", "5", "6", "2" },
                new string[] { "5", "6", "2", "2", "", "" },
                new string[] { "1", "", "1", "4", "", "" },
                new string[] { "4", "", "", "2", "", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
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
                Assert.AreEqual(5, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRelationMappingWithSimilarProperiesAndFMMWithoutAutoEmptyFieldAndRepeatedRows()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "1", "", "1", "7", "", "" },
                new string[] { "2", "", "", "5", "6", "2" },
                new string[] { "5", "6", "2", "2", "", "" },
                new string[] { "1", "", "1", "4", "", "" },
                new string[] { "4", "", "", "2", "", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
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
            testMapping.InterTypeAutoInternalResolution = false;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(10, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(5, trnasformer.GeneratingRelationships.Count);
            }

        }


        [TestMethod()]
        public void ObjectAndRealtionMappingWithSimilarIIIWithoutAutoProperiesAndEmptyAndRepeatedField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "1", "", "1", "7", "", "" },
                new string[] { "2", "", "", "5", "6", "2" },
                new string[] { "2", "", "", "5", "6", "2" },
                new string[] { "5", "6", "2", "2", "", "" },
                new string[] { "1", "", "1", "4", "", "" },
                new string[] { "4", "", "", "2", "", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
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
            testMapping.InterTypeAutoInternalResolution = false;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(12, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(6, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRealtionMappingWithSimilarFMIAutoProperiesAndEmptyAndRepeatedField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6",},
                new string[] { "1", "1", "", "7", "5", "6" },
                new string[] { "2", "3", "", "5", "6", "2" },
                new string[] { "2", "3", "", "5", "6", "3" },
                new string[] { "5", "6", "2", "2", "3", "9" },
                new string[] { "1", "1", "", "4", "9", "2" },
                new string[] { "4", "9", "2", "2", "11", "" }
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
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
                Assert.AreEqual(5, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void ObjectAndRealtionMappingWithSimilarFMMIIAutoProperiesAndEmptyAndNullField()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id_1", "id_2","id_3","id_4","id_5","id_6","id_7","id_8"},
                new string[] { "1", "", "1", "7", "", "", "", ""},
                new string[] { "2", "", "", "5", "6", "2", "1", ""},
                new string[] { "2", "", "", "5", "6", "2", null, "3"},
                new string[] { "5", "6", "2", "2", "", "", "", null},
                new string[] { "1", "", "1", "4", "", "", "5", ""},
                new string[] { "4", "", "", "2", "", "", null, ""}
            };

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(0, rowData[0][0]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(1, rowData[0][1]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap1.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(2, rowData[0][2]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );


            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("شخص"), "شخص");
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][0]),
                    new TableColumnMappingItem(3, rowData[0][3]) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                )
                { IsSetAsDisplayName = true }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][1]),
                    new TableColumnMappingItem(4, rowData[0][4]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );
            objMap2.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][2]),
                    new TableColumnMappingItem(5, rowData[0][5]) { ResolutionOption = PropertyInternalResolutionOption.MustMatch }
                )
                { IsSetAsDisplayName = false }
            );

            ObjectMapping objMap3 = new ObjectMapping(new OntologyTypeMappingItem("تماس_تلفنی"), "تماس_تلفنی");
            objMap3.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][6]),
                    new TableColumnMappingItem(3, rowData[0][6]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = true }
            );
            objMap3.AddProperty(
                new PropertyMapping(
                    new OntologyTypeMappingItem(rowData[0][7]),
                    new TableColumnMappingItem(4, rowData[0][7]) { ResolutionOption = PropertyInternalResolutionOption.Ignorable }
                )
                { IsSetAsDisplayName = false }
            );

            RelationshipMapping relMap1 = new RelationshipMapping(
                objMap1, objMap3,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            RelationshipMapping relMap2 = new RelationshipMapping(
                objMap3, objMap2,
                RelationshipBaseLinkMappingRelationDirection.Bidirectional,
                new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("call")
            );

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddObjectMapping(objMap3);
            testMapping.AddRelationshipMapping(relMap1);
            testMapping.AddRelationshipMapping(relMap2);
            testMapping.InterTypeAutoInternalResolution = false;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
                // Assert
                Assert.AreEqual(17, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(12, trnasformer.GeneratingRelationships.Count);
            }
        }

    }
}
