using UnityEngine;

public class CarController : MonoBehaviour {
	[SerializeField] private float turnSpeed;
	[SerializeField] private float topSpeed = 5f;
	[SerializeField] private float acceleration = 1f;


	[SerializeField] private PIDController turnPIDX = new();
	[SerializeField] private PIDController turnPIDY = new();
	[SerializeField] private PIDController turnPIDZ = new();


	[SerializeField] private PIDController speedPID = new();


	private Rigidbody rb;
	private Vector3 turnDirection;
	private Vector3 targetDestination;
	private float distanceToDestination;


	private void Awake() {
		rb = GetComponent<Rigidbody>();

		turnDirection = transform.forward;
		targetDestination = transform.position + transform.forward;
		distanceToDestination = 1;
	}

	public void ReceiveTargetInput(Vector3 mouseWorldPosition) {
		var diff = (mouseWorldPosition - transform.position);
		distanceToDestination = diff.magnitude;
		turnDirection = diff.normalized;
		targetDestination = mouseWorldPosition;
	}

	private void FixedUpdate() {
		DoAccelerate();
		DoTurn();

	}

	private void DoTurn() {

		var doTurn = Input.GetMouseButton(0);
		var targetRotation = Quaternion.LookRotation(doTurn ? turnDirection : transform.forward, Vector3.up);
		var diff = Quaternion.Inverse(rb.rotation) * targetRotation;
		var diffRemapped = RemapTorque(diff.eulerAngles);
		

		var error = diffRemapped.y;
		var output = turnPIDY.Update(Time.fixedDeltaTime, 0, error);
		
		// Debug.Log($"Cur: {remappedCurrent.y}, Tar: {remappedTarget.y} Diff: {diffRemapped.y}| Err: {error}, OP {output}");


		// float tX = turnPIDX.UpdateAngle(Time.fixedDeltaTime, remappedCurrent.x, remappedTarget.x);
		// float tY = turnPIDY.UpdateAngle(Time.fixedDeltaTime, remappedCurrent.y, remappedTarget.y);
		// float tZ = turnPIDZ.UpdateAngle(Time.fixedDeltaTime, remappedCurrent.z, remappedTarget.z);
		// Debug.Log($"X err {torque.x} out: {tX} | Y err {torque.y} out: {tY} | Z err {torque.z} out: {tZ}");

		// torque.x = Mathf.Abs(torque.x) * tX;
		// torque.y = Mathf.Abs(torque.y) * tY * turnSpeed;
		// torque.z = Mathf.Abs(torque.z) * tZ;
		// Debug.Log($"Torque: {torque}");
		// Debug.Log($"Torque Mag. {torque.magnitude}");

		rb.angularVelocity = new Vector3(0, turnSpeed * output * Time.fixedDeltaTime, 0);
		// rb.AddRelativeTorque(torque, ForceMode.VelocityChange);
	}

	private void DoAccelerate() {
		var doAccelerate = Input.GetMouseButton(0);
		if (doAccelerate) {
			
			rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);


			var velocity = rb.velocity;
			var mag = Mathf.Clamp(velocity.magnitude, 0, topSpeed);
			

			var newVelocity = transform.forward * mag;
			rb.velocity = newVelocity;
			
		}
		// var velocity = rb.velocity;
		// var currentSpeed = velocity.magnitude;
		// var velocityDir = velocity.normalized;
		// var targetSpeed = doAccelerate ? topSpeed : 0;
		//
		// var forceDirection = doAccelerate? transform.forward : velocityDir;

		// var error = targetSpeed - currentSpeed;
		// var output = speedPID.Update(Time.fixedDeltaTime, 0, error);

		// var force = forceDirection * output * acceleration;

		// Debug.Log($"Force: {force} | Output: {output} | Error: {error}");

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
	}
}