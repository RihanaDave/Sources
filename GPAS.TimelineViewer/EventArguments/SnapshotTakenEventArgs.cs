using System;
using System.Windows;

namespace GPAS.TimelineViewer.EventArguments
{
    public class SnapshotTakenEventArgs : EventArgs
    {
        public SnapshotTakenEventArgs(UIElement uIElement)
        {
            UIElement = uIElement;
        }

        public UIElement UIElement { get; protected set; }
    }
}
