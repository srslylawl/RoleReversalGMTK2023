using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
	[SerializeField] private InputManager InputManager;
	[SerializeField] private LevelData levelData;

	private List<CarTimeData> carTimeData = new List<CarTimeData>();
	private List<FrogTimeData> frogTimeData = new List<FrogTimeData>();

	private CarTimeData activeCarTimeData;

	enum GameMode {
		Frog,
		Car
	}

	private GameMode currentMode = GameMode.Car;

	private GoalPoint activeGoalPoint;

	private int CurrentTick;
	private int HighestTick;

	private int CurrentLevelCarIteration;

	private bool GamePaused = true;

	private void FixedUpdate() {
		if (GamePaused) {
			return;
		}

		switch (currentMode) {
			case GameMode.Frog:
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
		
		CurrentTick = 0;
	}

	private void SetActiveGoalPoint(int id) {
		if (activeGoalPoint != null) {
			activeGoalPoint.OnGoalReached = null;
		}

		activeGoalPoint = GoalPoint.goalPoints[id];
		activeGoalPoint.OnGoalReached = OnGoalReachedCallBack;
	}

	private void OnGoalReachedCallBack(GameObject go) {
		if (currentMode == GameMode.Car) {
			if (go.TryGetComponent(out CarController controller)) {
				var same = controller == activeCarTimeData.Car;
				if (same) {
					//GOAL REACHED
					//TODO: CHECK IF FROG WAS KILLED!
					Debug.Log($"{go} reached the goal!");
					EndRound();
					Next();
				}
			}
		}
	}

	private void Update() {
		var pressed = Input.GetKeyDown(KeyCode.Space);
		if (pressed) {
			if (GamePaused) {
				Next();
			}
			else {
				EndRound();
			}
		}
	}

	public void Next() {
		if (currentMode == GameMode.Car) {
			if (levelData.CarLevelDatas.Length < CurrentLevelCarIteration - 1) {
				throw new Exception($"Car Level Data too small! Current Iteration: {CurrentLevelCarIteration}, Cars: {levelData.CarLevelDatas.Length}");
			}

			var currentData = levelData.CarLevelDatas[CurrentLevelCarIteration];
			var carObject = Instantiate(currentData.CarPrefab, currentData.SpawnPoint, Quaternion.Euler(currentData.SpawnRotation));
			activeCarTimeData = new CarTimeData(carObject.GetComponent<CarController>());
			InputManager.SetCarController(activeCarTimeData.Car);
			activeCarTimeData.Car.SetMode(false, activeCarTimeData);
			SetActiveGoalPoint(currentData.GoalPointID);
			//spawn new car
		}

		StartRound();
	}

	public void StartRound() {
		GamePaused = false;
	}

	public void EndRound() {
		GamePaused = true;
		if (currentMode == GameMode.Car) {
			carTimeData.Add(activeCarTimeData);
			activeCarTimeData.Car.SetMode(true, activeCarTimeData);
			activeCarTimeData = null;
			InputManager.SetCarController(null);

			CurrentLevelCarIteration++;
		}

		ResetScene();
	}
}