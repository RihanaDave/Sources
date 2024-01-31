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
    public class VertexLatLng : GMapMarker, IShapable, IMapShape
    {
        /// <summary>
        /// ایجاد نقطه (رأس)
        /// </summary>
        /// <param name="point">نقطه</param>
        public VertexLatLng(PointLatLng point) : base(point)
        {
            Point = point;

            VertexPath.MouseEnter += VertexPath_MouseEnter;
            VertexPath.MouseLeave += VertexPath_MouseLeave;
            VertexPath.MouseDown += VertexPath_MouseDown;
            VertexPath.MouseMove += VertexPath_MouseMove;
            VertexPath.MouseUp += VertexPath_MouseUp;
        }

        StreamGeometry geometry = new StreamGeometry();
        StreamGeometryContext ctx;
        Path VertexPath = new Path();

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseButtonEventHandler MouseUp;

        public PointLatLng Point { get; set; }

#pragma warning disable CS0108 // 'VertexLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        public GMapControl Map { get; set; }
#pragma warning restore CS0108 // 'VertexLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.

        Style style;
        public Style Style
        {
            get { return style; }
            set
            {
                style = value;
                if (Shape != null)
                    (Shape as Path).Style = value;
            }
        }

        /// <summary>
        /// رأس مجددا ساخته می شود اما نمایش داده نمی شود. هر بار که رأس به نقشه اضافه شد، یا نقشه بزرگ و کوچک شد به طور خودکار فراخوانی می شود.
        /// </summary>
        /// <param name="map"></param>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                Map = map;

                if (!Point.IsEmpty)
                {
                    Position = Point;

                    Path shape = CreateVertexPath();

                    if (this.Shape is Path)
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

        private Path CreateVertexPath()
        {
            // Create a StreamGeometry to use to specify myPath.
            geometry.Clear();
            using (ctx = geometry.Open())
            {

                ctx.BeginFigure(new Point(), false, false);

                // Draw a line to the next specified point.
                ctx.LineTo(new Point(double.Epsilon, double.Epsilon), true, true);
            }
            
            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            //geometry.Freeze();

            // Create a path to draw a geometry with.

            // Specify the shape of the Path using the StreamGeometry.
            VertexPath.Data = geometry;
            VertexPath.Style = Style;
            if (VertexPath.StrokeEndLineCap == PenLineCap.Flat) VertexPath.StrokeEndLineCap = PenLineCap.Square;
            if (VertexPath.StrokeStartLineCap == PenLineCap.Flat) VertexPath.StrokeStartLineCap = PenLineCap.Square;

            return VertexPath;
        }

        public void Delete()
        {
            Point = PointLatLng.Empty;
            Erase();
        }

        /// <summary>
        /// رأس را روی نقشه نشان می دهد. اگر استایل ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Map?.Markers.Add(this);
        }

        /// <summary>
        /// رأس را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Map?.Markers.Remove(this);
            RegenerateShape(Map);
        }

        private void VertexPath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void VertexPath_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void VertexPath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        private void VertexPath_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void VertexPath_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }
    } 
}
