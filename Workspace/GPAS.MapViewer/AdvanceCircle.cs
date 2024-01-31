using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class AdvanceCircle : IAdvanceShape
    {
        /// <summary>
        /// ایجاد دایره ویژه
        /// </summary>
        /// <param name="center">مرکز دایره</param>
        /// <param name="radius">شعاع دایره به متر</param>
        public AdvanceCircle(PointLatLng center, double radius) 
        {
            Center = center;
            Radius = radius;

            fill = new CircleLatLng(Center, Radius);
            controller = new CircleController(Center, Radius, Center, true);

            //Map = map;

            Fill.MouseDown += Fill_MouseDown;
            Fill.MouseUp += Fill_MouseUp;
            Fill.MouseEnter += Fill_MouseEnter;
            Fill.MouseLeave += Fill_MouseLeave;
            Fill.MouseMove += Fill_MouseMove;

            Controller.Stroke.MouseDown += Stroke_MouseDown;
            Controller.Stroke.MouseUp += Stroke_MouseUp;
            Controller.Stroke.MouseEnter += Stroke_MouseEnter;
            Controller.Stroke.MouseLeave += Stroke_MouseLeave;
            Controller.Stroke.MouseMove += Stroke_MouseMove;

            Controller.CenterVertex.MouseDown += CenterVertex_MouseDown;
            Controller.CenterVertex.MouseUp += CenterVertex_MouseUp;
            Controller.CenterVertex.MouseEnter += CenterVertex_MouseEnter;
            Controller.CenterVertex.MouseLeave += CenterVertex_MouseLeave;
            Controller.CenterVertex.MouseMove += CenterVertex_MouseMove;

            MouseUp += AdvanceCircle_MouseUp;

            CreateShape();
        }

#pragma warning disable CS0169 // The field 'AdvanceCircle.MouseUpLocalPosition' is never used
        Point MouseDownLocalPosition, MouseUpLocalPosition, MouseLocalPosition;
#pragma warning restore CS0169 // The field 'AdvanceCircle.MouseUpLocalPosition' is never used
#pragma warning disable CS0169 // The field 'AdvanceCircle.MouseUpMapPosition' is never used
        PointLatLng MouseDownMapPosition, MouseUpMapPosition, MouseMapPosition, Bound;
#pragma warning restore CS0169 // The field 'AdvanceCircle.MouseUpMapPosition' is never used
        CircleLatLng fill;
        CircleController controller;
        Style fillStyle, strokeStyle, centerVertexStyle, centerVertexHoverStyle, strokeHoverStyle, radiusLineStyle;
        MapViewer map;
        private bool isController = true;
        int zIndex = 0;

        public event EventHandler<CircleDrawnEventArgs> CircleDrawn;
        public event MouseButtonEventHandler MouseUp;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;

        public PointLatLng Center { get; set; }

        public double Radius { get; set; }

        public Style FillStyle
        {
            get { return fillStyle; }
            set
            {
                fillStyle = value;
                if (Fill != null)
                    Fill.Style = fillStyle;
            }
        }

        public Style StrokeStyle
        {
            get { return strokeStyle; }
            set
            {
                strokeStyle = value;
                if (Controller != null)
                    Controller.StrokeStyle = strokeStyle;
            }
        }

        public Style StrokeHoverStyle
        {
            get { return strokeHoverStyle; }
            set
            {
                strokeHoverStyle = value;
            }
        }

        public Style CenterVertexStyle
        {
            get { return centerVertexStyle; }
            set
            {
                centerVertexStyle = value;
                if (Controller != null)
                    Controller.CenterVertexStyle = centerVertexStyle;
            }
        }

        public Style CenterVertexHoverStyle
        {
            get { return centerVertexHoverStyle; }
            set
            {
                centerVertexHoverStyle = value;
            }
        }

        public Style RadiusLineStyle
        {
            get { return radiusLineStyle; }
            set
            {
                radiusLineStyle = value;
                if (Controller != null)
                    Controller.RadiusLineStyle = radiusLineStyle;
            }
        }

        public MapViewer Map
        {
            get { return map; }
            set
            {
                map = value;
                if(map != null)
                {
                    if (Fill != null)
                        Fill.Map = map.gmapControl;

                    if (Controller != null)
                        Controller.Map = map.gmapControl;

                    FillStyle = Map.FindResource("FillCircle") as Style;
                    StrokeStyle = Map.FindResource("StrokeCircle") as Style;
                    CenterVertexStyle = Map.FindResource("CenterPointCircle") as Style;
                    StrokeHoverStyle = Map.FindResource("StrokeCircleSelected") as Style;
                    CenterVertexHoverStyle = Map.FindResource("CenterPointCircleSelected") as Style;

                    Map.gmapControl.MouseMove += GmapControl_MouseMove;
                    Map.gmapControl.MouseUp += GmapControl_MouseUp;
                }
            }
        }

        public DrawingMode DrawMode { get; set; }
        public CircleLatLng Fill { get { return fill; } }
        public CircleController Controller { get { return controller; } }

        public bool IsController
        {
            get { return isController; }
            set { isController = value; }
        }

        public int ZIndex { get { return zIndex; } }

        /// <summary>
        /// دایره ویژه ایجاد شده و روی نقشه نشان داده می شود.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public virtual void CreateShape()
        {
            if (Map != null)
            {
                CircleDrawnEventArgs args = new CircleDrawnEventArgs();

                Erase();
                if (!Center.IsEmpty)
                {
                    Fill.Center = Controller.Center = Center;

                    PointLatLng wcpPoint = new PointLatLng();
                    double minimumRadius = 0;
                    if (Controller.CenterVertex.Shape != null)//calc minimum radius
                    {
                        double wcp = (Controller.CenterVertex.Shape as Path).StrokeThickness;
                        GPoint centerPix = Map.gmapControl.MapProvider.Projection.FromLatLngToPixel(Center, Map.gmapControl.MaxZoom);
                        wcpPoint = Map.gmapControl.MapProvider.Projection.FromPixelToLatLng(centerPix.X + (long)wcp, centerPix.Y, Map.gmapControl.MaxZoom);
                        minimumRadius = GeoMath.LengthLine(Center, wcpPoint); //حداقل شعاع دایره که شعاع از این کوچکتر نمی تواند باشد
                    }

                    if (Radius < minimumRadius)
                    {
                        Radius = minimumRadius;
                        Bound = wcpPoint;
                    }

                    Controller.Bound = Bound;
                    Fill.Radius = Controller.Radius = Radius;
                    
                    if (IsController)
                        Controller.CreateController();
                    else
                        Controller.Delete();

                    args.Center = Center;
                    args.RadiusInMeters = Fill.Radius;

                    Draw();

                    Map.SendToFront(this);
                }
                OnCircleDrawn(args);
            }
        }

        /// <summary>
        /// تبدیل دایره ویژه به دایره معمولی
        /// </summary>
        /// <returns>دایره معمولی بدون کنترلر</returns>
        public CircleLatLng ConvertToCircleLatLng()
        {
            CircleLatLng circle = new CircleLatLng(Center, Radius) { Map = Map.gmapControl, Style = Map.FindResource("LockCircle") as Style };
            return circle;
        }

        /// <summary>
        /// اولویت نمایش شکل ویژه را تعیین می کند.
        /// به این صورت که قسمت تو پر در پایین ترین اولویت شکل قرار گرفته سپس خطوط حاشیه شکل و نهایتا نقاط در بالاترین اولویت قرار می گیرند.
        /// </summary>
        /// <param name="minIndex">ایندکس قسمت توپر شکل ویژه که در کمترین اولویت از شکل قرار دارد.</param>
        /// <returns>ایندکس آخرین عنصر اولویت بندی شده رو بر می گرداند.</returns>
        public int SendToFront(int minIndex)
        {
            Fill.ZIndex = minIndex++;
            zIndex = Fill.ZIndex;

            if (IsController)
                minIndex = Controller.SendToFront(minIndex);

            return minIndex;
        }

        /// <summary>
        /// شکل ویژه را حذف می کند.
        /// </summary>
        public virtual void Delete()
        {
            DrawMode = DrawingMode.End;
            Clear();
            Fill?.Delete();
            Controller?.Delete();
            Map?.AdvanceShapes?.Remove(this);

            if (Map.AdvanceShapes.Count > 0)
                Map.SelectedAdvanceShape = Map?.AdvanceShapes.Last();
            else
                Map.SelectedAdvanceShape = null;
        }

        /// <summary>
        /// دایره بر حسب طول و عرض جغرافیایی جابجا می شود.
        /// </summary>
        /// <param name="lat">عرض جغرافیایی</param>
        /// <param name="lng">طول جغرافیایی</param>
        public void Move(double lat, double lng)
        {
            double maxRadius = MaxRadius(new PointLatLng(Center.Lat + lat, Center.Lng + lng));

            if (maxRadius >= Radius)
                Center = new PointLatLng(Center.Lat + lat, Center.Lng + lng);
            else
            {

            }

            CreateShape();
        }

        /// <summary>
        /// نهایت شعاع مجاز برای دایره
        /// </summary>
        /// <param name="center"></param>
        /// <returns></returns>
        private double MaxRadius(PointLatLng center)
        {
            double maxRadius;

            double maxEastRadius = GeoMath.LengthLine(center, new PointLatLng(center.Lat, Map.EasternMostDegree));
            maxRadius = maxEastRadius;

            double maxWestRadius = GeoMath.LengthLine(center, new PointLatLng(center.Lat, Map.WesternMostDegree));
            maxRadius = (maxWestRadius < maxRadius) ? maxWestRadius : maxRadius;

            double maxNorthRadius = GeoMath.LengthLine(center, new PointLatLng(Map.NorthernMostDegree, center.Lng));
            maxRadius = (maxNorthRadius < maxRadius) ? maxNorthRadius : maxRadius;

            double maxSouthRadius = GeoMath.LengthLine(center, new PointLatLng(Map.SouthernMostDegree, center.Lng));
            maxRadius = (maxSouthRadius < maxRadius) ? maxSouthRadius : maxRadius;

            return maxRadius;
        }

        /// <summary>
        /// دایره ویژه حذف می شود.
        /// </summary>
        public void Clear()
        {
            Bound = Center = new PointLatLng();
            Radius = 0;
            CreateShape();
        }

        /// <summary>
        /// دایره ویژه روی نقشه نشان می دهد.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Fill.Draw();
            Controller.Draw();
        }

        /// <summary>
        /// دایره ویژه از روی نقشه پاک می شود
        /// </summary>
        public void Erase()
        {
            Fill.Erase();
            Controller.Erase();
        }

        private void AdvanceCircle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                e.Handled = true;

                DrawMode = DrawingMode.End;

                Controller.CenterVertexStyle = CenterVertexStyle;
                Controller.StrokeStyle = StrokeStyle;
                Controller.RadiusLineStyle = null;
            }
        }

        public virtual void OnCircleDrawn(CircleDrawnEventArgs args)
        {
            CircleDrawn?.Invoke(this, args);
        }

        private void CenterVertex_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void CenterVertex_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void CenterVertex_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        private void CenterVertex_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);

            if (e.LeftButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        private void CenterVertex_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);

            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;

                if (DrawMode == DrawingMode.End)
                {
                    DrawMode = DrawingMode.MovingShape;
                    Controller.CenterVertexStyle = centerVertexHoverStyle;
                }
            }

            MouseDownLocalPosition = p;
            MouseDownMapPosition = mp;
        }

        private void Stroke_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void Stroke_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void Stroke_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        private void Stroke_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);

            if (e.LeftButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        private void Stroke_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);

            e.Handled = true;

            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                if (DrawMode == DrawingMode.End)
                {
                    DrawMode = DrawingMode.Drawing;
                }
            }

            MouseDownLocalPosition = p;
            MouseDownMapPosition = mp;
        }

        private void Fill_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void Fill_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void Fill_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        private void Fill_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);

            if (e.LeftButton == MouseButtonState.Released)
            {
                e.Handled = true;
            }
        }

        private void Fill_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);

            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
            }

            MouseDownLocalPosition = p;
            MouseDownMapPosition = mp;
        }

        private void GmapControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                DrawMode = DrawingMode.End;

                Controller.StrokeStyle = StrokeStyle;
                Controller.CenterVertexStyle = CenterVertexStyle;
                Controller.RadiusLineStyle = null;
            }
        }

        private void GmapControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (DrawMode == DrawingMode.SetPoint || DrawMode == DrawingMode.Drawing)
                {
                    Controller.StrokeStyle = StrokeHoverStyle;
                    double maxRadius = MaxRadius(Center);

                    Radius = GeoMath.LengthLine(Center, mp);
                    Radius = (maxRadius < Radius) ? maxRadius : Radius;

                    double deg = GeoMath.Bearing(Center, mp);
                    Bound = GeoMath.RadialPoint(Center, Radius, deg);

                    CreateShape();
                    DrawMode = DrawingMode.Drawing;
                    if (Controller.ShowRadius)
                        Controller.RadiusLineStyle = Map.FindResource("RadiusLineCircle") as Style;
                }
                else if (DrawMode == DrawingMode.MovingShape)
                {
                    double dLat = mp.Lat - MouseMapPosition.Lat;
                    double dLng = mp.Lng - MouseMapPosition.Lng;

                    Move(dLat, dLng);
                }
            }

            MouseLocalPosition = p;
            MouseMapPosition = mp;
        }
    }
}
