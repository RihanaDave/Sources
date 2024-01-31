using GPAS.Workspace.Entities;
using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public class RelatedEventForShowInBrowser : INotifyPropertyChanged
    {
        public RelatedEventForShowInBrowser()
        {
            relationshipBasedKwLink = new RelationshipBasedKWLink();
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    NotifyPropertyChanged("Type");
                }
            }
        }

        private string relationship;
        public string Relationship
        {
            get => relationship;
            set
            {
                if (relationship != value)
                {
                    relationship = value;
                    NotifyPropertyChanged("Relationship");
                }
            }
        }

        private string label;
        public string Label
        {
            get => label;
            set
            {
                if (label != value)
                {
                    label = value;
                    NotifyPropertyChanged("Label");
                }
            }
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                if (text != value)
                {
                    text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        private string timeBegin;
        public string TimeBegin
        {
            get => timeBegin;
            set
            {
                if (timeBegin != value)
                {
                    timeBegin = value;
                    NotifyPropertyChanged("TimeBegin");
                }
            }
        }

        private string timeEnd;
        public string TimeEnd
        {
            get => timeEnd;
            set
            {
                if (timeEnd != value)
                {
                    timeEnd = value;
                    NotifyPropertyChanged("TimeEnd");
                }
            }
        }

        private RelationshipBasedKWLink relationshipBasedKwLink;
        public RelationshipBasedKWLink RelationshipBasedKwLink
        {
            get => relationshipBasedKwLink;
            set
            {
                relationshipBasedKwLink = value;
                NotifyPropertyChanged("RelationshipBasedKWLink");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
