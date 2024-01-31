using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Presentation.Controls.Browser.EventsArgs
{
    public class LoadInImageAnalysisRequestedEventArgs
    {
        public LoadInImageAnalysisRequestedEventArgs(KWObject objectRequested)
        {
            ObjectRequested = objectRequested;
        }

        public KWObject ObjectRequested
        {
            get;
        }
    }
}
