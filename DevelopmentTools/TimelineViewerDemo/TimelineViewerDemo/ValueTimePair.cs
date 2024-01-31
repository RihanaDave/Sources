using System;

namespace TimelineViewerDemo
{
    public class ValueTimePair
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if(value != isSelected)
                {
                    isSelected = value;
                    OnIsSelectedChanged();
                }
            }
        }

        public event EventHandler IsSelectedChanged;
        protected void OnIsSelectedChanged()
        {
            IsSelectedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
