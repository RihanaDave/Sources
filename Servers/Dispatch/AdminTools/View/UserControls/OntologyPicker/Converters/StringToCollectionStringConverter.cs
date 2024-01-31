using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.UserControls.OntologyPicker.Converters
{
    public class StringToCollectionStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return new ObservableCollection<string>
                {
                    value.ToString()
                };

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
