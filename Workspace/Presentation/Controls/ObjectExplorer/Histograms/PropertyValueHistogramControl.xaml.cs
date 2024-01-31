using GPAS.Histogram;
using GPAS.Workspace.Model;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Histograms
{
    public partial class PropertyValueHistogramControl
    {
        #region Event

        public class ChangeSelectionEventArgs
        {
            public ChangeSelectionEventArgs(List<PropertyValueStatistic> propertyValueStatistic)
            {
                PropertyValueStatistic = propertyValueStatistic ??
                                         throw new ArgumentNullException(nameof(propertyValueStatistic));
            }

            public List<PropertyValueStatistic> PropertyValueStatistic { get; }
        }

        public event EventHandler<ChangeSelectionEventArgs> DrillDownRequested;

        protected virtual void OnDrillDownRequested(List<PropertyValueStatistic> propertyValueStatistic)
        {
            if (propertyValueStatistic == null) throw new ArgumentNullException(nameof(propertyValueStatistic));
            DrillDownRequested?.Invoke(this, new ChangeSelectionEventArgs(propertyValueStatistic));
        }

        public event EventHandler<ChangeSelectionEventArgs> ShowRetrievedObjectsOnGraphRequested;

        protected virtual void OnShowRetrievedObjectsOnGraphRequested(
            List<PropertyValueStatistic> propertyValueStatistics)
        {
            if (propertyValueStatistics == null) throw new ArgumentNullException(nameof(propertyValueStatistics));
            ShowRetrievedObjectsOnGraphRequested?.Invoke(this, new ChangeSelectionEventArgs(propertyValueStatistics));
        }

        public event EventHandler<ChangeSelectionEventArgs> ShowRetrievedObjectsOnMapRequested;

        protected virtual void OnShowRetrievedObjectsOnMapRequested(
            List<PropertyValueStatistic> propertyValueStatistics)
        {
            if (propertyValueStatistics == null) throw new ArgumentNullException(nameof(propertyValueStatistics));
            ShowRetrievedObjectsOnMapRequested?.Invoke(this, new ChangeSelectionEventArgs(propertyValueStatistics));
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }

        #endregion

        #region Dependencies

        private readonly int defaultLoadLimit = 50;
        private int offset;
        public static readonly int PassObjectsCountLimit = int.Parse(ConfigurationManager.AppSettings["ObjectExplorerPassObjectsCountLimit"]);

        private ObjectSetBase RelatedObjectSet { get; set; }
        private PropertyValueCategory TotalPropertyValueCategory { get; set; }
        private PreviewStatistic ExploringPreviewStatistic { get; set; }

        protected Visibility UnloadableItemsTextBlock
        {
            get => (Visibility)GetValue(UnloadableItemsTextBlockProperty);
            set => SetValue(UnloadableItemsTextBlockProperty, value);
        }

        public static readonly DependencyProperty UnloadableItemsTextBlockProperty =
            DependencyProperty.Register(nameof(UnloadableItemsTextBlock), typeof(Visibility),
                typeof(PropertyValueHistogramControl), new PropertyMetadata(Visibility.Collapsed));

        protected bool FilterTextIsValid
        {
            get => (bool)GetValue(FilterTextIsValidProperty);
            set => SetValue(FilterTextIsValidProperty, value);
        }

        public static readonly DependencyProperty FilterTextIsValidProperty = DependencyProperty.Register(
            nameof(FilterTextIsValid), typeof(bool), typeof(PropertyValueHistogramControl), new PropertyMetadata(false));

        protected bool CanAddToGraphOrMap
        {
            get => (bool)GetValue(CanAddToGraphOrMapProperty);
            set => SetValue(CanAddToGraphOrMapProperty, value);
        }

        public static readonly DependencyProperty CanAddToGraphOrMapProperty = DependencyProperty.Register(
            nameof(CanAddToGraphOrMap), typeof(bool), typeof(PropertyValueHistogramControl),
            new PropertyMetadata(false));

        protected FilterMatchingType FilterMatchingType
        {
            get => (FilterMatchingType)GetValue(FilterMatchingTypeProperty);
            set => SetValue(FilterMatchingTypeProperty, value);
        }

        public static readonly DependencyProperty FilterMatchingTypeProperty = DependencyProperty.Register(
            nameof(FilterMatchingType), typeof(FilterMatchingType), typeof(PropertyValueHistogramControl),
            new PropertyMetadata(FilterMatchingType.Matching));

        protected FilterType FilterType
        {
            get => (FilterType)GetValue(FilterTypeProperty);
            set => SetValue(FilterTypeProperty, value);
        }

        public static readonly DependencyProperty FilterTypeProperty = DependencyProperty.Register(nameof(FilterType),
            typeof(FilterType), typeof(PropertyValueHistogramControl),
            new PropertyMetadata(FilterType.String, OnFilterTypeChanged));

        protected bool CanDrillDown
        {
            get { return (bool)GetValue(CanDrillDownProperty); }
            set { SetValue(CanDrillDownProperty, value); }
        }

        protected static readonly DependencyProperty CanDrillDownProperty = DependencyProperty.Register(nameof(CanDrillDown),
            typeof(bool), typeof(PropertyValueHistogramControl), new PropertyMetadata(false));

        #endregion

        #region Methods

        public PropertyValueHistogramControl()
        {
            InitializeComponent();
        }

        private static void OnFilterTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyValueHistogramControl)d).OnFilterTypeChanged(e);
        }

        private void OnFilterTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            SetFilterValidation();
        }

        public async Task ShowPropertyValueCategory(ObjectSetBase objectSet, PreviewStatistic exploringPreviewStatistic, int inputOffset)
        {
            if (inputOffset < 0)
            {
                throw new ArgumentException(nameof(inputOffset));
            }

            RelatedObjectSet = objectSet ?? throw new ArgumentNullException(nameof(objectSet));
            ExploringPreviewStatistic = exploringPreviewStatistic ?? throw new ArgumentNullException(nameof(exploringPreviewStatistic));
            offset = inputOffset;

            TotalPropertyValueCategory = await LoadPropertyValueCategoryFromRemoteServer(RelatedObjectSet, ExploringPreviewStatistic, offset);
            ShowNewPropertyValueCategory(TotalPropertyValueCategory);

            objectSet.AddVisualizationPanelTool(new PropertyValueHistogramTool
            {
                IsActiveTool = true,
                PropertyValueCategory = TotalPropertyValueCategory,
                ShowingCount = 10,
                VisualPanelToolType = VisualizationPanelToolType.PropertyValueHistogram,
                ExploringPreviewStatistic = ExploringPreviewStatistic
            });
        }

        public void ShowPropertyValueHistogramTool(ObjectSetBase objectSet, PropertyValueHistogramTool propertyValueHistogramTool)
        {
            if (propertyValueHistogramTool == null)
            {
                throw new ArgumentNullException(nameof(propertyValueHistogramTool));
            }

            RelatedObjectSet = objectSet ?? throw new ArgumentNullException(nameof(objectSet));
            TotalPropertyValueCategory = propertyValueHistogramTool.PropertyValueCategory;
            ShowNewPropertyValueCategory(propertyValueHistogramTool.PropertyValueCategory);
        }

        private void FillHistogram(PropertyValueCategory propertyValueCategory)
        {
            var items = new ObservableCollection<HistogramItem>();
            var level1 = new HistogramItem
            {
                Title = propertyValueCategory.Title,
                CanShowAll = false,
                TotaleItemsCount = propertyValueCategory.TotalValuesCount
            };
            if (propertyValueCategory.LoadedValuesCount < propertyValueCategory.TotalValuesCount)
            {
                level1.HasUnloadedItems = true;
            }

            var propertyIcon = new BitmapImage(Logic.OntologyIconProvider.GetPropertyTypeIconPath(propertyValueCategory.TypeUri));

            foreach (var superCategory in propertyValueCategory.LoadedValues)
            {
                var level2 = new HistogramItem
                {
                    Title = superCategory.PropertyValue,
                    Icon = propertyIcon,
                    Value = superCategory.Count,
                    RelatedElement = superCategory
                };

                level1.Items.Add(level2);
            }

            items.Add(level1);
            MainHistogram.Items = items;
        }

        private void UpdateHistogram(PropertyValueCategory propertyValueCategory)
        {
            var items = new ObservableCollection<HistogramItem>();
            var oldItems = MainHistogram.Items.ToList();
            var level1 = oldItems[0];
            if (propertyValueCategory.LoadedValuesCount < propertyValueCategory.TotalValuesCount)
            {
                level1.HasUnloadedItems = true;
            }

            var propertyIcon = new BitmapImage(Logic.OntologyIconProvider.GetPropertyTypeIconPath(propertyValueCategory.TypeUri));

            foreach (var superCategory in propertyValueCategory.LoadedValues)
            {
                var level3 = new HistogramItem
                {
                    Title = superCategory.PropertyValue,
                    Icon = propertyIcon,
                    Value = superCategory.Count,
                    RelatedElement = superCategory
                };
                level1.Items.Add(level3);
            }

            items.Add(level1);
            MainHistogram.Items = items;
        }

        private void ShowNewPropertyValueCategory(PropertyValueCategory propertyValueCategory)
        {
            if (propertyValueCategory == null)
            {
                throw new ArgumentNullException(nameof(propertyValueCategory));
            }

            ShowPropertyValueCategory(propertyValueCategory);
            ClearFilter();
        }

        private void ShowPropertyValueCategory(PropertyValueCategory propertyValueCategory)
        {
            if (propertyValueCategory == null)
            {
                throw new ArgumentNullException(nameof(propertyValueCategory));
            }

            UnloadableItemsTextBlock = propertyValueCategory.HasUnloadableValues ? Visibility.Visible : Visibility.Collapsed;
            UnableLoadValuesTextBlock.Text = $"Some properties occur less than {propertyValueCategory.MinimumLoadableValueCount} times";

            if (propertyValueCategory.LoadedValuesCount != 0)
            {
                FillHistogram(propertyValueCategory);
            }
            else
            {
                MainHistogram.ClearItems();
            }
        }

        private async Task<PropertyValueCategory> LoadPropertyValueCategoryFromRemoteServer(ObjectSetBase objectSet,
            PreviewStatistic exploringPreviewStatistic, int offset)
        {
            ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
            PropertyValueCategory propertyValueCategory =
                await objectExplorerModel.RetrievePropertyValueStatistics(objectSet, exploringPreviewStatistic,
                    defaultLoadLimit, offset);

            return propertyValueCategory;
        }

        private void BtnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilterOnRetrievedPropertyValueCategory();
            BtnClearFilter.IsEnabled = true;
        }

        private void ApplyFilterOnRetrievedPropertyValueCategory()
        {
            switch (FilterMatchingType)
            {
                case FilterMatchingType.Matching:
                    switch (FilterType)
                    {
                        case FilterType.String:
                            ApplyMatchingStringOnRetrievedPropertyValueCategory(FilterByValueTextBox.Text);
                            break;
                        case FilterType.Regex:
                            ApplyMatchingRegexOnRetrievedPropertyValueCategory(FilterByValueTextBox.Text);
                            break;
                    }

                    break;
                case FilterMatchingType.NotMatching:
                    switch (FilterType)
                    {
                        case FilterType.String:
                            ApplyNotMatchingStringOnRetrievedPropertyValueCategory(FilterByValueTextBox.Text);
                            break;
                        case FilterType.Regex:
                            ApplyNotMatchingRegexOnRetrievedPropertyValueCategory(FilterByValueTextBox.Text);
                            break;
                    }

                    break;
            }
        }

        private void ApplyMatchingRegexOnRetrievedPropertyValueCategory(string filterValue)
        {
            PropertyValueCategory newPropertyValueCategory = new PropertyValueCategory
            {
                Title = TotalPropertyValueCategory.Title,
                TypeUri = TotalPropertyValueCategory.TypeUri,
                HasUnloadableValues = TotalPropertyValueCategory.HasUnloadableValues,
                LoadedValuesCount = TotalPropertyValueCategory.LoadedValuesCount,
                MinimumLoadableValueCount = TotalPropertyValueCategory.MinimumLoadableValueCount,
                TotalValuesCount = TotalPropertyValueCategory.TotalValuesCount,
            };
            RegexOptions options = RegexOptions.Compiled;
            Regex regex = new Regex(filterValue, options);
            List<PropertyValueStatistic> newLoadedValues = new List<PropertyValueStatistic>();
            foreach (var currentOldValue in TotalPropertyValueCategory.LoadedValues)
            {
                MatchCollection regexResult = regex.Matches(currentOldValue.PropertyValue);
                if (regexResult.Count != 0)
                {
                    newLoadedValues.Add(currentOldValue);
                }
            }

            newPropertyValueCategory.LoadedValues = newLoadedValues;
            ShowPropertyValueCategory(newPropertyValueCategory);
        }

        private void ApplyMatchingStringOnRetrievedPropertyValueCategory(string filterValue)
        {
            PropertyValueCategory newPropertyValueCategory = new PropertyValueCategory
            {
                Title = TotalPropertyValueCategory.Title,
                TypeUri = TotalPropertyValueCategory.TypeUri,
                HasUnloadableValues = TotalPropertyValueCategory.HasUnloadableValues,
                LoadedValuesCount = TotalPropertyValueCategory.LoadedValuesCount,
                MinimumLoadableValueCount = TotalPropertyValueCategory.MinimumLoadableValueCount,
                TotalValuesCount = TotalPropertyValueCategory.TotalValuesCount,
                LoadedValues = TotalPropertyValueCategory.LoadedValues
                    .Where(lv => lv.PropertyValue.Equals(filterValue)).ToList(),
            };

            ShowPropertyValueCategory(newPropertyValueCategory);
        }

        private void ApplyNotMatchingRegexOnRetrievedPropertyValueCategory(string filterValue)
        {
            PropertyValueCategory newPropertyValueCategory = new PropertyValueCategory
            {
                Title = TotalPropertyValueCategory.Title,
                TypeUri = TotalPropertyValueCategory.TypeUri,
                HasUnloadableValues = TotalPropertyValueCategory.HasUnloadableValues,
                LoadedValuesCount = TotalPropertyValueCategory.LoadedValuesCount,
                MinimumLoadableValueCount = TotalPropertyValueCategory.MinimumLoadableValueCount,
                TotalValuesCount = TotalPropertyValueCategory.TotalValuesCount,
            };

            RegexOptions options = RegexOptions.Compiled;
            Regex regex = new Regex(filterValue, options);
            List<PropertyValueStatistic> newLoadedValues = new List<PropertyValueStatistic>();

            foreach (var currentOldValue in TotalPropertyValueCategory.LoadedValues)
            {
                MatchCollection regexResult = regex.Matches(currentOldValue.PropertyValue);
                if (regexResult.Count == 0)
                {
                    newLoadedValues.Add(currentOldValue);
                }
            }

            newPropertyValueCategory.LoadedValues = newLoadedValues;
            ShowPropertyValueCategory(newPropertyValueCategory);
        }

        private void ApplyNotMatchingStringOnRetrievedPropertyValueCategory(string filterValue)
        {
            PropertyValueCategory newPropertyValueCategory = new PropertyValueCategory
            {
                Title = TotalPropertyValueCategory.Title,
                TypeUri = TotalPropertyValueCategory.TypeUri,
                HasUnloadableValues = TotalPropertyValueCategory.HasUnloadableValues,
                LoadedValuesCount = TotalPropertyValueCategory.LoadedValuesCount,
                MinimumLoadableValueCount = TotalPropertyValueCategory.MinimumLoadableValueCount,
                TotalValuesCount = TotalPropertyValueCategory.TotalValuesCount,
                LoadedValues = TotalPropertyValueCategory.LoadedValues
                    .Where(lv => !lv.PropertyValue.Equals(filterValue)).ToList(),
            };

            ShowPropertyValueCategory(newPropertyValueCategory);
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            ShowNewPropertyValueCategory(TotalPropertyValueCategory);
            ClearFilter();
        }

        private void ClearFilter()
        {
            FilterByValueTextBox.Clear();
            BtnClearFilter.IsEnabled = false;
            StringRadioButton.IsChecked = true;
            MatchingRadioButton.IsChecked = true;
        }

        public bool IsCorrectRegexPattern(string filterByValueTextBlock)
        {
            if (string.IsNullOrWhiteSpace(filterByValueTextBlock)) return false;
            try
            {
                RegexOptions options = RegexOptions.Compiled;
                Regex regex = new Regex(filterByValueTextBlock, options);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetFilterValidation()
        {
            if (string.IsNullOrEmpty(FilterByValueTextBox.Text))
            {
                FilterTextIsValid = false;
            }
            else
            {
                FilterTextIsValid = FilterType != FilterType.Regex || IsCorrectRegexPattern(FilterByValueTextBox.Text);
            }
        }

        private void FilterByValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilterValidation();
        }

        private List<PropertyValueStatistic> GetSelectedItemsFromHistogram()
        {
            List<PropertyValueStatistic> selectedPreviewStatistic = new List<PropertyValueStatistic>();
            if (MainHistogram.SelectedItems != null && MainHistogram.SelectedItems.Count != 0)
            {
                selectedPreviewStatistic = MainHistogram.SelectedItems
                    .Select(selectedItem => (PropertyValueStatistic)selectedItem.RelatedElement).ToList();
            }

            return selectedPreviewStatistic;
        }

        private void DrillDown()
        {
            var items = GetSelectedItemsFromHistogram();

            if (items.Count == 0)
                return;

            OnDrillDownRequested(items);
        }

        private void DrillDownOnCurrentSelectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DrillDown();
        }

        private void AddCurrentSelectionToMapMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var items = GetSelectedItemsFromHistogram();
            if (items.Count == 0) return;
            OnShowRetrievedObjectsOnMapRequested(items);
        }

        private void AddCurrentSelectionToGraphMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var items = GetSelectedItemsFromHistogram();
            if (items.Count == 0) return;
            OnShowRetrievedObjectsOnGraphRequested(items);
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }

        private void mainGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MainHistogram.SelectedItems == null)
            {
                MainContextMenu.Visibility = Visibility.Collapsed;
                return;
            }

            var parentContent = FindParent<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (parentContent != null && ((HistogramItem)parentContent.DataContext).IsSelected)
            {
                MainContextMenu.IsOpen = false;
                MainContextMenu.IsOpen = true;
                MainContextMenu.Visibility = Visibility.Collapsed;
                MainContextMenu.Visibility = Visibility.Visible;
            }
            else
            {
                MainContextMenu.IsOpen = false;
                MainContextMenu.Visibility = Visibility.Collapsed;
            }

            CanAddToGraphOrMap = MainHistogram.SelectedItems.Sum(selectedItem =>
                ((PropertyValueStatistic)selectedItem.RelatedElement).Count) > PassObjectsCountLimit
                ? false : true;
        }

        private void MainHistogram_ShowMoreClicked(object sender, ChangeChildrenToShowCountEventArgs e)
        {
            ShowMoreItem(e.SelectedItem);
        }

        private async void ShowMoreItem(HistogramItem item)
        {
            offset++;
            try
            {
                WaitingControl.TaskIncrement();

                PropertyValueCategory propertyValueCategory =
                    await LoadPropertyValueCategoryFromRemoteServer(RelatedObjectSet, ExploringPreviewStatistic, offset * defaultLoadLimit);

                UpdateHistogram(propertyValueCategory);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }
        }

        private void mainGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainContextMenu.IsOpen = false;
        }

        private void DrillDownButton_Click(object sender, RoutedEventArgs e)
        {
            DrillDown();
        }

        private void MainHistogram_OnSnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        private void MainHistogram_SelectedItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CanDrillDown = MainHistogram.SelectedItems?.Count > 0;
        }

        #endregion
    }
}
