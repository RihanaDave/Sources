using System;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class ServersStatusViewModel : BaseViewModel
    {
        public ServersStatusViewModel()
        {
            CheckedServers = new ObservableCollection<ServerStatusModel>();
            allExistServers = new List<ServerStatusModel>
            {
                new ServerStatusModel {IsAvailable = false, ServerName = "Repository"},
                new ServerStatusModel {IsAvailable = false, ServerName = "File Repository"},
                new ServerStatusModel {IsAvailable = false, ServerName = "Horizon"},
                new ServerStatusModel {IsAvailable = false, ServerName = "Job"},
                new ServerStatusModel {IsAvailable = false, ServerName = "Search"}
            };
        }

        public ObservableCollection<ServerStatusModel> CheckedServers { get; set; }
        private readonly List<ServerStatusModel> allExistServers;

        public async Task CheckServersStatus()
        {
            await Task.Run(() =>
            {
                CheckingServersStatusProvider serversStatusProvider = new CheckingServersStatusProvider();

                foreach (var server in allExistServers)
                {
                    try
                    {
                        switch (server.ServerName)
                        {
                            case "Repository":
                                serversStatusProvider.CheckRepositoryStatus();
                                break;
                            case "File Repository":
                                serversStatusProvider.CheckFileRepositoryStatus();
                                break;
                            case "Horizon":
                                serversStatusProvider.CheckHorizonStatus();
                                break;
                            case "Job":
                                serversStatusProvider.CheckJobStatus();
                                break;
                            case "Search":
                                serversStatusProvider.CheckSearchStatus();
                                break;
                        }

                        server.IsAvailable = true;
                    }
                    catch (Exception)
                    {
                        server.IsAvailable = false;
                    }
                }
            });

            PrepareServersListToShow();
        }

        private void PrepareServersListToShow()
        {
            if (CheckedServers.Count > 0)
                CheckedServers.Clear();

            foreach (var server in allExistServers)
            {
                CheckedServers.Add(new ServerStatusModel
                {
                    IsAvailable = server.IsAvailable,
                    ServerName = server.ServerName
                });
            }
        }
    }
}
