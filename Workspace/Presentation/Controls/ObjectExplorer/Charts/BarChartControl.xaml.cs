using GPAS.BarChartViewer;
using GPAS.Logger;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Model;
using GPAS.Workspace.Presentation.Windows;
using GPAS.Workspace.ViewModel.ObjectExplorer.Formula;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Charts
{
    public partial class BarChartControl
    {
        #region Dependencies

        public double MinimumRange
        {
            get { return (double)GetValue(MinimumRangeProperty); }
            set { SetValue(MinimumRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumRangeProperty =
            DependencyProperty.Register("MinimumRange", typeof(double), typeof(BarChartControl), new PropertyMetadata((double)0, OnSetMinimumRangeChanged));

        private static void OnSetMinimumRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartControl).OnSetMinimumRangeChanged(e);
        }

        private void OnSetMinimumRangeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MinimumRange < DefaultMinValue)
            {
                if (e.OldValue != null)
                    MinimumRange = (double)e.OldValue < DefaultMinValue ? DefaultMinValue : (double)e.OldValue;
                else
                    MinimumRange = 0;

                throw new IndexOutOfRangeException("MinimumRange can't be less than " + DefaultMinValue);
            }
        }

        public double MaximumRange
        {
            get { return (double)GetValue(MaximumRangeProperty); }
            set { SetValue(MaximumRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumRangeProperty =
            DependencyProperty.Register("MaximumRange", typeof(double), typeof(BarChartControl), new PropertyMetadata((double)100, OnSetMaximumRangeChanged));

        private static void OnSetMaximumRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartControl).OnSetMaximumRangeChanged(e);
        }

        private void OnSetMaximumRangeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MaximumRange > DefaultMaxValue)
            {
                if (e.OldValue != null)
                    MaximumRange = (double)e.OldValue > DefaultMaxValue ? DefaultMaxValue : (double)e.OldValue;
                else
                    MaximumRange = 100;
                throw new IndexOutOfRangeException("MaximumRange can't be greater than " + DefaultMaxValue);
            }
        }

        public int BucketCount
        {
            get { return (int)GetValue(BucketCountProperty); }
            set { SetValue(BucketCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BucketCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BucketCountProperty =
            DependencyProperty.Register("BucketCount", typeof(int), typeof(BarChartControl), new PropertyMetadata(100));

        public bool CanAddToGraphOrMap
        {
            get { return (bool)GetValue(CanAddToGraphOrMapProperty); }
            set { SetValue(CanAddToGraphOrMapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanAddToGraphOrMap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanAddToGraphOrMapProperty =
            DependencyProperty.Register("CanAddToGraphOrMap", typeof(bool), typeof(BarChartControl), new PropertyMetadata(false));

        public bool CanDrillDown
        {
            get { return (bool)GetValue(CanDrillDownProperty); }
            set { SetValue(CanDrillDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanDrillDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanDrillDownProperty =
            DependencyProperty.Register("CanDrillDown", typeof(bool), typeof(BarChartControl), new PropertyMetadata(false));


        #endregion

        private int defaultBucketCount = 100;
        const double DefaultMinValue = ((double)decimal.MinValue / 2) + (19791209299969);
        const double DefaultMaxValue = ((double)decimal.MaxValue / 2) - (19791209299969);
        public static readonly int PassObjectsCountLimit = int.Parse(ConfigurationManager.AppSettings["ObjectExplorerPassObjectsCountLimit"]);
        MenuItem mnuDrillDown = null;
        MenuItem mnuAddToGraph = null;
        MenuItem mnuAddToMap = null;
        PaletteHelper paletteHeplper = new PaletteHelper();


        public PreviewStatistic ExploringPreviewStatistic { get; set; }

        public PropertyBarValues PropertyBarValues { get; set; }
        public PropertyBarValues DefaultPropertyBarValues { get; set; }

        public ObjectSetBase RelatedObjectSet { get; set; }

        public event EventHandler<BarChartControlDrillDownRequestEventArgs> DrillDownRequested;
        public void OnDrillDownRequested(BarChartControlDrillDownRequestEventArgs args)
        {
            DrillDownRequested?.Invoke(this, args);
        }

        public event EventHandler<BarChartControlDrillDownRequestEventArgs> ShowRetrievedObjectsOnGraphRequested;
        public void OnShowRetrievedObjectsOnGraphRequested(BarChartControlDrillDownRequestEventArgs args)
        {
            ShowRetrievedObjectsOnGraphRequested?.Invoke(this, args);
        }

        public event EventHandler<BarChartControlDrillDownRequestEventArgs> ShowRetrievedObjectsOnMapRequested;
        public void OnShowRetrievedObjectsOnMapRequested(BarChartControlDrillDownRequestEventArgs args)
        {
            ShowRetrievedObjectsOnMapRequested?.Invoke(this, args);
        }

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }
        protected MainWindow MainWindow
        {
            get { return (MainWindow)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainWindow.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty MainWindowProperty =
            DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(BarChartControl), new PropertyMetadata(null));

        public BarChartControl()
        {
            InitializeComponent();
            Init();
            Loaded += BranchingHistoryControl_Loaded;
        }

        private void BranchingHistoryControl_Loaded(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this);
            MainWindow = w as MainWindow;

            if (MainWindow == null)
                return;

            MainWindow.CurrentThemeChanged -= MainWindow_CurrentThemeChanged;
            MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;
        }

        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            barChartViewer.CurrentTheme = MainWindow.CurrentTheme == ThemeApplication.Light ?
                BranchingHistoryViewer.ThemeApplication.Light :
                BranchingHistoryViewer.ThemeApplication.Dark;
        }
        SolidColorBrush acceptBrush;
        SolidColorBrush rejectBrush;
        SolidColorBrush WarningBrush;
        private void Init()
        {
            DataContext = this;
            ITheme theme = paletteHeplper.GetTheme();

            acceptBrush = new SolidColorBrush(theme.PrimaryMid.Color);
            rejectBrush = new SolidColorBrush(Colors.OrangeRed);
            WarningBrush = new SolidColorBrush(Colors.Orange);


            foreach (var item in barChartViewer.ContextMenu.Items)
            {
                if (!(item is MenuItem))
                    continue;

                MenuItem mnu = (MenuItem)item;

                if (mnu.Uid == "mnuDrillDown")
                {
                    mnuDrillDown = mnu;
                }
                else if (mnu.Uid == "mnuAddToGraph")
                {
                    mnuAddToGraph = mnu;
                }
                else if (mnu.Uid == "mnuAddToMap")
                {
                    mnuAddToMap = mnu;
                }
            }

            barChartViewer.SetBinding(BarChartViewer.BarChartViewer.VerticalAxisModeProperty, new Binding("IsChecked")
            {
                Source = chkLogAxis,
                Mode = BindingMode.TwoWay,
                Converter = new Convertor.LogAxisCheckBoxToVerticalAxisModeConverter()
            });

            barChartViewer.SetBinding(BarChartViewer.BarChartViewer.CrosshairLabelModeProperty, new Binding("IsChecked")
            {
                Source = chkShowCrossHairs,
                Mode = BindingMode.TwoWay,
                Converter = new Convertor.ShowCrosshairsCheckBoxToCrosshairsModeConverter()
            });

            barChartViewer.SetBinding(BarChartViewer.BarChartViewer.CrosshairModeProperty, new Binding("IsChecked")
            {
                Source = chkShowCrossHairs,
                Mode = BindingMode.TwoWay,
                Converter = new Convertor.ShowCrosshairsCheckBoxToCrosshairsModeConverter()
            });

            barChartViewer.SetBinding(BarChartViewer.BarChartViewer.MinimumRangeProperty, new Binding("MinimumRange")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            barChartViewer.SetBinding(BarChartViewer.BarChartViewer.MaximumRangeProperty, new Binding("MaximumRange")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            barChartViewer.SetBinding(BarChartViewer.BarChartViewer.BucketCountProperty, new Binding("BucketCount")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            BtnDrillDown.SetBinding(Button.IsEnabledProperty, new Binding("CanDrillDown")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            mnuDrillDown.SetBinding(MenuItem.IsEnabledProperty, new Binding("CanDrillDown")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            mnuAddToGraph.SetBinding(MenuItem.IsEnabledProperty, new Binding("CanAddToGraphOrMap")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            mnuAddToMap.SetBinding(MenuItem.IsEnabledProperty, new Binding("CanAddToGraphOrMap")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });
        }

        public async Task RetrievePropertyBarValuesStatisticsRecalculate(ObjectSetBase objectSet, PreviewStatistic exploringProperty, int bucketCount, double minValue, double maxValue)
        {
            if (objectSet == null)
            {
                throw new ArgumentNullException(nameof(objectSet));
            }

            var propertyBarValues = await LoadRetrievePropertyBarValuesStatisticsFromServer(objectSet, exploringProperty, bucketCount, minValue, maxValue);

            SetNewData(objectSet, propertyBarValues, DefaultPropertyBarValues, exploringProperty);
        }

        public async Task RetrievePropertyBarValuesStatisticsWithDefaultRange(ObjectSetBase objectSet, PreviewStatistic exploringProperty)
        {
            if (objectSet == null)
            {
                throw new ArgumentNullException(nameof(objectSet));
            }
            if (exploringProperty == null)
            {
                throw new ArgumentNullException(nameof(exploringProperty));
            }

            var propertyBarValues = await LoadRetrievePropertyBarValuesStatisticsFromServer(objectSet, exploringProperty, defaultBucketCount, double.MinValue, double.MaxValue);
            SetNewData(objectSet, propertyBarValues, propertyBarValues, exploringProperty);
        }

        public void SetDefaultValue(ObjectSetBase objectSet, BarChartTool barChart)
        {
            SetNewData(objectSet, barChart.PropertyBarValues, barChart.DefaultPropertyBarValues, barChart.ExploringPreviewStatistic);
        }

        public void SetNewData(ObjectSetBase objectSet, PropertyBarValues propertyBarValues, PropertyBarValues defaultPropertyBarValues, PreviewStatistic exploringPreviewStatistic)
        {
            if (objectSet == null || propertyBarValues == null || defaultPropertyBarValues == null || exploringPreviewStatistic == null)
                return;

            var lastObjectSet = RelatedObjectSet;
            var lastPropertyBarValues = PropertyBarValues;
            var lastDefaultPropertyBarValues = DefaultPropertyBarValues;
            var lastExploringPreviewStatistic = ExploringPreviewStatistic;

            try
            {
                BucketCount = propertyBarValues.BucketCount;
                MinimumRange = propertyBarValues.Bars.FirstOrDefault().Start;
                MaximumRange = propertyBarValues.Bars.LastOrDefault().End;

                RelatedObjectSet = objectSet;
                PropertyBarValues = propertyBarValues;
                DefaultPropertyBarValues = defaultPropertyBarValues;
                ExploringPreviewStatistic = exploringPreviewStatistic;

                objectSet.AddVisualizationPanelTool(new BarChartTool()
                {
                    IsActiveTool = true,
                    VisualPanelToolType = VisualizationPanelToolType.BarChart,
                    PropertyBarValues = PropertyBarValues,
                    DefaultPropertyBarValues = DefaultPropertyBarValues,
                    ExploringPreviewStatistic = ExploringPreviewStatistic,
                });

                barChartViewer.SetBinding(BarChartViewer.BarChartViewer.HorizontalAxisLabelProperty, new Binding("Title")
                {
                    Source = propertyBarValues,
                    Mode = BindingMode.TwoWay,
                });

                List<ValueRangePair> bars = new List<ValueRangePair>();

                foreach (var bar in propertyBarValues.Bars)
                {
                    bars.Add(new ValueRangePair()
                    {
                        Start = bar.Start,
                        End = bar.End,
                        Value = bar.Count,
                    });
                }

                barChartViewer.SetValueRangeCollection(bars);
                ClearAllSelections();

                ResetToolbar();
            }
            catch (Exception ex)
            {
                SetNewData(lastObjectSet, lastPropertyBarValues, lastDefaultPropertyBarValues, lastExploringPreviewStatistic);
                throw ex;
            }
        }

        private void ClearAllSelections()
        {
            barChartViewer.RemoveAllSelectedRanges();
            barChartViewer.DeselectAllBars();
        }

        private void ResetToolbar()
        {
            txtBucketCount.Text = PropertyBarValues.BucketCount.ToString();
            txtFrom.Text = PropertyBarValues.Start.ToString();
            txtTo.Text = PropertyBarValues.End.ToString();
        }

        public async Task<PropertyBarValues> LoadRetrievePropertyBarValuesStatisticsFromServer(ObjectSetBase objectSet,
            PreviewStatistic exploringProperty, int bucketCount, double minValue, double maxValue)
        {
            try
            {
                BarChartsWaitingControl.Message = Properties.Resources.Executing_statistical_query;
                BarChartsWaitingControl.TaskIncrement();

                ObjectExplorerModel objectExplorerModel = new ObjectExplorerModel();

                var ontology = Logic.OntologyProvider.GetOntology();
                if (ontology.GetBaseDataTypeOfProperty(exploringProperty.TypeURI) == Ontology.BaseDataTypes.Int ||
                    ontology.GetBaseDataTypeOfProperty(exploringProperty.TypeURI) == Ontology.BaseDataTypes.Long)
                {
                    minValue = Math.Floor(minValue);
                    maxValue = Math.Ceiling(maxValue);
                }

                PropertyBarValues PropertyBarValuesStatistics =
                    await objectExplorerModel.RetrievePropertyBarValuesStatistics
                        (objectSet, exploringProperty, bucketCount, minValue, maxValue);

                return PropertyBarValuesStatistics;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                BarChartsWaitingControl.TaskDecrement();
            }
        }

        private void barChartViewer_SelectionChanged(object sender, BarChartSelectionChangedEventArgs e)
        {
            CanDrillDown = barChartViewer.SelectedBars.Count + barChartViewer.SelectedRanges.Count > 0;

            string header = Properties.Resources.Drill_Down_on_Current_Selections___N__Group_;
            mnuDrillDown.Header = string.Format(header, barChartViewer.SelectedBars.Count + barChartViewer.SelectedRanges.Count);

            CanAddToGraphOrMap = CanDrillDown && barChartViewer.GetTotalValueInAllSelected() <= PassObjectsCountLimit;
        }

        private void BtnDrillDown_Click(object sender, RoutedEventArgs e)
        {
            DrillDown();
        }

        private void DrillDown()
        {
            PropertyValueRangeDrillDown propertyValueRangeFilters = new PropertyValueRangeDrillDown()
            {
                PropertyTypeUri = PropertyBarValues.TypeUri,
            };

            propertyValueRangeFilters.ValueRanges = GetSelectedBarChartRanges();

            var ontology = Logic.OntologyProvider.GetOntology();
            if (ontology.GetBaseDataTypeOfProperty(propertyValueRangeFilters.PropertyTypeUri) == Ontology.BaseDataTypes.Int ||
                ontology.GetBaseDataTypeOfProperty(propertyValueRangeFilters.PropertyTypeUri) == Ontology.BaseDataTypes.Long)
            {
                foreach (var vr in propertyValueRangeFilters.ValueRanges)
                {
                    vr.Start = Math.Floor(vr.Start);
                    vr.End = Math.Ceiling(vr.End);
                }
            }

            OnDrillDownRequested(new BarChartControlDrillDownRequestEventArgs(propertyValueRangeFilters));
        }

        private List<NumericPropertyValueRange> GetSelectedBarChartRanges()
        {
            List<NumericPropertyValueRange> numericPropertyValueRanges = new List<NumericPropertyValueRange>();

            foreach (var bar in barChartViewer.SelectedBars)
            {
                numericPropertyValueRanges.Add(new NumericPropertyValueRange() { Start = bar.Start, End = bar.End });
            }

            foreach (var selection in barChartViewer.SelectedRanges)
            {
                numericPropertyValueRanges.Add(new NumericPropertyValueRange() { Start = selection.Start, End = selection.End });
            }

            return numericPropertyValueRanges;
        }

        private void txtBucketCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetIsEnabledRecalculateBucketsButton();

            switch (IsValidBucketCount(txtBucketCount.Text))
            {
                case ValidationStatus.Invalid:
                    TextFieldAssist.SetUnderlineBrush(txtBucketCount, rejectBrush);
                    break;
                case ValidationStatus.Valid:
                    TextFieldAssist.SetUnderlineBrush(txtBucketCount, acceptBrush);
                    break;
                case ValidationStatus.Warning:
                    TextFieldAssist.SetUnderlineBrush(txtBucketCount, WarningBrush);
                    break;
                default:
                    break;
            }
        }

        private void txtTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetIsEnabledRecalculateBucketsButton();
        }

        private void txtFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetIsEnabledRecalculateBucketsButton(); ;

            SetBackgroundValidationForRange();
        }

        private void SetIsEnabledRecalculateBucketsButton()
        {
            btnRecalculateBuckets.IsEnabled = IsPossibleRecalculateBuckets();

            SetBackgroundValidationForRange();
        }

        private void SetBackgroundValidationForRange()
        {
            object fromObj, toObj;
            ValidationStatus validationFrom = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Double, txtFrom.Text, out fromObj, CultureInfo.CurrentCulture).Status;
            ValidationStatus validationTo = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Double, txtTo.Text, out toObj, CultureInfo.CurrentCulture).Status;

            double from = (double)fromObj;
            double to = (double)toObj;
            if (from < DefaultMinValue)
            {
                validationFrom = ValidationStatus.Invalid;
            }

            if (to > DefaultMaxValue)
            {
                validationTo = ValidationStatus.Invalid;
            }

            if (validationFrom != ValidationStatus.Invalid && validationTo != ValidationStatus.Invalid)
            {
                if (!IsValidRange(from, to))
                {
                    validationFrom = ValidationStatus.Invalid;
                    validationTo = ValidationStatus.Invalid;
                }
            }

            switch (validationTo)
            {
                case ValidationStatus.Invalid:
                    TextFieldAssist.SetUnderlineBrush(txtTo, rejectBrush);
                    break;
                case ValidationStatus.Valid:
                    TextFieldAssist.SetUnderlineBrush(txtTo, acceptBrush);
                    break;
                case ValidationStatus.Warning:
                    TextFieldAssist.SetUnderlineBrush(txtTo, WarningBrush);
                    break;
                default:
                    break;
            }

            switch (validationFrom)
            {
                case ValidationStatus.Invalid:
                    TextFieldAssist.SetUnderlineBrush(txtFrom, rejectBrush);
                    break;
                case ValidationStatus.Valid:
                    TextFieldAssist.SetUnderlineBrush(txtFrom, acceptBrush);
                    break;
                case ValidationStatus.Warning:
                    TextFieldAssist.SetUnderlineBrush(txtFrom, WarningBrush);
                    break;
                default:
                    break;
            }
        }

        private bool IsPossibleRecalculateBuckets()
        {
            if (IsValidBucketCount(txtBucketCount.Text) == ValidationStatus.Invalid ||
                PropertyManager.IsPropertyValid(Ontology.BaseDataTypes.Double, txtFrom.Text, CultureInfo.CurrentCulture).Status == ValidationStatus.Invalid ||
                PropertyManager.IsPropertyValid(Ontology.BaseDataTypes.Double, txtTo.Text, CultureInfo.CurrentCulture).Status == ValidationStatus.Invalid)
            {
                return false;
            }
            else
            {
                double from = (double)ValueBaseValidation.ParsePropertyValue(Ontology.BaseDataTypes.Double, txtFrom.Text, CultureInfo.CurrentCulture);
                double to = (double)ValueBaseValidation.ParsePropertyValue(Ontology.BaseDataTypes.Double, txtTo.Text, CultureInfo.CurrentCulture);

                if (from < DefaultMinValue || to > DefaultMaxValue)
                    return false;

                return IsValidRange(from, to);
            }
        }

        private bool IsValidRange(double from, double to)
        {
            return from < to;
        }

        private ValidationStatus IsValidBucketCount(string bucketCount)
        {
            object resultObj;
            ValidationStatus valid = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.Int, bucketCount, out resultObj, CultureInfo.CurrentCulture).Status;
            if (valid == ValidationStatus.Invalid)
            {
                return valid;
            }
            else
            {
                int result = (int)resultObj;
                if (result >= 10 && result < 1000)
                {
                    return valid;
                }
                else
                {
                    return ValidationStatus.Invalid;
                }
            }
        }

        private void btnRestoreDefault_Click(object sender, RoutedEventArgs e)
        {
            SetNewData(RelatedObjectSet, DefaultPropertyBarValues, DefaultPropertyBarValues, ExploringPreviewStatistic);
        }

        private void btnRecalculateBuckets_Click(object sender, RoutedEventArgs e)
        {
            RecalculateBuckets();
        }

        private async void RecalculateBuckets()
        {
            try
            {
                if (!IsPossibleRecalculateBuckets())
                {
                    return;
                }

                int bucketCount = (int)(long)ValueBaseValidation.ParsePropertyValue(Ontology.BaseDataTypes.Int, txtBucketCount.Text, CultureInfo.CurrentCulture);
                double minValue = (double)ValueBaseValidation.ParsePropertyValue(Ontology.BaseDataTypes.Double, txtFrom.Text, CultureInfo.CurrentCulture);
                double maxValue = (double)ValueBaseValidation.ParsePropertyValue(Ontology.BaseDataTypes.Double, txtTo.Text, CultureInfo.CurrentCulture);

                await RetrievePropertyBarValuesStatisticsRecalculate(RelatedObjectSet, ExploringPreviewStatistic, bucketCount, minValue, maxValue);

            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtBucketCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RecalculateBuckets();
        }

        private void txtFrom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RecalculateBuckets();
        }

        private void txtTo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RecalculateBuckets();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (sender as MenuItem);

            if (menuItem.Uid == "mnuDrillDown")
            {
                DrillDown();
            }
            else if (menuItem.Uid == "mnuAddToGraph")
            {
                ShowOnGraph();
            }
            else if (menuItem.Uid == "mnuAddToMap")
            {
                ShowOnMap();
            }
            else if (menuItem.Uid == "mnuClearAllSelections")
            {
                ClearAllSelections();
            }
        }

        private void ShowOnGraph()
        {
            PropertyValueRangeDrillDown propertyValueRangeFilters = new PropertyValueRangeDrillDown()
            {
                PropertyTypeUri = PropertyBarValues.TypeUri,
            };

            propertyValueRangeFilters.ValueRanges = GetSelectedBarChartRanges();

            var ontology = Logic.OntologyProvider.GetOntology();
            if (ontology.GetBaseDataTypeOfProperty(propertyValueRangeFilters.PropertyTypeUri) == Ontology.BaseDataTypes.Int ||
                ontology.GetBaseDataTypeOfProperty(propertyValueRangeFilters.PropertyTypeUri) == Ontology.BaseDataTypes.Long)
            {
                foreach (var vr in propertyValueRangeFilters.ValueRanges)
                {
                    vr.Start = Math.Floor(vr.Start);
                    vr.End = Math.Ceiling(vr.End);
                }
            }

            OnShowRetrievedObjectsOnGraphRequested(new BarChartControlDrillDownRequestEventArgs(propertyValueRangeFilters));
        }

        private void ShowOnMap()
        {
            PropertyValueRangeDrillDown propertyValueRangeFilters = new PropertyValueRangeDrillDown()
            {
                PropertyTypeUri = PropertyBarValues.TypeUri,
            };

            propertyValueRangeFilters.ValueRanges = GetSelectedBarChartRanges();

            var ontology = Logic.OntologyProvider.GetOntology();
            if (ontology.GetBaseDataTypeOfProperty(propertyValueRangeFilters.PropertyTypeUri) == Ontology.BaseDataTypes.Int ||
                ontology.GetBaseDataTypeOfProperty(propertyValueRangeFilters.PropertyTypeUri) == Ontology.BaseDataTypes.Long)
            {
                foreach (var vr in propertyValueRangeFilters.ValueRanges)
                {
                    vr.Start = Math.Floor(vr.Start);
                    vr.End = Math.Ceiling(vr.End);
                }
            }

            OnShowRetrievedObjectsOnMapRequested(new BarChartControlDrillDownRequestEventArgs(propertyValueRangeFilters));
        }

        private void btnSnapShot_Click(object sender, RoutedEventArgs e)
        {
            OnSnapshotRequested(new SnapshotRequestEventArgs(barChartViewer, $"Object Explorer Application - Bar Chart {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}"));
        }
    }

    public class BarChartControlDrillDownRequestEventArgs : EventArgs
    {
        public BarChartControlDrillDownRequestEventArgs(PropertyValueRangeDrillDown propertyValueRangeDrillDown)
        {
            PropertyValueRangeDrillDown = propertyValueRangeDrillDown;
        }

        public PropertyValueRangeDrillDown PropertyValueRangeDrillDown { get; private set; }
    }
}
