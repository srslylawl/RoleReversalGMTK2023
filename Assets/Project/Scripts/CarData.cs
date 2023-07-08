using UnityEngine;

[CreateAssetMenu]
public class CarData : ScriptableObject {
	public float TurnSpeed;
	public float TopSpeed;
	public float Acceleration;
	public float Grip;
	public float MaxTiltAngle = 35f;
	public float MaxTiltPerSecond = 10f;
	public float VelocityTiltInfluence = .75f;

	public PIDController TurnPID = new PIDController();
}
