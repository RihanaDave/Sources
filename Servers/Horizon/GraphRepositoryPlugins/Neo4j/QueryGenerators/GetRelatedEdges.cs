using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class GetRelatedEdges
    {
        private readonly StringBuilder query;

        public GetRelatedEdges(long[] searchedVerticesID, string idKey, string alias)
        {
            query = new StringBuilder();
            query.Append("MATCH " + alias + "=(s)-[]-() ");

            string searchedVerticesIdString = FillSearchedVerticesID(searchedVerticesID);
            query.AppendFormat("WHERE s." + idKey + " IN [{0}] ", searchedVerticesIdString);
            query.AppendLine("RETURN DISTINCT " + alias);
        }

        public string GetQuery()
        {
            return query.ToString();
        }

        private string FillSearchedVerticesID(long[] searchedVerticesID)
        {
            StringBuilder propertieseValueList = new StringBuilder();

            foreach (var id in searchedVerticesID)
            {
                propertieseValueList.Append($"\"{id}\",");
            }

            propertieseValueList.Length--;
            return propertieseValueList.ToString();
        }
    }
}
