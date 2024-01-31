using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Presentation.Controls.Browser.EventsArgs
{
    public class ShowOnMapRequestedEventArgs
    {
        public ShowOnMapRequestedEventArgs(KWObject objectRequestedToShowOnMap)
        {
            ObjectRequestedToShowOnMap = objectRequestedToShowOnMap;
        }
        public KWObject ObjectRequestedToShowOnMap
        {
            get;
        }
    }
}
