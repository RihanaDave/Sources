using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet
{
    public class RecomputeSetEventArgs : EventArgs
    {
        public RecomputeSetEventArgs()
        {
            IsSuccesed = true;
        }

        public bool IsSuccesed { get; set; }
    }
}
