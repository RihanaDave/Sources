using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Controls.Sesrch.View;
using GPAS.Workspace.Presentation.Controls.Sesrch.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// منطق تعامل با HomeApplication.xaml
    /// </summary>
    public partial class HomeApplication
    {
        #region مدیریت رخدادها       

        /// <summary>
        /// کلاس آرگومان(های) فراخوانی رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        public class BrowseRequestedEventArgs
        {
            /// <summary>
            /// سازنده کلاس
            /// </summary>
            /// <param name="objectsRequestedForBrowse"></param>
            public BrowseRequestedEventArgs(IEnumerable<KWObject> objectsRequestedForBrowse)
            {
                ObjectsToBrowse = objectsRequestedForBrowse ?? throw new ArgumentNullException(nameof(objectsRequestedForBrowse));
            }

            /// <summary>
            /// اشیائی که درخواست نمایش (براوز) برای آن ها داده شده است
            /// </summary>
            public IEnumerable<KWObject> ObjectsToBrowse
            {
                get;
            }
        }

        /// <summary>
        /// رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        public event EventHandler<BrowseRequestedEventArgs> BrowseRequested;

        /// <summary>
        /// عملکرد مدیریت صدور رخداد رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        /// <param name="objectsRequestedForBrowse"></param>
        private void OnBrowseRequested(IEnumerable<KWObject> objectsRequestedForBrowse)
        {
            if (objectsRequestedForBrowse == null)
                throw new ArgumentNullException(nameof(objectsRequestedForBrowse));

            BrowseRequested?.Invoke(this, new BrowseRequestedEventArgs(objectsRequestedForBrowse));
        }
        #endregion

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public HomeApplication()
        {
            InitializeComponent();
            AddNewTab();
        }

        private void ShowQuickSearchResultsInResultViewer(List<KWObject> searchResults)
        {
            try
            {
                //  homeAppQuickSearchResultViewer.ShowResults(searchResults);
            }
            catch (Exception ex)
            { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void homeAppQuickSearchResultViewer_QuickSearchResultChoosen(object sender, Controls.QuickSearchResultViewerControl.QuickSearchResultChoosenEventArgs e)
        {
            List<KWObject> listToPass = new List<KWObject>();
            listToPass.Add(e.ChoosenResult);
            RequestBrowseObjects(listToPass);
        }

        private void RequestBrowseObjects(List<KWObject> listToPass)
        {
            try
            {
                OnBrowseRequested(listToPass);
            }
            catch (Exception ex)
            { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void homeAppQuickSearchControl_QuickSearchResultChoosen(object sender, Controls.QuickSearchControl.QuickSearchResultChoosenEventArgs e)
        {
            List<KWObject> listToPass = new List<KWObject> { e.ChoosenResult };
            RequestBrowseObjects(listToPass);
            //  homeAppQuickSearchControl.CloseResultPopup();
        }

        #region Variable

        int CountTabControl = 0;
        #endregion

        #region Function       

        private void CloseOtherTabs_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void MouseDown_OnHandler(object sender, MouseButtonEventArgs e)
        {

        }

        private void RemoveItemInTabControl(SearchViewModel dataContext)
        {
            if (dataContext == null)
                return;

            var Tabs = ObjectsTabControl.Items.OfType<TabItem>().ToList();
            TabItem closedTab = Tabs.FirstOrDefault(t => t.DataContext.Equals(dataContext));

            if (closedTab == null)
                return;

            ObjectsTabControl.Items.Remove(closedTab);
            CountTabControl--;

            if (ObjectsTabControl.SelectedIndex == CountTabControl)
            {
                ObjectsTabControl.SelectedIndex--;
            }

            if (CountTabControl == 0)
            {
                AddNewTab();
            }
        }

        private void CloseTabButton_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveItemInTabControl((SearchViewModel)((FrameworkElement)sender)?.DataContext);
        }


        public void AddNewItemTab()
        {
            AddNewTab();
        }

        private void AddNewTab()
        {
            SearchUserControl searchUserControl = new SearchUserControl();
            searchUserControl.BrowseObjectOnBrowser += SearchUserControl_BrowseObjectOnBrowser;
            Style style = FindResource("ItemTab") as Style;
            TabItem newTabItem = new TabItem()
            {
                Style = style,
                TabIndex = CountTabControl,
                DataContext = searchUserControl.DataContext,
                Content = searchUserControl,
            };

            ObjectsTabControl.Items.Insert(CountTabControl, newTabItem);
            ObjectsTabControl.SelectedItem = ObjectsTabControl.Items[CountTabControl];
            CountTabControl++;
        }

        private void SearchUserControl_BrowseObjectOnBrowser(object sender, KWObject e)
        {
            RequestBrowseObjects(new List<KWObject> { e });
        }

        private void AddNewTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewItemTab();
        }
        public void ClearSearchTextBoxBasicSearchControl()
        {

        }

        #endregion

        private void CloseTabButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                RemoveItemInTabControl((SearchViewModel)((FrameworkElement)sender)?.DataContext);
            }
        }

        private void CloseAll_OnClick(object sender, RoutedEventArgs e)
        {
            CloseAllTabs();
        }

        private void CloseAllTabs()
        {
            int Temp = CountTabControl;
            for (int i = 0; i < Temp; i++)
            {
                RemoveItemInTabControl((SearchViewModel)((FrameworkElement)ObjectsTabControl.Items[0])?.DataContext);
            }
        }

        public override void Reset()
        {
            CloseAllTabs();
        }
    }
}
