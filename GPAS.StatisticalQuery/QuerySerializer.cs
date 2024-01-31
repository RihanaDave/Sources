using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.StatisticalQuery.Formula.SetAlgebra;
using GPAS.StatisticalQuery.ObjectSet;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.StatisticalQuery
{
    public class QuerySerializer
    {
        private Type[] GetPreipheralTypes()
        {
            return new Type[] {
                typeof(Query),
                typeof(StartingObjectSet),
                typeof(FormulaStep),
                typeof(TypeBasedDrillDown),
                typeof(TypeBasedDrillDownPortionBase),
                typeof(LinkTypeBasedDrillDown),
                typeof(LinkedObjectTypeBasedDrillDown),
                typeof(LinkBasedDrillDown),
                typeof(PropertyValueBasedDrillDown),
                typeof(PropertyValueRangeDrillDown),
                typeof(PropertyValueRangeStatistics),
                typeof(PropertyValueRangeStatistic),
                typeof(OfObjectType),
                typeof(HasPropertyWithType),
                typeof(HasPropertyWithTypeAndValue),
                typeof(PerformSetOperation),
                typeof(Operator)
            };
        }
        public void Serialize(Query query, Stream streamWriter)
        {
            if (streamWriter == null)
                throw new ArgumentNullException(nameof(streamWriter));

            XmlTextWriter xmlTextWriter = new XmlTextWriter(streamWriter, Encoding.UTF8);
            XmlSerializer serializer = new XmlSerializer(typeof(Query), GetPreipheralTypes());
            serializer.Serialize(xmlTextWriter, query);
        }
        public Query Deserialize(MemoryStream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException(nameof(streamReader));

            using (MemoryStream resultQueryStream = new MemoryStream(streamReader.ToArray()))
            {
                using (StreamReader xmlStreamReader = new StreamReader(resultQueryStream, Encoding.UTF8))
                {
                    XmlReader xr = XmlReader.Create(xmlStreamReader);
                    XmlSerializer xs = new XmlSerializer(typeof(Query), GetPreipheralTypes());
                    return (Query)xs.Deserialize(xr);
                }
            }
        }
    }
}
