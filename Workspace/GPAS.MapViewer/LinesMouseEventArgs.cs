using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public class LinesMouseEventArgs
    {
        public LinesMouseEventArgs(MouseEventArgs mouseEventArgs, LineLatLng line, int index)
        {
            this.mouseEventArgs = mouseEventArgs;
            this.line = line;
            this.index = index;
        }

        MouseEventArgs mouseEventArgs;
        LineLatLng line;
        int index;

        public LineLatLng Line { get { return line; } }

        public int Index { get { return index; } }

        public MouseEventArgs MouseEventArgs { get { return mouseEventArgs; } }
    }

    public delegate void LinesMouseEventHandler(object sender, LinesMouseEventArgs e);
}
