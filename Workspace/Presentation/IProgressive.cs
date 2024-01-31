using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation
{
    public interface IProgressive
    {
        double MaximumProgress { get; set; }
        double ProgressValue { get; set; }
        bool IsIndeterminateProgress { get; set; }
    }
}
