using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.BarChartViewer.Converters
{
    public class OrientationToTextControlLayoutTransformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Orientation orientation = (Orientation)value;

            if (orientation == Orientation.Vertical)
            {
                return new TransformGroup()
                {
                    Children = new TransformCollection(new List<Transform>()
                    {
                        new ScaleTransform(){ ScaleY = -1 },
                    })
                };
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
