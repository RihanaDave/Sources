using GPAS.Workspace.ViewModel.ObjectExplorer.Formula;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Windows.ObjectExplorer
{
    public class SetAlgebraDialogResult
    {
        public bool IsDialogCanceled { get; set; }
        public SetAlgebraOperator UserChoice { get; set; }
    }
}
