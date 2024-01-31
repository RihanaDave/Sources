using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GPAS.Workspace.Logic
{
    public class StatisticalQueryProvider
    {
        public static readonly int PassObjectsCountLimit = int.Parse(ConfigurationManager.AppSettings["ObjectExplorerPassObjectsCountLimit"]);
        public async Task<QueryResult> RunQuery(Query query)
        {
            byte[] queryByteArray = SerializeQuery(query);
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.RunStatisticalQueryAsync(queryByteArray);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<GPAS.StatisticalQuery.ResultNode.PropertyBarValues> RetrievePropertyBarValuesStatistics
            (Query query, string exploredPropertyTypeUri, int bucketCount, double minValue, double maxValue)
        {
            byte[] queryByteArray = SerializeQuery(query);
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.RetrievePropertyBarValuesStatisticsAsync(queryByteArray, exploredPropertyTypeUri, bucketCount, minValue, maxValue);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<List<KWObject>> RetrieveObjectsByQuery(Query query)
        {
            byte[] queryByteArray = SerializeQuery(query);
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                long[] retrivedIDs = null;
                if (query.FormulaSequence.Last() is TypeBasedDrillDown
                 || query.FormulaSequence.Last() is PropertyValueBasedDrillDown
                 || query.FormulaSequence.Last() is PropertyValueRangeDrillDown)
                {
                    retrivedIDs = await remoteServiceClient.RetrieveObjectIDsByStatisticalQueryAsync(queryByteArray, PassObjectsCountLimit);
                }
                else if (query.FormulaSequence.Last() is LinkBasedDrillDown)
                {
                    retrivedIDs = await remoteServiceClient.RetrieveLinkedObjectIDsByStatisticalQueryAsync(queryByteArray, PassObjectsCountLimit);
                }
                else
                {
                    throw new NotSupportedException();
                }

                return (await ObjectManager.RetriveObjectsAsync(retrivedIDs)).ToList();
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        private static byte[] SerializeQuery(Query query)
        {
            MemoryStream streamWriter = new MemoryStream();
            QuerySerializer querySerializer = new QuerySerializer();
            querySerializer.Serialize(query, streamWriter);
            StreamUtility streamUtil = new StreamUtility();
            byte[] queryByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);
            return queryByteArray;
        }

        public async Task<PropertyValueStatistics> RetrivePropertyValueStatistics
            (Query query, string exploredPropertyTypeUri, int resultsLimit, long minimumCount, int startOffset = 0)
        {
            byte[] queryByteArray = SerializeQuery(query);
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.RetrievePropertyValueStatisticsAsync
                    (queryByteArray, exploredPropertyTypeUri, startOffset, resultsLimit, minimumCount);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }

        public async Task<LinkTypeStatistics> RetriveLinkTypeStatistics
            (Query query)
        {
            byte[] queryByteArray = SerializeQuery(query);
            WorkspaceServiceClient remoteServiceClient = null;
            try
            {
                remoteServiceClient = RemoteServiceClientFactory.GetNewClient();
                return await remoteServiceClient.RetrieveLinkTypeStatisticsAsync(queryByteArray);
            }
            finally
            {
                if (remoteServiceClient != null)
                    remoteServiceClient.Close();
            }
        }
    }
}
