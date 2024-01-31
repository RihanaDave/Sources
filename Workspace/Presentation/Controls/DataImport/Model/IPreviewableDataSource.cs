using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public interface IPreviewableDataSource
    {
        void RegeneratePreview();

        event EventHandler PreviewChanged;
    }

    public interface IPreviewableDataSource<T> : IPreviewableDataSource
    {
        T Preview { get; set; }
    }
}