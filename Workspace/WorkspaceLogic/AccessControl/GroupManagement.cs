using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.AccessControl
{
    public class GroupManagement
    {
        public async Task<List<GroupInfo>> GetGroups()
        {
            WorkspaceServiceClient client = RemoteServiceClientFactory.GetNewClient();
            List<GroupInfo> result = null;
            try
            {
                result = (await client.GetGroupsAsync()).ToList();
            }
            finally
            {
                if(client!= null)
                    client.Close();
            }

            return result;
        }

        public async Task<List<string>> GetGroupsOfUser(string userName)
        {
            List<string> result = null;
            WorkspaceServiceClient client = RemoteServiceClientFactory.GetNewClient();
            try
            {
                result = (await client.GetGroupsOfUserAsync(userName)).ToList();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
            
            return result;
        }


        public async Task<List<GroupClassificationBasedPermission>> GetClassificationBasedPermissionForGroups(List<string> groupNames)
        {
            List<GroupClassificationBasedPermission> result = null;
            WorkspaceServiceClient client = RemoteServiceClientFactory.GetNewClient();
            try
            {
                result = (await client.GetClassificationBasedPermissionForGroupsAsync(groupNames.ToArray())).ToList();
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
            return result;
        }

    }
}
