using System.ComponentModel;

namespace GPAS.TagCloudViewer.LayoutAlgorithms
{
    /// <summary>
    /// نوع شمارشی الگوریتم های چینش گره ها در نمایشگر گراف؛
    /// این انواع شمارشی دارای توضیحات در ویژگی Description  خود می باشند
    /// </summary>
    public enum LayoutAlgorithmTypeEnum
    {
        [Description("Tag Cloud Layout")]
        TagCloud
    }
}
