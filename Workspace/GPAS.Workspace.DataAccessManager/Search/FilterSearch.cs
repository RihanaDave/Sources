using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager.Search
{
    public class FilterSearch
    {
        private FilterSearch() { }
        public static async Task<IEnumerable<KWObject>> GetFilterSearchResult(Query filterSearchQuery, int count)
        {
            if (filterSearchQuery == null)
                throw new ArgumentNullException("filterSearchQuery");

            HashSet<KWObject> filterSearchResult = new HashSet<KWObject>();

            IEnumerable<KWObject> unpublishedObjectsForFilterSearch = GetUnpublishedObjectsForFilterSearch();

            IEnumerable<KWObject> cacheResultsOnObject = await ApplyFilterOnAsync(unpublishedObjectsForFilterSearch, filterSearchQuery);
            foreach (var item in cacheResultsOnObject)
            {
                if (!filterSearchResult.Contains(item))
                {
                    filterSearchResult.Add(item);
                }
            }

            if (filterSearchResult.Count < count)
            {
                IEnumerable<KWObject> serverResults = await PerformFilterSearchAsync(filterSearchQuery, count - cacheResultsOnObject.Count());
                foreach (var item in serverResults)
                {
                    if (!filterSearchResult.Contains(item))
                    {
                        filterSearchResult.Add(item);
                    }
                }
            }

            return filterSearchResult;
        }
        private static IEnumerable<KWObject> GetUnpublishedObjectsForFilterSearch()
        {
            HashSet<KWObject> result = new HashSet<KWObject>();

            IEnumerable<KWObject> unpublishedObjects = ObjectManager.GetLocallyChangedObjects();

            foreach (var item in unpublishedObjects)
            {
                if (!result.Contains(item))
                {
                    result.Add(item);
                }
            }

            IEnumerable<KWProperty> unpublishedProperties = PropertyManager.GetLocallyChangedProperties();

            foreach (var item in unpublishedProperties.Select(up => up.Owner))
            {
                if (!result.Contains(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static bool IsPropertySatisfyPropertyValueCriteria(KWProperty property, GPAS.FilterSearch.PropertyValueCriteria criteria)
        {
            if (property.TypeURI != criteria.PropertyTypeUri)
                throw new InvalidOperationException("Property and Criteria types are not match");

            BaseDataTypes propertyBaseType
                = System.GetOntology().GetBaseDataTypeOfProperty(criteria.PropertyTypeUri);

            if (!PropertyManager.IsPropertyValid(propertyBaseType, property.Value))
                return false;

            switch (propertyBaseType)
            {
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    {
                        long propertyValueFact = (long)PropertyManager.ParsePropertyValue
                            (propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as LongPropertyCriteriaOperatorValuePair;
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return propertyValueFact == operatorValuePair.CriteriaValue;
                            case LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                                return propertyValueFact > operatorValuePair.CriteriaValue;
                            case LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                                return propertyValueFact < operatorValuePair.CriteriaValue;
                            case LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                                return propertyValueFact >= operatorValuePair.CriteriaValue;
                            case LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                                return propertyValueFact <= operatorValuePair.CriteriaValue;
                            case LongPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return propertyValueFact != operatorValuePair.CriteriaValue;
                            default:
                                throw new InvalidOperationException("Unknown integer relational operator");
                        }
                    }
                case BaseDataTypes.Boolean:
                    {
                        bool propertyValueFact = (bool)PropertyManager.ParsePropertyValue
                            (propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as BooleanPropertyCriteriaOperatorValuePair;
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return propertyValueFact == operatorValuePair.CriteriaValue;
                            case BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return propertyValueFact != operatorValuePair.CriteriaValue;
                            default:
                                throw new InvalidOperationException("Unknown boolean relational operator");
                        }
                    }
                case BaseDataTypes.GeoPoint:
                    {
                        GeoLocationEntity propertyValueFact = 
                            (GeoLocationEntity)PropertyManager.ParsePropertyValue(propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as GeoPointPropertyCriteriaOperatorValuePair;

                        bool isPointInsideGeoShape = IsPointInsideCircle(propertyValueFact, operatorValuePair.CriteriaValue);
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return isPointInsideGeoShape;
                            case GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return !isPointInsideGeoShape;
                            default:
                                throw new InvalidOperationException("Unknown geo-point relational operator");
                        }
                    }
                case BaseDataTypes.DateTime:
                    {
                        DateTime propertyValueFact = (DateTime)PropertyManager.ParsePropertyValue
                            (propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as DateTimePropertyCriteriaOperatorValuePair;
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return propertyValueFact == operatorValuePair.CriteriaValue;
                            case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                                return propertyValueFact > operatorValuePair.CriteriaValue;
                            case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                                return propertyValueFact < operatorValuePair.CriteriaValue;
                            case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                                return propertyValueFact >= operatorValuePair.CriteriaValue;
                            case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                                return propertyValueFact <= operatorValuePair.CriteriaValue;
                            case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return propertyValueFact != operatorValuePair.CriteriaValue;
                            default:
                                throw new InvalidOperationException("Unknown date-time relational operator");
                        }
                    }
                case BaseDataTypes.String:
                    {
                        string propertyValueFact = (string)PropertyManager.ParsePropertyValue
                            (propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as StringPropertyCriteriaOperatorValuePair;
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return propertyValueFact.Equals(operatorValuePair.CriteriaValue);
                            case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return !(propertyValueFact.Equals(operatorValuePair.CriteriaValue));
                            case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                                return LikeOperator.LikeString
                                    (propertyValueFact.ToLowerInvariant()
                                    , string.Format("*{0}*", operatorValuePair.CriteriaValue.ToLowerInvariant())
                                    , Microsoft.VisualBasic.CompareMethod.Text);
                            default:
                                throw new InvalidOperationException("Unknown string relational operator");
                        }
                    }
                case BaseDataTypes.Double:
                    {
                        double propertyValueFact = (double)PropertyManager.ParsePropertyValue
                            (propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as FloatPropertyCriteriaOperatorValuePair;
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return propertyValueFact == operatorValuePair.CriteriaValue;
                            case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                                return propertyValueFact > operatorValuePair.CriteriaValue;
                            case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                                return propertyValueFact < operatorValuePair.CriteriaValue;
                            case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                                return propertyValueFact >= operatorValuePair.CriteriaValue;
                            case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                                return propertyValueFact <= operatorValuePair.CriteriaValue;
                            case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return propertyValueFact != operatorValuePair.CriteriaValue;
                            default:
                                throw new InvalidOperationException("Unknown float relational operator");
                        }
                    }
                case BaseDataTypes.HdfsURI:
                    {
                        Uri propertyValueFact = (Uri)PropertyManager.ParsePropertyValue
                            (propertyBaseType, property.Value);
                        var operatorValuePair = criteria.OperatorValuePair as StringPropertyCriteriaOperatorValuePair;
                        switch (operatorValuePair.CriteriaOperator)
                        {
                            case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                                return propertyValueFact.ToString().Equals(operatorValuePair.CriteriaValue);
                            case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                                return !(propertyValueFact.ToString().Equals(operatorValuePair.CriteriaValue));
                            case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                                return LikeOperator.LikeString
                                    (propertyValueFact.ToString().ToLowerInvariant()
                                    , string.Format("*{0}*", operatorValuePair.CriteriaValue.ToLowerInvariant())
                                    , Microsoft.VisualBasic.CompareMethod.Text);
                            default:
                                throw new InvalidOperationException("Unknown string relational operator (to compair HDFS URI)");
                        }
                    }
                case BaseDataTypes.None:
                    return false;
                default:
                    throw new InvalidOperationException("Unknown property base type");
            }
        }

        private static bool IsPointInsideCircle(GeoLocationEntity point, GeoCircleEntityRawData circle)
        {
            GeoLocationEntity centerPoint = new GeoLocationEntity()
            {
                Latitude = double.Parse(circle.Latitude),
                Longitude = double.Parse(circle.Longitude)
            };
            return distanceBetweenGeoLocationByMeter(point, centerPoint) <= double.Parse(circle.Radius);
        }

        private static double degreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private static double distanceBetweenGeoLocationByMeter(GeoLocationEntity point1, GeoLocationEntity point2)
        {
            double earthRadius = 6378137;

            double dLat = degreesToRadians(point2.Latitude - point1.Latitude);
            double dLng = degreesToRadians(point2.Longitude - point1.Longitude);

            double lat1 = degreesToRadians(point1.Latitude);
            double lat2 = degreesToRadians(point2.Latitude);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2) * Math.Cos(lat1) * Math.Cos(lat2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadius * c;
        }

        private static bool IsPropertySatisfyDateRangeCriteria(KWProperty property, DateRangeCriteria criteria)
        {
            var propertyBaseType = System.GetOntology().GetBaseDataTypeOfProperty(property.TypeURI);
            if (propertyBaseType != BaseDataTypes.DateTime)
                throw new InvalidOperationException("Property type is not Date-Time base");

            DateTime propertyValueFact = (DateTime)PropertyManager.ParsePropertyValue(propertyBaseType, property.Value);
            return propertyValueFact >= DateTime.Parse(criteria.StartTime, CultureInfo.InvariantCulture)
                && propertyValueFact <= DateTime.Parse(criteria.EndTime, CultureInfo.InvariantCulture);
        }

        private static bool IsPropertySatisfyDateTimePropertyRangeCriteria(KWProperty property, DateTimePropertyRangeCriteria criteria)
        {
            BaseDataTypes propertyBaseType = System.GetOntology().GetBaseDataTypeOfProperty(property.TypeURI);

            if (!property.TypeURI.Equals(criteria.PropertyTypeUri))
                throw new InvalidOperationException("Property typeUri is not match.");

            if (propertyBaseType == BaseDataTypes.DateTime)
            {
                DateTime propertyValueFact = (DateTime)PropertyManager.ParsePropertyValue(propertyBaseType, property.Value);
                return propertyValueFact >= criteria.StartTime && propertyValueFact <= criteria.EndTime;
            }
            else if (propertyBaseType == BaseDataTypes.GeoTime)
            {
                GeoTimeEntity propertyValueFact = (GeoTimeEntity)PropertyManager.ParsePropertyValue(propertyBaseType, property.Value);
                return propertyValueFact.DateRange.Intersect(new TimeInterval() { TimeBegin = criteria.StartTime, TimeEnd = criteria.EndTime }) != null;
                //در صورتی که بین بازه ویژگی از نوع زمان و موقعیت جغرافیایی و بازه شرط اشتراکی وجود داشت مقدار true را بر می گرداند
            }
            else
            {
                throw new InvalidOperationException("Property type is not Date-Time or Geo-Time base.");
            }
        }

        private static bool IsPropertySatisfyKeywordCriteria(KWProperty property, KeywordCriteria criteria)
        {
            bool satisfy = false;
            if (PropertyManager.IsPropertyValid(property))
            {
                string keyword = criteria.Keyword.ToLowerInvariant();
                if (LikeOperator.LikeString(property.Value.ToLowerInvariant(), $"*{keyword}*", Microsoft.VisualBasic.CompareMethod.Text))
                {
                    satisfy = true;
                }
            }
            return satisfy;

        }
        private static bool IsDisplayNameAndKeywordMatched(KWObject objectToCheck, KeywordCriteria criteria)
        {
            bool result = false;
            if (objectToCheck.DisplayName != null)
            {
                string keyword = criteria.Keyword.ToLowerInvariant();
                if (LikeOperator.LikeString(objectToCheck.DisplayName.Value.ToLowerInvariant(), $"*{keyword}*", Microsoft.VisualBasic.CompareMethod.Text))
                {
                    result = true;
                }
            }
            return result;
        }

        private static async Task<IEnumerable<KWObject>> PerformFilterSearchAsync(Query filterSearchQuery, int count)
        {
            if (filterSearchQuery == null)
                throw new ArgumentNullException(nameof(filterSearchQuery));
            if (count == 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            Dispatch.Entities.Concepts.KObject[] kObjects = null;
            QuerySerializer serializer = new QuerySerializer();
            MemoryStream streamWriter = new MemoryStream();
            serializer.Serialize(streamWriter, filterSearchQuery);
            StreamUtility streamUtil = new StreamUtility();
            byte[] filterSearchQueryByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                kObjects = await sc.PerformFilterSearchAsync(filterSearchQueryByteArray, count);
            }
            finally
            {
                if (sc != null)
                {
                    sc.Close();
                }
            }

            return await ObjectManager.GetObjectsListByIdAsync(kObjects);
        }

        public static async Task<IEnumerable<KWObject>> ApplyFilterOnAsync(IEnumerable<KWObject> objectsToFilter, Query filterSearchQuery)
        {
            if (objectsToFilter == null)
                throw new ArgumentNullException("objectsToFilter");
            if (filterSearchQuery == null)
                throw new ArgumentNullException("filterSearchQuery");

            if (!objectsToFilter.Any() || filterSearchQuery.IsEmpty())
                return new List<KWObject>();

            return await ApplyFilterCriteriaSetOnObjectsAsync(objectsToFilter, filterSearchQuery.CriteriasSet);
        }

        private static async Task<IEnumerable<KWObject>> ApplyFilterCriteriaSetOnObjectsAsync(IEnumerable<KWObject> objectsToFilter, GPAS.FilterSearch.CriteriaSet searchQuery)
        {
            IEnumerable<KWObject> totalResultObjects = new List<KWObject>();

            bool isFirstCriteria = true;

            foreach (var currentCriteria in searchQuery.Criterias)
                if (currentCriteria is KeywordCriteria)
                    AppendCriteriaResultOnTotalResult
                        (searchQuery.SetOperator
                        , ref isFirstCriteria
                        , ref totalResultObjects
                        , await ApplyKeywordCriteriaAsync(objectsToFilter, currentCriteria as KeywordCriteria));
                else if (currentCriteria is ObjectTypeCriteria)
                    AppendCriteriaResultOnTotalResult
                        (searchQuery.SetOperator
                        , ref isFirstCriteria
                        , ref totalResultObjects
                        , ApplyObjectTypeCriteria(objectsToFilter, currentCriteria as ObjectTypeCriteria));
                else if (currentCriteria is DateRangeCriteria)
                    AppendCriteriaResultOnTotalResult
                        (searchQuery.SetOperator
                        , ref isFirstCriteria
                        , ref totalResultObjects
                        , await ApplyDateRangeCriteriaAsync(objectsToFilter, currentCriteria as DateRangeCriteria));
                else if (currentCriteria is PropertyValueCriteria)
                    AppendCriteriaResultOnTotalResult
                        (searchQuery.SetOperator
                        , ref isFirstCriteria
                        , ref totalResultObjects
                        , await ApplyPropertyValueCriteriaAsync(objectsToFilter, currentCriteria as PropertyValueCriteria));
                else if (currentCriteria is DateTimePropertyRangeCriteria)
                    AppendCriteriaResultOnTotalResult
                        (searchQuery.SetOperator
                        , ref isFirstCriteria
                        , ref totalResultObjects
                        , await ApplyDateTimePropertyRangeCriteriaAsync(objectsToFilter, currentCriteria as DateTimePropertyRangeCriteria));
                else if (currentCriteria is ContainerCriteria)
                    AppendCriteriaResultOnTotalResult
                        (searchQuery.SetOperator
                        , ref isFirstCriteria
                        , ref totalResultObjects
                        , await ApplyFilterCriteriaSetOnObjectsAsync(objectsToFilter, (currentCriteria as ContainerCriteria).CriteriaSet));

            return totalResultObjects;
        }

        private static void AppendCriteriaResultOnTotalResult(GPAS.FilterSearch.BooleanOperator booleanOperator, ref bool isFirstCriteria, ref IEnumerable<KWObject> totalResultObjects, IEnumerable<KWObject> criteriaResultObjects)
        {
            if (booleanOperator == GPAS.FilterSearch.BooleanOperator.All)
                if (isFirstCriteria)
                {
                    totalResultObjects = criteriaResultObjects;
                    isFirstCriteria = false;
                }
                else
                    totalResultObjects = totalResultObjects.Intersect(criteriaResultObjects).ToList();
            else if (booleanOperator == GPAS.FilterSearch.BooleanOperator.Any)
                totalResultObjects = totalResultObjects.Union(criteriaResultObjects).ToList();
        }

        private static async Task<IEnumerable<KWObject>> ApplyPropertyValueCriteriaAsync(IEnumerable<KWObject> objectsToFilter, GPAS.FilterSearch.PropertyValueCriteria criteria)
        {
            List<KWObject> resultSubset = new List<KWObject>();
            // ویژگی‌های از نوع تعیین شده در معیار ورودی، برای اشیا داده شده بازیابی می‌شوند
            var propertiesWithCriteriaType = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(objectsToFilter, new string[] { criteria.PropertyTypeUri });
            foreach (var item in propertiesWithCriteriaType)
                // اشیایی که یکی از ویژگی‌هایشان شرایط را احراز کند، به لیست نتیجه افزوده می‌شوند
                if (IsPropertySatisfyPropertyValueCriteria(item, criteria))
                    resultSubset.Add(item.Owner);
            return resultSubset;
        }

        private static async Task<IEnumerable<KWObject>> ApplyDateRangeCriteriaAsync(IEnumerable<KWObject> objectsToFilter, DateRangeCriteria criteria)
        {
            List<KWObject> resultSubset = new List<KWObject>();
            // انواع ویژگی که دارای نوع پایه‌ای تاریخ باشند را از هستان شناسی استخراج می‌کنیم
            List<string> datetimeBaseTypesInOntology = new List<string>();
            foreach (var item in System.GetOntology().GetAllProperties())
                if (System.GetOntology().GetBaseDataTypeOfProperty(item.TypeName) == BaseDataTypes.DateTime)
                    datetimeBaseTypesInOntology.Add(item.TypeName);
            if (datetimeBaseTypesInOntology.Count == 0)
                return resultSubset;
            // ویژگی‌های مبتنی بر تاریخ برای اشیا ورودی بازیابی می‌شوند
            var propertiesWithCriteriaType = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(objectsToFilter, datetimeBaseTypesInOntology);
            foreach (var item in propertiesWithCriteriaType)
                // اشیایی که یکی از ویژگی‌هایشان شرایط را احراز کند، به لیست نتیجه افزوده می‌شوند
                if (IsPropertySatisfyDateRangeCriteria(item, criteria) && !resultSubset.Contains(item.Owner))
                    resultSubset.Add(item.Owner);
            return resultSubset;
        }

        private static async Task<IEnumerable<KWObject>> ApplyDateTimePropertyRangeCriteriaAsync(IEnumerable<KWObject> objectsToFilter, DateTimePropertyRangeCriteria criteria)
        {
            List<KWObject> resultSubset = new List<KWObject>();

            // انواع ویژگی که دارای نوع پایه‌ای تاریخ یا زمان و موقعیت جغرافیایی باشند را از هستان شناسی استخراج می‌کنیم
            List<string> datetimeBaseTypesInOntology = new List<string>();
            foreach (var item in System.GetOntology().GetAllProperties())
            {
                BaseDataTypes dataType = System.GetOntology().GetBaseDataTypeOfProperty(item.TypeName);
                if (dataType == BaseDataTypes.DateTime || dataType == BaseDataTypes.GeoTime)
                    datetimeBaseTypesInOntology.Add(item.TypeName);
            }

            if (datetimeBaseTypesInOntology.Count == 0)
                return resultSubset;

            if (!datetimeBaseTypesInOntology.Contains(criteria.PropertyTypeUri))
                return resultSubset;

            var propertiesWithCriteriaType = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(objectsToFilter, new List<string> { criteria.PropertyTypeUri });

            foreach (var item in propertiesWithCriteriaType)
                // اشیایی که یکی از ویژگی‌هایشان شرایط را احراز کند، به لیست نتیجه افزوده می‌شوند
                if (IsPropertySatisfyDateTimePropertyRangeCriteria(item, criteria))
                    resultSubset.Add(item.Owner);

            return resultSubset.Distinct();
        }

        private static IEnumerable<KWObject> ApplyObjectTypeCriteria(IEnumerable<KWObject> objectsToFilter, ObjectTypeCriteria criteria)
        {
            List<KWObject> resultSubset = new List<KWObject>();
            // اشیا از نوع تعیین شده در معیار ورودی، به لیست نتیجه افزوده می‌شوند
            foreach (var item in objectsToFilter)
                if (criteria.ObjectsTypeUri.Contains(item.TypeURI))
                    resultSubset.Add(item);
            return resultSubset;
        }

        private static async Task<IEnumerable<KWObject>> ApplyKeywordCriteriaAsync(IEnumerable<KWObject> objectsToFilter, KeywordCriteria criteria)
        {
            List<KWObject> resultSubset = new List<KWObject>();

            List<string> stringBaseTypesInOntology = new List<string>();
            List<string> hdfsUriBaseTypesInOntology = new List<string>();
            // نوع ویژگی‌های مبتنی بر رشته و یو.آر.آی هستان‌شناسی، تفکیک می‌شوند
            foreach (var item in System.GetOntology().GetAllProperties())
            {
                BaseDataTypes itemBaseType = System.GetOntology().GetBaseDataTypeOfProperty(item.TypeName);
                if (itemBaseType == BaseDataTypes.String)
                    stringBaseTypesInOntology.Add(item.TypeName);
                else if (itemBaseType == BaseDataTypes.HdfsURI)
                    hdfsUriBaseTypesInOntology.Add(item.TypeName);
            }
            // ویژگی‌های مبتنی بر رشته برای اشیا ورودی بازیابی می‌شوند
            if (stringBaseTypesInOntology.Count != 0)
            {
                var propertiesWithCriteriaType = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(objectsToFilter, stringBaseTypesInOntology);
                foreach (var item in propertiesWithCriteriaType)
                {
                    if (IsPropertySatisfyKeywordCriteria(item, criteria) && !resultSubset.Contains(item.Owner))
                    {
                        resultSubset.Add(item.Owner);
                    }

                }
            }
            // ویژگی‌های مبتنی بر یو.آر.آی برای اشیا ورودی بازیابی می‌شوند
            if (hdfsUriBaseTypesInOntology.Count != 0)
            {
                var propertiesWithCriteriaType = await PropertyManager.GetSpecifiedPropertiesOfObjectAsync(objectsToFilter, hdfsUriBaseTypesInOntology);
                foreach (var item in propertiesWithCriteriaType)
                {
                    // در صورت احراز معیار ورودی برای ویژگی، شی مربوطه به لیست نتیجه افزوده می‌شود
                    if (IsPropertySatisfyKeywordCriteria(item, criteria) && !resultSubset.Contains(item.Owner))
                        resultSubset.Add(item.Owner);
                }
            }
            // اشیایی که شرایط برای نام نمایشی‌شان احراز شود، به لیست نتیجه افزوده می‌شوند
            foreach (var item in objectsToFilter)
                if (IsDisplayNameAndKeywordMatched(item, criteria) && !resultSubset.Contains(item))
                    resultSubset.Add(item);

            return resultSubset;
        }
    }
}
