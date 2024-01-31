using GMap.NET;

namespace GPAS.MapViewer
{
    public class CircleDrawnEventArgs
    {
        public PointLatLng Center;
        public double RadiusInMeters = double.NaN;
    }
}