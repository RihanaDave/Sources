package Access;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import org.apache.http.ParseException;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.util.EntityUtils;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class HadoopManager {
	
	private static final String Hadoop_URL = "jdbc:drill:drillbit=" + ConfigurationManager.GetAppSettings("drillip") + ":31010";
	private static final String Hadoop_IP_Address = ConfigurationManager.GetAppSettings("hadoopIpAddress");
	
	private String GetHadoopBaseURL(){
		return "http://" + Hadoop_IP_Address + ":50070/webhdfs/v1";
	}
	
	private List<String> GetSeparators()
    {
        List<String> separators = new ArrayList();
        separators.add("TabSeparated");
        separators.add("CommaSeparated");
        separators.add("PipeSeparated");
        separators.add("SharpSeparated");
        separators.add("SlashSeparated");
        return separators;
    }
		
	public List<DirectoryContent> GetDirectoryContents(String path) throws ParseException, IOException, JSONException
    {
        List<DirectoryContent> resultList = new ArrayList<DirectoryContent>();
        
        String restString = GetHadoopBaseURL() + path + "/?op=LISTSTATUS";
        
        CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpGet getRequest = new HttpGet(restString);
		CloseableHttpResponse response = httpClient.execute(getRequest);
		JSONArray jsonArray = GetValueFromHttpResponse(response, "FileStatuses", "FileStatus");
		for (int i = 0; i < jsonArray.length(); i++) {
			DirectoryContent directoryContent = new DirectoryContent();			
			directoryContent.ContentType = jsonArray.getJSONObject(i).getString("type");
			directoryContent.DisplayName = jsonArray.getJSONObject(i).getString("pathSuffix");
			resultList.add(directoryContent);
		}
        return resultList;
    }
	
	private JSONArray GetValueFromHttpResponse(CloseableHttpResponse httpResponse,
			String parentKey, String childKey)
			throws ParseException, IOException, JSONException {		
		String responseAsString = EntityUtils.toString(httpResponse
				.getEntity());
		JSONObject jsonObject1 = new JSONObject(responseAsString);
		JSONObject jsonObject = jsonObject1.getJSONObject(parentKey);

		//JSONObject jsonObject = new JSONObject(value);
		JSONArray results = jsonObject.getJSONArray(childKey);
		//JSONObject first = results.getJSONObject(0);
//		JSONObject shipper = first.getJSONObject("shipper");
//		Integer id = shipper.getInt("id");
//		
//		value = jsonObject.getJSONObject(parentKey).get(childKey);
		return results;
	}
	
	public List<String> GetDirectories(String path)
    {
		List<String> result = new ArrayList<String>();        
        try
        {        
        	List<String> separators = GetSeparators();
        	for (int i = 0; i < separators.size(); i++) {
        		List<DirectoryContent> directoryContentList = null;
                String fileIngestionPath = String.format("/Datalake/FileIngestion/%s", separators.get(i));
                try{
                	directoryContentList = GetDirectoryContents(fileIngestionPath);
                	for (int j = 0; j < directoryContentList.size(); j++) {
                		String temp = directoryContentList.get(j).ContentType;
						if (directoryContentList.get(j).ContentType.equals("DIRECTORY")) {
							result.add(directoryContentList.get(j).DisplayName);
						}
					}
                }catch(Exception ex){
                	continue;
                }
                

                String streamingDataPath = String.format("/Datalake/StreamingData/%s", separators.get(i));
                try{
                	directoryContentList = GetDirectoryContents(streamingDataPath);
                	for (int j = 0; j < directoryContentList.size(); j++) {
						if (directoryContentList.get(j).ContentType.equals("DIRECTORY")) {
							result.add(directoryContentList.get(j).DisplayName);
						}
					}
                }catch(Exception ex){
                	continue;
                }                                              
			}            
        	return result;
        }
        catch (Exception ex)
        {            
        }
		return result;
    }

}
