using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public class UnmergeMoreLinksWindowEventArgs : EventArgs
    {
        public UnmergeMoreLinksWindowEventArgs(int result)
        {
            Result = result;
        }

        public int Result
        {
            get;
            private set;
        }
    }
}
