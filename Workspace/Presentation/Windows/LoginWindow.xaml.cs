using GPAS.Workspace.Logic;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows
{
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            SetVersion();
            Init();
        }

        public bool Result { get; protected set; } = false;

        private void SetVersion()
        {
            VersionTextBlock.Text = "version : " + Assembly.GetExecutingAssembly().GetName().Version;

            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
                return;

            CopyrightTextBlock.Text = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        private void PresentationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            Result = false;
            UserNameTextBox.Focus();
            UserNameTextBox.SelectAll();
#if DEBUG
#else
            PasswordTextBox.Password = string.Empty;
#endif
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginWaitingControl.Message = Properties.Resources.Securing_Connection_;
                LoginWaitingControl.TaskIncrement();
                await TryLogin();
            }
            finally
            {
                LoginWaitingControl.TaskDecrement();
            }
        }

        public bool SuccessfulLogin { get; set; } = false;

        private async Task TryLogin()
        {
            bool canUserLogin = false;
            Exception exception = null;
            string exceptionRecommandation = string.Empty;

            try
            {
                canUserLogin = await CanUserLoginAsync();
            }
            catch (Exception ex)
            {
                Result = false;
                if (ex is System.ServiceModel.Security.MessageSecurityException)
                {
                    if (ex?.InnerException?.Message != "User Name or Password is wrong...")
                    {
                        exception = ex;
                        exceptionRecommandation = Properties.Resources.Make_sure_your_computer_s_Time__is_synchronized_with_the_server;
                        ManageExceptionLogin(exception, exceptionRecommandation);
                        return;
                    }
                }
                else
                {
                    exception = ex;
                    ManageExceptionLogin(exception, exceptionRecommandation);
                    return;
                }
            }

            Result = canUserLogin;

            if (canUserLogin)
            {
                await ManageSuccessLogin();
            }
            else
            {
                ManageFailedLogin();
            }
        }

        private async Task ManageSuccessLogin()
        {
            LoginWaitingControl.Message = Properties.Resources.Loading_ontology___;
            // آماده سازی اولیه منطق محیط کاربری
            await Logic.System.InitializationAsync();
            Close();
        }

        private void ManageFailedLogin()
        {
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void ManageExceptionLogin(Exception exception, string exceptionRecommandation)
        {
            string exceptionMessage = $"{Properties.Resources.Login_Failed};{Environment.NewLine}{exception.Message}";
            if (!string.IsNullOrWhiteSpace(exceptionRecommandation))
            {
                exceptionMessage += $"{Environment.NewLine}{Environment.NewLine}{exceptionRecommandation}.";
            }
            Logger.ExceptionHandler exReporter = new Logger.ExceptionHandler();
            exReporter.WriteErrorLog(exception);
            KWMessageBox.Show(exceptionMessage, MessageBoxButton.OK, MessageBoxImage.Error);
        }

#if DEBUG
        public new bool? ShowDialog()
        {
            DebugModeSettings();
            return base.ShowDialog();
        }

        public new void Show()
        {
            DebugModeSettings();
            base.Show();
        }

        private void DebugModeSettings()
        {
            UserNameTextBox.Text = AccessControl.Users.NativeUser.Admin.ToString();
            PasswordTextBox.Password = "admin";
            btnLogin.Focus();
        }
#endif

        private async Task<bool> CanUserLoginAsync()
        {
            var authentication = new UserAccountControlProvider();
            bool result = await authentication.AuthenticateAsync(UserNameTextBox.Text, PasswordTextBox.Password);
            return result;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UserNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void MainBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }
    }
}
