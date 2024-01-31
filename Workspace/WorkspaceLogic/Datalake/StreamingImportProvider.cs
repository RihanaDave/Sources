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
    public class StreamingImportProvider
    {

        public static async Task StartListeningToSpecificPortNumber(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                ServiceAccess.RemoteService.StreamingIngestion serverSideStreamingIngestion = ConvertStreamingIngestionToServerSide(streamingIngestion);                
                await sc.StartStreamingIngestionAsync(serverSideStreamingIngestion);                                               
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        
        private static ServiceAccess.RemoteService.StreamingIngestion ConvertStreamingIngestionToServerSide(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            ServiceAccess.RemoteService.StreamingIngestion result = new ServiceAccess.RemoteService.StreamingIngestion()
            {
                id = streamingIngestion.id,
                Category = streamingIngestion.Category,
                Headers = streamingIngestion.Headers,
                InputPort = streamingIngestion.InputPort,
                RelatedDateTime = streamingIngestion.RelatedDateTime,
                StreamingDataSeparator = ConvertSeparatorToServerSide(streamingIngestion.StreamingDataSeparator),
                startTime = streamingIngestion.startTime
            };

            return result;
        }

        private static ServiceAccess.RemoteService.FileSeparator ConvertSeparatorToServerSide(Entities.Datalake.FileSeparator streamingDataSeparator)
        {
            ServiceAccess.RemoteService.FileSeparator result = ServiceAccess.RemoteService.FileSeparator.Comma;
            switch (streamingDataSeparator)
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
        public static async Task StopListeningToSpecificPortNumber(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                ServiceAccess.RemoteService.StreamingIngestion serverSideStreamingIngestion = ConvertStreamingIngestionToServerSide(streamingIngestion);
                await sc.StopStreamingIngestionAsync(serverSideStreamingIngestion);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
        }
        //public async static Task StopListeningToSpecificPortNumber(StreamingIngestion streamingIngestion)
        //{
        //    WorkspaceServiceClient sc = new RemoteServiceClientFactory.GetNewClient();
        //    try
        //    {
        //        await sc.StopStreamingIngestionAsync();
        //    }
        //    finally
        //    {
        //        if (sc != null)
        //            sc.Close();
        //    }
        //}

        public async static Task<bool> IsListenProcessorExist(Entities.Datalake.StreamingIngestion streamingIngestion)
        {
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {               
                return (await sc.IsListenProcessorExistAsync(ConvertToServerSideStreamingIngestion(streamingIngestion)));
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
                RelatedDateTime = streamingIngestion.RelatedDateTime,
                Headers = streamingIngestion.Headers,
                id = streamingIngestion.id,
                InputPort = streamingIngestion.InputPort,
                StreamingDataSeparator = CovertToServerSideSeparator(streamingIngestion.StreamingDataSeparator)
            };
            return result;
        }

        private static ServiceAccess.RemoteService.FileSeparator CovertToServerSideSeparator(Entities.Datalake.FileSeparator streamingDataSeparator)
        {
            ServiceAccess.RemoteService.FileSeparator result = ServiceAccess.RemoteService.FileSeparator.Comma;
            switch (streamingDataSeparator)
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
}
