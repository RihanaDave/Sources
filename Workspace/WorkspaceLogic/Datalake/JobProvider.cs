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
    public class JobProvider
    {
        public async static Task<string[][]> GetFileIngestionJobsRequestAsync()
        {
            WorkspaceServiceClient sc = null;
            List<string[]> totalFields = new List<string[]>();
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                List<string> serverSideJobRequest = (await sc.GetJobsStatusAsync()).ToList();
                for (int i = 0; i < serverSideJobRequest.Count; i++)
                {
                    string[] separatedLine = serverSideJobRequest.ElementAt(i).Split(',');
                    totalFields.Add(separatedLine);
                }                
                return totalFields.ToArray();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        public async static Task<string[][]> GetStreamJobsAsync()
        {
            WorkspaceServiceClient sc = null;
            List<string[]> totalFields = new List<string[]>();
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                List<string> serverSideStreamJobRequest = (await sc.GetStreamJobsStatusAsync()).ToList();
                for (int i = 0; i < serverSideStreamJobRequest.Count; i++)
                {
                    string[] separatedLine = serverSideStreamJobRequest.ElementAt(i).Split(',');
                    totalFields.Add(separatedLine);
                }
                return totalFields.ToArray();
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        
        public async static Task InsertStreamIngestionStartStatus(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            string query = string.Empty;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                await sc.InsertStreamIngestionStartStatusAsync(ConvertToServerSideStreamingIngestion(streamingIngestion));
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        private static ServiceAccess.RemoteService.StreamingIngestion ConvertToServerSideStreamingIngestion(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            ServiceAccess.RemoteService.StreamingIngestion result = new ServiceAccess.RemoteService.StreamingIngestion()
            {
                Category = streamingIngestion.Category,
                Headers = streamingIngestion.Headers,
                id = streamingIngestion.id,
                InputPort = streamingIngestion.InputPort,
                RelatedDateTime = streamingIngestion.RelatedDateTime,
                startTime = streamingIngestion.startTime,
                StreamingDataSeparator = ConvertToServerSideFileSeparator(streamingIngestion.StreamingDataSeparator)
            };

            return result;
        }

        public async static Task InsertStreamIngestionStopStatus(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            string query = string.Empty;
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                await sc.InsertStreamIngestionStopStatusAsync(ConvertToServerSideStreamingIngestion(streamingIngestion));
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        public async static Task InsertFileIngestionJobStatus(Entities.Datalake.IngestionFile ingestingFile)
        {
            string query = string.Empty;            
            WorkspaceServiceClient sc = null;
            try
            {
                sc = RemoteServiceClientFactory.GetNewClient();
                await sc.InsertFileIngestionJobStatusAsync(ConvertToServerSideIngestionFile(ingestingFile));
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }

        private static ServiceAccess.RemoteService.IngestionFile ConvertToServerSideIngestionFile(Entities.Datalake.IngestionFile ingestionFile)
        {
            ServiceAccess.RemoteService.IngestionFile result = new ServiceAccess.RemoteService.IngestionFile()
            {
                Category = ingestionFile.Category,
                DataFlowDateTime = ingestionFile.DataFlowDateTime,
                FilePath = ingestionFile.FilePath,
                Headers = ingestionFile.Headers,
                id = ingestionFile.id,
                TimeBegin = ingestionFile.TimeBegin,
                FileSeparator = ConvertToServerSideFileSeparator(ingestionFile.FileSeparator)
            };
            return result;
        }

        private static ServiceAccess.RemoteService.FileSeparator ConvertToServerSideFileSeparator(Entities.Datalake.FileSeparator fileSeparator)
        {
            ServiceAccess.RemoteService.FileSeparator result = ServiceAccess.RemoteService.FileSeparator.Comma;
            switch (fileSeparator)
            {
                case Entities.Datalake.FileSeparator.Tab:
                    result = ServiceAccess.RemoteService.FileSeparator.Tab;
                    break;
                case Entities.Datalake.FileSeparator.Comma:
                    result = ServiceAccess.RemoteService.FileSeparator.Comma;
                    break;
                case Entities.Datalake.FileSeparator.Pipe:
                    result = ServiceAccess.RemoteService.FileSeparator.Pipe;
                    break;
                case Entities.Datalake.FileSeparator.Sharp:
                    result = ServiceAccess.RemoteService.FileSeparator.Sharp;
                    break;
                case Entities.Datalake.FileSeparator.Slash:
                    result = ServiceAccess.RemoteService.FileSeparator.Slash;
                    break;
                default:
                    break;
            }
            return result;
        }                
    }   

    public enum JobRequestStatusWS
    {
        Pending,
        Busy,
        Timeout,
        Terminated,
        Failed,
        Success
    }

    public enum JobRequestTypeWS
    {
        FileIngestion,
        StreamingIngestion
    }
}
