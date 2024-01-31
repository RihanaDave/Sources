using GPAS.PropertiesValidation;
using GPAS.PropertiesValidation.Geo.Formats;
using GPAS.Workspace.Logic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    public class GeoPointPropertyMapModel : MultiPropertyMapModel
    {
        string latitudeTypeUri = "Latitude";
        string longitudeTypeUri = "Longitude";

        SinglePropertyMapModel latitude = null;
        [XmlIgnore]
        public SinglePropertyMapModel Latitude
        {
            get => latitude;
            protected set
            {
                if (SetValue(ref latitude, value))
                {
                    SetValidationForSampleValue();
                }
            }
        }

        SinglePropertyMapModel longitude = null;
        [XmlIgnore]
        public SinglePropertyMapModel Longitude
        {
            get => longitude;
            protected set
            {
                if (SetValue(ref longitude, value))
                {
                    SetValidationForSampleValue();
                }
            }
        }

        double? latConvertedDoubleValue;
        double? longConvertedDoubleValue;

        public GeoPointPropertyMapModel()
        {
            DataType = Ontology.BaseDataTypes.GeoPoint;

            Latitude = new SinglePropertyMapModel
            {
                Title = "Latitude",
                TypeUri = latitudeTypeUri,
                DataType = Ontology.BaseDataTypes.String,
                IsResolvable = false,
                OwnerObject = OwnerObject,
                ParentProperty = this,
            };

            Longitude = new SinglePropertyMapModel
            {
                Title = "Longitude",
                TypeUri = longitudeTypeUri,
                DataType = Ontology.BaseDataTypes.String,
                IsResolvable = false,
                OwnerObject = OwnerObject,
                ParentProperty = this,
            };

            InnerProperties.Add(Latitude);
            InnerProperties.Add(Longitude);
        }

        GeoTimeFormat format = GeoTimeFormat.CompoundDMS;
        public GeoTimeFormat Format
        {
            get => format;
            set
            {
                if (SetValue(ref format, value))
                {
                    SetValidationForSampleValue();
                    OnScenarioChanged();
                }
            }
        }

        protected override void SetValidationForSampleValue()
        {
            if (Latitude != null && Longitude != null)
            {
                bool isCompareGeoTime = CompareGeoPointValue();
                ValidationProperty validation = PropertyManager.IsPropertyValid(DataType, ToJson(), CultureInfo.CurrentCulture);

                if (isCompareGeoTime && validation.Status != ValidationStatus.Invalid)
                {
                    SampleValueValidationStatus = validation;
                }
                else
                {
                    validation.Status = ValidationStatus.Invalid;
                    SampleValueValidationStatus = validation;
                }
            }
        }

        private bool CompareGeoPointValue()
        {
            GeoSpecialFormats convertor = new GeoSpecialFormats();
            GeoSpecialTypes geoSpecial;
            switch (Format)
            {
                case GeoTimeFormat.DMS:
                    geoSpecial = GeoSpecialTypes.DMS;
                    break;
                case GeoTimeFormat.Decimal:
                    geoSpecial = GeoSpecialTypes.Decimal;
                    break;
                case GeoTimeFormat.CompoundDMS:
                    geoSpecial = GeoSpecialTypes.CompoundDMS;
                    break;
                case GeoTimeFormat.CompoundDecimal:
                    geoSpecial = GeoSpecialTypes.CompoundDecimal;
                    break;
                default:
                    geoSpecial = GeoSpecialTypes.DMS;
                    break;
            }

            if (convertor.GeoSpecialConvertor(Latitude.SampleValue, GeoComponentType.Latitude, geoSpecial, out latConvertedDoubleValue) &&
                convertor.GeoSpecialConvertor(Longitude.SampleValue, GeoComponentType.Longitude, geoSpecial, out longConvertedDoubleValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ToJson()
        {
            string lat = latConvertedDoubleValue == null ? string.Empty : latConvertedDoubleValue.ToString();
            string lng = longConvertedDoubleValue == null ? string.Empty : longConvertedDoubleValue.ToString();

            return $"{{ \"Latitude\":\"{lat}\" , \"Longitude\":\"{lng}\"}}";
        }

        protected override void AfterInnerPropertiesChanged()
        {
            base.AfterInnerPropertiesChanged();
            if (InnerProperties == null)
            {
                Latitude = null;
                Longitude = null;
            }
            else
            {
                var lat = InnerProperties.OfType<SinglePropertyMapModel>().FirstOrDefault(ip => ip.TypeUri == latitudeTypeUri);
                var lng = InnerProperties.OfType<SinglePropertyMapModel>().FirstOrDefault(ip => ip.TypeUri == longitudeTypeUri);

                if (lat != null)
                    Latitude = lat;

                if (lng != null)
                    Longitude = lng;
            }
        }
    }
}
