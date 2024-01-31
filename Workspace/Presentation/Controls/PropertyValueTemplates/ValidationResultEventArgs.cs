using GPAS.PropertiesValidation;
using System;

namespace GPAS.Workspace.Presentation.Controls.PropertyValueTemplates
{
    public class ValidationResultEventArgs : EventArgs
    {
        public ValidationStatus ValidationResult { get; }

        public ValidationResultEventArgs(ValidationStatus validationResult)
        {
            ValidationResult = validationResult;
        }
    }
}
