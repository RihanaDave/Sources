using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.EventArguments
{
    public class AddFilesEventArgs : EventArgs
    {
        public string[] FilesPath { get; }

        public AddFilesEventArgs(string[] filesPath)
        {
            FilesPath = filesPath ?? throw new ArgumentNullException(nameof(filesPath));
        }
    }
}
