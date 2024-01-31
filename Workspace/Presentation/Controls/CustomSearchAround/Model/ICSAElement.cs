using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public interface ICSAElement
    {
        bool IsValid { get; set; }
        ObservableCollection<WarningModel> Defections { get; set; }

        event EventHandler IsValidChanged;
        event NotifyCollectionChangedEventHandler DefectionsChanged;
        event EventHandler ScenarioChanged;
    }
}
