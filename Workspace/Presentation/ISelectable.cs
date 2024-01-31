using System;

namespace GPAS.Workspace.Presentation
{
    public interface ISelectable
    {
        bool IsSelected { get; set; }
        event EventHandler Selected;
        event EventHandler Deselected;
    }
}
