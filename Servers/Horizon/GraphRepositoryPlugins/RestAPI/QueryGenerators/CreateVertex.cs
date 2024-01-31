using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.RestAPI
{
    internal class CreateVertex
    {
        private string query;

        /// <summary>
        /// Creating Vertex without 'LET' to any variable
        /// </summary>
        /// <param name="className"></param>
        internal CreateVertex(string className)
        {
            query = string.Format("CREATE VERTEX {0} SET ", className);
        }
        /// <summary>
        /// This may 'LET' the creation result to an variable; Variable name convention contract: v{id}
        /// </summary>
        internal CreateVertex(string className, long vertexIdToLetVariable)
        {
            query = string.Format("LET v{0} = CREATE VERTEX {1} SET ", vertexIdToLetVariable, className);
        }

        // TODO: Add support for multivalue fields!
        // TODO: Use StringBuilder to prepair the query instead of string.concat
        internal void AddField(string fieldName, string fieldValue)
        {
            query += string.Format("{0} = '{1}', ", fieldName, fieldValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="values"></param>
        internal void AddMultiValueField(string fieldName, IEnumerable<string> values)
        {
            query += string.Format("{0} = [", fieldName);
            foreach (var fieldValue in values)
            {
                query += string.Format("'{0}', ", fieldValue);
            }
            query = query.Substring(0, query.Length - 2);
            query += "], ";
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
