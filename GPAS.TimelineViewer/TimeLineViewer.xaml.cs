using GPAS.TimelineViewer.Converter;
using GPAS.TimelineViewer.EventArguments;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;

namespace GPAS.TimelineViewer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TimelineViewer : UserControl
    {
        public ObservableCollection<SuperCategory> SuperCategories
        {
            get { return (ObservableCollection<SuperCategory>)GetValue(SuperCategoriesProperty); }
            private set { SetValue(SuperCategoriesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SuperCategories.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuperCategoriesProperty =
            DependencyProperty.Register("SuperCategories", typeof(ObservableCollection<SuperCategory>), typeof(TimelineViewer),
                new PropertyMetadata(null));

        public double MaximumCount
        {
            get { return (double)GetValue(MaximumCountProperty); }
            set { SetValue(MaximumCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumCountProperty =
            DependencyProperty.Register("MaximumCount", typeof(double), typeof(TimelineViewer),
                new PropertyMetadata(defaultMaximumCount, OnSetMaximumCountChanged));

        private static void OnSetMaximumCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelineViewer).OnSetMaximumCountChanged(e);
        }

        private void OnSetMaximumCountChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MaximumCount % defaultMaximumCount != 0)
            {
                MaximumCount += (defaultMaximumCount - (MaximumCount % defaultMaximumCount));
            }
            vAxis.MajorStep = MaximumCount / defaultMaximumCount;
        }

        public Brush SelectedBarColor
        {
            get { return (Brush)GetValue(SelectedBarColorProperty); }
            set { SetValue(SelectedBarColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBarColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBarColorProperty =
            DependencyProperty.Register("SelectedBarColor", typeof(Brush), typeof(TimelineViewer),
                new PropertyMetadata(defaultSelectedBarColor, OnSetSelectedBarColorChanged));


        private static void OnSetSelectedBarColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelineViewer).OnSetSelectedBarColorChanged(e);
        }

        private void OnSetSelectedBarColorChanged(DependencyPropertyChangedEventArgs e)
        {
            AssignSelectedBarColor(SelectedBarColor);
        }

        private void AssignSelectedBarColor(Brush brush)
        {
            mainChart.SelectionPalette = new ChartPalette();
            PaletteEntry selectionPalette = new PaletteEntry(brush);
            mainChart.SelectionPalette.GlobalEntries.Add(selectionPalette);
        }

        internal DateTime? LowerBound
        {
            get { return (DateTime?)GetValue(LowerBoundProperty); }
            set { SetValue(LowerBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LowerBound.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty LowerBoundProperty =
            DependencyProperty.Register("LowerBound", typeof(DateTime?), typeof(TimelineViewer),
                new PropertyMetadata(null, OnSetLowerBoundChanged));

        private static void OnSetLowerBoundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelineViewer).OnSetLowerBoundChanged(e);
        }

        private void OnSetLowerBoundChanged(DependencyPropertyChangedEventArgs e)
        {
            ValidateLowerBound();
            AssignSubCategoriesBound();
        }

        private void ValidateLowerBound()
        {
            if (LowerBound < ActualTotalLowerBound)
            {
                LowerBound = ActualTotalLowerBound;
                DateTime ub = Utility.MaxValue;
                Utility.DateTimeAddTryParse(LowerBound.Value, new TimeSpan(DataSegment.Ticks * NumberOfDataSegmentLoaded), out ub);
                if (ub > ActualTotalUpperBound)
                    ub = ActualTotalUpperBound;

                UpperBound = ub;
                return;
            }
        }

        internal DateTime? UpperBound
        {
            get { return (DateTime?)GetValue(UpperBoundProperty); }
            set { SetValue(UpperBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpperBound.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty UpperBoundProperty =
            DependencyProperty.Register("UpperBound", typeof(DateTime?), typeof(TimelineViewer),
                new PropertyMetadata(null, OnSetUpperBoundChanged));

        private static void OnSetUpperBoundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelineViewer).OnSetUpperBoundChanged(e);
        }

        private void OnSetUpperBoundChanged(DependencyPropertyChangedEventArgs e)
        {
            ValidateUpperBound();
            AssignSubCategoriesBound();
        }

        private void ValidateUpperBound()
        {
            if (UpperBound > ActualTotalUpperBound)
            {
                UpperBound = ActualTotalUpperBound;
                DateTime lb = Utility.MinValue;
                Utility.DateTimeAddTryParse(UpperBound.Value, new TimeSpan(-DataSegment.Ticks * NumberOfDataSegmentLoaded), out lb);
                if (lb < ActualTotalLowerBound)
                    lb = ActualTotalLowerBound;

                LowerBound = lb;
                return;
            }
        }

        private void AssignSubCategoriesBound()
        {
            if (SubCategories?.Count > 0)
            {
                foreach (var subC in SubCategories)
                {
                    subC.LowerBound = LowerBound;
                    subC.UpperBound = UpperBound;
                }
            }

            if (HasExtraSeies())
            {
                Category extraCat = GetExtraCategory();
                extraCat.LowerBound = LowerBound;
                extraCat.UpperBound = UpperBound;
            }
        }

        public DateTime TotalLowerBound
        {
            get { return (DateTime)GetValue(TotalLowerBoundProperty); }
            set { SetValue(TotalLowerBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalLowerBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalLowerBoundProperty =
            DependencyProperty.Register("TotalLowerBound", typeof(DateTime), typeof(TimelineViewer),
                new PropertyMetadata(Utility.MinValue));

        public DateTime TotalUpperBound
        {
            get { return (DateTime)GetValue(TotalUpperBoundProperty); }
            set { SetValue(TotalUpperBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalUpperBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalUpperBoundProperty =
            DependencyProperty.Register("TotalUpperBound", typeof(DateTime), typeof(TimelineViewer),
                new PropertyMetadata(Utility.MaxValue));

        internal DateTime ActualTotalLowerBound
        {
            get { return (DateTime)GetValue(ActualTotalLowerBoundProperty); }
            set { SetValue(ActualTotalLowerBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualTotalLowerBound.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty ActualTotalLowerBoundProperty =
            DependencyProperty.Register("ActualTotalLowerBound", typeof(DateTime), typeof(TimelineViewer), new PropertyMetadata(Utility.MinValue));

        internal DateTime ActualTotalUpperBound
        {
            get { return (DateTime)GetValue(ActualTotalUpperBoundProperty); }
            set { SetValue(ActualTotalUpperBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualTotalUpperBound.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty ActualTotalUpperBoundProperty =
            DependencyProperty.Register("ActualTotalUpperBound", typeof(DateTime), typeof(TimelineViewer), new PropertyMetadata(Utility.MaxValue));


        public BinSizesEnum Bin
        {
            get { return (BinSizesEnum)GetValue(BinProperty); }
            set { SetValue(BinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BinProperty =
            DependencyProperty.Register("Bin", typeof(BinSizesEnum), typeof(TimelineViewer),
                new PropertyMetadata(BinSizes.Default.RelatedBinSizesEnum, OnSetBinChanged));

        private static void OnSetBinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelineViewer).OnSetBinChanged(e);
        }

        private void OnSetBinChanged(DependencyPropertyChangedEventArgs e)
        {
            AssignBin();
        }

        private void AssignBin()
        {
            ActualBin = BinSizes.FindBinSizeFromEnum(Bin, TotalLowerBound, TotalUpperBound, NumberOfDataSegmentLoaded * NumberOfBarsPerEachDataSegment);
        }

        private void AssignDataSegment()
        {
            DateTime dateTime = (LowerBound.HasValue) ? LowerBound.Value : TotalLowerBound;
            DataSegment = new TimeSpan(ActualBin.GetDuration(dateTime).Ticks * NumberOfBarsPerEachDataSegment);
        }

        internal BinSize ActualBin
        {
            get { return (BinSize)GetValue(ActualBinProperty); }
            set { SetValue(ActualBinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActualBin.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty ActualBinProperty =
            DependencyProperty.Register("ActualBin", typeof(BinSize), typeof(TimelineViewer),
                new PropertyMetadata(BinSizes.Default, OnSetActualBinChanged));

        private static void OnSetActualBinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TimelineViewer).OnSetActualBinChanged(e);
        }

        private void OnSetActualBinChanged(DependencyPropertyChangedEventArgs e)
        {
            BinSizesEnum oldVal = ((BinSize)e.OldValue).RelatedBinSizesEnum;
            BinSizesEnum newVal = ((BinSize)e.NewValue).RelatedBinSizesEnum;
            ItemsNeededAction action = ItemsNeededAction.None;
            if (SubCategories.Count > 0)
            {
                if (newVal > oldVal)
                {
                    action = ItemsNeededAction.ZoomIn;
                }
                else
                {
                    action = ItemsNeededAction.ZoomOut;
                }
            }

            DateTime centerRange = new DateTime();
            if (cursorTime.HasValue)
                centerRange = cursorTime.Value;

            SetActualBinChanged(centerRange, cursorPosition, action);
        }



        public ObservableCollection<Category> CategoriesInShown
        {
            get { return (ObservableCollection<Category>)GetValue(CategoriesInShownProperty); }
            set { SetValue(CategoriesInShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CategoriesInShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoriesInShownProperty =
            DependencyProperty.Register(nameof(CategoriesInShown), typeof(ObservableCollection<Category>), typeof(TimelineViewer),
                new PropertyMetadata(null, OnSetCategoriesInShownChanged));

        private static void OnSetCategoriesInShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimelineViewer)d).OnSetCategoriesInShownChanged(e);
        }

        private void OnSetCategoriesInShownChanged(DependencyPropertyChangedEventArgs e)
        {
            if (CategoriesInShown != null)
            {
                CategoriesInShown.CollectionChanged -= CategoriesInShown_CollectionChanged;
                CategoriesInShown.CollectionChanged += CategoriesInShown_CollectionChanged;
            }

            OnCategoriesInShownChanged(e);
        }

        private void SetActualBinChanged(DateTime? focusTime, Point? focusPosition, ItemsNeededAction action)
        {
            AssignDataSegment();
            AssignLabelPropetiesForHorizontalAxis();

            if (action == ItemsNeededAction.None)
            {
                FetchItems(focusTime.Value, focusPosition, action);
            }
            else
            {
                if (SubCategories.Count > 0)
                {
                    if (focusTime.HasValue)
                    {
                        FetchItems(focusTime.Value, focusPosition, action);
                    }
                }
            }
        }

        private void AssignLabelPropetiesForHorizontalAxis()
        {
            DateTimeToHierarchyStringFormatConverter hierarchyDateTime = (DateTimeToHierarchyStringFormatConverter)TryFindResource("DateTimeToHierarchyStringFormat");
            hierarchyDateTime.Reset(ActualBin);
        }

        private void AssignBinForFilterWindows()
        {
            foreach (var fw in filterWindows)
            {
                fw.Bin = ActualBin;
            }
        }

        private void ResetHorizontalAxis()
        {
            hAxis.MajorStepUnit = (TimeInterval)(int)ActualBin.BinScale;
            hAxis.MajorStep = ActualBin.BinFactor;
        }

        public TimeSpan DataSegment { get; private set; }
        private DateTime? cursorTime = null;
        private Point? cursorPosition = null;
        private static readonly double defaultMaximumCount = 5.0;


        public event EventHandler<ItemsNeededEventArgs> ItemsNeeded;
        protected void OnItemsNeeded(ItemsNeededEventArgs args)
        {
            ItemsNeeded?.Invoke(this, args);
        }

        public event EventHandler<SegmentSelectionChangedEventArgs> SegmentSelectionChanged;
        protected void OnSegmentSelectionChanged(SegmentSelectionChangedEventArgs args)
        {
            SegmentSelectionChanged?.Invoke(this, args);
        }

        public event DependencyPropertyChangedEventHandler CategoriesInShownChanged;
        protected void OnCategoriesInShownChanged(DependencyPropertyChangedEventArgs e)
        {
            CategoriesInShownChanged?.Invoke(this, e);
        }

        public int NumberOfDataSegmentLoaded { get; private set; } = 7;

        public int NumberOfBarsPerEachDataSegment { get; private set; } = 100;

        private ObservableCollection<FilterWindow> filterWindows = new ObservableCollection<FilterWindow>();
        public ReadOnlyCollection<FilterRange> FilterWindows
        {
            get
            {
                return filterWindows.Select(fw => fw as FilterRange).ToList().AsReadOnly();
            }
        }


        private ItemsNeededEventArgs currentItemsNeededEventArgs;
        private FilterWindow selectedFilterWindow;
        DateTime startDragFilterWindow, endDragFilterWindow, moveDragFilterWindow;

        enum MoveStatus
        {
            Ready,
            Start,
            Stop,
            Moving,
        }

        MoveStatus movePanStatus = MoveStatus.Ready;
        MoveStatus resizingFilterWindowStatus = MoveStatus.Ready;
        MoveStatus movingFilterWindowStatus = MoveStatus.Ready;
        FilterWindowStrokeMouseEventState currentStrokeState = FilterWindowStrokeMouseEventState.End;

        public event EventHandler<FilterWindowsChangedEventArgs> FilterWindowsChanged;
        protected void OnFilterWindowsChanged(FilterWindowsChangedEventArgs args)
        {
            FilterWindowsChanged?.Invoke(this, args);
        }

        public event EventHandler<SnapshotTakenEventArgs> SnapshotTaken;
        protected void OnSnapshotTaken(SnapshotTakenEventArgs args)
        {
            SnapshotTaken?.Invoke(this, args);
        }

        private List<Range> selectedSegments = new List<Range>();
        public ReadOnlyCollection<Range> SelectedSegments
        {
            get
            {
                return selectedSegments.AsReadOnly();
            }
        }

        public ReadOnlyCollection<Category> SubCategories
        {
            get
            {
                List<Category> categories = new List<Category>();
                foreach (var sc in SuperCategories)
                {
                    categories.AddRange(sc.SubCategories);
                }

                return categories.AsReadOnly();
            }
        }
        PaletteHelper paletteHelper = new PaletteHelper();
        private static LinearGradientBrush defaultSelectedBarColor;

        public TimelineViewer()
        {
            InitializeComponent();
            DataContext = this;

            ITheme theme = paletteHelper.GetTheme();
            defaultSelectedBarColor = new LinearGradientBrush(new GradientStopCollection(new List<GradientStop>() {
                    new GradientStop(theme.PrimaryLight.Color, .8),
                    new GradientStop(theme.PrimaryDark.Color, .2),
                }), new Point(1, .5), new Point(0, .5));


            hAxis.SetBinding(DateTimeContinuousAxis.MinimumProperty, new Binding("ActualTotalLowerBound")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            hAxis.SetBinding(DateTimeContinuousAxis.MaximumProperty, new Binding("ActualTotalUpperBound")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            AssignSelectedBarColor(defaultSelectedBarColor);

            SuperCategories = new ObservableCollection<SuperCategory>();
            SuperCategories.CollectionChanged += SuperCategories_CollectionChanged;

            AssignDataSegment();
            ResetHorizontalAxis();

            filterWindows.CollectionChanged += FilterWindows_CollectionChanged;
            FilterWindowsChanged += TimelineViewer_FilterWindowsChanged;

            CategoriesInShown = new ObservableCollection<Category>();
            CategoriesInShown.CollectionChanged -= CategoriesInShown_CollectionChanged;
            CategoriesInShown.CollectionChanged += CategoriesInShown_CollectionChanged;


        }

        private void CategoriesInShown_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<Category> newItems = new List<Category>();
            if (e.NewItems != null)
                newItems = e.NewItems.OfType<Category>();

            IEnumerable<Category> oldItems = new List<Category>();
            if (e.OldItems != null)
                oldItems = e.OldItems.OfType<Category>();

            OnCategoriesInShownChanged(new DependencyPropertyChangedEventArgs(CategoriesInShownProperty, oldItems, newItems));
        }

        private void TimelineViewer_FilterWindowsChanged(object sender, EventArgs e)
        {
            btnClearFilters.IsEnabled = filterWindows.Count > 0;
        }

        public void ClearAllData()
        {
            subCategoryCounter = 0;
            RemoveHighlightAllBars();
            selectedSegments = new List<Range>();
            StopMoveFilterWindow();
            StopResizeFilterWindow();
            RemoveAllFilterWindows();
            MovePanStatus = MoveStatus.Stop;
            mainChart.Series.Clear();
            SuperCategories.Clear();
            TotalLowerBound = Utility.MinValue;
            TotalUpperBound = Utility.MaxValue;
        }

        public void Reset()
        {
            ClearAllData();
            ResetBinSize();
        }

        public void ResetBinSize()
        {
            ResetBinSize(BinSizes.Default.RelatedBinSizesEnum);
        }

        public void ResetBinSize(BinSizesEnum bin)
        {
            if (Bin == bin)
            {
                SetActualBinChanged(Utility.CenterRange(TotalLowerBound, TotalUpperBound), cursorPosition, ItemsNeededAction.None);
            }
            else
            {
                Bin = bin;
            }
        }

        private void FilterWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<FilterWindow> added = new List<FilterWindow>();
            List<FilterWindow> removed = new List<FilterWindow>();

            if (e.NewItems?.Count > 0)
            {
                foreach (FilterWindow item in e.NewItems)
                {
                    item.AddAnnotationsToChart(mainChart.Annotations);
                    added.Add(item);
                }
            }
            if (e.OldItems?.Count > 0)
            {
                foreach (FilterWindow item in e.OldItems)
                {
                    item.RemoveAnnotationsToChart(mainChart.Annotations);
                    removed.Add(item);
                }
            }

            OnFilterWindowsChanged(new FilterWindowsChangedEventArgs(added, removed));
        }

        private void SuperCategories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SuperCategory superCategory in e.NewItems)
                {
                    superCategory.SubCategotiesChanged -= SuperCategory_SubCategotiesChanged;
                    superCategory.SubCategotiesChanged += SuperCategory_SubCategotiesChanged;
                    AddSubCategoriesToChart(superCategory.SubCategories);
                }
            }
        }

        private void AddSuperCategories(IEnumerable<SuperCategory> superCategories)
        {
            if (superCategories == null)
                return;

            foreach (var superCat in superCategories)
            {
                SuperCategories.Add(superCat);
            }
        }

        private void SuperCategory_SubCategotiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                ObservableCollection<Category> subCategories = new ObservableCollection<Category>();
                foreach (Category subC in e.NewItems)
                {
                    subCategories.Add(subC);
                }

                AddSubCategoriesToChart(subCategories);
            }
        }

        private void SubCategory_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsChecked"))
            {
                CategoriesInShown = new ObservableCollection<Category>(SubCategories.Where(subC => subC.IsChecked));
            }
        }

        int subCategoryCounter = 0;
        Random random = new Random();
        private void AddSubCategoriesToChart(ObservableCollection<Category> subCategories)
        {
            try
            {
                AddExtraSeriesIfNeed();
                if (subCategories != null)
                {
                    ShowWaitingPrompt();
                    AssignSubCategoriesRelatedChart();
                    AssignActualTotalBounds();
                    AssignSubCategoriesBound();
                    foreach (Category cat in subCategories)
                    {
                        cat.PropertyChanged -= SubCategory_PropertyChanged;
                        cat.PropertyChanged += SubCategory_PropertyChanged;
                        if (cat.IsChecked)
                            CategoriesInShown.Add(cat);

                        if (cat.LegendColor == null)
                        {
                            cat.LegendColor = Palette.GetColorFromIndex(subCategoryCounter);
                        }

                        GenerateSeriesCategory(cat);

                        subCategoryCounter++;
                    }

                    HideWaitingPrompt();
                }
            }
            catch
            {
                HideWaitingPrompt();
            }
        }

        private void GenerateSeriesCategory(Category cat)
        {
            BarSeries series = new BarSeries() { CombineMode = ChartSeriesCombineMode.Stack };
            series.CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "PaddedFrom" };
            series.ValueBinding = new PropertyNameDataPointBinding() { PropertyName = "FrequencyCount" };
            series.AllowSelect = false;

            series.SetBinding(BarSeries.ItemsSourceProperty, new Binding("InBoundDataItems")
            {
                Source = cat,
                Mode = BindingMode.TwoWay,
            });

            series.SetBinding(BarSeries.VisibilityProperty, new Binding("IsChecked")
            {
                Source = cat,
                Mode = BindingMode.TwoWay,
                Converter = new System.Windows.Controls.BooleanToVisibilityConverter(),
            });

            series.SetBinding(BarSeries.TagProperty, new Binding() { Source = cat });

            cat.RelatedSeries = series;
            mainChart.Series.Add(series);

            series.IsHitTestVisible = true;

            Style defaultVisualStyle = new Style(typeof(Border));
            Setter setter = new Setter();
            setter.Property = Border.BackgroundProperty;
            setter.Value = new Binding("LegendColor")
            {
                Source = cat,
                Mode = BindingMode.OneWay,
            };

            defaultVisualStyle.Setters.Add(setter);
            series.DefaultVisualStyle = defaultVisualStyle;

            series.MouseLeftButtonDown += Series_MouseLeftButtonDown;
            series.MouseLeftButtonUp += Series_MouseLeftButtonUp;
        }

        private void Series_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MovePanStatus == MoveStatus.Moving)
            {
                MovePanStatus = MoveStatus.Stop;
                e.Handled = true;
            }
            else
            {
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    RemoveHighlightAllBars();
                    selectedSegments = new List<Range>();
                }
            }
        }

        private void Series_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void AddExtraSeriesIfNeed()
        {
            if (!HasExtraSeies())
            {
                Category cat = new Category()
                {
                    Identifier = "ExtraCat",
                    LegendColor = Brushes.Transparent,
                    Tag = "ExtraCat",
                    Title = "ExtraCat",
                };

                GenerateSeriesCategory(cat);
            }
        }

        private bool HasExtraSeies()
        {
            foreach (var series in mainChart.Series)
            {
                if (series.Tag is Category && ((Category)series.Tag).Identifier.ToString() == "ExtraCat")
                    return true;
            }

            return false;
        }

        private Category GetExtraCategory()
        {
            foreach (var series in mainChart.Series)
            {
                if (series.Tag is Category && ((Category)series.Tag).Identifier.ToString() == "ExtraCat")
                    return ((Category)series.Tag);
            }

            return null;
        }

        public async void HideWaitingPrompt()
        {
            await Dispatcher.BeginInvoke(new Action(() => { waitingGrid.Visibility = Visibility.Collapsed; }), DispatcherPriority.Normal);
        }

        public void ShowWaitingPrompt()
        {
            ShowWaitingPrompt(Properties.Resources.Waiting___);
        }

        public async void ShowWaitingPrompt(string waitingMessage)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                waitingTextBlock.Text = waitingMessage;
                waitingGrid.Visibility = Visibility.Visible;
            }), DispatcherPriority.Normal);
        }

        private void AssignSubCategoriesRelatedChart()
        {
            if (SubCategories?.Count > 0)
            {
                foreach (var subC in SubCategories)
                {
                    subC.RelatedChart = mainChart;
                }
            }
        }

        private void AssignTotalBounds(DateTime lower, DateTime upper)
        {
            TotalLowerBound = Utility.Floor(lower, ActualBin);
            Utility.DateTimeAddTryParse(upper, ActualBin.GetDuration(upper), out upper);
            TotalUpperBound = Utility.Floor(upper, ActualBin);
        }

        private void AssignActualTotalBounds()
        {
            DateTime tlb = new DateTime(TotalLowerBound.Ticks);
            DateTime tub = new DateTime(TotalUpperBound.Ticks);
            TimeSpan dataSegments = new TimeSpan(DataSegment.Ticks * NumberOfDataSegmentLoaded);
            if (tub - tlb < dataSegments) //اگر کل بازه از حداقل تعداد دیتا سگمنت ها کمتر بود
            {
                TimeSpan extra = dataSegments - (tub - tlb);
                TimeSpan halfExtra = new TimeSpan(extra.Ticks / 2);
                long extraTicks = (DataSegment.Ticks * NumberOfDataSegmentLoaded) - (tub - tlb).Ticks;
                long halfExtraTicks = extraTicks / 2;

                Utility.DateTimeAddTryParse(tlb, -halfExtra, out tlb);
                Utility.DateTimeAddTryParse(tlb, dataSegments, out tub);

                hAxis.SetBinding(DateTimeContinuousAxis.MinimumProperty, new Binding("ActualTotalLowerBound")
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

                hAxis.SetBinding(DateTimeContinuousAxis.MaximumProperty, new Binding("ActualTotalUpperBound")
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });
            }
            else
            {
                hAxis.SetBinding(DateTimeContinuousAxis.MinimumProperty, new Binding("LowerBound")
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

                hAxis.SetBinding(DateTimeContinuousAxis.MaximumProperty, new Binding("UpperBound")
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });
            }

            ActualTotalLowerBound = tlb;
            ActualTotalUpperBound = tub;
        }

        private void AssignLoadedBounds(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                Utility.DateTimeAddTryParse(ActualTotalLowerBound, new TimeSpan((ActualTotalUpperBound - ActualTotalLowerBound).Ticks / 2), out dateTime);
            }

            double halfNumberOfDataSegmentLoaded = (double)NumberOfDataSegmentLoaded / 2;
            DateTime lb = Utility.MinValue;
            DateTime ub = Utility.MaxValue;
            Utility.DateTimeAddTryParse(dateTime.Value, new TimeSpan((long)(-halfNumberOfDataSegmentLoaded * DataSegment.Ticks)), out lb);
            if (!Utility.DateTimeAddTryParse(lb, new TimeSpan(NumberOfDataSegmentLoaded * DataSegment.Ticks), out ub))
            {
                Utility.DateTimeAddTryParse(ub, new TimeSpan(-NumberOfDataSegmentLoaded * DataSegment.Ticks), out lb);
            }

            LowerBound = lb;
            UpperBound = ub;
        }

        enum BarSelectionMode
        {
            /// <summary>
            /// تنها میله های مرتبط با سگمنت های انتخاب شده هایلایت می وشند و رخداد SegmentSelectionChanged فراخوانی نمی شود.
            /// </summary>
            OnlyHighlight,
            /// <summary>
            /// علاوه بر هایلایت شدن میله های مرتبط با سگمنت های انتخابی، رخداد SegmentSelectionChanged نیز فراخوانی می شود.
            /// </summary>
            SendEvent,
        }

        int barSegmentsSelectorCounter = 0;
        BarSelectionMode barSelectMode = BarSelectionMode.SendEvent;
        bool selectedBarStatus = true;

        private void ChartSelectionBehavior_SelectionChanged(object sender, ChartSelectionChangedEventArgs e)
        {
            List<DataPoint> selectionChangedDataPoints = e.AddedPoints.ToList();
            selectionChangedDataPoints.AddRange(e.RemovedPoints);

            if (e.AddedPoints.Count > 0)
            {
                selectedBarStatus = true;
            }
            if (e.RemovedPoints.Count > 0)
            {
                selectedBarStatus = false;
            }

            barSegmentsSelectorCounter++;

            foreach (CategoricalDataPoint changedPoint in selectionChangedDataPoints)
            {
                foreach (BarSeries barSerie in mainChart.Series)
                {
                    foreach (var dataPoint in barSerie.DataPoints)
                    {
                        if (dataPoint.Category.Equals(changedPoint.Category))
                        {
                            if (changedPoint.DataItem is TimelineItemsSegment)
                            {
                                (dataPoint.DataItem as TimelineItemsSegment).IsSelected = selectedBarStatus;
                            }
                            dataPoint.IsSelected = selectedBarStatus;
                        }
                    }
                }
            }

            barSegmentsSelectorCounter--;
            if (barSegmentsSelectorCounter > 0) //به این معناست که هنوز عملیات انتخاب همه قسمت های میله تکمیل نشده.
                return;

            if (barSelectMode == BarSelectionMode.SendEvent)
            {
                List<Range> added = new List<Range>();
                List<Range> removed = new List<Range>();

                foreach (var dataPoint in selectionChangedDataPoints)
                {
                    if (!(dataPoint.DataItem is TimelineItemsSegment))
                        continue;

                    TimelineItemsSegment dataItem = dataPoint.DataItem as TimelineItemsSegment;

                    if (dataPoint.IsSelected)
                    {
                        added.Add(dataItem);
                    }
                    else
                    {
                        removed.Add(dataItem);
                    }
                }

                selectedSegments = ManageSelectedSegments(selectedSegments, added, removed);

                SegmentSelectionChangedEventArgs segmentSelectionChangedEventArgs = new SegmentSelectionChangedEventArgs(added, removed);
                OnSegmentSelectionChanged(segmentSelectionChangedEventArgs);
            }
        }

        private List<Range> ManageSelectedSegments(List<Range> selectedSegments, List<Range> added, List<Range> removed)
        {
            List<Range> allSelectedSegments = new List<Range>(selectedSegments);
            if (allSelectedSegments == null)
            {
                allSelectedSegments = new List<Range>();
            }

            if (added != null)
                allSelectedSegments.AddRange(added);

            if (removed?.Count > 0)
            {
                int count = allSelectedSegments.Count;
                int i = 0;
                while (i < count)
                {
                    Range addedSegment = allSelectedSegments[i];
                    foreach (var removedSegment in removed)
                    {
                        List<Range> split = addedSegment.Subtract(removedSegment);
                        if (split.Count == 0)
                        {
                            allSelectedSegments.Remove(addedSegment);
                            i--;
                            count--;
                        }
                        else if (split.Count == 1)
                        {
                            allSelectedSegments[i] = split[0];
                        }
                        else if (split.Count == 2)
                        {
                            allSelectedSegments[i] = split[0];
                            i++;
                            allSelectedSegments.Insert(i, split[1]);
                            count++;
                        }
                    }

                    i++;
                }
            }

            return allSelectedSegments;
        }

        Canvas renderSurfaceCanvas = null;
        Border plotAreaBorder = null;
        private void mainChart_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var layoutRootBorder = mainChart.FindChildByType<Border>();
                if (layoutRootBorder != null)
                {
                    Canvas adornerCanvas = (layoutRootBorder.Child as Grid).Children[1] as Canvas;
                    Canvas labelCanvas = adornerCanvas.Children[0] as Canvas;

                    renderSurfaceCanvas = labelCanvas.Children[0] as Canvas;
                    renderSurfaceCanvas.MouseWheel -= RenderSurface_MouseWheel;
                    renderSurfaceCanvas.MouseWheel += RenderSurface_MouseWheel;
                    renderSurfaceCanvas.MouseMove -= RenderSurfaceCanvas_MouseMove;
                    renderSurfaceCanvas.MouseMove += RenderSurfaceCanvas_MouseMove;
                    renderSurfaceCanvas.MouseLeave += RenderSurfaceCanvas_MouseLeave;
                    renderSurfaceCanvas.MouseLeave += RenderSurfaceCanvas_MouseLeave;
                    renderSurfaceCanvas.MouseUp -= RenderSurfaceCanvas_MouseUp;
                    renderSurfaceCanvas.MouseUp += RenderSurfaceCanvas_MouseUp;
                    renderSurfaceCanvas.MouseLeftButtonDown -= RenderSurfaceCanvas_MouseLeftButtonDown;
                    renderSurfaceCanvas.MouseLeftButtonDown += RenderSurfaceCanvas_MouseLeftButtonDown;

                    plotAreaBorder = renderSurfaceCanvas.Children[0] as Border;
                    plotAreaBorder.Background = Brushes.Transparent;
                }
            }
            catch
            {
                renderSurfaceCanvas = null;
                plotAreaBorder = null;
            }
        }

        private void RenderSurfaceCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                AfterCreateFilterWindow(e.GetPosition(mainChart));
            }
            else
            {
                StartMovingPan(e.GetPosition(cartesianChartGrid));
            }
        }

        private void RenderSurfaceCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MovePanStatus = MoveStatus.Stop;
        }

        private void RenderSurfaceCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TotalUpperBound - TotalLowerBound < new TimeSpan(DataSegment.Ticks * NumberOfDataSegmentLoaded))
                SetCursorTime(Utility.CenterRange(TotalLowerBound, TotalUpperBound));
            else
                SetCursorTime(GetCenterDataPointHorizontalAxis());

            cursorPosition = GetCenterDataPointHorizontalAxis();
        }

        private void RenderSurfaceCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            ShowCrosshair();
            Point p = e.GetPosition(mainChart);
            double xDiff = mainChart.ActualWidth - plotAreaBorder.ActualWidth;
            if (p.X >= xDiff)
            {
                SetCursorTime(p);
                cursorPosition = p;
            }
            else
            {
                if (TotalUpperBound - TotalLowerBound < new TimeSpan(DataSegment.Ticks * NumberOfDataSegmentLoaded))
                    SetCursorTime(Utility.CenterRange(TotalLowerBound, TotalUpperBound));
                else
                    SetCursorTime(GetCenterDataPointHorizontalAxis());

                cursorPosition = GetCenterDataPointHorizontalAxis();
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                cartesianChartGrid.Cursor = Cursors.Cross;
            }
            else
            {
                cartesianChartGrid.Cursor = Cursors.Hand;
            }

            ResizingFilterWindow(e.GetPosition(mainChart));
            MovingFilterWindow(e.GetPosition(mainChart));
            MovingPan(p);
        }

        private Point GetCenterDataPointHorizontalAxis()
        {
            double xDiff = mainChart.ActualWidth - plotAreaBorder.ActualWidth;
            return new Point((mainChart.ActualWidth + xDiff) / 2, 0);
        }

        private void SetCursorTime(DateTime dateTime)
        {
            try
            {
                if (dateTime < TotalLowerBound)
                {
                    dateTime = TotalLowerBound;
                }
                else if (dateTime > TotalUpperBound)
                {
                    dateTime = TotalUpperBound;
                }

                cursorTime = dateTime;
            }
            catch
            {

            }
        }

        private void SetCursorTime(Point p)
        {
            try
            {
                DateTime dateTime = new DateTime();

                var dt = ConvertPointToData(p);
                if (dt == null)
                    return;

                dateTime = (DateTime)dt.FirstValue;
                SetCursorTime(dateTime);
            }
            catch
            {

            }
        }

        public DataTuple ConvertPointToData(Point p)
        {
            try
            {
                return mainChart.ConvertPointToData(p);
            }
            catch
            {
                return null;
            }
        }

        private void RenderSurface_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChartContainerMouseWheel(sender, e);
        }

        private void zoomGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChartContainerMouseWheel(sender, e);
        }

        private void zoomGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MovePanStatus = MoveStatus.Stop;
        }

        private void ChartContainerMouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (e.Delta > 0 && zoomSlider.Value < zoomSlider.Maximum)
                    {
                        zoomSlider.Value++;
                    }
                    else if (e.Delta < 0 && zoomSlider.Value > zoomSlider.Minimum)
                    {
                        zoomSlider.Value--;
                    }
                }
                else
                {
                    double sizeMovePanPixel = plotAreaBorder.ActualWidth / 8;
                    ItemsNeededAction direction = ItemsNeededAction.PanLeft;
                    if (e.Delta < 0)
                        direction = ItemsNeededAction.PanRight;

                    ChangePanPosition(sizeMovePanPixel, direction);
                }
            }
            catch
            {
                HideWaitingPrompt();
            }
        }

        private void ChangePanPosition(double sizeMovePanPixel, ItemsNeededAction direction)
        {
            if (sizeMovePanPixel == 0)
                return;

            TimeSpan durationPerPX = GetDurationPerPixel();
            TimeSpan sizeMovePanDuration = new TimeSpan((long)(durationPerPX.Ticks * sizeMovePanPixel));

            DateTime centerChart = new DateTime((hAxis.ActualVisibleRange.Minimum.Ticks + hAxis.ActualVisibleRange.Maximum.Ticks) / 2);

            DateTime minBound = ActualTotalLowerBound;
            DateTime maxBound = ActualTotalUpperBound;
            Utility.DateTimeAddTryParse(ActualTotalLowerBound, -DataSegment, out minBound);
            Utility.DateTimeAddTryParse(ActualTotalUpperBound, DataSegment, out maxBound);
            if (direction == ItemsNeededAction.PanLeft)
            {
                DateTime lowerBoundItemsNeeded = Utility.MinValue;
                Utility.DateTimeAddTryParse(LowerBound.Value, DataSegment, out lowerBoundItemsNeeded);
                if (LowerBound > TotalLowerBound && hAxis.ActualVisibleRange.Minimum < lowerBoundItemsNeeded &&
                    (currentItemsNeededEventArgs == null || currentItemsNeededEventArgs.Action != ItemsNeededAction.PanLeft))
                {
                    FetchItems(centerChart, null, ItemsNeededAction.PanLeft);
                }
                else
                {
                    DateTime minWaitingBound = new DateTime();
                    Utility.DateTimeAddTryParse(hAxis.ActualVisibleRange.Minimum, -sizeMovePanDuration, out minWaitingBound);

                    if (hAxis.ActualRange.Minimum >= minWaitingBound && LowerBound > TotalLowerBound)
                    {
                        ShowWaitingPrompt();
                    }
                    else
                    {
                        AddPanPosition(sizeMovePanPixel);
                    }
                }
            }
            else if (direction == ItemsNeededAction.PanRight) //حرکت پن به سمت راست
            {
                DateTime upperBoundItemsNeeded = Utility.MaxValue;
                Utility.DateTimeAddTryParse(UpperBound.Value, -DataSegment, out upperBoundItemsNeeded);
                if (UpperBound < TotalUpperBound && hAxis.ActualVisibleRange.Maximum > upperBoundItemsNeeded &&
                    (currentItemsNeededEventArgs == null || currentItemsNeededEventArgs.Action != ItemsNeededAction.PanRight))
                {
                    FetchItems(centerChart, null, ItemsNeededAction.PanRight);
                }
                else
                {
                    DateTime maxWaitingBound = new DateTime();
                    Utility.DateTimeAddTryParse(hAxis.ActualVisibleRange.Maximum, sizeMovePanDuration, out maxWaitingBound);

                    if (hAxis.ActualRange.Maximum <= maxWaitingBound && UpperBound < TotalUpperBound)
                    {
                        ShowWaitingPrompt();
                    }
                    else
                    {
                        AddPanPosition(-sizeMovePanPixel);
                    }
                }
            }
        }

        private async void AddPanPosition(double x)
        {
            await Dispatcher.BeginInvoke(new Action(() =>
            {
                mainChart.PanOffset = new Point(mainChart.PanOffset.X + x, 0);

                AssignLabelPropetiesForHorizontalAxis();
            }), DispatcherPriority.Normal);
        }

        public BinSize GetActualBin()
        {
            return ActualBin;
        }

        private TimeSpan GetDurationPerPixel()
        {
            DateTime d0 = hAxis.ActualVisibleRange.Minimum;
            Point p0 = mainChart.ConvertDataToPoint(new DataTuple(d0, 0));
            Point p1 = new Point(p0.X + 1, 0);
            DateTime d1 = (DateTime)mainChart.ConvertPointToData(p1).FirstValue;
            return d1 - d0;
        }

        private void SetPanPositonFromDate(DateTime focusTime, Point? focusPosition)
        {
            DateTime minDisplayRange = ActualTotalLowerBound;
            DateTime maxDisplayRange = ActualTotalUpperBound;
            TimeSpan halfDataSegment = new TimeSpan(DataSegment.Ticks / 2);
            Utility.DateTimeAddTryParse(focusTime, -halfDataSegment, out minDisplayRange);
            Utility.DateTimeAddTryParse(focusTime, halfDataSegment, out maxDisplayRange);
            DateTime curMinRange = hAxis.ActualVisibleRange.Minimum;
            DateTime curMaxRange = hAxis.ActualVisibleRange.Maximum;
            DateTime curCenter = new DateTime((curMaxRange.Ticks + curMinRange.Ticks) / 2);

            double x = 1;
            if (curCenter < focusTime)//پد باید به سمت راست حرکت کند.
            {
                x *= -1;
                while (curCenter < focusTime)
                {
                    Point lastPanOffset = mainChart.PanOffset;
                    mainChart.PanOffset = new Point(mainChart.PanOffset.X + x, mainChart.PanOffset.Y);
                    if (lastPanOffset == mainChart.PanOffset)
                        return;

                    curMinRange = hAxis.ActualVisibleRange.Minimum;
                    curMaxRange = hAxis.ActualVisibleRange.Maximum;
                    curCenter = new DateTime((curMaxRange.Ticks + curMinRange.Ticks) / 2);
                }
            }
            else if (curCenter > focusTime)
            {
                while (curCenter > focusTime)
                {
                    Point lastPanOffset = mainChart.PanOffset;
                    mainChart.PanOffset = new Point(mainChart.PanOffset.X + x, mainChart.PanOffset.Y);
                    if (lastPanOffset == mainChart.PanOffset)
                        return;

                    curMinRange = hAxis.ActualVisibleRange.Minimum;
                    curMaxRange = hAxis.ActualVisibleRange.Maximum;
                    curCenter = new DateTime((curMaxRange.Ticks + curMinRange.Ticks) / 2);
                }
            }

            if (focusPosition.HasValue)
            {
                var curPoint = mainChart.ConvertDataToPoint(new DataTuple(focusTime, 0));

                if (curPoint.X < focusPosition.Value.X)
                {
                    x = 1;
                    while (curPoint.X < focusPosition.Value.X)
                    {
                        Point lastPanOffset = mainChart.PanOffset;
                        mainChart.PanOffset = new Point(mainChart.PanOffset.X + x, mainChart.PanOffset.Y);
                        if (lastPanOffset == mainChart.PanOffset)
                            return;

                        curPoint = mainChart.ConvertDataToPoint(new DataTuple(focusTime, 0));
                    }
                }
                else if (curPoint.X > focusPosition.Value.X)
                {
                    x = -1;
                    while (curPoint.X > focusPosition.Value.X)
                    {
                        Point lastPanOffset = mainChart.PanOffset;
                        mainChart.PanOffset = new Point(mainChart.PanOffset.X + x, mainChart.PanOffset.Y);
                        if (lastPanOffset == mainChart.PanOffset)
                            return;

                        curPoint = mainChart.ConvertDataToPoint(new DataTuple(focusTime, 0));
                    }
                }
            }
        }

        private void FetchItems(DateTime? focusTime, Point? focusPosition, ItemsNeededAction action)
        {
            try
            {
                if ((action == ItemsNeededAction.ZoomIn || action == ItemsNeededAction.ZoomOut || action == ItemsNeededAction.None))
                {
                    ShowWaitingPrompt();
                }

                TimeSpan dataSegments = new TimeSpan(NumberOfDataSegmentLoaded * DataSegment.Ticks);
                TimeSpan halfDataSegments = new TimeSpan(dataSegments.Ticks / 2);
                DateTime beginTime = new DateTime();
                DateTime endTime = new DateTime();

                if (focusTime.HasValue)
                {
                    Utility.DateTimeAddTryParse(focusTime.Value, -halfDataSegments, out beginTime);
                    beginTime = Utility.Floor(beginTime, ActualBin);

                    if (beginTime < TotalLowerBound)
                    {
                        beginTime = TotalLowerBound;
                    }

                    Utility.DateTimeAddTryParse(beginTime, dataSegments, out endTime);
                    Utility.DateTimeAddTryParse(endTime, ActualBin.GetDuration(endTime), out endTime);
                    endTime = Utility.Floor(endTime, ActualBin);

                    if (endTime > TotalUpperBound)
                    {
                        endTime = TotalUpperBound;
                        if (!Utility.DateTimeAddTryParse(endTime, -dataSegments, out beginTime))
                        {
                            Utility.DateTimeAddTryParse(beginTime, dataSegments, out endTime);
                        }
                    }
                }
                else
                {
                    beginTime = Utility.MinValue;
                    endTime = Utility.MaxValue;
                }

                currentItemsNeededEventArgs = new ItemsNeededEventArgs(beginTime, endTime, ActualBin.BinScale, ActualBin.BinFactor, action, focusTime, focusPosition,
                                                                            TotalLowerBound, TotalUpperBound, MaximumCount);
                OnItemsNeeded(currentItemsNeededEventArgs);
            }
            catch
            {
                HideWaitingPrompt();
            }
        }

        public async void AddItems(ItemsNeededEventArgs e)
        {
            try
            {
                if (e == null || e.FetchedItems == null)
                {
                    ClearAllData();
                    HideWaitingPrompt();
                    return;
                }

                if (currentItemsNeededEventArgs != null && currentItemsNeededEventArgs != e)
                {
                    return;
                }

                var beginTime = e.BeginTime;
                var endTime = e.EndTime;
                var focusTime = e.FocusTime;
                var focusPosition = e.FocusPosition;
                var action = e.Action;
                List<SuperCategory> fetchedItems = e.FetchedItems;
                DateTime totalLowerBound = e.TotalLowerBound;
                DateTime totalUpperBound = e.TotalUpperBound;
                double maximumCount = e.MaximumCount;

                ShowWaitingPrompt();

                AssignTotalBounds(totalLowerBound, totalUpperBound);

                barSelectMode = BarSelectionMode.OnlyHighlight;

                currentItemsNeededEventArgs = null;

                if (!focusTime.HasValue)
                {
                    focusTime = Utility.CenterRange(beginTime, endTime);
                }

                if (action == ItemsNeededAction.PanLeft || action == ItemsNeededAction.PanRight)
                {
                    focusTime = Utility.CenterRange(hAxis.ActualVisibleRange.Minimum, hAxis.ActualVisibleRange.Maximum);
                }

                AssignActualTotalBounds();
                if (action == ItemsNeededAction.ZoomIn || action == ItemsNeededAction.ZoomOut || action == ItemsNeededAction.None)
                {
                    ResetHorizontalAxis();
                    AssignBinForFilterWindows();
                }

                if (action == ItemsNeededAction.None)
                {
                    beginTime = TotalLowerBound;
                    endTime = TotalUpperBound;
                    focusTime = Utility.CenterRange(beginTime, endTime);
                }

                LowerBound = beginTime;
                UpperBound = endTime;

                if (action == ItemsNeededAction.None)
                    AssignLoadedBounds(focusTime);

                MaximumCount = maximumCount;
                await Dispatcher?.BeginInvoke(new Action(() => AddFetchedItemsToChart(fetchedItems)), DispatcherPriority.Normal);

                await Dispatcher?.BeginInvoke(new Action(() => SetPanPositonFromDate(focusTime.Value, focusPosition)), DispatcherPriority.Normal);

                HighlightSelectedSegments();

                await Dispatcher?.BeginInvoke(new Action(() =>
                {
                    foreach (var fw in filterWindows)
                    {
                        fw.SetBinChanged();
                    }

                }), DispatcherPriority.Normal);

                barSelectMode = BarSelectionMode.SendEvent;
                HideWaitingPrompt();
                var layoutRootBorder = mainChart.FindChildByType<Canvas>();

                var layoutRootBorder2 = mainChart.FindName("renderSurface");
            }
            catch
            {
                barSelectMode = BarSelectionMode.SendEvent;
                HideWaitingPrompt();
            }
        }

        private void HighlightSelectedSegments()
        {
            RemoveHighlightAllBars();
            HighlightRelatedBarsWithSegments(selectedSegments);
        }

        private void RemoveHighlightAllBars()
        {
            barSelectMode = BarSelectionMode.OnlyHighlight;
            DeselectAllBar();
            barSelectMode = BarSelectionMode.SendEvent;
        }

        private void DeselectAllBar()
        {
            foreach (var cat in SubCategories)
            {
                if (cat.RelatedSeries != null && cat.RelatedSeries.DataPoints != null)
                {
                    foreach (var dataPoint in cat.RelatedSeries.DataPoints)
                    {
                        if (dataPoint.IsSelected)
                        {
                            dataPoint.IsSelected = false;
                            if (dataPoint.DataItem is TimelineItemsSegment)
                            {
                                (dataPoint.DataItem as TimelineItemsSegment).IsSelected = false;
                            }
                        }
                    }
                }
            }
        }

        private void HighlightRelatedBarsWithSegments(List<Range> segments)
        {
            if (segments == null || segments.Count == 0)
                return;

            barSelectMode = BarSelectionMode.OnlyHighlight;
            SelectRelatedDataItemsWithSegments(segments);
            barSelectMode = BarSelectionMode.SendEvent;
        }

        private void SelectRelatedDataItemsWithSegments(List<Range> segments)
        {
            if (segments == null || segments.Count == 0)
                return;

            List<Range> curSegments = new List<Range>(segments);

            Dictionary<object, CategoricalDataPoint> dataPoints = new Dictionary<object, CategoricalDataPoint>();
            foreach (var cat in SubCategories)
            {
                if (cat.RelatedSeries == null)
                    continue;

                foreach (var dataPoint in cat.RelatedSeries.DataPoints)
                {
                    if (!dataPoints.Keys.Contains(dataPoint.Category))
                    {
                        dataPoints[dataPoint.Category] = dataPoint;
                    }
                }
            }

            foreach (var dataPoint in dataPoints.Values)
            {
                if (dataPoint.DataItem is TimelineItemsSegment)
                {
                    TimelineItemsSegment dataItem = (TimelineItemsSegment)dataPoint.DataItem;
                    foreach (var segment in curSegments)
                    {
                        if (Range.Intersect(segment, dataItem) != null)
                        {
                            dataItem.IsSelected = true;
                            dataPoint.IsSelected = true;
                        }
                    }
                }
            }
        }

        public void AddSegmentsToSelectedSegments(List<Range> segments)
        {
            List<Range> lastSelectedSegments = new List<Range>(SelectedSegments);
            AddSegmentsToHighlightedSegments(segments);
            OnSegmentSelectionChanged(new SegmentSelectionChangedEventArgs(segments, lastSelectedSegments));
        }

        public void AddSegmentsToHighlightedSegments(List<Range> segments)
        {
            selectedSegments = new List<Range>();
            RemoveHighlightAllBars();
            barSelectMode = BarSelectionMode.OnlyHighlight;
            selectedSegments = ManageSelectedSegments(selectedSegments, segments, null);
            SelectRelatedDataItemsWithSegments(selectedSegments);
            barSelectMode = BarSelectionMode.SendEvent;
        }

        public void AppendSegmentsToSelectedSegments(List<Range> segments)
        {
            AppendSegmentsToHighlightedSegments(segments);

            OnSegmentSelectionChanged(new SegmentSelectionChangedEventArgs(segments, new List<Range>()));
        }

        public void AppendSegmentsToHighlightedSegments(List<Range> segments)
        {
            RemoveHighlightAllBars();
            barSelectMode = BarSelectionMode.OnlyHighlight;
            selectedSegments = ManageSelectedSegments(selectedSegments, segments, null);
            SelectRelatedDataItemsWithSegments(selectedSegments);
            barSelectMode = BarSelectionMode.SendEvent;
        }

        public void RemoveSegmentsFromSelectedSegments(List<Range> segments)
        {
            RemoveSegmentsFromHighlightedSegments(segments);

            OnSegmentSelectionChanged(new SegmentSelectionChangedEventArgs(new List<Range>(), segments));
        }

        public void RemoveSegmentsFromHighlightedSegments(List<Range> segments)
        {
            RemoveHighlightAllBars();
            barSelectMode = BarSelectionMode.OnlyHighlight;
            selectedSegments = ManageSelectedSegments(selectedSegments, null, segments);
            SelectRelatedDataItemsWithSegments(selectedSegments);
            barSelectMode = BarSelectionMode.SendEvent;
        }

        public void RemoveAllSelectedSegments()
        {
            List<Range> lastSelectedSegments = new List<Range>(SelectedSegments);

            RemoveAllHighlightedSegments();

            OnSegmentSelectionChanged(new SegmentSelectionChangedEventArgs(new List<Range>(), lastSelectedSegments));
        }

        public void RemoveAllHighlightedSegments()
        {
            selectedSegments = new List<Range>();
            RemoveHighlightAllBars();
        }

        private void AddFetchedItemsToChart(List<SuperCategory> fetched)
        {
            List<Category> addedSubCategories = new List<Category>();
            foreach (var superCategory in fetched)
            {
                if (!SuperCategories.Select(sc => sc.Tag).Contains(superCategory.Tag))
                {
                    SuperCategories.Add(superCategory);
                }
                addedSubCategories.AddRange(superCategory.SubCategories);
            }

            foreach (var addedSubCat in addedSubCategories)
            {
                if (!SubCategories.Select(sc => sc.Identifier).Contains(addedSubCat.Identifier))
                {
                    SuperCategory parent = SuperCategories.Where(supC => supC.Tag.Equals(addedSubCat.Parent.Tag)).FirstOrDefault();
                    parent.SubCategories.Add(addedSubCat);
                }
            }

            Dictionary<object, IEnumerable<TimelineItemsSegment>> fetchedDataItems = new Dictionary<object, IEnumerable<TimelineItemsSegment>>();

            foreach (var superCategory in fetched)
            {
                foreach (var subCat in superCategory.SubCategories)
                {
                    fetchedDataItems.Add(subCat.Identifier, subCat.DataItems);
                }
            }

            foreach (var subC in SubCategories)
            {
                var a = fetchedDataItems.Keys.FirstOrDefault(fi => fi.Equals(subC.Identifier));

                if (a != null)
                    subC.DataItems = new ObservableCollection<TimelineItemsSegment>(fetchedDataItems[a]);
            }

            bool allSubCatNoDataItem = true;
            foreach (var subC in SubCategories)
            {
                if (subC.IsChecked && subC.InBoundDataItems.Count > 0)
                {
                    allSubCatNoDataItem = false;
                    break;
                }
            }

            AddExtraSeriesIfNeed();
            if (HasExtraSeies())
            {
                Category extraCat = GetExtraCategory();
                var lbei = new TimelineItemsSegment()
                {
                    FrequencyCount = 0,
                    Type = DataItemType.LowerBoundExtraItem,
                };
                var ubei = new TimelineItemsSegment()
                {
                    FrequencyCount = 0,
                    Type = DataItemType.UpperBoundExtraItem,
                };

                if (allSubCatNoDataItem)
                {
                    lbei.From = LowerBound.Value;
                    DateTime to = new DateTime();
                    Utility.DateTimeAddTryParse(lbei.From, ActualBin.GetDuration(lbei.From), out to);
                    lbei.To = to;

                    ubei.To = UpperBound.Value;
                    DateTime from = new DateTime();
                    Utility.DateTimeAddTryParse(ubei.To, -ActualBin.GetDuration(ubei.To), out from);
                    ubei.From = from;
                }
                else
                {
                    TimelineItemsSegment lastDataItem = null;
                    TimelineItemsSegment firstDataItem = null;
                    foreach (var subC in SubCategories)
                    {
                        if (subC.InBoundDataItems == null || subC.InBoundDataItems.Count == 0)
                            continue;

                        var minFrom = subC.InBoundDataItems.Min(di => di.From);
                        var f = subC.InBoundDataItems.Where(di => di.From == minFrom).FirstOrDefault();
                        var maxTo = subC.InBoundDataItems.Max(di => di.To);
                        var l = subC.InBoundDataItems.Where(di => di.To == maxTo).LastOrDefault();

                        if (firstDataItem == null)
                        {
                            firstDataItem = f;
                        }
                        else
                        {
                            if (firstDataItem.From > f.From)
                            {
                                firstDataItem = f;
                            }
                        }
                        if (lastDataItem == null)
                        {
                            lastDataItem = l;
                        }
                        else
                        {
                            if (lastDataItem.To < l.To)
                            {
                                lastDataItem = l;
                            }
                        }
                    }
                    lbei.From = firstDataItem.From;
                    lbei.To = firstDataItem.To;

                    ubei.From = lastDataItem.From;
                    ubei.To = lastDataItem.To;
                }

                extraCat.DataItems = new ObservableCollection<TimelineItemsSegment>()
                {
                    lbei,
                    ubei,
                };

                if (hAxis?.Minimum == Utility.MinValue)
                {
                    TimeSpan duration = ActualBin.GetDuration(Utility.MinValue);
                    DateTime to = new DateTime();
                    Utility.DateTimeAddTryParse(Utility.MinValue, duration, out to);
                    var tlbEI = new TimelineItemsSegment()
                    {
                        FrequencyCount = 0,
                        From = Utility.MinValue,
                        To = to,
                        Type = DataItemType.TotalLowerBoundExtraItem,
                    };

                    extraCat.DataItems = new ObservableCollection<TimelineItemsSegment>(new List<TimelineItemsSegment>() { tlbEI });
                }
                else
                {
                    var tlbEI = extraCat.DataItems.Where(t => t.Type == DataItemType.TotalLowerBoundExtraItem).FirstOrDefault();
                    if (tlbEI != null)
                        extraCat.DataItems.Remove(tlbEI);
                }

                if (hAxis?.Maximum == Utility.MaxValue)
                {
                    TimeSpan duration = ActualBin.GetDuration(Utility.MaxValue);
                    DateTime from = new DateTime();
                    Utility.DateTimeAddTryParse(Utility.MaxValue, -duration, out from);
                    var tubEI = new TimelineItemsSegment()
                    {
                        FrequencyCount = 0,
                        From = from,
                        To = Utility.MaxValue,
                        Type = DataItemType.TotalUpperBoundExtraItem,
                    };

                    extraCat.DataItems = new ObservableCollection<TimelineItemsSegment>(new List<TimelineItemsSegment>() { tubEI });
                }
                else
                {
                    var tubEI = extraCat.DataItems.Where(t => t.Type == DataItemType.TotalUpperBoundExtraItem).FirstOrDefault();
                    if (tubEI != null)
                        extraCat.DataItems.Remove(tubEI);
                }
            }
        }

        private void ShowCrosshair()
        {
            CrosshairChart.VerticalLineLabelVisibility = Visibility.Visible;
            CrosshairChart.VerticalLineVisibility = Visibility.Visible;
            CrosshairChart.HorizontalLineLabelVisibility = Visibility.Visible;
            CrosshairChart.HorizontalLineVisibility = Visibility.Visible;
        }

        private void HideCrosshair()
        {
            CrosshairChart.VerticalLineLabelVisibility = Visibility.Collapsed;
            CrosshairChart.VerticalLineVisibility = Visibility.Collapsed;
            CrosshairChart.HorizontalLineLabelVisibility = Visibility.Collapsed;
            CrosshairChart.HorizontalLineVisibility = Visibility.Collapsed;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            StopResizeFilterWindow();
            StopMoveFilterWindow();
            MovePanStatus = MoveStatus.Stop;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                if (resizingFilterWindowStatus == MoveStatus.Moving || resizingFilterWindowStatus == MoveStatus.Start)
                {
                    StopResizeFilterWindow();
                }

                if (movingFilterWindowStatus == MoveStatus.Moving || movingFilterWindowStatus == MoveStatus.Start)
                {
                    StopMoveFilterWindow();
                }

                MovePanStatus = MoveStatus.Stop;
            }
        }

        MoveStatus MovePanStatus
        {
            get { return movePanStatus; }
            set
            {
                movePanStatus = value;
                if (movePanStatus == MoveStatus.Start || movePanStatus == MoveStatus.Moving)
                {
                    foreach (var fw in filterWindows)
                    {
                        fw.IsHitTestVisibleForFill = false;
                        fw.IsHitTestVisibleForStroke = false;
                    }
                }
                else if (movePanStatus == MoveStatus.Ready || movePanStatus == MoveStatus.Stop)
                {
                    foreach (var fw in filterWindows)
                    {
                        fw.IsHitTestVisibleForFill = true;
                        fw.IsHitTestVisibleForStroke = true;
                    }
                }
            }
        }

        Point lastMousePosition = new Point(0, 0);
        private void cartesianChartGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void StartMovingPan(Point point)
        {
            MovePanStatus = MoveStatus.Start;
            lastMousePosition = point;
        }

        private void SelectedFilterWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            HideCrosshair();
            selectedFilterWindow = (FilterWindow)sender;
            StartMovingFilterWindow(e.GetPosition(mainChart));
        }

        private void SelectedFilterWindow_MouseLeave(object sender, FilterWindowStrokeMouseEventArgs e)
        {
            if (resizingFilterWindowStatus == MoveStatus.Moving)
            {
                if (currentStrokeState == FilterWindowStrokeMouseEventState.Start)
                {
                    startDragFilterWindow = (DateTime)mainChart.ConvertPointToData(e.GetPosition(mainChart)).FirstValue;
                }
                else if (currentStrokeState == FilterWindowStrokeMouseEventState.End)
                {
                    endDragFilterWindow = (DateTime)mainChart.ConvertPointToData(e.GetPosition(mainChart)).FirstValue;
                }
                else
                {
                    throw new NotSupportedException();
                }

                ValidateFilterWindowInResizing();
                GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);
            }

            ShowCrosshair();
        }

        private void SelectedFilterWindow_MouseEnter(object sender, FilterWindowStrokeMouseEventArgs e)
        {
            HideCrosshair();
        }

        private void SelectedFilterWindow_StrokeMouseLeftButtonDown(object sender, FilterWindowStrokeMouseButtonEventArgs e)
        {
            selectedFilterWindow = (FilterWindow)sender;
            currentStrokeState = e.StrokeState;

            startDragFilterWindow = selectedFilterWindow.From;
            endDragFilterWindow = selectedFilterWindow.To;

            StartResizeFilterWindow(e.GetPosition(mainChart));
        }
        private void SelectedFilterWindow_CloseButtonClicked(object sender, EventArgs e)
        {
            filterWindows.Remove((FilterWindow)sender);
        }

        private void cartesianChartGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopResizeFilterWindow();
            StopMoveFilterWindow();
            MovePanStatus = MoveStatus.Stop;
        }

        private void cartesianChartGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (resizingFilterWindowStatus == MoveStatus.Moving)
            {
                if (currentStrokeState == FilterWindowStrokeMouseEventState.Start)
                {
                    startDragFilterWindow = (DateTime)mainChart.ConvertPointToData(e.GetPosition(mainChart)).FirstValue;
                }
                else if (currentStrokeState == FilterWindowStrokeMouseEventState.End)
                {
                    endDragFilterWindow = (DateTime)mainChart.ConvertPointToData(e.GetPosition(mainChart)).FirstValue;
                }
                else
                {
                    throw new NotSupportedException();
                }

                ValidateFilterWindowInResizing();
                GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);
            }
        }

        private void cartesianChartGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void MovingPan(Point point)
        {
            if ((MovePanStatus == MoveStatus.Moving || MovePanStatus == MoveStatus.Start)
                && (movingFilterWindowStatus == MoveStatus.Stop || movingFilterWindowStatus == MoveStatus.Ready)
                && (resizingFilterWindowStatus == MoveStatus.Stop || resizingFilterWindowStatus == MoveStatus.Ready))
            {
                MovePanStatus = MoveStatus.Moving;
                double sizeMovePanPixel = point.X - lastMousePosition.X;
                ItemsNeededAction direction = ItemsNeededAction.PanRight;
                if (sizeMovePanPixel > 0)
                {
                    direction = ItemsNeededAction.PanLeft;
                }

                sizeMovePanPixel = Math.Abs(sizeMovePanPixel);
                ChangePanPosition(sizeMovePanPixel, direction);
                lastMousePosition = point;
            }
        }

        private void StopResizeFilterWindow()
        {
            if (resizingFilterWindowStatus == MoveStatus.Moving)
            {
                ShowCrosshair();
                mainChart.Grid.Cursor = Cursors.Arrow;

                foreach (var fw in filterWindows)
                {
                    fw.IsHitTestVisibleForFill = true;
                    fw.IsHitTestVisibleForStroke = true;
                }
                selectedFilterWindow.ToolTip.IsOpen = false;
                foreach (var subC in SubCategories)
                {
                    if (subC.RelatedSeries != null)
                        subC.RelatedSeries.IsHitTestVisible = true;
                }

                List<FilterWindow> added = new List<FilterWindow>() { selectedFilterWindow };
                List<FilterWindow> removed = new List<FilterWindow>();
                OnFilterWindowsChanged(new FilterWindowsChangedEventArgs(added, removed));
            }

            resizingFilterWindowStatus = MoveStatus.Stop;
        }

        private void StopMoveFilterWindow()
        {
            if (movingFilterWindowStatus == MoveStatus.Moving || movingFilterWindowStatus == MoveStatus.Start)
            {
                ShowCrosshair();
                movingFilterWindowStatus = MoveStatus.Stop;
                mainChart.Grid.Cursor = Cursors.Arrow;

                foreach (var fw in filterWindows)
                {
                    fw.IsHitTestVisibleForFill = true;
                    fw.IsHitTestVisibleForStroke = true;
                }
                selectedFilterWindow.ToolTip.IsOpen = false;
                foreach (var subC in SubCategories)
                {
                    if (subC.RelatedSeries != null)
                        subC.RelatedSeries.IsHitTestVisible = true;
                }

                List<FilterWindow> added = new List<FilterWindow>() { selectedFilterWindow };
                List<FilterWindow> removed = new List<FilterWindow>();
                OnFilterWindowsChanged(new FilterWindowsChangedEventArgs(added, removed));
            }
        }

        private bool CreateNewFilterWindow(Point point)
        {
            DateTime start = (DateTime)mainChart.ConvertPointToData(point).FirstValue;
            if (start < TotalLowerBound || start > TotalUpperBound)
                return false;

            selectedFilterWindow = new FilterWindow()
            {
                HorizontalAxis = hAxis,
                VerticalAxis = vAxis as NumericalAxis,
                ToolTipStyle = Resources["styleBarToolTip"] as Style,
                ContextMenu = Resources["FilterWindowContextMenu"] as ContextMenu,
            };

            selectedFilterWindow.CloseButtonClicked -= SelectedFilterWindow_CloseButtonClicked;
            selectedFilterWindow.CloseButtonClicked += SelectedFilterWindow_CloseButtonClicked;
            selectedFilterWindow.StrokeMouseLeftButtonDown -= SelectedFilterWindow_StrokeMouseLeftButtonDown;
            selectedFilterWindow.StrokeMouseLeftButtonDown += SelectedFilterWindow_StrokeMouseLeftButtonDown;
            selectedFilterWindow.MouseEnter -= SelectedFilterWindow_MouseEnter;
            selectedFilterWindow.MouseEnter += SelectedFilterWindow_MouseEnter;
            selectedFilterWindow.MouseMove -= SelectedFilterWindow_MouseMove;
            selectedFilterWindow.MouseMove += SelectedFilterWindow_MouseMove;
            selectedFilterWindow.MouseLeave -= SelectedFilterWindow_MouseLeave;
            selectedFilterWindow.MouseLeave += SelectedFilterWindow_MouseLeave;
            selectedFilterWindow.MouseLeftButtonDown -= SelectedFilterWindow_MouseLeftButtonDown;
            selectedFilterWindow.MouseLeftButtonDown += SelectedFilterWindow_MouseLeftButtonDown;

            startDragFilterWindow = start;
            endDragFilterWindow = startDragFilterWindow;

            ValidateFilterWindowInResizing();
            GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);
            return true;
        }

        private void SelectedFilterWindow_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        private void ValidateFilterWindowInResizing()
        {
            BindToChartFilterWindow();
        }

        private void BindToChartFilterWindow()
        {
            if (currentStrokeState == FilterWindowStrokeMouseEventState.Start)
            {
                if (startDragFilterWindow < TotalLowerBound)
                {
                    startDragFilterWindow = TotalLowerBound;
                }
                if (startDragFilterWindow > TotalUpperBound)
                {
                    startDragFilterWindow = TotalUpperBound;
                }
            }
            else if (currentStrokeState == FilterWindowStrokeMouseEventState.End)
            {
                if (endDragFilterWindow < TotalLowerBound)
                {
                    endDragFilterWindow = TotalLowerBound;
                }
                if (endDragFilterWindow > TotalUpperBound)
                {
                    endDragFilterWindow = TotalUpperBound;
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void GenerateFilterWindow(DateTime start, DateTime end)
        {
            selectedFilterWindow.From = start;
            selectedFilterWindow.To = end;
        }

        private void StartMovingFilterWindow(Point point)
        {
            moveDragFilterWindow = (DateTime)mainChart.ConvertPointToData(point).FirstValue;

            foreach (var fw in filterWindows)
            {
                fw.IsHitTestVisibleForStroke = false;
                fw.IsHitTestVisibleForFill = false;
            }

            startDragFilterWindow = selectedFilterWindow.From;
            endDragFilterWindow = selectedFilterWindow.To;

            GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);

            movingFilterWindowStatus = MoveStatus.Start;
            mainChart.Grid.Cursor = Cursors.ScrollWE;
            selectedFilterWindow.ToolTip.IsOpen = true;

            foreach (var subC in SubCategories)
            {
                if (subC.RelatedSeries != null)
                    subC.RelatedSeries.IsHitTestVisible = false;
            }
        }

        Point sourcePointForCreateFilterWindow = new Point();
        private void AfterCreateFilterWindow(Point point)
        {
            resizingFilterWindowStatus = MoveStatus.Start;
            sourcePointForCreateFilterWindow = point;
        }

        private void StartResizeFilterWindow(Point point)
        {
            if (selectedFilterWindow == null)
                return;
            foreach (var fw in filterWindows)
            {
                fw.IsHitTestVisibleForFill = false;
                if (fw != selectedFilterWindow)
                    fw.IsHitTestVisibleForStroke = false;
            }
            selectedFilterWindow.IsHitTestVisibleForFill = false;

            if (currentStrokeState == FilterWindowStrokeMouseEventState.Start)
            {
                startDragFilterWindow = (DateTime)mainChart.ConvertPointToData(point).FirstValue;
            }
            else if (currentStrokeState == FilterWindowStrokeMouseEventState.End)
            {
                endDragFilterWindow = (DateTime)mainChart.ConvertPointToData(point).FirstValue;
            }
            else
            {
                throw new NotSupportedException();
            }

            ValidateFilterWindowInResizing();
            GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);

            resizingFilterWindowStatus = MoveStatus.Moving;
            mainChart.Grid.Cursor = Cursors.SizeWE;

            foreach (var subC in SubCategories)
            {
                if (subC.RelatedSeries != null)
                    subC.RelatedSeries.IsHitTestVisible = false;
            }
        }

        private void ResizingFilterWindow(Point point)
        {
            if (resizingFilterWindowStatus == MoveStatus.Start)
            {
                if (CreateNewFilterWindow(sourcePointForCreateFilterWindow))
                {
                    currentStrokeState = FilterWindowStrokeMouseEventState.End;
                    StartResizeFilterWindow(sourcePointForCreateFilterWindow);
                }
            }

            if (resizingFilterWindowStatus == MoveStatus.Moving)
            {
                HideCrosshair();
                if (!filterWindows.Contains(selectedFilterWindow))
                {
                    filterWindows.Add(selectedFilterWindow);
                }

                if (currentStrokeState == FilterWindowStrokeMouseEventState.Start)
                {
                    startDragFilterWindow = (DateTime)mainChart.ConvertPointToData(point).FirstValue;
                    if (startDragFilterWindow > endDragFilterWindow)
                    {
                        var t = startDragFilterWindow;
                        startDragFilterWindow = endDragFilterWindow;
                        endDragFilterWindow = t;
                        currentStrokeState = FilterWindowStrokeMouseEventState.End;
                    }
                }
                else if (currentStrokeState == FilterWindowStrokeMouseEventState.End)
                {
                    endDragFilterWindow = (DateTime)mainChart.ConvertPointToData(point).FirstValue;
                }
                else
                {
                    throw new NotSupportedException();
                }

                ValidateFilterWindowInResizing();
                GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);

                selectedFilterWindow.ToolTip.IsOpen = true;
            }
        }

        private void MovingFilterWindow(Point point)
        {
            if (movingFilterWindowStatus == MoveStatus.Moving || movingFilterWindowStatus == MoveStatus.Start)
            {
                HideCrosshair();
                DateTime previousMoveDrad = moveDragFilterWindow;
                moveDragFilterWindow = (DateTime)mainChart.ConvertPointToData(point).FirstValue;
                TimeSpan dis = moveDragFilterWindow - previousMoveDrad;

                DateTime start = new DateTime();
                DateTime end = new DateTime();
                Utility.DateTimeAddTryParse(startDragFilterWindow, dis, out start);
                Utility.DateTimeAddTryParse(endDragFilterWindow, dis, out end);

                ValidateFilterWindowInMoving(start, end);
                GenerateFilterWindow(startDragFilterWindow, endDragFilterWindow);
            }
        }

        private void ValidateFilterWindowInMoving(DateTime start, DateTime end)
        {
            if (start < TotalLowerBound)
            {
                TimeSpan diff = end - start;
                start = TotalLowerBound;
                Utility.DateTimeAddTryParse(start, diff, out end);

            }
            if (end > TotalUpperBound)
            {
                TimeSpan diff = end - start;
                end = TotalUpperBound;
                Utility.DateTimeAddTryParse(end, -diff, out start);
            }

            startDragFilterWindow = start;
            endDragFilterWindow = end;
        }

        private void RemoveAllFilterWindows()
        {
            List<FilterWindow> added = new List<FilterWindow>();
            List<FilterWindow> removed = new List<FilterWindow>();

            foreach (FilterWindow item in filterWindows)
            {
                item.RemoveAnnotationsToChart(mainChart.Annotations);
                removed.Add(item);
            }

            filterWindows.Clear();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllFilterWindows();
        }

        private void btnSnapShot_Click(object sender, RoutedEventArgs e)
        {
            btnSnapShot.ContextMenu.IsOpen = true;
        }

        private void mnuSnapshotOnlyTimeline_Click(object sender, RoutedEventArgs e)
        {
            Brush oldBackground = chartGrid.Background;
            chartGrid.Background = Brushes.White;
            OnSnapshotTaken(new SnapshotTakenEventArgs(chartGrid));
            chartGrid.Background = oldBackground;
        }

        private void mnuSnapshotTimelineWithCategoryPan_Click(object sender, RoutedEventArgs e)
        {
            ToolbarBottomBorder.Visibility = Visibility.Collapsed;
            //  bool oldExpanderState = CategoryPanExpander.IsExpanded;
            //  CategoryPanExpander.IsExpanded = true;
            OnSnapshotTaken(new SnapshotTakenEventArgs(mainGrid));
            //  CategoryPanExpander.IsExpanded = oldExpanderState;
            ToolbarBottomBorder.Visibility = Visibility.Visible;
        }

        private void btnZoomPanToAllEvents_Click(object sender, RoutedEventArgs e)
        {
            if (SubCategories == null || SubCategories.Count == 0)
                return;

            BinSizesEnum binSizesEnum = BinSizeForAllDataInOneSegment(TotalLowerBound, TotalUpperBound);
            if (binSizesEnum == Bin)
            {
                SetPanPositonFromDate(Utility.CenterRange(TotalLowerBound, TotalUpperBound), null);
            }
            else
            {
                Bin = binSizesEnum;
            }
        }

        private BinSizesEnum BinSizeForAllDataInOneSegment(DateTime min, DateTime max)
        {
            TimeSpan totalDuration = max - min;
            TimeSpan customDataSegment = new TimeSpan(totalDuration.Ticks / NumberOfBarsPerEachDataSegment);

            for (int i = 16; i >= 1; i--)
            {
                BinSizesEnum binSizesEnum = (BinSizesEnum)i;
                BinSize binSize = BinSizes.FindBinSizeFromEnum(binSizesEnum, min, max, NumberOfDataSegmentLoaded * NumberOfBarsPerEachDataSegment);
                TimeSpan binDuration = binSize.GetDuration(min);
                if (i == 1 || binDuration > customDataSegment)
                {
                    return binSizesEnum;
                }
            }

            return BinSizesEnum.TenYears;
        }

        private void SnapshotTimeline_Opened(object sender, RoutedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
            SnapshotTimeline.Background = new SolidColorBrush(theme.Paper);
            SnapshotTimeline.Foreground = new SolidColorBrush(theme.Body);
        }


        private void mnuClearFilterWindow_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mnuClose = (MenuItem)sender;
            ContextMenu parent = (ContextMenu)mnuClose.Parent;
            CartesianChartAnnotation annotation = (CartesianChartAnnotation)parent.PlacementTarget;

            FilterWindow filterWindow = null;
            foreach (var fw in filterWindows)
            {
                if (fw.HasAnnotation(annotation))
                {
                    filterWindow = fw;
                    break;
                }
            }

            if (filterWindow != null)
            {
                filterWindows.Remove(filterWindow);
            }
        }

        private void SetAllFilterWindowsVerticalAxis()
        {
            foreach (FilterWindow fw in filterWindows)
            {
                fw.VerticalAxis = mainChart.VerticalAxis as NumericalAxis;
            }
        }
    }
}
