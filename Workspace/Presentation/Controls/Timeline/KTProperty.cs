using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Presentation.Controls.Timeline
{
    public class KTProperty : RelayNotifyPropertyChanged
    {
        private string typeUri;
        public string TypeURI
        {
            get => typeUri;
            set { SetValue(ref typeUri, value); }
        }
        
        private TimeRange value;
        public TimeRange Value
        {
            get => value;
            set { SetValue(ref this.value, value); }
        }

        private KWProperty relatedKWProperty;
        public KWProperty RelatedKWProperty
        {
            get => relatedKWProperty;
            set { SetValue(ref relatedKWProperty, value); }
        }
    }
}
