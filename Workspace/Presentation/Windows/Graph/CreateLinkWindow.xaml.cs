using GPAS.Workspace.Entities;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    /// <summary>
    /// Interaction logic for CreateLinkWindow.xaml
    /// </summary>
    public partial class CreateLinkWindow
    {
        private readonly LinkViewModel linkViewModel;

        public CreateLinkWindow(object sourceObject, object targetObject)
        {
            InitializeComponent();
            linkViewModel = (LinkViewModel)DataContext;
            linkViewModel.PrepareCreateLink(sourceObject, targetObject);
        }

        /// <summary>
        /// رخداد «ایجاد رابطه جدید» (لینک)
        /// </summary>
        public event EventHandler<object> NewLinkCreated;

        /// <summary>
        /// عملگر صدور رخداد ایجاد رابطه (لینک) جدید
        /// </summary>
        protected virtual void OnNewLinkCreated(KWLink createdLink)
        {
            if (createdLink == null)
                throw new ArgumentNullException(nameof(createdLink));

            NewLinkCreated?.Invoke(this, createdLink);
        }

        private async void CreateLink()
        {
            try
            {
                MainWaitingControl.TaskIncrement();
                var createLink = await linkViewModel.CreateNewLink();

                if (createLink != null)
                {
                    OnNewLinkCreated(createLink);
                }

                Close();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        private void MainBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            // Begin dragging the window
            DragMove();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CreateLink();
        }

        private void CreateLinkUserControl_OnLinkValidation(object sender, bool e)
        {
            OkButton.IsEnabled = e;
        }
    }
}
