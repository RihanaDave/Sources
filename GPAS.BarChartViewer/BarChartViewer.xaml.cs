using GPAS.BarChartViewer.Converters;
using GPAS.BranchingHistoryViewer;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Charting;
using Telerik.Windows.Controls.ChartView;
namespace GPAS.BarChartViewer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BarChartViewer : UserControl
    {
        #region Dependencies
        public LinearAxisMode VerticalAxisMode
        {
            get { return (LinearAxisMode)GetValue(VerticalAxisModeProperty); }
            set { SetValue(VerticalAxisModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalAxisMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalAxisModeProperty =
            DependencyProperty.Register("VerticalAxisMode", typeof(LinearAxisMode), typeof(BarChartViewer), new PropertyMetadata(LinearAxisMode.Normal, OnSetVerticalAxisModeChanged));

        private static void OnSetVerticalAxisModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetVerticalAxisModeChanged(e);
        }

        private void OnSetVerticalAxisModeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (VerticalAxisMode == LinearAxisMode.Normal)
            {
                chart.VerticalAxis = verticalLinearAxis;
            }
            else if (VerticalAxisMode == LinearAxisMode.Logarithmic)
            {
                chart.VerticalAxis = verticalLogarithmicAxis;
            }
            else
            {
                throw new NotSupportedException();
            }

            SetVerticalAxisRange();
            SetAllSelectedRangeVerticalAxis();
        }

        public OrientationStatus CrosshairLineMode
        {
            get { return (OrientationStatus)GetValue(CrosshairModeProperty); }
            set { SetValue(CrosshairModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CrosshairLineMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CrosshairModeProperty =
            DependencyProperty.Register("CrosshairLineMode", typeof(OrientationStatus), typeof(BarChartViewer), new PropertyMetadata(OrientationStatus.None));

        public OrientationStatus CrosshairLabelMode
        {
            get { return (OrientationStatus)GetValue(CrosshairLabelModeProperty); }
            set { SetValue(CrosshairLabelModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CrosshairLabelMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CrosshairLabelModeProperty =
            DependencyProperty.Register("CrosshairLabelMode", typeof(OrientationStatus), typeof(BarChartViewer), new PropertyMetadata(OrientationStatus.None));

        public string HorizontalAxisLabel
        {
            get { return (string)GetValue(HorizontalAxisLabelProperty); }
            set { SetValue(HorizontalAxisLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalAxisLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalAxisLabelProperty =
            DependencyProperty.Register("HorizontalAxisLabel", typeof(string), typeof(BarChartViewer), new PropertyMetadata(""));

        public string VerticalAxisLabel
        {
            get { return (string)GetValue(VerticalAxisLabelProperty); }
            set { SetValue(VerticalAxisLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalAxisLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalAxisLabelProperty =
            DependencyProperty.Register("VerticalAxisLabel", typeof(string), typeof(BarChartViewer), new PropertyMetadata("", OnSetVerticalAxisLabelChanged));

        private static void OnSetVerticalAxisLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetVerticalAxisLabelChanged(e);
        }

        private void OnSetVerticalAxisLabelChanged(DependencyPropertyChangedEventArgs e)
        {
            SetVerticalAxisLabelValueRangeCollection(VerticalAxisLabel);
        }

        public OrientationStatus AxisLabelMode
        {
            get { return (OrientationStatus)GetValue(AxisLabelModeProperty); }
            set { SetValue(AxisLabelModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AxisLabelMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AxisLabelModeProperty =
            DependencyProperty.Register("AxisLabelMode", typeof(OrientationStatus), typeof(BarChartViewer), new PropertyMetadata(OrientationStatus.None));

        public double MinimumRange
        {
            get { return (double)GetValue(MinimumRangeProperty); }
            set { SetValue(MinimumRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumRangeProperty =
            DependencyProperty.Register("MinimumRange", typeof(double), typeof(BarChartViewer), new PropertyMetadata((double)0, OnSetMinimumRangeChanged));

        private static void OnSetMinimumRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetMinimumRangeChanged(e);
        }

        private void OnSetMinimumRangeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MinimumRange < MinimumMinimumRange)
            {
                if (e.OldValue != null)
                    MinimumRange = (double)e.OldValue < MinimumMinimumRange ? MinimumMinimumRange : (double)e.OldValue;
                else
                    MinimumRange = 0;

                throw new IndexOutOfRangeException("MinimumRange can't be less than " + MinimumMinimumRange);
            }

            CalculateGap();

            SetAllSelectedRangeMinimumRange();
        }

        public double MaximumRange
        {
            get { return (double)GetValue(MaximumRangeProperty); }
            set { SetValue(MaximumRangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumRange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumRangeProperty =
            DependencyProperty.Register("MaximumRange", typeof(double), typeof(BarChartViewer), new PropertyMetadata((double)100, OnSetMaximumRangeChanged));

        private static void OnSetMaximumRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetMaximumRangeChanged(e);
        }

        private void OnSetMaximumRangeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (MaximumRange > MaximumMaximumRange)
            {
                if (e.OldValue != null)
                    MaximumRange = (double)e.OldValue > MaximumMaximumRange ? MaximumMaximumRange : (double)e.OldValue;
                else
                    MaximumRange = 100;
                throw new IndexOutOfRangeException("MaximumRange can't be greater than " + MaximumMaximumRange);
            }

            CalculateGap();

            SetAllSelectedRangeMinimumRange();
        }

        public int BucketCount
        {
            get { return (int)GetValue(BucketCountProperty); }
            set { SetValue(BucketCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BucketCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BucketCountProperty =
            DependencyProperty.Register("BucketCount", typeof(int), typeof(BarChartViewer), new PropertyMetadata(100, OnSetBucketCountChanged));

        private static void OnSetBucketCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetBucketCountChanged(e);
        }

        private void OnSetBucketCountChanged(DependencyPropertyChangedEventArgs e)
        {
            CalculateGap();

            SetAllSelectedRangeMinimumRange();
        }

        public Brush BarColor
        {
            get { return (Brush)GetValue(BarColorProperty); }
            set { SetValue(BarColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BarColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BarColorProperty =
            DependencyProperty.Register("BarColor", typeof(Brush), typeof(BarChartViewer), new PropertyMetadata(null, OnSetBarColorChanged));

        private static void OnSetBarColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetBarColorChanged(e);
        }

        private void OnSetBarColorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                BarColor = Resources["BarBrush"] as Brush;
                return;
            }

            SetBarColor();
        }

        private void SetBarColor()
        {
            Style defaultVisualStyle = new Style(typeof(Border));
            defaultVisualStyle.Setters.Add(new Setter(Border.BackgroundProperty, BarColor));
            barSeries.DefaultVisualStyle = defaultVisualStyle;
        }

        public Brush SelectedBarColor
        {
            get { return (Brush)GetValue(SelectedBarColorProperty); }
            set { SetValue(SelectedBarColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedBarColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBarColorProperty =
            DependencyProperty.Register("SelectedBarColor", typeof(Brush), typeof(BarChartViewer), new PropertyMetadata(null, OnSetSelectedBarColorChanged));

        private static void OnSetSelectedBarColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetSelectedBarColorChanged(e);
        }

        private void OnSetSelectedBarColorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                SelectedBarColor = Resources["SelectedBarBrush"] as Brush;
                return;
            }

            SelectionChartPalette.GlobalEntries[0] = new PaletteEntry(SelectedBarColor);
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(BarChartViewer), new PropertyMetadata(Orientation.Horizontal));

        public string EmptyContent
        {
            get { return (string)GetValue(EmptyContentProperty); }
            set { SetValue(EmptyContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EmptyContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EmptyContentProperty =
            DependencyProperty.Register("EmptyContent", typeof(string), typeof(BarChartViewer), new PropertyMetadata("No data plot"));

        public SelectionMode BarSelectionMode
        {
            get { return (SelectionMode)GetValue(BarSelectionModeProperty); }
            set { SetValue(BarSelectionModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BarSelectionMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BarSelectionModeProperty =
            DependencyProperty.Register("BarSelectionMode", typeof(SelectionMode), typeof(BarChartViewer),
                new PropertyMetadata(SelectionMode.Multiple, OnSetBarSelectionModeChanged));

        private static void OnSetBarSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BarChartViewer).OnSetBarSelectionModeChanged(e);
        }

        private void OnSetBarSelectionModeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (BarSelectionMode == SelectionMode.None || BarSelectionMode == SelectionMode.Single)
            {
                RemoveAllSelectedRanges();
                DeselectAllBars();
            }
        }

        #endregion

        #region Private Vars
        const double MinimumMinimumRange = ((double)decimal.MinValue / 2) + (19791209299969);
        const double MaximumMaximumRange = ((double)decimal.MaxValue / 2) - (19791209299969);

        private LinearAxis verticalLinearAxis = new LinearAxis() { TickOrigin = 0, Minimum = -1000 };
        private LogarithmicAxis verticalLogarithmicAxis = new LogarithmicAxis() { LogarithmBase = 10, Minimum = 0 };

        List<ValueRangePair> selectedBars = new List<ValueRangePair>();
        private ObservableCollection<SelectionRange> selectedRanges = new ObservableCollection<SelectionRange>();
        SelectionRange selectedSelectionRange;
        double startDrag, endDrag, moveDrag;
        bool isResizing = false;
        bool isMoving = false;
        SelectionRangeStrokeMouseEventState currentStrokeState = SelectionRangeStrokeMouseEventState.End;
        #endregion

        #region public Properties

        public List<ValueRangePair> ValueRangeCollection { get; private set; } = new List<ValueRangePair>();

        public double Gap { get; private set; }

        public ReadOnlyCollection<ValueRangePair> SelectedBars
        {
            get
            {
                return selectedBars.AsReadOnly();
            }
        }

        public ReadOnlyCollection<Range> SelectedRanges
        {
            get { return selectedRanges.Select(sr => (Range)sr).ToList().AsReadOnly(); }
        }
        #endregion

        #region Events
        public event EventHandler<BarChartSelectionChangedEventArgs> SelectionChanged;

        public void OnSelectionChanged(BarChartSelectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(this, args);
        }
        #endregion

        #region Private Methods

        private void Init()
        {
            DataContext = this;
            verticalLinearAxis.Title = Resources["txbVerticalAxisLabel"];
            Style style = this.FindResource("TextBlockStyle") as Style;
            verticalLinearAxis.LabelStyle = style;
            verticalLinearAxis.SetBinding(LogarithmicAxis.LabelTemplateProperty, new Binding("Orientation")
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = new OrientationToAxisLabelTemplateConverter()
            });

            verticalLogarithmicAxis.Title = Resources["txbVerticalAxisLabel"];
            verticalLogarithmicAxis.SetBinding(LogarithmicAxis.LabelTemplateProperty, new Binding("Orientation")
            {
                Source = this,
                Mode = BindingMode.OneWay,
                Converter = new OrientationToAxisLabelTemplateConverter()
            });

            chart.VerticalAxis = verticalLinearAxis;
            BarColor = Resources["BarBrush"] as Brush;

            selectedRanges.CollectionChanged += SelectedRanges_CollectionChanged;

            ITheme theme = paletteHelper.GetTheme();
            LinearGradientBrush defaultSelectedBarColor = new LinearGradientBrush(new GradientStopCollection(new List<GradientStop>() {
                    new GradientStop(theme.PrimaryDark.Color, 0),
                    new GradientStop(theme.PrimaryLight.Color, .5),
                    new GradientStop(theme.PrimaryDark.Color, 1),

                }), new Point(1, .5), new Point(0, .5));
            ChangeSelectedColor(defaultSelectedBarColor);

        }
        private PaletteHelper paletteHelper = new PaletteHelper();
       private void ChangeSelectedColor(Brush brush)
        {
            chart.SelectionPalette = new ChartPalette();
            PaletteEntry selectionPalette = new PaletteEntry(brush);
            chart.SelectionPalette.GlobalEntries.Add(selectionPalette);
        }

        private void SetVerticalAxisRange()
        {
            double maxValue = 0;
            double minValue = 0;
            if (ValueRangeCollection != null && ValueRangeCollection.Count != 0)
            {
                maxValue = ValueRangeCollection.Select(i => i.Value).Max();
                if (maxValue < 4)
                    maxValue = 4;

                minValue = ValueRangeCollection.Select(i => i.Value).Min();
                if (minValue > 0)
                    minValue = 0;
            }

            if (chart.VerticalAxis is LogarithmicAxis)
            {
                int strLenMax = maxValue.ToString().Length;
                maxValue = Math.Pow(10, strLenMax);
            }
            Style style = this.FindResource("TextBlockStyle") as Style;
            (chart.VerticalAxis as NumericalAxis).LabelStyle = style;
            (chart.VerticalAxis as NumericalAxis).Maximum = maxValue;
            (chart.VerticalAxis as NumericalAxis).Minimum = minValue;
        }

        private void SetVerticalAxisLabelValueRangeCollection(string verticalAxisLabel)
        {
            ValueRangeCollection?.ForEach(vrc => vrc.VerticalAxisLabel = verticalAxisLabel);
        }

        private void SelectedRanges_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            List<SelectionRange> added = new List<SelectionRange>();
            List<SelectionRange> removed = new List<SelectionRange>();

            if (e.NewItems?.Count > 0)
            {
                foreach (SelectionRange item in e.NewItems)
                {
                    item.AddAnnotationsToChart(chart.Annotations);
                    added.Add(item);
                }
            }
            if (e.OldItems?.Count > 0)
            {
                foreach (SelectionRange item in e.OldItems)
                {
                    item.RemoveAnnotationsToChart(chart.Annotations);
                    removed.Add(item);
                }
            }

            OnSelectionChanged(new BarChartSelectionChangedEventArgs(added, removed));
        }

        private void UserControl_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            StopResizeSelectionRange();
            StopMoveSelectionRange();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                if (isResizing)
                {
                    StopResizeSelectionRange();
                }

                if (isMoving)
                {
                    StopMoveSelectionRange();
                }
            }
        }

        private void ChartSelectionBehavior_SelectionChanged(object sender, ChartSelectionChangedEventArgs e)
        {
            selectedBars = chart.SelectedPoints.Select(sp => (ValueRangePair)sp.DataItem).ToList();

            var added = e.AddedPoints.Select(ap => (ValueRangePair)ap.DataItem);
            var removed = e.RemovedPoints.Select(rp => (ValueRangePair)rp.DataItem);

            if (BarSelectionMode == SelectionMode.None || BarSelectionMode == SelectionMode.Single)
            {
                RemoveAllSelectedRanges();
            }

            OnSelectionChanged(new BarChartSelectionChangedEventArgs(added, removed));
        }

        private void CartesianChartGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CreateNewSelectionRange(e.GetPosition(chart));
            currentStrokeState = SelectionRangeStrokeMouseEventState.End;
            StartResizeSelectionRange(e.GetPosition(chart));
        }

        private void CartesianChartGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopResizeSelectionRange();
            StopMoveSelectionRange();
        }

        private void CartesianChartGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                if (currentStrokeState == SelectionRangeStrokeMouseEventState.Start)
                {
                    startDrag = (double)chart.ConvertPointToData(e.GetPosition(chart)).FirstValue;
                }
                else if (currentStrokeState == SelectionRangeStrokeMouseEventState.End)
                {
                    endDrag = (double)chart.ConvertPointToData(e.GetPosition(chart)).FirstValue;
                }
                else
                {
                    throw new NotSupportedException();
                }

                ValidateSelectionRangeInResizing();
                GenerateSelectionRange(startDrag, endDrag);
            }
        }

        private void CartesianChartGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            ResizingSelectionRange(e.GetPosition(chart));
            MovingSelectionRange(e.GetPosition(chart));
        }

        private void SelectedSelectionRange_MouseLeave(object sender, SelectionRangeStrokeMouseEventArgs e)
        {
            if (isResizing)
            {
                if (currentStrokeState == SelectionRangeStrokeMouseEventState.Start)
                {
                    startDrag = (double)chart.ConvertPointToData(e.GetPosition(chart)).FirstValue;
                }
                else if (currentStrokeState == SelectionRangeStrokeMouseEventState.End)
                {
                    endDrag = (double)chart.ConvertPointToData(e.GetPosition(chart)).FirstValue;
                }
                else
                {
                    throw new NotSupportedException();
                }

                ValidateSelectionRangeInResizing();
                GenerateSelectionRange(startDrag, endDrag);
            }
        }

        private void SelectedSelectionRange_StrokeMouseLeftButtonDown(object sender, SelectionRangeStrokeMouseButtonEventArgs e)
        {
            selectedSelectionRange = (SelectionRange)sender;
            currentStrokeState = e.StrokeState;

            startDrag = selectedSelectionRange.Start;
            endDrag = selectedSelectionRange.End;

            StartResizeSelectionRange(e.GetPosition(chart));
        }

        private void SelectedSelectionRange_CloseButtonClicked(object sender, EventArgs e)
        {
            selectedRanges.Remove((SelectionRange)sender);
        }

        private void SelectedSelectionRange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectedSelectionRange = (SelectionRange)sender;

            StartMovingSelectionRange(e.GetPosition(chart));
        }

        private void StartMovingSelectionRange(Point point)
        {
            moveDrag = (double)chart.ConvertPointToData(point).FirstValue;

            foreach (var sr in selectedRanges)
            {
                sr.IsHitTestVisibleForStroke = false;
                sr.IsHitTestVisibleForFill = false;
            }

            startDrag = selectedSelectionRange.Start;
            endDrag = selectedSelectionRange.End;

            GenerateSelectionRange(startDrag, endDrag);

            isMoving = true;
            if (Orientation == Orientation.Horizontal)
                chart.Grid.Cursor = Cursors.ScrollWE;
            else
                chart.Grid.Cursor = Cursors.ScrollNS;

            selectedSelectionRange.ToolTip.IsOpen = true;
            barSeries.IsHitTestVisible = false;
        }

        private void MovingSelectionRange(Point point)
        {
            if (isMoving)
            {
                double previousMoveDrad = moveDrag;
                moveDrag = (double)chart.ConvertPointToData(point).FirstValue;
                double dis = moveDrag - previousMoveDrad;

                double start = startDrag + dis;
                double end = endDrag + dis;

                ValidateSelectionRangeInMoving(start, end);
                GenerateSelectionRange(startDrag, endDrag);
            }
        }

        private void StopMoveSelectionRange()
        {
            if (isMoving)
            {
                isMoving = false;
                chart.Grid.Cursor = Cursors.Arrow;

                foreach (var sr in selectedRanges)
                {
                    sr.IsHitTestVisibleForFill = true;
                    sr.IsHitTestVisibleForStroke = true;
                }
                selectedSelectionRange.ToolTip.IsOpen = false;
                barSeries.IsHitTestVisible = true;

                List<SelectionRange> added = new List<SelectionRange>() { selectedSelectionRange };
                List<SelectionRange> removed = new List<SelectionRange>();
                OnSelectionChanged(new BarChartSelectionChangedEventArgs(added, removed));
            }
        }

        private void ValidateSelectionRangeInMoving(double start, double end)
        {
            if (start < MinimumRange)
            {
                double diff = end - start;
                start = MinimumRange;
                end = start + diff;

            }
            if (end > MaximumRange)
            {
                double diff = end - start;
                end = MaximumRange;
                start = end - diff;
            }

            startDrag = start;
            endDrag = end;
        }

        private void CreateNewSelectionRange(Point point)
        {
            selectedSelectionRange = new SelectionRange()
            {
                Orientation = Orientation,
                HorizontalAxis = linearHorizontalAxis,
                VerticalAxis = chart.VerticalAxis as NumericalAxis,
                MinimumLength = Gap,
                ToolTipStyle = Resources["styleBarToolTip"] as Style,
            };

            selectedSelectionRange.CloseButtonClicked -= SelectedSelectionRange_CloseButtonClicked;
            selectedSelectionRange.CloseButtonClicked += SelectedSelectionRange_CloseButtonClicked;
            selectedSelectionRange.StrokeMouseLeftButtonDown -= SelectedSelectionRange_StrokeMouseLeftButtonDown;
            selectedSelectionRange.StrokeMouseLeftButtonDown += SelectedSelectionRange_StrokeMouseLeftButtonDown;
            selectedSelectionRange.MouseLeave -= SelectedSelectionRange_MouseLeave;
            selectedSelectionRange.MouseLeave += SelectedSelectionRange_MouseLeave;
            selectedSelectionRange.MouseLeftButtonDown -= SelectedSelectionRange_MouseLeftButtonDown;
            selectedSelectionRange.MouseLeftButtonDown += SelectedSelectionRange_MouseLeftButtonDown;

            startDrag = (double)chart.ConvertPointToData(point).FirstValue;
            endDrag = startDrag;

            ValidateSelectionRangeInResizing();
            GenerateSelectionRange(startDrag, endDrag);

            //selectedRanges.Add(selectedSelectionRange);
        }

        private void ValidateSelectionRangeLength()
        {
            if (Math.Abs(startDrag - endDrag) < Gap)
            {
                if (currentStrokeState == SelectionRangeStrokeMouseEventState.Start)
                {
                    if (startDrag <= endDrag)
                        startDrag = endDrag - Gap;
                    else
                        startDrag = endDrag + Gap;
                }
                else if (currentStrokeState == SelectionRangeStrokeMouseEventState.End)
                {
                    if (startDrag <= endDrag)
                        endDrag = startDrag + Gap;
                    else
                        endDrag = startDrag - Gap;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        private void CalculateGap()
        {
            if (ValueRangeCollection == null || ValueRangeCollection.Count == 0)
            {
                Gap = 0;
            }
            else
            {
                Gap = Math.Abs((MaximumRange - MinimumRange) / ValueRangeCollection.Count);
            }
        }

        private void SetAllSelectedRangeMinimumRange()
        {
            foreach (SelectionRange item in selectedRanges)
            {
                item.MinimumLength = Gap;
            }
        }

        private void SetAllSelectedRangeVerticalAxis()
        {
            foreach (SelectionRange item in selectedRanges)
            {
                item.VerticalAxis = chart.VerticalAxis as NumericalAxis;
            }
        }

        private void StartResizeSelectionRange(Point point)
        {
            foreach (var sr in selectedRanges)
            {
                sr.IsHitTestVisibleForFill = false;
                if (sr != selectedSelectionRange)
                    sr.IsHitTestVisibleForStroke = false;
            }
            selectedSelectionRange.IsHitTestVisibleForFill = false;

            if (currentStrokeState == SelectionRangeStrokeMouseEventState.Start)
            {
                startDrag = (double)chart.ConvertPointToData(point).FirstValue;
            }
            else if (currentStrokeState == SelectionRangeStrokeMouseEventState.End)
            {
                endDrag = (double)chart.ConvertPointToData(point).FirstValue;
            }
            else
            {
                throw new NotSupportedException();
            }

            ValidateSelectionRangeInResizing();
            GenerateSelectionRange(startDrag, endDrag);

            isResizing = true;
            if (Orientation == Orientation.Horizontal)
                chart.Grid.Cursor = Cursors.SizeWE;
            else
                chart.Grid.Cursor = Cursors.SizeNS;

            // selectedSelectionRange.ToolTip.IsOpen = true;
            barSeries.IsHitTestVisible = false;
        }

        private void StopResizeSelectionRange()
        {
            if (isResizing)
            {
                //GenerateSelectionRange();
                isResizing = false;
                chart.Grid.Cursor = Cursors.Arrow;

                foreach (var sr in selectedRanges)
                {
                    sr.IsHitTestVisibleForFill = true;
                    sr.IsHitTestVisibleForStroke = true;
                }
                selectedSelectionRange.ToolTip.IsOpen = false;
                barSeries.IsHitTestVisible = true;

                List<SelectionRange> added = new List<SelectionRange>() { selectedSelectionRange };
                List<SelectionRange> removed = new List<SelectionRange>();
                OnSelectionChanged(new BarChartSelectionChangedEventArgs(added, removed));
            }
        }

        private void ResizingSelectionRange(Point point)
        {
            if (isResizing)
            {
                if (!selectedRanges.Contains(selectedSelectionRange))
                {
                    if (BarSelectionMode == SelectionMode.None)
                    {
                        RemoveAllSelectedRanges();
                        DeselectAllBars();
                        return;
                    }
                    if (BarSelectionMode == SelectionMode.Single)
                    {
                        RemoveAllSelectedRanges();
                        DeselectAllBars();
                    }

                    selectedRanges.Add(selectedSelectionRange);
                }

                if (currentStrokeState == SelectionRangeStrokeMouseEventState.Start)
                {
                    startDrag = (double)chart.ConvertPointToData(point).FirstValue;
                    if (startDrag > endDrag)
                    {
                        var t = startDrag;
                        startDrag = endDrag;
                        endDrag = t;
                        currentStrokeState = SelectionRangeStrokeMouseEventState.End;
                    }
                }
                else if (currentStrokeState == SelectionRangeStrokeMouseEventState.End)
                {
                    endDrag = (double)chart.ConvertPointToData(point).FirstValue;
                }
                else
                {
                    throw new NotSupportedException();
                }

                ValidateSelectionRangeInResizing();
                GenerateSelectionRange(startDrag, endDrag);

                selectedSelectionRange.ToolTip.IsOpen = true;
            }
        }

        private void ValidateSelectionRangeInResizing()
        {
            ValidateSelectionRangeLength();
            BindToChartSelectionRange();
        }

        private void GenerateSelectionRange(double start, double end)
        {
            selectedSelectionRange.Start = start;
            selectedSelectionRange.End = end;
        }

        private void BindToChartSelectionRange()
        {
            if (currentStrokeState == SelectionRangeStrokeMouseEventState.Start)
            {
                if (startDrag < linearHorizontalAxis.Minimum)
                {
                    startDrag = linearHorizontalAxis.Minimum;
                }
                if (startDrag > linearHorizontalAxis.Maximum)
                {
                    startDrag = linearHorizontalAxis.Maximum;
                }
            }
            else if (currentStrokeState == SelectionRangeStrokeMouseEventState.End)
            {
                if (endDrag < linearHorizontalAxis.Minimum)
                {
                    endDrag = linearHorizontalAxis.Minimum;
                }
                if (endDrag > linearHorizontalAxis.Maximum)
                {
                    endDrag = linearHorizontalAxis.Maximum;
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region Public Methods

        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(BarChartViewer),
                new PropertyMetadata(ThemeApplication.Dark, OnSetCurrentThemeChanged));

        private static void OnSetCurrentThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BarChartViewer)d).OnSetCurrentThemeChanged(e);
        }

        private void OnSetCurrentThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
            foreach (var item in selectedRanges)
            {
                (item.ToolTip.Content as Label).Background =new SolidColorBrush(theme.CardBackground);
            }
        }
        public BarChartViewer()
        {
            InitializeComponent();
            Init();
        }

        public void SetValueRangeCollection(List<ValueRangePair> items)
        {
            ValueRangeCollection = new List<ValueRangePair>(items);
            SetVerticalAxisRange();
            CalculateGap();
            SetAllSelectedRangeMinimumRange();

            linearHorizontalAxis.Minimum = MinimumRange;
            linearHorizontalAxis.Maximum = MaximumRange;

            barSeries.ItemsSource = ValueRangeCollection;

            SetBarColor();

            SetVerticalAxisLabelValueRangeCollection(VerticalAxisLabel);
        }

        public void DeselectAllBars()
        {
            var selectedDataPoints = new List<CategoricalDataPoint>();
            foreach (var item in barSeries.DataPoints)
            {
                if (item.IsSelected)
                {
                    selectedDataPoints.Add(item);
                }
            }

            selectedDataPoints.Any(sdp => sdp.IsSelected = false);
        }

        public void RemoveAllSelectedRanges()
        {
            List<SelectionRange> added = new List<SelectionRange>();
            List<SelectionRange> removed = new List<SelectionRange>();

            foreach (SelectionRange item in selectedRanges)
            {
                item.RemoveAnnotationsToChart(chart.Annotations);
                removed.Add(item);
            }

            OnSelectionChanged(new BarChartSelectionChangedEventArgs(added, removed));

            selectedRanges.Clear();
        }

        public List<ValueRangePair> GetBarsInSelectedRanges()
        {
            List<ValueRangePair> allBarsInSelectionRanges = new List<ValueRangePair>();

            foreach (SelectionRange range in selectedRanges)
            {
                var startBar = ValueRangeCollection.FindIndex(vrc => vrc.Start <= range.Start && vrc.End >= range.Start);
                var endBar = ValueRangeCollection.FindIndex(vrc => vrc.Start <= range.End && vrc.End >= range.End);

                if (startBar < 0)
                    startBar = 0;
                if (endBar < 0)
                    endBar = ValueRangeCollection.Count - 1;

                for (int i = startBar; i <= endBar; i++)
                {
                    if (!allBarsInSelectionRanges.Contains(ValueRangeCollection[i]))
                    {
                        allBarsInSelectionRanges.Add(ValueRangeCollection[i]);
                    }
                }
            }

            return allBarsInSelectionRanges;
        }

        public double GetTotalValueInAllSelected()
        {
            double val = 0;
            List<ValueRangePair> allBarsInSelectionRanges = GetBarsInSelectedRanges();

            foreach (ValueRangePair bar in selectedBars)
            {
                if (!allBarsInSelectionRanges.Contains(bar))
                {
                    allBarsInSelectionRanges.Add(bar);
                }
            }

            foreach (ValueRangePair bar in allBarsInSelectionRanges)
            {
                val += bar.Value;
            }

            return val;
        }

        #endregion
    }
}
