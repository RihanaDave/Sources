using System.Collections.ObjectModel;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class AccessControlLimitModel : BaseModel
    {
        private string classification;
        public string Classification
        {
            get => classification;
            set
            {
                classification = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PermissionModel> permissions;
        public ObservableCollection<PermissionModel> Permissions
        {
            get => permissions;
            set
            {
                permissions = value;
                OnPropertyChanged();
            }
        }
    }
}
