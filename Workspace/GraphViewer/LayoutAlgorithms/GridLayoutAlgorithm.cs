using System;
using System.Collections.Generic;
using System.Linq;
using GraphX.Measure;
using GPAS.Graph.GraphViewer.Foundations;
using System.Threading;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// الگوریتم چینش «جدولی» گره ها در گراف
    /// </summary>
    public class GridLayoutAlgorithm : ILayoutAlgorithm
    {
        private readonly GraphData graphData;
        private readonly Dictionary<Vertex, Point> verteicesPrimaryPositions;
        private Dictionary<Vertex, Point> vertexFinalPositions = new Dictionary<Vertex, Point>();
        private readonly GridLayoutParameters parameters;
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="graph">شئ داده ای گرافی که الگوریتم روی آن اعمال خواهد شد</param>
        /// <param name="verteicesPositions">موقعیت گره هایی که می خواهیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="verticesSizes">اندازه گره هایی که می خواهعیم چینش را روی آن ها اعمال کنیم</param>
        /// <param name="param">پارامترهای الگوریتم چینش «جدولی» گره ها که می خواهیم توسط این الگوریتم اعمال شوند</param>
        public GridLayoutAlgorithm(GraphData graph, IDictionary<Vertex, Point> verteicesPositions, IDictionary<Vertex, Size> verticesSizes, GridLayoutParameters param)
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
        /// این الگوریتم با گره ها را به صورت جدولی تقریبا مربعی شکل نمایش می دهد
        /// و سعی دارد چینش نتیجه را در وسط چینش سابق گره ها قرار دهد
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
            // محاسبه مجموع موقعیت های افقی و عمودی و نیز درازا و پهنای گره ها
            double verticesXSum = 0;
            double verticesYSum = 0;
            double verticesWidthSum = 0;
            double verticesHeightSum = 0;
            foreach (var item in verteicesPrimaryPositions)
            {
                verticesXSum += item.Value.X;
                verticesYSum += item.Value.Y;
                verticesWidthSum += VertexSizes[item.Key].Width;
                verticesHeightSum += VertexSizes[item.Key].Height;
            }
            // محاسبه میانگین درازا و پهنای گره ها
            double verticesMeanWidth = verticesWidthSum / verteicesPrimaryPositions.Count;
            double verticesMeanHeight = verticesHeightSum / verteicesPrimaryPositions.Count;
            // محاسبه بیشترین تعداد قابل نمایش در هر سطر و ستون از چینش نهایی
            int maxHorizontalVerticesCount, maxVerticalVerticesCount;
            maxHorizontalVerticesCount = maxVerticalVerticesCount = Convert.ToInt32(Math.Ceiling(Math.Sqrt(verteicesPrimaryPositions.Count)));
            // محاسبه موقعیت مکانی اولین گره چینش نهایی در گراف
            Point firstVertexPosition
                = new Point
                    (verticesXSum / verteicesPrimaryPositions.Count - (verticesWidthSum + (parameters.HorizontalGapBetweenVertices * (verteicesPrimaryPositions.Count - 1))) / maxHorizontalVerticesCount / 2
                    , verticesYSum / verteicesPrimaryPositions.Count - (verticesWidthSum + (parameters.VerticalGapBetweenVertices * (verteicesPrimaryPositions.Count - 1))) / maxVerticalVerticesCount / 2);
            // محاسبه موقعیت تک تک گره ها براساس مقادیر فوق
            int counter = 0;
            foreach (var item in verteicesPrimaryPositions)
            {
                vertexFinalPositions.Add
                    (item.Key
                    , new Point
                        ((counter % maxHorizontalVerticesCount) * (verticesMeanWidth + parameters.HorizontalGapBetweenVertices) + firstVertexPosition.X
                        , (counter / maxVerticalVerticesCount) * (verticesMeanHeight + parameters.VerticalGapBetweenVertices) + firstVertexPosition.Y));
                counter++;
            }
        }

        public void ResetGraph(IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
        {
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
