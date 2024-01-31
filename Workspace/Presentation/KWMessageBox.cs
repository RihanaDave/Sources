using GPAS.Workspace.Presentation.Windows.MessageBoxes;
using System.Windows;

namespace GPAS.Workspace.Presentation
{
    public sealed class KWMessageBox
    {
        public static MessageBoxResult Show(string message)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(message);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(string message, MessageBoxButton button)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(message, button);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(string message, MessageBoxButton button, MessageBoxImage icon)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(message, button, icon);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(string message, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(message, button, icon, defaultResult);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(Window owner, string message)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(owner, message);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, MessageBoxButton button)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(owner, message, button);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, MessageBoxButton button, MessageBoxImage icon)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(owner, message, button, icon);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            KWMessageBoxWindow customMessageBoxWindow = new KWMessageBoxWindow(owner, message, button, icon, defaultResult);
            customMessageBoxWindow.ShowDialog();

            return customMessageBoxWindow.Result;
        }
    }
}
