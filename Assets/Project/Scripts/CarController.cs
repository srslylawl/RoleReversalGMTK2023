using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public interface IController {
	
}

public class CarController : MonoBehaviour, IController {


	[FormerlySerializedAs("car")] [SerializeField] private CarData carData;

	[SerializeField] private PIDController turnPIDY;

	private Rigidbody rb;
	private Vector3 turnDirection;
	private Vector3 targetDestination;
	private float distanceToDestination;
	private bool inputHeld;

	private Vector3 targetUpDir;

	private float tiltAmount;

	private bool IsBeingReplayed;

	private CarTimeData ownDataRef;


	private void Awake() {
		rb = GetComponent<Rigidbody>();

		var tf = transform;
		var forward = tf.forward;
		turnDirection = forward;
		targetDestination = tf.position + forward;
		distanceToDestination = 1;
		targetUpDir = Vector3.up;
		turnPIDY = carData.TurnPID;
	}
	
	#if UNITY_EDITOR
	private void OnValidate() {
		turnPIDY = carData.TurnPID;
	}
	
	#endif

	public void ReceiveTargetInput(Vector3 mouseWorldPosition) {
		var diff = (mouseWorldPosition - transform.position);
		distanceToDestination = diff.magnitude;
		turnDirection = diff.normalized;
		targetDestination = mouseWorldPosition;
	}

	public void ReceiveInputHeld(bool held) {
		inputHeld = held;
		// Debug.Log($"INPUT HELD: {held}");
	}

	public void SetMode(bool beingReplayed, CarTimeData _ownDataRef) {
		// rb.isKinematic = beingReplayed;
		IsBeingReplayed = beingReplayed;
		ownDataRef = _ownDataRef;
		ownDataRef.TimeDataMode = beingReplayed ? TimeDataMode.Replay : TimeDataMode.Record;
	}

	private void FixedUpdate() {
		if (!IsBeingReplayed) {
			DoAccelerate();
			DoTurn();
		}
	}

	private ContactPoint[] contacts = new ContactPoint[16];
	private void OnCollisionEnter(Collision other) {
		var amount = other.GetContacts(contacts);
		if (amount > 0) {
			for (int i = 0; i < amount; i++) {
				var cp = contacts[i];
				var otherCar = cp.otherCollider.GetComponentInParent<CarController>();
				if (otherCar != null) {
					//TURN OFF REPLAY
					Debug.Log($"CRASH WITH CAR: {otherCar.gameObject}");
					if (ownDataRef != null) {
						ownDataRef.TimeDataMode = TimeDataMode.Record;
					}
				}
			}
		}
	}

	public void ApplyTimeData(ObjectTimeData data) {
		rb.velocity = data.Velocity;
		rb.angularVelocity = data.AngularVelocity;
		rb.position = data.Position;
		rb.rotation = data.Rotation;
	}
	
	

	public ObjectTimeData GetTimeData() {
		var timeData = new ObjectTimeData();
		timeData.Position = rb.position;
		timeData.Velocity = rb.velocity;
		timeData.AngularVelocity = rb.angularVelocity;
		timeData.Rotation = rb.rotation;
		return timeData;
	}

	private void DoTurn() {
		var doTurn = inputHeld;
		var targetRotation = Quaternion.LookRotation(doTurn ? turnDirection : transform.forward, Vector3.up);
		var diff = Quaternion.Inverse(rb.rotation) * targetRotation;
		var diffRemapped = RemapTorque(diff.eulerAngles);

		var error = diffRemapped.y;
		var output = turnPIDY.Update(Time.fixedDeltaTime, 0, error);

		// Debug.Log($"Diff: {diffRemapped.y}| Err: {error}, OP {output}");

		rb.angularVelocity = new Vector3(0, carData.TurnSpeed * output * Time.fixedDeltaTime, 0);
		// rb.angularVelocity = transform.up * turnSpeed* output * Time.fixedDeltaTime;
	}

	private void DoAccelerate() {
		var forward = transform.forward;
		var doAccelerate = inputHeld;

		var velocity = rb.velocity;
		var mag = Mathf.Clamp(velocity.magnitude, 0, carData.TopSpeed);

		if (doAccelerate) {
			rb.AddForce(forward * carData.Acceleration, ForceMode.Acceleration);
		}

		var diffForward = 0f;
		var hasVelocity = velocity.magnitude > 0.1f;
		var fwd = hasVelocity ? velocity.normalized : forward;
		diffForward = Vector3.SignedAngle(hasVelocity ? velocity.normalized : forward, forward, Vector3.up) * carData.VelocityTiltInfluence;

		var error = tiltAmount - diffForward;

		tiltAmount = Mathf.Clamp(tiltAmount - error * carData.MaxTiltPerSecond * Time.fixedDeltaTime, -carData.MaxTiltAngle, carData.MaxTiltAngle);

		// Debug.Log($"Diff: {diffForward}, tiltAmt: {tiltAmount} | fwd: {fwd} | vel: {velocity} m {velocity.magnitude}");
		var rotateUp = Quaternion.AngleAxis(tiltAmount, forward);

		targetUpDir = rotateUp * Vector3.up;
		// Debug.Log($"Diff:  {diffForward} tarUp: {targetUpDir} mag: {targetUpDir.magnitude}");

		rb.rotation = Quaternion.LookRotation(forward, targetUpDir);

		var targetVelocity = forward * mag;
		var interpolatedVelocity = Vector3.MoveTowards(velocity, targetVelocity, carData.Grip * Time.fixedDeltaTime);
		rb.velocity = interpolatedVelocity.normalized * mag;
	}

	private Vector3 RemapTorque(Vector3 torque) {
		return new Vector3
		(
			torque.x > 180f ? torque.x - 360f : torque.x,
			torque.y > 180f ? torque.y - 360f : torque.y,
			torque.z > 180f ? torque.z - 360f : torque.z
		);
	}

	private void OnDrawGizmos() {
		var pos = transform.position;
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(pos, pos + transform.forward);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(pos, pos + turnDirection);
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(pos, pos + targetUpDir);
		if (!rb) return;
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(pos, pos + rb.velocity.normalized);
	}
}