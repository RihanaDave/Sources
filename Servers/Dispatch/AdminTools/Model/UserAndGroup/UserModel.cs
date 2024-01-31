namespace GPAS.Dispatch.AdminTools.Model.UserAndGroup
{
    public class UserModel : BaseModel
    {
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

        private string userName;
        public string UserName
        {
            get => userName;
            set
            {
                userName = value;
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
        public int Id
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
                createdTime = value;
                OnPropertyChanged();
            }
        }

        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isAdmin;
        public bool IsAdmin
        {
            get => isAdmin;
            set
            {
                isAdmin = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Password = string.Empty;
            UserName = string.Empty;
            Status = string.Empty;
            CreatedBy = string.Empty;
            CreatedTime = string.Empty;
            Email = string.Empty;
            Id = 0;
            isSelected = false;
            isAdmin = false;
        }
    }
}
