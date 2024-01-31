using GPAS.AccessControl;
using GPAS.FilterSearch;
using System.Collections.Generic;
using System.Text;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j.QueryGenerators
{
    public class SearchAround
    {
        protected string PrepareFirstPartOfQuery(Dictionary<string, long[]> objectList, string idKey)
        {
            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("CALL{");

            foreach (var item in objectList)
            {
                partOfQuery.Append("\nMATCH (s:" + item.Key + ") WHERE s." + idKey +
                    " IN [" + FillSearchedVerticesID(item.Value) + "] RETURN s\nUNION");
            }

            partOfQuery.Length -= "UNION".Length;
            partOfQuery.Append("}\nWITH s");
            return partOfQuery.ToString();
        }

        protected string FillSearchedVerticesID(long[] searchedVerticesID)
        {
            StringBuilder propertieseValueList = new StringBuilder();

            foreach (var id in searchedVerticesID)
            {
                propertieseValueList.Append($"'{id}',");
            }

            propertieseValueList.Length--;
            return propertieseValueList.ToString();
        }

        protected string FillSearchedVerticesTypeURI(string[] verticesTypeURI)
        {
            StringBuilder typeUris = new StringBuilder();

            foreach (var typeUri in verticesTypeURI)
            {
                typeUris.Append($"'{typeUri}',");
            }

            typeUris.Length--;
            return typeUris.ToString();
        }

        protected string FillRelationGroupNames(List<string> groupNames, string relationKey)
        {
            StringBuilder partOfQuery = new StringBuilder();
            partOfQuery.Append("AND (");

            foreach (var groupName in groupNames)
            {
                partOfQuery.Append(relationKey + "." + groupName + " >= '" + (byte)Permission.Read + "' OR ");
            }
            partOfQuery.Remove(partOfQuery.Length - 4, 4).Append(") ");
            return partOfQuery.ToString();
        }

        protected string FillVertexProperties(PropertyValueCriteria[] propertyValueCriterias, string alias)
        {
            if (propertyValueCriterias.Length == 0)
                return string.Empty;

            List<string> propertiesQuerypartOfQuery = new List<string>();

            foreach (PropertyValueCriteria property in propertyValueCriterias)
            {
                if (MainClass.Ontology.GetBaseDataTypeOfProperty(property.PropertyTypeUri) == GraphRepositoryBaseDataTypes.GeoPoint)
                {
                    GeoCircleModel geoPoint = Serialization.DeserializJson<GeoCircleModel>(property.OperatorValuePair.GetInvarientValue());
                    propertiesQuerypartOfQuery.Add($"distance({alias}.{property.PropertyTypeUri}, point({{latitude:{geoPoint.Latitude}," +
                        $" longitude: {geoPoint.Longitude }}})) <= {geoPoint.Radius}");
                }
                else
                {
                    string value = Serialization.EncodePropertyValueToUseInRetrieveQuery(property.PropertyTypeUri,
                        property.OperatorValuePair.GetInvarientValue());
                    propertiesQuerypartOfQuery.Add($"{alias}.{property.PropertyTypeUri} = {value}");
                }
            }

            return string.Join(" AND ", propertiesQuerypartOfQuery) + " ";
        }
    }
}
