using GPAS.AccessControl;
using GPAS.SearchAround;
using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class GetTransitiveRelatedVerticesWithCustomCriteria : SearchAround
    {
        private readonly StringBuilder query;

        public GetTransitiveRelatedVerticesWithCustomCriteria(Dictionary<string, long[]> searchedVertices, SearchAroundStep searchAroundStep,
            long resultLimit, AuthorizationParametters authorizationParametters, string idKey, string classificationKey)
        {
            string defaultRelationTypeUri = Serialization.Ontology.GetDefaultRelationshipTypeForEventBasedLink("", "", "");

            query = new StringBuilder();
            query.Append(PrepareFirstPartOfQuery(searchedVertices, idKey));
            query.AppendLine(PrepareSecoundPartOfQuery(searchAroundStep.LinkTypeUri, authorizationParametters, searchAroundStep,
                classificationKey, defaultRelationTypeUri));
            query.AppendLine(PrepareThirdPartOfQuery(searchAroundStep, authorizationParametters, defaultRelationTypeUri, classificationKey));
            query.AppendLine("RETURN DISTINCT s3." + idKey + " as sID, r11." + idKey + " as r1ID, r22." + idKey +
                " as r2ID, t." + idKey + " as tID LIMIT " + resultLimit + "");
        }

        public string GetQuery()
        {
            return query.ToString();
        }

        private string PrepareSecoundPartOfQuery(string[] typeUris, AuthorizationParametters authorizationParametters,
            SearchAroundStep searchAroundStep, string classificationKey, string defaultRelationTypeUri)
        {
            string eventProperties = FillVertexProperties(searchAroundStep.EventObjectPropertyCriterias, "e");
            string relationClassificationsString = FillSearchedVerticesTypeURI(authorizationParametters.readableClassifications.ToArray());
            string permissionPart = FillRelationGroupNames(authorizationParametters.permittedGroupNames, "r1");

            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("\nCALL{\nWITH s");

            foreach (var item in typeUris)
            {
                partOfQuery.AppendFormat("\nMATCH (s)-[r1:" + defaultRelationTypeUri + "]-(e:" + item + ") WHERE r1." +
                    classificationKey + " IN [{0}] " + permissionPart, relationClassificationsString);

                if (!string.IsNullOrEmpty(eventProperties))
                {
                    partOfQuery.Append("AND ");
                    partOfQuery.Append(eventProperties);
                }                    

                partOfQuery.Append("RETURN e, s as s2, r1 \nUNION WITH s");
            }

            partOfQuery.Length -= "UNION WITH s".Length;
            partOfQuery.Append("}");
            return partOfQuery.ToString();
        }

        private string PrepareThirdPartOfQuery(SearchAroundStep searchAroundStep, AuthorizationParametters authorizationParametters,
            string defaultRelationTypeUri, string classificationKey)
        {
            string targetProperties = FillVertexProperties(searchAroundStep.TargetObjectPropertyCriterias, "t");
            string relationClassificationsString = FillSearchedVerticesTypeURI(authorizationParametters.readableClassifications.ToArray());
            string permissionPart = FillRelationGroupNames(authorizationParametters.permittedGroupNames, "r2");

            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("CALL{\nWITH r1, s2, e");

            foreach (var item in searchAroundStep.TargetObjectTypeUri)
            {
                partOfQuery.AppendFormat("\nMATCH (s2)-[r1]-(e)-[r2:" + defaultRelationTypeUri + "]-(t:" + item + ") WHERE r2." +
                    classificationKey + " IN [{0}] " + permissionPart, relationClassificationsString);

                if (!string.IsNullOrEmpty(targetProperties))
                {
                    partOfQuery.Append("AND ");
                    partOfQuery.Append(targetProperties);
                }                    

                partOfQuery.Append("RETURN s2 as s3, r1 as r11, r2 as r22, t\nUNION WITH r1, s2, e");
            }

            partOfQuery.Length -= "UNION WITH r1, s2, e".Length;
            partOfQuery.Append("}");
            return partOfQuery.ToString();
        }
    }
}
