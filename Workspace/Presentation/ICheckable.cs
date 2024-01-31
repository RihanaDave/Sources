using System;

namespace GPAS.Workspace.Presentation
{
    public interface ICheckable
    {
        bool? IsChecked { get; set; }
        event EventHandler IsCheckedChanged;
    }
}
