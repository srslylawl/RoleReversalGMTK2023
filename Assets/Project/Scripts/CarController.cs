using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour {

	[SerializeField] private float turnSpeed;



	private Vector3 turnDirection;


	public void ReceiveTargetInput(Vector3 mouseWorldPosition) {
		turnDirection = (mouseWorldPosition - transform.position).normalized;
	}

	private void FixedUpdate() {
		var targetRotation = Quaternion.LookRotation(turnDirection, Vector3.up);
		
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1*Time.fixedDeltaTime*turnSpeed);
	}


	private void OnDrawGizmos() {
		var pos = transform.position;
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(pos, pos + transform.forward);
	}
}
