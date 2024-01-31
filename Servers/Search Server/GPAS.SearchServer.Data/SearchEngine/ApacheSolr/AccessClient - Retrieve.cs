﻿using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.Ontology;
using GPAS.PropertiesValidation.Geo;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.StatisticalQuery.Formula.SetAlgebra;
using GPAS.StatisticalQuery.ResultNode;
using GPAS.Utility;
using Newtonsoft.Json.Linq;
using RestSharp;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ACL = GPAS.AccessControl.ACL;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public partial class AccessClient
    {
        #region private functions

        private string GetPermittedGroupQuery(List<string> groupNames)
        {
            string result = string.Empty;

            foreach (var permittedGroup in groupNames)
            {
                result += $"({permittedGroup}:[{(byte)Permission.Read} TO * ]) OR ";
            }
            if (!result.Equals(string.Empty))
            {
                result = result.Substring(0, result.Length - 3);
            }
            result = $"({result})";
            return result;
        }

        public void GenerateSearchQuery(CriteriaSet searchQuery, AuthorizationParametters authorizationParametters,
            Ontology.Ontology ontology, ref string solrQuery)
        {
            if (searchQuery == null) throw new ArgumentNullException(nameof(searchQuery));

            solrQuery += "(";
            foreach (var tempQuery in searchQuery.Criterias)
            {
                if (tempQuery is KeywordCriteria)
                {
                    ExecuteKeywordQuery(tempQuery, authorizationParametters, ref solrQuery);
                }
                else if (tempQuery is ObjectTypeCriteria)
                {
                    ExecuteObjectTypeQuery(tempQuery, authorizationParametters, ref solrQuery);
                }
                else if (tempQuery is DateRangeCriteria)
                {
                    ExecuteDateRangeQuery(tempQuery, authorizationParametters, ref solrQuery);
                }
                else if (tempQuery is PropertyValueCriteria)
                {
                    ExecutePropertyValueQuery(tempQuery, authorizationParametters, ontology, ref solrQuery);
                }
                else if (tempQuery is ContainerCriteria criteria)
                {
                    GenerateSearchQuery(criteria.CriteriaSet, authorizationParametters, ontology, ref solrQuery);
                }
                else
                {
                    throw new NotSupportedException("Unknown criteria type");
                }
                bool isLastCriteria = true;
                if (searchQuery.SetOperator == BooleanOperator.All)
                {
                    solrQuery += " AND ";
                    isLastCriteria = false;
                }
                else if (searchQuery.SetOperator == BooleanOperator.Any)
                {
                    solrQuery += " OR  ";
                    isLastCriteria = false;
                }
                else
                    throw new NotSupportedException("Unknown criteria concatenation operator");

                if (isLastCriteria)
                {
                    solrQuery = $" {solrQuery}     ";
                }
            }

            solrQuery = solrQuery.Remove(solrQuery.Length - 5);
            solrQuery += ") ";
        }

        private void GenerateSearchFilterQueryToReturnChildsIDs(CriteriaSet searchQuery, AuthorizationParametters authorizationParametters,
            Ontology.Ontology ontology, ref string solrQuery)
        {
            if (searchQuery == null) throw new ArgumentNullException(nameof(searchQuery));

            solrQuery += "(";
            foreach (var tempQuery in searchQuery.Criterias)
            {
                if (tempQuery is PropertyValueCriteria)
                {
                    ExecutePropertyValueQueryToReturnChildIDs(tempQuery, authorizationParametters, ontology, ref solrQuery);
                }
                else
                {
                    throw new NotSupportedException("Unknown criteria type");
                }
                bool isLastCriteria = true;
                if (searchQuery.SetOperator == BooleanOperator.All)
                {
                    solrQuery += " AND ";
                    isLastCriteria = false;
                }
                else if (searchQuery.SetOperator == BooleanOperator.Any)
                {
                    solrQuery += " OR  ";
                    isLastCriteria = false;
                }
                else
                    throw new NotSupportedException("Unknown criteria concatenation operator");

                if (isLastCriteria)
                {
                    solrQuery = $" {solrQuery}     ";
                }
            }

            solrQuery = solrQuery.Remove(solrQuery.Length - 5);
            solrQuery += ") ";
        }

        private void ExecutePropertyValueQueryToReturnChildIDs(CriteriaBase tempQuery, AuthorizationParametters authorizationParametters,
            Ontology.Ontology ontology, ref string solrQuery)
        {
            string propertyName = ontology.GetTypeName(((PropertyValueCriteria)tempQuery).PropertyTypeUri);
            object propertyValue = new object();
            string query = "";
            switch (ontology.GetBaseDataTypeOfProperty(propertyName))
            {
                case BaseDataTypes.Long:
                case BaseDataTypes.Int:
                    propertyValue =
                        Int64.Parse(((LongPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue.ToString());
                    switch (
                        ((LongPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: {propertyValue}";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: {{{propertyValue} TO *]";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: [{propertyValue} TO *]";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: [* TO {propertyValue}}}";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: [* TO {propertyValue}]";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query = string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.LongValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: {propertyValue}";
                            break;
                    }
                    break;

                case BaseDataTypes.Double:
                    propertyValue = ((FloatPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;
                    propertyValue = propertyValue.ToString().Replace("/", ".");

                    switch (
                        ((FloatPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: {propertyValue}";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: {{{propertyValue} TO *]";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: [{propertyValue} TO *]";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: [* TO {propertyValue}}}";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: [* TO {propertyValue}]";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.DoubleValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: {propertyValue}";
                            break;
                    }
                    break;

                case BaseDataTypes.String:
                    propertyValue = ((StringPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;

                    switch (
                        ((StringPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query +=
                                $" AND {nameof(Property.KeywordTokenizedStringValue)}: *{CleanToSearchInSolr(propertyValue.ToString())}*";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: \"{propertyValue}\"";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.KeywordTokenizedStringValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: \"{propertyValue}\"";
                            break;
                    }

                    break;

                case BaseDataTypes.Boolean:
                    bool booleanPropertyValue = ((BooleanPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;


                    switch (
                        ((BooleanPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.BooleanValue)}: {booleanPropertyValue.ToString()}";
                            break;
                        case BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.BooleanValue), booleanPropertyValue.ToString());
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.BooleanValue)}: {booleanPropertyValue.ToString()}";
                            break;
                    }

                    break;

                case BaseDataTypes.DateTime:
                    propertyValue = ((DateTimePropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue.ToString(CultureInfo.InvariantCulture);
                    DateTime convertedpropertyValue = DateTime.Parse(propertyValue.ToString());

                    switch (
                        ((DateTimePropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [{1} TO {1}] ", nameof(Property.LongValue), ConvertDatePropertyToSolrLongDate(convertedpropertyValue));
                            break;
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: [{1} TO {1}])", nameof(Property.LongValue), ConvertDatePropertyToSolrLongDate(convertedpropertyValue));
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [{1} TO {1}]", nameof(Property.LongValue), ConvertDatePropertyToSolrLongDate(convertedpropertyValue));
                            break;
                    }
                    break;

                case BaseDataTypes.HdfsURI:
                    propertyValue = ((StringPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;

                    switch (
                        ((StringPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query +=
                                $" AND {nameof(Property.KeywordTokenizedStringValue)}: *{CleanToSearchInSolr(propertyValue.ToString())}*";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: {propertyValue}";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.StringValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: {propertyValue}";
                            break;
                    }
                    break;
            }
            query = $"( {query} AND {GetACLCriterions(authorizationParametters)})";
            solrQuery += query;
        }

        private string GetACLCriterions(AuthorizationParametters authorizationParametters)
        {
            StringUtility utility = new StringUtility();
            string canReadableInClassification = $"( {utility.SeperateByInputSeperator(authorizationParametters.readableClassifications, " ")} )";
            string queryCriterions = " ";
            queryCriterions += $"{nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier)} : {canReadableInClassification} AND ";
            queryCriterions += $"{GetPermittedGroupQuery(authorizationParametters.permittedGroupNames)}";
            queryCriterions += " ";
            return queryCriterions;
        }

        private void ExecuteKeywordQuery(CriteriaBase tempQuery, AuthorizationParametters authorizationParametters, ref string solrQuery)
        {
            string keyword = CleanToSearchInSolr(((KeywordCriteria)tempQuery).Keyword);
            string query = $"({{!parent which= \"ParentDocument:true\" v=\"KeywordTokenizedStringValue: *{keyword}*  " +
                $"AND {GetACLCriterions(authorizationParametters)}\"}})";
            solrQuery += query;
        }

        private void ExecutePropertyValueQuery(CriteriaBase tempQuery, AuthorizationParametters authorizationParametters,
            Ontology.Ontology ontology, ref string solrQuery)
        {
            string propertyName = ontology.GetTypeName(((PropertyValueCriteria)tempQuery).PropertyTypeUri);
            object propertyValue = new object();
            string query = "";
            BaseDataTypes dataType = ontology.GetBaseDataTypeOfProperty(propertyName);
            switch (dataType)
            {
                case BaseDataTypes.Long:
                case BaseDataTypes.Int:
                    propertyValue =
                        Int64.Parse(((LongPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue.ToString());
                    switch (
                        ((LongPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: {{{propertyValue} TO *]";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: [{propertyValue} TO *]";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: [* TO {propertyValue}}}";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: [* TO {propertyValue}]";
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.LongValue), propertyValue);
                            break;
                        case LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.LongValue)}: {propertyValue}";
                            break;
                    }
                    break;

                case BaseDataTypes.Double:
                    propertyValue = ((FloatPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;
                    propertyValue = propertyValue.ToString().Replace("/", ".");

                    switch (
                        ((FloatPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: {propertyValue}";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: {{{propertyValue} TO *]";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: [{propertyValue} TO *]";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: [* TO {propertyValue}}}";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: [* TO {propertyValue}]";
                            break;
                        case FloatPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.DoubleValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.DoubleValue)}: {propertyValue}";
                            break;
                    }
                    break;

                case BaseDataTypes.String:
                    propertyValue = ((StringPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;

                    switch (
                        ((StringPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query +=
                                $" AND {nameof(Property.KeywordTokenizedStringValue)}: *{CleanToSearchInSolr(propertyValue.ToString())}*";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: \\\"{propertyValue}\\\"";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.KeywordTokenizedStringValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: \\\"{propertyValue}\\\"";
                            break;
                    }

                    break;

                case BaseDataTypes.GeoPoint:
                    GeoCircleEntityRawData geoCircleEntityRawData =
                        ((GeoPointPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;

                    propertyValue = GeoCircle.GetGeoCircleStringValue(geoCircleEntityRawData);

                    switch (
                        ((GeoPointPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND  -{{!geofilt pt={geoCircleEntityRawData.Latitude},{geoCircleEntityRawData.Longitude} sfield=GeoValue d={geoCircleEntityRawData.Radius}}} ";
                            break;
                        case GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND  +{{!geofilt pt={geoCircleEntityRawData.Latitude},{geoCircleEntityRawData.Longitude} sfield=GeoValue d={geoCircleEntityRawData.Radius}}} ";
                            break;
                    }

                    break;

                case BaseDataTypes.GeoTime:
                    propertyValue = ((StringPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;

                    switch (
                        ((StringPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query +=
                                $" AND {nameof(Property.KeywordTokenizedStringValue)}: *{CleanToSearchInSolr(propertyValue.ToString())}*";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: \\\"{propertyValue}\\\"";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.KeywordTokenizedStringValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: \\\"{propertyValue}\\\"";
                            break;
                    }

                    break;

                case BaseDataTypes.Boolean:
                    bool booleanPropertyValue = ((BooleanPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;


                    switch (
                        ((BooleanPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.BooleanValue)}: {booleanPropertyValue.ToString()}";
                            break;
                        case BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.BooleanValue), booleanPropertyValue.ToString());
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.BooleanValue)}: {booleanPropertyValue.ToString()}";
                            break;
                    }

                    break;

                case BaseDataTypes.DateTime:
                    propertyValue = ((DateTimePropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;
                    long convertedpropertyValue = ConvertDatePropertyToSolrLongDate((DateTime)propertyValue);
                    query = $" {nameof(Property.TypeUri)}: {propertyName}";

                    switch (
                        ((DateTimePropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThan:
                            query += $" AND {nameof(Property.LongValue)}: {{{convertedpropertyValue} TO *]";
                            break;
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.GreaterThanOrEquals:
                            query += $" AND {nameof(Property.LongValue)}: [{convertedpropertyValue} TO *]";
                            break;
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.LessThan:
                            query += $" AND {nameof(Property.LongValue)}: [* TO {convertedpropertyValue}}}";
                            break;
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.LessThanOrEquals:
                            query += $" AND {nameof(Property.LongValue)}: [* TO {convertedpropertyValue}]";
                            break;
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.LongValue), convertedpropertyValue);
                            break;
                        case DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                        default:
                            query += $" AND {nameof(Property.LongValue)}: {convertedpropertyValue}";
                            break;
                    }
                    break;

                case BaseDataTypes.HdfsURI:
                    propertyValue = ((StringPropertyCriteriaOperatorValuePair)(((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaValue;

                    switch (
                        ((StringPropertyCriteriaOperatorValuePair)
                            (((PropertyValueCriteria)tempQuery).OperatorValuePair)).CriteriaOperator)
                    {
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Like:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query +=
                                $" AND {nameof(Property.KeywordTokenizedStringValue)}: *{CleanToSearchInSolr(propertyValue.ToString())}*";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: {propertyValue}";
                            break;
                        case StringPropertyCriteriaOperatorValuePair.RelationalOperator.NotEquals:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += string.Format(" AND {0}: [* TO *] AND -({0}: {1})", nameof(Property.StringValue), propertyValue);
                            break;
                        default:
                            query = $" {nameof(Property.TypeUri)}: {propertyName}";
                            query += $" AND {nameof(Property.KeywordTokenizedStringValue)}: {propertyValue}";
                            break;
                    }
                    break;
            }
            query = $"({{!parent which= ParentDocument:true v=\" {query} AND {GetACLCriterions(authorizationParametters)}\"}})";
            query = AmendSolrQuery(query, dataType);
            solrQuery += query;
        }

        private string CleanToSearchInSolr(string text)
        {
            string result = text.Replace("~", "\\\\~");
            result = result.Replace("+", "\\\\+");
            result = result.Replace("-", "\\\\-");
            result = result.Replace("&", "\\\\&");
            result = result.Replace("|", "\\\\|");
            result = result.Replace("!", "\\\\!");
            result = result.Replace("(", "\\\\(");
            result = result.Replace(")", "\\\\)");
            result = result.Replace("{", "\\\\{");
            result = result.Replace("}", "\\\\}");
            result = result.Replace("[", "\\\\[");
            result = result.Replace("]", "\\\\]");
            result = result.Replace("^", "\\\\^");
            result = result.Replace(":", "\\\\:");
            result = result.Replace(" ", "\\\\ ");
            return result;
        }

        private void ExecuteDateRangeQuery(CriteriaBase tempQuery, AuthorizationParametters authorizationParametters, ref string solrQuery)
        {
            List<int> result = new List<int>();

            string startTime = ((DateRangeCriteria)tempQuery).StartTime.ToString(CultureInfo.InvariantCulture);
            string endTime = ((DateRangeCriteria)tempQuery).EndTime.ToString(CultureInfo.InvariantCulture);

            DateTime convertedStartTime = DateTime.Parse(startTime);
            DateTime convertedEndTime = DateTime.Parse(endTime);

            string query =
                $"LongValue: [{ConvertDatePropertyToSolrLongDate(convertedStartTime)} TO {ConvertDatePropertyToSolrLongDate(convertedEndTime)}] OR ";
            if (query != "")
                query = query.Remove(query.Length - 4);

            query =
                $"({{!parent which= ParentDocument:true v=\" {query} AND {GetACLCriterions(authorizationParametters)}\"}})";

            solrQuery += query;
        }

        private long ConvertDatePropertyToSolrLongDate(DateTime datetimeValue)
        {
            DateTime utcTime1 = new DateTime(datetimeValue.Year, datetimeValue.Month, datetimeValue.Day, datetimeValue.Hour, datetimeValue.Minute, datetimeValue.Second);
            utcTime1 = DateTime.SpecifyKind(utcTime1, DateTimeKind.Utc);
            DateTimeOffset utcTime2 = utcTime1;

            return utcTime2.ToUnixTimeMilliseconds();

        }

        private void ExecuteObjectTypeQuery(CriteriaBase tempQuery, AuthorizationParametters authorizationParametters, ref string solrQuery)
        {
            ObservableCollection<string> objectsType = ((ObjectTypeCriteria)tempQuery).ObjectsTypeUri;
            string objectsTypeSubQuery = "";
            foreach (var objectType in objectsType)
            {
                objectsTypeSubQuery += "OwnerObjectTypeUri: " + objectType + " OR ";
            }
            if (objectsTypeSubQuery != "")
                objectsTypeSubQuery = objectsTypeSubQuery.Remove(objectsTypeSubQuery.Length - 4);
            else
            {
                return;
            }

            objectsTypeSubQuery =
                $"({{!parent which= ParentDocument:true v=\" {$"({objectsTypeSubQuery})"} AND {GetACLCriterions(authorizationParametters)}\"}})";
            solrQuery += objectsTypeSubQuery;
        }

        private IRestResponse ExecuteRetriveQueryOnParticularCollection(JObject query, string collectionUrl)
        {
            try
            {
                var client = new RestClient(collectionUrl);
                RestRequest request = GetExecuteRetriveQueryOnParticularCollectionRequest(query);
                var result = client.Execute(request);

                return result;
            }
            catch (Exception)
            {
                throw;
            }

        }

        private async Task<IRestResponse> ExecuteRetriveQueryOnParticularCollectionAsync(JObject query, string collectionUrl)
        {
            var client = new RestClient(collectionUrl);
            RestRequest request = GetExecuteRetriveQueryOnParticularCollectionRequest(query);
            var result = await client.ExecuteTaskAsync(request);

            if (!result.IsSuccessful)
            {
                throw result.ErrorException;
            }

            return result;
        }

        private static RestRequest GetExecuteRetriveQueryOnParticularCollectionRequest(JObject query)
        {
            var request = new RestRequest($"/query", Method.POST);
            request.AddParameter("application/json; charset=utf-8", query, ParameterType.RequestBody);
            return request;
        }

        #endregion


        #region Get Candidates functions
        private List<string> GetPropertyDocumentBasicFields()
        {
            List<string> propertyCollectionBasicFields = new List<string>(5);
            propertyCollectionBasicFields.Add("id");
            propertyCollectionBasicFields.Add("_version_");
            propertyCollectionBasicFields.Add("ParentDocument");
            propertyCollectionBasicFields.Add(nameof(Property.TypeUri));
            propertyCollectionBasicFields.Add(nameof(Property.BaseType));
            propertyCollectionBasicFields.Add(nameof(Property.OwnerObjectID));
            propertyCollectionBasicFields.Add(nameof(Property.OwnerObjectTypeUri));
            return propertyCollectionBasicFields;
        }

        private string GetSolrResponseByPostQuery(string query)
        {
            //Send Query.
            string solrQueryUri = ObjectCollection.SolrUrl + "/query";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(solrQueryUri);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            byte[] byteArray = Encoding.UTF8.GetBytes(query);
            Stream dataStream = null;
            try
            {
                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            finally
            {
                if (dataStream != null)
                    dataStream.Close();
            }
            // Get the response Of Query and Parse it. 
            var responseStream = request.GetResponse().GetResponseStream();
            string response = string.Empty;
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(responseStream);
                response = reader.ReadToEnd();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return response;
        }
        #endregion

        public string GetFileDocumentPossibleExtractedContent(string fileDocID)
        {
            var client = new RestClient($"{FileCollection.SolrUrl}/get?id={fileDocID}&fl=content");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JToken content = JToken.Parse(response.Content);
            if (content["doc"].HasValues)
            {
                return content["doc"]["content"][0].ToString();
            }
            else
            {
                throw new Exception($"any document not find with this ID {fileDocID}");
            }
        }

        public List<long> GetObjectDocumentIDByFilterCriteriaSet(List<long> objectIDs, CriteriaSet criteria,
            AuthorizationParametters authorizationParametters, Ontology.Ontology ontology, long resultLimit)
        {
            List<long> documentIDs = new List<long>();
            StringUtility utility = new StringUtility();
            string canReadableInClassification = $"( {utility.SeperateByInputSeperator(authorizationParametters.readableClassifications, " ")} )";
            string solrQuery = string.Empty;
            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append(" id:(");
            foreach (var objectID in objectIDs)
            {
                solrQueryIDPart.Append(" ").Append(objectID.ToString());
            }
            solrQueryIDPart.Append(") AND ");

            GenerateSearchQuery(criteria, authorizationParametters, ontology, ref solrQuery);
            solrQuery = solrQuery.Replace("v= \"", $"v = \" {solrQueryIDPart}");
            JToken docsJson = RetriveObjectsFromSolrByQuery(solrQuery, resultLimit);
            foreach (var child in docsJson.Children())
            {
                var docId = (long)child["id"];
                documentIDs.Add(docId);
            }
            return documentIDs;
        }

        public List<SearchObject> GetObjectDocumentIDByFilterCriteriaSet(
            CriteriaSet criteria, AuthorizationParametters authorizationParametters
            , Ontology.Ontology ontology, long resultLimit)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            StringUtility utility = new StringUtility();
            string solrQuery = string.Empty;
            GenerateSearchQuery(criteria, authorizationParametters, ontology, ref solrQuery);
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(solrQuery, resultLimit);
            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }

            return searchObjects;
        }

        public List<long> GetPropertyDocumentIDByFilterCriteriaSet(
            CriteriaSet criteria, AuthorizationParametters authorizationParametters
            , Ontology.Ontology ontology, long resultLimit)
        {
            List<long> documentIDs = new List<long>();
            StringUtility utility = new StringUtility();
            string solrQuery = string.Empty;
            GenerateSearchFilterQueryToReturnChildsIDs(criteria, authorizationParametters, ontology, ref solrQuery);
            JToken docsJson = RetriveObjectsFromSolrByQuery(solrQuery, resultLimit);
            foreach (var child in docsJson.Children())
            {
                string docFullID = child["id"].ToString();
                int separatorIndex = docFullID.IndexOf(PropertyDocIdSeparator);
                if (separatorIndex < 0)
                    throw new InvalidDataException("Unable to recognize ID separator character");
                docFullID = docFullID.Substring((separatorIndex + 1));
                long docId;
                long.TryParse(docFullID, out docId);
                documentIDs.Add(docId);
            }
            return documentIDs;
        }

        public List<SearchObject> GetEntityDocumentIDsForMatchedKeyword(string keyword, AuthorizationParametters authorizationParametters, long resultLimit, Ontology.Ontology ontology)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            StringUtility utility = new StringUtility();
            string canReadableInClassification = $"( {utility.SeperateByInputSeperator(authorizationParametters.readableClassifications, " ")} )";
            string commandQuery = "{!parent which= ParentDocument:true}";
            commandQuery += $"((({nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier)} : {canReadableInClassification}) AND ";
            commandQuery += $"({GetPermittedGroupQuery(authorizationParametters.permittedGroupNames)})) AND ";
            if (keyword.First().Equals('"') && keyword.Last().Equals('"'))
                keyword = '"' + CleanseSolrString(keyword.Trim('"')) + '"';
            else
                keyword = CleanseSolrString(keyword);

            commandQuery += $" (StringValue : {keyword}";
            commandQuery += " OR ";
            commandQuery += $" KeywordTokenizedStringValue : {keyword})) AND (";

            foreach (var entity in ontology.GetAllChilds(ontology.GetEntityTypeURI()))
            {
                commandQuery += $"OwnerObjectTypeUri: {entity} OR ";
            }

            commandQuery = commandQuery.Remove(commandQuery.Length - 4, 4) + ")";
            return RetriveMasterObjectsById(commandQuery, resultLimit);
        }

        private SearchObject ConvertJTokenObjectToSearchObject(JToken jObject)
        {
            SearchObject searchObject = new SearchObject();
            searchObject.Id = (long)jObject["id"];
            searchObject.TypeUri = (string)jObject["TypeUri"];
            searchObject.LabelPropertyID = (long)jObject["LabelPropertyID"];
            searchObject.IsMaster = jObject["IsMaster"] == null ? -1 : (int)jObject["IsMaster"];

            JToken jTokenMaster = JObject.Parse(jObject["Master"].ToString())["docs"];
            foreach (var child2 in jTokenMaster.Children())
            {
                SearchObjectMaster searchObjectMaster = new SearchObjectMaster();
                searchObjectMaster.Id = (long)child2["id"];
                searchObjectMaster.MasterId = (long)child2["MasterId"];
                searchObjectMaster.ResolveTo = child2["ResolveTo"] == null ? null : child2["ResolveTo"].ToObject<long[]>();

                searchObject.SearchObjectMaster = searchObjectMaster;
            }

            return searchObject;
        }

        public List<SearchObject> GetDocumentDocumentIDsForMatchedKeyword(string keyword, AuthorizationParametters authorizationParametters, long resultLimit, Ontology.Ontology ontology)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            StringUtility utility = new StringUtility();
            string canReadableInClassification = $"( {utility.SeperateByInputSeperator(authorizationParametters.readableClassifications, " ")} )";
            string commandQuery = "{!parent which= ParentDocument:true}";
            commandQuery += $"((({nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier)} : {canReadableInClassification}) AND ";
            commandQuery += $"({GetPermittedGroupQuery(authorizationParametters.permittedGroupNames)})) AND ";
            if (keyword.First().Equals('"') && keyword.Last().Equals('"'))
                keyword = '"' + CleanseSolrString(keyword.Trim('"')) + '"';
            else
                keyword = CleanseSolrString(keyword);

            commandQuery += $" (StringValue : {keyword}";
            commandQuery += " OR ";
            commandQuery += $" KeywordTokenizedStringValue : {keyword})) AND (";

            foreach (var entity in ontology.GetAllChilds(ontology.GetDocumentTypeURI()))
            {
                commandQuery += $"OwnerObjectTypeUri: {entity} OR ";
            }

            commandQuery = commandQuery.Remove(commandQuery.Length - 4, 4) + ")";

            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(commandQuery, resultLimit);

            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }
            return searchObjects;
        }


        public List<SearchObject> GetEventDocumentIDsForMatchedKeyword(string keyword, AuthorizationParametters authorizationParametters, long resultLimit, Ontology.Ontology ontology)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            StringUtility utility = new StringUtility();
            string canReadableInClassification = $"( {utility.SeperateByInputSeperator(authorizationParametters.readableClassifications, " ")} )";
            string commandQuery = "{!parent which= ParentDocument:true}";
            commandQuery += $"((({nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier)} : {canReadableInClassification}) AND ";
            commandQuery += $"({GetPermittedGroupQuery(authorizationParametters.permittedGroupNames)})) AND ";
            if (keyword.First().Equals('"') && keyword.Last().Equals('"'))
                keyword = '"' + CleanseSolrString(keyword.Trim('"')) + '"';
            else
                keyword = CleanseSolrString(keyword);

            commandQuery += $" (StringValue : {keyword}";
            commandQuery += " OR ";
            commandQuery += $" KeywordTokenizedStringValue : {keyword})) AND (";

            foreach (var entity in ontology.GetAllChilds(ontology.GetEventTypeURI()))
            {
                commandQuery += $"OwnerObjectTypeUri: {entity} OR ";
            }
            commandQuery = commandQuery.Remove(commandQuery.Length - 4, 4) + ")";
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(commandQuery, resultLimit);

            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }

            return searchObjects;
        }

        public List<long> GetFileDocumentOwnerObjectIDs(string keyword, AuthorizationParametters authorizationParametters, long resultLimit)
        {
            List<long> documentIDs = new List<long>();
            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            keyword = CleanseSolrString(keyword);
            query.Add(new JProperty("filter", $"_text_:{keyword} AND {GetACLCriterions(authorizationParametters)}"));
            query.Add(new JProperty("fields", nameof(Entities.SearchEngine.Documents.File.OwnerObjectIds)));
            query.Add(new JProperty("limit", resultLimit));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, FileCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    foreach (var ownerObjectId in child["OwnerObjectIds"])
                    {
                        documentIDs.Add(long.Parse(ownerObjectId.ToString()));
                    }
                }
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            return documentIDs;
        }

        private JToken RetriveObjectsFromSolrByQuery(string filterQuery, long resultLimit)
        {
            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filterQuery}"));
            query.Add(new JProperty("limit", $"{resultLimit}"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            return JObject.Parse(jsonResult["response"].ToString())["docs"];
        }

        private JToken RetriveObjectsFromSolrByQueryWithMasterId(string filterQuery, long resultLimit)
        {
            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filterQuery}"));
            query.Add(new JProperty("limit", $"{resultLimit}"));
            query.Add(new JProperty("fields", "*,Master:[subquery]"));

            JObject jParams = JObject.Parse($@"	{{
                                                    ""Master.q"":""{{!term f=id v=$row.id}}"",
                                                    ""Master.collection"":""Resolve_Collection"",
                                                    ""Master.fl"":""*""
                                                }}          
                                            ");

            query.Add(new JProperty("params", jParams));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            return JObject.Parse(jsonResult["response"].ToString())["docs"];
        }

        /// <summary>
        /// این متد همیشه اشیای مستر را بر میگرداند حتی اگر در واقع یک اسلیو جستجو شده باشد
        /// </summary>
        /// <param name="filterQuery"></param>
        /// <param name="resultLimit"></param>
        /// <returns></returns>
        private List<SearchObject> RetriveMasterObjectsById(string filterQuery, long resultLimit)
        {
            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filterQuery}"));
            query.Add(new JProperty("limit", $"{resultLimit}"));
            query.Add(new JProperty("fields", "*,Master:[subquery]"));

            JObject jParams = JObject.Parse($@"	{{
                                                    ""Master.q"":""{{!term f=id v=$row.id}}"",
                                                    ""Master.collection"":""Resolve_Collection"",
                                                    ""Master.fl"":""*""
                                                }}          
                                            ");

            query.Add(new JProperty("params", jParams));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            List<SearchObject> searchObjects = new List<SearchObject>();
            JToken docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];

            List<long> mastersOfSlavesIDs = new List<long>();
            Dictionary<long, List<SearchObject>> slavesByMaster = new Dictionary<long, List<SearchObject>>();

            foreach (var child in docsJson.Children())
            {
                SearchObject searchObject = ConvertJTokenObjectToSearchObject(child);
                if (searchObject.IsMaster == 0)
                {
                    if (!slavesByMaster.ContainsKey(searchObject.SearchObjectMaster.MasterId))
                        slavesByMaster[searchObject.SearchObjectMaster.MasterId] = new List<SearchObject>();

                    slavesByMaster[searchObject.SearchObjectMaster.MasterId].Add(searchObject);

                    if (!mastersOfSlavesIDs.Contains(searchObject.SearchObjectMaster.MasterId))
                        mastersOfSlavesIDs.Add(searchObject.SearchObjectMaster.MasterId);
                }
                else
                {
                    searchObjects.Add(searchObject);
                }
            }

            if (mastersOfSlavesIDs.Count > 0)
            {
                var spaceSeparatedIDs = string.Join(" ", mastersOfSlavesIDs);
                string masterOfSlavesQuery = string.Format("{{!parent which= ParentDocument:true}} id:({0})", spaceSeparatedIDs);
                JToken masterOfSlavesDocsJson = RetriveObjectsFromSolrByQueryWithMasterId(masterOfSlavesQuery, mastersOfSlavesIDs.Count);
                foreach (var child in masterOfSlavesDocsJson.Children())
                {
                    SearchObject searchObject = ConvertJTokenObjectToSearchObject(child);
                    if (!searchObjects.Any(so => so.Id == searchObject.Id))
                        searchObjects.Add(searchObject);
                }

                foreach (SearchObject searchObject in searchObjects)
                {
                    if (slavesByMaster.ContainsKey(searchObject.Id))
                    {
                        searchObject.Slaves = slavesByMaster[searchObject.Id];
                    }
                }
            }

            return searchObjects;
        }

        public JToken RetrieveEntireDocumentObjectFromSolr(string objDocID)
        {
            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"id:{objDocID}!*"));
            query.Add(new JProperty("limit", "1000"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            return jsonResult["response"]["docs"];
        }

        public JToken RetrieveImageDocumentFromSolr(string docId)
        {
            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", $"id:{docId}!*"),
                new JProperty("limit", "1000")
            };

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ImageCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            return jsonResult["response"]["docs"];
        }

        private List<string> RetriveDocumentsHasMultiValueFieldInFileCollection(string multiValueFieldName, string currentValue)
        {
            JObject jsonQuery = new JObject();
            jsonQuery.Add(new JProperty("query", "*:*"));
            jsonQuery.Add(new JProperty("filter", $"({multiValueFieldName} : {currentValue})"));
            jsonQuery.Add(new JProperty("fields", "id"));
            jsonQuery.Add(new JProperty("limit", "1000"));
            return RetriveDocumentIdsFromFileCollection(jsonQuery);
        }

        private List<string> RetriveDocumentIdsFromFileCollection(JObject jsonQuery)
        {
            List<string> ids = new List<string>();
            var client = new RestClient($"{FileCollection.SolrUrl}/query");
            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json", jsonQuery.ToString(), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JObject responseContent = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(responseContent["responseHeader"].ToString())["status"];
            // Extract IDs From Response
            foreach (var doc in responseContent["response"]["docs"].Children())
            {
                ids.Add(doc["id"].ToString());
            }
            return ids;
        }

        /// <returns>
        /// Result has Returned Json Object Like Below
        /// "doc":
        /// { "id":"1"
        ///   "field1":"value1"
        ///   ...
        /// }
        /// </returns>
        private JToken RetriveFromCollectionByIdAndFilterFields(string id, string collectionUrl, List<string> filterFields)
        {
            StringBuilder field = new StringBuilder();
            foreach (string f in filterFields)
            {
                field.Append("&fl=").Append(f);
            }
            var client = new RestClient($"{collectionUrl}/get?id={id}{field}");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            return JToken.Parse(response.Content);
        }

        public bool IsFileDocumentExistWithID(string fileDocID)
        {
            var client = new RestClient($"{FileCollection.SolrUrl}/get?id={fileDocID}&fl=id");
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JToken content = JToken.Parse(response.Content);
            if (content.HasValues)
                return true;
            else
                return false;
        }

        #region datasource

        private List<Entities.SearchEngine.Documents.DataSourceDocument> JsonToDataSource(JObject jsonResult)
        {
            List<Entities.SearchEngine.Documents.DataSourceDocument> dataSources = new List<Entities.SearchEngine.Documents.DataSourceDocument>();
            var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
            foreach (var child in JObject.Parse(jsonResult["response"].ToString())["docs"].Children<JObject>())
            {
                string name = string.Empty;
                string description = string.Empty;
                string type = string.Empty;
                string id = string.Empty;
                string createdBy = string.Empty;
                string createdTime = string.Empty;
                string classification = string.Empty;
                List<GroupPermission> groupPermission = new List<GroupPermission>();
                foreach (var item in child)
                {
                    switch (item.Key)
                    {
                        case nameof(DataSourceDocument.Name):
                            name = item.Value.ToString();
                            break;
                        case nameof(DataSourceDocument.Description):
                            description = item.Value.ToString();
                            break;
                        case nameof(DataSourceDocument.Type):
                            type = item.Value.ToString();
                            break;
                        case nameof(DataSourceDocument.CreatedBy):
                            createdBy = item.Value.ToString();
                            break;
                        case nameof(DataSourceDocument.CreatedTime):
                            createdTime = item.Value.ToString();
                            break;
                        case "id":
                            id = item.Value.ToString();
                            break;
                        case nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier):
                            classification = item.Value.ToString();
                            break;
                        case "_version_":
                            break;
                        default:
                            groupPermission.Add(
                                new GroupPermission()
                                {
                                    GroupName = item.Key,
                                    AccessLevel = (Permission)Enum.Parse(typeof(Permission), item.Value.ToString())
                                }
                                );
                            break;
                    }
                }
                dataSources.Add
                    (
                    new DataSourceDocument()
                    {
                        Id = id,
                        Description = description,
                        Type = long.Parse(type),
                        Name = name,
                        CreatedBy = createdBy,
                        CreatedTime = createdTime,
                        Acl = new Entities.SearchEngine.Documents.ACL()
                        {
                            ClassificationIdentifier = classification,
                            Permissions = groupPermission
                        }
                    }
                    );
            }

            return dataSources;
        }

        public async Task<List<DataSourceDocument>> GetDataSourcesPerType(long dataSourceType, int star, int count, string filter, AuthorizationParametters authorizationParametters)
        {
            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            filter = CleanseSolrString(filter);
            if (!string.IsNullOrEmpty(filter))
            {
                query.Add(new JProperty("filter", $"{nameof(DataSourceDocument.Name)}:*{filter}* AND {nameof(DataSourceDocument.Type)}:{dataSourceType} AND {GetACLCriterions(authorizationParametters)}"));
            }
            else
            {
                query.Add(new JProperty("filter", $"{nameof(DataSourceDocument.Type)}:{dataSourceType} AND {GetACLCriterions(authorizationParametters)}"));
            }
            query.Add(new JProperty("limit", count + 1));
            query.Add(new JProperty("sort", "id asc"));
            if (star != 0)
            {
                query.Add(new JProperty("offset", star * count));
            }
            else
            {
                query.Add(new JProperty("offset", star));
            }


            IRestResponse response = await ExecuteRetriveQueryOnParticularCollectionAsync(query, DataSourceCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                return JsonToDataSource(jsonResult);
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        public List<DataSourceDocument> GetAllDataSources(int count, string filter, AuthorizationParametters authorizationParametters)
        {
            int defaultStar = 0;
            List<DataSourceDocument> dataSourceSpecifications = new List<DataSourceDocument>();
            List<Task<List<DataSourceDocument>>> syncTasks = new List<Task<List<DataSourceDocument>>>();
            foreach (var dataSourceType in Enum.GetValues(typeof(DataSourceType)))
            {
                syncTasks.Add(GetDataSourcesPerType((long)(DataSourceType)dataSourceType, defaultStar, count, filter, authorizationParametters));
            }
            Task.WaitAll();
            foreach (var item in syncTasks)
            {
                dataSourceSpecifications.AddRange(item.Result);
            }
            return dataSourceSpecifications;
        }

        public List<Entities.SearchEngine.Documents.DataSourceDocument> GetDataSources(long dataSourceType, int star, int count, string filter, AuthorizationParametters authorizationParametters)
        {
            List<Entities.SearchEngine.Documents.DataSourceDocument> dataSourceSpecifications = new List<Entities.SearchEngine.Documents.DataSourceDocument>();
            List<Task<List<Entities.SearchEngine.Documents.DataSourceDocument>>> syncTasks = new List<Task<List<Entities.SearchEngine.Documents.DataSourceDocument>>>();
            syncTasks.Add(GetDataSourcesPerType((long)(DataSourceType)dataSourceType, star, count, filter, authorizationParametters));
            Task.WaitAll();
            foreach (var item in syncTasks)
            {
                dataSourceSpecifications.AddRange(item.Result);
            }
            return dataSourceSpecifications;
        }

        #endregion

        #region Geo Search
        public List<SearchObject> GetObjectDocumentIDByGeoCircleSearch(CircleSearchCriteria circleSearchCriteria
            , int maxResult
            , AuthorizationParametters authorizationParametters)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            string filterQuery = GenerateGeoCircleSearchQuery(circleSearchCriteria, maxResult, authorizationParametters);
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(filterQuery, maxResult);

            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }

            return searchObjects;
        }

        public List<SearchObject> GetObjectDocumentIDByGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria
            , int maxResult
            , AuthorizationParametters authorizationParametters)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            string filterQuery = GenerateGeoPolygonSearchQuery(polygonSearchCriteria, maxResult, authorizationParametters);
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(filterQuery, maxResult);

            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }

            return searchObjects;
        }

        private string GeneratePolygonSearchQueryPart(PolygonSearchCriteria polygonSearchCriteria)
        {
            string subQuery = "{!field f=GeoValue v=\"Intersects(POLYGON((";
            foreach (var vertex in (polygonSearchCriteria).Vertices)
            {
                subQuery += $"{vertex.Lng.ToString()} {vertex.Lat.ToString()},";
            }
            subQuery +=
                $"{(polygonSearchCriteria).Vertices[0].Lng.ToString()} {(polygonSearchCriteria).Vertices[0].Lat.ToString()})))\"}}";

            return subQuery;
        }

        public List<SearchObject> GetObjectDocumentIDByGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria
            , CriteriaSet filterSearchCriteria
            , int maxResult
            , AuthorizationParametters authorizationParametters
            , Ontology.Ontology ontology)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));

            string filterSearchQuery = string.Empty;
            GenerateSearchQuery(filterSearchCriteria, authorizationParametters, ontology, ref filterSearchQuery);
            string geoQuery = GenerateGeoCircleSearchQuery(circleSearchCriteria, maxResult, authorizationParametters);
            string filterQuery = $"+{filterSearchQuery} AND {geoQuery}";
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(filterQuery, maxResult);

            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }

            return searchObjects;
        }

        public List<SearchObject> GetObjectDocumentIDByGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria
            , CriteriaSet filterSearchCriteria
            , int maxResult
            , AuthorizationParametters authorizationParametters
            , Ontology.Ontology ontology)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));

            string filterSearchQuery = string.Empty;
            GenerateSearchQuery(filterSearchCriteria, authorizationParametters, ontology, ref filterSearchQuery);
            string geoQuery = GenerateGeoPolygonSearchQuery(polygonSearchCriteria, maxResult, authorizationParametters);
            string filterQuery = $"+{filterSearchQuery} AND {geoQuery}";
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(filterQuery, maxResult);

            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }

            return searchObjects;
        }

        private string GenerateGeoCircleSearchQuery(CircleSearchCriteria circleSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {

            string filterQuery = string.Empty;
            filterQuery =
                $"(+{{!parent which=\"ParentDocument: true\" v=\"{GetACLCriterions(authorizationParametters)} AND {{!geofilt pt = {(circleSearchCriteria).Center.Lat.ToString()},{(circleSearchCriteria).Center.Lng.ToString()} sfield=GeoValue d={(circleSearchCriteria).RediusInKiloMeters.ToString()}}}\"}})";
            return filterQuery;
        }

        private string GenerateGeoPolygonSearchQuery(PolygonSearchCriteria polygonSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {

            string filterQuery = string.Empty;
            filterQuery = $"{{!parent which=\"ParentDocument: true\"}} {GeneratePolygonSearchQueryPart(polygonSearchCriteria)}";

            return filterQuery;
        }
        #endregion

        #region ImageProcessing

        public List<RetrievedFace> RetrieveImageDocument(KeyValuePair<BoundingBox, List<double>> imageEmbedding, int treshould, AuthorizationParametters authorizationParametters)
        {
            //create query
            StringBuilder query = new StringBuilder();
            query.Append("dist(2,");
            foreach (var item in imageEmbedding.Value)
            {
                query.Append(item + ",");
            }
            for (int i = 1; i <= ImageDocument.NumberOfFeatue; i++)
            {
                query.Append(ImageDocument.GetFieldName(i) + ",");
            }
            query = query.Remove(query.Length - 1, 1);
            query.Append(")");

            //retrieve from solr
            List<RetrievedFace> retrievedFaces = new List<RetrievedFace>();
            JToken docsJson = RetriveImageFromSolrByQuery(query.ToString(), treshould, authorizationParametters);

            //convert JToken to RetrievedFace
            foreach (var child in docsJson.Children())
            {
                double dist = double.Parse(child[query.ToString()].ToString());

                RetrievedFace retrievedFace = new RetrievedFace()
                {
                    distance = dist,
                    imageId = child[nameof(ImageDocument.ImageId)].ToString(),
                    boundingBox = ConvertJTokenToBoundingBox(child[nameof(FaceSpecification.BoundingBox)])
                };
                retrievedFaces.Add(retrievedFace);
            }
            return retrievedFaces;
        }

        private Landmarks ConvertJTokenToLandmarks(JToken jToken)
        {
            List<double> result = new List<double>();
            foreach (var mark in jToken)
            {
                result.Add(
                    double.Parse(mark.ToString())
                    );
            }
            return new Landmarks()
            {
                marks = result
            };
        }

        private BoundingBox ConvertJTokenToBoundingBox(JToken boundingBox)
        {
            int x = int.Parse(boundingBox[0].ToString());
            int y = int.Parse(boundingBox[1].ToString());
            int width = int.Parse(boundingBox[2].ToString());
            int height = int.Parse(boundingBox[3].ToString());
            return new BoundingBox()
            {
                topLeft = new Point(x, y),
                width = width,
                height = height,
                landmarks = new Landmarks()
                {
                    marks = new List<double>()
                }
            };
        }
        private JToken RetriveImageFromSolrByQuery(string filterQuery, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            JObject query = new JObject();
            query.Add(new JProperty("query", GetACLCriterions(authorizationParametters)));
            query.Add(new JProperty("filter", $"{{!func}}{filterQuery}"));
            query.Add(new JProperty("fields", $"id , {nameof(ImageDocument.ImageId)} , {nameof(FaceSpecification.BoundingBox)} , {filterQuery}"));
            query.Add(new JProperty("sort", $"{filterQuery} asc"));
            query.Add(new JProperty("limit", $"{resultLimit}"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ImageCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            return JObject.Parse(jsonResult["response"].ToString())["docs"];
        }
        #endregion

        #region Object Explorer
        Dictionary<string, string> ontologyLowerCaseMapping = new Dictionary<string, string>();

        public QueryResult RunStatisticalQuery(StatisticalQuery.Query query, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            List<FormulaStep> FormulaSequence = query.FormulaSequence;
            string solrQuery = GenerateStatisticalDomainQuery(FormulaSequence, ontology, authorizationParametters);
            QueryResult result = ExecuteFinalSolrQuery(solrQuery);
            if (ontologyLowerCaseMapping.Count == 0)
                FillOntologyLowerCaseMap(ontology);

            PrepareStatisticsTypeUris(result.ObjectTypePreview);
            PrepareStatisticsTypeUris(result.PropertyTypePreview);

            return result;
        }

        private void PrepareStatisticsTypeUris(List<TypeBasedStatistic> stats)
        {
            foreach (var item in stats)
            {
                string lowerCaseTypeUri = item.TypeUri.ToLower();
                if (ontologyLowerCaseMapping.ContainsKey(lowerCaseTypeUri))
                    item.TypeUri = ontologyLowerCaseMapping[lowerCaseTypeUri];
            }
        }

        private void PrepareStatItemTypeUri(TypeBasedStatistic item)
        {
        }

        private void FillOntologyLowerCaseMap(Ontology.Ontology ontology)
        {
            foreach (var item in ontology.GetAllObjectTypeURIs())
            {
                ontologyLowerCaseMapping.Add(item.ToLower(), item);
            }
            foreach (var item in ontology.GetAllOntologyRelationships())
            {
                ontologyLowerCaseMapping.Add(item.ToLower(), item);
            }
            foreach (var item in ontology.GetAllProperties())
            {
                ontologyLowerCaseMapping.Add(item.TypeName.ToLower(), item.TypeName);
            }
        }

        private string GenerateStatisticalDomainQuery(List<FormulaStep> formulaSteps, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            List<string> solrQuerySteps = new List<string>();

            if (formulaSteps.Count != 0)
            {
                foreach (FormulaStep formulaStep in formulaSteps)
                {
                    if (formulaStep is TypeBasedDrillDown)
                    {
                        solrQuerySteps.Add(AmendSolrQuery(AddTypeBasedDrillDownToStatisticalQuery(formulaStep as TypeBasedDrillDown, ontology, authorizationParametters)));
                    }
                    else if (formulaStep is PropertyValueBasedDrillDown)
                    {
                        solrQuerySteps.Add(AddPropertyValueBasedDrillDownToStatisticalQuery(formulaStep as PropertyValueBasedDrillDown, ontology, authorizationParametters));
                    }
                    else if (formulaStep is PropertyValueRangeDrillDown)
                    {
                        solrQuerySteps.Add(AddPropertyValueRangeDrillDownToStatisticalQuery(formulaStep as PropertyValueRangeDrillDown, ontology, authorizationParametters));
                    }
                    else if (formulaStep is PerformSetOperation)
                    {
                        string tempresult = AddPerformSetOperationToStatisticalQuery(solrQuerySteps, formulaStep as PerformSetOperation, ontology, authorizationParametters);
                        solrQuerySteps.Clear();
                        solrQuerySteps.Add(tempresult);
                    }
                    else if (formulaStep is LinkBasedDrillDown)
                    {
                        if (solrQuerySteps.Count == 0)
                            solrQuerySteps.Add(
                                $"{{!parent which= ParentDocument:true }}{GetACLCriterions(authorizationParametters)}");
                        break;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                string finalSolrQuery = string.Empty;
                if (solrQuerySteps.Count > 1)
                {
                    finalSolrQuery = solrQuerySteps.Aggregate((i, j) => i + " AND " + j);
                }
                else
                    finalSolrQuery = solrQuerySteps[0];

                return finalSolrQuery;
            }
            else
            {
                solrQuerySteps.Add(
                    $"{{!parent which= ParentDocument:true }}{GetACLCriterions(authorizationParametters)}");
                return solrQuerySteps[0];
            }
        }

        private string AddPropertyValueRangeDrillDownToStatisticalQuery(PropertyValueRangeDrillDown propertyValueRangeDrillDown, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = string.Empty;
            string filterQuery = string.Empty;
            string propertyTypeUri = GetFieldName(ontology.GetBaseDataTypeOfProperty(propertyValueRangeDrillDown.DrillDownDetails.NumericPropertyTypeUri).ToString());
            foreach (var barRange in propertyValueRangeDrillDown.DrillDownDetails.Bars)
            {
                filterQuery += $"{propertyTypeUri}:[{barRange.Start} TO {barRange.End}] OR ";
            }
            filterQuery = filterQuery.Remove(filterQuery.Length - 4, 4);
            filterQuery =
                $"TypeUri:{propertyValueRangeDrillDown.DrillDownDetails.NumericPropertyTypeUri} AND ({filterQuery})";

            solrQuery =
                $"({{!parent which= ParentDocument:true v=\" ({filterQuery}) AND ({GetACLCriterions(authorizationParametters)})\"}})";

            return solrQuery;
        }

        private string AddPerformSetOperationToStatisticalQuery(List<string> firstOperandQueryPortion, PerformSetOperation performSetOperation, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = string.Empty;

            string firstOperandQuery = string.Empty;
            if (firstOperandQueryPortion.Count > 1)
                firstOperandQuery = firstOperandQueryPortion.Aggregate((i, j) => i + " AND " + j);
            else
                firstOperandQuery = firstOperandQueryPortion[0];

            string secondOperandQuery = GenerateStatisticalDomainQuery(performSetOperation.JoinedSetFormulaSequence, ontology, authorizationParametters);
            switch (performSetOperation.Operator)
            {
                case Operator.Union:
                    solrQuery = $"(({firstOperandQuery}) OR ({secondOperandQuery}))";
                    break;
                case Operator.Intersection:
                    solrQuery = $"(({firstOperandQuery}) AND ({secondOperandQuery}))";
                    break;
                case Operator.SubtractRight:
                    solrQuery = $"(-({firstOperandQuery}) +({secondOperandQuery}))";
                    break;
                case Operator.SubtractLeft:
                    solrQuery = $"(+({firstOperandQuery}) -({secondOperandQuery}))";
                    break;
                case Operator.ExclusiveOr:
                    solrQuery = string.Format("(+(({0}) OR ({1})) -(({0}) AND ({1})))", firstOperandQuery, secondOperandQuery);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return solrQuery;
        }

        private static string AddTypeBasedDrillDownToStatisticalQuery(TypeBasedDrillDown typeBasedDrillDownStep, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = string.Empty;

            ContainerCriteria formulaStep = new ContainerCriteria
            {
                CriteriaSet = new CriteriaSet
                {
                    SetOperator = BooleanOperator.Any
                }
            };

            ContainerCriteria OfObjectType = new ContainerCriteria
            {
                CriteriaSet = new CriteriaSet
                {
                    SetOperator = BooleanOperator.Any
                }
            };
            ContainerCriteria hasPropertyWithType = new ContainerCriteria
            {
                CriteriaSet = new CriteriaSet
                {
                    SetOperator = BooleanOperator.Any
                }
            };
            foreach (var formulaPortion in typeBasedDrillDownStep.Portions)
            {
                if (formulaPortion is HasPropertyWithType)
                {
                    PropertyValueCriteria temp = new PropertyValueCriteria
                    {
                        PropertyTypeUri =
                        ((HasPropertyWithType)formulaPortion).PropertyTypeUri
                    };

                    switch (ontology.GetBaseDataTypeOfProperty(temp.PropertyTypeUri))
                    {
                        case BaseDataTypes.Int:
                        case BaseDataTypes.Long:
                            temp.OperatorValuePair = new LongPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = long.MinValue
                            };
                            break;
                        case BaseDataTypes.Double:
                            temp.OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = float.MinValue
                            };
                            break;
                        case BaseDataTypes.Boolean:
                            temp.OperatorValuePair = new BooleanPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = true
                            };
                            break;
                        case BaseDataTypes.DateTime:
                            temp.OperatorValuePair = new DateTimePropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = DateTime.MinValue
                            };
                            break;
                        case BaseDataTypes.GeoPoint:
                            temp.OperatorValuePair = new GeoPointPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = new GeoCircleEntityRawData() { Latitude = "0", Longitude = "0", Radius = "1000000000" }
                            };
                            break;
                        case BaseDataTypes.HdfsURI:
                        case BaseDataTypes.GeoTime:
                        case BaseDataTypes.String:
                        default:
                            temp.OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                            {
                                CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                                CriteriaValue = "*"
                            };
                            break;
                    }

                    hasPropertyWithType.CriteriaSet.Criterias.Add(temp);
                }
                else if (formulaPortion is OfObjectType)
                {
                    ObjectTypeCriteria temp = new ObjectTypeCriteria();
                    var childs = ontology.GetAllChilds(((OfObjectType)formulaPortion).ObjectTypeUri);
                    foreach (var child in childs)
                    {
                        temp.ObjectsTypeUri.Add(child);
                    }

                    OfObjectType.CriteriaSet.Criterias.Add(temp);
                }
                else
                    throw new NotSupportedException();
            }

            if (hasPropertyWithType.CriteriaSet.Criterias.Count != 0)
                formulaStep.CriteriaSet.Criterias.Add(hasPropertyWithType);

            if (OfObjectType.CriteriaSet.Criterias.Count != 0)
                formulaStep.CriteriaSet.Criterias.Add(OfObjectType);

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(formulaStep);

            AccessClient accessClient = new AccessClient();
            accessClient.GenerateSearchQuery(criteriaSet, authorizationParametters, ontology, ref solrQuery);

            return solrQuery;
        }

        private static string AddPropertyValueBasedDrillDownToStatisticalQuery(PropertyValueBasedDrillDown propertyValueBasedDrillDownStep, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = string.Empty;

            ContainerCriteria formulaStep = new ContainerCriteria
            {
                CriteriaSet = new CriteriaSet
                {
                    SetOperator = BooleanOperator.Any
                }
            };

            foreach (var formulaPortion in propertyValueBasedDrillDownStep.Portions)
            {
                PropertyValueCriteria temp = new PropertyValueCriteria
                {
                    PropertyTypeUri = formulaPortion.PropertyTypeUri
                };
                switch (ontology.GetBaseDataTypeOfProperty(temp.PropertyTypeUri))
                {
                    case BaseDataTypes.Int:
                    case BaseDataTypes.Long:
                        temp.OperatorValuePair = new LongPropertyCriteriaOperatorValuePair
                        {
                            CriteriaOperator = LongPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                            CriteriaValue = long.Parse(formulaPortion.PropertyValue)
                        };
                        break;
                    case BaseDataTypes.Double:
                        temp.OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair
                        {
                            CriteriaOperator = FloatPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                            CriteriaValue = float.Parse(formulaPortion.PropertyValue)
                        };
                        break;
                    case BaseDataTypes.Boolean:
                        temp.OperatorValuePair = new BooleanPropertyCriteriaOperatorValuePair
                        {
                            CriteriaOperator = BooleanPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                            CriteriaValue = bool.Parse(formulaPortion.PropertyValue)
                        };
                        break;
                    case BaseDataTypes.DateTime:
                        temp.OperatorValuePair = new DateTimePropertyCriteriaOperatorValuePair
                        {
                            CriteriaOperator = DateTimePropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                            CriteriaValue = DateTime.Parse(formulaPortion.PropertyValue)
                        };
                        break;
                    case BaseDataTypes.HdfsURI:
                    case BaseDataTypes.GeoTime:
                    case BaseDataTypes.String:
                    default:
                        temp.OperatorValuePair = new StringPropertyCriteriaOperatorValuePair
                        {
                            CriteriaOperator = StringPropertyCriteriaOperatorValuePair.RelationalOperator.Equals,
                            CriteriaValue = formulaPortion.PropertyValue
                        };
                        break;
                }

                formulaStep.CriteriaSet.Criterias.Add(temp);

            }

            CriteriaSet criteriaSet = new CriteriaSet();
            criteriaSet.Criterias.Add(formulaStep);

            AccessClient accessClient = new AccessClient();
            accessClient.GenerateSearchQuery(criteriaSet, authorizationParametters, ontology, ref solrQuery);

            return solrQuery;
        }

        private string AmendSolrQuery(string solrQuery, BaseDataTypes dataType = BaseDataTypes.None)
        {
            solrQuery = solrQuery.Replace("\\\"*\\\"", "*");
            solrQuery = solrQuery.Replace("DateTimeValue: [0001-01-01T00:00:00.000Z TO 0001-01-01T00:00:00.000Z]", "DateTimeValue:*");

            if (dataType == BaseDataTypes.DateTime)
            {
                solrQuery = solrQuery.Replace("LongValue: -62135596800000", "LongValue:*");
            }
            else
            {
                solrQuery = solrQuery.Replace("LongValue: -9223372036854775808 ", "LongValue:* ");
                solrQuery = solrQuery.Replace("LongValue: 0 ", "LongValue:* ");
            }

            solrQuery = solrQuery.Replace("DateRangeValue: [0001-01-01T00:00:000Z TO 0001-01-01T00:00:000Z]", "DateRangeValue:*");
            solrQuery = solrQuery.Replace("DoubleValue: -3.402823E+38", "DoubleValue:*");
            solrQuery = solrQuery.Replace("TypeUri: زمان_و_موقعیت_جغرافیایی AND KeywordTokenizedStringValue:", "TypeUri: زمان_و_موقعیت_جغرافیایی AND DateRangeValue:");
            solrQuery = solrQuery.Replace("BooleanValue: True", "BooleanValue: *");

            return solrQuery;
        }

        private QueryResult ExecuteFinalSolrQuery(string solrQuery)
        {
            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };
            JObject jFacet = JObject.Parse(@"{""ObjectTypes"":{""type"": ""terms"", ""limit"": 1000, ""field"" : ""OwnerObjectTypeUri"",""facet"":{""UniqueTypeUriCount"":{""type"":""query"",""facet"":	{""TargetNodeTypeUriCount"":""unique(OwnerObjectID)""} }},""domain"":{""blockChildren"" :""ParentDocument: true""}}, ""PropertyTypes"":{""type"" : ""terms"", ""limit"": 10000, ""field"" : ""TypeUri"" ,""domain"":{""blockChildren"" : ""ParentDocument: true""}	}}");
            query.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                QueryResult result = new QueryResult
                {
                    ObjectTypePreview = new List<TypeBasedStatistic>(),
                    PropertyTypePreview = new List<TypeBasedStatistic>()
                };

                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    var PropertyTypesFacets = JObject.Parse(jsonResult["facets"].ToString())["PropertyTypes"]["buckets"];
                    var ObjectTypesFacets = JObject.Parse(jsonResult["facets"].ToString())["ObjectTypes"]["buckets"];

                    foreach (var child in PropertyTypesFacets.Children())
                    {
                        StatisticalQuery.ResultNode.TypeBasedStatistic propertyBasedStatistic = new StatisticalQuery.ResultNode.TypeBasedStatistic
                        {
                            TypeUri = (string)child["val"],
                            Frequency = (long)child["count"]
                        };
                        result.PropertyTypePreview.Add(propertyBasedStatistic);
                    }

                    foreach (var child in ObjectTypesFacets.Children())
                    {
                        StatisticalQuery.ResultNode.TypeBasedStatistic objectBasedStatistic = new StatisticalQuery.ResultNode.TypeBasedStatistic
                        {
                            TypeUri = (string)child["val"],
                            Frequency = (long)child["UniqueTypeUriCount"]["TargetNodeTypeUriCount"]
                        };
                        result.ObjectTypePreview.Add(objectBasedStatistic);
                    }
                }

                return result;
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        public PropertyBarValues RetrievePropertyBarValuesStatistics(StatisticalQuery.Query query, string numericPropertyTypeUri, long bucketCount, double minValue, double maxValue, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);
            if (minValue.Equals(double.MinValue) && maxValue.Equals(double.MaxValue))
            {
                FindMinMax(solrQuery, numericPropertyTypeUri, ref minValue, ref maxValue, ontology);
            }

            double gap = 0.0;

            //minValue = minValue - 1;
            maxValue = maxValue + 1;

            if (ontology.GetBaseDataTypeOfProperty(numericPropertyTypeUri).ToString() != "Double")
            {
                gap = Math.Round(Math.Abs((maxValue - minValue) / bucketCount));
                if (gap == 0)
                    gap = 1;
            }
            else
                gap = Convert.ToDouble(Math.Abs((maxValue - minValue) / bucketCount));


            if (solrQuery == "*:*")
                solrQuery = "";
            JObject finalQuery = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };
            JObject jFacet = JObject.Parse($@"	{{
                                                  ""Buckets"" : {{
                                                    ""type"": ""range"",
                                                    ""field"" : ""{GetFieldName(ontology.GetBaseDataTypeOfProperty(numericPropertyTypeUri).ToString())}"",
                                                    ""start"" : {minValue},
                                                    ""end"" : {maxValue},
                                                    ""gap"" : {gap},
	                                                ""domain"":{{
			                                                ""blockChildren"" : ""ParentDocument: true"",
                                                            ""filter"": "" {GetACLCriterions(authorizationParametters)} AND TypeUri: {numericPropertyTypeUri}""
                                                            }}
                                                    }}
                                                }}           
                                            ");
            finalQuery.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(finalQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                PropertyBarValues result = new PropertyBarValues
                {
                    Bars = new List<PropertyBarValue>()
                };

                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    var propertyValueFacet = JObject.Parse(jsonResult["facets"].ToString())["Buckets"]["buckets"];

                    foreach (var child in propertyValueFacet.Children())
                    {
                        PropertyBarValue propertyBasedStatistic = new PropertyBarValue
                        {
                            Start = (double)child["val"],
                            End = (double)child["val"] + gap,
                            Count = (long)child["count"]
                        };
                        result.Bars.Add(propertyBasedStatistic);
                    }
                }

                result.BucketCount = bucketCount;
                result.Start = minValue;
                result.End = maxValue - 1;

                return result;
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        private void FindMinMax(string solrQuery, string numericPropertyTypeUri, ref double minValue, ref double maxValue, Ontology.Ontology ontology)
        {
            if (solrQuery == "*:*")
                solrQuery = "";
            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };
            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MinMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Min"": ""min({0})"",
                                                           ""Max"": ""max({0})""
                                                         }},
                                                 ""domain"":{{
			                                            ""blockChildren"" : ""ParentDocument:true"",
			                                            ""filter"": ""TypeUri: {1}""
			                                            }}
                                                    }}
                                             }}	           
                                            ", GetFieldName(ontology.GetBaseDataTypeOfProperty(numericPropertyTypeUri).ToString()), numericPropertyTypeUri));
            query.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    minValue = (double)JObject.Parse(jsonResult["facets"].ToString())["MinMax"]["Min"];
                    maxValue = (double)JObject.Parse(jsonResult["facets"].ToString())["MinMax"]["Max"];
                }
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        public DateTimePropertyBarValues RetrieveDateTimePropertyBarValuesStatistics(StatisticalQuery.Query query, string dateTimePropertyTypeUri, DateTime minValue, DateTime maxValue, BinSizes gap, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);
            string strMinValue = string.Empty;
            string strMaxValue = string.Empty;
            if (minValue.Equals(DateTime.MinValue) && maxValue.Equals(DateTime.MaxValue))
            {
                FindDateTimeMinMax(solrQuery, dateTimePropertyTypeUri, ref minValue, ref maxValue, ontology);
            }


            if (solrQuery == "*:*")
                solrQuery = string.Empty;
            JObject finalQuery = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };
            JObject jFacet = JObject.Parse($@"	{{
                                                  ""Buckets"" : {{
                                                    ""type"": ""range"",
                                                    ""field"" : ""{GetFieldName(ontology.GetBaseDataTypeOfProperty(dateTimePropertyTypeUri).ToString())}"",
                                                    ""start"" : {minValue.ToString(CultureInfo.InvariantCulture)},
                                                    ""end"" : {maxValue.ToString(CultureInfo.InvariantCulture)},
                                                    ""gap"" : {ConvertBinSizesToString(gap)},
	                                                ""domain"":{{
			                                                ""blockChildren"" : ""ParentDocument: true"",
                                                            ""filter"": "" {GetACLCriterions(authorizationParametters)} AND TypeUri: {dateTimePropertyTypeUri}""
                                                            }}
                                                    }}
                                                }}           
                                            ");
            finalQuery.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(finalQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                DateTimePropertyBarValues result = new DateTimePropertyBarValues
                {
                    Bars = new List<DateTimePropertyBarValue>()
                };

                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    var propertyValueFacet = JObject.Parse(jsonResult["facets"].ToString())["Buckets"]["buckets"];

                    foreach (var child in propertyValueFacet.Children())
                    {
                        DateTimePropertyBarValue dateTimePropertyBasedStatistic = new DateTimePropertyBarValue
                        {
                            Start = ConvertUTCToDateTime(child["val"]),
                            Count = (long)child["count"]
                        };
                        result.Bars.Add(dateTimePropertyBasedStatistic);
                    }
                }

                return result;
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }


        public DateTimePropertyStackValues RetrieveDateTimePropertyBarValuesStatistics(StatisticalQuery.Query query, List<long> objectID, List<string> dateTimePropertyTypeUris, DateTime minValue, DateTime maxValue, BinSizes gap, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            //ToDo Insert objectIDs in to the solrQuery 
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);

            if (minValue.Equals(DateTime.MinValue) && maxValue.Equals(DateTime.MaxValue))
            {
                minValue = DateTime.MaxValue; maxValue = DateTime.MinValue;
                DateTime tempMinValue = DateTime.Now, tempMaxValue = DateTime.Now;
                foreach (var dateTimePropertyTypeUri in dateTimePropertyTypeUris)
                {
                    FindDateTimeMinMax(solrQuery, dateTimePropertyTypeUri, ref tempMinValue, ref tempMaxValue, ontology);
                    if (tempMinValue < minValue)
                        minValue = tempMaxValue;
                    if (tempMaxValue > maxValue)
                        minValue = tempMaxValue;
                }
            }

            if (solrQuery == "*:*")
                solrQuery = string.Empty;
            JObject finalQuery = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };

            string jsonFacet = "{ ";
            foreach (var dateTimePropertyTypeUri in dateTimePropertyTypeUris)
            {
                jsonFacet += string.Format(
                    @"                    ""{5}"" : {{
                                                    ""type"": ""range"",
                                                    ""field"" : ""{0}"",
                                                    ""start"" : {1},
                                                    ""end"" : {2},
                                                    ""gap"" : {3},
	                                                ""domain"":{{
			                                                ""blockChildren"" : ""ParentDocument: true"",
                                                            ""filter"": "" {4} AND TypeUri: {5}""
                                                            }}
                                                    }}                                                          
                                            ",
                    GetFieldName(ontology.GetBaseDataTypeOfProperty(dateTimePropertyTypeUri).ToString()), minValue.ToString(),
                    maxValue.ToString(), ConvertBinSizesToString(gap), GetACLCriterions(authorizationParametters),
                    dateTimePropertyTypeUri);
            }
            jsonFacet += " }";

            JObject jFacet = JObject.Parse(jsonFacet);
            finalQuery.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(finalQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                DateTimePropertyStackValues result = new DateTimePropertyStackValues
                {
                    Bars = new List<List<DateTimePropertyStackValue>>()
                };

                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    foreach (var dateTimePropertyTypeUri in dateTimePropertyTypeUris)
                    {
                        List<DateTimePropertyStackValue> temp = new List<DateTimePropertyStackValue>();

                        var propertyValueFacet = JObject.Parse(jsonResult["facets"].ToString())[dateTimePropertyTypeUri]["buckets"];

                        foreach (var child in propertyValueFacet.Children())
                        {
                            DateTimePropertyStackValue dateTimePropertyBasedStatistic = new DateTimePropertyStackValue
                            {
                                Start = ConvertUTCToDateTime(child["val"]),
                                Count = (long)child["count"]
                            };
                            temp.Add(dateTimePropertyBasedStatistic);
                        }

                        result.Bars.Add(temp);
                    }
                }

                return result;
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        private object ConvertBinSizesToString(BinSizes gap)
        {
            throw new NotImplementedException();
        }

        private string ConvertDateTimeToUTC(DateTime minValue)
        {
            throw new NotImplementedException();
        }

        private DateTime ConvertUTCToDateTime(JToken jToken)
        {
            throw new NotImplementedException();
        }

        private void FindDateTimeMinMax(string solrQuery, string dateTimePropertyTypeUri, ref DateTime minValue, ref DateTime maxValue, Ontology.Ontology ontology)
        {
            if (solrQuery == "*:*")
                solrQuery = "";
            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };
            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MinMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Min"": ""min({0})"",
                                                           ""Max"": ""max({0})""
                                                         }},
                                                 ""domain"":{{
			                                            ""blockChildren"" : ""ParentDocument:true"",
			                                            ""filter"": ""TypeUri: {1}""
			                                            }}
                                                    }}
                                             }}	           
                                            ", GetFieldName(ontology.GetBaseDataTypeOfProperty(dateTimePropertyTypeUri).ToString()), dateTimePropertyTypeUri));
            query.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    minValue = ConvertUTCToDateTime(JObject.Parse(jsonResult["facets"].ToString())["MinMax"]["Min"]);
                    maxValue = ConvertUTCToDateTime(JObject.Parse(jsonResult["facets"].ToString())["MinMax"]["Max"]);
                }
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }



        public LinkTypeStatistics RetrieveLinkTypeStatistics(StatisticalQuery.Query query, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            List<FormulaStep> FormulaSequence = query.FormulaSequence;
            string solrQuery = GenerateStatisticalDomainQuery(FormulaSequence, ontology, authorizationParametters);

            return ExecuteFinalSolrLinksStatisticalQuery(solrQuery, authorizationParametters);
        }

        private LinkTypeStatistics ExecuteFinalSolrLinksStatisticalQuery(string solrQuery, AuthorizationParametters authorizationParametters)
        {
            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", "0")
            };
            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{	
	                                            ""SourceLinkTypes"" : {{        
	                                               ""type"" : ""terms"",     
	                                               ""field"" : ""LinkTypeUri"", 	
	                                               ""domain"":{{
				                                            ""filter"":""{0}"",
                                                            ""join"":{{ 
                                                                        ""from"": ""id"", ""to"": ""SourceObjectId""
                                                                     }}
                                                    }}
                                                }},

                                                ""SourceObjectTypes"" : {{        
	                                               ""type"" : ""terms"",     
	                                               ""field"" : ""TargetObjectTypeUri"",
	                                               ""domain"":{{
	   			                                            ""filter"": ""{0}"",
				                                            ""join"":{{
                                                                      ""from"":""id"", ""to"":""SourceObjectId""
                                                                        }}
				                                            }},
		                                            ""facet"":{{	
			                                            ""TypeUriCount"":{{
				                                            ""type"":	""query"",	
				                                            ""facet"":	{{
                                                                         ""TargetNodeTypeUriCount"": ""unique(TargetObjectId)""
                                                                        }}   
	                                            }}
	                                            }}			
                                               }}

                                             }}
                                            ", GetACLCriterions(authorizationParametters)));
            query.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                LinkTypeStatistics result = new LinkTypeStatistics
                {
                    LinkedObjectTypes = new List<StatisticalQuery.ResultNode.TypeBasedStatistic>(),
                    LinkTypes = new List<StatisticalQuery.ResultNode.TypeBasedStatistic>()
                };

                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    var SourceLinkTypesFacets = JObject.Parse(jsonResult["facets"].ToString())["SourceLinkTypes"]["buckets"];
                    var SourceObjectTypesFacets = JObject.Parse(jsonResult["facets"].ToString())["SourceObjectTypes"]["buckets"];

                    foreach (var child in SourceLinkTypesFacets.Children())
                    {
                        TypeBasedStatistic propertyBasedStatistic = new TypeBasedStatistic
                        {
                            TypeUri = (string)child["val"],
                            Frequency = (long)child["count"]
                        };
                        result.LinkTypes.Add(propertyBasedStatistic);
                    }

                    foreach (var child in SourceObjectTypesFacets.Children())
                    {
                        TypeBasedStatistic objectBasedStatistic = new TypeBasedStatistic
                        {
                            TypeUri = (string)child["val"],
                            Frequency = (long)child["TypeUriCount"]["TargetNodeTypeUriCount"]
                        };
                        result.LinkedObjectTypes.Add(objectBasedStatistic);
                    }

                }

                return result;
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        public long[] RetrieveChartBinsObjectIDsByStatisticalQuery(StatisticalQuery.Query query, int PassObjectsCountLimit, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            List<long> result = new List<long>();
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);

            FormulaStep lastFormula = query.FormulaSequence.Last();

            string filterQuery = string.Empty;
            string propertyTypeUri = GetFieldName(((PropertyValueRangeDrillDown)lastFormula).DrillDownDetails.NumericPropertyTypeUri);
            foreach (var barRange in ((PropertyValueRangeDrillDown)lastFormula).DrillDownDetails.Bars)
            {
                filterQuery += $"{propertyTypeUri}:[{barRange.Start} TO {barRange.End}] OR ";
            }
            filterQuery = filterQuery.Remove(filterQuery.Length - 4, 4);

            JObject SolrQuery = new JObject
            {
                new JProperty("query", solrQuery),
                new JProperty("filter", filterQuery),
                new JProperty("limit", PassObjectsCountLimit)
            };
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(SolrQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            JToken queryResult = JObject.Parse(jsonResult["response"].ToString())["docs"];
            foreach (var child in queryResult.Children())
            {
                var docId = (long)child["id"];
                result.Add(docId);
            }

            return result.ToArray();
        }

        public long[] RetrieveLinkedObjectIDsByStatisticalQuery(StatisticalQuery.Query query, int PassObjectsCountLimit, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            List<long> result = new List<long>();
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);
            string findTargetSolrQuery = "{!join from=id to=SourceObjectId}" + solrQuery;
            string filterQuery = string.Empty;
            string targetObjects = string.Empty;
            string linkTypes = string.Empty;
            FormulaStep lastFormulaStep = query.FormulaSequence.Last();
            foreach (var item in ((LinkBasedDrillDown)lastFormulaStep).Portions)
            {
                if (item is LinkedObjectTypeBasedDrillDown)
                    targetObjects += "TargetObjectTypeUri:" + ((LinkedObjectTypeBasedDrillDown)item).LinkedObjectTypeUri + " OR ";
                else
                    linkTypes += "LinkTypeUri:" + ((LinkTypeBasedDrillDown)item).LinkTypeUri + " OR ";
            }
            if (targetObjects.Length > 0)
            {
                targetObjects = targetObjects.Remove(targetObjects.Length - 4, 4);
                filterQuery = targetObjects;
            }
            if (linkTypes.Length > 0)
            {
                linkTypes = linkTypes.Remove(linkTypes.Length - 4, 4);
                if (filterQuery.Length > 0)
                    filterQuery = $"({targetObjects}) AND ({linkTypes})";
                else
                    filterQuery = linkTypes;
            }

            JObject SolrQuery = new JObject
            {
                new JProperty("query", findTargetSolrQuery),
                new JProperty("filter", filterQuery),
                new JProperty("limit", PassObjectsCountLimit)
            };
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(SolrQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            JToken queryResult = JObject.Parse(jsonResult["response"].ToString())["docs"];
            foreach (var child in queryResult.Children())
            {
                var targetObjectId = (long)child["TargetObjectId"];
                result.Add(targetObjectId);
            }

            return result.ToArray();
        }

        public long[] RetrieveObjectIDsByStatisticalQuery(StatisticalQuery.Query query, int PassObjectsCountLimit, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            List<long> result = new List<long>();
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);

            JObject SolrQuery = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
                new JProperty("limit", PassObjectsCountLimit)
            };
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(SolrQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
            JToken queryResult = JObject.Parse(jsonResult["response"].ToString())["docs"];
            foreach (var child in queryResult.Children())
            {
                var docId = (long)child["id"];
                result.Add(docId);
            }

            return result.ToArray();
        }

        public PropertyValueStatistics RetrievePropertyValueStatistics
            (StatisticalQuery.Query query, string exploredPropertyTypeUri, int startOffset, int resultsLimit
            , long minimumCount, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            string solrQuery = GenerateStatisticalDomainQuery(query.FormulaSequence, ontology, authorizationParametters);

            JObject finalQuery = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("filter", solrQuery),
            };

            string tempQuery = $@"{{
                                                            ""PropertyHistogram"":{{
		                                                        ""type"" : ""terms"",     
		                                                        ""field"" : ""{GetFieldName(ontology.GetBaseDataTypeOfProperty(exploredPropertyTypeUri).ToString())}"",
		                                                        ""sort"": ""count"",
                                                                ""numBuckets"": true,
                                                                ""allBuckets"": true,
                                                                ""limit"": {resultsLimit},
                                                                ""offset"": {startOffset},
                                                                ""mincount"": {minimumCount},
                                                                ""domain"":
                                                                            {{ 
                                                                            ""filter"": ""TypeUri: {exploredPropertyTypeUri}"",
                                                                            ""blockChildren"" : ""ParentDocument: true""
                                                                            }}
                                                                }}
                                                            }}";

            JObject jFacet = JObject.Parse(tempQuery);

            finalQuery.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(finalQuery, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status != 0)
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }

            PropertyValueStatistics result = new PropertyValueStatistics()
            {
                Results = new List<PropertyValueStatistic>()
            };
            if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
            {
                result.TotalResultsCount = (int)JObject.Parse(jsonResult["facets"].ToString())["PropertyHistogram"]["numBuckets"];
                var propertyValueBuckets = JObject.Parse(jsonResult["facets"].ToString())["PropertyHistogram"]["buckets"];

                foreach (var child in propertyValueBuckets.Children())
                {
                    PropertyValueStatistic propertyValueStatistic = new PropertyValueStatistic()
                    {
                        PropertyValue = child["val"].ToString(),
                        Frequency = long.Parse(child["count"].ToString())
                    };
                    result.Results.Add(propertyValueStatistic);
                }
            }
            else
            {
                throw new Exception(string.Format("Result Count is Equal To Zero", response));
            }
            return result;
        }

        private string GetFieldName(string fieldType, bool isDateTimeLong = false)
        {
            Property propertyType = new Property();

            switch (fieldType)
            {
                case "Int":
                    return nameof(propertyType.LongValue);
                case "Boolean":
                    return nameof(propertyType.BooleanValue);
                case "DateTime":
                    return isDateTimeLong ? nameof(propertyType.LongValue) : nameof(propertyType.DateTimeValue);
                case "String":
                    return nameof(propertyType.KeywordTokenizedStringValue);
                case "Double":
                    return nameof(propertyType.DoubleValue);
                case "HdfsURI":
                    return nameof(propertyType.KeywordTokenizedStringValue);
                case "Long":
                    return nameof(propertyType.LongValue);
                case "None":
                case "GeoTime":
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Timeline

        private Dictionary<DateTime, long> RetrieveDateTimePropertyCountByBinLevel(string typeUri, string binLevel, DateTime minValue, DateTime maxValue, AuthorizationParametters authorizationParametters)
        {
            Dictionary<DateTime, long> result = new Dictionary<DateTime, long>();

            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("fields", "id"),
                new JProperty("filter", string.Format("TypeUri:{0}",typeUri))
            };

            // long mincount = 0;

            DateTime univDateTimeMin = minValue.ToUniversalTime();
            DateTime univDateTimeMax = maxValue.ToUniversalTime();

            DateTime x1 = new DateTime(univDateTimeMin.Year, 1, 1);
            DateTime x2 = new DateTime(univDateTimeMax.Year + 2, 1, 1);

            string startDate = x1.ToString("s") + "Z";
            string endDate = x2.ToString("s") + "Z";

            JObject jFacet = JObject.Parse($@" {{
                                                  ""Buckets"" : {{
                                                    ""type"" : ""range"",
                                                    ""field"" : ""DateTimeValue"",
                                                    ""start"" : ""{startDate}"",
                                                    ""end"" : ""{endDate}"",
                                                    ""gap"" : ""{binLevel}""
                                                    }}
                                               }}          
                                            ");

            query.Add(new JProperty("facet", jFacet));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                {
                    var propertyValueFacet = JObject.Parse(jsonResult["facets"].ToString())["Buckets"]["buckets"];

                    foreach (var child in propertyValueFacet.Children())
                    {
                        DateTime currentTime = ConvertUTCToDateTime(child["val"]);
                        long currentValue = (long)child["count"];

                        result.Add(currentTime, currentValue);
                    }
                }
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }

            return result;
        }

        private Dictionary<DateTime, long> RetrieveDateRangePropertyCountByBinLevel(string typeUri, string binLevel, DateTime minValue, DateTime maxValue, AuthorizationParametters authorizationParametters)
        {
            Dictionary<DateTime, long> result = new Dictionary<DateTime, long>();

            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("fields", "id"),
                new JProperty("filter", string.Format("TypeUri:{0}",typeUri))
            };

            long mincount = 0;

            string startDate = minValue.ToString("s") + "Z";
            string endDate = maxValue.AddYears(2).ToString("s") + "Z";

            JObject jParams = JObject.Parse($@"	{{
                                                    ""facet"" : ""true"",
                                                    ""facet.range"" : ""DateRangeValue"",
                                                    ""facet.range.start"" : ""{startDate}"",
                                                    ""facet.range.end"" : ""{endDate}"",
                                                    ""facet.range.gap"" : ""{binLevel}"",
                                                    ""facet.mincount"": {mincount}
                                                }}          
                                            ");

            query.Add(new JProperty("params", jParams));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var dateRangeValueFacet = JObject.Parse(jsonResult["facet_counts"].ToString())["facet_ranges"]["DateRangeValue"];
                if (dateRangeValueFacet != null)
                {
                    var children = dateRangeValueFacet.SelectToken("counts");
                    if (children.Count() != 0)
                    {
                        long i = 0;
                        DateTime dt = DateTime.MinValue;
                        long num = -100;

                        foreach (var item in children)
                        {
                            if ((i % 2) == 0)
                                dt = DateTime.Parse(item.ToString());
                            else if ((i % 2) != 0)
                                num = long.Parse(item.ToString());

                            if (num != -100 && dt != DateTime.MinValue)
                            {
                                result.Add(dt, num);

                                dt = DateTime.MinValue;
                                num = -100;
                            }

                            i++;
                        }
                    }
                }
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }

            return result;
        }

        public long GetTimelineMaxFrequecyCount(List<string> propertiesTypeUri, string binLevel, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            Dictionary<DateTime, long> result = new Dictionary<DateTime, long>();

            foreach (var property in propertiesTypeUri)
            {
                BaseDataTypes dateType = ontology.GetBaseDataTypeOfProperty(property);
                if (dateType == BaseDataTypes.DateTime)
                {
                    DateTime minValue = DateTime.MinValue;
                    DateTime maxValue = DateTime.MaxValue;
                    FindDateTimeMinMax("*:*", property, ref minValue, ref maxValue, ontology);

                    var list = RetrieveDateTimePropertyCountByBinLevel(property, binLevel, minValue, maxValue, authorizationParametters);
                    foreach (var item in list)
                    {
                        if (result.ContainsKey(item.Key))
                        {
                            DateTime currentTime = ConvertUTCToDateTime(item.Key);
                            long currentValue = item.Value;

                            result[currentTime] += currentValue;
                        }
                        else
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                }
                else if (dateType == BaseDataTypes.GeoTime)
                {
                    DateTime minValue = DateTime.MinValue;
                    DateTime maxValue = DateTime.MaxValue;
                    FindDateRangeMinMax(property, ref minValue, ref maxValue, ontology);

                    var list = RetrieveDateRangePropertyCountByBinLevel(property, binLevel, minValue, maxValue, authorizationParametters);
                    foreach (var item in list)
                    {
                        if (result.ContainsKey(item.Key))
                        {
                            DateTime currentTime = ConvertUTCToDateTime(item.Key);
                            long currentValue = item.Value;

                            result[currentTime] += currentValue;
                        }
                        else
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            if (result.Count != 0)
                return result.OrderBy(r => r.Value).LastOrDefault().Value;

            return 0;
        }

        public DateTime GetTimelineMaxDate(List<string> propertiesTypeUri, string binLevel, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            Dictionary<DateTime, long> result = new Dictionary<DateTime, long>();

            foreach (var property in propertiesTypeUri)
            {
                BaseDataTypes dateType = ontology.GetBaseDataTypeOfProperty(property);
                if (dateType == BaseDataTypes.DateTime)
                {
                    DateTime minValue = DateTime.MinValue;
                    DateTime maxValue = DateTime.MaxValue;
                    FindDateTimeMinMax("*:*", property, ref minValue, ref maxValue, ontology);

                    var list = RetrieveDateTimePropertyCountByBinLevel(property, binLevel, minValue, maxValue, authorizationParametters);
                    foreach (var item in list)
                    {
                        if (result.ContainsKey(item.Key))
                        {
                            DateTime currentTime = ConvertUTCToDateTime(item.Key);
                            long currentValue = item.Value;

                            long thisCount = result[currentTime];
                            if (currentValue > thisCount)
                                result[currentTime] = currentValue;
                        }
                        else
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                }
                else if (dateType == BaseDataTypes.GeoTime)
                {
                    DateTime minValue = DateTime.MinValue;
                    DateTime maxValue = DateTime.MaxValue;
                    FindDateRangeMinMax(property, ref minValue, ref maxValue, ontology);

                    var list = RetrieveDateRangePropertyCountByBinLevel(property, binLevel, minValue, maxValue, authorizationParametters);
                    foreach (var item in list)
                    {
                        if (result.ContainsKey(item.Key))
                        {
                            DateTime currentTime = ConvertUTCToDateTime(item.Key);
                            long currentValue = item.Value;

                            long thisCount = result[currentTime];
                            if (currentValue > thisCount)
                                result[currentTime] = currentValue;
                        }
                        else
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            if (result.Count != 0)
                return result.Max(x => x.Key);

            return DateTime.MaxValue;
        }

        public DateTime GetTimelineMinDate(List<string> propertiesTypeUri, string binLevel, Ontology.Ontology ontology, AuthorizationParametters authorizationParametters)
        {
            Dictionary<DateTime, long> result = new Dictionary<DateTime, long>();

            foreach (var property in propertiesTypeUri)
            {
                BaseDataTypes dateType = ontology.GetBaseDataTypeOfProperty(property);
                if (dateType == BaseDataTypes.DateTime)
                {
                    DateTime minValue = DateTime.MinValue;
                    DateTime maxValue = DateTime.MaxValue;
                    FindDateTimeMinMax("*:*", property, ref minValue, ref maxValue, ontology);

                    var list = RetrieveDateTimePropertyCountByBinLevel(property, binLevel, minValue, maxValue, authorizationParametters);
                    foreach (var item in list)
                    {
                        if (result.ContainsKey(item.Key))
                        {
                            DateTime currentTime = ConvertUTCToDateTime(item.Key);
                            long currentValue = item.Value;

                            long thisCount = result[currentTime];
                            if (currentValue > thisCount)
                                result[currentTime] = currentValue;
                        }
                        else
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                }
                else if (dateType == BaseDataTypes.GeoTime)
                {
                    DateTime minValue = DateTime.MinValue;
                    DateTime maxValue = DateTime.MaxValue;
                    FindDateRangeMinMax(property, ref minValue, ref maxValue, ontology);

                    var list = RetrieveDateRangePropertyCountByBinLevel(property, binLevel, minValue, maxValue, authorizationParametters);
                    foreach (var item in list)
                    {
                        if (result.ContainsKey(item.Key))
                        {
                            DateTime currentTime = ConvertUTCToDateTime(item.Key);
                            long currentValue = item.Value;

                            long thisCount = result[currentTime];
                            if (currentValue > thisCount)
                                result[currentTime] = currentValue;
                        }
                        else
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }
                }
            }

            if (result.Count != 0)
                return result.Min(x => x.Key);

            return DateTime.MinValue;
        }

        private List<DateTime> SortAscending(List<DateTime> list)
        {
            list.Sort((a, b) => a.CompareTo(b));
            return list;
        }

        private void FindDateRangeMinMax(string dateTimePropertyTypeUri, ref DateTime minValue, ref DateTime maxValue, Ontology.Ontology ontology)
        {
            List<DateTime> result = new List<DateTime>();

            JObject query = new JObject
            {
                new JProperty("query", "*:*"),
                new JProperty("fields", "id"),
                new JProperty("filter", string.Format("TypeUri:{0}",dateTimePropertyTypeUri))
            };

            long mincount = 1;

            string startDate = DateTime.MinValue.ToString("s") + "Z";
            string endDate = DateTime.MaxValue.ToString("s") + "Z";

            JObject jParams = JObject.Parse($@"	{{
                                                    ""facet"" : ""true"",
                                                    ""facet.range"" : ""DateRangeValue"",
                                                    ""facet.range.start"" : ""{startDate}"",
                                                    ""facet.range.end"" : ""{endDate}"",
                                                    ""facet.range.gap"" : ""+1YEAR"",
                                                    ""facet.mincount"": {mincount}
                                                }}          
                                            ");

            query.Add(new JProperty("params", jParams));

            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);

            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var dateRangeValueFacet = JObject.Parse(jsonResult["facet_counts"].ToString())["facet_ranges"]["DateRangeValue"];
                if (dateRangeValueFacet != null)
                {
                    var children = dateRangeValueFacet.SelectToken("counts");
                    if (children.Count() != 0)
                    {
                        long i = 0;
                        DateTime dt = DateTime.MinValue;

                        foreach (var item in children)
                        {
                            if ((i % 2) == 0)
                                dt = DateTime.Parse(item.ToString());

                            if (dt != DateTime.MinValue)
                            {
                                result.Add(dt);

                                dt = DateTime.MinValue;
                            }

                            i++;
                        }
                    }
                }
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }

            var sortedList = SortAscending(result);
            minValue = sortedList.FirstOrDefault();
            maxValue = sortedList.LastOrDefault();
        }

        #endregion

        #region TextualSearch

        public string CleanseSolrString(string value)
        {
            string result = "";
            result = value.Replace("\\", "\\\\");
            result = result.Replace("\"", "\\\"");
            result = result.Replace("~", "\\~");
            result = result.Replace("+", "\\+");
            result = result.Replace("-", "\\-");
            result = result.Replace("&", "\\&");
            result = result.Replace("|", "\\|");
            result = result.Replace("!", "\\!");
            result = result.Replace("(", "\\(");
            result = result.Replace(")", "\\)");
            result = result.Replace("{", "\\{");
            result = result.Replace("}", "\\}");
            result = result.Replace("[", "\\[");
            result = result.Replace("]", "\\]");
            result = result.Replace("^", "\\^");
            //result = result.Replace("*", "\\*");
            //result = result.Replace("?", "\\?");
            result = result.Replace(":", "\\:");

            return result;
        }

        private string CleanHighlightResults(string input)
        {
            return input.Replace(@"&#x2F;", @"/")
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&#x27;", "'")
                        .Replace("&quot;", "\"");
        }

        private void TextualExecuteStringQuery(TextualSearch.BaseSearchCriteria tempQuery, ref string solrQuery)
        {
            TextualSearch.StringBasedCriteria criteria = tempQuery as TextualSearch.StringBasedCriteria;
            if (criteria.CriteriaValue != null && criteria.CriteriaValue != "")
            {
                string query = $"({criteria.CriteriaName}: {criteria.CriteriaValue}  )";
                solrQuery += query;
                solrQuery += " AND ";
            }
        }

        private void TextualExecuteDoubleQuery(TextualSearch.BaseSearchCriteria tempQuery, ref string solrQuery)
        {
            TextualSearch.DoubleBasedCriteria criteria = tempQuery as TextualSearch.DoubleBasedCriteria;
            if (criteria.CriteriaValue != 0)
            {
                string query = $"({criteria.CriteriaName}: {criteria.CriteriaValue})";
                solrQuery += query;
                solrQuery += " AND ";
            }
        }

        private void TextualExecuteDoubleRangeQuery(TextualSearch.BaseSearchCriteria tempQuery, ref string solrQuery)
        {
            TextualSearch.DoubleRangeBasedCriteria criteria = tempQuery as TextualSearch.DoubleRangeBasedCriteria;
            if ((criteria.CriteriaStartValue != 0 || criteria.CriteriaEndValue != 0) && criteria.CriteriaStartValue <= criteria.CriteriaEndValue)
            {
                string query = $"({criteria.CriteriaName}: [{criteria.CriteriaStartValue } TO {criteria.CriteriaEndValue}])";
                solrQuery += query;
                solrQuery += " AND ";
            }
        }

        private void TextualExecuteDateQuery(TextualSearch.BaseSearchCriteria tempQuery, ref string solrQuery)
        {
            TextualSearch.DateBaseCriteria criteria = tempQuery as TextualSearch.DateBaseCriteria;
            if (criteria.CriteriaValue.HasValue)
            {
                string time = criteria.CriteriaValue.Value.ToString(CultureInfo.InvariantCulture);
                DateTime convertedStartTime = DateTime.Parse(time);

                string query = $"({criteria.CriteriaName}: {ConvertDatePropertyToSolrLongDate(convertedStartTime)})";
                solrQuery += query;
                solrQuery += " AND ";
            }
        }

        private void TextualExecuteDateRangeQuery(TextualSearch.BaseSearchCriteria tempQuery, ref string solrQuery)
        {
            TextualSearch.DateRangeBasedCriteria criteria = tempQuery as TextualSearch.DateRangeBasedCriteria;

            string startTime = !criteria.CriteriaStartValue.HasValue ? "*" : criteria.CriteriaStartValue.Value.ToString(CultureInfo.InvariantCulture);
            string endTime = !criteria.CriteriaEndValue.HasValue ? "*" : criteria.CriteriaEndValue.Value.ToString(CultureInfo.InvariantCulture);

            DateTime startTimeValue;
            DateTime endTimeValue;
            bool canParseStartTime = DateTime.TryParse(startTime, out startTimeValue);
            bool canParseEndTime = DateTime.TryParse(endTime, out endTimeValue);

            string part1 = canParseStartTime ? ConvertDatePropertyToSolrLongDate(DateTime.Parse(startTime)).ToString() : "*";
            string part2 = canParseEndTime ? ConvertDatePropertyToSolrLongDate(DateTime.Parse(endTime)).ToString() : "*";

            string query = $"({criteria.CriteriaName}: [{part1} TO {part2}])";
            solrQuery += query;
            solrQuery += " AND ";
        }

        private void TextualGenerateSearchQuery(List<TextualSearch.BaseSearchCriteria> criterias, ref string solrQuery)
        {
            if (criterias.Count > 0)
            {
                solrQuery += "(";
                foreach (var tempQuery in criterias)
                {
                    if (tempQuery is TextualSearch.StringBasedCriteria)
                    {
                        TextualExecuteStringQuery(tempQuery, ref solrQuery);
                    }
                    else if (tempQuery is TextualSearch.DoubleBasedCriteria)
                    {
                        TextualExecuteDoubleQuery(tempQuery, ref solrQuery);
                    }
                    else if (tempQuery is TextualSearch.DoubleRangeBasedCriteria)
                    {
                        TextualExecuteDoubleRangeQuery(tempQuery, ref solrQuery);
                    }
                    else if (tempQuery is TextualSearch.DateBaseCriteria)
                    {
                        TextualExecuteDateQuery(tempQuery, ref solrQuery);
                    }
                    else if (tempQuery is TextualSearch.DateRangeBasedCriteria)
                    {
                        TextualExecuteDateRangeQuery(tempQuery, ref solrQuery);
                    }

                    bool isLastCriteria = true;
                    isLastCriteria = false;

                    if (isLastCriteria)
                    {
                        solrQuery = $" {solrQuery}     ";
                    }
                }

                solrQuery = solrQuery.Remove(solrQuery.Length - 4);
                solrQuery += ") ";
            }
        }

        public List<TextualSearch.BaseSearchResult> GetResultsForTextualSearch(TextualSearch.TextualSearchQuery query, AuthorizationParametters authorizationParametters)
        {
            List<TextualSearch.BaseSearchResult> result = new List<TextualSearch.BaseSearchResult>();

            long totalRow = 0;
            long foundNumber = 0;
            string queryParam = "";

            if (query.HighlightMode == TextualSearch.HighlightMode.AllTheWords)
                queryParam = CleanseSolrString(query.QueryParam);
            else
                queryParam = "\"" + CleanseSolrString(query.QueryParam) + "\"";

            string filter = "";

            TextualGenerateSearchQuery(query.Criterias, ref filter);

            string field = query.SearchTarget == TextualSearch.SearchTargetSet.All ? nameof(Property.StringValue) : "content";
            string solrUrl = query.SearchTarget == TextualSearch.SearchTargetSet.All ? ObjectCollection.SolrUrl : FileCollection.SolrUrl;

            JObject jObject = new JObject
             {
                 new JProperty("query", queryParam),
                 new JProperty("fields", "id"),
                 new JProperty("filter",filter),
                 new JProperty("offset",query.StartIndex),
                 new JProperty("limit",query.ToIndex)
             };

            JObject jParams = JObject.Parse($@" {{
                                    	     		""hl"" : ""on"", 
                                                    ""hl.fl"" : ""{field}"",
                                                    ""hl.snippets"" : ""{query.Snippets}"",
                                                    ""hl.fragsize"" : ""{query.Fragsize}"",
                                                    ""hl.encoder"":""html"",
                                                    ""hl.simple.pre"":""<b>"",
                                                    ""hl.simple.post"":""</b>"" ,
                                                    ""hl.maxAnalyzedChars"":""-1"",
                                                    ""hl.method"":""fastVector"",
                                                    ""hl.useFastVectorHighlighter"":""true"",
                                                    ""hl.usePhraseHighlighter"":""true"",
                                                    ""hl.defType"":""edismax"",
		                                            ""hl.termVectors"":""true"",
                                                    ""hl.termPositions"":""true"",
                                                    ""hl.termOffsets"":""true""
                                                }}           
                                            ");

            jObject.Add(new JProperty("params", jParams));

            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MyCount"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Count"": ""unique(id)""
                                                         }}
                                                    }}
                                             }}	           
                                            "));
            jObject.Add(new JProperty("facet", jFacet));

            var response = ExecuteRetriveQueryOnParticularCollection(jObject, solrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                    totalRow = (long)JObject.Parse(jsonResult["facets"].ToString())["MyCount"]["Count"];

                if ((string)JObject.Parse(jsonResult["response"].ToString())["numFound"] != "0")
                    foundNumber = (long)JObject.Parse(jsonResult["response"].ToString())["numFound"];

                if ((string)JObject.Parse(jsonResult["highlighting"].ToString())["count"] != "0")
                {
                    var highlighting = JObject.Parse(jsonResult["highlighting"].ToString());
                    foreach (var child in highlighting.Children())
                    {
                        var contents = child.Last["content"];
                        if (contents != null)
                        {
                            long id = long.Parse(child.Path);
                            List<string> highlights = new List<string>();

                            foreach (var item in contents)
                            {
                                var innerContent = ((string)item).Trim();
                                innerContent = CleanHighlightResults(innerContent);

                                highlights.Add(innerContent);
                            }

                            TextualSearch.DocumentBasedSearchResult doc = new TextualSearch.DocumentBasedSearchResult()
                            {
                                FoundNumber = foundNumber,
                                TotalRow = totalRow,
                                ObjectId = id,
                                TextResult = new TextualSearch.TextResult() { PartOfText = highlights }
                            };

                            result.Add(doc);
                        }
                        else
                        {
                            var stringValues = child.Last["StringValue"];
                            if (stringValues != null)
                            {
                                long id = long.Parse(child.Path.Split('!')[0]);

                                List<string> highlights = new List<string>();
                                foreach (var item in stringValues)
                                {
                                    var innerContent = ((string)item).Trim();
                                    innerContent = CleanHighlightResults(innerContent);

                                    highlights.Add(innerContent);
                                }

                                TextualSearch.ObjectBasedSearchResult obj = new TextualSearch.ObjectBasedSearchResult()
                                {
                                    FoundNumber = foundNumber,
                                    TotalRow = totalRow,
                                    ObjectId = id,
                                    TextResult = new TextualSearch.TextResult() { PartOfText = highlights }
                                };

                                result.Add(obj);
                            }
                        }
                    }
                }

                return result;
            }
            else
            {
                throw new Exception($"Request for solr failed this message returned from solr: \n{response}");
            }
        }

        #endregion

        public List<SearchProperty> GetDBPropertyByObjectId(long objectId, AuthorizationParametters authorizationParametters)
        {
            List<SearchProperty> searchProperties = new List<SearchProperty>();

            string filterQuery = "OwnerObjectID:" + objectId;

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filterQuery} AND {GetACLCriterions(authorizationParametters)}"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
                return searchProperties;
            }
            else
            {
                return null;
            }
        }

        private SearchProperty ConvertJTokenPropertyToSearchProperty(JToken jProperty)
        {
            SearchProperty searchProperty = new SearchProperty();

            long id = long.Parse(jProperty["id"].ToString().Split('!')[1]);
            long ownerObjectID = long.Parse(jProperty["OwnerObjectID"].ToString());

            searchProperty.Id = id;
            searchProperty.TypeUri = (string)jProperty["TypeUri"];
            searchProperty.OwnerObject = GetObject(ownerObjectID);
            searchProperty.DataSourceID = (long)jProperty["DataSourceId"];
            searchProperty.Value = getValue(jProperty);

            return searchProperty;
        }

        private string getValue(JToken child)
        {
            if (child["StringValue"] != null)
            {
                return (string)child["StringValue"];
            }
            else if (child["DateTimeValue"] != null && child["LongValue"] != null)
            {
                return (string)child["DateTimeValue"];
            }
            else if (child["LongValue"] != null)
            {
                return (string)child["LongValue"];
            }
            else if (child["BooleanValue"] != null)
            {
                return (string)child["BooleanValue"];
            }
            else if (child["DateTimeValue"] != null)
            {
                return (string)child["DateTimeValue"];
            }
            else if (child["GeoValue"] != null)
            {
                string[] geoValuePart = child["GeoValue"].ToString().Split(',');
                return $"{{\"Latitude\":{geoValuePart[0]},\"Longitude\":{geoValuePart[1]} }}";
            }
            else if (child["DoubleValue"] != null)
            {
                return (string)child["DoubleValue"];
            }
            return "";

        }

        public SearchObject GetObject(long objectId)
        {
            string filterQuery = "id:" + objectId;
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(filterQuery, 1);

            SearchObject searchObject = null;
            foreach (var child in docsJson.Children())
            {
                searchObject = ConvertJTokenObjectToSearchObject(child);
            }
            return searchObject;
        }

        public List<SearchProperty> GetDBPropertyByObjectIds(long[] propertyIds, AuthorizationParametters authorizationParametters)
        {
            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("(id:*!");
            foreach (long propertyId in propertyIds)
            {
                solrQueryIDPart.Append(propertyId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("id:*!");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 8);
            result = result + ")";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} AND {GetACLCriterions(authorizationParametters)}"));
            query.Add(new JProperty("limit", propertyIds.LongLength.ToString()));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];

                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
                return searchProperties;
            }
            else
            {
                return null;
            }
        }

        public List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authorizationParametters)
        {
            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryOwnerObjectIDPart = new StringBuilder();
            solrQueryOwnerObjectIDPart.Append("OwnerObjectID:");
            foreach (long objectId in objectIDs)
            {
                solrQueryOwnerObjectIDPart.Append(objectId.ToString());
                solrQueryOwnerObjectIDPart.Append(" OR OwnerObjectID:");
            }
            string solrQueryOwnerObjectIDPartResult = solrQueryOwnerObjectIDPart.ToString().Substring(0, solrQueryOwnerObjectIDPart.ToString().Length - 17);


            StringBuilder solrQueryTypeUriPart = new StringBuilder();
            solrQueryTypeUriPart.Append("TypeUri:");
            foreach (string propertyTypeUri in specifiedPropertyTypeUris)
            {
                solrQueryTypeUriPart.Append(propertyTypeUri);
                solrQueryTypeUriPart.Append(" OR TypeUri:");
            }
            string solrQueryTypeUriPartResult = solrQueryTypeUriPart.ToString().Substring(0, solrQueryTypeUriPart.ToString().Length - 11);


            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            //query.Add(new JProperty("filter", $"({solrQueryOwnerObjectIDPartResult}) AND ({solrQueryTypeUriPartResult}) AND {GetACLCriterions(authorizationParametters)}"));
            query.Add(new JProperty("filter", $"({solrQueryOwnerObjectIDPartResult}) AND ({solrQueryTypeUriPartResult}) AND {GetACLCriterions(authorizationParametters)} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
                return searchProperties;
            }
            else
            {
                return null;
            }
        }

        public List<SearchDataSourceACL> RetrieveDataSourceACLs(long[] dataSourceIDs)
        {
            Dictionary<long, List<ACI>> acisDic = GetACIs(dataSourceIDs.ToList());
            return GetDBDataSourceACLs(dataSourceIDs, acisDic);
        }

        private Dictionary<long, List<ACI>> GetACIs(List<long> dataSourceIDs)
        {
            Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();

            List<SearchDataSourceAci> searchSourceAcis = new List<SearchDataSourceAci>();

            StringBuilder solrQuery = new StringBuilder();
            solrQuery.Append("dsid:");
            foreach (long dataSourceId in dataSourceIDs)
            {
                solrQuery.Append(dataSourceId.ToString());
                solrQuery.Append(" OR ");
            }
            string result = solrQuery.ToString().Substring(0, solrQuery.ToString().Length - 3);


            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, DataSourceAciCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchSourceAcis.Add(ConvertJTokenDataSourceAciToSearchDataSourceAci(child));
                }

                acisDic = DataReaderToDataSourceACI(searchSourceAcis);
                return acisDic;

            }
            else
            {
                return null;
            }
        }

        private SearchDataSourceAci ConvertJTokenDataSourceAciToSearchDataSourceAci(JToken jDataSourceAci)
        {
            SearchDataSourceAci searchSourceAci = new SearchDataSourceAci();

            searchSourceAci.dsid = long.Parse(jDataSourceAci["dsid"].ToString());
            searchSourceAci.GroupName = (string)jDataSourceAci["GroupName"];
            searchSourceAci.Permission = 4;// (byte)jDataSourceAci["Permission"];

            return searchSourceAci;
        }

        private Dictionary<long, List<ACI>> DataReaderToDataSourceACI(List<SearchDataSourceAci> collection)
        {
            Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();
            foreach (var item in collection)
            {
                long dsid = item.dsid;
                string groupname = item.GroupName;
                long permission = item.Permission;
                if (!acisDic.ContainsKey(dsid))
                {
                    List<ACI> aciTempList = new List<ACI>();
                    aciTempList.Add(new ACI()
                    {
                        GroupName = groupname,
                        AccessLevel = (Permission)permission
                    });
                    acisDic.Add(dsid, aciTempList);
                }
                else
                {
                    acisDic[dsid].Add(new ACI()
                    {
                        GroupName = groupname,
                        AccessLevel = (Permission)permission
                    });
                }
            }
            return acisDic;
        }

        private List<SearchDataSourceACL> GetDBDataSourceACLs(long[] dataSourceIDs, Dictionary<long, List<ACI>> acisDic)
        {
            if (!dataSourceIDs.Any())
                return new List<SearchDataSourceACL>();

            List<SearchDataSource> dataSources = new List<SearchDataSource>();

            StringBuilder solrQuery = new StringBuilder();
            solrQuery.Append("id:");
            foreach (long dataSourceId in dataSourceIDs)
            {
                solrQuery.Append(dataSourceId.ToString());
                solrQuery.Append(" OR ");
            }
            string result = solrQuery.ToString().Substring(0, solrQuery.ToString().Length - 3);


            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, DataSourceCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    dataSources.Add(ConvertJTokenDataSourceToSearchDataSource(child));
                }

                return DataReaderToDataSource(dataSources, acisDic);

            }
            else
            {
                return null;
            }
        }

        private SearchDataSource ConvertJTokenDataSourceToSearchDataSource(JToken jDataSource)
        {
            SearchDataSource searchSource = new SearchDataSource();

            searchSource.id = long.Parse(jDataSource["id"].ToString());
            searchSource.Name = jDataSource["Name"].ToString();
            searchSource.Description = jDataSource["Description"].ToString();
            searchSource.Type = (int)jDataSource["Type"];
            searchSource.CreateBy = jDataSource["CreatedBy"].ToString();
            searchSource.CreateTime = jDataSource["CreatedTime"].ToString();
            searchSource.ClassificationIdentifier = jDataSource["ClassificationIdentifier"].ToString();
            searchSource.Administrators = jDataSource["Administrators"].ToString();

            return searchSource;
        }

        private List<SearchDataSourceACL> DataReaderToDataSource(List<SearchDataSource> collection, Dictionary<long, List<ACI>> acisDic)
        {
            List<SearchDataSourceACL> dataSources = new List<SearchDataSourceACL>();
            foreach (var item in collection)
            {
                long id = item.id;
                string classification = item.ClassificationIdentifier;
                dataSources.Add(
                    new SearchDataSourceACL()
                    {
                        Id = id,
                        Acl = new ACL()
                        {
                            Classification = classification,
                            Permissions = acisDic[id]
                        }
                    }
                    );
            }
            return dataSources;
        }

        public List<SearchObject> GetObjectByIDs(long[] objectIDs)
        {
            if (!objectIDs.Any())
                return new List<SearchObject>();

            List<SearchObject> searchObjects = new List<SearchObject>();

            StringBuilder solrQuery = new StringBuilder();
            solrQuery.Append("(");
            foreach (long objectId in objectIDs)
            {
                //solrQuery.Append(objectId.ToString());
                //solrQuery.Append(" OR ");
                solrQuery.Append("id:");
                solrQuery.Append(objectId.ToString());
                solrQuery.Append(" OR ");
            }
            solrQuery.Append(")");
            string result = solrQuery.ToString().Substring(0, solrQuery.ToString().Length - 4);
            result = result + ")";

            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(result, objectIDs.LongLength);
            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }
            return searchObjects;
        }

        public long GetLastAsignedDataSourceId()
        {
            long lastId = 0;

            JObject jObject = new JObject
             {
                 new JProperty("query", "*:*")
             };

            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MyMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Max"": ""max(id)""
                                                         }}
                                                    }}
                                             }}	           
                                            "));
            jObject.Add(new JProperty("facet", jFacet));

            var response = ExecuteRetriveQueryOnParticularCollection(jObject, DataSourceCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                    lastId = (long)JObject.Parse(jsonResult["facets"].ToString())["MyMax"]["Max"];

                return lastId;
            }
            else
            {
                return 1;
            }
        }

        public long GetLastAsignedObjectId()
        {
            string filter = "{!parent which= ParentDocument:true}";
            long lastId = 0;

            JObject jObject = new JObject
             {
                 new JProperty("query", "*:*"),
                 new JProperty("filter",filter)
             };

            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MyMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Max"": ""max(id)""
                                                         }}
                                                    }}
                                             }}	           
                                            "));
            jObject.Add(new JProperty("facet", jFacet));

            var response = ExecuteRetriveQueryOnParticularCollection(jObject, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                    lastId = (long)JObject.Parse(jsonResult["facets"].ToString())["MyMax"]["Max"];

                return lastId;
            }
            else
            {
                return 1;
            }
        }

        public long GetLastAsignedPropertyId()
        {
            string filter = "OwnerObjectID:*";
            long lastId = 0;
            string result = "";

            JObject jObject = new JObject
             {
                 new JProperty("query", "*:*")
                 ,
                 new JProperty("filter",filter)
                 //new JProperty("fields", "id")
                 //,
             };

            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MyMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Max"": ""max(id)""
                                                         }}
                                                    }}
                                             }}	           
                                            "));
            jObject.Add(new JProperty("facet", jFacet));

            var response = ExecuteRetriveQueryOnParticularCollection(jObject, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                    result = JObject.Parse(jsonResult["facets"].ToString())["MyMax"]["Max"].ToString();

                lastId = long.Parse(result.Split('!')[1]);
                return lastId;
            }
            else
            {
                return 1;
            }
        }

        public long GetLastAsignedRelationId()
        {
            string filter = "SourceObjectId:*";
            long lastId = 0;
            string result = "";

            JObject jObject = new JObject
             {
                 new JProperty("query", "*:*")
                 ,
                 new JProperty("filter",filter)
                 //new JProperty("fields", "id")
                 //,
             };

            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MyMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Max"": ""max(id)""
                                                         }}
                                                    }}
                                             }}	           
                                            "));
            jObject.Add(new JProperty("facet", jFacet));

            var response = ExecuteRetriveQueryOnParticularCollection(jObject, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                    result = JObject.Parse(jsonResult["facets"].ToString())["MyMax"]["Max"].ToString();

                lastId = long.Parse(result.Split(new string[] { "!R" }, StringSplitOptions.None)[1]);
                return lastId;
            }
            else
            {
                return 1;
            }
        }

        public long GetLastAssignedGraphaId()
        {
            long lastId = 0;
            string result = "";

            JObject jObject = new JObject
             {
                 new JProperty("query", "*:*")
             };

            JObject jFacet = JObject.Parse(string.Format(
                                            @"{{
                                               ""MyMax"":{{
                                                 ""type"" : ""query"",
                                                 ""facet"":
                                                        {{
                                                           ""Max"": ""max(id)""
                                                         }}
                                                    }}
                                             }}	           
                                            "));
            jObject.Add(new JProperty("facet", jFacet));

            var response = ExecuteRetriveQueryOnParticularCollection(jObject, GraphCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                if ((string)JObject.Parse(jsonResult["facets"].ToString())["count"] != "0")
                    result = JObject.Parse(jsonResult["facets"].ToString())["MyMax"]["Max"].ToString();

                lastId = long.Parse(result.Split(new string[] { "!R" }, StringSplitOptions.None)[1]);
                return lastId;
            }
            else
            {
                return 1;
            }
        }

        public List<SearchProperty> GetDBPropertyByObjectIdsWithoutAuthorization(long[] objectIDs)
        {
            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("(OwnerObjectID:");
            foreach (long objectId in objectIDs)
            {
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("OwnerObjectID:");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 17);
            result = result + ")";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
                return searchProperties;
            }
            else
            {
                return null;
            }
        }

        public List<SearchRelationship> RetrieveRelationships(long[] relationshipIDs)
        {
            List<SearchRelationship> relationships = new List<SearchRelationship>();

            if (!relationshipIDs.Any())
                return relationships;

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("(id:*!R");
            foreach (long relationshipId in relationshipIDs)
            {
                solrQueryIDPart.Append(relationshipId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("id:*!R");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 8);
            result = result + ")";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    relationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }
                return relationships;
            }
            else
            {
                return null;
            }
        }

        public List<SearchRelationship> GetRelationships(List<long> relationshipIDs, AuthorizationParametters authorizationParametters)
        {
            List<SearchRelationship> relationships = new List<SearchRelationship>();

            if (!relationshipIDs.Any())
                return relationships;

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("(id:*!R");
            foreach (long relationshipId in relationshipIDs)
            {
                solrQueryIDPart.Append(relationshipId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("id:*!R");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 9);
            result = result + ")";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} AND {GetACLCriterions(authorizationParametters)} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    relationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }
                return relationships;
            }
            else
            {
                return null;
            }
        }

        public List<SearchGraphArrangement> GetGraphArrangements(AuthorizationParametters authorizationParametters)
        {
            List<SearchGraphArrangement> searchGraphArrangements = new List<SearchGraphArrangement>();

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, GraphCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    SearchGraphArrangement searchGraphArrangement = new SearchGraphArrangement();
                    searchGraphArrangement.Id = (long)child["id"];
                    searchGraphArrangement.DataSourceID = (long)child["dsid"];
                    searchGraphArrangement.Title = (string)child["Title"];
                    searchGraphArrangement.Description = (string)child["Description"];
                    searchGraphArrangement.NodesCount = (int)child["NodesCount"];
                    searchGraphArrangement.TimeCreated = (string)child["TimeCreated"];
                    searchGraphArrangement.GraphImage = Convert.FromBase64String((string)child["GraphImage_txt"]);
                    searchGraphArrangement.GraphArrangementXML = Convert.FromBase64String((string)child["GraphArrangement_txt"]);

                    searchGraphArrangements.Add(searchGraphArrangement);
                }
                return searchGraphArrangements;
            }
            else
            {
                return null;
            }
        }

        public byte[] GetGraphImage(int dbGrapharagmentID, AuthorizationParametters authorizationParametters)
        {
            string filter = "id:" + dbGrapharagmentID;

            byte[] graphImage = null;

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filter} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, GraphCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    graphImage = Convert.FromBase64String((string)child["GraphImage_txt"]);
                }

                return graphImage;
            }
            else
            {
                return null;
            }
        }

        public byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authorizationParametters)
        {
            string filter = "id:" + dbGraphArrangementID;

            byte[] GraphArrangement = null;

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filter} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, GraphCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    GraphArrangement = Convert.FromBase64String((string)child["GraphArrangement_txt"]);
                }

                return GraphArrangement;
            }
            else
            {
                return null;
            }
        }

        public List<SearchRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            List<SearchRelationship> relationships = new List<SearchRelationship>();

            if (!objectIDs.Any())
                return relationships;

            StringBuilder solrQueryIDPart = new StringBuilder();
            foreach (long objectId in objectIDs)
            {
                solrQueryIDPart.Append("SourceObjectId:*!R");
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("TargetObjectId:*!R");
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 3);
            //result = result + ")";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    relationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }
                return relationships;
            }
            else
            {
                return null;
            }
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission)
        {
            switch (conceptType)
            {
                case ConceptType.Property:
                    {
                        return GetSubsetOfTableByPermissionPropertyTable(IDs, groupNames, minimumPermission);
                    }
                case ConceptType.Relationship:
                    {
                        return GetSubsetOfTableByPermissionRelationTable(IDs, groupNames, minimumPermission);
                    }
                case ConceptType.Object:
                    {
                        return GetSubsetOfObjectConceptByPermission(IDs, groupNames, minimumPermission);
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        private long[] GetSubsetOfTableByPermissionPropertyTable(long[] ids, string[] groupNames, Permission minimumPermission)
        {
            List<long> permittedIds = new List<long>();
            if (!ids.Any())
                return permittedIds.ToArray();

            if (groupNames.Length == 0)
            {
                throw new Exception("groupNames is empty.");
            }

            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("OwnerObjectID:* AND");
            foreach (long id in ids)
            {
                solrQueryIDPart.Append(" id:");
                solrQueryIDPart.Append(id.ToString());
                solrQueryIDPart.Append(" and DataSourceId: ");
                solrQueryIDPart.Append(id.ToString());
                solrQueryIDPart.Append(" and ");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 3);

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
            }
            else
            {
                return null;
            }

            permittedIds = GetIDsFromResultPropertyTable(searchProperties);
            return permittedIds.ToArray();
        }

        private long[] GetSubsetOfTableByPermissionRelationTable(long[] ids, string[] groupNames, Permission minimumPermission)
        {
            List<long> permittedIds = new List<long>();
            if (!ids.Any())
                return permittedIds.ToArray();

            if (groupNames.Length == 0)
            {
                throw new Exception("groupNames is empty.");
            }

            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("SourceObjectId:* AND");
            foreach (long id in ids)
            {
                solrQueryIDPart.Append(" id:");
                solrQueryIDPart.Append(id.ToString());
                solrQueryIDPart.Append(" and DataSourceId: ");
                solrQueryIDPart.Append(id.ToString());
                solrQueryIDPart.Append(" and ");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 3);

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
            }
            else
            {
                return null;
            }

            permittedIds = GetIDsFromResultPropertyTable(searchProperties);
            return permittedIds.ToArray();
        }

        private List<long> GetIDsFromResultPropertyTable(List<SearchProperty> collection)
        {
            List<long> ids = new List<long>();
            foreach (var item in collection)
            {
                ids.Add(item.Id);
            }
            return ids;
        }

        private long[] GetSubsetOfObjectConceptByPermission(long[] ids, string[] groupNames, Permission minimumPermission)
        {
            List<SearchProperty> propertiesOfObjectIDs = GetPropertiesOfObjectsWithoutAuthorizationParameters(ids);

            Dictionary<long, long> PropertyToObjectIdMapping = new Dictionary<long, long>();
            foreach (var currentProperty in propertiesOfObjectIDs)
            {
                PropertyToObjectIdMapping.Add(currentProperty.Id, currentProperty.OwnerObject.Id);
            }
            long[] propertiesInPermission = GetSubsetOfTableByPermissionPropertyTable(PropertyToObjectIdMapping.Keys.ToArray(), groupNames, minimumPermission);
            HashSet<long> permittedObjectIds = new HashSet<long>();
            foreach (long currentPropertyId in propertiesInPermission)
            {
                long objectId;
                if (PropertyToObjectIdMapping.ContainsKey(currentPropertyId))
                {
                    PropertyToObjectIdMapping.TryGetValue(currentPropertyId, out objectId);
                    permittedObjectIds.Add(objectId);
                }

            }
            return permittedObjectIds.ToArray();
        }

        private List<SearchProperty> GetPropertiesOfObjectsWithoutAuthorizationParameters(long[] objectIDs)
        {
            if (objectIDs.Length == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");

            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("(OwnerObjectID:");
            foreach (long objectId in objectIDs)
            {
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("OwnerObjectID:");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 17);
            result = result + ")";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
                return searchProperties;
            }
            else
            {
                return null;
            }
        }

        public List<SearchDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        {
            return new List<SearchDataSourceACL>();
        }

        public List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids)
        {
            if (!ids.Any())
                throw new InvalidOperationException("id list is empty");

            List<DataSourceInfo> dataSources = new List<DataSourceInfo>();

            StringBuilder solrQuery = new StringBuilder();
            solrQuery.Append("id:");
            foreach (long id in ids)
            {
                solrQuery.Append(id.ToString());
                solrQuery.Append(" OR ");
            }
            string result = solrQuery.ToString().Substring(0, solrQuery.ToString().Length - 3);

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, DataSourceCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    DataSourceInfo dataSourceInfo = new DataSourceInfo();

                    dataSourceInfo.Id = long.Parse(child["id"].ToString());
                    dataSourceInfo.Name = child["Name"].ToString();
                    dataSourceInfo.Description = child["Description"].ToString();
                    dataSourceInfo.Type = (int)child["Type"];
                    dataSourceInfo.CreatedBy = child["CreatedBy"].ToString();
                    dataSourceInfo.CreatedTime = child["CreatedTime"].ToString();

                    dataSourceInfo.Acl = new ACL()
                    {
                        Classification = child["ClassificationIdentifier"].ToString(),
                        Permissions = new List<ACI>()
                          {
                               new ACI(){ AccessLevel= Permission.Owner, GroupName="Admin" }
                          }
                    };

                    dataSources.Add(dataSourceInfo);
                }

                return dataSources;
            }
            else
            {
                return null;
            }
        }

        public List<SearchObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID)
        {
            List<SearchObject> searchObjects = new List<SearchObject>();
            string filter = $"id:{firstID} and id:{lastID}";
            JToken docsJson = RetriveObjectsFromSolrByQueryWithMasterId(filter, 2);
            foreach (var child in docsJson.Children())
            {
                searchObjects.Add(ConvertJTokenObjectToSearchObject(child));
            }
            return searchObjects;
        }

        public List<SearchRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs)
        {
            List<SearchRelationship> relationships = new List<SearchRelationship>();

            if (!objectIDs.Any())
                return relationships;

            StringBuilder solrQueryIDPart = new StringBuilder();
            foreach (long objectId in objectIDs)
            {
                solrQueryIDPart.Append("SourceObjectId:*!R");
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 3);

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    relationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }
                return relationships;
            }
            else
            {
                return null;
            }
        }

        public List<SearchRelationship> GetRelationshipsBySourceObject(long objectID, string typeUri, AuthorizationParametters authorizationParametters)
        {
            List<SearchRelationship> relationships = new List<SearchRelationship>();

            string filter = $"SourceObjectId:* and LinkTypeUri:{typeUri} and SourceObjectId:{objectID}";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filter}"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    relationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }
                return relationships;
            }
            else
            {
                return null;
            }
        }

        public List<SearchRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authorizationParametters)
        {
            List<SearchRelationship> relationships = new List<SearchRelationship>();

            if (!objectIDs.Any())
                return relationships;

            StringBuilder solrQueryIDPart = new StringBuilder();
            foreach (long objectId in objectIDs)
            {
                solrQueryIDPart.Append("SourceObjectId:*!R");
                solrQueryIDPart.Append(" AND ");
                solrQueryIDPart.Append(" SourceObjectId:");
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append(" TargetObjectId:");
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 3);

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    relationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }
                return relationships;
            }
            else
            {
                return null;
            }
        }

        public SearchRelationship GetExistingRelationship(string typeUri, long source, long target, int direction, AuthorizationParametters authorizationParametters)
        {
            SearchRelationship searchRelationship = null;

            string filter = $"LinkTypeUri:{typeUri} and SourceObjectId:{source} and TargetObjectId:{target} and Direction:{direction}";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filter}"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchRelationship = ConvertJTokenRelationshipToSearchRelationship(child);
                }

                return searchRelationship;
            }
            else
            {
                return null;
            }
        }

        public List<SearchRelationship> GetSourceLink(long objectId, string typeUri, AuthorizationParametters authorizationParametters)
        {
            List<SearchRelationship> searchRelationships = new List<SearchRelationship>();

            string filter = $"LinkTypeUri:{typeUri} or TargetObjectId:{objectId} ";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{filter}"));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchRelationships.Add(ConvertJTokenRelationshipToSearchRelationship(child));
                }

                return searchRelationships;
            }
            else
            {
                return null;
            }
        }

        private SearchRelationship ConvertJTokenRelationshipToSearchRelationship(JToken jRelationship)
        {
            long id = long.Parse(jRelationship["id"].ToString().Split(new string[] { "!R" }, StringSplitOptions.None)[1]);
            long sourceObjectId = long.Parse(jRelationship["SourceObjectId"].ToString());
            long targetObjectId = long.Parse(jRelationship["TargetObjectId"].ToString());

            SearchRelationship searchRelationship = new SearchRelationship();

            searchRelationship.Id = id;
            searchRelationship.SourceObjectId = sourceObjectId;
            searchRelationship.SourceObjectTypeUri = jRelationship["SourceObjectTypeUri"].ToString();
            searchRelationship.TargetObjectId = targetObjectId;
            searchRelationship.TargetObjectTypeUri = jRelationship["TargetObjectTypeUri"].ToString();
            searchRelationship.DataSourceID = long.Parse(jRelationship["DataSourceId"].ToString());
            searchRelationship.TypeUri = jRelationship["LinkTypeUri"].ToString();
            searchRelationship.Direction = int.Parse(jRelationship["Direction"].ToString());

            return searchRelationship;
        }

        public List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authorizationParametters)
        {
            if (objectIDs.Count == 0)
                throw new ArgumentNullException(nameof(objectIDs), "No Object defined to retrieve properties");

            List<SearchProperty> searchProperties = new List<SearchProperty>();

            StringBuilder solrQueryIDPart = new StringBuilder();
            solrQueryIDPart.Append("(OwnerObjectID:");
            foreach (long objectId in objectIDs)
            {
                solrQueryIDPart.Append(objectId.ToString());
                solrQueryIDPart.Append(" OR ");
                solrQueryIDPart.Append("OwnerObjectID:");
            }
            string result = solrQueryIDPart.ToString().Substring(0, solrQueryIDPart.ToString().Length - 17);
            result = result + ")";
            result = result + $"  and  OwnerObjectTypeUri:{propertyTypeUri} and StringValue:{propertyValue}";

            JObject query = new JObject();
            query.Add(new JProperty("query", "*:*"));
            query.Add(new JProperty("filter", $"{result} "));
            IRestResponse response = ExecuteRetriveQueryOnParticularCollection(query, ObjectCollection.SolrUrl);
            var jsonResult = JObject.Parse(response.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            if (status == 0)
            {
                var docsJson = JObject.Parse(jsonResult["response"].ToString())["docs"];
                foreach (var child in docsJson.Children())
                {
                    searchProperties.Add(ConvertJTokenPropertyToSearchProperty(child));
                }
                return searchProperties;
            }
            else
            {
                return null;
            }
        }
    }
}
