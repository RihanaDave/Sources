using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Windows.DataImport;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    public partial class MappingGraphUserControl
    {
        MappingViewModel mappingViewModel = null;

        public MappingGraphUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mappingViewModel = (MappingViewModel)DataContext;
        }

        //TODO فعلا برای فعال و غیر فعال کردن دکمه افزودن موجودیت از این استفاده می‌کنیم
        //تا بعد ببینیم روش بهتری هست یا نه
        private bool isFirstTime = true;

        #region Event Handlers

        /// <summary>
        /// رویداد باز شدن کنترل افزودن موجودیت جدید
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AddNewObjectOnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {
            if (!isFirstTime)
                return;

            AddNewObjectButton.IsEnabled = false;
            isFirstTime = false;
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

            if (result)
                AddObject();
        }

        /// <summary>
        /// رویداد باز شدن کنترل افزودن لینک جدید
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AddNewLinkOnDialogOpened(object sender, DialogOpenedEventArgs eventArgs)
        {
            PrepareCreateLink();
        }

        /// <summary>
        /// رویداد بسته شدن کنترل افزودن لینک جدید
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void AddNewLinkOnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (bool.TryParse(eventArgs.Parameter.ToString(), out bool result))
            {
                if (!result)
                    return;

                AddLink();
            }
            else
            {
                eventArgs.Cancel();
            }
        }

        private void RemoveButtonOnClick(object sender, RoutedEventArgs e)
        {
            RemoveSelectedItems();
        }

        private void ClearAllButtonOnClick(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        /// <summary>
        /// با انتخاب نوع یک موجودت برای افزودن
        /// کلید افزودن فعال می‌شود
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectTypePickerOnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddNewObjectButton.IsEnabled = ObjectTypePicker.SelectedItem?.TypeUri != null;
        }

        /// <summary>
        /// کلید افزودن لینک در کنترل افزودن لینک طبق
        /// نتایج اعتبار‌سنجی این کنترل فعال یا غیر‌فعال می‌شود 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateLinkUserControlOnValidationChanged(object sender, bool e)
        {
            CreateLinkButton.IsEnabled = e;
        }

        #endregion

        #region Methods

        private void AddObject()
        {
            try
            {
                ((MappingViewModel)DataContext).AddNewObject();
                if (((MappingViewModel)DataContext).Map.ObjectCollection.LastOrDefault() is DocumentMapModel documentMapModel)
                {
                    SetPathWindow setPathWindow = new SetPathWindow(((MappingViewModel)DataContext), documentMapModel);
                    setPathWindow.Owner = Window.GetWindow(this);
                    setPathWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ارسال موجودیت‌های انتخاب شده برای آماده‌سازی ساخت لینک
        /// </summary>
        private void PrepareCreateLink()
        {
            try
            {
                ((MappingViewModel)DataContext).PrepareCreateLink(
                    mappingViewModel?.Map?.SelectedObjects[0],
                    mappingViewModel?.Map?.SelectedObjects[1]);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddLink()
        {
            try
            {
                ((MappingViewModel)DataContext).AddNewLink();

            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// حذف موجودیت‌ها و ارتباطات انتخاب شده
        /// </summary>
        private void RemoveSelectedItems()
        {
            try
            {
                List<ObjectMapModel> objects = null;
                if (mappingViewModel?.Map?.SelectedObjects != null)
                    objects = new List<ObjectMapModel>(mappingViewModel?.Map?.SelectedObjects);

                List<RelationshipMapModel> relationships = null;
                if (mappingViewModel?.Map?.SelectedRelationships != null)
                    relationships = new List<RelationshipMapModel>(mappingViewModel?.Map?.SelectedRelationships);

                mappingViewModel?.RemoveSelectedItems(objects, relationships);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAll()
        {
            if (((MappingViewModel)DataContext).Map.ObjectCollection.Any())
            {
                var result = KWMessageBox.Show(Properties.Resources.String_ClearAllMapping,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            try
            {
                ((MappingViewModel)DataContext).ClearAllItems();

            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
