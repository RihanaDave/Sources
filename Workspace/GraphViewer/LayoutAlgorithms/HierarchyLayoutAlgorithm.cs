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
    /// الگوریتم چینش «سلسله‌مراتبی» گره ها در گراف
    /// </summary>
    public class HierarchyLayoutAlgorithm : ILayoutAlgorithm
    {
        private readonly GraphData graphData;
        private readonly Dictionary<Vertex, Point> verticesPrimaryPositions;
        private Dictionary<Vertex, Point> vertexFinalPositions = new Dictionary<Vertex, Point>();
        private EfficientSugiyamaLayoutAlgorithm<Vertex, Edge, GraphData> hierarchicalAlg = null;
        private readonly HierarchyLayoutParameters parameters;
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="graph">شئ داده ای گرافی که الگوریتم روی آن اعمال خواهد شد</param>
        /// <param name="verteicesPositions">موقعیت گره هایی که می خواهیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="verticesSizes">اندازه گره هایی که می خواهعیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="param">پارامترهای الگوریتم چینش «دایره ای» گره ها که می خواهیم توسط این الگوریتم اعمال شوند</param>
        public HierarchyLayoutAlgorithm(GraphData graph, IDictionary<Vertex, Point> verteicesPositions, IDictionary<Vertex, Size> verticesSizes, HierarchyLayoutParameters param)
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
            verticesPrimaryPositions = (Dictionary<Vertex, Point>)verteicesPositions;
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
            if (verticesPrimaryPositions.Count == 0)
                return;
            if (verticesPrimaryPositions.Count == 1)
            {
                // افزودن تنها گره ورودی به خروجی الگوریتم
                foreach (var item in verticesPrimaryPositions)
                    vertexFinalPositions.Add(item.Key, item.Value);
                return;
            }
            // در صورتی که یالی بین گره‌های ورودی وجود نداشته باشد، الگوریتم قادر به اعمال چینش نیست
            bool atleastOneEdgeIsBetweenGivenVertices = false;
            foreach (var item in graphData.Edges)
            {
                if (graphData.Vertices.Contains(item.Source) && graphData.Vertices.Contains(item.Target))
                {
                    atleastOneEdgeIsBetweenGivenVertices = true;
                    break;
                }
            }
            if (!atleastOneEdgeIsBetweenGivenVertices)
            {
                // افزودن گره‌های ورودی به خروجی الگوریتم
                foreach (var item in verticesPrimaryPositions)
                {
                    vertexFinalPositions.Add(item.Key, item.Value);
                }
                return;
            }

            // تعیین موقعیت های مورد نیاز (برای تعیین موقعیت مرکزی که در توضیحات این عملکرد ذکر شد) با استفاده از موقعیت ابتدایی گره ها
            double verticesMinimumXBeforeRelayout = verticesPrimaryPositions.First().Value.X;
            double verticesMinimumYBeforeRelayout = verticesPrimaryPositions.First().Value.Y;
            double verticesMaximumXBeforeRelayout = verticesPrimaryPositions.First().Value.X;
            double verticesMaximumYBeforeRelayout = verticesPrimaryPositions.First().Value.Y;
            foreach (var item in verticesPrimaryPositions)
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

            // آماده سازی و اعمال یکی از چینش‌های سلسله‌مراتبی گراف‌ایکس به گره های مورد نظر
            hierarchicalAlg = new EfficientSugiyamaLayoutAlgorithm<Vertex, Edge, GraphData>
                (graphData, new EfficientSugiyamaLayoutParameters() { VertexDistance = 150, WidthPerHeight = 150, LayerDistance = 150 }
                , verticesPrimaryPositions, VertexSizes);
            if (hierarchicalAlg == null)
                return;
            hierarchicalAlg.Compute(cancellationToken);

            // تعیین موقعیت مرکزی گره ها پس از اعمال چینش سلسله‌مراتبی گراف‌ایکس
            double verticesMinimumXAfterRelayout = hierarchicalAlg.VertexPositions.First().Value.X;
            double verticesMinimumYAfterRelayout = hierarchicalAlg.VertexPositions.First().Value.Y;
            double verticesMaximumXAfterRelayout = hierarchicalAlg.VertexPositions.First().Value.X;
            double verticesMaximumYAfterRelayout = hierarchicalAlg.VertexPositions.First().Value.Y;
            foreach (var item in hierarchicalAlg.VertexPositions)
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
            // انتقال موقعیت گره ها به محوریت مرکز پیش از بازچینش گره ها و ثبت در مجموعه نهایی
            foreach (var item in hierarchicalAlg.VertexPositions)
                vertexFinalPositions.Add(item.Key, new Point(item.Value.X + horizontalDifference, item.Value.Y + verticalDifference));
        }

        public void ResetGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
        {
            if (hierarchicalAlg != null)
            {
                hierarchicalAlg.ResetGraph(vertices, edges);
            }
        }

        /// <summary>
        /// نیاز به اندازه های گره ها برای محاسبه این الگوریتم را برمی گرداند
        /// </summary>
        public bool NeedVertexSizes
        {// TODO: خارجی: بررسی شود
            get { return true; }
        }
        /// <summary>
        /// پشتیبانی از قابلیت فریز اشیا توسط این الگوریتم را برمی گرداند
        /// </summary>
        public bool SupportsObjectFreeze
        {// TODO: خارجی: بررسی شود
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
