using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Graph;
using GPAS.Horizon.Logic;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GPAS.Horizon.GraphRepository
{
    public interface IAccessClient
    {
        #region Database & Schema Management
        void Init(OntologyMaterial ontologyMaterial);
        void AddNewGroupPropertiesToEdgeClass(List<string> newGroupsName);
        void DropDatabase();
        void TruncateDatabase();
        #endregion

        #region Pend changes
        void AddVertices(List<Vertex> vertices);

        void AddVertexProperties(List<VertexProperty> vertexProperies);

        void UpsertVertices(List<Vertex> vertices);

        List<VertexProperty> GetVertexPropertiesUnion(List<long> ownerVertexIDs);

        void UpdateEdge(List<long> verticesID, long masterVertexID, string typeUri);

        void DeleteVertices(string typerUri, List<long> vertices);

        void AddEdge(List<Edge> edges);
        #endregion

        #region Apply pending changes
        void OpenConnetion();
        void ApplyChanges();
        #endregion

        #region Searches
        List<RelationshipBasedResultsPerSearchedObjects> RetrieveRelatedVertices(Dictionary<string, long[]> searchedVertices, string[] destinationVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters);

        List<EventBasedResultsPerSearchedObjects> RetrieveTransitiveRelatedVertices(Dictionary<string, long[]> searchedVertices, string[] intermediateVerticesTypeURI, long resultLimit, AuthorizationParametters authorizationParametters);

        CustomSearchAroundResultIDs[] RetrieveRelatedVerticesWithCustomCriteria(Dictionary<string, long[]> searchedVertices, SearchAround.CustomSearchAroundCriteria criteria, long resultLimit, AuthorizationParametters authorizationParametters);

        object RetrieveObjectWithId(long objectId);

        #endregion

        #region Index

        List<IndexModel> GetAllIndexes();

        void CreateIndex(IndexModel index);

        void EditIndex(IndexModel oldIndex, IndexModel newIndex);

        void DeleteIndex(IndexModel index);

        void DeleteAllIndexes();

        #endregion
    }
}
