using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.RestAPI
{
    internal class UpdateProperty
    {
        private string query;
        private string id;
        /// <summary>
        /// Creating Vertex without 'LET' to any variable
        /// </summary>
        /// <param name="className"></param>
        internal UpdateProperty(string className, long id)
        {
            query = string.Format("UPDATE {0} SET ", className);
            this.id = id.ToString();
        }

        // TODO: Use StringBuilder to prepair the query instead of string.concat
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
                query = query.Substring(0, query.Length - 2);
                query += string.Format(" where ID = {0}", id);
                return query;
            }
            else if (query.EndsWith("SET "))
            {
                query = query.Substring(0, query.Length - 5);
                query += string.Format(" where ID = {0}", id);
                return query;
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
