using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class DataSourceTypeToVisibilityControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type previewType = GetPreviewType(value);

            return IsSameType(previewType, parameter as Type) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool IsSameType(Type type1, Type type2)
        {
            if (type2 == null)
            {
                return type1 == null;
            }
            else
            {
                if (type1 == type2)
                    return true;
                else
                {
                    if (type1 == null)
                        return false;
                    else
                        return IsSameType(type1.BaseType, type2);
                }
            }
        }

        private Type GetPreviewType(object value)
        {
            if (value == null)
                return null;
            if (value is IPreviewableDataSource<DataTable> tablePreview)
                return tablePreview.Preview?.GetType();
            if (value is IPreviewableDataSource<string> stringPreview)
                return stringPreview.Preview?.GetType();
            return (value as IPreviewableDataSource<ImageSource>)?.Preview?.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
