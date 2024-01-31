using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public abstract class SingleDataSourceModel : DataSourceModel, ISingleDataSource
    {
        #region Properties

        FileInfoModel fileInfo = new FileInfoModel();
        public FileInfoModel FileInfo
        {
            get => fileInfo;
            set
            {
                if (SetValue(ref fileInfo, value))
                {
                    SetIcon();
                    IsValid = GetValidation();
                    DefectionMessageCollection = PrepareDefections();
                    RegeneratePreview();
                    ResetPathFieldCollection();
                    ResetMetaDataCollection();

                    if (fileInfo != null)
                    {
                        fileInfo.FullPathChanged -= FileInfo_FullPathChanged;
                        fileInfo.FullPathChanged += FileInfo_FullPathChanged;
                    }
                }
            }
        }

        #endregion

        #region Methods

        protected SingleDataSourceModel()
        {
            FileInfo = new FileInfoModel();
        }

        protected override ObservableCollection<DefectionModel> PrepareDefections()
        {
            ObservableCollection<DefectionModel> defections = base.PrepareDefections();
            if (!GetValidation())
            {
                defections.Add(new DefectionModel
                {
                    Message = "Data source file not available",
                    DefectionType = DefectionType.PreviewHasAnError
                });
            }
            return defections;
        }

        protected override bool GetValidation()
        {
            return FileInfo != null && FileInfo.IsExist();
        }

        protected override bool CanGeneratePreview()
        {
            return FileInfo != null && FileInfo.IsExist();
        }

        private void FileInfo_FullPathChanged(object sender, EventArgs e)
        {
            SetIcon();
            IsValid = GetValidation();
            DefectionMessageCollection = PrepareDefections();
            RegeneratePreview();
            ResetPathFieldCollection();
            ResetMetaDataCollection();
        }

        private void ResetPathFieldCollection()
        {
            RemovePathFields();
            AddPathFields();
        }

        protected override void ResetFieldCollection()
        {
            base.ResetFieldCollection();
            AddPathFields();
        }

        private void AddConstField()
        {
            if (FieldCollection == null)
                return;

            FieldCollection.Add(new ConstFieldModel());
        }

        private void AddPathFields()
        {
            if (FieldCollection == null || FileInfo == null)
                return;

            List<PathPartFieldModel> pathPartFieldModels = new List<PathPartFieldModel>();
            pathPartFieldModels.Add(GetFullPathField());
            pathPartFieldModels.Add(GetRootField());

            pathPartFieldModels.AddRange(AddDirectoryPathFields());

            pathPartFieldModels.Add(GetFileNameField());
            pathPartFieldModels.Add(GetExtensionField());

            FieldCollection = new ObservableCollection<DataSourceFieldModel>(
                FieldCollection.Concat(pathPartFieldModels)
            );
        }

        private void RemovePathFields()
        {
            if (FieldCollection == null)
                return;

            FieldCollection = new ObservableCollection<DataSourceFieldModel>(
                FieldCollection.Except(FieldCollection.OfType<PathPartFieldModel>())
            );
        }

        private IEnumerable<PathPartFieldModel> AddDirectoryPathFields()
        {
            List<PathPartFieldModel> result = new List<PathPartFieldModel>();
            if (FileInfo == null || !FileInfo.IsExist())
                return result;

            List<string> directories = FileInfo.GetDirectories().Reverse<string>().ToList();

            for (int i = 0; i < directories?.Count; i++)
            {
                string dir = directories[i];
                result.Add(new PathPartFieldModel()
                {
                    PartType = PathPartType.Directory,
                    Title = "Directory " + (i + 1),
                    SampleValue = dir,
                    DirectoryIndex = i
                });
            }

            return result;
        }

        private PathPartFieldModel GetFullPathField()
        {
            return new PathPartFieldModel()
            {
                PartType = PathPartType.FullPath,
                Title = "Full Path",
                SampleValue = FileInfo == null ? string.Empty : FileInfo.FullPath
            };
        }

        private PathPartFieldModel GetFileNameField()
        {
            return new PathPartFieldModel()
            {
                PartType = PathPartType.FileName,
                Title = "File Name",
                SampleValue = FileInfo == null ? string.Empty : FileInfo.NameWithoutExtension
            };
        }

        private PathPartFieldModel GetExtensionField()
        {
            return new PathPartFieldModel()
            {
                PartType = PathPartType.Extension,
                Title = "Extension",
                SampleValue = FileInfo == null ? string.Empty : FileInfo.Extension
            };
        }

        private PathPartFieldModel GetRootField()
        {
            string rootTitle = FileInfo == null ? string.Empty : "Drive Name";

            if (FileInfo != null)
            {
                if (FileInfo.IsNetWorkPath())
                    rootTitle = "Computer Name";

                if (FileInfo.IsLinuxPath())
                    rootTitle = "Root";
            }

            return new PathPartFieldModel()
            {
                PartType = PathPartType.Root,
                Title = rootTitle,
                SampleValue = FileInfo == null ? string.Empty : FileInfo.GetRoot(),
            };
        }

        protected override void ResetMetaDataFieldCollection()
        {
            base.ResetMetaDataFieldCollection();
        }

        protected override void ResetMetaDataCollection()
        {
            base.ResetMetaDataCollection();
            ResetFileMetaDataCollection();
        }

        private void ResetFileMetaDataCollection()
        {
            RemoveFileMetaDataCollection();
            AddFileMetaDataCollection();
        }

        private void RemoveFileMetaDataCollection()
        {
            if (MetaDataCollection == null)
                return;

            MetaDataCollection = new ObservableCollection<MetaDataItemModel>(
                MetaDataCollection.Except(MetaDataCollection.Where(md => md.Type == MetaDataType.File))
            );
        }

        private void AddFileMetaDataCollection()
        {
            IEnumerable<MetaDataItemModel> fileMetaData = GetFileMetaData().OfType<MetaDataItemModel>();

            if (MetaDataCollection == null)
                MetaDataCollection = new ObservableCollection<MetaDataItemModel>(fileMetaData.OrderBy(md => md.Type).ThenBy(md => md.Title));
            else
                MetaDataCollection = new ObservableCollection<MetaDataItemModel>(
                    MetaDataCollection.Concat(fileMetaData).OrderBy(md => md.Type).ThenBy(md => md.Title)
                );
        }

        private IEnumerable<MetaDataItemModel> GetFileMetaData()
        {
            if (FileInfo == null || !FileInfo.IsExist())
                return new List<MetaDataItemModel>();
            return FileInfo.GetMetaData().Select(m =>
            {
                m.OwnerDataSource = this;
                return m;
            });
        }

        #endregion

        #region Event

        #endregion
    }
}
