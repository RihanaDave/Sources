namespace GPAS.MapViewer
{
    public enum DrawingShape
    {
        None,
        Circle,
        Polygon,
        Route
    }

    public enum DrawingMode
    {
        Ready,
        SetPoint,
        Drawing,
        End,
        MovingPoint,
        MovingShape
    }
}