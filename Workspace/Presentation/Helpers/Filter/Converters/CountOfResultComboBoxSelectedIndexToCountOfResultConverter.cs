using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Filter.Converters
{
    public class CountOfResultComboBoxSelectedIndexToCountOfResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedIndex = 0;
            int countOfResult = 10;

            if (int.TryParse(value.ToString(), out countOfResult))
            {
                switch (countOfResult)
                {
                    case 30:
                        selectedIndex = 1;
                        break;
                    case 50:
                        selectedIndex = 2;
                        break;
                    case 100:
                        selectedIndex = 3;
                        break;
                    case 500:
                        selectedIndex = 4;
                        break;
                    case 10:
                    default:
                        selectedIndex = 0;
                        break;
                }
            }

            return selectedIndex;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selectedIndex = 0;
            int countOfResult = 10;
            if (int.TryParse(value.ToString(), out selectedIndex))
            {
                switch (selectedIndex)
                {
                    case 1:
                        countOfResult = 30;
                        break;
                    case 2:
                        countOfResult = 50;
                        break;
                    case 3:
                        countOfResult = 100;
                        break;
                    case 4:
                        countOfResult = 500;
                        break;
                    case 0:
                    default:
                        countOfResult = 10;
                        break;
                }
            }

            return countOfResult;
        }
    }
}
