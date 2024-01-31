using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Transformation;
using GPAS.PropertiesValidation.Geo.Formats;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GPAS.DataImport.SemiStructuredDataTransformation.Tests
{
    [TestClass()]
    public class PropertyMappingTests
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
        public void TransformConcepts_ApplyRegExOnFields()
        {
            // Assign
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                string[][] rowData = new string[][]
                {
                    new string[] { "id", "PhoneNumber" },
                    new string[] { "1", "(0331)35552358" },
                    new string[] { "2", "21" },
                    new string[] { "3", "+98852 2356856" },
                    new string[] { "4", "kjhsgdkjfhb" }
                };

                const string Cell_phnoe_id_TypeUri = "Cell_phnoe_id";
                const string Cell_phone_number_TypeUri = "Cell_phone_number";

                ObjectMapping objMap = new ObjectMapping(new OntologyTypeMappingItem("Cell_phone"), "Cell Phone");
                objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Cell_phnoe_id_TypeUri), new TableColumnMappingItem(0)) { IsSetAsDisplayName = true });
                TableColumnMappingItem phoneNumberPropertyMappingValueItem = new TableColumnMappingItem(1);
                phoneNumberPropertyMappingValueItem.RegularExpressionPattern = @"(\(0\d{2,3}\)|\+98\d{2,3} )\d{4,10}";
                objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Cell_phone_number_TypeUri), phoneNumberPropertyMappingValueItem));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(objMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);

                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);

                // Assert
                foreach (ImportingObject obj in trnasformer.GeneratingObjects)
                {
                    foreach (ImportingProperty prop in obj.Properties)
                    {
                        if (prop.TypeURI.Equals(Cell_phnoe_id_TypeUri))
                        {
                            if (prop.Value.Equals(rowData[1][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any(ip => ip.TypeURI.Equals(Cell_phone_number_TypeUri) && ip.Value.Equals(rowData[1][1])));
                                break;
                            }
                            else if (prop.Value.Equals(rowData[2][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any(ip => ip.TypeURI.Equals(Cell_phone_number_TypeUri) && ip.Value.Equals("")));
                                break;
                            }
                            else if (prop.Value.Equals(rowData[3][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any(ip => ip.TypeURI.Equals(Cell_phone_number_TypeUri) && ip.Value.Equals(rowData[3][1])));
                                break;
                            }
                            else if (prop.Value.Equals(rowData[4][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any(ip => ip.TypeURI.Equals(Cell_phone_number_TypeUri) && ip.Value.Equals("")));
                                break;
                            }
                        }
                    }
                }
            }
        }

        [TestMethod()]
        public void TransformConcepts_ApplyGeoSpecialConvertOnFields()
        {
            // Assign
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                string[][] rowData = new string[][]
                {
                    new string[] { "id", "position_dms" },
                    new string[] { "1", "aaaa" },
                    new string[] { "2", "-48°N 36°12.20'W" },
                    new string[] { "3", "45 23 11 N, 154 25 12 E" },
                    new string[] { "4", "15 S" }
                };

                const string ship_id_TypeUri = "Ship_id";
                const string timeAndLocation_TypeUri = "time_and_location";

                ObjectMapping objMap = new ObjectMapping(new OntologyTypeMappingItem("Ship_Type"), "Ship");
                objMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(ship_id_TypeUri), new TableColumnMappingItem(0)) { IsSetAsDisplayName = true });
                TableColumnMappingItem latitudePropertyMappingValueItem = new TableColumnMappingItem(1);
                TableColumnMappingItem longitudePropertyMappingValueItem = new TableColumnMappingItem(1);
                var geoTimeValueMappingItem = new GeoTimeValueMappingItem()
                {
                    Latitude = latitudePropertyMappingValueItem,
                    Longitude = longitudePropertyMappingValueItem
                };
                var geoTimePropertyMappingItem = new GeoTimePropertyMapping(new OntologyTypeMappingItem(timeAndLocation_TypeUri), geoTimeValueMappingItem)
                {
                    GeoSpecialFormat = GeoSpecialTypes.CompoundDMS
                };
                objMap.AddProperty(geoTimePropertyMappingItem);
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(objMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);

                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);

                // Assert
                foreach (ImportingObject obj in trnasformer.GeneratingObjects)
                {
                    foreach (ImportingProperty prop in obj.Properties)
                    {
                        if (prop.TypeURI.Equals(ship_id_TypeUri))
                        {
                            if (prop.Value.Equals(rowData[1][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any
                                    (ip => ip.TypeURI.Equals(timeAndLocation_TypeUri)
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Latitude.Equals("")
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Longitude.Equals("")));
                                break;
                            }
                            else if (prop.Value.Equals(rowData[2][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any
                                    (ip => ip.TypeURI.Equals(timeAndLocation_TypeUri)
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Latitude.Equals("")
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Longitude.Equals("")));
                                break;
                            }
                            else if (prop.Value.Equals(rowData[3][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any
                                    (ip => ip.TypeURI.Equals(timeAndLocation_TypeUri)
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Latitude.Equals("")
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Longitude.Equals("")));
                                break;
                            }
                            else if (prop.Value.Equals(rowData[4][0]))
                            {
                                Assert.IsTrue(obj.Properties.Any
                                    (ip => ip.TypeURI.Equals(timeAndLocation_TypeUri)
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Latitude.Equals("-15")
                                    && PropertiesValidation.GeoTime.GeoTimeEntityRawData(ip.Value).Longitude.Equals("")));
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}