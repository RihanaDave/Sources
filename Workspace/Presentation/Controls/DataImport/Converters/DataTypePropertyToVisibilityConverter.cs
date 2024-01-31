using GPAS.Ontology;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    /// <summary>
    /// اگر ویژگی از نوع تاریخ باشد در کنارش یک دکمه برای تغییر فرمت باید نمایش
    /// داده شود. این کلاس برای نمایش و عدم نمایش آن دکمه طراحی شده 
    /// </summary>
    public class DataTypePropertyToVisibilityConverter : IValueConverter
    {
        public BaseDataTypes[] VisibleDataTypes { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyMapModel property && VisibleDataTypes.Contains(property.DataType))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
