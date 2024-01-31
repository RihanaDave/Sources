package Access;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.Statement;

public class DataBaseProvider {

	private static final String MYSQLURL = "jdbc:mysql://"
			+ ConfigurationManager.GetAppSettings("mysqlip") + ":3306/";
	private static final String USER = ConfigurationManager
			.GetAppSettings("mysqlusername");
	private static final String PASSWORD = ConfigurationManager
			.GetAppSettings("mysqlpassword");
	private static final String DATABASENAME = "DataFlowDB";

	private String GetDatabaseQuery() {
		return String.format("CREATE DATABASE IF NOT EXISTS %s;", DATABASENAME);
	}

	private String GetFlowMetadataTableQuery() {
		return String
				.format("CREATE TABLE IF NOT EXISTS %s ( category VARCHAR(100) , datetime VARCHAR(100) , headers VARCHAR(1000) , dataSeparator VARCHAR(100) , PRIMARY KEY (category, datetime, dataSeparator));;",
						"DataFlowMetadata");
	}

	private String GetFileIngestionDataFlowStatusTableQuery() {
		return String
				.format("CREATE TABLE IF NOT EXISTS  %s (dataFlowUUID VARCHAR(100), category VARCHAR(100) , dataFlowDateTime VARCHAR(100) , dataSeparator VARCHAR(100) , timeBegin VARCHAR(100) , timeEnd VARCHAR(100) , dataFlowStatus VARCHAR(100) , PRIMARY KEY (dataFlowUUID));",
						"FileIngestionDataFlowStatus");
	}

	private String GetStreamingIngestionDataFlowStatusTableQuery() {
		return String
				.format("CREATE TABLE IF NOT EXISTS %s (dataFlowUUID VARCHAR(100), category VARCHAR(100) , dataFlowDateTime VARCHAR(100) , dataSeparator VARCHAR(100) , startTime VARCHAR(100), portNumber INT, dataFlowStatus VARCHAR(100) , PRIMARY KEY (portNumber));",
						"StreamingIngestionDataFlowStatus");
	}	
	
	public void ExecuteUpdateQuery(Connection con, String query){		
		try {
			
			String dataBaseUrl = MYSQLURL + DATABASENAME;					
			Statement stmt = con.createStatement();			
			stmt.executeUpdate(query);			
			CloseConnection(con);
		} catch (Exception e) {
			System.out.println(e);
		}			
	}
	
	public ResultSet ExecuteSelectQuery(Connection con, String query){
		ResultSet rs = null;		
		try {			
			Statement stmt = con.createStatement();
			rs = stmt.executeQuery(query.toString());					
		} catch (Exception e) {
			System.out.println(e);
		}			
		return rs;
	}

	public Connection OpenConnection(){
		Connection con = null;
		String dataBaseUrl = MYSQLURL + DATABASENAME;
		try {
			Class.forName("com.mysql.jdbc.Driver");
			con = DriverManager.getConnection(dataBaseUrl, USER, PASSWORD);			
		} catch (Exception e) {
			System.out.println(e);
		}
		return con;
	}
	
	public void CloseConnection(Connection con){
		
		try {
			con.close();
		} catch (Exception e) {
			System.out.println(e);
		}		
	}
	private void CreateDataBase() {
		try {
			Class.forName("com.mysql.jdbc.Driver");
			Connection con = DriverManager.getConnection(MYSQLURL, USER, PASSWORD);				
			Statement stmt = con.createStatement();
			stmt.executeUpdate(GetDatabaseQuery());
			System.out.println("Database created successfully...");
			CloseConnection(con);
		} catch (Exception e) {
			System.out.println(e);
		}
	}	

	private void CreateTables() {
		try {
			
			Connection con = OpenConnection();			
			Statement stmt = con.createStatement();
			stmt.executeUpdate(GetFlowMetadataTableQuery());
			stmt.executeUpdate(GetFileIngestionDataFlowStatusTableQuery());
			stmt.executeUpdate(GetStreamingIngestionDataFlowStatusTableQuery());
			System.out.println("Table created successfully...");
			CloseConnection(con);
		} catch (Exception e) {
			System.out.println(e);
		}
	}

	public void Init() {
		CreateDataBase();
		CreateTables();
	}
}
