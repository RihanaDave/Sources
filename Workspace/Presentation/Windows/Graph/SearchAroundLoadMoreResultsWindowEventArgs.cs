using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public class SearchAroundLoadMoreResultsWindowResultEventArgs : EventArgs
    {
        public SearchAroundLoadMoreResultsWindowResultEventArgs(int result)
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
