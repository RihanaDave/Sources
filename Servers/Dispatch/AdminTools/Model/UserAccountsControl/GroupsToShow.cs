using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model.UserAccountsControl
{
    public class GroupsToShow : BaseModel
    {
        private bool isGroupSelected;
        public bool IsGroupSelected
        {
            get => isGroupSelected;
            set
            {
                this.isGroupSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isEnable;
        public bool IsEnable
        {
            get => isEnable;
            set
            {
                this.isEnable = value;
                OnPropertyChanged();
            }
        }

        private string groupName;
        public string GroupName
        {
            get => groupName;
            set
            {
                this.groupName = value;
                OnPropertyChanged();
            }
        }
    }
}
