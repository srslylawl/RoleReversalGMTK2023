using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


public class Score {
	public int TotalFrogAmount;
	public int CurrentFrogsKilled;
	public int TotalLives;
	public int RemainingLives;

	public float SecondsRemaining;

	public int CalculateScore() {
		//each frog killed adds 100 to score
		int score = CurrentFrogsKilled * 100;
		
		//each remaining car live adds 200 to score;
		score += RemainingLives * 200;
		
		//each second remaining adds 10 to score;
		score += (int)(SecondsRemaining * 10);

		return score;
	}
}

public class GameStateManager : MonoBehaviour {
	[SerializeField] private InputManager InputManager;
	[SerializeField] private LevelData levelData;

	private List<CarTimeData> carTimeData = new List<CarTimeData>();
	private List<FrogTimeData> frogTimeData = new List<FrogTimeData>();

	private CarTimeData activeCarTimeData;
	private FrogTimeData activeFrogTimeData;

	[SerializeField] private float maxTime = 45;
	private float startOfRound;
    [SerializeField] private Score CurrentScore = new Score();

	enum GameMode {
		Frog,
		Car
	}

	private GameMode currentMode = GameMode.Car;

	private GoalPoint activeCarGoalPoint;
	private GoalPoint activeFrogGoalPoint;

	private bool activeFrogHasBeenKilled;

	private int CurrentTick;

	private int CurrentLevelCarIteration;
	private int CurrentLevelFrogIteration;

	private bool GamePaused = true;

	private void FixedUpdate() {
		if (GamePaused) {
			return;
		}

        switch (currentMode) {
			case GameMode.Frog:
				foreach (var timeData in frogTimeData) {
					if (timeData.TimeDataMode == TimeDataMode.Record) {
						timeData.TimeData[CurrentTick] = timeData.Frog.GetTimeData();
					}
				}
				if (activeFrogTimeData.TimeDataMode == TimeDataMode.Record) {
					activeFrogTimeData.TimeData[CurrentTick] = activeFrogTimeData.Frog.GetTimeData();
				}
				break;
			case GameMode.Car:
				foreach (var timeData in carTimeData) {
					//RECORD
					if (timeData.TimeDataMode == TimeDataMode.Record) {
						timeData.TimeData[CurrentTick] = timeData.Car.GetTimeData();
					}
				}

				if (activeCarTimeData.TimeDataMode == TimeDataMode.Record) {
					activeCarTimeData.TimeData[CurrentTick] = activeCarTimeData.Car.GetTimeData();
				}
				break;
		}

        //Set all positions
        foreach (var timeData in carTimeData) {
			if (timeData.TimeDataMode == TimeDataMode.Record) continue;
			
			var alreadyDead = timeData.DeathTick <= CurrentTick;
			if (!alreadyDead && timeData.TimeData.TryGetValue(CurrentTick, out var data)) {
				if (!timeData.Car.gameObject.activeSelf) {
					timeData.Car.gameObject.SetActive(true);
				}

				timeData.Car.ApplyTimeData(data);
			}
			else {
				timeData.Car.gameObject.SetActive(false);
			}
		}
		
		foreach (var timeData in frogTimeData) {
			if (timeData.TimeDataMode == TimeDataMode.Record) continue;
			var alreadyDead = timeData.DeathTick <= CurrentTick;
			if (!alreadyDead && timeData.TimeData.TryGetValue(CurrentTick, out var data)) {
				if (!timeData.Frog.gameObject.activeSelf) {
					timeData.Frog.gameObject.SetActive(true);
				}

				timeData.Frog.ApplyTimeData(data);
			}
			else {
				timeData.Frog.gameObject.SetActive(false);
			}
		}

		CurrentTick++;
	}

	private void ResetScene() {
		//Apply first tick data
		foreach (var timeData in carTimeData) {
			if (!timeData.Car.gameObject.activeSelf) {
				timeData.Car.gameObject.SetActive(true);
			}
			
			timeData.Car.ApplyTimeData(timeData.TimeData[-1]);
			timeData.TimeDataMode = TimeDataMode.Replay;
		}
		
		foreach (var timeData in frogTimeData) {
			if (!timeData.Frog.gameObject.activeSelf) {
				timeData.Frog.gameObject.SetActive(true);
			}
			
			timeData.Frog.ApplyTimeData(timeData.TimeData[-1]);
			timeData.TimeDataMode = TimeDataMode.Replay;
		}
		
		Debug.Log($"Set Origin Data for past objects: CurrentTick: {CurrentTick}");
		
		CurrentTick = 0;
	}

	private void ResetOnFailure() {
		StopRound();
		
		if (currentMode == GameMode.Car) {
			var currentData = levelData.CarLevelDatas[CurrentLevelCarIteration];
			var car = activeCarTimeData.Car;
			car.ApplyTimeData(activeCarTimeData.TimeData[-1]);
			Debug.Log($"Set Origin Data: {activeCarTimeData.TimeData[-1].Position} |CurrentTick: {CurrentTick}");
			
			InputManager.SetCarController(car);
			activeCarTimeData.Car.SetMode(false, activeCarTimeData);
			SetActiveCarGoalPoint(currentData.GoalPoint);
			car.OnFrogKilledCallBack = OnRunOverFrogCallBack;
			car.OnCrashCallBack = OnCrashCallBack;
			activeFrogTimeData.Frog.SetKillArrowActive(true);

			activeFrogHasBeenKilled = false;
		}
		
		if (currentMode == GameMode.Frog) {
			if (levelData.FrogLevelDatas.Length < CurrentLevelFrogIteration - 1) {
				throw new Exception($"Frog Level Data too small! Current Iteration: {CurrentLevelCarIteration}, Cars: {levelData.CarLevelDatas.Length}");
			}
		
			var cData = levelData.FrogLevelDatas[CurrentLevelFrogIteration];
			var frog = activeFrogTimeData.Frog;
			frog.gameObject.SetActive(true);
			frog.ApplyTimeData(activeFrogTimeData.TimeData[-1]);
			InputManager.SetFrogController(activeFrogTimeData.Frog);
			activeFrogTimeData.TimeDataMode = TimeDataMode.Record;
			SetActiveFrogGoalPoint(cData.GoalPoint);

			activeFrogHasBeenKilled = false;
		}
		
		ResetScene();
		
		UIManager.DisplayQuickText("Try again!", 2f, StartRound);
	}

	private void SetActiveCarGoalPoint(GoalPoint goalPoint) {
		if (activeCarGoalPoint != null) {
			activeCarGoalPoint.OnGoalReached = null;
			activeCarGoalPoint.SetGoalActive(false);
		}

		activeCarGoalPoint = goalPoint;
		
		if(activeCarGoalPoint) {
			activeCarGoalPoint.OnGoalReached = OnGoalReachedCallBack;
			activeCarGoalPoint.SetGoalActive(true);
		}
	}

	private void SetActiveFrogGoalPoint(GoalPoint goalPoint) {
		if (activeFrogGoalPoint != null) {
			activeFrogGoalPoint.OnGoalReached = null;
			activeFrogGoalPoint.SetGoalActive(false);
		}

		activeFrogGoalPoint = goalPoint;
		
		if(activeFrogGoalPoint) {
			activeFrogGoalPoint.OnGoalReached = OnGoalReachedCallBack;
			activeFrogGoalPoint.SetGoalActive(true);
		}
	}

	private void OnGettingRoadKilledCallBack() {
		StopRound();
		UIManager.DisplayQuickText("oof heavy traffic today!", 2f, ResetOnFailure);
	}

	private void OnGoalReachedCallBack(IController ctr) {
		if (currentMode == GameMode.Car) {
			if (ctr is CarController car) {
				var same = car == activeCarTimeData.Car;
				if (same) {
					//GOAL REACHED
					//TODO: CHECK IF FROG WAS KILLED! - frog needs to be killed to succeed!
					if (activeFrogHasBeenKilled) {
						Debug.Log($"{car} reached the goal and the frog is dead!");
						EndRound();
						Next();
					}
					else {
						
						Debug.Log($"{car} reached the goal! BUT FROG IS STILL ALIVE!");
					}

				}
			}
			if (ctr is FrogController frog) {
				var same = frog == activeFrogTimeData.Frog;
				if (same) {
					//FROG REACHED GOAL WHILE IN CAR MODE - LOSE CONDITION!
					StopRound();
					UIManager.DisplayQuickText("Frog escaped!", 2f, ResetOnFailure);

					return;
				}
			}
		}
		if (currentMode == GameMode.Frog) {
			if (ctr is FrogController controller) {
				var same = controller == activeFrogTimeData.Frog;
				if (same) {
					//GOAL REACHED
					EndRound();
					Next();
				}
			}
		}
    }

	private void StartTimer()
	{
        startOfRound = Time.time;
        UIManager.CountdowmTimeStart(maxTime);
    }
    private void EndTimer()
    {
        CurrentScore.SecondsRemaining += maxTime - Time.time - startOfRound;
        UIManager.CountdowmTimeStop();
    }
	
	private void OnCrashCallBack([CanBeNull] CarController otherCar) {
		if (currentMode != GameMode.Car) {
			throw new Exception("Car crash callback while in frog mode????");
		}

		if (otherCar == activeCarTimeData.Car)
			throw new Exception("Car crash with itself????");
		
		//quickly disable callbacks so we dont end up having to deal with resetting all accidentally killed frogs's death ticks
		activeCarTimeData.Car.OnFrogKilledCallBack = null;
		
		//RESTART ROUND
		StopRound();
		UIManager.DisplayQuickText("Traffic violation!", 2f, ResetOnFailure);
	}

	private void OnRunOverFrogCallBack(FrogController frog) {
		if (frog == activeFrogTimeData.Frog) {
			//Frog killed! Success!
			activeFrogTimeData.DeathTick = CurrentTick;
			CurrentScore.CurrentFrogsKilled++;
			activeFrogHasBeenKilled = true;
			
			//TODO: set indicator to goal
			// EndRound();
			// Next();

			return;
		}
		else {
			//update frog kill time
			frog.ownDataRef.DeathTick = CurrentTick;
		}
		//Make sure frog has not been killed already
		// CurrentScore.CurrentFrogsKilled++;
		
		//if all frogs have been killed, display score!
	}

	private void Start() {
        StartFrogMode();
	}

	private void StartFrogMode() {
		currentMode = GameMode.Frog;
		if (levelData.FrogLevelDatas.Length < CurrentLevelFrogIteration - 1) {
			throw new Exception($"Frog Level Data too small! Current Iteration: {CurrentLevelCarIteration}, Cars: {levelData.CarLevelDatas.Length}");
		}
		
		var currentData = levelData.FrogLevelDatas[CurrentLevelFrogIteration];
		var spTF = currentData.SpawnPoint.transform;
		var frogObject = Instantiate(currentData.FrogPrefab, spTF.position, spTF.rotation);
		frogObject.name = "FROG_" + CurrentLevelFrogIteration + 1;
		activeFrogTimeData = new FrogTimeData(frogObject.GetComponent<FrogController>());
		InputManager.SetFrogController(activeFrogTimeData.Frog);
		activeFrogTimeData.TimeDataMode = TimeDataMode.Record;
		
		var goal = currentData.GoalPoint;
		SetActiveFrogGoalPoint(goal);
		
		//Set camera to goal
		CameraScript.AssignGoalTarget(goal.transform);
		CameraScript.AssignFrogTarget(frogObject.transform);

		activeFrogTimeData.Frog.OnGettingRoadKilledCallBack = OnGettingRoadKilledCallBack;

		CameraScript.SwitchCameras(CameraScript.TargetCam.Goal);
		UIManager.DisplayQuickText("Get to safety!", 2f, () => {
			//Set camera to frog
			CameraScript.SwitchCameras(CameraScript.TargetCam.Frog);
			UIManager.StartReadyCountDown("Ready?", StartRound);
		});
	}

	private void StartCarMode() {
		currentMode = GameMode.Car;
		if (levelData.CarLevelDatas.Length < CurrentLevelCarIteration - 1) {
			throw new Exception($"Car Level Data too small! Current Iteration: {CurrentLevelCarIteration}, Cars: {levelData.CarLevelDatas.Length}");
		}

		activeFrogHasBeenKilled = false;
			
		//START CAR MODE
		var currentData = levelData.CarLevelDatas[CurrentLevelCarIteration];
		var spTF = currentData.SpawnPoint.transform;
		var carObject = Instantiate(currentData.CarPrefab, spTF.position, spTF.rotation);
		carObject.name = "CAR_" + CurrentLevelCarIteration + 1;
		var car = carObject.GetComponent<CarController>();
		activeCarTimeData = new CarTimeData(car);
		InputManager.SetCarController(car);
		activeCarTimeData.Car.SetMode(false, activeCarTimeData);
		SetActiveCarGoalPoint(currentData.GoalPoint);
		car.OnFrogKilledCallBack = OnRunOverFrogCallBack;
		car.OnCrashCallBack = OnCrashCallBack;
		
		//Set frog kill target
		activeFrogTimeData.Frog.SetKillArrowActive(true);
		activeFrogTimeData.Frog.SetMode(true, activeFrogTimeData);
		
		CameraScript.AssignCarZoomTarget(carObject.transform);
		CameraScript.SwitchCameras(CameraScript.TargetCam.CarZoom);
		UIManager.DisplayQuickText("Drive safely!", 2f, () => {
			CameraScript.SwitchCameras(CameraScript.TargetCam.Car);
			UIManager.StartReadyCountDown("Don't let him escape!", StartRound);
		});
		
	}

	public void Next() {
		//Change to next mode
		if (currentMode == GameMode.Car) {
			StartFrogMode();
			return;
		}
		

		if (currentMode == GameMode.Frog) {
			StartCarMode();
			return;
		}
	}

	public void StartRound() {
		GamePaused = false;
		StartTimer();
        InputManager.UpdateControllers = true;
	}

	public void StopRound() {
		GamePaused = true;
		InputManager.UpdateControllers = false;
	}

	public void EndRound() {
		EndTimer();
		StopRound();
		if (currentMode == GameMode.Car) {
			carTimeData.Add(activeCarTimeData);
			var car = activeCarTimeData.Car;
			car.SetMode(true, activeCarTimeData);
			car.OnFrogKilledCallBack = null;
			car.OnCrashCallBack = null;
			activeCarTimeData.DeathTick = CurrentTick;
			
			
			activeCarTimeData = null;
			InputManager.SetCarController(null);
			SetActiveCarGoalPoint(null);
			activeFrogHasBeenKilled = false;
			activeFrogTimeData.Frog.SetKillArrowActive(false);

			CurrentLevelCarIteration++;
		}

		if (currentMode == GameMode.Frog) {
			frogTimeData.Add(activeFrogTimeData);
			activeFrogTimeData.Frog.SetMode(true, activeFrogTimeData);
			activeFrogTimeData.Frog.OnGettingRoadKilledCallBack = null;
			InputManager.SetFrogController(null);
			activeFrogTimeData.DeathTick = CurrentTick;

			CameraScript.AssignFrogTarget(null);
			CurrentLevelFrogIteration++;
			
			//active frog goalpoint stays, as frog reaching goal while in car mode is a lose condition
		}

		ResetScene();
	}
}