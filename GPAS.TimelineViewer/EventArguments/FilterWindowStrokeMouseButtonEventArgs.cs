using System.Windows.Input;

namespace GPAS.TimelineViewer.EventArguments
{
    internal class FilterWindowStrokeMouseButtonEventArgs : MouseButtonEventArgs
    {
        internal FilterWindowStrokeMouseButtonEventArgs(FilterWindowStrokeMouseEventState strokeState,
                                                            MouseDevice mouse,
                                                            int timestamp,
                                                            MouseButton button,
                                                            StylusDevice stylusDevice) : base(mouse, timestamp, button, stylusDevice)
        {
            StrokeState = strokeState;
        }

        internal FilterWindowStrokeMouseEventState StrokeState { get; private set; }
    }
}
