using GPAS.Workspace.Presentation.Helpers;
using GPAS.Workspace.Presentation.Windows;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GPAS.Workspace.Presentation.ApplicationContainers
{
    public abstract partial class ApplicationContainerBase
    {
        #region Properties

        private double minHeight = 24;
        private double minWidth = 24;
        private double initialTopHeight = 200;
        private double initialBottomHeight = 200;
        private double initialLeftWidth = 400;
        private double initialRightWidth = 430;

        public abstract string Title
        {
            get;
        }

        public abstract BitmapImage Icon
        {
            get;
        }
        public abstract BitmapImage SelectedArrow
        {
            get;
        }

        public abstract Type MasterApplicationType
        {
            get;
        }

        public abstract Applications.PresentationApplication MasterApplication
        {
            get;
        }

        public abstract List<PresentationHelper> Helpers
        {
            get;
        }

        #endregion

        #region Dependency

        protected ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        protected static readonly DependencyProperty CurrentThemeProperty = DependencyProperty.Register(nameof(CurrentTheme),
            typeof(ThemeApplication), typeof(ApplicationContainerBase), new PropertyMetadata(null));


        protected MainWindow MainWindow
        {
            get { return (MainWindow)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        protected static readonly DependencyProperty MainWindowProperty =
            DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(ApplicationContainerBase), new PropertyMetadata(null));

        public bool LeftIsExpanded
        {
            get => (bool)GetValue(LeftIsExpandedProperty);
            set => SetValue(LeftIsExpandedProperty, value);
        }

        public static readonly DependencyProperty LeftIsExpandedProperty = DependencyProperty.Register(nameof(LeftIsExpanded),
            typeof(bool), typeof(ApplicationContainerBase), new PropertyMetadata(true, OnLeftIsExpandedPropertyChanged));

        public bool TopIsExpanded
        {
            get => (bool)GetValue(TopIsExpandedProperty);
            set => SetValue(TopIsExpandedProperty, value);
        }

        public static readonly DependencyProperty TopIsExpandedProperty = DependencyProperty.Register(nameof(TopIsExpanded),
            typeof(bool), typeof(ApplicationContainerBase), new PropertyMetadata(true, OnTopIsExpandedPropertyChanged));

        public bool RightIsExpanded
        {
            get => (bool)GetValue(RightIsExpandedProperty);
            set => SetValue(RightIsExpandedProperty, value);
        }

        public static readonly DependencyProperty RightIsExpandedProperty = DependencyProperty.Register(nameof(RightIsExpanded),
            typeof(bool), typeof(ApplicationContainerBase), new PropertyMetadata(true, OnRightIsExpandedPropertyChanged));

        public bool BottomIsExpanded
        {
            get => (bool)GetValue(BottomIsExpandedProperty);
            set => SetValue(BottomIsExpandedProperty, value);
        }

        public static readonly DependencyProperty BottomIsExpandedProperty = DependencyProperty.Register(nameof(BottomIsExpanded),
            typeof(bool), typeof(ApplicationContainerBase), new PropertyMetadata(true, OnBottomIsExpandedPropertyChanged));

        #endregion

        #region Methods

        public ApplicationContainerBase()
        {
            InitializeComponent();
            // افزودن کاربرد به نمایش
            MasterApplicationGrid.Children.Add(MasterApplication);
            Loaded += ApplicationContainerBase_Loaded;
        }

        private void ApplicationContainerBase_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            MainWindow = window as MainWindow;

            if (MainWindow == null)
                return;

            MainWindow.CurrentThemeChanged -= MainWindow_CurrentThemeChanged;
            MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;

            ManageAppearanceHelpers();
        }

        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CurrentTheme = MainWindow.CurrentTheme;
        }

        private static void OnLeftIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ApplicationContainerBase)d).OnLeftIsExpandedPropertyChanged(e);
        }

        private void OnLeftIsExpandedPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ChangeLeftGridWidthProperty();
            OnLeftHelperIsExpandedChanged(e);
        }

        private static void OnTopIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ApplicationContainerBase)d).OnTopIsExpandedPropertyChanged(e);
        }

        private void OnTopIsExpandedPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ChangeTopGridHeightProperty();
            OnTopHelperIsExpandedChanged(e);
        }

        private static void OnRightIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ApplicationContainerBase)d).OnRightIsExpandedPropertyChanged(e);
        }

        private void OnRightIsExpandedPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            RightIsExpandedPropertyChanged();
            ChangeRightGridWidthProperty();
            OnRightHelperIsExpandedChanged(e);
        }

        private static void OnBottomIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ApplicationContainerBase)d).OnBottomIsExpandedPropertyChanged(e);
        }

        private void OnBottomIsExpandedPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ChangeBottomGridHeightProperty();
            BottomIsExpandedPropertyChanged();
            OnBottomHelperIsExpandedChanged(e);
        }

        protected virtual void LeftIsExpandedPropertyChanged()
        {

        }

        protected virtual void TopIsExpandedPropertyChanged()
        {

        }

        protected virtual void RightIsExpandedPropertyChanged()
        {

        }

        protected virtual void BottomIsExpandedPropertyChanged()
        {

        }

        public event DependencyPropertyChangedEventHandler LeftHelperIsExpandedChanged;

        protected void OnLeftHelperIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            LeftHelperIsExpandedChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler TopHelperIsExpandedChanged;

        protected void OnTopHelperIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            TopHelperIsExpandedChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler RightHelperIsExpandedChanged;

        protected void OnRightHelperIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            RightHelperIsExpandedChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler BottomHelperIsExpandedChanged;

        protected void OnBottomHelperIsExpandedChanged(DependencyPropertyChangedEventArgs e)
        {
            BottomHelperIsExpandedChanged?.Invoke(this, e);
        }

        public void AddHelper(PresentationHelper helper, HelperPosition position)
        {
            // نمایش ابزارهای کمکی
            TabItem newHelperTabItem = new TabItem();
            newHelperTabItem.Header = helper.Title;
            newHelperTabItem.Content = helper;
            switch (position)
            {
                case HelperPosition.Left:
                    LeftTabControl.Items.Add(newHelperTabItem);
                    LeftTabControl.SelectedItem = newHelperTabItem;
                    break;
                case HelperPosition.Right:
                    RightTabControl.Items.Add(newHelperTabItem);
                    RightTabControl.SelectedItem = newHelperTabItem;
                    break;
                case HelperPosition.Top:
                    TopTabControl.Items.Add(newHelperTabItem);
                    TopTabControl.SelectedItem = newHelperTabItem;
                    break;
                case HelperPosition.Bottom:
                    BottomTabControl.Items.Add(newHelperTabItem);
                    BottomTabControl.SelectedItem = newHelperTabItem;
                    break;
                default:
                    break;
            }
            RefreshHelpersVisibility();
        }

        private void RefreshHelpersVisibility()
        {
            LeftGrid.Visibility = (LeftTabControl.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
            RightGrid.Visibility = (RightTabControl.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
            TopGrid.Visibility = (TopTabControl.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
            BottomGrid.Visibility = (BottomTabControl.Items.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public virtual void PerformCommandForShortcutKey(SupportedShortCutKey commandShortCutKey)
        {
            MasterApplication.PerformCommandForShortcutKey(commandShortCutKey);
        }

        protected void ShowHelper(PresentationHelper helperToShow)
        {
            var position = GetHelperPosition(helperToShow);
            var tabControl = GetTabControlByPosition(position);
            var tabItem = GetHelperTabItem(helperToShow, tabControl);
            ExpandGridByPosition(position);
            tabControl.SelectedItem = tabItem;
        }

        private TabItem GetHelperTabItem(PresentationHelper helperToShow, TabControl tabControl)
        {
            foreach (TabItem item in tabControl.Items)
            {
                if (item.Content == helperToShow)
                {
                    return item;
                }
            }
            throw new InvalidOperationException();
        }

        private HelperPosition GetHelperPosition(PresentationHelper helperToShow)
        {
            foreach (TabItem tabItem in LeftTabControl.Items)
            {
                if (tabItem.Content == helperToShow)
                {
                    return HelperPosition.Left;
                }
            }
            foreach (TabItem tabItem in RightTabControl.Items)
            {
                if (tabItem.Content == helperToShow)
                {
                    return HelperPosition.Right;
                }
            }
            foreach (TabItem tabItem in TopTabControl.Items)
            {
                if (tabItem.Content == helperToShow)
                {
                    return HelperPosition.Top;
                }
            }
            foreach (TabItem tabItem in BottomTabControl.Items)
            {
                if (tabItem.Content == helperToShow)
                {
                    return HelperPosition.Bottom;
                }
            }
            throw new InvalidOperationException();
        }

        private TabControl GetTabControlByPosition(HelperPosition position)
        {
            switch (position)
            {
                case HelperPosition.Left:
                    return LeftTabControl;
                case HelperPosition.Right:
                    return RightTabControl;
                case HelperPosition.Top:
                    return TopTabControl;
                case HelperPosition.Bottom:
                    return BottomTabControl;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void ExpandGridByPosition(HelperPosition position)
        {
            switch (position)
            {
                case HelperPosition.Left:
                    LeftIsExpanded = true;
                    break;
                case HelperPosition.Right:
                    RightIsExpanded = true;
                    break;
                case HelperPosition.Top:
                    TopIsExpanded = true;
                    break;
                case HelperPosition.Bottom:
                    BottomIsExpanded = true;
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void RightTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object newVal = (RightTabControl.SelectedItem as TabItem).Content;
            object oldVal = null;
            if (e.RemovedItems.Count > 0)
                oldVal = e.RemovedItems[0];

            OnRightHelperTabs_SelectionChanged(new DependencyPropertyChangedEventArgs(TabControl.SelectedItemProperty, oldVal, newVal));
        }

        private void LeftTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object newVal = (LeftTabControl.SelectedItem as TabItem).Content;
            object oldVal = null;
            if (e.RemovedItems.Count > 0)
                oldVal = e.RemovedItems[0];

            OnLeftHelperTabs_SelectionChanged(new DependencyPropertyChangedEventArgs(TabControl.SelectedItemProperty, oldVal, newVal));
        }

        private void TopTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object newVal = (TopTabControl.SelectedItem as TabItem).Content;
            object oldVal = null;
            if (e.RemovedItems.Count > 0)
                oldVal = e.RemovedItems[0];

            OnTopHelperTabs_SelectionChanged(new DependencyPropertyChangedEventArgs(TabControl.SelectedItemProperty, oldVal, newVal));
        }

        private void BottomTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object newVal = (BottomTabControl.SelectedItem as TabItem).Content;
            object oldVal = null;
            if (e.RemovedItems.Count > 0)
                oldVal = e.RemovedItems[0];

            OnBottomHelperTabs_SelectionChanged(new DependencyPropertyChangedEventArgs(TabControl.SelectedItemProperty, oldVal, newVal));
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }

        private void RunStoryboard(string controlName, DependencyProperty parameter, double from, double to)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(0))
            };

            Storyboard.SetTargetName(doubleAnimation, controlName);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(parameter));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin(this);
        }

        private void ChangeLeftGridWidthProperty()
        {
            if (LeftIsExpanded)
            {
                RunStoryboard(LeftGrid.Name, WidthProperty, minWidth, initialLeftWidth);
                LeftGrid.BeginAnimation(WidthProperty, null);
            }
            else
            {
                initialLeftWidth = LeftGrid.ActualWidth;
                RunStoryboard(LeftGrid.Name, WidthProperty, initialLeftWidth, minWidth);
            }
        }

        private void ChangeRightGridWidthProperty()
        {
            if (RightIsExpanded)
            {
                RunStoryboard(RightGrid.Name, WidthProperty, minWidth, initialRightWidth);
                RightGrid.BeginAnimation(WidthProperty, null);
                RightTabControl.TabStripPlacement = Dock.Right;
            }
            else
            {
                initialRightWidth = RightGrid.ActualWidth;
                RunStoryboard(RightGrid.Name, WidthProperty, initialRightWidth, minWidth);
                RightTabControl.TabStripPlacement = Dock.Left;
            }
        }

        private void ChangeTopGridHeightProperty()
        {
            if (TopIsExpanded)
            {
                RunStoryboard(TopGrid.Name, HeightProperty, minHeight, initialTopHeight);
                TopGrid.BeginAnimation(HeightProperty, null);
            }
            else
            {
                initialTopHeight = TopGrid.ActualHeight;
                RunStoryboard(TopGrid.Name, HeightProperty, initialTopHeight, minHeight);
            }
        }

        private void ChangeBottomGridHeightProperty()
        {
            if (BottomIsExpanded)
            {
                RunStoryboard(BottomGrid.Name, HeightProperty, minHeight, initialBottomHeight);
                BottomGrid.BeginAnimation(HeightProperty, null);
                BottomTabControl.TabStripPlacement = Dock.Bottom;
            }
            else
            {
                initialBottomHeight = BottomGrid.ActualHeight;
                RunStoryboard(BottomGrid.Name, HeightProperty, initialBottomHeight, minHeight);
                BottomTabControl.TabStripPlacement = Dock.Top;
            }
        }

        private void Open(object item)
        {
            if (!(item is Border) && !(item is TextBlock) && !(item is Grid))
                return;

            var tabItem = FindParent<TabItem>(item as DependencyObject);

            if (tabItem == null)
                return;

            var parentTabControl = FindParent<TabControl>(tabItem);

            if (tabItem.IsSelected)
            {
                switch (parentTabControl.Tag)
                {
                    case HelperPosition.Left:
                        LeftIsExpanded = !LeftIsExpanded;
                        break;
                    case HelperPosition.Top:
                        TopIsExpanded = !TopIsExpanded;
                        break;
                    case HelperPosition.Right:
                        RightIsExpanded = !RightIsExpanded;
                        break;
                    case HelperPosition.Bottom:
                        BottomIsExpanded = !BottomIsExpanded;
                        break;
                }
            }
            else
            {
                switch (parentTabControl.Tag)
                {
                    case HelperPosition.Left:
                        LeftIsExpanded = true;
                        break;
                    case HelperPosition.Top:
                        TopIsExpanded = true;
                        break;
                    case HelperPosition.Right:
                        RightIsExpanded = true;
                        break;
                    case HelperPosition.Bottom:
                        BottomIsExpanded = true;
                        break;
                }
            }
        }

        private void TopTabControl_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Open(e.OriginalSource);
        }

        private void BottomTabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Open(e.OriginalSource);
        }

        private void LeftTabControl_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Open(e.OriginalSource);
        }

        private void RightTabControl_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Open(e.OriginalSource);
        }

        private void ManageAppearanceHelpers()
        {
            RightTabControl.SelectedIndex = RightTabControl.Items.Count - 1;
            LeftTabControl.SelectedIndex = LeftTabControl.Items.Count - 1;
            TopTabControl.SelectedIndex = TopTabControl.Items.Count - 1;
            BottomTabControl.SelectedIndex = BottomTabControl.Items.Count - 1;

            LeftIsExpanded = false;
            RightIsExpanded = true;
            TopIsExpanded = false;
            BottomIsExpanded = false;
        }

        public virtual void Reset()
        {
            RightTabControl.SelectedIndex = 0;
            ManageAppearanceHelpers();
            MasterApplication.Reset();
            foreach (PresentationHelper helper in Helpers)
            {
                helper.Reset();
            }
        }

        #endregion

        #region Events

        public event DependencyPropertyChangedEventHandler RightHelperTabs_SelectionChanged;
        protected void OnRightHelperTabs_SelectionChanged(DependencyPropertyChangedEventArgs e)
        {
            RightHelperTabs_SelectionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler LeftHelperTabs_SelectionChanged;
        protected void OnLeftHelperTabs_SelectionChanged(DependencyPropertyChangedEventArgs e)
        {
            LeftHelperTabs_SelectionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler TopHelperTabs_SelectionChanged;
        protected void OnTopHelperTabs_SelectionChanged(DependencyPropertyChangedEventArgs e)
        {
            TopHelperTabs_SelectionChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler BottomHelperTabs_SelectionChanged;
        protected void OnBottomHelperTabs_SelectionChanged(DependencyPropertyChangedEventArgs e)
        {
            BottomHelperTabs_SelectionChanged?.Invoke(this, e);
        }

        #endregion        
    }
}
