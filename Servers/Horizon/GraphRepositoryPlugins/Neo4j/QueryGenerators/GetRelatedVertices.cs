using GPAS.AccessControl;
using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class GetRelatedVertices : SearchAround
    {
        private readonly StringBuilder query;

        public GetRelatedVertices(Dictionary<string, long[]> searchedVertices, string[] destinationVerticesTypeURI, long resultLimit,
            AuthorizationParametters authorizationParametters, string idKey, string classificationKey)
        {
            query = new StringBuilder();
            query.Append(PrepareFirstPartOfQuery(searchedVertices, idKey));
            query.Append("\n");
            query.Append(PrepareSecoundPartOfQuery(destinationVerticesTypeURI, classificationKey, authorizationParametters));
            query.AppendLine("\nRETURN DISTINCT s." + idKey + " as sID, r." + idKey + " as rID, t." + idKey + " as tID LIMIT " + resultLimit + "");
        }

        public string GetQuery()
        {
            return query.ToString();
        }

        protected string PrepareSecoundPartOfQuery(string[] typeUris, string classificationKey, AuthorizationParametters authorizationParametters)
        {
            string relationClassificationsString = FillSearchedVerticesTypeURI(authorizationParametters.readableClassifications.ToArray());
            string rPermissionPart = FillRelationGroupNames(authorizationParametters.permittedGroupNames, "r");

            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("CALL{\nWITH s");

            foreach (var typeUri in typeUris)
            {
                partOfQuery.Append("\nMATCH (s)-[r]-(t:" + typeUri + ") ");
                partOfQuery.AppendFormat("WHERE r." + classificationKey + " IN [{0}] ", relationClassificationsString);
                partOfQuery.Append(rPermissionPart + "RETURN t, r\nUNION WITH s");
            }

            partOfQuery.Length -= "UNION WITH s".Length;
            partOfQuery.Append("}\nWITH s, t, r");
            return partOfQuery.ToString();
        }
    }
}
