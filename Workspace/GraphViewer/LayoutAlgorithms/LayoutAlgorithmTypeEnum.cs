using System.ComponentModel;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// نوع شمارشی الگوریتم های چینش گره ها در نمایشگر گراف؛
    /// این انواع شمارشی دارای توضیحات در ویژگی Description  خود می باشند
    /// </summary>
    public enum LayoutAlgorithmTypeEnum
    {
        [Description("Auto Layout")]
        Auto,
        [Description("Grid Layout")]
        Grid,
        [Description("Hierarchy Layout")]
        Hierarchy,
        [Description("Circle Layout")]
        Circle,
        [Description("Sorted Liner Layout")]
        SortedLiner,
        [Description("Minimum Crossing Layout")]
        MinimumCrossing
    }
}
