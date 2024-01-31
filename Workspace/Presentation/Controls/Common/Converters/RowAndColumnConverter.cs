using GPAS.Workspace.Presentation.Controls.Waiting;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class RowAndColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                throw new ArgumentNullException();

            switch ((ElementType)parameter)
            {
                case ElementType.Row:
                    return CalculatingRow((Dock)value);
                case ElementType.Column:
                    return CalculatingColumn((Dock)value);
                default:
                    return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private int CalculatingRow(Dock position)
        {
            switch (position)
            {
                case Dock.Bottom:
                    return 2;
                case Dock.Top:
                    return 0;
                case Dock.Left:
                case Dock.Right:
                    return 1;
                default:
                    return 2;
            }
        }

        private int CalculatingColumn(Dock position)
        {
            switch (position)
            {
                case Dock.Bottom:
                case Dock.Top:
                    return 1;
                case Dock.Left:
                    return 0;
                case Dock.Right:
                    return 2;
                default:
                    return 0;
            }
        }
    }
}
