using GPAS.Dispatch.Logic;
using GPAS.Logger;
using System;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace GPAS.Dispatch.AdminTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private bool isStartupCompleted = false;

        [STAThread]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // تنظیم امکان مدیریت رخدادهای مدیریت نشده نرم افزار
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            //بارگذاری تنظیمات برنامه از سرور Dispatch
            LoadConfigFileFromDispatchService();

            ExceptionHandler.Init();

            OntologyLoader.OntologyLoader.Init
            (
                new DispatchOntologyDownLoader(),
                ConfigurationManager.AppSettings["OntologyLoaderFolderPath"],
                ConfigurationManager.AppSettings["OntologyIconsLoaderFolderPath"]
            );

            isStartupCompleted = true;
        }

        private void LoadConfigFileFromDispatchService()
        {
            string currentConfigPath = $".\\{AppDomain.CurrentDomain.FriendlyName}.config";

#if DEBUG
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string dispatchConfigPath = currentDirectory.Remove(currentDirectory.IndexOf("\\AdminTools\\bin\\", StringComparison.Ordinal));

            dispatchConfigPath = System.IO.Path.Combine(dispatchConfigPath, "IISHost\\bin\\GPAS.Dispatch.IISHost.dll");

            if (!System.IO.File.Exists(dispatchConfigPath))
                throw new System.IO.FileNotFoundException(nameof(dispatchConfigPath));

            Configuration serviceConfig = ConfigurationManager.OpenExeConfiguration(dispatchConfigPath);

#else
            Configuration serviceConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("\\", "Dispatch Service");
#endif

            serviceConfig.SaveAs(currentConfigPath, ConfigurationSaveMode.Modified);
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            foreach (ConfigurationSectionGroup sectionGroup in appConfig.SectionGroups)
            {
                RefreshSections(sectionGroup.Sections);
            }

            RefreshSections(appConfig.Sections);
        }

        private void RefreshSections(ConfigurationSectionCollection sections)
        {
            foreach (ConfigurationSection section in sections)
            {
                ConfigurationManager.RefreshSection(section.SectionInformation.Name);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteExceptionLog(e.Exception);
            MessageBox.Show($"{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}For more details see error logs.", "Admin Tools",
                MessageBoxButton.OK,
                MessageBoxImage.Stop);

            if (isStartupCompleted)
            {
                e.Handled = true;
            }
        }

        internal static void WriteExceptionLog(Exception ex)
        {
            var exLogger = new ExceptionHandler();
            exLogger.WriteErrorLog(ex);
        }

        internal static void ExitWorkspace()
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}:\r\r{1}", "Unable to exit workspace", ex.Message), "Exit", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        internal static void ShowWaitPrompt(string waitDescription = "", bool lockMainWindow = false)
        {
            try
            {
                //generalWaitingWindow.Show(waitDescription);
                //if (lockMainWindow && mainWindow != null)
                //    mainWindow.LockWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}:\r\r{1}", "Unable to show waiting prompt", ex.Message), "Waiting", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        internal static void HideWaitPrompt()
        {
            try
            {
                // generalWaitingWindow.Hide();
                //if (mainWindow != null)
                //    mainWindow.UnlockWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}:\r\r{1}", "Unable to hide waiting prompt", ex.Message), "Waiting", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

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
        #endregion
    }
}
