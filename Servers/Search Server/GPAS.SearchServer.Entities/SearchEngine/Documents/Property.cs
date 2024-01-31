
using GPAS.Ontology;
using System;
using System.Collections.Generic;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class Property
    {
        public string Id { get; set; }
        public string TypeUri { get; set; }
        public byte BaseType { get; set; }
        public string OwnerObjectID { get; set; }
        public string OwnerObjectTypeUri { get; set; }
        public ACL Acl { get; set; }
        public bool? BooleanValue { get; set; }
        public string StringValue { get; set; }
        public string KeywordTokenizedStringValue { get { return StringValue; } }
        public double? DoubleValue { get; set; }
        public string GeoValue { get; set; }
        public string DateRangeValue { get; set; }
        public string GeoTime { get; set; }
        public string GeoPoint { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public long? LongValue { get; set; }

        public long DataSourceId { get; set; }

        public static string GetPropertyValueFieldNameByBaseType(BaseDataTypes baseType)
        {
            switch (baseType)
            {
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    return nameof(LongValue);
                case BaseDataTypes.Boolean:
                    return nameof(BooleanValue);
                case BaseDataTypes.DateTime:
                    return nameof(DateTimeValue);
                case BaseDataTypes.String:
                case BaseDataTypes.HdfsURI:
                    return nameof(StringValue);
                case BaseDataTypes.Double:
                    return nameof(DoubleValue);
                case BaseDataTypes.GeoTime:
                    return nameof(GeoTime);
                case BaseDataTypes.GeoPoint:
                    return nameof(GeoPoint);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
