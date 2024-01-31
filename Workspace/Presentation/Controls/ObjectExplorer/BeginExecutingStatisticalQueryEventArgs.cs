using System;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer
{
    public class BeginExecutingStatisticalQueryEventArgs : EventArgs
    {
        public string WaitingMessage { get;  }

        public BeginExecutingStatisticalQueryEventArgs(string message)
        {
            WaitingMessage = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
