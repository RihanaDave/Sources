using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class AdvancePolygon : IAdvanceShape
    {
        /// <summary>
        /// ایجاد چندضلعی ویژه
        /// </summary>
        /// <param name="points">نقاط رئوس چندضلعی</param>
        public AdvancePolygon(List<PointLatLng> points) 
        {
            Points = points;
            DrawMode = DrawingMode.Ready;

            fill = new PolygonLatLng(Points);
            controller = new PolygonController(Points);

            MouseDown += AdvancePolygon_MouseDown;

            Fill.MouseDown += Fill_MouseDown;
            Fill.MouseEnter += Fill_MouseEnter;
            Fill.MouseLeave += Fill_MouseLeave;
            Fill.MouseMove += Fill_MouseMove;
            Fill.MouseUp += Fill_MouseUp;

            Controller.StrokeMouseDown += Controller_StrokeMouseDown;
            Controller.StrokeMouseEnter += Controller_StrokeMouseEnter;
            Controller.StrokeMouseLeave += Controller_StrokeMouseLeave;
            Controller.StrokeMouseMove += Controller_StrokeMouseMove;
            Controller.StrokeMouseUp += Controller_StrokeMouseUp;

            Controller.VerticesMouseDown += Controller_VerticesMouseDown;
            Controller.VerticesMouseEnter += Controller_VerticesMouseEnter;
            Controller.VerticesMouseLeave += Controller_VerticesMouseLeave;
            Controller.VerticesMouseMove += Controller_VerticesMouseMove;
            Controller.VerticesMouseUp += Controller_VerticesMouseUp;
        }

        private void AdvancePolygon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Controller.CheckCoincidentLines = true;
            Controller.CheckCrossPoints = true;
        }

        PolygonLatLng fill;
        PolygonController controller;
        Point MouseLocalPosition, MouseDownLocalPosition;
        PointLatLng MouseMapPosition, MouseDownMapPosition;
        Style fillStyle, strokeStyle, verticesStyle, crossPointsStyle, crossLinesStyle, coincidentLinesStyle, magnetVertexStyle, fillHoverStyle;
        MapViewer map;
        double perimeterInMeters = 0;

        private VertexLatLng MagnetVertex;
        private int MagnetVertexIndex = -1;
        bool isController = true;
        bool StartMove = false;
        int zIndex = 0;

        public readonly List<PointLatLng> Points = new List<PointLatLng>();

        public double PerimeterInMeters { get { return perimeterInMeters; } }

        public MapViewer Map
        {
            get { return map; }
            set
            {
                map = value;

                if (map != null)
                {
                    if (Fill != null)
                        Fill.Map = map.gmapControl;

                    if (Controller != null)
                        Controller.Map = map.gmapControl;

                    FillStyle = Map.FindResource("FillPolygon") as Style;
                    StrokeStyle = Map.FindResource("EdgePolygon") as Style;
                    VerticesStyle = Map.FindResource("VerticesPolygon") as Style;
                    CrossPointsStyle = Map.FindResource("VertexCrossPoint") as Style;
                    CrossLinesStyle = Map.FindResource("CrossEdgePolygon") as Style;
                    CoincidentLinesStyle = Map.FindResource("CoincidentEdgePolygon") as Style;
                    magnetVertexStyle = Map.FindResource("VertexPolygonMagnet") as Style;
                    FillHoverStyle = Map.FindResource("FillPolygonSelect") as Style;

                    Map.gmapControl.MouseDown += GmapControl_MouseDown;
                    Map.gmapControl.MouseMove += GmapControl_MouseMove;
                    Map.gmapControl.MouseUp += GmapControl_MouseUp;
                }
            }
        }

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
        public Style CrossPointsStyle
        {
            get { return crossPointsStyle; }
            set
            {
                crossPointsStyle = value;
                if (Controller != null)
                    Controller.CrossPointsStyle = CrossPointsStyle;
            }
        }

        public Style CrossLinesStyle
        {
            get { return crossLinesStyle; }
            set
            {
                crossLinesStyle = value;
                if (Controller != null)
                    Controller.CrossLinesStyle = crossLinesStyle;
            }
        }

        public Style CoincidentLinesStyle
        {
            get { return coincidentLinesStyle; }
            set
            {
                coincidentLinesStyle = value;
                if (Controller != null)
                    Controller.CoincidentLinesStyle = coincidentLinesStyle;
            }
        }

        public Style MagnetVertexStyle
        {
            get { return magnetVertexStyle; }
            set
            {
                magnetVertexStyle = value;
                if (MagnetVertex != null)
                    MagnetVertex.Style = magnetVertexStyle;
            }
        }

        public Style FillHoverStyle
        {
            get { return fillHoverStyle; }
            set
            {
                fillHoverStyle = value;
            }
        }

        public int ZIndex { get { return zIndex; } }

        public bool IsAnyVectorCross { get { return Controller?.CrossPoints?.Count > 0; } }
        public bool IsAnyVectorCoincident { get { return Controller?.CoincidentLines?.Count > 0; } }

        public DrawingMode DrawMode { get; set; }
        public PolygonLatLng Fill { get { return fill; } }
        public PolygonController Controller { get { return controller; } }

        public bool IsController
        {
            get { return isController; }
            set { isController = value; }
        }

        public event MouseButtonEventHandler MouseDown;
        public event MouseButtonEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseLeave;
        public event MouseEventHandler MouseEnter;

        public event EventHandler<PolygonDrawnEventArgs> PolygonDrawn;

        public virtual void OnPolygonDrawn(PolygonDrawnEventArgs args)
        {
            PolygonDrawn?.Invoke(this, args);
        }

        /// <summary>
        /// افزودن نقطه به نقاط رئوس چندضلعی. پس از افزودن مجددا چندضلعی ویژه ساخته می شود.
        /// </summary>
        public void AddPoint(PointLatLng point)
        {
            Points.Add(point);
            InsertOneVertex(Points.Count - 1);
        }

        /// <summary>
        /// نقطه جدیدی جایگزین نقطه مشخص شده از رئوس چندضلعی می شود. پس از جایگزینی مجددا چندضلعی ویژه ساخته می شود.
        /// </summary>
        /// <param name="index">ایندکس نقطه (زیرو بیس)</param>
        /// <param name="point">نقطه جدید</param>
        /// <returns>در صورت موفقست true در غیر این صورت false برمیگرداند.</returns>
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
        /// نقطه جدید در ایندکس مشخص شده از رئوس چندضلعی درج می شود. پس از درج مجددا چندضلعی ویژه ساخته می شود.
        /// </summary>
        /// <param name="index">ایندکس نقطه (زیرو بیس)</param>
        /// <param name="point">نقطه جدید</param>
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
        /// نقطه مشخص شده از رئوس چندضلعی حذف می شود. پس از حذف مجددا چندضلعی ویژه ساخته می شود.
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
        /// ایندکس مشخص شده از رئوس چندضلعی حذف می شود. پس از حذف مجددا چندضلعی ویژه ساخته می شود.
        /// </summary>
        public void RemoveAtPoint(int index)
        {
            Points.RemoveAt(index);
            DeleteOneVertex(index);
        }

        /// <summary>
        /// تمام نقاط رئوس چندضلعی را حذف می کند.  پس از حذف مجددا چندضلعی ویژه ساخته می شود.
        /// </summary>
        public void Clear()
        {
            Points.Clear();
            CreateShape();
        }

        /// <summary>
        /// چندضلعی ویژه ایجاد و روی نقشه نمایش داده می شود.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public virtual void CreateShape()
        {
            PolygonDrawnEventArgs args = new PolygonDrawnEventArgs();
            
            Erase();
            if (Points?.Count > 1)
            {
                Fill.Points = Points;

                Controller.Points = Points;

                if (IsController && !StartMove)
                    Controller.CreateController();
                else
                    Controller.Delete();

                args.Points = Points;
                args.isAnyVectorCoincident = IsAnyVectorCoincident;
                args.isAnyVectorCrossed = IsAnyVectorCross;
                perimeterInMeters = args.perimeterInMeters = Fill.PerimeterInMeters;

                Draw();

                Map.SendToFront(this);
            }
            else if (DrawMode == DrawingMode.End && Points.Count == 1)
            {
                Delete();
            }
            OnPolygonDrawn(args);
        }

        /// <summary>
        /// پس از جابجایی یک نقطه در شکل تغییرات لازم مرتبط با شکل را اعمال می کند.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        /// <param name="numVertex">ایندکس نقظه</param>
        private void MoveOneVertex(int numVertex)
        {
            PolygonDrawnEventArgs args = new PolygonDrawnEventArgs();
            Fill.Erase();
            Controller.EraseAfterMoveOneVertex(numVertex);

            if (Points?.Count > 1)
            {
                Fill.Points = Points;

                Controller.Points[numVertex] = Points[numVertex];

                if (IsController)
                    Controller.MoveOneVertex(numVertex);
                else
                    Controller.Delete();

                args.Points = Points;
                args.isAnyVectorCoincident = IsAnyVectorCoincident;
                args.isAnyVectorCrossed = IsAnyVectorCross;
                perimeterInMeters = args.perimeterInMeters = Fill.PerimeterInMeters;
                
                Fill.Draw();
                if (IsController)
                    Controller.DrawAfterMoveOneVertex(numVertex);
                else
                    Controller.Delete();
            }
            else if (DrawMode == DrawingMode.End && Points.Count == 1)
            {
                Delete();
            }

            OnPolygonDrawn(args);
        }

        /// <summary>
        /// پس از افزوده شدن نقطه در محل مورد نظر تغییرات لازم را در شکل اعمال می کند.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        /// <param name="numVertex">ایندکس نقظه</param>
        private void InsertOneVertex(int numVertex)
        {
            if (Controller.Stroke.Count > 0)
            {
                PolygonDrawnEventArgs args = new PolygonDrawnEventArgs();
                Fill.Erase();

                if (Points?.Count > 1)
                {
                    LineLatLng oldLine;

                    if (numVertex == 0)
                    {
                        oldLine = Controller.Stroke[Points.Count - 1];
                    }
                    else
                    {
                        oldLine = Controller.Stroke[numVertex - 1];
                    }
                    oldLine.Erase();

                    Fill.Points = Points;

                    if (IsController)
                    {
                        Controller.Points = Points;
                        Controller.InsertOneVertex(numVertex);
                    }
                    else
                        Controller.Delete();

                    args.Points = Points;
                    args.isAnyVectorCoincident = IsAnyVectorCoincident;
                    args.isAnyVectorCrossed = IsAnyVectorCross;
                    perimeterInMeters = args.perimeterInMeters = Fill.PerimeterInMeters;

                    Fill.Draw();
                    if (IsController)
                    {
                        oldLine.Draw();
                        Controller.Stroke[numVertex].Draw();
                    }
                }
                else if (DrawMode == DrawingMode.End && Points.Count == 1)
                {
                    Delete();
                }
                OnPolygonDrawn(args);
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
            PolygonDrawnEventArgs args = new PolygonDrawnEventArgs();
            Fill.Erase();
            //Controller.EraseAfterMoveOneVertex(numVertex);

            if (Points?.Count > 1)
            {
                LineLatLng oldLine;

                if (numVertex == 0)
                {
                    oldLine = Controller.Stroke[Controller.Stroke.Count - 1];
                }
                else
                {
                    oldLine = Controller.Stroke[numVertex - 1];
                }
                oldLine.Erase();

                Fill.Points = Points;

                if (IsController)
                {
                    Controller.Points = Points;
                    Controller.DeleteOneVertex(numVertex);
                }
                else
                    Controller.Delete();

                args.Points = Points;
                args.isAnyVectorCoincident = IsAnyVectorCoincident;
                args.isAnyVectorCrossed = IsAnyVectorCross;
                perimeterInMeters = args.perimeterInMeters = Fill.PerimeterInMeters;

                Fill.Draw();
                if (IsController)
                    oldLine.Draw();
            }
            else if (DrawMode == DrawingMode.End && Points.Count == 1)
            {
                Delete();
            }
            OnPolygonDrawn(args);
        }

        /// <summary>
        /// نقاط شکل را بهینه می کند. به این صورت که خطوط پشت سر هم با شیب یکسان را ب ه صورت یک خط در نظر می گیرد. معیار یکسانی شیب خطوط براساس ضریب دقت بررسی میشود.
        /// </summary>
        /// <param name="input">نقاط ورودی</param>
        /// <param name="accuracy">درجه دقت شیب خطوط</param>
        /// <returns>نقاط خروجی</returns>
        public static List<PointLatLng> OptimizationPoints(List<PointLatLng> input, DrawPointAccuracy accuracy)
        {
            if (input == null || input.Count < 3) return input;

            List<PointLatLng> output = new List<PointLatLng>() { input[0] };
            var p1 = input[1];

            for (int i = 1; i < input.Count - 1; i++)
            {
                var p0 = output.Last();
                var p2 = input[i + 1];

                var b1 = GeoMath.Bearing(p0, p1);
                var b2 = GeoMath.Bearing(p1, p2);

                if (Math.Abs(b1 - b2) > PrecisionAngle(accuracy, GeoMath.Distance(p0, p2)))
                {
                    output.Add(p2);
                    p1 = (i + 2 >= input.Count - 1) ? input[i + 1] : input[i + 2];
                }
            }

            output.Add(input.Last());

            return output;
        }

        /// <summary>
        /// حداقل زاویه مجاز بین خطوط پشت سرهم برای بهینه سازی نقاط را محاسبه می کند. این محاسبه بر اساس طول خط و ضریب دقت صورت می پذیرد
        /// </summary>
        /// <param name="accuracy">درجه دقت شیب خطوط</param>
        /// <param name="length">طول خط</param>
        /// <returns></returns>
        static double PrecisionAngle(DrawPointAccuracy accuracy, double length)
        {
            if (accuracy == DrawPointAccuracy.VeryLow) //بازای هر 300000 متر شعاع با دقت 1 درجه
                return 300000 / length;
            else if (accuracy == DrawPointAccuracy.Low) //بازای هر 100000 متر شعاع با دقت 1 درجه
                return 100000 / length;
            else if (accuracy == DrawPointAccuracy.Medium) //بازای هر 50000 متر شعاع با دقت 1 درجه
                return 50000 / length;
            else if (accuracy == DrawPointAccuracy.High) //بازای هر 20000 متر شعاع با دقت 1 درجه
                return 20000 / length;
            else //بازای هر 10000 متر شعاع با دقت 1 درجه
                return 10000 / length;
        }

        /// <summary>
        /// تبدیل چندضلعی ویژه به چندضلعی معمولی
        /// </summary>
        /// <returns>چندضلعی معمولی بدون کنترلر</returns>
        public PolygonLatLng ConvertToPolygonLatLng()
        {
            PolygonLatLng polygon = new PolygonLatLng(new List<PointLatLng>()) { Map = Map.gmapControl, Style = Map.FindResource("LockPolygon") as Style };
            foreach (var p in Points)
            {
                polygon.Points.Add(p);
            }
            return polygon;
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
        /// چندضلعی بر حسب طول و عرض جغرافیایی جابجا می شود.
        /// </summary>
        /// <param name="lat">عرض جغرافیایی</param>
        /// <param name="lng">طول جغرافیایی</param>
        public virtual void Move(double lat, double lng)
        {
            double northern, southern, western, eastern;
            northern = southern = Points[0].Lat;
            western = eastern = Points[0].Lng;

            for (int i = 1; i < Points.Count; i++)
            {
                if (Points[i].Lat > northern)
                    northern = Points[i].Lat;
                else if (Points[i].Lat < southern)
                    southern = Points[i].Lat;

                if (Points[i].Lng > eastern)
                    eastern = Points[i].Lng;
                else if (Points[i].Lng < western)
                    western = Points[i].Lng;
            }

            if (eastern + lng > Map.EasternMostDegree)
            {
                lng = Map.EasternMostDegree - eastern;
            }
            if (western + lng < Map.WesternMostDegree)
            {
                lng = Map.WesternMostDegree - western;
            }
            if (northern + lat > Map.NorthernMostDegree)
            {
                lat = Map.NorthernMostDegree - northern;
            }
            if (southern + lat < Map.SouthernMostDegree)
            {
                lat = Map.SouthernMostDegree - southern;
            }

            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new PointLatLng(Points[i].Lat + lat, Points[i].Lng + lng);
            }
            StartMove = true;
            CreateShape();
        }

        /// <summary>
        /// چندضلعی ویژه روی نقشه نشان می دهد.اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public virtual void Draw()
        {
            Fill.Draw();
            if (IsController)
                Controller.Draw();
        }

        /// <summary>
        /// چندضلعی را از روی نقشه پاک می کند.
        /// </summary>
        public virtual void Erase()
        {
            Fill.Erase();
            Controller.Erase();
        }

        private void Controller_VerticesMouseUp(object sender, VerticesMouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e.MouseButtonEventArgs);
        }

        private void Controller_VerticesMouseMove(object sender, VerticesMouseEventArgs e)
        {
            MouseMove?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_VerticesMouseLeave(object sender, VerticesMouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_VerticesMouseEnter(object sender, VerticesMouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e.MouseEventArgs);
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

        private void Controller_StrokeMouseUp(object sender, LinessMouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e.MouseButtonEventArgs);
        }

        private void Controller_StrokeMouseMove(object sender, LinesMouseEventArgs e)
        {
            MouseMove?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_StrokeMouseLeave(object sender, LinesMouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_StrokeMouseEnter(object sender, LinesMouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e.MouseEventArgs);
        }

        private void Controller_StrokeMouseDown(object sender, LinessMouseButtonEventArgs e)
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

        private void Fill_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
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

        private void Fill_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);

            Point p = e.GetPosition(Map.gmapControl);
            PointLatLng mp = Map.gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;

                DrawMode = DrawingMode.MovingShape;
                Fill.Style = FillHoverStyle;
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

        private void GmapControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DrawMode == DrawingMode.MovingPoint)
            {
                ReplacePoint(MagnetVertexIndex, MouseMapPosition);
                MagnetVertex = null;
                MagnetVertexIndex = -1;
                DrawMode = DrawingMode.End;
            }
            else if (DrawMode == DrawingMode.MovingShape)
            {
                DrawMode = DrawingMode.End;
                Fill.Style = FillStyle;
                if (StartMove)
                {
                    StartMove = false;
                    CreateShape();
                }
            }
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
            else if (DrawMode == DrawingMode.MovingShape)
            {
                double dLat = mp.Lat - MouseMapPosition.Lat;
                double dLng = mp.Lng - MouseMapPosition.Lng;

                Move(dLat, dLng);
            }

            MouseLocalPosition = p;
            MouseMapPosition = mp;
        }
    }
}
