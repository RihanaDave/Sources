using GraphX.Measure;
using GPAS.TagCloudViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.PCL.Logic.Algorithms.LayoutAlgorithms;
using GraphX.PCL.Logic.Algorithms.OverlapRemoval;
using System.Threading;
using GraphX.PCL.Common.Interfaces;

namespace GPAS.TagCloudViewer.LayoutAlgorithms
{
    public class TagCloudLayoutAlgorithm : ILayoutAlgorithm
    {
        private readonly GraphData graphData;
        private readonly Dictionary<Vertex, Point> verticesPrimaryPositions;
        private Dictionary<Vertex, Point> verticesFinalPositions = new Dictionary<Vertex, Point>();
        private CircularLayoutAlgorithm<Vertex, IGraphXEdge<Vertex>, GraphData> circularLayoutAlg = null;
        private LinLogLayoutAlgorithm<Vertex, IGraphXEdge<Vertex>, GraphData> linlogLayoutAlg = null;
        private readonly TagCloudLayoutParameters parameters;
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="graph">شئ داده ای گرافی که الگوریتم روی آن اعمال خواهد شد</param>
        /// <param name="verteicesPositions">موقعیت گره هایی که می خواهیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="verticesSizes">اندازه گره هایی که می خواهعیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="param">پارامترهای الگوریتم چینش «خودکار» گره ها که می خواهیم توسط این الگوریتم اعمال شوند</param>
        internal TagCloudLayoutAlgorithm(GraphData graph, IDictionary<Vertex, Point> verteicesPositions, IDictionary<Vertex, Size> verticesSizes, TagCloudLayoutParameters param)
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

            //Dictionary<Vertex, Rect> afterLayoutAlgRects = ApplyAlgorithm(verticesPrimaryPositions);

            //foreach (var item in afterLayoutAlgRects)
            //    verticesFinalPositions.Add(item.Key, new Point(item.Value.X, item.Value.Y));



            // الگوریتم لین‌لاگ براساس چینش ابتدایی گره‌ها اعمال می‌شود؛
            // در صورتی که چینش ابتدایی گره‌ها در حالت‌های خاص باشد (مثلا گره‌ها روی هم باشند) چینش
            // خوبی در خروجی ارائه نخواهد شد
            // برای رفع این مشکل ابتدا الگوریتم چینش دایره‌ای به گره‌ها اعمال و سپس لین‌لاگ اعمال می‌شود
            //KKLayoutAlgorithm<Vertex, IGraphXEdge<Vertex>, GraphData> alg
            //    = new KKLayoutAlgorithm<Vertex, IGraphXEdge<Vertex>, GraphData>(graphData, new KKLayoutParameters()
            //    {
            //        Height = 200,
            //        Width = 600,
            //        AdjustForGravity = false,
            //        DisconnectedMultiplier = 1,

            //    });
            Dictionary<Vertex, Thickness> vertexBorders = new Dictionary<Vertex, Thickness>(verticesPrimaryPositions.Count);
            Dictionary<Vertex, CompoundVertexInnerLayoutType> layoutTypes = new Dictionary<Vertex, CompoundVertexInnerLayoutType>(verticesPrimaryPositions.Count);
            foreach (var vp in verticesPrimaryPositions)
            {
                Vertex vertex = vp.Key;
                Point position = vp.Value;
                Size size = VertexSizes[vertex];
                vertexBorders.Add(vertex, new Thickness(position.X, position.Y, position.X + size.Width, position.Y + size.Height));
                layoutTypes.Add(vertex, vertex.Text.Equals("number") ? CompoundVertexInnerLayoutType.Contextual : CompoundVertexInnerLayoutType.Automatic);
            }
            CompoundFDPLayoutParameters oldParameters = new CompoundFDPLayoutParameters()
            {
                DisplacementLimitMultiplier = 0.2,
                ElasticConstant = 0.005,
                GravitationFactor = 100, //8
                IdealEdgeLength = 25,
                NestingFactor = 0.2,
                Phase1Iterations = 50, //50,
                Phase2Iterations = 70, //70,
                Phase2TemperatureInitialMultiplier = 0.5,
                Phase3Iterations = 30, //30,
                Phase3TemperatureInitialMultiplier = 0.2,
                RepulsionConstant = 150, //150
               // Seed = 1502244276,
                SeparationMultiplier = 15, //15
                TemperatureDecreasing = 0.5,
                TemperatureFactor = 0.95
            };

            CompoundFDPLayoutAlgorithm<Vertex, IGraphXEdge<Vertex>, GraphData> alg
                = new CompoundFDPLayoutAlgorithm<Vertex, IGraphXEdge<Vertex>, GraphData>(graphData, VertexSizes, vertexBorders, layoutTypes, verticesPrimaryPositions, oldParameters);
            alg.Compute(cancellationToken);

            // آماده‌سازی خروجی الگوریتم چینش برای اعمال  لگوریتم حذف همپوشانی‌ها
            Dictionary<Vertex, Rect> afterLayoutAlgRects = new Dictionary<Vertex, Rect>();
            foreach (var item in VertexSizes)
                afterLayoutAlgRects.Add(item.Key, new Rect(alg.VertexPositions[item.Key], item.Value));

            //// آماده سازی و اعمال چینش حذف همپوشانی اف.اس.ای به گره های مورد نظر
            //FSAAlgorithm<Vertex>
            //    overlapRemovalAlg = new FSAAlgorithm<Vertex>
            //        (afterLayoutAlgRects
            //        , new OverlapRemovalParameters() { HorizontalGap = 2, VerticalGap = 2 });
            //if (overlapRemovalAlg == null)
            //    return;
            //overlapRemovalAlg.Compute(cancellationToken);

            //// تعیین موقعیت مرکزی گره ها پس از اعمال چینش
            //double verticesMinimumXAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.X;
            //double verticesMinimumYAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.Y;
            //double verticesMaximumXAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.X;
            //double verticesMaximumYAfterRelayout = overlapRemovalAlg.Rectangles.First().Value.Y;
            //foreach (var item in overlapRemovalAlg.Rectangles)
            //{
            //    if (verticesMinimumXAfterRelayout > item.Value.X)
            //        verticesMinimumXAfterRelayout = item.Value.X;
            //    else if (verticesMaximumXAfterRelayout < item.Value.X)
            //        verticesMaximumXAfterRelayout = item.Value.X;
            //    if (verticesMinimumYAfterRelayout > item.Value.Y)
            //        verticesMinimumYAfterRelayout = item.Value.Y;
            //    else if (verticesMaximumYAfterRelayout < item.Value.Y)
            //        verticesMaximumYAfterRelayout = item.Value.Y;
            //}
            //Point centerPointAfterRelayout = new Point((verticesMinimumXAfterRelayout + verticesMaximumXAfterRelayout) / 2, (verticesMinimumYAfterRelayout + verticesMaximumYAfterRelayout) / 2);
            //// تعیین تفاضل میان نقطه مرکزی پیش و پس از اعمال چینش
            //double horizontalDifference = centerPointBeforeRelayout.X - centerPointAfterRelayout.X;
            //double verticalDifference = centerPointBeforeRelayout.Y - centerPointAfterRelayout.Y;
            //// انتقال موقعیت گره ها به محوریت مرکز پیش از بازچینش گره ها و ثبت در مجموعه نهایی
            //foreach (var item in overlapRemovalAlg.Rectangles)
            //    verticesFinalPositions.Add(item.Key, new Point(item.Value.X + horizontalDifference, item.Value.Y + verticalDifference));

            // تعیین موقعیت مرکزی گره ها پس از اعمال چینش
            double verticesMinimumXAfterRelayout = afterLayoutAlgRects.First().Value.X;
            double verticesMinimumYAfterRelayout = afterLayoutAlgRects.First().Value.Y;
            double verticesMaximumXAfterRelayout = afterLayoutAlgRects.First().Value.X;
            double verticesMaximumYAfterRelayout = afterLayoutAlgRects.First().Value.Y;
            foreach (var item in afterLayoutAlgRects)
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
            foreach (var item in afterLayoutAlgRects)
                verticesFinalPositions.Add(item.Key, new Point(item.Value.X + horizontalDifference, item.Value.Y + verticalDifference));
        }

        private Dictionary<Vertex, Rect> ApplyAlgorithm(Dictionary<Vertex, Point> verticesPrimaryPositions)
        {
            Dictionary<Vertex, Rect> result = new Dictionary<Vertex, Rect>();
            Vertex firstVertex = verticesPrimaryPositions.First().Key;

            foreach (Vertex checkingVertex in verticesPrimaryPositions.Keys)
            {
                Dictionary<Vertex, Rect> verticesRects = new Dictionary<Vertex, Rect>(result);
                //foreach (var item in result)
                //    verticesRects.Add(item.Key, item.Value);

                verticesRects.Add(checkingVertex, new Rect(verticesPrimaryPositions[checkingVertex], VertexSizes[checkingVertex]));

                FSAAlgorithm<Vertex> overlapRemovalAlg = new FSAAlgorithm<Vertex>
                    (verticesRects, new OverlapRemovalParameters() { HorizontalGap = 2, VerticalGap = 2 });
                overlapRemovalAlg.Compute(new CancellationToken());

                foreach (var item in overlapRemovalAlg.Rectangles)
                {
                    if (!result.ContainsKey(item.Key))
                    {
                        result.Add(item.Key, item.Value);
                    }
                    else
                    {
                        result[item.Key] = item.Value;
                    }
                }
            }

            return result;
        }

        public void ResetGraph(IEnumerable<Vertex> vertices, IEnumerable<IGraphXEdge<Vertex>> edges)
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