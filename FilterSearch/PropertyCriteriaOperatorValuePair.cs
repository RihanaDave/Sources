using GPAS.PropertiesValidation.Geo;
using System;
using System.Globalization;

namespace GPAS.FilterSearch
{
    public abstract class PropertyCriteriaOperatorValuePair : BaseModel
    {
        private const string DefaultDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        public string GetInvarientValue()
        {
            if (this is StringPropertyCriteriaOperatorValuePair stringPropertyCriteriaOperatorValuePair)
            {
                return stringPropertyCriteriaOperatorValuePair.CriteriaValue;
            }
            else if (this is LongPropertyCriteriaOperatorValuePair longPropertyCriteriaOperatorValuePair)
            {
                return longPropertyCriteriaOperatorValuePair.CriteriaValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (this is FloatPropertyCriteriaOperatorValuePair floatPropertyCriteriaOperatorValuePair)
            {
                return floatPropertyCriteriaOperatorValuePair.CriteriaValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (this is DateTimePropertyCriteriaOperatorValuePair dateTimePropertyCriteriaOperatorValuePair)
            {
                return dateTimePropertyCriteriaOperatorValuePair.CriteriaValue.ToString(DefaultDateTimeFormat, CultureInfo.InvariantCulture);
            }
            else if (this is BooleanPropertyCriteriaOperatorValuePair booleanPropertyCriteriaOperatorValuePair)
            {
                return booleanPropertyCriteriaOperatorValuePair.CriteriaValue.ToString(CultureInfo.InvariantCulture).ToLower();
            }
            else if(this is GeoPointPropertyCriteriaOperatorValuePair geoPointPropertyCriteriaOperatorValuePair)
            {
                return GeoCircle.GetGeoCircleStringValue(geoPointPropertyCriteriaOperatorValuePair.CriteriaValue);
            }
            else if (this is EmptyPropertyCriteriaOperatorValuePair)
            {
                return string.Empty;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}