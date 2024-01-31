using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.Waiting
{
    /// <summary>
    /// Interaction logic for WaitingControl.xaml
    /// </summary>
    public partial class WaitingControl
    {
        public WaitingControl()
        {
            InitializeComponent();
        }

        private int tasksCount;

        public int TasksCount
        {
            protected set
            {
                tasksCount = value < 0 ? 0 : value;

                Visibility = tasksCount == 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            get => tasksCount;
        }

        #region DependencyProperties

        /// <summary>
        /// متن پیام
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(WaitingControl),
                new PropertyMetadata(string.Empty, OnSetMessagePropertyChanged));

        private static void OnSetMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetMessagePropertyChanged(e);
        }

        private void OnSetMessagePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            OnMessageChanged(e);
        }

        /// <summary>
        /// اندازه قلم پیام
        /// </summary>
        public double MessageFontSize
        {
            get => (double)GetValue(MessageFontSizeProperty);
            set => SetValue(MessageFontSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageFontSizeProperty =
            DependencyProperty.Register(nameof(MessageFontSize), typeof(double), typeof(WaitingControl),
                new PropertyMetadata(14.0, OnSetMessageFontSizePropertyChanged));

        private static void OnSetMessageFontSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetMessageFontSizePropertyChanged(e);
        }

        private void OnSetMessageFontSizePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            OnMessageFontSizeChanged(e);
        }

        /// <summary>
        /// رنگ قلم پیام
        /// </summary>
        public Brush MessageForeground
        {
            get => (Brush)GetValue(MessageForegroundProperty);
            set => SetValue(MessageForegroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageForegroundProperty =
            DependencyProperty.Register(nameof(MessageForeground), typeof(Brush), typeof(WaitingControl),
                new PropertyMetadata(Brushes.Black, OnSetMessageForegroundPropertyChanged));

        private static void OnSetMessageForegroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetMessageForegroundPropertyChanged(e);
        }

        private void OnSetMessageForegroundPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            OnMessageForegroundChanged(e);
        }

        /// <summary>
        /// محل قرار‌گیری پیام نسبت به کنترل در حال چرخش
        /// </summary>
        public Dock MessagePosition
        {
            get => (Dock)GetValue(MessagePositionProperty);
            set => SetValue(MessagePositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessagePositionProperty =
            DependencyProperty.Register(nameof(MessagePosition), typeof(Dock), typeof(WaitingControl),
                new PropertyMetadata(Dock.Bottom, OnSetMessagePositionPropertyChanged));

        private static void OnSetMessagePositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetMessagePositionPropertyChanged(e);
        }

        private void OnSetMessagePositionPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            OnMessagePositionChanged(e);
        }

        /// <summary>
        /// اندازه کنترل دایره‌ای در حال چرخش
        /// </summary>
        public double ProgressSize
        {
            get => (double)GetValue(ProgressSizeProperty);
            set => SetValue(ProgressSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressSizeProperty =
            DependencyProperty.Register(nameof(ProgressSize), typeof(double), typeof(WaitingControl),
                new PropertyMetadata(50.0, OnSetProgressSizePropertyChanged));

        private static void OnSetProgressSizePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetProgressSizePropertyChanged(e);
        }

        private void OnSetProgressSizePropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            OnProgressSizeChanged(e);
        }

        /// <summary>
        /// رنگ پس زمینه کنترل
        /// </summary>
        public Brush ProgressBoxBackground
        {
            get => (Brush)GetValue(ProgressBoxBackgroundProperty);
            set => SetValue(ProgressBoxBackgroundProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressBoxBackgroundProperty =
            DependencyProperty.Register(nameof(ProgressBoxBackground), typeof(Brush), typeof(WaitingControl),
                new PropertyMetadata(Brushes.Transparent, OnSetProgressBoxBackgroundPropertyChanged));

        private static void OnSetProgressBoxBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetProgressBoxBackgroundPropertyChanged(e);
        }

        private void OnSetProgressBoxBackgroundPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            OnProgressBoxBackgroundChanged(e);
        }

        public Thickness ProgressMargin
        {
            get => (Thickness)GetValue(ProgressMarginProperty);
            set => SetValue(ProgressMarginProperty, value);
        }

        public static readonly DependencyProperty ProgressMarginProperty = DependencyProperty.Register(nameof(ProgressMargin),
            typeof(Thickness), typeof(WaitingControl), new PropertyMetadata(new Thickness(5)));

        public Thickness MessageMargin
        {
            get => (Thickness)GetValue(MessageMarginProperty);
            set => SetValue(MessageMarginProperty, value);
        }

        public static readonly DependencyProperty MessageMarginProperty = DependencyProperty.Register(nameof(MessageMargin),
            typeof(Thickness), typeof(WaitingControl), new PropertyMetadata(new Thickness(5)));

        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
        }

        public static readonly DependencyProperty BackgroundOpacityProperty =
            DependencyProperty.Register(nameof(BackgroundOpacity), typeof(double), typeof(WaitingControl), new PropertyMetadata(0.7));



        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsIndeterminate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(WaitingControl), new PropertyMetadata(true));



        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(WaitingControl),
                new PropertyMetadata(100.0, OnSetMaxValueChanged));

        private static void OnSetMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetMaxValueChanged(e);
        }

        private void OnSetMaxValueChanged(DependencyPropertyChangedEventArgs e)
        {
            PercentageValue = CalculatePercentageValue(ProgressValue, MaxValue);
        }

        public double ProgressValue
        {
            get { return (double)GetValue(ProgressValueProperty); }
            set { SetValue(ProgressValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(WaitingControl),
                new PropertyMetadata(0.0, OnSetProgressValueChanged));

        private static void OnSetProgressValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WaitingControl)d).OnSetProgressValueChanged(e);
        }

        private void OnSetProgressValueChanged(DependencyPropertyChangedEventArgs e)
        {
            PercentageValue = CalculatePercentageValue(ProgressValue, MaxValue);
        }

        public ProgressValueDisplayMode ProgressValueDisplayMode
        {
            get { return (ProgressValueDisplayMode)GetValue(ProgressValueDisplayModeProperty); }
            set { SetValue(ProgressValueDisplayModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressDisplayMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressValueDisplayModeProperty =
            DependencyProperty.Register(nameof(ProgressValueDisplayMode), typeof(ProgressValueDisplayMode), typeof(WaitingControl),
                new PropertyMetadata(ProgressValueDisplayMode.Percentage));



        protected double PercentageValue
        {
            get { return (double)GetValue(PercentageValueProperty); }
            set { SetValue(PercentageValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PercentageValue.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty PercentageValueProperty =
            DependencyProperty.Register(nameof(PercentageValue), typeof(double), typeof(WaitingControl), new PropertyMetadata(0.0));



        public ProgressShape ProgressShape
        {
            get { return (ProgressShape)GetValue(ProgressShapeProperty); }
            set { SetValue(ProgressShapeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressShape.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressShapeProperty =
            DependencyProperty.Register(nameof(ProgressShape), typeof(ProgressShape), typeof(WaitingControl),
                new PropertyMetadata(ProgressShape.Circular));



        #endregion

        #region Events

        public event DependencyPropertyChangedEventHandler MessageChanged;
        private void OnMessageChanged(DependencyPropertyChangedEventArgs e)
        {
            MessageChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler MessageFontSizeChanged;
        private void OnMessageFontSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            MessageFontSizeChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler MessageForegroundChanged;
        private void OnMessageForegroundChanged(DependencyPropertyChangedEventArgs e)
        {
            MessageForegroundChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler MessagePositionChanged;
        private void OnMessagePositionChanged(DependencyPropertyChangedEventArgs e)
        {
            MessagePositionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler ProgressSizeChanged;
        private void OnProgressSizeChanged(DependencyPropertyChangedEventArgs e)
        {
            ProgressSizeChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler ProgressBoxBackgroundChanged;
        private void OnProgressBoxBackgroundChanged(DependencyPropertyChangedEventArgs e)
        {
            ProgressBoxBackgroundChanged?.Invoke(this, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// افزایش یک واحدی کار‌ها
        /// و فعال‌سازی قابلیت مشاهده کنترل
        /// </summary>
        public void TaskIncrement()
        {
            TasksCount++;
        }

        /// <summary>
        /// کاهش یک واحد از کارها
        /// حال اگر تعداد کار‌ها به صفر برسد
        /// قابلیت مشاهده کنترل غیر‌فعال می‌شود
        /// </summary>
        public void TaskDecrement()
        {
            TasksCount--;
        }

        private double CalculatePercentageValue(double numericValue, double maxValue, int decimals = 2)
        {
            if (MaxValue == 0)
                return 0;

            return Math.Round(100 * numericValue / maxValue, decimals);
        }

        #endregion
    }
}
