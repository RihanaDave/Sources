using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.BarChartViewer.Converters
{
    public class OrientationToAxisLabelTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Orientation orientation = (Orientation)value;

            if (orientation == Orientation.Vertical)
            {
                FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TextBlock));
                fef.SetBinding(TextBlock.TextProperty, new Binding());
                fef.SetValue(TextBlock.PaddingProperty, new Thickness(3));
                fef.SetValue(TextBlock.LayoutTransformProperty, new TransformGroup()
                {
                    Children = new TransformCollection(new List<Transform>()
                    {
                        new RotateTransform(90),
                        new ScaleTransform(){ ScaleY = -1 },
                    })
                });

                return new DataTemplate() { VisualTree = fef };
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
