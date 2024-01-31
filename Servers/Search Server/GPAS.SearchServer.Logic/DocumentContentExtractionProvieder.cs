namespace GPAS.SearchServer.Logic
{
    public class DocumentContentExtractionProvieder
    {
        public string GetDocumentPossibleExtractedContent(long docID)
        {
            return SearchEngineProvider.GetNewDefaultSearchEngineClient().GetFileDocumentPossibleExtractedContent(docID.ToString());
        }
    }
}