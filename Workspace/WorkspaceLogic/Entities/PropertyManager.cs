using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// مدیریت ویژگی ها در سمت محیط کاربری
    /// </summary>
    public class PropertyManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private PropertyManager()
        { }

        /// <summary>
        /// بازیابی ویژگی های یک شی
        /// </summary>
        /// <param name="objectToGetProperties">شئی که می خواهیم ویژگی های آن را بگیریم</param>
        /// <returns>مجموعه ی ویژگی های موجود برای شی در مخزن داده ها را برمی گرداند</returns>
        public async static Task<IEnumerable<KWProperty>> GetPropertiesOfObjectAsync(KWObject objectToGetProperties)
        {
            if (objectToGetProperties == null)
                throw new ArgumentNullException(nameof(objectToGetProperties));

            return await DataAccessManager.PropertyManager.GetPropertiesForObjectAsync
                (objectToGetProperties);
        }

        public static IEnumerable<KWProperty> GetUnpublishedPropertiesOfObject(KWObject objectToGetProperties)
        {
            if (objectToGetProperties == null)
                throw new ArgumentNullException(nameof(objectToGetProperties));

            return DataAccessManager.PropertyManager.GetUnpublishedPropertiesForObject(objectToGetProperties);
        }

        public static bool IsUnpublishedProperty(KWProperty propertyToCheck)
        {
            if (propertyToCheck == null)
                throw new ArgumentNullException(nameof(propertyToCheck));

            return DataAccessManager.PropertyManager.IsUnpublishedProperty(propertyToCheck.ID);
        }

        /// <summary>
        /// بازیابی ویژگی های یک شی
        /// </summary>
        /// <param name="objectToGetProperties">شئی که می خواهیم ویژگی های آن را بگیریم</param>
        /// <returns>مجموعه ی ویژگی های موجود برای شی در مخزن داده ها را برمی گرداند</returns>
        public async static Task<IEnumerable<KWProperty>> GetPropertiesOfObjectsAsync(IEnumerable<KWObject> objectsToGetTheirProperties,
            IEnumerable<string> propertyTypes)
        {
            if (objectsToGetTheirProperties == null)
                throw new ArgumentNullException(nameof(objectsToGetTheirProperties));
            if (propertyTypes == null)
                throw new ArgumentNullException(nameof(propertyTypes));

            return await DataAccessManager.PropertyManager.GetSpecifiedPropertiesOfObjectAsync
                (objectsToGetTheirProperties, propertyTypes);
        }

        /// <summary>
        /// ایجاد یک ویژگی جدید برای یک شی
        /// </summary>
        /// <param name="objectToAddProperty">شئی که می خواهیم برای آن ویژگی تعریف کنیم</param>
        /// <param name="propertyTypeUri">نوع ویژگی در هستان شناسی برای ویژگی جدید</param>
        /// <param name="propertyValue">مقدار برای ویژگی جدید؛ این مقدار باید براساس نوع پایه قابل استفاده برای نوع ویژگی تعیین شده باشد</param>
        /// <returns>نمونه ویژگی ایجاد شده در مخزن داده ها را برمی گرداند</returns>
        public static KWProperty CreateNewPropertyForObject(KWObject objectToAddProperty, string propertyTypeUri, string propertyValue)
        {
            if (objectToAddProperty == null)
                throw new ArgumentNullException(nameof(objectToAddProperty));
            if (propertyTypeUri == null)
                throw new ArgumentNullException(nameof(propertyTypeUri));
            if (propertyValue == null)
                throw new ArgumentNullException(nameof(propertyValue));

            if (string.IsNullOrWhiteSpace(propertyTypeUri))
                throw new ArgumentException("Invalid argument", nameof(propertyTypeUri));

            return DataAccessManager.PropertyManager.CreateNewProperty(propertyTypeUri, propertyValue, objectToAddProperty);
        }

        public static void DeletePropertyForObject(KWProperty propertyToDelete)
        {
            if (propertyToDelete == null)
                throw new ArgumentNullException(nameof(propertyToDelete));

            DataAccessManager.PropertyManager.DeleteProperty(propertyToDelete);
        }

        public static KWProperty EditPropertyOfObject(KWProperty propertyToEditProperty, object newPropertyValue)
        {
            if (propertyToEditProperty == null)
                throw new ArgumentNullException(nameof(propertyToEditProperty));
            if (newPropertyValue == null)
                throw new ArgumentNullException(nameof(newPropertyValue));

            return DataAccessManager.PropertyManager.UpdatePropertyValue
                (propertyToEditProperty
                , GetInvariantCultureStringOfObject(newPropertyValue));
        }

        public async static Task<IEnumerable<KWProperty>> RetriveProeprtiesByIdAsync(List<long> propertyIdsToRetrive)
        {
            if (propertyIdsToRetrive == null)
                throw new ArgumentNullException(nameof(propertyIdsToRetrive));

            return await DataAccessManager.PropertyManager.GetPropertyListByIdAsync(propertyIdsToRetrive);
        }

        public static ValidationProperty IsPropertyValid(BaseDataTypes propertyBaseType, string propertyValue)
        {
            return IsPropertyValid(propertyBaseType, propertyValue, CultureInfo.InvariantCulture);
        }

        public static ValidationProperty IsPropertyValid(BaseDataTypes propertyBaseType, string propertyValue, CultureInfo cultureInfo)
        {
            return IsPropertyValid(propertyBaseType, propertyValue, cultureInfo, null, null, null, null, null);
        }

        public static ValidationProperty IsPropertyValid(BaseDataTypes propertyBaseType, string propertyValue, CultureInfo cultureInfo,
            string dateTimeFormat)
        {
            return IsPropertyValid(propertyBaseType, propertyValue, cultureInfo, dateTimeFormat, null, null, null, null);
        }

        public static ValidationProperty IsPropertyValid(BaseDataTypes propertyBaseType, string propertyValue,
            CultureInfo cultureInfo, string dateTimeFormat,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            if (propertyValue == null)
                throw new ArgumentNullException(nameof(propertyValue));

            return ValueBaseValidation.IsValidPropertyValue(propertyBaseType, propertyValue, cultureInfo, dateTimeFormat,
                geotimeStartDateCultureInfo, geotimeStartDateStringFormat,
                geotimeEndDateCultureInfo, geotimeEndDateStringFormat);
        }

        public static ValidationProperty IsPropertyValid(KWProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            BaseDataTypes baseDataTypeOfProperty = OntologyProvider.GetBaseDataTypeOfProperty(property.TypeURI);
            return IsPropertyValid(baseDataTypeOfProperty, property.Value);
        }
        public static object ParsePropertyValue(BaseDataTypes propertyBaseType, string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return ValueBaseValidation.ParsePropertyValue(propertyBaseType, value);
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string value, out object parsedValue)
        {
            return TryParsePropertyValue(propertyBaseType, value, out parsedValue, CultureInfo.InvariantCulture);
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string value, out object parsedValue,
            CultureInfo cultureInfo)
        {
            return TryParsePropertyValue(propertyBaseType, value, out parsedValue, cultureInfo, null, null, null, null, null);
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string value, out object parsedValue,
            CultureInfo cultureInfo, string dateTimeFormat)
        {
            return TryParsePropertyValue(propertyBaseType, value, out parsedValue, cultureInfo, dateTimeFormat, null, null, null, null);
        }

        public static ValidationProperty TryParsePropertyValue(BaseDataTypes propertyBaseType, string value, out object parsedValue,
            CultureInfo cultureInfo, string dateTimeFormat,
            CultureInfo geotimeStartDateCultureInfo, string geotimeStartDateStringFormat,
            CultureInfo geotimeEndDateCultureInfo, string geotimeEndDateStringFormat)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            return ValueBaseValidation.TryParsePropertyValue(propertyBaseType, value, out parsedValue, cultureInfo, dateTimeFormat,
                geotimeStartDateCultureInfo, geotimeStartDateStringFormat, geotimeEndDateCultureInfo, geotimeEndDateStringFormat);
        }

        public static async Task<ValidationProperty> ValidateDateTimeFormat(string json, string url)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            string responseContent = string.Empty;
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using (var httpClient = new HttpClient())
            {
                var httpResponse = await httpClient.PostAsync(url, httpContent);
                if (httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync();
                }
            }
            
            return JsonConvert.DeserializeObject<ValidationProperty>(responseContent);
        }

        #region تبدیل های سمت محیط کاربری - سمت سرویس دهنده راه دور
        /// <summary>
        /// رشته معادل برای شئ داده شده را براساس فرهنگ ثابت (اینورینت) برمی گرداند؛
        /// نوع شئ های قابل تبدیل: عدد صحیح، تاریخ-زمان و اعشاری می باشند
        /// </summary>
        private static string GetInvariantCultureStringOfObject(object valueToConvert)
        {
            // رشته معادل برای انواع سه گانه عدد صحیح، تاریخ-زمان و اعشاری داده شده را براساس فرهنگ ثابت (اینورینت) برمی گرداند
            if (valueToConvert.GetType() == typeof(long))
                return ((long)valueToConvert).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
            else if (valueToConvert.GetType() == typeof(DateTime))
                return ((DateTime)valueToConvert).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
            else if (valueToConvert.GetType() == typeof(double))
                return ((double)valueToConvert).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
            // و برای دیگر انواع، به صورت معمول، رشته معادل شئ را برمی گرداند
            else
                return valueToConvert.ToString();
        }

        internal static bool IsPropertyModified(KWProperty kwProperty)
        {
            return DataAccessManager.PropertyManager.IsModifiedProperty(kwProperty);
        }
        #endregion
    }
}
