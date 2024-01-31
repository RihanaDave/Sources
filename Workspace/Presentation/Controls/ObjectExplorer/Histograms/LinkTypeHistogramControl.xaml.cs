using GPAS.Histogram;
using GPAS.ObjectExplorerHistogramViewer;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Histograms
{
    /// <summary>
    /// Interaction logic for LinkTypeHistogramControl.xaml
    /// </summary>
    public partial class LinkTypeHistogramControl
    {
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

        public event EventHandler<SelectedPreviewStatisticsEventArgs> ShowRetrievedObjectsOnGraphRequested;
        protected virtual void OnShowRetrievedObjectsOnGraphRequested(List<PreviewStatistic> selectedPreviewStatistic)
        {
            if (selectedPreviewStatistic == null)
                throw new ArgumentNullException(nameof(selectedPreviewStatistic));

            ShowRetrievedObjectsOnGraphRequested?.Invoke(this, new SelectedPreviewStatisticsEventArgs(selectedPreviewStatistic));
        }        

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }

        #endregion

        protected bool CanAddToGraph
        {
            get => (bool)GetValue(CanAddToGraphProperty);
            set => SetValue(CanAddToGraphProperty, value);
        }

        public static readonly DependencyProperty CanAddToGraphProperty =
            DependencyProperty.Register(nameof(CanAddToGraph), typeof(bool),
                typeof(LinkTypeHistogramControl), new PropertyMetadata(false));

        public static readonly int PassObjectsCountLimit = int.Parse(ConfigurationManager.AppSettings["ObjectExplorerPassObjectsCountLimit"]);

        public LinkTypeHistogramControl()
        {
            InitializeComponent();
        }

        public void ShowLinkTypePreviewStatistics(List<PreviewStatistic> linkTypePreviewStatistics)
        {
            if (linkTypePreviewStatistics == null)
                throw new ArgumentNullException(nameof(linkTypePreviewStatistics));

            FillMainHistogram(linkTypePreviewStatistics);
        }

        private void FillMainHistogram(List<PreviewStatistic> previewStatistics)
        {
            var items = new ObservableCollection<HistogramItem>();

            var superCategories = previewStatistics.GroupBy(x => x.SuperCategory);

            foreach (var superCategory in superCategories)
            {
                var level1 = new HistogramItem
                {
                    Title = superCategory.Key
                };

                var categories = superCategory.GroupBy(x => x.Category);

                if (categories.Count() == 1 && categories.First().Key == superCategory.Key)
                {
                    foreach (var category in categories)
                    {
                        foreach (var statistic in category)
                        {
                            var level3 = new HistogramItem
                            {
                                Title = statistic.Title,
                                Icon = statistic.Icon,
                                Value = statistic.Count,
                                RelatedElement = statistic
                            };

                            level1.Items.Add(level3);
                        }
                    }
                }
                else
                {
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
                }

                items.Add(level1);
            }

            MainHistogram.Items = items;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
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
            
            CanAddToGraph = MainHistogram.SelectedItems.Sum(selectedItem => ((PreviewStatistic)selectedItem.RelatedElement).Count) > PassObjectsCountLimit ? false : true;
        }

        private void AddCurrentSelectionToGraphMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MainHistogram.SelectedItems == null)
                return;

            List<PreviewStatistic> selectedPreviewStatistic = MainHistogram.SelectedItems
                .Select(selectedItem => (PreviewStatistic)selectedItem.RelatedElement).ToList();

            OnShowRetrievedObjectsOnGraphRequested(selectedPreviewStatistic);
        }

        private void MainHistogram_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainContextMenu.IsOpen = false;
        }
    }
}
