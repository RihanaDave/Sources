using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.MapViewer
{
    public class LinessMouseButtonEventArgs
    {
        public LinessMouseButtonEventArgs(MouseButtonEventArgs mouseButtonEventArgs, LineLatLng line, int index)
        {
            this.mouseButtonEventArgs = mouseButtonEventArgs;
            this.line = line;
            this.index = index;
        }

        MouseButtonEventArgs mouseButtonEventArgs;
        LineLatLng line;
        int index;

        public LineLatLng Line { get { return line; } }

        public int Index { get { return index; } }

        public MouseButtonEventArgs MouseButtonEventArgs { get { return mouseButtonEventArgs; } }
    }

    public delegate void LinesMouseButtonEventHandler(object sender, LinessMouseButtonEventArgs e);
}
