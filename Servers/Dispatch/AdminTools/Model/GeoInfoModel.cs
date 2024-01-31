using System;
using System.Globalization;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class GeoInfoModel : BaseModel
    {
        private string ip;
        public string Ip
        {
            get => ip;
            set
            {
                ip = value;
                OnPropertyChanged();
            }
        }

        private string longitude;
        public string Longitude
        {
            get => longitude;
            set
            {
                longitude = value;
                OnPropertyChanged();
            }
        }

        private string latitude;
        public string Latitude
        {
            get => latitude;
            set
            {
                latitude = value;
                OnPropertyChanged();
            }
        }

        private string geoFilePath;
        public string GeoFilePath
        {
            get => geoFilePath;
            set
            {
                geoFilePath = value;
                OnPropertyChanged();
            }
        }

        private string importGeoCorrectSummery;
        public string ImportGeoCorrectSummery
        {
            get => importGeoCorrectSummery;
            set
            {
                importGeoCorrectSummery = value;
                OnPropertyChanged();
            }
        }

        private string importGeoInCorrectSummery;
        public string ImportGeoInCorrectSummery
        {
            get => importGeoInCorrectSummery;
            set
            {
                importGeoInCorrectSummery = value;
                OnPropertyChanged();
            }
        }
    }
}
