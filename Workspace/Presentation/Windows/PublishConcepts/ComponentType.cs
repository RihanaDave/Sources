using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Windows
{
    public enum ComponentType
    {
        [Description("Properties")]
        Property,
        [Description("Links")]
        Link,
        [Description("Medias")]
        Media
    }
}
