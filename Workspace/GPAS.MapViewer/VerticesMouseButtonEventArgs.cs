using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public class VerticesMouseButtonEventArgs
    {
        public VerticesMouseButtonEventArgs(MouseButtonEventArgs mouseButtonEventArgs, VertexLatLng vertex, int index)
        {
            this.mouseButtonEventArgs = mouseButtonEventArgs;
            this.vertex = vertex;
            this.index = index;
        }

        MouseButtonEventArgs mouseButtonEventArgs;
        VertexLatLng vertex;
        int index;

        public VertexLatLng Vertex { get { return vertex; } }

        public int Index { get { return index; } }

        public MouseButtonEventArgs MouseButtonEventArgs { get { return mouseButtonEventArgs; } }
    }

    public delegate void VerticesMouseButtonEventHandler(object sender, VerticesMouseButtonEventArgs e);
}
