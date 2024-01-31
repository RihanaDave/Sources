using GPAS.Horizon.Entities;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class IndexQueries
    {
        public string GetAllIndexesQuery()
        {
            return "SHOW INDEXES";
        }

        public string CreateIndexQuery(IndexModel index)
        {
            return $"CREATE INDEX {MakeIndexName(index)} IF NOT EXISTS FOR " +
                   $"(n:{index.NodeType}) ON ({MakeIndexItem(index.PropertiesType)})";
        }

        public string DeleteIndexQuery(IndexModel index)
        {
            return DeleteIndexQuery(MakeIndexName(index));
        }

        public string DeleteIndexQuery(string indexName)
        {
            return $"DROP INDEX {indexName} IF EXISTS";
        }

        private string MakeIndexName(IndexModel index)
        {
            StringBuilder indexName = new StringBuilder();
            indexName.Append(index.NodeType);
            indexName.Append("xandx");

            foreach (string property in index.PropertiesType)
            {
                indexName.Append(property);
                indexName.Append("xandx");
            }

            return indexName.Remove(indexName.Length - 5, 5).ToString();
        }

        private string MakeIndexItem(string[] items)
        {
            StringBuilder indexItems = new StringBuilder();
            foreach (string item in items)
            {
                indexItems.AppendFormat("n.{0}, ", item);
            }

            return indexItems.Remove(indexItems.Length - 2, 2).ToString();
        }
    }
}
