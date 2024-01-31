using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.Datalake;
using GPAS.Workspace.ServiceAccess;

namespace GPAS.Workspace.Logic.Datalake
{
    public class SearchProvider
    {
        public static async Task<string[]> DownloadFileFromDataLake(string category, DateTime dateTime, List<Entities.Datalake.SearchCriteria> conditions)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            string[] result = null;
            try
            {
                string dateTimeTemp = string.Format("{0}-{1}-{2}", dateTime.Year, dateTime.Month, dateTime.Day);
                result = await sc.GetDatalakeSliceAsync(category, dateTimeTemp, ConvertToServerSideSearchCriteria(conditions).ToArray());
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }

        private static List<ServiceAccess.RemoteService.SearchCriteria> ConvertToServerSideSearchCriteria(List<Entities.Datalake.SearchCriteria> conditions)
        {
            List<ServiceAccess.RemoteService.SearchCriteria> result = new List<ServiceAccess.RemoteService.SearchCriteria>();
            foreach (var currentWSCondition in conditions)
            {
                result.Add(new ServiceAccess.RemoteService.SearchCriteria()
                {
                    Type = currentWSCondition.Type,
                    Value = currentWSCondition.Value,
                    Comparator = ConvertToServerSideComparator(currentWSCondition.Comparator),
                    CriteriaDataType = ConvertToServerSideCriteriaDataType(currentWSCondition.CriteriaDataType)
                });
            }
            return result;
        }

        private static ServiceAccess.RemoteService.BaseDataType ConvertToServerSideCriteriaDataType(Entities.Datalake.BaseDataType criteriaDataType)
        {
            ServiceAccess.RemoteService.BaseDataType result = ServiceAccess.RemoteService.BaseDataType.String;
            switch (criteriaDataType)
            {
                case Entities.Datalake.BaseDataType.Integer:
                    result = ServiceAccess.RemoteService.BaseDataType.Integer;
                    break;
                case Entities.Datalake.BaseDataType.Double:
                    result = ServiceAccess.RemoteService.BaseDataType.Double;
                    break;
                case Entities.Datalake.BaseDataType.Date:
                    result = ServiceAccess.RemoteService.BaseDataType.Date;
                    break;
                case Entities.Datalake.BaseDataType.String:
                    result = ServiceAccess.RemoteService.BaseDataType.String;
                    break;
                default:
                    break;
            }
            return result;

        }

        private static ServiceAccess.RemoteService.ComparatorType ConvertToServerSideComparator(Entities.Datalake.ComparatorType comparator)
        {
            ServiceAccess.RemoteService.ComparatorType result = ServiceAccess.RemoteService.ComparatorType.Equal;
            switch (comparator)
            {
                case Entities.Datalake.ComparatorType.Equal:
                    result = ServiceAccess.RemoteService.ComparatorType.Equal;
                    break;
                case Entities.Datalake.ComparatorType.Like:
                    result = ServiceAccess.RemoteService.ComparatorType.Like;
                    break;
                case Entities.Datalake.ComparatorType.LessThan:
                    result = ServiceAccess.RemoteService.ComparatorType.LessThan;
                    break;
                case Entities.Datalake.ComparatorType.greatorThan:
                    result = ServiceAccess.RemoteService.ComparatorType.greatorThan;
                    break;
                default:
                    break;
            }
            return result;
        }
        
        public static async Task<string[]> GetCategories()
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            string[] result = null;
            try
            {
                result = await sc.GetDatalakeCategoriesAsync("/");
            }
            catch
            {
                throw;
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;

        }

        public static async Task<string[]> GetHeaders(string category, string dateTime)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            string[] result = null;
            try
            {
                result = await sc.GetDatalakeSliceHeadersAsync(category, dateTime);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;

        }

        public async static Task<string[][]> GetPreviewDataRelatedToSelectedCategoryAndDateTimeAsync(string category, string dateTime)
        {
            List<string[]> totalPreviewData = new List<string[]>();


            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            List<string> previewData = null;
            try
            {
                previewData = (await sc.GetPreviewDataFromDatalakeAsync(category, dateTime)).ToList();
                for (int i = 0; i < previewData.Count; i++)
                {
                    string[] separatedLine = previewData.ElementAt(i).Split(',');
                    totalPreviewData.Add(separatedLine);
                }

            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return totalPreviewData.ToArray();
        }

        public static async Task<string[]> GetDataLakeSearchResultAsCsv(string category, DateTime dateTime, List<Entities.Datalake.SearchCriteria> conditions)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            string[] result = null;
            try
            {
                string dateTimeTemp = string.Format("{0}-{1}-{2}", dateTime.Year, dateTime.Month, dateTime.Day);
                result = await sc.GetDatalakeSliceAsync(category, dateTimeTemp, ConvertToServerSideSearchCriteria(conditions).ToArray());
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return result;
        }
    }
}
