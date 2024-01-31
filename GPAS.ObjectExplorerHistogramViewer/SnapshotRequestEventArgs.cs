using System;
using System.Windows;

namespace GPAS.ObjectExplorerHistogramViewer
{
    public class SnapshotRequestEventArgs : EventArgs
    {
        public SnapshotRequestEventArgs(UIElement uIElement)
        {
            UIElement = uIElement;
        }

        public UIElement UIElement { get; private set; }
    }
}
