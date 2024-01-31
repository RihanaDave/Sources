using GPAS.Dispatch.Entities;
using GPAS.Dispatch.ServiceAccess.JobService;
using System.Data;

namespace GPAS.Dispatch.Logic
{
    public class ImportProvider
    {
        #region Import from attached Databases
        public string[] GetUriOfDatabasesForImport()
        {
            ServiceClient serviceClient = null;
            try
            {
                serviceClient = new ServiceClient();
                return serviceClient.GetUriOfDatabasesForImport();
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
        }

        public DataSet GetTablesAndViewsOfDatabaseForImport(string dbForImportURI)
        {
            ServiceClient serviceClient = null;
            try
            {
                serviceClient = new ServiceClient();
                return serviceClient.GetTablesAndViewsPreviewOfDatabaseForImport(dbForImportURI);
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
        }
        #endregion

        public void RegisterNewImportRequests(Entities.SemiStructuredDataImportRequestMetadata[] requestsData)
        {
            var requsetsDataInJobServiceFormat = new ServiceAccess.JobService.SemiStructuredDataImportRequestMetadata[requestsData.Length];
            for (int i = 0; i < requestsData.Length; i++)
            {
                requsetsDataInJobServiceFormat[i] = new ServiceAccess.JobService.SemiStructuredDataImportRequestMetadata()
                {
                    serializedMaterialBase = requestsData[i].serializedMaterialBase,
                    serializedTypeMapping = requestsData[i].serializedTypeMapping,
                    serializedACL = requestsData[i].serializedACL
                };
            }

            ServiceClient serviceClient = null;
            try
            {
                serviceClient = new ServiceClient();
                serviceClient.RegisterNewImportRequests(requsetsDataInJobServiceFormat);
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
        }
    }
}
