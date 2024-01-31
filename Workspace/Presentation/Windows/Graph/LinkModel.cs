using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public class LinkModel : BaseModel
    {
        private int id;
        public int Id
        {
            get => id;
            set
            {
                SetValue(ref id, value);
            }
        }

        private string sourceTypeUri;
        public string SourceTypeUri
        {
            get => sourceTypeUri;
            set
            {
                SetValue(ref sourceTypeUri, value);
            }
        }

        private string sourceDisplayName;
        public string SourceDisplayName
        {
            get => sourceDisplayName;
            set
            {
                SetValue(ref sourceDisplayName, value);
            }
        }

        private string targetTypeUri;
        public string TargetTypeUri
        {
            get => targetTypeUri;
            set
            {
                SetValue(ref targetTypeUri, value);
            }
        }

        private string targetDisplayName;
        public string TargetDisplayName
        {
            get => targetDisplayName;
            set
            {
                SetValue(ref targetDisplayName, value);
            }
        }

        private LinkDirection direction;
        public LinkDirection Direction
        {
            get => direction;
            set
            {
                DirectionChanged = !value.Equals(OldDirection);
                SetValue(ref direction, value);
                PrepareEditedProperty();
            }
        }

        private string linkType = string.Empty;
        public string LinkType
        {
            get => linkType;
            set
            {
                LinkTypeChanged = !value.Equals(OldLinkType);
                SetValue(ref linkType, value);
                PrepareEditedProperty();
            }
        }

        private string description = string.Empty;
        public string Description
        {
            get => description;
            set
            {
                DescriptionChanged = !value.Equals(OldDescription);
                SetValue(ref description, value);
                PrepareEditedProperty();
            }
        }

        private string linkTypeToShow = string.Empty;
        public string LinkTypeToShow
        {
            get => linkTypeToShow;
            set
            {
                SetValue(ref linkTypeToShow, value);
            }
        }

        private bool isUnpublished;
        public bool IsUnpublished
        {
            get => isUnpublished;
            set
            {
                SetValue(ref isUnpublished, value);
            }
        }

        private object sourceObject;
        public object SourceObject
        {
            get => sourceObject;
            set
            {
                SetValue(ref sourceObject, value);
            }
        }

        private object targetObject;
        public object TargetObject
        {
            get => targetObject;
            set
            {
                SetValue(ref targetObject, value);
            }
        }

        private object tag;
        public object Tag
        {
            get => tag;
            set
            {
                SetValue(ref tag, value);
            }
        }

        private bool edited;
        public bool Edited
        {
            get => edited;
            set
            {
                SetValue(ref edited, value);
            }
        }

        private bool selectable;
        public bool Selectable
        {
            get => selectable;
            set
            {
                SetValue(ref selectable, value);
            }
        }

        public string OldLinkType { get; set; }
        public LinkDirection OldDirection { get; set; }
        public string OldDescription { get; set; }

        public bool DirectionChanged { get; set; }
        public bool LinkTypeChanged { get; set; }
        public bool DescriptionChanged { get; set; }

        private void PrepareEditedProperty()
        {
            Edited = DescriptionChanged || LinkTypeChanged || DirectionChanged;
        }
    }
}
