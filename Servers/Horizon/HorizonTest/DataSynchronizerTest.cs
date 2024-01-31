using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Logic.Synchronization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GPAS.Horizon.Logic.Tests
{
    [TestClass]
    public class DataSynchronizerTest
    {
        // به خاطر وابستگی تست به شناسه‌های ثابت، غیرفعال شد
        //[TestMethod()]
        public async Task SyncPublishChanges_ModifyAnExistProperty_SyncGraphRepositoryWithChanges()
        {
            //Arrenge
            //long[] objectIDList = new long[] { 1288067 };
            ModifiedProperty mProp = new ModifiedProperty { Id = 501196, TypeUri = "قد" };
            //Act
            var indexingProvider = new IndexingProvider();
            await indexingProvider.SynchronizePublishChanges
                (new AddedConcepts()
                {
                    AddedObjects = new List<KObject>(),
                    AddedProperties = new List<KProperty>(),
                    AddedRelationships = new List<RelationshipBaseKlink>(),
                    AddedMedias = new List<KMedia>()
                }
                , new ModifiedConcepts()
                {
                    ModifiedProperties = new List<ModifiedProperty>() { mProp },
                    DeletedMedias = new List<KMedia>()
                }
                , new ResolvedObject[] { }, 1200, false);
            //Assert
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت، غیرفعال شد
        //[TestMethod()]
        public async Task SyncPublishChanges_ModifySomeObjectsAndProperties_SyncGraphRepositoryWithChanges()
        {
            //Arrenge
            List<ModifiedProperty> hpli = new List<ModifiedProperty>();
            hpli.Add(new ModifiedProperty { Id = 4853, TypeUri = "نام" });
            hpli.Add(new ModifiedProperty { Id = 90349, TypeUri = "شماره_تلفن" });
            hpli.Add(new ModifiedProperty { Id = 90352, TypeUri = "مدت_زمان_مکالمه" });

            //Act
            var indexingProvider = new IndexingProvider();
            await indexingProvider.SynchronizePublishChanges
                (new AddedConcepts()
                {
                    AddedObjects = new List<KObject>(),
                    AddedProperties = new List<KProperty>(),
                    AddedRelationships = new List<RelationshipBaseKlink>(),
                    AddedMedias = new List<KMedia>()
                }
                , new ModifiedConcepts()
                {
                    ModifiedProperties = hpli,
                    DeletedMedias = new List<KMedia>()
                }
                , new ResolvedObject[] { }, 1200, false);


            //localhost.jSampleCalService lo = new localhost.jSampleCalService();
            //var xx =lo.addation(8, 8);

            //var result=  Queries.GenerateQueriesBaseOnALLOntologyType();
            //var ssss=Queries.GenerateQueriesBaseOnALLOntologyType();
            //Queries.ExcuteAllQueriesForLoadingData(ssss);
            //  HorizonDAL dal = new HorizonDAL();
            //   var ss= dal.TruncateHorizonTables();
            //  var s =dal.ExcutePerformSearchFilterCommand(" SELECT ObjectID FROM ObjectsTable WHERE ObjectID IN (  (SELECT ObjectID FROM ObjectsTable WHERE DisplayName LIKE N'%اصفهان%' UNION SELECT ObjectID FROM StringPropertiesTable WHERE Value LIKE N'%اصفهان%'))");
        }

        //private static List<AccessControled<RelationshipBaseKlink>> GetSampleRelationship(KObject obj1, KObject obj2, KRelationship rel)
        //{
        //    return new List<AccessControled<RelationshipBaseKlink>>()
        //    {
        //         new AccessControled<RelationshipBaseKlink>()
        //         {
        //             ConceptInstance = new RelationshipBaseKlink() { Source = obj1, Target = obj2, Relationship = rel, TypeURI = "دوست" },
        //             Acl = new ACL()
        //             {
        //                 Classification = "R",
        //                 Permissions =
        //                 {
        //                     new ACI() { GroupName = NativeGroup.Administrators.ToString(), AccessLevel = Permission.Owner },
        //                     new ACI() { GroupName = NativeGroup.EveryOne.ToString(), AccessLevel = Permission.Write }
        //                 }
        //             }
        //         }
        //    };
        //}

        private static List<RelationshipBaseKlink> GetSampleRelationship(KObject obj1, KObject obj2, KRelationship rel)
        {
            return new List<RelationshipBaseKlink>()
            {
                new RelationshipBaseKlink() { Source = obj1, Target = obj2, Relationship = rel, TypeURI = "دوست" }
            };
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت، غیرفعال شد
        //[TestMethod()]
        public async Task SyncPublishChanges_AddRelationshipForAddingObjects_SyncGraphRepositoryWithChanges()
        {
            //Arrenge
            var obj1 = new KObject() { Id = 199912, TypeUri = "شخص" };
            var obj2 = new KObject() { Id = 199913, TypeUri = "شخص" };
            var rel = new KRelationship() { Id = 288881, Direction = LinkDirection.SourceToTarget };

            var addedObj = new List<KObject>() { obj1, obj2 };
            List<RelationshipBaseKlink> addedRel = GetSampleRelationship(obj1, obj2, rel);

            var indexingProvider = new IndexingProvider();

            //Act
            await indexingProvider.SynchronizePublishChanges
                (new AddedConcepts()
                {
                    AddedObjects = addedObj,
                    AddedProperties = new List<KProperty>(),
                    AddedRelationships = addedRel,
                    AddedMedias = new List<KMedia>()
                }
                , new ModifiedConcepts() { ModifiedProperties = new List<ModifiedProperty>(), DeletedMedias = new List<KMedia>() }
                , new ResolvedObject[] { }, 1200, false);
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت، غیرفعال شد
        //[TestMethod()]
        public async Task SyncPublishChanges_AddRelationshipWithExistingSource_SyncGraphRepositoryWithChanges()
        {
            //Arrenge
            var obj1 = new KObject() { Id = 199912, TypeUri = "شخص" };
            var obj2 = new KObject() { Id = 199913, TypeUri = "شخص" };
            var rel = new KRelationship() { Id = 288881, Direction = LinkDirection.SourceToTarget };

            var addedObj = new List<KObject>() { obj2 };
            List<RelationshipBaseKlink> addedRel = GetSampleRelationship(obj1, obj2, rel);

            var indexingProvider = new IndexingProvider();

            //Act
            await indexingProvider.SynchronizePublishChanges
                (new AddedConcepts() { AddedObjects = addedObj, AddedProperties = new List<KProperty>(), AddedRelationships = addedRel, AddedMedias = new List<KMedia>() }
                , new ModifiedConcepts() { ModifiedProperties = new List<ModifiedProperty>(), DeletedMedias = new List<KMedia>() }
                , new ResolvedObject[] { }, 1200, false);
        }

        // به خاطر وابستگی تست به شناسه‌های ثابت، غیرفعال شد
        //[TestMethod()]
        public async Task SyncPublishChanges_AddRelationshipWithExistingEnds_SyncGraphRepositoryWithChanges()
        {
            //Arrenge
            var obj1 = new KObject() { Id = 199912, TypeUri = "شخص" };
            var obj2 = new KObject() { Id = 199913, TypeUri = "شخص" };
            var rel = new KRelationship() { Id = 288881, Direction = LinkDirection.SourceToTarget };

            var addedObj = new List<KObject>() { };
            List<RelationshipBaseKlink> addedRel = GetSampleRelationship(obj1, obj2, rel);

            var indexingProvider = new IndexingProvider();

            //Act
            await indexingProvider.SynchronizePublishChanges
                (new AddedConcepts() { AddedObjects = addedObj, AddedProperties = new List<KProperty>(), AddedRelationships = addedRel, AddedMedias = new List<KMedia>() }
                , new ModifiedConcepts() { ModifiedProperties = new List<ModifiedProperty>(), DeletedMedias = new List<KMedia>() }
                , new ResolvedObject[] { }, 1200, false);
        }
    }
}