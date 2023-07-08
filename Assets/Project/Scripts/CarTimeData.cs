using System.Collections.Generic;

public enum TimeDataMode {
	Record,
	Replay
}

public class CarTimeData {
	public CarController Car;

	public TimeDataMode TimeDataMode;

	public Dictionary<int, ObjectTimeData> TimeData = new();
	
	public CarTimeData(CarController car) {
		Car = car;
	}
}


public class FrogTimeData {
	public FrogController Frog;
	
	public TimeDataMode TimeDataMode;

	public int DeathTick = 99999;

	public Dictionary<int, ObjectTimeData> TimeData = new();
	
	public FrogTimeData(FrogController frog) {
		Frog = frog;
	}
}
