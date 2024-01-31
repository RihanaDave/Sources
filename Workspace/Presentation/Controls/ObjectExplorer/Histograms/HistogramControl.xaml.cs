using GPAS.Histogram;
using GPAS.Logger;
using GPAS.ObjectExplorerHistogramViewer;
using GPAS.Workspace.Logic;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Histograms
{
    /// <summary>
    /// Interaction logic for HistogramControl.xaml
    /// </summary>
    public partial class HistogramControl : INotifyPropertyChanged
    {
        #region متغیرهای سراسری
        public readonly int minimumShowingHistogramKeyValuePairs = 10;

        public static readonly int PassObjectsCountLimit = int.Parse(ConfigurationManager.AppSettings["ObjectExplorerPassObjectsCountLimit"]);

        private Dictionary<HistogramKeyValuePair, PreviewStatistic> keyValueToPreviewStatisticMapping = new Dictionary<HistogramKeyValuePair, PreviewStatistic>();

        public PreviewStatistics PreviewStatisticsToShowHistogram { get; set; }

        private HistogramSuperCategoryType selectedSuperCategoryType;
        public HistogramSuperCategoryType SelectedSuperCategoryType
        {
            get { return selectedSuperCategoryType; }
            set
            {
                if (selectedSuperCategoryType != value)
                {
                    selectedSuperCategoryType = value;
                    NotifyPropertyChanged("SelectedSuperCategoryType");
                }
            }
        }

        private string typeURI;
        public string TypeURI
        {
            get { return typeURI; }
            set
            {
                if (typeURI != value)
                {
                    typeURI = value;
                    NotifyPropertyChanged("TypeURI");
                }
            }
        }

        private bool canAddToGraphOrMap;
        public bool CanAddToGraphOrMap
        {
            get { return canAddToGraphOrMap; }
            set
            {
                if (canAddToGraphOrMap != value)
                {
                    canAddToGraphOrMap = value;
                    NotifyPropertyChanged("CanAddToGraphOrMap");
                }
            }
        }

        #endregion

        #region رخدادها
        public class SelectedPreviewStatisticsEventArgs
        {
            public SelectedPreviewStatisticsEventArgs(List<PreviewStatistic> previewStatistics)
            {
                if (previewStatistics == null)
                    throw new ArgumentNullException(nameof(previewStatistics));

                PreviewStatistics = previewStatistics;
            }

            public List<PreviewStatistic> PreviewStatistics
            {
                get;
                private set;
            }
        }

        public class SelectedPreviewStatisticEventArgs
        {
            public SelectedPreviewStatisticEventArgs(PreviewStatistic previewStatistic)
            {
                if (previewStatistic == null)
                    throw new ArgumentNullException(nameof(previewStatistic));


                PreviewStatistic = previewStatistic;
            }

            public PreviewStatistic PreviewStatistic
            {
                get;
                private set;
            }
        }

        public event EventHandler<SelectedPreviewStatisticsEventArgs> DrillDownRequested;
        protected virtual void OnDrillDownRequested(List<PreviewStatistic> selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException("selectedPreviewStatistic");

            DrillDownRequested?.Invoke(this, new SelectedPreviewStatisticsEventArgs(selectedPreviewStatistic));
        }

        public event EventHandler<SelectedPreviewStatisticEventArgs> HistogramThisValueRequested;
        protected virtual void OnHistogramThisValueRequested(PreviewStatistic selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException(nameof(selectedPreviewStatistic));

            HistogramThisValueRequested?.Invoke(this, new SelectedPreviewStatisticEventArgs(selectedPreviewStatistic));
        }

        public event EventHandler<SelectedPreviewStatisticEventArgs> BarChartThisValueRequested;
        protected virtual void OnBarChartThisValueRequested(PreviewStatistic selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException(nameof(selectedPreviewStatistic));

            BarChartThisValueRequested?.Invoke(this, new SelectedPreviewStatisticEventArgs(selectedPreviewStatistic));
        }

        public event EventHandler<SelectedPreviewStatisticEventArgs> PropertyValueFilterRequested;
        protected virtual void OnPropertyValueFilterRequested(PreviewStatistic selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException(nameof(selectedPreviewStatistic));

            PropertyValueFilterRequested?.Invoke(this, new SelectedPreviewStatisticEventArgs(selectedPreviewStatistic));
        }

        public event EventHandler<SelectedPreviewStatisticsEventArgs> ShowRetrievedObjectsOnGraphRequested;
        protected virtual void OnShowRetrievedObjectsOnGraphRequested(List<PreviewStatistic> selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException("selectedPreviewStatistic");

            ShowRetrievedObjectsOnGraphRequested?.Invoke(this, new SelectedPreviewStatisticsEventArgs(selectedPreviewStatistic));
        }

        public event EventHandler<SelectedPreviewStatisticsEventArgs> ShowRetrievedObjectsOnMapRequested;
        protected virtual void OnShowRetrievedObjectsOnMapRequested(List<PreviewStatistic> selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException("selectedPreviewStatistic");

            ShowRetrievedObjectsOnMapRequested?.Invoke(this, new SelectedPreviewStatisticsEventArgs(selectedPreviewStatistic));
        }       

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event EventHandler<Utility.TakeSnapshotEventArgs> SnapshotTaken;

        private void OnSnapshotTaken(PngBitmapEncoder image, string defaultFileName)
        {
            SnapshotTaken?.Invoke(this, new Utility.TakeSnapshotEventArgs(image, defaultFileName));
        }

        #endregion

        public HistogramControl()
        {
            InitializeComponent();
            Init();
        }

        #region توابع
        private void Init()
        {
            DataContext = this;
            PreviewStatisticsToShowHistogram = new PreviewStatistics();
        }

        public void ShowPreviewStatistics(PreviewStatistics previewStatistics)
        {
            if (previewStatistics == null)
            {
                throw new ArgumentNullException(nameof(previewStatistics));
            }

            keyValueToPreviewStatisticMapping = new Dictionary<HistogramKeyValuePair, PreviewStatistic>();

            PreviewStatisticsToShowHistogram = previewStatistics;

            FillMainHistogram(previewStatistics);
        }

        private void FillMainHistogram(PreviewStatistics previewStatistics)
        {
            MainHistogram.Icon = previewStatistics.Icon;
            MainHistogram.Title = previewStatistics.Title;
            MainHistogram.Description = string.Format("{0} Objects", previewStatistics.ObjectsCount);

            var items = new ObservableCollection<HistogramItem>();

            var superCategories = previewStatistics.Content.GroupBy(x => x.SuperCategory);

            foreach (var superCategory in superCategories)
            {
                var level1 = new HistogramItem
                {
                    Title = superCategory.Key
                };

                var categories = superCategory.GroupBy(x => x.Category);

                foreach (var category in categories)
                {
                    var level2 = new HistogramItem
                    {
                        Title = category.Key
                    };

                    foreach (var statistic in category)
                    {
                        var level3 = new HistogramItem
                        {
                            Title = statistic.Title,
                            Icon = statistic.Icon,
                            Value = statistic.Count,
                            RelatedElement = statistic
                        };

                        level2.Items.Add(level3);
                    }

                    level1.Items.Add(level2);
                }

                items.Add(level1);
            }

            MainHistogram.Items = items;
        }

        private long GetSelectedKeyValuePairCount(List<PreviewStatistic> previewStatistic)
        {
            long count = 0;

            foreach (var statistic in previewStatistic)
            {
                count += statistic.Count;
            }

            return count;
        }

        private List<PreviewStatistic> GetSelectedObjectsPreviewStatistics()
        {
            List<PreviewStatistic> selectedPreviewStatistic = MainHistogram.SelectedItems
                .Select(selectedItem => (PreviewStatistic)selectedItem.RelatedElement).ToList();

            return selectedPreviewStatistic.Where(x => !x.Category.ToLower().Contains("property")).ToList();
        }

        private List<PreviewStatistic> GetSelectedPropertiesPreviewStatistics()
        {
            List<PreviewStatistic> selectedPreviewStatistic = MainHistogram.SelectedItems
                .Select(selectedItem => (PreviewStatistic)selectedItem.RelatedElement).ToList();

            return selectedPreviewStatistic.Where(x => x.Category.ToLower().Contains("property")).ToList();
        }

        public void Reset()
        {
            MainHistogram.Reset();
        }

        #endregion

        #region رخدادگردانها
        private void DrillDownOnObjectsOfThisTypeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MainHistogram.SelectedItems != null)
            {
                if (MainHistogram.SelectedItems.Count == 0)
                    return;

                OnDrillDownRequested(GetSelectedObjectsPreviewStatistics());
            }
        }

        private void DrillDownOnObjectsWithSelectedPropertyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MainHistogram.SelectedItems.Count == 0)
                return;

            OnDrillDownRequested(GetSelectedPropertiesPreviewStatistics());
        }

        private void DrillDownOnThisTypesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MainHistogram.SelectedItems.Count == 0)
                return;

            var allStatistics = GetSelectedObjectsPreviewStatistics();
            allStatistics.AddRange(GetSelectedPropertiesPreviewStatistics());

            OnDrillDownRequested(allStatistics);
        }

        private void AddCurrentSelectionToGraphMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnShowRetrievedObjectsOnGraphRequested(GetSelectedObjectsPreviewStatistics());
        }

        private void AddCurrentSelectionToMapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnShowRetrievedObjectsOnMapRequested(GetSelectedObjectsPreviewStatistics());
        }

        private void AddCurrentSelectionToGraphForPropertyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnShowRetrievedObjectsOnGraphRequested(GetSelectedPropertiesPreviewStatistics());
        }

        private void AddCurrentSelectionToMapForPropertyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OnShowRetrievedObjectsOnMapRequested(GetSelectedPropertiesPreviewStatistics());
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateSelectedSuperCategoryType();
            e.Handled = true;
        }

        private void UpdateSelectedSuperCategoryType()
        {
            if (MainHistogram.SelectedItems != null)
            {
                if (MainHistogram.SelectedItems.Count == 0)
                {
                    SelectedSuperCategoryType = HistogramSuperCategoryType.None;
                }
                else
                {
                    int objectCount = 0;
                    int propertiesCount = 0;

                    List<PreviewStatistic> selectedPreviewStatistic = MainHistogram.SelectedItems
                        .Select(selectedItem => (PreviewStatistic)selectedItem.RelatedElement).ToList();

                    foreach (var item in selectedPreviewStatistic)
                    {
                        if (item.Category.Contains("Property"))
                        {
                            propertiesCount++;
                        }
                        else
                        {
                            objectCount++;
                        }
                    }

                    if (objectCount == 0 && propertiesCount != 0)
                    {
                        if (propertiesCount > 1)
                        {
                            SelectedSuperCategoryType = HistogramSuperCategoryType.MultipleProperty;
                        }
                        else
                        {
                            SelectedSuperCategoryType = HistogramSuperCategoryType.SingleProperty;
                            TypeURI = selectedPreviewStatistic.First(x => x.Category.Contains("Property")).TypeURI;
                        }
                    }
                    else if (objectCount != 0 && propertiesCount == 0)
                    {
                        SelectedSuperCategoryType = HistogramSuperCategoryType.Object;
                    }
                    else if (objectCount != 0 && propertiesCount != 0)
                    {
                        SelectedSuperCategoryType = HistogramSuperCategoryType.Merge;
                    }

                    long selectedCount = GetSelectedKeyValuePairCount(selectedPreviewStatistic);

                    CanAddToGraphOrMap = selectedCount <= PassObjectsCountLimit;
                }
            }
        }

        private void AddCurrentSelectionToGraphForMergeStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var allSelectedStatistics = GetSelectedObjectsPreviewStatistics();
            allSelectedStatistics.AddRange(GetSelectedPropertiesPreviewStatistics());

            OnShowRetrievedObjectsOnGraphRequested(allSelectedStatistics);
        }

        private void AddCurrentSelectionToMapForMergeStateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var allSelectedStatistics = GetSelectedObjectsPreviewStatistics();
            allSelectedStatistics.AddRange(GetSelectedPropertiesPreviewStatistics());

            OnShowRetrievedObjectsOnMapRequested(allSelectedStatistics);
        }

        private void HistogramThisPropertyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var propertiesPreviewStatistics = GetSelectedPropertiesPreviewStatistics();

            if (propertiesPreviewStatistics.Count == 1)
            {
                OnHistogramThisValueRequested(propertiesPreviewStatistics.First());
            }
        }

        private void BarChartThisPropertyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var propertiesPreviewStatistics = GetSelectedPropertiesPreviewStatistics();

            if (propertiesPreviewStatistics.Count == 1)
            {
                OnBarChartThisValueRequested(propertiesPreviewStatistics.First());
            }
        }

        private void PropertyMatchedValueFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var propertiesPreviewStatistics = GetSelectedPropertiesPreviewStatistics();

            if (propertiesPreviewStatistics.Count == 1)
            {
                OnPropertyValueFilterRequested(propertiesPreviewStatistics.First());
            }
        }

        private void MainHistogram_OnSnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e.Snapshot, e.DefaultFileName);
        }

        #endregion       
    }

    public enum HistogramSuperCategoryType
    {
        Object,
        MultipleProperty,
        SingleProperty,
        Merge,
        None
    }
}
