using GPAS.Workspace.Entities;
using System;

namespace GPAS.Workspace.Presentation.Controls.Map
{
    public class SimpleMarkerDoubleClickEventArgs :EventArgs
    {
        public KWObject SimpleMarker { get; }

        public SimpleMarkerDoubleClickEventArgs(KWObject simpleMarker)
        {
            if (simpleMarker == null)
            {
                throw new ArgumentNullException(nameof(simpleMarker));
            }

            SimpleMarker = simpleMarker;
        }
    }
}
