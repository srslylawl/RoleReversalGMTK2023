using System;
using UnityEngine;

public class InputManager : MonoBehaviour {
	private Camera cam;

	private Vector3 mouseWorldPos;


	public CarController CarController;
	
	private void Awake() {
		cam = Camera.main;
	}


	private void Update() {
		var screenPos = Input.mousePosition;
		var ray = cam.ScreenPointToRay(screenPos);
		// Define the plane
		Vector3 planeNormal = Vector3.up; // Plane normal pointing upwards
		Vector3 planePoint = new Vector3(0f, 0f, 0f); // Point on the plane
        
		// Calculate the intersection
		float t = Vector3.Dot(planePoint - ray.origin, planeNormal) / Vector3.Dot(ray.direction, planeNormal);
		Vector3 intersectionPoint = ray.origin + t * ray.direction;

		mouseWorldPos = intersectionPoint;
		
		CarController.ReceiveTargetInput(mouseWorldPos);
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(mouseWorldPos, .3f);
	}
}
