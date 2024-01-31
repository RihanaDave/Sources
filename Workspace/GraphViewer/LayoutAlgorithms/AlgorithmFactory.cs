using System;
using System.Collections.Generic;
using GraphX.Measure;
using GraphX.PCL.Common.Enums;
using QuickGraph;
using GPAS.Graph.GraphViewer.Foundations;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// کلاس ایجاد الگوریتم های مورد نیاز برای نمایشگر گراف
    /// </summary>
    public sealed class AlgorithmFactory
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public AlgorithmFactory()
        { }
        /// <summary>
        /// یک الگوریتم چینش گره ها ایجاد می کند و برمی گرداند
        /// </summary>
        public ILayoutAlgorithm CreateLayoutAlgorithm(LayoutAlgorithmTypeEnum newAlgorithmType, GraphData graph, IDictionary<Vertex, Point> positions, IDictionary<Vertex, Size> sizes, ILayoutParameters parameters = null)
        {
        //    if (graph == null)
        //        throw new ArgumentNullException("graph");
            if (positions == null)
                throw new ArgumentNullException("positions");
            if (sizes == null)
                throw new ArgumentNullException("sizes");

            // کد زیر از کدهای مبدا کنترل گراف ایکس گرفته شده است

            if (graph == null) return null;
            if (parameters == null) parameters = CreateLayoutParameters(newAlgorithmType);
            IMutableBidirectionalGraph<Vertex, Edge> _graph = (IMutableBidirectionalGraph<Vertex, Edge>)graph.ToBidirectionalGraph();
            // Freeze Vertex compatability could added to Layout Algorithms Factory
            /*var dic = new Dictionary<TVertex, Point>();
            if (Positions != null)
            {
                dic = Positions.Where(a => a.Key.SkipProcessing == ProcessingOptionEnum.Freeze).ToDictionary(a=> a.Key, a=> a.Value);
            }*/
            _graph.RemoveEdgeIf(a => a.SkipProcessing == ProcessingOptionEnum.Exclude);
            _graph.RemoveVertexIf(a => a.SkipProcessing == ProcessingOptionEnum.Exclude);
            switch (newAlgorithmType)
            {
                case LayoutAlgorithmTypeEnum.Auto:
                    return new AutoLayoutAlgorithm((GraphData)_graph, positions, sizes, parameters as AutoLayoutParameters);
                case LayoutAlgorithmTypeEnum.Grid:
                    return new GridLayoutAlgorithm((GraphData)_graph, positions, sizes, parameters as GridLayoutParameters);
                case LayoutAlgorithmTypeEnum.Hierarchy:
                    return new HierarchyLayoutAlgorithm((GraphData)_graph, positions, sizes, parameters as HierarchyLayoutParameters);
                case LayoutAlgorithmTypeEnum.Circle:
                    return new CircleLayoutAlgorithm((GraphData)_graph, positions, sizes, parameters as CircleLayoutParameters);
                case LayoutAlgorithmTypeEnum.SortedLiner:
                    return new SortedLinerLayoutAlgorithm((GraphData)_graph, positions, sizes, parameters as SortedLinerLayoutParameters);
                case LayoutAlgorithmTypeEnum.MinimumCrossing:
                    return new MinimumCrossingLayoutAlgorithm((GraphData)_graph, positions, sizes, parameters as MinimumCrossingLayoutParameters);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// پارامترهای پیش فرض برای هر یک نوع چینش گره ها را ایجاد می کند و برمی گرداند
        /// </summary>
        public ILayoutParameters CreateLayoutParameters(LayoutAlgorithmTypeEnum algorithmType)
        {
            // کد زیر از کد های مبدا کنترل گراف ایکس گرفته شده است

            switch (algorithmType)
            {
                case LayoutAlgorithmTypeEnum.Auto:
                    return new AutoLayoutParameters();
                case LayoutAlgorithmTypeEnum.Grid:
                    return new GridLayoutParameters();
                case LayoutAlgorithmTypeEnum.Hierarchy:
                    return new HierarchyLayoutParameters();
                case LayoutAlgorithmTypeEnum.Circle:
                    return new CircleLayoutParameters();
                case LayoutAlgorithmTypeEnum.SortedLiner:
                    return new SortedLinerLayoutParameters();
                case LayoutAlgorithmTypeEnum.MinimumCrossing:
                    return new MinimumCrossingLayoutParameters();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
