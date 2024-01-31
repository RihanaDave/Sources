using GPAS.PropertiesValidation;
using System;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventsArgs
{
    public class ValidationStatusChangedEventArgs : EventArgs
    {
        public ValidationStatusChangedEventArgs(ValidationStatus oldValue, ValidationStatus newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ValidationStatus OldValue { get; protected set; }
        public ValidationStatus NewValue { get; protected set; }
    }
}
