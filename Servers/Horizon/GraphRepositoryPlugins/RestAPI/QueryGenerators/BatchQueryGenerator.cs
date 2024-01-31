using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.RestAPI
{
    internal class BatchQueryGenerator : IDisposable
    {
        private List<string> queries;

        public BatchQueryGenerator()
        {
            this.queries = new List<string>(1000);
        }

        public BatchQueryGenerator(List<string> queries)
        {
            this.queries = queries;
        }
        
        internal void AddQuery(string query)
        {
            queries.Add(query);
        }
        
        internal void AddQueryRange(IEnumerable<string> newQueries)
        {
            queries.AddRange(newQueries);
        }

        private string GetFinalizedQuery()
        {
            if (queries.Count == 0)
                throw new InvalidOperationException();
            
            StringBuilder builder = new StringBuilder("BEGIN\n", 12 + queries.Sum(s => s.Length) + queries.Count);
            foreach (string innerQuery in queries)
            {
                builder.AppendFormat("{0}\n", innerQuery);
            }
            builder.Append("COMMIT");
            return builder.ToString();
        }

        internal string GetQueryText()
        {
            return GetFinalizedQuery();
        }

        public void Dispose()
        {
            queries.Clear();
        }
    }
}
