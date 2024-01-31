using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventsArgs;
using System;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias
{
    public interface IBaseFilterSearchControl
    {
        event EventHandler RemoveButtonClicked;

        event EventHandler<ValidationStatusChangedEventArgs> ValidationStatusChanged;

        ValidationStatus ValidationStatus { get; set; }

        string ValidationMessage { get; set; }

        ContainerFilterSearchControl ParentContainerControl { get; set; }

        CriteriaBase CriteriaBase { get; set; }
    }
}
