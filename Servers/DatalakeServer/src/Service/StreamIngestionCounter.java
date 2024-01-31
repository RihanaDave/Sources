package Service;

public class StreamIngestionCounter {
	public static Integer listennigProcessorCount = 1;
	public Integer GetListennigProcessorCount(){
		return listennigProcessorCount;
	}
	
	public void IncreaseListennigProcessorCount(){
		listennigProcessorCount++;
	}
	public void DecreaseListennigProcessorCount(){
		listennigProcessorCount--;
	}
}
