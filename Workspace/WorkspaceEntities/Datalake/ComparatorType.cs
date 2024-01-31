using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Datalake
{
    public enum ComparatorType
    {
        [Description("=")]
        Equal,
        [Description("Like")]
        Like,
        [Description("<")]
        LessThan,
        [Description(">")]
        greatorThan
    }
}
