using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.SearchServer.Access.SearchEngine;
using GPAS.SearchServer.Entities;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic
{
    public class SearchAround
    {
        private Ontology.Ontology CurrentOntology = null;

        private readonly string FilterSearchResultDefaultCountAppSettingValue
            = System.Configuration.ConfigurationManager.AppSettings["FilterSearchResultDefaultCount"];

        public SearchAround()
        {
            CurrentOntology = OntologyProvider.GetOntology();
        }
        private object lockObj = new object();
        /// <summary>
        /// این تابع بر اساس نوع ویژگی و مقدار جست و جو انجام می دهد.
        /// </summary>
        /// <param name="typeUri"> نوع ویژگی را مشخص می کند. </param>
        /// <param name="value">  مقدار ویژگی را تعیین می کند. </param>
        /// <returns>    لیستی از SearchDBProperty به عنوان خروجی برمی گرداند.   </returns>
        public List<PropertiesMatchingResults> FindPropertiesSameWith(KProperty[] properties, int totalResultsThreshold, AuthorizationParametters authorizationParametters)
        {
            List<PropertiesMatchingResults> totalResults = new List<PropertiesMatchingResults>();
            Parallel.ForEach(properties, (currentProperty) =>
            {
                BaseDataTypes baseType = CurrentOntology.GetBaseDataTypeOfProperty(currentProperty.TypeUri);
                if (baseType == BaseDataTypes.None
                    || PropertiesValidation.ValueBaseValidation.IsValidPropertyValue(baseType, currentProperty.Value).Status==PropertiesValidation.ValidationStatus.Invalid)
                {
                    throw new FaultException("Property Type is Invalid");
                }
                else
                {
                    long[] samePropertiesIDs = SearchForPropertiesWtihTypeAndValue(currentProperty.TypeUri, currentProperty.Value, totalResultsThreshold, authorizationParametters).ToArray();
                    PropertiesMatchingResults currentResult = new PropertiesMatchingResults()
                    {
                        SearchedPropertyID = currentProperty.Id,
                        ResultPropertiesID = samePropertiesIDs
                    };
                    lock (lockObj)
                    {
                        totalResults.Add(currentResult);
                    }
                }
            });
            return totalResults;
        }

        private List<long> SearchForPropertiesWtihTypeAndValue(string type, string value, int totalResultsThreshold, AuthorizationParametters authorizationParametters)
        {
            BaseDataTypes baseType = CurrentOntology.GetBaseDataTypeOfProperty(type);
            PropertyValueCriteria criteria = new PropertyValueCriteria()
            {
                PropertyTypeUri = type,
                OperatorValuePair = GetEqualValuePair(baseType, value)
            };

            CriteriaSet searchCriteriaSet = new CriteriaSet();
            searchCriteriaSet.Criterias.Add(criteria);

            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            List<long> resultObjectIDList = accessClient.GetPropertyDocumentIDByFilterCriteriaSet(
                searchCriteriaSet, authorizationParametters
                , OntologyProvider.GetOntology(), totalResultsThreshold + 1
                );
            return resultObjectIDList;
        }

        private PropertyCriteriaOperatorValuePair GetEqualValuePair(BaseDataTypes baseType, string value)
        {
            object parsedValue = PropertiesValidation.ValueBaseValidation.ParsePropertyValue(baseType, value);
            switch (baseType)
            {
                case BaseDataTypes.DateTime:
                    return new DateTimePropertyCriteriaOperatorValuePair()
                    {
                        CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                        CriteriaValue = (DateTime)parsedValue
                    };
                case BaseDataTypes.String:
                case BaseDataTypes.HdfsURI:
                    return new StringPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                        CriteriaValue = (string)parsedValue
                    };
                case BaseDataTypes.Double:
                    return new FloatPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                        CriteriaValue = (float)((double)parsedValue)
                    };
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    return new LongPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                        CriteriaValue = (long)parsedValue
                    };
                case BaseDataTypes.Boolean:
                    return new BooleanPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaOperator = BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                        CriteriaValue = (bool)parsedValue
                    };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}