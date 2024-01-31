namespace GPAS.Dispatch.AdminTools.Model.UserAndGroup
{
    public class GroupModel : BaseModel
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
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

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Name = string.Empty;
            Description = string.Empty;
            Id = 0;
            CreatedTime = string.Empty;
            CreatedBy = string.Empty;
            IsEnabled = false;
            IsSelected = false;
        }
    }
}
