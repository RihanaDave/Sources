using GPAS.Dispatch.Entities.DatalakeEntities;
using GPAS.Dispatch.ServiceAccess.DataLakeService;
using System.Collections.Generic;

namespace GPAS.Dispatch.Logic.Datalake
{
    public class DataLakeProvider
    {        
        public string[] DownloadFileFromDataLake(string category, string dateTime, List<SearchCriteria> searchCriterias)
        {
            datalakeQuery query = GenerateDrillQuery(category, dateTime, searchCriterias);
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                datalakeQuery d = new datalakeQuery();
                var resultFile = proxy.GetDatalakeSlice(query);
                return resultFile.Split('\n');
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public void StopListeningToSpecificPortNumber(StreamingIngestion streamingIngestion)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                proxy.StopStreamingIngestionAsync(ConvertToServerSideStreamingIngestion(streamingIngestion));
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public string[] GetPreviewDataRelatedToSelectedCategoryAndDateTime(string category, string dateTime)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                var jobsStatus = proxy.GetPreviewData(category, dateTime);
                return jobsStatus;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
        private streamingIngestion ConvertToServerSideStreamingIngestion(StreamingIngestion streamingIngestion)
        {
            streamingIngestion result = new ServiceAccess.DataLakeService.streamingIngestion()
            {
                Category = streamingIngestion.Category,
                dataFlowDateTime = streamingIngestion.RelatedDateTime.ToString(),
                Headers = streamingIngestion.Headers,
                id = streamingIngestion.id.ToString(),
                InputPortNumber = streamingIngestion.InputPort,
                Separator = CovertToServerSideSeparator(streamingIngestion.StreamingDataSeparator), 
                startTime = streamingIngestion.startTime.ToString()
            };
            return result;
        }

        private string CovertToServerSideSeparator(FileSeparator streamingDataSeparator)
        {
            string result = string.Empty;
            switch (streamingDataSeparator)
            {
                case FileSeparator.Tab:
                    result = "Tab";
                    break;
                case FileSeparator.Comma:
                    result = "Comma";
                    break;
                case FileSeparator.Pipe:
                    result = "Pipe";
                    break;
                case FileSeparator.Sharp:
                    result = "Sharp";
                    break;
                case FileSeparator.Slash:
                    result = "Slash";
                    break;
                default:
                    break;
            }
            return result;
        }
        public void StartListeningToSpecificPortNumber(StreamingIngestion streamingIngestion)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                proxy.StartStreamingIngestion(ConvertToServerSideStreamingIngestion(streamingIngestion));                
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }            
        }        

        private datalakeQuery GenerateDrillQuery(string dlCategory, string dlDateTime, List<SearchCriteria> dlSearchCriterias)
        {
            List<searchCriteria> drillSearchCriteria = new List<searchCriteria>();

            foreach (var currentDLSearchCriteria in dlSearchCriterias)
            {
                searchCriteria tt = new searchCriteria();
                drillSearchCriteria.Add(new searchCriteria()
                {
                    Type = currentDLSearchCriteria.Type,
                    Value = currentDLSearchCriteria.Value,
                    Comparator = ConvertFromDLComparatorTypeToDrillComparatorType(currentDLSearchCriteria.Comparator),
                    CriteriaDataType = ConvertFrolDLCriteriaDataTypeToDrillCriteriaDataType(currentDLSearchCriteria.CriteriaDataType)

                });
            }

            datalakeQuery result = new datalakeQuery()
            {
                Category = dlCategory,
                DateTime = dlDateTime,
                SearchCriterias = drillSearchCriteria.ToArray()
            };

            return result;
        }

        private string ConvertFrolDLCriteriaDataTypeToDrillCriteriaDataType(BaseDataType criteriaDataType)
        {
            string result = string.Empty;

            switch (criteriaDataType)
            {
                case BaseDataType.Integer:
                    result = "Integer";
                    break;
                case BaseDataType.Double:
                    result = "Double";
                    break;
                case BaseDataType.Date:
                    result = "Date";
                    break;
                case BaseDataType.String:
                    result = "String";
                    break;
                default:
                    break;
            }
            return result;
        }

        private string ConvertFromDLComparatorTypeToDrillComparatorType(ComparatorType comparator)
        {
            string result = string.Empty;

            switch (comparator)
            {
                case ComparatorType.Equal:
                    result = "Equal";
                    break;
                case ComparatorType.Like:
                    result = "Like";
                    break;
                case ComparatorType.LessThan:
                    result = "LessThan";
                    break;
                case ComparatorType.greatorThan:
                    result = "GreatorThan";
                    break;
                default:
                    break;
            }
            return result;
        }

        public string[] GetHeaders(string category, string dateTime)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                var headers = proxy.GetHeaders(category, dateTime);
                return headers;
            }            
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }        
    }
}
