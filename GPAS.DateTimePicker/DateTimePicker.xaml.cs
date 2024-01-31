using GPAS.Ontology;
using GPAS.PropertiesValidation;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.DateTimePicker
{
    /// <summary>
    /// Interaction logic for DateTimePicker.xaml
    /// </summary>
    public partial class DateTimePicker
    {
        #region dependenciesproperties
        public Brush NewBackground
        {
            get { return (Brush)GetValue(NewBackgroundProperty); }
            set { SetValue(NewBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewBackgroundProperty =
            DependencyProperty.Register(nameof(NewBackground), typeof(Brush), typeof(DateTimePicker),
                new PropertyMetadata(Brushes.Black));



        public Brush NewForeground
        {
            get { return (Brush)GetValue(NewForegroundProperty); }
            set { SetValue(NewForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewForegroundProperty =
            DependencyProperty.Register(nameof(NewForeground), typeof(Brush), typeof(DateTimePicker),
                new PropertyMetadata(Brushes.Black));

        public DayOfWeek FirstDayOfWeek
        {
            get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
            set => SetValue(FirstDayOfWeekProperty, value);
        }

        // Using a DependencyProperty as the backing store for FirstDayOfWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstDayOfWeekProperty =
            DependencyProperty.Register(nameof(FirstDayOfWeek), typeof(DayOfWeek), typeof(DateTimePicker),
                new PropertyMetadata(DayOfWeek.Sunday));

        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        // Using a DependencyProperty as the backing store for FirstDayOfWeek.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register(nameof(Hint), typeof(string), typeof(DateTimePicker),
                new PropertyMetadata(string.Empty));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DateTimePicker),
                new PropertyMetadata(false, OnSetIsOpenChanged));

        private static void OnSetIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePicker)d).OnSetIsOpenChanged(e);
        }

        private void OnSetIsOpenChanged(DependencyPropertyChangedEventArgs e)
        {

            if (IsOpen)
            {
                OpenDialog();
                OnOpened();
            }
            else
            {
                CloseDialog();
                OnClosed();
            }
        }


        internal bool InternalDiplayErorrMessage
        {
            get { return (bool)GetValue(InternalDiplayErorrMessageProperty); }
            set { SetValue(InternalDiplayErorrMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InternalDiplayErorrMessage.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty InternalDiplayErorrMessageProperty =
            DependencyProperty.Register(nameof(InternalDiplayErorrMessage), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true));

        public bool DiplayErrorMessage
        {
            get => (bool)GetValue(DiplayErrorMessageProperty);
            set => SetValue(DiplayErrorMessageProperty, value);
        }

        // Using a DependencyProperty as the backing store for DiplayErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DiplayErrorMessageProperty =
            DependencyProperty.Register(nameof(DiplayErrorMessage), typeof(bool), typeof(DateTimePicker), new PropertyMetadata(true, OnSetDiplayErrorMessageChanged));

        private static void OnSetDiplayErrorMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePicker)d).OnSetDiplayErrorMessageChanged(e);
        }

        private void OnSetDiplayErrorMessageChanged(DependencyPropertyChangedEventArgs e)
        {
            SetInternalDiplayErorrMessage();
        }

        public ValidationProperty IsValid
        {
            get => (ValidationProperty)GetValue(IsValidProperty);
            protected set => SetValue(IsValidProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(nameof(IsValid), typeof(ValidationProperty), typeof(DateTimePicker), new PropertyMetadata(null, OnSetIsValidChanged));

        private static void OnSetIsValidChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePicker)d).OnSetIsValidChanged(e);
        }

        private void OnSetIsValidChanged(DependencyPropertyChangedEventArgs e)
        {
            SetInternalDiplayErorrMessage();
            OnIsValidChanged(e);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(DateTimePicker),
                new PropertyMetadata(string.Empty, OnSetTextChanged));

        private static void OnSetTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePicker)d).OnSetTextChanged(e);
        }

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            if (GetValueValidation(Text, CultureInfo.CurrentCulture).Status != ValidationStatus.Invalid)
            {
                ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.DateTime, Text, out var selectedDateTime,
                    CultureInfo.CurrentCulture);
                SelectedDateTime = (DateTime)selectedDateTime;
            }
            else if (string.IsNullOrEmpty(Text))
            {
                SelectedDateTime = null;
            }

            SetValidation(Text, CultureInfo.CurrentCulture);
            OnTextChanged(e);
        }

        public DateTime? SelectedDateTime
        {
            get => (DateTime?)GetValue(SelectedDateTimeProperty);
            set => SetValue(SelectedDateTimeProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedDateTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedDateTimeProperty =
            DependencyProperty.Register(nameof(SelectedDateTime), typeof(DateTime?), typeof(DateTimePicker),
                new PropertyMetadata(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), OnSetSelectedDateTimeChanged));

        private static void OnSetSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePicker)d).OnSetSelectedDateTimeChanged(e);
        }

        private void OnSetSelectedDateTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedDateTime == null)
            {
                Text = string.Empty;
            }
            else
            {
                Text = SelectedDateTime?.ToString(CultureInfo.CurrentCulture);
            }

            SetValidation(SelectedDateTime?.ToString(CultureInfo.CurrentCulture), CultureInfo.CurrentCulture);
            OnSelectedDateTimeChanged(e);
        }


        #endregion

        #region Methods

        public DateTimePicker()
        {
            InitializeComponent();
        }

        private void SetValidation(string dateTimeValue, CultureInfo cultureInfo)
        {
            IsValid = GetValueValidation(dateTimeValue, cultureInfo);
        }

        private ValidationProperty GetValueValidation(string dateTimeValue, CultureInfo cultureInfo)
        {
            return ValueBaseValidation.IsValidPropertyValue(BaseDataTypes.DateTime, dateTimeValue, cultureInfo);
        }

        private void SetInternalDiplayErorrMessage()
        {
            InternalDiplayErorrMessage = DiplayErrorMessage && IsValid.Status != ValidationStatus.Valid;
        }

        private void CombinedDialogOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
        {
            CombinedCalendar.SelectedDate = SelectedDateTime;

            if (SelectedDateTime != null)
                CombinedClock.Time = (DateTime)SelectedDateTime;
        }

        private void CombinedDialogClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            if (Equals(eventArgs.Parameter, "DateTimePickerTrue"))
            {
                DateTime selectedDate = CombinedCalendar.SelectedDate == null ?
                    CombinedCalendar.DisplayDate :
                    CombinedCalendar.SelectedDate.Value;

                var date = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day);
                var combined = date.AddSeconds(CombinedClock.Time.TimeOfDay.TotalSeconds);
                SelectedDateTime = combined;
            }
        }

        private void RunButtonCommand(Button owner)
        {
            if (owner.IsEnabled)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(owner);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv?.Invoke();
            }
        }

        private void CloseDialog()
        {
            RunButtonCommand(CancelButton);
        }

        private void OpenDialog()
        {
            RunButtonCommand(OpenDialogButton);
        }

        #endregion

        #region Events

        public event EventHandler Closed;
        protected void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Opened;
        protected void OnOpened()
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        public event DependencyPropertyChangedEventHandler SelectedDateTimeChanged;
        protected void OnSelectedDateTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectedDateTimeChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler TextChanged;
        protected void OnTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler IsValidChanged;
        protected void OnIsValidChanged(DependencyPropertyChangedEventArgs e)
        {
            IsValidChanged?.Invoke(this, e);
        }

        #endregion
    }
}
