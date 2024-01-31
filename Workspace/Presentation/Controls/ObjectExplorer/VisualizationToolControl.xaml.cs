using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer
{
    /// <summary>
    /// Interaction logic for VisualizationToolControl.xaml
    /// </summary>
    public partial class VisualizationToolControl
    {
        #region Global Variables

        private List<PreviewStatistic> menuItems = new List<PreviewStatistic>();
        public List<PreviewStatistic> MenuItems {
            get => menuItems;
            set
            {
                menuItems = value;
                if(MenuItems == null || MenuItems.Count == 0)
                {
                    DisplayDropDownMenu = false;
                    IsEnabled = false;
                }
                else
                {
                    DisplayDropDownMenu = true;
                    IsEnabled = true;
                }

                PopupListView.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("MenuItems") { Source = this });
            }
        }

        #endregion

        #region Events
        public event EventHandler<RoutedEventArgs> ButtonClicked;

        public void OnButtonClicked(RoutedEventArgs args)
        {
            ButtonClicked?.Invoke(this, args);
        }

        public class DropDownMenuItemClickEventArgs : EventArgs
        {
            public DropDownMenuItemClickEventArgs(PreviewStatistic preview)
            {
                Preview = preview;
            }
            public PreviewStatistic Preview { get; private set; }
        }

        public event EventHandler<DropDownMenuItemClickEventArgs> DropDownMenuItemClicked;

        public void OnDropDownMenuItemClicked(PreviewStatistic preview)
        {
            DropDownMenuItemClicked?.Invoke(this, new DropDownMenuItemClickEventArgs(preview));
        }
        #endregion

        #region Methodes

        public VisualizationToolControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region Dependencies
        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(PackIconKind), 
                typeof(VisualizationToolControl), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(VisualizationToolControl), new PropertyMetadata(""));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(VisualizationToolControl), new PropertyMetadata(""));

        public bool DisplayDropDownMenu
        {
            get { return (bool)GetValue(DisplayDropDownMenuProperty); }
            set { SetValue(DisplayDropDownMenuProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayDropDownMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayDropDownMenuProperty =
            DependencyProperty.Register("DisplayDropDownMenu", typeof(bool), typeof(VisualizationToolControl), new PropertyMetadata(false));

        public VisualizationToolControlViewMode ViewMode
        {
            get { return (VisualizationToolControlViewMode)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModeProperty =
            DependencyProperty.Register("ViewMode", typeof(VisualizationToolControlViewMode), typeof(VisualizationToolControl), new PropertyMetadata(VisualizationToolControlViewMode.GridView, OnSetViewModeChanged));

        private static void OnSetViewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as VisualizationToolControl).OnSetViewModeChanged(e);
        }

        private void OnSetViewModeChanged(DependencyPropertyChangedEventArgs e)
        {
            if(ViewMode == VisualizationToolControlViewMode.GridView)
            {
                ButtonPanel.Visibility = Visibility.Visible;
                ButtonToolbar.Visibility = Visibility.Collapsed;
            }
            else
            {
                ButtonPanel.Visibility = Visibility.Collapsed;
                ButtonToolbar.Visibility = Visibility.Visible;
            }
        }

        #endregion


        /// <summary>
        /// رویداد باز شدن کنترل افزودن موجودیت جدید
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AddNewObjectOnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {
            MenuItemsListView.ItemsSource = MenuItems;
            TopMenuItemsListView.ItemsSource = MenuItems;

            if (MenuItems.Count == 0)
            {                
                DialogHost.CloseDialogCommand.Execute(false, null);
            }           
            OnButtonClicked(eventArgs);
        }

        /// <summary>
        /// رویداد بسته شدن کنترل افزودن موجودیت جدید
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AddNewPropertyOnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            bool result = false;
            if (eventArgs.Parameter != null)
            {
                if (!bool.TryParse(eventArgs.Parameter.ToString(), out result))
                {
                    eventArgs.Cancel();
                    return;
                }
            }
        }

        private void MenuItemsListViewOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            DialogHost.CloseDialogCommand.Execute(true, null);
            OnDropDownMenuItemClicked((PreviewStatistic)(sender as ListViewItem)?.DataContext);
        }
    }

    public enum VisualizationToolControlViewMode
    {
        GridView,
        ToolbarView
    }
}
