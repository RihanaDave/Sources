using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    public class ShowOnGraphRequestedEventArgs : EventArgs
    {
        public ShowOnGraphRequestedEventArgs(IEnumerable<KWObject> objectRequestedToShowOnGraph)
        {
            ObjectRequestedToShowOnGraph = objectRequestedToShowOnGraph;
        }
        public IEnumerable<KWObject> ObjectRequestedToShowOnGraph
        {
            get;
            private set;
        }
    }
}
