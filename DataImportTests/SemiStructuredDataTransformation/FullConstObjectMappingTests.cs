using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Transformation;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataImportTests.SemiStructuredDataTransformation
{
    [TestClass]
    public class FullConstObjectMappingTests
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

        [TestMethod]
        public void TransformConcepts_1ConstObjMapOnMultiRowSemiStructuredData_Generate1Object()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Org_Name_TypeUri = "name";
            const string Org_Name_Value = "سازمان ۱";

            ObjectMapping objMap = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Name_TypeUri), new ConstValueMappingItem(Org_Name_Value)) { IsSetAsDisplayName = true });
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap);

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(1, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(0, trnasformer.GeneratingRelationships.Count);

            Assert.AreEqual(Org_TypeUri, trnasformer.GeneratingObjects[0].TypeUri);
            Assert.AreEqual(2, trnasformer.GeneratingObjects[0].Properties.Count);
            Assert.IsTrue(trnasformer.GeneratingObjects[0].Properties.Any(p => p.TypeURI.Equals(Org_Name_TypeUri)));
            Assert.IsTrue(trnasformer.GeneratingObjects[0].Properties.Any(p => p.TypeURI.Equals(fakeOntology.GetDefaultDisplayNamePropertyTypeUri())));
            Assert.IsTrue(trnasformer.GeneratingObjects[0].Properties.All(p => p.Value.Equals(Org_Name_Value)));
        }

        [TestMethod]
        public void TransformConcepts_1ConstObjMapWithMultiValueContPropOnMultiRowSemiStructuredData_Generate1Object()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Org_Name_TypeUri = "name";
            const string Org_Name_Value = "سازمان ۱";
            const string Org_Address_TypeUri = "address";
            const string Org_County_Value = "کشور";
            const string Org_City_Value = "شهر";
            const string Org_LocalAddress_Value = "میدان ۱، خ ۲، پ ۳";

            ObjectMapping objMap = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Name_TypeUri), new ConstValueMappingItem(Org_Name_Value)) { IsSetAsDisplayName = true });
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Address_TypeUri)
                , new MultiValueMappingItem()
                {
                    MultiValues = new List<SingleValueMappingItem>()
                    {
                        new ConstValueMappingItem(Org_County_Value),
                        new ConstValueMappingItem(" "),
                        new ConstValueMappingItem(Org_City_Value),
                        new ConstValueMappingItem(" "),
                        new ConstValueMappingItem(Org_LocalAddress_Value)
                    }
                }));
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap);

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(1, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(0, trnasformer.GeneratingRelationships.Count);

            Assert.AreEqual(Org_TypeUri, trnasformer.GeneratingObjects[0].TypeUri);
            Assert.AreEqual(3, trnasformer.GeneratingObjects[0].Properties.Count);
            ImportingProperty tempProperty;
            tempProperty = trnasformer.GeneratingObjects[0].Properties.Single(p => p.TypeURI.Equals(Org_Name_TypeUri));
            Assert.AreEqual(Org_Name_Value, tempProperty.Value);
            tempProperty = trnasformer.GeneratingObjects[0].Properties.Single(p => p.TypeURI.Equals(Org_Address_TypeUri));
            Assert.AreEqual($"{Org_County_Value} {Org_City_Value} {Org_LocalAddress_Value}", tempProperty.Value);
        }

        [TestMethod]
        public void TransformConcepts_ObjMapWithMultiValuePropWithOnlyOneTableBasedPartOnMultiRowSemiStructuredData_GenerateMultipleObjects()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "no"},
                new string[] { "1" },
                new string[] { "1" },
                new string[] { "2" },
                new string[] { "3" }
            };
            const string Org_TypeUri = "organization";
            const string Org_Name_TypeUri = "name";
            const string Org_Name_Value = "سازمان ۱";
            const string Org_Address_TypeUri = "address";
            const string Org_County_Value = "کشور";
            const string Org_City_Value = "شهر";

            ObjectMapping objMap = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Name_TypeUri), new ConstValueMappingItem(Org_Name_Value)) { IsSetAsDisplayName = true });
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Address_TypeUri)
                , new MultiValueMappingItem()
                {
                    MultiValues = new List<SingleValueMappingItem>()
                    {
                        new ConstValueMappingItem(Org_County_Value),
                        new ConstValueMappingItem(" "),
                        new ConstValueMappingItem(Org_City_Value),
                        new ConstValueMappingItem(" میدان ۱، خ ۲، پ "),
                        new TableColumnMappingItem(0)
                    }
                }));
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap);

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(0, trnasformer.GeneratingRelationships.Count);

            for (int i = 0; i < trnasformer.GeneratingObjects.Count; i++)
            {
                Assert.AreEqual(Org_TypeUri, trnasformer.GeneratingObjects[i].TypeUri);
                Assert.AreEqual(3, trnasformer.GeneratingObjects[i].Properties.Count);
                ImportingProperty tempProperty;
                tempProperty = trnasformer.GeneratingObjects[i].Properties.Single(p => p.TypeURI.Equals(Org_Name_TypeUri));
                Assert.AreEqual(Org_Name_Value, tempProperty.Value);
                tempProperty = trnasformer.GeneratingObjects[i].Properties.Single(p => p.TypeURI.Equals(Org_Address_TypeUri));
                Assert.IsTrue(tempProperty.Value.StartsWith($"{Org_County_Value} {Org_City_Value} میدان ۱، خ ۲، پ "));
            }
        }

        [TestMethod]
        public void TransformConcepts_1ConstObjMapWithGeoTimeContPropOnMultiRowSemiStructuredData_Generate1Object()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Meeting_TypeUri = "meeting";
            const string Meeting_Name_TypeUri = "name";
            const string Meeting_Name_Value = "ملاقات ۱";
            const string Meeting_Lat_Value = "27.15";
            const string Meeting_Long_Value = "38.24";
            const string Meeting_TimeBegin_Value = "2010-11-12 15:16:17";
            const string Meeting_TimeEnd_Value = "2010-11-12 18:06:04";

            ObjectMapping objMap = new ObjectMapping(new OntologyTypeMappingItem(Meeting_TypeUri), "Meeting");
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Meeting_Name_TypeUri), new ConstValueMappingItem(Meeting_Name_Value)) { IsSetAsDisplayName = true });
            objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(fakeOntology.GetDateRangeAndLocationPropertyTypeUri())
                , new GeoTimeValueMappingItem()
                {
                    Latitude = new ConstValueMappingItem(Meeting_Lat_Value),
                    Longitude = new ConstValueMappingItem(Meeting_Long_Value),
                    TimeBegin = new ConstValueMappingItem(Meeting_TimeBegin_Value),
                    TimeEnd = new ConstValueMappingItem(Meeting_TimeEnd_Value)
                }));
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap);

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(1, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(0, trnasformer.GeneratingRelationships.Count);

            Assert.AreEqual(Meeting_TypeUri, trnasformer.GeneratingObjects[0].TypeUri);
            Assert.AreEqual(3, trnasformer.GeneratingObjects[0].Properties.Count);
            ImportingProperty tempProperty;
            tempProperty = trnasformer.GeneratingObjects[0].Properties.Single(p => p.TypeURI.Equals(Meeting_Name_TypeUri));
            Assert.AreEqual(Meeting_Name_Value, tempProperty.Value);
            tempProperty = trnasformer.GeneratingObjects[0].Properties.Single(p => p.TypeURI.Equals(fakeOntology.GetDateRangeAndLocationPropertyTypeUri()));
        }

        [TestMethod]
        public void TransformConcepts_2ConstObjMapOnMultiRowSemiStructuredData_Generate2Objects()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Org_Name_TypeUri = "org_name";
            const string Org_Name_Value = "سازمان ۱";
            const string Person_TypeUri = "person";
            const string Person_Name_TypeUri = "person_name";
            const string Person_Name_Value = "شخص ۱";

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            objMap1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Name_TypeUri), new ConstValueMappingItem(Org_Name_Value)) { IsSetAsDisplayName = true });
            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "Person");
            objMap2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_Name_TypeUri), new ConstValueMappingItem(Person_Name_Value)) { IsSetAsDisplayName = true });
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);

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
            Assert.AreEqual(0, trnasformer.GeneratingRelationships.Count);

            ImportingObject orgInstance = trnasformer.GeneratingObjects.Single(o => o.TypeUri.Equals(Org_TypeUri));
            Assert.IsTrue(orgInstance.TypeUri.Equals(Org_TypeUri));
            Assert.AreEqual(2, orgInstance.Properties.Count);
            Assert.IsTrue(orgInstance.Properties.Any(p => p.TypeURI.Equals(Org_Name_TypeUri)));
            Assert.IsTrue(orgInstance.Properties.All(p => p.Value.Equals(Org_Name_Value)));

            ImportingObject personInstance = trnasformer.GeneratingObjects.Single(o => o.TypeUri.Equals(Person_TypeUri));
            Assert.IsTrue(personInstance.TypeUri.Equals(Person_TypeUri));
            Assert.AreEqual(2, personInstance.Properties.Count);
            Assert.IsTrue(personInstance.Properties.Any(p => p.TypeURI.Equals(Person_Name_TypeUri)));
            Assert.IsTrue(personInstance.Properties.All(p => p.Value.Equals(Person_Name_Value)));
        }
        
        [TestMethod]
        public void TransformConcepts_2ConstObjMapWith1RelBetweenThemOnMultiRowSemiStructuredData_Generate1Rel()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Org_Name_TypeUri = "org_name";
            const string Org_Name_Value = "سازمان ۱";
            const string Person_TypeUri = "person";
            const string Person_Name_TypeUri = "person_name";
            const string Person_Name_Value = "شخص ۱";
            const string Rel_TypeUri = "relation";
            const string Rel_Description = "رابطه ۱";

            ObjectMapping objMap1 = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            objMap1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Org_Name_TypeUri), new ConstValueMappingItem(Org_Name_Value)) { IsSetAsDisplayName = true });
            ObjectMapping objMap2 = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "Person");
            objMap2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_Name_TypeUri), new ConstValueMappingItem(Person_Name_Value)) { IsSetAsDisplayName = true });
            RelationshipMapping relMap = new RelationshipMapping
                (objMap1, objMap2, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary
                , new ConstValueMappingItem(Rel_Description), new OntologyTypeMappingItem(Rel_TypeUri));
            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(objMap1);
            testMapping.AddObjectMapping(objMap2);
            testMapping.AddRelationshipMapping(relMap);

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(1, trnasformer.GeneratingRelationships.Count);
            Assert.AreEqual(Rel_TypeUri, trnasformer.GeneratingRelationships[0].TypeURI);
            Assert.AreEqual(Rel_Description, trnasformer.GeneratingRelationships[0].Description);
            Assert.AreEqual(ImportingRelationshipDirection.SourceToTarget, trnasformer.GeneratingRelationships[0].Direction);
        }

        [TestMethod]
        public void TransformConcepts_1ConstAnd1PropertyBasedObjMapOnSemiStructuredData_Generate1ConstObjAndMultipleVariableObj()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Person_TypeUri = "person";

            ObjectMapping constObjMap = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            constObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("name"), new ConstValueMappingItem("سازمان ۱")) { IsSetAsDisplayName = true });

            ObjectMapping varObjMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "Person");
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("person_code"), new TableColumnMappingItem(0, "کد", PropertyInternalResolutionOption.Ignorable)) { IsSetAsDisplayName = true });
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("phonenumber"), new TableColumnMappingItem(0, "شماره تلفن", PropertyInternalResolutionOption.Ignorable)));

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(constObjMap);
            testMapping.AddObjectMapping(varObjMap);

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(5, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(1, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Org_TypeUri)));
            Assert.AreEqual(4, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Person_TypeUri)));
            Assert.AreEqual(0, trnasformer.GeneratingRelationships.Count);
        }
        
        [TestMethod]
        public void TransformConcepts_1ConstAnd1PropertyBasedObjMapAnd1RelMapOnSemiStructuredData_Generate1ConstObjAndMultipleVariableObjAndRelToThem()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Person_TypeUri = "person";

            ObjectMapping constObjMap = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            constObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("name"), new ConstValueMappingItem("سازمان ۱")) { IsSetAsDisplayName = true });

            ObjectMapping varObjMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "Person");
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("person_code"), new TableColumnMappingItem(0, "کد", PropertyInternalResolutionOption.Ignorable)) { IsSetAsDisplayName = true });
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("phonenumber"), new TableColumnMappingItem(0, "شماره تلفن", PropertyInternalResolutionOption.Ignorable)));

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(constObjMap);
            testMapping.AddObjectMapping(varObjMap);
            testMapping.AddRelationshipMapping(new RelationshipMapping(constObjMap, varObjMap, RelationshipBaseLinkMappingRelationDirection.SecondaryToPrimary, new ConstValueMappingItem("مدیر"), new OntologyTypeMappingItem("مدیریت")));

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(5, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(1, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Org_TypeUri)));
            Assert.AreEqual(4, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Person_TypeUri)));
            Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
        }

        [TestMethod]
        public void TransformConcepts_1ConstAnd1PropertyBasedObjMapAnd1RelMapOnSemiStructuredDataWithResolution_Generate1ConstObjAndMultipleResolvedObjAndRelToThem()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "id", "PhoneNumber" },
                new string[] { "1", "35552358" },
                new string[] { "1", "35552358" },
                new string[] { "2", "35552359" },
                new string[] { "3", "35552360" }
            };
            const string Org_TypeUri = "organization";
            const string Person_TypeUri = "person";

            ObjectMapping constObjMap = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            constObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("name"), new ConstValueMappingItem("سازمان ۱")) { IsSetAsDisplayName = true });

            ObjectMapping varObjMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "Person");
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("person_code"), new TableColumnMappingItem(0, "کد", PropertyInternalResolutionOption.FindMatch)) { IsSetAsDisplayName = true });
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("phonenumber"), new TableColumnMappingItem(0, "شماره تلفن", PropertyInternalResolutionOption.Ignorable)));

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(constObjMap);
            testMapping.AddObjectMapping(varObjMap);
            testMapping.AddRelationshipMapping(new RelationshipMapping(constObjMap, varObjMap, RelationshipBaseLinkMappingRelationDirection.SecondaryToPrimary, new ConstValueMappingItem("مدیر"), new OntologyTypeMappingItem("مدیریت")));

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(1, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Org_TypeUri)));
            Assert.AreEqual(3, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Person_TypeUri)));
            Assert.AreEqual(3, trnasformer.GeneratingRelationships.Count);
        }

        [TestMethod]
        public void TransformConcepts_2ConstAnd1PropertyBasedEventMapBetweenThemOnSemiStructuredData_Generate2ConstObjAndMultipleEventsRelationedToThem()
        {
            // Assign
            string[][] rowData = new string[][]
            {
                new string[] { "Call_Time" },
                new string[] { "2010-10-12 11:00:00" },
                new string[] { "2010-10-12 12:00:00" },
                new string[] { "2010-10-12 13:00:00" },
                new string[] { "2010-10-12 13:00:00" }
            };
            const string Org_TypeUri = "organization";
            const string Call_Event_TypeUri = "call";

            ObjectMapping constObjMap1 = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            constObjMap1.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("name"), new ConstValueMappingItem("سازمان ۱")) { IsSetAsDisplayName = true });

            ObjectMapping constObjMap2 = new ObjectMapping(new OntologyTypeMappingItem(Org_TypeUri), "Organization");
            constObjMap2.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("name"), new ConstValueMappingItem("سازمان ۲")) { IsSetAsDisplayName = true });

            ObjectMapping varObjMap = new ObjectMapping(new OntologyTypeMappingItem(Call_Event_TypeUri), "Phone Call");
            varObjMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem("call_time"), new TableColumnMappingItem(0, "زمان تماس", PropertyInternalResolutionOption.Ignorable)) { IsSetAsDisplayName = true });

            TypeMapping testMapping = new TypeMapping();
            testMapping.AddObjectMapping(constObjMap1);
            testMapping.AddObjectMapping(constObjMap2);
            testMapping.AddObjectMapping(varObjMap);
            testMapping.AddRelationshipMapping(new RelationshipMapping(constObjMap1, varObjMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("برقراری تماس"), new OntologyTypeMappingItem("relation")));
            testMapping.AddRelationshipMapping(new RelationshipMapping(varObjMap, constObjMap2, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("دریافت تماس"), new OntologyTypeMappingItem("relation")));

            SemiStructuredDataTransformer trnasformer;

            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref rowData, testMapping);
            }
            // Assert
            Assert.AreEqual(6, trnasformer.GeneratingObjects.Count);
            Assert.AreEqual(2, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Org_TypeUri)));
            Assert.AreEqual(4, trnasformer.GeneratingObjects.Count(o => o.TypeUri.Equals(Call_Event_TypeUri)));
            Assert.AreEqual(8, trnasformer.GeneratingRelationships.Count);
        }
    }
}
