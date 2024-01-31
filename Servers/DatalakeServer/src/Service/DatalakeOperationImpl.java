package Service;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.UnsupportedEncodingException;
import java.net.URI;
import java.net.URISyntaxException;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.text.DateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Base64;
import java.util.Calendar;
import java.util.Collection;
import java.util.Formatter;
import java.util.List;
import java.util.Scanner;
import java.util.concurrent.TimeUnit;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.text.SimpleDateFormat;
import java.time.format.DateTimeFormatter;
import java.util.Date;

import javax.jws.WebService;

//import oadd.org.joda.time.DateTime;
//import oadd.org.joda.time.format.DateTimeFormat;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.ParseException;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.HttpClient;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpDelete;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.client.utils.URIBuilder;
import org.apache.http.entity.ByteArrayEntity;
import org.apache.http.entity.ContentProducer;
import org.apache.http.entity.ContentType;
import org.apache.http.entity.EntityTemplate;
import org.apache.http.entity.FileEntity;
import org.apache.http.entity.StringEntity;
import org.apache.http.entity.mime.MultipartEntity;
import org.apache.http.entity.mime.MultipartEntityBuilder;
import org.apache.http.entity.mime.content.FileBody;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.impl.client.HttpClientBuilder;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.params.HttpParams;
import org.apache.http.util.EntityUtils;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import org.w3c.dom.Document;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;

import Access.ConfigurationManager;
import Access.DataBaseProvider;
import Access.DirectoryContent;
import Access.HadoopManager;
import Access.QueryEnginProvider;

@WebService(endpointInterface = "Service.DatalakeOperation")
public class DatalakeOperationImpl implements DatalakeOperation {

	// Global Variables
	// static final String JDBC_DRIVER = "org.apache.drill.jdbc.Driver";
	// static final String DB_URL = "jdbc:drill:drillbit=172.16.8.202:31010";
	// static final String USER = "admin";
	// static final String PASS = "admin";

	// public static final String listenTCPProcessorId = ConfigurationManager
	// .GetAppSettings("listenTCPProcessorId");
	// public static final String firstUpdateAttributeProcessorId =
	// ConfigurationManager
	// .GetAppSettings("firstUpdateAttributeProcessorId");
	// public static final String secondUpdateAttributeProcessorId =
	// ConfigurationManager
	// .GetAppSettings("secondUpdateAttributeProcessorId");
	public static final String siteUrl = ConfigurationManager
			.GetAppSettings("siteUrl");
	public static final String portNumber = ConfigurationManager
			.GetAppSettings("portNumber");	
	public static final String rootNiFiProcessGroupId = ConfigurationManager
			.GetAppSettings("rootNiFiProcessGroupId");
	public static final String streamIngestionConnectionId = ConfigurationManager
			.GetAppSettings("streamIngestionConnectionId");

	public StreamIngestionCounter streamIngestionCounter = new StreamIngestionCounter();

	// //////////////////////////////////////////////////////////////////////////////////////////////////////////
	// //////////////////////////////////////////////Services///////////////////////////////////////////////////
	@Override
	public String GetDatalakeSlice(DatalakeQuery drillquery) {
		return getDatalakeSliceFromQueryEngin(drillquery);
	}

	@Override
	public List<String> GetHeaders(String category, String dateTime) {
		return getHeadersFromDB(category, dateTime);
	}

	@Override
	public List<String> GetJobsStatus() {
		List<String> jobsStatus = getJobsStatusFromDB();
		return jobsStatus;
	}

	@Override
	public List<String> GetStreamJobsStatus() {
		List<String> jobsStatus = getStreamJobsStatusFromDB();
		return jobsStatus;
	}

	@Override
	public List<String> GetPreviewData(String category, String dateTime) {
		return getPreviewDataFromQueryEngin(category, dateTime);
	}

	@Override
	public void InsertFileIngestionJobStatus(IngestionFile ingestionFile) {
		try {
			String query = GenerateQuery(ingestionFile);
			ExecuteSQLInserQuery(query);
		} catch (Exception e) {
			System.out.println(e);
		}
	}

	@Override
	public void InsertStreamIngestionStartStatus(
			StreamingIngestion streamingIngestion) {
		try {
			String query = GenerateQueryToStartStreamIngestion(streamingIngestion);
			ExecuteSQLInserQuery(query);
		} catch (Exception e) {
			System.out.println(e);
		}
	}

	@Override
	public void InsertStreamIngestionStopStatus(
			StreamingIngestion streamingIngestion) {
		try {
			String query = GenerateQueryToStopStreamIngestion(streamingIngestion);
			ExecuteSQLInserQuery(query);
		} catch (Exception e) {
			System.out.println(e);
		}
	}

	@Override
	public Boolean IsListenProcessorExist(StreamingIngestion streamingIngestion) {
		Boolean result = false;
		try {
			result = CheckListenProcessorExistance(streamingIngestion);
		} catch (Exception e) {
			System.out.println(e);
		}
		return result;
	}

	@Override
	public void test() {
		StreamingIngestion streamingIngestion = new StreamingIngestion();
		streamingIngestion.Category = "TG";
		streamingIngestion.InputPortNumber = "3333";
		streamingIngestion.Headers = "H1,H2,H3";

		try {
			StartStreamingIngestion(streamingIngestion);
			StopSpecificStreamingIngestion(streamingIngestion);
		} catch (Exception e) {
			e.printStackTrace();
		}
		// //System.out.println("Counter = " +
		// streamIngestionCounter.GetListennigProcessorCount());
		// streamIngestionCounter.IncreaseListennigProcessorCount();
		// try {
		// LoadTemplateToNiFiCanvas("6ef148f2-f233-46fb-a575-19575de7557c",
		// streamIngestionCounter.GetListennigProcessorCount());
		// } catch (IOException | JSONException e) {
		// // TODO Auto-generated catch block
		// e.printStackTrace();
		// }
		// try {
		// GetTemplateNameIdMapping();
		// GetProcessorsNameIdMapping();
		// } catch (IOException | JSONException e) {
		// // TODO Auto-generated catch block
		// e.printStackTrace();
		// }
		// StreamingIngestion streamingIngestion = new StreamingIngestion();
		// streamingIngestion.Category = "TG";
		// streamingIngestion.InputPortNumber = "5555";
		// String templatePath;
		// try {
		// templatePath = ChangeStreamIngestionTemplate(streamingIngestion);
		// UploadTemplate(templatePath);
		// } catch (ParserConfigurationException | IOException | SAXException e)
		// {
		// // TODO Auto-generated catch block
		// e.printStackTrace();
		// }

		// List<String> result = new ArrayList<String>();
		// Connection conn = null;
		// Statement stmt = null;
		// QueryEnginProvider queryEnginProvider = new QueryEnginProvider();
		// try {
		// conn = queryEnginProvider.OpenConnection();
		// String query = "select columns[1] as `C1` from dfs.`/rrr.csv`";
		// ResultSet rs = queryEnginProvider.ExecuteSelectQuery(conn, query);
		// while (rs.next()) {
		// String first = rs.getString("C1");
		// String row = first + " ";
		// result.add(row);
		// System.out.print("Row: " + row);
		// }
		// rs.close();
		// queryEnginProvider.CloseConnection(conn);
		// } catch (SQLException se) {
		// // Handle errors for JDBC
		// se.printStackTrace();
		// } catch (Exception e) {
		// // Handle errors for Class.forName
		// e.printStackTrace();
		// } finally {
		// try {
		// if (stmt != null)
		// stmt.close();
		// } catch (SQLException se2) {
		// }
		// try {
		// if (conn != null)
		// conn.close();
		// } catch (SQLException se) {
		// se.printStackTrace();
		// }
		// }
		// return result;
	}

	@Override
	public void StartStreamingIngestion(StreamingIngestion streamingIngestion) {
		try {
			StartListeningToSpecificPortNumber(streamingIngestion);
		} catch (Exception e) {
			e.printStackTrace();
		}

	}

	@Override
	public void StopStreamingIngestion(StreamingIngestion streamingIngestion) {
		try {
			StopSpecificStreamingIngestion(streamingIngestion);
		} catch (Exception e) {
			e.printStackTrace();
		}

	}

	@Override
	public List<String> GetDatalakeCategories(String path) {
		List<String> result = new ArrayList<String>();
		try {
			HadoopManager hadoopManager = new HadoopManager();
			result = hadoopManager.GetDirectories(path);
		} catch (Exception e) {
			e.printStackTrace();
		}
		return result;

	}

	// //////////////////////////////////////////////////////////////////////////////////////////////////////////
	// //////////////////////////////////////////////Methods/////////////////////////////////////////////////////
	private String getDatalakeSliceFromQueryEngin(DatalakeQuery drillquery) {
		Connection conn = null;
		ResultSet rs = null;
		QueryEnginProvider queryEnginProvider = new QueryEnginProvider();
		StringBuilder resultStr = new StringBuilder();
		Formatter resultfmt = new Formatter(resultStr);
		List<String> headers = getHeadersFromDB(drillquery.Category,
				drillquery.DateTime);
		List<String> directories = generateDirectories(drillquery.Category,
				drillquery.DateTime);

		String firstPartOfQuery = GenerateFirstPartOfQuery(headers);

		try {
			conn = queryEnginProvider.OpenConnection();
			for (int i = 0; i < directories.size(); i++) {
				StringBuilder query = new StringBuilder();
				Formatter fmt = new Formatter(query);
				fmt.format(
						"select %s from dfs.`%s` %s limit 1000",
						firstPartOfQuery.toString(),
						directories.get(i),
						GenerateWhereClause(drillquery.SearchCriterias, headers));
				try {
					rs = queryEnginProvider.ExecuteSelectQuery(conn,
							query.toString());
					if (rs != null) {
						for (int j = 0; j < headers.size(); j++) {
							if (j == headers.size() - 1) {
								resultfmt.format("%s\n", headers.get(j));
							} else {
								resultfmt.format("%s,", headers.get(j));
							}
						}
						while (rs.next()) {
							for (int j = 0; j < headers.size(); j++) {
								if (j == headers.size() - 1) {
									resultfmt.format("%s\n",
											rs.getString(headers.get(j)));
								} else {
									resultfmt.format("%s,",
											rs.getString(headers.get(j)));
								}
							}
						}
					} else {
						continue;
					}

				} catch (SQLException se) {
					se.printStackTrace();
					continue;

				}
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
		if (conn != null)
			queryEnginProvider.CloseConnection(conn);
		return resultStr.toString();
	}

	private List<String> getPreviewDataFromQueryEngin(String category,
			String dateTime) {
		List<String> previewData = new ArrayList<String>();
		QueryEnginProvider queryEnginProvider = new QueryEnginProvider();
		List<String> headers = getHeadersFromDB(category, dateTime);
		List<String> directories = generateDirectories(category, dateTime);
		Connection conn = null;
		ResultSet rs = null;
		Statement stmt = null;
		String firstPartOfQuery = GenerateFirstPartOfQuery(headers);

		try {
			conn = queryEnginProvider.OpenConnection();
			for (int i = 0; i < directories.size(); i++) {
				StringBuilder query = new StringBuilder();
				Formatter fmt = new Formatter(query);
				fmt.format("select %s from dfs.`%s` limit 5",
						firstPartOfQuery.toString(), directories.get(i));

				try {
					rs = queryEnginProvider.ExecuteSelectQuery(conn,
							query.toString());
					if (rs != null) {
						StringBuilder resultStr = new StringBuilder();
						Formatter resultfmt = new Formatter(resultStr);
						for (int j = 0; j < headers.size(); j++) {
							if (j == headers.size() - 1) {
								resultfmt.format("%s\n", headers.get(j));
							} else {
								resultfmt.format("%s,", headers.get(j));
							}
						}
						previewData.add(resultStr.toString());
						while (rs.next()) {
							resultStr = new StringBuilder();
							resultfmt = new Formatter(resultStr);
							for (int j = 0; j < headers.size(); j++) {
								if (j == headers.size() - 1) {
									resultfmt.format("%s\n",
											rs.getString(headers.get(j)));
								} else {
									resultfmt.format("%s,",
											rs.getString(headers.get(j)));
								}
							}
							previewData.add(resultStr.toString());
						}
						break;
					} else {
						continue;
					}

				} catch (SQLException se) {
					se.printStackTrace();
					continue;

				}
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
		if (conn != null)
			queryEnginProvider.CloseConnection(conn);
		return previewData;
	}

	private String GenerateFirstPartOfQuery(List<String> headers) {
		String firstPartOfQuery = "";
		for (int i = 0; i < headers.size(); i++) {
			StringBuilder columnsStr = new StringBuilder();
			Formatter fmtTemp = new Formatter(columnsStr);
			if (i == (headers.size() - 1)) {
				fmtTemp.format("columns[%s] as `%s`", i, headers.get(i));

			} else {
				fmtTemp.format("columns[%s] as `%s`,", i, headers.get(i));

			}
			firstPartOfQuery += columnsStr.toString() + " ";
		}
		return firstPartOfQuery;
	}

	private Boolean CheckListenProcessorExistance(
			StreamingIngestion streamingIngestion) {
		DataBaseProvider dataBaseProvider = new DataBaseProvider();
		Connection con = null;
		Boolean result = false;
		try {
			con = dataBaseProvider.OpenConnection();
			StringBuilder query = new StringBuilder();
			Formatter fmt = new Formatter(query);
			fmt.format(
					"select exists (select * from  StreamingIngestionDataFlowStatus where portNumber = '%s') as existance",
					streamingIngestion.InputPortNumber);
			ResultSet rs = dataBaseProvider.ExecuteSelectQuery(con,
					query.toString());
			String isExist = "0";
			if (rs.next()) {
				isExist = rs.getString("existance");
			}
			if (isExist.equals("1")) {
				result = true;
			} else {
				result = false;
			}
		} catch (Exception e) {
			System.out.println(e);
		} finally {
			dataBaseProvider.CloseConnection(con);
		}
		return result;
	}

	private void ExecuteSQLInserQuery(String query) {
		try {
			DataBaseProvider dataBaseProvider = new DataBaseProvider();
			Connection con = dataBaseProvider.OpenConnection();
			dataBaseProvider.ExecuteUpdateQuery(con, query);
			dataBaseProvider.CloseConnection(con);
		} catch (Exception e) {
			System.out.println(e);
		}
	}

	private String GenerateQuery(IngestionFile ingestionFile) {
		StringBuilder query = new StringBuilder();
		Formatter fmt = new Formatter(query);
		fmt.format(
				"INSERT INTO FileIngestionDataFlowStatus (dataFlowUUID, category, dataFlowDateTime, "
						+ "dataSeparator, timeBegin, timeEnd, dataFlowStatus) VALUES('%s','%s','%s','%s','%s','%s','%s')",
				ingestionFile.id, ingestionFile.Category,
				ingestionFile.DataFlowDateTime, ingestionFile.FileSeparator,
				ingestionFile.TimeBegin, "Not Defiend", "Pending");
		return query.toString();
	}

	private String GenerateQueryToStartStreamIngestion(
			StreamingIngestion streamingIngestion) {
		StringBuilder query = new StringBuilder();
		Formatter fmt = new Formatter(query);
		fmt.format(
				"INSERT INTO StreamingIngestionDataFlowStatus (dataFlowUUID, category, dataFlowDateTime, "
						+ "dataSeparator, startTime, portNumber, dataFlowStatus) VALUES('%s','%s','%s','%s','%s','%s','%s')",
				streamingIngestion.id, streamingIngestion.Category,
				streamingIngestion.dataFlowDateTime,
				streamingIngestion.Separator, streamingIngestion.startTime,
				streamingIngestion.InputPortNumber, "Started");
		return query.toString();
	}

	private String GenerateQueryToStopStreamIngestion(
			StreamingIngestion streamingIngestion) {
		StringBuilder query = new StringBuilder();
		Formatter fmt = new Formatter(query);
		fmt.format(
				"DELETE FROM StreamingIngestionDataFlowStatus where portNumber='%s'",
				streamingIngestion.InputPortNumber);
		return query.toString();
	}

	private String GenerateWhereClause(List<SearchCriteria> searchCriterias,
			List<String> headers) {
		String whereClauseStr = "where ";
		for (int i = 0; i < searchCriterias.size(); i++) {
			StringBuilder whereClause = null;
			Formatter fmt = null;
			switch (searchCriterias.get(i).Comparator) {
			case "Equal":
				switch (searchCriterias.get(i).CriteriaDataType) {
				case "String":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s = '%s')",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Integer":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s = %s)",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Double":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s = %s)",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Date":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s = '%s')",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				}
				break;
			case "GreatorThan":
				switch (searchCriterias.get(i).CriteriaDataType) {
				case "String":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s > '%s')",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Integer":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s > %s)",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Double":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s > %s)",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Date":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s > '%s')",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;

				}
				break;
			case "LessThan":
				switch (searchCriterias.get(i).CriteriaDataType) {
				case "String":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s < '%s')",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Integer":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s < %s)",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Double":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s < %s)",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;
				case "Date":
					whereClause = new StringBuilder();
					fmt = new Formatter(whereClause);
					fmt.format(
							"(%s < '%s')",
							GenerateColumnFormat(searchCriterias.get(i).Type,
									headers), searchCriterias.get(i).Value);
					break;

				}
				break;
			case "Like":
				whereClause = new StringBuilder();
				fmt = new Formatter(whereClause);
				fmt.format(
						"(%s like '%s')",
						GenerateColumnFormat(searchCriterias.get(i).Type,
								headers), "%" + searchCriterias.get(i).Value
								+ "%");
				break;
			}
			if (i == (searchCriterias.size() - 1)) {
				whereClauseStr += whereClause.toString();
			} else {
				whereClauseStr += whereClause.toString() + " and ";
			}

		}
		return whereClauseStr;
	}

	private String GenerateColumnFormat(String type, List<String> headers) {
		String columnFormat = "";
		for (int i = 0; i < headers.size(); i++) {
			if (type.equals(headers.get(i).toString())) {
				columnFormat = "columns[" + i + "]";
				break;
			}
		}
		return columnFormat;
	}

	private String getDirectoryFromDatabase(String category, String dateTime) {
		String result = "";
		try {
			Class.forName("com.mysql.jdbc.Driver");
			Connection con = DriverManager.getConnection(
					"jdbc:mysql://localhost:3306/nifi", "root", "nifi");
			Statement stmt = con.createStatement();
			StringBuilder query = new StringBuilder();
			Formatter fmt = new Formatter(query);
			fmt.format(
					"select directory from DataFlowMetadata where category = '%s' and datetime = '%s'",
					category, dateTime);
			ResultSet rs = stmt.executeQuery(query.toString());
			if (rs.next()) {
				result = rs.getString(1);
			}

			con.close();
		} catch (Exception e) {
			System.out.println(e);
		}

		return result;
	}

	private List<String> getStreamJobsStatusFromDB() {
		Connection con = null;
		DataBaseProvider dataBaseProvider = new DataBaseProvider();
		List<String> jobsStatus = new ArrayList<String>();
		try {
			String query = "select * from StreamingIngestionDataFlowStatus limit 1000";

			con = dataBaseProvider.OpenConnection();
			ResultSet rs = dataBaseProvider.ExecuteSelectQuery(con, query);
			while (rs.next()) {
				String rowTemp = "";
				String dataFlowUUIDColumn = rs.getString("dataFlowUUID");
				String categoryColumn = rs.getString("category");
				String relatedDatetimeColumn = rs.getString("dataFlowDateTime");
				String dataSeparatorColumn = rs.getString("dataSeparator");
				String timeBeginColumn = rs.getString("startTime");
				String timeEndColumn = rs.getString("portNumber");
				String dataFlowStatusColumn = rs.getString("dataFlowStatus");
				rowTemp = dataFlowUUIDColumn + "," + categoryColumn + ","
						+ relatedDatetimeColumn + "," + dataSeparatorColumn
						+ "," + timeBeginColumn + "," + timeEndColumn + ","
						+ dataFlowStatusColumn;
				jobsStatus.add(rowTemp);
			}
		} catch (Exception e) {
			System.out.println(e);
		} finally {
			dataBaseProvider.CloseConnection(con);
		}
		return jobsStatus;
	}

	private List<String> getJobsStatusFromDB() {
		Connection con = null;
		DataBaseProvider dataBaseProvider = new DataBaseProvider();
		List<String> jobsStatus = new ArrayList<String>();
		try {
			String query = "select * from FileIngestionDataFlowStatus limit 1000";

			con = dataBaseProvider.OpenConnection();
			ResultSet rs = dataBaseProvider.ExecuteSelectQuery(con, query);
			while (rs.next()) {
				String rowTemp = "";
				String dataFlowUUIDColumn = rs.getString("dataFlowUUID");
				String categoryColumn = rs.getString("category");
				String relatedDatetimeColumn = rs.getString("dataFlowDateTime");
				String dataSeparatorColumn = rs.getString("dataSeparator");
				String timeBeginColumn = rs.getString("timeBegin");
				String timeEndColumn = rs.getString("timeEnd");
				String dataFlowStatusColumn = rs.getString("dataFlowStatus");
				rowTemp = dataFlowUUIDColumn + "," + categoryColumn + ","
						+ relatedDatetimeColumn + "," + dataSeparatorColumn
						+ "," + timeBeginColumn + "," + timeEndColumn + ","
						+ dataFlowStatusColumn;
				jobsStatus.add(rowTemp);
			}
		} catch (Exception e) {
			System.out.println(e);
		} finally {
			dataBaseProvider.CloseConnection(con);
		}
		return jobsStatus;
	}

	private List<String> getHeadersFromDB(String category, String dateTime) {
		List<String> headers = new ArrayList<String>();
		DataBaseProvider dataBaseProvider = new DataBaseProvider();
		Connection con = null;
		String storedHeaders = "";
		String storedDataSeparator = "";
		List<String> temp = new ArrayList<String>();
		try {
			con = dataBaseProvider.OpenConnection();
			StringBuilder query = new StringBuilder();
			Formatter fmt = new Formatter(query);
			fmt.format(
					"select headers, dataSeparator from DataFlowMetadata where category = '%s' and datetime = '%s'",
					category, dateTime);
			ResultSet rs = dataBaseProvider.ExecuteSelectQuery(con,
					query.toString());
			while (rs.next()) {
				storedHeaders = rs.getString(1);
				storedDataSeparator = rs.getString(2);
				String decodedHeader = new String(Base64.getDecoder().decode(
						storedHeaders));
				temp = Arrays
						.asList(decodedHeader
								.split(getSpliterFromStoredSeparator(storedDataSeparator)));
				for (int i = 0; i < temp.size(); i++) {
					if (!headers.contains(temp.get(i))) {
						headers.add(temp.get(i));
					}
				}
			}
			dataBaseProvider.CloseConnection(con);
		} catch (Exception e) {
			System.out.println(e);
		}
		return headers;
	}

	private String getSpliterFromStoredSeparator(String mysqlSeparator) {
		String result = "";
		if (mysqlSeparator.equals("TabSeparated")) {
			result = "\\t";

		} else if (mysqlSeparator.equals("CommaSeparated")) {
			result = ",";

		} else if (mysqlSeparator.equals("PipeSeparated")) {
			result = "\\|";

		} else if (mysqlSeparator.equals("SharpSeparated")) {
			result = "\\#";

		} else if (mysqlSeparator.equals("SlashSeparated")) {
			result = "\\/";

		}

		return result;
	}

	// private List<String> generateHeaders(String category, String dateTime) {
	// List<String> headers = new ArrayList<String>();
	// Connection conn = null;
	// ResultSet rs = null;
	// Statement stmt = null;
	// List<String> directories = generateDirectories(category, dateTime);
	// for (int i = 0; i < directories.size(); i++) {
	// try {
	// Class.forName(JDBC_DRIVER);
	// conn = DriverManager.getConnection(DB_URL, USER, PASS);
	// String firstFileInDirectory = getFirstFileRelatedToCategoryAndDate(
	// category, dateTime);
	// if (!firstFileInDirectory.equals("")) {
	// StringBuilder query = new StringBuilder();
	// Formatter fmt = new Formatter(query);
	// fmt.format("select * from dfs.`%s/%s` limit 1",
	// directories.get(i), firstFileInDirectory);
	// stmt = conn.createStatement();
	// rs = stmt.executeQuery(query.toString());
	// // String splitter =
	// // getSpliterFromDirectory(directories.get(i));
	// while (rs.next()) {
	// String[] temp = rs.getString(1).toString().split(",");
	// for (int j = 0; j < temp.length; j++) {
	// if (j == 0) {
	// headers.add(temp[j].trim()
	// .substring(2, temp[j].length() - 1)
	// .trim());
	// } else if (j == temp.length - 1) {
	// headers.add(temp[j].trim()
	// .substring(1, temp[j].length() - 2)
	// .trim());
	// } else {
	// headers.add(temp[j].trim()
	// .substring(1, temp[j].length() - 1)
	// .trim());
	// }
	// }
	// }
	// }
	// break;
	// } catch (SQLException se) {
	// // Handle errors for JDBC
	// se.printStackTrace();
	//
	// } catch (Exception e) {
	// // Handle errors for Class.forName
	// e.printStackTrace();
	// continue;
	// }
	// continue;
	// }
	//
	// try {
	// if (stmt != null)
	// stmt.close();
	// } catch (SQLException se2) {
	// }
	// try {
	// if (conn != null)
	// conn.close();
	// } catch (SQLException se) {
	// se.printStackTrace();
	// }
	// return headers;
	//
	// }

	private List<String> generateDirectories(String category, String dateTime) {
		List<String> result = new ArrayList<String>();
		List<String> separators = getSeparators();

		for (int i = 0; i < separators.size(); i++) {
			StringBuilder directory = new StringBuilder();
			Formatter fmt = new Formatter(directory);
			fmt.format("/Datalake/FileIngestion/%s/%s/%s/", separators.get(i),
					category, dateTime);
			result.add(directory.toString());
		}
		for (int i = 0; i < separators.size(); i++) {
			StringBuilder directory = new StringBuilder();
			Formatter fmt = new Formatter(directory);
			fmt.format("/Datalake/StreamingData/%s/%s/%s/", separators.get(i),
					category, dateTime);
			result.add(directory.toString());
		}
		return result;
	}

	private List<String> getSeparators() {
		List<String> separators = new ArrayList<String>();
		separators.add("TabSeparated");
		separators.add("CommaSeparated");
		separators.add("PipeSeparated");
		separators.add("SharpSeparated");
		separators.add("SlashSeparated");
		return separators;
	}

	public String ChangeStreamIngestionTemplate(
			StreamingIngestion streamingIngestion)
			throws ParserConfigurationException, IOException, SAXException {
		String newFilePath = "/home/sshuser/workingdirectory/DatalakeServicefiles/NewStreamIngestionTemplate.xml";
		try {
			String filepath = "/home/sshuser/workingdirectory/DatalakeServicefiles/StreamIngestionTemplate.xml";
			DocumentBuilderFactory docFactory = DocumentBuilderFactory
					.newInstance();
			DocumentBuilder docBuilder = docFactory.newDocumentBuilder();
			Document doc = docBuilder.parse(filepath);

			// Get the root element
			Node template = doc.getFirstChild();
			NodeList templateChilds = template.getChildNodes();
			// Change Template Name
			for (int i = 0; i < templateChilds.getLength(); i++) {
				Node currentTemplateChild = templateChilds.item(i)
						.getNextSibling();
				if (currentTemplateChild != null
						&& "name".equals(currentTemplateChild.getNodeName())) {
					currentTemplateChild.setTextContent("Stream Ingestion "
							+ "(" + streamingIngestion.InputPortNumber + ")");
					break;
				}
			}

			for (int i = 0; i < templateChilds.getLength(); i++) {
				Node currentTemplateChild = templateChilds.item(i)
						.getNextSibling();
				if (currentTemplateChild != null
						&& "snippet".equals(currentTemplateChild.getNodeName())) {
					NodeList snippetChilds = currentTemplateChild
							.getChildNodes();
					for (int j = 0; j < snippetChilds.getLength(); j++) {
						Node currentSnippetChild = snippetChilds.item(j)
								.getNextSibling();
						if (currentSnippetChild != null
								&& "connections".equals(currentSnippetChild
										.getNodeName())) {
							continue;
						} else if (currentSnippetChild != null
								&& "processors".equals(currentSnippetChild
										.getNodeName())) {
							NodeList processorChilds = currentSnippetChild
									.getChildNodes();
							for (int k = 0; k < processorChilds.getLength(); k++) {
								Node currentProcessorChild = processorChilds
										.item(k).getNextSibling();
								if (currentProcessorChild != null
										&& "name".equals(currentProcessorChild
												.getNodeName())) {
									if ("ListenTCP"
											.equals(currentProcessorChild
													.getTextContent())) {
										Node processorConfigChild = processorChilds
												.item(k).getPreviousSibling();
										if (processorConfigChild != null
												&& "config"
														.equals(processorConfigChild
																.getNodeName())) {
											NodeList processorConfigChilds = processorConfigChild
													.getChildNodes();
											for (int l = 0; l < processorConfigChilds
													.getLength(); l++) {
												Node currentConfigChild = processorConfigChilds
														.item(l)
														.getNextSibling();
												if (currentConfigChild != null
														&& "properties"
																.equals(currentConfigChild
																		.getNodeName())) {
													NodeList propertiesChilds = currentConfigChild
															.getChildNodes();
													for (int m = 0; m < propertiesChilds
															.getLength(); m++) {
														Node currentPropertiesChild = propertiesChilds
																.item(m)
																.getNextSibling();
														if (currentPropertiesChild != null
																&& "entry"
																		.equals(currentPropertiesChild
																				.getNodeName())) {
															NodeList currentEntryChilds = currentPropertiesChild
																	.getChildNodes();
															for (int n = 0; n < currentEntryChilds
																	.getLength(); n++) {
																Node currentKeyChild = currentEntryChilds
																		.item(n)
																		.getNextSibling();
																if (currentKeyChild != null
																		&& "key".equals(currentKeyChild
																				.getNodeName())
																		&& "Port"
																				.equals(currentKeyChild
																						.getTextContent())) {
																	Node currentValueChild = currentKeyChild
																			.getNextSibling()
																			.getNextSibling();
																	if (currentValueChild != null
																			&& "value"
																					.equals(currentValueChild
																							.getNodeName())) {
																		currentValueChild
																				.setTextContent(streamingIngestion.InputPortNumber
																						.toString());
																	}
																}
															}

														}
													}

												}
											}
										}
									}
								}
							}
						}
					}
					break;
				}
			}

			for (int i = 0; i < templateChilds.getLength(); i++) {
				Node currentTemplateChild = templateChilds.item(i)
						.getNextSibling();
				if (currentTemplateChild != null
						&& "snippet".equals(currentTemplateChild.getNodeName())) {
					NodeList snippetChilds = currentTemplateChild
							.getChildNodes();
					for (int j = 0; j < snippetChilds.getLength(); j++) {
						Node currentSnippetChild = snippetChilds.item(j)
								.getNextSibling();
						if (currentSnippetChild != null
								&& "connections".equals(currentSnippetChild
										.getNodeName())) {
							continue;
						} else if (currentSnippetChild != null
								&& "processors".equals(currentSnippetChild
										.getNodeName())) {
							NodeList processorChilds = currentSnippetChild
									.getChildNodes();
							for (int k = 0; k < processorChilds.getLength(); k++) {
								Node currentProcessorChild = processorChilds
										.item(k).getNextSibling();
								if (currentProcessorChild != null
										&& "name".equals(currentProcessorChild
												.getNodeName())) {
									currentProcessorChild
											.setTextContent(currentProcessorChild
													.getTextContent()
													+ " ("
													+ streamingIngestion.InputPortNumber
															.toString() + ")");
								}
							}
						}
					}
					break;
				}
			}

			TransformerFactory transformerFactory = TransformerFactory
					.newInstance();
			Transformer transformer = transformerFactory.newTransformer();
			DOMSource source = new DOMSource(doc);
			StreamResult result = new StreamResult(new File(newFilePath));
			transformer.transform(source, result);

		} catch (ParserConfigurationException pce) {
			pce.printStackTrace();
		} catch (TransformerException tfe) {
			tfe.printStackTrace();
		} catch (IOException ioe) {
			ioe.printStackTrace();
		} catch (SAXException sae) {
			sae.printStackTrace();
		}

		return newFilePath;
	}

	private void UploadTemplate(String templatePath)
			throws ClientProtocolException, IOException,
			ParserConfigurationException, SAXException {
		String pathname = templatePath;
		String restString = "http://"
				+ siteUrl
				+ ":"
				+ portNumber
				+ "/nifi-api/process-groups/" 
				+ rootNiFiProcessGroupId
				+ "/templates/upload";
		File file = new File(pathname);
		HttpClient httpclient = new DefaultHttpClient();
		HttpPost httpPost = new HttpPost(restString);
		FileBody uploadFilePart = new FileBody(file);
		MultipartEntity reqEntity = new MultipartEntity();
		reqEntity.addPart("template", uploadFilePart);
		httpPost.setEntity(reqEntity);
		HttpResponse response = httpclient.execute(httpPost);
		// int code = response.getStatusLine().getStatusCode();
	}

	private Boolean IsTemplateExist(String templateName)
			throws ClientProtocolException, IOException, JSONException {
		Boolean result = false;
		List<Template> existanceTemplates = GetTemplateNameIdMapping();
		for (int i = 0; i < existanceTemplates.size(); i++) {
			if (existanceTemplates.get(i).Name.equals(templateName)) {
				result = true;
				break;
			}
		}
		return result;
	}

	private Boolean IsListenTCPProcessorExist(String processorName)
			throws ClientProtocolException, IOException, JSONException {
		Boolean result = false;
		List<Processor> existanceProcessors = GetProcessorsNameIdMapping();
		for (int i = 0; i < existanceProcessors.size(); i++) {
			if (existanceProcessors.get(i).Name.equals(processorName)) {
				result = true;
				break;
			}
		}
		return result;
	}

	private void LoadTemplateToNiFiCanvas(String templateIdToLoad)
			throws ClientProtocolException, IOException, JSONException {
		List<Template> existanceTemplates = GetTemplateNameIdMapping();
		JSONObject jsonObjectToLoadSpecificTemplate = null;
		String message = "";
		StringEntity entity = null;
		String restString = "http://"
				+ siteUrl
				+ ":"
				+ portNumber
				+ "/nifi-api/process-groups/"
				+ rootNiFiProcessGroupId  
				+ "/template-instance";
		HttpClient httpclient = new DefaultHttpClient();
		HttpPost httpPost = new HttpPost(restString);
		httpPost.setHeader("Content-type", "application/json");
		jsonObjectToLoadSpecificTemplate = GenerateJsonObjectToLoadSpecificTemplate(
				templateIdToLoad);
		message = jsonObjectToLoadSpecificTemplate.toString();
		entity = new StringEntity(message);
		httpPost.setEntity(entity);
		HttpResponse response = httpclient.execute(httpPost);
		int code = response.getStatusLine().getStatusCode();
		//System.out.println("Code = " + code);
	}

	private List<Processor> GetRelatedProcessorsToSpecificTemplate(
			String portNumber) throws ClientProtocolException, IOException,
			JSONException {
		List<Processor> resultList = new ArrayList<Processor>();
		List<Processor> existanceProcessors = GetProcessorsNameIdMapping();
		for (int i = 0; i < existanceProcessors.size(); i++) {
			if (existanceProcessors.get(i).Name.contains(portNumber)) {
				resultList.add(existanceProcessors.get(i));
			}
		}
		return resultList;
	}

	private String GetTemplateIdByName(String templateName)
			throws ClientProtocolException, IOException, JSONException {
		String templateId = "";
		List<Template> existanceTemplate = GetTemplateNameIdMapping();
		for (int i = 0; i < existanceTemplate.size(); i++) {
			if (existanceTemplate.get(i).Name.equals(templateName)) {
				templateId = existanceTemplate.get(i).Id;
			}
		}
		return templateId;
	}

	private List<Template> GetTemplateNameIdMapping()
			throws ClientProtocolException, IOException, JSONException {
		List<Template> resultList = new ArrayList<Template>();
		String restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/flow/templates";
		CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpGet getRequest = new HttpGet(restString);
		CloseableHttpResponse response = httpClient.execute(getRequest);
		String responseAsString = EntityUtils.toString(response.getEntity());
		JSONObject jsonObject = new JSONObject(responseAsString);
		JSONArray templates = jsonObject.getJSONArray("templates");
		for (int i = 0; i < templates.length(); i++) {
			Template template = new Template();
			template.Id = templates.getJSONObject(i).getJSONObject("template")
					.get("id").toString();
			template.Name = templates.getJSONObject(i)
					.getJSONObject("template").get("name").toString();
			resultList.add(template);
		}
		// for (int i = 0; i < resultList.size(); i++) {
		// System.out.println(resultList.get(i).Id + " ---- " +
		// resultList.get(i).Name);
		// }
		return resultList;
	}

	private List<Processor> GetProcessorsNameIdMapping()
			throws ClientProtocolException, IOException, JSONException {
		List<Processor> resultList = new ArrayList<Processor>();
		String restString = "http://"
				+ siteUrl
				+ ":"
				+ portNumber
				+ "/nifi-api/process-groups/" 
				+ rootNiFiProcessGroupId 
				+ "/processors";
		CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpGet getRequest = new HttpGet(restString);
		CloseableHttpResponse response = httpClient.execute(getRequest);
		String responseAsString = EntityUtils.toString(response.getEntity());
		JSONObject jsonObject = new JSONObject(responseAsString);
		JSONArray processors = jsonObject.getJSONArray("processors");
		for (int i = 0; i < processors.length(); i++) {
			Processor processor = new Processor();
			processor.Id = processors.getJSONObject(i)
					.getJSONObject("component").get("id").toString();
			processor.Name = processors.getJSONObject(i)
					.getJSONObject("component").get("name").toString();
			processor.PosisionX = (Double) processors.getJSONObject(i)
					.getJSONObject("position").get("x");
			processor.PosisionY = (Double) processors.getJSONObject(i)
					.getJSONObject("position").get("y");
			resultList.add(processor);
		}
		// for (int i = 0; i < resultList.size(); i++) {
		// System.out.println(resultList.get(i).Id + " ---- " +
		// resultList.get(i).Name);
		// }
		return resultList;
	}

	private void StartListeningToSpecificPortNumber(
			StreamingIngestion streamingIngestion)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, ParserConfigurationException, SAXException,
			java.text.ParseException, ParseException, URISyntaxException {
		if (IsTemplateExist("Stream Ingestion " + "("
				+ streamingIngestion.InputPortNumber + ")")) {
			if (IsListenTCPProcessorExist("ListenTCP " + "("
					+ streamingIngestion.InputPortNumber + ")")) {
				List<Processor> relatedProcessorsId = GetRelatedProcessorsToSpecificTemplate(streamingIngestion.InputPortNumber
						.toString());
				ChangeUpdateAttributeProcessorProperties(streamingIngestion,
						relatedProcessorsId);
				StartNiFiProcessors(relatedProcessorsId);
			} else {
				LoadTemplateToNiFiCanvas(
						GetTemplateIdByName("Stream Ingestion " + "("
								+ streamingIngestion.InputPortNumber + ")"));
				List<Processor> relatedProcessorsId = GetRelatedProcessorsToSpecificTemplate(streamingIngestion.InputPortNumber
						.toString());
				ChangeProcessorProperties(streamingIngestion,
						relatedProcessorsId);
				StartNiFiProcessors(relatedProcessorsId);
			}

		} else {
			String newTemplatePath = ChangeStreamIngestionTemplate(streamingIngestion);
			UploadTemplate(newTemplatePath);
			LoadTemplateToNiFiCanvas(GetTemplateIdByName("Stream Ingestion "
					+ "(" + streamingIngestion.InputPortNumber + ")"));
			List<Processor> relatedProcessorsId = GetRelatedProcessorsToSpecificTemplate(streamingIngestion.InputPortNumber
					.toString());
			ChangeProcessorProperties(streamingIngestion,
					relatedProcessorsId);
			StartNiFiProcessors(relatedProcessorsId);
		}
		streamIngestionCounter.IncreaseListennigProcessorCount();
	}

	private void ChangeProcessorProperties(
			StreamingIngestion streamingIngestion,
			List<Processor> relatedProcessorsId)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, java.text.ParseException, ParseException, URISyntaxException {
		ChangeUpdateAttributeProcessorProperties(streamingIngestion,
				relatedProcessorsId);
		ChangePutSQLProcessorConnectionPool(streamingIngestion,
				relatedProcessorsId);

	}

	// private void StartListeningToSpecificPortNumber(
	// StreamingIngestion streamingIngestion)
	// throws ClientProtocolException, IOException, JSONException,
	// InterruptedException, java.text.ParseException,
	// ParserConfigurationException, SAXException {
	// String restString = "http://" + siteUrl + ":" + portNumber
	// + "/nifi-api/processors/" + listenTCPProcessorId;
	//
	// StopNiFiProcessor(listenTCPProcessorId);
	// StopNiFiProcessor(firstUpdateAttributeProcessorId);
	// StopNiFiProcessor(secondUpdateAttributeProcessorId);
	//
	// while (true) {
	// CloseableHttpResponse processorState =
	// GetNiFiProcessorState(listenTCPProcessorId);
	// String listenTCPProcessorState = GetValueFromHttpResponse(
	// processorState, "component", "state", true).toString();
	// if (listenTCPProcessorState.equals("STOPPED")) {
	// UpdateNiFiProcessor(listenTCPProcessorId, streamingIngestion);
	// StartNiFiProcessor(listenTCPProcessorId);
	// break;
	// } else {
	// TimeUnit.SECONDS.sleep(1);
	// }
	// }
	//
	// while (true) {
	// CloseableHttpResponse processorState =
	// GetNiFiProcessorState(firstUpdateAttributeProcessorId);
	// String firstUpdateAttributeProcessorState = GetValueFromHttpResponse(
	// processorState, "component", "state", true).toString();
	// if (firstUpdateAttributeProcessorState.equals("STOPPED")) {
	// UpdateNiFiProcessor(firstUpdateAttributeProcessorId,
	// streamingIngestion);
	// StartNiFiProcessor(firstUpdateAttributeProcessorId);
	// break;
	// } else {
	// TimeUnit.SECONDS.sleep(1);
	// }
	// }
	//
	// while (true) {
	// CloseableHttpResponse processorState =
	// GetNiFiProcessorState(secondUpdateAttributeProcessorId);
	// String secondUpdateAttributeProcessorState = GetValueFromHttpResponse(
	// processorState, "component", "state", true).toString();
	// if (secondUpdateAttributeProcessorState.equals("STOPPED")) {
	// UpdateNiFiProcessor(secondUpdateAttributeProcessorId,
	// streamingIngestion);
	// StartNiFiProcessor(secondUpdateAttributeProcessorId);
	// break;
	// } else {
	// TimeUnit.SECONDS.sleep(1);
	// }
	// }
	//
	// }

	private void ChangePutSQLProcessorConnectionPool(
			StreamingIngestion streamingIngestion,
			List<Processor> relatedProcessorsId) throws ClientProtocolException, IOException, JSONException, InterruptedException, ParseException, URISyntaxException {
		JSONObject jsonObjectToChangePutSQLProcessorConnectionPool = null;
		String message = "";
		String restString = "";
		StringEntity entity = null;
		CloseableHttpClient httpClient = null;
		CloseableHttpResponse response = null;
		HttpPut putRequest = null;
		String putSQLProcessorId = GetProcessorIdByNameFromList(relatedProcessorsId, 
				"PutSQL (" + streamingIngestion.InputPortNumber + ")");
		restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + putSQLProcessorId;

		httpClient = HttpClients.createDefault();
		putRequest = new HttpPut(restString);
		putRequest.setHeader("Accept", "application/json");
		putRequest.setHeader("Content-type", "application/json");

		response = GetNiFiProcessorState(putSQLProcessorId);
					
		int processorVersion = (int) GetValueFromHttpResponse(
				response, "revision", "version", true);
		response = GetNiFiProcessorState(putSQLProcessorId);
		String assignedConnectionPoolId = GetAssignedConnectionPoolId(response);
		jsonObjectToChangePutSQLProcessorConnectionPool = GenerateJsonObjectToChangePutSQLProcessorConnectionPool(
				putSQLProcessorId, processorVersion, streamingIngestion);
		message = jsonObjectToChangePutSQLProcessorConnectionPool.toString();
		entity = new StringEntity(message);
		putRequest.setEntity(entity);

		response = httpClient.execute(putRequest);
		int code = response.getStatusLine().getStatusCode();
		// System.out.println("Code = " + code);
			
		httpClient.close();
		TimeUnit.SECONDS.sleep(1);
		
		RemoveAssignedConnectionPool(assignedConnectionPoolId);
		
	}

	private String GetAssignedConnectionPoolId(CloseableHttpResponse response) throws ParseException, IOException, JSONException {
		String responseAsString = EntityUtils.toString(response
				.getEntity());
		JSONObject jsonObject = new JSONObject(responseAsString);
		String assignedConnectionPoolId = jsonObject.getJSONObject("component")
				.getJSONObject("config")
				.getJSONObject("properties")
				.get("JDBC Connection Pool").toString();
		return assignedConnectionPoolId;
	}

	private void RemoveAssignedConnectionPool(String assignedConnectionPoolId) throws ClientProtocolException, IOException, ParseException, JSONException, URISyntaxException {
		CloseableHttpResponse controllerServiceState = GetNiFiControllerServiceState(assignedConnectionPoolId);
		String controllerServiceVersion = GetValueFromHttpResponse(controllerServiceState,
				"revision", "version", true).toString();
		CloseableHttpClient httpClient = HttpClients.createDefault();
		URIBuilder builder = new URIBuilder();
		builder.setScheme("http").setHost(siteUrl + ":" + portNumber)
				.setPath("/nifi-api/controller-services/" + assignedConnectionPoolId)
				.setParameter("version", controllerServiceVersion);
		URI uri = builder.build();
		HttpDelete deleteRequest = new HttpDelete(uri);
		CloseableHttpResponse response = httpClient.execute(deleteRequest);		
	}

	private CloseableHttpResponse GetNiFiControllerServiceState(
			String controllerServiceId) throws ClientProtocolException, IOException {
		String restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/controller-services/" + controllerServiceId;
		CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpGet getRequest = new HttpGet(restString);
		CloseableHttpResponse response = httpClient.execute(getRequest);
		return response;
	}

	private JSONObject GenerateJsonObjectToChangePutSQLProcessorConnectionPool(
			String putSQLProcessorId, int processorVersion,
			StreamingIngestion streamingIngestion) throws JSONException {		

		JSONObject json = new JSONObject();

		JSONObject jsonRevisionObj = new JSONObject();
		jsonRevisionObj.put("version", processorVersion);
		json.put("revision", jsonRevisionObj);

		json.put("id", putSQLProcessorId);

		JSONObject jsonComponentObj = new JSONObject();
		jsonComponentObj.put("id", putSQLProcessorId);
		jsonComponentObj.put("state", "STOPPED");
		JSONObject jsonConfigObj = new JSONObject();
		JSONObject jsonPropertiesObj = new JSONObject();
		jsonPropertiesObj.put("JDBC Connection Pool", streamIngestionConnectionId);
		jsonConfigObj.put("properties", jsonPropertiesObj);
		
		JSONObject jsonDescriptorsObj = new JSONObject();
		JSONObject jsonJDBCConnectionPoolObj = new JSONObject();
		JSONArray jsonAllowableValuesArray = new JSONArray();
		JSONObject jsonAllowableValue1Obj = new JSONObject();
		jsonAllowableValue1Obj.put("value", streamIngestionConnectionId);
		jsonAllowableValuesArray.put(jsonAllowableValue1Obj);
		JSONObject jsonCanReadObj = new JSONObject();
		jsonCanReadObj.put("canRead", true);
		jsonAllowableValuesArray.put(jsonCanReadObj);
		JSONObject jsonAllowableValue2Obj = new JSONObject();
		jsonAllowableValue2Obj.put("value", streamIngestionConnectionId);
		jsonAllowableValuesArray.put(jsonAllowableValue2Obj);
		jsonDescriptorsObj.put("JDBC Connection Pool", streamIngestionConnectionId);
		jsonConfigObj.put("properties", jsonDescriptorsObj);
		
		jsonComponentObj.put("config", jsonConfigObj);
		json.put("component", jsonComponentObj);

		JSONObject jsonStatusObj = new JSONObject();
		jsonStatusObj.put("id", putSQLProcessorId);
		json.put("status", jsonStatusObj);

		return json;
	}

	private void ChangeUpdateAttributeProcessorProperties(
			StreamingIngestion streamingIngestion,
			List<Processor> relatedProcessorsId)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, java.text.ParseException {

		String relatedListenTCPProcessorId = GetProcessorIdByNameFromList(
				relatedProcessorsId, "ListenTCP ("
						+ streamingIngestion.InputPortNumber + ")");
		String relatedFirstUpdateAttributeProcessorId = GetProcessorIdByNameFromList(
				relatedProcessorsId, "UpdateAttribute1 ("
						+ streamingIngestion.InputPortNumber + ")");
		String relatedSecondUpdateAttributeProcessorId = GetProcessorIdByNameFromList(
				relatedProcessorsId, "UpdateAttribute2 ("
						+ streamingIngestion.InputPortNumber + ")");
		StopNiFiProcessor(relatedListenTCPProcessorId);
		StopNiFiProcessor(relatedFirstUpdateAttributeProcessorId);
		StopNiFiProcessor(relatedSecondUpdateAttributeProcessorId);

		while (true) {
			CloseableHttpResponse processorState = GetNiFiProcessorState(relatedListenTCPProcessorId);
			String listenTCPProcessorState = GetValueFromHttpResponse(
					processorState, "component", "state", true).toString();
			if (listenTCPProcessorState.equals("STOPPED")) {
				UpdateListenTCPProcessor(relatedListenTCPProcessorId,
						streamingIngestion);
				StartNiFiProcessor(relatedListenTCPProcessorId);
				break;
			} else {
				TimeUnit.SECONDS.sleep(1);
			}
		}

		while (true) {
			CloseableHttpResponse processorState = GetNiFiProcessorState(relatedFirstUpdateAttributeProcessorId);
			String firstUpdateAttributeProcessorState = GetValueFromHttpResponse(
					processorState, "component", "state", true).toString();
			if (firstUpdateAttributeProcessorState.equals("STOPPED")) {
				ChangeUpdateAttribute1Processor(
						relatedFirstUpdateAttributeProcessorId,
						streamingIngestion);
				StartNiFiProcessor(relatedFirstUpdateAttributeProcessorId);
				break;
			} else {
				TimeUnit.SECONDS.sleep(1);
			}
		}

		while (true) {
			CloseableHttpResponse processorState = GetNiFiProcessorState(relatedSecondUpdateAttributeProcessorId);
			String secondUpdateAttributeProcessorState = GetValueFromHttpResponse(
					processorState, "component", "state", true).toString();
			if (secondUpdateAttributeProcessorState.equals("STOPPED")) {
				ChangeUpdateAttribute2Processor(
						relatedSecondUpdateAttributeProcessorId,
						streamingIngestion);
				StartNiFiProcessor(relatedSecondUpdateAttributeProcessorId);
				break;
			} else {
				TimeUnit.SECONDS.sleep(1);
			}
		}

	}

	private void StartNiFiProcessors(List<Processor> relatedProcessorsId)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException {
		for (int i = 0; i < relatedProcessorsId.size(); i++) {
			StartNiFiProcessor(relatedProcessorsId.get(i).Id);
		}
	}

	private void StartNiFiProcessor(String rocessorToStartId)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException {
		String restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + rocessorToStartId;
		CloseableHttpResponse processorState = GetNiFiProcessorState(rocessorToStartId);
		int listeningTCPProcessorVersion = (int) GetValueFromHttpResponse(
				processorState, "revision", "version", true);
		JSONObject jsonObjectToStopProcessor = GenerateJsonObjectToStartProcessor(
				rocessorToStartId, listeningTCPProcessorVersion);
		CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpPut putRequest = new HttpPut(restString);
		String message = jsonObjectToStopProcessor.toString();
		StringEntity entity = new StringEntity(message);
		putRequest.setEntity(entity);
		putRequest.setHeader("Accept", "application/json");
		putRequest.setHeader("Content-type", "application/json");
		CloseableHttpResponse response = httpClient.execute(putRequest);
		int code = response.getStatusLine().getStatusCode();
		// System.out.println("Code = " + code);
		httpClient.close();
		TimeUnit.SECONDS.sleep(1);
	}

	private JSONObject GenerateJsonObjectToStartProcessor(String processorId,
			int processorVersion) throws JSONException {
		JSONObject json = new JSONObject();

		JSONObject jsonRevisionObj = new JSONObject();
		jsonRevisionObj.put("version", processorVersion);
		json.put("revision", jsonRevisionObj);

		json.put("id", processorId);

		JSONObject jsonComponentObj = new JSONObject();
		jsonComponentObj.put("id", processorId);
		jsonComponentObj.put("state", "RUNNING");
		json.put("component", jsonComponentObj);

		JSONObject jsonStatusObj = new JSONObject();
		jsonStatusObj.put("id", processorId);
		json.put("status", jsonStatusObj);

		return json;
	}

	private void UpdateListenTCPProcessor(String processorId,
			StreamingIngestion streamingIngestion)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, java.text.ParseException {
		JSONObject jsonObjectToUpdateListenProcessorPortNumber = null;
		String message = "";
		String restString = "";
		StringEntity entity = null;
		CloseableHttpClient httpClient = null;
		CloseableHttpResponse response = null;
		HttpPut putRequest = null;
		restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + processorId;

		httpClient = HttpClients.createDefault();
		putRequest = new HttpPut(restString);
		putRequest.setHeader("Accept", "application/json");
		putRequest.setHeader("Content-type", "application/json");

		response = GetNiFiProcessorState(processorId);
		int listenTCPProcessorVersion = (int) GetValueFromHttpResponse(
				response, "revision", "version", true);
		jsonObjectToUpdateListenProcessorPortNumber = GenerateJsonObjectToUpdateListenProcessorPortNumber(
				processorId, listenTCPProcessorVersion, streamingIngestion);
		message = jsonObjectToUpdateListenProcessorPortNumber.toString();
		entity = new StringEntity(message);
		putRequest.setEntity(entity);

		response = httpClient.execute(putRequest);
		int code = response.getStatusLine().getStatusCode();
		// System.out.println("Code = " + code);
		httpClient.close();
		TimeUnit.SECONDS.sleep(1);
	}

	private void ChangeUpdateAttribute1Processor(String processorId,
			StreamingIngestion streamingIngestion)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, java.text.ParseException {
		JSONObject jsonObjectToUpdateListenProcessorPortNumber = null;
		String message = "";
		String restString = "";
		StringEntity entity = null;
		CloseableHttpClient httpClient = null;
		CloseableHttpResponse response = null;
		HttpPut putRequest = null;
		restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + processorId;

		httpClient = HttpClients.createDefault();
		putRequest = new HttpPut(restString);
		putRequest.setHeader("Accept", "application/json");
		putRequest.setHeader("Content-type", "application/json");

		response = GetNiFiProcessorState(processorId);
		int listenTCPProcessorVersion = (int) GetValueFromHttpResponse(
				response, "revision", "version", true);
		jsonObjectToUpdateListenProcessorPortNumber = GenerateJsonObjectToChangeFirstUpdateAttributeProcessor(
				processorId, listenTCPProcessorVersion, streamingIngestion);
		message = jsonObjectToUpdateListenProcessorPortNumber.toString();
		entity = new StringEntity(message);
		putRequest.setEntity(entity);

		response = httpClient.execute(putRequest);
		int code = response.getStatusLine().getStatusCode();
		// System.out.println("Code = " + code);
		httpClient.close();
		TimeUnit.SECONDS.sleep(1);
	}

	private void ChangeUpdateAttribute2Processor(String processorId,
			StreamingIngestion streamingIngestion)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, java.text.ParseException {
		JSONObject jsonObjectToUpdateListenProcessorPortNumber = null;
		String message = "";
		String restString = "";
		StringEntity entity = null;
		CloseableHttpClient httpClient = null;
		CloseableHttpResponse response = null;
		HttpPut putRequest = null;
		restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + processorId;

		httpClient = HttpClients.createDefault();
		putRequest = new HttpPut(restString);
		putRequest.setHeader("Accept", "application/json");
		putRequest.setHeader("Content-type", "application/json");

		response = GetNiFiProcessorState(processorId);
		int listenTCPProcessorVersion = (int) GetValueFromHttpResponse(
				response, "revision", "version", true);
		jsonObjectToUpdateListenProcessorPortNumber = GenerateJsonObjectToChangeSecondUpdateAttributeProcessor(
				processorId, listenTCPProcessorVersion, streamingIngestion);
		message = jsonObjectToUpdateListenProcessorPortNumber.toString();
		entity = new StringEntity(message);
		putRequest.setEntity(entity);

		response = httpClient.execute(putRequest);
		int code = response.getStatusLine().getStatusCode();
		// System.out.println("Code = " + code);
		httpClient.close();
		TimeUnit.SECONDS.sleep(1);

	}

	private JSONObject GenerateJsonObjectToChangeSecondUpdateAttributeProcessor(
			String secondUpdateAttributeProcessorId,
			int secondUpdateAttributeProcessorVersion,
			StreamingIngestion streamingIngestion) throws JSONException {
		// String separator = GenerateSeparator(streamingIngestion.Separator);

		JSONObject json = new JSONObject();

		JSONObject jsonRevisionObj = new JSONObject();
		jsonRevisionObj.put("version", secondUpdateAttributeProcessorVersion);
		json.put("revision", jsonRevisionObj);

		json.put("id", secondUpdateAttributeProcessorId);

		JSONObject jsonComponentObj = new JSONObject();
		jsonComponentObj.put("id", secondUpdateAttributeProcessorId);
		jsonComponentObj.put("state", "STOPPED");
		JSONObject jsonConfigObj = new JSONObject();
		JSONObject jsonPropertiesObj = new JSONObject();
		jsonPropertiesObj.put("category", streamingIngestion.Category);
		jsonPropertiesObj.put("headers", streamingIngestion.Headers);
		jsonPropertiesObj.put("separator",
				ConvertToAppropriateSeparator(streamingIngestion.Separator));
		jsonConfigObj.put("properties", jsonPropertiesObj);
		jsonComponentObj.put("config", jsonConfigObj);
		json.put("component", jsonComponentObj);

		JSONObject jsonStatusObj = new JSONObject();
		jsonStatusObj.put("id", secondUpdateAttributeProcessorId);
		json.put("status", jsonStatusObj);

		return json;
	}

	private String ConvertToAppropriateSeparator(String separator) {
		String result = "";
		if (separator.equals("Tab")) {
			result = "TabSeparated";
		} else if (separator.equals("Comma")) {
			result = "CommaSeparated";
		} else if (separator.equals("Pipe")) {
			result = "PipeSeparated";
		} else if (separator.equals("Sharp")) {
			result = "SharpSeparated";
		} else if (separator.equals("Slash")) {
			result = "SlashSeparated";
		}
		return result;
	}

	private String GenerateSeparator(FileSeparator separator) {
		String result = "";
		switch (separator) {
		case Tab:
			result = "TabSeparated";
			break;
		case Comma:
			result = "CommaSeparated";
			break;
		case Pipe:
			result = "PipeSeparated";
			break;
		case Sharp:
			result = "SharpSeparated";
			break;
		case Slash:
			result = "SlashSeparated";
			break;
		}
		return result;
	}

	private String GenerateArrivalTime(StreamingIngestion streamingIngestion)
			throws java.text.ParseException {
		String result = "";
		if (streamingIngestion.dataFlowDateTime == null) {
			result = "${now():format('yyyy-MM-dd')}";
		} else {
			StringBuilder stringBuilder = new StringBuilder();
			SimpleDateFormat sdf = new SimpleDateFormat(
					"MM/dd/yyyy HH:mm:ss aa");
			Calendar calendar = Calendar.getInstance();
			calendar.setTime(sdf.parse(streamingIngestion.dataFlowDateTime));
			int year = calendar.get(Calendar.YEAR);
			int month = calendar.get(Calendar.MONTH);
			month += 1;
			int day = calendar.get(Calendar.DAY_OF_MONTH);
			result = String.format("%s-%s-%s", year, month, day);
		}
		return result;
	}

	private String GenerateFileExtension(StreamingIngestion streamingIngestion) {
		String result = "";
		switch (streamingIngestion.Separator) {
		case "Tab":
			result = "tsv";
			break;
		case "Comma":
			result = "csv";
			break;
		case "Pipe":
			result = "psv";
			break;
		case "Sharp":
			result = "sharpsv";
			break;
		case "Slash":
			result = "slashsv";
			break;
		}
		return result;
	}

	private JSONObject GenerateJsonObjectToChangeFirstUpdateAttributeProcessor(
			String firstUpdateAttributeProcessorId,
			int firstUpdateAttributeProcessorVersion,
			StreamingIngestion streamingIngestion) throws JSONException,
			java.text.ParseException {

		String arrivalTime = GenerateArrivalTime(streamingIngestion);
		String fileExtension = GenerateFileExtension(streamingIngestion);

		JSONObject json = new JSONObject();

		JSONObject jsonRevisionObj = new JSONObject();
		jsonRevisionObj.put("version", firstUpdateAttributeProcessorVersion);
		json.put("revision", jsonRevisionObj);

		json.put("id", firstUpdateAttributeProcessorId);

		JSONObject jsonComponentObj = new JSONObject();
		jsonComponentObj.put("id", firstUpdateAttributeProcessorId);
		jsonComponentObj.put("state", "STOPPED");
		JSONObject jsonConfigObj = new JSONObject();
		JSONObject jsonPropertiesObj = new JSONObject();
		jsonPropertiesObj.put("arrivalTime", arrivalTime);
		jsonPropertiesObj.put("fileExtension", fileExtension);
		jsonConfigObj.put("properties", jsonPropertiesObj);
		jsonComponentObj.put("config", jsonConfigObj);
		json.put("component", jsonComponentObj);

		JSONObject jsonStatusObj = new JSONObject();
		jsonStatusObj.put("id", firstUpdateAttributeProcessorId);
		json.put("status", jsonStatusObj);

		return json;
	}

	private JSONObject GenerateJsonObjectToLoadSpecificTemplate(
			String templateId) throws JSONException, ClientProtocolException, IOException {
		List<Processor> existanceProcessors = GetProcessorsNameIdMapping();
		Double templatePositionX = GetMaximumX(existanceProcessors);
		Double templatePositionY = GetMinimumY(existanceProcessors);
		JSONObject json = new JSONObject();
		json.put("originX", 2000.0 + templatePositionX);
		json.put("originY", templatePositionY);
		json.put("templateId", templateId);
		return json;
	}

	private Double GetMinimumY(List<Processor> existanceProcessors) {
		Double result = 100000.0;
		for (int i = 0; i < existanceProcessors.size(); i++) {
			if (existanceProcessors.get(i).PosisionY < result) {
				result = existanceProcessors.get(i).PosisionY;
			}
		}
		return result;		
	}

	private Double GetMaximumX(List<Processor> existanceProcessors) {
		Double result = 0.0;
		for (int i = 0; i < existanceProcessors.size(); i++) {
			if (existanceProcessors.get(i).PosisionX > result) {
				result = existanceProcessors.get(i).PosisionX;
			}
		}
		return result;		
	}

	private JSONObject GenerateJsonObjectToUpdateListenProcessorPortNumber(
			String listenTCPProcessorId, int listenTCPProcessorVersion,
			StreamingIngestion streamingIngestion) throws JSONException {
		JSONObject json = new JSONObject();

		JSONObject jsonRevisionObj = new JSONObject();
		jsonRevisionObj.put("version", listenTCPProcessorVersion);
		json.put("revision", jsonRevisionObj);

		json.put("id", listenTCPProcessorId);

		JSONObject jsonComponentObj = new JSONObject();
		jsonComponentObj.put("id", listenTCPProcessorId);
		jsonComponentObj.put("state", "STOPPED");
		JSONObject jsonConfigObj = new JSONObject();
		JSONObject jsonPropertiesObj = new JSONObject();
		jsonPropertiesObj.put("Port", streamingIngestion.InputPortNumber);
		jsonConfigObj.put("properties", jsonPropertiesObj);
		jsonComponentObj.put("config", jsonConfigObj);
		json.put("component", jsonComponentObj);

		JSONObject jsonStatusObj = new JSONObject();
		jsonStatusObj.put("id", listenTCPProcessorId);
		json.put("status", jsonStatusObj);

		return json;
	}

	private void StopNiFiProcessor(String processorToStopId)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException {
		String restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + processorToStopId;
		CloseableHttpResponse processorState = GetNiFiProcessorState(processorToStopId);
		int listeningTCPProcessorVersion = (int) GetValueFromHttpResponse(
				processorState, "revision", "version", true);
		JSONObject jsonObjectToStopProcessor = GenerateJsonObjectToStopProcessor(
				processorToStopId, listeningTCPProcessorVersion);
		CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpPut putRequest = new HttpPut(restString);
		String message = jsonObjectToStopProcessor.toString();
		StringEntity entity = new StringEntity(message);
		putRequest.setEntity(entity);
		putRequest.setHeader("Accept", "application/json");
		putRequest.setHeader("Content-type", "application/json");
		CloseableHttpResponse response = httpClient.execute(putRequest);
		int code = response.getStatusLine().getStatusCode();
		// System.out.println("Code = " + code);
		httpClient.close();
		TimeUnit.SECONDS.sleep(1);
	}

	private void StopSpecificStreamingIngestion(
			StreamingIngestion streamingIngestion)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException, URISyntaxException {
		if (IsTemplateExist("Stream Ingestion " + "("
				+ streamingIngestion.InputPortNumber + ")")) {
			if (IsListenTCPProcessorExist("ListenTCP " + "("
					+ streamingIngestion.InputPortNumber + ")")) {
				List<Processor> relatedProcessorsId = GetRelatedProcessorsToSpecificTemplate(streamingIngestion.InputPortNumber
						.toString());
				StopNiFiProcessors(relatedProcessorsId);
				RemoveNiFiProcessorsFromCanvas(relatedProcessorsId,
						streamingIngestion);
			}

		}
	}

	private void RemoveNiFiProcessorsFromCanvas(List<Processor> processorsId,
			StreamingIngestion streamingIngestion)
			throws ClientProtocolException, IOException, JSONException,
			URISyntaxException {
		String relatedListenTCPId = GetProcessorIdByNameFromList(processorsId,
				"ListenTCP (" + streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedListenTCPId);

		String relatedUpdateAttribute1Id = GetProcessorIdByNameFromList(
				processorsId, "UpdateAttribute1 ("
						+ streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedUpdateAttribute1Id);

		String relatedUpdateAttribute2Id = GetProcessorIdByNameFromList(
				processorsId, "UpdateAttribute2 ("
						+ streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedUpdateAttribute2Id);

		String relatedReplaceText1Id = GetProcessorIdByNameFromList(
				processorsId, "ReplaceText1 ("
						+ streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedReplaceText1Id);

		String relatedReplaceText2Id = GetProcessorIdByNameFromList(
				processorsId, "ReplaceText2 ("
						+ streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedReplaceText2Id);

		String relatedPutHDFSId = GetProcessorIdByNameFromList(processorsId,
				"PutHDFS (" + streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedPutHDFSId);

		String relatedPutFileId = GetProcessorIdByNameFromList(processorsId,
				"PutFile (" + streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedPutFileId);

		String relatedGetFileId = GetProcessorIdByNameFromList(processorsId,
				"GetFile (" + streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedGetFileId);

		String relatedReplaceText3Id = GetProcessorIdByNameFromList(
				processorsId, "ReplaceText3 ("
						+ streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedReplaceText3Id);

		String relatedPutSQLId = GetProcessorIdByNameFromList(processorsId,
				"PutSQL (" + streamingIngestion.InputPortNumber + ")");
		RemoveNiFiProcessorFromCanvas(relatedPutSQLId);

	}

	private String GetProcessorIdByNameFromList(List<Processor> processorsId,
			String processorName) {
		String result = "";

		for (int i = 0; i < processorsId.size(); i++) {
			if (processorsId.get(i).Name.equals(processorName)) {
				result = processorsId.get(i).Id;
				break;
			}
		}
		return result;
	}

	private void RemoveNiFiProcessorFromCanvas(String processorId)
			throws ClientProtocolException, IOException, JSONException,
			URISyntaxException {
		CloseableHttpResponse processorState = GetNiFiProcessorState(processorId);
		String processorVersion = GetValueFromHttpResponse(processorState,
				"revision", "version", true).toString();
		CloseableHttpClient httpClient = HttpClients.createDefault();
		URIBuilder builder = new URIBuilder();
		builder.setScheme("http").setHost(siteUrl + ":" + portNumber)
				.setPath("/nifi-api/processors/" + processorId)
				.setParameter("version", processorVersion);
		URI uri = builder.build();
		HttpDelete deleteRequest = new HttpDelete(uri);
		CloseableHttpResponse response = httpClient.execute(deleteRequest);
	}

	private void StopNiFiProcessors(List<Processor> relatedProcessorsId)
			throws ClientProtocolException, IOException, JSONException,
			InterruptedException {
		for (int i = 0; i < relatedProcessorsId.size(); i++) {
			StopNiFiProcessor(relatedProcessorsId.get(i).Id);
		}
	}

	private JSONObject GenerateJsonObjectToStopProcessor(String processorId,
			int processorVersion) throws JSONException {
		JSONObject json = new JSONObject();

		JSONObject jsonRevisionObj = new JSONObject();
		jsonRevisionObj.put("version", processorVersion);
		json.put("revision", jsonRevisionObj);

		json.put("id", processorId);

		JSONObject jsonComponentObj = new JSONObject();
		jsonComponentObj.put("id", processorId);
		jsonComponentObj.put("state", "STOPPED");
		json.put("component", jsonComponentObj);

		JSONObject jsonStatusObj = new JSONObject();
		jsonStatusObj.put("id", processorId);
		json.put("status", jsonStatusObj);

		return json;

	}

	private CloseableHttpResponse GetNiFiProcessorState(String processorId)
			throws ClientProtocolException, IOException, JSONException {
		String restString = "http://" + siteUrl + ":" + portNumber
				+ "/nifi-api/processors/" + processorId;
		CloseableHttpClient httpClient = HttpClients.createDefault();
		HttpGet getRequest = new HttpGet(restString);
		CloseableHttpResponse response = httpClient.execute(getRequest);
		return response;
	}

	private Object GetValueFromHttpResponse(CloseableHttpResponse httpResponse,
			String parentKey, String childKey, Boolean isNested)
			throws ParseException, IOException, JSONException {
		Object value = "";
		if (isNested) {
			String responseAsString = EntityUtils.toString(httpResponse
					.getEntity());
			JSONObject jsonObject = new JSONObject(responseAsString);
			value = jsonObject.getJSONObject(parentKey).get(childKey);
		} else {
			String responseAsString = EntityUtils.toString(httpResponse
					.getEntity());
			JSONObject jsonObject = new JSONObject(responseAsString);
			value = jsonObject.get(parentKey);
		}
		return value;
	}
	// //////////////////////////////////////////////////////////////////////////////////////////////////////////

}