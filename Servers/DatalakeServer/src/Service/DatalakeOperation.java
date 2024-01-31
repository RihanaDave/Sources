package Service;

import java.util.List;

import javax.jws.WebMethod;
import javax.jws.WebService;


@WebService
public interface DatalakeOperation {
	@WebMethod String GetDatalakeSlice(DatalakeQuery drillquery);
	@WebMethod List<String> GetHeaders(String category, String dateTime);
	@WebMethod List<String> GetJobsStatus();
	@WebMethod List<String> GetStreamJobsStatus();
	@WebMethod List<String> GetPreviewData(String category, String dateTime);
	@WebMethod void InsertFileIngestionJobStatus(IngestionFile ingestionFile);
	@WebMethod void InsertStreamIngestionStartStatus(StreamingIngestion streamingIngestion);
	@WebMethod void InsertStreamIngestionStopStatus(StreamingIngestion streamingIngestion);
	@WebMethod Boolean IsListenProcessorExist(StreamingIngestion streamingIngestion);	
	@WebMethod void StartStreamingIngestion(StreamingIngestion streamingIngestion);
	@WebMethod void StopStreamingIngestion(StreamingIngestion streamingIngestion);
	@WebMethod List<String> GetDatalakeCategories(String path);
	@WebMethod void test();
}