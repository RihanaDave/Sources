using System;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows
{
    public class SnapshotRequestEventArgs : EventArgs
    {
        public SnapshotRequestEventArgs(UIElement uIElement, string defaultFileName)
        {
            UIElement = uIElement;
            DefaultFileName = defaultFileName;
        }

        public UIElement UIElement { get; protected set; }
        public string DefaultFileName { get; protected set; }
    }
}
