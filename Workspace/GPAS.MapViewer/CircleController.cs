using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class CircleController
    {

        /// <summary>
        /// ایجاد کنترلر دایره
        /// </summary>
        /// <param name="center">نقطه مرکز دایره</param>
        /// <param name="radius">شعاع دایره بر حسب متر</param>
        /// <param name="bound">نقطه محصور کننده دایره</param>
        /// <param name="showRadius">نمایش خط فرضی شعاع</param>
        public CircleController(PointLatLng center, double radius, PointLatLng bound, bool showRadius)
        {
            Center = center;
            Radius = radius;
            Bound = bound;
            ShowRadius = showRadius;

            stroke= new CircleLatLng(Center, Radius);
            centerVertex = new VertexLatLng(Center);
            radiusLine = new LineLatLng(Center, Bound);

            CreateController();
        }

        CircleLatLng stroke;
        VertexLatLng centerVertex;
        Style strokeStyle, centerVertexStyle, radiusLineStyle;
        GMapControl map;
        LineLatLng radiusLine;

        public bool ShowRadius { get; set; }

        public CircleLatLng Stroke { get { return stroke; } }

        public VertexLatLng CenterVertex { get { return centerVertex; } }

        public PointLatLng Center { get; set; }

        public double Radius { get; set; }

        public PointLatLng Bound { get; set; }

        public LineLatLng RadiusLine { get { return radiusLine; } }

        public GMapControl Map
        {
            get { return map; }
            set
            {
                map = value;
                if (Stroke != null)
                    Stroke.Map = map;

                if (CenterVertex != null)
                    CenterVertex.Map = map;

                if (RadiusLine != null)
                    RadiusLine.Map = map;
            }
        }

        public Style StrokeStyle
        {
            get { return strokeStyle; }
            set
            {
                strokeStyle = value;
                if (Stroke != null)
                    Stroke.Style = strokeStyle;
            }
        }

        public Style CenterVertexStyle
        {
            get { return centerVertexStyle; }
            set
            {
                centerVertexStyle = value;
                if (CenterVertex !=null)
                    CenterVertex.Style = centerVertexStyle;
            }
        }

        public Style RadiusLineStyle
        {
            get { return radiusLineStyle; }
            set
            {
                radiusLineStyle = value;
                if (RadiusLine != null)
                    RadiusLine.Style = radiusLineStyle;
            }
        }

        /// <summary>
        /// کنترلر دایره را ایجاد می کند. در اصل نقطه مرکز و حاشیه دایره را ایجاد می کند اما نمایش نمی دهد. 
        /// </summary>
        public virtual void CreateController()
        {
            if (!Center.IsEmpty)
            {
                Stroke.Center = CenterVertex.Point = RadiusLine.StartPoint = Center;
                Stroke.Radius = Radius;
                RadiusLine.EndPoint = Bound;
            }
        }

        /// <summary>
        /// اولویت نمایش اجزا کنترلر را تعیین می کند.
        /// به این صورت که ابتدا خطوط و سپس رئوس را ترسیم می کند.
        /// </summary>
        /// <param name="minIndex">ایندکسی که باید از آن شروع کند.</param>
        /// <returns>ایندکس آخرین عنصر اولویت بندی شده رو بر می گرداند.</returns>
        public int SendToFront(int minIndex)
        {
            Stroke.ZIndex = minIndex++;
            CenterVertex.ZIndex = minIndex++;
            return minIndex;
        }

        /// <summary>
        /// کنترلر را حذف می کند.
        /// </summary>
        public void Delete()
        {
            Erase();
            CenterVertex?.Delete();
            Stroke?.Delete();
            RadiusLine?.Delete();
        }

        /// <summary>
        /// کنترلر را روی نقشه نشان می دهد. اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Stroke.Draw();
            if (ShowRadius)
                RadiusLine.Draw();
            CenterVertex.Draw();
        }

        /// <summary>
        /// کنترلر را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Stroke.Erase();
            RadiusLine.Erase();
            CenterVertex.Erase();
        }
    }
}
