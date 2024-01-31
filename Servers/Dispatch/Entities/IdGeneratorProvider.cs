using RestSharp;
using System;
using System.Configuration;

namespace GPAS.Dispatch.Entities
{
    public class IdGeneratorProvider
    {
        private static readonly string mainUrl = ConfigurationManager.AppSettings["IdGeneratorApiUrl"];

        public long GetNewId(IdGeneratorItems item)
        {
            return HttpGetMethode(mainUrl + getApiType(item));
        }

        public long GetNewRangeId(IdGeneratorItems item, long range)
        {
            return HttpGetMethode(mainUrl + getApiType(item) + "Range/" + range);
        }

        private string getApiType(IdGeneratorItems item)
        {
            switch (item)
            {
                case IdGeneratorItems.Object:
                    return "NewObjectId";
                case IdGeneratorItems.Property:
                    return "NewPropertyId";
                case IdGeneratorItems.Relation:
                    return "NewRelationId";
                case IdGeneratorItems.Media:
                    return "NewMediaId";
                case IdGeneratorItems.Graph:
                    return  "NewGraphId";
                case IdGeneratorItems.DataSourse:
                    return "NewDataSourceId";
                case IdGeneratorItems.Investigation:
                    return "NewInvestigationId";
                default:
                    throw new NotSupportedException();
            }
        }

        private long HttpGetMethode(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);
            return long.Parse(response.Content);
        }
    }
}
