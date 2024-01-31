using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [Serializable]
    public class MetaDataFieldModel : DataSourceFieldModel
    {
        public MetaDataFieldModel()
        {
            Type = FieldType.MetaData;
        }

        MetaDataItemModel relatedMetaDataItem;

        [XmlIgnore]
        public MetaDataItemModel RelatedMetaDataItem
        {
            get => relatedMetaDataItem;
            set
            {
                if (SetValue(ref relatedMetaDataItem, value))
                {
                    if (RelatedMetaDataItem != null)
                    {
                        RelatedMetaDataItem.ValueChanged -= RelatedMetaDataItem_ValueChanged;
                        RelatedMetaDataItem.ValueChanged += RelatedMetaDataItem_ValueChanged;

                        SetSampleValue();
                    }
                }
            }
        }

        private void RelatedMetaDataItem_ValueChanged(object sender, EventArgs e)
        {
            SetSampleValue();
        }

        private void SetSampleValue()
        {
            string sampleVal = string.Empty;

            if (RelatedMetaDataItem.Value == null)
                SampleValue = string.Empty;
            else
            {
                if (RelatedMetaDataItem.Value is string[])
                {
                    sampleVal = string.Join(", ", ((string[])RelatedMetaDataItem.Value));
                }
                else
                {
                    sampleVal = RelatedMetaDataItem.Value.ToString();
                }

                SampleValue = sampleVal;
            }
        }
    }
}
