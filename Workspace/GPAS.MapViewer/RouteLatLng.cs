using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class RouteLatLng : GMapMarker, IShapable, IMapShape
    {
        /// <summary>
        /// ایجاد مسیر بصورت هاله
        /// </summary>
        /// <param name="points">نقاط رئوس مسیر</param>
        /// <param name="radius">شعاع هاله برحسب متر</param>
        public RouteLatLng(List<PointLatLng> points, double radius) : this(points, radius, true)
        {

        }

        /// <summary>
        /// ایجاد مسیر
        /// </summary>
        /// <param name="points">نقاط رئوس مسیر</param>
        /// <param name="radius">شعاع هاله برحسب متر</param>
        /// <param name="haloMode">هاله باشد یا نباشد</param>
        public RouteLatLng(List<PointLatLng> points, double radius, bool haloMode) : base((points?.Count > 0) ? points[0] : new PointLatLng())
        {
            Points = points;
            Radius = radius;
            HaloMode = haloMode;

            haloRoutePath.MouseEnter += HaloRoutePath_MouseEnter;
            haloRoutePath.MouseLeave += HaloRoutePath_MouseLeave;
            haloRoutePath.MouseDown += HaloRoutePath_MouseDown;
            haloRoutePath.MouseMove += HaloRoutePath_MouseMove;
            haloRoutePath.MouseUp += HaloRoutePath_MouseUp;
        }

        Path haloRoutePath = new Path();
        StreamGeometryContext ctx;
        StreamGeometry geometry = new StreamGeometry();
        double lengthInMeter;

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseButtonEventHandler MouseUp;

        public bool HaloMode { get; set; }

#pragma warning disable CS0108 // 'RouteLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        public GMapControl Map { get; set; }
#pragma warning restore CS0108 // 'RouteLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.

        public List<PointLatLng> Points = new List<PointLatLng>();

        public double Radius { get; set; }

        private Style style;
        public Style Style
        {
            get { return style; }
            set
            {
                style = value;

                if (Shape is Path)
                    (Shape as Path).Style = value;
            }
        }

        public double LengthInMeter
        {
            get
            {
                return lengthInMeter;
            }
        }

        /// <summary>
        /// هاله مسیر مجددا ساخته می شود اما نمایش داده نمی شود. هر بار که هاله مسیر به نقشه اضافه شد، یا نقشه بزرگ و کوچک شد به طور خودکار فراخوانی می شود.
        /// </summary>
        /// <param name="map"></param>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                Map = map;
                if (Points?.Count > 1)
                {
                    Position = Points[0];

                    var localPath = new List<Point>(Points.Count);
                    GPoint offset = Map.FromLatLngToLocal(Points[0]);
                    foreach (var point in Points)
                    {
                        var p = Map.FromLatLngToLocal(point);
                        localPath.Add(GeoMath.GPointToPoint(offset, p));
                    }

                    lengthInMeter = CalcLength(Points);

                    PointLatLng wBound = GeoMath.RadialPoint(Points[0], Radius, 270);
                    PointLatLng eBound = new PointLatLng(wBound.Lat, Points[0].Lng + (Points[0].Lng - wBound.Lng));

                    GPoint ePix = Map.FromLatLngToLocal(eBound);
                    GPoint wPix = Map.FromLatLngToLocal(wBound);

                    double radiusInPixel = Math.Abs(offset.X - ePix.X) + Math.Abs(offset.X - wPix.X);

                    var shape = CreateHaloRoutePath(localPath);

                    if (Shape is Path)
                    {
                        (Shape as Path).Data = shape.Data;
                        (Shape as Path).Style = shape.Style;
                    }
                    else
                    {
                        Shape = shape;
                    }
                    if (HaloMode)
                        (Shape as Path).StrokeThickness = radiusInPixel;
                }
                else
                {
                    geometry.Clear();
                    Shape = null;
                }
            }
        }

        private Path CreateHaloRoutePath(List<Point> localPath)
        {
            // Create a StreamGeometry to use to specify myPath.

            geometry.Clear();
            using ( ctx = geometry.Open())
            {
                ctx.BeginFigure(localPath[0], false, false);

                // Draw a line to the next specified point.
                ctx.PolyLineTo(localPath, true, true);
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            //geometry.Freeze();

            haloRoutePath.Data = geometry;
            haloRoutePath.Style = Style;

            return haloRoutePath;
        }

        /// <summary>
        /// محاسبه طول مسیر.
        /// </summary>
        /// <param name="points">نقاط مسیر</param>
        /// <returns>طول مسیر</returns>
        public static double CalcLength(List<PointLatLng> points)
        {
            double perimeter = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                perimeter += GeoMath.LengthLine(points[i], points[i + 1]);
            }
            return perimeter;
        }

        /// <summary>
        /// مستطیلی که مسیر در آن محصور شده است.
        /// </summary>
        /// <returns>مستطیل</returns>
        public RectLatLng GetRect()
        {
            RectLatLng rect = new RectLatLng();

            if (Points.Count > 0)
            {
                double minLng, maxLng, minLat, maxLat;
                minLng = maxLng = Points[0].Lng;
                minLat = maxLat = Points[0].Lat;

                for (int i = 1; i < Points.Count; i++)
                {
                    if (minLat > Points[i].Lat)
                    {
                        minLat = Points[i].Lat;
                    }
                    else if (maxLat < Points[i].Lat)
                    {
                        maxLat = Points[i].Lat;
                    }
                    if (minLng > Points[i].Lng)
                    {
                        minLng = Points[i].Lng;
                    }
                    else if (maxLng < Points[i].Lng)
                    {
                        maxLng = Points[i].Lng;
                    }
                }
                rect = new RectLatLng(minLat, minLng, maxLng - minLng, maxLat - minLat);
            }

            return rect;
        }

        /// <summary>
        /// شکل را حذف می کند.
        /// </summary>
        public void Delete()
        {
            Points.Clear();
            Radius = 0;
            Erase();
        }

        /// <summary>
        /// شکل را به شکل ویژه تبدیل می کند.
        /// </summary>
        /// <returns>شکل ویژه</returns>
        public AdvanceRoute ConvertToAdvanceRoute()
        {
            AdvanceRoute route = new AdvanceRoute(new List<PointLatLng>(), Radius) { DrawMode = DrawingMode.End };
            foreach (var p in Points)
            {
                route.Points.Add(p);
            }
            return route;
        }

        /// <summary>
        /// هاله مسیر را روی نقشه نشان می دهد. اگر استایل ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Map?.Markers.Add(this);
        }

        /// <summary>
        /// هاله مسیر را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Map?.Markers.Remove(this);
        }

        private void HaloRoutePath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void HaloRoutePath_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void HaloRoutePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        private void HaloRoutePath_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void HaloRoutePath_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }
    }
}
