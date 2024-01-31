using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission
{
    public class ClassificationModel : BaseModel
    {
        private string title;
        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }

        private string identifier;
        public string Identifier
        {
            get => identifier;
            set => SetValue(ref identifier, value);
        }

        private bool isSelectable;
        public bool IsSelectable
        {
            get => isSelectable;
            set => SetValue(ref isSelectable, value);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (SetValue(ref isSelected, value))
                {
                    OnSelectionChange();
                }
            }
        }

        ACLModel ownerACL;
        [XmlIgnore]
        public ACLModel OwnerACL
        {
            get => ownerACL;
            set=> SetValue(ref ownerACL, value);
        }

        public event EventHandler SelectionChange;
        protected void OnSelectionChange()
        {
            SelectionChange?.Invoke(this, EventArgs.Empty);
        }

        public bool Equals(ClassificationModel classification)
        {
            return Identifier.Equals(classification?.Identifier);
        }

        public bool Equals(string identifier)
        {
            return Identifier.Equals(identifier);
        }
    }
}
