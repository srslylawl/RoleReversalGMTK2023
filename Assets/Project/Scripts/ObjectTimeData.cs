using UnityEngine;

public struct ObjectTimeData {
	public Vector3 Position;
	public Vector3 Velocity;
	public Quaternion Rotation;
	public Vector3 AngularVelocity;
	public bool HasInput;
	

	public static ObjectTimeData Empty() {
		return new ObjectTimeData() {
			Position = Vector3.zero,
			AngularVelocity = Vector3.zero,
			Rotation = Quaternion.identity,
			Velocity = Vector3.zero
		};
	}
}
