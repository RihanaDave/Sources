using GPAS.Ontology;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [Serializable]
    public class DocumentMapModel : ObjectMapModel
    {
        private readonly Ontology.Ontology ontology = new Ontology.Ontology();

        #region Propertes

        private SinglePropertyMapModel path;
        public SinglePropertyMapModel Path
        {
            get => path;
            set
            {
                if (SetValue(ref path, value))
                {
                    Path.SampleValueChanged -= Path_SampleValueChanged;
                    Path.SampleValueChanged += Path_SampleValueChanged;

                    Path.ScenarioChanged -= Path_ScenarioChanged;
                    Path.ScenarioChanged += Path_ScenarioChanged;

                    Path.HasResolutionChanged -= Path_HasResolutionChanged;
                    Path.HasResolutionChanged += Path_HasResolutionChanged;

                    SetDisplayNameProperty();
                    OnPathChanged();
                    OnScenarioChanged();
                }
            }
        }

        private void Path_HasResolutionChanged(object sender, EventArgs e)
        {
            PrepareWarnings();
            SetValidation();
        }

        private DocumentPathOption pathOption = DocumentPathOption.File;
        public DocumentPathOption PathOption
        {
            get => pathOption;
            set
            {
                if (SetValue(ref pathOption, value))
                {
                    SetDisplayNameProperty();
                    OnScenarioChanged();
                }
            }
        }

        #endregion

        #region Methods

        public DocumentMapModel()
        {
            Path = new SinglePropertyMapModel
            {
                DataType = BaseDataTypes.HdfsURI,
                Title = "Path",
                HasResolution = false,
                OwnerObject = this,
            };

            DisplayNameChangeable = false;

            SinglePropertyMapModel singlePropertyMapModel = new SinglePropertyMapModel();
            singlePropertyMapModel.TypeUri = ontology.GetDocumentNamePropertyTypUri();
            singlePropertyMapModel.DataType = BaseDataTypes.String;
            singlePropertyMapModel.Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(singlePropertyMapModel.TypeUri);
            singlePropertyMapModel.IconPath = OntologyIconProvider.GetPropertyTypeIconPath(singlePropertyMapModel.TypeUri).ToString();
            singlePropertyMapModel.OwnerObject = this;
            singlePropertyMapModel.Editable = false;
            Properties.Add(singlePropertyMapModel);
            singlePropertyMapModel.IsDisplayName = true;
            singlePropertyMapModel.IsSelected = true;

            CreateValue();
        }

        private void Path_ScenarioChanged(object sender, EventArgs e)
        {
            OnScenarioChanged();
        }

        protected override void AfterOwnerMapChanged()
        {
            base.AfterOwnerMapChanged();
            CreateValue();
        }

        private void CreateValue()
        {
            if (OwnerMap?.OwnerDataSource?.FieldCollection == null)
                return;

            ((SinglePropertyMapModel)DisplayNameProperty).ValueCollection.Clear();

            ((SinglePropertyMapModel)DisplayNameProperty).ValueCollection.Add(
                new ValueMapModel
                {
                    Field = OwnerMap.OwnerDataSource.FieldCollection.FirstOrDefault(x => x.Type == FieldType.Const),
                    OwnerProperty = DisplayNameProperty
                });
        }

        private void Path_SampleValueChanged(object sender, EventArgs e)
        {
            SetDisplayNameProperty();
        }

        private void SetDisplayNameProperty()
        {
            if (((SinglePropertyMapModel)DisplayNameProperty)?.ValueCollection?.Count > 0)
            {
                string fileName = "This is a folder";

                if (PathOption == DocumentPathOption.File)
                {
                    fileName = Path.SampleValue.Split(
                    System.IO.Path.AltDirectorySeparatorChar,
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.PathSeparator,
                    System.IO.Path.VolumeSeparatorChar
                    ).LastOrDefault();
                }

                ((SinglePropertyMapModel)DisplayNameProperty).ValueCollection[0].SampleValue = fileName;
            }
        }

        public override IEnumerable<PropertyMapModel> GetAllProperties()
        {
            if (Properties == null)
                return new List<PropertyMapModel>() { Path };
            else
                return Properties.Concat(new List<PropertyMapModel>() { Path });
        }

        #endregion

        #region Event

        public event EventHandler PathChanged;
        protected void OnPathChanged()
        {
            PathChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
