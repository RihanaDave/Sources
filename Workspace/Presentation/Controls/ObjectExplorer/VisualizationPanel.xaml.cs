using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using GPAS.Workspace.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel;
using GPAS.Logger;
using GPAS.Workspace.Presentation.Controls.ObjectExplorer.Charts;
using GPAS.Workspace.Presentation.Utility;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer
{
    /// <summary>
    /// Interaction logic for VisualizationPanel.xaml
    /// </summary>
    /// 
    public partial class VisualizationPanel : INotifyPropertyChanged
    {
        #region متغیرهای سراسری
        ObjectSetBase currentObjectSet = null;
        #endregion

        #region رخدادها        
        public class ChangePreviewStatisticsSelectionEventArgs
        {
            public ChangePreviewStatisticsSelectionEventArgs(List<PreviewStatistic> previewStatistics)
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

        public event EventHandler<ChangePreviewStatisticsSelectionEventArgs> ShowRetrievedLinkedObjectsOnGraphRequested;
        protected virtual void OnShowRetrievedLinkedObjectsOnGraphRequested(List<PreviewStatistic> previewStatistics)
        {
            if (previewStatistics == null)
                throw new ArgumentNullException(nameof(previewStatistics));

            ShowRetrievedLinkedObjectsOnGraphRequested?.Invoke(this, new ChangePreviewStatisticsSelectionEventArgs(previewStatistics));
        }

        public class ChangeSelectionEventArgs
        {
            public ChangeSelectionEventArgs(List<PropertyValueStatistic> propertyValueStatistic)
            {
                if (propertyValueStatistic == null)
                    throw new ArgumentNullException(nameof(propertyValueStatistic));

                PropertyValueStatistic = propertyValueStatistic;
            }

            public List<PropertyValueStatistic> PropertyValueStatistic
            {
                get;
                private set;
            }
        }

        public event EventHandler<ChangeSelectionEventArgs> DrillDownRequested;
        protected virtual void OnDrillDownRequested(List<PropertyValueStatistic> propertyValueStatistic)
        {
            if (propertyValueStatistic == null)
                throw new ArgumentNullException(nameof(propertyValueStatistic));

            DrillDownRequested?.Invoke(this, new ChangeSelectionEventArgs(propertyValueStatistic));
        }

        public event EventHandler<BarChartControlDrillDownRequestEventArgs> BarChartDrillDownRequested;
        public void OnBarChartDrillDownRequested(BarChartControlDrillDownRequestEventArgs args)
        {
            BarChartDrillDownRequested?.Invoke(this, args);
        }

        public event EventHandler<BarChartControlDrillDownRequestEventArgs> BarChartShowRetrievedObjectsOnGraphRequested;
        public void OnBarChartShowRetrievedObjectsOnGraphRequested(BarChartControlDrillDownRequestEventArgs args)
        {
            BarChartShowRetrievedObjectsOnGraphRequested?.Invoke(this, args);
        }

        public event EventHandler<BarChartControlDrillDownRequestEventArgs> BarChartShowRetrievedObjectsOnMapRequested;
        public void OnBarChartShowRetrievedObjectsOnMapRequested(BarChartControlDrillDownRequestEventArgs args)
        {
            BarChartShowRetrievedObjectsOnMapRequested?.Invoke(this, args);
        }

        public event EventHandler<ChangeSelectionEventArgs> ShowRetrievedPropertyObjectsOnGraphRequested;
        protected virtual void OnShowRetrievedPropertyObjectsOnGraphRequested(List<PropertyValueStatistic> propertyValueStatistics)
        {
            if (propertyValueStatistics == null)
                throw new ArgumentNullException(nameof(propertyValueStatistics));

            ShowRetrievedPropertyObjectsOnGraphRequested?.Invoke(this, new ChangeSelectionEventArgs(propertyValueStatistics));
        }

        public event EventHandler<ChangeSelectionEventArgs> ShowRetrievedPropertyObjectsOnMapRequested;
        protected virtual void OnShowRetrievedPropertyObjectsOnMapRequested(List<PropertyValueStatistic> propertyValueStatistics)
        {
            if (propertyValueStatistics == null)
                throw new ArgumentNullException(nameof(propertyValueStatistics));

            ShowRetrievedPropertyObjectsOnMapRequested?.Invoke(this, new ChangeSelectionEventArgs(propertyValueStatistics));
        }

        public class ChangeScreenSizeEventArgs
        {
            public ChangeScreenSizeEventArgs(ScreenStatus screenStatus)
            {
                ScreenStatus = screenStatus;
            }

            public ScreenStatus ScreenStatus
            {
                get;
                private set;
            }
        }

        public event EventHandler<ChangeScreenSizeEventArgs> ScreenSizeChanged;
        protected virtual void OnScreenSizeChanged(ScreenStatus screenStatus)
        {
            ScreenSizeChanged?.Invoke(this, new ChangeScreenSizeEventArgs(screenStatus));
        }

        public event EventHandler<EventArgs> LinkTypeHistogramRequested;
        protected virtual void OnLinkTypeHistogramRequested()
        {
            if (LinkTypeHistogramRequested != null)
                LinkTypeHistogramRequested(this, EventArgs.Empty);
        }

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(PngBitmapEncoder image, string defaultFileName)
        {
            SnapshotTaken?.Invoke(this, new TakeSnapshotEventArgs(image, defaultFileName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region توابع
        public VisualizationPanel()
        {
            InitializeComponent();
            Init();
        }

        public void Init(ObjectSetBase objectSet = null)
        {
            DataContext = this;
            currentObjectSet = objectSet;
            propertyValueHistogramControl.Visibility = Visibility.Hidden;
            linkTypeHistogramControl.Visibility = Visibility.Hidden;

            ShowVisualPanelStartStep();
        }

        public int defaultloadLimit = 50;

        public async Task ShowPropertyValueHistogramControl(ObjectSetBase objectSet, PreviewStatistic exploringPreviewStatistic)
        {
            currentObjectSet = objectSet;
            await propertyValueHistogramControl.ShowPropertyValueCategory(currentObjectSet, exploringPreviewStatistic, 0);
            ShowVisualPanelPropertyValueHistogramControl();
        }

        public async Task RetrievePropertyBarValuesStatisticsBarChartControl(ObjectSetBase objectSet,
            PreviewStatistic exploringProperty)
        {
            currentObjectSet = objectSet;
            await barChartControl.RetrievePropertyBarValuesStatisticsWithDefaultRange(currentObjectSet, exploringProperty);
            ShowVisualPanelBarChartControl();
        }

        public async Task ShowLinkTypeHistogramControl(ObjectSetBase objectSet)
        {
            currentObjectSet = objectSet;
            ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();
            List<PreviewStatistic> previewStatistics = await objectExplorerModel.RetrieveLinkTypeStatistics(objectSet);
            objectSet.AddVisualizationPanelTool(new ViewModel.ObjectExplorer.VisualizationPanel.LinkTypeHistogramTool()
            {
                IsActiveTool = true,
                linkTypeStatistics = previewStatistics,
                VisualPanelToolType = ViewModel.ObjectExplorer.VisualizationPanel.VisualizationPanelToolType.LinkTypeHistogram
            });
            linkTypeHistogramControl.ShowLinkTypePreviewStatistics(previewStatistics);
            ShowVisualPanelLinkTypeHistogramControl();
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task ShowActiveToolOfSelectedObjectSetBase(ObjectSetBase objectSet)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            currentObjectSet = objectSet;
            string title = objectSet.Title;
            VisualizationPanelToolBase relatedActiveVisualTool = currentObjectSet.GetActiveVisualPanelTool();

            if (relatedActiveVisualTool != null)
            {
                if (relatedActiveVisualTool is PropertyValueHistogramTool)
                {
                    propertyValueHistogramControl.ShowPropertyValueHistogramTool(currentObjectSet, (relatedActiveVisualTool as PropertyValueHistogramTool));
                    ShowVisualPanelPropertyValueHistogramControl();
                }
                else if (relatedActiveVisualTool is LinkTypeHistogramTool)
                {
                    linkTypeHistogramControl.ShowLinkTypePreviewStatistics((relatedActiveVisualTool as LinkTypeHistogramTool).linkTypeStatistics);
                    ShowVisualPanelLinkTypeHistogramControl();
                }
                else if (relatedActiveVisualTool is BarChartTool)
                {
                    barChartControl.SetDefaultValue(currentObjectSet, (relatedActiveVisualTool as BarChartTool));
                    ShowVisualPanelBarChartControl();
                }
            }
            else
            {
                ShowVisualPanelStartStep();
            }


        }

        private void ResetVisualizationToolsGridLocation()
        {
            if (ToolControlsLocation == VisualizationPanelToolControlsLocation.CenterPanel)
            {
                double left, top;
                left = (visualizationBorderGrid.ActualWidth - visualizationToolsGrid.ActualWidth) / 2;
                left = (left < 0) ? 0 : left;
                top = (visualizationBorderGrid.ActualHeight - visualizationToolsGrid.ActualHeight) / 2;
                top += startStepStackPanel.ActualHeight;
                top = (top < startStepStackPanel.ActualHeight) ? startStepStackPanel.ActualHeight : top;

                visualizationToolsGrid.Margin = new Thickness(left, top, 0, 0);
            }
            else if (ToolControlsLocation == VisualizationPanelToolControlsLocation.TopPanel)
            {
                visualizationToolsGrid.Margin = new Thickness(0, -80, 0, 0);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ShowVisualPanelStartStep()
        {
            startStepStackPanel.Visibility = Visibility.Visible;

            propertyValueHistogramControl.Visibility = Visibility.Collapsed;
            linkTypeHistogramControl.Visibility = Visibility.Collapsed;
            barChartControl.Visibility = Visibility.Collapsed;

            SetToolControlsLocation(VisualizationPanelToolControlsLocation.CenterPanel);
        }

        public void HideAllControl()
        {
            startStepStackPanel.Visibility = Visibility.Hidden;
            linkTypeHistogramControl.Visibility = Visibility.Collapsed;
            barChartControl.Visibility = Visibility.Collapsed;
            propertyValueHistogramControl.Visibility = Visibility.Collapsed;
        }

        public void ShowVisualPanelBarChartControl()
        {
            if (ToolControlsLocation == VisualizationPanelToolControlsLocation.TopPanel)
            {
                HideAllControl();
                barChartControl.Visibility = Visibility.Visible;
            }
            else
            {
                HideAllControl();
                SetToolControlsLocation(VisualizationPanelToolControlsLocation.TopPanel);
                barChartControl.Visibility = Visibility.Visible;
            }
        }

        public void ShowVisualPanelPropertyValueHistogramControl()
        {
            if (ToolControlsLocation == VisualizationPanelToolControlsLocation.TopPanel)
            {
                HideAllControl();
                propertyValueHistogramControl.Visibility = Visibility.Visible;
            }
            else
            {
                HideAllControl();
                SetToolControlsLocation(VisualizationPanelToolControlsLocation.TopPanel);
                propertyValueHistogramControl.Visibility = Visibility.Visible;
            }
        }

        private void ShowVisualPanelLinkTypeHistogramControl()
        {
            if (ToolControlsLocation == VisualizationPanelToolControlsLocation.TopPanel)
            {
                HideAllControl();
                linkTypeHistogramControl.Visibility = Visibility.Visible;
            }
            else
            {
                HideAllControl();
                SetToolControlsLocation(VisualizationPanelToolControlsLocation.TopPanel);
                linkTypeHistogramControl.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Dependencies

        public ScreenStatus VisualizationScreenStatus
        {
            get { return (ScreenStatus)GetValue(VisualizationScreenStatusProperty); }
            set { SetValue(VisualizationScreenStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisualizationScreenStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisualizationScreenStatusProperty =
            DependencyProperty.Register("VisualizationScreenStatus", typeof(ScreenStatus), typeof(VisualizationPanel),
                new PropertyMetadata(ScreenStatus.normal, OnSetVisualizationScreenStatusChanged));

        private static void OnSetVisualizationScreenStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as VisualizationPanel).OnSetVisualizationScreenStatusChanged(e);
        }

        private void OnSetVisualizationScreenStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            OnScreenSizeChanged(VisualizationScreenStatus);
        }

        public PreviewStatistics PreviewStatistics
        {
            get { return (PreviewStatistics)GetValue(PreviewStatisticsProperty); }
            set { SetValue(PreviewStatisticsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PreviewStatistics.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewStatisticsProperty =
            DependencyProperty.Register("PreviewStatistics", typeof(PreviewStatistics), typeof(VisualizationPanel), new PropertyMetadata(null, OnSetPreviewStatisticsChanged));

        private static void OnSetPreviewStatisticsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as VisualizationPanel).OnSetPreviewStatisticsChanged(e);
        }

        private void OnSetPreviewStatisticsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (PreviewStatistics == null)
            {
                throw new ArgumentNullException(nameof(PreviewStatistics));
            }

            var ontology = Logic.OntologyProvider.GetOntology();
            var propertyTypePreviewStatistics = PreviewStatistics.Content.Where(ps =>
                                                    ps.Category == PreviewStatistic.PropertyTypesSuperCategoryTitle &&
                                                    ontology.GetBaseDataTypeOfProperty(ps.TypeURI) != Ontology.BaseDataTypes.GeoTime &&
                                                    ontology.GetBaseDataTypeOfProperty(ps.TypeURI) != Ontology.BaseDataTypes.GeoPoint &&
                                                    ontology.GetBaseDataTypeOfProperty(ps.TypeURI) != Ontology.BaseDataTypes.DateTime
                                                ).ToList();

            var numericalPropertyTypePreviewStatistics = propertyTypePreviewStatistics.Where(ps =>
                                                            ontology.GetBaseDataTypeOfProperty(ps.TypeURI) == Ontology.BaseDataTypes.Double ||
                                                            ontology.GetBaseDataTypeOfProperty(ps.TypeURI) == Ontology.BaseDataTypes.Int ||
                                                            ontology.GetBaseDataTypeOfProperty(ps.TypeURI) == Ontology.BaseDataTypes.Long
                                                        ).ToList();

            chartToolControl.MenuItems = numericalPropertyTypePreviewStatistics;
            //pieChartToolControl.MenuItems = numericalPropertyTypePreviewStatistics;
            propertyValueToolControl.MenuItems = propertyTypePreviewStatistics;
        }

        public VisualizationPanelToolControlsLocation ToolControlsLocation
        {
            get { return (VisualizationPanelToolControlsLocation)GetValue(ToolControlsLocationProperty); }
            set { SetValue(ToolControlsLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToolControlsLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolControlsLocationProperty =
            DependencyProperty.Register("ToolControlsLocation", typeof(VisualizationPanelToolControlsLocation), typeof(VisualizationPanel), new PropertyMetadata(VisualizationPanelToolControlsLocation.CenterPanel));

        private void SetToolControlsLocation(VisualizationPanelToolControlsLocation location)
        {
            ToolControlsLocation = location;
            if (ToolControlsLocation == VisualizationPanelToolControlsLocation.TopPanel)
            {
                foreach (VisualizationToolControl visualizationToolControl in visualizationToolsGrid.Children)
                {
                    visualizationToolControl.ViewMode = VisualizationToolControlViewMode.ToolbarView;
                    visualizationToolControl.Width = 130;
                }
            }
            else if (ToolControlsLocation == VisualizationPanelToolControlsLocation.CenterPanel)
            {
                foreach (VisualizationToolControl visualizationToolControl in visualizationToolsGrid.Children)
                {
                    visualizationToolControl.ViewMode = VisualizationToolControlViewMode.GridView;
                    visualizationToolControl.Width = 390;
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            ResetVisualizationToolsGridLocation();
        }

        #endregion

        #region رخدادگردانها
        private void btnChangeScreenSize_Click(object sender, RoutedEventArgs e)
        {
            if (VisualizationScreenStatus == ScreenStatus.normal)
            {
                VisualizationScreenStatus = ScreenStatus.fullScreen;
            }
            else
            {
                VisualizationScreenStatus = ScreenStatus.normal;
            }
        }

        private async void chartToolControl_DropDownMenuItemClicked(object sender, VisualizationToolControl.DropDownMenuItemClickEventArgs e)
        {
            try
            {
                await RetrievePropertyBarValuesStatisticsBarChartControl(currentObjectSet, e.Preview);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void propertyValueToolControl_DropDownMenuItemClicked(object sender, VisualizationToolControl.DropDownMenuItemClickEventArgs e)
        {
            try
            {
                ChartsWaitingControl.Message = Properties.Resources.Executing_statistical_query;
                ChartsWaitingControl.TaskIncrement();
                await ShowPropertyValueHistogramControl(currentObjectSet, e.Preview);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ChartsWaitingControl.TaskDecrement();
            }
        }

        private void pieChartToolControl_DropDownMenuItemClicked(object sender, VisualizationToolControl.DropDownMenuItemClickEventArgs e)
        {

        }

        private void linkTypeHistogramToolControl_ButtonClicked(object sender, RoutedEventArgs e)
        {
            OnLinkTypeHistogramRequested();
        }

        private void groupByToolControl_ButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void timeLineToolControl_ButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void visualizationBorderGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResetVisualizationToolsGridLocation();
        }

        private void propertyValueHistogramControl_ShowRetrievedObjectsOnGraphRequested(object sender, Histograms.PropertyValueHistogramControl.ChangeSelectionEventArgs e)
        {
            OnShowRetrievedPropertyObjectsOnGraphRequested(e.PropertyValueStatistic);
        }

        private void propertyValueHistogramControl_ShowRetrievedObjectsOnMapRequested(object sender, Histograms.PropertyValueHistogramControl.ChangeSelectionEventArgs e)
        {
            OnShowRetrievedPropertyObjectsOnMapRequested(e.PropertyValueStatistic);
        }

        private void linkTypeHistogramControl_ShowRetrievedObjectsOnGraphRequested(object sender, Histograms.LinkTypeHistogramControl.SelectedPreviewStatisticsEventArgs e)
        {
            OnShowRetrievedLinkedObjectsOnGraphRequested(e.PreviewStatistics);
        }

        private void propertyValueHistogramControl_DrillDownRequested(object sender, Histograms.PropertyValueHistogramControl.ChangeSelectionEventArgs e)
        {
            OnDrillDownRequested(e.PropertyValueStatistic);
        }

        private void barChartControl_DrillDownRequested(object sender, Charts.BarChartControlDrillDownRequestEventArgs e)
        {
            OnBarChartDrillDownRequested(e);
        }

        private void barChartControl_ShowRetrievedObjectsOnMapRequested(object sender, BarChartControlDrillDownRequestEventArgs e)
        {
            OnBarChartShowRetrievedObjectsOnMapRequested(e);
        }

        private void barChartControl_ShowRetrievedObjectsOnGraphRequested(object sender, BarChartControlDrillDownRequestEventArgs e)
        {
            OnBarChartShowRetrievedObjectsOnGraphRequested(e);
        }

        private void barChartControl_SnapshotRequested(object sender, Windows.SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        private void linkTypeHistogramControl_SnapshotTaken(object sender, Histogram.TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e.Snapshot, e.DefaultFileName);
        }

        private void PropertyValueHistogramControl_OnSnapshotTaken(object sender, Histogram.TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e.Snapshot, e.DefaultFileName);
        }

        public void Reset()
        {
            VisualizationScreenStatus = ScreenStatus.normal;
        }

        #endregion
    }

    public enum ScreenStatus
    {
        normal,
        fullScreen
    }

    public enum VisualizationPanelTools
    {
        None,
        Chart,
        PropertyValueHistogram,
        GroupBy,
        LinkTypeHistogram,
        PieChart,
        Timeline,
        Unknown
    }

    public enum VisualizationPanelToolControlsLocation
    {
        CenterPanel,
        TopPanel
    }
}
