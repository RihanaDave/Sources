using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace GPAS.ObjectExplorerHistogramViewer.Convertors
{
    // از کلاس زیر برای مقداردهی طول یا عرض یک عضو از واسط کاربری به مقدار بخشی از 
    // عضو دیگر استفاده می شود
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val, par;
            double.TryParse(value.ToString(), NumberStyles.Float, culture, out val);
            double.TryParse(parameter.ToString(), NumberStyles.Float, culture, out par);

            return val * par;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }
}
