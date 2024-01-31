using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.MapViewer
{
    public abstract class GeoMath
    {
        public static PureProjection Projection = GMap.NET.Projections.MercatorProjection.Instance;
        static double EarthRadius = 6378137;

        /// <summary>
        /// زاویه بین دو نقطه بر حسب درجه را محاسبه می کند.
        /// خط عمودی به سمت شمال زاویه 0 درجه است.
        /// </summary>
        /// <param name="point1">نقطه اول</param>
        /// <param name="point2">نقطه دوم</param>
        /// <returns>زاویه برحسب درجه</returns>
        public static double Bearing(PointLatLng point1, PointLatLng point2)
        {
            return Projection.GetBearing(point1, point2);
        }

        /// <summary>
        /// زاویه بین دو خط (3 نقطه) که نقطه انتهای خط اول نقطه ابتدای خط دوم است را محاسبه می کند.
        /// خط عمودی به سمت شمال زاویه 0 درجه است.
        /// </summary>
        /// <param name="point1">نقطه اول</param>
        /// <param name="center">نقطه مشترک</param>
        /// <param name="point3">نقطه آخر</param>
        /// <returns>زاویه برحسب درجه</returns>
        public static double Angle(PointLatLng point1, PointLatLng center, PointLatLng point3)
        {
            var bearing1 = Bearing(point1, center);
            var bearing2 = Bearing(center, point3);
            return DifferenceBearing(bearing1, bearing2);
        }

        /// <summary>
        /// اختلاف دو زاویه برحسب درجه را محاسبه می کند.
        /// جواب همیشه عددی بین 0 تا 360 درجه است.
        /// </summary>
        /// <param name="bearing1">زاویه اول</param>
        /// <param name="bearing2">زاویه دوم</param>
        /// <returns>اختلاف بین دو زاویه برحسب درجه</returns>
        public static double DifferenceBearing(double bearing1, double bearing2)
        {
            //return bearing1 - bearing2;
            bearing1 = BetweenZero360(bearing1);
            bearing2 = BetweenZero360(bearing2);

            return BetweenZero360(bearing1 - bearing2);
        }

        /// <summary>
        /// زاویه عمود بر زاویه مورد نظر
        /// </summary>
        /// <param name="bearing">زاویه اصلی</param>
        /// <returns>زاویه عمود بر حسب درجه.</returns>
        public static double Perpendicular(double bearing)
        {
            bearing = BetweenZero360(bearing);

            return BetweenZero360(bearing + 90);
        }

        /// <summary>
        /// زاویه داده شده را به زاویه ای بین 0 تا 360 درجه تبدیل می کند.
        /// </summary>
        /// <param name="bearing">زاویه بر حسب درجه</param>
        /// <returns>زاویه بین 0 تا 360</returns>
        public static double BetweenZero360(double bearing)
        {
            return (bearing < 0)? 360 + bearing % 360: bearing % 360;
        }

        /// <summary>
        /// تبدیل زاویه برحسب رادیان به درجه.
        /// </summary>
        /// <param name="rad">زاویه برحسب رادیان</param>
        /// <returns>زاویه برحسب درجه</returns>
        public static double RadiansToDegrees(double rad)
        {
            return PureProjection.RadiansToDegrees(rad);
        }

        /// <summary>
        /// تبدیل زاویه بر حسب درجه به رادیان.
        /// </summary>
        /// <param name="deg">زاویه برحسب درجه</param>
        /// <returns>زاویه برحسب رادیان</returns>
        public static double DegreesToRadians(double deg)
        {
            return PureProjection.DegreesToRadians(deg);
        }

        /// <summary>
        /// طول یک خط که متشکل از دو نقطه است را محاسبه می کند.
        /// </summary>
        /// <param name="point1">نقطه اول</param>
        /// <param name="point2">نقطه دوم</param>
        /// <returns>طول خط برحسب متر</returns>
        public static double LengthLine(PointLatLng point1, PointLatLng point2)
        {
            if (point2.Lng <= 0 && point1.Lng <= 0 || point2.Lng >= 0 && point1.Lng >= 0) //اگر خط نصف النهار مبدا را قطع نمی کند
            {
                return Distance(point1, point2);
            }
            else
            {//اگر شعاع نصف النهار مبدا را قطع می کند آنگاه
             //شعاع را به دو قسمت از نقطه شروع شعاع تا نقطه برخورد با نصف النهار مبدا و از نقطه برخورد با نصف النهار تا انتهای شعاع تقسیم می کنیم
                decimal dLng = (decimal)(point1.Lng - point2.Lng);
                decimal dLat = (decimal)(point1.Lat - point2.Lat);

                decimal m = (dLat == 0) ? decimal.MaxValue : dLng / dLat; //Y=-aX+c , -a=m

                decimal c;
                if (m == decimal.MaxValue)
                    c = (decimal)point1.Lat;
                else if (m == 0)
                    c = (decimal)point1.Lng;
                else
                    c = ((decimal)point1.Lng - (m * (decimal)point1.Lat));

                decimal a = (m == decimal.MaxValue) ? 1 : -m;

                double zeroLat = dLng == 0 ? 0 : (Double)(c / a); //محاسبه طول جغرافیایی نقطه برخورد شعاع با نصف النهار مبدا

                PointLatLng zeroPoint = new PointLatLng(zeroLat, 0);

                return LengthLine(point1, zeroPoint) + LengthLine(zeroPoint, point2); //طول شعاع حاصل جمع طول دو خط محاسبه شده است
            }
        }

        /// <summary>
        /// فاصله بین دو نقطه را محاسبه می کند.
        /// </summary>
        /// <param name="point1">نقطه اول</param>
        /// <param name="point2">نقطه دوم</param>
        /// <returns>فاصله برحسب متر</returns>
        public static double Distance(PointLatLng point1, PointLatLng point2)
        {
            return Projection.GetDistance(point1, point2) * 1000;
        }

        /// <summary>
        /// فاصله بین دو نقطه را برحسب درجه محاسبه می کند
        /// در دایره شعاع را بر حسب درجه بدست می آورد
        /// </summary>
        /// <param name="center">نقظه اول یا مرکز دایره</param>
        /// <param name="bound">نقطه دوم یا یکی از نقاط روی حاشیه دایره</param>
        /// <returns>شعاع دایره بر حسب درجه</returns>
        public static double DistanceInDegree(PointLatLng center, PointLatLng bound)
        {
            double dLat = bound.Lat - center.Lat;
            double dLng = bound.Lng - center.Lng;

            return Math.Sqrt(dLat * dLat + dLng * dLng);            
        }

        /// <summary>
        /// نقطه آفست را نقطه مرکز محور مختصات فرض کرده و مختصات نقطه دیگر را بر حسب آفست محاسبه می کند 
        /// </summary>
        /// <param name="offset">آفست</param>
        /// <param name="point">نقطه دیگر که در نظر داریم مختصات آنرا برحسب آفست داشته باشیم</param>
        /// <returns></returns>
        public static Point GPointToPoint(GPoint offset, GPoint point)
        {
            return new Point(point.X - offset.X, point.Y - offset.Y);
        }

        /// <summary>
        /// Calculates a point at the specified distance along a radial from a
        /// center point.
        /// </summary>
        /// <param name="latitude">The latitude of the center point.</param>
        /// <param name="longitude">The longitude of the center point.</param>
        /// <param name="distance">The distance in meters.</param>
        /// <param name="radial">
        /// The radial in degrees, measures clockwise from north.
        /// </param>
        /// <returns>
        /// A <see cref="Vector"/> containing the Latitude and Longitude of the
        /// calculated point.
        /// </returns>
        /// <remarks>The antemeridian is not considered.</remarks>
        public static PointLatLng RadialPoint(PointLatLng point, double distance, double radial)
        {
            //var φ2 = Math.asin(Math.sin(φ1) * Math.cos(d / R) +
            //        Math.cos(φ1) * Math.sin(d / R) * Math.cos(brng));
            //var λ2 = λ1 + Math.atan2(Math.sin(brng) * Math.sin(d / R) * Math.cos(φ1),
            //                         Math.cos(d / R) - Math.sin(φ1) * Math.sin(φ2));
            double latitude = DegreesToRadians(point.Lat);
            double longitude = DegreesToRadians(point.Lng);
            distance = MetersToRadians(distance);
            radial = DegreesToRadians(radial);

            double latitudeDistance = Math.Cos(latitude) * Math.Sin(distance);
            double radialLat = Math.Asin((Math.Sin(latitude) * Math.Cos(distance)) + (latitudeDistance * Math.Cos(radial)));

            double y = Math.Sin(radial) * latitudeDistance;
            double x = Math.Cos(distance) - (Math.Sin(latitude) * Math.Sin(radialLat));
            double deltaLon = Math.Atan2(y, x);

            double radialLon = ((longitude + deltaLon + Math.PI) % (2 * Math.PI)) - Math.PI;

            return new PointLatLng(RadiansToDegrees(radialLat), RadiansToDegrees(radialLon));
        }

        /// <summary>
        /// Converts the specified distance in meters to radians.
        /// </summary>
        /// <param name="meters">The distance in meters.</param>
        /// <returns>The specified distance converted to radians.</returns>
        public static double MetersToRadians(double meters)
        {
            return meters / EarthRadius;
        }

        public static GPoint FromLatLngToTileXY(PointLatLng point, int zoom)
        {
            var px = Projection.FromLatLngToPixel(point, zoom);
            return Projection.FromPixelToTileXY(px);
        }

        public static PointLatLng FromTileXYToLatLng(GPoint tile, GSize tileSize, int zoom)
        {
            var lastTile = Projection.GetTileMatrixMaxXY(zoom);

            GPoint gpTileTopLeft = new GPoint(tile.X * tileSize.Width, tile.Y * tileSize.Height);
            PointLatLng pllTileTopLeft = FromPixelToLatLng(gpTileTopLeft, zoom);

            if (tile.X == lastTile.Width + 1)
            {
                pllTileTopLeft.Lng = 180;
            }
            if (tile.Y == lastTile.Height + 1)
            {
                pllTileTopLeft.Lat = -85.0511287798066;
            }

            return pllTileTopLeft;
        }

        public static GPoint FromPixelToTileXY(GPoint point)
        {
            return Projection.FromPixelToTileXY(point);
        }

        public static GSize GetTileSize()
        {
            return Projection.TileSize;
        }

        public static PointLatLng FromPixelToLatLng(GPoint point, int zoom)
        {
            return Projection.FromPixelToLatLng(point, zoom);
        }

        public static GPoint FromLatLngToPixel(PointLatLng p, int zoom)
        {
            return Projection.FromLatLngToPixel(p, zoom);
        }

        public static bool IsPointLatLngInsideRangeTilesXY(PointLatLng point, GPoint tileMinView, GPoint tileMaxView, GSize tileSize, int zoom)
        {
            var pllTopLeft = FromTileXYToLatLng(tileMinView, tileSize, zoom);
            var pllRightBottom = FromTileXYToLatLng(new GPoint(tileMaxView.X + 1, tileMaxView.Y + 1), tileSize, zoom);

            if (point.Lat < pllRightBottom.Lat || point.Lat > pllTopLeft.Lat || point.Lng < pllTopLeft.Lng || point.Lng > pllRightBottom.Lng)
                return false;

            return true;
        }

        public static bool IsPointLatLngInsideArea(PointLatLng point, RectLatLng area)
        {
            if (point.Lat > area.Top || point.Lat < area.Bottom || point.Lng < area.Left || point.Lng > area.Right)
                return false;

            return true;
        }
    }
}
