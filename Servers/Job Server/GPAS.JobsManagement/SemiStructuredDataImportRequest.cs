using GPAS.AccessControl;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.Material.SemiStructured;
using System;

namespace GPAS.JobsManagement
{
    [Serializable]
    public class SemiStructuredDataImportRequest : DataImportRequest
    {
        public SemiStructuredDataImportRequest()
        {
        }

        public SemiStructuredDataImportRequest(MaterialBase importMaterial, TypeMapping importMapping)
        {
            ImportMaterial = importMaterial;
            ImportMapping = importMapping;
        }

        public MaterialBase ImportMaterial
        {
            get;
            set;
        }

        public TypeMapping ImportMapping
        {
            get;
            set;
        }

        public ACL DataSourceACL
        {
            get;
            set;
        }
    }
}
