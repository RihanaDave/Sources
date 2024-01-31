using GPAS.Logger;
using GPAS.Workspace.Logic.LogReader;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using GPAS.Workspace.Presentation.Windows;
using GPAS.Workspace.ViewModel.Publish.PendingChanges;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace GPAS.Workspace.Presentation
{
    /// <summary>
    /// منطق تعامل برای
    /// App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region مدیریت رخداد
        public static event EventHandler<EventArgs> WorkspaceInitializationCompleted;
        protected static void OnWorkspaceInitializationCompleted()
        {
            WorkspaceInitializationCompleted?.Invoke(null, EventArgs.Empty);
        }
        #endregion

        private static bool IsInitializationCompleted { get; set; } = false;

        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public static MainWindow MainWindow { get; protected set; } = null;
        static LoginWindow LoginWindow = null;

        public static LogReader LogReader { get; } = new LogReader();
        string LogServerAddress = ConfigurationManager.AppSettings["LogService_ServerAddress"];
        int LogPortNumber = int.Parse(ConfigurationManager.AppSettings["LogService_PortNumber"]);

        [STAThread]
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // تنظیم امکان مدیریت رخدادهای مدیریت نشده نرم افزار
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            if (!PreventMultipleInstanceRuning())
            {
                return;
            }

            SetMaterialDesignColor((ThemeApplication)int.Parse(ConfigurationManager.AppSettings["Theme"]));

            ApplyApplicationInputArgumentsToWorkspace(e.Args);
            // تعیین نوع خروج از نرم افزار
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Logic.System.MinimalInitialization();
            LogReader.Start(IPAddress.Parse(LogServerAddress), LogPortNumber);
            ShowLogin();
        }

        private static void LoginWindow_Closing(object sender, EventArgs e)
        {
            if (LoginWindow.Result)
            {
                ShowMainWindow();
            }
            else
            {
                ExitWorkspace();
            }
        }

        private static void MainWindow_Closed(object sender, EventArgs e)
        {
            if (MainWindow.LogOut)
            {
                LogOut();
                ShowLogin();
            }
            else
                ExitWorkspace();
        }

        public static void LogOut()
        {
            IsInitializationCompleted = false;
            UserAccountControlProvider.Reset();
            UnpublishedChangesManager.ClearCache();
        }

        public static void ShowLogin()
        {
            LoginWindow = new LoginWindow();
            LoginWindow.Closing -= LoginWindow_Closing;
            LoginWindow.Closing += LoginWindow_Closing;
            LoginWindow.Show();
        }

        private static async void ShowMainWindow()
        {
            MainWindow = new MainWindow();
            MainWindow.Closed -= MainWindow_Closed;
            MainWindow.Closed += MainWindow_Closed;

            await MainWindow.Initialization();
            MainWindow.Show();

            // صدور رخداد تکمیل آماده سازی محیط کاربری
            OnWorkspaceInitializationCompleted();
            // مقداردهی «صحیح» برای متغیر نشاندهنده تکمیل آماده سازی اولیه
            IsInitializationCompleted = true;
        }

        private static string appGuid = string.Format("{0}969476a0 - 08ce - 4d38 - a3c1 - d125b41f60ce", AssemblyTitle);
        private static Mutex mutex;
        /// <summary></summary>
        /// <returns>If no other instance running returns 'True' otherwise returns 'False'</returns>
        private static bool PreventMultipleInstanceRuning()
        {
            string proccessName = string.Format("Global\\{0}", appGuid);
            mutex = new Mutex(false, proccessName);
            if (!mutex.WaitOne(0, false))
            {
                KWMessageBox.Show("An instance of application is already running"
                    , MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return false;
            }
            return true;
        }

        private void SetMaterialDesignColor(ThemeApplication newTheme)
        {
            ITheme theme = paletteHelper.GetTheme();

            theme.PrimaryLight = new ColorPair((Color)ColorConverter.ConvertFromString("#59D5DF"),
                (Color)ColorConverter.ConvertFromString("#000000"));
            theme.PrimaryMid = new ColorPair((Color)ColorConverter.ConvertFromString("#00A3AD"),
                (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            theme.PrimaryDark = new ColorPair((Color)ColorConverter.ConvertFromString("#00747E"),
                (Color)ColorConverter.ConvertFromString("#FFFFFF"));

            theme.SecondaryMid = new ColorPair((Color)ColorConverter.ConvertFromString("#59D5DF"),
                (Color)ColorConverter.ConvertFromString("#000000"));

            switch (newTheme)
            {
                case ThemeApplication.Dark:
                    SetDarkTheme(theme);
                    break;
                case ThemeApplication.Light:
                    SetLightTheme(theme);
                    break;
            }
        }

        private void SetLightTheme(ITheme theme)
        {
            IBaseTheme baseTheme = new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);

            theme.Paper = (Color)ColorConverter.ConvertFromString("#EFEFEF");
            theme.CardBackground = (Color)ColorConverter.ConvertFromString("#DFDFDF");

            paletteHelper.SetTheme(theme);
        }

        private void SetDarkTheme(ITheme theme)
        {
            IBaseTheme baseTheme = new MaterialDesignDarkTheme();
            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);
        }

        public void SetLightTheme()
        {
            SetLightTheme(paletteHelper.GetTheme());
            SaveThemeInApplicationSettings((int)ThemeApplication.Light);
        }

        public void SetDarkTheme()
        {
            SetDarkTheme(paletteHelper.GetTheme());
            SaveThemeInApplicationSettings((int)ThemeApplication.Dark);
        }

        private void SaveThemeInApplicationSettings(int value)
        {
            var xmlDoc = new XmlDocument();

            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            if (xmlDoc.DocumentElement != null)
                foreach (XmlElement element in xmlDoc.DocumentElement)
                {
                    if (!element.Name.Equals("appSettings"))
                        continue;

                    foreach (XmlNode node in element.ChildNodes)
                        if (node.Attributes != null && node.Attributes[0].Value.Equals("Theme"))
                            node.Attributes[1].Value = value.ToString();
                }

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void LoginWindow_CancelButtonClicked(object sender, EventArgs e)
        {
            ExitWorkspace();
        }

        private void ApplyApplicationInputArgumentsToWorkspace(string[] args)
        {
            ApplyLanguageArgumentToWorkspace(args);
        }

        private void ApplyLanguageArgumentToWorkspace(string[] args)
        {
            CultureInfo workspaceCulture = null;
            bool isInputArgumentsContainLanguageArgument = false;
            string inputLanguage = "";

            foreach (string arg in args)
            {
                if (arg
                    .TrimStart(new char[] { '-' })
                    .StartsWith("lang", true, CultureInfo.InvariantCulture)
                || arg
                    .TrimStart(new char[] { '-' })
                    .StartsWith("language", true, CultureInfo.InvariantCulture))
                {
                    inputLanguage = arg.Substring(arg.IndexOf('=') + 1);
                    try
                    {
                        // Validate Input language
                        workspaceCulture = new CultureInfo(inputLanguage);
                        isInputArgumentsContainLanguageArgument = true;
                    }
                    catch
                    {
                    }
                    break;
                }
            }

            if (isInputArgumentsContainLanguageArgument)
            {
                // ذخیره فرهنگ داده شده در ورودی، برای استفاده‌ی بعدی محیط کاربری
                Presentation.Properties.Settings.Default.WorkspaceLastRunCulture = inputLanguage;
                Presentation.Properties.Settings.Default.Save();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Presentation.Properties.Settings.Default.WorkspaceLastRunCulture))
                {
                    // ذخیره فرهنگ پیش‌فرض، برای استفاده‌ی بعدی محیط کاربری
                    Presentation.Properties.Settings.Default.WorkspaceLastRunCulture = Presentation.Properties.Settings.Default.DefaultWorkspaceLastRunCulture;
                    Presentation.Properties.Settings.Default.Save();
                }
                workspaceCulture = new CultureInfo(Presentation.Properties.Settings.Default.WorkspaceLastRunCulture);
            }

            if (workspaceCulture != null)
            {
                Thread.CurrentThread.CurrentCulture = workspaceCulture;
                Thread.CurrentThread.CurrentUICulture = workspaceCulture;
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.GetType() == typeof(NotSupportedException))
            {
                KWMessageBox.Show("This feature is not supported.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                Logic.System.WriteExceptionLog(e.Exception);
                KWMessageBox.Show(string.Format("Unhandled exception occured;\r\r{0}\r\rPlease contact your admin.",
                    e.Exception.Message),
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
            }

            if (IsInitializationCompleted)
            {
                e.Handled = true;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (IsInitializationCompleted)
            {
                try
                {
                    // آماده سازی پایان کار منطق محیط کاربری
                    Logic.System.Finalization();
                    LogReader.Stop();
                }
                catch (Exception ex)
                {
                    KWMessageBox.Show(string.Format("{0}:\r\r{1}", "Finalization Error", ex.Message),
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
            }
        }

        internal static void ExitWorkspace()
        {
            try
            {
                Current.Shutdown();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format("{0}:\r\r{1}", "Unable to exit workspace", ex.Message),
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        delegate void NoInputNoOutputDelegate(string waitDescription, bool lockMainWindow);

        #region Assembly Attribute Accessors
        // Source: Windows form project Aboutbox example behind code! (with some changes)

        public static string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != string.Empty)
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        internal static void PropmotUserToPublishChanges(List<PublishingChangedObjectVm> publishingChangedConcepts)
        {
            var publishWindow = new PublishWindow();
            publishWindow.ShowDialog();
        }

        #endregion
    }
}
