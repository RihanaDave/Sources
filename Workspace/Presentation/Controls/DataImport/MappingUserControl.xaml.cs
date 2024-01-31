using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport
{
    /// <summary>
    /// Interaction logic for MappingUserControl.xaml
    /// </summary>
    public partial class MappingUserControl 
    {
        public MappingUserControl()
        {
            InitializeComponent();
        }

        private void MappingFirstStepUserControlOnNextStep(object sender, EventArgs e)
        {
            ((MappingViewModel)DataContext).CurrentMappingControl = MappingControlType.TabularMappingSecondStep;
        }

        private void MappingSecondStepUserControlOnPreviousStep(object sender, EventArgs e)
        {
            ((MappingViewModel)DataContext).CurrentMappingControl = MappingControlType.TabularMappingFirstStep;
        }
    }
}
