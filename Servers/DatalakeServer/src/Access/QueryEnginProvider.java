package Access;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.Statement;

public class QueryEnginProvider {
	//private static final String JDBC_DRIVER = "org.apache.drill.jdbc.Driver";
	private static final String DB_URL = "jdbc:drill:drillbit=" + ConfigurationManager.GetAppSettings("drillip") + ":31010";
	private static final String USER = ConfigurationManager.GetAppSettings("drillusername");
	private static final String PASS = ConfigurationManager.GetAppSettings("drillpassword");
		
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
		try {
			Class.forName("org.apache.drill.jdbc.Driver");
			con = DriverManager.getConnection(DB_URL, USER, PASS);			
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
}
