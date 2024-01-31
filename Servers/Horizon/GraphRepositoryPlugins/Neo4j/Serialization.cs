using GPAS.Horizon.Logic;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j
{
    public static class Serialization
    {
        public static OntologyMaterial Ontology;

        public static string GetMd5HashCode(string str)
        {
            StringBuilder md5Hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(str));
            for (int i = 0; i < bytes.Length; i++)
            {
                md5Hash.Append(bytes[i].ToString("x2"));
            }

            return md5Hash.ToString();
        }

        public static string EncodePropertyValueToUseInStoreQuery(GraphRepositoryBaseDataTypes propertyBaseDataType,
            string propertyValue)
        {
            string retrievePropertyValue;
            switch (propertyBaseDataType)
            {
                case GraphRepositoryBaseDataTypes.String:
                case GraphRepositoryBaseDataTypes.HdfsURI:
                    retrievePropertyValue = $"\"{RemoveBadChar(propertyValue)}\"";
                    break;
                case GraphRepositoryBaseDataTypes.DateTime:
                    retrievePropertyValue = DateTimeOffset.Parse(propertyValue, CultureInfo.InvariantCulture,
                        DateTimeStyles.None).ToUnixTimeMilliseconds().ToString();
                    break;
                default:
                    retrievePropertyValue = propertyValue;
                    break;
            }

            return retrievePropertyValue;
        }

        public static string EncodePropertyValueToUseInRetrieveQuery(string propertyTypeURI, string propertyValue)
        {
            GraphRepositoryBaseDataTypes propertyBaseDataType = Ontology.GetBaseDataTypeOfProperty(propertyTypeURI);
            string retrievePropertyValue;
            switch (propertyBaseDataType)
            {
                case GraphRepositoryBaseDataTypes.String:
                case GraphRepositoryBaseDataTypes.HdfsURI:
                    retrievePropertyValue = $"'{RemoveBadChar(propertyValue)}'";
                    break;
                default:
                    retrievePropertyValue = propertyValue;
                    break;
            }

            return retrievePropertyValue;
        }

        public static string GetVertexTypeUriIndexName(string typeUri)
        {
            return string.Format("Index{0}", GetMd5HashCode(typeUri));
        }

        public static T DeserializJson<T>(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
                throw new ArgumentNullException(nameof(jsonString));

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
        }

        private static string RemoveBadChar(string inputString)
        {
            Regex pattern = new Regex(",'`\n\r\t\"");
            return pattern.Replace(inputString, " ");
        }
    }
}
