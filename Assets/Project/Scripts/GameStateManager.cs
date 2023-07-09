using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


public class Score {
	public int CurrentFrogsKilled;
	public int BonusFrogsKilled;

	public float CurrentTimeRemaining;
	public float BonusTime;

	public void AddTimeRemainingToScore() {
		BonusTime += CurrentTimeRemaining;
		CurrentTimeRemaining = 0;
	}

	public void UpdateScore() {
		UIManager.SetScore(CalculateScore());
	}

	public int CalculateScore() {
		//each bonus frog killed adds 500 to score
		int score = BonusFrogsKilled * 500;
		
		//each second remaining adds 10 to score;
		score += (int)(BonusTime * 10);

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

		CurrentScore.CurrentTimeRemaining = Mathf.Max(0, CurrentScore.CurrentTimeRemaining - Time.fixedDeltaTime);
		UIManager.SetBonusTimerAmount((int)CurrentScore.CurrentTimeRemaining);

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
		
		// Debug.Log($"Set Origin Data for past objects: CurrentTick: {CurrentTick}");
		
		CurrentTick = 0;
	}

	private void UpdateFrogKillUI() {
		UIManager.SetFrogsKilledText($"{CurrentScore.CurrentFrogsKilled} / {levelData.FrogLevelDatas.Length}");
	}

	private void ResetOnFailure() {
		StopRound();
		OnReset?.Invoke();
		OnReset = null;
		
		
		if (currentMode == GameMode.Car) {
			// var currentData = levelData.CarLevelDatas[CurrentLevelCarIteration];
			var car = activeCarTimeData.Car;
			car.Reset();
			car.ApplyTimeData(activeCarTimeData.TimeData[-1]);
			// Debug.Log($"Set Origin Data: {activeCarTimeData.TimeData[-1].Position} |CurrentTick: {CurrentTick}");
			
			InputManager.SetCarController(car);
			activeCarTimeData.Car.SetMode(false, activeCarTimeData);
			// SetActiveCarGoalPoint(currentData.GoalPoint);
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

		AudioManager.PlaySound("drumGo");
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
		AudioManager.PlaySound("drumFail");
		UIManager.DisplayQuickText("oof heavy traffic today!", 2f, ResetOnFailure);
	}

	private void OnGoalReachedCallBack(IController ctr) {
		if (GamePaused) {
			Debug.Log("GOal reached but game paused!");
			return;
		}
		
		if (currentMode == GameMode.Car) {
			if (ctr is CarController car) {
				var same = car == activeCarTimeData.Car;
				if (same) {
					if (activeFrogHasBeenKilled) {
						Debug.Log($"{car} reached the goal and the frog is dead!");
						AudioManager.PlaySound("successSmall");
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
					AudioManager.PlaySound("drumFail");
					UIManager.DisplayQuickText("Frog escaped!", 2f, ResetOnFailure);
					Debug.Log("FROG ESCAPED!");

					return;
				}
			}
		}
		if (currentMode == GameMode.Frog) {
			if (ctr is FrogController controller) {
				var same = controller == activeFrogTimeData.Frog;
				if (same) {
					//GOAL REACHED
					AudioManager.PlaySound("successSmall");
					EndRound();
					Next();
				}
			}
		}
    }

	private void ResetBonusTimer() {
		CurrentScore.CurrentTimeRemaining = maxTime;
		UIManager.SetBonusTimerAmount((int)CurrentScore.CurrentTimeRemaining);
	}
	
	private void OnCrashCallBack([CanBeNull] CarController otherCar) {
		if (GamePaused) {
			return;
		}
		
		if (currentMode != GameMode.Car) {
			throw new Exception("Car crash callback while in frog mode????");
		}

		if (otherCar == activeCarTimeData.Car)
			throw new Exception("Car crash with itself????");
		
		//quickly disable callbacks so we dont end up having to deal with resetting all accidentally killed frogs's death ticks
		activeCarTimeData.Car.OnFrogKilledCallBack = null;

		AudioManager.PlaySound("carCrash");
		StopRound();
		if (otherCar != null) {
			AudioManager.PlaySound("drumFail");
			UIManager.DisplayQuickText("Traffic violation!", 2f, ResetOnFailure);
			return;
		}
		
		//Environment collision
		AudioManager.PlaySound("drumFail");
		UIManager.DisplayQuickText("Don't text and drive!", 2f, ResetOnFailure);
	}

	private Action OnReset;

	private void OnRunOverFrogCallBack(FrogController frog) {
		if (frog == activeFrogTimeData.Frog) {
			//Frog killed! Success!
			var oldDeathTick = activeFrogTimeData.DeathTick;
			activeFrogTimeData.DeathTick = CurrentTick;
			CurrentScore.CurrentFrogsKilled++;
			activeFrogHasBeenKilled = true;
			UIManager.SetFrogsKilledText($"{CurrentScore.CurrentFrogsKilled} / {levelData.FrogLevelDatas.Length}");
			
			//Activate goal
			SetActiveCarGoalPoint(levelData.CarLevelDatas[CurrentLevelCarIteration].GoalPoint);

			OnReset += () => {
				SetActiveCarGoalPoint(null);
				activeFrogTimeData.DeathTick = oldDeathTick;
				CurrentScore.CurrentFrogsKilled--;
				UpdateFrogKillUI();
			};
		}
		else {
			//update frog kill time
			//TODO: if we kill a frog and then lose the round, the frog will disappear in future rounds as no car actively kills him (our state gets reset)
			var oldDeathTick = frog.ownDataRef.DeathTick;
			frog.ownDataRef.DeathTick = CurrentTick;
			CurrentScore.BonusFrogsKilled++;
			OnReset += () => {
				frog.ownDataRef.DeathTick = oldDeathTick;
				CurrentScore.BonusFrogsKilled--;
			};
			return;
		}

		if (CurrentScore.CurrentFrogsKilled >= levelData.FrogLevelDatas.Length) {
			StopRound();
			GameSuccess();
		}
	}

	private void Start() {
		AudioManager.PlayAmbientLoop("wind");
		ResetBonusTimer();
		CurrentScore.UpdateScore();
		UpdateFrogKillUI();
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
			AudioManager.PlaySound("drumGo");
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
		// SetActiveCarGoalPoint(currentData.GoalPoint);
		car.OnFrogKilledCallBack = OnRunOverFrogCallBack;
		car.OnCrashCallBack = OnCrashCallBack;
		
		//Set frog kill target
		activeFrogTimeData.Frog.SetKillArrowActive(true);
		activeFrogTimeData.Frog.SetMode(true, activeFrogTimeData);
		
		CameraScript.AssignCarZoomTarget(carObject.transform);
		CameraScript.SwitchCameras(CameraScript.TargetCam.CarZoom);
		UIManager.DisplayQuickText("Don't let him escape!", 2f, () => {
			CameraScript.SwitchCameras(CameraScript.TargetCam.Car);
			
			AudioManager.PlaySound("drumGo");
			UIManager.StartReadyCountDown("Drive Safely!", StartRound);
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
		InputManager.SetUpdateControllers(true);
		AudioManager.PlaySound("countDownGo");
	}

	public void StopRound() {
		GamePaused = true;
		InputManager.SetUpdateControllers(false);
	}

	public void EndRound() {
		CurrentScore.AddTimeRemainingToScore();
		CurrentScore.UpdateScore();
		ResetBonusTimer();
		StopRound();
		OnReset = null;
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

	public void GameOver()
	{
		UIManager.OpenGameOverUI();
	}

	public void GameSuccess()
    {
        UIManager.CloseGameUI();
        UIManager.OpenScoreUI(CurrentScore.CalculateScore());
    }
}