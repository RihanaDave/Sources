using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.OntologyPickers;
using GPAS.Workspace.Presentation.Windows.DataImport;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for TabularMappingSecondStepUserControl.xaml
    /// </summary>
    public partial class TabularMappingSecondStepUserControl
    {
        
        #region Events

        public event EventHandler<EventArgs> PreviousStep;

        protected void OnPreviousStep()
        {
            PreviousStep?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Method

        public TabularMappingSecondStepUserControl()
        {
            InitializeComponent();
        }

        private void PropertyPickerOnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddPropertyToSelectedObject(((PropertyTypePicker)sender).SelectedItem);
        }

        private void PropertyPicker_SelectedItemReselected(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddPropertyToSelectedObject(((PropertyTypePicker)sender).SelectedItem);
        }

        private void AddPropertyToSelectedObject(Ontology.PropertyNode propertyNode)
        {
            if (propertyNode == null)
                return;

            ((MappingViewModel)DataContext).AddPropertyToSelectedObject(propertyNode.TypeUri);
        }

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            SaveMap();
        }

        private void LoadMapButtonOnClick(object sender, RoutedEventArgs e)
        {
            LoadMap();
        }

        /// <summary>
        /// رفتن به مرحله قبل
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousButtonOnClick(object sender, RoutedEventArgs e)
        {
            OnPreviousStep();
        }

        #endregion

        private void EditPathDocument_Click(object sender, RoutedEventArgs e)
        {
            SetPathWindow setPathWindow = new SetPathWindow((MappingViewModel)DataContext,(DocumentMapModel)((Button)sender).DataContext);
            setPathWindow.Owner = Window.GetWindow(this);
            setPathWindow.ShowDialog();
        }
    }
}
