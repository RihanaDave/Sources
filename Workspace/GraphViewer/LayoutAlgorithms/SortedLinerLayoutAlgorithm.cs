using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using GPAS.Graph.GraphViewer.Foundations;
using System.Threading;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// الگوریتم چینش «خطی مرتب شده» گره ها در گراف
    /// </summary>
    public class SortedLinerLayoutAlgorithm : ILayoutAlgorithm
    {
        private readonly GraphData graphData;
        private readonly Dictionary<Vertex, Point> verteicesPrimaryPositions;
        private Dictionary<Vertex, Point> vertexFinalPositions = new Dictionary<Vertex, Point>();
        private readonly SortedLinerLayoutParameters parameters;

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="graph">شئ داده ای گرافی که الگوریتم روی آن اعمال خواهد شد</param>
        /// <param name="verteicesPositions">موقعیت گره هایی که می خواهیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="verticesSizes">اندازه گره هایی که می خواهعیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="param">پارامترهای الگوریتم چینش «خطی مرتب شده» گره ها که می خواهیم توسط این الگوریتم اعمال شوند</param>
        /// <remarks>
        /// نوع مورد استفاده برای گره (ورتکس) ها مرتب سازی این الگوریتم را مشخص می کند؛ می توان با بازنویسی (overrode) عملکرد مقایسه آن، نوع چینش را تغییر داد
        /// </remarks>
        public SortedLinerLayoutAlgorithm(GraphData graph, IDictionary<Vertex, Point> verteicesPositions, IDictionary<Vertex, Size> verticesSizes, SortedLinerLayoutParameters param)
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
        /// این الگوریتم با استفاده از ترتیب تعریف شده برای چینش گره ها، گره ها را
        /// به صورت خطی مرتب می کند و سعی دارد که چینش در وسط موقعیت مکانی گره ها پیش از چینش قرار گیرد
        /// </remarks>
        public void Compute(CancellationToken cancellationToken)
        {
            // TODO: Impelement Cancel capability

            // اعمال الگوریتم برای کمتر از دو گره بی معنی است
            if (verteicesPrimaryPositions.Count == 0)
                return;
            if (verteicesPrimaryPositions.Count == 1)
            {
                vertexFinalPositions.Add(verteicesPrimaryPositions.First().Key, verteicesPrimaryPositions.First().Value);
                return;
            }
            // محاسبه مجموع موقعیت مکانی افقی و عمودی و همچنین پهنای گره ها
            double verticesXSum = 0;
            double verticesYSum = 0;
            double verticesWidthSum = 0;
            foreach (var item in verteicesPrimaryPositions)
            {
                verticesXSum += item.Value.X;
                verticesYSum += item.Value.Y;
                verticesWidthSum += VertexSizes[item.Key].Width;
            }
            // موقعیت مکانی گره بعدی (در اینجا اولین گره) را مشخص می کند
            Point nextVertexPosition
                = new Point
                    (verticesXSum / verteicesPrimaryPositions.Count - (verticesWidthSum + (parameters.GapBetweenVertices * (verteicesPrimaryPositions.Count - 1))) / 2
                    , verticesYSum / verteicesPrimaryPositions.Count);
            // گره ها را به ترتیبی مرتب شدن آن ها یکی یکی براساس آخرین موقعیت مکانی
            // تعیین شده (موقعیت گره بعدی - متغیر بالا) با رعایت فاصله داده شده در
            // پارامتر ورودی در لیست موقعیت نهایی چینش می کند
            foreach (var item in verteicesPrimaryPositions.OrderBy((arg) => { return arg.Key; }))
            {
                vertexFinalPositions.Add(item.Key, nextVertexPosition);
                nextVertexPosition = new Point(nextVertexPosition.X + VertexSizes[item.Key].Width + parameters.GapBetweenVertices, nextVertexPosition.Y);
            }
        }

        public void ResetGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
        {
        }

        /// <summary>
        /// نیاز به اندازه های گره ها برای محاسبه این الگوریتم را برمی گرداند
        /// </summary>
        public bool NeedVertexSizes
        { get { return true; } }
        /// <summary>
        /// پشتیبانی از قابلیت فریز اشیا توسط این الگوریتم را برمی گرداند
        /// </summary>
        public bool SupportsObjectFreeze
        { get { return false; } }
        /// <summary>
        /// موقعیت نهایی گره ها پس از محاسبه الگوریتم برای آن ها را برمی گرداند؛
        /// در صورتی که تعداد اعضای این دیکشنری از ورودی الگوریتم کمتر باشد (مثلا صفر باشد) الگوریتم کامل اجرا نشده است
        /// </summary>
        public IDictionary<Vertex, Point> VertexPositions
        { get { return vertexFinalPositions; } }
        /// <summary>
        /// اندازه گره ها برای استفاده توسط الگوریتم را مقداردهی می کند/برمی گرداند
        /// </summary>
        public IDictionary<Vertex, GraphX.Measure.Size> VertexSizes
        {
            get;
            set;
        }
    }
}