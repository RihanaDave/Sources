using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.GeographicalStaticLocation;
using Microsoft.VisualBasic.FileIO;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class IpToGeoSpacialViewModel : BaseViewModel
    {
        public IpToGeoSpacialViewModel()
        {
            GeoInfoModel = new GeoInfoModel();
        }

        public GeoInfoModel GeoInfoModel { get; set; }

        public async Task<bool> AddGeoSpecialInformation()
        {
            GeoInfoModel.ImportGeoCorrectSummery = string.Empty;
            GeoInfoModel.ImportGeoInCorrectSummery = string.Empty;

            bool result = false;

            var longitude = double.Parse(GeoInfoModel.Longitude, NumberStyles.Float, CultureInfo.CurrentCulture);
            var latitude = double.Parse(GeoInfoModel.Latitude, NumberStyles.Float, CultureInfo.CurrentCulture);

            await Task.Run(() =>
            {
                GeographicalLocationAccess geoSpecialInformationManager = new GeographicalLocationAccess();
                result = geoSpecialInformationManager.InsertGeoSpecialInformationBasedOnIP(GeoInfoModel.Ip, latitude, longitude);
            });

            return result;
        }

        public async Task ImportGeoSpacialFile()
        {
            long successfullyAddedRowCount = 0;
            long incorrectRowCount = 0;

            GeoInfoModel.ImportGeoCorrectSummery = string.Empty;
            GeoInfoModel.ImportGeoInCorrectSummery = string.Empty;

            await Task.Run(() =>
            {
                using (TextFieldParser parser = new TextFieldParser(GeoInfoModel.GeoFilePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        //Processing row
                        string[] fields = parser.ReadFields();
                        if (fields != null && fields.Length >= 3 &&
                            fields[0].Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Length == 4 &&
                            IPAddress.TryParse(fields[0], out _) && double.TryParse(fields[1], out var latitude) &&
                            double.TryParse(fields[2], out var longitude) && latitude <= 90.0 && latitude >= -90.0 &&
                            longitude <= 180.0 && longitude >= -180.0)
                        {
                            GeographicalLocationAccess geoSpecialInformationManager = new GeographicalLocationAccess();
                            try
                            {
                                geoSpecialInformationManager.InsertGeoSpecialInformationBasedOnIP(fields[0], latitude,
                                    longitude);
                                successfullyAddedRowCount++;
                            }
                            catch
                            {
                                incorrectRowCount++;
                            }
                        }
                        else
                        {
                            incorrectRowCount++;
                        }
                    }
                }

                GeoInfoModel.ImportGeoCorrectSummery = $"{successfullyAddedRowCount} Lines Have Been Imported";
                GeoInfoModel.ImportGeoInCorrectSummery = $"{incorrectRowCount} Incorrect Formatted Lines Have Been Detected";
            });
        }
    }
}
