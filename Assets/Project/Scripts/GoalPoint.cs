using System;
using UnityEngine;

public class GoalPoint : MonoBehaviour {
	[SerializeField] private float Radius = 2f;
	
	public Action<GameObject> OnGoalReached;

	private SphereCollider coll;
	private void Awake() {
		coll = gameObject.AddComponent<SphereCollider>();
		coll.isTrigger = true;
		coll.radius = Radius;
	}

	private void OnTriggerEnter(Collider other) {
		OnGoalReached?.Invoke(other.attachedRigidbody.gameObject);
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, Radius);
	}
}
