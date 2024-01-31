using GMap.NET;
using System.Collections.Generic;

namespace GPAS.MapViewer
{
    public class PathDrawnEventArgs
    {
        public List<PointLatLng> Points = new List<PointLatLng>();
        public double LengthInMeters = double.NaN;
        public double WidthInMeters = double.NaN;
        public List<List<PointLatLng>> InnerPolygons = new List<List<PointLatLng>>();
    }
}