using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using GPAS.Workspace.Presentation.Windows.DataImport;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for SetMappingPropertiesValueUserControl.xaml
    /// </summary>
    public partial class SetMappingPropertiesValueUserControl
    {
        #region Properties

        private DependencyObject clickedElement;
        private UserControl draggingItem;
        private bool isDraggingStarted;
        private Point startPoint;


        public PropertyMapModel SelectedProperty
        {
            get => (PropertyMapModel)GetValue(SelectedPropertyProperty);
            set => SetValue(SelectedPropertyProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPropertyProperty =
            DependencyProperty.Register(nameof(SelectedProperty), typeof(PropertyMapModel),
                typeof(SetMappingPropertiesValueUserControl), new PropertyMetadata(null, OnSetSelectedPropertyChanged));

        private static void OnSetSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SetMappingPropertiesValueUserControl)d).OnSetSelectedPropertyChanged(e);
        }

        private void OnSetSelectedPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            SelectedSingleProperty = GetSelectedSingleProperty(SelectedProperty);
            if(SelectedProperty is MultiPropertyMapModel multiProperty)
            {
                multiProperty.SelectedLeafPropertyChanged -= MultiProperty_SelectedLeafPropertyChanged;
                multiProperty.SelectedLeafPropertyChanged += MultiProperty_SelectedLeafPropertyChanged;
            }
        }

        public SinglePropertyMapModel SelectedSingleProperty
        {
            get { return (SinglePropertyMapModel)GetValue(SelectedSinglePropertyProperty); }
            set { SetValue(SelectedSinglePropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSingleProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSinglePropertyProperty =
            DependencyProperty.Register(nameof(SelectedSingleProperty), typeof(SinglePropertyMapModel), typeof(SetMappingPropertiesValueUserControl),
                new PropertyMetadata(null));



        #endregion

        #region Method

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public SetMappingPropertiesValueUserControl()
        {
            InitializeComponent();
        }

        private void MultiProperty_SelectedLeafPropertyChanged(object sender, EventArgs e)
        {
            SelectedSingleProperty = GetSelectedSingleProperty(SelectedProperty);
        }

        private SinglePropertyMapModel GetSelectedSingleProperty(PropertyMapModel property)
        {
            if (property is SinglePropertyMapModel singleProperty)
                return singleProperty;
            else if (property is MultiPropertyMapModel multiProperty)
                return multiProperty.SelectedLeafProperty;

            return null;
        }

        private void AddValue(DataSourceFieldModel selectedFiled)
        {
            ((MappingViewModel)DataContext).AddValueToProperty(SelectedSingleProperty, selectedFiled);
        }

        private void RemoveValue(ValueMapModel value)
        {
            ((MappingViewModel)DataContext).RemoveValueFromProperty(SelectedSingleProperty, value);
        }

        private void RemoveAllValue()
        {
            ((MappingViewModel)DataContext).ClearAllValueFromProperty(SelectedProperty);
        }

        /// <summary>
        /// اعتبار‌سنجی آیتم‌های درگ شده بر روی کنترل 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsValidDraggingItem(DragEventArgs e)
        {
            bool isValid = true;
            var item = e.Data.GetData(e.Data.GetFormats()[0]) as TreeViewItem;

            if (item == null)
            {
                var listItem = e.Data.GetData(e.Data.GetFormats()[0]) as ListViewItem;

                if (listItem == null)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        /// <summary>
        /// نگهداشتن درگ بر روی لیست مقادیر
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValuesListViewOnDragOver(object sender, DragEventArgs e)
        {
            if (IsValidDraggingItem(e))
                return;

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        ///  نکهداشتن درگ بر روی آیتم‌‌های لیست مقادیر
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueListViewDragOverOnHandler(object sender, DragEventArgs e)
        {
            if (IsValidDraggingItem(e))
                return;

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void ValuesListViewOnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(e.Data.GetFormats()[0]) is TreeViewItem item)
            {
                AddValue((DataSourceFieldModel)item.DataContext);
                e.Handled = true;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            if (parentObject is T parent)
                return parent;

            return FindParent<T>(parentObject);
        }

        private void ValueListViewPreviewMouseLeftButtonDownOnHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // در صورتی که بر‌روی متن مقادیر ثابت کلیک شده باشد عملیات درگ باید 
                //نادیده گرفته شود و انجام نشود در غیر اینصورت خطای "نخ متفاوت" می‌دهد 
                if (FindParent<TextBox>((DependencyObject)e.OriginalSource) != null)
                    return;

                if (FindParent<Button>((DependencyObject)e.OriginalSource) != null)
                    return;

                clickedElement = (DependencyObject)sender;
                startPoint = e.GetPosition(sender as ListViewItem);
                draggingItem = new DraggingDataSourceFieldUserControl
                {
                    Visibility = Visibility.Hidden,
                    Opacity = .3,
                    IsHitTestVisible = false,
                    Text = ((ValueMapModel)((ListViewItem)sender).DataContext).Field.Title
                };
                MainGrid.Children.Add(draggingItem);
                isDraggingStarted = true;
            }
            catch
            {
                // ignored
            }
        }

        private void ValueListViewDropOnHandler(object sender, DragEventArgs e)
        {
            ValueMapModel droppedValue = null;
            if (e.Data.GetData(e.Data.GetFormats()[0]) is TreeViewItem treeItem && treeItem.DataContext is DataSourceFieldModel field)
            {
                AddValue(field);
                droppedValue = SelectedSingleProperty.ValueCollection.Last();
            }
            else if (e.Data.GetData(e.Data.GetFormats()[0]) is ListViewItem listItem && listItem.DataContext is ValueMapModel valueMap)
                droppedValue = valueMap;
            else
                return;

            ValueMapModel targetValue = ((ListViewItem)sender).DataContext as ValueMapModel;
            MoveValues(droppedValue, targetValue);
            e.Handled = true;
        }

        private void MoveValues(ValueMapModel source, ValueMapModel target)
        {
            ((MappingViewModel)DataContext).MoveValueInProperty(SelectedSingleProperty, source, target);
        }

        private void DeleteButtonOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RemoveValue((ValueMapModel)((Button)sender).DataContext);
        }

        private void DataSourceFieldsTreeViewOnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (!(((TreeViewItem)sender).DataContext is DataSourceFieldModel selectedField))
                return;

            clickedElement = (DependencyObject)sender;
            startPoint = e.GetPosition(sender as TreeViewItem);
            draggingItem = new DraggingDataSourceFieldUserControl
            {
                Visibility = Visibility.Hidden,
                Opacity = .3,
                IsHitTestVisible = false,
                Text = selectedField.Title
            };
            MainGrid.Children.Add(draggingItem);
            isDraggingStarted = true;

        }

        private void DataSourceFieldsTreeViewOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingStarted = false;
        }

        private void MainGridOnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!isDraggingStarted)
                    return;

                if (e.OriginalSource is TextBox)
                    return;

                if (e.OriginalSource is Button)
                    return;

                // Get the current mouse position
                Point mousePos = e.GetPosition(null);
                Vector diff = startPoint - mousePos;

                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Panel.SetZIndex(draggingItem, MainGrid.Children.Count);
                    draggingItem.Visibility = Visibility.Visible;
                    GiveFeedback += DragSource_GiveFeedback;
                    DragDrop.DoDragDrop(clickedElement, clickedElement, DragDropEffects.Copy);
                    isDraggingStarted = false;
                    draggingItem.Visibility = Visibility.Hidden;
                    draggingItem.Margin = new Thickness(0);
                    MaxHeight = double.PositiveInfinity;
                    GiveFeedback -= DragSource_GiveFeedback;
                    MainGrid.Children.Remove(draggingItem);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                var p = System.Windows.Forms.Control.MousePosition;
                var myPos = PointToScreen(new Point(0, 0));
                var mousePosition = new Point(p.X - myPos.X, p.Y - myPos.Y);
                double left = mousePosition.X;
                double top = mousePosition.Y - 15;

                draggingItem.Margin = new Thickness(left, top, 0, 0);
            }
            catch
            {
                // ignored
            }
        }

        private void DataSourceFieldsTreeViewOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((TreeViewItem)sender).IsSelected)
            {
                if (DataSourceFieldsTreeView.SelectedItem is DataSourceFieldModel selectedItem)
                {
                    AddValue(selectedItem);
                }
            }
        }

        private void AddButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (DataSourceFieldsTreeView.SelectedItem is DataSourceFieldModel dataSourceFieldModel)
            {
                AddValue(dataSourceFieldModel);
            }
        }

        private void ClearButtonOnClick(object sender, RoutedEventArgs e)
        {
            RemoveAllValue();
        }

        private void ValueListViewMouseUpOnHandler(object sender, MouseButtonEventArgs e)
        {
            isDraggingStarted = false;
        }

        private void RegularExpressionMenuItemOnClick(object sender, RoutedEventArgs e)
        {
            RegularExpressionWindow regularExpressionWindow =
                new RegularExpressionWindow(((MenuItem)sender).DataContext as ValueMapModel)
                {
                    Owner = Window.GetWindow(this)
                };

            regularExpressionWindow.ShowDialog();
        }

        private void PropertyConfigButtonOnClick(object sender, RoutedEventArgs e)
        {
            ShowDateTimeConfig(((Button)sender).DataContext as DateTimePropertyMapModel);
        }

        private void ShowDateTimeConfig(DateTimePropertyMapModel dateTimeProperty)
        {
            DateTimeConfigWindow dateTimeConfigWindow = new DateTimeConfigWindow(dateTimeProperty)
            {
                Owner = Window.GetWindow(this)
            };
            dateTimeConfigWindow.ShowDialog();
        }

        private void RecalculateButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            MaterialDesignThemes.Wpf.ButtonProgressAssist.SetIsIndicatorVisible(button, true);

            MetaDataItemModel metaDataItem = ((MetaDataFieldModel)button.DataContext)?.RelatedMetaDataItem;
            if (metaDataItem == null) return;
            RecalculateMetaDataItem(metaDataItem);
        }

        private async void RecalculateMetaDataItem(MetaDataItemModel metaDataItem)
        {
            if (DataContext is MappingViewModel viewModel)
                await viewModel.Map?.OwnerDataSource?.RecalculateMetaDataItemAsync(metaDataItem);
        }

        #endregion
    }
}
