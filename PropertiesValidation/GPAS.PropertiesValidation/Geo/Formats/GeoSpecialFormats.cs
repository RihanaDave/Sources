using GPAS.PropertiesValidation.Geo.Formats;
using System;

namespace GPAS.PropertiesValidation
{
    public class GeoSpecialFormats
    {
        public bool GeoSpecialConvertor(string value, GeoComponentType geoComponentType, GeoSpecialTypes geoSpecialType, out double? totalDegrees)
        {
            totalDegrees = null;
            double convertedValue = 0.0;
            Angle angle;
            switch (geoSpecialType)
            {
                case GeoSpecialTypes.DMS:
                    if (geoComponentType == GeoComponentType.Latitude)
                        if (DMSToDecimalGeoConvertor.LatitudeTryParse(value, null, out angle))
                        {
                            totalDegrees = angle.TotalDegrees;
                            return true;
                        }
                        else
                            return false;
                    else if (DMSToDecimalGeoConvertor.LongitudeTryParse(value, null, out angle))
                    {
                        totalDegrees = angle.TotalDegrees;
                        return true;
                    }
                    else
                        return false;

                case GeoSpecialTypes.CompoundDMS:
                    if (geoComponentType == GeoComponentType.Latitude)
                        if (DMSToDecimalGeoConvertor.LatitudeTryParse(value, null, out angle))
                        {
                            totalDegrees = angle.TotalDegrees;
                            return true;
                        }
                        else
                            return false;
                    else if (DMSToDecimalGeoConvertor.LongitudeTryParse(value, null, out angle))
                    {
                        totalDegrees = angle.TotalDegrees;
                        return true;
                    }
                    else
                        return false;

                case GeoSpecialTypes.Decimal:
                    if (Double.TryParse(value, out convertedValue))
                    {
                        totalDegrees = convertedValue;
                        if (geoComponentType == GeoComponentType.Latitude)
                        {
                            if (totalDegrees > 90.0 || totalDegrees < -90.0)
                            {
                                return false;
                            }
                        }
                        else if (geoComponentType == GeoComponentType.Longitude)
                        {
                            if (totalDegrees > 180.0 || totalDegrees < -180.0)
                            {
                                return false;
                            }
                        }
                            return true;
                    }
                    else
                    {
                        return false;
                    }

                case GeoSpecialTypes.CompoundDecimal:
                    /*
                    if (geoComponentType == GeoComponentType.Latitude)
                        //split latitude
                        else
                        //splite longitude
                */
                    totalDegrees = convertedValue;
                    return true;
                default:
                    return false;
            }
        }
    }
}
