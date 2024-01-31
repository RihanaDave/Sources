﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Converters.ServersSynchronizationStatus
{
    public class BoolToRedEllipseColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if (!(bool)value)
                return Brushes.Red;
            else return Brushes.White;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
