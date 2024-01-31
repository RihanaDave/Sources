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
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class AdvanceRoute : IAdvanceShape
    {
        /// <summary>
        /// ایجاد مسیر ویژه
        /// </summary>
        /// <param name="points">نقاط رئوس مسیر ویژه</param>
        /// <param name="radius">شعاع هاله مسیر ویژه بر حسب متر</param>
        public AdvanceRoute(List<PointLatLng> points, double radius)
        {
            Points = points;
            Radius = radius;
            DrawMode = DrawingMode.Ready;

            halo = new RouteLatLng(Points, Radius);
            controller = new RouteController(Points);

            //Map = mapViewer;

            Controller.VerticesMouseDown += Controller_VerticesMouseDown;
            Controller.VerticesMouseUp += Controller_VerticesMouseUp;
            Controller.VerticesMouseEnter += Controller_VerticesMouseEnter;
            Controller.VerticesMouseLeave += Controller_VerticesMouseLeave;
            Controller.VerticesMouseMove += Controller_VerticesMouseMove;

            Controller.LinesMouseDown += Controller_LinesMouseDown;
            Controller.LinesMouseUp += Controller_LinesMouseUp;
            Controller.LinesMouseEnter += Controller_LinesMouseEnter;
            Controller.LinesMouseLeave += Controller_LinesMouseLeave;
            Controller.LinesMouseMove += Controller_LinesMouseMove;

            Halo.MouseDown += Halo_MouseDown;
            Halo.MouseUp += Halo_MouseUp;
            Halo.MouseEnter += Halo_MouseEnter;
            Halo.MouseLeave += Halo_MouseLeave;
            Halo.MouseMove += Halo_MouseMove;
        }

        RouteLatLng halo;
        RouteController controller;
        Point MouseLocalPosition, MouseDownLocalPosition;
        PointLatLng MouseMapPosition, MouseDownMapPosition;
        Style haloStyle, linesStyle, verticesStyle;
        MapViewer map;
        double lengthInMeter;
        VertexLatLng MagnetVertex;
        int MagnetVertexIndex = -1;
        private bool isController = true;
        int zIndex = 0;
        double radius;

        public static double CeillingRadius = 2000000;
        public event EventHandler<PathDrawnEventArgs> RouteDrawn;
        public event MouseButtonEventHandler MouseDown;
        public event MouseButtonEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseLeave;
        public event MouseEventHandler MouseEnter;

        public readonly List<PointLatLng> Points = new List<PointLatLng>();

        public double Radius
        {
            get { return radius; }
            set { radius = value > CeillingRadius ? CeillingRadius : value;  }
        }

        public MapViewer Map
        {
            get { return map; }
            set
            {
                map = value;
                if (map != null)
                {
                    if (Halo != null)
                        Halo.Map = map.gmapControl;

                    if (Controller != null)
                        Controller.Map = map.gmapControl;

                    HaloStyle = Map.FindResource("HaloRoute") as Style;
                    LinesStyle = Map.FindResource("EdgePolygon") as Style;
                    VerticesStyle = Map.FindResource("VerticesPolygon") as Style;

                    Map.gmapControl.MouseDown += GmapControl_MouseDown;
                    Map.gmapControl.MouseMove += GmapControl_MouseMove;
                    Map.gmapControl.MouseUp += GmapControl_MouseUp;
                }
            }
        }

        public DrawingMode DrawMode { get; set; }

        public Style HaloStyle
        {
            get { return haloStyle; }
            set
            {
                haloStyle = value;
                if (Halo != null)
                    Halo.Style = haloStyle;
            }
        }

        public Style LinesStyle
        {
            get { return linesStyle; }
            set
            {
                linesStyle = value;
                if (Controller != null)
                    Controller.LinesStyle = linesStyle;
            }
        }

        public Style VerticesStyle
        {
            get { return verticesStyle; }
            set
            {
                verticesStyle = value;
                if (Controller != null)
                    Controller.VerticesStyle = verticesStyle;
            }
        }

        public double LengthInMeter { get { return lengthInMeter; } }

        public RouteLatLng Halo { get { return halo; } }
        public RouteController Controller { get { return controller; } }

        public bool IsController
        {
            get { return isController; }
            set { isController = value; }
        }

        public int ZIndex { get { return zIndex; } }

        /// <summary>
        /// افزودن نقطه به نقاط رئوس مسیر. پس از افزودن مجددا مسیر ویژه ساخته می شود.
        /// </summary>
        public void AddPoint(PointLatLng point)
        {
            Points.Add(point);
            InsertOneVertex(Points.Count - 1);
        }

        /// <summary>
        /// نقطه جدیدی جایگزین نقطه مشخص شده از رئوس مسیر می شود. پس از جایگزینی مجددا مسیر ویژه ساخته می شود.
        /// </summary>
        public bool ReplacePoint(int index, PointLatLng point)
        {
            if (index < Points.Count)
            {
                Points[index] = point;
                MoveOneVertex(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// نقطه جدید در ایندکس مشخص شده از رئوس مسیر درج می شود. پس از درج مجددا مسیر ویژه ساخته می شود.
        /// </summary>
        public void InsertPoint(int index, PointLatLng point)
        {
            if (index < this.Points.Count)
            {
                Points.Insert(index, point);
                InsertOneVertex(index);
            }
            else
            {
                AddPoint(point);
            }
        }

        /// <summary>
        /// نقطه مشخص شده از رئوس مسیر حذف می شود. پس از حذف مجددا مسیر ویژه ساخته می شود.
        /// </summary>
        public Boolean RemovePoint(PointLatLng point)
        {
            int index = Points.IndexOf(point);
            bool result = Points.Remove(point);

            if (result)
            {
                DeleteOneVertex(index);
            }
            return result;
        }

        /// <summary>
        /// ایندکس مشخص شده از رئوس مسیر حذف می شود. پس از حذف مجددا مسیر ویژه ساخته می شود.
        /// </summary>
        public void RemoveAtPoint(int index)
        {
            Points.RemoveAt(index);
            DeleteOneVertex(index);
        }

        /// <summary>
        /// تمام نقاط رئوس مسیر را حذف می کند.  پس از حذف مجددا مسیر ویژه ساخته می شود.
        /// </summary>
        public void Clear()
        {
            Points.Clear();
            CreateShape();
        }

        /// <summary>
        /// مسیر ویژه ایجاد و روی نقشه نمایش داده می شود.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public virtual void CreateShape()
        {
            PathDrawnEventArgs args = new PathDrawnEventArgs();

            Erase();
            if (Points?.Count > 1)
            {
                Halo.Points = Points;
                Halo.Radius = Radius;

                Controller.Points = Points;

                if (IsController)
                    Controller.CreateController();
                else
                    Controller.Delete();

                args.Points = Points;
                args.WidthInMeters = Radius * 2;
                lengthInMeter = args.LengthInMeters = Halo.LengthInMeter;
                FindInnerPolygon(args);

                Draw();

                Map.SendToFront(this);
            }
            if (DrawMode == DrawingMode.End && Points.Count == 1)
            {
                Delete();
            }
            OnRouteDrawn(args);
        }

        private void FindInnerPolygon(PathDrawnEventArgs args)
        {
            var acc = (Radius < 10000) ? DrawPointAccuracy.Medium : DrawPointAccuracy.VeryLow;
            foreach (var innerPolygon in ConvertToPolygon(acc))
            {
                args.InnerPolygons.Add(innerPolygon.Points);
            }
        }

        /// <summary>
        /// پس از جابجایی یک نقطه در شکل تغییرات لازم مرتبط با شکل را اعمال می کند.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        /// <param name="numVertex">ایندکس نقظه</param>
        private void MoveOneVertex(int numVertex)
        {
            PathDrawnEventArgs args = new PathDrawnEventArgs();
            Halo.Erase();
            Controller.EraseAfterMoveOneVertex(numVertex);

            if (Points?.Count > 1)
            {
                Halo.Points = Points;
                Halo.Radius = Radius;

                Controller.Points[numVertex] = Points[numVertex];

                if (IsController)
                    Controller.MoveOneVertex(numVertex);
                else
                    Controller.Delete();

                args.Points = Points;
                args.WidthInMeters = 2 * Radius;
                lengthInMeter = args.LengthInMeters = Halo.LengthInMeter;
                FindInnerPolygon(args);

                Halo.Draw();
                if (IsController)
                    Controller.DrawAfterMoveOneVertex(numVertex);
                else
                    Controller.Delete();

                Map.SendToFront(this);
            }
            else if (DrawMode == DrawingMode.End && Points.Count == 1)
            {
                Delete();
            }

            OnRouteDrawn(args);
        }

        /// <summary>
        /// پس از افزوده شدن نقطه در محل مورد نظر تغییرات لازم را در شکل اعمال می کند.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        /// <param name="numVertex">ایندکس نقظه</param>
        private void InsertOneVertex(int numVertex)
        {
            if (Controller.Lines.Count > 0)
            {
                PathDrawnEventArgs args = new PathDrawnEventArgs();
                Halo.Erase();

                if (Points?.Count > 1)
                {
                    LineLatLng oldLine = (numVertex > 1) ? Controller.Lines[numVertex - 2] : null;
                    
                    oldLine?.Erase();

                    Halo.Points = Points;
                    Halo.Radius = Radius;

                    if (IsController)
                    {
                        Controller.Points = Points;
                        Controller.InsertOneVertex(numVertex);
                    }
                    else
                        Controller.Delete();

                    args.Points = Points;
                    args.WidthInMeters = Radius * 2;
                    lengthInMeter = args.LengthInMeters = Halo.LengthInMeter;
                    FindInnerPolygon(args);

                    Halo.Draw();
                    if (IsController)
                    {
                        oldLine?.Draw();
                        if (numVertex < Controller.Lines.Count)
                            Controller.Lines[numVertex]?.Draw();
                    }
                }
                else if (DrawMode == DrawingMode.End && Points.Count == 1)
                {
                    Delete();
                }
                OnRouteDrawn(args);
            }
            else
            {
                CreateShape();
            }
        }

        /// <summary>
        /// پس از حذف شدن یک نقطه تغییرات لازم را در شکل اعمال می کند.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        /// <param name="numVertex">ایندکس نقظه</param>
        private void DeleteOneVertex(int numVertex)
        {
            PathDrawnEventArgs args = new PathDrawnEventArgs();
            Halo.Erase();

            if (Points?.Count > 1)
            {
                LineLatLng oldLine = (numVertex > 0) ? Controller.Lines[numVertex - 1] : null;

                oldLine?.Erase();

                Halo.Points = Points;
                Halo.Radius = Radius;
                Controller.Points = Points;

                if (IsController)
                {
                    Controller.Points = Points;
                    Controller.DeleteOneVertex(numVertex);
                }
                else
                    Controller.Delete();

                args.Points = Points;
                args.WidthInMeters = Radius * 2;
                lengthInMeter = args.LengthInMeters = Halo.LengthInMeter;
                FindInnerPolygon(args);

                Halo.Draw();
                if (IsController)
                    oldLine?.Draw();
            }
            else if (DrawMode == DrawingMode.End && Points.Count == 1)
            {
                Delete();
            }
            OnRouteDrawn(args);
        }

        /// <summary>
        /// تبدیل مسیر ویژه به مسیر معمولی
        /// </summary>
        /// <returns>مسیر معمولی بدون کنترلر</returns>
        public RouteLatLng ConvertToRouteLatLng()
        {
            RouteLatLng route = new RouteLatLng(new List<PointLatLng>(), Radius) { Map = Map.gmapControl, Style = Map.FindResource("LockRoute") as Style };
            foreach (var p in Points)
            {
                route.Points.Add(p);
            }
            return route;
        }

        /// <summary>
        /// هاله دور مسیر را به ازای هر خط از مسیر به یک چند ضلعی تبدیل می کند.
        /// قسمت های ابتدا و انتهای خطوط با توجه به دقت خواسته شده گرد می شوند.
        /// </summary>
        /// <param name="Accuracy">دقت گرد شدن ابتدا و انتهای هاله</param>
        /// <returns>مجموعه چند ضلعی ها</returns>
        public List<PolygonLatLng> ConvertToMultiPolygon(DrawPointAccuracy Accuracy)
        {
            LineLatLng[] upLines = new LineLatLng[Controller.Lines.Count];
            LineLatLng[] downLines = new LineLatLng[Controller.Lines.Count];
            List<PolygonLatLng> polygons = new List<PolygonLatLng>();

            for (int i = 0; i < Controller.Lines.Count; i++)
            {
                PolygonLatLng polygon = new PolygonLatLng(new List<PointLatLng>()) { Map = Map.gmapControl, Style = Map.FindResource("LockPolygon") as Style };

                var line = Controller.Lines[i];

                upLines[i] = MoveLine(line, Radius, GeoMath.Perpendicular(line.Angle));

                downLines[i] = MoveLine(line, -Radius, GeoMath.Perpendicular(line.Angle));

                polygon.Points.AddRange(FindHaloPoints(downLines[i], upLines[i], line.StartPoint, Radius, Accuracy, HaloPointMode.Start));
                polygon.Points.AddRange(FindHaloPoints(upLines[i], downLines[i], line.EndPoint, Radius, Accuracy, HaloPointMode.End));

                polygons.Add(polygon);
            }

            return polygons;
        }

        /// <summary>
        /// هاله دور مسیر را به یک چند ضلعی تبدیل می کند.
        /// اگر قسمت های از چند ضلعی ایجاد شده با هم مشترک بود به ازای هز خط یک چندضلعی ایجاد می کند.
        /// قسمت های ابتدا و انتهای خطوط با توجه به دقت خواسته شده گرد می شوند.
        /// </summary>
        /// <param name="Accuracy">دقت گرد شدن ابتدا و انتهای هاله</param>
        /// <returns>یک یا چندین چند ضلعی </returns>
        public List<PolygonLatLng> ConvertToPolygon(DrawPointAccuracy Accuracy)
        {
            LineLatLng[] upLines = new LineLatLng[Controller.Lines.Count];
            LineLatLng[] downLines = new LineLatLng[Controller.Lines.Count];
            PolygonLatLng polygon = new PolygonLatLng(new List<PointLatLng>()) { Map = Map.gmapControl, Style = Map.FindResource("LockPolygon") as Style };

            for (int i = 0; i < Controller.Lines.Count; i++)
            {

                var line = Controller.Lines[i];

                upLines[i] = MoveLine(line, Radius, GeoMath.Perpendicular(line.Angle));

                downLines[i] = MoveLine(line, -Radius, GeoMath.Perpendicular(line.Angle));

                if (i == 0)
                {
                    polygon.Points.AddRange(FindHaloPoints(downLines[i], upLines[i], line.StartPoint, Radius, Accuracy, HaloPointMode.Start));
                }
                else
                {
                    polygon.Points.AddRange(FindHaloPoints(upLines[i - 1], upLines[i], line.StartPoint, Radius, Accuracy, HaloPointMode.Center));
                }
                if (i == Controller.Lines.Count - 1)
                {
                    polygon.Points.AddRange(FindHaloPoints(upLines[i], downLines[i], line.EndPoint, Radius, Accuracy, HaloPointMode.End));
                }
            }

            for (int i = Controller.Lines.Count - 2; i >= 0; i--)
            {
                var line = Controller.Lines[i];
                LineLatLng d0 = new LineLatLng(downLines[i + 1].EndPoint, downLines[i + 1].StartPoint);
                LineLatLng d1 = new LineLatLng(downLines[i].EndPoint, downLines[i].StartPoint);

                polygon.Points.AddRange(FindHaloPoints(d0, d1, line.EndPoint, Radius, Accuracy, HaloPointMode.Center));
            }

            List<CrossPoint> crosses = polygon.CrossPoints();
            List<CoincidentLine> Coins = polygon.CoincidentLines();

            if (crosses.Count > 0 || Coins.Count > 0)
                return ConvertToMultiPolygon(Accuracy);

            return new List<PolygonLatLng>() { polygon };
        }

        enum HaloPointMode
        {
            Start = 0,
            Center = 1,
            End = 2
        }

        private List<PointLatLng> FindHaloPoints(LineLatLng l1, LineLatLng l2, PointLatLng center, double r, DrawPointAccuracy Accuracy, HaloPointMode mode)
        {
            double degree = PrecisionAngle(Accuracy);
            List<PointLatLng> points = new List<PointLatLng>();

            PointLatLng cross = l1.CrossPoint(l2);
            double A = GeoMath.BetweenZero360(l1.Angle - l2.Angle);
            if (mode == HaloPointMode.Center && cross.IsEmpty && A > 180)
            {
                return points;
            }

            if (cross.IsEmpty)
            {
                double s, e;
                s = e = 0;
                if (mode == HaloPointMode.Start)
                {
                    s = GeoMath.BetweenZero360(Math.Round(GeoMath.Bearing(center, l1.StartPoint)));
                    e = GeoMath.BetweenZero360(Math.Round(GeoMath.Bearing(center, l2.StartPoint)));

                }
                else if (mode == HaloPointMode.Center)
                {
                    s = GeoMath.BetweenZero360(Math.Round(GeoMath.Bearing(center, l1.EndPoint)));
                    e = GeoMath.BetweenZero360(Math.Round(GeoMath.Bearing(center, l2.StartPoint)));
                }
                else
                {
                    s = GeoMath.BetweenZero360(Math.Round(GeoMath.Bearing(center, l1.EndPoint)));
                    e = GeoMath.BetweenZero360(Math.Round(GeoMath.Bearing(center, l2.EndPoint)));
                }

                double l1Ang = GeoMath.BetweenZero360(Math.Round(l1.Angle));

                if (Math.Round(Math.Abs(s - e), 2) > 180
                    || (mode == HaloPointMode.Start && l1Ang >= 90 && l1Ang < 270)
                    || (mode == HaloPointMode.End && GeoMath.BetweenZero360(l1Ang + 180) >= 90 && GeoMath.BetweenZero360(l1Ang + 180) < 270))
                {
                    s = GeoMath.BetweenZero360(180 + s);
                    e = GeoMath.BetweenZero360(180 + e);
                    r = -r;
                }

                if (s > e)
                {
                    for (double i = s; i > e; i = i - degree)
                    {
                        PointLatLng p1 = GeoMath.RadialPoint(center, r, i);
                        points.Add(p1);
                    }
                }
                else
                {
                    for (double i = s; i < e; i = i + degree)
                    {
                        PointLatLng p1 = GeoMath.RadialPoint(center, r, i);
                        points.Add(p1);
                    }
                }
                points.Add(GeoMath.RadialPoint(center, r, e));
            }
            else
            {
                points.Add(cross);
            }
            return points;
        }

        private double PrecisionAngle(DrawPointAccuracy accuracy)
        {
            if (accuracy == DrawPointAccuracy.VeryLow) //بازای هر 10000000 متر شعاع با دقت 1 درجه
                return 10000000 / Radius;
            else if (accuracy == DrawPointAccuracy.Low) //بازای هر 1000000 متر شعاع با دقت 1 درجه
                return 1000000 / Radius;
            else if (accuracy == DrawPointAccuracy.Medium) //بازای هر 100000 متر شعاع با دقت 1 درجه
                return 100000 / Radius;
            else if (accuracy == DrawPointAccuracy.High) //بازای هر 10000 متر شعاع با دقت 1 درجه
                return 10000 / Radius;
            else //بازای هر 1000 متر شعاع با دقت 1 درجه
                return 1000 / Radius;
        }

        private LineLatLng MoveLine(LineLatLng line, double meter, double bearing)
        {
            return new LineLatLng(GeoMath.RadialPoint(line.StartPoint, meter, bearing), GeoMath.RadialPoint(line.EndPoint, meter, bearing)) { Map = Map.gmapControl };
        }

        /// <summary>
        /// اولویت نمایش شکل ویژه را تعیین می کند.
        /// به این صورت که قسمت تو پر در پایین ترین اولویت شکل قرار گرفته سپس خطوط حاشیه شکل و نهایتا نقاط در بالاترین اولویت قرار می گیرند.
        /// </summary>
        /// <param name="minIndex">ایندکس قسمت توپر شکل ویژه که در کمترین اولویت از شکل قرار دارد.</param>
        /// <returns>ایندکس آخرین عنصر اولویت بندی شده رو بر می گرداند.</returns>
        public int SendToFront(int minIndex)
        {
            Halo.ZIndex = minIndex++;
            zIndex = Halo.ZIndex;

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
            Halo?.Delete();
            Controller?.Delete();
            Map?.AdvanceShapes?.Remove(this);

            if (Map.AdvanceShapes.Count > 0)
                Map.SelectedAdvanceShape = Map?.AdvanceShapes.Last();
            else
                Map.SelectedAdvanceShape = null;
        }

        public virtual void OnRouteDrawn(PathDrawnEventArgs args)
        {
            RouteDrawn?.Invoke(this, args);
        }

        /// <summary>
        /// مسیر ویژه روی نقشه نشان می دهد.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            Halo.Draw();

            Controller.Draw();
        }

        /// <summary>
        /// مسیر ویژه را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            Halo.Erase();

            Controller.Erase();
        }

        /// <summary>
        /// این متد برای مسیر ویژه غیرفعال است.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public virtual void Move(double lat, double lng)
        {

        }

        private void Halo_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void Controller_VerticesMouseMove(object sender, VerticesMouseEventArgs e)
        {
            MouseMove?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_LinesMouseMove(object sender, LinesMouseEventArgs e)
        {
            MouseMove?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_LinesMouseLeave(object sender, LinesMouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_VerticesMouseLeave(object sender, VerticesMouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e.MouseEventArgs);
        }

        private void Halo_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void Halo_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        private void Controller_VerticesMouseEnter(object sender, VerticesMouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_LinesMouseEnter(object sender, LinesMouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e.MouseEventArgs);
        }

        private void Halo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void Controller_LinesMouseUp(object sender, LinessMouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e.MouseButtonEventArgs);
        }

        private void Controller_VerticesMouseUp(object sender, VerticesMouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e.MouseButtonEventArgs);
        }

        private void Halo_MouseDown(object sender, MouseButtonEventArgs e)
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
            if (DrawMode == DrawingMode.MovingPoint)
            {
                ReplacePoint(MagnetVertexIndex, MouseMapPosition);
                MagnetVertex = null;
                MagnetVertexIndex = -1;
                DrawMode = DrawingMode.End;
            }
        }

        private void Controller_LinesMouseDown(object sender, LinessMouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e.MouseButtonEventArgs);

            Point p = e.MouseButtonEventArgs.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.MouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                e.MouseButtonEventArgs.Handled = true;

                if (DrawMode == DrawingMode.End)
                {
                    InsertPoint(e.Index + 1, mp);
                    MagnetVertex = Controller.Vertices[e.Index + 1];
                    MagnetVertexIndex = e.Index + 1;
                    DrawMode = DrawingMode.MovingPoint;
                }
            }

            MouseDownLocalPosition = p;
            MouseDownMapPosition = mp;
        }

        private void Controller_VerticesMouseDown(object sender, VerticesMouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e.MouseButtonEventArgs);

            Point p = e.MouseButtonEventArgs.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.MouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                e.MouseButtonEventArgs.Handled = true;

                if (e.MouseButtonEventArgs.ClickCount == 1)
                {
                    if (DrawMode == DrawingMode.Drawing)
                    {
                        DrawMode = DrawingMode.SetPoint;
                    }
                    if (DrawMode == DrawingMode.End)
                    {
                        MagnetVertex = e.Vertex;
                        MagnetVertexIndex = e.Index;
                        DrawMode = DrawingMode.MovingPoint;
                    }
                }
                else if (e.MouseButtonEventArgs.ClickCount == 2)
                {
                    if (DrawMode == DrawingMode.SetPoint || DrawMode == DrawingMode.Drawing)
                    {
                        ReplacePoint(Points.Count - 1, mp);
                        DrawMode = DrawingMode.End;
                    }
                    else if (DrawMode == DrawingMode.End)
                    {
                        RemovePoint(e.Vertex.Point);
                    }
                }
            }

            MouseDownLocalPosition = p;
            MouseDownMapPosition = mp;
        }

        private void GmapControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 1)
                {
                    if (DrawMode == DrawingMode.Drawing)
                    {
                        DrawMode = DrawingMode.SetPoint;
                    }
                }
            }

            MouseDownLocalPosition = p;
            MouseDownMapPosition = mp;
        }

        private void GmapControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (DrawMode == DrawingMode.SetPoint)
            {
                AddPoint(mp);
                DrawMode = DrawingMode.Drawing;
            }
            else if (DrawMode == DrawingMode.Drawing)
            {
                ReplacePoint(Points.Count - 1, mp);
            }
            else if (DrawMode == DrawingMode.MovingPoint)
            {
                ReplacePoint(MagnetVertexIndex, mp);
            }

            MouseLocalPosition = p;
            MouseMapPosition = mp;
        }
    }
}
