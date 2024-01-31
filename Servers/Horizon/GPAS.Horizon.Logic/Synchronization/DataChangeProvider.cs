using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Access.DataClient;
using GPAS.Horizon.Entities;
using GPAS.Horizon.Entities.Graph;
using GPAS.Ontology;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Horizon.Logic.Synchronization
{
    internal class DataChangeProvider
    {
        internal void SynchronizePublishChanges(AddedConceptsWithAcl addedConceptsWithAcl, ModifiedConcepts modifiedConcepts)
        {
            ValidateSynchronizePublishChangesArguments(addedConceptsWithAcl, modifiedConcepts);

            if (!IsAnySynchronizableChangeForHorizon(addedConceptsWithAcl, modifiedConcepts))
            {
                return;
            }
            // Open Connection
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            graphRepositoryProvider.OpenConnection();
            // Added Concepts except relationships
            // prevent to add Objects and their properties has been resolved To other object.
            graphRepositoryProvider.AddVertices(ConvertKObjectToVertex(addedConceptsWithAcl.AddedObjects));
            graphRepositoryProvider.AddVertexProperties(ConvertKPropertyToVertexProperty(addedConceptsWithAcl.AddedProperties.Where(p => IsValidProperty(p.TypeUri, p.Value))));
            // Modified Properties Of Concepts
            graphRepositoryProvider.UpsertVertices(GenerateVerticesToUpdateFromModifiedProperties(modifiedConcepts.ModifiedProperties));
            // Resolved Objects
            // Added Relationships
            graphRepositoryProvider.AddEdges(ConvertRelationshipBaseKlinkToVertexProperty(addedConceptsWithAcl.AddedRelationshipsWithAcl));
            // Commit Changes And Close Connection
            graphRepositoryProvider.ApplyChange();
        }

        private bool IsAnySynchronizableChangeForHorizon(AddedConceptsWithAcl addedConcepts, ModifiedConcepts modifiedConcepts)
        {
            return addedConcepts.AddedObjects.Count > 0
                || addedConcepts.AddedProperties.Count > 0
                || addedConcepts.AddedRelationshipsWithAcl.Count > 0
                || modifiedConcepts.ModifiedProperties.Count > 0;
        }

        private void ValidateSynchronizePublishChangesArguments(AddedConceptsWithAcl addedConcepts, ModifiedConcepts modifiedConcepts)
        {
            if (addedConcepts == null)
                throw new ArgumentNullException(nameof(addedConcepts));
            if (addedConcepts.AddedObjects == null)
                throw new ArgumentNullException(nameof(addedConcepts.AddedObjects));
            if (addedConcepts.AddedProperties == null)
                throw new ArgumentNullException(nameof(addedConcepts.AddedProperties));
            if (addedConcepts.AddedRelationshipsWithAcl == null)
                throw new ArgumentNullException(nameof(addedConcepts.AddedRelationshipsWithAcl));
            if (modifiedConcepts == null)
                throw new ArgumentNullException(nameof(modifiedConcepts));
            if (modifiedConcepts.ModifiedProperties == null)
                throw new ArgumentNullException(nameof(modifiedConcepts.ModifiedProperties));
        }

        private bool IsValidProperty(string propertyTypeUri, string propertyValue)
        {
            BaseDataTypes baseType = OntologyProvider.GetOntology().GetBaseDataTypeOfProperty(propertyTypeUri);
            if (PropertiesValidation.ValueBaseValidation.IsValidPropertyValue(baseType, propertyValue).Status==PropertiesValidation.ValidationStatus.Valid)
            {
                return true;
            }
            else
            {
                return false;
            }                  
        }

        private void SetPropertiesOwnerToMasterVertexAndCleanValue(ref Vertex masterVertex)
        {
            //assign correct ownerID to master vertex properties and clean values
            foreach (var prop in masterVertex.Properties)
            {
                prop.OwnerVertexID = masterVertex.ID;
                prop.OwnerVertexTypeURI = masterVertex.TypeUri;
            }
        }

        private List<VertexProperty> RemoveSameValuePropertiesAndLabelPropery(List<VertexProperty> resolutionCondidatePropertiesUnion)
        {
            string labelProperty = OntologyProvider.GetOntology().GetDefaultDisplayNamePropertyTypeUri();
            List<VertexProperty> distinctProperties = new List<VertexProperty>();
            foreach (var currentVertexProperty in resolutionCondidatePropertiesUnion)
            {
                if (!distinctProperties.Where(dp => dp.TypeUri == currentVertexProperty.TypeUri &&
                dp.Value == currentVertexProperty.Value).Any() && !(currentVertexProperty.TypeUri == labelProperty))
                {
                    distinctProperties.Add(currentVertexProperty);
                }
            }
            return distinctProperties;
        }

        private List<Vertex> GenerateVerticesToUpdateFromModifiedProperties(IEnumerable<ModifiedProperty> modifiedProperties)
        {
            if (modifiedProperties == null)
                throw new ArgumentNullException(nameof(modifiedProperties));
            if (!modifiedProperties.Any())
                return new List<Vertex>();
            RetrieveDataClient retrieveDataClient = new RetrieveDataClient();
            List<KProperty> kProperties = retrieveDataClient.RetrievePropertiesOfObjects(modifiedProperties.Select(mp => mp.OwnerObjectID).ToList());
            List<Vertex> result = new List<Vertex>();
            foreach (IGrouping<long, KProperty> currentProperties in kProperties.GroupBy(p => p.Owner.Id))
            {
                Vertex vertex = new Vertex()
                {
                    ID = currentProperties.Key, // Owner ID
                    TypeUri = currentProperties.FirstOrDefault().Owner.TypeUri,
                    Properties = ConvertDBPropertyToVertexProperty(currentProperties.Where(p => IsValidProperty(p.TypeUri, p.Value)))
                };
                result.Add(vertex);
            }
            return result;
        }

        private List<Vertex> ConvertKObjectToVertex(List<KObject> objects)
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            List<Vertex> result = new List<Vertex>();
            foreach (var currentObject in objects)
            {
                result.Add(new Vertex()
                {
                    ID = currentObject.Id,
                    TypeUri = currentObject.TypeUri,
                    Properties = new List<VertexProperty>()
                });
            }
            return result;
        }

        private List<VertexProperty> ConvertDBPropertyToVertexProperty(IEnumerable<KProperty> DBProperties)
        {
            if (DBProperties == null)
                throw new ArgumentNullException(nameof(DBProperties));

            List<VertexProperty> result = new List<VertexProperty>();
            foreach (var currentDBProperty in DBProperties)
            {
                result.Add(new VertexProperty()
                {
                    TypeUri = currentDBProperty.TypeUri,
                    Value = currentDBProperty.Value,
                    OwnerVertexID = currentDBProperty.Owner.Id,
                    OwnerVertexTypeURI = currentDBProperty.Owner.TypeUri
                });
            }
            return result;
        }

        private List<Edge> ConvertRelationshipBaseKlinkToVertexProperty(List<AccessControled<RelationshipBaseKlink>> relationships)
        {
            if (relationships == null)
                throw new ArgumentNullException(nameof(relationships));

            List<Edge> result = new List<Edge>();
            foreach (var currentRelation in relationships)
            {
                result.Add(new Edge()
                {
                    ID = currentRelation.ConceptInstance.Relationship.Id,
                    Direction = currentRelation.ConceptInstance.Relationship.Direction,
                    TypeUri = currentRelation.ConceptInstance.TypeURI,
                    SourceVertexID = currentRelation.ConceptInstance.Source.Id,
                    TargetVertexID = currentRelation.ConceptInstance.Target.Id,
                    SourceVertexTypeUri = currentRelation.ConceptInstance.Source.TypeUri,
                    TargetVertexTypeUri = currentRelation.ConceptInstance.Target.TypeUri,
                    Acl = currentRelation.Acl
                });
            }
            return result;
        }

        private List<VertexProperty> ConvertKPropertyToVertexProperty(IEnumerable<KProperty> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            List<VertexProperty> result = new List<VertexProperty>();
            foreach (var currentProperty in properties)
            {
                result.Add(new VertexProperty()
                {
                    OwnerVertexID = currentProperty.Owner.Id,
                    OwnerVertexTypeURI = currentProperty.Owner.TypeUri,
                    TypeUri = currentProperty.TypeUri,
                    Value = currentProperty.Value
                });
            }
            return result;
        }

        internal AddedConceptsWithAcl GetAddedConceptsWithAcl(AddedConcepts addedConcepts, long dataSourceID)
        {
            if (dataSourceID == -1)
                throw new Exception("dsid is invalid(-1).");
            RetrieveDataClient retrieveDataClient = new RetrieveDataClient();
            //retrive acl from database
            var temp = new List<long>();
            temp.Add(dataSourceID);
            Dictionary<long, AccessControl.ACL> aclsRetrieved = retrieveDataClient.GetDataSourceACLs(temp);

            //convert RelationshipBaseKlink to AccessControled<RelationshipBaseKlink>
            List<AccessControled<RelationshipBaseKlink>> retrievedRelationships = new List<AccessControled<RelationshipBaseKlink>>();
            foreach (var item in addedConcepts.AddedRelationships)
            {
                retrievedRelationships.Add(
                    new AccessControled<RelationshipBaseKlink>()
                    {
                        ConceptInstance = item,
                        Acl = aclsRetrieved[dataSourceID]
                    }
                    );
            }
            AddedConceptsWithAcl addedConceptsWithAcl = new AddedConceptsWithAcl()
            {
                AddedObjects = addedConcepts.AddedObjects,
                AddedProperties = addedConcepts.AddedProperties,
                AddedMedias = addedConcepts.AddedMedias,
                AddedRelationshipsWithAcl = retrievedRelationships
            };
            return addedConceptsWithAcl;
        }
    }
}