using GPAS.AccessControl;
using GPAS.Dispatch.Entities;
//using GPAS.Dispatch.ServiceAccess.RepositoryService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    public class UserAccountControlProvider
    {
        public void RegisterNewDataSourceToRepositoryServer(long dsId, string name, DataSourceType type, ACL acl, string description, string createdBy, string createdTime)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {
                //proxy = new ServiceClient();
                //proxy.RegisterNewDataSource(dsId, name, type, acl, description, createdBy, createdTime);

                serviceClient = new ServiceAccess.SearchService.ServiceClient();
                serviceClient.RegisterNewDataSource(dsId, name, type, acl, description, createdBy, createdTime);
            }
            finally
            {
                //if (proxy != null)
                    //proxy.Close();

                if (serviceClient != null)
                    serviceClient.Close();
            }
        }

        public void SynchronizeNewDataSourceInSearchServer(long dsId, string name, DataSourceType type, ACL acl, string description, string createdBy, string createdTime)
        {
            DataSourceInfo dataSourceInfo = new DataSourceInfo()
            {
                Id = dsId,
                Name = name,
                Type = (byte)type,
                Description = description,
                CreatedBy = createdBy,
                CreatedTime = createdTime,
                Acl = acl
            };

            PublishProvider publishProvider = new PublishProvider();
            publishProvider.SynchronizeDataSourceInSearchServer(dataSourceInfo);
        }

        public Tuple<long[], long[]> GetReadableSubsetOfConcepts(long[] objIDs, long[] relationshipIDs, string[] groupNames)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient proxy = null;
            try
            {

                //proxy = new ServiceClient();
                proxy = new ServiceAccess.SearchService.ServiceClient();

                long[] readableObjects = proxy.GetSubsetOfConceptsByPermission(ServiceAccess.SearchService.ConceptType.Object, objIDs, groupNames, Permission.Read);
                long[] readableRelationships = proxy.GetSubsetOfConceptsByPermission(ServiceAccess.SearchService.ConceptType.Relationship, relationshipIDs, groupNames, Permission.Read);

                return new Tuple<long[], long[]>(readableObjects, readableRelationships);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public static AuthorizationParametters GetUserAuthorizationParametters(string userName)
        {
            DataAccess.ClassificationBasedGroupPermissionsDataAccess classificationBasedGroupPermissionsDataAccess = new DataAccess.ClassificationBasedGroupPermissionsDataAccess();
            return classificationBasedGroupPermissionsDataAccess.GetUserAuthorizationParametters(userName);
        }

        public List<DataSourceACL> RetriveDataSourceACLs(long[] dataSourceIDs)
        {
            //ServiceClient proxy = null;
            GPAS.Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = null;
            try
            {

                //proxy = new ServiceClient();
                serviceClient = new ServiceAccess.SearchService.ServiceClient();

                //var dbDataSourceAclList = proxy.RetrieveDataSourceACLs(dataSourceIDs).ToList();
                ServiceAccess.SearchService.SearchDataSourceACL[] result= serviceClient.RetrieveDataSourceACLs(dataSourceIDs);


                List<DataSourceACL> dataSourceAclList = new List<DataSourceACL>();

                foreach (var item in result)
                {
                    DataSourceACL dataSourceACL = new DataSourceACL()
                    {
                         Id = item.Id,
                          Acl = item.Acl
                    };

                    dataSourceAclList.Add(dataSourceACL);
                }

                
                //foreach (var dbDataSourceAcl in dbDataSourceAclList)
                //{
                //    dataSourceAclList.Add(new DataSourceACL()
                //    {
                //        Acl = dbDataSourceAcl.Acl,
                //        Id = dbDataSourceAcl.Id
                //    });
                //}

                return dataSourceAclList;

            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
        }
    }
}
