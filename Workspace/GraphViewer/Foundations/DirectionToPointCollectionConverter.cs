using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class DirectionToPointCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection pointsBidirectional = new PointCollection(new List<Point>() {
                new Point(-10, 15),
                new Point(-10, 35),
                new Point(-50, 0),
                new Point(-10, -35),
                new Point(-10, -15),
                new Point(10, -15),
                new Point(10, -35),
                new Point(50, 0),
                new Point(10, 35),
                new Point(10, 15),
            }); //-10,15 -10,35 -50,0 -10,-35 -10,-15 10,-15 10,-35 50,0 10,35 10,15

            PointCollection pointsFromSourceToTarget = new PointCollection(new List<Point>() {
                new Point(-50, -15),
                new Point(10, -15),
                new Point(10, -35),
                new Point(50, 0),
                new Point(10, 35),
                new Point(10, 15),
                new Point(-50, 15),
            }); //-50,-15 10,-15 10,-35 50,0 10,35 10,15 -50,15

            PointCollection pointsFromTargetToSource = new PointCollection(new List<Point>() {
                new Point(50, 15),
                new Point(-10, 15),
                new Point(-10, 35),
                new Point(-50, 0),
                new Point(-10, -35),
                new Point(-10, -15),
                new Point(50, -15),
            }); //50,15 -10,15 -10,35 -50,0 -10,-35 -10,-15 50,-15

            if (value is EdgeDirection)
            {
                EdgeDirection direction = (EdgeDirection)value;

                if(direction == EdgeDirection.Bidirectional)
                {
                    return pointsBidirectional;
                }
                else if(direction == EdgeDirection.FromSourceToTarget)
                {
                    return pointsFromSourceToTarget;
                }
                else if (direction == EdgeDirection.FromTargetToSource)
                {
                    return pointsFromTargetToSource;
                }
            }

            return pointsBidirectional;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
