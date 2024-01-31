using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Dispatch.Entities.NLP;
using GPAS.Dispatch.ServiceAccess;
using System.Configuration;
using GPAS.Dispatch.Entities.NLP.Summarization;

namespace GPAS.Dispatch.Logic
{
    public class NLPProvider
    {
        static readonly int MaxNumberOfTagCloudKeyPhrases = int.Parse(ConfigurationManager.AppSettings["MaxNumberOfTagCloudKeyPhrases"]);
        public string GetDocumentPlaneText(long docID)
        {
            ServiceAccess.SearchService.ServiceClient searchServerClient = new ServiceAccess.SearchService.ServiceClient();
            return searchServerClient.GetDocumentPossibleExtractedContent(docID);
        }

        public List<DetectedLanguage> DetectLanguage(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            NlpServiceClient proxy = new NlpServiceClient();
            List<DetectedLanguage> detectedLanguages = proxy.DetectLanguage(content);
            return detectedLanguages;
        }
        public TagCloudKeyPhrase[] GetTagCloud(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            NlpServiceClient proxy = new NlpServiceClient();
            List<TagCloudKeyPhrase> tagCloudKeyPhrase = proxy.GetTagCloud(content, MaxNumberOfTagCloudKeyPhrases);
            return tagCloudKeyPhrase.ToArray();
        }
        public TagCloudKeyPhrase[] GetLanguageTagCluod(string content, Language language)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            NlpServiceClient proxy = new NlpServiceClient();
            List<TagCloudKeyPhrase> tagCloudKeyPhrase = proxy.GetLanguageTagCloud(content, MaxNumberOfTagCloudKeyPhrases, language);
            return tagCloudKeyPhrase.ToArray();
        }
        public List<string> GetSummarize(SummarizationRequest summarizationRequest)
        {
            if (summarizationRequest.Content == null)
            {
                throw new ArgumentNullException(nameof(summarizationRequest.Content));
            }

            NlpServiceClient proxy = new NlpServiceClient();
            if (summarizationRequest.Rate.RateType == SummarizationRateType.Paragraph)
            {
                return proxy.GetSummarize(summarizationRequest.Content, summarizationRequest.Rate.RateValue);
            }
            else
            {
                return proxy.GetSummarize(summarizationRequest.Content, (summarizationRequest.Rate.RateValue / 100));

            }
        }

        public List<string> GetLanguageSummarize(SummarizationRequest summarizationRequest, Language lang)
        {
            if (summarizationRequest.Content == null)
            {
                throw new ArgumentNullException(nameof(summarizationRequest.Content));
            }
            NlpServiceClient proxy = new NlpServiceClient();
            if (summarizationRequest.Rate.RateType == SummarizationRateType.Paragraph)
            {
                return proxy.GetLanguageSummarize(summarizationRequest.Content, summarizationRequest.Rate.RateValue, lang);
            }
            else
            {
                return proxy.GetLanguageSummarize(summarizationRequest.Content, (summarizationRequest.Rate.RateValue / 100), lang);

            }
        }
    }
}
