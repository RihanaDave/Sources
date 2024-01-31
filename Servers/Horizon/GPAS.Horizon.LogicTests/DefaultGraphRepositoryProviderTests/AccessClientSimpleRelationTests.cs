using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.FilterSearch;
using GPAS.Horizon.GraphRepository;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Graph;
using GPAS.Horizon.Logic;
using GPAS.SearchAround;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Horizon.LogicTests.DefaultGraphRepositoryProviderTests
{
    [TestClass()]
    public class AccessClientSimpleRelationTests
    {
        #region Shared definitions
        private IAccessClient accessClient = null;
        const long Vertex1SupposalID = 10000000368;
        const long Vertex2SupposalID = 10000000393;
        List<Vertex> vertices;
        ACL supposalAcl;
        List<Edge> edges;
        AuthorizationParametters retrieveMatchAuthParam;
        CustomSearchAroundCriteria simpleCsaCriteria;
        CustomSearchAroundCriteria propertyMatchingCriteria;

        Dictionary<string, long[]> item = new Dictionary<string, long[]>();
        #endregion

        #region Tests Start-up / Clean-up
        [TestInitialize]
        public void TestInit()
        {
            // Shared "Assign"s
            List<string> supposalGroups = new List<string> { "group1", "group2" };

            ResetAccessClient();

            accessClient.AddNewGroupPropertiesToEdgeClass(supposalGroups);

            item.Add("شخص", new long[] { Vertex1SupposalID });

            vertices = new List<Vertex>() {
                new Vertex() { ID = Vertex1SupposalID, TypeUri = "شخص", Properties = new List<VertexProperty>() {
                        new VertexProperty() { OwnerVertexID = Vertex1SupposalID, OwnerVertexTypeURI = "شخص", TypeUri = "قد", Value = "0.0"},
                        new VertexProperty() { OwnerVertexID = Vertex1SupposalID, OwnerVertexTypeURI = "شخص", TypeUri = "سن", Value = "0"},
                        new VertexProperty() { OwnerVertexID = Vertex1SupposalID, OwnerVertexTypeURI = "شخص", TypeUri = "نام", Value = "فرهاد"}}},
                new Vertex() { ID = Vertex2SupposalID, TypeUri = "سند", Properties = new List<VertexProperty>() {
                        new VertexProperty() { OwnerVertexID = Vertex2SupposalID, OwnerVertexTypeURI = "سند", TypeUri = "منبع", Value = "100.0"},
                        new VertexProperty() { OwnerVertexID = Vertex2SupposalID, OwnerVertexTypeURI = "سند", TypeUri = "منبع", Value = "100"},
                        new VertexProperty() { OwnerVertexID = Vertex2SupposalID, OwnerVertexTypeURI = "سند", TypeUri = "نام", Value = "پایان نامه"}}}
            };
            supposalAcl = new ACL()
            {
                Permissions = new List<ACI>() {
                    new ACI() { GroupName = supposalGroups[0], AccessLevel = Permission.Read }},
                Classification = "C1"
            };
            edges = new List<Edge>() {
                new Edge() { TypeUri = "شبیه", ID = 4000, Direction = LinkDirection.Bidirectional, SourceVertexID = Vertex1SupposalID,
                    TargetVertexID = Vertex2SupposalID, SourceVertexTypeUri = "شخص", TargetVertexTypeUri = "سند", Acl = supposalAcl },
            };
            retrieveMatchAuthParam = new AuthorizationParametters()
            {
                permittedGroupNames = supposalGroups,
                readableClassifications = new List<string> { "C1", "C2" }
            };
            simpleCsaCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { vertices[0].TypeUri },
                LinksFromSearchSet = new SearchAroundStep[] {
                    new SearchAroundStep() { LinkTypeUri =new string[] { edges[0].TypeUri }, TargetObjectTypeUri = new string[] { vertices[1].TypeUri } }
                }
            };
            propertyMatchingCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { vertices[0].TypeUri },
                LinksFromSearchSet = new SearchAroundStep[] {
                    new SearchAroundStep() {
                        LinkTypeUri = new string[] {edges[0].TypeUri },
                        TargetObjectTypeUri = new string[] { vertices[1].TypeUri },
                        TargetObjectPropertyCriterias = new PropertyValueCriteria[] {
                            new PropertyValueCriteria() {
                                PropertyTypeUri = vertices[1].Properties[2].TypeUri,
                                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair() {
                                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                    CriteriaValue = vertices[1].Properties[2].Value
                                }
                            }
                        }
                    }
                }
            };
        }

        private void ResetAccessClient()
        {
            accessClient = AccessClientFactory.GetNewInstanceOfDefaultGraphRepositoryAccessClient();
        }

        //[TestCleanup]
        //public void TestCleanup()
        //{
        //}

        private void DeleteVetices(List<Vertex> vertices)
        {
            ResetAccessClient();
            IEnumerable<IGrouping<string, Vertex>> verticesGroupedByTypeUri = vertices.GroupBy(v => v.TypeUri);
            foreach (IGrouping<string, Vertex> groupedVertices in verticesGroupedByTypeUri)
            {
                accessClient.DeleteVertices(groupedVertices.Key, vertices.Select(v => v.ID).ToList());
            }
            accessClient.ApplyChanges();
        }
        #endregion

        #region Deprecated

        //[TestMethod()]
        // تابع مذکور ویژگی‌ها را از مخزن داده‌ها بازیابی می‌کند و نمی‌توان
        // انتظار داشت شئی که به مخزن گرافی افزوده شده، از مخزن داده‌ای بازیابی شود
        public void GetVertexPropertiesUnionTest()
        {
            //Assign
            List<Vertex> vertices = new List<Vertex>();

            vertices.Add(new Vertex()
            {
                ID = 10000000368,
                TypeUri = "شخص",
                Properties = new List<VertexProperty>()
            });
            vertices.ElementAt(0).Properties.Add(new VertexProperty()
            {
                OwnerVertexID = 10000000368,
                OwnerVertexTypeURI = "شخص",
                TypeUri = "قد",
                Value = "0"
            });
            vertices.ElementAt(0).Properties.Add(new VertexProperty()
            {
                OwnerVertexID = 10000000368,
                OwnerVertexTypeURI = "شخص",
                TypeUri = "سن",
                Value = "0"
            });
            vertices.ElementAt(0).Properties.Add(new VertexProperty()
            {
                OwnerVertexID = 10000000368,
                OwnerVertexTypeURI = "شخص",
                TypeUri = "نام",
                Value = "سلمان"
            });

            vertices.Add(new Vertex()
            {
                ID = 10000000393,
                TypeUri = "سند",
                Properties = new List<VertexProperty>()
            });
            vertices.ElementAt(1).Properties.Add(new VertexProperty()
            {
                OwnerVertexID = 10000000393,
                OwnerVertexTypeURI = "سند",
                TypeUri = "منبع",
                Value = "100"
            });
            vertices.ElementAt(1).Properties.Add(new VertexProperty()
            {
                OwnerVertexID = 10000000393,
                OwnerVertexTypeURI = "سند",
                TypeUri = "منبع",
                Value = "100"
            });
            vertices.ElementAt(1).Properties.Add(new VertexProperty()
            {
                OwnerVertexID = 10000000393,
                OwnerVertexTypeURI = "سند",
                TypeUri = "نام",
                Value = "پایان نامه"
            });

            List<VertexProperty> expectedResult = new List<VertexProperty>();
            foreach (var vertex in vertices)
            {
                expectedResult.Add(new VertexProperty()
                {
                    OwnerVertexID = vertex.ID,
                    TypeUri = "ID",
                    Value = vertex.ID.ToString()
                });
                foreach (var property in vertex.Properties)
                {
                    expectedResult.Add(new VertexProperty()
                    {
                        OwnerVertexID = vertex.ID,
                        TypeUri = property.TypeUri,
                        Value = property.Value.ToString()
                    });
                }
            }

            List<VertexProperty> result;

            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.ApplyChanges();
                result = accessClient.GetVertexPropertiesUnion(vertices.Select(v => v.ID).ToList());
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            bool isMatch;
            foreach (var item in result)
            {
                isMatch = false;
                foreach (var expectedItem in expectedResult)
                {
                    if ((item.TypeUri == expectedItem.TypeUri) &&
                        (item.Value == expectedItem.Value) &&
                        (item.OwnerVertexID == expectedItem.OwnerVertexID))
                    {
                        isMatch = true;
                        break;
                    }
                }
                if (isMatch == false)
                {
                    Assert.Fail();
                }
            }
        }
        #endregion

        [TestMethod()]
        public void AddVertex_AndFinallyCleanItUp_NoExceptionMayThrown()
        {
            // Assign
            List<Vertex> vertices = new List<Vertex>()
            {
                new Vertex() { ID = 10000000365, TypeUri = "شخص", Properties = new List<VertexProperty>()
                    {
                        new VertexProperty() { OwnerVertexID = 10000000365, OwnerVertexTypeURI = "شخص", TypeUri = "قد", Value = "0.0"}
                    }
                }
            };
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.ApplyChanges();
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            //  No exception may thrown!
        }

        [TestMethod()]
        public void AddTwoVertexAndOneEdge_AndFinallyCleanThemUp_NoExceptionMayThrown()
        {
            // Assign
            //  Uses shared "Assign"s
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            //  No exception may thrown!
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnSourceVertexWithTargetType_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices(item, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, retrievedResults.Count);
            Assert.AreEqual(Vertex1SupposalID, retrievedResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedResults[0].NotLoadedResults.Length);
            Assert.AreEqual(Vertex2SupposalID, retrievedResults[0].NotLoadedResults[0].TargetObjectID);
            Assert.AreEqual(edges[0].ID, retrievedResults[0].NotLoadedResults[0].RelationshipID);
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnSourceVertexWithTargetTypeAndS2TEdgeDirection_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = LinkDirection.SourceToTarget;
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;

            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices(item, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, retrievedResults.Count);
            Assert.AreEqual(Vertex1SupposalID, retrievedResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedResults[0].NotLoadedResults.Length);
            Assert.AreEqual(Vertex2SupposalID, retrievedResults[0].NotLoadedResults[0].TargetObjectID);
            Assert.AreEqual(edges[0].ID, retrievedResults[0].NotLoadedResults[0].RelationshipID);
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnSourceVertexWithTargetTypeAndT2SEdgeDirection_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = LinkDirection.TargetToSource;
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;

            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices(item, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, retrievedResults.Count);
            Assert.AreEqual(Vertex1SupposalID, retrievedResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedResults[0].NotLoadedResults.Length);
            Assert.AreEqual(Vertex2SupposalID, retrievedResults[0].NotLoadedResults[0].TargetObjectID);
            Assert.AreEqual(edges[0].ID, retrievedResults[0].NotLoadedResults[0].RelationshipID);
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnTargetVertexWithSourceType_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;

            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices(item, new string[] { vertices[0].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, retrievedResults.Count);
            Assert.AreEqual(Vertex2SupposalID, retrievedResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedResults[0].NotLoadedResults.Length);
            Assert.AreEqual(Vertex1SupposalID, retrievedResults[0].NotLoadedResults[0].TargetObjectID);
            Assert.AreEqual(edges[0].ID, retrievedResults[0].NotLoadedResults[0].RelationshipID);
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnSourceVertexWithATypeExceptTargetType_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices(item, new string[] { "Another_Type" }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(0, retrievedResults.Count);
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnSourceVertexWithWithNotMatchingAcis_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            retrieveMatchAuthParam.permittedGroupNames = new List<string> { "group3" };
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices(item, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(0, retrievedResults.Count);
        }

        [TestMethod()]
        public void RetrieveRelatedVertices_ForOneAddedEdgeOnSourceVertexWithWithNotMatchingClassification_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            retrieveMatchAuthParam.readableClassifications = new List<string> { "C3" };
            List<RelationshipBasedResultsPerSearchedObjects> retrievedResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedResults = accessClient.RetrieveRelatedVertices
                    (item, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(0, retrievedResults.Count);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithMatchingSimpleCriteriaFromSource_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex1SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[1].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithMatchingSimpleCriteriaFromSourceS2TEdgeDirection_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = LinkDirection.SourceToTarget;
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex1SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[1].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithMatchingSimpleCriteriaFromSourceT2SEdgeDirection_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = LinkDirection.TargetToSource;
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex1SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[1].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithMatchingSimpleCriteriaFromTarget_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            simpleCsaCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { vertices[1].TypeUri },
                LinksFromSearchSet = new SearchAroundStep[] {
                    new SearchAroundStep() { LinkTypeUri = new string[] {edges[0].TypeUri }, TargetObjectTypeUri = new string[] { vertices[0].TypeUri } }
                }
            };
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                Dictionary<string, long[]> docItem = new Dictionary<string, long[]>();
                item.Add("سند", new long[] { Vertex2SupposalID });
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria(docItem, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex2SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithNonMatchingSimpleCriteria_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            simpleCsaCriteria.LinksFromSearchSet[0].LinkTypeUri = new string[] { "Another_Type" };
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithMatchingSimpleCriteriaButNonMatchingAci_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            retrieveMatchAuthParam.permittedGroupNames = new List<string> { "group3" };
            CustomSearchAroundResultIDs[] searchResult;
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OnOneAddedEdgeWithMatchingSimpleCriteriaButNonMatchingClassification_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            retrieveMatchAuthParam.readableClassifications = new List<string> { "C3" };
            CustomSearchAroundResultIDs[] searchResult;
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchingEdgeIncludeMatchingPropertyFilterFromSource_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex1SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[1].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchingEdgeIncludeMatchingPropertyFilterFromSourceWithS2TEdgeDirection_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = LinkDirection.SourceToTarget;
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex1SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[1].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchingEdgeIncludeMatchingPropertyFilterFromSourceWithT2SEdgeDirection_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = LinkDirection.TargetToSource;
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex1SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[1].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchingEdgeIncludeMatchingPropertyFilterFromTarget_MayRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            propertyMatchingCriteria.SourceSetObjectTypes = new string[] { vertices[1].TypeUri };
            propertyMatchingCriteria.LinksFromSearchSet[0].TargetObjectTypeUri = new string[] { vertices[0].TypeUri };
            propertyMatchingCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].PropertyTypeUri = vertices[0].Properties[2].TypeUri;
            (propertyMatchingCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair as StringPropertyCriteriaOperatorValuePair)
                .CriteriaValue = vertices[0].Properties[2].Value;
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                Dictionary<string, long[]> docItem = new Dictionary<string, long[]>();
                item.Add("سند", new long[] { Vertex2SupposalID });
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria(docItem, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(Vertex2SupposalID, searchResult[0].RelationshipNotLoadedResultIDs[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults.Length);
            Assert.AreEqual(edges[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].RelationshipID);
            Assert.AreEqual(vertices[0].ID, searchResult[0].RelationshipNotLoadedResultIDs[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchedEdgeIncludeNonMatchingPropertyTypeFilter_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            propertyMatchingCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].PropertyTypeUri = "label";
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchedEdgeIncludeNonMatchingPropertyValueFilter_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            (propertyMatchingCriteria.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair as StringPropertyCriteriaOperatorValuePair)
                .CriteriaValue = "Another_Value";
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchingEdgeIncludeMatchingPropertyFilterFromSourceWithNotMatchingAci_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            retrieveMatchAuthParam.permittedGroupNames = new List<string> { "group3" };
            CustomSearchAroundResultIDs[] searchResult;
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_OneMatchingEdgeIncludeMatchingPropertyFilterFromSourceWithNotMatchingClassification_MayNotRetrieveEdgeID()
        {
            // Assign
            //  Uses shared "Assign"s
            retrieveMatchAuthParam.readableClassifications = new List<string> { "C3" };
            CustomSearchAroundResultIDs[] searchResult;
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (item, propertyMatchingCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(propertyMatchingCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }
    }
}