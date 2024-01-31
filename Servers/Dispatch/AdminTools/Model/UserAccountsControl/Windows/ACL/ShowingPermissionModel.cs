using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model.UserAccountsControl.Windows.ACL
{
    public class ShowingPermissionModel : BaseModel
    {
        private string permission;
        public string Permission
        {
            get => permission;
            set
            {
                permission = value;
                OnPropertyChanged();
            }
        }

        private bool isSelectable;
        public bool IsSelectable
        {
            get => isSelectable;
            set
            {
                isSelectable = value;
                OnPropertyChanged();
            }
        }
    }
}
