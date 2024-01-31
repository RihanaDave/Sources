using GMap.NET;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public class PolygonDrawnEventArgs
    {
        /// <summary>
        /// If atleast 3 points defined, "PerimeterInMeters" equals the perimeter of polygon shape in Meters scale; Otherwise equals double.NaN
        /// </summary>
        
        public List<PointLatLng> Points = new List<PointLatLng>();

        public double perimeterInMeters = double.NaN;
        public bool isAnyVectorCrossed = false;
        public bool isAnyVectorCoincident = false;
    }
}