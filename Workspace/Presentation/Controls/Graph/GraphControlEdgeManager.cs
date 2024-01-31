using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Graph;
using System;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// کلاس مدیریت یال های کنترل گراف
    /// </summary>
    internal class GraphControlEdgeManager
    {
        /// <summary>
        /// سازنده کلاس؛ که به خاطر جلوگیری از ایجاد این شی توسط استفاده کننده بیرونی از دسترس خارج (محلی) شده است
        /// </summary>
        private GraphControlEdgeManager()
        { }
        
        public static Edge EdgeFactory(EdgeMetadata edgeMetadata, GraphControl graphControl)
        {
            if (edgeMetadata == null)
                throw new ArgumentNullException(nameof(edgeMetadata));
            if (graphControl == null)
                throw new ArgumentNullException(nameof(graphControl));
            
            // تلاش برای بازیابی گره های متناظر با مبدا و مقصد یال از گراف کنترل میزبان
            Vertex vrtxSource = graphControl.GetRelatedVertex(edgeMetadata.From);
            Vertex vrtxTarget = graphControl.GetRelatedVertex(edgeMetadata.To);
            // بررسی وجود گره های متناظر با مبدا و مقصد یال در گراف کنترل
            if (vrtxSource == null || vrtxTarget == null)
                throw new InvalidOperationException("The Source or/and Target Object(s) of the Link was/were not added to Graph before");
            // ایجاد یال جدید براساس ویژگی های لینک داده شده
            EdgeDirection newEdgeDirection = edgeMetadata.GetDirection();
            string newEdgeTitle = edgeMetadata.GetTitle();
            string newEdgeTypeUri = edgeMetadata.GetNearestTypeUri();
            Uri newEdgeIconUri = null;
            if(!string.IsNullOrWhiteSpace(newEdgeTypeUri))
            {
                newEdgeIconUri = OntologyIconProvider.GetTypeIconPath(newEdgeTypeUri);
            }
            Edge newEdge = new Edge(vrtxSource, vrtxTarget, newEdgeDirection, newEdgeTitle, newEdgeIconUri);
            // برگرداندن یال جدید به فراخواننده
            return newEdge;
        }

        internal static EdgeDirection GetEdgeDirectionFromLinkDirection(LinkDirection linkDirection)
        {
            switch (linkDirection)
            {
                case LinkDirection.SourceToTarget:
                    return EdgeDirection.FromSourceToTarget;
                case LinkDirection.TargetToSource:
                    return EdgeDirection.FromTargetToSource;
                case LinkDirection.Bidirectional:
                    return EdgeDirection.Bidirectional;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
