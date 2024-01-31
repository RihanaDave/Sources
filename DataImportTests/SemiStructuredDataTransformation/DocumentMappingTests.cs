using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Transformation;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace GPAS.DataImport.SemiStructuredDataTransformation.Tests
{
    [TestClass()]
    public class DocumentMappingTests
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
        public void TransformConcepts_DefineSingleDocumentMapping()
        {
            // Assign
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                { return true; };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { @"D:\test\1\1.txt" }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SingleFile = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(1, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual("TXT", trnasformer.GeneratingObjects[0].TypeUri);
                Assert.AreEqual(1, trnasformer.GeneratingObjects[0].Properties.Count);
                Assert.IsInstanceOfType(trnasformer.GeneratingObjects[0], typeof(ImportingDocument));
                Assert.AreEqual("1.txt", (trnasformer.GeneratingObjects[0] as ImportingDocument).LabelProperty.Value);
                Assert.AreEqual(rowData[1][0], (trnasformer.GeneratingObjects[0] as ImportingDocument).DocumentPath);
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineSingleDocumentMappingForPathOfAnEmptyFile()
        {
            // Assign
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                { return true; };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 0; };

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { @"D:\test\1\1.txt" }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SingleFile = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(0, trnasformer.GeneratingObjects.Count);
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineSingleFolderOfDocumentsMapping()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string subFile1 = @"D:\test\1\1.txt";
                string subFile2 = @"D:\test\1\2.txt";

                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    return new FileInfo[]
                    {
                        new FileInfo(subFile1),
                        new FileInfo(subFile2)
                    };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { @"D:\test\1\" }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.FolderContent = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Count);

                Assert.AreEqual("TXT", trnasformer.GeneratingObjects[0].TypeUri);
                Assert.AreEqual(1, trnasformer.GeneratingObjects[0].Properties.Count);
                Assert.IsInstanceOfType(trnasformer.GeneratingObjects[0], typeof(ImportingDocument));

                Assert.AreEqual("TXT", trnasformer.GeneratingObjects[1].TypeUri);
                Assert.AreEqual(1, trnasformer.GeneratingObjects[1].Properties.Count);
                Assert.IsInstanceOfType(trnasformer.GeneratingObjects[1], typeof(ImportingDocument));

                ImportingDocument imDoc1 = (trnasformer.GeneratingObjects[0] as ImportingDocument);
                ImportingDocument imDoc2 = (trnasformer.GeneratingObjects[1] as ImportingDocument);

                if (imDoc1.DocumentPath.Equals(subFile1) && imDoc2.DocumentPath.Equals(subFile2))
                {
                    Assert.AreEqual("1.txt", imDoc1.LabelProperty.Value);
                    Assert.AreEqual("2.txt", imDoc2.LabelProperty.Value);
                }
                else if (imDoc1.DocumentPath.Equals(subFile2) && imDoc2.DocumentPath.Equals(subFile1))
                {
                    Assert.AreEqual("2.txt", imDoc1.LabelProperty.Value);
                    Assert.AreEqual("1.txt", imDoc2.LabelProperty.Value);
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineFolderOfFoldersOfDocumentsMapping()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string rootDir = @"D:\test\1\";
                string subDir1 = @"D:\test\1\1-1\";
                string subFile1_1 = @"D:\test\1\1-1\1.txt";
                string subFile1_2 = @"D:\test\1\1-1\2.txt";

                string subDir2 = @"D:\test\1\1-2\";
                string subFile2_1 = @"D:\test\1\1-2\3.txt";
                string subFile2_2 = @"D:\test\1\1-2\4.txt";
                string subFile2_3 = @"D:\test\1\1-2\5.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                {
                    if (di.FullName.Equals(rootDir))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir1),
                            new DirectoryInfo(subDir2)
                        };
                    }
                    return new DirectoryInfo[] { };
                };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(subDir1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    if (di.FullName.Equals(subDir2))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile2_1),
                            new FileInfo(subFile2_2),
                            new FileInfo(subFile2_3)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { rootDir }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SubFoldersContent = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(5, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o.TypeUri.Equals("TXT")));
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o.Properties.Count == 1));
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o is ImportingDocument));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("3.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("4.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("5.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineNestedFoldersOfDocumentsMapping()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string rootDir = @"D:\test\1\";
                string subDir1 = @"D:\test\1\1-1\";
                string subFile1_1 = @"D:\test\1\1-1\1.txt";
                string subFile1_2 = @"D:\test\1\1-1\2.txt";

                string subDir2 = @"D:\test\1\1-1\1-1-1\";
                string subFile2_1 = @"D:\test\1\1-1\1-1-1\3.txt";
                string subFile2_2 = @"D:\test\1\1-1\1-1-1\4.txt";
                string subFile2_3 = @"D:\test\1\1-1\1-1-1\5.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                {
                    if (di.FullName.Equals(rootDir))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir1)
                        };
                    }
                    if (di.FullName.Equals(subDir1))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir2)
                        };
                    }
                    return new DirectoryInfo[] { };
                };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(subDir1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    if (di.FullName.Equals(subDir2))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile2_1),
                            new FileInfo(subFile2_2),
                            new FileInfo(subFile2_3)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { rootDir }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SubFoldersContent = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(5, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o.TypeUri.Equals("TXT")));
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o.Properties.Count == 1));
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o is ImportingDocument));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("3.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("4.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("5.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_AllPathOptionsTrue()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1 = @"D:\test\1\1.txt";
                string subFile2 = @"D:\test\1\2.txt";
                string subFile3 = @"D:\test\1\3.txt";

                string subDir1 = @"D:\test\1\1-1\";
                string subFile1_1_1 = @"D:\test\1\1-1\11.txt";
                string subFile1_1_2 = @"D:\test\1\1-1\12.txt";

                string testPath2 = @"D:\test\2\21.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir1)
                        };
                    }
                    return new DirectoryInfo[] { };
                };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1),
                            new FileInfo(subFile2),
                            new FileInfo(subFile3)
                        };
                    }
                    if (di.FullName.Equals(subDir1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1_1),
                            new FileInfo(subFile1_1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { testPath1 },
                    new string[] { testPath2 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SingleFile = true;
                docMap.PathOptions.FolderContent = true;
                docMap.PathOptions.SubFoldersContent = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(6, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o.TypeUri.Equals("TXT")));
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o.Properties.Count == 1));
                Assert.IsTrue(trnasformer.GeneratingObjects.All(o => o is ImportingDocument));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("3.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("11.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("12.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("21.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_OnlySingleFileOptionTrue()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1 = @"D:\test\1\1.txt";
                string subFile2 = @"D:\test\1\2.txt";
                string subFile3 = @"D:\test\1\3.txt";

                string subDir1 = @"D:\test\1\1-1\";
                string subFile1_1_1 = @"D:\test\1\1-1\11.txt";
                string subFile1_1_2 = @"D:\test\1\1-1\12.txt";

                string testPath2 = @"D:\test\2\21.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir1)
                        };
                    }
                    return new DirectoryInfo[] { };
                };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1),
                            new FileInfo(subFile2),
                            new FileInfo(subFile3)
                        };
                    }
                    if (di.FullName.Equals(subDir1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1_1),
                            new FileInfo(subFile1_1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { testPath1 },
                    new string[] { testPath2 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SingleFile = true;
                docMap.PathOptions.FolderContent = false;
                docMap.PathOptions.SubFoldersContent = false;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(1, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("21.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_OnlyFolderContentOptionTrue()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1 = @"D:\test\1\1.txt";
                string subFile2 = @"D:\test\1\2.txt";
                string subFile3 = @"D:\test\1\3.txt";

                string subDir1 = @"D:\test\1\1-1\";
                string subFile1_1_1 = @"D:\test\1\1-1\11.txt";
                string subFile1_1_2 = @"D:\test\1\1-1\12.txt";

                string testPath2 = @"D:\test\2\21.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir1)
                        };
                    }
                    return new DirectoryInfo[] { };
                };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1),
                            new FileInfo(subFile2),
                            new FileInfo(subFile3)
                        };
                    }
                    if (di.FullName.Equals(subDir1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1_1),
                            new FileInfo(subFile1_1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { testPath1 },
                    new string[] { testPath2 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SingleFile = false;
                docMap.PathOptions.FolderContent = true;
                docMap.PathOptions.SubFoldersContent = false;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(3, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("3.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_OnlySubFoldersContentOptionTrue()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1 = @"D:\test\1\1.txt";
                string subFile2 = @"D:\test\1\2.txt";
                string subFile3 = @"D:\test\1\3.txt";

                string subDir1 = @"D:\test\1\1-1\";
                string subFile1_1_1 = @"D:\test\1\1-1\11.txt";
                string subFile1_1_2 = @"D:\test\1\1-1\12.txt";

                string testPath2 = @"D:\test\2\21.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new DirectoryInfo[]
                        {
                            new DirectoryInfo(subDir1)
                        };
                    }
                    return new DirectoryInfo[] { };
                };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1),
                            new FileInfo(subFile2),
                            new FileInfo(subFile3)
                        };
                    }
                    if (di.FullName.Equals(subDir1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1_1),
                            new FileInfo(subFile1_1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "docPath" },
                    new string[] { testPath1 },
                    new string[] { testPath2 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Document_TypeUri = "Document";

                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(0));
                docMap.PathOptions.SingleFile = false;
                docMap.PathOptions.FolderContent = false;
                docMap.PathOptions.SubFoldersContent = true;
                docMap.SetDocumentNameAsDisplayName();
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(docMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("11.txt")));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.LabelProperty.Value.Equals("12.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineSingleDocumentMappingWithOneRelationshipToPersonWhereDocumentFieldRefersToSingleFile()
        {
            // Assign
            using (ShimsContext.Create())
            {
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                { return true; };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath" },
                    new string[] { "1", "reza", @"D:\test\1\1.txt" }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2));
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.TypeUri.Equals(Person_TypeUri)));
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.TypeUri.Equals("TXT")));
                Assert.AreEqual(1, trnasformer.GeneratingRelationships.Count);
                Assert.AreEqual(AppearedIn_TypeUri, trnasformer.GeneratingRelationships[0].TypeURI);
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineDocumentMappingWithOneRelationshipToPersonWhereDocumentFieldRefersToFolder()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1 = @"D:\test\1\1.txt";
                string subFile2 = @"D:\test\1\2.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return true; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1),
                            new FileInfo(subFile2)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath" },
                    new string[] { "1", "reza", testPath1 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2));
                docMap.PathOptions.FolderContent = true;
                docMap.PathOptions.SubFoldersContent = false;
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(3, trnasformer.GeneratingObjects.Count);
                Assert.IsTrue(trnasformer.GeneratingObjects.Any(o => o.TypeUri.Equals(Person_TypeUri)));
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).Count());
                Assert.AreEqual(2, trnasformer.GeneratingRelationships.Count);
                Assert.AreEqual(AppearedIn_TypeUri, trnasformer.GeneratingRelationships[0].TypeURI);
                Assert.AreEqual(AppearedIn_TypeUri, trnasformer.GeneratingRelationships[1].TypeURI);
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineDocumentRelationshipToPersonWithSperateFoldersForEachPerson()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1_1 = @"D:\test\1\1.txt";
                string subFile1_2 = @"D:\test\1\2.txt";

                string testPath2 = @"D:\test\2\";
                string subFile2_1 = @"D:\test\2\3.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return di.FullName.Equals(testPath1) || di.FullName.Equals(testPath2); };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                { return new DirectoryInfo[] { }; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    if (di.FullName.Equals(testPath2))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile2_1)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath" },
                    new string[] { "1", "reza", testPath1 },
                    new string[] { "2", "amin", testPath2 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2));
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(5, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals(Person_TypeUri)).Count());
                Assert.AreEqual(3, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).Count());
                Assert.AreEqual(3, trnasformer.GeneratingRelationships.Count);
                Assert.IsTrue(trnasformer.GeneratingRelationships.All(r => r.TypeURI.Equals(AppearedIn_TypeUri)));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("3.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineDocumentRelationshipToPersonWithSameFolderForAllPersonWithoutResolvingDocumentPathes()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1_1 = @"D:\test\1\1.txt";
                string subFile1_2 = @"D:\test\1\2.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return di.FullName.Equals(testPath1); };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                { return new DirectoryInfo[] { }; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath" },
                    new string[] { "1", "reza", testPath1 },
                    new string[] { "2", "amin", testPath1 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.Ignorable));
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(6, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals(Person_TypeUri)).Count());
                Assert.AreEqual(4, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).Count());
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
                Assert.IsTrue(trnasformer.GeneratingRelationships.All(r => r.TypeURI.Equals(AppearedIn_TypeUri)));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineDocumentRelationshipToPersonWithSameFolderForAllPersonWithResolvingDocumentPathes()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1_1 = @"D:\test\1\1.txt";
                string subFile1_2 = @"D:\test\1\2.txt";
                
                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return di.FullName.Equals(testPath1); };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                { return new DirectoryInfo[] { }; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath" },
                    new string[] { "1", "reza", testPath1 },
                    new string[] { "2", "amin", testPath1 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.FindMatch));
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals(Person_TypeUri)).Count());
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).Count());
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
                Assert.IsTrue(trnasformer.GeneratingRelationships.All(r => r.TypeURI.Equals(AppearedIn_TypeUri)));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
            }
        }


        [TestMethod()]
        public void TransformConcepts_DefineDocumentRelationshipToPersonWithSameFolderForAllPersonWithResolvingDocumentPathesAndAdditionalProperty()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1_1 = @"D:\test\1\1.txt";
                string subFile1_2 = @"D:\test\1\2.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return di.FullName.Equals(testPath1); };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                { return new DirectoryInfo[] { }; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath", "docAuthor" },
                    new string[] { "1", "reza", testPath1, "author1" },
                    new string[] { "2", "amin", testPath1, "author1" }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string Author_typeUri = "Author";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.FindMatch));
                docMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Author_typeUri), new TableColumnMappingItem(3, "نویسنده", PropertyInternalResolutionOption.FindMatch)));
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(4, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals(Person_TypeUri)).Count());
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).Count());
                Assert.IsTrue(trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).All(o => o.Properties.Any(p=> p.TypeURI.Equals(Author_typeUri) && p.Value.Equals(rowData[1][3]))));
                Assert.IsTrue(trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).All(o => o.Properties.Count == 2));
                Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
                Assert.IsTrue(trnasformer.GeneratingRelationships.All(r => r.TypeURI.Equals(AppearedIn_TypeUri)));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[1][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("1.txt")));
                Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.LabelProperty.Value.Equals(rowData[2][1]) && r.Target.LabelProperty.Value.Equals("2.txt")));
            }
        }

        [TestMethod()]
        public void TransformConcepts_DefineDocumentRelationshipToPersonWithSameFolderAndResolvablePathesAndPersons()
        {
            // Assign
            using (ShimsContext.Create())
            {
                string testPath1 = @"D:\test\1\";
                string subFile1_1 = @"D:\test\1\1.txt";
                string subFile1_2 = @"D:\test\1\2.txt";

                #region Fake Assigns
                SetNeededAppsettingsFakeAssigns();
                System.IO.Fakes.ShimFileInfo.AllInstances.ExistsGet = (fi) =>
                {
                    if (fi.FullName.EndsWith("\\"))
                        return false;
                    else
                        return true;
                };
                System.IO.Fakes.ShimFileInfo.AllInstances.LengthGet = (fi) =>
                { return 100; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.ExistsGet = (di) =>
                { return di.FullName.Equals(testPath1); };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetDirectories = (di) =>
                { return new DirectoryInfo[] { }; };
                System.IO.Fakes.ShimDirectoryInfo.AllInstances.GetFiles = (di) =>
                {
                    if (di.FullName.Equals(testPath1))
                    {
                        return new FileInfo[]
                        {
                            new FileInfo(subFile1_1),
                            new FileInfo(subFile1_2)
                        };
                    }
                    return new FileInfo[] { };
                };
                Ontology.Fakes.ShimOntology.AllInstances.GetDocumentTypeUriByFileExtensionString = (onto, ext) =>
                { return ext.ToUpper(); };
                #endregion

                string[][] rowData = new string[][]
                {
                    new string[] { "id", "name", "docPath" },
                    new string[] { "1", "reza", testPath1 },
                    new string[] { "1", "reza", testPath1 }
                };
                string[][] passingData = new string[rowData.Length][];
                Array.Copy(rowData, passingData, rowData.Length);

                const string Person_TypeUri = "Person";
                const string Person_id_TypeUri = "Person_id";
                const string Person_name_TypeUri = "Person_number";
                const string Document_TypeUri = "Document";
                const string AppearedIn_TypeUri = "Appeared_In";

                ObjectMapping personMap = new ObjectMapping(new OntologyTypeMappingItem(Person_TypeUri), "شخص");
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_id_TypeUri), new TableColumnMappingItem(0, "شناسه", PropertyInternalResolutionOption.FindMatch)));
                personMap.AddProperty(new PropertyMapping(new OntologyTypeMappingItem(Person_name_TypeUri), new TableColumnMappingItem(1)) { IsSetAsDisplayName = true });
                DocumentMapping docMap = new DocumentMapping(new OntologyTypeMappingItem(Document_TypeUri), "سند", new TableColumnMappingItem(2, "", PropertyInternalResolutionOption.FindMatch));
                RelationshipMapping relMap = new RelationshipMapping(personMap, docMap, RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary, new ConstValueMappingItem("حضور در"), new OntologyTypeMappingItem(AppearedIn_TypeUri));
                TypeMapping testMapping = new TypeMapping();
                testMapping.AddObjectMapping(personMap);
                testMapping.AddObjectMapping(docMap);
                testMapping.AddRelationshipMapping(relMap);

                SemiStructuredDataTransformer trnasformer = new SemiStructuredDataTransformer(fakeOntology);
                // Act
                trnasformer.TransformConcepts(ref passingData, testMapping);
                // Assert
                Assert.AreEqual(3, trnasformer.GeneratingObjects.Count);
                Assert.AreEqual(1, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals(Person_TypeUri)).Count());
                Assert.AreEqual(2, trnasformer.GeneratingObjects.Where(o => o.TypeUri.Equals("TXT")).Count());
                Assert.IsTrue(trnasformer.GeneratingRelationships.All(r => r.TypeURI.Equals(AppearedIn_TypeUri)));
                // بررسی شرایط نامطلوب غیرفعال شد
                //Assert.AreEqual(4, trnasformer.GeneratingRelationships.Count);
                //Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.DisplayName.Equals(rowData[1][1]) && r.Target.DisplayName.Equals("1.txt")));
                //Assert.IsTrue(trnasformer.GeneratingRelationships.Any(r => r.Source.DisplayName.Equals(rowData[1][1]) && r.Target.DisplayName.Equals("2.txt")));
            }
        }
    }
}