using GPAS.StatisticalQuery;
using GPAS.TimelineViewer;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.ViewModel.ObjectExplorer;
using GPAS.Workspace.ViewModel.ObjectExplorer.Formula;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Model
{
    public class ObjectExplorerModel
    {
        ResourceDictionary iconsResource = new ResourceDictionary();
        private int defaultBucketCount = 100;
        private double defaultMinValue = double.MinValue;
        private double defaultMaxValue = double.MaxValue;
        public ObjectExplorerModel()
        {
            iconsResource.Source = new Uri("/Resources/Icons.xaml", UriKind.Relative);
        }

        /// <summary>Derive a new Object Set</summary>
        /// <returns>New Active Set</returns>
        public async Task<DerivedObjectSet> Explore(ObjectSetBase activeObjectSet, List<PreviewStatistic> newFilter)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(activeObjectSet);
            queryGenerator.AppendEquivalentFormula(newFilter);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            PreviewStatistics previewStatistics = await RunQuery(statisticalQuery);
            DerivedObjectSet derivedObjectSet = CreateNewDerivedObjectSet(activeObjectSet, previewStatistics, GetEquivalentFormula(newFilter));
            return derivedObjectSet;
        }
        private FormulaBase GetEquivalentFormula(List<PreviewStatistic> filterStatistics)
        {
            TypeBasedDrillDown formula = new TypeBasedDrillDown()
            {
                Icon = iconsResource["ObjectExplorerHistogramIcon"] as BitmapImage
            };
            formula.AddItemsToFilterByProperty(filterStatistics);
            return formula;
        }

        private async Task<PreviewStatistics> RunQuery(Query statisticalQuery)
        {
            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            QueryResult queryResult = await statisticalQueryProvider.RunQuery(statisticalQuery);
            PreviewStatistics previewStatistics = ConvertQueryResultToPreviewStatistics(queryResult);
            return previewStatistics;
        }

        public async Task<DerivedObjectSet> Explore(ObjectSetBase objectSet, string propertyType, string propertyValue)
        {
            PropertyValueStatistic tempStat = new PropertyValueStatistic()
            {
                Category = new PropertyValueCategory()
                {
                    TypeUri = propertyType
                },
                PropertyValue = propertyValue
            };
            return await Explore(objectSet, new List<PropertyValueStatistic>() { tempStat });
        }
        public async Task<DerivedObjectSet> Explore(ObjectSetBase activeObjectSet, List<PropertyValueStatistic> propertyValuefilter)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(activeObjectSet);
            queryGenerator.AppendEquivalentFormula(propertyValuefilter);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            PreviewStatistics previewStatistics = await RunQuery(statisticalQuery);
            DerivedObjectSet derivedObjectSet = CreateNewDerivedObjectSet(activeObjectSet, previewStatistics, GetEquivalentFormula(propertyValuefilter));
            return derivedObjectSet;
        }
        public async Task<DerivedObjectSet> Explore(ObjectSetBase activeObjectSet, PropertyValueRangeDrillDown propertyValueRangeFilters)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(activeObjectSet);
            queryGenerator.AppendEquivalentFormula(propertyValueRangeFilters);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            PreviewStatistics previewStatistics = await RunQuery(statisticalQuery);
            propertyValueRangeFilters.Icon = iconsResource["ObjectExplorerHistogramIcon"] as BitmapImage;
            DerivedObjectSet derivedObjectSet = CreateNewDerivedObjectSet(activeObjectSet, previewStatistics, propertyValueRangeFilters);
            return derivedObjectSet;
        }

        private FormulaBase GetEquivalentFormula(List<PropertyValueStatistic> propertyValuefilter)
        {
            PropertyValueBasedDrillDown formula = new PropertyValueBasedDrillDown()
            {
                Icon = iconsResource["ObjectExplorerHistogramIcon"] as BitmapImage,
                FilteredBy = propertyValuefilter
            };
            return formula;
        }

        /// <summary>Derive a new Object Set</summary>
        /// <param name="first">Joined Object Set</param>
        /// <param name="second">Active Object Set</param>
        /// <returns>New Active Set</returns>
        public async Task<DerivedObjectSet> Explore(ObjectSetBase first, ObjectSetBase second, SetAlgebraOperator setOperator)
        {
            PerformSetOperation operation = GetEquivalentFormula(first, setOperator);

            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(second);
            queryGenerator.AppendEquivalentFormula(operation);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            PreviewStatistics newStat = await RunQuery(statisticalQuery);
            return CreateNewDerivedObjectSet(second, newStat, operation);
        }

        private PerformSetOperation GetEquivalentFormula(ObjectSetBase joinedSet, SetAlgebraOperator setOperator)
        {
            return new PerformSetOperation()
            {
                Icon = GetSetOperationIcon(setOperator),
                JoinedSet = joinedSet,
                Operator = setOperator
            };
        }
        private BitmapImage GetSetOperationIcon(SetAlgebraOperator setOperator)
        {
            switch (setOperator)
            {
                case SetAlgebraOperator.ExclusiveOr:
                    return iconsResource["SetAlgebraExclusiveOrIcon"] as BitmapImage;
                case SetAlgebraOperator.Intersection:
                    return iconsResource["SetAlgebraIntersectionIcon"] as BitmapImage;
                case SetAlgebraOperator.SubtractLeft:
                    return iconsResource["SetAlgebraSubtractLeftIcon"] as BitmapImage;
                case SetAlgebraOperator.SubtractRight:
                    return iconsResource["SetAlgebraSubtractRightIcon"] as BitmapImage;
                case SetAlgebraOperator.Union:
                    return iconsResource["SetAlgebraUnionIcon"] as BitmapImage;
                default:
                    throw new NotSupportedException();
            }
        }

        public async Task RecomputeStatistics(ObjectSetBase objectSet)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            PreviewStatistics newStat = await RunQuery(statisticalQuery);
            UpdateObjectSetPreviewStatistics(objectSet, newStat);

            await UpdateObjectSetActiveToolStatistics(objectSet);
        }

        private async Task UpdateObjectSetActiveToolStatistics(ObjectSetBase objectSet)
        {
            VisualizationPanelToolBase relatedActiveTool = objectSet.GetActiveVisualPanelTool();
            if (relatedActiveTool != null)
            {
                if (relatedActiveTool is PropertyValueHistogramTool)
                {
                    PropertyValueHistogramTool propertyValueHistogramTool = (relatedActiveTool as PropertyValueHistogramTool);
                    PropertyValueCategory propertyValueCategory = await RetrievePropertyValueStatistics(objectSet, propertyValueHistogramTool.ExploringPreviewStatistic, 50, 0);
                    propertyValueHistogramTool.PropertyValueCategory = propertyValueCategory;
                }
                else if (relatedActiveTool is LinkTypeHistogramTool)
                {
                    LinkTypeHistogramTool linkTypeHistogramTool = (relatedActiveTool as LinkTypeHistogramTool);
                    List<PreviewStatistic> linkTypeStatistics = await RetrieveLinkTypeStatistics(objectSet);
                    linkTypeHistogramTool.linkTypeStatistics = linkTypeStatistics;
                }
                else if (relatedActiveTool is BarChartTool)
                {
                    BarChartTool barChartTool = (relatedActiveTool as BarChartTool);
                    PropertyBarValues propertyBarValues = await RetrievePropertyBarValuesStatistics
                                                                        (objectSet, barChartTool.ExploringPreviewStatistic, defaultBucketCount, defaultMinValue, defaultMaxValue);
                    barChartTool.PropertyBarValues = propertyBarValues;
                }
            }
        }

        private static void UpdateObjectSetPreviewStatistics(ObjectSetBase objectSet, PreviewStatistics newStat)
        {
            objectSet.Preview = newStat;
            objectSet.ObjectsCount = newStat.ObjectsCount;
            if (objectSet is DerivedObjectSet)
            {
                ((DerivedObjectSet)objectSet).AppliedFormula.ObjectsCountDifference
                    = newStat.ObjectsCount - ((DerivedObjectSet)objectSet).ParentSet.ObjectsCount;
            }
            newStat.Icon = objectSet.Icon;
            newStat.Title = objectSet.Title;
        }

        public async Task<PropertyValueCategory> RetrievePropertyValueStatistics(ObjectSetBase objectSet, PreviewStatistic exploringProperty, int loadLimit, int startOffset)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            long minimumCount = Convert.ToInt64(Math.Ceiling((decimal)objectSet.ObjectsCount / 500));

            StatisticalQueryProvider statProvider = new StatisticalQueryProvider();
            PropertyValueStatistics retrievedStatistics = await statProvider.RetrivePropertyValueStatistics
                (statisticalQuery, exploringProperty.TypeURI, loadLimit, minimumCount, startOffset);

            PropertyValueCategory result = new PropertyValueCategory()
            {
                TypeUri = exploringProperty.TypeURI,
                Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(exploringProperty.TypeURI),
                LoadedValues = new List<PropertyValueStatistic>(retrievedStatistics.Results.Count),
                LoadedValuesCount = startOffset + retrievedStatistics.Results.Count,
                TotalValuesCount = retrievedStatistics.TotalResultsCount,
                HasUnloadableValues
                    = (retrievedStatistics.Results.Count < loadLimit)
                      && (retrievedStatistics.Results.Count + startOffset < retrievedStatistics.TotalResultsCount),
                MinimumLoadableValueCount = minimumCount
            };
            AddLoadedValuesToPropertyValueCategory(result, retrievedStatistics.Results);
            return result;
        }

        public async Task<PropertyBarValues> RetrievePropertyBarValuesStatistics(ObjectSetBase objectSet, PreviewStatistic exploringProperty,
                                                                                            int bucketCount, double minValue, double maxValue)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            StatisticalQueryProvider statProvider = new StatisticalQueryProvider();
            var retrievedStatistics = await statProvider.RetrievePropertyBarValuesStatistics(statisticalQuery, exploringProperty.TypeURI,
                                                                                                                bucketCount, minValue, maxValue);

            PropertyBarValues result = new PropertyBarValues()
            {
                Bars = new List<PropertyBarValue>(),
                End = retrievedStatistics.End,
                BucketCount = (int)retrievedStatistics.BucketCount,
                Start = retrievedStatistics.Start,
                Title = Logic.OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(exploringProperty.TypeURI),
                Unit = retrievedStatistics.Unit,
                TypeUri = exploringProperty.TypeURI
            };

            foreach (var bar in retrievedStatistics.Bars)
            {
                result.Bars.Add(new PropertyBarValue()
                {
                    Count = bar.Count,
                    End = bar.End,
                    Start = bar.Start
                });
            }

            // AddLoadedValuesToPropertyValueCategory(result, retrievedStatistics.Results);
            return result;
        }

        public async Task<List<PreviewStatistic>> RetrieveLinkTypeStatistics(ObjectSetBase objectSet)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            StatisticalQueryProvider statProvider = new StatisticalQueryProvider();
            LinkTypeStatistics retrievedStatistics = await statProvider.RetriveLinkTypeStatistics(statisticalQuery);
            List<PreviewStatistic> result = GeneratePreviewStatisticFromLinkTypeStatistics(retrievedStatistics);
            return result;
        }

        private List<PreviewStatistic> GeneratePreviewStatisticFromLinkTypeStatistics(LinkTypeStatistics linkTypeStatistics)
        {
            List<PreviewStatistic> result = new List<PreviewStatistic>();
            foreach (var currentLinkType in linkTypeStatistics.LinkTypes)
            {
                if (!currentLinkType.TypeUri.Equals(OntologyProvider.GetOntology().GetAllConceptsTypeURI()))
                {
                    result.Add(new PreviewStatistic()
                    {
                        Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(currentLinkType.TypeUri)),
                        Count = currentLinkType.Frequency,
                        IsSelected = false,
                        Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentLinkType.TypeUri),
                        TypeURI = currentLinkType.TypeUri,
                        Category = "Link Types",
                        SuperCategory = "Link Types"
                    });
                }
            }

            foreach (var currentLinkedObjectType in linkTypeStatistics.LinkedObjectTypes)
            {
                result.Add(new PreviewStatistic()
                {
                    Icon = new BitmapImage(OntologyIconProvider.GetPropertyTypeIconPath(currentLinkedObjectType.TypeUri)),
                    Count = currentLinkedObjectType.Frequency,
                    IsSelected = false,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentLinkedObjectType.TypeUri),
                    TypeURI = currentLinkedObjectType.TypeUri,
                    Category = GetPreviewStatisticCategory(currentLinkedObjectType.TypeUri),
                    SuperCategory = "Linked Object Types"
                });
            }

            return result;
        }

        private static void AddLoadedValuesToPropertyValueCategory
            (PropertyValueCategory category, List<StatisticalQuery.ResultNode.PropertyValueStatistic> retrievedNodesToAdd)
        {
            foreach (StatisticalQuery.ResultNode.PropertyValueStatistic resultNode in retrievedNodesToAdd)
            {
                category.LoadedValues.Add(new PropertyValueStatistic()
                {
                    PropertyValue = resultNode.PropertyValue,
                    Count = resultNode.Frequency,
                    Category = category,
                    IsSelected = false
                });
            }
        }

        public async Task<List<KWObject>> RetrieveExploredObjects(ObjectSetBase objectSet, List<PropertyValueStatistic> filter)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            queryGenerator.AppendEquivalentFormula(filter);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            return await RetrieveObjectsByQuery(statisticalQuery);
        }

        public async Task<List<KWObject>> RetrieveExploredObjects(ObjectSetBase objectSet, List<PreviewStatistic> filter)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            queryGenerator.AppendEquivalentFormula(filter);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            return await RetrieveObjectsByQuery(statisticalQuery);
        }

        public async Task<List<KWObject>> RetrieveExploredObjects(ObjectSetBase objectSet, PropertyValueRangeDrillDown propertyValueRangeFilters)
        {
            StatisticalQueryGenerator queryGenerator = new StatisticalQueryGenerator();
            queryGenerator.AppendEquivalentFormula(objectSet);
            queryGenerator.AppendEquivalentFormula(propertyValueRangeFilters);
            Query statisticalQuery = queryGenerator.GenerateQuery();

            return await RetrieveObjectsByQuery(statisticalQuery);
        }

        private async Task<List<KWObject>> RetrieveObjectsByQuery(Query statisticalQuery)
        {
            StatisticalQueryProvider statisticalQueryProvider = new StatisticalQueryProvider();
            List<KWObject> retrieveObjects = await statisticalQueryProvider.RetrieveObjectsByQuery(statisticalQuery);
            return retrieveObjects;
        }

        public void ShowRetrievedObjectsOnGraph(List<KWObject> objectsToShowOnGraph)
        {

        }
        public void ShowRetrievedObjectsOnMap(List<KWObject> objectsToShowOnMap)
        {

        }

        /// <summary>
        /// Create new derived object set, and set it as Active-Set
        /// </summary>
        private DerivedObjectSet CreateNewDerivedObjectSet(ObjectSetBase parentSet, PreviewStatistics newSetStatistics, FormulaBase appliedFormula)
        {
            DerivedObjectSet derivedObjectSet = new DerivedObjectSet()
            {
                Icon = appliedFormula.Icon,
                IsInActiveSetSequence = true,
                Preview = newSetStatistics,
                ParentSet = parentSet,
                ObjectsCount = newSetStatistics.ObjectsCount,
                AppliedFormula = appliedFormula
            };
            derivedObjectSet.Title = GetDerivedObjectTitle(appliedFormula);
            derivedObjectSet.SubTitle = GetDerivedObjectSubtitle(derivedObjectSet);
            derivedObjectSet.ToolTip = derivedObjectSet.SubTitle;

            derivedObjectSet.AppliedFormula.IsInActiveSetSequence = true;
            derivedObjectSet.AppliedFormula.ObjectsCountDifference = newSetStatistics.ObjectsCount - parentSet.ObjectsCount;

            newSetStatistics.Title = derivedObjectSet.Title;
            newSetStatistics.Icon = derivedObjectSet.Icon;

            return derivedObjectSet;
        }

        private string GetDerivedObjectSubtitle(DerivedObjectSet derivedObjectSet)
        {
            if (derivedObjectSet == null || derivedObjectSet.AppliedFormula == null)
            {
                throw new ArgumentNullException();
            }

            if (derivedObjectSet.AppliedFormula is PropertyValueBasedDrillDown)
            {
                return $"{Properties.Resources.ObjectsMatchingOneOfTheDefinedPropertyValues_} {derivedObjectSet.Title}";
            }
            else
            {
                return derivedObjectSet.Title;
            }
        }

        private string GetDerivedObjectTitle(FormulaBase appliedFormula)
        {
            if (appliedFormula == null)
            {
                throw new ArgumentNullException();
            }

            if (appliedFormula is TypeBasedDrillDown)
            {
                TypeBasedDrillDown formula = appliedFormula as TypeBasedDrillDown;
                if (formula.FilteredBy == null || formula.FilteredBy.Count == 0)
                {
                    return Properties.Resources.No_set;
                }

                List<PreviewStatistic> filteredByCat1 = formula.FilteredBy.Where(f => f.SuperCategory == PreviewStatistic.ObjectTypesSuperCategoryTitle).ToList();
                List<PreviewStatistic> filteredByCat2 = formula.FilteredBy.Where(f => f.SuperCategory == PreviewStatistic.PropertyTypesSuperCategoryTitle).ToList();

                StringBuilder resultBuilder = new StringBuilder();
                if (filteredByCat1.Count > 0)
                {
                    resultBuilder.Append(Properties.Resources.Of_type_);
                    resultBuilder.Append(" ");

                    for (int i = 0; i < filteredByCat1.Count; i++)
                    {
                        if (i > 0)
                            resultBuilder.Append(", ");

                        var fb = filteredByCat1[i];
                        resultBuilder.Append(fb.Title);
                    }
                }
                if (filteredByCat1.Count > 0 && filteredByCat2.Count > 0)
                {
                    resultBuilder.Append(" ");
                    resultBuilder.Append(Properties.Resources.And);
                    resultBuilder.Append(" ");
                }
                if (filteredByCat2.Count > 0)
                {
                    resultBuilder.Append(Properties.Resources.Has_property_type);
                    resultBuilder.Append(" ");

                    for (int i = 0; i < filteredByCat2.Count; i++)
                    {
                        if (i > 0)
                            resultBuilder.Append(", ");

                        var fb = filteredByCat2[i];
                        resultBuilder.Append(fb.Title);
                    }
                }
                return resultBuilder.ToString();
            }
            else if (appliedFormula is PropertyValueBasedDrillDown)
            {
                PropertyValueBasedDrillDown formula = appliedFormula as PropertyValueBasedDrillDown;
                if (formula.FilteredBy == null || formula.FilteredBy.Count == 0)
                {
                    return Properties.Resources.No_set;
                }
                StringBuilder resultBuilder = new StringBuilder();
                if (formula.FilteredBy.Count > 0)
                {
                    for (int i = 0; i < formula.FilteredBy.Count; i++)
                    {
                        if (i > 0)
                            resultBuilder.Append(", ");
                        if (i > 2)
                        {
                            resultBuilder.Append(", or ");
                            resultBuilder.Append(formula.FilteredBy.Count - 2);
                            resultBuilder.Append("more");
                        }

                        var fb = formula.FilteredBy[i];
                        resultBuilder.Append(OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(fb.Category.TypeUri));
                        resultBuilder.Append(" = ");
                        resultBuilder.Append(fb.PropertyValue);
                    }
                }
                return resultBuilder.ToString();
            }
            else if (appliedFormula is PerformSetOperation)
            {
                PerformSetOperation formula = appliedFormula as PerformSetOperation;
                if (formula.JoinedSet == null)
                {
                    return Properties.Resources.No_set;
                }
                StringBuilder resultBuilder = new StringBuilder();
                switch (formula.Operator)
                {
                    case SetAlgebraOperator.Union:
                        resultBuilder.Append(Properties.Resources.Union);
                        break;
                    case SetAlgebraOperator.Intersection:
                        resultBuilder.Append(Properties.Resources.Intersection);
                        break;
                    case SetAlgebraOperator.SubtractRight:
                        resultBuilder.Append(Properties.Resources.Subtract_Right);
                        break;
                    case SetAlgebraOperator.SubtractLeft:
                        resultBuilder.Append(Properties.Resources.Subtract_Left);
                        break;
                    case SetAlgebraOperator.ExclusiveOr:
                        resultBuilder.Append(Properties.Resources.Exclusive_Or);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                resultBuilder.AppendFormat(" ( {0} )", formula.JoinedSet.Title);
                return resultBuilder.ToString();
            }
            else if (appliedFormula is PropertyValueRangeDrillDown)
            {
                PropertyValueRangeDrillDown formula = appliedFormula as PropertyValueRangeDrillDown;
                if (string.IsNullOrWhiteSpace(formula.PropertyTypeUri) || formula.ValueRanges.Count == 0)
                {
                    return Properties.Resources.No_set;
                }
                StringBuilder resultBuilder = new StringBuilder();
                resultBuilder.AppendFormat("{0} between ", OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(formula.PropertyTypeUri));
                for (int i = 0; i < formula.ValueRanges.Count; i++)
                {
                    if (i > 0)
                        resultBuilder.Append(" or ");
                    if (i > 2)
                    {
                        resultBuilder.AppendFormat(" or {0} more ranges", formula.ValueRanges.Count - 2);
                        break;
                    }
                    var range = formula.ValueRanges[i];
                    resultBuilder.AppendFormat("[{0:N2} , {1:N2})", range.Start, range.End);
                }
                return resultBuilder.ToString();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private PreviewStatistics ConvertQueryResultToPreviewStatistics(QueryResult queryResult)
        {
            long objectsCount = GetTotalObjectsCount(queryResult);
            PreviewStatistics previewStatistics = new PreviewStatistics()
            {
                Icon = iconsResource["ObjectExplorerApplicationIcon"] as BitmapImage,
                ObjectsCount = objectsCount,
                Title = string.Empty,
                Content = GeneratePreviewStatisticFromQueryResult(queryResult)
            };
            return previewStatistics;
        }
        private static long GetTotalObjectsCount(QueryResult queryResult)
        {
            long objectsCount = 0;
            foreach (var currentObjectType in queryResult.ObjectTypePreview)
            {
                if (currentObjectType.TypeUri.Equals(OntologyProvider.GetOntology().GetAllConceptsTypeURI()))
                {
                    objectsCount = currentObjectType.Frequency;
                    break;
                }
            }
            return objectsCount;
        }
        private ObservableCollection<PreviewStatistic> GeneratePreviewStatisticFromQueryResult(QueryResult queryResult)
        {
            SortQueryResult(ref queryResult);

            ObservableCollection<PreviewStatistic> result = new ObservableCollection<PreviewStatistic>();
            foreach (var currentObjectPreview in queryResult.ObjectTypePreview)
            {
                if (!currentObjectPreview.TypeUri.Equals(OntologyProvider.GetOntology().GetAllConceptsTypeURI()))
                {
                    result.Add(new PreviewStatistic()
                    {
                        Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(currentObjectPreview.TypeUri)),
                        Count = currentObjectPreview.Frequency,
                        IsSelected = false,
                        Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentObjectPreview.TypeUri),
                        TypeURI = currentObjectPreview.TypeUri,
                        Category = GetPreviewStatisticCategory(currentObjectPreview.TypeUri),
                        SuperCategory = Properties.Resources.Object_Types
                    });
                }
            }

            foreach (var currentPropertyPreview in queryResult.PropertyTypePreview)
            {
                result.Add(new PreviewStatistic()
                {
                    Icon = new BitmapImage(OntologyIconProvider.GetPropertyTypeIconPath(currentPropertyPreview.TypeUri)),
                    Count = currentPropertyPreview.Frequency,
                    IsSelected = false,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(currentPropertyPreview.TypeUri),
                    TypeURI = currentPropertyPreview.TypeUri,
                    Category = Properties.Resources.Property_Type,
                    SuperCategory = Properties.Resources.Property_Type
                });
            }

            return result;
        }

        private void SortQueryResult(ref QueryResult queryResult)
        {
            queryResult.ObjectTypePreview.Sort((p, q) => p.Frequency.CompareTo(q.Frequency));
            queryResult.PropertyTypePreview.Sort((p, q) => p.Frequency.CompareTo(q.Frequency));

            queryResult.ObjectTypePreview.Reverse();
            queryResult.PropertyTypePreview.Reverse();
        }

        private string GetPreviewStatisticCategory(string typeUri)
        {
            string result = string.Empty;

            if (OntologyProvider.GetOntology().IsEntity(typeUri))
            {
                result = Properties.Resources.Entity_Types;
            }
            else if (OntologyProvider.GetOntology().IsEvent(typeUri))
            {
                result = Properties.Resources.Event_Types;
            }
            else if (OntologyProvider.GetOntology().IsDocument(typeUri))
            {
                result = Properties.Resources.Document_Types;
            }

            return result;
        }

        #region Timeline

        public long GetTimeLineMaxFrequecyCount(string[] propertiesTypeUri, BinScaleLevel binScaleLevel, double binFactor)
        {
            TimelineProvider timelineProvider = new TimelineProvider();
            return timelineProvider.GetTimeLineMaxFrequecyCount(propertiesTypeUri, binScaleLevel, binFactor);
        }

        public DateTime GetTimeLineMaxDate(string[] propertiesTypeUri, BinScaleLevel binScaleLevel, double binFactor)
        {
            TimelineProvider timelineProvider = new TimelineProvider();
            return timelineProvider.GetTimeLineMaxDate(propertiesTypeUri, binScaleLevel, binFactor);
        }

        public DateTime GetTimeLineMinDate(string[] propertiesTypeUri, BinScaleLevel binScaleLevel, double binFactor)
        {
            TimelineProvider timelineProvider = new TimelineProvider();
            return timelineProvider.GetTimeLineMinDate(propertiesTypeUri, binScaleLevel, binFactor);
        }

        #endregion
    }
}
