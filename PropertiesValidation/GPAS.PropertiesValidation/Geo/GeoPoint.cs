using GPAS.Dispatch.Entities.Concepts.Geo;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace GPAS.PropertiesValidation
{
    public class GeoPoint
    {
        public static string GetGeoPointStringValue(GeoLocationEntityRawData geoLocationEntityRawData)
        {
            return JsonConvert.SerializeObject(geoLocationEntityRawData);
        }
        public static GeoLocationEntityRawData GeoPointEntityRawData(string geoPointStringValue)
        {
            return JsonConvert.DeserializeObject<GeoLocationEntityRawData>(geoPointStringValue);
        }

        public static ValidationProperty IsValidGeoPoint(string propertyValue, out GeoLocationEntityRawData geoLocationRawData)
        {
            return IsValidGeoPoint(propertyValue, CultureInfo.InvariantCulture, out geoLocationRawData);
        }

        public static ValidationProperty IsValidGeoPoint(string propertyValue, CultureInfo cultureInfo,
            out GeoLocationEntityRawData geoLocationRawData)
        {
            ValidationProperty validationResult = new ValidationProperty()
            {
                Status = ValidationStatus.Invalid,
                Message = "The values (latitude or longitude) entered are not valid"
            };

            geoLocationRawData = null;
            try
            {
                geoLocationRawData = GeoPointEntityRawData(propertyValue);
                if(geoLocationRawData == null)
                    return validationResult;
            }
            catch
            {
                return validationResult;
            }

            ValidationProperty latitudeValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Double,
                geoLocationRawData.Latitude, out var parsedLat, cultureInfo);
            ValidationProperty longitudeValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Double,
                geoLocationRawData.Longitude,out var parsedLng, cultureInfo);

            bool latitudeInBound = DMSToDecimalGeoConvertor.LatitudeTryParse(geoLocationRawData.Latitude, cultureInfo, out _);
            bool longitudeInBound = DMSToDecimalGeoConvertor.LongitudeTryParse(geoLocationRawData.Longitude, cultureInfo, out _);

            if (latitudeValidation.Status != ValidationStatus.Invalid &&
               longitudeValidation.Status != ValidationStatus.Invalid &&
               latitudeInBound && longitudeInBound)
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

                validationResult.Status = ValidationStatus.Invalid;
                validationResult.Message = errorMessage;
                return validationResult;
            }

            return validationResult;
        }
    }
}
