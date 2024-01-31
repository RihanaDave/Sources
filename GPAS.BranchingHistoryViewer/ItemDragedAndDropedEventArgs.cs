using GPAS.BranchingHistoryViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.BranchingHistoryViewer
{
    public class ItemDragedAndDropedEventArgs : EventArgs
    {
        public ItemDragedAndDropedEventArgs(ObjectBase sourceItem, ObjectBase destinationItem, Point position)
        {
            SourceItem = sourceItem;
            DestinationItem = destinationItem;
            Position = position;
        }

        public ObjectBase SourceItem
        {
            get;
            private set;
        }

        public ObjectBase DestinationItem
        {
            get;
            private set;
        }

        public Point Position
        {
            get;
            private set;
        }
    }
}
