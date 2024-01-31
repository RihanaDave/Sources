using System;
using System.Globalization;
using System.Linq;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [Serializable]
    public class DateTimeTableColumnMappingItem : TableColumnMappingItem
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        private DateTimeTableColumnMappingItem()
            : base()
        { }

        public DateTimeTableColumnMappingItem(int columnIndex
                , string dateTimeCultureName, DateTimeStyles dateTimeStyles = DefaultDateTimeStyles, string columnTitle = "", PropertyInternalResolutionOption resolutionOption = PropertyInternalResolutionOption.Ignorable
)
            : base(columnIndex, columnTitle, resolutionOption)
        {
            DateTimeCultureName = dateTimeCultureName;
            DateTimeStyles = dateTimeStyles;
        }

        public string DateTimeCultureName { get; set; }
        public DateTimeStyles DateTimeStyles { get; set; }

        public static readonly string DefaultDateTimeCultureName = CultureInfo.InvariantCulture.Name;
        private const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        public const DateTimeStyles DefaultDateTimeStyles = DateTimeStyles.None;

        public static bool ParseDateTimeValue(string value, string cultureName, DateTimeStyles styles, out string invariantCultureValue)
        {
            bool result = false;
            CultureInfo culture = CultureInfo.CreateSpecificCulture(cultureName);
            DateTime parsedValue;
            if (DateTime.TryParse(value, culture, styles, out parsedValue))
            {
                invariantCultureValue = parsedValue.ToString(DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                result = true;
            }
            else
            {
                invariantCultureValue = value;
                result = false;
            }
            return result;
        }
        public static bool ParseDateTimeValue(string value, string cultureName, string formatIdentifier, DateTimeStyles styles, out string invariantCultureValue)
        {
            return ParseDateTimeValue(value, cultureName, styles, out invariantCultureValue);
        }
        public static string GetDateTimePatternsDescription(string cultureName)
        {
            CultureInfo culture = CultureInfo.CreateSpecificCulture(cultureName);
            string[] patterns = culture.DateTimeFormat.GetAllDateTimePatterns();
            string dateTimePatternSample = string.Empty;
            foreach (var currentPattern in patterns)
            {
                if (currentPattern.Equals(patterns.Last()))
                {
                    dateTimePatternSample = dateTimePatternSample + currentPattern;
                }
                else
                {
                    dateTimePatternSample = dateTimePatternSample + currentPattern + "\n";
                }
            }
            return dateTimePatternSample;
        }

        private static string[] GetDateTimePatterns(char dateTimeFormat, CultureInfo culture)
        {
            DateTimeFormatInfo dtfi = DateTimeFormatInfo.GetInstance(culture);
            string[] patterns = dtfi.GetAllDateTimePatterns(dateTimeFormat);
            return patterns;
        }
    }
}
