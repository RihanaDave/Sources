using System;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class BatchQueryGenerator : IDisposable
    {
        private StringBuilder queries;
        private bool hasQuery;

        public BatchQueryGenerator()
        {
            Init();
        }

        private void Init()
        {
            hasQuery = false;
            queries = new StringBuilder();
            queries.Append("CALL apoc.cypher.runMany(' \n");
        }

        public void Reset()
        {
            Init();
        }

        public string MakeCreateVerticesQuery(string body)
        {
            body = body.Trim();
            body = body.Remove(body.Length - 1);
            StringBuilder query = new StringBuilder();
            query.Append("CREATE ");
            query.AppendLine(body);

            return query.ToString();
        }

        public string MakeCreateIndexQuery(string typeUri, string idKey)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine($"CREATE INDEX " +
                   $"{Serialization.GetVertexTypeUriIndexName(typeUri)} IF NOT EXISTS FOR " +
                   $"(n:{typeUri}) ON (n.{idKey})");

            return query.ToString();
        }

        public string MakeCreateEdgesQuery(string body)
        {
            body = body.Trim();
            StringBuilder query = new StringBuilder();
            query.Append("CALL apoc.cypher.runMany(' ");
            query.AppendLine(body);
            query.Append("', {statistics:false});");

            return query.ToString();
        }

        public void AddUpdateEdgesQuery(string body)
        {
            AddQuery(body);
        }

        public void AddUpdatePropertiesQuery(string body)
        {
            AddQuery(body);
        }

        public void AddDeleteVerticesQuery(string body)
        {
            AddQuery(body);
        }

        public string GetFinalQuery()
        {
            if (hasQuery)
            {
                queries.Append("', {statistics:false});");
                return queries.ToString();
            }

            return string.Empty;           
        }

        public void Dispose()
        {
            queries = new StringBuilder();
        }

        private void AddQuery(string body)
        {
            if (!string.IsNullOrEmpty(body))
            {
                body = body.Trim();
                queries.Append(body);
                hasQuery = true;
            }
        }
    }
}
