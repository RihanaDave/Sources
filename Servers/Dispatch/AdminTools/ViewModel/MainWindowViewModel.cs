using System.Collections.ObjectModel;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.AdminTools.View;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public ObservableCollection<SidebarItem> SidebarItems { get; set; }
        public ObservableCollection<SidebarItem> AllSidebarItems { get; set; }

        public MainWindowViewModel()
        {
            SidebarItems = new ObservableCollection<SidebarItem>();

            AllSidebarItems = new ObservableCollection<SidebarItem>
            {
                new SidebarItem
                {
                    ItemIcon = "User",
                    ItemText = "Users management",
                    ItemEvent = View.SidebarItems.UserManager
                },
                new SidebarItem
                {
                    ItemIcon = "Users",
                    ItemText = "Groups management",
                    ItemEvent = View.SidebarItems.GroupManager
                },
                new SidebarItem
                {
                    ItemIcon = "DatabaseRefresh",
                    ItemText = "Synchronization",
                    ItemEvent = View.SidebarItems.IndexesSynchronization
                },
                new SidebarItem
                {
                    ItemIcon = "DatabaseSearch",
                    ItemText = "Object viewer",
                    ItemEvent = View.SidebarItems.ObjectViewer
                },
                new SidebarItem
                {
                    ItemIcon = "DatabaseSettings",
                    ItemText = "Horizon index manager",
                    ItemEvent = View.SidebarItems.HorizonIndexManager
                },
                new SidebarItem
                {
                    ItemIcon = "DatabaseRemove",
                    ItemText = "Remove all data",
                    ItemEvent = View.SidebarItems.RemoveAllData
                },
                new SidebarItem
                {
                    ItemIcon = "FormatListBulleted",
                    ItemText = "job management",
                    ItemEvent = View.SidebarItems.JobManager
                },
                new SidebarItem
                {
                    ItemIcon = "Sync",
                    ItemText = "Ip to geo special",
                    ItemEvent = View.SidebarItems.IpToGeoSpecial
                },
                new SidebarItem
                {
                    ItemIcon = "HelpNetworkOutline",
                    ItemText = "Servers status",
                    ItemEvent = View.SidebarItems.ServersStatus
                }
            };
        }

        public void ShowDataManagerItems()
        {
            if (SidebarItems.Count != 0)
                SidebarItems.Clear();

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Home",
                ItemText = "Home",
                ItemEvent = View.SidebarItems.Home
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "DatabaseRefresh",
                ItemText = "Synchronization",
                ItemEvent = View.SidebarItems.IndexesSynchronization
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "DatabaseSearch",
                ItemText = "Object viewer",
                ItemEvent = View.SidebarItems.ObjectViewer
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "DatabaseSettings",
                ItemText = "Horizon index manager",
                ItemEvent = View.SidebarItems.HorizonIndexManager
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "DatabaseRemove",
                ItemText = "Remove all data",
                ItemEvent = View.SidebarItems.RemoveAllData
            });
        }

        public void ShowUsersManagerItems()
        {
            if (SidebarItems.Count != 0)
                SidebarItems.Clear();

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Home",
                ItemText = "Home",
                ItemEvent = View.SidebarItems.Home
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "User",
                ItemText = "Users management",
                ItemEvent = View.SidebarItems.UserManager
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Users",
                ItemText = "Groups management",
                ItemEvent = View.SidebarItems.GroupManager
            });
        }

        public void ShowJobManagerItems()
        {
            if (SidebarItems.Count != 0)
                SidebarItems.Clear();

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Home",
                ItemText = "Home",
                ItemEvent = View.SidebarItems.Home
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "FormatListBulleted",
                ItemText = "job management",
                ItemEvent = View.SidebarItems.JobManager
            });
        }

        public void ShowIpToGeoItems()
        {
            if (SidebarItems.Count != 0)
                SidebarItems.Clear();

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Home",
                ItemText = "Home",
                ItemEvent = View.SidebarItems.Home
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Sync",
                ItemText = "Ip to geo special",
                ItemEvent = View.SidebarItems.IpToGeoSpecial
            });
        }

        public void ShowServersStatusItems()
        {
            if (SidebarItems.Count != 0)
                SidebarItems.Clear();

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "Home",
                ItemText = "Home",
                ItemEvent = View.SidebarItems.Home
            });

            SidebarItems.Add(new SidebarItem
            {
                ItemIcon = "HelpNetworkOutline",
                ItemText = "Servers status",
                ItemEvent = View.SidebarItems.ServersStatus
            });
        }


    }
}
