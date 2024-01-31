using System;
using System.ServiceModel.Channels;
using System.Xml.Schema;
using GPAS.Workspace.ServiceAccess.RemoteService;

namespace GPAS.Workspace.ServiceAccess
{
    public static class RemoteServiceClientFactory
    {
        public static string EnteredUsername = string.Empty;
        public static string EnteredPassword = string.Empty;

        public static WorkspaceServiceClient GetNewClient()
        {
            WorkspaceServiceClient sc = new WorkspaceServiceClient();
            sc.ClientCredentials.UserName.UserName = EnteredUsername;
            sc.ClientCredentials.UserName.Password = EnteredPassword;
            return sc;
        }
    }
}
