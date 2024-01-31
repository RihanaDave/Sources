using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.AdminTools.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Dispatch.AdminTools.View.Windows
{
    public partial class EditHorizonIndexWindow : Window
    {
        private readonly HorizonIndexModel oldIndex;

        public EditHorizonIndexWindow(HorizonIndexManagerViewModel viewModel, HorizonIndexModel selectedIndex)
        {
            InitializeComponent();
            DataContext = viewModel;
            oldIndex = selectedIndex;
            PropertyPickerUserControl.ObjectTypeUriCollection = new ObservableCollection<string>()
            {
                selectedIndex.TypeUri
            };

            ((HorizonIndexManagerViewModel)DataContext).EditedIndex.TypeUri = selectedIndex.TypeUri;

            foreach (HorizonIndexModel property in selectedIndex.Properties)
            {
                ((HorizonIndexManagerViewModel)DataContext).EditedIndex.Properties.Add(property);
            }
        }

        private async void EditIndex()
        {
            try
            {
                WaitiningGrid.Visibility = Visibility.Visible;
                await ((HorizonIndexManagerViewModel)DataContext).EditIndex(oldIndex);
                await ((HorizonIndexManagerViewModel)DataContext).GetAllIndexes();
                CloseWindow();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Properties.Resources.HorizonIndexManager, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                WaitiningGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            EditIndex();
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

            if (((HorizonIndexManagerViewModel)DataContext).EditedIndex.Properties.Any(p => p.TypeUri.Equals(propertyType)))
                return;

            HorizonIndexModel newProperty = new HorizonIndexModel()
            {
                TypeUri = propertyType
            };

            ((HorizonIndexManagerViewModel)DataContext).EditedIndex.Properties.Add(newProperty);
        }

        private void DeletePropertyButton_Click(object sender, RoutedEventArgs e)
        {
            ((HorizonIndexManagerViewModel)DataContext).EditedIndex.Properties.
                Remove(((Button)sender).DataContext as HorizonIndexModel);
        }

        private void CloseWindow()
        {
            Close();
            ((HorizonIndexManagerViewModel)DataContext).EditedIndex.Reset();
        }
    }
}
