using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Controls.Sesrch.Enum;
using GPAS.Workspace.Presentation.Controls.Sesrch.Model;
using GPAS.Workspace.Presentation.Controls.Sesrch.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.View
{
    /// <summary>
    /// Interaction logic for SearchUserControl.xaml
    /// </summary>
    public partial class SearchUserControl : UserControl
    {
        public SearchViewModel ViewModel { get; set; } = new SearchViewModel();
        #region SearchEvent
        private void AllSearch_Click(object sender, EventArgs e)
        {
            ViewModel.ShowSearchORProgressButton = true;
            Search();
        }

        public event EventHandler<KWObject> BrowseObjectOnBrowser;

        protected void OnBrowseObjectOnBrowser(KWObject kwObjrct)
        {
            BrowseObjectOnBrowser?.Invoke(this, kwObjrct);
        }

        #endregion

        public SearchUserControl()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        private void Search()
        {
            try
            {
                ViewModel.ModelSearch.PageNumber = 0;
                ViewModel.GetFromTo(ViewModel.ModelSearch.PageNumber, ViewModel.ModelSearch.ItemPerPage);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        private async void ShowObjectOnBrowser(long objectId)
        {
            try
            {
                MainWaitingControl.TaskIncrement();
                KWObject result = await ViewModel.GetObjectById(objectId);

                if (result != null)
                    OnBrowseObjectOnBrowser(result);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        public void ClickedAseOrDecButtonInSearchResult()
        {
            if (ViewModel.ModelSearch.SortOrder == SortOrder.SortAscending)
            {
                ViewModel.ModelSearch.SortOrder = SortOrder.SortDescending;
            }
            else
            {
                ViewModel.ModelSearch.SortOrder = SortOrder.SortAscending;
            }
        }

        private void SearchResultUserControl_SortByAscOrDescEvent_1(object sender, EventArgs e)
        {
            ClickedAseOrDecButtonInSearchResult();
        }

        private void SearchResultUserControl_ItemPerPageChanged(object sender, Gpas.Pagination.Events.PaginationEventHandler e)
        {
            ViewModel.GetFromTo(e.FromItem, e.ToItem);
        }

        private void SearchResultUserControl_PageNumberChanged(object sender, Gpas.Pagination.Events.PaginationEventHandler e)
        {
            ViewModel.GetFromTo(e.FromItem, e.ToItem);
        }

        private void SearchResultUserControl_ShowObjectOnBrowser(object sender, long e)
        {
            ShowObjectOnBrowser(e);
        }
    }
}
