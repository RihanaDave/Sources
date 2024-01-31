using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Ontology;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace GPAS.PropertiesValidation
{
    // TODO: Clean!
    public class ValueBaseValidation
    {
        public static ValidationProperty IsValidPropertyValue(BaseDataTypes propertyBaseType, string propertyValue)
        {
            return IsValidPropertyValue(propertyBaseType, propertyValue, CultureInfo.InvariantCulture);
        }

        public static ValidationProperty IsValidPropertyValue(BaseDataTypes propertyBaseType, string propertyValue, CultureInfo cultureInfo)
        {
            return IsValidPropertyValue(propertyBaseType, propertyValue, cultureInfo, null, null, null, null, null);
        }

        public static ValidationProperty IsValidPropertyValue(BaseDataTypes propertyBaseType, string propertyValue,
            CultureInfo cultureInfo, string dateTimeFormat,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            ValidationProperty result = new ValidationProperty();
            switch (propertyBaseType)
            {
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    if (long.TryParse(propertyValue, NumberStyles.None, cultureInfo, out var longVar)
                        && IsJsonValid(longVar))
                    {//Valid Or Warning
                        result.Status = ValidationStatus.Valid;
                    }
                    else
                    {//Invalid
                        result.Status = ValidationStatus.Invalid;
                    }
                    break;

                case BaseDataTypes.Double:
                    if (double.TryParse(propertyValue, NumberStyles.Float, cultureInfo, out var doubleVar)
                        && IsJsonValid(doubleVar))
                    {//Valid Or Warning
                        result.Status = ValidationStatus.Valid;
                    }
                    else
                    {//Invalid
                        result.Status = ValidationStatus.Invalid;
                    }
                    break;

                case BaseDataTypes.Boolean:
                    if (bool.TryParse(propertyValue, out var boolVar)
                        && IsJsonValid(boolVar))
                    {//Valid Or Warning
                        result.Status = ValidationStatus.Valid;
                    }
                    else
                    {//Invalid
                        result.Status = ValidationStatus.Invalid;
                    }
                    break;

                case BaseDataTypes.DateTime:

                    DateTime dateTimeVar = DateTime.MinValue;
                    bool parseResult = TryParseDateTime(propertyValue, cultureInfo, dateTimeFormat, ref dateTimeVar);

                    if (parseResult && IsJsonValid(dateTimeVar))
                    {
                        //Valid Or Warning
                        DateTime limitDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        if (dateTimeVar < limitDateTime)
                        {
                            result.Status = ValidationStatus.Warning;
                            result.Message = $"Warning! DateTime value can not be smaller than {limitDateTime}.";
                        }
                        else
                        {
                            result.Status = ValidationStatus.Valid;
                        }
                    }
                    else
                    {
                        //Invalid
                        result.Status = ValidationStatus.Invalid;
                        result.Message = "Warning! Invalid input value.";
                    }
                    break;

                case BaseDataTypes.HdfsURI:
                    if (!UriHasInvalidChar(propertyValue) &&
                        Uri.TryCreate(propertyValue, UriKind.Absolute, out var uriVar)
                        && IsJsonValid(uriVar))
                    {//Valid Or Warning
                        result.Status = ValidationStatus.Valid;
                    }
                    else
                    {//Invalid
                        result.Status = ValidationStatus.Invalid;
                    }
                    break;

                case BaseDataTypes.String:
                    if (IsJsonValid(propertyValue))
                    {//Valid Or Warning
                        byte[] array = Encoding.ASCII.GetBytes(propertyValue);
                        int limitStringLength = 32765;
                        if (array.Length > limitStringLength)
                        {
                            result.Status = ValidationStatus.Warning;
                            result.Message = $"Value string length must be smaller than {limitStringLength}.";
                        }
                        else
                        {
                            result.Status = ValidationStatus.Valid;
                        }
                    }
                    else
                    {//Invalid
                        result.Status = ValidationStatus.Invalid;
                    }
                    break;
                case BaseDataTypes.GeoTime:
                    result = GeoTime.IsValidGeoTime(propertyValue, cultureInfo, out _,
                        geotimeStartDateCultureInfo, geotimeStartDateStringFormat,
                        geotimeEndDateCultureInfo, geotimeEndDateStringFormat
                        );
                    break;
                case BaseDataTypes.GeoPoint:
                    result = GeoPoint.IsValidGeoPoint(propertyValue, cultureInfo, out _);
                    break;
                // TODO: بحرانی - نوع ویژگی پایه یوآرآی به اچ دی اف اس اضافه شود
                default:
                    throw new NotSupportedException("Unknown property type");
            }
            return result;
        }

        private static bool UriHasInvalidChar(string uri)
        {
            if (uri.Intersect(System.IO.Path.GetInvalidPathChars()).Count() > 0)
                return true;

            return false;
        }

        public static object ParsePropertyValue(BaseDataTypes propertyBaseType, string propertyValue)
        {
            return ParsePropertyValue(propertyBaseType, propertyValue, CultureInfo.InvariantCulture);
        }

        public static object ParsePropertyValue(BaseDataTypes propertyBaseType, string propertyValue, CultureInfo cultureInfo)
        {
            return ParsePropertyValue(propertyBaseType, propertyValue, cultureInfo, null, null, null, null, null);
        }

        public static object ParsePropertyValue(BaseDataTypes propertyBaseType, string propertyValue,
            CultureInfo cultureInfo, string dateTimeFormat,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            switch (propertyBaseType)
            {
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    long longVar = long.Parse(propertyValue, NumberStyles.None, cultureInfo);
                    longVar = (long)JsonParse(longVar);

                    return longVar;
                case BaseDataTypes.Double:
                    double doubleVar = double.Parse(propertyValue, NumberStyles.Float, cultureInfo);
                    doubleVar = (double)JsonParse(doubleVar);

                    return doubleVar;
                case BaseDataTypes.Boolean:
                    bool boolVar = bool.Parse(propertyValue);
                    boolVar = (bool)JsonParse(boolVar);

                    return boolVar;
                case BaseDataTypes.DateTime:
                    DateTime dateTimeVar = ParseDateTime(propertyValue, cultureInfo, dateTimeFormat);
                    dateTimeVar = (DateTime)JsonParse(dateTimeVar);

                    return dateTimeVar;
                case BaseDataTypes.HdfsURI:
                    Uri uriVar = new Uri(propertyValue, UriKind.Absolute);
                    uriVar = (Uri)JsonParse(uriVar);

                    return uriVar;
                case BaseDataTypes.String:
                    string stringVar = JsonParse(propertyValue).ToString();

                    return stringVar;
                case BaseDataTypes.GeoTime:
                    GeoTimeEntityRawData geoTimeEntityRawData = GeoTime.GeoTimeEntityRawData(propertyValue);
                    return new GeoTimeEntity()
                    {
                        DateRange = new TimeInterval()
                        {
                            TimeBegin = (DateTime)ParsePropertyValue(BaseDataTypes.DateTime, geoTimeEntityRawData.TimeBegin,
                            geotimeStartDateCultureInfo, geotimeStartDateStringFormat, null, null, null, null),
                            TimeEnd = (DateTime)ParsePropertyValue(BaseDataTypes.DateTime, geoTimeEntityRawData.TimeEnd,
                            geotimeEndDateCultureInfo, geotimeEndDateStringFormat, null, null, null, null)
                        },
                        Location = new GeoLocationEntity()
                        {
                            Latitude = (double)ParsePropertyValue(BaseDataTypes.Double, geoTimeEntityRawData.Latitude, cultureInfo),
                            Longitude = (double)ParsePropertyValue(BaseDataTypes.Double, geoTimeEntityRawData.Longitude, cultureInfo)
                        }
                    };

                case BaseDataTypes.GeoPoint:
                    GeoLocationEntityRawData geoLocationEntityRawData = GeoPoint.GeoPointEntityRawData(propertyValue);
                    return new GeoLocationEntity()
                    {
                        Latitude = (double)ParsePropertyValue(BaseDataTypes.Double, geoLocationEntityRawData.Latitude, cultureInfo),
                        Longitude = (double)ParsePropertyValue(BaseDataTypes.Double, geoLocationEntityRawData.Longitude, cultureInfo)
                    };

                // TODO: بحرانی - نوع ویژگی پایه یوآرآی به اچ دی اف اس اضافه شود
                default:
                    throw new NotSupportedException("Unknown property type");
            }
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string propertyValue, out object parsedValue)
        {
            return TryParsePropertyValue(propertyBaseType, propertyValue, out parsedValue, CultureInfo.InvariantCulture);
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string propertyValue, out object parsedValue,
            CultureInfo cultureInfo)
        {
            return TryParsePropertyValue(propertyBaseType, propertyValue, out parsedValue, cultureInfo, null, null, null, null, null);
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string propertyValue, out object parsedValue,
            CultureInfo cultureInfo, string dateTimeFormat,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            ValidationProperty isParsed = new ValidationProperty();

            switch (propertyBaseType)
            {
                case BaseDataTypes.Int:

                    if (int.TryParse(propertyValue, NumberStyles.None, cultureInfo, out var parsedIntValue))
                    {
                        isParsed.Status = JsonTryParse(parsedIntValue, out var jsonValue) ? ValidationStatus.Valid : ValidationStatus.Invalid;

                        parsedValue = jsonValue;
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                        parsedValue = parsedIntValue;
                    }
                    break;

                case BaseDataTypes.Long:

                    if (long.TryParse(propertyValue, NumberStyles.None, cultureInfo, out var parsedLongValue))
                    {
                        isParsed.Status = JsonTryParse(parsedLongValue, out var jsonValue) ? ValidationStatus.Valid : ValidationStatus.Invalid;

                        parsedValue = jsonValue;
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                        parsedValue = parsedLongValue;
                    }
                    break;

                case BaseDataTypes.Double:

                    if (double.TryParse(propertyValue, NumberStyles.Float, cultureInfo, out var parsedDoubleValue))
                    {
                        isParsed.Status = JsonTryParse(parsedDoubleValue, out var jsonValue) ? ValidationStatus.Valid : ValidationStatus.Invalid;

                        parsedValue = jsonValue;
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                        parsedValue = parsedDoubleValue;
                    }
                    break;

                case BaseDataTypes.Boolean:

                    if (bool.TryParse(propertyValue, out var parsedBoolValue))
                    {
                        isParsed.Status = JsonTryParse(parsedBoolValue, out var jsonValue) ? ValidationStatus.Valid : ValidationStatus.Invalid;

                        parsedValue = jsonValue;
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                        parsedValue = parsedBoolValue;
                    }
                    break;

                case BaseDataTypes.DateTime:
                    DateTime parsedDateTimeValue = new DateTime();
                    bool parseResult = TryParseDateTime(propertyValue, cultureInfo, dateTimeFormat, ref parsedDateTimeValue);

                    if (parseResult)
                    {
                        if (JsonTryParse(parsedDateTimeValue, out var jsonValue))
                        {
                            DateTime limitDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            if ((DateTime)jsonValue < limitDateTime)
                            {
                                isParsed.Status = ValidationStatus.Warning;
                                isParsed.Message = $"Warning! DateTime value can not be smaller than {limitDateTime}.";
                            }
                            else
                            {
                                isParsed.Status = ValidationStatus.Valid;
                            }
                        }
                        else
                        {
                            isParsed.Status = ValidationStatus.Invalid;
                        }

                        parsedValue = jsonValue;
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                        parsedValue = parsedDateTimeValue;
                    }
                    break;

                case BaseDataTypes.HdfsURI:

                    if (Uri.TryCreate(propertyValue, UriKind.Absolute, out var parsedUriValue))
                    {
                        isParsed.Status = JsonTryParse(parsedUriValue, out var jsonValue) ? ValidationStatus.Valid : ValidationStatus.Invalid;

                        parsedValue = jsonValue;
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                        parsedValue = parsedUriValue;
                    }
                    break;

                case BaseDataTypes.String:
                    if (JsonTryParse(propertyValue, out var parsedStringValue))
                    {
                        byte[] array = Encoding.ASCII.GetBytes(propertyValue);
                        int limitStringLength = 32765;
                        if (array.Length > limitStringLength)
                        {
                            isParsed.Status = ValidationStatus.Warning;
                            isParsed.Message = $"Value string length must be smaller than {limitStringLength}.";
                        }
                        else
                        {
                            isParsed.Status = ValidationStatus.Valid;
                        }
                    }
                    else
                    {
                        isParsed.Status = ValidationStatus.Invalid;
                    }
                    parsedValue = parsedStringValue;
                    break;

                case BaseDataTypes.GeoTime:
                    isParsed.Status = GeoTime.IsValidGeoTime(propertyValue, out GeoTimeEntityRawData parsedGeoTimeRawDataValue,
                        geotimeStartDateCultureInfo, geotimeStartDateStringFormat,
                        geotimeEndDateCultureInfo, geotimeEndDateStringFormat).Status;

                    parsedValue = new GeoTimeEntity()
                    {
                        DateRange = new TimeInterval()
                        {
                            TimeBegin = (DateTime)ParsePropertyValue(BaseDataTypes.DateTime, parsedGeoTimeRawDataValue.TimeBegin,
                            geotimeStartDateCultureInfo, geotimeStartDateStringFormat, null, null, null, null),
                            TimeEnd = (DateTime)ParsePropertyValue(BaseDataTypes.DateTime, parsedGeoTimeRawDataValue.TimeEnd,
                            geotimeEndDateCultureInfo, geotimeEndDateStringFormat, null, null, null, null)
                        },
                        Location = new GeoLocationEntity()
                        {
                            Latitude = (double)ParsePropertyValue(BaseDataTypes.Double, parsedGeoTimeRawDataValue.Latitude, cultureInfo),
                            Longitude = (double)ParsePropertyValue(BaseDataTypes.Double, parsedGeoTimeRawDataValue.Longitude, cultureInfo)
                        }
                    };
                    break;

                case BaseDataTypes.GeoPoint:
                    isParsed.Status = GeoPoint.IsValidGeoPoint(propertyValue, cultureInfo,
                        out GeoLocationEntityRawData parsedGeoLocationRawDataValue).Status;
                    parsedValue = new GeoLocationEntity()
                    {
                        Latitude = (double)ParsePropertyValue(BaseDataTypes.Double, parsedGeoLocationRawDataValue.Latitude, cultureInfo),
                        Longitude = (double)ParsePropertyValue(BaseDataTypes.Double, parsedGeoLocationRawDataValue.Longitude, cultureInfo)
                    };
                    break;

                // TODO: بحرانی - نوع ویژگی پایه یوآرآی به اچ دی اف اس اضافه شود
                default:
                    throw new NotSupportedException("Unknown property type");
            }

            return isParsed;
        }

        private static bool TryParseDateTime(string propertyValue, CultureInfo cultureInfo, string dateTimeFormat,
            ref DateTime parsedDateTimeValue)
        {
            if (dateTimeFormat == null)
            {
                return DateTime.TryParse(propertyValue, cultureInfo, DateTimeStyles.NoCurrentDateDefault,
                    out parsedDateTimeValue);
            }
            else
            {
                try
                {
                    return DateTime.TryParseExact(propertyValue, dateTimeFormat, cultureInfo, DateTimeStyles.None,
                        out parsedDateTimeValue);
                }
                catch
                {
                    return false;
                }
            }
        }

        private static DateTime ParseDateTime(string propertyValue, CultureInfo cultureInfo, string dateTimeFormat)
        {
            if (dateTimeFormat == null)
            {
                return DateTime.Parse(propertyValue, cultureInfo, DateTimeStyles.NoCurrentDateDefault);
            }
            else
            {
                return DateTime.ParseExact(propertyValue, dateTimeFormat, cultureInfo, DateTimeStyles.None);
            }
        }

        private static bool JsonTryParse(object val, out object result)
        {
            try
            {
                result = JsonParse(val);
                return true;
            }
            catch
            {
                if (val is string)
                {
                    result = string.Empty;
                }
                else if (val is long || val is int || val is double)
                {
                    result = 0;
                }
                else if (val is bool)
                {
                    result = false;
                }
                else if (val is DateTime)
                {
                    result = DateTime.MinValue;
                }
                else if (val is Uri)
                {
                    result = null;
                }
                else
                {
                    result = null;
                }

                return false;
            }
        }

        private static bool IsJsonValid(object val)
        {
            try
            {
                JsonParse(val);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object JsonParse(object val)
        {
            var jsonJObject = new JObject(new JProperty("value", val));
            JObject.Parse(jsonJObject.ToString());
            return val;
        }
    }
}
