using GPAS.AccessControl;
using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class GetTransitiveRelatedVertices : SearchAround
    {
        private readonly StringBuilder query;

        public GetTransitiveRelatedVertices(Dictionary<string, long[]> searchedVertices, string[] intermediateVerticesTypeURI,
            long resultLimit, AuthorizationParametters authorizationParametters, string idKey, string classificationKey)
        {
            query = new StringBuilder();
            query.Append(PrepareFirstPartOfQuery(searchedVertices, idKey));
            query.AppendLine(PrepareSecoundPartOfQuery(intermediateVerticesTypeURI, authorizationParametters, classificationKey));
            query.AppendLine("RETURN DISTINCT s2." + idKey + " as sID, r1." + idKey + " as r1ID, r2." + idKey +
                " as r2ID, t." + idKey + " as tID LIMIT " + resultLimit + "");
        }

        public string GetQuery()
        {
            return query.ToString();
        }

        private string PrepareSecoundPartOfQuery(string[] typeUris, AuthorizationParametters authorizationParametters, 
            string classificationKey)
        {
            string relationClassificationsString = FillSearchedVerticesTypeURI(authorizationParametters.readableClassifications.ToArray());
            string r1PermissionPart = FillRelationGroupNames(authorizationParametters.permittedGroupNames, "r1");
            string r2PermissionPart = FillRelationGroupNames(authorizationParametters.permittedGroupNames, "r2");

            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("\nCALL{\nWITH s");

            foreach (var item in typeUris)
            {
                partOfQuery.AppendFormat("\nMATCH (s)-[r1]-(e:" + item + ")-[r2]-(t) WHERE r1." + classificationKey + " IN [{0}] " +
                    r1PermissionPart + "AND r2." + classificationKey + " IN [{1}] " + r2PermissionPart,
                    relationClassificationsString, relationClassificationsString);
                partOfQuery.Append("RETURN e, s as s2, r1, r2, t \nUNION WITH s");
            }

            partOfQuery.Length -= "UNION WITH s".Length;
            partOfQuery.Append("}");
            return partOfQuery.ToString();
        }
    }
}
