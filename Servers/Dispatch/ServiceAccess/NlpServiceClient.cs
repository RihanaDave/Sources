using GPAS.Dispatch.Entities.NLP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace GPAS.Dispatch.ServiceAccess
{
    public class NlpServiceClient
    {
        public static readonly string NlpServiceURL = ConfigurationManager.AppSettings["NlpServiceURL"];
        public List<TagCloudKeyPhrase> GetTagCloud(string content, int count)
        {
            List<TagCloudKeyPhrase> tagCloudKeyPhrase = new List<TagCloudKeyPhrase>();
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            string encodedContent = System.Convert.ToBase64String(byteContent);
            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("doc", encodedContent));
            jsonRequest.Add(new JProperty("n_tags", count));

            var client = new RestClient(NlpServiceURL);
            var request = new RestRequest("/getTagCloud", Method.POST);
            request.AddParameter("application/json", jsonRequest.ToString(), ParameterType.RequestBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JObject responseJson;
            try
            {
                responseJson = JObject.Parse(response.Content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse server response;{Environment.NewLine}Response Content:{Environment.NewLine}{response.Content}", ex);
            }
            var keyPhrases = responseJson["keyphrases"];
            foreach (var keyPhrase in keyPhrases.Children())
            {
                tagCloudKeyPhrase.Add(
                    new TagCloudKeyPhrase()
                    {
                        TextOfKeyPhrase = keyPhrase["text"].ToString(),
                        Score = float.Parse(keyPhrase["score"].ToString())
                    }
                    );
            }
            return tagCloudKeyPhrase;
        }

        public List<DetectedLanguage> DetectLanguage(string content)
        {
            List<DetectedLanguage> detectedLanguages = new List<DetectedLanguage>();
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            string encodedContent = System.Convert.ToBase64String(byteContent);
            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("doc", encodedContent));

            var client = new RestClient(NlpServiceURL);
            var request = new RestRequest("/detectLanguage", Method.POST);
            request.AddParameter("application/json", jsonRequest.ToString(), ParameterType.RequestBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JObject responseJson = JObject.Parse(response.Content);
            var languages = responseJson["languages"];
            foreach (var language in languages.Children())
            {
               var t= language["language_name"].ToString();
                int y =(int) double.Parse(language["percent"].ToString());
                detectedLanguages.Add(
                    new DetectedLanguage()
                    {
                        LanguageName = language["language_name"].ToString(),
                        Percent = (int)double.Parse(language["percent"].ToString())
                    }
                    );
            }
            return detectedLanguages;
        }

        public List<string> GetSummarize(string content, double size)
        {
            List<string> summarizeResult = new List<string>();
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            string encodedContent = System.Convert.ToBase64String(byteContent);
            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("doc", encodedContent));
            jsonRequest.Add(new JProperty("size", size));
            var client = new RestClient(NlpServiceURL);
            var request = new RestRequest("/summarize", Method.POST);
            request.AddParameter("application/json", jsonRequest.ToString(), ParameterType.RequestBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JObject responseJson = JObject.Parse(response.Content);
            var summarizes = responseJson["summary"];
            foreach (var summarize in summarizes.Children())
            {
                summarizeResult.Add(summarize.ToString());
            }
            return summarizeResult;
        }

        public List<string> GetLanguageSummarize(string content, double size, Language lang)
        {
            List<string> summarizeResult = new List<string>();
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            string encodedContent = System.Convert.ToBase64String(byteContent);
            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("doc", encodedContent));
            jsonRequest.Add(new JProperty("size", size));
            jsonRequest.Add(new JProperty("lang", lang.ToString()));

            var client = new RestClient(NlpServiceURL);
            var request = new RestRequest("/getLanguageSummary", Method.POST);
            request.AddParameter("application/json", jsonRequest.ToString(), ParameterType.RequestBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JObject responseJson = JObject.Parse(response.Content);
            var summarizes = responseJson["summary"];
            foreach (var summarize in summarizes.Children())
            {
                summarizeResult.Add(summarize.ToString());
            }
            return summarizeResult;
        }

        public List<TagCloudKeyPhrase> GetLanguageTagCloud(string content, int count, Language language)
        {
            List<TagCloudKeyPhrase> tagCloudKeyPhrase = new List<TagCloudKeyPhrase>();
            byte[] byteContent = Encoding.UTF8.GetBytes(content);
            string encodedContent = System.Convert.ToBase64String(byteContent);
            JObject jsonRequest = new JObject();
            jsonRequest.Add(new JProperty("doc", encodedContent));
            jsonRequest.Add(new JProperty("n_tags", count));
            jsonRequest.Add(new JProperty("lang", language.ToString()));
            var client = new RestClient(NlpServiceURL);
            var request = new RestRequest("/getLanguageTagCloud", Method.POST);
            request.AddParameter("application/json", jsonRequest.ToString(), ParameterType.RequestBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            JObject responseJson = JObject.Parse(response.Content);
            var keyPhrases = responseJson["keyphrases"];
            foreach (var keyPhrase in keyPhrases.Children())
            {
                tagCloudKeyPhrase.Add(
                    new TagCloudKeyPhrase()
                    {
                        TextOfKeyPhrase = keyPhrase["text"].ToString(),
                        Score = float.Parse(keyPhrase["score"].ToString())
                    }
                    );
            }
            return tagCloudKeyPhrase;
        }
    }
}
