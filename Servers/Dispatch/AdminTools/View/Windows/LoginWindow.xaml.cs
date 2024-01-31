using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GPAS.Dispatch.AdminTools.ViewModel;
using GPAS.Dispatch.Logic;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        private readonly LoginViewModel loginViewModel;
        public bool SuccessfulLogin;

        public LoginWindow()
        {
            InitializeComponent();
            loginViewModel = new LoginViewModel();
            DataContext = loginViewModel;
            Init();
        }

        private void Init()
        {
            VersionTextBlock.Text = "version : " + Assembly.GetExecutingAssembly().GetName().Version;

            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0)
                return;

            CopyrightTextBlock.Text = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }

        private async void TryLogin()
        {
            if (!LoginValidation())
                return;

            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                MainBorder.IsEnabled = false;
                bool loginResult = await loginViewModel.UserLogin();

                if (loginResult)
                {
                    SuccessfulLogin = true;

                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    SuccessfulLogin = false;
                    ErrorTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                MainBorder.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private bool LoginValidation()
        {

            UserNameTexBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            PasswordPasswordBox.GetBindingExpression(PasswordBoxAssistant.BoundPassword)?.UpdateSource();

            return IsValid(MainBorder);
        }

        public bool IsValid(DependencyObject control)
        {
            if (Validation.GetHasError(control))
                return false;

            for (var i = 0; i != VisualTreeHelper.GetChildrenCount(control); ++i)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                if (!IsValid(child)) { return false; }
            }

            return true;
        }

        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void PasswordPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPasswordBox.SelectAll();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UserNameTexBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void PasswordPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) TryLogin();
        }
    }
}
