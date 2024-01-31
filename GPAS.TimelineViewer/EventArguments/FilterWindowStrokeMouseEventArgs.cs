using System.Windows.Input;

namespace GPAS.TimelineViewer.EventArguments
{
    internal class FilterWindowStrokeMouseEventArgs : MouseEventArgs
    {
        internal FilterWindowStrokeMouseEventArgs(FilterWindowStrokeMouseEventState strokeState,
                                                            MouseDevice mouse,
                                                            int timestamp,
                                                            StylusDevice stylusDevice) : base(mouse, timestamp, stylusDevice)
        {
            StrokeState = strokeState;
        }

        internal FilterWindowStrokeMouseEventState StrokeState { get; private set; }
    }
}
