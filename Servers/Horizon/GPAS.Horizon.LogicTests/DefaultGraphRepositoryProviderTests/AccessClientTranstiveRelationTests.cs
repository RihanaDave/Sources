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
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Horizon.LogicTests.DefaultGraphRepositoryProviderTests
{
    [TestClass()]
    public class AccessClientTranstiveRelationTests
    {
        #region Shared definitions
        private IAccessClient accessClient = null;
        const long SourceVertexSupposalID = 10000000368;
        const long IntermediaryVertexSupposalID = 10000000372;
        const long TargetVertexSupposalID = 10000000393;
        List<Vertex> vertices;
        ACL supposalAcl;
        List<Edge> edges;
        AuthorizationParametters retrieveMatchAuthParam;
        CustomSearchAroundCriteria simpleCsaCriteria;
        CustomSearchAroundCriteria simpleCsaCriteriaWithPropertyFilter;
        Dictionary<string, long[]> SourceVertex;
        Dictionary<string, long[]> TargetVertex;
        #endregion

        #region Tests Start-up / Clean-up
        [TestInitialize]
        public void TestInit()
        {
            // Shared "Assign"s
            List<string> supposalGroups = new List<string> { "group1", "group2" };

            ResetAccessClient();

            accessClient.AddNewGroupPropertiesToEdgeClass(supposalGroups);

            SourceVertex = new Dictionary<string, long[]>();
            SourceVertex.Add("شخص", new long[] { SourceVertexSupposalID });

            TargetVertex = new Dictionary<string, long[]>();
            TargetVertex.Add("سند", new long[] { TargetVertexSupposalID });

            vertices = new List<Vertex>() {
                new Vertex() { ID = SourceVertexSupposalID, TypeUri = "شخص", Properties = new List<VertexProperty>() {
                        new VertexProperty() { OwnerVertexID = SourceVertexSupposalID, OwnerVertexTypeURI = "شخص", TypeUri = "قد", Value = "0.0"},
                        new VertexProperty() { OwnerVertexID = SourceVertexSupposalID, OwnerVertexTypeURI = "شخص", TypeUri = "سن", Value = "0"},
                        new VertexProperty() { OwnerVertexID = SourceVertexSupposalID, OwnerVertexTypeURI = "شخص", TypeUri = "نام", Value = "فرهاد"}}},
                new Vertex() { ID = IntermediaryVertexSupposalID, TypeUri = "رخداد", Properties = new List<VertexProperty>() {
                        new VertexProperty() { OwnerVertexID = IntermediaryVertexSupposalID, OwnerVertexTypeURI = "رخداد", TypeUri = "label", Value = "رخداد مهم!"}}},
                new Vertex() { ID = TargetVertexSupposalID, TypeUri = "سند", Properties = new List<VertexProperty>() {
                        new VertexProperty() { OwnerVertexID = TargetVertexSupposalID, OwnerVertexTypeURI = "سند", TypeUri = "منبع", Value = "100.0"},
                        new VertexProperty() { OwnerVertexID = TargetVertexSupposalID, OwnerVertexTypeURI = "سند", TypeUri = "منبع", Value = "0"},
                        new VertexProperty() { OwnerVertexID = TargetVertexSupposalID, OwnerVertexTypeURI = "سند", TypeUri = "نام", Value = "پایان نامه"}}}
            };
            supposalAcl = new ACL()
            {
                Permissions = new List<ACI>() {
                    new ACI() { GroupName = supposalGroups[0], AccessLevel = Permission.Read }},
                Classification = "C1"
            };
            edges = new List<Edge>() {
                new Edge() { TypeUri = "حضور_در", ID = 10000004001, Direction = LinkDirection.SourceToTarget,
                    SourceVertexID = SourceVertexSupposalID, TargetVertexID = IntermediaryVertexSupposalID,
                    SourceVertexTypeUri ="شخص", TargetVertexTypeUri = "رخداد", Acl = supposalAcl },

                new Edge() { TypeUri = "حضور_در", ID = 10000004002, Direction = LinkDirection.SourceToTarget,
                    SourceVertexID = IntermediaryVertexSupposalID, TargetVertexID = TargetVertexSupposalID,
                    SourceVertexTypeUri ="رخداد", TargetVertexTypeUri = "سند", Acl = supposalAcl }
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
                    new SearchAroundStep() { LinkTypeUri = new string[] {vertices[1].TypeUri }, TargetObjectTypeUri = new string[] { vertices[2].TypeUri } }
                }
            };
            simpleCsaCriteriaWithPropertyFilter = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = new string[] { vertices[0].TypeUri },
                LinksFromSearchSet = new SearchAroundStep[] {
                    new SearchAroundStep() {
                        LinkTypeUri = new string[] {vertices[1].TypeUri },
                        TargetObjectTypeUri = new string[] { vertices[2].TypeUri },
                        TargetObjectPropertyCriterias = new PropertyValueCriteria[] {
                            new PropertyValueCriteria() {
                                PropertyTypeUri = vertices[2].Properties[2].TypeUri,
                                OperatorValuePair = new StringPropertyCriteriaOperatorValuePair() {
                                    CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                    CriteriaValue = vertices[2].Properties[2].Value
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

        [TestMethod()]
        public void AddThreeVerticesAndTwoEdges_AndFinallyCleanThemUp_NoExceptionMayThrown()
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
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationAndSearchOnSourceVertexWithIntermediaryVertexType_MayRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (SourceVertex, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, retrievedTransitiveResults.Count);
            Assert.AreEqual(SourceVertexSupposalID, retrievedTransitiveResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedTransitiveResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(TargetVertexSupposalID, retrievedTransitiveResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationInTwoApplyChanges_MayRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(new List<Edge>() { edges[0] });
                accessClient.ApplyChanges();
                ResetAccessClient();
                accessClient.AddEdge(new List<Edge>() { edges[1] });
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (SourceVertex, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, retrievedTransitiveResults.Count);
            Assert.AreEqual(SourceVertexSupposalID, retrievedTransitiveResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedTransitiveResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(TargetVertexSupposalID, retrievedTransitiveResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationWithBiDirectionalEdges_MayRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = edges[1].Direction = LinkDirection.Bidirectional;
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (SourceVertex, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, retrievedTransitiveResults.Count);
            Assert.AreEqual(SourceVertexSupposalID, retrievedTransitiveResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedTransitiveResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(TargetVertexSupposalID, retrievedTransitiveResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationAndSearchOnTargetVertexWithIntermediaryVertexType_MayRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;

            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (TargetVertex, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, retrievedTransitiveResults.Count);
            Assert.AreEqual(TargetVertexSupposalID, retrievedTransitiveResults[0].SearchedObjectID);
            Assert.AreEqual(1, retrievedTransitiveResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == retrievedTransitiveResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(SourceVertexSupposalID, retrievedTransitiveResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationAndSearchOnNotMatchedIntermediaryVertexType_MayNotRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;

            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (SourceVertex, new string[] { "Another_Type" }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(0, retrievedTransitiveResults.Count);
        }

        [TestMethod()]
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationWithNotMatchAciOnOneEdge_MayNotRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[1].Acl.Permissions[0].GroupName = "group3";
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(new List<Edge>() { edges[0] });
                accessClient.ApplyChanges();
                ResetAccessClient();
                accessClient.AddEdge(new List<Edge>() { edges[1] });
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (SourceVertex, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(0, retrievedTransitiveResults.Count);
        }

        [TestMethod()]
        public void RetrieveTransitiveRelatedVertices_ForAddedTransitiveRelationWithNotMatchClassificationOnOneEdge_MayNotRetrieveEdgeIDs()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[1].Acl.Classification = "C3";
            List<EventBasedResultsPerSearchedObjects> retrievedTransitiveResults;
            //Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(new List<Edge>() { edges[0] });
                accessClient.ApplyChanges();
                ResetAccessClient();
                accessClient.AddEdge(new List<Edge>() { edges[1] });
                accessClient.ApplyChanges();
                retrievedTransitiveResults = accessClient.RetrieveTransitiveRelatedVertices
                    (SourceVertex, new string[] { vertices[1].TypeUri }, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(0, retrievedTransitiveResults.Count);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchOnSourceVertesWithSimpleTransitiveCsaCriteria_MayRetrieveTheRelation()
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
                    (SourceVertex, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(SourceVertexSupposalID, searchResult[0].EventBasedNotLoadedResults[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(vertices[2].ID, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchOnTargetVertesWithSimpleTransitiveCsaCriteria_MayRetrieveTheRelation()
        {
            // Assign
            //  Uses shared "Assign"s
            simpleCsaCriteria.LinksFromSearchSet[0].TargetObjectTypeUri = new string[] { vertices[0].TypeUri };
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (TargetVertex, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(TargetVertexSupposalID, searchResult[0].EventBasedNotLoadedResults[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(vertices[0].ID, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndBidirectionalEdgesWithSimpleTransitiveCsaCriteria_MayRetrieveTheRelation()
        {
            // Assign
            //  Uses shared "Assign"s
            edges[0].Direction = edges[1].Direction = LinkDirection.Bidirectional;
            CustomSearchAroundResultIDs[] searchResult;
            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (SourceVertex, simpleCsaCriteria, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }
            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteria.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(SourceVertexSupposalID, searchResult[0].EventBasedNotLoadedResults[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(vertices[2].ID, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchWithNotMatchSimpleTransitiveCsaCriteria_MayNotRetrieveTheRelation()
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
                    (SourceVertex, simpleCsaCriteria, 100, retrieveMatchAuthParam);
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
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchWithNotMatchSimpleTransitiveCsaCriteriaButNonMatchingAci_MayNotRetrieveTheRelation()
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
                    (SourceVertex, simpleCsaCriteria, 100, retrieveMatchAuthParam);
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
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchWithNotMatchSimpleTransitiveCsaCriteriaButNonMatchingClassification_MayNotRetrieveTheRelation()
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
                    (SourceVertex, simpleCsaCriteria, 100, retrieveMatchAuthParam);
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
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchOnSourceVertesWithSimplePropertyIncludedCsaCriteria_MayRetrieveTheRelation()
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
                    (SourceVertex, simpleCsaCriteriaWithPropertyFilter, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(SourceVertexSupposalID, searchResult[0].EventBasedNotLoadedResults[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(vertices[2].ID, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchOnTargetVertesWithSimplePropertyIncludedCsaCriteria_MayRetrieveTheRelation()
        {
            // Assign
            //  Uses shared "Assign"s
            simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].TargetObjectTypeUri = new string[] { vertices[0].TypeUri };
            simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].PropertyTypeUri = vertices[0].Properties[2].TypeUri;
            (simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair as StringPropertyCriteriaOperatorValuePair)
                .CriteriaValue = vertices[0].Properties[2].Value;
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (TargetVertex, simpleCsaCriteriaWithPropertyFilter, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
            Assert.AreEqual(TargetVertexSupposalID, searchResult[0].EventBasedNotLoadedResults[0].SearchedObjectID);
            Assert.AreEqual(1, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults.Length);
            Assert.IsTrue((edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[0].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.IsTrue((edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].FirstRealationshipID)
                       || (edges[1].ID == searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].SecondRealationshipID));
            Assert.AreEqual(vertices[0].ID, searchResult[0].EventBasedNotLoadedResults[0].NotLoadedResults[0].TargetObjectID);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchWithCsaCriteriaIncludingNotMatchedPropertyValueFilter_MayNotRetrieveTheRelation()
        {
            // Assign
            //  Uses shared "Assign"s
            (simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].OperatorValuePair as StringPropertyCriteriaOperatorValuePair)
                .CriteriaValue = "Another_Value";
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (SourceVertex, simpleCsaCriteriaWithPropertyFilter, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }

        [TestMethod()]
        public void PerformCustomSearchAround_ForAddedTransitiveRelationAndSearchWithCsaCriteriaIncludingNotMatchedPropertyTypeFilter_MayNotRetrieveTheRelation()
        {
            // Assign
            //  Uses shared "Assign"s
            simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].TargetObjectPropertyCriterias[0].PropertyTypeUri = "label";
            CustomSearchAroundResultIDs[] searchResult;

            // Act
            try
            {
                accessClient.AddVertices(vertices);
                accessClient.AddEdge(edges);
                accessClient.ApplyChanges();
                searchResult = accessClient.RetrieveRelatedVerticesWithCustomCriteria
                    (SourceVertex, simpleCsaCriteriaWithPropertyFilter, 100, retrieveMatchAuthParam);
            }
            finally
            {
                // Clean-up
                DeleteVetices(vertices);
            }

            // Assert
            Assert.AreEqual(1, searchResult.Length);
            Assert.AreEqual(simpleCsaCriteriaWithPropertyFilter.LinksFromSearchSet[0].GUID, searchResult[0].SearchAroundStepGuid);
            Assert.AreEqual(0, searchResult[0].EventBasedNotLoadedResults.Length);
            Assert.AreEqual(0, searchResult[0].RelationshipNotLoadedResultIDs.Length);
        }
    }
}