using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows
{
    public partial class PublishWindow 
    {
        #region Properties               
       
        public bool Success;

        #endregion        

        #region Methods

        public PublishWindow()
        {
            InitializeComponent();
        }

        public void ShowUnpublishedObjects(Tuple<List<ViewModel.Publish.UnpublishedObject>,
            List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>> seperatedUnpublishedObjects)
        {
            if (seperatedUnpublishedObjects == null)
                throw new ArgumentNullException(nameof(seperatedUnpublishedObjects));

            PublishUserControl.ShowUnpublishedObjects(seperatedUnpublishedObjects);
        }

        private async void Publish()
        {
            try
            {
                MainWaitingControl.Message = Properties.Resources.Publish_inprogress;
                MainWaitingControl.TaskIncrement();

                Success = await PublishUserControl.Publish();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);                
                KWMessageBox.Show(string.Format("{0}\n\n{1}", Properties.Resources.Invalid_Server_Response, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
                Close();
            }
        }

        private void PublishUserControl_BeginOfAccessLevelChecking(object sender, EventArgs e)
        {
            MainWaitingControl.Message = Properties.Resources.Please_Wait;
            MainWaitingControl.TaskIncrement();
        }

        private void PublishUserControl_EndOfAccessLevelChecking(object sender, EventArgs e)
        {
            MainWaitingControl.TaskDecrement();
        }

        private void PublishWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MainWaitingControl.TasksCount > 0)
            {
                e.Cancel = true;
            }
        }

        private void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            Publish();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion        
    }
}
