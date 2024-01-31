using System;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class DocumentCreationRequestSubmitedEventAgrs : EventArgs
    {
        public DocumentCreationRequestSubmitedEventAgrs(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            FilePath = filePath;
        }

        public string FilePath { get; private set; }
    }
}
