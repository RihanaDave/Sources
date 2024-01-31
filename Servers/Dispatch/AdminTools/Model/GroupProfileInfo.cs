using GPAS.AccessControl.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model
{
    class GroupProfileInfo : BaseModel
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

        private string description;
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private string createdBy;
        public string CreatedBy
        {
            get => createdBy;
            set
            {
                createdBy = value;
                OnPropertyChanged();
            }
        }

        private string createdTime;
        public string CreatedTime
        {
            get => createdTime;
            set
            {
                createdTime = value;
                OnPropertyChanged();
            }
        }

        private int id;
        public int ID
        {
            get => id;
            set
            {
                this.id = value;
                OnPropertyChanged();
            }
        }

        public List<UserInfo> MembershipUsers { get; set; }
    }
}
