using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    /// <summary>
    /// Interaction logic for DateFilterSearchControl.xaml
    /// </summary>
    public partial class DateFilterSearchControl : BaseFilterSearchControl
    {
        #region Dependencies

        public DateTime? StartDate
        {
            get { return (DateTime?)GetValue(StartDateProperty); }
            set { SetValue(StartDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartDateProperty =
            DependencyProperty.Register(nameof(StartDate), typeof(DateTime?), typeof(DateFilterSearchControl),
                new PropertyMetadata(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), OnSetStartDateChanged));

        private static void OnSetStartDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateFilterSearchControl)d).OnSetStartDateChanged(e);
        }

        private void OnSetStartDateChanged(DependencyPropertyChangedEventArgs e)
        {
            SetInternalValdationStatus();
            SetDateRangeCriteria();
            OnStartDateChanged();
        }

        public DateTime? EndDate
        {
            get { return (DateTime?)GetValue(EndDateProperty); }
            set { SetValue(EndDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndDateProperty =
            DependencyProperty.Register(nameof(EndDate), typeof(DateTime?), typeof(DateFilterSearchControl),
                new PropertyMetadata(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day), OnSetEndDateChanged));

        private static void OnSetEndDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateFilterSearchControl)d).OnSetEndDateChanged(e);
        }

        private void OnSetEndDateChanged(DependencyPropertyChangedEventArgs e)
        {
            SetInternalValdationStatus();
            SetDateRangeCriteria();
            OnEndDateChanged();
        }

        protected ValidationStatus ValidationStatusStartDate
        {
            get { return (ValidationStatus)GetValue(ValidationStatusStartDateProperty); }
            set { SetValue(ValidationStatusStartDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationStatusStartDate.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ValidationStatusStartDateProperty =
            DependencyProperty.Register(nameof(ValidationStatusStartDate), typeof(ValidationStatus), typeof(DateFilterSearchControl),
                new PropertyMetadata(ValidationStatus.Invalid, OnSetValidationStatusStartDateChanged));

        private static void OnSetValidationStatusStartDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateFilterSearchControl)d).OnSetValidationStatusStartDateChanged(e);
        }

        private void OnSetValidationStatusStartDateChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
        }

        protected ValidationStatus ValidationStatusEndDate
        {
            get { return (ValidationStatus)GetValue(ValidationStatusEndDateProperty); }
            set { SetValue(ValidationStatusEndDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationStatusEndDate.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ValidationStatusEndDateProperty =
            DependencyProperty.Register(nameof(ValidationStatusEndDate), typeof(ValidationStatus), typeof(DateFilterSearchControl),
                new PropertyMetadata(ValidationStatus.Invalid, OnSetValidationStatusEndDateChanged));

        private static void OnSetValidationStatusEndDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateFilterSearchControl)d).OnSetValidationStatusEndDateChanged(e);
        }

        private void OnSetValidationStatusEndDateChanged(DependencyPropertyChangedEventArgs e)
        {
            SetValidationStatus();
        }

        protected string ValidationMessageStartDate
        {
            get { return (string)GetValue(ValidationMessageStartDateProperty); }
            set { SetValue(ValidationMessageStartDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationMessageStartDate.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ValidationMessageStartDateProperty =
            DependencyProperty.Register(nameof(ValidationMessageStartDate), typeof(string), typeof(DateFilterSearchControl),
                new PropertyMetadata(Properties.Resources.InValid));



        protected string ValidationMessageEndDate
        {
            get { return (string)GetValue(ValidationMessageEndDateProperty); }
            set { SetValue(ValidationMessageEndDateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationMessageEndDate.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ValidationMessageEndDateProperty =
            DependencyProperty.Register(nameof(ValidationMessageEndDate), typeof(string), typeof(DateFilterSearchControl),
                new PropertyMetadata(Properties.Resources.InValid));

        #endregion

        #region Events
        public event EventHandler StartDateChanged;
        protected void OnStartDateChanged()
        {
            StartDateChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler EndDateChanged;
        protected void OnEndDateChanged()
        {
            EndDateChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Variables

        private DateRangeCriteria dateRangeCriteria = null;

        #endregion

        #region Methodes

        public DateFilterSearchControl()
        {
            InitializeComponent();
            Init();
        }

        protected override void AfterSetCriteriaBase()
        {
            dateRangeCriteria = CriteriaBase as DateRangeCriteria;
        }

        protected override void SetValidationStatus()
        {
            if (ValidationStatusStartDate == ValidationStatus.Invalid || ValidationStatusEndDate == ValidationStatus.Invalid)
                ValidationStatus = ValidationStatus.Invalid;
            else if (ValidationStatusStartDate == ValidationStatus.Warning || ValidationStatusEndDate == ValidationStatus.Warning)
                ValidationStatus = ValidationStatus.Warning;
            else
                ValidationStatus = ValidationStatus.Valid;

            ValidationMessage = ValidationMessageStartDate + "\n\n" + ValidationMessageEndDate;
        }

        private void Init()
        {
            DataContext = this;

            if (CriteriaBase == null)
            {
                CriteriaBase = new DateRangeCriteria();
            }
        }

        private void SetDateRangeCriteria()
        {
            if (ValidationStatusStartDate != ValidationStatus.Invalid)
            {
                dateRangeCriteria.StartTime = StartDate.ToString();
            }
            if (ValidationStatusEndDate != ValidationStatus.Invalid)
            {
                dateRangeCriteria.EndTime = EndDate.ToString();
            }
        }

        private void SetInternalValdationStatus()
        {
            object startValueObject, endValueObject;
            ValidationProperty startResultValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.DateTime, StartDate.ToString(), out startValueObject, CultureInfo.CurrentCulture);
            ValidationProperty endResultValidation = ValueBaseValidation.TryParsePropertyValue(Ontology.BaseDataTypes.DateTime, EndDate.ToString(), out endValueObject, CultureInfo.CurrentCulture);

            if (startResultValidation.Status != ValidationStatus.Invalid && endResultValidation.Status != ValidationStatus.Invalid)
            {
                if ((DateTime)startValueObject > (DateTime)endValueObject)
                {
                    startResultValidation.Status = ValidationStatus.Invalid;
                    startResultValidation.Message = Properties.Resources.Begin_date_must_be_smaller_than_end_date_;
                    endResultValidation.Status = ValidationStatus.Invalid;
                    endResultValidation.Message = Properties.Resources.End_date_must_be_greater_than_begin_date_;
                }
            }

            ValidationMessageStartDate = startResultValidation.Message;
            ValidationMessageEndDate = endResultValidation.Message;

            ValidationStatusStartDate = startResultValidation.Status;
            ValidationStatusEndDate = endResultValidation.Status;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveButtonClicked();
        }

        private void EndDatePicker_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        private void StartDatePicker_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsSelected = true;
        }

        private void StartDatePicker_IsValidChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ValidationMessageStartDate = ((ValidationProperty)e.NewValue).Message;
            ValidationStatusStartDate = ((ValidationProperty)e.NewValue).Status;
        }

        private void EndDatePicker_IsValidChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ValidationMessageEndDate = ((ValidationProperty)e.NewValue).Message;
            ValidationStatusEndDate = ((ValidationProperty)e.NewValue).Status;
        }

        #endregion   
    }
}
