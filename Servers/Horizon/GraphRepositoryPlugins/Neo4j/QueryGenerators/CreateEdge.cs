using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class CreateEdge
    {
        private readonly StringBuilder query;
        private readonly StringBuilder propertiese;

        public CreateEdge(string typeUri, string sourceTypeUri, string targetTypeUri, string idPropertyKey, long sourceVertexId, 
            long targetVertexId)
        {
            query = new StringBuilder();
            propertiese = new StringBuilder();

            query.Append($"MATCH (a:{sourceTypeUri}),(b:{targetTypeUri}) WHERE a.{idPropertyKey} = \"{sourceVertexId}\" " +
                $"AND b.{idPropertyKey} = \"{targetVertexId}\" ");
            query.AppendLine("CREATE (a)-[:" + typeUri + " {0}]->(b);");
            query.AppendLine("\n");
            propertiese.Append("{");
        }

        public void AddProperty(string name, string value)
        {
            propertiese.Append($"{name}:\"{value}\",");
        }

        public string GetQuery()
        {
            propertiese.Length--;
            propertiese.Append("}");
            return string.Format(query.ToString(), propertiese.ToString());
        }
    }
}
