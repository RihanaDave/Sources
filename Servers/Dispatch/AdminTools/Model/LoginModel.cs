namespace GPAS.Dispatch.AdminTools.Model
{
    public class LoginModel : BaseModel
    {
        public LoginModel()
        {
#if DEBUG

            UserName = AccessControl.Users.NativeUser.Admin.ToString();
            Password = "admin";

#endif
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
    }
}
