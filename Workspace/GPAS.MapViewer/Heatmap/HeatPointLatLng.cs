using GMap.NET;

namespace GPAS.MapViewer
{
    public class HeatPointLatLng
    {
        public PointLatLng Point;
        public double Intensity;

        public HeatPointLatLng(PointLatLng point, double intensity)
        {
            Point = point;
            Intensity = intensity;
        }

        public override int GetHashCode()
        {
            return Point.Lat.GetHashCode() ^ Point.Lng.GetHashCode() ^ Intensity.GetHashCode();
        }
    }
}
