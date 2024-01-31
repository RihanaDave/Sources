package src.java.main;

import java.io.IOException;

import javax.xml.parsers.ParserConfigurationException;
import javax.xml.ws.Endpoint;

import org.xml.sax.SAXException;

import Access.ConfigurationManager;
import Service.DatalakeOperationImpl;

public class Publisher {

	public static void main(String[] args) throws ParserConfigurationException, IOException, SAXException {
		System.out.println("program is runnig...");
		
		//TODO create logger class for write log in InitProvider.
		//create database and tables if not exists.
		InitProvider.Init();

		//get ip and port from config.properties to start service.
		String ip = ConfigurationManager.GetAppSettings("ip");
		String port = ConfigurationManager.GetAppSettings("port");
		String url = String.format("http://%s:%s/DatalakeService", ip, port);
		Endpoint.publish(url, new DatalakeOperationImpl());
		System.out.println("service is running at << "+ url+" >>");

	}

}
