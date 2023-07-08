using System;
using UnityEngine;

[Serializable]
public class LevelData {
	public CarLevelData[] CarLevelDatas;
	public FrogLevelData[] FrogLevelDatas;
}


[Serializable]
public struct CarLevelData {
	public GameObject CarPrefab;
	public SpawnPoint SpawnPoint;
	public GoalPoint GoalPoint;
}


[Serializable]
public struct FrogLevelData {
	public GameObject FrogPrefab;
	public SpawnPoint SpawnPoint;
	public GoalPoint GoalPoint;
}