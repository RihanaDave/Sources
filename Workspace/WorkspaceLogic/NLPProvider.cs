using GPAS.Dispatch.Entities.NLP;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace GPAS.Workspace.Logic
{
    public class NLPProvider
    {
        public async Task<string> GetDocumentPlaneTextAsync(long docID)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.GetDocumentPlaneTextAsync(docID);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
        public async Task<TagCloudKeyPhrase[]> GetTagCloudAsync(string content)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.GetTagCloudAsync(content);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
        public async Task<TagCloudKeyPhrase[]> GetLanguageTagCloudAsync(string content, Language language)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.GetLanguageTagCloudAsync(content,language);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
        public async Task<string[]> GetSummarizationdAsync(Dispatch.Entities.NLP.SummarizationRequest summarizationRequest)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.GetSummarizeAsync(summarizationRequest);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<DetectedLanguage[]> DetectLanguageAsync(string content)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.DetectLanguageAsync(content);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<string[]> GetLanguageSummarizationdAsync(SummarizationRequest summarizationRequest, Language lang)
        {
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.GetLanguageSummarizeAsync(summarizationRequest,lang);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
    }
}