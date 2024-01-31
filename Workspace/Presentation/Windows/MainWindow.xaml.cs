using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using GPAS.Workspace.Presentation.ApplicationContainers;
using GPAS.Workspace.Presentation.Applications;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Observers.ObjectsRemoving;
using GPAS.Workspace.Presentation.Observers.Properties;
using GPAS.Workspace.Presentation.Windows.Investigation;
using GPAS.Workspace.Presentation.Windows.Investigation.EventArguments;
using GPAS.Workspace.Presentation.Windows.Investigation.ViewModels;
using MaterialDesignThemes.Wpf;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Windows
{
    /// <summary>
    /// منطق تعامل برای MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public InvestigationStatus investigationStatus;


        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(MainWindow),
                new PropertyMetadata(ThemeApplication.Dark, OnSetCurrentThemeChanged));

        private static void OnSetCurrentThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow)d).OnSetCurrentThemeChanged(e);
        }

        private void OnSetCurrentThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            OnCurrentThemeChanged(e);
        }

        public event DependencyPropertyChangedEventHandler CurrentThemeChanged;
        protected void OnCurrentThemeChanged(DependencyPropertyChangedEventArgs e)
        {
            CurrentThemeChanged?.Invoke(this, e);
        }

        private PresentationApplications currentApplication;
        public PresentationApplications CurrentApplication
        {
            get => currentApplication;
            set
            {
                if (currentApplication == value)
                    return;

                currentApplication = value;
                DialogHost.CloseDialogCommand.Execute(false, null);
                NotifyPropertyChanged("CurrentApplication");
            }
        }

        private bool showImageAnalysisApp;
        public bool ShowImageAnalysisApp
        {
            get => showImageAnalysisApp;
            set
            {
                if (showImageAnalysisApp != value)
                {
                    showImageAnalysisApp = value;
                    NotifyPropertyChanged("ShowImageAnalysisApp");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool LogOut { get; protected set; } = false;

        private readonly DataImportViewModel DataImportViewModel;

        /// <summary>
        /// سازنده پنجره اصلی
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            CurrentApplication = PresentationApplications.Graph;
            CurrentTheme = (ThemeApplication)int.Parse(ConfigurationManager.AppSettings["Theme"]);
            DataContext = this;

            DataImportViewModel = new DataImportViewModel();
            DataImportApplication.DataContext = DataImportViewModel;
        }

        internal async Task Initialization()
        {
            ShowImageAnalysisApp = await CanShowImageAnalysisApp();
            InitiateObservers();
        }

        private async Task<bool> CanShowImageAnalysisApp()
        {
            bool imageAnalysisServiceVisibilityStatus = await Logic.System.GetImageAnalysisServiceVisibilityStatus();
            return imageAnalysisServiceVisibilityStatus;
        }

        Help.ShortcutKeys ShortcutKeysHelpWindow;

        private ObjectsShowingObserver browseObjectsObserver = new ObjectsShowingObserver();
        private ObjectsShowingObserver showObjectsOnGraphObserver = new ObjectsShowingObserver();
        private ObjectsShowingObserver showObjectsOnMapObserver = new ObjectsShowingObserver();
        private ObjectsShowingObserver showObjectsOnImageAnalysisObserver = new ObjectsShowingObserver();
        private LinksShowingObserver showLinksOnGraphObserver = new LinksShowingObserver();

        private ObjectsRemovingObserver ObjectsRemovingObserver = new ObjectsRemovingObserver();
        private PropertiesChangeableObserver PropertiesChangingObserver = new PropertiesChangeableObserver();

        /// <summary>
        /// آماده‌سازی ناظرهای پنجره اصلی که وظیفه ارتباط بین کاربردها را برعهده دارند
        /// </summary>
        private void InitiateObservers()
        {
            //تنظیم ناظر نمایش یک شی در براوزر
            browseObjectsObserver.AddListener(BrowserApplicationContainer.MasterApplication as BrowserApplication);
            (HomeApplicationContainer.MasterApplication as HomeApplication).BrowseRequested += (sender, e) =>
            {
                browseObjectsObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectsToBrowse });
                CurrentApplication = PresentationApplications.Browser;
            };
            (GraphApplicationContainer.MasterApplication as GraphApplication).BrowseRequested += (sender, e) =>
            {
                browseObjectsObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectsToBrowse });
                CurrentApplication = PresentationApplications.Browser;
            };
            (MapApplicationContainer.MasterApplication as MapApplication).MarkerBrowseRequested += (sender, e) =>
            {
                browseObjectsObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectsToBrowse });
                CurrentApplication = PresentationApplications.Browser;
            };
            //تنظیم ناظر نمایش شی در گراف
            showObjectsOnGraphObserver.AddListener(GraphApplicationContainer.MasterApplication as GraphApplication);

            //تنظیم ناظر نمایش شی روی نقشه
            showObjectsOnMapObserver.AddListener(MapApplicationContainer.MasterApplication as MapApplication);
            (BrowserApplicationContainer.MasterApplication as BrowserApplication).ShowOnMapRequested += (sender, e) =>
            {
                showObjectsOnMapObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = new[] { e.ObjectRequestedToShowOnMap } });
                CurrentApplication = PresentationApplications.Map;
            };

            ObjectsRemovingObserver.AddListener(MapApplicationContainer.MasterApplication as MapApplication);
            ObjectsRemovingObserver.AddListener(BrowserApplicationContainer.MasterApplication as BrowserApplication);
            ObjectsRemovingObserver.AddListener(GraphApplicationContainer.MasterApplication as GraphApplication);
            PropertiesChangingObserver.AddListener(MapApplicationContainer.MasterApplication as MapApplication);
            PropertiesChangingObserver.AddListener(GraphApplicationContainer.MasterApplication as GraphApplication);


            ((BrowserApplication)BrowserApplicationContainer.MasterApplication).PropertiesChanged +=
                (sender, e) =>
                {
                    IEnumerable<KWProperty> added = e.NewItems?.OfType<KWProperty>();
                    IEnumerable<KWProperty> removed = e.OldItems?.OfType<KWProperty>();
                    PropertiesChangingObserver.ReportAction(new PropertiesChangedArgs(added, removed));
                };

            ((GraphApplication)GraphApplicationContainer.MasterApplication).RemoveObjectFromMapRequest +=
                (sender, e) =>
                {
                    ObjectsRemovingObserver.ReportAction(new ObjectsRemovingArgs
                    { ObjectsToRemove = e.ObjectsRequestedToRemoveFromMap });
                };

            (GraphApplicationContainer.MasterApplication as GraphApplication).ShowOnMapRequested += (sender, e) =>
            {
                showObjectsOnMapObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectsRequestedToShowOnMap });
                CurrentApplication = PresentationApplications.Map;
            };
            (ObjectExplorerApplicationContainer.MasterApplication as ObjectExplorerApplication).ShowOnMapRequested += (sender, e) =>
            {
                showObjectsOnMapObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectRequestedToShowOnMap });
                CurrentApplication = PresentationApplications.Map;
            };

            //بروزرسانی زبانه‌های نمایشگر اشیاء بعد از انتشار
            (GraphApplicationContainer.MasterApplication as GraphApplication).PublishedSuccess += (sender, e) =>
            {
                ((BrowserApplication)BrowserApplicationContainer.MasterApplication).RefreshAllTabs();
            };

            // تنظیم ناظر نمایش لینک در گراف
            showLinksOnGraphObserver.AddListener(GraphApplicationContainer.MasterApplication as GraphApplication);
            (BrowserApplicationContainer.MasterApplication as BrowserApplication).AddRelationshipToGraph += (sender, e) =>
            {
                showLinksOnGraphObserver.ReportAction
                (new LinksShowingArgs { LinksToShow = new KWLink[] { e.RelationshipToShow } });
                CurrentApplication = PresentationApplications.Graph;
            };

            (ObjectExplorerApplicationContainer.MasterApplication as ObjectExplorerApplication).ShowOnGraphRequested += (sender, e) =>
            {
                showObjectsOnGraphObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectRequestedToShowOnGraph });
                CurrentApplication = PresentationApplications.Graph;
            };

            showObjectsOnImageAnalysisObserver.AddListener(ImageAnalysisApplicationContainer.MasterApplication as ImageAnalysisApplicatioin);
            (ImageAnalysisApplicationContainer.MasterApplication as ImageAnalysisApplicatioin).ShowOnGraphRequested += (sender, e) =>
            {
                showObjectsOnGraphObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = new[] { e.ObjectRequestedToShowOnGraph } });
                CurrentApplication = PresentationApplications.Graph;
            };

            (BrowserApplicationContainer.MasterApplication as BrowserApplication).LoadInImageAnalysisRequested += (sender, e) =>
            {
                showObjectsOnImageAnalysisObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = new[] { e.ObjectRequested } });
                CurrentApplication = PresentationApplications.ImageAnalysis;
            };

            DataImportApplication.ShowOnGraphRequested += (sender, e) =>
            {
                showObjectsOnGraphObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = e.ObjectRequestedToShowOnGraph });
                CurrentApplication = PresentationApplications.Graph;
            };
        }

        private void GetInvestigationWindow_LoadInvestigationButtonClicked(object sender, LoadInvestigationEventArgs e)
        {
            LoadSelectedInvestigation(e);
        }

        private async void LoadSelectedInvestigation(LoadInvestigationEventArgs investigationStatuse)
        {
            try
            {
                if (investigationStatuse == null)
                    throw new ArgumentException(nameof(investigationStatuse));

                //App.ShowWaitPrompt(Properties.Resources.Loading_Saved_Investigation_Status);
                //App.MainWindow.LockWindow();

                await InvestigationProvider.AddUnpublishedConceptsToCache(investigationStatuse.InvestigationStatus.Status.SaveInvestigationUnpublishedConcepts);

                await GraphApplicationContainer.SetGraphApplicationStatus(investigationStatuse.InvestigationStatus.Status.GraphStatus);

                await BrowserApplicationContainer.SetBrowserApplicationStatus(investigationStatuse.InvestigationStatus.Status.BrowserStatus);

                await MapApplicationContainer.SetMapApplicationStatus(investigationStatuse.InvestigationStatus.Status.MapStatus);

                //App.MainWindow.UnlockWindow();
                //App.HideWaitPrompt();

                var notificationManager = new NotificationManager();

                notificationManager.Show(new NotificationContent
                {
                    Title = Properties.Resources.Selected_Investigation_Loaded_Successfully,
                    Message = Properties.Resources.String_LoadInvestigation,
                    Type = NotificationType.Information
                });
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show($"{Properties.Resources.Invalid_Server_Response}\n\n{ex.Message}");
            }
            finally
            {
                MainWaitingControl.TaskDecrement();
            }
        }

        private ApplicationContainerBase GetShowingApplication()
        {
            if (HomeApplicationContainer.Visibility == Visibility.Visible)
                return HomeApplicationContainer;
            if (BrowserApplicationContainer.Visibility == Visibility.Visible)
                return BrowserApplicationContainer;
            if (GraphApplicationContainer.Visibility == Visibility.Visible)
                return GraphApplicationContainer;
            if (MapApplicationContainer.Visibility == Visibility.Visible)
                return MapApplicationContainer;
            if (DataSourceApplicationContainer.Visibility == Visibility.Visible)
                return DataSourceApplicationContainer;
            if (ObjectExplorerApplicationContainer.Visibility == Visibility.Visible)
                return ObjectExplorerApplicationContainer;
            if (DataImportApplication.Visibility == Visibility.Visible)
                return ObjectExplorerApplicationContainer;
            throw new InvalidOperationException();
        }

        #region رخدادگردان رخدادهای نوار کاربردها و کاربردها
        private void btnHomeClick(object sender, RoutedEventArgs e)
        {
            CurrentApplication = PresentationApplications.Home;
        }

        private void btnBrowserClick(object sender, RoutedEventArgs e)
        {
            CurrentApplication = PresentationApplications.Browser;
        }
        private void btnGraphClick(object sender, RoutedEventArgs e)
        {
            CurrentApplication = PresentationApplications.Graph;
        }
        private void MapAppButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentApplication = PresentationApplications.Map;
        }
        private void btnBigDataSearchClick(object sender, RoutedEventArgs e)
        {
            CurrentApplication = PresentationApplications.BigDataSearch;
        }
        #endregion

        /// <summary>
        /// انجام مقدامات ترک محیط‌کاربری
        /// </summary>
        /// <returns>در صورت آمادگی محیط کاربری برای خروج مقدار صحیح و در غیر اینصورت مقدار غلط برگردانده می‌شود</returns>
        private async Task<bool> PrepairQuitWorkspace()
        {
            bool readyToQuitWorkspace = false;
            Task<bool> hasAnyChangeInConcepts = Task.Run(() => { return (GraphApplicationContainer.MasterApplication as GraphApplication).HasAnyChangeInConceptsAsync(); });
            Task.WaitAll(new Task[] { hasAnyChangeInConcepts });
            bool hasGraphAnythingToPublish = (GraphApplicationContainer.MasterApplication as GraphApplication).HasGraphAnythingToPublish();

            if (!hasAnyChangeInConcepts.Result && !hasGraphAnythingToPublish)
            {
                readyToQuitWorkspace = true;
                return readyToQuitWorkspace;
            }

            if (hasAnyChangeInConcepts.Result)
            {
                switch (KWMessageBox.Show
                    (
                    Properties.Resources.Some_concepts_have_changed_Do_you_want_to_publish_them,
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes
                    ))
                {
                    case MessageBoxResult.Yes:
                        try
                        {
                            Task<bool> isAnyUnpublishedChanges = Task.Run(() => { return UnpublishedChangesManager.IsAnyUnpublishedChangesAsync(); });
                            Task.WaitAll(new Task[] { isAnyUnpublishedChanges });
                            if (isAnyUnpublishedChanges.Result)
                            {
                                var publishWindow = new PublishWindow();
                                Task<UnpublishedConcepts> unpublishedConcepts = Task.Run(() => { return UnpublishedChangesManager.GetAllUnpublishedChangesAsync(); });
                                Task.WaitAll(new Task[] { unpublishedConcepts });

                                MainWaitingControl.Message = Properties.Resources.Publish_inprogress;
                                MainWaitingControl.TaskIncrement();
                                Task<Tuple<List<ViewModel.Publish.UnpublishedObject>,
                                    List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>
                                >> unpublishedObjects = Task.Run(() =>
                                {
                                    return PendingChangesPublishManager.GetUnpublishedObjects(unpublishedConcepts
                                        .Result);
                                });
                                Task.WaitAll(unpublishedObjects);
                                MainWaitingControl.TaskDecrement();
                                publishWindow.ShowUnpublishedObjects(unpublishedObjects.Result);
                                publishWindow.ShowDialog();
                            }
                            else
                            {
                                KWMessageBox.Show(Properties.Resources.There_Is_No_Items_To_Publish_,
                                    MessageBoxButton.OK, MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                            }
                        }
                        catch (Exception ex)
                        {
                            KWMessageBox.Show(ex.Message, MessageBoxButton.OK,
                                MessageBoxImage.Information, MessageBoxResult.OK);
                        }
                        if ((GraphApplicationContainer.MasterApplication as GraphApplication).HasGraphAnythingToPublish() && !hasAnyChangeInConcepts.Result)
                            switch (KWMessageBox.Show(Properties.Resources.Your_Graph_Has_Content_That_May_Be_Needed_Later_Do_You_Want_To_Save_Your_Graph,
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                                MessageBoxResult.Yes))
                            {
                                case MessageBoxResult.Yes:
                                    await (GraphApplicationContainer.MasterApplication as GraphApplication).PromptUserToPublishGraph();
                                    readyToQuitWorkspace = true;
                                    break;
                                case MessageBoxResult.No:
                                    readyToQuitWorkspace = true;
                                    break;
                                case MessageBoxResult.Cancel:
                                    readyToQuitWorkspace = false;
                                    break;
                            }
                        break;
                    case MessageBoxResult.No:
                        if ((GraphApplicationContainer.MasterApplication as GraphApplication).HasGraphAnythingToPublish() && !hasAnyChangeInConcepts.Result)
                            switch (KWMessageBox.Show(Properties.Resources.Your_Graph_Has_Content_That_May_Be_Needed_Later_Do_You_Want_To_Save_Your_Graph,
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                                MessageBoxResult.Yes))
                            {
                                case MessageBoxResult.Yes:
                                    await (GraphApplicationContainer.MasterApplication as GraphApplication).PromptUserToPublishGraph();
                                    readyToQuitWorkspace = true;
                                    break;
                                case MessageBoxResult.No:
                                    readyToQuitWorkspace = true;
                                    break;
                                case MessageBoxResult.Cancel:
                                    readyToQuitWorkspace = false;
                                    break;
                            }
                        readyToQuitWorkspace = true;
                        break;
                    case MessageBoxResult.Cancel:
                        readyToQuitWorkspace = false;
                        break;
                }
            }
            else
            {
                if ((GraphApplicationContainer.MasterApplication as GraphApplication).HasGraphAnythingToPublish() &&
                    !hasAnyChangeInConcepts.Result)
                    switch (KWMessageBox.Show(Properties.Resources.Your_Graph_Has_Content_That_May_Be_Needed_Later_Do_You_Want_To_Save_Your_Graph,
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
                        MessageBoxResult.Yes))
                    {
                        case MessageBoxResult.Yes:
                            await (GraphApplicationContainer.MasterApplication as GraphApplication).PromptUserToPublishGraph();
                            break;
                        case MessageBoxResult.No:
                            readyToQuitWorkspace = true;
                            break;
                        case MessageBoxResult.Cancel:
                            readyToQuitWorkspace = false;
                            break;
                    }
            }

            return readyToQuitWorkspace;
        }

        private void HideRightClickMenu()
        {
            (GraphApplicationContainer.MasterApplication as GraphApplication).HideRightClickMenuItem();
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        private async void PublishButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideRightClickMenu();
            try
            {

                if (await UnpublishedChangesManager.IsAnyUnpublishedChangesAsync())
                {
                    var publishWindow = new PublishWindow();
                    UnpublishedConcepts unpublishedConcepts = await UnpublishedChangesManager.GetAllUnpublishedChangesAsync();
                    MainWaitingControl.Message = Properties.Resources.Publish_inprogress;
                    MainWaitingControl.TaskIncrement();
                    Tuple<List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>> unpublishedObjects =
                        await PendingChangesPublishManager.GetUnpublishedObjects(unpublishedConcepts);
                    MainWaitingControl.TaskDecrement();
                    publishWindow.ShowUnpublishedObjects(unpublishedObjects);
                    publishWindow.ShowDialog();

                    if (publishWindow.Success)
                    {
                        ((BrowserApplication)BrowserApplicationContainer.MasterApplication).RefreshAllTabs();
                    }
                }
                else
                {
                    KWMessageBox.Show(Properties.Resources.There_Is_No_Items_To_Publish_,
                                     MessageBoxButton.OK, MessageBoxImage.Information,
                                    MessageBoxResult.OK);
                }
            }
            catch (Exception ex)
            {
                MainWaitingControl.TaskDecrement();
                KWMessageBox.Show(ex.Message);
            }
            finally
            {

            }
        }

        private void NewDataImportButton_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.DataImport;
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.Home;
        }

        private void Button_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.Graph;
        }

        private void Button_PreviewMouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.Browser;
        }
        private void MapAppButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.Map;
        }
        private void PresentationWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (GraphApplicationContainer.MasterApplication as GraphApplication).HideRightClickMenuItem();
            (MapApplicationContainer.MasterApplication as MapApplication).CloseAllPopups();
        }

        private void PresentationWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (GraphApplicationContainer.MasterApplication as GraphApplication).HideRightClickMenuItem();
        }

        private void mainWindowQuickSearchControl_QuickSearchResultChoosen(object sender, Controls.QuickSearchControl.QuickSearchResultChoosenEventArgs e)
        {
            var objectsToShow = new[] { e.ChoosenResult };
            ShowObjectsOnGraph(objectsToShow);
        }

        public void ShowObjectsOnGraph(IEnumerable<KWObject> objectsToShow)
        {
            showObjectsOnGraphObserver.ReportAction(new ObjectsShowingArgs { ObjectsToShow = objectsToShow });
            CurrentApplication = PresentationApplications.Graph;
        }

        private void PresentationWindow_Executed_allShortcutKeys(object sender, ExecutedRoutedEventArgs e)
        {
            string parameter = e.Parameter as string;
            SupportedShortCutKey supportedShortCutKey;
            supportedShortCutKey = GetSupportedShortcutKeyByCommandParameter(parameter);
            PerformCommandForShortcutKey(supportedShortCutKey);
        }

        private void PerformCommandForShortcutKey(SupportedShortCutKey supportedShortCutKey)
        {
            var currentApp = GetShowingApplication();
            switch (supportedShortCutKey)
            {
                case SupportedShortCutKey.Ctrl_F:
                    MainWindowQuickSearchControl.Focus();
                    break;
                case SupportedShortCutKey.Ctrl_L:
                    CurrentApplication = PresentationApplications.Graph;
                    GraphApplicationContainer.PerformCommandForShortcutKey(supportedShortCutKey);
                    break;
                case SupportedShortCutKey.Del:
                    if (currentApp == GraphApplicationContainer)
                    {
                        GraphApplicationContainer.PerformCommandForShortcutKey(supportedShortCutKey);
                    }
                    break;
                case SupportedShortCutKey.Shift_Del:
                    GraphApplicationContainer.PerformCommandForShortcutKey(supportedShortCutKey);
                    break;
                case SupportedShortCutKey.Ctrl_D:
                    GraphApplicationContainer.PerformCommandForShortcutKey(supportedShortCutKey);
                    break;
                case SupportedShortCutKey.F1:
                    ShowShortcutKeysHelpWindow();
                    break;
                case SupportedShortCutKey.Esc:
                    GraphApplicationContainer.PerformCommandForShortcutKey(supportedShortCutKey);
                    break;
                default:
                    currentApp.PerformCommandForShortcutKey(supportedShortCutKey);
                    break;
            }
        }

        private void ShowShortcutKeysHelpWindow()
        {
            ShortcutKeysHelpWindow = new Help.ShortcutKeys();
            ShortcutKeysHelpWindow.ShowDialog();
        }

        private static SupportedShortCutKey GetSupportedShortcutKeyByCommandParameter(string parameter)
        {
            SupportedShortCutKey supportedShortCutKey;
            switch (parameter)
            {
                case "Ctrl_A":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_A;
                    break;
                case "Ctrl_S":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_S;
                    break;
                case "Ctrl_Shift_I":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_Shift_I;
                    break;
                case "Ctrl_F":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_F;
                    break;
                case "Ctrl_L":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_L;
                    break;
                case "Ctrl_B":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_B;
                    break;
                case "Ctrl_N":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_N;
                    break;
                case "RightClickKey":
                    supportedShortCutKey = SupportedShortCutKey.RightClickKey;
                    break;
                case "F1":
                    supportedShortCutKey = SupportedShortCutKey.F1;
                    break;
                case "Esc":
                    supportedShortCutKey = SupportedShortCutKey.Esc;
                    break;
                case "Del":
                    supportedShortCutKey = SupportedShortCutKey.Del;
                    break;
                case "Shift_Del":
                    supportedShortCutKey = SupportedShortCutKey.Shift_Del;
                    break;
                case "Ctrl_D":
                    supportedShortCutKey = SupportedShortCutKey.Ctrl_D;
                    break;
                default:
                    supportedShortCutKey = SupportedShortCutKey.Unknown;
                    break;
            }

            return supportedShortCutKey;
        }

        private void CommandBinding_CanExecute_allShortcutKeys(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DatalakeAppButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.BigDataSearch;
        }

        private void PresentationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogOut = false;
#if DEBUG
            CurrentApplication = PresentationApplications.Graph;
#else
                CurrentApplication = PresentationApplications.Home;
#endif
        }

        private void EditPermisionsButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
            ShowInvestigationPermissionsDialog();
        }

        private void ShowInvestigationPermissionsDialog()
        {
            SettingsWindow settingsWindow = new SettingsWindow()
            {
                Owner = GetWindow(this)
            };

            settingsWindow.ThemeChanged -= SettingsWindow_ThemeChanged;
            settingsWindow.ThemeChanged += SettingsWindow_ThemeChanged;
            settingsWindow.ShowDialog();
        }

        private void SettingsWindow_ThemeChanged(object sender, ThemeChangedEventArgs e)
        {
            CurrentTheme = e.Theme;
        }

        private void ApplicationStackPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (MapApplicationContainer.MasterApplication as MapApplication).CloseAllPopups();
        }

        private async void GraphApplicationButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("SelectedMapObjects"))
            {
                List<KWObject> selectedObjects = e.Data.GetData("SelectedMapObjects") as List<KWObject>;
                CurrentApplication = PresentationApplications.Graph;
                await (GraphApplicationContainer.MasterApplication as GraphApplication).ShowObjectsAsync(selectedObjects);
            }
        }

        private async void BrowserApplicationButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("SelectedMapObjects"))
            {
                List<KWObject> selectedObjects = e.Data.GetData("SelectedMapObjects") as List<KWObject>;
                CurrentApplication = PresentationApplications.Browser;
                await (BrowserApplicationContainer.MasterApplication as BrowserApplication).ShowObjectsAsync(selectedObjects);
            }
        }

        private void PresentationWindow_Deactivated(object sender, EventArgs e)
        {
            (MapApplicationContainer.MasterApplication as MapApplication).CloseAllPopups();
        }

        private async void DataSourceAppButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.DataSource;
            await (DataSourceApplicationContainer.MasterApplication as DataSourceApplication).LoadAllDataSources();
        }

        private void ImgAnalysisAppButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.ImageAnalysis;
            //await(DataSourceApplicationContainer.MasterApplication as DataSourceApplication).LoadAllDataSources();
        }

        private void ObjectExplorerAppButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentApplication = PresentationApplications.ObjectExplorer;
            (ObjectExplorerApplicationContainer.MasterApplication as ObjectExplorerApplication).Init();
        }

        private void objectExplorerApplicationContainer_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            TakeSnapshot(e.UIElement, e.DefaultFileName);
        }

        private void ObjectExplorerApplicationContainer_SnapshotTaken(object sender, Utility.TakeSnapshotEventArgs e)
        {
            SaveSnapshot(e.Snapshot, e.DefaultFileName);
        }

        public void TakeSnapshot(UIElement uIElement, string defaultFileName)
        {
            try
            {
                MainWaitingControl.Message = Properties.Resources.Take_Snapshot;
                MainWaitingControl.TaskDecrement();

                string filePath = string.Empty;

                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.FileName = defaultFileName; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "PNG |*.png"; // Filter files by extension

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filePath = dlg.FileName;

                    int width = (int)uIElement.RenderSize.Width;
                    int height = (int)uIElement.RenderSize.Height;

                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                    renderTargetBitmap.Render(uIElement);

                    PngBitmapEncoder pngImage = new PngBitmapEncoder();
                    pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                    using (Stream fileStream = File.Create(filePath))
                    {
                        pngImage.Save(fileStream);
                    }

                    if (KWMessageBox.Show(Properties.Resources.Snapshot_saved_successfully_ + "\n\n" + Properties.Resources.Do_you_want_to_display_image_,
                                       MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Process.Start(filePath);
                    }
                }


                MainWaitingControl.TaskDecrement();
            }
            catch (Exception ex)
            {
                MainWaitingControl.TaskDecrement();
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveSnapshot(PngBitmapEncoder snapshot, string defaultFileName)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.FileName = defaultFileName; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "PNG |*.png"; // Filter files by extension

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    using (Stream fileStream = File.Create(dlg.FileName))
                    {
                        snapshot.Save(fileStream);
                    }

                    if (KWMessageBox.Show(Properties.Resources.Snapshot_saved_successfully_ + "\n\n" + Properties.Resources.Do_you_want_to_display_image_,
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Process.Start(dlg.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void graphApplicationContainer_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            TakeSnapshot(e.UIElement, e.DefaultFileName);
        }

        private void GraphApplicationContainer_SnapshotTaken(object sender, Utility.TakeSnapshotEventArgs e)
        {
            SaveSnapshot(e.Snapshot, e.DefaultFileName);
        }

        private void mapApplicationContainer_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            TakeSnapshot(e.UIElement, e.DefaultFileName);
        }

        private async void InvestigationButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GraphApplicationStatus currentGraphApplicationStatus = await GraphApplicationContainer.GetGraphApplicationStatus();

            BrowserApplicationStatus currentBrowserApplicationStatus = BrowserApplicationContainer.GetBrowserApplicationStatus();

            MapApplicationStatus currentMapApplicationStatus = MapApplicationContainer.GetMapApplicationStatus();

            SaveInvestigationUnpublishedConcepts unpublishedConcepts = await UnpublishedChangesManager.GetAllSaveInvestigationUnpublishedChanges();

            InvestigationProvider investigationProvider = new InvestigationProvider();

            investigationStatus = new InvestigationStatus
            {
                GraphStatus = currentGraphApplicationStatus,
                BrowserStatus = currentBrowserApplicationStatus,
                MapStatus = currentMapApplicationStatus,
                SaveInvestigationUnpublishedConcepts = unpublishedConcepts
            };

            InvestigationViewModel investigationViewModel = new InvestigationViewModel(GraphApplicationContainer.TakeBitMapImageOfGraph(), investigationStatus);

            var InvestigationWidow = new InvestigationManagerWindow(investigationViewModel);
            InvestigationWidow.LoadInvestigationButtonClicked += GetInvestigationWindow_LoadInvestigationButtonClicked;
            InvestigationWidow.ShowDialog();
        }

        private void MapApplicationContainer_SnapshotTaken(object sender, Utility.TakeSnapshotEventArgs e)
        {
            SaveSnapshot(e.Snapshot, e.DefaultFileName);
        }

        private void GraphApplicationContainer_DocumentCreationRequestSubmited(object sender, Controls.Graph.DocumentCreationRequestSubmitedEventAgrs e)
        {
            CurrentApplication = PresentationApplications.DataImport;
            DataImportApplication.DocumentCreationRequest(e.FilePath);
        }
        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            LogOut = true;
            Close();
        }

        private async void PresentationWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (!(await PrepairQuitWorkspace()))
                    e.Cancel = true;
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(string.Format(Properties.Resources.Unable_To_Prepairing_Exit, ex.Message),
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}
