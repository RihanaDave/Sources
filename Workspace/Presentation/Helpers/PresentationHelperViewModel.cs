using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Helpers
{
    public class PresentationHelperViewModel : RelayNotifyPropertyChanged
    {
        private bool showWaiting = false;
        public bool ShowWaiting
        {
            get { return showWaiting; }
            set { SetValue(ref showWaiting, value); }
        }

        private string waitingMessage = Properties.Resources.Please_Wait;
        public string WaitingMessage
        {
            get { return waitingMessage; }
            set { SetValue(ref waitingMessage, value); }
        }

        public event UnhandledExceptionEventHandler UnhandledException;
        protected void OnUnhandledException(UnhandledExceptionEventArgs args)
        {
            UnhandledException?.Invoke(this, args);
        }
    }
}
