using System;

namespace GPAS.Workspace.Presentation.Windows
{
    public class ThemeChangedEventArgs : EventArgs
    {
        public ThemeApplication Theme { get; private set; }

        public ThemeChangedEventArgs(ThemeApplication theme)
        {
            Theme = theme;
        }
    }
}
