using GPAS.Workspace.Presentation.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Helpers.Graph
{
    /// <summary>
    /// Interaction logic for FlowsHelper.xaml
    /// </summary>
    public partial class FlowsHelper
    {
        public FlowsHelper()
        {
            InitializeComponent();
            DataContext = this;
        }

        GraphControl RelatedGraphControl;
        public void Init(GraphControl graphControl)
        {
            if (RelatedGraphControl != null)
                throw new InvalidOperationException(Properties.Resources.Control_Is_Data_Initialized_Before);

            RelatedGraphControl = graphControl;

            BindAllProperties();
            Reset();
        }

        private void BindAllProperties()
        {
            ShowHideToggleButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsFlowsShown")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
            });

            AllObjectsRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding("FlowsSourceObjects")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
                Converter = new FlowsSourceObjectsToAllObjectsIsCheckedConverter()
            });

            SelectedObjectsRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding("FlowsSourceObjects")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
                Converter = new FlowsSourceObjectsToSelectedObjectsIsCheckedConverter()
            });

            AnimateRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding("FlowsVisualStyle")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
                Converter = new FlowsVisualStyleToAnimatedIsCheckedConverter()
            });

            StaticRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding("FlowsVisualStyle")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
                Converter = new FlowsVisualStyleToStaticIsCheckedConverter()
            });

            NoneRadioButton.SetBinding(RadioButton.IsCheckedProperty, new Binding("FlowsVisualStyle")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
                Converter = new FlowsVisualStyleToNoneIsCheckedConverter()
            });

            ScaleToFlowValueCheckBox.SetBinding(RadioButton.IsCheckedProperty, new Binding("IncludeWeightInFlows")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
            });

            SpeedSlider.SetBinding(Slider.ValueProperty, new Binding("FlowsSpeedInMiliSeconds")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
                Converter = new MiliSecondsToSpeedSliderValueConverter(),
                ConverterParameter = new double[] { SpeedSlider.Minimum, SpeedSlider.Maximum },
            });

            FlowColorPicker.SetBinding(ColorPickerViewer.ColorPickerViewer.SelectedColorProperty, new Binding("FlowsColor")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.TwoWay,
            });

            mainChart.SetBinding(BarChartViewer.BarChartViewer.MinimumRangeProperty, new Binding("MinFlowPathWeight")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.OneWay,
            });

            mainChart.SetBinding(BarChartViewer.BarChartViewer.MaximumRangeProperty, new Binding("MaxFlowPathWeight")
            {
                Source = RelatedGraphControl,
                Mode = BindingMode.OneWay,
            });

            ChartDataSource = new ObservableCollection<BarChartViewer.ValueRangePair>();
            this.SetBinding(FlowsHelper.ChartDataSourceProperty, new Binding("TotalFlowPathes")
            {
                Source = RelatedGraphControl.graphviewerMain,
                Mode = BindingMode.TwoWay,
                Converter = new FlowPathCollectionToValueRangePairCollectionConverter(),
                ConverterParameter = BucketCount
            });
        }

        public ObservableCollection<BarChartViewer.ValueRangePair> ChartDataSource
        {
            get { return (ObservableCollection<BarChartViewer.ValueRangePair>)GetValue(ChartDataSourceProperty); }
            set { SetValue(ChartDataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChartDataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChartDataSourceProperty =
            DependencyProperty.Register("ChartDataSource", typeof(ObservableCollection<BarChartViewer.ValueRangePair>), typeof(FlowsHelper),
                new PropertyMetadata(null, OnSetChartDataSourceChanged));

        private static void OnSetChartDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FlowsHelper).OnSetChartDataSourceChanged(e);
        }

        private void OnSetChartDataSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ChartDataSource.Count > 0)
            {
                mainChart.MinimumRange = ChartDataSource.Min(cds => cds.Start);
                mainChart.MaximumRange = ChartDataSource.Max(cds => cds.End);
            }

            mainChart.DeselectAllBars();
            mainChart.RemoveAllSelectedRanges();
            mainChart.SetValueRangeCollection(ChartDataSource.ToList());
        }

        public int BucketCount
        {
            get { return (int)GetValue(BucketCountProperty); }
            set { SetValue(BucketCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BucketCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BucketCountProperty =
            DependencyProperty.Register("BucketCount", typeof(int), typeof(FlowsHelper), new PropertyMetadata(100));


        private void mainChart_SelectionChanged(object sender, BarChartViewer.BarChartSelectionChangedEventArgs e)
        {
            if (RelatedGraphControl == null)
                return;

            if (mainChart.SelectedBars?.Count > 0)
            {
                RelatedGraphControl.MinFlowPathWeightToShow = mainChart.SelectedBars.Select(sb => sb.Start).Min();

                double epsilon = 1.0d;
                do
                {
                    epsilon /= 2.0d;
                }
                while ((double)(1.0 + epsilon) != 1.0);

                double max = mainChart.SelectedBars.Select(sb => sb.End).Max();
                while (max - epsilon == max)
                {
                    epsilon *= 2;
                }
                RelatedGraphControl.MaxFlowPathWeightToShow = max - epsilon;
            }
            else if (mainChart.SelectedRanges?.Count > 0)
            {
                RelatedGraphControl.MinFlowPathWeightToShow = mainChart.SelectedRanges.Select(sr => sr.Start).Min();
                RelatedGraphControl.MaxFlowPathWeightToShow = mainChart.SelectedRanges.Select(sr => sr.End).Max();
            }
            else
            {
                RelatedGraphControl.MinFlowPathWeightToShow = RelatedGraphControl.MinFlowPathWeight;
                RelatedGraphControl.MaxFlowPathWeightToShow = RelatedGraphControl.MaxFlowPathWeight;
            }
        }

        private void FlowColorPicker_Click(object sender, RoutedEventArgs e)
        {
            FlowColorPicker.IsDropDownOpen = !FlowColorPicker.IsDropDownOpen;
        }

        private void AllObjectsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowFlowsType.Text = "Show flow for all objects";
        }

        private void SelectedObjectsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowFlowsType.Text = "Show flow for selected objects";
        }

        public override void Reset()
        {
            ShowHideToggleButton.IsChecked = false;
            AllObjectsRadioButton.IsChecked = true;

            if (RelatedGraphControl != null)
                RelatedGraphControl.FlowsSpeedInMiliSeconds = 4000;

            AnimateRadioButton.IsChecked = true;
            FlowColorPicker.SelectedColor = Brushes.Red;
            ScaleToFlowValueCheckBox.IsChecked = true;
            mainChart.RemoveAllSelectedRanges();
        }
    }
}
