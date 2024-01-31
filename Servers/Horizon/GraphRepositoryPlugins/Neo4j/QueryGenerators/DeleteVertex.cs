using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class DeleteVertex
    {
        public string MakeQuery(string idPropertyKey, string vertexType, List<long> ids)
        {
            StringBuilder query = new StringBuilder();
            StringBuilder idsList = new StringBuilder();

            query.Append("MATCH (n:" + vertexType + ") WHERE n." + idPropertyKey + " IN [{0}] DETACH DELETE n;\n");

            foreach (var id in ids)
            {
                idsList.Append($"\"{id}\",");
            }

            idsList.Length--;

            return string.Format(query.ToString(), idsList.ToString());
        }
    }
}
