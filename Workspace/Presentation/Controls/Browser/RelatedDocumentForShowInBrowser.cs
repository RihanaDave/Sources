using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public class RelatedDocumentForShowInBrowser : BaseModel
    {
        private string relationship;
        public string Relationship
        {
            get => relationship;
            set
            {
                SetValue(ref relationship, value);
            }
        }

        private string related;
        public string Related
        {
            get => related;
            set
            {
                SetValue(ref related, value);
            }
        }

        private string dateBegin;
        public string DateBegin
        {
            get => dateBegin;
            set
            {
                SetValue(ref dateBegin, value);
            }
        }

        private string dateEnd;
        public string DateEnd
        {
            get => dateEnd;
            set
            {
                SetValue(ref dateEnd, value);
            }
        }

        private string text;
        public string Text
        {
            get => text;
            set
            {
                SetValue(ref text, value);
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                SetValue(ref type, value);
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                SetValue(ref name, value);
            }
        }

        private string relationshipType;
        public string RelationshipType
        {
            get => relationshipType;
            set
            {
                SetValue(ref relationshipType, value);
            }
        }

        private string documentTitle;
        public string DocumentTitle
        {
            get => documentTitle;
            set
            {
                SetValue(ref documentTitle, value);
            }
        }

        private RelationshipBasedKWLink relationshipBasedKwLink;
        public RelationshipBasedKWLink RelationshipBasedKwLink
        {
            get => relationshipBasedKwLink;
            set
            {
                SetValue(ref relationshipBasedKwLink, value);
            }
        }
    }
}
