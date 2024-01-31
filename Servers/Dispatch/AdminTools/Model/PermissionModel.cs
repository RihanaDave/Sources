using GPAS.AccessControl;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class PermissionModel : BaseModel
    {
        private string groupName;
        public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;
                OnPropertyChanged();
            }
        }

        private Permission accessLevel;
        public Permission AccessLevel
        {
            get => accessLevel;
            set
            {
                accessLevel = value;
                OnPropertyChanged();
            }
        }
    }
}
