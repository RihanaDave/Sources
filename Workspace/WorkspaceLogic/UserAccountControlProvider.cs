using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    public class UserAccountControlProvider
    {
        static UserAccountControlProvider()
        {
            Reset();
        }

        public static void Reset()
        {
            ManuallyEnteredDataACL = GetDefaultDataEnteranceACL();
            ImportACL = GetDefaultDataEnteranceACL();
            PublishGraphACL = GetDefaultPublishGraphACL();
        }

        private static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static ACL GetDefaultDataEnteranceACL()
        {
            return new ACL()
            {
                Permissions = new List<ACI>()
                {
                    new ACI() {GroupName = NativeGroup.Administrators.ToString(), AccessLevel = Permission.Owner }
                },
                Classification = Classification.EntriesTree.First.Value.IdentifierString
            };
        }

        private static ACL GetDefaultPublishGraphACL()
        {
            return new ACL()
            {
                Permissions = new List<ACI>()
                {
                    new ACI() {GroupName = NativeGroup.Administrators.ToString(), AccessLevel = Permission.Owner },
                    new ACI() {GroupName = NativeGroup.EveryOne.ToString(), AccessLevel = Permission.Read }
                },
                Classification = Classification.EntriesTree.First.Value.IdentifierString
            };
        }

        public async Task<bool> AuthenticateAsync(string userName, string password)
        {
            string hashedPassword = GetHash(password);
            hashedPassword = Base64Encode(hashedPassword);

            RemoteServiceClientFactory.EnteredUsername = userName;
            RemoteServiceClientFactory.EnteredPassword = hashedPassword;

            WorkspaceServiceClient serviceClient = RemoteServiceClientFactory.GetNewClient();
            return await serviceClient.AuthenticateAsync(userName, hashedPassword);
        }

        public static ACL ImportACL { get; set; }
        public static ACL ManuallyEnteredDataACL { get; set; }
        public static ACL PublishGraphACL { get; set; }

        public string GetLoggedInUserName()
        {
            return RemoteServiceClientFactory.EnteredUsername;
        }
        public Tuple<long[], long[]> GetReadableSubsetOfConcepts(long[] objIDs, long[] relationshipIDs, string[] groupNames)
        {
            WorkspaceServiceClient serviceClient = RemoteServiceClientFactory.GetNewClient();
            return serviceClient.GetReadableSubsetOfConcepts(objIDs, relationshipIDs, groupNames);
        }
        public async Task<GroupClassificationBasedPermission[]> GetClassificationBasedPermissionForGroups(string[] groupNames)
        {
            WorkspaceServiceClient serviceClient = RemoteServiceClientFactory.GetNewClient();
            return (await serviceClient.GetClassificationBasedPermissionForGroupsAsync(groupNames));
        }
        public async Task<List<KWDataSourceACL>> GetDataSourceAcl(long?[] dataSourceIDs)
        {
            List<long> notNullIDs = new List<long>();
            notNullIDs = dataSourceIDs.Where(x => x != null).Select(x => x.Value).ToList();

            WorkspaceServiceClient serviceClient = RemoteServiceClientFactory.GetNewClient();
            List<KWDataSourceACL> dataSourceACLs = (ConvertToKWDataSourceACL(await serviceClient.GetDataSourceACLAsync(notNullIDs.ToArray())));
            if (dataSourceIDs.Contains(null))
            {
                dataSourceACLs.Add(new KWDataSourceACL()
                {
                    Acl = ManuallyEnteredDataACL,
                    Id = 0
                });
            }

            return dataSourceACLs;
        }

        private List<KWDataSourceACL> ConvertToKWDataSourceACL(DataSourceACL[] dataSourceACL)
        {
            List<KWDataSourceACL> result = new List<KWDataSourceACL>();
            foreach (var currentDataSourceACL in dataSourceACL)
            {
                result.Add(new KWDataSourceACL()
                {
                    Id = currentDataSourceACL.Id,
                    Acl = currentDataSourceACL.Acl
                });
            }
            return result;
        }        

        public async Task<bool> HasLoggedInUserUpperWritePermission(ACL aclToCheck)
        {
            bool result = false;
            Logic.AccessControl.GroupManagement groupManagement = new Logic.AccessControl.GroupManagement();
            Logic.UserAccountControlProvider authentication = new Logic.UserAccountControlProvider();
            List<string> groupsName = await groupManagement.GetGroupsOfUser(authentication.GetLoggedInUserName());
            List<string> groupsWithUpperWritePermission = aclToCheck.Permissions.Where(p => (p.AccessLevel == Permission.Owner) || (p.AccessLevel == Permission.Write))
                .Select(g => g.GroupName).ToList();
            foreach (var currentGroup in groupsName)
            {
                if (groupsWithUpperWritePermission.Contains(currentGroup))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public async Task<bool> HasLoggedInUserWritePermissionForPublishGraph()
        {
            bool result = false;
            Logic.AccessControl.GroupManagement groupManagement = new Logic.AccessControl.GroupManagement();
            Logic.UserAccountControlProvider authentication = new Logic.UserAccountControlProvider();
            List<string> groupsName = await groupManagement.GetGroupsOfUser(authentication.GetLoggedInUserName());
            List<string> groupsWithWritePermission = UserAccountControlProvider.PublishGraphACL.Permissions.Where(p => p.AccessLevel == Permission.Write)
                .Select(g => g.GroupName).ToList();
            foreach (var currentGroup in groupsName)
            {
                if (groupsWithWritePermission.Contains(currentGroup))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
