using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.MapViewer
{
    public class CrossPoint
    {
        public CrossPoint(LineLatLng line1, LineLatLng line2)
        {
            Line1 = line1;
            Line2 = line2;
        }

        PointLatLng point;
        LineLatLng line1, line2;

        public LineLatLng Line1
        {
            get { return line1; }
            set
            {
                line1 = value;
                point = SetCrossPoint();
            }
        }

        public LineLatLng Line2
        {
            get { return line2; }
            set
            {
                line2 = value;
                point = SetCrossPoint();
            }
        }

        public VertexLatLng Vertex { get { return new VertexLatLng(point); } }

        private PointLatLng SetCrossPoint()
        {
            if (Line1 != null)
                return Line1.CrossPoint(Line2);
            else
                return PointLatLng.Empty;
        }

        /// <summary>
        /// نقطه تقاطع دو خط با هم را می یابد.
        /// </summary>
        /// <param name="line1">خط اول</param>
        /// <param name="line2">خط دوم</param>
        /// <returns>نقطه تقاطع</returns>
        public static PointLatLng FindCrossPoint(LineLatLng line1, LineLatLng line2)
        {
            if (line1 != null)
                return line1.CrossPoint(line2);
            else
                return PointLatLng.Empty;
        }
    }
}
