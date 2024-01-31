using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model.UserAccountsControl.Windows.ACL
{
   public class ClickTreeEnableOrDisableCombo:BaseModel
    {
        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
        }
        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
        private int index;
        public int Index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged();
            }
        }
    }
}
