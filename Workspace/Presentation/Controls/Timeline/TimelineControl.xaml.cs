using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.TimelineViewer;
using GPAS.TimelineViewer.EventArguments;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using GPAS.FilterSearch;
using System.ServiceModel;

namespace GPAS.Workspace.Presentation.Controls.Timeline
{
    /// <summary>
    /// Interaction logic for TimelineControl.xaml
    /// </summary>
    public partial class TimelineControl : PresentationControl
    {
        #region Dependencies

        /// <summary>
        /// مجموعه ای از اشیاء که قصد داریم ویژگی های زمانی آن ها را روی خط زمان‌ مشاهده نماییم.
        /// </summary>
        public ObservableCollection<KWObject> ObjectCollection
        {
            get { return (ObservableCollection<KWObject>)GetValue(ObjectCollectionProperty); }
            set { SetValue(ObjectCollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectCollection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectCollectionProperty =
            DependencyProperty.Register(nameof(ObjectCollection), typeof(ObservableCollection<KWObject>), typeof(TimelineControl),
                new PropertyMetadata(null, OnSetObjectCollectionChanged));

        private static void OnSetObjectCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineControl)d).OnSetObjectCollectionChanged(e);
        }

        private void OnSetObjectCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ObjectCollection != null)
            {
                ObjectCollection.CollectionChanged -= ObjectCollection_CollectionChanged;
                ObjectCollection.CollectionChanged += ObjectCollection_CollectionChanged;
            }

            OnObjectCollectionChanged(e);
        }


        /// <summary>
        /// کوئری فعال خط زمان را برمی گرداند
        /// </summary>
        public Query FilterQuery
        {
            get { return (Query)GetValue(FilterQueryProperty); }
            protected set { SetValue(FilterQueryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterQuery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterQueryProperty =
            DependencyProperty.Register(nameof(FilterQuery), typeof(Query), typeof(TimelineControl),
                new PropertyMetadata(null, OnSetFilterQueryChanged));


        private static void OnSetFilterQueryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineControl)d).OnSetFilterQueryChanged(e);
        }

        private void OnSetFilterQueryChanged(DependencyPropertyChangedEventArgs e)
        {
            OnFilterQueryChanged(e);
        }


        /// <summary>
        /// بازه های زمانی انتخاب شده به وسیله کلیک رو میله های خط زمان را بر می گرداند.
        /// این بازه های زمانی با بزرگ نمایی یا کوچک نمایی خط زمان تغییر نمی کنند و مستقل از میله های هایلایت شده هستند
        /// </summary>
        public List<TimeRange> SelectedRanges
        {
            get { return (List<TimeRange>)GetValue(SelectedRangesProperty); }
            protected set { SetValue(SelectedRangesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedRanges.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedRangesProperty =
            DependencyProperty.Register(nameof(SelectedRanges), typeof(List<TimeRange>), typeof(TimelineControl),
                new PropertyMetadata(null, OnSetSelectedRangesChanged));


        private static void OnSetSelectedRangesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineControl)d).OnSetSelectedRangesChanged(e);
        }

        private void OnSetSelectedRangesChanged(DependencyPropertyChangedEventArgs e)
        {
            OnSelectedRangesChanged(e);
        }


        /// <summary>
        /// لیستی از پنجره های فیلتر فعال را برمی گرداند
        /// </summary>
        public List<TimeRange> FilterWindows
        {
            get { return (List<TimeRange>)GetValue(FilterWindowsProperty); }
            protected set { SetValue(FilterWindowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterWindows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterWindowsProperty =
            DependencyProperty.Register(nameof(FilterWindows), typeof(List<TimeRange>), typeof(TimelineControl),
                new PropertyMetadata(null, OnSetFilterWindowsChanged));


        private static void OnSetFilterWindowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineControl)d).OnSetFilterWindowsChanged(e);
        }

        private void OnSetFilterWindowsChanged(DependencyPropertyChangedEventArgs e)
        {
            OnFilterWindowsChanged(e);
        }



        /// <summary>
        /// لیست TypeUriهای ویژگی های لود شده در خط زمان را که تیک خورده اند بر می گرداند.
        /// </summary>
        public IEnumerable<string> CheckedPropertyTypeUriList
        {
            get { return (IEnumerable<string>)GetValue(CheckedPropertyTypeUriListProperty); }
            protected set { SetValue(CheckedPropertyTypeUriListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedPropertyTypeUriList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedPropertyTypeUriListProperty =
            DependencyProperty.Register(nameof(CheckedPropertyTypeUriList), typeof(IEnumerable<string>), typeof(TimelineControl),
                new PropertyMetadata(null, OnSetCheckedPropertyTypeUriListChanged));


        private static void OnSetCheckedPropertyTypeUriListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineControl)d).OnSetCheckedPropertyTypeUriListChanged(e);
        }

        private void OnSetCheckedPropertyTypeUriListChanged(DependencyPropertyChangedEventArgs e)
        {
            OnCheckedPropertyTypeUriListChanged(e);
        }


        #endregion

        #region Variables

        Dictionary<string, BaseDataTypes> PropertyTypeUrisWithBaseDataTypes = new Dictionary<string, BaseDataTypes>();
        List<KWObject> SelectedObjectList = new List<KWObject>();
        List<SuperCategory> SuperCategories = new List<SuperCategory>();
        SuperCategory DefaultSuperCategory = new SuperCategory();
        string TitleDefaultSuperCategory = "Properties";

        /// <summary>
        /// لیست تمام ویژگی های لود در خط زمان را بر می گرداند.
        /// </summary>
        public List<KTProperty> TotalProperties { get; protected set; } = new List<KTProperty>();

        /// <summary>
        /// TypeURIهای اشیاء لود شده در خط زمان را بر می گرداند
        /// </summary>
        private IEnumerable<string> ObjectTypeUris
        {
            get
            {
                if (TotalProperties == null)
                    return new List<string>();

                return TotalProperties.Select(p => p?.RelatedKWProperty?.Owner?.TypeURI).Distinct();
            }
        }

        /// <summary>
        /// لیستی از اشیاء را برمی گرداند که ویژگی های آنها در خط زمان لود شده است.
        /// </summary>
        private IEnumerable<KWObject> EffectiveObjects
        {
            get
            {
                if (TotalProperties == null)
                    return new List<KWObject>();

                return TotalProperties.Select(p => p?.RelatedKWProperty?.Owner).Distinct();
            }
        }

        /// <summary>
        /// لیست TypeUriهای ویژگی های لود در خط زمان را بر می گرداند.
        /// </summary>
        public IEnumerable<string> PropertyTypeUriList
        {
            get
            {
                if (PropertyTypeUrisWithBaseDataTypes == null)
                    return new List<string>();

                return PropertyTypeUrisWithBaseDataTypes.Keys;
            }
        }

        /// <summary>
        /// لیست ویژگی های در حال نمایش در خط زمان را برمی گرداند.
        /// </summary>
        IEnumerable<KTProperty> TotalPropertiesInShown
        {
            get
            {
                if (TotalProperties == null || CheckedPropertyTypeUriList == null)
                    return new List<KTProperty>();

                return TotalProperties.Where(p => CheckedPropertyTypeUriList.Contains(p?.TypeURI));
            }
        }

        private Ontology.Ontology Ontology
        {
            get
            {
                return OntologyProvider.GetOntology();
            }
        }

        #endregion

        #region Methodes

        public TimelineControl()
        {
            InitializeComponent();

            InitProperties();
        }

        private void InitProperties()
        {
            CheckedPropertyTypeUriList = new List<string>();
            SelectedRanges = new List<TimeRange>();
            FilterWindows = new List<TimeRange>();

            ObjectCollection = new ObservableCollection<KWObject>();
            ObjectCollection.CollectionChanged -= ObjectCollection_CollectionChanged;
            ObjectCollection.CollectionChanged += ObjectCollection_CollectionChanged;
        }

        private List<TimeRange> ConvertObjectToTimeRangeList(object obj)
        {
            List<TimeRange> result = new List<TimeRange>();

            if (obj is IList<TimeRange>)
                result.AddRange(((IList<TimeRange>)obj).OfType<TimeRange>().ToList());
            else if (obj is TimeRange)
                result.Add((TimeRange)obj);

            return result;
        }

        private void ObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<KWObject> oldItems = new ObservableCollection<KWObject>(ObjectCollection);
            if (e.NewItems != null)
                oldItems = new ObservableCollection<KWObject>(ObjectCollection.Except(e.NewItems.OfType<KWObject>()));

            OnObjectCollectionChanged(new DependencyPropertyChangedEventArgs(ObjectCollectionProperty, oldItems, ObjectCollection));
        }

        private async void LoadNewData()
        {
            try
            {
                ShowWaiting();
                if (ObjectCollection == null)
                    return;

                CreatePropertyTypeUriWithBaseDataType();
                await CreateTotalProperties();

                SetEffectivePropertyTypeUriWithBaseDataTypeList();

                timelineViewer.ClearAllData();
                timelineViewer.ResetBinSize(timelineViewer.GetActualBin().RelatedBinSizesEnum);
            }
            catch (Exception ex)
            {
                if (!ex.GetType().Name.Contains("FaultException"))
                {
                    throw ex;
                }
            }
            finally
            {
                HideWaiting();
            }
        }

        private async Task CreateTotalProperties()
        {
            if (TotalProperties == null)
                return;

            List<KTProperty> kTProperties = new List<KTProperty>();

            var totalKWPropertyList = await PropertyManager.GetPropertiesOfObjectsAsync(ObjectCollection, PropertyTypeUriList);

            if (totalKWPropertyList == null)
                return;

            foreach (KWProperty kWProperty in totalKWPropertyList)
            {
                if (IsValidValue(kWProperty))
                {
                    KTProperty kTProperty = ConvertKWPropertyToKTProperty(kWProperty);
                    if (kTProperty != null && kTProperty.Value != null)
                        kTProperties.Add(kTProperty);
                }
            }

            TotalProperties = kTProperties;
        }

        private KTProperty ConvertKWPropertyToKTProperty(KWProperty kWProperty)
        {
            if (kWProperty == null)
                return null;

            if (PropertyTypeUrisWithBaseDataTypes == null || !PropertyTypeUrisWithBaseDataTypes.ContainsKey(kWProperty.TypeURI))
                return null;

            KTProperty kTProperty = new KTProperty()
            {
                RelatedKWProperty = kWProperty,
                TypeURI = kWProperty.TypeURI,
            };

            object objectValue = ValueBaseValidation.ParsePropertyValue(PropertyTypeUrisWithBaseDataTypes[kWProperty.TypeURI], kWProperty.Value);

            if (objectValue is DateTime)
                kTProperty.Value = ConvertDateTimeToTimeRange((DateTime)objectValue);
            else if (objectValue is GeoTimeEntity)
                kTProperty.Value = ConvertGeoTimeEntityToTimeRange((GeoTimeEntity)objectValue);
            else
                throw new Exception("Value is invalid");

            return kTProperty;
        }

        private TimeRange ConvertGeoTimeEntityToTimeRange(GeoTimeEntity geoTimeEntity)
        {
            if (geoTimeEntity == null)
                return null;

            return new TimeRange() { From = geoTimeEntity.DateRange.TimeBegin, To = geoTimeEntity.DateRange.TimeEnd };
        }

        private TimeRange ConvertDateTimeToTimeRange(DateTime dateTime)
        {
            if (dateTime == null)
                return null;

            return new TimeRange() { From = dateTime, To = dateTime };
        }

        private bool IsValidValue(KWProperty kWProperty)
        {
            if (kWProperty == null)
                return false;

            if (PropertyTypeUrisWithBaseDataTypes == null || !PropertyTypeUrisWithBaseDataTypes.ContainsKey(kWProperty.TypeURI))
                return false;

            if (PropertyManager.IsPropertyValid(PropertyTypeUrisWithBaseDataTypes[kWProperty.TypeURI], kWProperty.Value).Status != ValidationStatus.Invalid)
                return true;

            return false;
        }

        private void SetEffectivePropertyTypeUriWithBaseDataTypeList()
        {
            var activePropertyTypeUriWithBaseDataType = new Dictionary<string, BaseDataTypes>();

            if (TotalProperties != null)
            {
                foreach (string item in TotalProperties.Select(p => p.TypeURI).Distinct())
                {
                    activePropertyTypeUriWithBaseDataType.Add(item, PropertyTypeUrisWithBaseDataTypes[item]);
                }
            }

            PropertyTypeUrisWithBaseDataTypes = activePropertyTypeUriWithBaseDataType;
        }

        private void CreatePropertyTypeUriWithBaseDataType()
        {
            PropertyTypeUrisWithBaseDataTypes.Clear();

            if (ObjectCollection == null)
                return;

            foreach (string typeUri in GetObjectCollectionTypeUriList())
            {
                GetPropertiesList(Ontology.GetHierarchyPropertiesOfObject(typeUri));
            }
        }

        private IEnumerable<string> GetObjectCollectionTypeUriList()
        {
            if (ObjectCollection == null)
                return new List<string>();

            return ObjectCollection.Select(o => o.TypeURI).Distinct();
        }

        private void GetPropertiesList(ObservableCollection<OntologyNode> properties)
        {
            if (properties == null || properties.Count <= 0)
                return;

            foreach (var item in properties)
            {
                if (!(item is PropertyNode))
                    continue;

                PropertyNode property = item as PropertyNode;

                if (property.IsLeaf)
                {
                    if (!PropertyTypeUriList.Contains(property.TypeUri) &&
                        (property.BaseDataType == BaseDataTypes.DateTime || property.BaseDataType == BaseDataTypes.GeoTime))
                        PropertyTypeUrisWithBaseDataTypes.Add(property.TypeUri, property.BaseDataType);
                }
                else
                {
                    GetPropertiesList(property.Children);
                }
            }
        }

        private void TimelineViewer_ItemsNeeded(object sender, ItemsNeededEventArgs e)
        {
            ProvidingItems(e);
        }

        private async void ProvidingItems(ItemsNeededEventArgs e)
        {
            try
            {
                ShowWaiting();
                if (e.Action == ItemsNeededAction.None || e.Action == ItemsNeededAction.ZoomIn || e.Action == ItemsNeededAction.ZoomOut)
                {
                    e.TotalLowerBound = GetTotalLowerBound(e.BinScaleLevel, e.BinFactor);
                    e.TotalUpperBound = GetTotalUpperBound(e.BinScaleLevel, e.BinFactor);
                    e.MaximumCount = await GetMaxFrequencyAsync(e.BinScaleLevel, e.BinFactor);
                }

                CreateSuperCategories(e);

                e.FetchedItems = SuperCategories;
                timelineViewer.AddItems(e);
            }
            finally
            {
                HideWaiting();
            }
        }

        private async Task<double> GetMaxFrequencyAsync(BinScaleLevel binScaleLevel, double binFactor)
        {
            return await Task.Run(() => GetMaxFrequency(binScaleLevel, binFactor));
        }

        private double GetMaxFrequency(BinScaleLevel binScaleLevel, double binFactor)
        {
            Dictionary<DateTime, double> barFromValues = new Dictionary<DateTime, double>();
            double countOfBigRanges = 0;

            if (TotalProperties != null)
            {
                foreach (KTProperty kTProperty in TotalProperties)
                {
                    if (kTProperty.Value.IsBigRange(binScaleLevel, binFactor))
                    {
                        //در صورتی که بازه زمانی بزرگ بود جهت تسریع در اجرای الگوریتم به مقدار خروجی یک واحد افزوده می شود.
                        //در واقع فرض می شود که بازه زمانی بزرگ با میله ای که بزرگترین مقدار را نمایش می دهد اشتراک دارد.
                        countOfBigRanges++;
                    }
                    else
                    {
                        IEnumerable<TimeRange> relatedBars = kTProperty.Value.GetRelatedBars(binScaleLevel, binFactor);

                        foreach (TimeRange relatedBar in relatedBars)
                        {
                            if (barFromValues.ContainsKey(relatedBar.From))
                                barFromValues[relatedBar.From]++;
                            else
                                barFromValues.Add(relatedBar.From, 1);
                        }
                    }
                }
            }

            if (barFromValues?.Count <= 0)
                return countOfBigRanges;

            double maximumCountFromSmallRanges = barFromValues.Values.Max();
            return maximumCountFromSmallRanges + countOfBigRanges; // بزرگترین مقدار بازه های کوچک + تعداد بازه های بزرگ
        }

        /// <summary>
        /// کمترین مقدار نمایش داده شده را برحسب میله های خط زمان محاسبه می کند.
        /// این مقدار به طول میله ها وابسته است.
        /// </summary>
        /// <param name="binScaleLevel">واحد زمانی طول میله ها</param>
        /// <param name="binFactor">تعداد برای هر واحد زمانی</param>
        /// <returns>کمترین مقدار زمانی</returns>
        private DateTime GetTotalLowerBound(BinScaleLevel binScaleLevel, double binFactor)
        {
            if (TotalProperties == null || TotalProperties.Count == 0)
                return TimelineViewer.Utility.MinValue;

            DateTime? min = TotalProperties.Select(p => p?.Value?.From).Min();

            if (min == null)
                return TimelineViewer.Utility.MinValue;

            return TimelineViewer.Utility.Floor(min.Value, new BinSize() { BinFactor = binFactor, BinScale = binScaleLevel });
        }

        /// <summary>
        /// بیشترین مقدار نمایش داده شده را برحسب میله های خط زمان محاسبه می کند.
        /// این مقدار به طول میله ها وابسته است.
        /// </summary>
        /// <param name="binScaleLevel">واحد زمانی طول میله ها</param>
        /// <param name="binFactor">تعداد برای هر واحد زمانی</param>
        /// <returns>بیشترین مقدار زمانی</returns>
        private DateTime GetTotalUpperBound(BinScaleLevel binScaleLevel, double binFactor)
        {
            if (TotalProperties == null || TotalProperties.Count == 0)
                return TimelineViewer.Utility.MaxValue;

            DateTime? max = TotalProperties.Select(p => p?.Value?.To).Max();

            if (max == null)
                return TimelineViewer.Utility.MaxValue;

            return TimelineViewer.Utility.Ceiling(max.Value, new BinSize() { BinFactor = binFactor, BinScale = binScaleLevel });
        }

        private void CreateSuperCategories(ItemsNeededEventArgs e)
        {
            SuperCategories.Clear();
            CreateDefaultSuperCategory();
            CreateSubCategories(e);

            if (DefaultSuperCategory.HasChild())
                SuperCategories.Add(DefaultSuperCategory);
        }

        private void CreateSubCategories(ItemsNeededEventArgs e)
        {
            if (TotalProperties == null || TotalProperties.Count == 0)
                return;

            foreach (string propertyTypeUri in PropertyTypeUriList)
            {
                CreateSubCategory(DefaultSuperCategory, propertyTypeUri, e);
            }
        }

        private void CreateDefaultSuperCategory()
        {
            ResetDefaultSuperCategory();
        }

        private void CreateSubCategory(SuperCategory superCat, string propertyTypeUri, ItemsNeededEventArgs e)
        {
            if (superCat == null)
                return;

            if (e == null)
                return;

            Category cat = null;
            if (superCat.ExistsSubCategory(propertyTypeUri))
            {
                cat = superCat.FindSubCategory(propertyTypeUri);
            }
            else
            {
                cat = new Category()
                {
                    Identifier = propertyTypeUri,
                    LowerBound = e.BeginTime,
                    UpperBound = e.EndTime,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(propertyTypeUri),
                    Icon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(propertyTypeUri)),
                };

                superCat.SubCategories.Add(cat);
            }

            var allPropertyInCat = TotalProperties.Where(p => p.TypeURI.Equals(cat.Identifier.ToString()));
            AddItemsToCategory(cat, allPropertyInCat, e);
        }

        private void AddItemsToCategory(Category cat, IEnumerable<KTProperty> allPropertyInCat, ItemsNeededEventArgs e)
        {
            if (cat == null)
                return;

            if (allPropertyInCat == null)
                return;

            if (e == null)
                return;

            if (cat.DataItems == null)
                cat.DataItems = new ObservableCollection<TimelineItemsSegment>();

            foreach (KTProperty kTProperty in allPropertyInCat)
            {
                IEnumerable<TimeRange> relatedBars = kTProperty.Value.GetRelatedBarsInBound(e.BinScaleLevel, e.BinFactor, e.BeginTime, e.EndTime);

                if (relatedBars == null)
                    return;

                foreach (TimeRange relatedBar in relatedBars)
                {
                    TimelineItemsSegment dataItem = cat.DataItems.FirstOrDefault(di => relatedBar.Equals(di));
                    if (dataItem == null)
                    {
                        dataItem = new TimelineItemsSegment()
                        {
                            From = relatedBar.From,
                            To = relatedBar.To,
                            Tag = new HashSet<KTProperty>(),
                        };

                        cat.DataItems.Add(dataItem);
                    }

                    dataItem.FrequencyCount++;
                    ((HashSet<KTProperty>)dataItem.Tag).Add(kTProperty);
                }
            }
        }

        private void ResetDefaultSuperCategory()
        {
            DefaultSuperCategory = new SuperCategory()
            {
                Title = TitleDefaultSuperCategory,
                Tag = TitleDefaultSuperCategory
            };
        }

        private void timelineViewer_SegmentSelectionChanged(object sender, SegmentSelectionChangedEventArgs e)
        {
            SelectedRanges = timelineViewer.SelectedSegments.Select(ss => TimeRange.ConvertRangeToTimeRange(ss)).ToList();
        }

        /// <summary>
        /// لیستی از اشیائی که ویژگی های آن ها با بازه های زمانی داده شده اشتراک زمانی دارند را بر می گرداند.
        /// </summary>
        /// <param name="timeRanges">بازه های زمانی</param>
        /// <returns>لیستی از اشیاء</returns>
        public IEnumerable<KWObject> GetKWObjectsFromTimeRanges(IEnumerable<TimeRange> timeRanges)
        {
            IEnumerable<KWObject> objectsInRanges = new List<KWObject>();

            if (timeRanges == null)
                return objectsInRanges;

            objectsInRanges = TotalPropertiesInShown.Where(
                        p => timeRanges.FirstOrDefault(tr => tr.Intersect(p?.Value) != null) != null
                        ).Select(p => p?.RelatedKWProperty?.Owner).Distinct();
            //اشیاء با ویژگی در حال نمایش که با بازه های زمانی داده شده مرتبط هستند را بر می گرداند

            return objectsInRanges;
        }

        /// <summary>
        /// لیستی از اشیاء را گرفته و میله هایی از نمودار خط زمان را که با ویژگی های از نوع زمان این ویژگی ها اشتراک زمانی دارند را انتخاب می کند.
        /// </summary>
        /// <param name="objectsToSelect">لیست اشیاء</param>
        public void HighlightBarsRelatedObjects(IEnumerable<KWObject> objectsToSelect)
        {
            SelectedRanges.Clear();

            if (objectsToSelect == null)
            {
                SelectedObjectList = new List<KWObject>();
            }
            else
            {
                SelectedObjectList = objectsToSelect.Where(o => EffectiveObjects.Contains(o)).ToList();
            }

            HighlightRangesRelatedObjects(SelectedObjectList);
        }

        private void HighlightRangesRelatedObjects(IEnumerable<KWObject> objectsToSelect)
        {
            IEnumerable<KTProperty> relatedProperties = GetRelatedPropertiesInShown(objectsToSelect);
            HighlightRangesRelatedProperties(relatedProperties);
        }

        private void HighlightRangesRelatedProperties(IEnumerable<KTProperty> kTProperties)
        {
            IEnumerable<TimeRange> relatedTimeRanges = kTProperties.Select(p => p?.Value);
            HighlightRangesRelatedTimeRanges(relatedTimeRanges);
        }

        private void HighlightRangesRelatedTimeRanges(IEnumerable<TimeRange> timeRanges)
        {
            timelineViewer.AddSegmentsToHighlightedSegments(timeRanges.Select(tr => tr.ConvertToRange()).ToList());
        }

        private IEnumerable<KTProperty> GetRelatedPropertiesInShown(IEnumerable<KWObject> objects)
        {
            IEnumerable<KTProperty> relatedProperties = new List<KTProperty>();
            if (objects == null)
                return relatedProperties;

            relatedProperties = TotalPropertiesInShown.Where(p => objects.Contains(p?.RelatedKWProperty?.Owner));
            //تمام ویژگی هایی که مالک آنها در لیست اشیا ورودی باشد را برمی گرداند

            return relatedProperties;
        }

        private void TimelineViewer_CategoriesInShownChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SelectedRanges == null || SelectedRanges.Count == 0)
            {
                IEnumerable<KWObject> objectsToSelect = new List<KWObject>();
                objectsToSelect = TotalProperties.Where(p => SelectedObjectList.Contains(p?.RelatedKWProperty?.Owner)).Select(p => p?.RelatedKWProperty?.Owner);
                HighlightRangesRelatedObjects(objectsToSelect);
            }

            if (timelineViewer.CategoriesInShown == null)
                CheckedPropertyTypeUriList = new List<string>();
            else
                CheckedPropertyTypeUriList = timelineViewer.CategoriesInShown.Select(c => c.Identifier.ToString());
        }

        private void TimelineViewer_SnapshotTaken(object sender, SnapshotTakenEventArgs e)
        {
            OnSnapshotRequested(new Windows.SnapshotRequestEventArgs(e.UIElement, $"Timeline {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}"));
        }

        /// <summary>
        /// نشانگر انتظار را به نمایش در می آورد. پیغام نمایش داده شده بصورت پیش فرض عبارت "Waiting..." است.
        /// </summary>
        public void ShowWaiting()
        {
            timelineViewer.ShowWaitingPrompt();
        }

        /// <summary>
        /// نشانگر انتظار را به همراه پیغام ارسال شده نمایش می دهد.
        /// </summary>
        /// <param name="message">پیغام ارسال شده جهت نمایش در نشانگر انتظار</param>
        public void ShowWaiting(string message)
        {
            timelineViewer.ShowWaitingPrompt(message);
        }

        /// <summary>
        /// نشانگر انتظار را مخفی می کند.
        /// </summary>
        public void HideWaiting()
        {
            timelineViewer.HideWaitingPrompt();
        }

        private void TimelineViewer_FilterWindowsChanged(object sender, FilterWindowsChangedEventArgs e)
        {
            FilterWindows = timelineViewer.FilterWindows.Select(fw => TimeRange.ConvertFilterRangeToTimeRange(fw)).ToList();
        }

        /// <summary>
        /// کوئری متناسب با پنجره های فیلتر را برمی گرداند. 
        /// این کوئری برای فیلتر کردن اشیا در کابردهای گراف و نقشه کاربرد دارد.
        /// </summary>
        private Query CreateFilterQuery()
        {
            Query query = new Query();
            query.CriteriasSet.SetOperator = BooleanOperator.Any;
            foreach (var typeUri in CheckedPropertyTypeUriList)
            {
                foreach (var filter in FilterWindows)
                {
                    query.CriteriasSet.Criterias.Add(new DateTimePropertyRangeCriteria()
                    {
                        PropertyTypeUri = typeUri,
                        StartTime = filter.From,
                        EndTime = filter.To,
                    });
                }
            }

            return query;
        }

        public void Reset()
        {
            timelineViewer.Reset();
        }

        #endregion

        #region Events

        /// <summary>
        /// هنگام ایجاد تغییر در ویژگی مجموعه اشیاء این رخداد صادر می شود.
        /// </summary>
        public event DependencyPropertyChangedEventHandler ObjectCollectionChanged;
        protected void OnObjectCollectionChanged(DependencyPropertyChangedEventArgs e)
        {
            LoadNewData();

            ObjectCollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// پس از انتخاب یک بازه زمانی (کلیک روی میله های نمودار خط زمان) این رخداد صادر می شود.
        /// </summary>
        public event DependencyPropertyChangedEventHandler SelectedRangesChanged;
        protected void OnSelectedRangesChanged(DependencyPropertyChangedEventArgs e)
        {
            OnObjectsSelectionRequested(new ObjectsSelectionRequestEventArgs(GetKWObjectsFromTimeRanges(SelectedRanges)));

            SelectedRangesChanged?.Invoke(this, e);
        }

        /// <summary>
        /// لیستی از اشیاء را ارسال می کند که درخواست انتخاب آنها را دارد.
        /// این رخداد هنگام انتخاب یک میله (با کلیک ماوس) یا تغییر تیک TypeUri های ویژگی ها فراخوانی می شود.
        /// </summary>
        public event EventHandler<ObjectsSelectionRequestEventArgs> ObjectsSelectionRequested;
        protected void OnObjectsSelectionRequested(ObjectsSelectionRequestEventArgs e)
        {
            ObjectsSelectionRequested?.Invoke(this, e);
        }

        /// <summary>
        /// این رخداد هنگام تغییر لیست پنجره های فیلتر صادر می شود.
        /// </summary>
        public event DependencyPropertyChangedEventHandler FilterWindowsChanged;
        protected void OnFilterWindowsChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterQuery = CreateFilterQuery();

            FilterWindowsChanged?.Invoke(this, e);
        }

        /// <summary>
        /// این رخداد با تغییر کوئری فعال کنترل خط زمان صادر می شود.
        /// در واقع با تغییر لیست پنجره های فیلتر یا تغییر تیک TypeUri های ویژگی ها فراخوانی می شود.
        /// </summary>
        public event DependencyPropertyChangedEventHandler FilterQueryChanged;
        protected void OnFilterQueryChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterQueryChanged?.Invoke(this, e);
        }

        /// <summary>
        /// با تغییر تیک TypeUri های ویژگی ها صادر می شود
        /// </summary>
        public event DependencyPropertyChangedEventHandler CheckedPropertyTypeUriListChanged;
        protected void OnCheckedPropertyTypeUriListChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterQuery = CreateFilterQuery();

            if (SelectedRanges?.Count > 0)
                OnObjectsSelectionRequested(new ObjectsSelectionRequestEventArgs(GetKWObjectsFromTimeRanges(SelectedRanges)));

            CheckedPropertyTypeUriListChanged?.Invoke(this, e);
        }

        /// <summary>
        /// رخدادی مبنی بر عکس گرفتن از کنترل خط زمان را ارسال می کند.
        /// </summary>
        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        protected void OnSnapshotRequested(Windows.SnapshotRequestEventArgs e)
        {
            SnapshotRequested?.Invoke(this, e);
        }

        #endregion
    }
}