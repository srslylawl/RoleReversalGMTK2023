using System;
using System.Collections.Generic;
using UnityEngine;

public class GoalPoint : MonoBehaviour {
	[SerializeField] private float Radius = 2f;
	[SerializeField] private int GoalPointID = -1;
	
	public Action<GameObject> OnGoalReached;

	public static Dictionary<int, GoalPoint> goalPoints;


	private SphereCollider coll;
	private void Awake() {
		coll = gameObject.AddComponent<SphereCollider>();
		coll.isTrigger = true;
		coll.radius = Radius;
	}

	private void Start() {
		goalPoints ??= new Dictionary<int, GoalPoint>();

		goalPoints[GoalPointID] = this;
	}

	private void OnTriggerEnter(Collider other) {
		OnGoalReached?.Invoke(other.attachedRigidbody.gameObject);
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position, Radius);
	}
}
