using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.BranchingHistoryViewer.ViewModel
{
    public class RecomputeEventArgs : EventArgs
    {
        public RecomputeEventArgs(ObjectBase objectBase)
        {
            IsSuccesed = true;
            ObjectBase = objectBase;
        }

        public bool IsSuccesed { get; set; }
        public ObjectBase ObjectBase
        {
            get;
            private set;
        }
    }
}
