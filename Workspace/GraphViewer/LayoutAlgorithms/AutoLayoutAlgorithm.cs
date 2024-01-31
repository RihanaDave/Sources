using GraphX.Measure;
using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using System.Threading;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    public class AutoLayoutAlgorithm : ILayoutAlgorithm
    {
        private readonly GraphData graphData;
        private readonly Dictionary<Vertex, Point> verticesPrimaryPositions;
        private Dictionary<Vertex, Point> verticesFinalPositions = new Dictionary<Vertex, Point>();
        private CircularLayoutAlgorithm<Vertex, Edge, GraphData> circularLayoutAlg = null;
        private LinLogLayoutAlgorithm<Vertex, Edge, GraphData> linlogLayoutAlg = null;
        private readonly AutoLayoutParameters parameters;
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="graph">شئ داده ای گرافی که الگوریتم روی آن اعمال خواهد شد</param>
        /// <param name="verteicesPositions">موقعیت گره هایی که می خواهیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="verticesSizes">اندازه گره هایی که می خواهعیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="param">پارامترهای الگوریتم چینش «خودکار» گره ها که می خواهیم توسط این الگوریتم اعمال شوند</param>
        internal AutoLayoutAlgorithm(GraphData graph, IDictionary<Vertex, Point> verteicesPositions, IDictionary<Vertex, Size> verticesSizes, AutoLayoutParameters param)
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
        /// در پیاده سازی این الگوریتم از الگوریتم چینش «لین لاگ» و نیز الگور یتم رفع
        /// همپوشانی «اف.اس.ای» گراف ایکس استفاده شده است؛
        /// برای حفظ محوریت موقعیت ابتدایی گره‌های گراف پس از اعمال الگوریتم (حفظ
        /// موقعیت بصری کاربر)، یک نقطه به عنوان موقعیت مرکزی چینش در نظر گرفته
        /// می شود و همه گره ها پس از اعمال چینش‌ها به آن ها به نسبت آن موقعیت
        /// مرکزی جابجا می شوند
        /// </remarks>
        public void Compute(CancellationToken cancellationToken)
        {
            // اعمال الگوریتم برای کمتر از دو گره بی معنی است
            if (verticesPrimaryPositions.Count == 0)
                return;
            if (verticesPrimaryPositions.Count == 1)
            {
                verticesFinalPositions.Add(verticesPrimaryPositions.First().Key, verticesPrimaryPositions.First().Value);
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

            // الگوریتم لین‌لاگ براساس چینش ابتدایی گره‌ها اعمال می‌شود؛
            // در صورتی که چینش ابتدایی گره‌ها در حالت‌های خاص باشد (مثلا گره‌ها روی هم باشند) چینش
            // خوبی در خروجی ارائه نخواهد شد
            // برای رفع این مشکل ابتدا الگوریتم چینش دایره‌ای به گره‌ها اعمال و سپس لین‌لاگ اعمال می‌شود

            //آماده سازی و اعمال چینش دایره‌ای گراف‌ایکس به گره های مورد نظر
            circularLayoutAlg = new CircularLayoutAlgorithm<Vertex, Edge, GraphData>
                (graphData, verticesPrimaryPositions
                , VertexSizes, new CircularLayoutParameters() { });
            if (circularLayoutAlg == null)
                return;
            circularLayoutAlg.Compute(cancellationToken);

            // آماده سازی و اعمال چینش لین‌لاگ گراف‌ایکس -با پارامترهای پیش‌فرض- به گره های مورد نظر
            linlogLayoutAlg = new LinLogLayoutAlgorithm<Vertex, Edge, GraphData>
                (graphData, circularLayoutAlg.VertexPositions
                , new LinLogLayoutParameters() { });
            if (linlogLayoutAlg == null)
                return;
            linlogLayoutAlg.Compute(cancellationToken);
            
            // آماده‌سازی خروجی الگوریتم چینش برای اعمال  لگوریتم حذف همپوشانی‌ها
            Dictionary<Vertex, Rect> afterLayoutAlgRects = new Dictionary<Vertex, Rect>();
            foreach (var item in VertexSizes)
                afterLayoutAlgRects.Add(item.Key, new Rect(linlogLayoutAlg.VertexPositions[item.Key], item.Value));

            // آماده سازی و اعمال چینش حذف همپوشانی اف.اس.ای به گره های مورد نظر
            FSAAlgorithm<Vertex>
                overlapRemovalAlg = new FSAAlgorithm<Vertex>
                    (afterLayoutAlgRects
                    , new OverlapRemovalParameters()
                    { HorizontalGap = parameters.HorizontalGapBetweenVertices, VerticalGap = parameters.VerticalGapBetweenVertices });
            if (overlapRemovalAlg == null)
                return;
            overlapRemovalAlg.Compute(cancellationToken);

            // تعیین موقعیت مرکزی گره ها پس از اعمال چینش
            double verticesMinimumXAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.X;
            double verticesMinimumYAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.Y;
            double verticesMaximumXAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.X;
            double verticesMaximumYAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.Y;
            foreach (var item in overlapRemovalAlg.Rectangles)
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
            foreach (var item in overlapRemovalAlg.Rectangles)
                verticesFinalPositions.Add(item.Key, new Point(item.Value.X + horizontalDifference, item.Value.Y + verticalDifference));
        }

        public void ResetGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
        {
            if (circularLayoutAlg != null)
                circularLayoutAlg.ResetGraph(vertices, edges);
            if (linlogLayoutAlg != null)
                linlogLayoutAlg.ResetGraph(vertices, edges);
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
            get { return verticesFinalPositions; }
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