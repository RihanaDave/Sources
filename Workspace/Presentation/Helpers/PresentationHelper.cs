using GPAS.Logger;
using Notifications.Wpf;
using Notifications.Wpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Helpers
{
    /// <summary>
    /// این کنترل انتزاعی به عنوان پدر تمام ابزارهای کمکی لایه ارائه در نظر گرفته شده
    /// </summary>
    public abstract class PresentationHelper : UserControl
    {
        /// <summary>
        /// سازنده
        /// </summary>
        public PresentationHelper()
        {

        }

        protected void HandleException(Exception ex)
        {
            if (ex == null)
                return;

            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
            KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK);
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PresentationHelper), new PropertyMetadata(Properties.Resources.Untitled));


        private NotificationManager NotificationManager { get; } = new NotificationManager();

        protected void ShowNotification(string message, string title)
        {
            ShowNotification(message, title, NotificationType.Information, null, null, null);
        }

        protected void ShowNotification(string message, string title, NotificationType notificationType)
        {
            ShowNotification(message, title, notificationType, null, null, null);
        }

        protected void ShowNotification(string message, string title, NotificationType notificationType, TimeSpan? expirationTime, Action onClick, Action onClose)
        {
            NotificationContent notificationContent = new NotificationContent
            {
                Title = title,
                Message = message,
                Type = notificationType
            };

            NotificationManager.Show(notificationContent, string.Empty, expirationTime, onClick, onClose);
        }

        public virtual void Reset()
        {

        }
    }
}
