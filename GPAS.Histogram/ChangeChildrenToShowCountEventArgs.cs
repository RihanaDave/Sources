using System;

namespace GPAS.Histogram
{
    public class ChangeChildrenToShowCountEventArgs : EventArgs
    {
        public HistogramItem SelectedItem { get; }

        public ChangeChildrenToShowCountEventArgs(HistogramItem item)
        {
            SelectedItem = item ?? throw new ArgumentNullException(nameof(item));
        }
    }
}
