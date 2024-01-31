using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.Graph;
using GPAS.Workspace.Presentation.Controls.CustomSearchAround;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Notifications.Wpf;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows.CustomSearchAround
{
    /// <summary>
    /// Interaction logic for CustomSearchAroundWindow.xaml
    /// </summary>
    public partial class CustomSearchAroundWindow : Window
    {
        CustomSearchAroundViewModel ViewModel = new CustomSearchAroundViewModel();
        private readonly NotificationManager NotificationManager = new NotificationManager();

        public CustomSearchAroundWindow(IEnumerable<KWObject> objects)
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.SourceObjectCollection = new ObservableCollection<KWObject>(objects);
        }

        public event EventHandler<CustomSearchAroundSearchRequestEventArgs> SearchRequest;

        protected void OnSearchRequest(CustomSearchAroundSearchRequestEventArgs e)
        {
            SearchRequest?.Invoke(this, e);
        }

        private void CSAControl_SearchRequest(object sender, CustomSearchAroundSearchRequestEventArgs e)
        {
            if (SearchHasResult(e.SearchAroundResult))
                Close();
            else
            {
                NotificationContent notificationContent = new NotificationContent()
                {
                    Message = "No results found!",
                    Title = "Custom search around",
                    Type = NotificationType.Information,
                };

                NotificationManager.Show(notificationContent, "WindowArea");
            }

            OnSearchRequest(e);
        }

        private bool SearchHasResult(KWCustomSearchAroundResult searchAroundResult)
        {
            if (searchAroundResult?.RalationshipBasedResult == null && searchAroundResult?.EventBasedResult == null)
                return false;
            else if (searchAroundResult.RalationshipBasedResult == null)
                return searchAroundResult.EventBasedResult.Count > 0;
            else if (searchAroundResult.EventBasedResult == null)
                return searchAroundResult.RalationshipBasedResult.Count > 0;
            else
                return (searchAroundResult.RalationshipBasedResult.Count + searchAroundResult.EventBasedResult.Count) > 0;
        }
    }
}
