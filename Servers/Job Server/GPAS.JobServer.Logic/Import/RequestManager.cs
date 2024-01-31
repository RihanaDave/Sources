using GPAS.AccessControl;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.JobServer.Logic.Entities;
using GPAS.JobsManagement;
using System.Collections.Generic;
using System.IO;

namespace GPAS.JobServer.Logic.SemiStructuredDataImport
{
    public class RequestManager
    {
        public void RegisterNewImportRequests(SemiStructuredDataImportRequestMetadata[] requestsData)
        {
            var newRequests = new List<SemiStructuredDataImportRequest>(requestsData.Length);
            foreach (var reqData in requestsData)
            {
                MaterialBaseSerializer materialBaseSerializer = new MaterialBaseSerializer();
                MemoryStream materialMemStream = new MemoryStream(reqData.serializedMaterialBase);
                MaterialBase material = materialBaseSerializer.Deserialize(materialMemStream);

                TypeMappingSerializer typeMappingSerializer = new TypeMappingSerializer();
                MemoryStream mappingMemStream = new MemoryStream(reqData.serializedTypeMapping);
                TypeMapping mapping = typeMappingSerializer.Deserialize(mappingMemStream);

                ACLSerializer aCLSerializer = new ACLSerializer();
                MemoryStream aclMemStream = new MemoryStream(reqData.serializedACL);
                ACL acl = aCLSerializer.Deserialize(aclMemStream);

                newRequests.Add(new SemiStructuredDataImportRequest()
                {
                    ImportMaterial = material,
                    ImportMapping = mapping,
                    DataSourceACL = acl
                });
            }
            JobsStoreAndRetrieveProvider.SaveNewRequests(newRequests);
        }
    }
}