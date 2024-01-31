using System;
using System.Windows.Media.Imaging;

namespace GPAS.Histogram
{
    public class TakeSnapshotEventArgs : EventArgs
    {
        public PngBitmapEncoder Snapshot { get; protected set; }
        public string DefaultFileName { get; protected set; }

        public TakeSnapshotEventArgs(PngBitmapEncoder image, string defaultFileName)
        {
            Snapshot = image ?? throw new ArgumentNullException(nameof(image));
            DefaultFileName = defaultFileName;
        }
    }
}
