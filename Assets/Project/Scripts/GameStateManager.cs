using System;
using System.Collections.Generic;
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

	[SerializeField] private Score CurrentScore = new Score();

	enum GameMode {
		Frog,
		Car
	}

	private GameMode currentMode = GameMode.Car;

	private GoalPoint activeGoalPoint;

	private int CurrentTick;
	private int HighestTick;

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

					if (timeData.TimeDataMode == TimeDataMode.Replay) {
						if (timeData.TimeData.TryGetValue(CurrentTick, out var data)) {
							if (!timeData.Frog.gameObject.activeSelf) {
								timeData.Frog.gameObject.SetActive(true);
							}

							timeData.Frog.ApplyTimeData(data);
						}
						else {
							timeData.Frog.gameObject.SetActive(false);
						}
					}
				}
				activeFrogTimeData.TimeData[CurrentTick] = activeFrogTimeData.Frog.GetTimeData();
				break;
			case GameMode.Car:
				foreach (var timeData in carTimeData) {
					if (timeData.TimeDataMode == TimeDataMode.Record) {
						timeData.TimeData[CurrentTick] = timeData.Car.GetTimeData();
					}

					if (timeData.TimeDataMode == TimeDataMode.Replay) {
						if (timeData.TimeData.TryGetValue(CurrentTick, out var data)) {
							if (!timeData.Car.gameObject.activeSelf) {
								timeData.Car.gameObject.SetActive(true);
							}

							timeData.Car.ApplyTimeData(data);
						}
						else {
							timeData.Car.gameObject.SetActive(false);
						}
					}
				}
				
				activeCarTimeData.TimeData[CurrentTick] = activeCarTimeData.Car.GetTimeData();
				break;
		}


		//Set all positions
		foreach (var timeData in carTimeData) {
			if (timeData.TimeData.TryGetValue(CurrentTick, out var data)) {
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
			if (timeData.TimeData.TryGetValue(CurrentTick, out var data)) {
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
		HighestTick = Math.Max(CurrentTick, HighestTick);
	}

	private void ResetScene() {
		GamePaused = true;
		//Apply first tick data
		foreach (var timeData in carTimeData) {
			timeData.Car.ApplyTimeData(timeData.TimeData[0]);

			if (timeData.TimeDataMode == TimeDataMode.Record) {
				//make sure to set the rest of the times to the last tick
				var last = timeData.TimeData[CurrentTick-1];
				for (int i = CurrentTick; i <= HighestTick; i++) {
					timeData.TimeData[i] = last;
				}
			}
			timeData.TimeDataMode = TimeDataMode.Replay;
		}
		
		foreach (var timeData in frogTimeData) {
			timeData.Frog.ApplyTimeData(timeData.TimeData[0]);
			if (timeData.TimeDataMode == TimeDataMode.Record) {
				//make sure to set the rest of the times to the last tick
				var last = timeData.TimeData[CurrentTick-1];
				for (int i = CurrentTick; i <= HighestTick; i++) {
					timeData.TimeData[i] = last;
				}
			}
			timeData.TimeDataMode = TimeDataMode.Replay;
		}
		
		CurrentTick = 0;
	}

	private void SetActiveGoalPoint(GoalPoint goalPoint) {
		if (activeGoalPoint != null) {
			activeGoalPoint.OnGoalReached = null;
		}

		activeGoalPoint = goalPoint;
		activeGoalPoint.OnGoalReached = OnGoalReachedCallBack;
	}

	private void OnGoalReachedCallBack(GameObject go) {
		if (currentMode == GameMode.Car) {
			if (go.TryGetComponent(out CarController controller)) {
				var same = controller == activeCarTimeData.Car;
				if (same) {
					//GOAL REACHED
					//TODO: CHECK IF FROG WAS KILLED! - frog needs to be killed to succeed!
					Debug.Log($"{go} reached the goal!");
					EndRound();
					Next();
				}
			}
		}

		if (currentMode == GameMode.Frog) {
			if (go.TryGetComponent(out FrogController controller)) {
				var same = controller == activeFrogTimeData.Frog;
				if (same) {
					//GOAL REACHED
					EndRound();
					Next();
				}
			}
		}
	}
	private void OnCrashCallBack(CarController car) {
		if (currentMode != GameMode.Car) {
			throw new Exception("Car crash callback while in frog mode????");
		}

		if (car != activeCarTimeData.Car)
			throw new Exception("Car crash called with inactive car");


		if (CurrentScore.RemainingLives == 0) {
			//TODO: GAME OVER - OUT OF LIVES
		}
		
		CurrentScore.RemainingLives--;
		
		//TODO: UPDATE CAR LIVES UI
		
		EndRound();
	}

	private void OnRunOverFrogCallBack(FrogController frog) {
		//Make sure frog has not been killed already
		CurrentScore.CurrentFrogsKilled++;
		
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
		activeFrogTimeData = new FrogTimeData(frogObject.GetComponent<FrogController>());
		InputManager.SetFrogController(activeFrogTimeData.Frog);
		activeFrogTimeData.TimeDataMode = TimeDataMode.Record;
		SetActiveGoalPoint(currentData.GoalPoint);
		
		UIManager.StartReadyCountDown("Get to safety!", StartRound);
		CameraScript.AssignFrogTarget(frogObject.transform);
		CameraScript.SwitchCameras(false);

	}

	private void StartCarMode() {
		currentMode = GameMode.Car;
		if (levelData.CarLevelDatas.Length < CurrentLevelCarIteration - 1) {
			throw new Exception($"Car Level Data too small! Current Iteration: {CurrentLevelCarIteration}, Cars: {levelData.CarLevelDatas.Length}");
		}
			
		//START CAR MODE
		var currentData = levelData.CarLevelDatas[CurrentLevelCarIteration];
		var spTF = currentData.SpawnPoint.transform;
		var carObject = Instantiate(currentData.CarPrefab, spTF.position, spTF.rotation);
		activeCarTimeData = new CarTimeData(carObject.GetComponent<CarController>());
		InputManager.SetCarController(activeCarTimeData.Car);
		activeCarTimeData.Car.SetMode(false, activeCarTimeData);
		SetActiveGoalPoint(currentData.GoalPoint);
		
		UIManager.StartReadyCountDown("Don't let him escape!", StartRound);
		CameraScript.SwitchCameras(true);
	}

	public void Next() {
		//Change to next mode
		if (currentMode == GameMode.Car) {
			StartFrogMode();
		}

		if (currentMode == GameMode.Frog) {
			StartCarMode();
		}
		
	}

	public void StartRound() {
		GamePaused = false;
		InputManager.UpdateControllers = true;
	}

	public void EndRound() {
		GamePaused = true;
		InputManager.UpdateControllers = false;
		if (currentMode == GameMode.Car) {
			carTimeData.Add(activeCarTimeData);
			activeCarTimeData.Car.SetMode(true, activeCarTimeData);
			activeCarTimeData = null;
			InputManager.SetCarController(null);

			CurrentLevelCarIteration++;
		}

		if (currentMode == GameMode.Frog) {
			frogTimeData.Add(activeFrogTimeData);
			activeFrogTimeData.TimeDataMode = TimeDataMode.Replay;
			// activeFrogTimeData = null;
			InputManager.SetFrogController(null);

			CameraScript.AssignFrogTarget(null);
			CurrentLevelFrogIteration++;
		}

		ResetScene();
	}
}