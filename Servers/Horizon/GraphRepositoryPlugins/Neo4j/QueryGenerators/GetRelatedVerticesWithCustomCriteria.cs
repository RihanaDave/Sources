using GPAS.AccessControl;
using GPAS.FilterSearch;
using GPAS.SearchAround;
using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class GetRelatedVerticesWithCustomCriteria : SearchAround
    {
        private readonly StringBuilder query;

        public GetRelatedVerticesWithCustomCriteria(Dictionary<string, long[]> searchedVertices, SearchAroundStep searchAroundStep,
            long resultLimit, AuthorizationParametters authorizationParametters, string idKey, string classificationKey)
        {
            query = new StringBuilder();
            query.Append(PrepareFirstPartOfQuery(searchedVertices, idKey));
            query.AppendLine(PrepareSecoundPartOfQuery(searchAroundStep.LinkTypeUri, authorizationParametters, classificationKey));
            query.AppendLine(PrepareThirdPartOfQuery(searchAroundStep.TargetObjectTypeUri, searchAroundStep));
            query.AppendLine("RETURN DISTINCT s3." + idKey + " as sID, r2." + idKey + " as rID, t." + idKey + " as tID LIMIT " + resultLimit + "");
        }

        public string GetQuery()
        {
            return query.ToString();
        }

        private string PrepareSecoundPartOfQuery(string[] typeUris, AuthorizationParametters authorizationParametters,
            string classificationKey)
        {
            string relationClassificationsString = FillSearchedVerticesTypeURI(authorizationParametters.readableClassifications.ToArray());
            string permissionPart = FillRelationGroupNames(authorizationParametters.permittedGroupNames, "r");

            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("\nCALL{\nWITH s");

            foreach (var item in typeUris)
            {
                partOfQuery.AppendFormat("\nMATCH (s)-[r:" + item + "]-(t) WHERE r." + classificationKey + " IN [{0}] " + permissionPart,
                    relationClassificationsString);

                partOfQuery.Append("RETURN r, s as s2 \nUNION WITH s");
            }

            partOfQuery.Length -= "UNION WITH s".Length;
            partOfQuery.Append("}");
            return partOfQuery.ToString();
        }

        private string PrepareThirdPartOfQuery(string[] typeUris, SearchAroundStep searchAroundStep)
        {
            string targetProperties = FillVertexProperties(searchAroundStep.TargetObjectPropertyCriterias, "t");

            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("CALL{\nWITH r, s2");

            foreach (var item in typeUris)
            {
                partOfQuery.Append("\nMATCH (s2)-[r]-(t:" + item + ") ");

                if (!string.IsNullOrEmpty(targetProperties))
                    partOfQuery.Append("WHERE " + targetProperties);

                partOfQuery.Append("RETURN s2 as s3, r as r2, t\nUNION WITH r, s2");
            }

            partOfQuery.Length -= "UNION WITH r, s2".Length;
            partOfQuery.Append("}");
            return partOfQuery.ToString();
        }
    }
}
