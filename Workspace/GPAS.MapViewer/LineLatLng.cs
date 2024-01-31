using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Numerics;

namespace GPAS.MapViewer
{
    public class LineLatLng : GMapMarker, IShapable, IMapShape
    {
        /// <summary>
        /// ایجاد خط
        /// </summary>
        /// <param name="startPoint">نقطه شروع</param>
        /// <param name="endPoint">نقطه پایان</param>
        public LineLatLng(PointLatLng startPoint, PointLatLng endPoint) : base(startPoint)
        {
            //Map = new GMapControl();
            StartPoint = startPoint;
            EndPoint = endPoint;

            LinePath.MouseEnter += LinePath_MouseEnter;
            LinePath.MouseLeave += LinePath_MouseLeave;
            LinePath.MouseDown += LinePath_MouseDown;
            LinePath.MouseMove += LinePath_MouseMove;
            LinePath.MouseUp += LinePath_MouseUp;
        }

        StreamGeometry geometry = new StreamGeometry();
        StreamGeometryContext ctx;
        Path LinePath = new Path();

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseButtonEventHandler MouseUp;

        PointLatLng startPoint = new PointLatLng();
        public PointLatLng StartPoint
        {
            get { return startPoint; }
            set
            {
                startPoint = value;

                slope = CalcSlop();
                a = CalcA();
                b = CalcB();
                c = CalcC();
                lengthInMeter = GeoMath.LengthLine(startPoint, endPoint);
                angle = GeoMath.Bearing(startPoint, endPoint);
            }
        }

        PointLatLng endPoint = new PointLatLng();
        public PointLatLng EndPoint
        {
            get { return endPoint; }
            set
            {
                endPoint = value;

                slope = CalcSlop();
                a = CalcA();
                b = CalcB();
                c = CalcC();
                lengthInMeter = GeoMath.LengthLine(startPoint, endPoint);
                angle = GeoMath.Bearing(startPoint, endPoint);
            }
        }

        decimal slope, a, c, b = 1; //Ax+By=C ; B always is 1 , A=-slope, C=startPoint.Lng - (slope * endPoint.Lat)
        double angle;

        public decimal Slope { get { return slope; } }
        public decimal A { get { return a; } }
        public decimal B { get { return b; } }
        public decimal C { get { return c; } }
        public double Angle { get { return angle; } }

        Boolean vertical = false;
        public bool Vertical { get { return vertical; } }

        double lengthInMeter;
        public Double LengthInMeter { get { return lengthInMeter; } }

#pragma warning disable CS0108 // 'LineLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        public GMapControl Map { get; set; }
#pragma warning restore CS0108 // 'LineLatLng.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.

        Style style;
        public Style Style
        {
            get { return style; }
            set
            {
                style = value;
                if (this.Shape != null)
                    (this.Shape as Path).Style = value;
            }
        }

        /// <summary>
        /// محاسبه A در معادله خط Ax+By=C
        /// </summary>
        /// <returns></returns>
        private decimal CalcA()
        {
            return (vertical) ? 1 : -slope;

            //return (decimal)this.StartPoint.Lng - (decimal)this.EndPoint.Lng;
        }

        /// <summary>
        /// محاسبه B در معادله خط Ax+By=C
        /// </summary>
        /// <returns></returns>
        private decimal CalcB()
        {
            return (Vertical) ? 0 : 1;

            //return (decimal)this.EndPoint.Lat - (decimal)this.StartPoint.Lat;
        }

        /// <summary>
        /// محاسبه C در معادله خط Ax+By=C
        /// </summary>
        /// <returns></returns>
        private decimal CalcC()
        {
            if (Vertical)
                return (decimal)StartPoint.Lat;

            if (Slope == 0)
                return (decimal)StartPoint.Lng;

            return (decimal)StartPoint.Lng - (Slope * (decimal)StartPoint.Lat);
        }

        /// <summary>
        /// محاسبه شیب خط
        /// </summary>
        /// <returns></returns>
        private decimal CalcSlop()
        {

            decimal dLat = (decimal)StartPoint.Lat - (decimal)EndPoint.Lat;
            decimal dLng = (decimal)StartPoint.Lng - (decimal)EndPoint.Lng;

            if (dLat == 0)
            {
                vertical = true;
                return decimal.MaxValue;
            }
            else
            {
                vertical = false;
                return (dLng) / (dLat);
            }
        }

        /// <summary>
        /// خط مجددا ساخته می شود اما نمایش داده نمی شود. هر بار که خط به نقشه اضافه شد، یا نقشه بزرگ و کوچک شد به طور خودکار فراخوانی می شود.
        /// </summary>
        /// <param name="map"></param>
        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null)
            {
                Map = map;

                if (!StartPoint.IsEmpty && !EndPoint.IsEmpty)
                {
                    Position = StartPoint;

                    var offset = Map.FromLatLngToLocal(StartPoint);
                    Point start = new Point(0, 0);
                    GPoint gEnd = Map.FromLatLngToLocal(EndPoint);
                    Point end = new Point(gEnd.X - offset.X, gEnd.Y - offset.Y);

                    Path shape = CreateLinePath(start, end);

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

        private Path CreateLinePath(Point start, Point end)
        {
            // Create a StreamGeometry to use to specify myPath.
            geometry.Clear();
            using (ctx = geometry.Open())
            {
                ctx.BeginFigure(start, false, false);
                
                // Draw a line to the next specified point.
                ctx.LineTo(end, true, true);
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            //geometry.Freeze();

            // Create a path to draw a geometry with.
            {
                // Specify the shape of the Path using the StreamGeometry.
                LinePath.Data = geometry;
                LinePath.Style = Style;
            }
            return LinePath;
        }

        /// <summary>
        /// نقطه تقاطع خط با خط دیگر را بررسی مس کند.
        /// </summary>
        /// <param name="otherLine">خط دیگر</param>
        /// <returns>نقطه تقاطع</returns>
        public PointLatLng CrossPoint(LineLatLng otherLine)
        {
            PointLatLng crossPoint = new PointLatLng();
            if (otherLine == null) return crossPoint;

            decimal delta = this.A * otherLine.B - otherLine.A * this.B;

            if (delta == 0)
            {
                //Is parallel; if C1==C2 ==> 
            }
            else
            {

                decimal lat = ((otherLine.B * this.C) - (this.B * otherLine.C)) / delta;
                decimal lng = ((this.A * otherLine.C) - (otherLine.A * this.C)) / delta;

                if (BetWeen(lat, (decimal)this.StartPoint.Lat, (decimal)this.EndPoint.Lat) && BetWeen(lat, (decimal)otherLine.startPoint.Lat, (decimal)otherLine.endPoint.Lat) &&
                    BetWeen(lng, (decimal)this.StartPoint.Lng, (decimal)this.EndPoint.Lng) && BetWeen(lng, (decimal)otherLine.startPoint.Lng, (decimal)otherLine.EndPoint.Lng))
                {
                    crossPoint = new PointLatLng((double)lat, (double)lng);
                }
            }

            return crossPoint;
        }

        /// <summary>
        /// منطق بودن خط بر خط دیگر را بررسی می کند.
        /// </summary>
        /// <param name="otherLine">خط دیگر</param>
        /// <returns>آن قسمتی از دو خط را که بر هم منطبق است بصورت یک خط بر می گرداند.</returns>
        public LineLatLng CoincidentLine(LineLatLng otherLine)
        {
            if (otherLine == null) return null;
            if (this.A == otherLine.A && this.C == otherLine.C) //if A==A ==> is parallel And if C==C ==>is Coincident
            {
                LineLatLng e1 = (LineLatLng)this.MemberwiseClone(); ;
                LineLatLng e2 = (LineLatLng)otherLine.MemberwiseClone();

                if (Math.Abs(this.Slope) <= 1)
                {
                    e1.SortLineByLat();
                    e2.SortLineByLat();

                    if (e1.StartPoint.Lat > e2.StartPoint.Lat)
                    {
                        LineLatLng t = e2;
                        e2 = e1;
                        e1 = t;
                    }

                    if (e1.EndPoint.Lat <= e2.StartPoint.Lat) //no Coincident (The two lines do not have any subscriber intervals)
                    {
                        return null;
                    }
                    else if (e2.EndPoint.Lat <= e1.EndPoint.Lat) //full Coincident (line2 is full coincident with line1)
                    {
                        LineLatLng result = new LineLatLng(e2.StartPoint, e2.EndPoint);
                        result.Map = Map;
                        return result;
                    }
                    else
                    {
                        LineLatLng result = new LineLatLng(e2.StartPoint, e1.EndPoint);
                        result.Map = Map;
                        return result; //half Coincident (From the starting point of line2 to the end of line1, they coincide)
                    }
                }
                else
                {
                    e1.SortLineByLng();
                    e2.SortLineByLng();

                    if (e1.StartPoint.Lng > e2.StartPoint.Lng)
                    {
                        LineLatLng t = e2;
                        e2 = e1;
                        e1 = t;
                    }

                    if (e1.EndPoint.Lng <= e2.StartPoint.Lng) //no Coincident (The two lines do not have any subscriber intervals)
                    {
                        return null;
                    }
                    else if (e2.EndPoint.Lng <= e1.EndPoint.Lng) //full Coincident (line2 is full coincident with line1)
                    {
                        LineLatLng result = new LineLatLng(e2.StartPoint, e2.EndPoint);
                        result.Map = Map;
                        return result;
                    }
                    else
                    {
                        LineLatLng result = new LineLatLng(e2.StartPoint, e1.EndPoint);
                        result.Map = Map;
                        return result; //half Coincident (From the starting point of line2 to the end of line1, they coincide)
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// اگر عرض جغرافیایی نقطه ابتدای خط از نقطه انتهایی خط بیشتر باشد نقطه ابتدا و انتها را جابجا می کند.
        /// </summary>
        private void SortLineByLat()
        {
            if (StartPoint.Lat > EndPoint.Lat)
            {
                ExchangePoints();
            }
        }

        /// <summary>
        /// اگر طول جغرافیایی نقطه ابتدای خط از نقطه انتهایی خط بیشتر باشد نقطه ابتدا و انتها را جابجا می کند.
        /// </summary>
        private void SortLineByLng()
        {
            if (StartPoint.Lng > EndPoint.Lng)
            {
                ExchangePoints();
            }
        }

        /// <summary>
        /// موجود بودن یک عدد درون یک بازه عددی را بررسی می کند
        /// </summary>
        /// <param name="num">عدد</param>
        /// <param name="min">کف بازه</param>
        /// <param name="max">سقف بازه</param>
        /// <returns></returns>
        private bool BetWeen(decimal num, decimal min, decimal max)
        {
            if (min > max)
            {
                ExchangeNum(ref min, ref max);
            }
            return num >= min && num <= max;
        }

        /// <summary>
        /// نقطه ابتدا و انتهای خط را جابجا می کند.
        /// </summary>
        public void ExchangePoints()
        {
            PointLatLng p = StartPoint;
            StartPoint = EndPoint; ;
            EndPoint = p;
        }

        /// <summary>
        /// دو عدد را با هم جابجا می کند.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void ExchangeNum(ref decimal a, ref decimal b)
        {
            a = a + b;
            b = a - b;
            a = a - b;
        }

        /// <summary>
        /// شکل را حذف می کند.
        /// </summary>
        public void Delete()
        {
            StartPoint = PointLatLng.Empty;
            EndPoint = PointLatLng.Empty;
            Erase();
        }

        /// <summary>
        /// خط را روی نقشه نشان می دهد. اگر استایل ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Map?.Markers.Add(this);
        }

        /// <summary>
        /// خط را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Map?.Markers.Remove(this);
            RegenerateShape(Map);
        }

        private void LinePath_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void LinePath_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void LinePath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        private void LinePath_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void LinePath_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }
    }
}
