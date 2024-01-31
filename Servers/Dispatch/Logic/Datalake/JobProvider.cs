using GPAS.Dispatch.ServiceAccess.DataLakeService;
using GPAS.Dispatch.Entities.DatalakeEntities;

namespace GPAS.Dispatch.Logic.Datalake
{
    public class JobProvider
    {
        public string[] GetJobsStatus()
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                var jobsStatus = proxy.GetJobsStatus();
                return jobsStatus;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }        

        public void InsertFileIngestionJobStatus(IngestionFile ingestingFile)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                proxy.InsertFileIngestionJobStatus(ConvertToServerSideIngestionFile(ingestingFile));
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        private ingestionFile ConvertToServerSideIngestionFile(IngestionFile ingestionFile)
        {
            ingestionFile result = new ingestionFile()
            {
                id = ingestionFile.id.ToString(),
                Category = ingestionFile.Category,
                DataFlowDateTime = ingestionFile.DataFlowDateTime.ToString(),
                FilePath = ingestionFile.FilePath,
                FileSeparator = ConvertToServerSideFileSeparator(ingestionFile.FileSeparator),
                Headers = ingestionFile.Headers,
                TimeBegin = ingestionFile.TimeBegin.ToString()
            };
            return result;
        }

        private string ConvertToServerSideFileSeparator(FileSeparator fileSeparator)
        {
            string result = string.Empty;
            switch (fileSeparator)
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

        public bool IsListenProcessorExist(StreamingIngestion streamingIngestion)
        {
            bool result = false;
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                result =  proxy.IsListenProcessorExist(ConvertToServerSideStreamingIngestion(streamingIngestion));
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
            return result;
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

        public void InsertStreamIngestionStartStatus(StreamingIngestion streamingIngestion)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                proxy.InsertStreamIngestionStartStatus(ConvertToServerSideStreamingIngestion(streamingIngestion));
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public void InsertStreamIngestionStopStatus(StreamingIngestion streamingIngestion)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                proxy.InsertStreamIngestionStopStatus(ConvertToServerSideStreamingIngestion(streamingIngestion));
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public string[] GetStreamJobsStatus()
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();
                var jobsStatus = proxy.GetStreamJobsStatus();
                return jobsStatus;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}
