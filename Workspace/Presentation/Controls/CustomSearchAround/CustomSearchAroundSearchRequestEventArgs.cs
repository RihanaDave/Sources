using GPAS.Workspace.Entities;
using System;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround
{
    public class CustomSearchAroundSearchRequestEventArgs : EventArgs
    {
        public CustomSearchAroundSearchRequestEventArgs(KWCustomSearchAroundResult searchAroundResult)
        {
            SearchAroundResult = searchAroundResult;
        }

        public KWCustomSearchAroundResult SearchAroundResult
        {
            get; protected set;
        }
    }
}
