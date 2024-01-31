using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GPAS.PropertiesValidation
{
    public class DMSToDecimalGeoConvertor
    {
        private static Angle LongitudeParseDegreesMinutesSeconds(string value, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ParseLongitude(value, provider, GeoParser.degreeMinuteSecondRegex);
        }
        private static Angle LongitudeParseDegreesMinutes(string value, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ParseLongitude(value, provider, GeoParser.degreeMinuteRegex);
        }
        private static Angle LongitudeParseDegrees(string value, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ParseLongitude(value, provider, GeoParser.degreeRegex);
        }
        public static bool LongitudeTryParse(string value, IFormatProvider provider, out Angle angle)
        {
            //Location (Latitude and Longitude) DMS Format Conversion
            angle = LongitudeParseDegreesMinutesSeconds(value, provider);
            if (angle != null)
            {
                return true;
            }

            angle = LongitudeParseDegreesMinutes(value, provider);
            if (angle != null)
            {
                return true;
            }

            angle = LongitudeParseDegrees(value, provider);
            if (angle != null)
            {
                return true;
            }


            //Latitude DMS Format Conversion
            string valueDegreesMinutesSeconds = value;
            string valueDegreesMinutes = value;
            string valueDegrees = value;

            valueDegreesMinutesSeconds = "0 0 0 N, " + valueDegreesMinutesSeconds;
            angle = LongitudeParseDegreesMinutesSeconds(valueDegreesMinutesSeconds, provider);
            if (angle != null)
            {
                return true;
            }

            valueDegreesMinutes = "0 0 N, " + valueDegreesMinutes;
            angle = LongitudeParseDegreesMinutes(valueDegreesMinutes, provider);
            if (angle != null)
            {
                return true;
            }

            valueDegrees = "0 N, " + valueDegrees;
            angle = LongitudeParseDegrees(valueDegrees, provider);
            if (angle != null)
            {
                return true;
            }

            return angle != null;
        }
        private static Angle ParseLongitude(string input, IFormatProvider provider, Regex regex)
        {
            var match = regex.Match(input.Replace(", ", " "));
            if (match.Success)
            {
                Angle latitude = ParseAngle(
                    provider,
                    TryGetValue(match, "latSuf"),
                    TryGetValue(match, "latDeg"),
                    TryGetValue(match, "latMin"),
                    TryGetValue(match, "latSec"));

                Angle longitude = ParseAngle(
                      provider,
                      TryGetValue(match, "lonSuf"),
                      TryGetValue(match, "lonDeg"),
                      TryGetValue(match, "lonMin"),
                      TryGetValue(match, "lonSec"));

                if ((latitude == null) ||
                  (longitude == null) ||
                  (Math.Abs(latitude.TotalDegrees) > 90.0) ||
                  (Math.Abs(longitude.TotalDegrees) > 180.0))
                {
                    return null;
                }

                return longitude;
            }
            return null;
        }

        private static Angle LatitudeParseDegreesMinutesSeconds(string value, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ParseLatitude(value, provider, GeoParser.degreeMinuteSecondRegex);
        }
        private static Angle LatitudeParseDegreesMinutes(string value, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ParseLatitude(value, provider, GeoParser.degreeMinuteRegex);
        }
        private static Angle LatitudeParseDegrees(string value, IFormatProvider provider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            return ParseLatitude(value, provider, GeoParser.degreeRegex);
        }

        public static bool LatitudeTryParse(string value, IFormatProvider provider, out Angle angle)
        {
            //Location (Latitude and Longitude) DMS Format Conversion
            angle = LatitudeParseDegreesMinutesSeconds(value, provider);
            if (angle != null)
            {
                return true;
            }

            angle = LatitudeParseDegreesMinutes(value, provider);
            if (angle != null)
            {
                return true;
            }

            angle = LatitudeParseDegrees(value, provider);
            if (angle != null)
            {
                return true;
            }


            //Latitude DMS Format Conversion
            string valueDegreesMinutesSeconds = value;
            string valueDegreesMinutes = value;
            string valueDegrees = value;

            valueDegreesMinutesSeconds += ", 0 0 0 E";
            angle = LatitudeParseDegreesMinutesSeconds(valueDegreesMinutesSeconds, provider);
            if (angle != null)
            {
                return true;
            }

            valueDegreesMinutes += ", 0 0 E";
            angle = LatitudeParseDegreesMinutes(valueDegreesMinutes, provider);
            if (angle != null)
            {
                return true;
            }

            valueDegrees += ", 0 E";
            angle = LatitudeParseDegrees(valueDegrees, provider);
            if (angle != null)
            {
                return true;
            }

            return angle != null;
        }

        private static Angle ParseLatitude(string input, IFormatProvider provider, Regex regex)
        {
            var match = regex.Match(input.Replace(", ", " "));
            if (match.Success)
            {
                Angle latitude = ParseAngle(
                    provider,
                    TryGetValue(match, "latSuf"),
                    TryGetValue(match, "latDeg"),
                    TryGetValue(match, "latMin"),
                    TryGetValue(match, "latSec"));

                Angle longitude = ParseAngle(
                      provider,
                      TryGetValue(match, "lonSuf"),
                      TryGetValue(match, "lonDeg"),
                      TryGetValue(match, "lonMin"),
                      TryGetValue(match, "lonSec"));

                if ((latitude == null) ||
                  (longitude == null) ||
                  (Math.Abs(latitude.TotalDegrees) > 90.0) ||
                  (Math.Abs(longitude.TotalDegrees) > 180.0))
                {
                    return null;
                }
                return latitude;
            }
            return null;
        }


        private static string TryGetValue(System.Text.RegularExpressions.Match match, string groupName)
        {
            var group = match.Groups[groupName];

            // Need to check that only a single capture occured, as the suffixes are used more than once
            if (group.Success && (group.Captures.Count == 1))
            {
                return group.Value;
            }
            return null;
        }
        private static Angle ParseAngle(IFormatProvider provider, string suffix, string degrees, string minutes = null, string seconds = null)
            {
            double degreeValue = 0;
            double minuteValue = 0;
            double secondValue = 0;

            // First try parsing the values (minutes and seconds are optional)
            if (!double.TryParse(degrees, NumberStyles.Float, provider, out degreeValue) ||
                ((minutes != null) && !double.TryParse(minutes, NumberStyles.Float, provider, out minuteValue)) ||
                ((seconds != null) && !double.TryParse(seconds, NumberStyles.Float, provider, out secondValue)))
            {
                return null;
            }

            // We've parsed all the information! Make everything the same
            // sign.
            minuteValue = Math.Abs(minuteValue);
            secondValue = Math.Abs(secondValue);

            // Check the suffix (takes priority over positive/negtive sign).
            if (!string.IsNullOrEmpty(suffix))
            {
                // Change degreeValue into a known sign
                degreeValue = Math.Abs(degreeValue);

                if (suffix.Equals("S", StringComparison.OrdinalIgnoreCase) ||
                    suffix.Equals("W", StringComparison.OrdinalIgnoreCase))
                {
                    return Angle.FromDegrees(-degreeValue, -minuteValue, -secondValue);
                }

                // Else assume it's N/E and return positive angles.
                return Angle.FromDegrees(degreeValue, minuteValue, secondValue);
            }

            // Check if we need to negate to match the degrees (if we type
            // "-6° 12.3'" we expect the whole thing to be negative).
            // We can't just check if degreeValue is negative, as we could
            // have "-0° 12.3'".
            var negativeSign = NumberFormatInfo.GetInstance(provider).NegativeSign;
            if (degrees.StartsWith(negativeSign, StringComparison.Ordinal))
            {
                minuteValue = -minuteValue;
                secondValue = -secondValue;
            }
            return Angle.FromDegrees(degreeValue, minuteValue, secondValue);
        }

    }
}
