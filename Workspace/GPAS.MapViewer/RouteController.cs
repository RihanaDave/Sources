using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class RouteController
    {
        /// <summary>
        /// ایجاد کنترلر مسیر
        /// </summary>
        /// <param name="points">نقاط رئوس کنترلر</param>
        public RouteController(List<PointLatLng> points)
        {
            Points = points;
            CreateController();
        }

        public event VerticesMouseEventHandler VerticesMouseEnter;
        public event VerticesMouseEventHandler VerticesMouseLeave;
        public event VerticesMouseButtonEventHandler VerticesMouseDown;
        public event VerticesMouseEventHandler VerticesMouseMove;
        public event VerticesMouseButtonEventHandler VerticesMouseUp;

        public event LinesMouseEventHandler LinesMouseEnter;
        public event LinesMouseEventHandler LinesMouseLeave;
        public event LinesMouseButtonEventHandler LinesMouseDown;
        public event LinesMouseEventHandler LinesMouseMove;
        public event LinesMouseButtonEventHandler LinesMouseUp;

        public List<PointLatLng> Points = new List<PointLatLng>();

        GMapControl map;
        public GMapControl Map
        {
            get { return map; }
            set
            {
                map = value;
                if (Lines != null)
                    foreach (LineLatLng line in Lines)
                    {
                        line.Map = map;
                    }
            }
        }

        private Style linesStyle;
        public Style LinesStyle
        {
            get { return linesStyle; }
            set
            {
                linesStyle = value;

                if (Lines != null)
                    foreach (LineLatLng line in Lines)
                    {
                        line.Style = linesStyle;
                    }

                if (Vertices != null)
                    foreach (VertexLatLng v in Vertices)
                    {
                        v.Map = map;
                    }
            }
        }

        private Style verticesStyle;
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

        List<VertexLatLng> vertices = new List<VertexLatLng>();
        public ReadOnlyCollection<VertexLatLng> Vertices { get { return vertices.AsReadOnly(); } }

        List<LineLatLng> lines = new List<LineLatLng>();
        public ReadOnlyCollection<LineLatLng> Lines { get { return lines.AsReadOnly(); } }

        /// <summary>
        /// کنترلر مسیر را ایجاد می کند. در اصل رئوس و خطوط مسیر ایجاد می شود اما نمایش داده نمی شود.
        /// </summary>
        public virtual void CreateController()
        {
            for (int i = Vertices.Count - 1; i >= Points.Count; i--)//رئوس اضافی را حذف می کند. برای زمانی است که یکی از نقاط حذف شده باشد.
            {
                Vertices[i].Erase();
                vertices.Remove(Vertices[i]);
            }

            for (int i = Lines.Count - 1; i >= Points.Count - 1; i--)//خطوط اضافی را حذف می کند. برای زمانی است که یکی از نقاط حذف شده باشد.
            {
                if (i > 0)
                {
                    lines[i].Erase();
                    lines.Remove(Lines[i]);
                }
            }

            if (Points?.Count > 1)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    PointLatLng point = Points[i];

                    if (i < Vertices.Count)//اگر راس از قبل وجود داشت تنها مختصات آن آپدیت می شود
                    {
                        Vertices[i].Point = point;
                    }
                    else//اگر راس وجود نداشت یک راس جدید می سازد
                    {
                        InsertOneVertex(i, point);
                    }

                    if (i < Points.Count - 1)//نقطه مقصد خط تعیین می شود. در صورتی ک ب نقطه آخر رسیده باشد دیگر نیازی نیست خطی ترسیم شود.
                    {
                        PointLatLng destPoint = Points[i + 1];

                        if (i < Lines.Count)//اگر خط از قبل وجود داشت فقط مختصات آن آپدیت می شود
                        {
                            Lines[i].StartPoint = point;
                            Lines[i].EndPoint = destPoint;
                        }
                        else//اگر خط وجود نداشت یک خط جدید می سازد.
                        {
                            InsertOneLine(i, point, destPoint);
                        }
                    }
                }
                Map = map;
                VerticesStyle = verticesStyle;
                LinesStyle = linesStyle;
            }
        }

        /// <summary>
        /// یک خظ در مکان مورد نظر اضافه می کند.
        /// </summary>
        /// <param name="position">مکان خط</param>
        /// <param name="startPoint">نقطه مبدا خط</param>
        /// <param name="endPoint">نقطه مقصد خط</param>
        private void InsertOneLine(int position, PointLatLng startPoint, PointLatLng endPoint)
        {
            LineLatLng line = new LineLatLng(startPoint, endPoint);
            line.MouseEnter += Line_MouseEnter;
            line.MouseLeave += Line_MouseLeave;
            line.MouseMove += Line_MouseMove;
            line.MouseDown += Line_MouseDown;
            line.MouseUp += Line_MouseUp;
            lines.Insert(position, line);
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
            if (numVertex > Points.Count) return;

            if (Points?.Count > 1)
            {
                LineLatLng lineNext, linePrev;

                if (numVertex > 0 && numVertex < Points.Count - 1)
                {
                    lineNext = Lines[numVertex];
                    linePrev = Lines[numVertex - 1];

                    linePrev.EndPoint = lineNext.StartPoint = Points[numVertex];
                }
                else if (numVertex == 0)
                {
                    lineNext = Lines[numVertex];
                    lineNext.StartPoint = Points[numVertex];
                }
                else if(numVertex == Points.Count - 1)
                {
                    linePrev = Lines[numVertex - 1];
                    linePrev.EndPoint = Points[numVertex];
                }

                Vertices[numVertex].Point = Points[numVertex];
            }

            Map = map;
            VerticesStyle = verticesStyle;
            LinesStyle = linesStyle;
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
                LineLatLng lineNext, linePrev;

                if (numVertex > 0 && numVertex < Points.Count - 1)
                {
                    linePrev = Lines[numVertex - 1];

                    InsertOneLine(numVertex - 1, linePrev.StartPoint, Points[numVertex]);
                    Lines[numVertex - 1].ZIndex = linePrev.ZIndex;
                    linePrev.StartPoint = Points[numVertex];
                }
                else if (numVertex == 0)
                {
                    lineNext = Lines[numVertex];
                    InsertOneLine(numVertex, Points[numVertex], lineNext.StartPoint);
                    Lines[numVertex].ZIndex = lineNext.ZIndex;
                }
                else if (numVertex == Points.Count - 1)
                {
                    linePrev = Lines[Lines.Count - 1];
                    InsertOneLine(numVertex - 1, linePrev.EndPoint, Points[numVertex]);
                    Lines[numVertex - 1].ZIndex = linePrev.ZIndex;
                }

                InsertOneVertex(numVertex, Points[numVertex]);
                Vertices[numVertex].ZIndex = (numVertex == 0) ? Vertices[0].ZIndex : Vertices[numVertex - 1].ZIndex;
            }

            Map = map;
            VerticesStyle = verticesStyle;
            LinesStyle = linesStyle;
        }

        /// <summary>
        /// پس از حذف یک نقطه از شکل تغییرات لازم مرتبط با شکل را اعمال می کند.
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void DeleteOneVertex(int numVertex)
        {
            if (numVertex >= Points.Count + 1) return;

            LineLatLng lineNext, linePrev;

            if (numVertex > 0 && numVertex < Points.Count)
            {
                lineNext = Lines[numVertex];
                linePrev = Lines[numVertex - 1];

                linePrev.EndPoint = lineNext.EndPoint;
                lineNext.Delete();
                lines.RemoveAt(numVertex);
            }
            else if (numVertex == 0)
            {
                lineNext = Lines[numVertex];
                lineNext.Delete();
                lines.RemoveAt(numVertex);
            }
            else if (numVertex == Points.Count)
            {
                linePrev = Lines[Lines.Count - 1];
                linePrev.Delete();
                lines.RemoveAt(Lines.Count - 1);
            }

            Vertices[numVertex].Delete();
            vertices.RemoveAt(numVertex);

            Map = map;
            VerticesStyle = verticesStyle;
            LinesStyle = linesStyle;
        }

        /// <summary>
        /// اولویت نمایش اجزا کنترلر را تعیین می کند.
        /// به این صورت که ابتدا خطوط و سپس رئوس را ترسیم می کند.
        /// </summary>
        /// <param name="minIndex">ایندکسی که باید از آن شروع کند.</param>
        /// <returns>ایندکس آخرین عنصر اولویت بندی شده رو بر می گرداند.</returns>
        public int SendToFront(int minIndex)
        {
            foreach (var l in Lines)
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
            DeleteLines();
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

        private void DeleteLines()
        {
            if (lines != null)
                foreach (var l in lines)
                {
                    l.Delete();
                }
            lines.Clear();
        }

        /// <summary>
        /// کنترلر را روی نقشه نشان می دهد. اگر استایل ها ست نشده باشد چیزی نشان نمی دهد.
        /// </summary>
        public void Draw()
        {
            DrawLines();
            DrawVertices();
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
        /// خطوط را روی نقشه نشان می دهد.
        /// </summary>
        private void DrawLines()
        {
            foreach (LineLatLng line in Lines)
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

            LineLatLng lineNext, linePrev;

            if (numVertex > 0 && numVertex < Points.Count - 1)
            {
                lineNext = Lines[numVertex];
                linePrev = Lines[numVertex - 1];

                lineNext.Draw();
                linePrev.Draw();
            }
            else if (numVertex == 0)
            {
                lineNext = Lines[numVertex];
                lineNext.Draw();
            }
            else if (numVertex == Points.Count - 1)
            {
                linePrev = Lines[numVertex - 1];
                linePrev.Draw();
            }
            Vertices[numVertex].Draw();
        }

        /// <summary>
        /// راس و خطوط مرتبط با تغییرات حرکت راس را پاک می کند. 
        /// </summary>
        /// <param name="numVertex">ایندکس راس</param>
        public void EraseAfterMoveOneVertex(int numVertex)
        {
            if (numVertex >= Points.Count) return;

            LineLatLng lineNext, linePrev;

            if (numVertex > 0 && numVertex < Points.Count - 1)
            {
                lineNext = Lines[numVertex];
                linePrev = Lines[numVertex - 1];

                lineNext.Erase();
                linePrev.Erase();
            }
            else if (numVertex == 0)
            {
                lineNext = Lines[numVertex];
                lineNext.Erase();
            }
            else if (numVertex == Points.Count - 1)
            {
                linePrev = Lines[numVertex - 1];
                linePrev.Erase();
            }
            Vertices[numVertex].Erase();
        }

        /// <summary>
        /// کنترلر را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
            EraseLines();
            EraseVertices();
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
        /// خطوط را از روی نقشه پاک می کند.
        /// </summary>
        private void EraseLines()
        {
            foreach (LineLatLng line in Lines)
            {
                line.Erase();
            }
        }

        private void Line_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinessMouseButtonEventArgs args = new LinessMouseButtonEventArgs(e, line, index);
            OnLinesMouseUp(args);
        }

        private void OnLinesMouseUp(LinessMouseButtonEventArgs args)
        {
            LinesMouseUp?.Invoke(this, args);
        }

        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinessMouseButtonEventArgs args = new LinessMouseButtonEventArgs(e, line, index);
            OnLinesMouseDown(args);
        }

        private void OnLinesMouseDown(LinessMouseButtonEventArgs args)
        {
            LinesMouseDown?.Invoke(this, args);
        }

        private void Line_MouseMove(object sender, MouseEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinesMouseEventArgs args = new LinesMouseEventArgs(e, line, index);
            OnLinesMouseMove(args);
        }

        private void OnLinesMouseMove(LinesMouseEventArgs args)
        {
            LinesMouseMove?.Invoke(this, args);
        }

        private void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinesMouseEventArgs args = new LinesMouseEventArgs(e, line, index);
            OnLinesMouseLeave(args);
        }

        private void OnLinesMouseLeave(LinesMouseEventArgs args)
        {
            LinesMouseLeave?.Invoke(this, args);
        }

        private void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            LineLatLng line = sender as LineLatLng;
            int index = Lines.IndexOf(line);
            if (index < 0)
            {
                return;
            }

            LinesMouseEventArgs args = new LinesMouseEventArgs(e, line, index);
            OnLinesMouseEnter(args);
        }

        private void OnLinesMouseEnter(LinesMouseEventArgs args)
        {
            LinesMouseEnter?.Invoke(this, args);
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
