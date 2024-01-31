using System;

namespace GPAS.MapViewer
{
    public class MarkerDoubleClickEventArgs : EventArgs
    {
        public object Marker { get; }

        public MarkerDoubleClickEventArgs(object marker)
        {
            if (marker == null)
            {
                throw new ArgumentNullException(nameof(marker));
            }

            Marker = marker;
        }
    }
}
