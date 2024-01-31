using GPAS.AccessControl.Groups;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Graph;
using GPAS.Horizon.GraphRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;

namespace GPAS.Horizon.Logic
{
    public class GraphRepositoryProvider
    {
        public enum GraphRepositoryBaseDataTypes { Int, Boolean, DateTime, String, Double, HdfsURI, Long, None, GeoTime, GeoPoint };
        private static Ontology.Ontology localOntology = null;
        private static string PluginPath = null;
        private const string GraphRepositoryPluginRelativePath = "GraphRepositoryPluginRelativePath";

        private CompositionContainer compositionContainer;

        public GraphRepositoryProvider()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[GraphRepositoryPluginRelativePath];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{GraphRepositoryPluginRelativePath}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }
            if (localOntology == null)
            {
                localOntology = OntologyProvider.GetOntology();
            }

            // کد ترکیب اسمبلی پلاگین برگرفته از مثال مایکروسافت در آدرس زیر است:
            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        public void Init()
        {
            OntologyMaterial ontologyMaterial = OntologyMaterial.GetOntologyMaterial(localOntology);
            accessClient.Init(ontologyMaterial);
        }

        public IAccessClient GetNewSearchEngineClient()
        {
            return accessClient;
        }


        // از این جا به بعد کدها با نقشی متفاوت پیاده‌سازی شده است
        // این بخش سعی داشته نقش اتصال به مخزن گرافی را از استفاده کننده
        // مخفی کند در حالی که این نقش برای
        // IAccessClient
        // تعریف شده است

        [Import(typeof(IAccessClient))]
        private IAccessClient accessClient = null;

        public void OpenConnection()
        {
            accessClient = GetNewSearchEngineClient();
            accessClient.OpenConnetion();
        }

        public void AddVertices(List<Vertex> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (vertices.Count == 0)
                return;
            accessClient.AddVertices(vertices);
        }

        public void AddVertexProperties(List<VertexProperty> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            if (properties.Count == 0)
                return;
            accessClient.AddVertexProperties(properties);
        }

        public void UpsertVertices(List<Vertex> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (vertices.Count == 0)
                return;
            accessClient.UpsertVertices(vertices);
        }

        public void DeleteVertices(string typerUri, List<long> vertexIDs)
        {
            if (vertexIDs == null)
                throw new ArgumentNullException(nameof(vertexIDs));
            if (vertexIDs.Count == 0)
                return;
            accessClient.DeleteVertices(typerUri, vertexIDs);
        }

        public void AddEdges(List<Edge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));
            if (edges.Count == 0)
                return;
            accessClient.AddEdge(edges);
        }

        public void UpdateEdges(List<long> verticesID, long masterVertexID, string typeUri)
        {
            if (verticesID == null)
                throw new ArgumentNullException(nameof(verticesID));
            if (verticesID.Count == 0)
                return;
            accessClient.UpdateEdge(verticesID, masterVertexID, typeUri);
        }

        public List<VertexProperty> GetVertexPropertiesUnion(List<long> ownerVerticesIDs)
        {
            if (ownerVerticesIDs == null)
                throw new ArgumentNullException(nameof(ownerVerticesIDs));

            return accessClient.GetVertexPropertiesUnion(ownerVerticesIDs);
        }

        public void AddNewGroupPropertiesToEdgeClass(List<string> newGroupsName)
        {
            if (newGroupsName == null)
                throw new ArgumentNullException(nameof(newGroupsName));
            if (newGroupsName.Any(g => !GroupNameValidator.IsGroupNameValid(g)))
                throw new ArgumentException("At least one group name is not valid!");

            if (newGroupsName.Count == 0)
                return;

            accessClient = GetNewSearchEngineClient();
            accessClient.AddNewGroupPropertiesToEdgeClass(newGroupsName);
        }

        public void ApplyChange()
        {
            accessClient.ApplyChanges();
        }

        public List<IndexModel> GetAllIndexes()
        {
            return accessClient.GetAllIndexes();
        }

        public void CreateIndex(IndexModel index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            accessClient.CreateIndex(index);
        }

        public void EditIndex(IndexModel oldIndex, IndexModel newIndex)
        {
            if (oldIndex == null)
                throw new ArgumentNullException(nameof(oldIndex));

            if (newIndex == null)
                throw new ArgumentNullException(nameof(newIndex));

            accessClient.EditIndex(oldIndex, newIndex);
        }

        public void DeleteIndex(IndexModel index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            accessClient.DeleteIndex(index);
        }

        public void DeleteAllIndexes()
        {
            accessClient.DeleteAllIndexes();
        }
    }
}
