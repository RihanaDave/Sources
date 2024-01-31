using GPAS.AccessControl;
using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.Publish;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace GPAS.JobServer.Logic.Import
{
    public class PublishManager
    {
        private static readonly int publishMaximumRetryTimes = int.Parse(ConfigurationManager.AppSettings["PublishMaximumRetryTimes"]);
        private static readonly int publishMinimumIntervalBetweenLogsInSeconds = int.Parse(ConfigurationManager.AppSettings["MinimumIntervalBetwweenIterrativeLogsReportInSeconds"]);
        private static readonly bool reportFullDetailsInLog = bool.Parse(ConfigurationManager.AppSettings["ReportFullDetailsInImportLog"]);
        private static readonly double publishAcceptableFailsPercentage = double.Parse(ConfigurationManager.AppSettings["PublishAcceptableFailsPercentage"]);
        private static readonly int maximumNumberOfGlobalResolutionCandidates = int.Parse(ConfigurationManager.AppSettings["MaxNumberOfGlobalResolutionCandidates"]);

        // This event report last published object index
        public static event EventHandler<PublishedEventArgs> ImportingObjectBatchPublished;

        protected static void OnImportingObjectBatchPublished(int lastIndexOfImportingObj, int totalImportingObjects)
        {
            if (lastIndexOfImportingObj < 0)
                throw new ArgumentNullException(nameof(lastIndexOfImportingObj));
            if (totalImportingObjects < 0)
                throw new ArgumentNullException(nameof(lastIndexOfImportingObj));

            ImportingObjectBatchPublished?.Invoke(null, new PublishedEventArgs(lastIndexOfImportingObj, totalImportingObjects));
        }

        // This event report last published relationship index
        public static event EventHandler<PublishedEventArgs> ImportingRelationBatchPublished;

        protected static void OnImportingRelationBatchPublished(int lastIndexOfImportingRelation, int totalImportingRelations)
        {
            if (lastIndexOfImportingRelation < 0)
                throw new ArgumentNullException(nameof(lastIndexOfImportingRelation));
            if (totalImportingRelations < 0)
                throw new ArgumentNullException(nameof(totalImportingRelations));
            ImportingRelationBatchPublished?.Invoke(null, new PublishedEventArgs(lastIndexOfImportingRelation, totalImportingRelations));
        }

        public static void GenerateImportingExtractedConcepts
            (List<ImportingObject> importingObjects
            , List<ImportingRelationship> importingRelationships
            , long registeredSemiStructuredDataSourceID
            , ACL dataSourceAcl
            , ProcessLogger logger = null
            , int lastPublishedObjectIndex = -1
            , int lastPublishedRelationIndex = -1)
        {
            ConceptsPublisher publisher = new ConceptsPublisher()
            {
                PublishMaximumRetryTimes = publishMaximumRetryTimes,
                PublishMinimumIntervalBetweenLogsInSeconds = publishMinimumIntervalBetweenLogsInSeconds,
                ReportFullDetailsInLog = reportFullDetailsInLog,
                PublishAcceptableFailsPercentage = publishAcceptableFailsPercentage
            };
            var publishAdaptor = new SemiStructuredDataImport.PublishAdaptor();
            publisher.InitToPublishFromSemiStructuredSource(importingObjects, importingRelationships, publishAdaptor, registeredSemiStructuredDataSourceID, dataSourceAcl, logger);
            publisher.MaximumNumberOfGlobalResolutionCandidates = maximumNumberOfGlobalResolutionCandidates;
            publisher.ImportingObjectBatchPublished += Publisher_ImportingObjectBatchPublished;
            publisher.ImportingRelationBatchPublished += Publisher_ImportingRelationBatchPublished;
            publisher.PublishConceptsInBatchMode(lastPublishedObjectIndex, lastPublishedRelationIndex);
            publisher.ImportingObjectBatchPublished -= Publisher_ImportingObjectBatchPublished;
            publisher.ImportingRelationBatchPublished -= Publisher_ImportingRelationBatchPublished;
        }

        private static void Publisher_ImportingRelationBatchPublished(object sender, PublishedEventArgs e)
        {
            OnImportingRelationBatchPublished(e.LastIndexOfConcept, e.TotalImportingConcept);
        }

        private static void Publisher_ImportingObjectBatchPublished(object sender, PublishedEventArgs e)
        {
            OnImportingObjectBatchPublished(e.LastIndexOfConcept, e.TotalImportingConcept);
        }


        public static long RegisterNewSemiStructuredDataSource
            (DataSourceMetadata dataSource, ProcessLogger logger = null)
        {
            DataSourceRegisterationProvider regProvider = GetNewDSRegProvider(dataSource, logger);
            regProvider.Register();
            return regProvider.DataSourceID;
        }

        public static long RegisterNewDataSourceForSharedFile
            (DataSourceMetadata dataSource, string fileJobSharePath, ProcessLogger logger)
        {
            DataSourceRegisterationProvider regProvider = GetNewDSRegProvider(dataSource, logger);
            regProvider.Register(fileJobSharePath);
            return regProvider.DataSourceID;
        }

        private static DataSourceRegisterationProvider GetNewDSRegProvider(DataSourceMetadata dataSource, ProcessLogger logger)
        {
            var publishAdaptor = new SemiStructuredDataImport.PublishAdaptor();
            var regProvider = new DataSourceRegisterationProvider(dataSource, publishAdaptor, logger);
            regProvider.ProcessMaximumRetryTimes = publishMaximumRetryTimes;
            regProvider.ReportFullDetailsInLog = reportFullDetailsInLog;
            return regProvider;
        }
    }
}
