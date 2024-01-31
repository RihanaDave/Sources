using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.BarChartViewer
{
    public class SelectionRangeStrokeMouseEventArgs : MouseEventArgs
    {
        public SelectionRangeStrokeMouseEventArgs(SelectionRangeStrokeMouseEventState strokeState,
                                                            MouseDevice mouse,
                                                            int timestamp,
                                                            StylusDevice stylusDevice) : base(mouse, timestamp, stylusDevice)
        {
            StrokeState = strokeState;
        }

        public SelectionRangeStrokeMouseEventState StrokeState { get; private set; }
    }
}
