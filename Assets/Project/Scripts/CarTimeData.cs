using System.Collections.Generic;

public enum TimeDataMode {
	Record,
	Replay
}

public class CarTimeData {
	public CarController Car;

	public TimeDataMode TimeDataMode;

	public int DeathTick = 9999999;

	public Dictionary<int, ObjectTimeData> TimeData = new();
	
	public CarTimeData(CarController car) {
		Car = car;
		var nullTimeData = ObjectTimeData.Empty();
		nullTimeData.Position = car.transform.position;
		nullTimeData.Rotation = car.transform.rotation;
		TimeData[-1] = nullTimeData;
	}
}


public class FrogTimeData {
	public FrogController Frog;
	
	public TimeDataMode TimeDataMode;

	public int DeathTick = 9999999;

	public Dictionary<int, ObjectTimeData> TimeData = new();
	
	public FrogTimeData(FrogController frog) {
		Frog = frog;
		var nullTimeData = ObjectTimeData.Empty();
		nullTimeData.Position = frog.transform.position;
		nullTimeData.Rotation = frog.transform.rotation;
		TimeData[-1] = nullTimeData;
	}
}
