using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;

namespace GPAS.BarChartViewer
{
    public class SelectionRange : Range
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

           

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.FromProperty, new Binding("Start")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            startLineAnnotation.SetBinding(CartesianGridLineAnnotation.ValueProperty, new Binding("Start")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            mainAnnotation.SetBinding(CartesianPlotBandAnnotation.ToProperty, new Binding("End")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            endLineAnnotation.SetBinding(CartesianGridLineAnnotation.ValueProperty, new Binding("End")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(SelectionRange), new PropertyMetadata(null));

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(SelectionRange), new PropertyMetadata(null));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(SelectionRange), new PropertyMetadata(1.0, OnSetStrokeThicknessChanged));

        private static void OnSetStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectionRange).OnSetStrokeThicknessChanged(e);
        }

        private void OnSetStrokeThicknessChanged(DependencyPropertyChangedEventArgs e)
        {
            startLineAnnotation.StrokeThickness = StrokeThickness + 6;
            endLineAnnotation.StrokeThickness = StrokeThickness + 6;
        }

        public LinearAxis HorizontalAxis
        {
            get { return (LinearAxis)GetValue(HorizontalAxisProperty); }
            set { SetValue(HorizontalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalAxisProperty =
            DependencyProperty.Register("HorizontalAxis", typeof(LinearAxis), typeof(SelectionRange), new PropertyMetadata(null, OnSetHorizontalAxisChanged));

        private static void OnSetHorizontalAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectionRange).OnSetHorizontalAxisChanged(e);
        }

        private void OnSetHorizontalAxisChanged(DependencyPropertyChangedEventArgs e)
        {
            SetChart();
            CalculateSizeEachValue();
        }

        public NumericalAxis VerticalAxis
        {
            get { return (NumericalAxis)GetValue(VerticalAxisProperty); }
            set { SetValue(VerticalAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalAxisProperty =
            DependencyProperty.Register("VerticalAxis", typeof(NumericalAxis), typeof(SelectionRange), new PropertyMetadata(null, OnSetVerticalAxisChanged));

        private static void OnSetVerticalAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectionRange).OnSetVerticalAxisChanged(e);
        }

        private void OnSetVerticalAxisChanged(DependencyPropertyChangedEventArgs e)
        {
            SetChart();
            CalculateSizeEachValue();
            SetCloseButtonLocation();
        }

        public bool IsHitTestVisibleForFill
        {
            get { return (bool)GetValue(IsHitTestVisibleForFillProperty); }
            set { SetValue(IsHitTestVisibleForFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHitTestVisibleForFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHitTestVisibleForFillProperty =
            DependencyProperty.Register("IsHitTestVisibleForFill", typeof(bool), typeof(SelectionRange), new PropertyMetadata(true));

        public bool IsHitTestVisibleForStroke
        {
            get { return (bool)GetValue(IsHitTestVisibleForStrokeProperty); }
            set { SetValue(IsHitTestVisibleForStrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsHitTestVisibleForStroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHitTestVisibleForStrokeProperty =
            DependencyProperty.Register("IsHitTestVisibleForStroke", typeof(bool), typeof(SelectionRange), new PropertyMetadata(true));

        public double MinimumLength
        {
            get { return (double)GetValue(MinimumLengthProperty); }
            set { SetValue(MinimumLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumLengthProperty =
            DependencyProperty.Register("MinimumLength", typeof(double), typeof(SelectionRange), new PropertyMetadata(0.0, OnSetMinimumLengthChanged));

        private static void OnSetMinimumLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectionRange).OnSetMinimumLengthChanged(e);
        }

        private void OnSetMinimumLengthChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        public Style ToolTipStyle
        {
            get { return (Style)GetValue(ToolTipStyleProperty); }
            set { SetValue(ToolTipStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToolTipStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolTipStyleProperty =
            DependencyProperty.Register("ToolTipStyle", typeof(Style), typeof(SelectionRange), new PropertyMetadata(null, OnSetToolTipStyleChanged));

        private static void OnSetToolTipStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectionRange).OnSetToolTipStyleChanged(e);
        }

        private void OnSetToolTipStyleChanged(DependencyPropertyChangedEventArgs e)
        {
            lblContentToolTip.Style = ToolTipStyle;
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SelectionRange), new PropertyMetadata(Orientation.Horizontal));


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

        RadCartesianChart radCartesianChart;


        Point sizeEachValue = new Point();
        ValueRangePair rangeEachPixel = new ValueRangePair();

        public ToolTip ToolTip { get; private set; }

        public event EventHandler<EventArgs> CloseButtonClicked;
        public void OnCloseButtonClicked()
        {
            CloseButtonClicked?.Invoke(this, new EventArgs());
        }

        public event EventHandler<SelectionRangeStrokeMouseButtonEventArgs> StrokeMouseLeftButtonDown;
        public void OnStrokeMouseLeftButtonDown(SelectionRangeStrokeMouseButtonEventArgs args)
        {
            StrokeMouseLeftButtonDown?.Invoke(this, args);
        }

        public event EventHandler<SelectionRangeStrokeMouseEventArgs> MouseLeave;
        public void OnMouseLeave(SelectionRangeStrokeMouseEventArgs args)
        {
            MouseLeave?.Invoke(this, args);
        }

        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonDown;
        public void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            MouseLeftButtonDown?.Invoke(this, args);
        }
        Button btnClose;
        public SelectionRange()
        {
            SetBindingsProperties();
            lblContentToolTip.Style = ToolTipStyle;
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            lblContentToolTip.Background = new SolidColorBrush(theme.CardBackground);

            ToolTip = new ToolTip()
            {
                Content = lblContentToolTip,
                Style = null,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
            };

            this.RangeChanged += SelectionRange_RangeChanged;
            Fill = new LinearGradientBrush(new GradientStopCollection(new List<GradientStop>() {
                new GradientStop(theme.PrimaryMid.Color, 0),
                new GradientStop(Color.FromArgb(15,0,0,0), 1),
            }), 90);

            mainAnnotation.ToolTip = ToolTip;
            mainAnnotation.MouseLeftButtonDown += MainAnnotation_MouseLeftButtonDown;
            mainAnnotation.MouseLeave += MainAnnotation_MouseLeave;
            mainAnnotation.MouseEnter += MainAnnotation_MouseEnter;

            startLineAnnotation.ToolTip = ToolTip;
            startLineAnnotation.MouseLeftButtonDown += StartLineAnnotation_MouseLeftButtonDown;
            startLineAnnotation.MouseLeave += StartLineAnnotation_MouseLeave;
            startLineAnnotation.MouseEnter += StartLineAnnotation_MouseEnter;

            endLineAnnotation.ToolTip = ToolTip;
            endLineAnnotation.MouseLeftButtonDown += EndLineAnnotation_MouseLeftButtonDown;
            endLineAnnotation.MouseLeave += EndLineAnnotation_MouseLeave;
            endLineAnnotation.MouseEnter += EndLineAnnotation_MouseEnter;

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
                    Width = 16,
                    Height = 16,
                }
            };

            btnClose.SetBinding(Button.IsHitTestVisibleProperty, new Binding("IsHitTestVisible")
            {
                Source = this,
                Mode = BindingMode.TwoWay
            });

            closeAnnotation.Content = btnClose;
            btnClose.MouseEnter += BtnClose_MouseEnter;
            btnClose.Click += BtnClose_Click;
            btnClose.MouseLeave += BtnClose_MouseLeave;
        }

        private void MainAnnotation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
        }

        private void MainAnnotation_MouseEnter(object sender, MouseEventArgs e)
        {
            mainAnnotation.Cursor = Cursors.SizeAll;
        }

        private void BtnClose_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void EndLineAnnotation_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void StartLineAnnotation_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void MainAnnotation_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeaveInvoker(e);
        }

        private void MouseLeaveInvoker(MouseEventArgs e)
        {
            var dp = radCartesianChart.ConvertPointToData(e.GetPosition(radCartesianChart));
            double hVal = (double)dp.FirstValue;

            if (hVal < Start || hVal > End)
            {
                if (hVal > End)
                {
                    OnMouseLeave(new SelectionRangeStrokeMouseEventArgs(SelectionRangeStrokeMouseEventState.End,
                        e.MouseDevice, e.Timestamp, e.StylusDevice));
                }
                else if (hVal < Start)
                {
                    OnMouseLeave(new SelectionRangeStrokeMouseEventArgs(SelectionRangeStrokeMouseEventState.Start,
                        e.MouseDevice, e.Timestamp, e.StylusDevice));
                }
            }
        }

        private void EndLineAnnotation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnStrokeMouseLeftButtonDown(new SelectionRangeStrokeMouseButtonEventArgs(SelectionRangeStrokeMouseEventState.End,
                                                                            e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice));
        }

        private void StartLineAnnotation_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnStrokeMouseLeftButtonDown(new SelectionRangeStrokeMouseButtonEventArgs(SelectionRangeStrokeMouseEventState.Start,
                                                                            e.MouseDevice, e.Timestamp, e.ChangedButton, e.StylusDevice));
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            OnCloseButtonClicked();
        }

        private void BtnClose_MouseEnter(object sender, MouseEventArgs e)
        {
            btnClose.Cursor = Cursors.Hand;
        }

        private void SelectionRange_RangeChanged(object sender, RangeChangedEventArgs e)
        {
            if (Start > End)
            {
                var t = Start;
                Start = End;
                End = t;
                return;
            }

            SetCloseButtonLocation();

            lblContentToolTip.Content = "[" + Math.Round(Start, 4) + ", " + Math.Round(End, 4) + "]";
            
        }

        private void SetCloseButtonLocation()
        {
            double verticalValue = VerticalAxis.Maximum;//((radCartesianChart.Grid.Clip as RectangleGeometry).Rect.Height / sizeEachValue.Y) - (0 * rangeEachPixel.Value);

            closeAnnotation.VerticalValue = verticalValue;

            closeAnnotation.HorizontalValue = End - (10.5 * rangeEachPixel.End);
        }

        private void EndLineAnnotation_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Orientation == Orientation.Horizontal)
                endLineAnnotation.Cursor = Cursors.SizeWE;
            else
                endLineAnnotation.Cursor = Cursors.SizeNS;
        }

        private void StartLineAnnotation_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Orientation == Orientation.Horizontal)
                startLineAnnotation.Cursor = Cursors.SizeWE;
            else
                startLineAnnotation.Cursor = Cursors.SizeNS;
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

        private void CalculateSizeEachValue()
        {
            if (VerticalAxis == null || HorizontalAxis == null || HorizontalAxis.Chart == null || VerticalAxis.Chart == null || HorizontalAxis.Chart != VerticalAxis.Chart)
                return;

            var d0 = radCartesianChart.ConvertPointToData(new Point(0, 0), radCartesianChart.HorizontalAxis, radCartesianChart.VerticalAxis);
            var d1 = radCartesianChart.ConvertPointToData(new Point(1, 1), radCartesianChart.HorizontalAxis, radCartesianChart.VerticalAxis);

            if (d0.FirstValue == null || d0.SecondValue == null || d1.FirstValue == null || d1.SecondValue == null)
            {
                return;
            }

            rangeEachPixel.Start = 0;
            rangeEachPixel.End = (double)d1.FirstValue - (double)d0.FirstValue;
            rangeEachPixel.Value = Math.Abs((double)d1.SecondValue - (double)d0.SecondValue);

            sizeEachValue.X = 1 / rangeEachPixel.End;
            sizeEachValue.Y = 1 / rangeEachPixel.Value;
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
            CalculateSizeEachValue();

            SetCloseButtonLocation();
        }
    }
}
