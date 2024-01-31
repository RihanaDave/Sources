
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace GPAS.MapViewer
{
    public class PolygonController
    {
        /// <summary>
        /// ایجاد کنترلر چند ضلعی
        /// </summary>
        /// <param name="points">نقاط رئوس چندضلعی</param>
        public PolygonController(List<PointLatLng> points)
        {
            Points = points;
            CheckCoincidentLines = true;
            CheckCrossPoints = true;

            CreateController();
        }

        Style strokeStyle, verticesStyle, crossPointsStyle, crossLinesStyle, coincidentLinesStyle;
        GMapControl map;

        public event VerticesMouseEventHandler VerticesMouseEnter;
        public event VerticesMouseEventHandler VerticesMouseLeave;
        public event VerticesMouseButtonEventHandler VerticesMouseDown;
        public event VerticesMouseEventHandler VerticesMouseMove;
        public event VerticesMouseButtonEventHandler VerticesMouseUp;

        public event LinesMouseEventHandler StrokeMouseEnter;
        public event LinesMouseEventHandler StrokeMouseLeave;
        public event LinesMouseButtonEventHandler StrokeMouseDown;
        public event LinesMouseEventHandler StrokeMouseMove;
        public event LinesMouseButtonEventHandler StrokeMouseUp;

        public List<PointLatLng> Points = new List<PointLatLng>();

        public bool ShowCrossPoints { get; set; }

        public GMapControl Map
        {
            get { return map; }
            set
            {
                map = value;

                if (Stroke != null)
                    foreach (LineLatLng line in Stroke)
                    {
                        line.Map = map;
                    }

                if (Vertices != null)
                    foreach (VertexLatLng v in Vertices)
                    {
                        v.Map = map;
                    }

                if (CrossPoints != null)
                    foreach (var cross in CrossPoints)
                    {
                        cross.Vertex.Map = map;
                    }
            }
        }

        public Style StrokeStyle
        {
            get { return strokeStyle; }
            set
            {
                strokeStyle = value;

                if (Stroke != null)
                    foreach (LineLatLng line in Stroke)
                    {
                        line.Style = strokeStyle;
                    }
            }
        }

        public Style VerticesStyle
        {
            get { return verticesStyle; }
            set
            {
                verticesStyle = value;

                if (Vertices != null)
                    foreach (VertexLatLng v in Vertices)
                    {
                        v.Style = verticesStyle;
                    }
            }
        }

        public Style CrossPointsStyle
        {
            get { return crossPointsStyle; }
            set
            {
                crossPointsStyle = value;
                if (CrossPoints != null)
                    foreach (var cross in CrossPoints)
                    {
                        cross.Vertex.Style = crossPointsStyle;
                    }
            }
        }

        public Style CrossLinesStyle
        {
            get { return crossLinesStyle; }
            set
            {
                crossLinesStyle = value;
                if (CrossPoints != null)
                    foreach (var cross in CrossPoints)
                    {
                        cross.Line1.Style = cross.Line2.Style = crossLinesStyle; ;
                    }
            }
        }

        public Style CoincidentLinesStyle
        {
            get { return coincidentLinesStyle; }
            set
            {
                coincidentLinesStyle = value;
                if (CoincidentLines != null)
                    foreach (var coin in CoincidentLines)
                    {
                        coin.Line1.Style = coin.Line2.Style = coincidentLinesStyle;
                    }
            }
        }

        List<VertexLatLng> vertices = new List<VertexLatLng>();
        public ReadOnlyCollection<VertexLatLng> Vertices { get { return vertices.AsReadOnly(); } }

        List<CrossPoint> crossPoints = new List<CrossPoint>();
        public ReadOnlyCollection<CrossPoint> CrossPoints { get { return crossPoints.AsReadOnly(); } }

        List<LineLatLng> stroke = new List<LineLatLng>();
        public ReadOnlyCollection<LineLatLng> Stroke { get { return stroke.AsReadOnly(); } }

        List<CoincidentLine> coincidentLines = new List<CoincidentLine>();
        public ReadOnlyCollection<CoincidentLine> CoincidentLines { get { return coincidentLines.AsReadOnly(); } }

        public bool CheckCrossPoints { get; set; }

        public bool CheckCoincidentLines { get; set; }

        /// <summary>
        /// کنترلر چند ضلعی را ایجاد می کند. در اصل رئوس و خطوط چند ضلعی ایجاد می شود اما نمایش داده نمی شود. 
        /// </summary>
        public virtual void CreateController()
        {
            for (int j = Vertices.Count - 1; j >= Points.Count; j--)//رئوس اضافی را حذف می کند. برای زمانی است که یکی از نقاط حذف شده باشد.
            {
                vertices[j].Erase();
                vertices.Remove(vertices[j]);
            }
            for (int j = stroke.Count - 1; j >= Points.Count; j--)//خطوط اضافی را حذف می کند. برای زمانی است که یکی از نقاط حذف شده باشد.
            {
                stroke[j].Erase();
                stroke.Remove(stroke[j]);
            }
            
            crossPoints.Clear();
            coincidentLines.Clear();

            if (Points?.Count > 1)
            {
                int i = 0;
                foreach (PointLatLng point in this.Points)
                {
                    if (i < vertices.Count)//اگر راس از قبل وجود داشت تنها مختصات آن آپدیت می شود
                    {
                        Vertices[i].Point = point;
                    }
                    else//اگر راس وجود نداشت یک راس جدید می سازد
                    {
                        InsertOneVertex(i, point);
                    }

                    PointLatLng destPoint;
                    if (i < Points.Count - 1)//نقطه مقصد خط تعیین می شود. اگر به نقطه آخر رسیده بود به نقطه اول وصل می شود.
                    {
                        destPoint = Points[i + 1];
                    }
                    else
                    {
                        destPoint = Points[0];
                    }

                    if (i < Stroke.Count)//اگر خط از قبل وجود داشت فقط مختصات آن آپدیت می شود
                    {
                        stroke[i].StartPoint = point;
                        stroke[i].EndPoint = destPoint;
                    }
                    else//اگر خط وجود نداشت یک خط جدید می سازد.
                    {
                        InsertOneEdge(i ,point, destPoint);
                    }

                    i++;
                }
                if (CheckCrossPoints)
                {
                    SetCrossPoints();
                }
                if (CheckCoincidentLines) { 
                    SetCoincidentLine();
                }


                Map = map;
                VerticesStyle = verticesStyle;
                StrokeStyle = strokeStyle;
                CoincidentLinesStyle = coincidentLinesStyle;
                CrossLinesStyle = crossLinesStyle;
                CrossPointsStyle = crossPointsStyle;
            }
        }

        /// <summary>
        /// یک خظ در مکان مورد نظر اضافه می کند.
        /// </summary>
        /// <param name="position">مکان خط</param>
        /// <param name="startPoint">نقطه مبدا خط</param>
        /// <param name="endPoint">نقطه مقصد خط</param>
        private void InsertOneEdge(int position, PointLatLng startPoint, PointLatLng endPoint)
        {
            LineLatLng line = new LineLatLng(startPoint, endPoint);
            line.MouseEnter += Line_MouseEnter;
            line.MouseLeave += Line_MouseLeave;
            line.MouseMove += Line_MouseMove;
            line.MouseDown += Line_MouseDown;
            line.MouseUp += Line_MouseUp;
            stroke.Insert(position, line);
        }

        /// <summary>
        /// یک راس در مکان مورد نظر اضافه می کند.
        /// </summary>
        /// <param name="position">مکان راس</param>
        /// <param name="point">نقطه راس</param>
        private void InsertOneVertex(int position, PointLatLng point)
        {
            VertexLatLng vertex = new VertexLatLng(point);
            vertex.MouseEnter += Vertex_MouseEnter;
            vertex.MouseLeave += Vertex_MouseLeave;
            vertex.MouseMove += Vertex_MouseMove;
            vertex.MouseDown += Vertex_MouseDown;
            vertex.MouseUp += Vertex_MouseUp;
            vertices.Insert(position, vertex);
        }

        /// <summary>
        /// پس از جابجایی یک نقطه در شکل تغییرات لازم مرتبط با شکل را اعمال می کند.
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void MoveOneVertex(int numVertex)
        {
            if (numVertex >= Points.Count) return;

            if (Points?.Count > 1)
            {
                LineLatLng line1;
                if (numVertex == 0)
                {
                    line1 = Stroke[Stroke.Count - 1];
                }
                else
                {
                    line1 = Stroke[numVertex - 1];
                }

                List<CrossPoint> line1CrossPoints = LineCrossPointsCollection(line1);
                List<CoincidentLine> line1CoincidentLine = LineCoincidentLineCollection(line1);
                
                LineLatLng line2 = Stroke[numVertex];
                
                List<CrossPoint> line2CrossPoints = LineCrossPointsCollection(line2);
                List<CoincidentLine> line2CoincidentLine = LineCoincidentLineCollection(line2);
              
                Vertices[numVertex].Point = line1.EndPoint = line2.StartPoint = Points[numVertex];
                
                if (CheckCrossPoints)
                {
                    if (line1CrossPoints.Count > 0 || line2CrossPoints.Count > 0)
                    {
                        crossPoints.Clear();
                        SetCrossPoints();
                    }
                    else
                    {
                        SetLineCrossPoints(line1);
                        SetLineCrossPoints(line2);
                    }
                }
                if (CheckCoincidentLines)
                {
                    if (line1CoincidentLine.Count > 0 || line2CoincidentLine.Count > 0)
                    {
                        coincidentLines.Clear();
                        SetCoincidentLine();
                    }
                    else
                    {
                        SetLineCoincidentLine(line1);
                        SetLineCoincidentLine(line2);
                    }
                }


                Map = map;
                VerticesStyle = verticesStyle;
                StrokeStyle = strokeStyle;
                CoincidentLinesStyle = coincidentLinesStyle;
                CrossLinesStyle = crossLinesStyle;
                CrossPointsStyle = crossPointsStyle;
            }
        }

        /// <summary>
        /// پس از افزودن یک نقطه در شکل تغییرات لازم مرتبط با شکل را اعمال می کند.
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void InsertOneVertex(int numVertex)
        {
            if (numVertex >= Points.Count) return;

            if (Points?.Count > 1)
            {
                PointLatLng newPoint = Points[numVertex];

                LineLatLng oldLine;
                if (numVertex == 0)
                {
                    oldLine = Stroke[Stroke.Count - 1];
                }
                else
                {
                    oldLine = Stroke[numVertex - 1];
                }

                List<CrossPoint> oldLineCrossPoints = LineCrossPointsCollection(oldLine);
                List<CoincidentLine> oldLineCoincidentLine = LineCoincidentLineCollection(oldLine);

                InsertOneEdge(numVertex, newPoint, oldLine.EndPoint);
                Stroke[numVertex].ZIndex = oldLine.ZIndex;
                oldLine.EndPoint = newPoint;
                InsertOneVertex(numVertex, newPoint);
                Vertices[numVertex].ZIndex = (numVertex == 0) ? Vertices[Vertices.Count - 1].ZIndex : Vertices[numVertex - 1].ZIndex;
                
                if (CheckCrossPoints)
                {
                    if (oldLineCrossPoints.Count > 0)
                    {
                        crossPoints.Clear();
                        SetCrossPoints();
                    }
                    else
                    {
                        SetLineCrossPoints(oldLine);
                    }
                }
                if (CheckCoincidentLines)
                {
                    if (oldLineCoincidentLine.Count > 0)
                    {
                        coincidentLines.Clear();
                        SetCoincidentLine();
                    }
                    else
                    {
                        SetLineCoincidentLine(oldLine);
                    }
                }

                Map = map;
                VerticesStyle = verticesStyle;
                StrokeStyle = strokeStyle;
                CoincidentLinesStyle = coincidentLinesStyle;
                CrossLinesStyle = crossLinesStyle;
                CrossPointsStyle = crossPointsStyle;
            }
        }

        /// <summary>
        /// پس از حذف یک نقطه از شکل تغییرات لازم مرتبط با شکل را اعمال می کند.
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void DeleteOneVertex(int numVertex)
        {
            if (numVertex > Points.Count) return;

            LineLatLng line1;
            if (numVertex == 0)
            {
                line1 = Stroke[Stroke.Count - 1];
            }
            else
            {
                line1 = Stroke[numVertex - 1];
            }

            List<CrossPoint> line1CrossPoints = LineCrossPointsCollection(line1);
            List<CoincidentLine> line1CoincidentLine = LineCoincidentLineCollection(line1);

            LineLatLng line2 = Stroke[numVertex];

            List<CrossPoint> line2CrossPoints = LineCrossPointsCollection(line2);
            List<CoincidentLine> line2CoincidentLine = LineCoincidentLineCollection(line2);

            Vertices[numVertex].Delete();
            vertices.RemoveAt(numVertex);

            line1.EndPoint = line2.EndPoint;
            
            line2.Delete();
            stroke.RemoveAt(numVertex);

            if (CheckCrossPoints)
            {
                if (line1CrossPoints.Count > 0 || line2CrossPoints.Count > 0)
                {
                    crossPoints.Clear();
                    SetCrossPoints();
                }
                else
                {
                    SetLineCrossPoints(line1);
                }
            }
            if (CheckCoincidentLines)
            {
                if (line1CoincidentLine.Count > 0 || line2CoincidentLine.Count > 0)
                {
                    coincidentLines.Clear();
                    SetCoincidentLine();
                }
                else
                {
                    SetLineCoincidentLine(line1);
                }
            }

            Map = map;
            VerticesStyle = verticesStyle;
            StrokeStyle = strokeStyle;
            CoincidentLinesStyle = coincidentLinesStyle;
            CrossLinesStyle = crossLinesStyle;
            CrossPointsStyle = crossPointsStyle;
        }

        /// <summary>
        /// تطابق تمام خطوط چندضلعی را با هم بررسی کرده و ثبت می کند.
        /// </summary>
        private void SetCoincidentLine()
        {
            for (int i = 0; i < Stroke.Count; i++)
            {
                LineLatLng e1 = Stroke[i];
                for (int j = i; j < Stroke.Count; j++)
                {
                    LineLatLng e2 = Stroke[j];

                    if (Stroke.Count > 1 && e1 != e2)//for coincident line
                    {
                        CoincidentLine coincidentLine = new CoincidentLine(e1, e2);
                        if (coincidentLine.Coincident != null)
                        {
                            coincidentLines.Add(coincidentLine);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// تقاطع تمام خطوط چندضلعی را باهم بررسی کرده و ثبت می کند.
        /// </summary>
        private void SetCrossPoints()
        {
            for (int i = 0; i < Stroke.Count; i++)
            {
                LineLatLng e1 = Stroke[i];
                for (int j = i; j < Stroke.Count; j++)
                {
                    LineLatLng e2 = Stroke[j];
                    if (Stroke.Count > 3 && e1 != e2 && e1.StartPoint != e2.EndPoint && e2.StartPoint != e1.EndPoint)//for cross point
                    {
                        CrossPoint cross = new CrossPoint(e1, e2);

                        if (!cross.Vertex.Point.IsEmpty)
                        {
                            crossPoints.Add(cross);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// خطوط منطبق با خط داده شده را در شکل می یابد.
        /// </summary>
        /// <param name="line">خط</param>
        private void SetLineCoincidentLine(LineLatLng line)
        {
            for (int i = 0; i < Stroke.Count; i++)
            {
                LineLatLng line2 = Stroke[i];

                if (Stroke.Count > 1 && line != line2)//for coincident line
                {
                    CoincidentLine coincidentLine = new CoincidentLine(line, line2);
                    if (coincidentLine.Coincident != null)
                    {
                        coincidentLines.Add(coincidentLine);
                    }
                }
            }
        }

        /// <summary>
        /// خطوط منطبق با خط داده شده را برمی گرداند
        /// </summary>
        /// <param name="line">خط داده شده</param>
        /// <returns>مجموعه ای از خطوط منطبق</returns>
        private List<CoincidentLine> LineCoincidentLineCollection(LineLatLng line)
        {
            List<CoincidentLine> CL = new List<CoincidentLine>();
            foreach (var coin in CoincidentLines)
            {
                if (line == coin.Line1 || line == coin.Line2)
                    CL.Add(coin);
            }
            return CL;
        }

        /// <summary>
        /// نقاط متقاطع خط داده شده با سایر خطوط چند ضلعی را برمی گرداند.
        /// </summary>
        /// <param name="line">خط</param>
        private void SetLineCrossPoints(LineLatLng line)
        {
            for (int i = 0; i < Stroke.Count; i++)
            {
                LineLatLng line2 = Stroke[i];
                if (Stroke.Count > 3 && line != line2 && line.StartPoint != line2.EndPoint && line2.StartPoint != line.EndPoint)//for cross point
                {
                    CrossPoint cross = new CrossPoint(line, line2);

                    if (!cross.Vertex.Point.IsEmpty)
                    {
                        crossPoints.Add(cross);
                    }
                }
            }
        }

        /// <summary>
        /// نقاط متقاطع خط داده شده با سایر خطوط چند ضلعی را برمی گرداند.
        /// </summary>
        /// <param name="line">خط داده شده</param>
        /// <returns>مجموعه ای از نقاط متقاطع</returns>
        private List<CrossPoint> LineCrossPointsCollection(LineLatLng line)
        {
            List<CrossPoint> CP = new List<CrossPoint>();
            foreach (var cross in CrossPoints)
            {
                if (line == cross.Line1 || line == cross.Line2)
                    CP.Add(cross);
            }
            return CP;
        }

        /// <summary>
        /// نقاط متقاطع چند ضلعی داده شده را بر میگرداند.
        /// </summary>
        /// <param name="polygon">چند ضلعی</param>
        /// <returns>نقاط متقاطع</returns>
        public static List<CrossPoint> FindCrossPoints(PolygonLatLng polygon)
        {
            List<CrossPoint> crossPoints = new List<CrossPoint>();
            List<LineLatLng> stroke = CreateStroke(polygon);

            for (int i = 0; i < stroke.Count; i++)
            {
                LineLatLng e1 = stroke[i];
                for (int j = i; j < stroke.Count; j++)
                {
                    LineLatLng e2 = stroke[j];
                    if (stroke.Count > 3 && e1 != e2 && e1.StartPoint != e2.EndPoint && e2.StartPoint != e1.EndPoint)//for cross point
                    {
                        CrossPoint cross = new CrossPoint(e1, e2);

                        if (!cross.Vertex.Point.IsEmpty)
                        {
                            crossPoints.Add(cross);
                        }
                    }
                }
            }

            return crossPoints;
        }

        /// <summary>
        /// خطوط منطبق چند ضلعی داده شده را برمیگرداند.
        /// </summary>
        /// <param name="polygon">چندضلعی</param>
        /// <returns>خطوط منطبق</returns>
        public static List<CoincidentLine> FindCoincidentLines(PolygonLatLng polygon)
        {
            List<CoincidentLine> coincidentLines = new List<CoincidentLine>();
            List<LineLatLng> sroke = CreateStroke(polygon);

            for (int i = 0; i < sroke.Count; i++)
            {
                LineLatLng e1 = sroke[i];
                for (int j = i; j < sroke.Count; j++)
                {
                    LineLatLng e2 = sroke[j];

                    if (sroke.Count > 1 && e1 != e2)//for coincident line
                    {
                        CoincidentLine coincidentLine = new CoincidentLine(e1, e2);
                        if (coincidentLine.Coincident != null)
                        {
                            coincidentLines.Add(coincidentLine);
                        }
                    }
                }
            }

            return coincidentLines;
        }

        private static List<LineLatLng> CreateStroke(PolygonLatLng polygon)
        {
            List<LineLatLng> stroke = new List<LineLatLng>();

            int i = 0;
            foreach (PointLatLng point in polygon.Points)
            {
                PointLatLng destPoint;
                if (i < polygon.Points.Count - 1)//نقطه مقصد خط تعیین می شود. اگر به نقطه آخر رسیده بود به نقطه اول وصل می شود.
                {
                    destPoint = polygon.Points[i + 1];
                }
                else
                {
                    destPoint = polygon.Points[0];
                }
                LineLatLng line = new LineLatLng(point, destPoint);
                stroke.Add(line);

                i++;
            }

            return stroke;
        }

        /// <summary>
        /// اولویت نمایش اجزا کنترلر را تعیین می کند.
        /// به این صورت که ابتدا خطوط و سپس رئوس را ترسیم می کند.
        /// </summary>
        /// <param name="minIndex">ایندکسی که باید از آن شروع کند.</param>
        /// <returns>ایندکس آخرین عنصر اولویت بندی شده رو بر می گرداند.</returns>
        public int SendToFront(int minIndex)
        {
            foreach (var l in Stroke)
            {
                l.ZIndex = minIndex++;
            }

            foreach (var v in Vertices)
            {
                v.ZIndex = minIndex++;
            }
            return minIndex;
        }

        /// <summary>
        /// کنترلر را حذف می کند.
        /// </summary>
        public void Delete()
        {
            Erase();
            DeleteVertices();
            DeleteStroke();
            DeleteCrossPoints();
        }

        private void DeleteCrossPoints()
        {
            if (crossPoints != null)
                foreach (var c in crossPoints)
                {
                    c.Vertex.Delete();
                }
            crossPoints.Clear();
        }

        private void DeleteStroke()
        {
            if (stroke != null)
                foreach (var s in stroke)
                {
                    s.Delete();
                }
            stroke.Clear();
        }

        private void DeleteVertices()
        {
            if (vertices != null)
                foreach (var v in vertices)
                {
                    v.Delete();
                }
            vertices.Clear();
        }

        /// <summary>
        /// کنترلر را روی نقشه نشان می دهد. اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            DrawStroke();
            DrawVertices();
            if (ShowCrossPoints)
                DrawCrossPoints();
        }

        /// <summary>
        /// نقاط برخورد خطوط را روی نقشه نشان می دهد.
        /// </summary>
        private void DrawCrossPoints()
        {
            foreach (var cross in CrossPoints)
            {
                cross.Vertex.Draw();
            }
        }

        /// <summary>
        /// رئوس را روی نقشه نشان می دهد.
        /// </summary>
        private void DrawVertices()
        {
            foreach (VertexLatLng v in Vertices)
            {
                v.Draw();
            }
        }

        /// <summary>
        /// خطوط حاشیه ای را روی نقشه نشان می دهد.
        /// </summary>
        private void DrawStroke()
        {
            foreach (LineLatLng line in Stroke)
            {
                line.Draw();
            }
        }

        /// <summary>
        /// راس و خطوط مرتبط با تغییرات حرکت راس را ترسیم می کند. 
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void DrawAfterMoveOneVertex(int numVertex)
        {
            if (numVertex >= Points.Count) return;

            LineLatLng line1;
            if (numVertex == 0)
            {
                line1 = Stroke[Points.Count - 1];
            }
            else
            {
                line1 = Stroke[numVertex - 1];
            }

            LineLatLng line2 = Stroke[numVertex];

            line1.Draw();
            line2.Draw();
            Vertices[numVertex].Draw();
        }

        /// <summary>
        /// راس و خطوط مرتبط با تغییرات حرکت راس را پاک می کند. 
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void EraseAfterMoveOneVertex(int numVertex)
        {
            if (numVertex >= Points.Count) return;

            LineLatLng line1;
            if (numVertex == 0)
            {
                line1 = Stroke[Points.Count - 1];
            }
            else
            {
                line1 = Stroke[numVertex - 1];
            }
            
            LineLatLng line2 = Stroke[numVertex];

            line1.Erase();
            line2.Erase();
            Vertices[numVertex].Erase();
        }

        /// <summary>
        /// کنترلر را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            EraseStroke();
            EraseVertices();
            EraseCrossPoints();
        }

        /// <summary>
        /// نقاط برخورد خطوط را روی نقشه نشان می دهد.
        /// </summary>
        private void EraseCrossPoints()
        {
            foreach (var cross in CrossPoints)
            {
                cross.Vertex.Erase();
            }
        }

        /// <summary>
        /// رئوس را از روی نقشه پاک می کند.
        /// </summary>
        private void EraseVertices()
        {
            foreach (VertexLatLng v in Vertices)
            {
                v.Erase();
            }
        }

        /// <summary>
        /// خطوط حاشیه ای را از روی نقشه پاک می کند.
        /// </summary>
        private void EraseStroke()
        {
            foreach (LineLatLng line in Stroke)
            {
                line.Erase();
            }
        }

        private void Line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Stroke.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinessMouseButtonEventArgs args = new LinessMouseButtonEventArgs(e, line, index);
            OnStrokeMouseUp(args);
        }

        private void OnStrokeMouseUp(LinessMouseButtonEventArgs args)
        {
            StrokeMouseUp?.Invoke(this, args);
        }

        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Stroke.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinessMouseButtonEventArgs args = new LinessMouseButtonEventArgs(e, line, index);
            OnStrokeMouseDown(args);
        }

        private void OnStrokeMouseDown(LinessMouseButtonEventArgs args)
        {
            StrokeMouseDown?.Invoke(this, args);
        }

        private void Line_MouseMove(object sender, MouseEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Stroke.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinesMouseEventArgs args = new LinesMouseEventArgs(e, line, index);
            OnStrokeMouseMove(args);
        }

        private void OnStrokeMouseMove(LinesMouseEventArgs args)
        {
            StrokeMouseMove?.Invoke(this, args);
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Stroke.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinesMouseEventArgs args = new LinesMouseEventArgs(e, line, index);
            OnStrokeMouseLeave(args);
        }

        private void OnStrokeMouseLeave(LinesMouseEventArgs args)
        {
            StrokeMouseLeave?.Invoke(this, args);
        }

        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Stroke.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinesMouseEventArgs args = new LinesMouseEventArgs(e, line, index);
            OnStrokeMouseEnter(args);
        }

        private void OnStrokeMouseEnter(LinesMouseEventArgs args)
        {
            StrokeMouseEnter?.Invoke(this, args);
        }

        private void Vertex_MouseUp(object sender, MouseButtonEventArgs e)
        {
            VertexLatLng vertex = sender as VertexLatLng;
            int index = Vertices.IndexOf(vertex);
            if (index < 0)
            {
                return;
            }

            VerticesMouseButtonEventArgs args = new VerticesMouseButtonEventArgs(e, vertex, index);
            OnVerticesMouseUp(args);
        }

        private void OnVerticesMouseUp(VerticesMouseButtonEventArgs args)
        {
            VerticesMouseUp?.Invoke(this, args);
        }

        private void Vertex_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VertexLatLng vertex = sender as VertexLatLng;
            int index = Vertices.IndexOf(vertex);
            if (index < 0)
            {
                return;
            }

            VerticesMouseButtonEventArgs args = new VerticesMouseButtonEventArgs(e, vertex, index);
            OnVerticesMouseDown(args);
        }

        private void OnVerticesMouseDown(VerticesMouseButtonEventArgs args)
        {
            VerticesMouseDown?.Invoke(this, args);
        }

        private void Vertex_MouseMove(object sender, MouseEventArgs e)
        {
            VertexLatLng vertex = sender as VertexLatLng;
            int index = Vertices.IndexOf(vertex);
            if (index < 0)
            {
                return;
            }

            VerticesMouseEventArgs args = new VerticesMouseEventArgs(e, vertex, index);
            OnVerticesMouseMove(args);
        }

        private void OnVerticesMouseMove(VerticesMouseEventArgs args)
        {
            VerticesMouseMove?.Invoke(this, args);
        }

        private void Vertex_MouseLeave(object sender, MouseEventArgs e)
        {
            VertexLatLng vertex = sender as VertexLatLng;
            int index = Vertices.IndexOf(vertex);
            if (index < 0)
            {
                return;
            }

            VerticesMouseEventArgs args = new VerticesMouseEventArgs(e, vertex, index);
            OnVerticesMouseLeave(args);
        }

        private void OnVerticesMouseLeave(VerticesMouseEventArgs args)
        {
            VerticesMouseLeave?.Invoke(this, args);
        }

        private void Vertex_MouseEnter(object sender, MouseEventArgs e)
        {
            VertexLatLng vertex = sender as VertexLatLng;
            int index = Vertices.IndexOf(vertex);
            if (index < 0)
            {
                return;
            }

            VerticesMouseEventArgs args = new VerticesMouseEventArgs(e, vertex, index);
            OnVerticesMouseEnter(args);
        }

        private void OnVerticesMouseEnter(VerticesMouseEventArgs args)
        {
            VerticesMouseEnter?.Invoke(this, args);
        }
    }
}
