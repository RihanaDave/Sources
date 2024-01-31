using GPAS.RepositoryServer.Entities;
using GPAS.RepositoryServer.Entities.Publish;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;

namespace GPAS.RepositoryServer.Logic
{
    public class PublishManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public PublishManager()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[StoragePluginName];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{StoragePluginName}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }

            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        public void Publish(DBAddedConcepts addedConcept, DBModifiedConcepts modifiedConcept, DBResolvedObject[] resolvedObjects, long dataSourceID)
        {
            StorageClient.Publish(addedConcept, modifiedConcept, resolvedObjects, dataSourceID);
        }


        //--------------------------------------------------
        //private static string connctionString;
        //private NpgsqlConnection connection = null;
        //private NpgsqlTransaction transaction = null;

        //public PublishManager()
        //{
        //    connctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //    if (connctionString == null)
        //        throw new ConfigurationErrorsException("Unable to read 'Database Connection String' app setting");
        //}
        //public void Publish(DBAddedConcepts addedConcepts, DBModifiedConcepts modifiedConcepts, DBResolvedObject[] resolvedObjects, long dataSourceID)
        //{
        //    connection = new NpgsqlConnection(connctionString);
        //    try
        //    {
        //        connection.Open();
        //        using (transaction = connection.BeginTransaction())
        //        {
        //            InsertAddedConceptToRepository(addedConcepts,dataSourceID);
        //            UpdateModifiedConceptsInRepository(modifiedConcepts);
        //            ApplyResolutionChanges(resolvedObjects);
        //            transaction.Commit();
        //        }
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //private void ApplyResolutionChanges(DBResolvedObject[] resolvedObjects)
        //{
        //    if (resolvedObjects == null || resolvedObjects.Length == 0)
        //        return;

        //    ObjectManager objectManager = new ObjectManager();
        //    ProperyManager propertyManager = new ProperyManager();
        //    RelationshipManager relationshipManager = new RelationshipManager();
        //    MediaManager mediaManager = new MediaManager();
        //    foreach (DBResolvedObject rObj in resolvedObjects)
        //    {
        //        HashSet<long> reolvedObjIDs = new HashSet<long>(rObj.ResolvedObjectIDs);

        //        objectManager.SetResolveMasterFor(reolvedObjIDs, rObj.ResolutionMasterObjectID, connection, transaction);
        //        propertyManager.ChangePropertiesOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID, rObj.MatchedProperties, connection, transaction);
        //        relationshipManager.ChangeRelationshipsOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID, connection, transaction);
        //        mediaManager.ChangeMediasOwner(reolvedObjIDs, rObj.ResolutionMasterObjectID, connection, transaction);
        //    }
        //}

        //private void UpdateModifiedConceptsInRepository(DBModifiedConcepts modifiedConcept)
        //{
        //    if (modifiedConcept == null)
        //        return;

        //    if (modifiedConcept.ModifiedPropertyList != null)
        //    {
        //        ProperyManager propertyManager = new ProperyManager();
        //        foreach (var modifiedProperty in modifiedConcept.ModifiedPropertyList)
        //        {
        //            propertyManager.EditProperty(modifiedProperty.Id, modifiedProperty.NewValue, connection, transaction);
        //        }
        //    }

        //    if (modifiedConcept.DeletedMediaIDList != null)
        //    {
        //        MediaManager mediaManager = new MediaManager();
        //        mediaManager.DeleteMedias(modifiedConcept.DeletedMediaIDList, connection, transaction);
        //    }
        //}

        //private void InsertAddedConceptToRepository(DBAddedConcepts addedConcept, long dataSourceID)
        //{
        //    if (addedConcept == null)
        //        return;

        //    ObjectManager objectManager = new ObjectManager();
        //    objectManager.AddNewObjects(addedConcept.AddedObjectList, connection, transaction);

        //    ProperyManager propertyManager = new ProperyManager();
        //    propertyManager.AddNewProperties(addedConcept.AddedPropertyList, connection, transaction , dataSourceID);

        //    RelationshipManager relationshipManager = new RelationshipManager();
        //    relationshipManager.AddNewRelationships(addedConcept.AddedRelationshipList, connection, transaction, dataSourceID);

        //    MediaManager mediaManager = new MediaManager();
        //    mediaManager.AddNewMedia(addedConcept.AddedMediaList, connection, transaction, dataSourceID);
        //}
    }
}