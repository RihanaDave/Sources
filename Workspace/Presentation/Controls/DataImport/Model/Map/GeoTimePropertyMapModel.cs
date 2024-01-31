using GPAS.PropertiesValidation;
using GPAS.PropertiesValidation.Geo.Formats;
using GPAS.Workspace.Logic;
using System;
using System.Linq;
using System.Globalization;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class GeoTimePropertyMapModel : MultiPropertyMapModel
    {
        string geoPointTypeUri = "GeoPoint";
        string startDateTypeUri = "StartDate";
        string endDateTypeUri = "EndDate";

        GeoPointPropertyMapModel geoPoint = null;
        [XmlIgnore]
        public GeoPointPropertyMapModel GeoPoint
        {
            get => geoPoint;
            protected set
            {
                if (SetValue(ref geoPoint, value))
                {
                    SetValidationForSampleValue();
                }
            }
        }

        DateTimePropertyMapModel startDate = null;
        [XmlIgnore]
        public DateTimePropertyMapModel StartDate
        {
            get => startDate;
            protected set
            {
                if (SetValue(ref startDate, value))
                {
                    SetValidationForSampleValue();
                }
            }
        }

        DateTimePropertyMapModel endDate = null;
        [XmlIgnore]
        public DateTimePropertyMapModel EndDate
        {
            get => endDate;
            protected set
            {
                if (SetValue(ref endDate, value))
                {
                    SetValidationForSampleValue();
                }
            }
        }

        double? latConvertedDoubleValue;
        double? longConvertedDoubleValue;

        public GeoTimePropertyMapModel()
        {
            DataType = Ontology.BaseDataTypes.GeoTime;

            GeoPoint = new GeoPointPropertyMapModel()
            {
                Title = "GeoPoint",
                TypeUri = geoPointTypeUri,
                IsResolvable = false,
                OwnerObject = OwnerObject,
                ParentProperty = this,
            };

            StartDate = new DateTimePropertyMapModel
            {
                Title = "Start Date",
                TypeUri = startDateTypeUri,
                DataType = Ontology.BaseDataTypes.DateTime,
                IsResolvable = false,
                OwnerObject = OwnerObject,
                ParentProperty = this,

            };

            EndDate = new DateTimePropertyMapModel
            {
                Title = "End Date",
                TypeUri = endDateTypeUri,
                DataType = Ontology.BaseDataTypes.DateTime,
                IsResolvable = false,
                OwnerObject = OwnerObject,
                ParentProperty = this,
            };

            InnerProperties.Add(GeoPoint);
            InnerProperties.Add(StartDate);
            InnerProperties.Add(EndDate);
        }

        protected override void SetValidationForSampleValue()
        {
            if (GeoPoint != null && StartDate != null && EndDate != null)
            {
                bool isCompareGeoTime = CompareGeoTimeValue();
                var tempValidation = PropertyManager.IsPropertyValid(DataType, ToJson(),
                    CultureInfo.CurrentCulture, null,
                    StartDate.Configuration.Culture,
                    StartDate.Configuration.StringFormat,
                    EndDate.Configuration.Culture,
                    EndDate.Configuration.StringFormat);

                if (isCompareGeoTime && tempValidation.Status != ValidationStatus.Invalid)
                {
                    SampleValueValidationStatus = tempValidation;
                }
                else
                {
                    tempValidation.Status = ValidationStatus.Invalid;
                    SampleValueValidationStatus = tempValidation;
                }
            }
        }

        private bool CompareGeoTimeValue()
        {
            GeoSpecialFormats convertor = new GeoSpecialFormats();
            GeoSpecialTypes geoSpecial;
            switch (GeoPoint.Format)
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

            if (convertor.GeoSpecialConvertor(GeoPoint.Latitude.SampleValue, GeoComponentType.Latitude, geoSpecial, out latConvertedDoubleValue) &&
                convertor.GeoSpecialConvertor(GeoPoint.Longitude.SampleValue, GeoComponentType.Longitude, geoSpecial, out longConvertedDoubleValue))
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
            string sDate = StartDate == null ? string.Empty : StartDate.SampleValue;
            string eDate = EndDate == null ? string.Empty : EndDate.SampleValue;

            return $"{{ \"Latitude\":\"{lat}\" , \"Longitude\":\"{lng}\" , \"TimeBegin\":\"{sDate}\" , \"TimeEnd\":\"{eDate}\"}}";
        }

        protected override void AfterInnerPropertiesChanged()
        {
            base.AfterInnerPropertiesChanged();
            if (InnerProperties == null)
            {
                GeoPoint = null;
                StartDate = null;
                EndDate = null;
            }
            else
            {
                var gp = InnerProperties.OfType<GeoPointPropertyMapModel>().FirstOrDefault(ip => ip.TypeUri == geoPointTypeUri);
                var sd = InnerProperties.OfType<DateTimePropertyMapModel>().FirstOrDefault(ip => ip.TypeUri == startDateTypeUri);
                var ed = InnerProperties.OfType<DateTimePropertyMapModel>().FirstOrDefault(ip => ip.TypeUri == endDateTypeUri);

                if (gp != null)
                    GeoPoint = gp;

                if (sd != null)
                    StartDate = sd;

                if (ed != null)
                    EndDate = ed;
            }
        }
    }
}
