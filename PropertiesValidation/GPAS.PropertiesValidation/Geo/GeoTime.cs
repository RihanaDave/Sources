using GPAS.Dispatch.Entities.Concepts.Geo;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace GPAS.PropertiesValidation
{
    public class GeoTime
    {
        public static string GetGeoTimeStringValue(GeoTimeEntityRawData geoTimeEntityRawData)
        {
            return JsonConvert.SerializeObject(geoTimeEntityRawData);
        }
        public static GeoTimeEntityRawData GeoTimeEntityRawData(string GeoTimeStringValue)
        {
            return JsonConvert.DeserializeObject<GeoTimeEntityRawData>(GeoTimeStringValue);
        }

        public static ValidationProperty IsValidGeoTime(string propertyValue, out GeoTimeEntityRawData geoTimeRawData,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            return IsValidGeoTime(propertyValue, CultureInfo.InvariantCulture, out geoTimeRawData,
                geotimeStartDateCultureInfo, geotimeStartDateStringFormat,
                geotimeEndDateCultureInfo, geotimeEndDateStringFormat);
        }

        public static ValidationProperty IsValidGeoTime(string propertyValue, CultureInfo cultureInfo,
            out GeoTimeEntityRawData geoTimeRawData,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            ValidationProperty validationResult = new ValidationProperty()
            {
                Status = ValidationStatus.Invalid,
                Message = "The values (latitude, longitude, start date or end date) entered are not valid"
            };

            geoTimeRawData = null;
            try
            {
                geoTimeRawData = GeoTimeEntityRawData(propertyValue);
            }
            catch
            {
                return validationResult;
            }

            ValidationProperty latitudeValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Double,
                geoTimeRawData.Latitude, out var parsedLat, cultureInfo);
            ValidationProperty longitudeValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Double,
                geoTimeRawData.Longitude, out var parsedLng, cultureInfo);
            ValidationProperty timeBeginValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.DateTime,
                geoTimeRawData.TimeBegin, out var parsedBegin, geotimeStartDateCultureInfo, geotimeStartDateStringFormat, null, null, null, null);
            ValidationProperty timeEndValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.DateTime,
                geoTimeRawData.TimeEnd, out var parsedEnd, geotimeEndDateCultureInfo, geotimeEndDateStringFormat, null, null, null, null);

            bool latitudeInBound = DMSToDecimalGeoConvertor.LatitudeTryParse(geoTimeRawData.Latitude, cultureInfo, out _);
            bool longitudeInBound = DMSToDecimalGeoConvertor.LongitudeTryParse(geoTimeRawData.Longitude, cultureInfo, out _);

            if (latitudeValidation.Status != ValidationStatus.Invalid &&
               longitudeValidation.Status != ValidationStatus.Invalid &&
               timeBeginValidation.Status != ValidationStatus.Invalid &&
               timeEndValidation.Status != ValidationStatus.Invalid &&
               latitudeInBound && longitudeInBound &&
               (DateTime)parsedBegin <= (DateTime)parsedEnd)
            {
                validationResult.Status = ValidationStatus.Valid;
                validationResult.Message = validationResult.Status.ToString();
            }
            else
            {
                string errorMessage = string.Empty;
                if (latitudeValidation.Status == ValidationStatus.Invalid)
                {
                    errorMessage += "Latitude must be numerical";
                }
                else
                {
                    if (!latitudeInBound)
                    {
                        errorMessage += "Latitude is out of range [-90, 90]";
                    }
                }

                if (longitudeValidation.Status == ValidationStatus.Invalid)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                        errorMessage += Environment.NewLine;

                    errorMessage += "Longitude must be numerical";
                }
                else
                {
                    if (!longitudeInBound)
                    {
                        if (!string.IsNullOrEmpty(errorMessage))
                            errorMessage += Environment.NewLine;

                        errorMessage += "Longitude is out of range [-180, 180]";
                    }
                }

                if (parsedBegin == null || timeBeginValidation.Status == ValidationStatus.Invalid)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                        errorMessage += Environment.NewLine;

                    errorMessage += "Start date must be date or time";
                }

                if (parsedEnd == null || timeEndValidation.Status == ValidationStatus.Invalid)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                        errorMessage += Environment.NewLine;

                    errorMessage += "End date must be date or time";
                }

                if ((DateTime)parsedBegin <= (DateTime)parsedEnd)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                        errorMessage += Environment.NewLine;

                    errorMessage += "Time begin must be smaller than time end";
                }

                validationResult.Status = ValidationStatus.Invalid;
                validationResult.Message = errorMessage;
                return validationResult;
            }

            return validationResult;
        }
    }
}
