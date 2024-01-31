using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.AdminTools.View;
using GPAS.Dispatch.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public static event PropertyChangedEventHandler StaticPropertyChanged;

        protected static void NotifyStaticPropertyChanged([CallerMemberName] string name = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        public static List<DataSourceACL> DataSourceACLs = new List<DataSourceACL>();

        private static SidebarItems currentControl = SidebarItems.Home;
        public static SidebarItems CurrentControl
        {
            get => currentControl;
            set
            {
                currentControl = value;
                NotifyStaticPropertyChanged();
            }
        }

        private long allItemNumber = 1;
        public long AllItemNumber {
            get=> allItemNumber;
            set
            {
                allItemNumber = value;
                OnPropertyChanged();
            }
        }

        public SidebarItems SpecifyEvent(object selectedItem)
        {
            CurrentControl = ((SidebarItem)selectedItem).ItemEvent;
            return CurrentControl;
        }

        private JobModel selectedJobRequest;
        public JobModel SelectedJobRequest
        {
            get => selectedJobRequest;
            set
            {
                selectedJobRequest = value;
                OnPropertyChanged();
            }
        }
    }
}
