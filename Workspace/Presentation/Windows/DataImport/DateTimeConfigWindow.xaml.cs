using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.DataImport
{
    /// <summary>
    /// Interaction logic for DateTimeConfigWindow.xaml
    /// </summary>
    public partial class DateTimeConfigWindow
    {
        public List<TimeZoneInfo> TimeZoneInfoList { get; set; }

        public DateTimePropertyMapModel Property
        {
            get { return (DateTimePropertyMapModel)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Property.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.Register(nameof(Property), typeof(DateTimePropertyMapModel), typeof(DateTimeConfigWindow),
                new PropertyMetadata(null, OnSetPropertyChanged));

        private static void OnSetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimeConfigWindow)d).OnSetPropertyChanged(e);
        }

        private void OnSetPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (Property.Configuration.CultureName)
            {
                case "fa":
                    Calendar = CalendarType.Jalali;
                    break;
                case "ar":
                    Calendar = CalendarType.Hijri;
                    break;
                case "en":
                default:
                    Calendar = CalendarType.Gregorian;
                    break;
            }
        }

        public CalendarType Calendar
        {
            get { return (CalendarType)GetValue(CalendarProperty); }
            set { SetValue(CalendarProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Calendar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalendarProperty =
            DependencyProperty.Register(nameof(Calendar), typeof(CalendarType), typeof(DateTimeConfigWindow),
                new PropertyMetadata(CalendarType.Gregorian, OnSetCalendarChanged));

        private static void OnSetCalendarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimeConfigWindow)d).OnSetCalendarChanged(e);
        }

        private void OnSetCalendarChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (Calendar)
            {
                case CalendarType.Jalali:
                    Property.Configuration.CultureName = "fa";
                    break;
                case CalendarType.Hijri:
                    Property.Configuration.CultureName = "ar";
                    break;
                case CalendarType.Gregorian:
                default:
                    Property.Configuration.CultureName = "en";
                    break;
            }
        }

        public DateTimeConfigWindow(DateTimePropertyMapModel property)
        {
            InitializeComponent();
            Property = property;
            GetTimeZoneInfoList();
        }

        public void GetTimeZoneInfoList()
        {
            TimeZoneInfoList = TimeZoneInfo.GetSystemTimeZones().ToList();
        }

        /// <summary>
        /// حرکت دادن پنجره با ماوس
        /// </summary>
        /// <param name="sender"/>
        /// <param name="e"/>
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
