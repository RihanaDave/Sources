using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GPAS.BarChartViewer
{
    public class SelectionRangeStrokeMouseButtonEventArgs : MouseButtonEventArgs
    {
        public SelectionRangeStrokeMouseButtonEventArgs(SelectionRangeStrokeMouseEventState strokeState,
                                                            MouseDevice mouse,
                                                            int timestamp,
                                                            MouseButton button,
                                                            StylusDevice stylusDevice) : base(mouse, timestamp, button, stylusDevice)
        {
            StrokeState = strokeState;
        }

        public SelectionRangeStrokeMouseEventState StrokeState { get; private set; }
    }
}
