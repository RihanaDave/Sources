using GPAS.PropertiesValidation.Geo.Formats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GPAS.PropertiesValidation.Tests
{
    [TestClass()]
    public class GeoSpecialFormatsTests
    {
        [TestMethod()]
        public void GeoSpecialConvertorTest()
        {
            //Assign
            GeoSpecialFormats geoTest = new GeoSpecialFormats();
            string[] testLocationsLatitudes =
                {
                "32° 13' 11.99\" N " ,
                "32° 13' 11.99\" S" ,
                "35° 44' 55.90\" N ",
                "40° 45' 36\" N",
                "40° 45' 36\" N  73° 59' 2.4\" W",
                "41 25N , 120 58W",
                "41 25N",
                "-31.96 , 115.84",
                "90°00'00\"N , 180°00'00\"W",
                "90°00'00\"S"
            };
            string[] testLocationsLongitudes =
                {
                "32° 13' 11.99\" E" ,
                "32° 13' 11.99\" W" ,
                "51° 30' 46.19\" E" ,
                "73° 59' 2.4\" W" ,
                "40° 45' 36\" N  73° 59' 2.4\" W" ,
                "41 25N , 120 58W" ,
                "120 58W" ,
                "-31.96 , 115.84" ,
                "90°00'00\"N , 180°00'00\"W" ,
                "180°00'00\"E"
            };
            
            GeoSpecialTypes[] types = {
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.CompoundDMS,
                GeoSpecialTypes.CompoundDMS,
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.DMS,
                GeoSpecialTypes.DMS
            };
            double[] expectedLatitudes = {
                32.22,
                -32.22,
                35.7488633,
                40.76,
                40.76,
                41.416,
                41.416,
                -31.96,
                90,
                -90
            };
            double[] expectedLongitudes = {
                32.22,
                -32.22,
                51.5128326,
                -73.984,
                -73.984,
                -120.9666,
                -120.9666,
                115.84,
                -180,
                180
            };
            bool[] expectedIsLatitude = {
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true
            };
            bool[] expectedIsLongitude = {
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true
            };
            double?[] latitude = new double?[testLocationsLongitudes.Length];
            double?[] longitude = new double?[testLocationsLongitudes.Length];
            bool[] isLatitude = new bool[testLocationsLongitudes.Length];
            bool[] isLongitude = new bool[testLocationsLongitudes.Length];

            //Act
            for (int i = 0; i < testLocationsLatitudes.Length; i++)
            {
                isLatitude[i] = geoTest.GeoSpecialConvertor(testLocationsLatitudes[i], GeoComponentType.Latitude, types[i] , out latitude[i]);
                isLongitude[i] = geoTest.GeoSpecialConvertor(testLocationsLongitudes[i], GeoComponentType.Longitude, types[i] , out longitude[i]);

            }

            //Assert

            for (int i = 0; i < testLocationsLatitudes.Length; i++)
            {
                Assert.IsTrue(Math.Abs((latitude[i].Value - expectedLatitudes[i])) < 0.001);
                Assert.IsTrue(Math.Abs((longitude[i].Value - expectedLongitudes[i])) < 0.001);
                Assert.AreEqual(isLatitude[i],expectedIsLatitude[i]);
                Assert.AreEqual(isLongitude[i],expectedIsLongitude[i]);
            }

        }
    }
}