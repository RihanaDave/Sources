using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.AdminTools.View.Windows;
using GPAS.Dispatch.AdminTools.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.UserControls
{
    public partial class HorizonIndexManagerUserControl : UserControl
    {
        public HorizonIndexManagerUserControl()
        {
            InitializeComponent();
        }

        private void ObjectPickerUserControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            if (((HorizonIndexManagerViewModel)DataContext).AllIndexCollection.Any(p =>
            p.TypeUri.Equals(((Ontology.OntologyNode)e.NewValue).TypeUri)))
            {
                ObjectPickerUserControl.SelectedItem = null;
                return;
            }


            ((HorizonIndexManagerViewModel)DataContext).NewIndex.TypeUri = ((Ontology.OntologyNode)e.NewValue).TypeUri;
            ((HorizonIndexManagerViewModel)DataContext).NewIndex.Properties = new ObservableCollection<HorizonIndexModel>();
        }

        private void PropertyPickerUserControl_SelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddProperty(e);
        }

        private void PropertyPickerUserControl_SelectedItemReselected(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddProperty(e);
        }

        private void AddProperty(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            string propertyType = ((Ontology.OntologyNode)e.NewValue).TypeUri;

            if (((HorizonIndexManagerViewModel)DataContext).NewIndex.Properties.Any(p => p.TypeUri.Equals(propertyType)))
                return;

            HorizonIndexModel newProperty = new HorizonIndexModel()
            {
                TypeUri = propertyType
            };

            ((HorizonIndexManagerViewModel)DataContext).NewIndex.Properties.Add(newProperty);
        }

        private void DeletePropertyButton_Click(object sender, RoutedEventArgs e)
        {
            ((HorizonIndexManagerViewModel)DataContext).NewIndex.Properties.Remove(((Button)sender).DataContext as HorizonIndexModel);
        }

        private void CreateIndexButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewIndex();
        }

        private void IndexDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteIndex(((Button)sender).DataContext as HorizonIndexModel);
        }

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllIndexes();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetAllIndexes();
        }

        public async void GetAllIndexes()
        {
            try
            {
                WaitiningControl.TaskIncrement();
                await ((HorizonIndexManagerViewModel)DataContext).GetAllIndexes();
                PrepareObjectTypeToShow();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.HorizonIndexManager, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                WaitiningControl.TaskDecrement();
            }
        }

        private async void CreateNewIndex()
        {
            try
            {
                WaitiningControl.TaskIncrement();
                await ((HorizonIndexManagerViewModel)DataContext).CreateIndex();
                GetAllIndexes();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.HorizonIndexManager, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                WaitiningControl.TaskDecrement();
            }
        }

        private async void DeleteIndex(HorizonIndexModel index)
        {
            try
            {
                var result = MessageBox.Show(Properties.Resources.String_DeleteHorizonIndex,
                Properties.Resources.HorizonIndexManager,
                MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                WaitiningControl.TaskIncrement();
                await ((HorizonIndexManagerViewModel)DataContext).DeleteIndex(index);
                await ((HorizonIndexManagerViewModel)DataContext).GetAllIndexes();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.HorizonIndexManager, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                WaitiningControl.TaskDecrement();
            }
        }

        private async void DeleteAllIndexes()
        {
            try
            {
                var result = MessageBox.Show(Properties.Resources.String_DeleteAllHorizonIndexes,
                Properties.Resources.HorizonIndexManager,
                MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;

                WaitiningControl.TaskIncrement();
                await ((HorizonIndexManagerViewModel)DataContext).DeleteAllIndexes();
                GetAllIndexes();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.HorizonIndexManager, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                WaitiningControl.TaskDecrement();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditHorizonIndexWindow editHorizonIndexWindow =
                new EditHorizonIndexWindow((HorizonIndexManagerViewModel)DataContext,
                ((Button)sender).DataContext as HorizonIndexModel);

            editHorizonIndexWindow.ShowDialog();
        }

        private void PrepareObjectTypeToShow()
        {
            ObjectPickerUserControl.SelectedItem = null;

            ObservableCollection<string> exceptType = new ObservableCollection<string>(
                ((HorizonIndexManagerViewModel)DataContext).AllIndexCollection.Select(i => i.TypeUri));

            ObjectPickerUserControl.ExceptTypeUriCollection = exceptType;
        }
    }
}
