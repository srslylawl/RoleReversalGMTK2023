using System;
using UnityEngine;

[Serializable]
public class LevelData {
	public CarLevelData[] CarLevelDatas;
}


[Serializable]
public struct CarLevelData {
	public GameObject CarPrefab;
	public SpawnPoint SpawnPoint;
	public GoalPoint GoalPoint;
}