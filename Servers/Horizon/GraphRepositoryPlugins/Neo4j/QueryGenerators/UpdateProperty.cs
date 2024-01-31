using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class UpdateProperty
    {
        private readonly StringBuilder query;
        private readonly StringBuilder Propertiese;

        internal UpdateProperty(string typUri, string idKey, long id)
        {
            query = new StringBuilder();
            Propertiese = new StringBuilder();
            query.Append("MATCH (n:" + typUri + " {" + idKey + ":\"" + id + "\"}) ");
        }

        public void AddMultiValueProperty(string name, IEnumerable<string> values)
        {
            Propertiese.Append($"{name}: \"{string.Join(",", values)}\",");
        }

        public void AddProperty(string name, string value)
        {
            Propertiese.Append($"{name}: {value},");
        }

        public void AddGeoProperty(string name, GeoPointModel value)
        {
            Propertiese.Append($"{name}: point({{latitude:{value.Latitude}, longitude:{value.Longitude}}}),");
        }

        public string GetQuery()
        {
            Propertiese.Length--;
            query.AppendLine(string.Format("SET n += {{{0}}};", Propertiese.ToString()));
            return query.ToString();
        }
    }
}
