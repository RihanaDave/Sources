using GPAS.TimelineViewer.EventArguments;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;

namespace GPAS.TimelineViewer
{
    internal class FilterWindow : FilterRange
    {
        private void SetBindingsProperties()
        {
            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.FillProperty, new Binding("Fill")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.StrokeProperty, new Binding("Stroke")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.StrokeThicknessProperty, new Binding("StrokeThickness")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.AxisProperty, new Binding("HorizontalAxis")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            closeAnnotation.SetBinding(CartesianCustomAnnotation.HorizontalAxisProperty, new Binding("HorizontalAxis")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            startLineAnnotation.SetBinding(CartesianGridLineAnnotation.AxisProperty, new Binding("HorizontalAxis")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            endLineAnnotation.SetBinding(CartesianGridLineAnnotation.AxisProperty, new Binding("HorizontalAxis")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            closeAnnotation.SetBinding(CartesianCustomAnnotation.VerticalAxisProperty, new Binding("VerticalAxis")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.IsHitTestVisibleProperty, new Binding("IsHitTestVisibleForFill")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            closeAnnotation.SetBinding(CartesianCustomAnnotation.IsHitTestVisibleProperty, new Binding("IsHitTestVisibleForFill")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            startLineAnnotation.SetBinding(CartesianGridLineAnnotation.IsHitTestVisibleProperty, new Binding("IsHitTestVisibleForStroke")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            endLineAnnotation.SetBinding(CartesianGridLineAnnotation.IsHitTestVisibleProperty, new Binding("IsHitTestVisibleForStroke")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            btnClose.SetBinding(Button.IsHitTestVisibleProperty, new Binding("IsHitTestVisible")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.FromProperty, new Binding("From")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            startLineAnnotation.SetBinding(CartesianGridLineAnnotation.ValueProperty, new Binding("From")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.ToProperty, new Binding("To")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            endLineAnnotation.SetBinding(CartesianGridLineAnnotation.ValueProperty, new Binding("To")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.ContextMenuProperty, new Binding("ContextMenu")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            endLineAnnotation.SetBinding(CartesianGridLineAnnotation.ContextMenuProperty, new Binding("ContextMenu")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            startLineAnnotation.SetBinding(CartesianGridLineAnnotation.ContextMenuProperty, new Binding("ContextMenu")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });

            closeAnnotation.SetBinding(CartesianCustomAnnotation.ContextMenuProperty, new Binding("ContextMenu")
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(FilterWindow), new PropertyMetadata(null));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(FilterWindow), new PropertyMetadata(null));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(FilterWindow), new PropertyMetadata(1.0, OnSetStrokeThicknessChanged));

        private static void OnSetStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetStrokeThicknessChanged(e);
        }

        private void OnSetStrokeThicknessChanged(DependencyPropertyChangedEventArgs e)
        {
            startLineAnnotation.StrokeThickness = StrokeThickness + minimumThicknessLineAnnotation;
            endLineAnnotation.StrokeThickness = StrokeThickness + minimumThicknessLineAnnotation;
        }

        public DateTimeContinuousAxis HorizontalAxis
        {
            get { return (DateTimeContinuousAxis)GetValue(HorizontalAxisProperty); }
            set { SetValue(HorizontalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalAxisProperty =
            DependencyProperty.Register("HorizontalAxis", typeof(DateTimeContinuousAxis), typeof(FilterWindow), new PropertyMetadata(null, OnSetHorizontalAxisChanged));

        private static void OnSetHorizontalAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetHorizontalAxisChanged(e);
        }

        private void OnSetHorizontalAxisChanged(DependencyPropertyChangedEventArgs e)
        {
            SetChart();
            CalculateBinEachPixel();
        }

        public NumericalAxis VerticalAxis
        {
            get { return (NumericalAxis)GetValue(VerticalAxisProperty); }
            set { SetValue(VerticalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalAxisProperty =
            DependencyProperty.Register("VerticalAxis", typeof(NumericalAxis), typeof(FilterWindow), new PropertyMetadata(null, OnSetVerticalAxisChanged));

        private static void OnSetVerticalAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetVerticalAxisChanged(e);
        }

        private void OnSetVerticalAxisChanged(DependencyPropertyChangedEventArgs e)
        {
            SetChart();
            CalculateBinEachPixel();
            SetCloseButtonLocation();
        }

        public bool IsHitTestVisibleForFill
        {
            get { return (bool)GetValue(IsHitTestVisibleForFillProperty); }
            set { SetValue(IsHitTestVisibleForFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHitTestVisibleForFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHitTestVisibleForFillProperty =
            DependencyProperty.Register("IsHitTestVisibleForFill", typeof(bool), typeof(FilterWindow), new PropertyMetadata(true));

        public bool IsHitTestVisibleForStroke
        {
            get { return (bool)GetValue(IsHitTestVisibleForStrokeProperty); }
            set { SetValue(IsHitTestVisibleForStrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHitTestVisibleForStroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHitTestVisibleForStrokeProperty =
            DependencyProperty.Register("IsHitTestVisibleForStroke", typeof(bool), typeof(FilterWindow), new PropertyMetadata(true));

        public DateTime? LowerBound
        {
            get { return (DateTime?)GetValue(LowerBoundProperty); }
            set { SetValue(LowerBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LowerBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LowerBoundProperty =
            DependencyProperty.Register("LowerBound", typeof(DateTime?), typeof(FilterWindow), new PropertyMetadata(Utility.MinValue, OnSetLowerBoundChanged));

        private static void OnSetLowerBoundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetLowerBoundChanged(e);
        }

        private void OnSetLowerBoundChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        public DateTime? UpperBound
        {
            get { return (DateTime?)GetValue(UpperBoundProperty); }
            set { SetValue(UpperBoundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpperBound.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpperBoundProperty =
            DependencyProperty.Register("UpperBound", typeof(DateTime?), typeof(FilterWindow), new PropertyMetadata(Utility.MaxValue, OnSetUpperBoundChanged));

        private static void OnSetUpperBoundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetUpperBoundChanged(e);
        }

        private void OnSetUpperBoundChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        public Style ToolTipStyle
        {
            get { return (Style)GetValue(ToolTipStyleProperty); }
            set { SetValue(ToolTipStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToolTipStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolTipStyleProperty =
            DependencyProperty.Register("ToolTipStyle", typeof(Style), typeof(FilterWindow), new PropertyMetadata(null, OnSetToolTipStyleChanged));

        private static void OnSetToolTipStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetToolTipStyleChanged(e);
        }

        private void OnSetToolTipStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            lblContentToolTip.Style = ToolTipStyle;
        }

        public ContextMenu ContextMenu
        {
            get { return (ContextMenu)GetValue(ContextMenuProperty); }
            set { SetValue(ContextMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextMenuProperty =
            DependencyProperty.Register("ContextMenu", typeof(ContextMenu), typeof(FilterWindow), new PropertyMetadata(null));

        internal BinSize Bin
        {
            get { return (BinSize)GetValue(BinProperty); }
            set { SetValue(BinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bin.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty BinProperty =
            DependencyProperty.Register("Bin", typeof(BinSize), typeof(FilterWindow), new PropertyMetadata(BinSizes.Default, OnSetBinChanged));

        private static void OnSetBinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FilterWindow).OnSetBinChanged(e);
        }

        private void OnSetBinChanged(DependencyPropertyChangedEventArgs e)
        {
            SetBinChanged();
        }

        internal void SetBinChanged()
        {
            CalculateBinEachPixel();

            SetCloseButtonLocation();
        }

        System.Windows.Controls.Label lblContentToolTip = new System.Windows.Controls.Label();
        CartesianPlotBandAnnotation mainAnnotation = new CartesianPlotBandAnnotation();
        CartesianCustomAnnotation closeAnnotation = new CartesianCustomAnnotation();
        CartesianGridLineAnnotation startLineAnnotation = new CartesianGridLineAnnotation()
        {
            StrokeThickness = 6,
            IsHitTestVisible = true,
            Stroke = new SolidColorBrush()
        };

        CartesianGridLineAnnotation endLineAnnotation = new CartesianGridLineAnnotation()
        {
            StrokeThickness = 6,
            IsHitTestVisible = true,
            Stroke = new SolidColorBrush()
        };

        double minimumThicknessLineAnnotation = 6;

        RadCartesianChart radCartesianChart;

        public ToolTip ToolTip { get; private set; }

        TimelineItemsSegment rangeEachPixel = new TimelineItemsSegment();

        public event EventHandler<EventArgs> CloseButtonClicked;
        protected void OnCloseButtonClicked()
        {
            CloseButtonClicked?.Invoke(this, new EventArgs());
        }

        internal event EventHandler<FilterWindowStrokeMouseButtonEventArgs> StrokeMouseLeftButtonDown;
        internal void OnStrokeMouseLeftButtonDown(FilterWindowStrokeMouseButtonEventArgs args)
        {
            StrokeMouseLeftButtonDown?.Invoke(this, args);
        }

        internal event EventHandler<FilterWindowStrokeMouseEventArgs> MouseEnter;
        internal void OnMouseEnter(FilterWindowStrokeMouseEventArgs args)
        {
            MouseEnter?.Invoke(this, args);
        }

        internal event EventHandler<FilterWindowStrokeMouseEventArgs> MouseLeave;
        internal void OnMouseLeave(FilterWindowStrokeMouseEventArgs args)
        {
            MouseLeave?.Invoke(this, args);
        }

        public event EventHandler<MouseEventArgs> MouseMove;
        protected void OnMouseMove(MouseEventArgs args)
        {
            MouseMove?.Invoke(this, args);
        }

        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonDown;
        protected void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            MouseLeftButtonDown?.Invoke(this, args);
        }

        readonly Button btnClose;


        public FilterWindow()
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            lblContentToolTip.Style = ToolTipStyle;

            ToolTip = new ToolTip()
            {
                Content = lblContentToolTip,
                Style = null,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
            };

            this.RangeChanged += FilterWindow_RangeChanged;
            Fill = new LinearGradientBrush(new GradientStopCollection(new List<GradientStop>() {
                new GradientStop(theme.PrimaryMid.Color, 0),
                new GradientStop(Color.FromArgb(15,0,0,0), 1),
            }), 90);

            mainAnnotation.ToolTip = ToolTip;
            mainAnnotation.MouseLeftButtonDown += MainAnnotation_MouseLeftButtonDown;
            mainAnnotation.MouseLeave += MainAnnotation_MouseLeave;
            mainAnnotation.MouseEnter += MainAnnotation_MouseEnter;
            mainAnnotation.MouseMove += MainAnnotation_MouseMove;

            startLineAnnotation.ToolTip = ToolTip;
            startLineAnnotation.MouseLeftButtonDown += StartLineAnnotation_MouseLeftButtonDown;
            startLineAnnotation.MouseLeave += StartLineAnnotation_MouseLeave;
            startLineAnnotation.MouseEnter += StartLineAnnotation_MouseEnter;
            startLineAnnotation.MouseMove += StartLineAnnotation_MouseMove;

            endLineAnnotation.ToolTip = ToolTip;
            endLineAnnotation.MouseLeftButtonDown += EndLineAnnotation_MouseLeftButtonDown;
            endLineAnnotation.MouseLeave += EndLineAnnotation_MouseLeave;
            endLineAnnotation.MouseEnter += EndLineAnnotation_MouseEnter;
            endLineAnnotation.MouseMove += EndLineAnnotation_MouseMove;

            btnClose = new Button()
            {
                Padding = new Thickness(0),
                Margin = new Thickness(-8, -5, 0, 0),
                BorderBrush = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                ToolTip = "Close filter",
                Foreground = new SolidColorBrush(theme.Body),
                Content = new PackIcon
                {
                    Kind = PackIconKind.Close,
                    Background = Brushes.Transparent,
                    Foreground = Brushes.Red,
                    Width=16,Height=16,
                }
            };

            closeAnnotation.Content = btnClose;
            btnClose.MouseEnter += BtnClose_MouseEnter;
            btnClose.Click += BtnClose_Click;
            btnClose.MouseLeave += BtnClose_MouseLeave;
            btnClose.MouseMove += BtnClose_MouseMove;
            SetBindingsProperties();
        }

        private void BtnClose_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void EndLineAnnotation_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void StartLineAnnotation_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void MainAnnotation_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void BtnClose_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseButtonClicked();
        }

        private void BtnClose_MouseEnter(object sender, MouseEventArgs e)
        {
            btnClose.Cursor = Cursors.Hand;
            MouseEnterInvoker(e);
        }

        private void EndLineAnnotation_MouseEnter(object sender, MouseEventArgs e)
        {
            endLineAnnotation.Cursor = Cursors.SizeWE;
            MouseEnterInvoker(e);
        }

        private void EndLineAnnotation_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void EndLineAnnotation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnStrokeMouseLeftButtonDown(new FilterWindowStrokeMouseButtonEventArgs(FilterWindowStrokeMouseEventState.End,
                                                                            e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice));
        }

        private void StartLineAnnotation_MouseEnter(object sender, MouseEventArgs e)
        {
            startLineAnnotation.Cursor = Cursors.SizeWE;
            MouseEnterInvoker(e);
        }

        private void StartLineAnnotation_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void StartLineAnnotation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnStrokeMouseLeftButtonDown(new FilterWindowStrokeMouseButtonEventArgs(FilterWindowStrokeMouseEventState.Start,
                                                                            e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice));
        }

        private void MainAnnotation_MouseEnter(object sender, MouseEventArgs e)
        {
            mainAnnotation.Cursor = Cursors.SizeAll;
            MouseEnterInvoker(e);
        }

        private void MainAnnotation_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void MouseEnterInvoker(MouseEventArgs e)
        {
            var dp = radCartesianChart.ConvertPointToData(e.GetPosition(radCartesianChart));
            DateTime hVal = (DateTime)dp.FirstValue;

            if (hVal >= From && hVal <= To)
            {
                if (hVal <= To)
                {
                    OnMouseEnter(new FilterWindowStrokeMouseEventArgs(FilterWindowStrokeMouseEventState.End,
                        e.MouseDevice, e.Timestamp, e.StylusDevice));
                }
                else if (hVal >= From)
                {
                    OnMouseEnter(new FilterWindowStrokeMouseEventArgs(FilterWindowStrokeMouseEventState.Start,
                        e.MouseDevice, e.Timestamp, e.StylusDevice));
                }
            }
        }

        private void MouseLeaveInvoker(MouseEventArgs e)
        {
            var dp = radCartesianChart.ConvertPointToData(e.GetPosition(radCartesianChart));
            DateTime hVal = (DateTime)dp.FirstValue;

            if (hVal < From || hVal > To)
            {
                if (hVal > To)
                {
                    OnMouseLeave(new FilterWindowStrokeMouseEventArgs(FilterWindowStrokeMouseEventState.End,
                        e.MouseDevice, e.Timestamp, e.StylusDevice));
                }
                else if (hVal < From)
                {
                    OnMouseLeave(new FilterWindowStrokeMouseEventArgs(FilterWindowStrokeMouseEventState.Start,
                        e.MouseDevice, e.Timestamp, e.StylusDevice));
                }
            }
        }

        private void MainAnnotation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
        }

        private void FilterWindow_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            if (From > To)
            {
                var t = From;
                From = To;
                To = t;
                return;
            }

            SetCloseButtonLocation();

            lblContentToolTip.Content = From.ToString("ddd dd, MMM. yyyy") + "\n" + To.ToString("ddd dd, MMM. yyyy");
        }

        private void SetCloseButtonLocation()
        {
            double verticalValue = VerticalAxis.Maximum;
            closeAnnotation.VerticalValue = verticalValue;

            long ticks = (long)(To.Ticks - (10.5 * rangeEachPixel.Duration.Ticks));
            if (ticks >= Utility.MinValue.Ticks && ticks <= Utility.MaxValue.Ticks)
                closeAnnotation.HorizontalValue = new DateTime(ticks);
        }

        public void AddAnnotationsToChart(PresenterCollection<CartesianChartAnnotation> annotations)
        {
            if (!annotations.Contains(mainAnnotation))
            {
                annotations.Add(mainAnnotation);

                if (!annotations.Contains(startLineAnnotation))
                    annotations.Add(startLineAnnotation);

                if (!annotations.Contains(endLineAnnotation))
                    annotations.Add(endLineAnnotation);

                if (!annotations.Contains(closeAnnotation))
                {
                    SetCloseButtonLocation();
                    annotations.Add(closeAnnotation);
                }
            }
        }

        internal void RemoveAnnotationsToChart(PresenterCollection<CartesianChartAnnotation> annotations)
        {
            if (annotations.Contains(mainAnnotation))
            {
                annotations.Remove(mainAnnotation);

                if (annotations.Contains(startLineAnnotation))
                    annotations.Remove(startLineAnnotation);

                if (annotations.Contains(endLineAnnotation))
                    annotations.Remove(endLineAnnotation);

                if (annotations.Contains(closeAnnotation))
                    annotations.Remove(closeAnnotation);
            }
        }

        private void CalculateBinEachPixel()
        {
            if (VerticalAxis == null || HorizontalAxis == null || HorizontalAxis.Chart == null || VerticalAxis.Chart == null || HorizontalAxis.Chart != VerticalAxis.Chart)
                return;

            DateTime center = Utility.CenterRange(HorizontalAxis.ActualVisibleRange.Minimum, HorizontalAxis.ActualVisibleRange.Maximum);
            Point p0 = radCartesianChart.ConvertDataToPoint(new DataTuple(center, 0));
            Point p1 = new Point(p0.X + 1, p0.Y + 1);

            var d0 = radCartesianChart.ConvertPointToData(p0, radCartesianChart.HorizontalAxis, radCartesianChart.VerticalAxis);
            var d1 = radCartesianChart.ConvertPointToData(p1, radCartesianChart.HorizontalAxis, radCartesianChart.VerticalAxis);

            if (d0.FirstValue == null || d0.SecondValue == null || d1.FirstValue == null || d1.SecondValue == null)
            {
                return;
            }

            rangeEachPixel.From = (DateTime)(d0.FirstValue);
            rangeEachPixel.To = (DateTime)(d1.FirstValue);
            rangeEachPixel.FrequencyCount = Math.Abs((double)d1.SecondValue - (double)d0.SecondValue);
        }

        private void SetChart()
        {
            if (VerticalAxis == null || HorizontalAxis == null || HorizontalAxis.Chart == null || VerticalAxis.Chart == null || HorizontalAxis.Chart != VerticalAxis.Chart)
            {
                radCartesianChart = null;
            }
            else
            {
                radCartesianChart = VerticalAxis.Chart as RadCartesianChart;
                radCartesianChart.SizeChanged -= RadCartesianChart_SizeChanged;
                radCartesianChart.SizeChanged += RadCartesianChart_SizeChanged;
            }
        }

        private void RadCartesianChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateBinEachPixel();

            SetCloseButtonLocation();
        }

        internal bool HasAnnotation(CartesianChartAnnotation annotation)
        {
            if (annotation == mainAnnotation || annotation == closeAnnotation || annotation == endLineAnnotation || annotation == startLineAnnotation)
                return true;
            return false;
        }
    }
}
