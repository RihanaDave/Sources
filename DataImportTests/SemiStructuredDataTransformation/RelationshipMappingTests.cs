using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Transformation;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.DataImport.SemiStructuredDataTransformation.Tests
{
    [TestClass()]
    public class RelationshipMappingTests
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
        public void TransformConcepts_ForRelationshipWithResolvedSourceAndNotResolvedTarget_NoRedundantRelationshipWillGenerate()
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
            objMap1.AddProperty
                (new PropertyMapping
                    (new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri)
                    , new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                    )
                { IsSetAsDisplayName = true });
            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap2.AddProperty
                (new PropertyMapping
                    (new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri)
                    , new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                    )
                { IsSetAsDisplayName = true });
            RelationshipMapping relMap = new RelationshipMapping
                (objMap1, objMap2, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary
                , new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("Call"));
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
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void TransformConcepts_ForNotResolvedRelationshipEnds_GenerateRedundantRelationships()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id" },
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "6" },
                new string[] { "4", "6" }
            };
            const string Cell_phnoe_id_TypeUri = "Cell_phone_id";
            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri), new TableColumnMappingItem(0)) { IsSetAsDisplayName = true });
            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
            RelationshipMapping relMap = new RelationshipMapping
                (objMap1, objMap2, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary
                , new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("Call"));
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
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void TransformConcepts_ForResolvedRelationshipSourceAndTargetEnds_DoNotGenerateRedundantRelationships()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id" },
                new string[] { "1", "5" },
                new string[] { "2", "6" },
                new string[] { "2", "6" },
                new string[] { "4", "6" }
            };
            const string Cell_phnoe_id_TypeUri = "Cell_phone_id";
            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty
                (new PropertyMapping
                    (new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri)
                    , new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                    )
                { IsSetAsDisplayName = true });
            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap2.AddProperty
                (new PropertyMapping
                    (new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri)
                    , new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                    )
                { IsSetAsDisplayName = true });
            RelationshipMapping relMap = new RelationshipMapping
                (objMap1, objMap2, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary
                , new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("Call"));
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
                Assert.AreEqual(3, trnasformer.GeneratingRelationships.Count);
            }
        }

        [TestMethod()]
        public void TransformConcepts_ForReplacingSourceAndTargetEnds_GenerateSeparateRelationships()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "sender_id", "receiver_id" },
                new string[] { "2", "4" },
                new string[] { "4", "2" }
            };
            const string Cell_phnoe_id_TypeUri = "Cell_phone_id";
            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap1.AddProperty
                (new PropertyMapping
                    (new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri)
                    , new TableColumnMappingItem(0) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                    )
                { IsSetAsDisplayName = true });
            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
            objMap2.AddProperty
                (new PropertyMapping
                    (new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri)
                    , new TableColumnMappingItem(1) { ResolutionOption = PropertyInternalResolutionOption.FindMatch }
                    )
                { IsSetAsDisplayName = true });

            RelationshipMapping relMap = new RelationshipMapping
                (objMap1, objMap2, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary
                , new ConstValueMappingItem("desc"), new OntologyTypeMappingItem("Call"));
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);
            testMapping.InterTypeAutoInternalResolution = true;
            SemiStructuredDataTransformer trnasformer;
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();

                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(2, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(2, trnasformer.GeneratingRelationships.Count);
        }

        //Same Source and Traget relationships may not generate
    }
}