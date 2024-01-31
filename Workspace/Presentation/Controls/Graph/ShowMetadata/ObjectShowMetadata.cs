using GPAS.Workspace.Entities;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    /// <summary>
    /// جزئیات لازم برای نمایش یک شئ توسط گراف
    /// </summary>
    public class ObjectShowMetadata
    {
        public ObjectShowMetadata()
        {
            ObjectToShow = null;
            NonDefaultPositionX = double.NaN;
            NonDefaultPositionY = double.NaN;
            NewVisiblity = Visibility.Visible;
            ForceShowSubGroups = true;
        }

        public KWObject ObjectToShow { get; set; }
        public double NonDefaultPositionX { get; set; }
        public double NonDefaultPositionY { get; set; }
        public Visibility NewVisiblity { get; set; }
        public bool ForceShowSubGroups { get; set; }
    }
}
