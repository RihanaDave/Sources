using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using GPAS.Graph.GraphViewer.Foundations;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using System.Threading;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// الگوریتم چینش «دایره ای» گره ها در گراف
    /// </summary>
    public class CircleLayoutAlgorithm : ILayoutAlgorithm
    {
        private readonly GraphData graphData;
        private readonly Dictionary<Vertex, Point> verteicesPrimaryPositions;
        private Dictionary<Vertex, Point> vertexFinalPositions = new Dictionary<Vertex, Point>();
        private CircularLayoutAlgorithm<Vertex, Edge, GraphData> alg = null;
        private readonly CircleLayoutParameters parameters;
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="graph">شئ داده ای گرافی که الگوریتم روی آن اعمال خواهد شد</param>
        /// <param name="verteicesPositions">موقعیت گره هایی که می خواهیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="verticesSizes">اندازه گره هایی که می خواهعیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="param">پارامترهای الگوریتم چینش «دایره ای» گره ها که می خواهیم توسط این الگوریتم اعمال شوند</param>
        public CircleLayoutAlgorithm(GraphData graph, IDictionary<Vertex, Point> verteicesPositions, IDictionary<Vertex, Size> verticesSizes, CircleLayoutParameters param)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (verteicesPositions == null)
                throw new ArgumentNullException("verteicesPositions");
            if (verticesSizes == null)
                throw new ArgumentNullException("verticesSizes");
            if (param == null)
                throw new ArgumentNullException("param");

            // مقداردهی اولیه خصوصیات داخلی الگوریتم
            graphData = graph;
            verteicesPrimaryPositions = (Dictionary<Vertex, Point>)verteicesPositions;
            VertexSizes = verticesSizes;
            parameters = param;
        }
        /// <summary>
        /// از سرگیری پردازش الگوریتم چینش
        /// </summary>
        /// <remarks>
        /// در پیاده سازی این الگوریتم از الگوریتم چینش «دایره ای» گراف ایکس استفاده شده است؛
        /// مشکلی که استفاده از این الگوریتم وجود دارد، قرار شروع شدن چینش گره ها از نقطه صفر و صفر
        /// است که به خاطر احتمال جابجایی گره از دید کاربر مطلوب ما نیست.
        /// برای رفع این مشکل، یک نقطه به عنوان موقعیت مرکزی چینش در نظر گرفته می شود و همه گره ها
        /// پس از اعمال چینش دایره ای به آن ها به نسبت آن موقعیت مرکزی جابجا می شوند
        /// </remarks>
        public void Compute(CancellationToken cancellationToken)
        {
            // اعمال الگوریتم برای کمتر از دو گره بی معنی است
            if (verteicesPrimaryPositions.Count == 0)
                return;
            if (verteicesPrimaryPositions.Count == 1)
            {
                vertexFinalPositions.Add(verteicesPrimaryPositions.First().Key, verteicesPrimaryPositions.First().Value);
                return;
            }
            // تعیین موقعیت های مورد نیاز (برای تعیین موقعیت مرکزی که در توضیحات این عملکرد ذکر شد) با استفاده از موقعیت ابتدایی گره ها
            double verticesMinimumXBeforeRelayout = verteicesPrimaryPositions.First().Value.X;
            double verticesMinimumYBeforeRelayout = verteicesPrimaryPositions.First().Value.Y;
            double verticesMaximumXBeforeRelayout = verteicesPrimaryPositions.First().Value.X;
            double verticesMaximumYBeforeRelayout = verteicesPrimaryPositions.First().Value.Y;
            foreach (var item in verteicesPrimaryPositions)
            {
                if (verticesMinimumXBeforeRelayout > item.Value.X)
                    verticesMinimumXBeforeRelayout = item.Value.X;
                else if (verticesMaximumXBeforeRelayout < item.Value.X)
                    verticesMaximumXBeforeRelayout = item.Value.X;
                if (verticesMinimumYBeforeRelayout > item.Value.Y)
                    verticesMinimumYBeforeRelayout = item.Value.Y;
                else if (verticesMaximumYBeforeRelayout < item.Value.Y)
                    verticesMaximumYBeforeRelayout = item.Value.Y;
            }
            Point centerPointBeforeRelayout = new Point((verticesMinimumXBeforeRelayout + verticesMaximumXBeforeRelayout) / 2, (verticesMinimumYBeforeRelayout + verticesMaximumYBeforeRelayout) / 2);
            // آماده سازی و اعمال چینش دایره ای گراف ایکس به گره های مورد نظر
            alg = new CircularLayoutAlgorithm<Vertex, Edge, GraphData>
                (graphData, verteicesPrimaryPositions
                , VertexSizes, new CircularLayoutParameters() { });
            if (alg == null)
                return;
            alg.Compute(cancellationToken);
            // تعیین موقعیت مرکزی گره ها پس از اعمال چینش دایره ای گراف ایکس
            double verticesMinimumXAfterRelayout = alg.VertexPositions.First().Value.X;
            double verticesMinimumYAfterRelayout = alg.VertexPositions.First().Value.Y;
            double verticesMaximumXAfterRelayout = alg.VertexPositions.First().Value.X;
            double verticesMaximumYAfterRelayout = alg.VertexPositions.First().Value.Y;
            foreach (var item in alg.VertexPositions)
            {
                if (verticesMinimumXAfterRelayout > item.Value.X)
                    verticesMinimumXAfterRelayout = item.Value.X;
                else if (verticesMaximumXAfterRelayout < item.Value.X)
                    verticesMaximumXAfterRelayout = item.Value.X;
                if (verticesMinimumYAfterRelayout > item.Value.Y)
                    verticesMinimumYAfterRelayout = item.Value.Y;
                else if (verticesMaximumYAfterRelayout < item.Value.Y)
                    verticesMaximumYAfterRelayout = item.Value.Y;
            }
            Point centerPointAfterRelayout = new Point((verticesMinimumXAfterRelayout + verticesMaximumXAfterRelayout) / 2, (verticesMinimumYAfterRelayout + verticesMaximumYAfterRelayout) / 2);
            // تعیین تفاضل میان نقطه مرکزی پیش و پس از اعمال چینش
            double horizontalDifference = centerPointBeforeRelayout.X - centerPointAfterRelayout.X;
            double verticalDifference = centerPointBeforeRelayout.Y - centerPointAfterRelayout.Y;

            System.Windows.Point firstVertexFinalPosition = new System.Windows.Point
                (alg.VertexPositions.First().Value.X + horizontalDifference
                , alg.VertexPositions.First().Value.Y + verticalDifference);
            var centerWindowsPosition = new System.Windows.Point(centerPointBeforeRelayout.X, centerPointBeforeRelayout.Y);
            double VerticesDistanceFromCenter = System.Windows.Point.Subtract(centerWindowsPosition, firstVertexFinalPosition).Length;

            bool IsIncreaseDistanceFromCenterNeeded = false;
            if (parameters != null && VerticesDistanceFromCenter < parameters.VerticesMinimumDistanceFromCenter)
            {
                IsIncreaseDistanceFromCenterNeeded = true;
            }

            // انتقال موقعیت گره ها به محوریت مرکز پیش از بازچینش گره ها و ثبت در مجموعه نهایی
            if (IsIncreaseDistanceFromCenterNeeded)
            {
                double DistanceChangeRatio = parameters.VerticesMinimumDistanceFromCenter / VerticesDistanceFromCenter;
                foreach (var item in alg.VertexPositions)
                {
                    double newX = item.Value.X + horizontalDifference;
                    double newY = item.Value.Y + verticalDifference;
                    
                    Point finalPosition = new Point
                        (((newX - centerPointBeforeRelayout.X) * DistanceChangeRatio) + centerPointBeforeRelayout.X
                        , ((newY - centerPointBeforeRelayout.Y) * DistanceChangeRatio) + centerPointBeforeRelayout.Y);

                    vertexFinalPositions.Add(item.Key, finalPosition);
                }
            }
            else
            {
                foreach (var item in alg.VertexPositions)
                {
                    double newX = item.Value.X + horizontalDifference;
                    double newY = item.Value.Y + verticalDifference;
                    vertexFinalPositions.Add(item.Key, new Point(newX, newY));
                }
            }
        }

        public void ResetGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
        {
            if (alg != null)
            {
                alg.ResetGraph(vertices, edges);
            }
        }

        /// <summary>
        /// نیاز به اندازه های گره ها برای محاسبه این الگوریتم را برمی گرداند
        /// </summary>
        public bool NeedVertexSizes
        {
            get { return true; }
        }
        /// <summary>
        /// پشتیبانی از قابلیت فریز اشیا توسط این الگوریتم را برمی گرداند
        /// </summary>
        public bool SupportsObjectFreeze
        {
            get { return false; }
        }
        /// <summary>
        /// موقعیت نهایی گره ها پس از محاسبه الگوریتم برای آن ها را برمی گرداند؛
        /// در صورتی که تعداد اعضای این دیکشنری از ورودی الگوریتم کمتر باشد (مثلا صفر باشد) الگوریتم کامل اجرا نشده است
        /// </summary>
        public IDictionary<Foundations.Vertex, Point> VertexPositions
        {
            get { return vertexFinalPositions; }
        }
        /// <summary>
        /// اندازه گره ها برای استفاده توسط الگوریتم را مقداردهی می کند/برمی گرداند
        /// </summary>
        public IDictionary<Foundations.Vertex, GraphX.Measure.Size> VertexSizes
        {
            get;
            set;
        }
    }
}
