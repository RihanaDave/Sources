using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.MapViewer
{
    public class CoincidentLine
    {
        public CoincidentLine(LineLatLng line1, LineLatLng line2)
        {
            Line1 = line1;
            Line2 = line2;
        }
        
        LineLatLng line1, line2, coincident;

        public LineLatLng Line1
        {
            get { return line1; }
            set
            {
                line1 = value;
                coincident = SetCoincidentLine();
            }
        }

        public LineLatLng Line2
        {
            get { return line2; }
            set
            {
                line2 = value;
                coincident = SetCoincidentLine();
            }
        }

        public LineLatLng Coincident { get { return coincident; } }

        private LineLatLng SetCoincidentLine()
        {
            return Line1?.CoincidentLine(Line2);
        }

        /// <summary>
        /// تطابق دو خط را با هم بررسی مس کند.
        /// </summary>
        /// <param name="line1">خط اول</param>
        /// <param name="line2">خط دوم</param>
        /// <returns>خط مطابق</returns>
        public static LineLatLng FindCoincidentLine(LineLatLng line1, LineLatLng line2)
        {
            return line1?.CoincidentLine(line2);
        }
    }
}
