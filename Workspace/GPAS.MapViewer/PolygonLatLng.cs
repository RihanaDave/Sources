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
    public class PolygonLatLng:GMapMarker, IShapable, IMapShape
    {
        /// <summary>
        /// ایجاد چندضلعی
        /// </summary>
        /// <param name="points">نقاط رئوس چند ضلعی</param>
        public PolygonLatLng(List<PointLatLng> points) : base((points?.Count > 0) ? points[0] : new PointLatLng())
        {
            Points = points;

            fillPolygonPath.MouseEnter += FillPolygonPath_MouseEnter;
            fillPolygonPath.MouseLeave += FillPolygonPath_MouseLeave;
            fillPolygonPath.MouseDown += FillPolygonPath_MouseDown;
            fillPolygonPath.MouseMove += FillPolygonPath_MouseMove;
            fillPolygonPath.MouseUp += FillPolygonPath_MouseUp;
        }

        StreamGeometry geometry = new StreamGeometry();
        StreamGeometryContext ctx;
        Path fillPolygonPath = new Path();
        double perimeterInMeters = 0;

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseButtonEventHandler MouseUp;

#pragma warning disable CS0108 // 'PolygonLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        public GMapControl Map { get; set; }
#pragma warning restore CS0108 // 'PolygonLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.

        public List<PointLatLng> Points = new List<PointLatLng>();

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

        public bool IsAnyVectorCross { get { return CrossPoints().Count > 0; } }

        public bool IsAnyVectorCoincident { get { return CoincidentLines().Count > 0; } }

        public double PerimeterInMeters
        {
            get
            {
                return perimeterInMeters;
            }
        }

        /// <summary>
        /// چندضلعی مجددا ساخته می شود اما نمایش داده نمی شود. هر بار که چندضلعی به نقشه اضافه شد، یا نقشه بزرگ و کوچک شد به طور خودکار فراخوانی می شود.
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

                    List<Point> localPath = new List<Point>(Points.Count);
                    GPoint offset = Map.FromLatLngToLocal(Points[0]);

                    perimeterInMeters = CalcPerimeter(Points);

                    for (int i = 0; i < Points.Count; i++)
                    {
                        var p = Map.FromLatLngToLocal(Points[i]);
                        localPath.Add(GeoMath.GPointToPoint(offset, p));
                    }

                    Path shape = CreateFillPolygonPath(localPath);

                    if (Shape is Path)
                    {
                        (Shape as Path).Data = shape.Data;
                        (Shape as Path).Style = shape.Style;
                    }
                    else
                    {
                        Shape = shape;
                    }
                }
                else
                {
                    geometry.Clear();
                    Shape = null;
                }
            }
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        private Path CreateFillPolygonPath(List<Point> localPath)
        {
            // Create a StreamGeometry to use to specify myPath.
            geometry.Clear();
            using (ctx = geometry.Open())
            {
                ctx.BeginFigure(localPath[0], true, true);

                // Draw a line to the next specified point.
                ctx.PolyLineTo(localPath, true, true);
                
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            //geometry.Freeze();

            fillPolygonPath.Data = geometry;
            fillPolygonPath.Style = Style;

            return fillPolygonPath;
        }

        /// <summary>
        /// محسط چند ضلعی را محاسبه می کند.
        /// </summary>
        /// <param name="points">نقاط چند ضلعی</param>
        /// <returns>محیط چندضلعی</returns>
        public static double CalcPerimeter(List<PointLatLng> points)
        {
            double perimeter = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (i == points.Count - 1)
                {
                    perimeter += GeoMath.LengthLine(points[i], points[0]);
                }
                else
                {
                    perimeter += GeoMath.LengthLine(points[i], points[i + 1]);
                }
            }
            return perimeter;
        }

        /// <summary>
        /// مستطیلی که چند ضلعی در آن محصور شده است.
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
                    else if(maxLat < Points[i].Lat)
                    {
                        maxLat = Points[i].Lat;
                    }
                    if(minLng > Points[i].Lng)
                    {
                        minLng = Points[i].Lng;
                    }
                    else if(maxLng < Points[i].Lng)
                    {
                        maxLng = Points[i].Lng;
                    }
                }
                rect = new RectLatLng(minLat, minLng, maxLng - minLng, maxLat - minLat);
            }

            return rect;
        }

        /// <summary>
        /// نقاط متقاطع چند ضلعی را محاسبه می کند.
        /// </summary>
        /// <returns>مجموعه نقاط متقاطع</returns>
        public List<CrossPoint> CrossPoints()
        {
            return PolygonController.FindCrossPoints(this);
        }

        /// <summary>
        /// خطوط منطبق چند ضلعی را محاسبه می کند.
        /// </summary>
        /// <returns>مجموعه خطوط منطبق</returns>
        public List<CoincidentLine> CoincidentLines()
        {
            return PolygonController.FindCoincidentLines(this);
        }

        /// <summary>
        /// شکل را به شکل ویژه تبدیل می کند.
        /// </summary>
        /// <returns>شکل ویژه</returns>
        public AdvancePolygon ConvertToAdvancePolygon()
        {
            AdvancePolygon polygon = new AdvancePolygon(new List<PointLatLng>()) { DrawMode = DrawingMode.End };
            foreach (var p in Points)
            {
                polygon.Points.Add(p);
            }
            return polygon;
        }

        /// <summary>
        /// شکل را حذف می کند.
        /// </summary>
        public void Delete()
        {
            Points.Clear();
            Erase();
        }

        /// <summary>
        /// چندضلعی را روی نقشه نشان می دهد. اگر استایل ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Map?.Markers.Add(this);
        }

        /// <summary>
        /// چندضلعی را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Map?.Markers.Remove(this);
        }

        private void FillPolygonPath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void FillPolygonPath_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void FillPolygonPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        private void FillPolygonPath_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void FillPolygonPath_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }
    }
}
