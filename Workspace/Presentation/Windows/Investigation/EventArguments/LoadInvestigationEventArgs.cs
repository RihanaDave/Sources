using GPAS.Workspace.Presentation.Windows.Investigation.Models;
using System;

namespace GPAS.Workspace.Presentation.Windows.Investigation.EventArguments
{
    public class LoadInvestigationEventArgs : EventArgs
    {
        public LoadInvestigationEventArgs(InvestigationModel investigationModel)
        {
            InvestigationStatus = investigationModel;
        }

        public InvestigationModel InvestigationStatus
        {
            get;
            protected set;
        }
    }
}
