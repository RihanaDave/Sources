using GPAS.AccessControl.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model.UserAndGroup
{
    public class UserProfileInfoModel : BaseModel
    {
        private string fullName;
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged();
            }
        }

        private string firstName;
        public string FirstName
        {
            get => firstName;
            set
            {
                firstName = value;
                OnPropertyChanged();
            }
        }
        
        private string lastName;
        public string LastName
        {
            get => lastName;
            set
            {
                lastName = value;
                OnPropertyChanged();
            }
        }

        private string login;
        public string Login
        {
            get => login;
            set
            {
                login = value;
                OnPropertyChanged();
            }
        }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }
        
        private int id;
        public int ID
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }
        
        private string status;
        public string Status
        {
            get => status;
            set
            {
                status = value;
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
                this.createdTime = value;
                OnPropertyChanged();
            }
        }

        public UserInfo userInfo { get; set; }

    }
}

