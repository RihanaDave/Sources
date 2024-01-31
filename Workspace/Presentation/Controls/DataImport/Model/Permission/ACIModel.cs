using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission
{
    public class ACIModel : BaseModel
    {
        private string groupName;
        public string GroupName
        {
            get => groupName;
            set => SetValue(ref groupName, value);
        }

        private bool isSelectable = true;
        public bool IsSelectable
        {
            get => isSelectable;
            set => SetValue(ref isSelectable, value);
        }

        private AccessControl.Permission accessLevel;
        public AccessControl.Permission AccessLevel
        {
            get => accessLevel;
            set
            {
                if (SetValue(ref accessLevel, value))
                {
                    IsSelectable = AccessLevel != AccessControl.Permission.Owner;
                    OnAccessLevelChanged();
                }
            }
        }

        ACLModel ownerACL;
        [XmlIgnore]
        public ACLModel OwnerACL
        {
            get => ownerACL;
            set => SetValue(ref ownerACL, value);
        }

        public event EventHandler AccessLevelChanged;
        protected void OnAccessLevelChanged()
        {
            AccessLevelChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
