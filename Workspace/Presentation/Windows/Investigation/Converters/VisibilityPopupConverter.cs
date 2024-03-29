﻿using GPAS.Workspace.Presentation.Windows.Investigation.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.Investigation.Converters
{
   public class VisibilityPopupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (parameter != null && (ShowInvestigationPopupEnum)parameter == (ShowInvestigationPopupEnum)value))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}