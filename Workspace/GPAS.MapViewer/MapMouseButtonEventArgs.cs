using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public class MapMouseButtonEventArgs
    {
        public MapMouseButtonEventArgs(MouseButtonEventArgs mouseButtonEventArgs, PointLatLng point)
        {
            MouseButtonEventArgs = mouseButtonEventArgs;
            Point = point;
        }

        public MouseButtonEventArgs MouseButtonEventArgs { get; protected set; }
        public PointLatLng Point { get; protected set; }
    }
}
