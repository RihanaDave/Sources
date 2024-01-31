using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet
{
    public abstract class ObjectSetBase : FrameworkElement
    {        
        public ObjectSetBase()
        {       
            visualizationPanelTools = new List<VisualizationPanelToolBase>();
        }

        static ObjectSetBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ObjectSetBase),
               new FrameworkPropertyMetadata(typeof(ObjectSetBase)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public static readonly RoutedEvent ActivatedSetEvent =
         EventManager.RegisterRoutedEvent("ActivatedSet", RoutingStrategy.Bubble,
         typeof(RoutedEventHandler), typeof(ObjectSetBase));

        public event RoutedEventHandler ActivatedSet
        {
            add { AddHandler(ActivatedSetEvent, value); }
            remove { RemoveHandler(ActivatedSetEvent, value); }
        }

        public void OnActivatedSet()
        {
            RoutedEventArgs args = new RoutedEventArgs(ObjectSetBase.ActivatedSetEvent);
            RaiseEvent(args);
        }

        public static readonly RoutedEvent DeactivatedSetEvent =
         EventManager.RegisterRoutedEvent("DeactivatedSet", RoutingStrategy.Bubble,
         typeof(RoutedEventHandler), typeof(ObjectSetBase));

        public event RoutedEventHandler DeactivatedSet
        {
            add { AddHandler(DeactivatedSetEvent, value); }
            remove { RemoveHandler(DeactivatedSetEvent, value); }
        }

        public void OnDeactivatedSet()
        {
            RoutedEventArgs args = new RoutedEventArgs(ObjectSetBase.DeactivatedSetEvent);
            RaiseEvent(args);
        }

        public event EventHandler<RecomputeSetEventArgs> RecomputeSet;

        public virtual void OnRecomputeSet()
        {
            RecomputeSet?.Invoke(this, new RecomputeSetEventArgs());
        }

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(ObjectSetBase), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ObjectSetBase), new PropertyMetadata(""));

        public string SubTitle
        {
            get { return (string)GetValue(SubTitleProperty); }
            set { SetValue(SubTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubTitleProperty =
            DependencyProperty.Register("SubTitle", typeof(string), typeof(ObjectSetBase), new PropertyMetadata(""));

        public long ObjectsCount
        {
            get { return (long)GetValue(ObjectsCountProperty); }
            set { SetValue(ObjectsCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectsCountProperty =
            DependencyProperty.Register("ObjectsCount", typeof(long), typeof(ObjectSetBase), new PropertyMetadata((long)0));

        public bool IsInActiveSetSequence
        {
            get { return (bool)GetValue(IsInActiveSetSequenceProperty); }
            set { SetValue(IsInActiveSetSequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInActiveSetSequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInActiveSetSequenceProperty =
            DependencyProperty.Register("IsInActiveSetSequence", typeof(bool), typeof(ObjectSetBase), new PropertyMetadata(false));

        public bool IsActiveSet
        {
            get { return (bool)GetValue(IsActiveSetProperty); }
            set { SetValue(IsActiveSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActiveSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveSetProperty =
            DependencyProperty.Register("IsActiveSet", typeof(bool), typeof(ObjectSetBase), new PropertyMetadata(false, OnSetIsActiveSetChanged));

        private static void OnSetIsActiveSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var osb = d as ObjectSetBase;
            osb.OnSetIsActiveSetChanged(e);
        }

        private void OnSetIsActiveSetChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                OnActivatedSet();
            }
            else
            {
                OnDeactivatedSet();
            }
        }

        public PreviewStatistics Preview
        {
            get { return (PreviewStatistics)GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Preview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register("Preview", typeof(PreviewStatistics), typeof(ObjectSetBase), new PropertyMetadata(null));


        public List<VisualizationPanelToolBase> visualizationPanelTools = null;
        public void AddVisualizationPanelTool(VisualizationPanelToolBase visualPanelTool)
        {
            DeactiveAllObjectSetVisualTools();

            if (visualizationPanelTools.Select(vt => vt.VisualPanelToolType).Contains(visualPanelTool.VisualPanelToolType))
            {
                UpdateVisualizationPanelTools(visualPanelTool);
            }
            else
            {
                visualizationPanelTools.Add(visualPanelTool);
            }
        }

        public void UpdateHistogramPropertyValueSortType(PropertyValueHistogramSortType newSortType)
        {
            DeactiveAllObjectSetVisualTools();

            if (visualizationPanelTools.Where(vt => vt.VisualPanelToolType == VisualizationPanelToolType.PropertyValueHistogram).Any())
            {
                PropertyValueHistogramTool propValueHistTool = (visualizationPanelTools.Where(vt => vt.VisualPanelToolType == VisualizationPanelToolType.PropertyValueHistogram).First()
                   as PropertyValueHistogramTool);
                propValueHistTool.SortType = newSortType;

                propValueHistTool.IsActiveTool = true;
            }
        }

        public void UpdateHistogramPropertyValueShowingCount(int newShowingCount)
        {
            DeactiveAllObjectSetVisualTools();

            if (visualizationPanelTools.Where(vt => vt.VisualPanelToolType == VisualizationPanelToolType.PropertyValueHistogram).Any())
            {
                PropertyValueHistogramTool propValueHistTool = (visualizationPanelTools.Where(vt => vt.VisualPanelToolType == VisualizationPanelToolType.PropertyValueHistogram).First()
                   as PropertyValueHistogramTool);
                propValueHistTool.ShowingCount = newShowingCount;

                propValueHistTool.IsActiveTool = true;
            }
        }

        private void DeactiveAllObjectSetVisualTools()
        {
            foreach (var currentTool in visualizationPanelTools)
            {
                currentTool.IsActiveTool = false;
            }
        }

        private void UpdateVisualizationPanelTools(VisualizationPanelToolBase visualPanelTool)
        {
            if (visualizationPanelTools.Select(vt => vt.VisualPanelToolType).Contains(visualPanelTool.VisualPanelToolType))
            {
                VisualizationPanelToolBase savedVisualPanelTool = visualizationPanelTools.Where(vt => vt.VisualPanelToolType == visualPanelTool.VisualPanelToolType).FirstOrDefault();
                if (savedVisualPanelTool.VisualPanelToolType == VisualizationPanelToolType.PropertyValueHistogram)
                {
                    savedVisualPanelTool.IsActiveTool = visualPanelTool.IsActiveTool;
                    (savedVisualPanelTool as PropertyValueHistogramTool).ShowingCount = (visualPanelTool as PropertyValueHistogramTool).ShowingCount;
                    (savedVisualPanelTool as PropertyValueHistogramTool).SortType = (visualPanelTool as PropertyValueHistogramTool).SortType;
                    (savedVisualPanelTool as PropertyValueHistogramTool).PropertyValueCategory = (visualPanelTool as PropertyValueHistogramTool).PropertyValueCategory;
                    (savedVisualPanelTool as PropertyValueHistogramTool).ExploringPreviewStatistic = (visualPanelTool as PropertyValueHistogramTool).ExploringPreviewStatistic;
                }
                else if (savedVisualPanelTool.VisualPanelToolType == VisualizationPanelToolType.LinkTypeHistogram)
                {
                    savedVisualPanelTool.IsActiveTool = visualPanelTool.IsActiveTool;
                    (savedVisualPanelTool as LinkTypeHistogramTool).linkTypeStatistics = (visualPanelTool as LinkTypeHistogramTool).linkTypeStatistics;
                }
                else if(savedVisualPanelTool.VisualPanelToolType == VisualizationPanelToolType.BarChart)
                {
                    savedVisualPanelTool.IsActiveTool = visualPanelTool.IsActiveTool;
                    (savedVisualPanelTool as BarChartTool).PropertyBarValues = (visualPanelTool as BarChartTool).PropertyBarValues;
                }
            }
        }

        public VisualizationPanelToolBase GetActiveVisualPanelTool()
        {
            VisualizationPanelToolBase activeVisualPanelTool = visualizationPanelTools.Where(vt => vt.IsActiveTool == true).FirstOrDefault();
            return activeVisualPanelTool;
        }
    }
}
