using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public class VerticesMouseEventArgs
    {
        public VerticesMouseEventArgs(MouseEventArgs mouseEventArgs, VertexLatLng vertex, int index)
        {
            this.mouseEventArgs = mouseEventArgs;
            this.vertex = vertex;
            this.index = index;
        }

        MouseEventArgs mouseEventArgs;
        VertexLatLng vertex;
        int index;

        public VertexLatLng Vertex { get { return vertex; } }

        public int Index { get { return index; } }

        public MouseEventArgs MouseEventArgs { get { return mouseEventArgs; } }
    }

    public delegate void VerticesMouseEventHandler(object sender, VerticesMouseEventArgs e);
}
