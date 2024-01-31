using GPAS.Workspace.Presentation.Controls.TagCloud;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public class ContentAnalysisInfo
    {
        public ContentAnalysisInfo(long objectId)
        {
            ObjectId = objectId;
            RetrievedSummarization = new List<string>();
        }

        public KeyPhraseModel[] RetrievedKeyPhrases { get; set; }

        public string DocContent { get; set; }

        public long ObjectId { get; set; }

        public bool IsContentAnalysisControlsSetBefore { get; set; }

        public bool IsSummarizationControlsSetBefore { get; set; }

        public List<string> RetrievedSummarization { get; set; }
    
    }
}
