using System.Collections.Generic;
using System.Text;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class CreateVertex
    {
        private readonly StringBuilder query;
        private readonly StringBuilder Propertiese;

        public CreateVertex(string typeUri)
        {
            query = new StringBuilder();
            Propertiese = new StringBuilder();

            query.Append("(:" + typeUri + " {0}),");
            Propertiese.Append("{");
        }

        public void AddProperty(string name, string value)
        {
            Propertiese.Append($"{name}: {value},");
        }

        public void AddGeoProperty(string name, GeoPointModel value)
        {
            Propertiese.Append($"{name}: point({{latitude:{value.Latitude}, longitude:{value.Longitude}}}),");
        }

        public void AddMultiValueProperty(string name, IEnumerable<string> values)
        {
            Propertiese.Append($"{name}:[");
            foreach (string value in values)
            {
                Propertiese.Append($"'{value}',");
            }
            Propertiese.Length--;
            Propertiese.Append("],");
        }

        public string GetQuery()
        {
            Propertiese.Length--;
            Propertiese.Append("}");
            return string.Format(query.ToString(), Propertiese.ToString());
        }
    }
}
