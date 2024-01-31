namespace GPAS.Dispatch.AdminTools.Model
{
    public class ServerStatusModel : BaseModel
    {
        private bool isAvailable;
        public bool IsAvailable
        {
            get => isAvailable;
            set
            {
                isAvailable = value;
                OnPropertyChanged();
            }
        }

        private string serverName;
        public string ServerName
        {
            get => serverName;
            set
            {
                serverName = value;
                OnPropertyChanged();
            }
        }
    }
}
