using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public Dictionary<int, ObjectTimeData> TimeData = new();
	
	public FrogTimeData(FrogController frog) {
		Frog = frog;
	}
}
