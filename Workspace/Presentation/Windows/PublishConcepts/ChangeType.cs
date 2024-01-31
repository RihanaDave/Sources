using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Windows
{
    public enum ChangeType
    {
        [Description("Added")]
        Added,
        [Description("Changed")]
        Changed,
        [Description("Deleted")]
        Deleted
    }
}
