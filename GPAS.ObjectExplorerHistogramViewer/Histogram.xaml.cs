using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace GPAS.ObjectExplorerHistogramViewer
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl, INotifyPropertyChanged
    {
        #region متغیرهای سراسری
        public readonly int minimumShowingHistogramKeyValuePairs = 10;

        private bool showHeader;
        public bool ShowHeader
        {
            get { return this.showHeader; }
            set
            {
                if (this.showHeader != value)
                {
                    this.showHeader = value;
                    this.NotifyPropertyChanged("ShowHeader");
                }
            }
        }
        private BitmapImage icon;
        public BitmapImage Icon
        {
            get { return this.icon; }
            set
            {
                if (this.icon != value)
                {
                    this.icon = value;
                    this.NotifyPropertyChanged("Icon");
                }
            }
        }

        private string title;
        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.NotifyPropertyChanged("Title");
                }
            }
        }

        private string itemCount;
        public string ItemCount
        {
            get { return this.itemCount; }
            set
            {
                if (this.itemCount != value)
                {
                    this.itemCount = value;
                    this.NotifyPropertyChanged("ItemCount");
                }
            }
        }

        public ObservableCollection<HistogramSuperCategory> HistogramSuperCategories { get; set; }
        #endregion

        #region رخدادها
        public class ChangeHistogramSubCategorySizeEventArgs
        {
            public ChangeHistogramSubCategorySizeEventArgs(HistogramSubCategory histogramSubCategoryToChange)
            {
                if (histogramSubCategoryToChange == null)
                    throw new ArgumentNullException(nameof(histogramSubCategoryToChange));

                HistogramSubCategoryToChange = histogramSubCategoryToChange;
            }

            public HistogramSubCategory HistogramSubCategoryToChange
            {
                get;
                private set;
            }
        }

        public event EventHandler<ChangeHistogramSubCategorySizeEventArgs> ShowMoreButtonClicked;
        protected virtual void OnShowMoreButtonClicked(HistogramSubCategory histogramSubCategoryToChange)
        {
            if (histogramSubCategoryToChange == null)
                throw new ArgumentNullException(nameof(histogramSubCategoryToChange));

            ShowMoreButtonClicked?.Invoke(this, new ChangeHistogramSubCategorySizeEventArgs(histogramSubCategoryToChange));
        }

        public event EventHandler<ChangeHistogramSubCategorySizeEventArgs> ShowAllButtonClicked;
        protected virtual void OnShowAllButtonClicked(HistogramSubCategory histogramSubCategoryToChange)
        {
            if (histogramSubCategoryToChange == null)
                throw new ArgumentNullException(nameof(histogramSubCategoryToChange));

            ShowAllButtonClicked?.Invoke(this, new ChangeHistogramSubCategorySizeEventArgs(histogramSubCategoryToChange));
        }

        public event EventHandler<ChangeHistogramSubCategorySizeEventArgs> ShowFewerButtonClicked;

        public event EventHandler<EventArgs> SelectionChanged;
        protected virtual void OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        protected virtual void OnShowFewerButtonClicked(HistogramSubCategory histogramSubCategoryToChange)
        {
            if (histogramSubCategoryToChange == null)
                throw new ArgumentNullException(nameof(histogramSubCategoryToChange));

            ShowFewerButtonClicked?.Invoke(this, new ChangeHistogramSubCategorySizeEventArgs(histogramSubCategoryToChange));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event EventHandler<SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }
        #endregion

        public Histogram()
        {
            InitializeComponent();
            Init();
        }

        #region توابع
        private void Init()
        {
            HistogramSuperCategories = new ObservableCollection<HistogramSuperCategory>();
            DataContext = this;
        }

        public void ShowHistogramCategories(BitmapImage icon, string title, string histogeramItemCounts, ObservableCollection<HistogramSuperCategory> histogramSuperCategoriesToShow, bool showHeader = true)
        {
            HistogramSuperCategoryItemsControl.ItemsSource = null;
            ShowHeader = showHeader;
            HistogramSuperCategories = histogramSuperCategoriesToShow;
            Icon = icon;
            Title = title;
            ItemCount = histogeramItemCounts;

            if (histogramSuperCategoriesToShow.Count == 0)
            {
                HistogramSuperCategories.Clear();
                HistogramSuperCategoryItemsControl.ItemsSource = HistogramSuperCategories;
                NothingToPreviewPromptLabelForSearchResults.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                NothingToPreviewPromptLabelForSearchResults.Visibility = Visibility.Collapsed;
                HistogramSuperCategoryItemsControl.ItemsSource = HistogramSuperCategories;
            }
        }

        public void ClearAllHistogramKeyValuePair(HistogramSuperCategory histogramSuperCategory)
        {
            foreach (HistogramSubCategory currentSubCategory in histogramSuperCategory.HistogramSubCategories)
            {
                foreach (HistogramKeyValuePair currentKeyValuePair in currentSubCategory.ShowingHistogramKeyValuePairs)
                {
                    currentKeyValuePair.IsSelected = false;
                }
            }

            OnSelectionChanged();
        }

        private void ChangeHistogramSelectionStateForLeftMouseButton(ref HistogramKeyValuePair selectedHistogramKeyValuePair)
        {
            if (((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
            {
                if (selectedHistogramKeyValuePair.IsSelected)
                {
                    selectedHistogramKeyValuePair.IsSelected = false;
                }
                else
                {
                    selectedHistogramKeyValuePair.IsSelected = true;
                    selectedHistogramKeyValuePair.relatedHistogramSubCategory.LastSelectedKeyValuePair = selectedHistogramKeyValuePair;
                }
            }
            else if (((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
            {
                if (selectedHistogramKeyValuePair.relatedHistogramSubCategory.LastSelectedKeyValuePair != null)
                {
                    int selectedKeyValuePairIndex = selectedHistogramKeyValuePair.relatedHistogramSubCategory.ShowingHistogramKeyValuePairs.IndexOf(selectedHistogramKeyValuePair);
                    int lastSelectedKeyValuePair = selectedHistogramKeyValuePair.relatedHistogramSubCategory.ShowingHistogramKeyValuePairs.IndexOf(selectedHistogramKeyValuePair.relatedHistogramSubCategory.LastSelectedKeyValuePair);

                    if (selectedKeyValuePairIndex < lastSelectedKeyValuePair)
                    {
                        foreach (HistogramKeyValuePair currentKeyValue in selectedHistogramKeyValuePair.relatedHistogramSubCategory.ShowingHistogramKeyValuePairs)
                        {
                            currentKeyValue.IsSelected = false;
                        }

                        for (int i = selectedKeyValuePairIndex; i < lastSelectedKeyValuePair + 1; i++)
                        {
                            selectedHistogramKeyValuePair.relatedHistogramSubCategory.ShowingHistogramKeyValuePairs.ElementAt(i).IsSelected = true;
                        }
                    }
                    else if (selectedKeyValuePairIndex > lastSelectedKeyValuePair)
                    {
                        foreach (HistogramKeyValuePair currentKeyValue in selectedHistogramKeyValuePair.relatedHistogramSubCategory.ShowingHistogramKeyValuePairs)
                        {
                            currentKeyValue.IsSelected = false;
                        }

                        for (int i = lastSelectedKeyValuePair; i < selectedKeyValuePairIndex + 1; i++)
                        {
                            selectedHistogramKeyValuePair.relatedHistogramSubCategory.ShowingHistogramKeyValuePairs.ElementAt(i).IsSelected = true;
                        }
                    }
                }
            }
            else
            {
                if (HistogramSuperCategories != null)
                {
                    foreach (HistogramSuperCategory currentSuperCategory in HistogramSuperCategories)
                    {
                        foreach (HistogramSubCategory currentSubCategory in currentSuperCategory.HistogramSubCategories)
                        {
                            foreach (HistogramKeyValuePair currentKeyValuePair in currentSubCategory.ShowingHistogramKeyValuePairs)
                            {
                                currentKeyValuePair.IsSelected = false;
                            }
                        }
                    }
                    selectedHistogramKeyValuePair.IsSelected = true;
                    selectedHistogramKeyValuePair.relatedHistogramSubCategory.LastSelectedKeyValuePair = selectedHistogramKeyValuePair;
                }
            }

            OnSelectionChanged();
        }

        private int GetHistogramSelectedItemCounts()
        {
            int selectedCount = 0;
            if (HistogramSuperCategories != null)
            {
                foreach (HistogramSuperCategory currentSuperCategory in HistogramSuperCategories)
                {
                    foreach (HistogramSubCategory currentSubCategory in currentSuperCategory.HistogramSubCategories)
                    {
                        foreach (HistogramKeyValuePair currentKeyValuePair in currentSubCategory.ShowingHistogramKeyValuePairs)
                        {
                            if (currentKeyValuePair.IsSelected)
                            {
                                selectedCount++;
                            }
                        }
                    }
                }
            }

            return selectedCount;
        }

        private void ChangeHistogramSelectionStateForRightMouseButton(ref HistogramKeyValuePair selectedHistogramKeyValuePair)
        {
            if (!((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))))
            {
                if (HistogramSuperCategories != null && GetHistogramSelectedItemCounts() <= 1)
                {
                    foreach (HistogramSuperCategory currentSuperCategory in HistogramSuperCategories)
                    {
                        foreach (HistogramSubCategory currentSubCategory in currentSuperCategory.HistogramSubCategories)
                        {
                            foreach (HistogramKeyValuePair currentKeyValuePair in currentSubCategory.ShowingHistogramKeyValuePairs)
                            {
                                currentKeyValuePair.IsSelected = false;
                            }
                        }
                    }
                    selectedHistogramKeyValuePair.IsSelected = true;
                    selectedHistogramKeyValuePair.relatedHistogramSubCategory.LastSelectedKeyValuePair = selectedHistogramKeyValuePair;
                }
            }

            OnSelectionChanged();
        }

        public void ClearAllHistogramSelectionis()
        {
            foreach (HistogramSuperCategory currentSuperCategory in HistogramSuperCategories)
            {
                foreach (HistogramSubCategory currentSubCategory in currentSuperCategory.HistogramSubCategories)
                {
                    foreach (HistogramKeyValuePair currentKeyValuePair in currentSubCategory.ShowingHistogramKeyValuePairs)
                    {
                        currentKeyValuePair.IsSelected = false;
                    }

                    foreach (HistogramKeyValuePair currentKeyValuePair in currentSubCategory.NotShowingHistogramKeyValuePairs)
                    {
                        currentKeyValuePair.IsSelected = false;
                    }
                }
            }

            OnSelectionChanged();
        }

        public void TakeSnapShot()
        {
            btnSnapShot.Visibility = Visibility.Collapsed;
            scrollViewer.ScrollToHome();
            OnSnapshotRequested(new SnapshotRequestEventArgs(mainGrid));
            btnSnapShot.Visibility = Visibility.Visible;
        }

        #endregion

        #region رخدادگردانها
        private void clearSelectionHyperLink_Click(object sender, RoutedEventArgs e)
        {
            HistogramSuperCategory histogramSuperCategory = (sender as Hyperlink).DataContext as HistogramSuperCategory;
            ClearAllHistogramKeyValuePair(histogramSuperCategory);
        }

        private void clearPropertySelectionHyperLink_Click(object sender, RoutedEventArgs e)
        {
            HistogramSuperCategory histogramSuperCategory = (sender as Hyperlink).DataContext as HistogramSuperCategory;
            ClearAllHistogramKeyValuePair(histogramSuperCategory);
        }

        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HistogramKeyValuePair selectedHistogramKeyValuePair = ((sender as Grid).DataContext as HistogramKeyValuePair);
            if (selectedHistogramKeyValuePair.KeyValueType != HisogramKeyValueType.ChangeHistogramShowingSize && e.ChangedButton == MouseButton.Left)
            {
                ChangeHistogramSelectionStateForLeftMouseButton(ref selectedHistogramKeyValuePair);
            }
            else if (selectedHistogramKeyValuePair.KeyValueType != HisogramKeyValueType.ChangeHistogramShowingSize && e.ChangedButton == MouseButton.Right)
            {
                ChangeHistogramSelectionStateForRightMouseButton(ref selectedHistogramKeyValuePair);
            }
        }

        private void showMoreHyperLink_Click(object sender, RoutedEventArgs e)
        {
            HistogramSubCategory selectedHistogramSubCategory = ((sender as Hyperlink).DataContext as HistogramKeyValuePair).relatedHistogramSubCategory;
            OnShowMoreButtonClicked(selectedHistogramSubCategory);
        }

        private void showFewerHyperLink_Click(object sender, RoutedEventArgs e)
        {
            HistogramSubCategory selectedHistogramSubCategory = ((sender as Hyperlink).DataContext as HistogramKeyValuePair).relatedHistogramSubCategory;
            OnShowFewerButtonClicked(selectedHistogramSubCategory);
        }

        private void showAllHyperLink_Click(object sender, RoutedEventArgs e)
        {
            HistogramSubCategory selectedHistogramSubCategory = ((sender as Hyperlink).DataContext as HistogramKeyValuePair).relatedHistogramSubCategory;
            OnShowAllButtonClicked(selectedHistogramSubCategory);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void btnSnapShot_Click(object sender, RoutedEventArgs e)
        {
            TakeSnapShot();
        }
        #endregion
    }
}
