using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class DateTimeConfiguration : BaseModel
    {
        private string stringFormat;
        public string StringFormat
        {
            get => stringFormat;
            set
            {
                if (SetValue(ref stringFormat, value))
                {
                    SetStringFormatWithTimeZone();
                    OnStringFormatChanged();
                }
            }
        }

        private string stringFormatWithTimeZone;
        [XmlIgnore]
        public string StringFormatWithTimeZone
        {
            get => stringFormatWithTimeZone;
            protected set
            {
                SetValue(ref stringFormatWithTimeZone, value);
            }
        }

        private TimeZoneInfo timeZone;
        [XmlIgnore]
        public TimeZoneInfo TimeZone
        {
            get => timeZone;
            set
            {
                if (SetValue(ref timeZone, value))
                {
                    TimeZoneId = TimeZone == null ? DefaultTimeZone.Id : TimeZone.Id;
                    TimeZoneValue = long.Parse(TimeZone.BaseUtcOffset.TotalMilliseconds.ToString());
                    SetStringFormatWithTimeZone();
                    OnTimeZoneChanged();
                }
            }
        }

        private string timeZoneId;
        public string TimeZoneId
        {
            get => timeZoneId;
            set
            {
                if (SetValue(ref timeZoneId, value))
                {
                    TimeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.Id.Equals(TimeZoneId));
                }
            }
        }        

        private long timeZoneValue;
        public long TimeZoneValue
        {
            get => timeZoneValue;
            set => SetValue(ref timeZoneValue, value);
        }

        private CultureInfo culture;
        [XmlIgnore]
        public CultureInfo Culture
        {
            get => culture;
            set
            {
                if (SetValue(ref culture, value))
                {
                    CultureName = Culture.Name;
                    OnCultureChanged();
                }
            }
        }

        private string cultureName;
        public string CultureName
        {
            get => cultureName;
            set
            {
                if (SetValue(ref cultureName, value))
                {
                    Culture = CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.Name.Equals(CultureName));
                }
            }
        }

        [XmlIgnore]
        public static CultureInfo DefaultCulture { get; protected set; } = CultureInfo.GetCultureInfo("en");

        [XmlIgnore]
        public static string DefaultStringFormat { get; protected set; } = CultureInfo.GetCultureInfo("en").DateTimeFormat.ShortDatePattern;

        [XmlIgnore]
        public static TimeZoneInfo DefaultTimeZone { get; protected set; } = TimeZoneInfo.Utc;

        [XmlIgnore]
        public static long DefaultTimeZoneValue { get; protected set; } = long.Parse(DefaultTimeZone.BaseUtcOffset.TotalMilliseconds.ToString());

        public DateTimeConfiguration()
        {
            Culture = DefaultCulture;
            StringFormat = DefaultStringFormat;
            TimeZone = DefaultTimeZone;
        }

        private void SetStringFormatWithTimeZone()
        {
            TimeZoneInfo timeZone = TimeZone;
            if(timeZone == null)
                timeZone = DefaultTimeZone;

            StringFormatWithTimeZone = StringFormat + (timeZone.BaseUtcOffset.Ticks == 0 ? string.Empty : " zzz");
        }

        public static string GetStringValueTimeZone(TimeZoneInfo timeZone)
        {
            if (timeZone == null)
                timeZone = DefaultTimeZone;

            string sign = timeZone.BaseUtcOffset.Hours >= 0 ? "+" : "-";
            return sign + AddZeroIfNeeded(timeZone.BaseUtcOffset.Hours) + ":" + AddZeroIfNeeded(timeZone.BaseUtcOffset.Minutes);
        }

        private static string AddZeroIfNeeded(int number)
        {
            int absNumber = Math.Abs(number);
            return absNumber < 10 ? "0" + absNumber : absNumber.ToString();
        }

        public event EventHandler StringFormatChanged;
        protected void OnStringFormatChanged()
        {
            StringFormatChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CultureChanged;
        protected void OnCultureChanged()
        {
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TimeZoneChanged;
        protected void OnTimeZoneChanged()
        {
            TimeZoneChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}