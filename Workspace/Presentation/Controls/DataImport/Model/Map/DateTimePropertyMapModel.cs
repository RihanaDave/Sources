using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Windows.DataImport;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Configuration;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class DateTimePropertyMapModel : SinglePropertyMapModel
    {
        private DateTimeConfiguration configuration;
        public DateTimeConfiguration Configuration
        {
            get => configuration;
            set
            {
                if (SetValue(ref configuration, value))
                {
                    Configuration.CultureChanged -= ConfigurationOnCultureChanged;
                    Configuration.CultureChanged += ConfigurationOnCultureChanged;

                    Configuration.StringFormatChanged -= ConfigurationOnStringFormatChanged;
                    Configuration.StringFormatChanged += ConfigurationOnStringFormatChanged;

                    Configuration.TimeZoneChanged -= Configuration_TimeZoneChanged;
                    Configuration.TimeZoneChanged += Configuration_TimeZoneChanged;

                    SetHasDefaultConfig();
                    SetSampleValueWithTimeZone();
                    SetValidationForSampleValue();
                    OnScenarioChanged();
                }
            }
        }

        bool hasDefaultConfig = true;
        [XmlIgnore]
        public bool HasDefaultConfig
        {
            get => hasDefaultConfig;
            set => SetValue(ref hasDefaultConfig, value);
        }

        private string sampleValueWithTimeZone;
        [XmlIgnore]
        public string SampleValueWithTimeZone
        {
            get => sampleValueWithTimeZone;
            protected set
            {
                SetValue(ref sampleValueWithTimeZone, value);
            }
        }

        public DateTimePropertyMapModel()
        {
            DataType = Ontology.BaseDataTypes.DateTime;
            Configuration = new DateTimeConfiguration();
            SetHasDefaultConfig();
            SetSampleValueWithTimeZone();
            SetValidationForSampleValue();
        }

        private void SetHasDefaultConfig()
        {
            HasDefaultConfig = Configuration == null ||
                (Configuration.CultureName == DateTimeConfiguration.DefaultCulture.Name &&
                Configuration.StringFormat == DateTimeConfiguration.DefaultStringFormat &&
                Configuration.TimeZoneId == DateTimeConfiguration.DefaultTimeZone.Id);
        }

        private void ConfigurationOnCultureChanged(object sender, EventArgs e)
        {
            SetHasDefaultConfig();
            SetValidationForSampleValue();
            OnScenarioChanged();
        }

        private void ConfigurationOnStringFormatChanged(object sender, EventArgs e)
        {
            SetHasDefaultConfig();
            SetValidationForSampleValue();
            OnScenarioChanged();
        }

        private void Configuration_TimeZoneChanged(object sender, EventArgs e)
        {
            SetHasDefaultConfig();
            SetSampleValueWithTimeZone();
            SetValidationForSampleValue();
            OnScenarioChanged();
        }

        private void SetSampleValueWithTimeZone()
        {
            TimeZoneInfo timeZone = DateTimeConfiguration.DefaultTimeZone;
            if (!HasDefaultConfig)
                timeZone = Configuration.TimeZone;

            SampleValueWithTimeZone = SampleValue + (timeZone.BaseUtcOffset.Ticks == 0 ? string.Empty : " " +
                DateTimeConfiguration.GetStringValueTimeZone(timeZone));
        }

        protected async override void SetValidationForSampleValue()
        {
            if (DataType == Ontology.BaseDataTypes.None)
                return;

            DateTimeValidationModel validationModel;
            if (HasDefaultConfig)
            {
                validationModel = new DateTimeValidationModel()
                {
                    DateTimeValue = SampleValue,
                    StringFormat = DateTimeConfiguration.DefaultStringFormat,
                    TimeZoneTimestamp = DateTimeConfiguration.DefaultTimeZoneValue,
                    CalenderType = CalendarType.Gregorian
                };
            }
            else
            {
                validationModel = new DateTimeValidationModel()
                {
                    DateTimeValue = SampleValue,
                    StringFormat = Configuration.StringFormat,
                    TimeZoneTimestamp = Configuration.TimeZoneValue
                };

                switch (Configuration.CultureName)
                {
                    case "fa":
                        validationModel.CalenderType = CalendarType.Jalali;
                        break;
                    case "ar":
                        validationModel.CalenderType = CalendarType.Hijri;
                        break;
                    case "en":
                        validationModel.CalenderType = CalendarType.Gregorian;
                        break;
                }
            }

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };

            string url = ConfigurationManager.AppSettings["ValidationMicroServiceUrl"];
            string json = JsonConvert.SerializeObject(validationModel, jsonSerializerSettings);
            SampleValueValidationStatus = await PropertyManager.ValidateDateTimeFormat(json, url);
        }

        protected override void AfterSampleValueChanged()
        {
            SetSampleValueWithTimeZone();
            SetValidationForSampleValue();
        }
    }
}
