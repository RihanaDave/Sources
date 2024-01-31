using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers.Properties
{
    public interface IPropertiesChangeableListener
    {
        void ChangeProperties(PropertiesChangedArgs args);
    }
}
