using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.DataMapping.SemiStructured
{
    [Serializable]
    public class DocumentMapping : ObjectMapping
    {
        /// <summary>
        /// سازنده کلاس؛ این سازنده برای سری‌سازی کلاس در نظر گرفته شده و برای جلوگیری از استفاده نامطلوب، محلی شده
        /// </summary>
        protected DocumentMapping()
        { }

        public DocumentMapping(OntologyTypeMappingItem objectType, string mappingTitle, TableColumnMappingItem documentPathMapping)
            : base(objectType, mappingTitle)
        {
            DocumentPathMapping = documentPathMapping;
            PathOptions = new DocumentPathOptions();
            IsDocumentNameAsDisplayName = true;
        }

        public ValueMappingItem DocumentPathMapping { get; set; }
        public DocumentPathOptions PathOptions { get; set; }
        public bool IsDocumentNameAsDisplayName { get; set; }

        public override ValueMappingItem GetDisplayName()
        {
            if (IsDocumentNameAsDisplayName)
            {
                return DocumentPathMapping;
            }
            else
            {
                return base.GetDisplayName();
            }
        }

        public override void SetDisplayName(PropertyMapping propertyToSetAsDisplayName)
        {
            base.SetDisplayName(propertyToSetAsDisplayName);
            IsDocumentNameAsDisplayName = false;
        }

        public void SetDocumentNameAsDisplayName()
        {
            IsDocumentNameAsDisplayName = true;

            foreach (var property in Properties)
            {
                property.IsSetAsDisplayName = false;
            }
        }
    }
}
