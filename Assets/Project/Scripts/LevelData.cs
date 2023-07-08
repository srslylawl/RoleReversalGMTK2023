using System;
using UnityEngine;

[CreateAssetMenu]
public class LevelData : ScriptableObject {
	public CarLevelData[] CarLevelDatas;
	// public 
}


[Serializable]
public struct CarLevelData {
	public GameObject CarPrefab;
	public Vector3 SpawnPoint;
	public Vector3 SpawnRotation;
}