using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class CircleLatLng : GMapMarker, IShapable, IMapShape
    {
        /// <summary>
        /// ایجاد دایره
        /// </summary>
        /// <param name="center">نقطه مرکز</param>
        /// <param name="radius">شعاع به متر</param>
        public CircleLatLng(PointLatLng center, double radius) : base(center)
        {
            Center = center;
            Radius = radius;

            circlePath.MouseEnter += CirclePath_MouseEnter;
            circlePath.MouseLeave += CirclePath_MouseLeave;
            circlePath.MouseDown += CirclePath_MouseDown;
            circlePath.MouseMove += CirclePath_MouseMove;
            circlePath.MouseUp += CirclePath_MouseUp;
        }

        Path circlePath = new Path();
        StreamGeometry geometry = new StreamGeometry();
        StreamGeometryContext ctx;

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseButtonEventHandler MouseUp;

        public PointLatLng Center { get; set; }

#pragma warning disable CS0108 // 'CircleLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        public GMapControl Map { get; set; }
#pragma warning restore CS0108 // 'CircleLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.

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

        public PointLatLng EastBound;
        public PointLatLng WestBound;
        public PointLatLng NorthBound;
        public PointLatLng SouthBound;

        public GPoint CenterPix;
        public GPoint EastBoundPix;
        public GPoint WestBoundPix;
        public GPoint NorthBoundPix;
        public GPoint SouthBoundPix;

        /// <summary>
        /// دایره مجددا ساخته می شود اما نمایش داده نمی شود. هر بار که دایره به نقشه اضافه شد، یا نقشه بزرگ و کوچک شد به طور خودکار فراخوانی می شود.
        /// </summary>
        /// <param name="map"></param>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                Map = map;

                if (!Center.IsEmpty)
                {
                    NorthBound = GeoMath.RadialPoint(Center, Radius, 0);
                    EastBound = GeoMath.RadialPoint(Center, Radius, 90);
                    SouthBound = GeoMath.RadialPoint(Center, Radius, 180);
                    WestBound = GeoMath.RadialPoint(Center, Radius, 270);

                    CenterPix = Map.FromLatLngToLocal(Center);
                    EastBoundPix = Map.FromLatLngToLocal(EastBound);
                    WestBoundPix = Map.FromLatLngToLocal(WestBound);
                    NorthBoundPix = Map.FromLatLngToLocal(NorthBound);
                    SouthBoundPix = Map.FromLatLngToLocal(SouthBound);

                    Position = Center;
                    var offset = map.FromLatLngToLocal(new PointLatLng(NorthBound.Lat, WestBound.Lng));
                    Point c, w, e, n, s;
                    c = GeoMath.GPointToPoint(offset, CenterPix);
                    w = GeoMath.GPointToPoint(offset, WestBoundPix);
                    e = GeoMath.GPointToPoint(offset, EastBoundPix);
                    n = GeoMath.GPointToPoint(offset, NorthBoundPix);
                    s = GeoMath.GPointToPoint(offset, SouthBoundPix);
                    Offset = new Point(-(w.X + c.X), -(n.Y + c.Y));

                    Path shape = CreateCirclePath(c, w, e, n, s);

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
        }

        private Path CreateCirclePath(Point center, Point west, Point east, Point north, Point south)
        {
            geometry.Clear();
            using (ctx = geometry.Open())
            {
                ctx.BeginFigure(west, true, false);
                ctx.ArcTo(north, new Size(Math.Abs(west.X - center.X), Math.Abs(center.Y - north.Y)), 0, false, SweepDirection.Clockwise, true, true);
                ctx.ArcTo(east, new Size(Math.Abs(east.X - center.X), Math.Abs(center.Y - north.Y)), 0, false, SweepDirection.Clockwise, true, true);
                ctx.ArcTo(south, new Size(Math.Abs(center.X - east.X), Math.Abs(south.Y - center.Y)), 0, false, SweepDirection.Clockwise, true, true);
                ctx.ArcTo(west, new Size(Math.Abs(center.X - west.X), Math.Abs(south.Y - center.Y)), 0, false, SweepDirection.Clockwise, true, true);
            }

            //geometry.Freeze();

            circlePath.Data = geometry;
            circlePath.Style = Style;

            return circlePath;
        }

        /// <summary>
        /// شکل را حذف می کند
        /// </summary>
        public void Delete()
        {
            Center = PointLatLng.Empty;
            Radius = 0;
            Erase();
        }

        /// <summary>
        /// شکل را به شکل ویژه تبدیل می کند.
        /// </summary>
        /// <returns>شکل ویژه</returns>
        public AdvanceCircle ConvertToAdvanceCircle()
        {
            AdvanceCircle circle = new AdvanceCircle(Center, Radius) { DrawMode = DrawingMode.End };
            return circle;
        }

        /// <summary>
        /// دایره را روی نقشه نشان می دهد. اگر استایل ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Map?.Markers.Add(this);
        }

        /// <summary>
        /// دایره را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Map?.Markers.Remove(this);
        }

        private void CirclePath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void CirclePath_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void CirclePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        private void CirclePath_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void CirclePath_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }
    }
}
