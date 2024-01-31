using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.RestAPI
{
    internal class DeleteVertex
    {
        private string query;

        internal DeleteVertex()
        {
            query = string.Format("");
        }

        // TODO: Use StringBuilder to prepair the query instead of string.concat
        internal void DeleteVertexByID(string vertexClass,long vertexID)
        {
            query += string.Format("DELETE VERTEX {0} WHERE ID = {1}\n", vertexClass, vertexID);
        }

        internal void DeleteVertexByRID(string vertexClass, string vertexRID)
        {
            query += string.Format("DELETE VERTEX {0} WHERE @rid = {1}\n", vertexClass, vertexRID);
        }

        private string GetFinalizedQuery()
        {
            if (query.EndsWith(", "))
            {
                return query.Substring(0, query.Length - 2);
            }
            else if (query.EndsWith("SET "))
            {
                return query.Substring(0, query.Length - 5);
            }
            else if (query.EndsWith("\n"))
            {
                return query.Substring(0, query.Length - 1);
            }
            else
            {
                return query;
            }
        }

        internal string GetQueryText()
        {
            return GetFinalizedQuery();
        }
    }
}
