using System;

namespace GPAS.Horizon.GraphRepositoryPlugins.OrientNet
{
    internal class CreateEdge
    {
        private string query;

        /// <summary></summary>
        /// <param name="sourceVertexOrid">Orient ID of source vertex with format #n:n</param>
        /// <param name="targetVertexOrid">Orient ID of target vertex with format #n:n</param>
        internal CreateEdge(string className, string sourceVertexOrid, string targetVertexOrid)
        {
            query = string.Format("CREATE EDGE {0} FROM {1} TO {2} SET ", className, sourceVertexOrid, targetVertexOrid);
        }

        /// <summary></summary>
        /// <param name="className"></param>
        /// <param name="sourceVertexOrid">Orient ID of source vertex with format #n:n</param>
        /// <param name="targetVertexId">ID of target vertex; This may 'LET' to a property with name convension: v{id}</param>
        internal CreateEdge(string className, string sourceVertexOrid, long targetVertexId)
        {
            query = string.Format("CREATE EDGE {0} FROM {1} TO $v{2} SET ", className, sourceVertexOrid, targetVertexId.ToString());
        }


        /// <summary></summary>
        /// <param name="sourceVertexId">ID of source vertex; This may 'LET' to a property with name convension: v{id}</param>
        /// <param name="targetVertexOrid">Orient ID of target vertex with format #n:n</param>
        internal CreateEdge(string className, long sourceVertexId, string targetVertexOrid)
        {
            query = string.Format("CREATE EDGE {0} FROM $v{1} TO {2} SET ", className, sourceVertexId, targetVertexOrid);
        }

        /// <summary></summary>
        /// <param name="sourceVertexId">ID of source vertex; This may 'LET' to a property with name convension: v{id}</param>
        /// <param name="targetVertexOrid">ID of target vertex; This may 'LET' to a property with name convension: v{id}</param>
        internal CreateEdge(string className, long sourceVertexId, long targetVertexId)
        {
            query = string.Format("CREATE EDGE {0} FROM $v{1} TO $v{2} SET ", className, sourceVertexId, targetVertexId);
        }


        /// <summary></summary>
        /// <param name="sourceVertexId">ID of source vertex</param>
        /// <param name="targetVertexId">ID of target vertex</param>
        internal CreateEdge(string edgeClassName, string sourceVertexId, string sourceVertexClassName, string targetVertexId, string targetVertexClassName)
        {
            query = string.Format("CREATE EDGE {0} FROM (SELECT FROM {1} WHERE ID = '{2}') TO (SELECT FROM {3} WHERE ID = '{4}') SET "
                , edgeClassName, sourceVertexClassName, sourceVertexId, targetVertexClassName, targetVertexId);
        }


        // TODO: Support for multivalue fields!
        // TODO: Use StringBuilder to prepair the query instead of string.concat
        internal void AddField(string fieldName, string fieldValue)
        {
            query += string.Format("{0} = '{1}', ", fieldName, fieldValue);
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
            else
            {
                throw new InvalidOperationException();
            }
        }

        internal string GetQueryText()
        {
            return GetFinalizedQuery();
        }
    }
}
